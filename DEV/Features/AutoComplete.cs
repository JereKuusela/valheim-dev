using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace DEV {
  using OptionsFetcher = Func<int, string, List<string>>;
  ///<summary>Provides improved autocomplete (options/some info for each parameter, support for named parameters).</summary>
  public static class AutoComplete {
    private static Dictionary<string, OptionsFetcher> OptionsFetchers = new Dictionary<string, OptionsFetcher>();
    ///<summary>Returns options, either from the custom or the default options fetcher.</summary>
    public static List<string> GetOptions(string command, int index, string parameter) {
      if (OptionsFetchers.ContainsKey(command)) return OptionsFetchers[command](index, parameter);
      if (!Terminal.commands.ContainsKey(command)) return new List<string>();
      var fetcher = Terminal.commands[command].m_tabOptionsFetcher;
      if (fetcher != null) return fetcher();
      return new List<string>();
    }
    ///<summary>Registers a new custom options fetcher.</summary>
    public static void Register(string command, OptionsFetcher fetcher) => OptionsFetchers[command] = fetcher;

    ///<summary>Registers an options fetcher without parameters.</summary>
    public static void RegisterEmpty(string command) {
      Register(command, (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        return ParameterInfo.None;
      });
    }
    ///<summary>Registers an options fetcher for an admion action.</summary>
    public static void RegisterAdmin(string command) {
      Register(command, (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Create("Name / IP / UserId");
        return ParameterInfo.None;
      });
    }
    ///<summary>Registers an options fetcher with only the default fetcher.</summary>
    public static void RegisterDefault(string command) {
      Register(command, (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return Terminal.commands[command].m_tabOptionsFetcher();
        return ParameterInfo.None;
      });
    }
  }

  ///<summary>Helper class for parameter options/info. The main purpose is to provide some caching to avoid performance issues.</summary>
  public static class ParameterInfo {
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
    private static List<string> playerNames = new List<string>();
    public static List<string> PlayerNames {
      get {
        if (ZNet.instance && ZNet.instance.m_players.Count != playerNames.Count)
          playerNames = ZNet.instance.m_players.Select(player => player.m_name).ToList();
        return playerNames;
      }
    }
    private static List<string> hairs = new List<string>();
    public static List<string> Hairs {
      get {
        // Missing proper caching.
        if (ObjectDB.instance)
          hairs = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Hair").Select(item => item.name).ToList();
        return hairs;
      }
    }
    private static List<string> beards = new List<string>();
    public static List<string> Beards {
      get {
        // Missing proper caching.
        if (ObjectDB.instance)
          beards = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Beard").Select(item => item.name).ToList();
        return beards;
      }
    }
    private static List<string> keyCodes = new List<string>();
    public static List<string> KeyCodes {
      get {
        if (keyCodes.Count == 0) {
          var values = Enum.GetNames(typeof(KeyCode));
          keyCodes = values.Select(value => value.ToLower()).ToList();
        }
        return keyCodes;
      }
    }
    private static List<string> keyCodesWithNegative = new List<string>();
    public static List<string> KeyCodesWithNegative {
      get {
        if (keyCodesWithNegative.Count == 0) {
          var values = Enum.GetNames(typeof(KeyCode));
          keyCodesWithNegative = values.Select(value => value.ToLower()).ToList();
          keyCodesWithNegative.AddRange(keyCodesWithNegative.Select(value => "-" + value).ToList());
        }
        return keyCodesWithNegative;
      }
    }
    public static List<string> CommandNames {
      get {
        return Terminal.commands.Keys.ToList();
      }
    }
    private static string Format(string value) {
      if (!value.EndsWith(".") && !value.EndsWith("!"))
        value += ".";
      return "?" + value;
    }
    public static List<string> None = new List<string>() { Format("Too many parameters") };
    public static List<string> InvalidNamed = new List<string>() { Format("Invalid named parameter") };
    public static List<string> Origin = new List<string>() { "player", "object", "world" };
    public static List<string> Create(string name, string type) => new List<string>() { Format($"{name} should be a {type}") };
    public static List<string> Create(string name) => new List<string>() { Format($"{name}") };
  }

  [HarmonyPatch(typeof(Terminal.ConsoleCommand), "GetTabOptions")]
  public class GetTabOptionsWithImprovedAutoComplete {
    private static Terminal GetInput() {
      if (!Console.instance && !Chat.instance) return null;
      Terminal input = Console.instance == null ? Chat.instance as Terminal : Console.instance as Terminal;
      if (input.m_input.text == "" && Chat.instance) input = Chat.instance;
      return input;
    }
    private static string GetName(string parameter) {
      var split = parameter.Split('=');
      if (split.Length < 2) return "";
      return split[0];
    }
    private static int GetNameIndex(string parameter) => parameter.Split(',').Length - 1;

    private static List<string> GetOptions(string[] parameters) {
      var commandName = parameters.First();
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
        if (Settings.Substitution) {
          var substitutions = TerminalUtils.GetAmountOfSubstitutions(parameters);
          for (var i = 0; i < parameters.Length; i++) {
            if (substitutions <= 0) break; // Early break if there are no substitutions.
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
                // 1. =$ foo,bar -> substitutions: 0, parameter: 1, par: 0 => 1
                // 2. =$,$ foo ->  substitutions: -1, parameter: 0, par: 1 => 0
                // 3. =$,$ foo bar -> substitutions: 0, parameter: 0, par: 1 => 1
                // 4. =foo,$ bar -> substitutions: 0, parameter: 0, par: 1 => 1
                index = substitutions + GetNameIndex(parameter) + GetNameIndex(par);
              } else {
                index = i;
              }
            }
          }

        }
      }
      return AutoComplete.GetOptions(commandName, index, name);
    }
    public static bool Prefix(ref List<string> __result) {
      if (!Settings.ImprovedAutoComplete) return true;
      var input = GetInput();
      var command = input.m_input.text;
      var parameters = command.Split(' ');
      if (parameters.Length < 2)
        __result = input.m_commandList;
      else
        __result = GetOptions(parameters);
      return false;
    }
  }

  [HarmonyPatch(typeof(Terminal), "tabCycle")]
  public class TabCycleWithImprovedAutoComplete {
    public static void Prefix(Terminal __instance, ref List<string> options, ref string word) {
      if (!Settings.ImprovedAutoComplete) return;
      // Auto complete is parameter specific, so need to use the current word instead of always using the first.
      word = TerminalUtils.GetLastWord(__instance);
      if (options == null) return;
      // Cycling the help text wouldn't make any sense.
      options = options.Where(option => !option.Contains("?")).ToList();
    }
  }

  [HarmonyPatch(typeof(Terminal), "updateSearch")]
  public class UpdateSearchWithImprovedAutoComplete {
    public static void Prefix(Terminal __instance, ref string word) {
      if (!Settings.ImprovedAutoComplete || !__instance.m_search) return;
      // Auto complete is parameter specific, so need to use the current word instead of always using the first.
      word = TerminalUtils.GetLastWord(__instance);

    }
    public static void Postfix(Terminal __instance, List<string> options, ref string word) {
      if (!Settings.ImprovedAutoComplete || options == null || !__instance.m_search) return;
      var helpText = options.All(option => option.StartsWith("?"));
      // Always show the help text since there isn't any real search option.
      if (helpText)
        __instance.m_search.text = "<color=white>" + string.Join(", ", options.Select(option => option.Substring(1))) + "</color>";
      if (Settings.DisableParameterWarnings) {
        var index = __instance.m_search.text.IndexOf(", <color=yellow>WARNING</color>");
        if (index >= 0)
          __instance.m_search.text = __instance.m_search.text.Substring(0, index) + "</color>"; // Bit hacky to manually add color tag back.
      }
    }
  }
}
