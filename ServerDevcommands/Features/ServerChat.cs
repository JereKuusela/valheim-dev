using System.Collections.Generic;
using System;
using System.Reflection.Emit;
using HarmonyLib;
using Splatform;
using Service;
using UnityEngine;

namespace ServerDevcommands;

[HarmonyPatch(typeof(Chat), nameof(Chat.SendText))]
public class ServerChat
{
  public static ZNet.PlayerInfo ServerClient => serverClient ??= CreatePlayerInfo();
  private static ZNet.PlayerInfo? serverClient;
  public static UserInfo UserInfo => userInfo ??= new UserInfo { Name = ServerClient.m_userInfo.m_displayName, UserId = ServerClient.m_userInfo.m_id };
  private static UserInfo? userInfo;

  private static ZNet.PlayerInfo CreatePlayerInfo() => new()
  {
    m_name = Settings.ServerChatName,
    // Receiving chat messages requires a valid character ID.
    m_characterID = new ZDOID(ZDOMan.GetSessionID(), uint.MaxValue),
    // Valheim 1.0: m_serverAssignedDisplayName lives on CrossNetworkUserInfo now.
    m_userInfo = new() { m_id = GetServerUserId(), m_displayName = Settings.ServerChatName, m_serverAssignedDisplayName = Settings.ServerChatName },
    m_publicPosition = false,
    m_position = Vector3.zero,
  };
  public static void RefreshPlayerInfo()
  {
    serverClient = null;
    userInfo = null;
  }
  private static PlatformUserID GetServerUserId()
  {
    try
    {
      // Steamworks is not available for Microsoft clients.
      var steamGameServer = Type.GetType("Steamworks.SteamGameServer, com.rlabrecque.steamworks.net", false);
      var getSteamId = steamGameServer?.GetMethod("GetSteamID");
      var steamId = getSteamId?.Invoke(null, null)?.ToString();
      if (!string.IsNullOrEmpty(steamId))
        return new PlatformUserID(ZNet.instance.m_steamPlatform, steamId);
    }
    catch
    {
    }

    if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
      return new PlatformUserID("playfab", ZPlayFabMatchmaking.m_instance.m_serverData.remotePlayerId);
    else if (ZNet.instance.m_hostSocket == null)
      return new PlatformUserID(ZNet.instance.m_steamPlatform, "Server");
    else
      return new PlatformUserID(ZNet.instance.m_steamPlatform, ZNet.instance.m_hostSocket.GetHostName());
  }
  public static void Write(ZPackage pkg)
  {
    pkg.Write(ServerClient.m_name);
    pkg.Write(ServerClient.m_characterID);
    pkg.Write(ServerClient.m_userInfo.m_id.ToString());
    pkg.Write(ServerClient.m_userInfo.m_displayName);
    pkg.Write(ServerClient.m_userInfo.m_serverAssignedDisplayName);
    // Valheim 1.0 added m_playfabId to the PlayerList packet, before the position flag.
    pkg.Write(ServerClient.m_userInfo.m_playfabId ?? string.Empty);
    // Server position is never public.
    pkg.Write(false);
  }
  static void Postfix(Talker.Type type, string text)
  {
    if (Player.m_localPlayer) return;
    if (!Settings.IsServerChat) return;
    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", [
      Vector3.zero,
      (int)type,
      UserInfo,
      text,
    ]);
  }
}

// Server client is only sent to clients, so this is needed for the server to recognize it.
[HarmonyPatch(typeof(ZNet), nameof(ZNet.TryGetPlayerByPlatformUserID))]
public class RecognizeServerClient
{
  static bool Postfix(bool result, PlatformUserID platformUserID, ref ZNet.PlayerInfo playerInfo)
  {
    if (result) return result;
    if (platformUserID != ServerChat.ServerClient.m_userInfo.m_id) return result;

    playerInfo = ServerChat.ServerClient;
    return true;
  }
}

// Valheim 1.0 moved the packet writing (and the Write(count) anchor) from SendPlayerList into WritePlayerInfo.
[HarmonyPatch(typeof(ZNet), nameof(ZNet.WritePlayerInfo))]
public class AddExtraPlayer
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var matcher = new CodeMatcher(instructions).MatchStartForward(new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZPackage), nameof(ZPackage.Write), [typeof(int)])));
    if (matcher.IsInvalid)
    {
      Log.Error("AddExtraPlayer: ZPackage.Write(int) anchor not found in ZNet.WritePlayerInfo; server chat player entry disabled.");
      return instructions;
    }
    var writePos = matcher.Pos;
    // The receiver of that Write call is the ZPackage local; clone its load instead of assuming local slot 0.
    var loadPackage = matcher.MatchStartBackwards(new CodeMatch(i => i.IsLdloc())).Instruction.Clone();
    return matcher.Start().Advance(writePos + 1)
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
      .InsertAndAdvance(loadPackage)
      .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AddExtraPlayer), nameof(AddServer))))
      .InstructionEnumeration();
  }

  static void AddServer(ZNet net, ZPackage pkg)
  {
    if (!Settings.IsServerChat) return;
    // This is needed in case multiple mods are adding extra players.
    var prev = pkg.GetPos();
    pkg.SetPos(0);
    if (IsExtraPlayerAdded(net, pkg.ReadInt()))
    {
      pkg.SetPos(prev);
    }
    else
    {
      pkg.SetPos(0);
      pkg.Write(net.m_players.Count + 1);
      ServerChat.Write(pkg);
    }
  }
  static bool IsExtraPlayerAdded(ZNet net, int count) => count >= net.m_players.Count + 1;
}