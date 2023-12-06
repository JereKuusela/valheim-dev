using System;
using Service;
using UnityEngine;

namespace ServerDevcommands;
///<summary>Adds output when used without the parameter.</summary>
public class TeleportCommand
{
  public TeleportCommand()
  {
    AutoComplete.Register("tp", (int index, int subIndex) =>
    {
      if (index == 0) return ParameterInfo.PlayerNames;
      if (index == 1) return ParameterInfo.XZY("Target coordinates", subIndex);
      if (index == 2) return ParameterInfo.Create("Target rotation in degrees.");
      if (index == 3) return ParameterInfo.Flag("fast");
      return ParameterInfo.None;
    });
    Helper.Command("tp", "[player1,player2,...] [x,z,y/player] [rotY=0] [fast=false] - Teleports the player to coordinates or another player.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing player id/name.");
      Helper.ArgsCheck(args, 3, "Missing coordinates.");
      var players = PlayerInfo.FindPlayers(Parse.Split(args[1]));
      Quaternion rot = Quaternion.identity;
      if (args.Length > 3)
      {
        var y = Parse.Float(args[3], 0f);
        rot = Quaternion.Euler(0f, y, 0f);
      }

      var split = Parse.Split(args[2]);
      Vector3 pos;
      if (split.Length > 1)
      {
        pos = Parse.VectorXZY(split);
        var ground = WorldGenerator.instance.GetHeight(pos.x, pos.z);
        pos.y = Mathf.Max(pos.y, ground);
      }
      else
      {
        var target = PlayerInfo.FindPlayers(split);
        if (target.Count == 0) throw new InvalidOperationException($"No target player found with id/name '{args[2]}'.");
        pos = target[0].Pos;
        rot = target[0].Rot;
      }
      var fastTeleport = args.Length > 4 && args[4].Equals("fast", StringComparison.OrdinalIgnoreCase);
      foreach (var player in players)
      {
        ZRoutedRpc.instance.InvokeRoutedRPC(player.PeerId, "RPC_TeleportPlayer", [pos, rot, !fastTeleport]);
      }
    });
  }
}
