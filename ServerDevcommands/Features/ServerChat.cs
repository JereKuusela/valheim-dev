using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Splatform;
using UnityEngine;

namespace ServerDevcommands;
[HarmonyPatch(typeof(Chat), nameof(Chat.SendText))]
public class ServerChat
{
  public static ZNet.PlayerInfo ServerInfo = new()
  {
    m_name = "Server",
    m_characterID = ZDOID.None,
    m_userInfo = new() { m_id = new("Steam_0"), m_displayName = "Server" },
    m_serverAssignedDisplayName = "Server",
    m_publicPosition = false,
    m_position = Vector3.zero,
  };
  public static void Write(ZPackage pkg, bool pos)
  {
    pkg.Write(ServerInfo.m_name);
    pkg.Write(ServerInfo.m_characterID);
    pkg.Write(ServerInfo.m_userInfo.m_id.ToString());
    pkg.Write(ServerInfo.m_userInfo.m_displayName);
    pkg.Write(ServerInfo.m_serverAssignedDisplayName);
    pkg.Write(ServerInfo.m_publicPosition);
    if (pos) pkg.Write(ServerInfo.m_position);
  }
  static void Postfix(Talker.Type type, string text)
  {
    if (Player.m_localPlayer) return;
    UserInfo info = new() { Name = ServerInfo.m_userInfo.m_displayName, UserId = ServerInfo.m_userInfo.m_id };
    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", [
      Vector3.zero,
      (int)type,
      info,
      text,
    ]);
  }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.TryGetPlayerByPlatformUserID))]
public class TryGetPlayerByPlatformUserID
{
  static bool Postfix(bool result, PlatformUserID platformUserID, ref ZNet.PlayerInfo playerInfo)
  {
    if (result) return result;
    if (platformUserID.ToString() != "Steam_0") return result;

    playerInfo = ServerChat.ServerInfo;
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
    pkg.SetPos(0);
    if (IsExtraPlayerAdded(net, pkg.ReadInt())) return;
    pkg.SetPos(0);
    pkg.Write(net.m_players.Count + 1);
    ServerChat.Write(pkg, false);
  }
  static bool IsExtraPlayerAdded(ZNet net, int count) => count == net.m_players.Count + 1;
}