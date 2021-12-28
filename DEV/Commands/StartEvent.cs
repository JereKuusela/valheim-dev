using System.Linq;
using UnityEngine;

namespace DEV {

  public class StartEventCommand : BaseCommands {
    private static void DoStartEvent(string[] args, Terminal context) {
      if (args.Length < 2) return;
      var name = args[1];
      if (!RandEventSystem.instance.HaveEvent(name)) {
        context.AddString("Random event not found:" + name);
        return;
      }
      var x = TryParameterFloat(args, 2, 0);
      var z = TryParameterFloat(args, 3, 0);
      RandEventSystem.instance.SetRandomEventByName(name, new Vector3(x, 0, z));
    }
    ///<summary>Must be replaced because base game command uses the player position which won't work on the server. </summary>
    public StartEventCommand() {
      new Terminal.ConsoleCommand("event", "[name] [x] [z] - start event.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        var parameters = AddPlayerPosXZ(args.Args, 2);
        if (ZNet.instance.IsServer()) DoStartEvent(parameters, args.Context);
        else SendCommand(parameters);
      }, true, false, true, false, false, () => RandEventSystem.instance.m_events.Select(ev => ev.m_name).ToList());
    }
  }
}