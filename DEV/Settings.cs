using System.Collections.Generic;
using BepInEx.Configuration;

namespace DEV {
  public static class Settings {
    public static bool Cheats => (ZNet.instance && ZNet.instance.IsServer()) || Console.instance.IsCheatsEnabled();
    public static ConfigEntry<bool> configMapCoordinates;
    public static bool MapCoordinates => Cheats && configMapCoordinates.Value;
    public static ConfigEntry<bool> configShowPrivatePlayers;
    public static bool ShowPrivatePlayers => Cheats && configShowPrivatePlayers.Value;


    public static void Init(ConfigFile config) {
      var section = "1. General";
      configMapCoordinates = config.Bind(section, "Show map coordinates", true, "The map shows coordinates on hover.");
      configShowPrivatePlayers = config.Bind(section, "Show private players", true, "The map shows private players.");

    }


    public static List<string> Options = new List<string>() {
      "map_coordinates", "private_players"
    };
    private static string State(bool value) => value ? "enabled" : "disabled";
    private static void Toggle(Terminal context, ConfigEntry<bool> setting, string name, bool reverse = false) {
      setting.Value = !setting.Value;
      BaseCommands.AddMessage(context, $"{name} {State(reverse ? !setting.Value : setting.Value)}.");

    }
    public static void UpdateValue(Terminal context, string key, string value) {
      if (key == "map_coordinates") Toggle(context, configMapCoordinates, "Map coordinates");
      if (key == "private_players") Toggle(context, configShowPrivatePlayers, "Private players");
    }
  }
}