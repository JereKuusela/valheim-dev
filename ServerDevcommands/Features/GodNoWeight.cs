using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Player), nameof(Player.GetMaxCarryWeight))]
public class CarryWeightWithGodMode
{
  static void Postfix(Player __instance, ref float __result)
  {
    var noLimit = Settings.GodModeNoWeightLimit && __instance == Player.m_localPlayer && __instance.InGodMode();
    if (noLimit) __result = 1E10f;
  }
}
[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateInventoryWeight))]
public class UpdateInventoryWeight
{
  static bool Prefix(Player player, InventoryGui __instance)
  {
    var noLimit = Settings.GodModeNoWeightLimit && player == Player.m_localPlayer && player.InGodMode();
    if (noLimit)
    {
      var weight = Mathf.CeilToInt(player.GetInventory().GetTotalWeight());
      __instance.m_weight.text = $"{weight}/-";
    }
    return !noLimit;
  }
}
