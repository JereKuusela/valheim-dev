using System.Collections.Generic;

namespace ServerDevcommands;
///<summary>Adds the improved auto complete to the default commands.</summary>
public static class DefaultAutoComplete
{
  public static void Register()
  {
    AutoComplete.RegisterEmpty("aggravate");
    AutoComplete.RegisterEmpty("challenge");
    AutoComplete.RegisterEmpty("cheers");
    AutoComplete.RegisterEmpty("clear");
    AutoComplete.Register("fov ", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Amount", "a positive number");
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("hidebetatext");
    AutoComplete.Register("help ", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Page", "number");
      if (index == 1) return ParameterInfo.Create("Page size", "a positive integer (default is 5)");
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("info");
    AutoComplete.Register("lodbias", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Amount", "a positive number");
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("nonono");
    AutoComplete.RegisterEmpty("opterrain");
    AutoComplete.RegisterEmpty("point");
    AutoComplete.RegisterEmpty("ping");
    AutoComplete.RegisterEmpty("resetbinds");
    AutoComplete.RegisterEmpty("resetsharedmap");
    AutoComplete.RegisterEmpty("resetspawn");
    AutoComplete.RegisterEmpty("respawn");
    AutoComplete.Register("s", (int index) =>
    {
      return ParameterInfo.Create("Message");
    });
    AutoComplete.Register("say", (int index) =>
    {
      return ParameterInfo.Create("Message");
    });
    AutoComplete.RegisterEmpty("restartparty");
    AutoComplete.RegisterEmpty("sit");
    AutoComplete.RegisterEmpty("thumbsup");
    AutoComplete.RegisterEmpty("tutorialreset");
    AutoComplete.RegisterEmpty("tutorialtoggle");
    AutoComplete.RegisterEmpty("wave");
    AutoComplete.Register("W", (int index) =>
    {
      if (index == 0) return ParameterInfo.PlayerNames;
      return ParameterInfo.Create("Message");
    });

    AutoComplete.RegisterAdmin("ban");
    AutoComplete.RegisterEmpty("banned");
    AutoComplete.RegisterAdmin("kick");
    AutoComplete.RegisterEmpty("save");
    AutoComplete.RegisterAdmin("unban");

    AutoComplete.RegisterDefault("addstatus");
    AutoComplete.Register("beard", (int index) =>
    {
      if (index == 0) return ParameterInfo.Beards;
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("clearstatus");
    AutoComplete.RegisterEmpty("dpsdebug");
    AutoComplete.RegisterEmpty("exploremap");
    // Event added to the replaced command.
    AutoComplete.Register("ffsmooth", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("0 = normal, 1 = add smooth movement");
      return ParameterInfo.None;
    });
    AutoComplete.Register("find", (int index) =>
    {
      if (index == 0) return ParameterInfo.Ids;
      if (index == 1) return ParameterInfo.Create("Max amount", "a positive integer (default 1)");
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("fly");
    AutoComplete.RegisterEmpty("freefly");
    AutoComplete.Register("forcedelete", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Radius", "in meters (from 0.0 to 50.0, default is 5.0).");
      if (index == 1) return ParameterInfo.ObjectIds;
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("gc");
    AutoComplete.RegisterEmpty("genloc");
    AutoComplete.RegisterEmpty("ghost");
    AutoComplete.RegisterEmpty("god");
    AutoComplete.Register("hair", (int index) =>
    {
      if (index == 0) return ParameterInfo.Hairs;
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("heal");
    AutoComplete.Register("itemset", (int index) =>
    {
      if (index == 0) return Terminal.commands["itemset"].m_tabOptionsFetcher();
      if (index == 1) return new() { "keep", "clear" };
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("killall");
    AutoComplete.RegisterEmpty("killenemies");
    AutoComplete.RegisterEmpty("killtame");
    AutoComplete.RegisterEmpty("listkeys");
    AutoComplete.Register("location", (int index) =>
    {
      if (index == 0) return ParameterInfo.LocationIds;
      if (index == 1) return new List<string>() { "SAVE" };
      return ParameterInfo.None;
    });
    AutoComplete.Register("maxfps", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Amount", "a positive integer");
      return ParameterInfo.None;
    });
    AutoComplete.Register("model", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("<color=yellow>0</color> = male, <color=yellow>1</color> = female");
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("nocost");
    AutoComplete.RegisterEmpty("noportals");
    AutoComplete.Register("players", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Amount", "a positive integer (0 disables the override)");
      return ParameterInfo.None;
    });
    // Pos added to the replaced command.
    AutoComplete.RegisterEmpty("printcreatures");
    AutoComplete.RegisterEmpty("printseeds");
    AutoComplete.RegisterEmpty("printlocations");
    AutoComplete.RegisterEmpty("puke");
    AutoComplete.RegisterEmpty("randomevent");
    AutoComplete.RegisterEmpty("removebirds");
    AutoComplete.RegisterEmpty("removedrops");
    AutoComplete.RegisterEmpty("removefish");
    AutoComplete.Register("recall ", (int index) =>
    {
      if (index == 0) return ParameterInfo.PlayerNames;
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("resetcharacter");
    AutoComplete.RegisterEmpty("resetenv");
    AutoComplete.RegisterEmpty("resetkeys");
    AutoComplete.RegisterEmpty("resetwind");
    AutoComplete.Register("removekey", (int index) =>
    {
      if (index == 0) return ParameterInfo.GlobalKeys;
      return ParameterInfo.None;
    });
    AutoComplete.Register("setkey", (int index) =>
    {
      if (index == 0) return ParameterInfo.GlobalKeys;
      return ParameterInfo.None;
    });
    AutoComplete.RegisterDefault("setpower");
    AutoComplete.Register("skiptime", (int index) =>
    {
      if (index == 0)
      {
        if (Settings.DisableParameterWarnings)
          return ParameterInfo.Create("Seconds", "a number (default 240.0)");
        else
          return ParameterInfo.Create("Seconds", "a number (default 240.0), <color=yellow>WARNING</color>: High negative values may cause issues because object timestamps won't get updated!");
      }
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("sleep");
    AutoComplete.Register("spawn", (int index) =>
    {
      if (index == 0) return ParameterInfo.ObjectIds;
      if (index == 1)
      {
        if (Settings.DisableParameterWarnings)
          return ParameterInfo.Create("Amount", "a positive integer (default 1)");
        else
          return ParameterInfo.Create("Amount", "a positive integer (default 1), <color=yellow>WARNING</color>: Very high values (100+) may crash the game!, <color=yellow>WARNING</color>: Some objects can't be removed after spawning!");
      }
      if (index == 2)
      {
        if (Settings.DisableParameterWarnings)
          return ParameterInfo.Create("Level", "a positive integer (default 1)");
        else
          return ParameterInfo.Create("Level", "a positive integer (default 1), <color=yellow>WARNING</color>: High values (5+) may crash the server when the creature is killed!, <color=yellow>WARNING</color>: Some objects can't be removed after spawning!");
      }
      if (index == 2) return ParameterInfo.Create("<color=yellow>p</color> to automatically pick up items. <color=yellow>e</color> to automatically equip items.");
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("stopevent");
    AutoComplete.RegisterEmpty("tame");
    AutoComplete.Register("test", (int index) =>
    {
      if (index == 0) return new() { "oldcomfort" };
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("time");
    AutoComplete.Register("timescale", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Multiplier", "sets how fast the time goes (from 0.0 to 3.0). Value 0 can be used to pause the game.");
      if (index == 1) return ParameterInfo.Create("Transition duration", "causes the change to be applied gradually over time (seconds). Default value 0 applies the change instantly.");
      return ParameterInfo.None;
    });
    AutoComplete.Register("tod", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Time", "overrides the time of the day (from 0.0 to 1.0, with 0.5 being the mid day). Value -1 removes the override.");
      return ParameterInfo.None;
    });
  }
}
