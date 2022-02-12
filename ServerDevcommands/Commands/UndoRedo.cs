namespace ServerDevcommands {
  ///<summary>Commands for undo/redo.</summary>
  public class UndoRedoCommand {
    public UndoRedoCommand() {
      new Terminal.ConsoleCommand("undo", "Reverts some commands.", delegate (Terminal.ConsoleEventArgs args) {
        UndoManager.Undo(args.Context);
      });
      new Terminal.ConsoleCommand("redo", "Restores reverted commands.", delegate (Terminal.ConsoleEventArgs args) {
        UndoManager.Redo(args.Context);
      });
      AutoComplete.RegisterEmpty("undo");
      AutoComplete.RegisterEmpty("redo");
    }
  }
}
