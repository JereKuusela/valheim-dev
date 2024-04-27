using System.Linq;
using UnityEngine;

namespace ServerDevcommands;
///<summary>Adds server side support.</summary>
public class FindCommand
{
  public FindCommand()
  {
    var defaultCommand = Terminal.commands["find"];
    Helper.Command("find", defaultCommand.Description, (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing the search term.");
      if (args.Length < 3) args.Args = [.. args.Args, "10"];
      var parameters = Helper.AddPlayerPosXZY(args.Args, 3);
      // Client only has surroundings which is not very useful.
      if (!ZNet.instance.IsServer())
      {
        ServerExecution.Send(parameters);
        return;
      }

      var prefab = args[1].GetStableHashCode();
      var pos = Parse.VectorXZY(string.Join(",", args.Args.Skip(3)));
      var locations = ZoneSystem.instance.GetLocationList().Where(l => Helper.IsValid(l.m_location) && l.m_location.m_prefab.Name == args[1]);
      var list = locations.Select(l => l.m_position).ToList();
      var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => zdo.IsValid() && zdo.GetPrefab() == prefab);
      list.AddRange(zdos.Select(zdo => zdo.GetPosition()));
      list.Sort((Vector3 a, Vector3 b) => Vector3.Distance(a, pos).CompareTo(Vector3.Distance(b, pos)));
      var count = list.Count;
      list = list.Take(Parse.Int(args.Args, 2, 10)).ToList();
      var text = list.Select(p => Format(pos, p)).ToList();
      args.Context.AddString($"Found {count} of {args[1]}. Showing {list.Count} closest:");
      args.Context.AddString(string.Join("\n", text));
      if (RedirectOutput.Target == null)
        ServerExecution.RPC_Do_Pins(null, string.Join("|", list.Select(Helper.PrintVectorXZY)));
      else
        RedirectOutput.Target.Invoke(ServerExecution.RPC_Pins, string.Join("|", list.Select(Helper.PrintVectorXZY)));
    });
    AutoComplete.Register("find", (int index) =>
    {
      if (index == 0) return ParameterInfo.Ids;
      if (index == 1) return ParameterInfo.Create("Max amount", "a positive integer (default 10)");
      if (index == 2) return ParameterInfo.Create("X coordinate", "if not specified, the current position is used");
      if (index == 3) return ParameterInfo.Create("Z coordinate", "if not specified, the current position is used");
      return ParameterInfo.None;
    });
  }

  private static string Format(Vector3 pos, Vector3 p)
  {
    var distance = Vector3.Distance(pos, p);
    var format = Settings.FindFormat
      .Replace("{pos_x", "{0")
      .Replace("{pos_y", "{1")
      .Replace("{pos_z", "{2")
      .Replace("{distance", "{3");
    return string.Format(format, p.x, p.y, p.z, distance);
  }
}
