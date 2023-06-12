using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Character), nameof(Character.ApplyDamage))]
public class GodAlwaysParry {
  static void Postfix(Character __instance, HitData hit) {
    if (!Settings.GodModeAlwaysParry || __instance != Player.m_localPlayer) return;
    var attacker = hit.GetAttacker();
    if (!attacker || attacker == Player.m_localPlayer) return;
    if (Player.m_localPlayer.m_blockTimer < 0f) {
      Player.m_localPlayer.m_perfectBlockEffect.Create(hit.m_point, Quaternion.identity, null, 1f, -1);
      attacker.Stagger(-hit.m_dir);
    }
  }
}
