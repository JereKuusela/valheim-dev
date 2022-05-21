using System;
using System.Linq;
using UnityEngine;
namespace ServerDevcommands;
///<summary>Adds support for mouse wheel binding and other features.</summary>
public class BindCommand {
  private void Print(Terminal terminal, string command) {
    // Mouse wheel hack.
    if (command.StartsWith("none")) command = "wheel" + command.Substring(4);
    terminal.AddString(command);
  }
  public BindCommand() {
    new Terminal.ConsoleCommand("bind", "[keycode,modifier1,modifier2,...] [command] [parameters] - Binds a key (with modifier keys) to a command.", (args) => {
      if (args.Length < 2) return;
      var keys = Parse.Split(args[1]).Select(key => key.ToLower()).ToArray();
      // Mouse wheel hack.
      if (keys[0] == "wheel") keys[0] = "none";
      if (!Enum.TryParse<KeyCode>(keys[0], true, out var keyCode)) {
        args.Context.AddString("'" + keys[0] + "' is not a valid UnityEngine.KeyCode.");
        return;
      }
      var keysStr = keys[0];
      if (keys.Length > 1) {
        keysStr += $" keys={string.Join(",", keys.Skip(1))}";
      }
      var item = $"{keysStr} {string.Join(" ", args.Args.Skip(2))}";
      Terminal.m_bindList.Add(item);
      Terminal.updateBinds();
    }, optionsFetcher: () => ParameterInfo.KeyCodes);
    AutoComplete.Register("bind", (int index, int subIndex) => {
      if (index == 0 && subIndex == 0) return ParameterInfo.KeyCodes;
      if (index == 0 && subIndex == 1) return ParameterInfo.KeyCodesWithNegative;
      return ParameterInfo.Create("The command to bind.");
    }, new() {
      { "keys", (int index) => ParameterInfo.KeyCodesWithNegative }
    });
    new Terminal.ConsoleCommand("unbind", "[keycode] [amount = 0] - Clears binds from a key. Optional parameter can be used to specify amount of removed binds.", (args) => {
      if (args.Length < 2) return;
      // Mouse wheel hack.
      if (args[1] == "wheel") args.Args[1] = "none";
      args.Args[1] = args[1].ToLower();
      var amount = Parse.TryInt(args.Args, 2, 0);
      if (amount == 0) amount = int.MaxValue;
      for (var i = Terminal.m_bindList.Count - 1; i >= 0 && amount > 0; i--) {
        if (Terminal.m_bindList[i].Split(' ')[0].ToLower() != args[1]) continue;
        Print(args.Context, Terminal.m_bindList[i]);
        Terminal.m_bindList.RemoveAt(i);
        amount--;
      }
      Terminal.updateBinds();
    });
    AutoComplete.Register("unbind", (int index) => {
      if (index == 0) return ParameterInfo.KeyCodes;
      if (index == 1) return ParameterInfo.Create("Amount of binds to remove from the key.");
      return ParameterInfo.None;
    });
    new Terminal.ConsoleCommand("printbinds", "Prints all key binds.", (args) => {
      foreach (var text in Terminal.m_bindList) Print(args.Context, text);
    });
    AutoComplete.RegisterEmpty("printbinds");
  }
}
