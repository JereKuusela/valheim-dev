using System.Collections.Generic;
using System.Linq;

namespace DEV {

  public partial class Commands {

    public static Stack<IEnumerable<ZDO>> Spawns = new Stack<IEnumerable<ZDO>>();
    public static Stack<string> History = new Stack<string>();
    public static int HistoryOffset = 0;

    public static void AddUndoSpawn() {
      new Terminal.ConsoleCommand("undo_spawn", "- Removes spawned objects.", delegate (Terminal.ConsoleEventArgs args) {
        if (Spawns.Count == 0)
          return;
        var toRemove = Spawns.Pop();
        foreach (var zdo in toRemove) {
          if (zdo != null && zdo.IsValid())
            ZDOMan.instance.DestroyZDO(zdo);
        }
        HistoryOffset++;
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Removing " + toRemove.Count() + " spawned objects", 0, null);
      }, true, false, true, false, false);
    }

    public static void AddRedoSpawn() {
      new Terminal.ConsoleCommand("redo_spawn", "- Restores undoed spawned objects.", delegate (Terminal.ConsoleEventArgs args) {
        if (Spawns.Count == 0)
          return;
        var toRemove = Spawns.Pop();
        foreach (var zdo in toRemove) {
          if (zdo != null && zdo.IsValid())
            ZDOMan.instance.DestroyZDO(zdo);
        }
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Removing " + toRemove.Count() + " spawned objects", 0, null);
      }, true, false, true, false, false);
    }
  }
}