using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Service;

namespace DEV {
  public static class Settings {
    public static bool Cheats => (ZNet.instance && ZNet.instance.IsServer()) || Console.instance.IsCheatsEnabled();
    public static ConfigEntry<bool> configMapCoordinates;
    public static bool MapCoordinates => Cheats && configMapCoordinates.Value;
    public static ConfigEntry<bool> configShowPrivatePlayers;
    public static bool ShowPrivatePlayers => Cheats && configShowPrivatePlayers.Value;
    public static ConfigEntry<bool> configAutoDevcommands;
    public static bool AutoDevcommands => configAutoDevcommands.Value;
    public static ConfigEntry<bool> configAutoDebugMode;
    public static bool AutoDebugMode => configAutoDebugMode.Value;
    public static ConfigEntry<bool> configAutoGodMode;
    public static bool AutoGodMode => configAutoGodMode.Value;
    public static ConfigEntry<bool> configAutoGhostMode;
    public static bool AutoGhostMode => configAutoGhostMode.Value;
    public static ConfigEntry<bool> configAutoNoCost;
    public static bool AutoNoCost => configAutoNoCost.Value;
    public static ConfigEntry<bool> configDisableEvents;
    public static bool DisableEvents => configDisableEvents.Value;
    public static ConfigEntry<bool> configDebugConsole;
    public static bool DebugConsole => configDebugConsole.Value;
    public static ConfigEntry<bool> configAutoFly;
    public static bool AutoFly => configAutoFly.Value;
    public static ConfigEntry<bool> configGodModeNoStamina;
    public static bool GodModeNoStamina => configGodModeNoStamina.Value;
    public static ConfigEntry<bool> configGodModeNoStagger;
    public static bool GodModeNoStagger => configGodModeNoStagger.Value;
    public static ConfigEntry<bool> configAliasing;
    public static bool Aliasing => configAliasing.Value;
    public static ConfigEntry<bool> configDisableParameterWarnings;
    public static bool DisableParameterWarnings => configDisableParameterWarnings.Value;
    public static ConfigEntry<bool> configSubstitution;
    public static bool Substitution => configSubstitution.Value;
    public static ConfigEntry<bool> configImprovedAutoComplete;
    public static bool ImprovedAutoComplete => configImprovedAutoComplete.Value;
    public static ConfigEntry<bool> configMultiCommand;
    public static bool MultiCommand => configMultiCommand.Value;
    public static ConfigEntry<bool> configNoDrops;
    public static bool NoDrops => Cheats && configNoDrops.Value;
    public static ConfigEntry<string> configCommandAliases;
    private static Dictionary<string, string> Aliases = new Dictionary<string, string>();
    public static string[] AliasKeys = new string[0];

    private static void ParseAliases(string value) {
      Aliases = value.Split('¤').Select(str => str.Split(' ')).ToDictionary(split => split[0], split => string.Join(" ", split.Skip(1)));
      Aliases = Aliases.Where(kvp => kvp.Key != "").ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
      AliasKeys = Aliases.Keys.OrderBy(key => key).ToArray();
    }
    public static string GetAlias(string key) => Aliases.ContainsKey(key) ? Aliases[key] : "_";
    public static void RegisterCommands() {
      foreach (var alias in Aliases) {
        AliasCommand.AddCommand(alias.Key, alias.Value);
      }
    }

    private static void SaveAliases() {
      var value = string.Join("¤", Aliases.Select(kvp => kvp.Key + " " + kvp.Value));
      configCommandAliases.Value = value;
    }

    public static void AddAlias(string alias, string value) {
      if (value == "") {
        RemoveAlias(alias);
      } else {
        Aliases[alias] = value;
        SaveAliases();
      }
    }
    public static void RemoveAlias(string alias) {
      if (!Aliases.ContainsKey(alias)) return;
      Aliases.Remove(alias);
      SaveAliases();
    }

