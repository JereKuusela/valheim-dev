using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Inventory))]
public class GodNoUsage
{
  [HarmonyPatch(nameof(Inventory.RemoveOneItem)), HarmonyPrefix]
  static bool NoUsage(Player __instance)
  {
    var noUsage = Settings.GodModeNoUsage && __instance.InGodMode();
    return !noUsage;
  }
}
