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
  private static readonly List<string> enumGlobalKeys = [.. Enum.GetNames(typeof(GlobalKeys)).Select(s => s.ToLowerInvariant())];
  private static readonly TimedCache globalKeys = new(() =>
  {
    if (!ZNetScene.instance) return null;
    var defeatKeys = ZNetScene.instance.m_namedPrefabs.Values
      .Select(prefab => prefab.GetComponent<Character>()?.m_defeatSetGlobalKey)
      .Where(key => !string.IsNullOrEmpty(key))
      .Select(key => key!.ToLowerInvariant());
    var keys = enumGlobalKeys.Union(defeatKeys).ToList();
    keys.Sort();
    return keys;
  }, enumGlobalKeys);
  public static List<string> GlobalKeys => globalKeys.Get();
  private static IEnumerable<KeySlider> WorldModifierSliders => ServerOptionsGUI.m_modifiers?.OfType<KeySlider>() ?? [];
  public static List<string> WorldModifiers => [.. WorldModifierSliders.Select(slider => slider.m_modifier.ToString())];
  public static List<string> WorldModifierOptions(string modifier) => [.. WorldModifierSliders
    .Where(slider => slider.m_modifier.ToString().Equals(modifier, StringComparison.OrdinalIgnoreCase))
    .SelectMany(slider => slider.m_settings.Select(setting => setting.m_modifierValue.ToString()))];
  public static List<string> ActiveGlobalKeys
  {
    get
    {
      if (!ZoneSystem.instance) return [];
      return ZoneSystem.instance.GetGlobalKeys();
    }
  }
  private static readonly TimedCache ids = new(() =>
  {
    var all = ObjectIds.Concat(LocationIds).ToList();
    all.Sort();
    return all;
  });
  public static List<string> Ids => ids.Get();
  private static readonly TimedCache environments = new(() =>
  {
    if (!EnvMan.instance) return null;
    var names = EnvMan.instance.m_environments.Select(env => env.m_name.Replace(" ", "_")).ToList();
    names.Sort();
    return names;
  });
  public static List<string> Environments => environments.Get();
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
  private static readonly TimedCache objectIds = new(() => ZNetScene.instance ? ZNetScene.instance.GetPrefabNames() : null);
  public static List<string> ObjectIds => objectIds.Get();
  private static readonly TimedCache roomIds = new(() => DungeonDB.instance ? DungeonDB.instance.m_rooms.Select(room => room.m_prefab.Name).ToList() : null);
  public static List<string> RoomIds => roomIds.Get();
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
  private static readonly TimedCache statusEffects = new(() => ObjectDB.instance ? ObjectDB.instance.m_StatusEffects.Select(se => se.name).ToList() : null);
  public static List<string> StatusEffects => statusEffects.Get();
  public static List<string> EffectAreas
  {
    get
    {
      return [.. Enum.GetNames(typeof(EffectArea.Type))];
    }
  }
  private static readonly TimedCache itemIds = new(() => ObjectDB.instance ? ObjectDB.instance.m_items.Select(item => item.name).ToList() : null);
  public static List<string> ItemIds => itemIds.Get();
  public static List<string> PlayerNames
  {
    get => ZNet.instance?.m_players.Select(player => player.m_name.Contains(" ") ? $"\"{player.m_name}\"" : player.m_name).ToList() ?? ["No players found!"];
  }
  public static List<string> PublicPlayerNames
  {
    get => ZNet.instance?.m_players.Where(player => player.m_publicPosition).Select(player => player.m_name.Contains(" ") ? $"\"{player.m_name}\"" : player.m_name).ToList() ?? ["No public players found!"];
  }
  private static readonly TimedCache hairs = new(() => ObjectDB.instance ? ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Hair").Select(item => item.name).ToList() : null);
  public static List<string> Hairs => hairs.Get();
  private static readonly TimedCache beards = new(() => ObjectDB.instance ? ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Beard").Select(item => item.name).ToList() : null);
  public static List<string> Beards => beards.Get();
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
