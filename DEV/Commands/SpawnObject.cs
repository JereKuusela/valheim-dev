using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DEV {
  public class SpawnObjectCommand : UndoCommand {
    private static void SetLevel(GameObject obj, float level) {
      if (level < 1) return;
      var character = obj.GetComponent<Character>();
      if (character)
        character.SetLevel((int)level);
    }
    private static void SetHealth(GameObject obj, float health) {
      if (health == 0) return;
      var character = obj.GetComponent<Character>();
      if (character) {
        character.SetMaxHealth(health);
        character.SetHealth(character.GetMaxHealth());
      }
      var wearNTear = obj.GetComponent<WearNTear>();
      if (wearNTear) {
        wearNTear.m_nview.GetZDO().Set("health", health);
      }
    }
    private static void RotateAndScale(GameObject obj, Quaternion rotation, Vector3 scale) {
      var view = obj.GetComponent<ZNetView>();
      if (view == null) return;
      if (rotation != Quaternion.identity) {
        view.GetZDO().SetRotation(rotation);
        obj.transform.rotation = rotation;
      }
      if (scale != Vector3.one && view.m_syncInitialScale)
        view.SetLocalScale(scale);
    }
    private static List<GameObject> DoSpawnObject(GameObject prefab, Vector3 position, int count, bool snap) {
      var spawned = new List<GameObject>();
      for (int i = 0; i < count; i++) {
        var spawnPosition = position;
        if (i > 0)
          spawnPosition += UnityEngine.Random.insideUnitSphere * 0.5f;
        if (snap && ZoneSystem.instance.FindFloor(spawnPosition, out var height))
          spawnPosition.y = height;
        var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, spawnPosition, Quaternion.identity);
        spawned.Add(obj);
      }
      return spawned;
    }

    public SpawnObjectCommand() {
      new Terminal.ConsoleCommand("spawn_object", "[name] (stars=n amount=n pos=x,z,y rot=z,x,z scale=x,y,z refPos=x,z,y refRot=y,x,z health=n) - Spawns an object.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          return;
        }
        string name = args[1];
        var prefab = GetPrefab(name);
        if (!prefab) return;

        var relativeRotation = Quaternion.identity;
        var baseRotation = Quaternion.identity;
        var scale = Vector3.one;
        var relativePosition = Vector3.zero;
        var basePosition = Vector3.zero;
        var player = Player.m_localPlayer.transform;
        if (player) {
          basePosition = player.position;
          relativePosition = new Vector3(2.0f, 0, 0);
          baseRotation = player.transform.rotation;
        }
        var level = 1;
        var amount = 1;
        var health = 0f;
        var snap = true;
        foreach (var arg in args.Args) {
          var split = arg.Split('=');
          if (split.Length < 2) continue;
          if (split[0] == "health")
            health = TryFloat(split[1], 0);
          if (split[0] == "star" || split[0] == "stars")
            level = TryInt(split[1], 0) + 1;
          if (split[0] == "level" || split[0] == "levels")
            level = TryInt(split[1], 1);
          if (split[0] == "amount")
            amount = TryInt(split[1], 1);
          if (split[0] == "rot" || split[0] == "rotation") {
            relativeRotation = ParseAngleYXZ(split[1]);
          }
          if (split[0] == "refRot" || split[0] == "refRotation") {
            baseRotation = ParseAngleYXZ(split[1], baseRotation);
          }
          if (split[0] == "sc" || split[0] == "scale") {
            var values = TrySplit(split[1], ",");
            if (values.Length == 1) {
              var value = TryParameterFloat(values, 0, 1);
              scale = new Vector3(value, value, value);
            } else {
              scale.x = TryParameterFloat(values, 0, 1f);
              scale.y = TryParameterFloat(values, 1, 1f);
              scale.z = TryParameterFloat(values, 2, 1f);
            }
            // Sanity check.
            if (scale.x == 0) scale.x = 1;
            if (scale.y == 0) scale.y = 1;
            if (scale.z == 0) scale.z = 1;
          }
          if (split[0] == "pos" || split[0] == "position") {
            relativePosition = ParsePositionXZY(split[1]);
            snap = split[1].Split(',').Length < 3;
          }
          if (split[0] == "refPos" || split[0] == "refPosition") {
            basePosition = ParsePositionXZY(split[1], basePosition);
          }
        }
        var spawnPosition = basePosition;
        spawnPosition += baseRotation * Vector3.forward * relativePosition.x;
        spawnPosition += baseRotation * Vector3.right * relativePosition.z;
        spawnPosition += baseRotation * Vector3.up * relativePosition.y;
        var spawnRotation = baseRotation * relativeRotation;
        var spawned = DoSpawnObject(prefab, spawnPosition, amount, snap);
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Spawning object " + name, spawned.Count, null);
        var spawns = new List<ZDO>();
        foreach (var obj in spawned) {
          SetLevel(obj, level);
          SetHealth(obj, health);
          RotateAndScale(obj, spawnRotation, scale);
          var netView = obj.GetComponent<ZNetView>();
          if (netView)
            spawns.Add(netView.GetZDO());
        }
        args.Context.AddString("Spawned: " + name + " at " + PrintVectorXZY(spawnPosition));
        Spawns.Push(spawns);

        // Disable player based positioning.
        AddToHistory("spawn_object " + name + " refRot=" + PrintAngleYXZ(baseRotation) + " refPos=" + PrintVectorXZY(basePosition) + " " + string.Join(" ", args.Args.Skip(2)));
      }, true, false, true, false, false, () => ZNetScene.instance.GetPrefabNames());
    }
  }
}