using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace DEV {
  using Fetcher = Func<int, string, List<string>>;
  public static class CommandParameters {
    private static Dictionary<string, Fetcher> Fetchers = new Dictionary<string, Fetcher>();
    public static void AddFetcher(string command, Fetcher fetcher) => Fetchers[command] = fetcher;
    public static List<string> Fetch(string command, int index, string parameter) {
      if (Fetchers.ContainsKey(command)) return Fetchers[command](index, parameter);
      if (!Terminal.commands.ContainsKey(command)) return new List<string>();
      var fetcher = Terminal.commands[command].m_tabOptionsFetcher;
      if (fetcher != null)
        return fetcher();
      return new List<string>();
    }
    public static List<string> CreateRange(int min, int max) => Enumerable.Range(min, max - min + 1).Select(number => number.ToString()).ToList();
    public static void RegisterBaseGameFetchers() {
      AddFetcher("raiseskill", (int index, string parameter) => {
        if (index == 0)
          return Terminal.commands["raiseskill"].m_tabOptionsFetcher();
        if (index == 1)
          return CreateRange(-100, 100);
        return new List<string>();
      });
    }
  }

  public static class Parameters {
    private static List<string> ids = new List<string>();
    public static List<string> Ids {
      get {
        if (ZNetScene.instance && ZNetScene.instance.m_namedPrefabs.Count != ids.Count)
          ids = ZNetScene.instance.GetPrefabNames();
        return ids;
      }
    }
    private static List<string> itemIds = new List<string>();
    public static List<string> ItemIds {
      get {
        if (ObjectDB.instance && ObjectDB.instance.m_items.Count != itemIds.Count)
          itemIds = ObjectDB.instance.m_items.Select(item => item.name).ToList();
        return itemIds;
      }
    }
    public static List<string> None = new List<string>() { "[NONE]" };
    public static List<string> Number = new List<string>() { "[NUMBER]" };
    public static List<string> Origin = new List<string>() { "player", "object", "world" };
    public static List<string> CreateNone(string name, string type) => new List<string>() { $"[{name} has no parameters]" };
    public static List<string> Create(string name, string type) => new List<string>() { $"[{name} must be a {type}]" };
  }

  [HarmonyPatch(typeof(Terminal.ConsoleCommand), "GetTabOptions")]
  public class UseParameterSpecificOptions {
    private static string GetInput() {
      if (!Console.instance && !Chat.instance) return "";
      var input = Console.instance ? Console.instance.m_input : Chat.instance.m_input;
      if (input.text == "" && Chat.instance) input = Chat.instance.m_input;
      return input.text;
    }
    private static string GetName(string parameter) {
      var split = parameter.Split('=');
      if (split.Length < 2) return "";
      return split[0];
    }
    private static int GetNameIndex(string parameter) => parameter.Split('|').Length - 1;
    public static bool Prefix(ref List<string> __result) {
      var input = GetInput();
      var parameters = input.Split(' ');
      if (parameters.Length < 2) return true;
      var command = parameters.First();
      parameters = parameters.Skip(1).ToArray();
      var parameter = parameters.Last();
      var name = GetName(parameter);
      var index = 0;
      if (name != "") {
        // Named parameter can appear anywhere so makes more sense to return their internal index.
        index = GetNameIndex(parameter);
      } else {
        // Ignore named parameters for the index.
        index = TerminalUtils.GetPositionalParameters(parameters).Count() - 1;
        var substitutions = TerminalUtils.GetSubstitutions(parameters).Count();
        for (var i = 0; i < parameters.Length; i++) {
          var par = parameters[i];
          var count = par.Count(character => character == '$');
          // Ignore substituded parameters for the index.
          index -= count;
          substitutions -= count;
          if (substitutions <= 0) {
            // For substituded, use the name/index where it appears.
            name = GetName(par);
            if (name != "") {
              // Cases to handle:
              // 1. =$ foo|bar -> substitutions: 0, parameter: 1, par: 0 => 1
              // 2. =$|$ foo ->  substitutions: -1, parameter: 0, par: 1 => 0
              // 3. =$|$ foo bar -> substitutions: 0, parameter: 0, par: 1 => 1
              // 4. =foo|$ bar -> substitutions: 0, parameter: 0, par: 1 => 1
              index = substitutions + GetNameIndex(parameter) + GetNameIndex(par);
            } else {
              index = i;
            }
            break;
          }
        }
      }
      __result = CommandParameters.Fetch(command, index, name);
      return false;
    }
  }

  [HarmonyPatch(typeof(Terminal), "tabCycle")]
  public class UseParameterSpecificCycling {
    public static void Prefix(Terminal __instance, ref string word) {
      word = TerminalUtils.GetLastWord(__instance);
    }
  }

  [HarmonyPatch(typeof(Terminal), "updateSearch")]
  public class UseParameterSpecificSearch {
    public static void Prefix(Terminal __instance, ref string word) {
      word = TerminalUtils.GetLastWord(__instance);
    }
  }
}
