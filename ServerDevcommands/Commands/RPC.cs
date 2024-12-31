using System.Linq;
using Service;

namespace ServerDevcommands;
///<summary>Adds duration and intensity.</summary>
public class RPCCommand
{
  public RPCCommand()
  {
    AutoComplete.Register("rpc", static (int index) =>
    {
      if (index == 0) return ParameterInfo.PlayerNames;
      if (index == 1) return ParameterInfo.Create("RPC name.");
      return ParameterInfo.Create("Parameters.");
    });
    Helper.Command("rpc", "[id1,id2,...] [name] [arg1] [arg2] ... - Sends rpc.", static (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing id.");
      Helper.ArgsCheck(args, 3, "Missing RPC name");
      var ids = Parse.Split(args[1]);
      var players = PlayerInfo.FindPlayers(ids);
      foreach (var player in players)
      {
        ZRoutedRpc.instance.InvokeRoutedRPC(ZNet.GetUID(), player.ZDOID, args[2], args.Args.Skip(3).ToArray());
      }
      var objects = ids.Select(static s =>
      {
        var split = s.Split(':');
        if (split.Length == 1) return null!;
        if (!long.TryParse(split[0], out var user)) return null;
        if (!uint.TryParse(split[1], out var id)) return null;
        ZDOID zid = new(user, id);
        return ZDOMan.instance.m_objectsByID.TryGetValue(zid, out var zdo) ? zdo : null;
      }).ToArray();
      foreach (var obj in objects)
      {
        if (obj == null) continue;
        ZRoutedRpc.instance.InvokeRoutedRPC(ZNet.GetUID(), obj.m_uid, args[2], args.Args.Skip(3).ToArray());
      }
    });
  }
}

