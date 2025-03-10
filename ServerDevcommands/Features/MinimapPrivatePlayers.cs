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
    ZPackage pkg = new();
    var count = Settings.ServerClient ? obj.m_players.Count + 1 : obj.m_players.Count;
    pkg.Write(count);
    if (Settings.ServerClient)
      ServerChat.Write(pkg, true);
    foreach (var info in obj.m_players)
    {
      pkg.Write(info.m_name);
      pkg.Write(info.m_characterID);
      pkg.Write(info.m_userInfo.m_id.ToString());
      pkg.Write(info.m_userInfo.m_displayName);
      pkg.Write(info.m_serverAssignedDisplayName);
      pkg.Write(info.m_publicPosition);
      pkg.Write(info.m_position);
    }
    foreach (var peer in obj.m_peers)
    {
      if (!peer.IsReady()) continue;
      var rpc = peer.m_rpc;
      if (!obj.IsAdmin(rpc.GetSocket().GetHostName())) continue;
      rpc.Invoke("DEV_PrivatePlayerList", [pkg]);
    }
  }
}
///<summary>Client side code to receive private players.</summary>
[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
public class RegisterRpcPrivatePositions
{
  private static void RPC_PrivatePlayerList(ZRpc rpc, ZPackage pkg)
  {
    if (!Settings.ShowPrivatePlayers)
    {
      IgnoreDefaultList.Active = false;
      return;
    }
    IgnoreDefaultList.Active = true;
    var obj = ZNet.instance;
    obj.m_players.Clear();
    var length = pkg.ReadInt();
    for (var i = 0; i < length; i++)
    {
      ZNet.PlayerInfo info = default;
      info.m_name = pkg.ReadString();
      info.m_characterID = pkg.ReadZDOID();
      info.m_userInfo.m_id = new PlatformUserID(pkg.ReadString());
      info.m_userInfo.m_displayName = pkg.ReadString();
      info.m_serverAssignedDisplayName = pkg.ReadString();
      info.m_publicPosition = pkg.ReadBool();
      info.m_position = pkg.ReadVector3();
      obj.m_players.Add(info);
    }
  }
  static void Postfix(ZNet __instance, ZRpc rpc)
  {
    if (__instance.IsServer()) return;
    rpc.Register<ZPackage>("DEV_PrivatePlayerList", new(RPC_PrivatePlayerList));
  }
}
///<summary>Two RPC calls modifying the player list may lead to glitches (especially with poor network conditions).</summary>
[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PlayerList))]
public class IgnoreDefaultList
{
  public static bool Active = false;
  static bool Prefix() => !Active;
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
      var characterID = playerInfo.m_characterID;
      if (!characterID.IsNone() && !(playerInfo.m_characterID == __instance.m_characterID) && playerInfo.m_position != Vector3.zero)
      {
        playerList.Add(playerInfo);
      }
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
