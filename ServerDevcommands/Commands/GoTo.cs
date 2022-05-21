using System;
using System.Linq;
using UnityEngine;
namespace ServerDevcommands;
///<summary>Adds support for the y coordinate.</summary>
public class GotoCommand {
  public GotoCommand() {
    AutoComplete.Register("goto", (int index, int subIndex) => {
      if (index == 0) return ParameterInfo.XZY("Coordinates", subIndex);
      return ParameterInfo.XZY("Coordinates", index);
    });
    Helper.Command("goto", "[x,z,y] - Teleports to the coordinates. If y is not given, teleports to the ground level.", (args) => {
      Helper.ArgsCheck(args, 2, "Missing the coordinates.");
      var player = Helper.GetPlayer();
      var y = player.IsDebugFlying() ? player.transform.position.y : ZoneSystem.instance.m_waterLevel;
      var split = Parse.Split(args[1]);
      if (args.Length > 2)
        split = args.Args.Skip(1).ToArray();

      if (split.Length < 2) throw new InvalidOperationException("Missing the z coordinate.");
      var pos = Parse.TryVectorXZY(split, new Vector3(0f, y, 0f));
      player.TeleportTo(pos, player.transform.rotation, true);
      Helper.AddMessage(args.Context, $"Teleported to (X,Z,Y): {pos.x}, {pos.z}, {pos.y}.");
    });
  }
}
