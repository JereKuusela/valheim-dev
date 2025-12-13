using HarmonyLib;
using System.Collections.Generic;

namespace ServerDevcommands;

[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateRecipeList))]
static class LimitRecipesToStationPatch
{
    [HarmonyPriority(Priority.First)]
    static void Prefix(ref List<Recipe> recipes)
    {
        if (!Settings.configNoCostLimitRecipesToStation.Value) return;
        Player localPlayer = Player.m_localPlayer;
        if (localPlayer && localPlayer.m_currentStation && (ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost) || localPlayer.NoCostCheat()))
        {
            recipes.RemoveAll(recipe => !localPlayer.RequiredCraftingStation(recipe, 1, Settings.configNoCostRespectStationLevel.Value));
        }
    }
}