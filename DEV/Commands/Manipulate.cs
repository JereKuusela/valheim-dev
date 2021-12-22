using System;
using System.Collections.Generic;
using UnityEngine;
namespace DEV {
  public partial class Commands {
    private static ZNetView GetView(Terminal.ConsoleEventArgs args) {
      if (args.Length < 2) return null;
      if (Player.m_localPlayer == null) return null;
      var obj = Player.m_localPlayer.GetHoverObject();
      if (obj == null) {
        args.Context.AddString("Error: Nothing being hovered");
        return null;
      }
      var view = obj.GetComponentInParent<ZNetView>();
      if (view == null) {
        args.Context.AddString("Error: No net view.");
        return null;
      }
      return view;
    }
    public static void AddManipulate() {
      new Terminal.ConsoleCommand("move", "[x,z,y] [player/object/world] - Moves hovered object relative to selected origin (relative to player if not given).", delegate (Terminal.ConsoleEventArgs args) {
        var view = GetView(args);
        if (!view) return;
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
      }, true, false, true, false, false);
      new Terminal.ConsoleCommand("rotate", "[x,y,z/reset] [player/object/world] - Rotates hovered object around a given axis relative to selected origin (relative to player y axis if not given).", delegate (Terminal.ConsoleEventArgs args) {
        var view = GetView(args);
        if (!view) return;
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
        var view = GetView(args);
        if (!view) return;
        var scale = ParsePositionXZY(args.Args[1], Vector3.one);
        if (view.m_syncInitialScale)
          view.SetLocalScale(scale);
      }, true, false, true, false, false);
      new Terminal.ConsoleCommand("target", "[operation] [amount] - Modifies the targeted object.", delegate (Terminal.ConsoleEventArgs args) {
        var view = GetView(args);
        if (!view) return;
        var operation = args[1];
        var character = view.GetComponent<Character>();
        var output = "Error: Invalid operation.";
        if (operation == "health")
          output = ChangeHealth(character, args.TryParameterInt(2, 1));
        if (operation == "stars")
          output = SetStars(character, args.TryParameterInt(2, 1));
        if (operation == "tame")
          output = MakeTame(character);
        if (operation == "wild")
          output = MakeWild(character);
        if (operation == "baby")
          output = SetBaby(view.GetComponent<Growup>());
        if (operation == "info")
          output = GetInfo(view, args.Context);
        if (operation == "sleep")
          output = MakeSleep(view.GetComponent<MonsterAI>());
        args.Context.AddString(output);
      }, true, false, true, false, false, () => Operations);
    }
    private static List<string> Operations = new List<string>(){
      "health",
      "stars",
      "baby",
      "wild",
      "tame",
      "info",
      "sleep"
    };

    private static string ChangeHealth(Character obj, int amount) {
      if (obj == null) return "Error: Not a creature.";
      obj.SetMaxHealth(amount);
      obj.SetHealth(obj.GetMaxHealth());
      return "Health changed.";
    }
    private static string SetStars(Character obj, int amount) {
      if (obj == null) return "Error: Not a creature.";
      obj.SetLevel(amount + 1);
      return "Stars changed.";
    }
    private static string SetBaby(Growup obj) {
      if (obj == null) return "Error: Not an offspring.";
      obj.m_nview.GetZDO().Set("spawntime", DateTime.MaxValue.Ticks);
      return "Growth disabled.";
    }
    private static string MakeTame(Character obj) {
      if (obj == null) return "Error: Not a creature.";
      obj.SetTamed(true);
      var AI = obj.GetComponent<BaseAI>();
      if (AI) {
        AI.SetAlerted(false);
        AI.SetHuntPlayer(false);
        AI.SetPatrolPoint();
        AI.SetTargetInfo(ZDOID.None);
        var monster = obj.GetComponent<MonsterAI>();
        if (monster) {
          monster.m_targetCreature = null;
          monster.m_targetStatic = null;
          monster.SetDespawnInDay(false);
          monster.SetEventCreature(false);
        }
      }
      return "Target made tame.";
    }
    private static string MakeWild(Character obj) {
      if (obj == null) return "Error: Not a creature.";
      obj.SetTamed(false);
      var AI = obj.GetComponent<BaseAI>();
      if (AI) {
        AI.SetAlerted(false);
        AI.SetTargetInfo(ZDOID.None);
        var monster = obj.GetComponent<MonsterAI>();
        if (monster) {
          monster.m_targetCreature = null;
          monster.m_targetStatic = null;
        }
      }
      return "Target made wild.";
    }
    private static string MakeSleep(MonsterAI obj) {
      if (obj == null) return "Error: Not a creature.";
      obj.m_nview.GetZDO().Set("sleeping", true);
      return "Target made to sleep.";
    }
    private static string GetInfo(ZNetView obj, Terminal terminal) {
      terminal.AddString("Id: " + obj.GetPrefabName());
      var character = obj.GetComponent<Character>();
      if (character) {
        terminal.AddString("Health: " + character.GetHealth().ToString("F0") + " / " + character.GetMaxHealth());
        terminal.AddString("Stars: " + (character.GetLevel() - 1));
        terminal.AddString("Tamed: " + (character.IsTamed() ? "Yes" : "No"));
        var growUp = obj.GetComponent<Growup>();
        if (growUp)
          terminal.AddString("Baby: " + (growUp.m_baseAI.GetTimeSinceSpawned().TotalSeconds < 0 ? "Yes" : "No"));
      }
      return "";
    }
  }
}
