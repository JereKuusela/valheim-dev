using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerDevcommands {
  ///<summary>Helper class for parameter options/info. The main purpose is to provide some caching to avoid performance issues.</summary>
  public static class ParameterInfo {
    private static List<string> globalKeys = new List<string>{
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
    public static List<string> GlobalKeys {
      get {
        return globalKeys;
      }
    }
    private static List<string> ids = new List<string>();
    public static List<string> Ids {
      get {
        if (ObjectIds.Count + LocationIds.Count != objectIds.Count) {
          ids = new List<string>();
          ids.AddRange(ObjectIds);
          ids.AddRange(LocationIds);
          ids.Sort();
        }
        return ids;
      }
    }
    private static List<string> objectIds = new List<string>();
    public static List<string> ObjectIds {
      get {
        if (ZNetScene.instance && ZNetScene.instance.m_namedPrefabs.Count != objectIds.Count)
          objectIds = ZNetScene.instance.GetPrefabNames();
        return objectIds;
      }
    }
    private static List<string> locationIds = new List<string>();
    public static List<string> LocationIds {
      get {
        if (ZoneSystem.instance && ZoneSystem.instance.m_locations.Count != locationIds.Count)
          locationIds = ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList();
        return locationIds;
      }
    }
    private static List<string> itemIds = new List<string>();
    public static List<string> ItemIds {
      get {
        if (ObjectDB.instance && ObjectDB.instance.m_items.Count != itemIds.Count)
          itemIds = ObjectDB.instance.m_items.Select(item => item.name).ToList();
        return itemIds;
      }
    }
    private static List<string> playerNames = new List<string>();
    public static List<string> PlayerNames {
      get {
        if (ZNet.instance && ZNet.instance.m_players.Count != playerNames.Count)
          playerNames = ZNet.instance.m_players.Select(player => player.m_name).ToList();
        return playerNames;
      }
    }
    private static List<string> hairs = new List<string>();
    public static List<string> Hairs {
      get {
        // Missing proper caching.
        if (ObjectDB.instance)
          hairs = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Hair").Select(item => item.name).ToList();
        return hairs;
      }
    }
    private static List<string> beards = new List<string>();
    public static List<string> Beards {
      get {
        // Missing proper caching.
        if (ObjectDB.instance)
          beards = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Beard").Select(item => item.name).ToList();
        return beards;
      }
    }
    private static List<string> keyCodes = new List<string>();
    public static List<string> KeyCodes {
      get {
        if (keyCodes.Count == 0) {
          var values = Enum.GetNames(typeof(KeyCode));
          keyCodes = values.Select(value => value.ToLower()).ToList();
        }
        return keyCodes;
      }
    }
    private static List<string> keyCodesWithNegative = new List<string>();
    public static List<string> KeyCodesWithNegative {
      get {
        if (keyCodesWithNegative.Count == 0) {
          var values = Enum.GetNames(typeof(KeyCode));
          keyCodesWithNegative = values.Select(value => value.ToLower()).ToList();
          keyCodesWithNegative.AddRange(keyCodesWithNegative.Select(value => "-" + value).ToList());
        }
        return keyCodesWithNegative;
      }
    }
    public static List<string> CommandNames {
      get {
        return Terminal.commands.Keys.ToList();
      }
    }

    public static List<string> Origin = new List<string>() { "player", "object", "world" };
    public static List<string> Create(string value) => new List<string>() { $"?{value}" };
    public static List<string> Error(string value) => new List<string>() { $"?<color=red>Error:</color> {value}" };
    public static List<string> Create(string name, string value, string description) => Create($"{name}=<color=yellow>{value}</color> | {description}");
    public static List<string> CreateWithMinMax(string name, string value, string description) => Create($"{name}=<color=yellow>{value}</color> or {name}=<color=yellow>min-max</color> | {description}");
    public static List<string> Create(string values, string description) => Create($"{values} | {description}");
    public static List<string> None = Error("Too many parameters!");
    public static List<string> InvalidNamed(string name) => Error($"Invalid named parameter {name}!");
    public static List<string> Flag(string name) => Error($"{name} is a flag so it doesn't have any arguments!");
    public static List<string> XZY(string name, string description, int index) {
      if (index == 0) return ParameterInfo.Create($"{name}=<color=yellow>X</color>,Z,Y | {description}.");
      if (index == 1) return ParameterInfo.Create($"{name}=X,<color=yellow>Z</color>,Y | {description}.");
      if (index == 2) return ParameterInfo.Create($"{name}=X,Z,<color=yellow>Y</color> | {description}.");
      return ParameterInfo.None;
    }
    public static List<string> Scale(string name, string description, int index) {
      if (index == 0) return ParameterInfo.Create($"{name}=<color=yellow>number</color> or {XZY(name, description, index)[0]}");
      return XZY(name, description, index);
    }
    public static List<string> YXZ(string name, string description, int index) {
      if (index == 0) return ParameterInfo.Create($"{name}=<color=yellow>Y</color>,X,Z | {description}.");
      if (index == 1) return ParameterInfo.Create($"{name}=Y,<color=yellow>X</color>,Z | {description}.");
      if (index == 2) return ParameterInfo.Create($"{name}=Y,X,<color=yellow>Z</color> | {description}.");
      return ParameterInfo.None;
    }
  }
}