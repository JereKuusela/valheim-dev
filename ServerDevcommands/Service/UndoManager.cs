using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace ServerDevcommands;
public interface UndoAction
{
  void Undo();
  void Redo();
  string UndoMessage();
  string RedoMessage();
}
public class UndoManager
{
  private static BindingFlags Binding = BindingFlags.Instance | BindingFlags.Public;
  private static List<object> History = new();
  private static int Index = -1;
  private static bool Executing = false;
  public static int MaxSteps = 50;
  public static void Add(UndoAction action)
  {
    Add((object)action);
  }
  ///<summary>Intended to be used with reflection.</summary>
  private static void Add(object action)
  {
    // During undo/redo more steps won't be added.
    if (Executing) return;
    if (History.Count > MaxSteps - 1)
      History = History.Skip(History.Count - MaxSteps + 1).ToList();
    if (Index < History.Count - 1)
      History = History.Take(Index + 1).ToList();
    History.Add(action);
    Index = History.Count - 1;
  }

  public static bool Undo(Terminal terminal)
  {
    if (Index < 0)
    {
      Helper.AddMessage(terminal, "Nothing to undo.");
      return false;
    }
    Executing = true;
    try
    {
      var obj = History[Index];
      obj.GetType().GetMethod("Undo", Binding).Invoke(obj, null);
      var message = obj.GetType().GetMethod("UndoMessage", Binding).Invoke(obj, null);
      Helper.AddMessage(terminal, (string)message);
    }
    catch (Exception e) { ServerDevcommands.Log.LogWarning(e); }
    Index--;
    Executing = false;
    return true;
  }
  public static bool Redo(Terminal terminal)
  {
    if (Index < History.Count - 1)
    {
      Executing = true;
      Index++;
      try
      {
        var obj = History[Index];
        obj.GetType().GetMethod("Redo", Binding).Invoke(obj, null);
        var message = obj.GetType().GetMethod("RedoMessage", Binding).Invoke(obj, null);
        Helper.AddMessage(terminal, (string)message);
      }
      catch (Exception e) { ServerDevcommands.Log.LogWarning(e); }
      Executing = false;
      return true;
    }
    Helper.AddMessage(terminal, "Nothing to redo.");
    return false;
  }
}
