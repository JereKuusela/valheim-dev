using HarmonyLib;
namespace ServerDevcommands;

[HarmonyPatch(typeof(Terminal), nameof(Terminal.TryRunCommand))]
public class LogCommand
{
  static void Postfix(string text)
  {
    if (!ZNet.instance) return;
    if (ZNet.instance.IsServer()) return;
    var server = ZNet.instance.GetServerRPC();
    if (server != null) server.Invoke(CommandLogging.RPC_Log, new[] { text });
  }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
public class CommandLogging
{

  public static string RPC_Log = "DEV_Log";

  private static void RPC_LogCommand(ZRpc rpc, string command)
  {
    var hostname = rpc.GetSocket().GetHostName();
    var name = ZNet.instance.GetPeer(rpc).m_playerName;
    var pos = ZNet.instance.GetPeer(rpc).m_refPos;
    var message = $"{hostname}/{name} ({pos.x:F0} ,{pos.z:F0}, {pos.y:F0}): {command}";
    ZLog.Log(message);
  }
  static void Postfix(ZNet __instance, ZRpc rpc)
  {
    if (__instance.IsServer())
    {
      rpc.Register<string>(RPC_Log, new(RPC_LogCommand));
    }
  }
}
