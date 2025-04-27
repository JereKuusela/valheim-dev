using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
///<summary>Adds support for mouse wheel binding and other features.</summary>
[HarmonyPatch]
public class BindCommand
{
  public BindCommand()
  {
    new Terminal.ConsoleCommand("bind", "[keycode,modifier1,modifier2,...] [command] [parameters] - Binds a key (with modifier keys) to a command.", (args) =>
    {
      if (args.Length < 2) return;
      BindManager.AddBind(args[1], string.Join(" ", args.Args.Skip(2)));
    }, optionsFetcher: () => ParameterInfo.KeyCodes);
    AutoComplete.Register("bind", (int index, int subIndex) =>
    {
      if (index == 0 && subIndex == 0) return ParameterInfo.KeyCodes;
      if (index == 0 && subIndex == 1) return ParameterInfo.KeyCodesWithNegative;
      return ParameterInfo.None;
    }, new() {
      { "keys", (int index) => ParameterInfo.KeyCodesWithNegative }
    });
    AutoComplete.Offsets["bind"] = 1;

    new Terminal.ConsoleCommand("unbind", "[keycode] - Clears binds from a key.", (args) =>
    {
      if (args.Length < 2) return;
      BindManager.RemoveBind(args[1]);
    });

    AutoComplete.Register("unbind", (int index) =>
    {
      if (index == 0) return ParameterInfo.KeyCodes;
      return ParameterInfo.None;
    });
    new Terminal.ConsoleCommand("printbinds", "Prints all key binds.", (args) =>
    {
      BindManager.PrintBinds(args.Context);
    });
    AutoComplete.RegisterEmpty("printbinds");
    new Terminal.ConsoleCommand("resetbinds", "Removes all custom key binds.", (args) =>
    {
      BindManager.ClearBinds();
    });
    AutoComplete.RegisterEmpty("resetbinds");
  }

  [HarmonyPatch(typeof(Chat), nameof(Chat.Update)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> DisableDefaultBindExecution(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
         .MatchStartForward(new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(Chat), nameof(Chat.m_wasFocused))))
        .Advance(4)
        .Insert(new CodeInstruction(OpCodes.Pop), new CodeInstruction(OpCodes.Ldc_I4_1))
        .InstructionEnumeration();
  }
  [HarmonyPatch(typeof(Chat), nameof(Chat.Update)), HarmonyPostfix]
  static void ExecuteBestBinds(Chat __instance)
  {
    if (__instance.m_input.isFocused) return;
    if (Console.instance && Console.instance.m_chatWindow.gameObject.activeInHierarchy) return;
    var binds = BindManager.GetBestKeyCommands();
    foreach (var bind in binds)
    {
      bind.Executed = true;
      if (bind.WasExecuted) continue;
      if (bind.Command == "") continue;
      __instance.TryRunCommand(bind.Command, true, true);
    }
    var offBinds = BindManager.GetOffBinds();
    foreach (var bind in offBinds)
    {
      bind.WasExecuted = false;
      if (bind.OffCommand == "") continue;
      __instance.TryRunCommand(bind.OffCommand, true, true);
    }
  }

  public static void SetMode(string mode)
  {
    BindManager.SetMode(mode);
  }
}
