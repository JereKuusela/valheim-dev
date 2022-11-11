using UnityEngine;

namespace ServerDevcommands;
///<summary>Adds a delay to command execution.</summary>
public class WaitCommand
{
  public WaitCommand()
  {
    new Terminal.ConsoleCommand("wait", "[duration] - Milliseconds to wait before executing the next command.", (args) =>
    {
      if (args.Length < 2) return;
      CommandQueue.QueueTimer = Parse.Int(args.Args, 1, 0) / 1000f;
      // Runs at end of frame, so remove one frame to convert it to the start of frame.
      // Add half to make it mid-frame.
      CommandQueue.QueueTimer -= Time.deltaTime * 1.5f;
    });
    AutoComplete.Register("wait", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Duration in milliseconds.");
      return ParameterInfo.None;
    });
  }
}
