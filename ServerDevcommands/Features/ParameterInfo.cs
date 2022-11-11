using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ServerDevcommands;
///<summary>Helper class for parameter options/info. The main purpose is to provide some caching to avoid performance issues.</summary>
public partial class ParameterInfo
{
  public static HashSet<string> SpecialCommands = new() {
    "bind",
    "alias"
  };
  public static HashSet<string> CompositeCommands = new()
  {
  };
  public static HashSet<string> SpecialCommands1 = new() {
    "server"
  };
  public static HashSet<string> SpecialCommands2 = new() {
    "bind",
    "alias"
  };
  public static void AddSpecialCommand1(string command)
  {
    SpecialCommands.Add(command);
    SpecialCommands1.Add(command);
  }
  public static void AddSpecialCommand2(string command)
  {
    SpecialCommands.Add(command);
    SpecialCommands2.Add(command);
  }
  public static void AddCompositeCommand(string command)
  {
    CompositeCommands.Add(command);
  }
  private static List<string> globalKeys = new() {
    "defeated_bonemass",
    "defeated_dragon",
    "defeated_eikthyr",
    "defeated_gdking",
    "defeated_goblinking",
    "KilledBat",
    "KilledTroll",
    "killed_surtling",
    "nomap",
    "noportals"
  };
  public static List<string> GlobalKeys
  {
    get
    {
      return globalKeys;
    }
  }
  private static List<string> ids = new();
  public static List<string> Ids
  {
    get
    {
      if (ObjectIds.Count + LocationIds.Count != objectIds.Count)
      {
        ids = new();
        ids.AddRange(ObjectIds);
        ids.AddRange(LocationIds);
        ids.Sort();
      }
      return ids;
    }
  }
  private static List<string> environments = new();
  public static List<string> Environments
  {
    get
    {
      if (EnvMan.instance && EnvMan.instance.m_environments.Count != environments.Count)
      {
        environments = EnvMan.instance.m_environments.Select(env => env.m_name.Replace(" ", "_")).ToList();
        environments.Sort();
      }
      return environments;
    }
  }
  private static List<string> colors = new() {
    "#rrggbbaa",
    "aqua",
    "black",
    "blue",
    "brown",
    "cyan",
    "darkblue",
    "fuchsia",
    "green",
    "grey",
    "lightblue",
    "lime",
    "magenta",
    "maroon",
    "navy",
    "olive",
    "orange",
    "purple",
    "red",
    "silver",
    "teal",
    "white",
    "yellow"
  };
  public static List<string> Colors
  {
    get
    {
      return colors;
    }
  }
  private static List<string> objectIds = new();
  public static List<string> ObjectIds
  {
    get
    {
      if (ZNetScene.instance && ZNetScene.instance.m_namedPrefabs.Count != objectIds.Count)
        objectIds = ZNetScene.instance.GetPrefabNames();
      return objectIds;
    }
  }
  private static List<string> locationIds = new();
  public static List<string> LocationIds
  {
    get
    {
      if (ZoneSystem.instance && ZoneSystem.instance.m_locations.Count != locationIds.Count)
        locationIds = ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList();
      return locationIds;
    }
  }
  private static List<string> events = new();
  public static List<string> Events
  {
    get
    {
      if (RandEventSystem.instance && RandEventSystem.instance.m_events.Count != events.Count)
        events = RandEventSystem.instance.m_events.Select(ev => ev.m_name).ToList();
      return events;
    }
  }
  private static List<string> statusEffects = new();
  public static List<string> StatusEffects
  {
    get
    {
      if (ObjectDB.instance && ObjectDB.instance.m_StatusEffects.Count != statusEffects.Count)
        statusEffects = ObjectDB.instance.m_StatusEffects.Select(se => se.name).ToList();
      return statusEffects;
    }
  }
  public static List<string> EffectAreas
  {
    get
    {
      return Enum.GetNames(typeof(EffectArea.Type)).ToList();
    }
  }
  private static List<string> itemIds = new();
  public static List<string> ItemIds
  {
    get
    {
      if (ObjectDB.instance && ObjectDB.instance.m_items.Count != itemIds.Count)
        itemIds = ObjectDB.instance.m_items.Select(item => item.name).ToList();
      return itemIds;
    }
  }
  private static List<string> playerNames = new();
  public static List<string> PlayerNames
  {
    get
    {
      if (ZNet.instance && ZNet.instance.m_players.Count != playerNames.Count)
        playerNames = ZNet.instance.m_players.Select(player => player.m_name).ToList();
      return playerNames;
    }
  }
  private static List<string> hairs = new();
  public static List<string> Hairs
  {
    get
    {
      // Missing proper caching.
      if (ObjectDB.instance)
        hairs = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Hair").Select(item => item.name).ToList();
      return hairs;
    }
  }
  private static List<string> beards = new();
  public static List<string> Beards
  {
    get
    {
      // Missing proper caching.
      if (ObjectDB.instance)
        beards = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Beard").Select(item => item.name).ToList();
      return beards;
    }
  }
  private static List<string> keyCodes = new();
  public static List<string> KeyCodes
  {
    get
    {
      if (keyCodes.Count == 0)
      {
        var values = Enum.GetNames(typeof(KeyCode));
        keyCodes = values.Select(value => value.ToLower()).ToList();
        keyCodes.Add("wheel");
      }
      return keyCodes;
    }
  }
  private static List<string> keyCodesWithNegative = new();
  public static List<string> KeyCodesWithNegative
  {
    get
    {
      if (keyCodesWithNegative.Count == 0)
      {
        var values = Enum.GetNames(typeof(KeyCode));
        keyCodesWithNegative = values.Select(value => value.ToLower()).ToList();
        keyCodesWithNegative.AddRange(keyCodesWithNegative.Select(value => "-" + value).ToList());
      }
      return keyCodesWithNegative;
    }
  }
  public static List<string> CommandNames
  {
    get
    {
      return Terminal.commands.Keys.ToList();
    }
  }

