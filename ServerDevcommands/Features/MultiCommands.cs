namespace ServerDevcommands;
///<summary>Code related to handling multiple commands per line.</summary>
public static class MultiCommands
{
  private static string[]? PreviousCommands = null;
  private static int CurrentCommand = -1;
  private static int DiscardCaretDelta = 0;
  ///<summary>Discarding previous commands makes handling much simpler.</summary>
  public static void DiscardPreviousCommands(UnityEngine.UI.InputField input)
  {
    if (!Settings.MultiCommand) return;
    PreviousCommands = input.text.Split(';');
    DiscardCaretDelta = input.caretPosition;
    for (CurrentCommand = 0; CurrentCommand < PreviousCommands.Length; CurrentCommand++)
    {
      var command = PreviousCommands[CurrentCommand];
      if (input.caretPosition > command.Length)
        input.caretPosition -= command.Length;
      else break;
      // ';" character.
      input.caretPosition--;
    }
    DiscardCaretDelta -= input.caretPosition;
    if (CurrentCommand >= PreviousCommands.Length)
    {
      PreviousCommands = null;
      CurrentCommand = -1;
    }
    else
      input.text = PreviousCommands[CurrentCommand];
  }
  public static void RestorePreviousCommands(UnityEngine.UI.InputField input)
  {
    if (PreviousCommands == null) return;
    PreviousCommands[CurrentCommand] = input.text;
    input.text = string.Join(";", PreviousCommands);
    if (DiscardCaretDelta != 0) input.caretPosition += DiscardCaretDelta;
    PreviousCommands = null;
    CurrentCommand = -1;
  }
  public static bool IsMulti(string text) => Settings.MultiCommand && text.Contains(";");
  public static string[] Split(string text) => text.Split(';');
}
