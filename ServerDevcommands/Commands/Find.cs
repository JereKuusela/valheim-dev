using System;
using System.Linq;
using Service;
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
      // First priority is exact matches (from both prefabs and locations).
      var prefabs = Selector.GetAllPrefabs([args[1]]);
      var pos = Parse.VectorXZY(string.Join(",", args.Args.Skip(3)));
      var locations = ZoneSystem.instance.GetLocationList().Where(l => Helper.IsValid(l.m_location) && l.m_location.m_prefab.Name == args[1]).ToList();
      var anyMatches = prefabs.Count > 0 || locations.Count > 0;
      if (!anyMatches)
      {
        var lower = args[1].ToLower();
        locations = ZoneSystem.instance.GetLocationList().Where(l => Helper.IsValid(l.m_location) && l.m_location.m_prefab.Name.ToLower().Contains(lower)).ToList();
        prefabs = Selector.GetAllPrefabs(["*" + args[1] + "*"]);
      }
      var list = locations.Select(l => Tuple.Create(l.m_location.m_prefab.Name, l.m_position)).ToList();
      var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => zdo.IsValid() && prefabs.Contains(zdo.GetPrefab()));
      list.AddRange(zdos.Select(zdo => Tuple.Create(ZNetScene.instance.GetPrefab(zdo.m_prefab).name, zdo.GetPosition())));
      list.Sort((Tuple<string, Vector3> a, Tuple<string, Vector3> b) => Vector3.Distance(a.Item2, pos).CompareTo(Vector3.Distance(b.Item2, pos)));
      var count = list.Count;
      list = list.Take(Parse.Int(args.Args, 2, 10)).ToList();
      var text = list.Select(p => Format(pos, p.Item2, p.Item1)).ToList();
      args.Context.AddString($"Found {count} of {args[1]}. Showing {list.Count} closest:");
      args.Context.AddString(string.Join("\n", text));
      if (RedirectOutput.Target == null)
        ServerExecution.RPC_Do_Pins(null, string.Join("|", list.Select(item => Helper.PrintVectorXZY(item.Item2))));
      else
        RedirectOutput.Target.Invoke(ServerExecution.RPC_Pins, string.Join("|", list.Select(item => Helper.PrintVectorXZY(item.Item2))));
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

  private static string Format(Vector3 pos, Vector3 p, string name)
  {
    var distance = Vector3.Distance(pos, p);
    var format = Settings.FindFormat
      .Replace("{pos_x", "{0")
      .Replace("{pos_y", "{1")
      .Replace("{pos_z", "{2")
      .Replace("{distance", "{3")
      .Replace("{name", "{4");
    return string.Format(format, p.x, p.y, p.z, distance, name);
  }
}
