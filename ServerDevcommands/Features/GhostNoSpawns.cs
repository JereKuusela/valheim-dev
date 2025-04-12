using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ServerDevcommands;

[HarmonyPatch]
public class GhostIgnorePlayers
{
  private static List<Player> Players()
  {
    if (!Settings.GhostNoSpawns) return Player.s_players;
    return Player.s_players.Where(p => !Ghost.IsGhost(p)).ToList();
  }

  static IEnumerable<CodeInstruction> ReplacePlayers(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
         .MatchStartForward(
             new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(Player), nameof(Player.s_players))))
         .SetAndAdvance(
              OpCodes.Call, Transpilers.EmitDelegate(Players).operand)
         .InstructionEnumeration();
  }
  [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.GetPlayersInZone)), HarmonyPostfix]
  static void GhostNoSpawns(List<Player> players)
  {
    if (!Settings.GhostNoSpawns) return;
    var toRemove = players.Where(Ghost.IsGhost).ToList();
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
