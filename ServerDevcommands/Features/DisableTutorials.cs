using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Raven), nameof(Raven.Spawn))]
public class DisableTutorials
{
  static bool Prefix() => !Settings.DisableTutorials;
}
