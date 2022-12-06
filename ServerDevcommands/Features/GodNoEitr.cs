using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Player), nameof(Player.HaveEitr))]
public class HaveEitrWithGodMode
{
  static void Postfix(Player __instance, ref bool __result)
  {
    var noUsage = Settings.GodModeNoEitr && __instance.InGodMode();
    __result |= noUsage;
  }
}
[HarmonyPatch(typeof(Player), nameof(Player.UseEitr))]
public class UseEitrWithGodMode
{
  static bool Prefix(Player __instance)
  {
    var noUsage = Settings.GodModeNoEitr && __instance.InGodMode();
    return !noUsage;
  }
}
