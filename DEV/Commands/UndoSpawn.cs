using System.Collections.Generic;
using System.Linq;
using Service;

namespace DEV {
  public class UndoCommand : BaseCommands {
    public static Stack<IEnumerable<ZDO>> Spawns = new Stack<IEnumerable<ZDO>>();
    public static List<string> History = new List<string>();
    public static int HistoryIndex = 0;
    protected static bool Redo = false;
    public static void AddToHistory(string command) {
      if (Redo && HistoryIndex < History.Count)
        HistoryIndex++;
      if (!Redo) {
        History = History.Take(HistoryIndex).ToList();
        History.Add(command);
        HistoryIndex++;
      }
    }
  }
  public class UndoSpawnCommand : UndoCommand {
    public UndoSpawnCommand() {
      new Terminal.ConsoleCommand("undo_spawn", "- Removes spawned objects.", delegate (Terminal.ConsoleEventArgs args) {
        if (Spawns.Count == 0)
          return;
        var toRemove = Spawns.Pop();
        foreach (var zdo in toRemove) {
          if (zdo != null && zdo.IsValid())
            ZDOMan.instance.DestroyZDO(zdo);
        }
        if (HistoryIndex > 0)
          HistoryIndex--;
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Removing " + toRemove.Count() + " spawned objects", 0, null);
      }, true, true);
    }
  }
  public class RedoSpawnCommand : UndoCommand {
    public RedoSpawnCommand() {
      new Terminal.ConsoleCommand("redo_spawn", "- Restores undoed spawned objects.", delegate (Terminal.ConsoleEventArgs args) {
        if (HistoryIndex >= History.Count)
          return;
        Redo = true;
        Console.instance.TryRunCommand(History[HistoryIndex]);
        Redo = false;
      }, true, true);
      new Terminal.ConsoleCommand("dev_undo", "Reverts some commands.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        if (!UndoManager.Undo()) AddMessage(args.Context, "Nothing to undo.");
      }, true, true);
      new Terminal.ConsoleCommand("dev_redo", "Restores reverted commands.", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) return;
        if (!UndoManager.Redo()) AddMessage(args.Context, "Nothing to redo.");
      }, true, true);
    }
  }
}