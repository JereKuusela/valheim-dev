using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Player), nameof(Player.Update))]
public class DisableDebugModeKeys
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
         .MatchStartForward(new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(Player), nameof(Player.m_debugMode))))
         .SetAndAdvance( // Replace the debugmode check with a custome one.
              OpCodes.Call, Transpilers.EmitDelegate(() => Player.m_debugMode && !Settings.DisableDebugModeKeys).operand)
         .InstructionEnumeration();
  }
}
