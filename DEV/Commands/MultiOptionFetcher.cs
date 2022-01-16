using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace DEV {
  public static class CommandParameters {
    private static Dictionary<string, Func<int, List<string>>> Fetchers = new Dictionary<string, Func<int, List<string>>>();
    public static void AddFetcher(string command, Func<int, List<string>> fetcher) => Fetchers[command] = fetcher;
    public static List<string> Fetch(string command, int index) {
      if (index == 0 || !Fetchers.ContainsKey(command))
        return Terminal.commands[command].m_tabOptionsFetcher();
      return Fetchers[command](index);
    }
    public static List<string> CreateRange(int min, int max) => Enumerable.Range(min, max - min + 1).Select(number => number.ToString()).ToList();
    public static void RegisterBaseGameFetchers() {
      AddFetcher("raiseskill", (int index) => {
        if (index == 1)
          return CreateRange(-100, 100);
        return new List<string>();
      });
    }
  }

  /*
    [HarmonyPatch(typeof(Terminal.ConsoleCommand), "GetTabOptions")]
    public class UseParameterSpecificOptions {
      private string CachedCommand = "";
      private Dictionary<int, List<string>> Options = null;
      private string GetInput() => Console.instance.m_input.text == "" ? Chat.instance.m_input.text : Console.instance.m_input.text;
      private int GetInputLength() => GetInput().Split(' ').Length;
      public bool Prefix(Terminal.ConsoleCommand __instance, ref List<string> __result) {
        if (__instance.Command != CachedCommand) Options = new Dictionary<int, List<string>>();
        CachedCommand = __instance.Command;
        var index = GetInputLength();
        if (index == 0) return true;
        index -= 1; // Ignore the command name.
        if (!Options.ContainsKey(index))
          Options[index] = CommandParameters.Fetch(CachedCommand, index);
        __result = Options[index];
        return false;
      }
    }

    [HarmonyPatch(typeof(Terminal), "tabCycle")]
    public class UseParameterSpecificAutocomplete {
      public void Prefix(Terminal __instance, ref string word) {
        word = __instance.m_input.text.Split(' ').Last();
      }

    }
  */
}
