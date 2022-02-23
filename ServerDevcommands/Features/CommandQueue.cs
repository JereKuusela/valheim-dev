using System.Collections.Generic;

namespace ServerDevcommands {
  public class CommandQueueItem {
    public string Command;
    public Terminal terminal;
  }
  ///<summary>Helper for queueing commands.</summary>
  public class CommandQueue {

    private static Queue<CommandQueueItem> Items = new Queue<CommandQueueItem>();
    ///<summary>Timer for the next command execution</summary>
    public static float QueueTimer = 0f;
    public static void TickQueue(float delta) {
      QueueTimer -= delta;
      TryRun();
    }
    ///<summary>Runs the next command from the queue.</summary>
    public static void TryRun() {
      if (Items.Count == 0 || QueueTimer <= 0f) return;
      QueueTimer = Settings.CommandDelay;
      var item = Items.Dequeue();
      item.terminal.TryRunCommand(item.Command);
    }
    public static void Add(Terminal terminal, string command) {
      Items.Enqueue(new CommandQueueItem { Command = command, Terminal = terminal });
    }
  }
}
