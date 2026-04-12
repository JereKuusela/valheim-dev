using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ServerDevcommands;

[HarmonyPatch]
public class FlyFeatures
{
  // Tracks when noclip was enabled by this mod for better compatibility with other mods.
  private static bool NoClipTurnedOn = false;

  private static bool NoClipPlayerEnabled() => Player.m_localPlayer && Player.m_localPlayer.IsDebugFlying() && Settings.FlyNoClip;
  private static bool NoClipCameraEnabled() => Settings.NoClipView || NoClipPlayerEnabled();

  private static bool CheckKeys(List<KeyCode> required, List<KeyCode> banned) => required.All(BindManager.GetKey) && !banned.Any(BindManager.GetKey);

  private static bool IsFlyUp()
  {
    if (!CheckKeys(Settings.FlyUpRequiredKeys, Settings.FlyUpBannedKeys)) return false;
    if (Settings.FlyUpRequiredKeys.Count > Settings.FlyDownRequiredKeys.Count) return true;
    return !CheckKeys(Settings.FlyDownRequiredKeys, Settings.FlyDownBannedKeys);
  }

  private static bool IsFlyDown()
  {
    if (!CheckKeys(Settings.FlyDownRequiredKeys, Settings.FlyDownBannedKeys)) return false;
    if (Settings.FlyDownRequiredKeys.Count > Settings.FlyUpRequiredKeys.Count) return true;
    return !CheckKeys(Settings.FlyUpRequiredKeys, Settings.FlyUpBannedKeys);
  }

  private static Vector2 CheckInvert(Vector2 vector)
  {
    if (!Settings.FreeFlyCameraInvert) return vector;
    if (ZInput.IsGamepadActive())
    {
      if (PlayerController.m_invertCameraX)
        vector.x *= -1f;
      if (PlayerController.m_invertCameraY)
        vector.y *= -1f;
    }
    else if (PlayerController.m_invertMouse)
      vector.y *= -1f;
    return vector;
  }

  [HarmonyPatch(typeof(Character), nameof(Character.UpdateDebugFly)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> UpdateDebugFlyTranspiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchStartForward(new CodeMatch(OpCodes.Ldstr, "Jump"))
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .Set(
          OpCodes.Call,
          Transpilers.EmitDelegate(IsFlyUp).operand)
      .MatchStartForward(
          new CodeMatch(
              OpCodes.Ldc_I4,
              (int)KeyCode.LeftControl))
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .Set(
          OpCodes.Call,
          Transpilers.EmitDelegate(IsFlyDown).operand)
      .InstructionEnumeration();
  }

  [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.UpdateFreeFly)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> UpdateFreeFlyTranspiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
     .MatchStartForward(new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameCamera), nameof(GameCamera.m_freeFlyYaw))))
     .Advance(-2)
     .InsertAndAdvance(
         new CodeInstruction(OpCodes.Ldloc_0),
         new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FlyFeatures), nameof(CheckInvert))),
         new CodeInstruction(OpCodes.Stloc_0))
     .InstructionEnumeration();
  }

  [HarmonyPatch(typeof(Player), nameof(Player.SetControls)), HarmonyPrefix]
  static void SetControlsPrefix(Player __instance, ref bool jump, ref bool crouch)
  {
    if (__instance != Player.m_localPlayer) return;
    jump = jump && !__instance.IsDebugFlying();
    crouch = crouch && !__instance.IsDebugFlying();
  }

  [HarmonyPatch(typeof(Player), nameof(Player.LateUpdate)), HarmonyPostfix]
  static void LateUpdatePostfix(Player __instance)
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
    var noClip = NoClipPlayerEnabled();
    if (noClip)
    {
      if (!NoClipTurnedOn)
      {
        if (Settings.NoClipClearEnvironment)
        {
          // Forced environments rely on collider checks so they won't deactivate with noclip.
          EnvMan.instance.SetForceEnvironment("");
        }
        Ship.GetLocalShip()?.OnTriggerExit(__instance.m_collider);
        // Water surfaces keep track of entered players and need manual trigger exits.
        var obj = __instance.GetComponent<IWaterInteractable>();
        var surfaces = UnityEngine.Object.FindObjectsByType<LiquidSurface>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var surface in surfaces)
        {
          if (surface.m_inWater == null || !surface.m_inWater.Contains(obj)) continue;
          surface.OnTriggerExit(__instance.m_collider);
        }
        var volumes = UnityEngine.Object.FindObjectsByType<WaterVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var volume in volumes)
        {
          if (volume.m_inWater == null || !volume.m_inWater.Contains(obj)) continue;
          volume.OnTriggerExit(__instance.m_collider);
        }
      }
      NoClipTurnedOn = true;
      __instance.m_collider.enabled = false;
    }
    else if (NoClipTurnedOn)
    {
      NoClipTurnedOn = false;
      __instance.m_collider.enabled = true;
    }
  }


  // Prevents the rotation quaternion spam error while flying.
  [HarmonyPatch(typeof(Character), nameof(Character.UpdateEyeRotation)), HarmonyPrefix]
  static void UpdateEyeRotationPrefix(Character __instance)
  {
    __instance.m_lookDir = __instance.m_lookDir.normalized;
  }

  [HarmonyPatch(typeof(Player), nameof(Player.IsDebugFlying)), HarmonyPostfix]
  static bool IsDebugFlyingPostfix(bool result, Player __instance) => __instance == Player.m_localPlayer ? Settings.IsEnabled(PermissionHash.Fly, result) : result;


  [HarmonyPatch(typeof(Player), nameof(Player.InDebugFlyMode)), HarmonyPostfix]
  static bool InDebugFlyModePostfix(bool result, Player __instance) => __instance == Player.m_localPlayer ? Settings.IsEnabled(PermissionHash.Fly, result) : result;



  [HarmonyPatch(typeof(Character), nameof(Character.UnderWorldCheck)), HarmonyPrefix]
  static bool UnderWorldCheckPrefix(Character __instance) => __instance != Player.m_localPlayer || !NoClipPlayerEnabled();

  [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.GetCameraPosition)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> GetCameraPositionTranspiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
         .MatchStartForward(
             new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameCamera), nameof(GameCamera.m_minWaterDistance))))
         .Repeat(matcher => matcher
           .SetAndAdvance(
             OpCodes.Call,
             Transpilers.EmitDelegate(
                 (GameCamera instance) => NoClipCameraEnabled() ? float.MinValue : instance.m_minWaterDistance).operand)
         )
         .InstructionEnumeration();
  }

  [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.CollideRay2)), HarmonyPrefix]
  static bool CollideRay2Prefix() => !NoClipCameraEnabled();
}