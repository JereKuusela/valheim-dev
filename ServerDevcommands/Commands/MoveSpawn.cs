using System;
using System.Linq;
using UnityEngine;

namespace ServerDevcommands;
///<summary>Commnand to print the seed.</summary>
public class MoveSpawn
{
  public MoveSpawn()
  {
    new Terminal.ConsoleCommand("move_spawn", "[x,z,y] Moves the default spawn point to the given coordinates (player's current coordinates if not given).", (args) =>
    {
      var parameters = Helper.AddPlayerPosXZY(args.Args, 1);
      if (ZNet.instance.IsServer())
      {
        var zs = ZoneSystem.instance;
        if (!zs) return;
        var coords = Parse.VectorXZY(Parse.Split(parameters[1]));

        // Spawn is based on the location icon. Other mods can alter this so the safest way is to use the icon data.
        zs.tempIconList.Clear();
        zs.GetLocationIcons(zs.tempIconList);

        var spawns = zs.tempIconList.Where(icon => icon.Value == "StartTemple").Select(icon => icon.Key).ToArray();
        if (spawns.Length == 0)
        {
          Helper.AddMessage(args.Context, "Default spawn point not found. Creating a new one.");
          if (CreateSpawnLocation(coords))
          {
            Helper.AddMessage(args.Context, $"Spawn created at {coords:F0}");
            zs.SendLocationIcons(ZRoutedRpc.Everybody);
          }
          else
            Helper.AddMessage(args.Context, "Failed to create spawn point. Default spawn point not found in location prefabs.");
        }
        else
        {
          if (spawns.Length > 1)
            Helper.AddMessage(args.Context, "Multiple spawn points found, moving the first one.");

          if (MoveSpawnLocation(spawns[0], coords))
          {
            Helper.AddMessage(args.Context, $"Spawn moved to {coords:F0}");
            zs.SendLocationIcons(ZRoutedRpc.Everybody);
          }
          else
            Helper.AddMessage(args.Context, "Failed to move spawn point. Default spawn point not found in location instances.");
        }
      }
      else ServerExecution.Send(parameters);
    }, true, true);
    AutoComplete.Register("move_spawn", (index, subIndex) =>
    {
      if (index == 0) return ParameterInfo.XZY("Coordinates (player's current coordinates if not given).", subIndex);
      return ParameterInfo.None;
    });
  }
  private bool CreateSpawnLocation(Vector3 to)
  {
    var zs = ZoneSystem.instance;
    var toZone = ZoneSystem.GetZone(to);
    var loc = zs.m_locations.FirstOrDefault(l => l.m_prefabName == "StartTemple");
    if (loc == null) return false;
    var spawn = new ZoneSystem.LocationInstance
    {
      m_position = to,
      m_location = loc
    };
    zs.m_locationInstances[toZone] = spawn;
    return true;
  }
  private bool MoveSpawnLocation(Vector3 from, Vector3 to)
  {
    var zs = ZoneSystem.instance;
    var fromZone = ZoneSystem.GetZone(from);
    var toZone = ZoneSystem.GetZone(to);
    if (!zs.m_locationInstances.TryGetValue(fromZone, out var spawn))
      return false;

    spawn.m_position = to;
    zs.m_locationInstances.Remove(fromZone);
    zs.m_locationInstances[toZone] = spawn;
    return true;
  }
}
