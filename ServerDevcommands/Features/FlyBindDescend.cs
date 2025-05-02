using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Character), nameof(Character.UpdateDebugFly))]
public class FlyBindDescend
{

  static bool IsFlyUp()
  {
    if (!CheckKeys(Settings.FlyUpRequiredKeys, Settings.FlyUpBannedKeys)) return false;
    if (Settings.FlyUpRequiredKeys.Count > Settings.FlyDownRequiredKeys.Count) return true;
    return !CheckKeys(Settings.FlyDownRequiredKeys, Settings.FlyDownBannedKeys);
  }
  static bool IsFlyDown()
  {
    if (!CheckKeys(Settings.FlyDownRequiredKeys, Settings.FlyDownBannedKeys)) return false;
    if (Settings.FlyDownRequiredKeys.Count > Settings.FlyUpRequiredKeys.Count) return true;
    return !CheckKeys(Settings.FlyUpRequiredKeys, Settings.FlyUpBannedKeys);
  }

  private static bool CheckKeys(List<KeyCode> required, List<KeyCode> banned) => required.All(BindManager.GetKey) && !banned.Any(BindManager.GetKey);
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchStartForward(new CodeMatch(OpCodes.Ldstr, "Jump"))
      .SetOpcodeAndAdvance(OpCodes.Nop) // Remove the "Jump" value.
      .Set( // Replace the keycode check with a custom function.
          OpCodes.Call,
          Transpilers.EmitDelegate(IsFlyUp).operand)
      .MatchStartForward(
          new CodeMatch(
              OpCodes.Ldc_I4,
              (int)KeyCode.LeftControl))
      .SetOpcodeAndAdvance(OpCodes.Nop) // Remove the left control value.
      .SetOpcodeAndAdvance(OpCodes.Nop) // Remove the true value.
      .Set( // Replace the keycode check with a custom function.
          OpCodes.Call,
          Transpilers.EmitDelegate(IsFlyDown).operand)
      .InstructionEnumeration();
  }
}
