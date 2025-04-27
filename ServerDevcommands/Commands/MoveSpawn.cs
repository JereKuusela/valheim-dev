using System.Linq;

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
        var location = zs.m_locationInstances.First(location => Helper.IsValid(location.Value.m_location) && location.Value.m_location.m_prefab.Name == "StartTemple");
        var newLocation = location.Value;
        newLocation.m_position = coords;
        zs.m_locationInstances[location.Key] = newLocation;
        Helper.AddMessage(args.Context, $"Spawn moved to {coords:F0}");
        zs.SendLocationIcons(ZRoutedRpc.Everybody);
      }
      else ServerExecution.Send(parameters);
    }, true, true);
    AutoComplete.Register("move_spawn", (index, subIndex) =>
    {
      if (index == 0) return ParameterInfo.XZY("Coordinates (player's current coordinates if not given).", subIndex);
      return ParameterInfo.None;
    });
  }
}
