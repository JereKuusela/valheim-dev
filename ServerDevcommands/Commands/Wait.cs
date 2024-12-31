namespace ServerDevcommands;
///<summary>Adds a delay to command execution.</summary>
public class WaitCommand
{
  public WaitCommand()
  {
    new Terminal.ConsoleCommand("wait", "[duration] - Milliseconds to wait before executing the next command.", static (args) =>
    {
      // Not intended to be executed.
    });
    AutoComplete.Register("wait", static (int index) => index == 0 ? ParameterInfo.Create("Duration in milliseconds.") : ParameterInfo.None);
  }
}
