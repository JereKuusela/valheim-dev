using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace DEV {

  public partial class Commands {
    public static void AddSpawnLocation() {
      new Terminal.ConsoleCommand("spawn_location", "[name] [seed/?] [rotation/?] [x,z,y] - Spawns a given location.", delegate (Terminal.ConsoleEventArgs args) {
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

        var seed = args.TryParameterInt(2, UnityEngine.Random.Range(0, 99999));
        var rotation = args.TryParameterFloat(3, (float)UnityEngine.Random.Range(0, 16) * 22.5f);
        var pos = Player.m_localPlayer.transform.position;
        var split = TryParameterSplit(args.Args, 4, ",");
        var x = TryParameterFloat(split, 0, pos.x);
        var z = TryParameterFloat(split, 1, pos.z);
        if (ZoneSystem.instance.FindFloor(pos, out var value))
          pos.y = value;
        var y = TryParameterFloat(split, 2, pos.y);
        AddedZDOs.zdos.Clear();
        ZoneSystem.instance.SpawnLocation(location, seed, new Vector3(x, y, z), Quaternion.Euler(0f, rotation, 0f), ZoneSystem.SpawnMode.Full, new List<GameObject>());
        Spawns.Push(AddedZDOs.zdos.ToList());
        AddedZDOs.zdos.Clear();
        args.Context.AddString("Spawned: " + name);
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