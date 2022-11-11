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
  private void Print(Terminal terminal, string command)
  {
    // Mouse wheel hack.
    var key = Settings.MouseWheelBindKey.ToString();
    if (command.StartsWith(key, StringComparison.OrdinalIgnoreCase))
      command = "wheel" + command.Substring(key.Length);
    terminal.AddString(command);
  }
  public BindCommand()
  {
    new Terminal.ConsoleCommand("bind", "[keycode,modifier1,modifier2,...] [command] [parameters] - Binds a key (with modifier keys) to a command.", (args) =>
    {
      if (args.Length < 2) return;
      var keys = Parse.Split(args[1]).Select(key => key.ToLower()).ToArray();
      // Mouse wheel hack.
      if (keys[0] == "wheel") keys[0] = Settings.MouseWheelBindKey.ToString().ToLower();
      if (!Enum.TryParse<KeyCode>(keys[0], true, out var keyCode))
      {
        args.Context.AddString("'" + keys[0] + "' is not a valid UnityEngine.KeyCode.");
        return;
      }
      var keysStr = keys[0];
      if (keys.Length > 1)
      {
        keysStr += $" keys={string.Join(",", keys.Skip(1))}";
      }
      var item = $"{keysStr} {string.Join(" ", args.Args.Skip(2))}";
      Terminal.m_bindList.Add(item);
      Terminal.updateBinds();
      BindManager.ToBeSaved = true;
    }, optionsFetcher: () => ParameterInfo.KeyCodes);
    AutoComplete.Register("bind", (int index, int subIndex) =>
    {
      if (index == 0 && subIndex == 0) return ParameterInfo.KeyCodes;
      if (index == 0 && subIndex == 1) return ParameterInfo.KeyCodesWithNegative;
      return ParameterInfo.Create("The command to bind.");
    }, new() {
      { "keys", (int index) => ParameterInfo.KeyCodesWithNegative }
    });
    new Terminal.ConsoleCommand("unbind", "[keycode/tag] [amount = 0] [silent] - Clears binds from a key. Optional parameter can be used to specify amount of removed binds.", (args) =>
    {
      if (args.Length < 2) return;
      // Mouse wheel hack.
      if (args[1] == "wheel") args.Args[1] = Settings.MouseWheelBindKey.ToString().ToLower();
      var key = args.Args[1].ToLower();
      if (Enum.TryParse<KeyCode>(args.Args[1], true, out var _))
      {
        var silent = args.Length > 3;
        var amount = Parse.Int(args.Args, 2, 0);
        if (amount == 0) amount = int.MaxValue;
        for (var i = Terminal.m_bindList.Count - 1; i >= 0 && amount > 0; i--)
        {
          if (Terminal.m_bindList[i].Split(' ')[0].ToLower() != key) continue;
          if (!silent) Print(args.Context, Terminal.m_bindList[i]);
          Terminal.m_bindList.RemoveAt(i);
          amount--;
        }
      }
      else
      {
        var silent = args.Length > 2;
        key = $"tag={key}";
        for (var i = Terminal.m_bindList.Count - 1; i >= 0; i--)
        {
          if (!Terminal.m_bindList[i].Contains(key)) continue;
          if (!silent) Print(args.Context, Terminal.m_bindList[i]);
          Terminal.m_bindList.RemoveAt(i);
        }
      }
      Terminal.updateBinds();
      BindManager.ToBeSaved = true;
    });
    AutoComplete.Register("unbind", (int index) =>
    {
      if (index == 0) return ParameterInfo.KeyCodes;
      if (index == 1) return ParameterInfo.Create("Amount of binds to remove from the key.");
      return ParameterInfo.None;
    });
    new Terminal.ConsoleCommand("printbinds", "Prints all key binds.", (args) =>
    {
      foreach (var text in Terminal.m_bindList) Print(args.Context, text);
    });
    AutoComplete.RegisterEmpty("printbinds");
  }

  [HarmonyPatch(typeof(Chat), nameof(Chat.Update)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> DisableDefaultBindExecution(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
         .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(Chat), nameof(Chat.m_wasFocused))))
        .Advance(4)
        .Insert(new CodeInstruction(OpCodes.Pop), new CodeInstruction(OpCodes.Ldc_I4_1))
        .InstructionEnumeration();
  }
  [HarmonyPatch(typeof(Chat), nameof(Chat.Update)), HarmonyPostfix]
  static void ExecuteBestBinds(Chat __instance)
  {
    if (__instance.m_input.isFocused) return;
    if (Console.instance && Console.instance.m_chatWindow.gameObject.activeInHierarchy) return;
    var commands = Terminal.m_binds.Where(kvp => Input.GetKeyDown(kvp.Key)).SelectMany(kvp => kvp.Value).Where(Valid).ToArray();
    if (commands.Length == 0) return;
    if (Settings.BestCommandMatch)
    {
      var max = commands.Max(CountKeys);
      commands = commands.Where(cmd => CountKeys(cmd) == max).ToArray();
    }
    commands = commands.Select(CleanUp).ToArray();
    foreach (var cmd in commands) __instance.TryRunCommand(cmd, true, true);
  }
  public static int CountKeys(string command)
  {
    if (!command.Contains("keys=")) return 0;
    var args = command.Split(' ');
    var arg = args.First(arg => arg.StartsWith("keys=")).Split('=');
    if (arg.Length < 2) return 0;
    var keys = Parse.Split(arg[1]);
    return CountKeys(keys);
  }
  public static int CountKeys(string[] keys) => keys.Count(key => !key.StartsWith("-", StringComparison.Ordinal) && Enum.TryParse<KeyCode>(key, true, out var _));

  public static bool Valid(string command)
  {
    if (!command.Contains("keys=")) return true;
    var args = command.Split(' ');
    var arg = args.First(arg => arg.StartsWith("keys=")).Split('=');
    if (arg.Length < 2) return true;
    var keys = Parse.Split(arg[1]);
    return Valid(keys);
  }

  public static bool Valid(string[] keys)
  {
    var tool = Player.m_localPlayer?.GetRightItem()?.m_dropPrefab;
    var toolName = tool ? Utils.GetPrefabName(tool).ToLower() : "";
    var toolRequired = false;
    var hasTool = false;
    var inBuildMode = Player.m_localPlayer?.InPlaceMode() ?? false;
    foreach (var key in keys)
    {
      if (key.StartsWith("-"))
      {
        var sub = key.Substring(1);
        if (Enum.TryParse<KeyCode>(sub, true, out var keyCode))
        {
          if (Input.GetKey(keyCode)) return false;
        }
        else if (sub == "build")
        {
          if (inBuildMode) return false;
        }
        else if (key.Substring(1) == toolName) return false;

      }
      else
      {
        if (Enum.TryParse<KeyCode>(key, true, out var keyCode))
        {
          if (!Input.GetKey(keyCode)) return false;
        }
        else if (key == "build")
        {
          if (!inBuildMode) return false;
        }
        else
        {
          toolRequired = true;
          if (key == toolName) hasTool = true;
        }
      }
    }
    if (toolRequired && !hasTool) return false;
    return true;
  }
  public static string CleanUp(string command)
  {
    command = string.Join(" ", command.Split(' ').Where(arg => !arg.StartsWith("tag=", StringComparison.OrdinalIgnoreCase)));
    // The command itself may contain multiple commands with key checks.
    // This is not really intended usage but this should give some basic support for it.
    if (command.Split(' ').Count(arg => arg.StartsWith("keys=", StringComparison.OrdinalIgnoreCase)) < 2)
      command = string.Join(" ", command.Split(' ').Where(arg => !arg.StartsWith("keys=", StringComparison.OrdinalIgnoreCase)));
    return command;
  }
}
