using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
namespace ServerDevcommands;

public class NoClip {
  public static bool Enabled() => Player.m_localPlayer && Player.m_localPlayer.IsDebugFlying() && Settings.FlyNoClip;
}

[HarmonyPatch(typeof(Player), nameof(Player.LateUpdate))]
public class NoObjectCollision {
  static void Postfix(Player __instance) {
    if (__instance != Player.m_localPlayer) return;
    __instance.m_collider.enabled = !NoClip.Enabled();

  }
}
[HarmonyPatch(typeof(Character), nameof(Character.UnderWorldCheck))]
public class NoGroundCollision {
  static bool Prefix(Character __instance) => !NoClip.Enabled();
}

[HarmonyPatch(typeof(GameCamera), nameof(GameCamera.GetCameraPosition))]
public class NoCameraCapping {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
         .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameCamera), nameof(GameCamera.m_minWaterDistance))))
         .Repeat(matcher => matcher
           .SetAndAdvance( // Replace the m_offset3 value with a custom function.
             OpCodes.Call,
             Transpilers.EmitDelegate<Func<GameCamera, float>>(
                 (GameCamera instance) => NoClip.Enabled() ? float.MinValue : instance.m_minWaterDistance).operand)
         )
         .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(GameCamera), nameof(GameCamera.CollideRay2))]
public class NoCameraClipping {
  static bool Prefix(GameCamera __instance) => !NoClip.Enabled();
}
