using System;
using HarmonyLib;
using UnityEngine;

namespace ServerDevcommands;

[HarmonyPatch]
public class GodFeatures
{
  [HarmonyPatch(typeof(Player), nameof(Player.InGodMode)), HarmonyPostfix]
  static bool InGodModePostfix(bool result, Player __instance) => __instance == Player.m_localPlayer ? Settings.IsEnabled(PermissionHash.God, result) : result;

  private static bool IsAttacking = false;

  [HarmonyPatch(typeof(Player), nameof(Player.IsDodgeInvincible)), HarmonyPostfix]
  static void IsDodgeInvinciblePostfix(Player __instance, ref bool __result)
  {
    var noUsage = Settings.GodModeAlwaysDodge && __instance.InGodMode();
    __result |= noUsage;
  }

  [HarmonyPatch(typeof(Character), nameof(Character.ApplyDamage)), HarmonyPostfix]
  static void ApplyDamagePostfix(Character __instance, HitData hit)
  {
    if (!Settings.GodModeAlwaysParry || __instance != Player.m_localPlayer || !__instance.InGodMode()) return;
    var attacker = hit.GetAttacker();
    if (!attacker || attacker == Player.m_localPlayer) return;
    if (Player.m_localPlayer.m_blockTimer < 0f)
    {
      Player.m_localPlayer.m_perfectBlockEffect.Create(hit.m_point, Quaternion.identity, null, 1f, -1);
      attacker.Stagger(-hit.m_dir);
    }
  }

  [HarmonyPatch(typeof(Player), nameof(Player.EdgeOfWorldKill)), HarmonyPrefix]
  static bool EdgeOfWorldKillPrefix(Player __instance) => !Settings.GodModeNoEdgeOfWorld || !__instance.InGodMode();

  [HarmonyPatch(typeof(Player), nameof(Player.HaveEitr)), HarmonyPostfix]
  static void HaveEitrPostfix(Player __instance, ref bool __result)
  {
    var noUsage = Settings.GodModeNoEitr && __instance.InGodMode();
    __result |= noUsage;
  }

  [HarmonyPatch(typeof(Attack), nameof(Attack.Start)), HarmonyPrefix]
  static void AttackStartPrefix(Humanoid character)
  {
    if (character == Player.m_localPlayer)
      IsAttacking = true;
  }

  [HarmonyPatch(typeof(Attack), nameof(Attack.Start)), HarmonyPostfix]
  static void AttackStartPostfix(Humanoid character)
  {
    if (character == Player.m_localPlayer)
      IsAttacking = false;
  }

  [HarmonyPatch(typeof(Player), nameof(Player.GetMaxEitr)), HarmonyPostfix]
  static void GetMaxEitrPostfix(Player __instance, ref float __result)
  {
    var noUsage = Settings.GodModeNoEitr && __instance.InGodMode();
    if (noUsage && IsAttacking) __result = Math.Max(__result, 0.1f);
  }

  [HarmonyPatch(typeof(Player), nameof(Player.UseEitr)), HarmonyPrefix]
  static bool UseEitrPrefix(Player __instance)
  {
    var noUsage = Settings.GodModeNoEitr && __instance.InGodMode();
    return !noUsage;
  }

  [HarmonyPatch(typeof(Character), nameof(Character.ApplyPushback), typeof(Vector3), typeof(float)), HarmonyPrefix]
  static bool ApplyPushbackPrefix(Character __instance)
  {
    var noKnockback = Settings.GodModeNoKnockback && __instance.InGodMode() && __instance.IsPlayer();
    return !noKnockback;
  }

  [HarmonyPatch(typeof(ParticleMist), nameof(ParticleMist.Update)), HarmonyPrefix]
  static bool ParticleMistUpdatePrefix()
  {
    var player = Player.m_localPlayer;
    var noMist = Settings.GodModeNoMist && player && player.InGodMode();
    return !noMist;
  }

  [HarmonyPatch(typeof(Character), nameof(Character.AddStaggerDamage)), HarmonyPrefix]
  static bool AddStaggerDamagePrefix(Character __instance)
  {
    var noStaggering = Settings.GodModeNoStagger && __instance.InGodMode() && __instance.IsPlayer();
    return !noStaggering;
  }

  [HarmonyPatch(typeof(Player), nameof(Player.HaveStamina)), HarmonyPostfix]
  static void HaveStaminaPostfix(Player __instance, ref bool __result)
  {
    var noUsage = Settings.GodModeNoStamina && __instance.InGodMode();
    __result |= noUsage;
  }

  [HarmonyPatch(typeof(Player), nameof(Player.UseStamina)), HarmonyPrefix]
  static bool UseStaminaPrefix(Player __instance)
  {
    var noUsage = Settings.GodModeNoStamina && __instance.InGodMode();
    return !noUsage;
  }

  [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveOneItem)), HarmonyPrefix]
  static bool RemoveOneItemPrefix(Inventory __instance, ref bool __result)
  {
    if (Player.m_localPlayer?.m_inventory != __instance) return true;
    var noUsage = Settings.GodModeNoUsage && Player.m_localPlayer.InGodMode();
    if (noUsage) __result = true;
    return !noUsage;
  }

  [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), typeof(ItemDrop.ItemData), typeof(int)), HarmonyPrefix]
  static bool RemoveItemByDataPrefix(Inventory __instance, int amount, ref bool __result)
  {
    if (amount > 1) return true;
    if (Player.m_localPlayer?.m_inventory != __instance) return true;
    var noUsage = Settings.GodModeNoUsage && Player.m_localPlayer.InGodMode();
    if (noUsage) __result = true;
    return !noUsage;
  }

  [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), typeof(string), typeof(int), typeof(int), typeof(bool)), HarmonyPrefix]
  static bool RemoveItemByNamePrefix(Inventory __instance, int amount)
  {
    if (amount > 1) return true;
    if (Player.m_localPlayer?.m_inventory != __instance) return true;
    var noUsage = Settings.GodModeNoUsage && Player.m_localPlayer.InGodMode();
    return !noUsage;
  }

  [HarmonyPatch(typeof(Player), nameof(Player.GetMaxCarryWeight)), HarmonyPostfix]
  static void GetMaxCarryWeightPostfix(Player __instance, ref float __result)
  {
    var noLimit = Settings.GodModeNoWeightLimit && __instance == Player.m_localPlayer && __instance.InGodMode();
    if (noLimit) __result = 1E10f;
  }

  [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateInventoryWeight)), HarmonyPrefix]
  static bool UpdateInventoryWeightPrefix(Player player, InventoryGui __instance)
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