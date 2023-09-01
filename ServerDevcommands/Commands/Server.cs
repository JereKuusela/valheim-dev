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
    }, original.m_tabOptionsFetcher);
  }
  public ServerCommand()
  {
    new Terminal.ConsoleCommand("server", "[command] - Executes the given command on the server.", (args) =>
    {
      if (args.Length < 2) return;
      var command = string.Join(" ", args.Args.Skip(1));
      var server = ZNet.instance.GetServerRPC();
      Console.instance.AddString("Sending command: " + command);
      server?.Invoke(ServerExecution.RPC_Command, new[] { command });
    }, true, true);

    MakeServer("randomevent");
    AutoComplete.RegisterEmpty("randomevent");
    MakeServer("stopevent");
    AutoComplete.RegisterEmpty("stopevent");
    MakeServer("genloc");
    AutoComplete.RegisterEmpty("genloc");
    MakeServer("sleep");
    AutoComplete.RegisterEmpty("sleep");
    MakeServer("skiptime");
    AutoComplete.Register("skiptime", (int index) =>
    {
      if (index == 0)
      {
        if (Settings.DisableParameterWarnings)
          return ParameterInfo.Create("Seconds", "a number (default 240.0)");
        else
          return ParameterInfo.Create("Seconds", "a number (default 240.0), <color=yellow>WARNING</color>: High negative values may cause issues because object timestamps won't get updated!");
      }
      return ParameterInfo.None;
    });
    var original = Terminal.commands["resetkeys"];
    Helper.Command("resetkeys", original.Description, (args) =>
    {
      Player.m_localPlayer?.ResetUniqueKeys();
      if (ZNet.instance && !ZNet.instance.IsServer())
      {
        ServerExecution.Send(args);
      }
      else
      {
        ZoneSystem.instance.ResetGlobalKeys();
        args.Context.AddString("Global and player keys cleared");
      }
    }, original.m_tabOptionsFetcher);
    AutoComplete.RegisterEmpty("resetkeys");
  }
}
