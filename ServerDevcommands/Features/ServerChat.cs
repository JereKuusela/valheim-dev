using System.Collections.Generic;
using System;
using System.Reflection.Emit;
using HarmonyLib;
using Splatform;
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
    m_userInfo = new() { m_id = GetServerUserId(), m_displayName = Settings.ServerChatName },
    m_serverAssignedDisplayName = Settings.ServerChatName,
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
    if (ZNet.m_onlineBackend == OnlineBackendType.Steamworks)
      return new PlatformUserID(ZNet.instance.m_steamPlatform, ZNet.instance.m_hostSocket.GetHostName());
    return new PlatformUserID("playfab", ZPlayFabMatchmaking.m_instance.m_serverData.remotePlayerId);
  }
  public static void Write(ZPackage pkg)
  {
    pkg.Write(ServerClient.m_name);
    pkg.Write(ServerClient.m_characterID);
    pkg.Write(ServerClient.m_userInfo.m_id.ToString());
    pkg.Write(ServerClient.m_userInfo.m_displayName);
    pkg.Write(ServerClient.m_serverAssignedDisplayName);
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

[HarmonyPatch(typeof(ZNet), nameof(ZNet.SendPlayerList))]
public class AddExtraPlayer
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions).MatchStartForward(new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZPackage), nameof(ZPackage.Write), [typeof(int)])))
      .Advance(1)
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
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