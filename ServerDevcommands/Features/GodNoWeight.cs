using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Player), nameof(Player.GetMaxCarryWeight))]
public class CarryWeightWithGodMode
{
  static void Postfix(Player __instance, ref float __result)
  {
    var noLimit = Settings.GodModeNoWeightLimit && __instance.InGodMode();
    if (noLimit) __result = 1E10f;
  }
}
[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateInventoryWeight))]
public class UpdateInventoryWeight
{
  static bool Prefix(Player player, InventoryGui __instance)
  {
    var noLimit = Settings.GodModeNoWeightLimit && player.InGodMode();
    if (noLimit)
    {
      var weight = Mathf.CeilToInt(player.GetInventory().GetTotalWeight());
      __instance.m_weight.text = $"{weight}/-";
    }
    return !noLimit;
  }
}
