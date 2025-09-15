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
  public string HostId;
  public long PeerId;
  public ZDOID ZDOID;
  public PlayerInfo(ZNetPeer peer)
  {
    HostId = peer.m_rpc.GetSocket().GetHostName();
    Name = peer.m_playerName;
    Pos = peer.m_refPos;
    ZDOID = peer.m_characterID;
    PeerId = ZDOID.UserID;
    var zdo = ZDOMan.instance.GetZDO(peer.m_characterID);
    if (zdo != null)
    {
      Pos = zdo.m_position;
      Rot = zdo.GetRotation();
    }
  }
  public PlayerInfo(ZNet.PlayerInfo info)
  {
    HostId = info.m_userInfo.m_id.ToString();
    Name = info.m_name;
    Pos = info.m_position;
    ZDOID = info.m_characterID;
    PeerId = ZDOID.UserID;
    var zdo = ZDOMan.instance.GetZDO(info.m_characterID);
    if (zdo != null)
    {
      Pos = zdo.m_position;
      Rot = zdo.GetRotation();
    }
  }
  public PlayerInfo(Player player)
  {
    HostId = "self";
    Name = player.GetPlayerName();
    ZDOID = player.GetZDOID();
    PeerId = ZDOID.UserID;
    Pos = player.transform.position;
    Rot = player.transform.rotation;
  }

  public static List<PlayerInfo> FindPlayers(string[] args)
  {
    List<PlayerInfo> players = ZNet.instance.IsServer()
      ? ZNet.instance.GetPeers().Select(peer => new PlayerInfo(peer)).ToList()
      : ZNet.instance.m_players.Select(player => new PlayerInfo(player)).ToList();
    if (Player.m_localPlayer && players.All(p => p.ZDOID != Player.m_localPlayer.GetZDOID()))
      players.Add(new(Player.m_localPlayer));

    Dictionary<ZDOID, PlayerInfo> foundPlayers = [];
    foreach (var argu in args)
    {
      if (argu == "*" || argu == "all") return players;
      if (argu == "others") return [.. players.Where(p => p.ZDOID != Player.m_localPlayer?.GetZDOID())];
      var arg = argu.ToLowerInvariant();
      foreach (var player in players)
      {
        var name = player.Name.ToLowerInvariant();
        if (player.HostId == argu)
          foundPlayers[player.ZDOID] = player;
        else if (name == arg)
          foundPlayers[player.ZDOID] = player;
        else if (arg[0] == '*' && arg[arg.Length - 1] == '*' && name.Contains(arg.Substring(1, arg.Length - 2)))
          foundPlayers[player.ZDOID] = player;
        else if (arg[0] == '*' && player.Name.EndsWith(arg.Substring(1), StringComparison.OrdinalIgnoreCase))
          foundPlayers[player.ZDOID] = player;
        else if (arg[arg.Length - 1] == '*' && player.Name.StartsWith(arg.Substring(0, arg.Length - 1), StringComparison.OrdinalIgnoreCase))
          foundPlayers[player.ZDOID] = player;
      }
    }
    List<PlayerInfo> ret = [.. foundPlayers.Values];
    if (ret.Count == 0) throw new InvalidOperationException($"No target player found with id/name '{string.Join(",", args)}'.");
    return ret;
  }
}