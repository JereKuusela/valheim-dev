using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
[HarmonyPatch(typeof(GameCamera), nameof(GameCamera.UpdateFreeFly))]
public class FreeFlyCameraInvert
{
  static Vector2 CheckInvert(Vector2 vector)
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
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
     .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameCamera), nameof(GameCamera.m_freeFlyYaw))))
     .Advance(-2)
     .InsertAndAdvance(
         new CodeInstruction(OpCodes.Ldloc_0),
         new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FreeFlyCameraInvert), nameof(CheckInvert))),
         new CodeInstruction(OpCodes.Stloc_0))
     .InstructionEnumeration();
  }
}
