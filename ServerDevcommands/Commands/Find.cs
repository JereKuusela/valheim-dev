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
      // Works fine on single player.
      if (!ZNet.m_instance || ZNet.instance.IsServer() && !ZNet.instance.IsDedicated())
      {
        defaultCommand.RunAction(args);
        return;
      }
      if (args.Length < 3) args.Args = args.Args.Append("1").ToArray();
      var parameters = Helper.AddPlayerPosXZY(args.Args, 3);
      // Client only has surroundings which is not very useful.
      if (!ZNet.instance.IsServer())
      {
        ServerExecution.Send(parameters);
        return;
      }

      var prefab = args[1].GetStableHashCode();
      var pos = Parse.VectorXZY(string.Join(",", args.Args.Skip(3)));
      var locations = ZoneSystem.instance.GetLocationList().Where(l => l.m_location != null && l.m_location.m_prefabName == args[1]);
      var list = locations.Select(l => l.m_position).ToList();
      var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => zdo.IsValid() && zdo.GetPrefab() == prefab);
      list.AddRange(zdos.Select(zdo => zdo.GetPosition()));
      list.Sort((Vector3 a, Vector3 b) => Vector3.Distance(a, pos).CompareTo(Vector3.Distance(b, pos)));
      list = list.Take(Parse.Int(args.Args, 2, 1)).ToList();
      var text = list.Select(p => $"{Helper.PrintVectorXZY(p)}, distance {Vector3.Distance(p, pos)}").ToList();
      args.Context.AddString(string.Join("\n", text));
      if (RedirectOutput.Target != null)
      {
        var rpc = RedirectOutput.Target;
        rpc.Invoke(ServerExecution.RPC_Pins, string.Join("|", list.Select(Helper.PrintVectorXZY)));
      }
    }, () => ParameterInfo.Ids);
    AutoComplete.Register("find", (int index) =>
    {
      if (index == 0) return ParameterInfo.Ids;
      if (index == 1) return ParameterInfo.Create("Max amount", "a positive integer (default 1)");
      if (index == 2) return ParameterInfo.Create("X coordinate", "if not specified, the current position is used");
      if (index == 3) return ParameterInfo.Create("Z coordinate", "if not specified, the current position is used");
      return ParameterInfo.None;
    });
  }
}
