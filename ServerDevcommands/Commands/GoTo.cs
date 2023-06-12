using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ServerDevcommands;
///<summary>Adds support for the y coordinate.</summary>
public class GotoCommand {
  private Vector3? LastPosition = null;
  private Quaternion? LastRotation = null;
  private void ParseArgs(Terminal.ConsoleEventArgs args, Player player, ref Vector3 pos, ref Quaternion rot) {
    if (args.Length < 2) {
      pos.y = WorldGenerator.instance.GetHeight(pos.x, pos.z);
      return;
    }
    if (args.Length == 2 && args[1] == "last") {
      if (LastPosition == null)
        throw new InvalidOperationException("No last position");
      pos = LastPosition ?? pos;
      rot = LastRotation ?? rot;
      return;
    }
    var split = Parse.Split(args[1]);
    if (args.Length > 2)
      split = args.Args.Skip(1).ToArray();
    if (Parse.TryFloat(split[0], out var value)) {
      if (split.Length < 2) {
        pos.y = value;
      } else {
        var posXZ = Parse.VectorXZY(split);
        var y = player.IsDebugFlying() ? player.transform.position.y : WorldGenerator.instance.GetHeight(posXZ.x, posXZ.z);
        pos = Parse.VectorXZY(split, new Vector3(pos.x, y, pos.z));
      }
      return;
    }
    var info = Helper.FindPlayer(string.Join(" ", args.Args.Skip(1)));
    pos = info.m_position;
  }
  public GotoCommand() {
    AutoComplete.Register("goto", (int index, int subIndex) => {
      if (index == 0 && subIndex == 0) return ParameterInfo.PublicPlayerNames;
      if (index == 0) return ParameterInfo.XZY("Coordinates", subIndex);
      return ParameterInfo.XZY("Coordinates", index);

    });
    Helper.Command("goto", "[x,z,y or altitude or last or player or no parameter] - Teleports to the coordinates. If y is not given, teleports to the ground level.", (args) => {
      var player = Helper.GetPlayer();
      var pos = player.transform.position;
      var rot = player.transform.rotation;
      ParseArgs(args, player, ref pos, ref rot);
      LastPosition = player.transform.position;
      LastRotation = player.transform.rotation;
      player.TeleportTo(pos, rot, true);
      Helper.AddMessage(args.Context, $"Teleported to (X,Z,Y): {pos.x}, {pos.z}, {pos.y}.");
    });
  }
}


[HarmonyPatch(typeof(Player), nameof(Player.TeleportTo))]
public class FasterTeleport1 {
  static void Postfix(Player __instance, bool __result) {
    if (Settings.DebugModeFastTeleport && __result && Player.m_debugMode)
      __instance.m_teleportTimer = 15f;
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdateTeleport))]
public class FasterTeleport2 {
  static void Postfix(Player __instance) {
    if (Settings.DebugModeFastTeleport && Player.m_debugMode)
      __instance.m_teleportCooldown = Mathf.Max(__instance.m_teleportCooldown, 1.5f);
  }
}


