using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DEV {
  public partial class Commands {

    private static Dictionary<int, string> KnownDataIds = new string[]{
      "huntplayer",
      "spawnpoint",
      "patrolPoint",
      "patrol",
      "spawntime",
      "lastWorldTime",
      "haveTarget",
      "alert",
      "owner",
      "ownerName",
      "lastTime",
      "level",
      "product",
      "max_health",
      "noise",
      "level",
      "tiltrot",
      "BodyVelocity",
      "addedDefaultItenms",
      "InUse",
      "items",
      "StartTime",
      "fuel",
      "slot0",
      "slot1",
      "slot2",
      "slot3",
      "slot4",
      "slot5",
      "slot6",
      "slot7",
      "slot8",
      "slot9",
      "slot10",
      "slotstatus0",
      "slotstatus1",
      "slotstatus2",
      "slotstatus3",
      "slotstatus4",
      "slotstatus5",
      "slotstatus6",
      "slotstatus7",
      "slotstatus8",
      "slotstatus9",
      "slotstatus10",
      "timeOfDeath",
      "alive_time",
      "spawn_id",
      "health",
      "state",
      "rooms",
      "Content",
      "lastTime",
      "RodOwner",
      "CatchId",
      "IsBlocking",
      "SpawnTime",
      "durability",
      "stack",
      "quality",
      "variant",
      "crafterID",
      "crafterName",
      "item",
      "submerged",
      "LiquidData",
      "location",
      "seed",
      "data",
      "Health",
      "Health0",
      "health",
      "DespawnInDay",
      "EventCreature",
      "sleeping",
      "picked",
      "picked_time",
      "itemPrefab",
      "itemStack",
      "creator",
      "platTime",
      "Stealth",
      "stamina",
      "DebugFly",
      "playerID",
      "playerName",
      "wakeup",
      "dead",
      "dodgeinv",
      "baseValue",
      "pvp",
      "permitted",
      "pu_id0",
      "pu_id1",
      "pu_id2",
      "pu_id3",
      "pu_id4",
      "pu_id5",
      "pu_id6",
      "pu_id7",
      "pu_id8",
      "pu_id9",
      "pu_id10",
      "pu_name0",
      "pu_name1",
      "pu_name2",
      "pu_name3",
      "pu_name4",
      "pu_name5",
      "pu_name6",
      "pu_name7",
      "pu_name8",
      "pu_name9",
      "pu_name10",
      "lovePoints",
      "pregant",
      "Hue",
      "Saturation",
      "Value",
      "InitVel",
      "landed",
      "user",
      "seAttrib",
      "rudder",
      "forward",
      "spawntime",
      "done",
      "text",
      "accTime",
      "bakeTimer",
      "StartTime",
      "SpawnOre",
      "SpawnAmount",
      "queued",
      "item0",
      "item1",
      "item2",
      "item3",
      "item4",
      "item5",
      "item6",
      "item7",
      "item8",
      "item9",
      "item10",
      "TamedName",
      "TameTimeLeft",
      "TameLastFeeding",
      "HaveSaddle",
      "target",
      "tag",
      "TCData",
      "SpawnPoint",
      "ModelIndex",
      "SkinColor",
      "HairColor",
      "RightItem",
      "RightItemVariant",
      "RightBackItem",
      "RightBackItemVariant",
      "LeftItem",
      "LeftItemVariant",
      "LeftBackItem",
      "LeftBackItemVariant",
      "ChestItem",
      "LegItem",
      "HelmetItem",
      "ShoulderItem",
      "ShoulderItemVariant",
      "BeardItem",
      "HairItem",
      "UtilityItem",
      "support",
      "scale",
      "vel",
      "body_vel",
      "body_avel",
      "refPos",
      "attachJoint",
      "parentID"
    }.ToHashSet().ToDictionary(value => value.GetStableHashCode());
    private static bool IsIncluded(string id, string name) {
      if (id.StartsWith("*") && id.EndsWith("*")) {
        return name.Contains(id.Substring(1, id.Length - 3));
      }
      if (id.StartsWith("*")) return name.EndsWith(id.Substring(1));
      if (id.EndsWith("*")) return name.StartsWith(id.Substring(0, id.Length - 2));
      return id == name;
    }
    private static IEnumerable<string> GetPrefabs(string id) {
      IEnumerable<GameObject> values = ZNetScene.instance.m_namedPrefabs.Values;
      if (id.Contains("*"))
        values = values.Where(prefab => IsIncluded(id, prefab.name));
      else
        values = values.Where(prefab => prefab.name == id);
      return values.Select(prefab => prefab.name);
    }
    private static IEnumerable<ZDO> GetZDOs(string id, float distance) {
      var code = id.GetStableHashCode();
      var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => code == zdo.GetPrefab());
      var position = Player.m_localPlayer ? Player.m_localPlayer.transform.position : Vector3.zero;
      if (distance > 0)
        return zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) <= distance);
      return zdos;
    }
    protected static void Log(IEnumerable<string> values) {
      ZLog.Log("\n" + string.Join("\n", values));
    }
    public static void AddListObjects() {
      new Terminal.ConsoleCommand("list_objects", "[name] [max distance=-1] - Lists nearby objects.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          return;
        }
        var name = args[1];
        var distance = TryParameterFloat(args.Args, 2, -1);
        var prefabs = GetPrefabs(name);
        var texts = prefabs.Select(id => {
          var zdos = GetZDOs(id, distance);
          return zdos.Select(zdo => id + " (" + zdo.m_uid.m_userID + "/" + zdo.m_uid.m_id + "): " + zdo.GetPosition().ToString("F0"));
        }).Aggregate((acc, list) => acc.Concat(list));
        Log(texts);
      }, true, false, true, false, false, () => ZNetScene.instance.GetPrefabNames());
    }

    private static string GetKey(int key) => KnownDataIds.ContainsKey(key) ? KnownDataIds[key] : key.ToString();
    public static void AddListData() {
      new Terminal.ConsoleCommand("list_data", "[id] - Lists object data.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          return;
        }
        var split = args.Args[1].Split('/');
        var userID = TryParameterLong(split, 0, 0);
        var id = TryParameterUInt(split, 1, 0);
        var zDOID = new ZDOID() {
          m_userID = userID,
          m_id = id
        };
        var zdo = ZDOMan.instance.GetZDO(zDOID);
        if (zdo == null) return;
        var name = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).name;
        var texts = new List<string>() { name + ": " + zdo.GetPosition().ToString("F0") };
        texts.AddRange(zdo.m_longs != null ? zdo.m_longs.Select(kvp => GetKey(kvp.Key) + " (long): " + kvp.Value) : new string[0]);
        texts.AddRange(zdo.m_ints != null ? zdo.m_ints.Select(kvp => GetKey(kvp.Key) + " (int): " + kvp.Value) : new string[0]);
        texts.AddRange(zdo.m_floats != null ? zdo.m_floats.Select(kvp => GetKey(kvp.Key) + " (float): " + kvp.Value) : new string[0]);
        texts.AddRange(zdo.m_strings != null ? zdo.m_strings.Select(kvp => GetKey(kvp.Key) + " (string): " + kvp.Value) : new string[0]);
        Log(texts);
      }, true, false, true, false, false);
      new Terminal.ConsoleCommand("data_item", "[id] - WOW.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 4) {
          return;
        }
        var split = args[1].Split('/');
        var userID = TryParameterLong(split, 0, 0);
        var id = TryParameterUInt(split, 1, 0);
        var zDOID = new ZDOID() {
          m_userID = userID,
          m_id = id
        };
        var zdo = ZDOMan.instance.GetZDO(zDOID);
        if (zdo == null) return;
        var data = args[2].GetStableHashCode();
        var item = args[3].GetStableHashCode();
        zdo.Set(data, item);
      }, true, false, true, false, false);
    }
  }
}
