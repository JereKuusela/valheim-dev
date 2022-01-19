using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace DEV {
  public class ManipulateCommand : BaseCommands {
    private static bool IsIncluded(string id, string name) {
      if (id.StartsWith("*") && id.EndsWith("*")) {
        return name.Contains(id.Substring(1, id.Length - 3));
      }
      if (id.StartsWith("*")) return name.EndsWith(id.Substring(1));
      if (id.EndsWith("*")) return name.StartsWith(id.Substring(0, id.Length - 2));
      return id == name;
    }
    public static IEnumerable<int> GetPrefabs(string id) {
      IEnumerable<GameObject> values = ZNetScene.instance.m_namedPrefabs.Values;
      if (id == "*") { } // Pass
      else if (id.Contains("*"))
        values = values.Where(prefab => IsIncluded(id, prefab.name));
      else
        values = values.Where(prefab => prefab.name == id);
      return values.Where(prefab => prefab.name != "Player").Select(prefab => prefab.name.GetStableHashCode());
    }
    public static IEnumerable<ZDO> GetZDOs(string id, float distance) {
      var codes = GetPrefabs(id);
      var sector = Player.m_localPlayer ? Player.m_localPlayer.m_nview.GetZDO().GetSector() : new Vector2i(0, 0);
      var index = ZDOMan.instance.SectorToIndex(sector);
      IEnumerable<ZDO> zdos = ZDOMan.instance.m_objectsBySector[index];
      zdos = zdos.Where(zdo => codes.Contains(zdo.GetPrefab()));
      var position = Player.m_localPlayer ? Player.m_localPlayer.transform.position : Vector3.zero;
      if (distance > 0)
        return zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) <= distance);
      return zdos;
    }
    public ManipulateCommand() {
      new Terminal.ConsoleCommand("move", "[x,z,y] [player/object/world] - Moves hovered object relative to selected origin (relative to player if not given).", delegate (Terminal.ConsoleEventArgs args) {
        var view = GetHovered(args);
        if (!view) return;
        view.ClaimOwnership();
        var zdo = view.GetZDO();
        var position = view.transform.position;
        var relative = ParsePositionXZY(args.Args[1]);
        var origin = args.Length > 2 ? args[2].ToLower() : "player";
        var rotation = Player.m_localPlayer.transform.rotation;
        if (origin == "world")
          rotation = Quaternion.identity;
        if (origin == "object")
          rotation = view.transform.rotation;
        position += rotation * Vector3.forward * relative.x;
        position += rotation * Vector3.right * relative.z;
        position += rotation * Vector3.up * relative.y;
        zdo.SetPosition(position);
        view.transform.position = position;
      }, true, true);
      new Terminal.ConsoleCommand("rotate", "[x,y,z/reset] [player/object/world] - Rotates hovered object around a given axis relative to selected origin (relative to player y axis if not given).", delegate (Terminal.ConsoleEventArgs args) {
        var view = GetHovered(args);
        if (!view) return;
        view.ClaimOwnership();
        var zdo = view.GetZDO();
        if (args[1].ToLower() == "reset") {
          zdo.SetRotation(Quaternion.identity);
          view.transform.rotation = Quaternion.identity;
          return;
        }
        var relative = ParsePositionYXZ(args.Args[1]);
        var origin = args.Length > 2 ? args[2].ToLower() : "player";
        var originRotation = Player.m_localPlayer.transform.rotation;
        if (origin == "world")
          originRotation = Quaternion.identity;
        if (origin == "object")
          originRotation = view.transform.rotation;
        var transform = view.transform;
        var position = transform.position;
        transform.RotateAround(position, originRotation * Vector3.up, relative.y);
        transform.RotateAround(position, originRotation * Vector3.forward, relative.x);
        transform.RotateAround(position, originRotation * Vector3.right, relative.z);
        zdo.SetRotation(view.transform.rotation);
      }, true, false, true, false, false);
      new Terminal.ConsoleCommand("scale", "[x,z,y] - Scales hovered object.", delegate (Terminal.ConsoleEventArgs args) {
        var view = GetHovered(args);
        if (!view) return;
        view.ClaimOwnership();
        var scale = ParseScale(args.Args[1]);
        if (view.m_syncInitialScale)
          view.SetLocalScale(scale);
      }, true, false, true, false, false);
      new Terminal.ConsoleCommand("target", "[operation=value] [id=*] [radius=0] - Modifies the targeted object.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        var pars = ParseArgs(args);
        if (!Operations.Contains(pars.Operation)) {
          AddMessage(args.Context, "Error: Invalid operation.");
          return;
        }
        IEnumerable<ZDO> zdos;
        if (pars.Radius > 0f) {
          zdos = GetZDOs(pars.Id, pars.Radius);
        } else {
          var view = GetHovered(args);
          if (!view) return;
          if (!GetPrefabs(pars.Id).Contains(view.GetZDO().GetPrefab())) {
            AddMessage(args.Context, "Skipped: ¤ has invalid id.");
            return;
          }
          zdos = new ZDO[] { view.GetZDO() };
        }
        var scene = ZNetScene.instance;
        foreach (var zdo in zdos) {
          var view = scene.FindInstance(zdo);
          if (view == null) {
            args.Context.AddString("Skipped: ¤ is not loaded.");
            continue;
          }
          view.ClaimOwnership();
          var character = view.GetComponent<Character>();
          var output = "Error: Invalid operation.";
          if (pars.Operation == "health")
            output = ChangeHealth(character, TryInt(pars.Value));
          if (pars.Operation == "stars")
            output = SetStars(character, TryInt(pars.Value));
          if (pars.Operation == "tame")
            output = MakeTame(character);
          if (pars.Operation == "wild")
            output = MakeWild(character);
          if (pars.Operation == "baby")
            output = SetBaby(view.GetComponent<Growup>());
          if (pars.Operation == "info")
            output = GetInfo(view);
          if (pars.Operation == "sleep")
            output = MakeSleep(view.GetComponent<MonsterAI>());
          if (pars.Operation == "visual")
            output = SetVisual(view.GetComponent<ItemStand>(), TryParameterString(pars.Value.Split('|'), 0), TryParameterInt(pars.Value.Split('|'), 1, 0));
          if (pars.Operation == "helmet")
            output = SetHelmet(character, TryParameterString(pars.Value.Split('|'), 0), TryParameterInt(pars.Value.Split('|'), 1, 0));
          if (pars.Operation == "left_hand")
            output = SetLeftHand(character, TryParameterString(pars.Value.Split('|'), 0), TryParameterInt(pars.Value.Split('|'), 1, 0));
          if (pars.Operation == "right_hand")
            output = SetRightHand(character, TryParameterString(pars.Value.Split('|'), 0), TryParameterInt(pars.Value.Split('|'), 1, 0));
          if (pars.Operation == "chest")
            output = SetChest(character, TryParameterString(pars.Value.Split('|'), 0), TryParameterInt(pars.Value.Split('|'), 1, 0));
          if (pars.Operation == "shoulders")
            output = SetShoulder(character, TryParameterString(pars.Value.Split('|'), 0), TryParameterInt(pars.Value.Split('|'), 1, 0));
          if (pars.Operation == "legs")
            output = SetLegs(character, TryParameterString(pars.Value.Split('|'), 0), TryParameterInt(pars.Value.Split('|'), 1, 0));
          if (pars.Operation == "utility")
            output = SetUtility(character, TryParameterString(pars.Value.Split('|'), 0), TryParameterInt(pars.Value.Split('|'), 1, 0));
          if (pars.Operation == "remove") {
            Actions.Remove(view.gameObject);
            output = "Entity ¤ destroyed.";
          }
          var message = output.Replace("¤", Utils.GetPrefabName(view.gameObject));
          if (pars.Radius == 0)
            AddMessage(args.Context, message);
          else
            args.Context.AddString(message);
        }
      }, true, true, optionsFetcher: () => Operations);
      CommandParameters.AddFetcher("target", (int index, string parameter) => {
        if (parameter == "baby" || parameter == "tame" || parameter == "wild" || parameter == "remove" || parameter == "sleep" || parameter == "info") return Parameters.None;
        if (parameter == "id") return Parameters.Ids;
        if (parameter == "left_hand" || parameter == "right_hand" || parameter == "helmet" || parameter == "chest" || parameter == "shoulders" || parameter == "legs" || parameter == "utility") return Parameters.ItemIds;
        if (parameter == "radius" || parameter == "health" || parameter == "stars") return Parameters.Number;
        return Operations;
      });
    }
    private static TargetParameters ParseArgs(Terminal.ConsoleEventArgs args) {
      var parameters = new TargetParameters();
      foreach (var arg in args.Args) {
        var split = arg.Split('=');
        if (Operations.Contains(split[0]))
          parameters.Operation = split[0];
        if (split.Length < 2) continue;
        if (split[0] == "radius")
          parameters.Radius = Math.Min(TryFloat(split[1], 100f), 100f);
        if (split[0] == "id")
          parameters.Id = split[1];
        if (Operations.Contains(split[0]))
          parameters.Value = split[1];
      }
      return parameters;
    }
    private static List<string> Operations = new List<string>(){
      "health",
      "stars",
      "baby",
      "wild",
      "tame",
      "info",
      "sleep",
      "visual",
      "remove",
      "ids",
      "radius",
      "helmet",
      "left_hand",
      "right_hand",
      "legs",
      "chest",
      "shoulder",
      "utility"
    };
    private static string ChangeHealth(Character obj, int amount) {
      if (obj == null) return "Skipped: ¤ is not a creature..";
      var previous = obj.GetMaxHealth();
      Actions.SetHealth(obj, amount);
      return $"¤ health changed from {previous.ToString("F0")} to {amount.ToString("F0")}.";
    }
    private static string SetStars(Character obj, int amount) {
      if (obj == null) return "Skipped: ¤ is not a creature.";
      var previous = obj.GetLevel() - 1;
      Actions.SetLevel(obj.gameObject, amount + 1);
      return $"¤ stars changed from {previous} to {amount}.";
    }
    private static string SetBaby(Growup obj) {
      if (obj == null) return "Skipped: ¤ is not an offspring.";
      Actions.SetBaby(obj);
      return "¤ growth disabled.";
    }
    private static string MakeTame(Character obj) {
      if (obj == null) return "Skipped: ¤ is not a creature.";
      Actions.SetTame(obj, true);
      return "¤ made tame.";
    }
    private static string MakeWild(Character obj) {
      if (obj == null) return "Skipped: ¤ is not a creature.";
      Actions.SetTame(obj, false);
      return "¤ made wild.";
    }
    private static string MakeSleep(MonsterAI obj) {
      if (obj == null) return "Skipped: ¤ is not a creature.";
      Actions.SetSleeping(obj, true);
      return "¤ made to sleep.";
    }
    private static string SetVisual(ItemStand obj, string item, int variant) {
      if (obj == null) return "Skipped: ¤ is not an item stand.";
      Actions.SetVisual(obj, item, variant);
      return $"Visual of ¤ set to {item} with variant {variant} .";
    }
    private static string SetHelmet(Character obj, string item, int variant) {
      if (obj == null) return "Skipped: ¤ is not a creature.";
      Actions.SetVisual(obj, VisSlot.Helmet, item, variant);
      return $"Helmet of ¤ set to {item} with variant {variant} .";
    }
    private static string SetLeftHand(Character obj, string item, int variant) {
      if (obj == null) return "Skipped: ¤ is not a creature.";
      Actions.SetVisual(obj, VisSlot.HandLeft, item, variant);
      return $"Left hand of ¤ set to {item} with variant {variant} .";
    }
    private static string SetRightHand(Character obj, string item, int variant) {
      if (obj == null) return "Skipped: ¤ is not a creature.";
      Actions.SetVisual(obj, VisSlot.HandRight, item, variant);
      return $"Right hand of ¤ set to {item} with variant {variant} .";
    }
    private static string SetChest(Character obj, string item, int variant) {
      if (obj == null) return "Skipped: ¤ is not a creature.";
      Actions.SetVisual(obj, VisSlot.Chest, item, variant);
      return $"Chest of ¤ set to {item} with variant {variant} .";
    }
    private static string SetShoulder(Character obj, string item, int variant) {
      if (obj == null) return "Skipped: ¤ is not a creature.";
      Actions.SetVisual(obj, VisSlot.Shoulder, item, variant);
      return $"Shoulder of ¤ set to {item} with variant {variant} .";
    }
    private static string SetLegs(Character obj, string item, int variant) {
      if (obj == null) return "Skipped: ¤ is not a creature.";
      Actions.SetVisual(obj, VisSlot.Legs, item, variant);
      return $"Legs of ¤ set to {item} with variant {variant} .";
    }
    private static string SetUtility(Character obj, string item, int variant) {
      if (obj == null) return "Skipped: ¤ is not a creature.";
      Actions.SetVisual(obj, VisSlot.Utility, item, variant);
      return $"Utility item of ¤ set to {item} with variant {variant} .";
    }
    private static string GetInfo(ZNetView obj) {
      var info = new List<string>();
      info.Add("Id: ¤");
      info.Add("Pos: " + obj.transform.position.ToString("F1"));
      var character = obj.GetComponent<Character>();
      if (character) {
        info.Add("Health: " + character.GetHealth().ToString("F0") + " / " + character.GetMaxHealth());
        info.Add("Stars: " + (character.GetLevel() - 1));
        info.Add("Tamed: " + (character.IsTamed() ? "Yes" : "No"));
        var growUp = obj.GetComponent<Growup>();
        if (growUp)
          info.Add("Baby: " + (growUp.m_baseAI.GetTimeSinceSpawned().TotalSeconds < 0 ? "Yes" : "No"));
      } else {
        var health = obj.GetZDO().GetFloat("health", -1f);
        if (health > -1f)
          info.Add("Health: " + health.ToString("F0"));
      }
      var equipment = obj.GetComponent<VisEquipment>();
      if (equipment) {
        if (equipment.m_rightItem != "")
          info.Add("Right hand: " + equipment.m_rightItem);
        if (equipment.m_leftItem != "")
          info.Add("Left hand: " + equipment.m_leftItem);
        if (equipment.m_helmetItem != "")
          info.Add("Helmet: " + equipment.m_helmetItem);
        if (equipment.m_shoulderItem != "")
          info.Add("Shoulders: " + equipment.m_shoulderItem);
        if (equipment.m_chestItem != "")
          info.Add("Chest: " + equipment.m_chestItem);
        if (equipment.m_legItem != "")
          info.Add("Legs: " + equipment.m_legItem);
        if (equipment.m_utilityItem != "")
          info.Add("Utility: " + equipment.m_utilityItem);
      }
      return string.Join(", ", info);
    }
  }

  public class TargetParameters {
    public float Radius = 0f;
    public string Id = "*";
    public string Operation = "";
    public string Value = "";
  }
}
