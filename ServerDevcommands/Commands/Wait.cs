namespace ServerDevcommands;
///<summary>Adds a delay to command execution.</summary>
public class WaitCommand {

  public WaitCommand() {
    new Terminal.ConsoleCommand("wait", "[duration] - Milliseconds to wait before executing the next command.", (Terminal.ConsoleEventArgs args) => {
      if (args.Length < 2) return;
      CommandQueue.QueueTimer = Parse.TryInt(args.Args, 1, 0) / 1000f;
    });
    AutoComplete.Register("wait", (int index) => {
      if (index == 0) return ParameterInfo.Create("Duration in milliseconds.");
      return null;
    });
  }
}
