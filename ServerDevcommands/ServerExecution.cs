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
[HarmonyPatch]
public class ServerExecution
{
  [HarmonyPatch(typeof(ZNet), nameof(ZNet.InternalCommand)), HarmonyPrefix]
  static bool InternalCommandPrefix(ZNet __instance, ZRpc rpc, string command)
  {
    if (!__instance.IsServer())
      return true;

    RedirectOutput.Target = rpc;
    if (!IsRpcCommandAllowed(__instance, rpc, command))
    {
      __instance.RemotePrint(rpc, $"Unauthorized to use command '{GetCommandName(command)}'.");
      return false;
    }

    Console.instance.TryRunCommand(command);
    return false;
  }
  [HarmonyPatch(typeof(ZNet), nameof(ZNet.InternalCommand)), HarmonyFinalizer]
  static void InternalCommandFinalizer()
  {
    RedirectOutput.Target = null;
  }

  private static bool IsRpcCommandAllowed(ZNet znet, ZRpc rpc, string command)
  {
    // Local server-side invocations (no rpc sender) keep base behavior.
    if (rpc == null)
      return true;

    var commandName = GetCommandName(command);
    if (!Terminal.commands.TryGetValue(commandName, out var cmd))
      return false;

    var peer = znet.GetPeer(rpc);
    if (peer == null)
      return false;

    var hostname = rpc.GetSocket().GetHostName();
    var characterId = peer.m_characterID.UserID.ToString();
    var permissions = PermissionLoader.Data.Resolve(hostname, characterId);
    return permissions.IsCommandAllowed(cmd, command);
  }

  private static string GetCommandName(string command)
  {
    var kvp = Parse.Kvp(command, ' ');
    return kvp.Key;
  }


  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(string command)
  {
    Console.instance.AddString("Sending command: " + command);
    ZNet.instance.RemoteCommand(command);
  }
  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(IEnumerable<string> args) => Send(string.Join(" ", args));
  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(Terminal.ConsoleEventArgs args) => Send(args.Args);

  public static string RPC_Pins = "DEV_Pins";
  public static string RPC_RequestIds = "EW_RequestIds";
  public static string RPC_SyncLocationIds = "EW_SyncLocationIds";
  public static string RPC_SyncVegetationIds = "EW_SyncVegetationIds";

  public static void RPC_Do_Pins(long sender, string data)
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

  public static void RPC_DoRequestIds(ZRpc rpc)
  {
    var locationIds = string.Join("|", ParameterInfo.LocationIds);
    var vegetationIds = string.Join("|", ParameterInfo.VegetationIds);
    rpc.Invoke(RPC_SyncLocationIds, locationIds);
    rpc.Invoke(RPC_SyncVegetationIds, vegetationIds);
  }

  public static void ReceiveLocationIds(long sender, string locationIds)
  {
    ParameterInfo.SetServerLocationIds([.. locationIds.Split('|').Where(s => !string.IsNullOrEmpty(s))]);
  }
  public static void ReceiveVegetationIds(long sender, string vegetationIds)
  {
    ParameterInfo.SetServerVegetationIds([.. vegetationIds.Split('|').Where(s => !string.IsNullOrEmpty(s))]);
  }

  [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo)), HarmonyPostfix]
  static void RegisterRPCs(ZNet __instance, ZRpc rpc)
  {
    if (!__instance.IsServer())
      return;
    ZNetPeer peer = __instance.GetPeer(rpc);
    rpc.Register(RPC_RequestIds, RPC_DoRequestIds);
  }

  // Base game also registers RPCs on ZoneSystem. At least this ensures that ZNet is fully initialized.
  [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start)), HarmonyPostfix]
  static void InitServer()
  {
    if (ZNet.instance.IsServer())
    {
      PermissionLoader.FromFile();
    }
    else
    {
      ZRoutedRpc.instance.Register<string>(RPC_Pins, RPC_Do_Pins);
      ZRoutedRpc.instance.Register<string>(RPC_SyncLocationIds, ReceiveLocationIds);
      ZRoutedRpc.instance.Register<string>(RPC_SyncVegetationIds, ReceiveVegetationIds);
      ZRoutedRpc.instance.Register<ZPackage>(RPC_Unban.RPC_Permissions, Admin.ReceivePermissions);
    }
  }
}
