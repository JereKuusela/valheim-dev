using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ServerDevcommands;
///<summary>Helper class for parameter options/info. The main purpose is to provide some caching to avoid performance issues.</summary>
public partial class ParameterInfo
{
  private static List<string> components = [];
  public static List<string> Components
  {
    get
    {
      if (components.Count == 0)
        components = ComponentInfo.Types.Select(t => t.Name).ToList();
      return components;
    }
  }
  public static List<string> Command(int index)
  {
    var text = Console.instance.m_input.text;
    var parameters = text.Split(';').Last().Split(' ');
    parameters = parameters.Skip(parameters.Length - index - 1).ToArray();
    // Plain only done automatically for the first command, so commands involving subcommands have to do it again.
    parameters = Aliasing.Plain(string.Join(" ", parameters)).Split(' ');
    return AutoComplete.GetOptions(parameters);
  }
  private static readonly List<string> globalKeys = Enum.GetNames(typeof(GlobalKeys)).Select(s => s.ToLowerInvariant()).ToList();
  public static List<string> GlobalKeys
  {
    get
    {
      return globalKeys;
    }
  }
  private static List<string> ids = [];
  public static List<string> Ids
  {
    get
    {
      if (ObjectIds.Count + LocationIds.Count != objectIds.Count)
      {
        ids = [.. ObjectIds, .. LocationIds];
        ids.Sort();
      }
      return ids;
    }
  }
  private static List<string> environments = [];
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
  private static readonly List<string> colors = [
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
  ];
  public static List<string> Colors
  {
    get
    {
      return colors;
    }
  }
  private static List<string> objectIds = [];
  public static List<string> ObjectIds
  {
    get
    {
      if (ZNetScene.instance && ZNetScene.instance.m_namedPrefabs.Count != objectIds.Count)
        objectIds = ZNetScene.instance.GetPrefabNames();
      return objectIds;
    }
  }
  public static List<string> LocationIds
  {
    get
    {
      // No caching to make Expand World easier to use.
      return ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList();
    }
  }
  public static List<string> Events
  {
    get
    {
      // No caching to make Expand World easier to use.
      return RandEventSystem.instance.m_events.Select(ev => ev.m_name).ToList();
    }
  }
  private static List<string> statusEffects = [];
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
  private static List<string> itemIds = [];
  public static List<string> ItemIds
  {
    get
    {
      if (ObjectDB.instance && ObjectDB.instance.m_items.Count != itemIds.Count)
        itemIds = ObjectDB.instance.m_items.Select(item => item.name).ToList();
      return itemIds;
    }
  }
  public static List<string> PlayerNames
  {
    get => ZNet.instance?.m_players.Select(player => player.m_name).ToList() ?? [];
  }
  public static List<string> PublicPlayerNames
  {
    get => ZNet.instance?.m_players.Where(player => player.m_publicPosition).Select(player => player.m_name).ToList() ?? [];
  }
  private static List<string> hairs = [];
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
  private static List<string> beards = [];
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
  private static List<string> keyCodes = [];
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
  private static List<string> keyCodesWithNegative = [];
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

  public static List<string> Origin = ["player", "object", "world"];
  public static List<string> Create(string value) => [$"?{value}"];
  public static List<string> Error(string value) => [$"?<color=red>Error:</color> {value}"];
  public static List<string> Create(string name, string value, string description) => Create($"{name}=<color=yellow>{value}</color> | {description}");
  public static List<string> CreateWithMinMax(string name, string value, string description) => Create($"{name}=<color=yellow>{value}</color> or {name}=<color=yellow>min-max</color> | {description}");
  public static List<string> Create(string values, string description) => Create($"{values} | {description}");
  public static List<string> None = Error("Too many parameters!");
  public static List<string> Missing = Create("No autocomplete available.");
  public static List<string> InvalidNamed(string name) => Error($"Invalid named parameter {name}!");
  public static List<string> Flag(string name) => Error($"{name} is a flag so it doesn't have any arguments!");
  public static List<string> Flag(string name, int subIndex) => subIndex == 0 ? [name] : Flag(name);
  public static List<string> Flag(string name, string description) => Error($"{name} is a flag so it doesn't have any arguments! | {description}");
  public static List<string> Flag(string name, string description, int subIndex) => subIndex == 0 ? [name] : Flag(name, description);
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