    public static void Init(ConfigFile config) {
      var section = "1. General";
      configNoDrops = config.Bind(section, "No creature drops", false, "Disables drops from creatures (if you control the zone), intended to fix high star enemies crashing the game.");
      configAutoDebugMode = config.Bind(section, "Automatic debug mode", false, "Automatically enables debug mode when enabling devcommands.");
      configAutoFly = config.Bind(section, "Automatic fly mode", false, "Automatically enables fly mode when enabling devcommands (if debug mode).");
      configAutoNoCost = config.Bind(section, "Automatic no cost mode", false, "Automatically enables no cost mode when enabling devcommands (if debug mode).");
      configAutoGodMode = config.Bind(section, "Automatic god mode", false, "Automatically enables god mode when enabling devcommands.");
      configAutoGhostMode = config.Bind(section, "Automatic ghost mode", false, "Automatically enables ghost mode when enabling devcommands.");
      configAutoDevcommands = config.Bind(section, "Automatic devcommands", true, "Automatically enables devcommands when joining servers.");
      configGodModeNoStamina = config.Bind(section, "No stamina usage with god mode", true, "");
      configGodModeNoStagger = config.Bind(section, "No staggering with god mode", true, "");
      configMapCoordinates = config.Bind(section, "Show map coordinates", true, "The map shows coordinates on hover.");
      configShowPrivatePlayers = config.Bind(section, "Show private players", false, "The map shows private players.");
      configDisableEvents = config.Bind(section, "Disable random events", false, "Disables random events (server side setting).");
      section = "2. Console";
      configAliasing = config.Bind(section, "Alias system", true, "Enables the command aliasing system (allows creating new commands).");
      configImprovedAutoComplete = config.Bind(section, "Improved autocomplete", true, "Enables parameter info or options for every parameter.");
      configCommandAliases = config.Bind(section, "Command aliases", "", "Internal data for aliases.");
      configMultiCommand = config.Bind(section, "Multiple commands per line", true, "Enables multiple commands when separated with ;.");
      configSubstitution = config.Bind(section, "Substitution system", true, "Enables the command parameter substitution system ($ gets replaced with the next free parameter).");
      configDebugConsole = config.Bind(section, "Debug console", false, "Extra debug information about aliasing.");
      configDisableParameterWarnings = config.Bind(section, "Disable parameter warnings", false, "Removes warning texts from some command parameter descriptions.");
      configCommandAliases.SettingChanged += (s, e) => ParseAliases(configCommandAliases.Value);
      ParseAliases(configCommandAliases.Value);
    }


    public static List<string> Options = new List<string>() {
      "map_coordinates", "private_players", "auto_devcommands", "auto_debugmode", "auto_fly", "god_no_stagger",
      "auto_nocost", "auto_ghost", "auto_god","debug_console", "no_drops", "aliasing", "god_no_stamina",
      "substitution", "improved_autocomplete", "disable_events", "disable_warnings", "multiple_commands"
    };
    private static string State(bool value) => value ? "enabled" : "disabled";
    private static void Toggle(Terminal context, ConfigEntry<bool> setting, string name, bool reverse = false) {
      setting.Value = !setting.Value;
      Helper.AddMessage(context, $"{name} {State(reverse ? !setting.Value : setting.Value)}.");

    }
    public static void UpdateValue(Terminal context, string key, string value) {
      if (key == "map_coordinates") Toggle(context, configMapCoordinates, "Map coordinates");
      if (key == "private_players") Toggle(context, configShowPrivatePlayers, "Private players");
      if (key == "auto_devcommands") Toggle(context, configAutoDevcommands, "Automatic devcommands");
      if (key == "auto_debugmode") Toggle(context, configAutoDebugMode, "Automatic debug mode");
      if (key == "auto_fly") Toggle(context, configAutoFly, "Automatic fly");
      if (key == "auto_nocost") Toggle(context, configAutoNoCost, "Automatic no cost");
      if (key == "auto_god") Toggle(context, configAutoGodMode, "Automatic god mode");
      if (key == "auto_ghost") Toggle(context, configAutoGhostMode, "Automatic ghost mode");
      if (key == "debug_console") Toggle(context, configDebugConsole, "Debug console");
      if (key == "no_drops") Toggle(context, configNoDrops, "Creature drops", true);
      if (key == "aliasing") Toggle(context, configAliasing, "Command aliasing");
      if (key == "substitution") Toggle(context, configSubstitution, "Command parameter substitution");
      if (key == "improved_autocomplete") Toggle(context, configImprovedAutoComplete, "Improved autocomplete");
      if (key == "disable_events") Toggle(context, configDisableEvents, "Random events", true);
      if (key == "disable_warnings") Toggle(context, configDisableParameterWarnings, "Command parameter warnings", true);
      if (key == "multiple_commands") Toggle(context, configMultiCommand, "Multiple commands per line");
      if (key == "god_no_stamina") Toggle(context, configGodModeNoStamina, "Stamina usage with god mode", true);
      if (key == "god_no_stagger") Toggle(context, configGodModeNoStagger, "Staggering with god mode", true);
    }
  }
}
