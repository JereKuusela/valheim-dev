using System;
using Service;
using UnityEngine;

namespace ServerDevcommands;
///<summary>Adds output when used without the parameter.</summary>
public class TeleportCommand
{
  public TeleportCommand()
  {
    AutoComplete.Register("tp", static (int index, int subIndex) =>
    {
      if (index == 0) return ParameterInfo.PlayerNames;
      if (index == 1) return subIndex == 0 ? ParameterInfo.PublicPlayerNames : ParameterInfo.XZYR("Coordinates", subIndex);
      if (index == 2) return ParameterInfo.Flag("fast", subIndex);
      return ParameterInfo.None;
    });
    Helper.Command("tp", "[player1,player2,...] [x,z,y,rot/player] [fast=false] - Teleports the player to coordinates or another player.", static (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing player id/name.");
      var players = PlayerInfo.FindPlayers(Parse.Split(args[1]));
      Vector3 pos = Player.m_localPlayer ? Player.m_localPlayer.transform.position : Vector3.zero;
      Quaternion rot = Player.m_localPlayer ? Player.m_localPlayer.transform.rotation : Quaternion.identity;
      if (args.Length > 2)
      {
        var split = Parse.Split(args[2]);
        if (split.Length > 1)
        {
          pos = Parse.VectorXZY(split);
          var ground = WorldGenerator.instance.GetHeight(pos.x, pos.z);
          pos.y = Mathf.Max(pos.y, ground);
          if (split.Length > 3)
          {
            var y = Parse.Float(split[3], 0f);
            rot = Quaternion.Euler(0f, y, 0f);
          }
        }
        else
        {
          var target = PlayerInfo.FindPlayers(split);
          if (target.Count == 0) throw new InvalidOperationException($"No target player found with id/name '{args[2]}'.");
          pos = target[0].Pos;
          rot = target[0].Rot;
        }
      }
      var fastTeleport = args.Length > 3 && args[3].Equals("fast", StringComparison.OrdinalIgnoreCase);
      foreach (var player in players)
      {
        ZRoutedRpc.instance.InvokeRoutedRPC(0, player.ZDOID, "RPC_TeleportTo", [pos, rot, !fastTeleport]);
      }
    });
  }
}
