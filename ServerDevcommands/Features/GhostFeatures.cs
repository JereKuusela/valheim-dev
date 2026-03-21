using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ServerDevcommands;

[HarmonyPatch]
public class GhostFeatures
{
  public static readonly int HashIgnoreSleep = "DEV_GhostIgnoreSleep".GetHashCode();
  public static readonly int HashGhost = "DEV_Ghost".GetHashCode();

  private static float? PreviousGhostY = null;
  private static bool? PreviousPublicReferencePosition = null;

  public static bool IsGhost(Player player) => player.InGhostMode() || player.m_nview?.GetZDO()?.GetBool(HashGhost) == true;

  private static List<Player> Players()
  {
    if (!Settings.GhostNoSpawns) return Player.s_players;
    return Player.s_players.Where(player => !IsGhost(player)).ToList();
  }

  private static IEnumerable<CodeInstruction> ReplacePlayers(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
         .MatchStartForward(
             new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(Player), nameof(Player.s_players))))
         .SetAndAdvance(
              OpCodes.Call, Transpilers.EmitDelegate(Players).operand)
         .InstructionEnumeration();
  }

  [HarmonyPatch(typeof(Game), nameof(Game.EverybodyIsTryingToSleep)), HarmonyPostfix]
  static bool EverybodyIsTryingToSleepPostfix(bool result)
  {
    if (result) return result;
    var zdos = ZNet.instance.GetAllCharacterZDOS();
    if (zdos.Count == 0) return false;
    var someoneSleeping = zdos.Any(zdo => zdo.GetBool(ZDOVars.s_inBed));
    if (!someoneSleeping) return false;
    return zdos.All(zdo => zdo.GetBool(ZDOVars.s_inBed) || zdo.GetBool(HashIgnoreSleep));
  }

  [HarmonyPatch(typeof(Player), nameof(Player.SetGhostMode)), HarmonyPostfix]
  static void SetGhostModePostfix(Player __instance)
  {
    if (__instance != Player.m_localPlayer) return;

    __instance.m_nview?.GetZDO()?.Set(HashGhost, __instance.InGhostMode());
    if (Settings.GhostIgnoreSleep)
      __instance.m_nview?.GetZDO()?.Set(HashIgnoreSleep, __instance.InGhostMode());
  }

  [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.SendZDOs)), HarmonyPrefix]
  static void SendZDOsPrefix()
  {
    if (!Player.m_localPlayer || !Player.m_localPlayer.m_nview) return;
    if (Player.m_localPlayer.InGhostMode() && Settings.GhostInvisibility)
    {
      var zdo = Player.m_localPlayer.m_nview.GetZDO();
      if (zdo == null) return;
      PreviousGhostY = zdo.m_position.y;
      zdo.m_position.y = 10000f;
      var seMan = Player.m_localPlayer.GetSEMan();
      if (seMan != null)
      {
        foreach (var se in seMan.GetStatusEffects()) se.RemoveStartEffects();
      }
    }
  }

  [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.SendZDOs)), HarmonyPostfix]
  static void SendZDOsPostfix()
  {
    if (!Player.m_localPlayer || !Player.m_localPlayer.m_nview) return;
    if (PreviousGhostY.HasValue)
    {
      Player.m_localPlayer.m_nview.GetZDO().m_position.y = PreviousGhostY.Value;
      PreviousGhostY = null;
    }
  }

  [HarmonyPatch(typeof(ZNet), nameof(ZNet.SendPeriodicData)), HarmonyPrefix]
  static void SendPeriodicDataPrefix(ZNet __instance)
  {
    if (!Player.m_localPlayer || !Player.m_localPlayer.m_nview) return;
    if (Player.m_localPlayer.InGhostMode() && Settings.GhostInvisibility)
    {
      PreviousPublicReferencePosition = __instance.m_publicReferencePosition;
      __instance.m_publicReferencePosition = false;
    }
  }

  [HarmonyPatch(typeof(ZNet), nameof(ZNet.SendPeriodicData)), HarmonyPostfix]
  static void SendPeriodicDataPostfix(ZNet __instance)
  {
    if (PreviousPublicReferencePosition.HasValue)
    {
      __instance.m_publicReferencePosition = PreviousPublicReferencePosition.Value;
      PreviousPublicReferencePosition = null;
    }
  }

  [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.GetPlayersInZone)), HarmonyPostfix]
  static void GetPlayersInZonePostfix(List<Player> players)
  {
    if (!Settings.GhostNoSpawns) return;
    var toRemove = players.Where(IsGhost).ToList();
    foreach (var player in toRemove)
      players.Remove(player);
  }

  [HarmonyPatch(typeof(Player), nameof(Player.GetClosestPlayer)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> GetClosestPlayerTranspiler(IEnumerable<CodeInstruction> instructions) => ReplacePlayers(instructions);

  [HarmonyPatch(typeof(Player), nameof(Player.IsPlayerInRange), typeof(Vector3), typeof(float)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> IsPlayerInRangeTranspiler(IEnumerable<CodeInstruction> instructions) => ReplacePlayers(instructions);

  [HarmonyPatch(typeof(Player), nameof(Player.IsPlayerInRange), typeof(Vector3), typeof(float), typeof(float)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> IsPlayerInRange2Transpiler(IEnumerable<CodeInstruction> instructions) => ReplacePlayers(instructions);
}