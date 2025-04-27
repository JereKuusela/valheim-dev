using System.Linq;
namespace ServerDevcommands;
///<summary>Utility command for server side execution.</summary>
public class ServerCommand
{

  private void MakeServer(string name)
  {
    var original = Terminal.commands[name];
    Helper.Command(name, original.Description, (args) =>
    {
      if (ZNet.instance && !ZNet.instance.IsServer())
      {
        ServerExecution.Send(args);
      }
      else original.action(args);
    });
  }
  public ServerCommand()
  {
    new Terminal.ConsoleCommand("server", "[command] - Executes the given command on the server.", (args) =>
    {
      if (args.Length < 2) return;
      var command = string.Join(" ", args.Args.Skip(1));
      var server = ZNet.instance.GetServerRPC();
      Console.instance.AddString("Sending command: " + command);
      server?.Invoke(ServerExecution.RPC_Command, [command]);
    }, true, true);
    AutoComplete.RegisterEmpty("server");
    AutoComplete.Offsets["server"] = 0;

    MakeServer("randomevent");
    AutoComplete.RegisterEmpty("randomevent");
    MakeServer("stopevent");
    AutoComplete.RegisterEmpty("stopevent");
    AutoComplete.RegisterEmpty("genloc");
    AutoComplete.RegisterEmpty("sleep");
    AutoComplete.Register("skiptime", index =>
    {
      if (index == 0) return ParameterInfo.Create("Seconds", "a number (default 240.0)");
      return ParameterInfo.None;
    });
    AutoComplete.RegisterEmpty("resetkeys");
  }
}
