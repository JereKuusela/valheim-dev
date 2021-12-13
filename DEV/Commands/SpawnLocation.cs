using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace DEV {

  public partial class Commands {
    public static void AddSpawnLocation() {
      new Terminal.ConsoleCommand("spawn_location", "[name] [...args (rot=y, pos=x,z,y, seed=n)] - Spawns a given location.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          return;
        }
        var obj = ZoneSystem.instance;
        var name = args[1];
        var location = obj.GetLocation(name);
        if (location == null) {
          ZLog.Log("Missing location:" + name);
          args.Context.AddString("Missing location:" + name);
          return;
        }
        if (location.m_prefab == null) {
          ZLog.Log("Missing prefab in location:" + name);
          args.Context.AddString("Missing location:" + name);
          return;
        }

        var seed = UnityEngine.Random.Range(0, 99999);
        var rotation = (float)UnityEngine.Random.Range(0, 16) * 22.5f;
        var position = Vector3.zero;
        var player = Player.m_localPlayer.transform;
        if (player) {
          position = player.position + player.forward * 2f;
        }
        var snap = true;
        foreach (var arg in args.Args) {
          var split = arg.Split('=');
          if (split.Length < 2) continue;
          if (split[0] == "seed")
            seed = TryInt(split[1], 0);
          if (split[0] == "rot" || split[0] == "rotation")
            rotation = TryFloat(split[1], 0);
          if (split[0] == "pos" || split[0] == "position") {
            var values = TrySplit(split[1], ",");
            if (player) {
              position = player.position;
              position += player.forward * TryParameterFloat(values, 0, 0f);
              position += player.right * TryParameterFloat(values, 1, 0f);
              position += player.up * TryParameterFloat(values, 2, 0f);
            } else {
              position.x = TryParameterFloat(values, 0, 0f);
              position.z = TryParameterFloat(values, 0, 0f);
              position.y = TryParameterFloat(values, 0, 0f);
            }
            snap = values.Length < 3;
          }
        }
        if (snap && ZoneSystem.instance.FindFloor(position, out var value))
          position.y = value;

        AddedZDOs.zdos.Clear();
        ZoneSystem.instance.SpawnLocation(location, seed, position, Quaternion.Euler(0f, rotation, 0f), ZoneSystem.SpawnMode.Full, new List<GameObject>());
        Spawns.Push(AddedZDOs.zdos.ToList());
        AddedZDOs.zdos.Clear();
        args.Context.AddString("Spawned: " + name);
        // Disable player based positioning.
        AddToHistory("spawn_location " + name + " pos=" + position.x + "," + position.z + "," + position.y + " seed=" + seed + " rot=" + rotation + " " + string.Join(" ", args.Args.Skip(2)));

      }, true, false, true, false, false, () => ZoneSystem.instance.m_locations.Select(location => location.m_prefabName).ToList());
    }

    ///<summary>Catch new zdos for undo.</summary>
    [HarmonyPatch(typeof(ZNetView), "Awake")]
    public class AddedZDOs {
      public static List<ZDO> zdos = new List<ZDO>();
      public static void Postfix(ZNetView __instance) {
        zdos.Add(__instance.GetZDO());
      }
    }
  }
}