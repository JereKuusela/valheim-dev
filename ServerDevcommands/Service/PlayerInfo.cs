using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Service;


public class PlayerInfo
{
  public string Name;
  public Vector3 Pos;
  public Quaternion Rot;
  public long PeerId;
  public string HostId;
  public ZDOID ZDOID;
  public PlayerInfo(ZNetPeer peer)
  {
    Name = peer.m_playerName;
    Pos = peer.m_refPos;
    Rot = Quaternion.identity;
    PeerId = peer.m_uid;
    ZDOID = peer.m_characterID;
    HostId = peer.m_rpc.GetSocket().GetHostName();

  }
  public PlayerInfo(Player player)
  {
    Name = player.GetPlayerName();
    Pos = player.transform.position;
    Rot = player.transform.rotation;
    PeerId = ZNet.GetUID();
    ZDOID = player.GetZDOID();
    HostId = "server";
  }

  public static List<PlayerInfo> FindPlayers(string[] args)
  {
    List<PlayerInfo> players = ZNet.instance.GetPeers().Select(peer => new PlayerInfo(peer)).ToList();
    if (ZNet.instance.IsServer() && !ZNet.instance.IsDedicated() && Player.m_localPlayer)
      players.Add(new(Player.m_localPlayer));

    Dictionary<long, PlayerInfo> foundPlayers = [];
    foreach (var argu in args)
    {
      if (argu == "*") return players;
      var arg = argu.ToLowerInvariant();
      foreach (var player in players)
      {
        var name = player.Name.ToLowerInvariant();
        if (player.HostId == argu)
          foundPlayers[player.PeerId] = player;
        else if (player.PeerId.ToString() == argu || name == arg)
          foundPlayers[player.PeerId] = player;
        else if (arg[0] == '*' && arg[arg.Length - 1] == '*' && name.Contains(arg.Substring(1, arg.Length - 2)))
          foundPlayers[player.PeerId] = player;
        else if (arg[0] == '*' && player.Name.EndsWith(arg.Substring(1), StringComparison.OrdinalIgnoreCase))
          foundPlayers[player.PeerId] = player;
        else if (arg[arg.Length - 1] == '*' && player.Name.StartsWith(arg.Substring(0, arg.Length - 1), StringComparison.OrdinalIgnoreCase))
          foundPlayers[player.PeerId] = player;
      }
    }
    return [.. foundPlayers.Values];
  }
}