using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerDevcommands {
  ///<summary>Feature for disabling commands.</summary>
  public class BlackList {

    public static List<string> AllowedCommands = new List<string>();
    public static List<string> DisallowedCommands = new List<string>();

    public static bool CanRun(string command) {
      if (DisallowedCommands.Contains(command.ToLower())) return false;
      if (DisallowedCommands.Any(black => (black + " ").StartsWith(command, StringComparison.OrdinalIgnoreCase))) return false;
      return true;
    }

    public static void UpdateCommands(string blackList) {
      DisallowedCommands = blackList.Split(',').Select(s => s.Trim().ToLower()).ToList();
      AllowedCommands = Terminal.commands.Keys.Where(CanRun).ToList();
    }
  }
}
