using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Terminal.ConsoleCommand), nameof(Terminal.ConsoleCommand.RunAction))]
public class DisableCommandMessages
{
  static void Prefix()
  {
    PreventMessages.Enabled = Settings.DisableMessages;
  }
  static void Postfix()
  {
    PreventMessages.Enabled = false;
  }
}

[HarmonyPatch(typeof(MessageHud), nameof(MessageHud.ShowMessage))]
public class PreventMessages
{
  public static bool Enabled = false;
  static bool Prefix() => !Enabled;
}
