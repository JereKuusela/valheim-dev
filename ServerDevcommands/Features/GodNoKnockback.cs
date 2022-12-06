using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Character), nameof(Character.ApplyPushback), typeof(Vector3), typeof(float))]
public class ApplyPushback
{
  static bool Prefix(Character __instance)
  {
    var noKnockback = Settings.GodModeNoKnockback && __instance.InGodMode() && __instance.IsPlayer();
    return !noKnockback;
  }
}