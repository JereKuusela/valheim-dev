using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

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
    public static ConfigEntry<bool> configDebugConsole;
    public static bool DebugConsole => configDebugConsole.Value;
    public static ConfigEntry<bool> configAutoFly;
    public static bool AutoFly => configAutoFly.Value;
    public static ConfigEntry<bool> configDefaultAliases;
    public static ConfigEntry<string> configCommandAliases;
    private static Dictionary<string, string> CustomAliases = new Dictionary<string, string>();
    private static Dictionary<string, string> DefaultAliases = new Dictionary<string, string>() {
      {"move", "object move=$|$ radius=$ id=$"},
      {"rotate", "object rotate=$|$ radius=$ id=$"},
      {"scale", "object scale=$|$ radius=$ id=$"},
      {"stars", "object stars=$ radius=$ id=$"},
      {"health", "object health=$ radius=$ id=$"},
      {"remove", "object remove id=$ radius=$"},
      {"change_helmet", "object helmet=$ radius=$ id=$"},
      {"change_left", "object left_hand=$ radius=$ id=$"},
      {"change_right", "object right_hand=$ radius=$ id=$"},
      {"change_legs", "object legs=$ radius=$ id=$"},
      {"change_chest", "object chest=$ radius=$ id=$"},
      {"change_shoulders", "object shoulders=$ radius=$ id=$"},
      {"change_utility", "object utility=$ radius=$ id=$"},
      {"essential", "object tame health=1E30 radius=$ id=$"}
    };
    private static Dictionary<string, string> Aliases = new Dictionary<string, string>();
    public static string[] AliasKeys = new string[0];

    private static void ParseAliases(string value) {
      CustomAliases = value.Split('¤').Select(str => str.Split(' ')).ToDictionary(split => split[0], split => string.Join(" ", split.Skip(1)));
      CustomAliases = CustomAliases.Where(kvp => kvp.Key != "").ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
      Aliases.Clear();
      foreach (var kvp in CustomAliases) Aliases.Add(kvp.Key, kvp.Value);
      if (Settings.configDefaultAliases.Value) {
        foreach (var kvp in DefaultAliases) {
          if (!Aliases.ContainsKey(kvp.Key))
            Aliases.Add(kvp.Key, kvp.Value);
        }
      }
      AliasKeys = Aliases.Keys.OrderByDescending(key => key.Length).ToArray();
    }
    public static string GetAlias(string key) => Aliases.ContainsKey(key) ? Aliases[key] : "_";
    public static void RegisterCommands() {
      foreach (var alias in Aliases) {
        AliasCommand.AddCommand(alias.Key, alias.Value);
      }
    }
    public static void RegisterDefaultAliases() {
      if (configDefaultAliases.Value) {
        foreach (var alias in DefaultAliases) {
          if (CustomAliases.ContainsKey(alias.Key)) continue;
          AliasCommand.AddCommand(alias.Key, alias.Value);
        }
      } else {

        foreach (var alias in DefaultAliases) {
          if (CustomAliases.ContainsKey(alias.Key)) continue;
          Terminal.commands.Remove(alias.Key);
        }
      }
    }
    private static void SaveAliases() {
      var value = string.Join("¤", CustomAliases.Select(kvp => kvp.Key + " " + kvp.Value));
      configCommandAliases.Value = value;
    }

    public static void AddAlias(string alias, string value) {
      if (value == "") {
        RemoveAlias(alias);
      } else {
        CustomAliases[alias] = value;
        SaveAliases();
      }
    }
    public static void RemoveAlias(string alias) {
      if (!CustomAliases.ContainsKey(alias)) return;
      CustomAliases.Remove(alias);
      SaveAliases();
    }

    public static void Init(ConfigFile config) {
      var section = "1. General";
      configDefaultAliases = config.Bind(section, "Default aliases", true, "Adds some useful aliases for common operations.");
      configDefaultAliases.SettingChanged += (s, e) => {
        ParseAliases(configCommandAliases.Value);
        RegisterDefaultAliases();
      };
      configDebugConsole = config.Bind(section, "Debug console", false, "Extra debug information about aliasing.");
      configAutoDebugMode = config.Bind(section, "Automatic debug mode", false, "Automatically enables debug mode when enabling devcommands.");
      configAutoFly = config.Bind(section, "Automatic fly mode", false, "Automatically enables fly mode when enabling devcommands (if debug mode).");
      configAutoNoCost = config.Bind(section, "Automatic no cost mode", false, "Automatically enables no cost mode when enabling devcommands (if debug mode).");
      configAutoGodMode = config.Bind(section, "Automatic god mode", false, "Automatically enables god mode when enabling devcommands.");
      configAutoGhostMode = config.Bind(section, "Automatic ghost mode", false, "Automatically enables ghost mode when enabling devcommands.");
      configAutoDevcommands = config.Bind(section, "Automatic devcommands", true, "Automatically enables devcommands when joining servers.");
      configMapCoordinates = config.Bind(section, "Show map coordinates", true, "The map shows coordinates on hover.");
      configShowPrivatePlayers = config.Bind(section, "Show private players", true, "The map shows private players.");
      configCommandAliases = config.Bind(section, "Command aliases", "", "Internal data for aliases.");
      configCommandAliases.SettingChanged += (s, e) => ParseAliases(configCommandAliases.Value);
      ParseAliases(configCommandAliases.Value);
    }


    public static List<string> Options = new List<string>() {
      "map_coordinates", "private_players", "auto_devcommands", "auto_debugmode", "auto_fly",
      "auto_nocost", "auto_ghost", "auto_god", "default_aliases", "debug_console"
    };
    private static string State(bool value) => value ? "enabled" : "disabled";
    private static void Toggle(Terminal context, ConfigEntry<bool> setting, string name, bool reverse = false) {
      setting.Value = !setting.Value;
      BaseCommands.AddMessage(context, $"{name} {State(reverse ? !setting.Value : setting.Value)}.");

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
      if (key == "default_aliases") Toggle(context, configDefaultAliases, "Default aliases");
      if (key == "debug_console") Toggle(context, configDebugConsole, "Debug console");
    }
  }
}