  public static List<string> Origin = new() { "player", "object", "world" };
  public static List<string> Create(string value) => new() { $"?{value}" };
  public static List<string> Error(string value) => new() { $"?<color=red>Error:</color> {value}" };
  public static List<string> Create(string name, string value, string description) => Create($"{name}=<color=yellow>{value}</color> | {description}");
  public static List<string> CreateWithMinMax(string name, string value, string description) => Create($"{name}=<color=yellow>{value}</color> or {name}=<color=yellow>min-max</color> | {description}");
  public static List<string> Create(string values, string description) => Create($"{values} | {description}");
  public static List<string> None = Error("Too many parameters!");
  public static List<string> Missing = Create("No autocomplete available.");
  public static List<string> InvalidNamed(string name) => Error($"Invalid named parameter {name}!");
  public static List<string> Flag(string name) => Error($"{name} is a flag so it doesn't have any arguments!");
  public static List<string> Flag(string name, string description) => Error($"{name} is a flag so it doesn't have any arguments! | {description}");
  public static List<string> XZY(string name, string description, int index)
  {
    if (index == 0) return Create($"{name}=<color=yellow>X</color>,Z,Y | {description}.");
    if (index == 1) return Create($"{name}=X,<color=yellow>Z</color>,Y | {description}.");
    if (index == 2) return Create($"{name}=X,Z,<color=yellow>Y</color> | {description}.");
    return None;
  }
  public static List<string> XZY(string description, int index)
  {
    if (index == 0) return Create($"<color=yellow>X</color>,Z,Y | {description}.");
    if (index == 1) return Create($"X,<color=yellow>Z</color>,Y | {description}.");
    if (index == 2) return Create($"X,Z,<color=yellow>Y</color> | {description}.");
    return None;
  }
  public static List<string> XYZ(string name, string description, int index)
  {
    if (index == 0) return Create($"{name}=<color=yellow>X</color>,Z,Y | {description}.");
    if (index == 1) return Create($"{name}=X,<color=yellow>Y</color>,Z | {description}.");
    if (index == 2) return Create($"{name}=X,Y,<color=yellow>Z</color> | {description}.");
    return None;
  }
  public static List<string> XYZ(string description, int index)
  {
    if (index == 0) return Create($"<color=yellow>X</color>,Z,Y | {description}.");
    if (index == 1) return Create($"X,<color=yellow>Y</color>,Z | {description}.");
    if (index == 2) return Create($"X,Y,<color=yellow>Z</color> | {description}.");
    return None;
  }
  public static List<string> YXZ(string name, string description, int index)
  {
    if (index == 0) return Create($"{name}=<color=yellow>Y</color>,X,Z | {description}.");
    if (index == 1) return Create($"{name}=Y,<color=yellow>X</color>,Z | {description}.");
    if (index == 2) return Create($"{name}=Y,X,<color=yellow>Z</color> | {description}.");
    return None;
  }
  public static List<string> YXZ(string description, int index)
  {
    if (index == 0) return Create($"<color=yellow>Y</color>,X,Z | {description}.");
    if (index == 1) return Create($"Y,<color=yellow>X</color>,Z | {description}.");
    if (index == 2) return Create($"Y,X,<color=yellow>Z</color> | {description}.");
    return None;
  }
  public static List<string> XZ(string name, string description, int index)
  {
    if (index == 0) return Create($"{name}=<color=yellow>X</color>,Z | {description}.");
    if (index == 1) return Create($"{name}=X,<color=yellow>Z</color> | {description}.");
    return None;
  }
  public static List<string> XZ(string description, int index)
  {
    if (index == 0) return Create($"<color=yellow>X</color>,Z | {description}.");
    if (index == 1) return Create($"X,<color=yellow>Z</color> | {description}.");
    return None;
  }
  public static List<string> FRU(string name, string description, int index)
  {
    if (index == 0) return Create($"{name}=<color=yellow>forward</color>,right,up | {description}.");
    if (index == 1) return Create($"{name}=forward,<color=yellow>right</color>,up | {description}.");
    if (index == 2) return Create($"{name}=forward,right,<color=yellow>up</color> | {description}.");
    return None;
  }
  public static List<string> FRU(string description, int index)
  {
    if (index == 0) return Create($"<color=yellow>forward</color>,right,up | {description}.");
    if (index == 1) return Create($"forward,<color=yellow>right</color>,up | {description}.");
    if (index == 2) return Create($"forward,right,<color=yellow>up</color> | {description}.");
    return None;
  }
  public static List<string> Scale(string name, string description, int index)
  {
    if (index == 0) return Create($"{name}=<color=yellow>number</color> or {XYZ(name, description, index)[0].Substring(0)}");
    return XYZ(name, description, index);
  }
  public static List<string> Scale(string description, int index)
  {
    if (index == 0) return Create($"<color=yellow>number</color> or {XYZ(description, index)[0].Substring(0)}");
    return XYZ(description, index);
  }
}
