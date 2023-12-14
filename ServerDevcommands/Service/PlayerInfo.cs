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
  public long Character;
  public string HostId;
  public ZDOID ZDOID;
  public PlayerInfo(ZNetPeer peer)
  {
    HostId = peer.m_rpc.GetSocket().GetHostName();
    Name = peer.m_playerName;
    Pos = peer.m_refPos;
    ZDOID = peer.m_characterID;
    var zdo = ZDOMan.instance.GetZDO(peer.m_characterID);
    if (zdo != null)
    {
      Character = zdo.GetLong(ZDOVars.s_playerID, 0L);
      Pos = zdo.m_position;
      Rot = zdo.GetRotation();
    }
  }
  public PlayerInfo(Player player)
  {
    HostId = "self";
    Name = player.GetPlayerName();
    ZDOID = player.GetZDOID();
    Character = player.GetPlayerID();
    Pos = player.transform.position;
    Rot = player.transform.rotation;
  }

  public static List<PlayerInfo> FindPlayers(string[] args)
  {
    List<PlayerInfo> players = ZNet.instance.GetPeers().Select(peer => new PlayerInfo(peer)).ToList();
    if (Player.m_localPlayer)
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
          foundPlayers[player.Character] = player;
        else if (player.Character.ToString() == argu || name == arg)
          foundPlayers[player.Character] = player;
        else if (arg[0] == '*' && arg[arg.Length - 1] == '*' && name.Contains(arg.Substring(1, arg.Length - 2)))
          foundPlayers[player.Character] = player;
        else if (arg[0] == '*' && player.Name.EndsWith(arg.Substring(1), StringComparison.OrdinalIgnoreCase))
          foundPlayers[player.Character] = player;
        else if (arg[arg.Length - 1] == '*' && player.Name.StartsWith(arg.Substring(0, arg.Length - 1), StringComparison.OrdinalIgnoreCase))
          foundPlayers[player.Character] = player;
      }
    }
    List<PlayerInfo> ret = [.. foundPlayers.Values];
    if (ret.Count == 0) throw new InvalidOperationException($"No target player found with id/name '{string.Join(",", args)}'.");
    return ret;
  }
}