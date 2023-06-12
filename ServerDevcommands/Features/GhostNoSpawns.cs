using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ServerDevcommands;

[HarmonyPatch]
public class GhostIgnorePlayers {
  private static List<Player> Players() {
    return Player.s_players.Where(p => !p.InGhostMode()).ToList();
  }

  static IEnumerable<CodeInstruction> ReplacePlayers(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
         .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(Player), nameof(Player.s_players))))
         .SetAndAdvance(
              OpCodes.Call, Transpilers.EmitDelegate(Players).operand)
         .InstructionEnumeration();
  }
  [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.GetPlayersInZone)), HarmonyPostfix]
  static void GhostNoSpawns(List<Player> players) {
    var toRemove = players.Where(p => p.InGhostMode()).ToList();
    foreach (var player in toRemove)
      players.Remove(player);
  }
  [HarmonyPatch(typeof(Player), nameof(Player.GetClosestPlayer)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> GetClosestPlayer(IEnumerable<CodeInstruction> i) => ReplacePlayers(i);

  [HarmonyPatch(typeof(Player), nameof(Player.IsPlayerInRange), typeof(Vector3), typeof(float)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> IsPlayerInRange(IEnumerable<CodeInstruction> i) => ReplacePlayers(i);

  [HarmonyPatch(typeof(Player), nameof(Player.IsPlayerInRange), typeof(Vector3), typeof(float), typeof(float)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> IsPlayerInRange2(IEnumerable<CodeInstruction> i) => ReplacePlayers(i);

}
