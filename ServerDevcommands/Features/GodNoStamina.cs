using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Player), nameof(Player.HaveStamina))]
public class HaveStaminaWithGodMode
{
  static void Postfix(Player __instance, ref bool __result)
  {
    var noUsage = Settings.GodModeNoStamina && __instance.InGodMode();
    __result |= noUsage;
  }
}
[HarmonyPatch(typeof(Player), nameof(Player.UseStamina))]
public class UseStaminaWithGodMode
{
  static bool Prefix(Player __instance)
  {
    var noUsage = Settings.GodModeNoStamina && __instance.InGodMode();
    return !noUsage;
  }
}
