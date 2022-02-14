using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace ServerDevcommands {
  using NamedOptionsFetchers = Dictionary<string, Func<int, List<string>>>;

  using OptionsFetcher = Func<int, List<string>>;
  ///<summary>Provides improved autocomplete (options/some info for each parameter, support for named parameters).</summary>
  public static class AutoComplete {
    private static Dictionary<string, OptionsFetcher> OptionsFetchers = new Dictionary<string, OptionsFetcher>();
    private static Dictionary<string, NamedOptionsFetchers> OptionsNamedFetchers = new Dictionary<string, NamedOptionsFetchers>();
    ///<summary>Returns options, either from the custom or the default options fetcher.</summary>
    public static List<string> GetOptions(string command, int index, string namedParameter) {
      command = command.ToLower();
      if (namedParameter != "") {
        if (OptionsNamedFetchers.TryGetValue(command, out var namedOptions)) {
          if (namedOptions.TryGetValue(namedParameter.ToLower(), out var namedFetcher)) {
            return namedFetcher(index);
          }
        }
        return ParameterInfo.InvalidNamed(namedParameter);
      }
      if (OptionsFetchers.ContainsKey(command)) return OptionsFetchers[command](index) ?? ParameterInfo.None;
      if (!Terminal.commands.ContainsKey(command)) return ParameterInfo.None;
      var fetcher = Terminal.commands[command].m_tabOptionsFetcher;
      if (fetcher != null) return fetcher();
      return ParameterInfo.None;
    }
    ///<summary>Registers a new custom options fetcher.</summary>
    public static void Register(string command, OptionsFetcher fetcher, NamedOptionsFetchers namedFetchers) {
      OptionsFetchers[command.ToLower()] = fetcher;
      OptionsNamedFetchers[command.ToLower()] = namedFetchers.ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value);
    }
    public static void Register(string command, OptionsFetcher fetcher) {
      OptionsFetchers[command] = fetcher;
    }

    ///<summary>Registers an options fetcher without parameters.</summary>
    public static void RegisterEmpty(string command) {
      Register(command, (int index) => null);
    }
    ///<summary>Registers an options fetcher for an admion action.</summary>
    public static void RegisterAdmin(string command) {
      Register(command, (int index) => {
        if (index == 0) return ParameterInfo.Create("Name / IP / UserId");
        return null;
      });
    }
    ///<summary>Registers an options fetcher with only the default fetcher.</summary>
    public static void RegisterDefault(string command) {
      Register(command, (int index) => {
        if (index == 0) return Terminal.commands[command].m_tabOptionsFetcher();
        return null;
      });
    }
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
      // While executing, options are used to make the first parameter case insensitive. So the default options should be returned.
      if (TerminalUtils.IsExecuting) return true;
      if (!Settings.ImprovedAutoComplete) return true;
      var input = GetInput();
      var text = input.m_input.text;
      var parameters = text.Split(' ');
      if (parameters.Length < 2) __result = input.m_commandList;
      else __result = GetOptions(parameters);
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
      if (Settings.CommandDescriptions && options == __instance.m_commandList) {
        if (Terminal.commands.TryGetValue(word, out var command))
          options = ParameterInfo.Create(command.Description);
      }
      var helpText = options.All(option => option.StartsWith("?"));
      // Always show the help text since there isn't any real search option.
      if (helpText)
        __instance.m_search.text = "<color=white>" + string.Join(", ", options.Select(option => option.Substring(1))) + "</color>";
    }
  }
}