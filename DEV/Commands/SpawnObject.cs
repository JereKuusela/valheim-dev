using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DEV {

  class SpawnObjectParameters {
    public Quaternion RelativeRotation = Quaternion.identity;
    public Quaternion BaseRotation = Quaternion.identity;
    public Vector3 Scale = Vector3.one;
    public Vector3 RelativePosition = Vector3.zero;
    public Vector3 BasePosition = Vector3.zero;
    public int Level = 1;
    public int Amount = 1;
    public float Health = 0f;
    public string Name = "";
    public int Variant = 0;
    public bool Tamed = false;
    public bool Hunt = false;
    public bool Snap = true;
  }
  public class SpawnObjectCommand : UndoCommand {
    private static List<GameObject> SpawnObject(GameObject prefab, Vector3 position, int count, bool snap) {
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
    private static ZNet.PlayerInfo FindPlayer(string name) {
      var players = ZNet.instance.m_players;
      var player = players.FirstOrDefault(player => player.m_name == name);
      if (!player.m_characterID.IsNone()) return player;
      player = players.FirstOrDefault(player => player.m_name.ToLower().StartsWith(name.ToLower()));
      if (!player.m_characterID.IsNone()) return player;
      return players.FirstOrDefault(player => player.m_name.ToLower().Contains(name.ToLower()));
    }
    private static SpawnObjectParameters ParseArgs(Terminal.ConsoleEventArgs args) {
      var pars = new SpawnObjectParameters();
      if (Player.m_localPlayer) {
        pars.BasePosition = Player.m_localPlayer.transform.position;
        pars.BaseRotation = Player.m_localPlayer.transform.rotation;
      }
      var useDefaultRelativePosition = true;
      foreach (var arg in args.Args) {
        var split = arg.Split('=');
        if (split[0] == "tame" || split[0] == "tamed")
          pars.Tamed = true;
        if (split[0] == "hunt")
          pars.Hunt = true;
        if (split.Length < 2) continue;
        if (split[0] == "health" || split[0] == "durability")
          pars.Health = TryFloat(split[1], 0);
        if (split[0] == "name" || split[0] == "crafter")
          pars.Name = split[1];
        if (split[0] == "variant")
          pars.Variant = TryInt(split[1], 0);
        if (split[0] == "star" || split[0] == "stars")
          pars.Level = TryInt(split[1], 0) + 1;
        if (split[0] == "level" || split[0] == "levels")
          pars.Level = TryInt(split[1], 1);
        if (split[0] == "amount")
          pars.Amount = TryInt(split[1], 1);
        if (split[0] == "rot" || split[0] == "rotation") {
          pars.RelativeRotation = ParseAngleYXZ(split[1]);
        }
        if (split[0] == "refRot" || split[0] == "refRotation") {
          pars.BaseRotation = ParseAngleYXZ(split[1], pars.BaseRotation);
        }
        if (split[0] == "sc" || split[0] == "scale") {
          var values = TrySplit(split[1], ",");
          if (values.Length == 1) {
            var value = TryParameterFloat(values, 0, 1);
            pars.Scale = new Vector3(value, value, value);
          } else {
            pars.Scale.x = TryParameterFloat(values, 0, 1f);
            pars.Scale.y = TryParameterFloat(values, 1, 1f);
            pars.Scale.z = TryParameterFloat(values, 2, 1f);
          }
          // Sanity check.
          if (pars.Scale.x == 0) pars.Scale.x = 1;
          if (pars.Scale.y == 0) pars.Scale.y = 1;
          if (pars.Scale.z == 0) pars.Scale.z = 1;
        }
        if (split[0] == "pos" || split[0] == "position") {
          useDefaultRelativePosition = false;
          pars.RelativePosition = ParsePositionXZY(split[1]);
          pars.Snap = split[1].Split(',').Length < 3;
        }
        if (split[0] == "refPos" || split[0] == "refPosition") {
          useDefaultRelativePosition = false;
          if (split[1].Contains(","))
            pars.BasePosition = ParsePositionXZY(split[1], pars.BasePosition);
          else {
            var player = FindPlayer(split[1]);
            if (player.m_characterID.IsNone()) {
              args.Context.AddString("Error: Unable to find the player.");
              return null;
            } else if (!player.m_publicPosition) {
              args.Context.AddString("Error: Player doesn't have a public position.");
              return null;
            } else {
              pars.BasePosition = player.m_position;
            }
          }
        }
      }
      // For usability, spawn in front of the player if nothing is specified (similar to the base game command).
      if (useDefaultRelativePosition)
        pars.RelativePosition = new Vector3(2.0f, 0, 0);
      return pars;
    }
    private static Vector3 GetPosition(Vector3 basePosition, Vector3 relativePosition, Quaternion rotation) {
      var position = basePosition;
      position += rotation * Vector3.forward * relativePosition.x;
      position += rotation * Vector3.right * relativePosition.z;
      position += rotation * Vector3.up * relativePosition.y;
      return position;
    }
    private static void Manipulate(IEnumerable<GameObject> spawned, SpawnObjectParameters pars) {
      var rotation = pars.BaseRotation * pars.RelativeRotation;
      var total = pars.Amount;
      foreach (var obj in spawned) {
        Actions.SetLevel(obj, pars.Level);
        Actions.SetHealth(obj, pars.Health);
        Actions.SetVariant(obj, pars.Variant);
        Actions.SetName(obj, pars.Name);
        Actions.SetHunt(obj, pars.Hunt);
        Actions.SetTame(obj, pars.Tamed);
        total -= Actions.SetStack(obj, total);
        Actions.SetRotation(obj, rotation);
        Actions.SetScale(obj, pars.Scale);
      }
    }
    public SpawnObjectCommand() {
      new Terminal.ConsoleCommand("spawn_object", "[name] (stars=n amount=n pos=x,z,y rot=z,x,z scale=x,y,z refPos=x,z,y refRot=y,x,z health=n) - Spawns an object.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        var prefabName = args[1];
        var prefab = GetPrefab(prefabName);
        if (!prefab) return;

        var pars = ParseArgs(args);
        if (pars == null) return;
        var itemDrop = prefab.GetComponent<ItemDrop>();
        var count = pars.Amount;
        if (itemDrop)
          count = (int)Math.Ceiling((double)count / itemDrop.m_itemData.m_shared.m_maxStackSize);
        var position = GetPosition(pars.BasePosition, pars.RelativePosition, pars.BaseRotation);
        var spawned = SpawnObject(prefab, position, count, pars.Snap);
        Manipulate(spawned, pars);
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Spawning object " + prefabName, spawned.Count, null);
        args.Context.AddString("Spawned: " + prefabName + " at " + PrintVectorXZY(position));
        var spawns = spawned.Select(obj => obj.GetComponent<ZNetView>()?.GetZDO()).Where(obj => obj != null).ToList();
        Spawns.Push(spawns);

        // Disable player based positioning.
        AddToHistory("spawn_object " + prefabName + " refRot=" + PrintAngleYXZ(pars.BaseRotation) + " refPos=" + PrintVectorXZY(pars.BasePosition) + " " + string.Join(" ", args.Args.Skip(2)));
      }, true, false, true, false, false, () => ZNetScene.instance.GetPrefabNames());
    }
  }
}