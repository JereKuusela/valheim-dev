using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using static ZRpc;

namespace ServerDevcommands;

[HarmonyPatch(typeof(Terminal), nameof(Terminal.AddString), typeof(string))]
public class RedirectOutput
{
  public static ZRpc? Target = null;

  static void Postfix(string text)
  {
    if (ZNet.m_isServer && Target != null)
    {
      ZNet.instance.RemotePrint(Target, text);
    }
  }
}

// Base game has a way to execute commands on the server, but there is no admin check on it.
[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
public class ServerExecution
{

  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(string command)
  {
    var server = ZNet.instance.GetServerRPC();
    Console.instance.AddString("Sending command: " + command);
    server?.Invoke(RPC_Command, command);
  }
  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(IEnumerable<string> args) => Send(string.Join(" ", args));
  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(Terminal.ConsoleEventArgs args) => Send(args.Args);

  public static string RPC_Command = "DEV_Command";
  public static string RPC_Pins = "DEV_Pins";
  public static string RPC_RequestIds = "EW_RequestIds";
  public static string RPC_SyncLocationIds = "EW_SyncLocationIds";
  public static string RPC_SyncVegetationIds = "EW_SyncVegetationIds";
  private static bool IsAllowed(ZRpc rpc, string command)
  {
    var zNet = ZNet.instance;
    if (!zNet.enabled)
    {
      return false;
    }
    if (rpc != null && !zNet.IsAdmin(rpc.GetSocket().GetHostName()))
    {
      Console.instance.AddString("Unauthorized to use devcommands.");
      return false;
    }
    if (!DisableCommands.CanRun(command, rpc))
    {
      Console.instance.AddString("Unauthorized to use this command.");
      return false;
    }
    return true;
  }
  private static void RPC_Do_Command(ZRpc rpc, string command)
  {
    RedirectOutput.Target = rpc;
    if (IsAllowed(rpc, command))
      Console.instance.TryRunCommand(command);
    RedirectOutput.Target = null;
  }
  public static void RPC_Do_Pins(ZRpc? rpc, string data)
  {
    var pins = Parse.Split(data, '|').Select(Parse.VectorXZY).ToArray();
    var findPins = Console.instance.m_findPins;
    foreach (var pin in findPins)
      Minimap.instance?.RemovePin(pin);
    findPins.Clear();
    foreach (var pos in pins)
    {
      var pin = Minimap.instance?.AddPin(pos, Minimap.PinType.Icon3, "", false, false, Player.m_localPlayer.GetPlayerID());
      if (pin != null)
        findPins.Add(pin);
    }
  }

  public static void RequestIds()
  {
    var server = ZNet.instance.GetServerRPC();
    if (server == null)
      return;
    server.Invoke(RPC_RequestIds);
  }

  public static void ReceiveLocationIds(ZRpc rpc, string locationIds)
  {
    ParameterInfo.SetServerLocationIds([.. locationIds.Split('|').Where(s => !string.IsNullOrEmpty(s))]);
  }
  public static void ReceiveVegetationIds(ZRpc rpc, string vegetationIds)
  {
    ParameterInfo.SetServerVegetationIds([.. vegetationIds.Split('|').Where(s => !string.IsNullOrEmpty(s))]);
  }

  static void Postfix(ZNet __instance, ZRpc rpc)
  {
    ZNetPeer peer = __instance.GetPeer(rpc);
    if (peer.m_uid == 0)
      return;
    if (__instance.IsServer())
    {
      rpc.Register<string>(RPC_Command, RPC_Do_Command);
    }
    else
    {
      rpc.Register<string>(RPC_Pins, RPC_Do_Pins);
      rpc.Register<string>(RPC_SyncLocationIds, ReceiveLocationIds);
      rpc.Register<string>(RPC_SyncVegetationIds, ReceiveVegetationIds);
    }
  }
}
