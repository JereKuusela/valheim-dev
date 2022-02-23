using HarmonyLib;

namespace ServerDevcommands {
  [HarmonyPatch(typeof(Player), "HaveStamina")]
  public class HaveStaminaWithGodMode {
    public static void Postfix(Player __instance, ref bool __result) {
      var noUsage = Settings.GodModeNoStamina && __instance.InGodMode();
      if (noUsage) __result = true;
    }
  }
  [HarmonyPatch(typeof(Player), "UseStamina")]
  public class UseStaminaWithGodMode {
    public static bool Prefix(Player __instance) {
      var noUsage = Settings.GodModeNoStamina && __instance.InGodMode();
      return !noUsage;
    }
  }
}
