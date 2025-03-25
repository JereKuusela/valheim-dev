using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Splatform;
using UnityEngine;
namespace ServerDevcommands;
///<summary>Server side code to include private player positions.</summary>
[HarmonyPatch(typeof(ZNet), nameof(ZNet.UpdatePlayerList))]
public class Server_UpdatePrivatePositions
{
  static void Postfix(ZNet __instance)
  {
    if (!__instance.IsServer() || !Settings.ShowPrivatePlayers) return;
    var idToPeer = __instance.m_peers.Where(peer => peer.IsReady() && peer.m_characterID != ZDOID.None).ToDictionary(peer => peer.m_characterID, static peer => peer);
    for (var i = 0; i < __instance.m_players.Count; i++)
    {
      var player = __instance.m_players[i];
      if (player.m_characterID == __instance.m_characterID) continue;
      if (!idToPeer.TryGetValue(player.m_characterID, out var peer))
      {
        ServerDevcommands.Log.LogError("Unable to find the peer to set private position.");
        continue;
      }
      if (peer.m_publicRefPos) continue;
      player.m_position = peer.m_refPos;
      __instance.m_players[i] = player;
    }
  }
}
///<summary>Server side code to send private players.</summary>
[HarmonyPatch(typeof(ZNet), nameof(ZNet.SendPlayerList))]
public class SendPrivatePositionsToAdmins
{
  static void Postfix(ZNet __instance)
  {
    if (!__instance.IsServer() || !Settings.ShowPrivatePlayers) return;
    if (__instance.m_peers.Count == 0) return;
    SendToAdmins(__instance);
  }
  private static void SendToAdmins(ZNet obj)
  {
    var count = obj.m_players.Where(p => !p.m_publicPosition).Count();
    if (count == 0) return;
    var peers = obj.m_peers.Where(peer => peer.IsReady() && obj.IsAdmin(peer.m_rpc.GetSocket().GetHostName())).ToList();
    if (peers.Count == 0) return;
    ZPackage pkg = new();
    pkg.Write(count);
    foreach (var info in obj.m_players)
    {
      if (info.m_publicPosition) continue;
      pkg.Write(info.m_characterID);
      pkg.Write(info.m_position);
    }
    foreach (var peer in peers)
      peer.m_rpc.Invoke("DEV_PrivatePlayerList2", [pkg]);
  }
}
///<summary>Client side code to receive private players.</summary>
[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
public class RegisterRpcPrivatePositions
{
  public static readonly Dictionary<ZDOID, Vector3> PrivatePositions = [];
  private static void RPC_PrivatePlayerList(ZRpc rpc, ZPackage pkg)
  {
    if (!Settings.ShowPrivatePlayers) return;
    var length = pkg.ReadInt();
    for (var i = 0; i < length; i++)
    {
      var id = pkg.ReadZDOID();
      var pos = pkg.ReadVector3();
      PrivatePositions[id] = pos;
    }
    for (var i = 0; i < ZNet.instance.m_players.Count; i++)
    {
      if (ZNet.instance.m_players[i].m_publicPosition) continue;
      if (PrivatePositions.TryGetValue(ZNet.instance.m_players[i].m_characterID, out var pos))
        ZNet.instance.m_players[i] = ZNet.instance.m_players[i] with { m_position = pos };
    }
  }
  static void Postfix(ZNet __instance, ZRpc rpc)
  {
    if (__instance.IsServer()) return;
    rpc.Register<ZPackage>("DEV_PrivatePlayerList2", new(RPC_PrivatePlayerList));
  }
}

///<summary>Remove filtering from the map.</summary>
[HarmonyPatch(typeof(ZNet), nameof(ZNet.GetOtherPublicPlayers))]
public class IncludePrivatePlayersInTheMap
{
  static void Postfix(ZNet __instance, List<ZNet.PlayerInfo> playerList)
  {
    if (!Settings.ShowPrivatePlayers) return;
    foreach (var playerInfo in __instance.m_players)
    {
      if (playerInfo.m_publicPosition) continue;
      var id = playerInfo.m_characterID;
      if (id.IsNone()) continue;
      if (playerInfo.m_characterID == __instance.m_characterID) continue;
      if (playerInfo.m_position == Vector3.zero) continue;
      playerList.Add(playerInfo);
    }
  }
}
///<summary>Simple way to distinguish private players.</summary>
[HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdatePlayerPins))]
public class AddCheckedToPrivatePlayers
{
  static void Postfix(Minimap __instance)
  {
    if (!Settings.ShowPrivatePlayers) return;
    for (int i = 0; i < __instance.m_tempPlayerInfo.Count; i++)
    {
      var pin = __instance.m_playerPins[i];
      var info = __instance.m_tempPlayerInfo[i];
      pin.m_checked = !info.m_publicPosition;
    }
  }
}
