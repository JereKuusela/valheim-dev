using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Splatform;
using UnityEngine;

namespace ServerDevcommands;
[HarmonyPatch(typeof(Chat), nameof(Chat.SendText))]
public class ServerChat
{
  public static ZNet.PlayerInfo ServerClient = new()
  {
    m_name = "Server",
    m_characterID = ZDOID.None,
    m_userInfo = new() { m_id = new("Steam_0"), m_displayName = "Server" },
    m_serverAssignedDisplayName = "Server",
    m_publicPosition = false,
    m_position = Vector3.zero,
  };
  // "Send private player" feature always sends the position.
  // forcePos is not needed for other mods copying this code.
  public static void Write(ZPackage pkg, bool forcePos)
  {
    pkg.Write(ServerClient.m_name);
    pkg.Write(ServerClient.m_characterID);
    pkg.Write(ServerClient.m_userInfo.m_id.ToString());
    pkg.Write(ServerClient.m_userInfo.m_displayName);
    pkg.Write(ServerClient.m_serverAssignedDisplayName);
    // Server position is never public.
    pkg.Write(false);
    if (forcePos) pkg.Write(ServerClient.m_position);
  }
  static void Postfix(Talker.Type type, string text)
  {
    if (Player.m_localPlayer) return;
    UserInfo info = new() { Name = ServerClient.m_userInfo.m_displayName, UserId = ServerClient.m_userInfo.m_id };
    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", [
      Vector3.zero,
      (int)type,
      info,
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
    if (platformUserID.ToString() != "Steam_0") return result;

    playerInfo = ServerChat.ServerClient;
    return true;
  }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.SendPlayerList))]
public class AddExtraPlayer
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZPackage), nameof(ZPackage.Write), [typeof(int)])))
      .Advance(1)
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AddExtraPlayer), nameof(AddServer))))
      .InstructionEnumeration();
  }

  static void AddServer(ZNet net, ZPackage pkg)
  {
    if (!Settings.ServerClient) return;
    // This is needed in case multiple mods are adding extra players.
    pkg.SetPos(0);
    if (IsExtraPlayerAdded(net, pkg.ReadInt())) return;
    pkg.SetPos(0);
    pkg.Write(net.m_players.Count + 1);
    ServerChat.Write(pkg, false);
  }
  static bool IsExtraPlayerAdded(ZNet net, int count) => count >= net.m_players.Count + 1;
}