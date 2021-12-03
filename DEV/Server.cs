using System;
using System.Collections.Generic;
using HarmonyLib;
using Service;

namespace DEV {

  [HarmonyPatch(typeof(Terminal), "AddString")]
  public class RedirectOutput {
    public static ZRpc Target = null;

    public static void Postfix(string text) {
      if (ZNet.m_isServer && Target != null) {
        ZNet.instance.RemotePrint(Target, text);
      }
    }
  }

  /// <summary>Registers the server to accept resetkeys message (like clients do).</summary>
  [HarmonyPatch(typeof(ZoneSystem), "Start")]
  public class Faa {
    public static void Postfix(ZoneSystem __instance) {
      if (ZNet.instance.IsServer()) {
        ZRoutedRpc.instance.Register<List<string>>("GlobalKeys", new Action<long, List<string>>(__instance.RPC_GlobalKeys));
      }
    }
  }

  [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
  public class ServerCommands {
    public static string RPC_Command = "DEV_Command";
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
    private static void RPC_Do_Command(ZRpc rpc, string command) {
      RedirectOutput.Target = rpc;
      if (IsAllowed(rpc))
        Console.instance.TryRunCommand(command);
      RedirectOutput.Target = null;
    }
    public static void Postfix(ZNet __instance, ZRpc rpc) {
      if (__instance.IsServer()) {
        rpc.Register<string>(RPC_Command, new Action<ZRpc, string>(RPC_Do_Command));
      }
    }
  }
}
