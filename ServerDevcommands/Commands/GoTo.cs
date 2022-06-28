using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
///<summary>Adds support for the y coordinate.</summary>
public class GotoCommand {
  public GotoCommand() {
    AutoComplete.Register("goto", (int index, int subIndex) => {
      if (index == 0) return ParameterInfo.XZY("Coordinates", subIndex);
      return ParameterInfo.XZY("Coordinates", index);
    });
    Helper.Command("goto", "[x,z,y or altitude or no parameter] - Teleports to the coordinates. If y is not given, teleports to the ground level.", (args) => {
      var player = Helper.GetPlayer();
      Vector3 pos = player.transform.position;
      if (args.Length < 2) {
        pos.y = WorldGenerator.instance.GetHeight(pos.x, pos.z);
      } else {
        var split = Parse.Split(args[1]);
        if (args.Length > 2)
          split = args.Args.Skip(1).ToArray();

        if (split.Length < 2)
          pos.y = Parse.TryFloat(split, 0, WorldGenerator.instance.GetHeight(pos.x, pos.z));
        else {
          var posXZ = Parse.TryVectorXZY(split);
          var y = player.IsDebugFlying() ? player.transform.position.y : WorldGenerator.instance.GetHeight(posXZ.x, posXZ.z);
          pos = Parse.TryVectorXZY(split, new Vector3(pos.x, y, pos.z));
        }
      }
      player.TeleportTo(pos, player.transform.rotation, true);
      Helper.AddMessage(args.Context, $"Teleported to (X,Z,Y): {pos.x}, {pos.z}, {pos.y}.");
    });
  }
}


[HarmonyPatch(typeof(Player), nameof(Player.TeleportTo))]
public class FasterTeleport {
  static void Postfix(Player __instance, bool __result) {
    if (Settings.DebugModeFastTeleport && __result && Player.m_debugMode)
      __instance.m_teleportTimer = 15f;
  }
}



