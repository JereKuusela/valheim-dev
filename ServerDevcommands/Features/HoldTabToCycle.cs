using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;

[HarmonyPatch(typeof(Terminal), nameof(Terminal.UpdateInput))]
public class HoldTabToCycle
{
  const float Delay = 0.4f;
  const float Interval = 0.05f;
  static int Frame = -1;
  static bool Pressed = false;
  static float Next = 0f;

  public static bool IsTabPressed()
  {
    if (Frame == Time.frameCount) return Pressed;
    Frame = Time.frameCount;
    if (ZInput.GetKeyDown(KeyCode.Tab))
    {
      Next = Time.unscaledTime + Delay;
      return Pressed = true;
    }
    Pressed = Console.IsVisible() && ZInput.GetKey(KeyCode.Tab) && Time.unscaledTime >= Next;
    if (Pressed) Next = Time.unscaledTime + Interval;
    return Pressed;
  }

  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
         .MatchStartForward(
              new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)KeyCode.Tab),
              new CodeMatch(OpCodes.Ldc_I4_1),
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZInput), nameof(ZInput.GetKeyDown), [typeof(KeyCode), typeof(bool)])))
         .ThrowIfInvalid("Tab check not found in Terminal.UpdateInput")
         .SetAndAdvance(OpCodes.Nop, null)
         .SetAndAdvance(OpCodes.Nop, null)
         .SetOperandAndAdvance(AccessTools.Method(typeof(HoldTabToCycle), nameof(IsTabPressed)))
         .InstructionEnumeration();
  }
}
