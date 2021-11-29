using System;
using HarmonyLib;
using Service;

namespace DEV {

  // Adds new commands.
  [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
  public class ServerCommands {
    public static string RandomEvent = "randomevent";
    public static string RPC_RandomEvent = "DEV_RandomEvent";
    public static string StopEvent = "stopevent";
    public static string RPC_StopEvent = "DEV_StopEvent";
    public static string StartEvent = "event";
    public static string RPC_StartEvent = "DEV_StartEvent";
    public static string ResetKeys = "resetkeys";
    public static string RPC_ResetKeys = "DEV_ResetKeys";
    public static string SetKey = "setkey";
    public static string RPC_SetKey = "DEV_SetKey";
    public static string Sleep = "sleep";
    public static string RPC_Sleep = "DEV_Sleep";
    public static string SkipTime = "skiptime";
    public static string RPC_SkipTime = "DEV_SkipTime";
    private static bool IsAllowed(ZRpc rpc) {
      var zNet = ZNet.instance;
      if (!zNet.enabled) {
        return false;
      }
      if (rpc != null && !zNet.m_adminList.Contains(rpc.GetSocket().GetHostName())) {
        Console.instance.AddString("Unauthorized to use devcommands.");
        return false;
      }
      if (!Admin.Enabled) {
        Console.instance.AddString("Server not authorized to use devcommands.");
        return false;
      }
      return true;
    }
    private static void RPC_Do_RandomEvent(ZRpc rpc) {
      RedirectOutput.Target = rpc;
      if (IsAllowed(rpc))
        Console.instance.TryRunCommand(RandomEvent);
      RedirectOutput.Target = null;
    }
    private static void RPC_Do_StopEvent(ZRpc rpc) {
      RedirectOutput.Target = rpc;
      if (IsAllowed(rpc))
        Console.instance.TryRunCommand(StopEvent);
      RedirectOutput.Target = null;
    }
    private static void RPC_Do_StartEvent(ZRpc rpc, string name) {
      RedirectOutput.Target = rpc;
      if (IsAllowed(rpc)) {
        if (!RandEventSystem.instance.HaveEvent(name)) {
          Console.instance.AddString("Random event not found:" + name);
          RedirectOutput.Target = null;
          return;
        }
        var peer = ZDOMan.instance.FindPeer(rpc);
        if (peer == null) {
          Console.instance.AddString("Player not found.");
          RedirectOutput.Target = null;
          return;
        }
        RandEventSystem.instance.SetRandomEventByName(name, peer.m_peer.m_refPos);
      }
      RedirectOutput.Target = null;
    }
    private static void RPC_Do_ResetKeys(ZRpc rpc) {
      RedirectOutput.Target = rpc;
      if (IsAllowed(rpc))
        Console.instance.TryRunCommand(ResetKeys);
      RedirectOutput.Target = null;
    }
    private static void RPC_Do_SetKey(ZRpc rpc, string name) {
      RedirectOutput.Target = rpc;
      if (IsAllowed(rpc))
        Console.instance.TryRunCommand(SetKey + " " + name);
      RedirectOutput.Target = null;
    }
    private static void RPC_Do_Sleep(ZRpc rpc) {
      RedirectOutput.Target = rpc;
      if (IsAllowed(rpc))
        Console.instance.TryRunCommand(Sleep);
      RedirectOutput.Target = null;
    }
    private static void RPC_Do_SkipTime(ZRpc rpc, string amount) {
      RedirectOutput.Target = rpc;
      if (IsAllowed(rpc))
        Console.instance.TryRunCommand(amount == "" ? SkipTime : SkipTime + " " + amount);
      RedirectOutput.Target = null;
    }
    public static void Postfix(ZRpc rpc) {
      if (ZNet.m_isServer) {
        rpc.Register(RPC_RandomEvent, new ZRpc.RpcMethod.Method(RPC_Do_RandomEvent));
        rpc.Register(RPC_StopEvent, new ZRpc.RpcMethod.Method(RPC_Do_StopEvent));
        rpc.Register(RPC_StartEvent, new Action<ZRpc, string>(RPC_Do_StartEvent));
        rpc.Register(RPC_ResetKeys, new ZRpc.RpcMethod.Method(RPC_Do_ResetKeys));
        rpc.Register(RPC_SetKey, new Action<ZRpc, string>(RPC_Do_SetKey));
        rpc.Register(RPC_Sleep, new ZRpc.RpcMethod.Method(RPC_Do_Sleep));
        rpc.Register(RPC_SkipTime, new Action<ZRpc, string>(RPC_Do_SkipTime));
      }
    }
  }

  [HarmonyPatch(typeof(Terminal), "AddString")]
  public class RedirectOutput {
    public static ZRpc Target = null;

    public static void Postfix(string text) {
      if (ZNet.m_isServer && Target != null) {
        ZNet.instance.RemotePrint(Target, text);
      }
    }


  }
}
