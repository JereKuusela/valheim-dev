using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
        components = [.. ComponentInfo.Types.Select(t => t.Name)];
      return components;
    }
  }
  private static readonly List<string> globalKeys = [.. Enum.GetNames(typeof(GlobalKeys)).Select(s => s.ToLowerInvariant())];
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
        environments = [.. EnvMan.instance.m_environments.Select(env => env.m_name.Replace(" ", "_"))];
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
  private static List<string> roomIds = [];
  public static List<string> RoomIds
  {
    get
    {
      if (DungeonDB.instance && DungeonDB.instance.m_rooms.Count != roomIds.Count)
        roomIds = [.. DungeonDB.instance.m_rooms.Select(room => room.m_prefab.Name)];
      return roomIds;
    }
  }
  public static List<string> LocationIds
  {
    get
    {
      if (ServerLocationIdsCache != null)
        return ServerLocationIdsCache;
      if (!ZoneSystem.instance) return [];
      if (LocationDataCache != ZoneSystem.instance.m_locations)
        LocationIdsCache = null;
      if (LocationIdsCache == null)
        LocationIdsCache = [.. ZoneSystem.instance.m_locations.Where(Helper.IsValid).Select(l => l.m_prefab.Name).Distinct()];
      LocationDataCache = ZoneSystem.instance.m_locations;
      return LocationIdsCache;
    }
  }
  // Needed to track changes by Expand World.
  private static List<ZoneSystem.ZoneLocation>? LocationDataCache = null;
  private static List<string>? LocationIdsCache = null;
  private static List<string>? ServerLocationIdsCache = null;
  public static void SetServerLocationIds(List<string>? data)
  {
    ServerLocationIdsCache = data;
  }
  public static List<string> VegetationIds
  {
    get
    {
      if (ServerVegetationIdsCache != null)
        return ServerVegetationIdsCache;

      if (!ZoneSystem.instance) return [];
      if (VegetationDataCache != ZoneSystem.instance.m_vegetation)
        VegetationIdsCache = null;
      if (VegetationIdsCache == null)
        VegetationIdsCache = [.. ZoneSystem.instance.m_vegetation.Where(veg => veg != null && veg.m_prefab != null).Select(l => l.m_name).Distinct()];
      VegetationDataCache = ZoneSystem.instance.m_vegetation;
      return VegetationIdsCache;
    }
  }
  // Needed to track changes by Expand World.
  private static List<ZoneSystem.ZoneVegetation>? VegetationDataCache = null;
  private static List<string>? VegetationIdsCache = null;
  private static List<string>? ServerVegetationIdsCache = null;
  public static void SetServerVegetationIds(List<string>? data)
  {
    ServerVegetationIdsCache = data;
  }

  public static List<string> Events
  {
    get
    {
      // No caching to make Expand World easier to use.
      return [.. RandEventSystem.instance.m_events.Select(ev => ev.m_name)];
    }
  }
  private static List<string> statusEffects = [];
  public static List<string> StatusEffects
  {
    get
    {
      if (ObjectDB.instance && ObjectDB.instance.m_StatusEffects.Count != statusEffects.Count)
        statusEffects = [.. ObjectDB.instance.m_StatusEffects.Select(se => se.name)];
      return statusEffects;
    }
  }
  public static List<string> EffectAreas
  {
    get
    {
      return [.. Enum.GetNames(typeof(EffectArea.Type))];
    }
  }
  private static List<string> itemIds = [];
  public static List<string> ItemIds
  {
    get
    {
      if (ObjectDB.instance && ObjectDB.instance.m_items.Count != itemIds.Count)
        itemIds = [.. ObjectDB.instance.m_items.Select(item => item.name)];
      return itemIds;
    }
  }
  public static List<string> PlayerNames
  {
    get => ZNet.instance?.m_players.Select(player => player.m_name.Contains(" ") ? $"\"{player.m_name}\"" : player.m_name).ToList() ?? ["No players found!"];
  }
  public static List<string> PublicPlayerNames
  {
    get => ZNet.instance?.m_players.Where(player => player.m_publicPosition).Select(player => player.m_name.Contains(" ") ? $"\"{player.m_name}\"" : player.m_name).ToList() ?? ["No public players found!"];
  }
  private static List<string> hairs = [];
  public static List<string> Hairs
  {
    get
    {
      // Missing proper caching.
      if (ObjectDB.instance)
        hairs = [.. ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Hair").Select(item => item.name)];
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
        beards = [.. ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Beard").Select(item => item.name)];
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
        keyCodes = [.. values.Select(value => value.ToLower())];
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
        keyCodesWithNegative = [.. values.Select(value => value.ToLower())];
        keyCodesWithNegative.AddRange(keyCodesWithNegative.Select(value => "-" + value).ToList());
      }
      return keyCodesWithNegative;
    }
  }
  public static List<string> CommandNames
  {
    get
    {
      return [.. Terminal.commands.Keys];
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
  public static List<string> XZYR(string description, int index)
  {
    if (index == 0) return Create($"<color=yellow>X</color>,Z,Y,R | {description}.");
    if (index == 1) return Create($"X,<color=yellow>Z</color>,Y,R | {description}.");
    if (index == 2) return Create($"X,Z,<color=yellow>Y</color>,R | {description}.");
    if (index == 4) return Create($"X,Z,Y,<color=yellow>R</color> | {description}.");
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
    if (index == 0) return Create($"{name}=<color=yellow>number</color> or {name}=<color=yellow>X</color>,Z,Y,free | {description}");
    if (index == 1) return Create($"{name}=X,<color=yellow>Y</color>,Z,free  | {description}.");
    if (index == 2) return Create($"{name}=X,Y,<color=yellow>Z</color>,free  | {description}.");
    if (index == 3) return Create($"{name}=X,Y,Z<color=yellow>free</color>  | If given, each axis is randomized separately.");
    return None;
  }
  public static List<string> Scale(string description, int index)
  {
    if (index == 0) return Create($"<color=yellow>number</color> or {XYZ(description, index)[0].Substring(0)}");
    return XYZ(description, index);
  }
}
