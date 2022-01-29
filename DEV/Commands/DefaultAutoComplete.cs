using System.Collections.Generic;

namespace DEV {
  ///<summary>Adds the improved auto complete to the default commands.</summary>
  public static class DefaultAutoComplete {
    public static void Register() {
      AutoComplete.Register("bind", (int index, string parameter) => {
        if (parameter == "keys") return ParameterInfo.KeyCodes;
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.KeyCodesWithNegative;
        return ParameterInfo.Create("The command to bind.");
      });
      AutoComplete.RegisterEmpty("challenge");
      AutoComplete.RegisterEmpty("cheers");
      AutoComplete.RegisterEmpty("clear");
      AutoComplete.Register("fov ", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Create("Amount", "number");
        return ParameterInfo.None;
      });
      AutoComplete.RegisterEmpty("hidebetatext");
      AutoComplete.Register("help ", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Create("Page", "number");
        if (index == 1) return ParameterInfo.Create("Page size", "number (default is 5)");
        return ParameterInfo.None;
      });
      AutoComplete.RegisterEmpty("info");
      AutoComplete.Register("lodbias", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Create("Amount", "number (from 1 to 5)");
        return ParameterInfo.None;
      });
      AutoComplete.RegisterEmpty("nomap");
      AutoComplete.RegisterEmpty("nonono");
      AutoComplete.RegisterEmpty("opterrain");
      AutoComplete.RegisterEmpty("point");
      AutoComplete.RegisterEmpty("ping");
      AutoComplete.RegisterEmpty("printbinds");
      AutoComplete.RegisterEmpty("resetbinds");
      AutoComplete.RegisterEmpty("resetspawn");
      AutoComplete.RegisterEmpty("respawn");
      AutoComplete.Register("s", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        return ParameterInfo.Create("Message");
      });
      AutoComplete.Register("say", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        return ParameterInfo.Create("Message");
      });
      AutoComplete.RegisterEmpty("sit");
      AutoComplete.RegisterEmpty("thumbsup");
      AutoComplete.RegisterEmpty("tutorialreset");
      AutoComplete.Register("unbind", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.KeyCodes;
        return ParameterInfo.None;
      });
      AutoComplete.RegisterEmpty("wave");
      AutoComplete.Register("W", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.PlayerNames;
        return ParameterInfo.Create("Message");
      });

      AutoComplete.RegisterAdmin("ban");
      AutoComplete.RegisterEmpty("banned");
      AutoComplete.RegisterAdmin("kick");
      AutoComplete.RegisterEmpty("save");
      AutoComplete.RegisterAdmin("unban");

      AutoComplete.RegisterDefault("addstatus");
      AutoComplete.Register("beard", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Beards;
        return ParameterInfo.None;
      });
      AutoComplete.RegisterEmpty("clearstatus");
      AutoComplete.RegisterEmpty("dpsdebug");
      AutoComplete.RegisterDefault("env");
      AutoComplete.RegisterEmpty("exploremap");
      // Event added to the replaced command.
      AutoComplete.Register("ffsmooth", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Create("0 = normal, 1 = add smooth movement");
        return ParameterInfo.None;
      });
      AutoComplete.Register("find", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Ids;
        if (index == 1) return ParameterInfo.Create("Max amount", "number (default is 1)");
        return ParameterInfo.None;
      });
      AutoComplete.RegisterEmpty("fly");
      AutoComplete.RegisterEmpty("freefly");
      AutoComplete.RegisterEmpty("gc");
      AutoComplete.RegisterEmpty("genloc");
      AutoComplete.RegisterEmpty("ghost");
      AutoComplete.RegisterEmpty("god");
      AutoComplete.Register("goto", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Create("X coordinate", "number");
        if (index == 1) return ParameterInfo.Create("Z coordinate", "number");
        return ParameterInfo.None;
      });
      AutoComplete.Register("hair", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Hairs;
        return ParameterInfo.None;
      });
      AutoComplete.RegisterEmpty("heal");
      AutoComplete.Register("itemset", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return Terminal.commands["itemset"].m_tabOptionsFetcher();
        if (index == 1) return new List<string>() { "keep", "clear" };
        return ParameterInfo.None;
      });
      AutoComplete.RegisterEmpty("killall");
      AutoComplete.RegisterEmpty("listkeys");
      AutoComplete.RegisterDefault("location");
      AutoComplete.Register("model", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Create("0 = male, 1 = female");
        return ParameterInfo.None;
      });
      AutoComplete.RegisterEmpty("nocost");
      AutoComplete.Register("players", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Create("Amount", "number from (0 to 5)");
        return ParameterInfo.None;
      });
      // Pos added to the replaced command.
      AutoComplete.RegisterEmpty("printcreatures");
      AutoComplete.RegisterEmpty("printlocations");
      AutoComplete.RegisterEmpty("puke");
      AutoComplete.Register("raiseskill", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return Terminal.commands["raiseskill"].m_tabOptionsFetcher();
        if (index == 1) return ParameterInfo.Create("Amount", "number (from -100 to 100)");
        return ParameterInfo.None;
      });
      AutoComplete.RegisterEmpty("randomevent");
      AutoComplete.RegisterEmpty("removebirds");
      AutoComplete.RegisterEmpty("removedrops");
      AutoComplete.RegisterEmpty("removefish");
      AutoComplete.RegisterEmpty("resetcharacter");
      AutoComplete.RegisterEmpty("resetenv");
      AutoComplete.RegisterEmpty("resetkeys");
      AutoComplete.RegisterEmpty("resetwind");
      AutoComplete.Register("setkey", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Create("Name");
        return ParameterInfo.None;
      });
      AutoComplete.RegisterDefault("setpower");
      AutoComplete.Register("skiptime", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Create("Seconds", "number (default 240), <color=yellow>WARNING</color>: High negative values may cause issues because object timestamps won't get updated!");
        return ParameterInfo.None;
      });
      AutoComplete.RegisterEmpty("sleep");
      AutoComplete.Register("spawn", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Ids;
        if (index == 1) return ParameterInfo.Create("Amount", "number (default 1), <color=yellow>WARNING</color>: Very high values (100+) may crash the game!, <color=yellow>WARNING</color>: Some objects can't be removed after spawning!");
        if (index == 2) return ParameterInfo.Create("Level", "number (default 1), <color=yellow>WARNING</color>: High values (5+) may crash the server when the creature is killed!, <color=yellow>WARNING</color>: Some objects can't be removed after spawning!");
        return ParameterInfo.None;
      });
      AutoComplete.RegisterEmpty("stopevent");
      AutoComplete.RegisterEmpty("tame");
      AutoComplete.Register("test", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return new List<string>() { "oldcomfort" };
        return ParameterInfo.None;
      });
      AutoComplete.RegisterEmpty("time");
      AutoComplete.Register("tod", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Create("Amount", "number (from 0.0 to 1.0)");
        return ParameterInfo.None;
      });
      AutoComplete.Register("wind", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.Create("Angle", "number (from 0 to 360)");
        if (index == 1) return ParameterInfo.Create("Intensity", "number (from 0.0 to 1.0)");
        return ParameterInfo.None;
      });
    }
  }
}
