using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(ParticleMist), nameof(ParticleMist.Update))]
public class NoMist
{
  static bool Prefix()
  {
    var player = Player.m_localPlayer;
    var noMist = Settings.GodModeNoMist && player && player.InGodMode();
    return !noMist;
  }
}