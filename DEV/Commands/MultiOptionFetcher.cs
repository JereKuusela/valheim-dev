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
    public static List<string> None = new List<string>() { "No parameters!" };
    public static List<string> Number = new List<string>() { "[NUMBER]" };
  }

  [HarmonyPatch(typeof(Terminal.ConsoleCommand), "GetTabOptions")]
  public class UseParameterSpecificOptions {
    private static string GetInput() => Console.instance.m_input.text == "" ? Chat.instance.m_input.text : Console.instance.m_input.text;
    private static int GetInputLength() => GetInput().Split(' ').Length;
    private static string GetParameter() {
      var split = GetInput().Split(' ').Last().Split('=');
      if (split.Length < 2) return "";
      return split[0];
    }
    public static bool Prefix(Terminal.ConsoleCommand __instance, ref List<string> __result) {
      var length = GetInputLength();
      if (length < 2) return true;
      var index = length - 2; // -1 -> Lenght to index. -1 -> Ignore the command name.
      __result = CommandParameters.Fetch(__instance.Command, index, GetParameter());
      return false;
    }
  }

  [HarmonyPatch(typeof(Terminal), "tabCycle")]
  public class UseParameterSpecificCycling {
    public static void Prefix(Terminal __instance, ref string word) {
      word = __instance.m_input.text.Split(' ').Last().Split('=').Last();
    }
  }

  [HarmonyPatch(typeof(Terminal), "updateSearch")]
  public class UseParameterSpecificSearch {
    public static void Prefix(Terminal __instance, ref string word) {
      word = __instance.m_input.text.Split(' ').Last().Split('=').Last();
    }
  }
}
