using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
///<summary>Adds coordinates to the parameters (required for server side support).</summary>
public class StartEventCommand
{
  private static void DoStartEvent(string[] args, Vector3 position, Terminal context)
  {
    if (args.Length < 2) return;
    var name = args[1];
    if (!RandEventSystem.instance.HaveEvent(name))
    {
      context.AddString("Random event not found:" + name);
      return;
    }
    RandEventSystem.instance.SetRandomEventByName(name, new(position.x, 0, position.z));
  }
  public StartEventCommand()
  {
    Helper.Command("event", "[name] [x] [z] - start event.", (args) =>
    {
      if (args.Length < 2) return;
      var (parameters, position) = Helper.AddPlayerPosXZ(args.Args, 2);
      if (ZNet.instance.IsServer()) DoStartEvent(parameters, position, args.Context);
      else ServerExecution.Send(parameters);
    });
    AutoComplete.Register("event", index =>
    {
      if (index == 0) return RandEventSystem.instance.m_events.Select(ev => ev.m_name).ToList();
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