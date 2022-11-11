using System.Linq;
namespace ServerDevcommands;
///<summary>Utility command for server side execution.</summary>
public class ServerCommand
{
  public ServerCommand()
  {
    new Terminal.ConsoleCommand("server", "[command] - Executes the given command on the server.", (args) =>
    {
      if (args.Length < 2) return;
      var command = string.Join(" ", args.Args.Skip(1));
      var server = ZNet.instance.GetServerRPC();
      Console.instance.AddString("Sending command: " + command);
      if (server != null) server.Invoke(ServerExecution.RPC_Command, new[] { command });
    }, true, true);
  }
}
