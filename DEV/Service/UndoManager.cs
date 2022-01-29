using System.Collections.Generic;
using System.Linq;

namespace Service {
  public interface UndoAction {
    void Undo();
    void Redo();
  }
  public class UndoManager {
    private static List<UndoAction> History = new List<UndoAction>();
    private static int Index = -1;
    private static bool Executing = false;
    public static int MaxSteps = 50;
    public static void Add(UndoAction action) {
      // During undo/redo more steps won't be added.
      if (Executing) return;
      if (History.Count > MaxSteps - 1)
        History = History.Skip(History.Count - MaxSteps + 1).ToList();
      if (Index < History.Count - 1)
        History = History.Take(Index + 1).ToList();
      History.Add(action);
      Index = History.Count - 1;
    }

    public static bool Undo() {
      if (Index < 0) return false;
      Executing = true;
      try {
        History[Index].Undo();
      } catch { }
      Index--;
      Executing = false;
      return true;
    }
    public static bool Redo() {
      if (Index < History.Count - 1) {
        Executing = true;
        Index++;
        try {
          History[Index].Redo();
        } catch { }
        Executing = false;
        return true;
      }
      return false;
    }
  }
}