using System.Collections.Generic;

namespace ServerDevcommands {
  public class CommandQueueItem {
    public string Command;
    public Terminal Terminal;
  }
  ///<summary>Helper for queueing commands.</summary>
  public class CommandQueue {

    private static Queue<CommandQueueItem> Items = new Queue<CommandQueueItem>();
    ///<summary>Timer for the next command execution</summary>
    public static float QueueTimer = 0f;
    public static void TickQueue(float delta) {
      QueueTimer -= delta;
      if (CanRun()) Run();
    }
    ///<summary>Runs the next command from the queue.</summary>
    public static void Run() {
      if (Items.Count == 0) return;
      QueueTimer = Settings.CommandDelay / 1000f;
      var item = Items.Dequeue();
      item.Terminal.TryRunCommand(item.Command);
    }
    public static bool CanRun() => QueueTimer <= 0f;
    public static void Add(Terminal terminal, string command) {
      Items.Enqueue(new CommandQueueItem { Command = command, Terminal = terminal });
    }
  }
}
