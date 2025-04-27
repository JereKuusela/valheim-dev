namespace ServerDevcommands;
///<summary>Adds a delay to command execution.</summary>
public class WaitCommand
{
  public WaitCommand()
  {
    new Terminal.ConsoleCommand("wait", "[duration] - Milliseconds to wait before executing the next command.", (args) =>
    {
      // Not intended to be executed.
    });
    AutoComplete.Register("wait", index => index == 0 ? ParameterInfo.Create("Duration in milliseconds.") : ParameterInfo.None);
  }
}
