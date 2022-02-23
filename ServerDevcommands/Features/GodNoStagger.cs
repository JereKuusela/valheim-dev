using HarmonyLib;

namespace ServerDevcommands {
  [HarmonyPatch(typeof(Character), "AddStaggerDamage")]
  public class AddStaggerDamage {
    public static bool Prefix(Character __instance) {
      var noStaggering = Settings.GodModeNoStagger && __instance.InGodMode() && __instance.IsPlayer();
      return !noStaggering;
    }
  }
}
