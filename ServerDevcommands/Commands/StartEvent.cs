using System.Linq;
using HarmonyLib;
namespace ServerDevcommands;
///<summary>Adds coordinates to the parameters (required for server side support).</summary>
public class StartEventCommand
{
  private static void DoStartEvent(string[] args, Terminal context)
  {
    if (args.Length < 2) return;
    var name = args[1];
    if (!RandEventSystem.instance.HaveEvent(name))
    {
      context.AddString("Random event not found:" + name);
      return;
    }
    var x = Parse.Float(args, 2, 0);
    var z = Parse.Float(args, 3, 0);
    RandEventSystem.instance.SetRandomEventByName(name, new(x, 0, z));
  }
  public StartEventCommand()
  {
    Helper.Command("event", "[name] [x] [z] - start event.", (args) =>
    {
      if (args.Length < 2) return;
      var parameters = Helper.AddPlayerPosXZ(args.Args, 2);
      if (ZNet.instance.IsServer()) DoStartEvent(parameters, args.Context);
      else ServerExecution.Send(parameters);
    }, () => RandEventSystem.instance.m_events.Select(ev => ev.m_name).ToList());
    AutoComplete.Register("event", (int index) =>
    {
      if (index == 0) return Terminal.commands["event"].m_tabOptionsFetcher();
      if (index == 1) return ParameterInfo.Create("X coordinate", "number (default is the current position)");
      if (index == 2) return ParameterInfo.Create("Z coordinate", "number (default is the current position)");
      return ParameterInfo.None;
    });
  }
}

[HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.StartRandomEvent))]
public class DisableRandomEvents
{
  static bool Prefix() => !Settings.DisableEvents;
}