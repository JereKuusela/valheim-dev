using HarmonyLib;

namespace ServerDevcommands {
  [HarmonyPatch(typeof(Raven), nameof(Raven.Spawn))]
  public class DisableTutorials {
    public static bool Prefix() => !Settings.DisableTutorials;
  }
}
