using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerDevcommands {
  ///<summary>Helper class for parameter options/info. The main purpose is to provide some caching to avoid performance issues.</summary>
  public static class ParameterInfo {
    private static List<string> ids = new List<string>();
    public static List<string> Ids {
      get {
        if (ZNetScene.instance && ZNetScene.instance.m_namedPrefabs.Count != ids.Count)
          ids = ZNetScene.instance.GetPrefabNames();
        return ids;
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
    private static string Format(string value) {
      if (!value.EndsWith(".") && !value.EndsWith("!"))
        value += ".";
      return "?" + value;
    }
    public static List<string> None = new List<string>() { Format("Too many parameters") };
    public static List<string> Origin = new List<string>() { "player", "object", "world" };
    public static List<string> Create(string name) => new List<string>() { Format($"{name}") };
    public static List<string> Create(string name, string type) => Create($"{name} should be {type}");
    public static List<string> InvalidNamed(string name) => Create($"Invalid named parameter {name}");
    public static List<string> Flag(string name) => Create($"{name} is a flag so it doesn't have any arguments");
    public static List<string> XZY(int index) {
      if (index == 0) return ParameterInfo.Create("X", "a number");
      if (index == 1) return ParameterInfo.Create("Z", "a number");
      if (index == 2) return ParameterInfo.Create("Y", "a number");
      return ParameterInfo.None;
    }
    public static List<string> Scale(int index) {
      if (index == 0) return ParameterInfo.Create("X", "a number (also sets Z and Y if not given)");
      return XZY(index);
    }
    public static List<string> YXZ(int index) {
      if (index == 0) return ParameterInfo.Create("Y", "number");
      if (index == 1) return ParameterInfo.Create("X", "number");
      if (index == 2) return ParameterInfo.Create("Z", "number");
      return ParameterInfo.None;
    }
  }
}