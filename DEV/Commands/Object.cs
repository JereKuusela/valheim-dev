using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace DEV {
  public class ObjectCommand : BaseCommands {
    private static bool IsIncluded(string id, string name) {
      if (id.StartsWith("*") && id.EndsWith("*")) {
        return name.Contains(id.Substring(1, id.Length - 3));
      }
      if (id.StartsWith("*")) return name.EndsWith(id.Substring(1));
      if (id.EndsWith("*")) return name.StartsWith(id.Substring(0, id.Length - 2));
      return id == name;
    }
    public static IEnumerable<int> GetPrefabs(string id) {
      id = id.ToLower();
      IEnumerable<GameObject> values = ZNetScene.instance.m_namedPrefabs.Values;
      if (id == "*") { } // Pass
      else if (id.Contains("*"))
        values = values.Where(prefab => IsIncluded(id, prefab.name.ToLower()));
      else
        values = values.Where(prefab => prefab.name.ToLower() == id);
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
    private static void Execute(Terminal context, IEnumerable<string> operations, IEnumerable<ZDO> zdos) {
      foreach (var operation in operations) {
        var name = operation.Split('=')[0];
        if (!Operations.Contains(name)) {
          AddMessage(context, $"Error: Invalid operation {name}.");
          return;
        }
      }
      var scene = ZNetScene.instance;
      zdos = zdos.Where(zdo => {
        var view = scene.FindInstance(zdo);
        if (view == null)
          context.AddString($"Skipped: {view.name} is not loaded.");
        return view != null;
      });
      foreach (var operation in operations) {
        var split = operation.Split('=');
        var name = split[0];
        var values = (split.Length > 1 ? split[1] : "").Split('|');
        foreach (var zdo in zdos) {
          var view = scene.FindInstance(zdo);
          view.ClaimOwnership();
          var character = view.GetComponent<Character>();
          var output = "Error: Invalid operation.";
          if (name == "health")
            output = ChangeHealth(view, TryParameterFloat(values, 0));
          if (name == "stars")
            output = SetStars(character, TryParameterInt(values, 0));
          if (name == "tame")
            output = MakeTame(character);
          if (name == "wild")
            output = MakeWild(character);
          if (name == "baby")
            output = SetBaby(view.GetComponent<Growup>());
          if (name == "info")
            output = GetInfo(view);
          if (name == "sleep")
            output = MakeSleep(view.GetComponent<MonsterAI>());
          if (name == "visual")
            output = SetVisual(view.GetComponent<ItemStand>(), TryParameterString(values, 0), TryParameterInt(values, 1, 0));
          if (name == "helmet")
            output = SetHelmet(character, TryParameterString(values, 0), TryParameterInt(values, 1, 0));
          if (name == "left_hand")
            output = SetLeftHand(character, TryParameterString(values, 0), TryParameterInt(values, 1, 0));
          if (name == "right_hand")
            output = SetRightHand(character, TryParameterString(values, 0), TryParameterInt(values, 1, 0));
          if (name == "chest")
            output = SetChest(character, TryParameterString(values, 0), TryParameterInt(values, 1, 0));
          if (name == "shoulders")
            output = SetShoulder(character, TryParameterString(values, 0), TryParameterInt(values, 1, 0));
          if (name == "legs")
            output = SetLegs(character, TryParameterString(values, 0), TryParameterInt(values, 1, 0));
          if (name == "utility")
            output = SetUtility(character, TryParameterString(values, 0), TryParameterInt(values, 1, 0));
          if (name == "move")
            output = Move(view, values);
          if (name == "rotate")
            output = Rotate(view, values);
          if (name == "scale")
            output = Scale(view, values);
          if (name == "remove") {
            Actions.Remove(view.gameObject);
            output = "Entity ¤ destroyed.";
          }
          var message = output.Replace("¤", Utils.GetPrefabName(view.gameObject));
          if (zdos.Count() == 1)
            AddMessage(context, message);
          else
            context.AddString(message);
        }
      }
    }
    public ObjectCommand() {
      new Terminal.ConsoleCommand("object", "[operation=value] [id=*] [radius=0] - Modifies the targeted object.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        var pars = ParseArgs(args);
        IEnumerable<ZDO> zdos;
        if (pars.Radius > 0f) {
          zdos = GetZDOs(pars.Id, pars.Radius);
        } else {
          var view = GetHovered(args);
          if (!view) return;
          if (!GetPrefabs(pars.Id).Contains(view.GetZDO().GetPrefab())) {
            AddMessage(args.Context, $"Skipped: {view.name}  has invalid id.");
            return;
          }
          zdos = new ZDO[] { view.GetZDO() };
        }
        Execute(args.Context, pars.Operations, zdos);

      }, true, true, optionsFetcher: () => Operations);
      CommandParameters.AddFetcher("object", (int index, string parameter) => {
        if (parameter == "baby" || parameter == "tame" || parameter == "wild" || parameter == "remove" || parameter == "sleep" || parameter == "info") return Parameters.Create(parameter, "none");
        if (parameter == "id") {
          if (index == 0) return Parameters.Ids;
          return Parameters.Create(parameter, "none");
        }
        if (parameter == "left_hand" || parameter == "right_hand" || parameter == "helmet" || parameter == "chest" || parameter == "shoulders" || parameter == "legs" || parameter == "utility") {
          if (index == 0) return Parameters.ItemIds;
          if (index == 1) return Parameters.Create("visual", "number");
          return Parameters.Create(parameter, "none");
        }
        if (parameter == "radius" || parameter == "range" || parameter == "health" || parameter == "stars") {
          if (index == 0) return Parameters.Create(parameter, "number");
          return Parameters.Create(parameter, "none");
        }
        if (parameter == "move") {
          if (index == 0) return Parameters.Create(parameter, "x,z,y");
          if (index == 1) return Parameters.Origin;
          return Parameters.Create(parameter, "none");
        }
        if (parameter == "rotate") {
          if (index == 0) return Parameters.Create(parameter, "y,x,z|reset");
          if (index == 1) return Parameters.Origin;
          return Parameters.Create(parameter, "none");
        }
        if (parameter == "scale") {
          if (index == 0) return Parameters.Create(parameter, "x,z,y|number");
          return Parameters.Create(parameter, "none");
        }
        var args = Operations.Concat(Params).ToList();
        args.Sort();
        return args;
      });
    }
    private static TargetParameters ParseArgs(Terminal.ConsoleEventArgs args) {
      var parameters = new TargetParameters();
      foreach (var arg in args.Args) {
        var split = arg.Split('=');
        if (Operations.Contains(split[0]))
          parameters.Operations.Add(arg);
        if (split.Length < 2) continue;
        if (split[0] == "radius" || split[0] == "range")
          parameters.Radius = Math.Min(TryFloat(split[1], 0f), 100f);
        if (split[0] == "id")
          parameters.Id = split[1] == "" ? "*" : split[1];
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
      "helmet",
      "left_hand",
      "right_hand",
      "legs",
      "chest",
      "shoulders",
      "utility",
      "move",
      "rotate",
      "scale"
    };
    private static List<string> Params = new List<string>(){
      "id",
      "radius",
      "range",
    };

    private static string ChangeHealth(ZNetView obj, float amount) {
      var character = obj.GetComponent<Character>();
      if (character == null && obj.GetComponent<WearNTear>() == null && obj.GetComponent<TreeLog>() == null && obj.GetComponent<Destructible>() == null && obj.GetComponent<TreeBase>() == null)
        return "Skipped: ¤ is not a creature or a destructible.";
      var previous = Actions.SetHealth(obj.gameObject, amount);
      var amountStr = amount == 0f ? "default" : amount.ToString("F0");
      return $"¤ health changed from {previous.ToString("F0")} to {amountStr}.";
    }
    private static string SetStars(Character obj, int amount) {
      if (obj == null) return "Skipped: ¤ is not a creature.";
      var previous = obj.GetLevel() - 1;
      Actions.SetLevel(obj.gameObject, amount + 1);
      return $"¤ stars changed from {previous} to {amount}.";
    }
    private static string Move(ZNetView obj, string[] values) {
      var offset = TryParameterOffset(values, 0);
      var origin = TryParameterString(values, 1, "player").ToLower();
      Actions.Move(obj, offset, origin);
      return $"¤ moved {offset.ToString("F1")} from {origin}.";
    }
    private static string Rotate(ZNetView obj, string[] values) {
      if (TryParameterString(values, 0) == "reset") {
        Actions.ResetRotation(obj);
        return $"¤ rotation reseted.";
      }
      var relative = TryParameterRotation(values, 0);
      var origin = TryParameterString(values, 1, "player").ToLower();
      Actions.Rotate(obj, relative, origin);
      return $"¤ rotated {relative.ToString("F1")} from {origin}.";
    }
    private static string Scale(ZNetView obj, string[] values) {
      var scale = TryParameterScale(values, 0);
      Actions.Scale(obj, scale);
      return $"¤ scaled to {scale.ToString("F1")}.";
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
        var health = Actions.GetHealth(obj);
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
    public List<string> Operations = new List<string>();
  }
}
