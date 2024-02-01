using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
namespace ServerDevcommands;

public class NoClip
{
  public static bool PlayerEnabled() => Player.m_localPlayer && Player.m_localPlayer.IsDebugFlying() && Settings.FlyNoClip;
  public static bool CameraEnabled() => Settings.NoClipView || PlayerEnabled();
  // Tracks when the noclip was turned on by this mod (better compatibility with other noclip mods).
  public static bool TurnedOn = false;
}

[HarmonyPatch(typeof(Player), nameof(Player.SetControls))]
public class SetControls
{

  static void Prefix(Player __instance, ref bool jump, ref bool crouch)
  {
    if (__instance != Player.m_localPlayer) return;
    jump = jump && !__instance.IsDebugFlying();
    crouch = crouch && !__instance.IsDebugFlying();

  }
}
[HarmonyPatch(typeof(Player), nameof(Player.LateUpdate))]
public class NoObjectCollision
{
  static void Postfix(Player __instance)
  {
    if (__instance != Player.m_localPlayer) return;
    if (__instance.IsDebugFlying())
    {
      __instance.m_crouchToggled = false;
      __instance.m_zanim.SetBool(Character.s_onGround, true);
      __instance.m_zanim.SetBool(Character.s_inWater, false);
      __instance.m_zanim.SetBool(Character.s_encumbered, false);
      __instance.m_zanim.SetFloat(Character.s_sidewaySpeed, 0f);
      __instance.m_zanim.SetFloat(Character.s_forwardSpeed, 0f);
      __instance.m_zanim.SetFloat(Character.s_turnSpeed, 0f);
    }
    var noClip = NoClip.PlayerEnabled();
    if (noClip)
    {
      if (!NoClip.TurnedOn)
      {
        if (Settings.NoClipClearEnvironment)
        {
          // Forced environemnts rely on collider check so they won't get deactivated with noclip.
          // Easiest way around this is to just get rid of them.
          EnvMan.instance.SetForceEnvironment("");
        }
        Ship.GetLocalShip()?.OnTriggerExit(__instance.m_collider);
        // Water surfaces keep track of the entered player, must be manually removed.
        var obj = __instance.GetComponent<IWaterInteractable>();
        var surfaces = UnityEngine.Object.FindObjectsOfType<LiquidSurface>(true);
        foreach (var surface in surfaces)
        {
          if (surface.m_inWater == null || !surface.m_inWater.Contains(obj)) continue;
          surface.OnTriggerExit(__instance.m_collider);
        }
        var volumes = UnityEngine.Object.FindObjectsOfType<WaterVolume>(true);
        foreach (var volume in volumes)
        {
          if (volume.m_inWater == null || !volume.m_inWater.Contains(obj)) continue;
          volume.OnTriggerExit(__instance.m_collider);
        }
      }
      NoClip.TurnedOn = true;
      __instance.m_collider.enabled = false;
    }
    else if (NoClip.TurnedOn)
    {
      NoClip.TurnedOn = false;
      __instance.m_collider.enabled = true;
    }
  }
}
[HarmonyPatch(typeof(Character), nameof(Character.UnderWorldCheck))]
public class NoGroundCollision
{
  static bool Prefix(Character __instance) => __instance != Player.m_localPlayer || !NoClip.PlayerEnabled();
}

[HarmonyPatch(typeof(GameCamera), nameof(GameCamera.GetCameraPosition))]
public class NoCameraCapping
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
         .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameCamera), nameof(GameCamera.m_minWaterDistance))))
         .Repeat(matcher => matcher
           .SetAndAdvance( // Replace the m_offset3 value with a custom function.
             OpCodes.Call,
             Transpilers.EmitDelegate<Func<GameCamera, float>>(
                 (GameCamera instance) => NoClip.CameraEnabled() ? float.MinValue : instance.m_minWaterDistance).operand)
         )
         .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(GameCamera), nameof(GameCamera.CollideRay2))]
public class NoCameraClipping
{
  static bool Prefix() => !NoClip.CameraEnabled();
}
