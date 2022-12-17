using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Inventory))]
public class GodNoUsage
{
  [HarmonyPatch(nameof(Inventory.RemoveOneItem)), HarmonyPrefix]
  static bool NoUsage1(Inventory __instance)
  {
    if (Player.m_localPlayer?.m_inventory != __instance) return true;
    var noUsage = Settings.GodModeNoUsage && Player.m_localPlayer.InGodMode();
    return !noUsage;
  }
  [HarmonyPatch(nameof(Inventory.RemoveItem), typeof(ItemDrop.ItemData), typeof(int)), HarmonyPrefix]
  static bool NoUsage2(Inventory __instance, int amount)
  {
    if (amount > 1) return true;
    if (Player.m_localPlayer?.m_inventory != __instance) return true;
    var noUsage = Settings.GodModeNoUsage && Player.m_localPlayer.InGodMode();
    return !noUsage;
  }
  [HarmonyPatch(nameof(Inventory.RemoveItem), typeof(string), typeof(int), typeof(int)), HarmonyPrefix]
  static bool NoUsage3(Inventory __instance, int amount)
  {
    if (amount > 1) return true;
    if (Player.m_localPlayer?.m_inventory != __instance) return true;
    var noUsage = Settings.GodModeNoUsage && Player.m_localPlayer.InGodMode();
    return !noUsage;
  }
}
