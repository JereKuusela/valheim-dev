using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace ServerDevcommands;

[HarmonyPatch(typeof(Terminal), nameof(Terminal.AddString), new[] { typeof(string) })]
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

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.ClearGlobalKeys))]
public class ResetServerKeys
{
  static void Postfix()
  {
    ZNet.World?.m_startingGlobalKeys.Clear();
  }
}
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
  private static bool IsAllowed(ZRpc rpc, string command)
  {
    var zNet = ZNet.instance;
    if (!zNet.enabled)
    {
      return false;
    }
    if (rpc != null && !zNet.ListContainsId(zNet.m_adminList, rpc.GetSocket().GetHostName()))
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
  private static void RPC_Do_Pins(ZRpc rpc, string data)
  {
    var pins = Parse.Split(data, '|').Select(Parse.VectorXZY).ToArray();
    var findPins = Console.instance.m_findPins;
    foreach (var pin in findPins)
      Minimap.instance?.RemovePin(pin);
    findPins.Clear();
    if (pins.Length == 1)
    {
      Chat.instance?.SendPing(pins[0]);
      return;
    }
    foreach (var pos in pins)
    {
      var pin = Minimap.instance?.AddPin(pos, Minimap.PinType.Icon3, "", false, false, Player.m_localPlayer.GetPlayerID());
      if (pin != null)
        findPins.Add(pin);
    }
  }

  static void Postfix(ZNet __instance, ZRpc rpc)
  {
    if (__instance.IsServer())
    {
      rpc.Register<string>(RPC_Command, new(RPC_Do_Command));
    }
    else
    {
      rpc.Register<string>(RPC_Pins, new(RPC_Do_Pins));
    }
  }
}
