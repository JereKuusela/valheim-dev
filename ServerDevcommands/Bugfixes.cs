using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

namespace ServerDevcommands;

// Fixes console from resetting caret position at start when deleting a selection range
// happens first time opening console, or browsing commands history
[HarmonyPatch(typeof(TMP_InputField), "KeyPressed")]
public class TerminalFix
{
  static int caretPos = 0;
  static int textLen = 0;
  static int anchorPos = 0;

  static void Prefix(TMP_InputField __instance)
  {
    TMP_InputField input = __instance;
    textLen = input.text.Length;
    caretPos = input.caretPosition;
    anchorPos = input.selectionAnchorPosition;
  }

  static void Postfix(TMP_InputField __instance, Event evt)
  {
    TMP_InputField input = __instance;
    if (caretPos == anchorPos) return; // only need workaround for selections

    // first case: caret back to start
    if (input.caretPosition == 0 && (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace))
    {
      if (textLen != input.text.Length)
        input.caretPosition = Math.Min(Math.Min(caretPos, anchorPos), textLen);
    }
    
    // second case: when caret is moved at 1 while typing with an active selection
    if (input.caretPosition == 1 && (evt.character != 0))
    {
      input.caretPosition = Math.Min(Math.Min(caretPos, anchorPos), textLen) + 1; // position after typed character
    }
  }
}

// Fixes the rotation quaternion spam error while flying
[HarmonyPatch(typeof(Character), nameof(Character.UpdateEyeRotation))]
public class UpdateEyeRotationFix
{
  static void Prefix(Character __instance)
  {
    __instance.m_lookDir = __instance.m_lookDir.normalized;
  }
}
