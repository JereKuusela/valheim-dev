using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Player))]
public class GodNoEitr
{
  [HarmonyPatch(nameof(Player.HaveEitr)), HarmonyPostfix]
  static void AlwaysHaveEitr(Player __instance, ref bool __result)
  {
    var noUsage = Settings.GodModeNoEitr && __instance.InGodMode();
    __result |= noUsage;
  }
  [HarmonyPatch(nameof(Player.GetMaxEitr)), HarmonyPostfix]
  static void AlwaysHaveMaxEitr(Player __instance, ref bool __result)
  {
    var noUsage = Settings.GodModeNoEitr && __instance.InGodMode();
    __result |= noUsage;
  }
  [HarmonyPatch(nameof(Player.UseEitr)), HarmonyPrefix]
  static bool NoEitrUsage(Player __instance)
  {
    var noUsage = Settings.GodModeNoEitr && __instance.InGodMode();
    return !noUsage;
  }
}
