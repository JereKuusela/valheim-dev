using System;
using System.Collections.Generic;
using UnityEngine;

namespace DEV {

  public partial class Commands {

    private static void SetLevel(GameObject obj, float level) {
      if (level < 1) return;
      Character component2 = obj.GetComponent<Character>();
      if (component2)
        component2.SetLevel((int)level);
    }
    private static void Rotate(GameObject obj, float level) {
      if (level < 1) return;
      Character component2 = obj.GetComponent<Character>();
      if (component2)
        component2.SetLevel((int)level);
      else
        obj.transform.rotation = Quaternion.Euler(0, level, 0);
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
    private static GameObject SpawnObject(GameObject prefab, Vector3 position) {
      return UnityEngine.Object.Instantiate<GameObject>(prefab, position, Quaternion.identity);
    }
    private static List<GameObject> DoSpawnObject(GameObject prefab, int count) {
      var player = Player.m_localPlayer;
      var spawned = new List<GameObject>();
      var spawnPosition = player.transform.position + player.transform.forward * 2f;
      for (int i = 0; i < count; i++) {
        var position = spawnPosition;
        if (i > 0)
          position += UnityEngine.Random.insideUnitSphere * 0.5f;
        if (ZoneSystem.instance.FindFloor(position, out var height))
          position.y = height;
        var obj = SpawnObject(prefab, position);
        spawned.Add(obj);
      }
      return spawned;
    }
    private static GameObject GetPrefab(string name) {
      var prefab = ZNetScene.instance.GetPrefab(name);
      if (!prefab)
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Missing object " + name, 0, null);
      return prefab;
    }

    public static void AddSpawnObject() {
      new Terminal.ConsoleCommand("spawn_object", "[name] [rotation/(y,x,z)] [scale/(x,y,z)] [amount] - Spawns an object.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          return;
        }
        string name = args[1];
        Quaternion rotation = Quaternion.identity;
        var angles = TrySplit(args.Args, 2, ",");
        var angle = Vector3.zero;
        angle.y = TryParameterFloat(angles, 0, 1f);
        angle.x = TryParameterFloat(angles, 1, 1f);
        angle.z = TryParameterFloat(angles, 2, 1f);
        rotation = Quaternion.Euler(angle);
        Vector3 scale = Vector3.one;
        var scales = TrySplit(args.Args, 3, ",");
        if (scales.Length == 1) {
          var value = TryParameterFloat(scales, 0, 1);
          scale = new Vector3(value, value, value);
        } else {
          scale.x = TryParameterFloat(scales, 0, 1f);
          scale.y = TryParameterFloat(scales, 1, 1f);
          scale.z = TryParameterFloat(scales, 2, 1f);
          // Sanity check.
          if (scale.x == 0) scale.x = 1;
          if (scale.y == 0) scale.y = 1;
          if (scale.z == 0) scale.z = 1;
        }
        var count = args.TryParameterInt(4, 1);
        DateTime now = DateTime.Now;
        var prefab = GetPrefab(name);
        if (!prefab) return;

        var spawned = DoSpawnObject(prefab, count);
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Spawning object " + name, spawned.Count, null);
        var spawns = new List<ZDO>();
        foreach (var obj in spawned) {
          RotateAndScale(obj, rotation, scale);
          var netView = obj.GetComponent<ZNetView>();
          if (netView)
            spawns.Add(netView.GetZDO());
        }
        ZLog.Log("Spawn time :" + (DateTime.Now - now).TotalMilliseconds + " ms");
        Gogan.LogEvent("Cheat", "Spawn", name, count);
        Spawns.Push(spawns);
      }, true, false, true, false, false, () => ZNetScene.instance.GetPrefabNames());
    }
  }
}