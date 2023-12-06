using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerDevcommands;

public class MultiCommands(Terminal terminal, string row)
{
  public static bool IsMulti(string text) => Settings.MultiCommand && text.Contains(";");
  private static string[] Split(string text) => text.Split(';').Select(s => s.Trim()).ToArray();
  private static readonly List<MultiCommands> Groups = [];
  public static void Handle(Terminal terminal, string row)
  {
    MultiCommands cmd = new(terminal, row);
    cmd.Run(0);
    if (cmd.IsDone()) return;
    Groups.Add(cmd);
  }
  public static void Execute(float dt)
  {
    for (var i = 0; i < Groups.Count; i++)
    {
      Groups[i].Run(dt);
      if (Groups[i].IsDone())
      {
        Groups.RemoveAt(i);
        i--;
      }
    }
  }


  private readonly Queue<string> Commands = new(Split(row).Select(s => Aliasing.Plain(s)));
  private readonly Terminal Terminal = terminal;
  private float WaitTimer = 0f;

  public bool IsDone() => Commands.Count() == 0;

  public void Run(float dt)
  {
    if (WaitTimer > -0.01)
      WaitTimer -= dt;
    // Another check to execute at the same frame as the timer is done.
    if (WaitTimer > -0.01)
      return;
    while (Commands.Count() > 0)
    {
      var command = Commands.Dequeue();
      if (command.StartsWith("wait ", StringComparison.InvariantCultureIgnoreCase))
      {
        var args = command.Split(' ');
        if (args.Length < 2) continue;
        WaitTimer = float.Parse(args[1]) / 1000f;
        // Might need some tweaks to take in account frame time.
        break;
      }
      Terminal.TryRunCommand(command);
    }
  }
}