using System;
using HarmonyLib;
namespace ServerDevcommands;

[HarmonyPatch(typeof(Terminal), nameof(Terminal.TryRunCommand))]
public class LogCommand {
  static void Postfix(string text) {
    if (!ZNet.instance) return;
    if (ZNet.instance.IsServer()) return;
    var server = ZNet.instance.GetServerRPC();
    server?.Invoke(CommandLogging.RPC_Log, new[] { text });
  }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
public class CommandLogging {

  public static string RPC_Log = "DEV_Log";

  private static string Format(ZNetPeer peer, string command) {
    return String.Format(
      Settings.Format(Settings.CommandLogFormat).Replace("command", "6"),
      peer.m_socket.GetHostName(), peer.m_playerName, peer.m_characterID.UserID.ToString(), peer.m_refPos.x, peer.m_refPos.y, peer.m_refPos.z, command
    );
  }
  private static void RPC_LogCommand(ZRpc rpc, string command) {
    var message = Format(ZNet.instance.GetPeer(rpc), command);
    ZLog.Log(message);
  }
  static void Postfix(ZNet __instance, ZRpc rpc) {
    if (__instance.IsServer()) {
      rpc.Register<string>(RPC_Log, new(RPC_LogCommand));
    }
  }
}
