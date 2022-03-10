using HarmonyLib;

namespace ServerDevcommands {
  [HarmonyPatch(typeof(Character), nameof(Character.ApplyPushback))]
  public class ApplyPushback {
    public static bool Prefix(Character __instance) {
      var noKnockback = Settings.GodModeNoKnockback && __instance.InGodMode() && __instance.IsPlayer();
      return !noKnockback;
    }
  }
}
