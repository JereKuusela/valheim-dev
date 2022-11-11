using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(MessageHud), nameof(MessageHud.QueueUnlockMsg))]
public class DisableUnlockMessages
{
  static bool Prefix()
  {
    return !Settings.DisableUnlockMessages;
  }
}
