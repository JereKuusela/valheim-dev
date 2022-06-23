using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Character), nameof(Character.UpdateDebugFly))]
public class FlyBindDescend {

  static bool IsFlyUp() {
    if (MouseWheelBinding.CouldExecute()) return false;
    if (!BindCommand.Valid(Settings.FlyUpKeys)) return false;
    if (!BindCommand.Valid(Settings.FlyDownKeys)) return true;
    return BindCommand.CoundKeys(Settings.FlyUpKeys) >= BindCommand.CoundKeys(Settings.FlyDownKeys);
  }
  static bool IsFlyDown() {
    if (MouseWheelBinding.CouldExecute()) return false;
    if (!BindCommand.Valid(Settings.FlyDownKeys)) return false;
    if (!BindCommand.Valid(Settings.FlyUpKeys)) return true;
    return BindCommand.CoundKeys(Settings.FlyDownKeys) >= BindCommand.CoundKeys(Settings.FlyUpKeys);
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Ldstr,
                  "Jump"))
          .SetOpcodeAndAdvance(OpCodes.Nop) // Remove the "Jump" value.
          .Set( // Replace the keycode check with a custom function.
              OpCodes.Call,
              Transpilers.EmitDelegate<Func<bool>>(IsFlyUp).operand)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Ldc_I4,
                  (int)KeyCode.LeftControl))
          .SetOpcodeAndAdvance(OpCodes.Nop) // Remove the left control value.
          .Set( // Replace the keycode check with a custom function.
              OpCodes.Call,
              Transpilers.EmitDelegate<Func<bool>>(IsFlyDown).operand)
          .InstructionEnumeration();
  }
}
