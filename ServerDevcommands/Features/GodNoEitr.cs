using System;
using HarmonyLib;

namespace ServerDevcommands;
[HarmonyPatch(typeof(Player))]
public class GodNoEitr {
  [HarmonyPatch(nameof(Player.HaveEitr)), HarmonyPostfix]
  static void AlwaysHaveEitr(Player __instance, ref bool __result) {
    var noUsage = Settings.GodModeNoEitr && __instance.InGodMode();
    __result |= noUsage;
  }
  private static bool IsAttacking = false;
  [HarmonyPatch(typeof(Attack), nameof(Attack.Start)), HarmonyPrefix]
  static void AttackStart(Humanoid character) {
    if (character == Player.m_localPlayer)
      IsAttacking = true;
  }
  [HarmonyPatch(typeof(Attack), nameof(Attack.Start)), HarmonyPostfix]
  static void AttackEnd(Humanoid character) {
    if (character == Player.m_localPlayer)
      IsAttacking = false;
  }
  [HarmonyPatch(nameof(Player.GetMaxEitr)), HarmonyPostfix]
  static void AlwaysHaveMaxEitr(Player __instance, ref float __result) {
    var noUsage = Settings.GodModeNoEitr && __instance.InGodMode();
    if (noUsage && IsAttacking) __result = Math.Max(__result, 0.1f);
  }
  [HarmonyPatch(nameof(Player.UseEitr)), HarmonyPrefix]
  static bool NoEitrUsage(Player __instance) {
    var noUsage = Settings.GodModeNoEitr && __instance.InGodMode();
    return !noUsage;
  }
}
