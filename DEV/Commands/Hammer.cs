using HarmonyLib;
using UnityEngine;

namespace DEV {
  public class HammerCommand : BaseCommands {
    private static GameObject Override = null;
    public static void SetHammerOverride(Player player, GameObject obj) {
      if (player != Player.m_localPlayer) return;
      var piece = obj.GetComponent<Piece>();
      if (!piece)
        piece = obj.AddComponent<Piece>();
      piece.m_allowedInDungeons = true;
      piece.m_clipEverything = true;
      piece.m_canBeRemoved = true;

      Override = obj;
      player.SetupPlacementGhost();
    }
    public static void RemoveHammerOverride(Player player) {
      if (player != Player.m_localPlayer) return;
      Override = null;
      player.SetupPlacementGhost();
    }

    public static Piece GetHammerOverride() {
      if (Override) return Override.GetComponent<Piece>();
      return null;
    }
    public HammerCommand() {
      new Terminal.ConsoleCommand("hammer", "[name] - Adds an object to the hammer placement (hovered object by default).", delegate (Terminal.ConsoleEventArgs args) {
        if (!Player.m_localPlayer) {
          return;
        }
        if (args.Length > 1) {
          string name = args[1];
          var prefab = GetPrefab(name);
          if (prefab)
            SetHammerOverride(Player.m_localPlayer, prefab);

        } else {
          var view = GetHovered(args);
          if (!view) return;
          SetHammerOverride(Player.m_localPlayer, view.gameObject);
        }
      }, true, false, true, false, false, () => ZNetScene.instance.GetPrefabNames());
    }
  }

  ///<summary>Overrides the piece selection.</summary>
  [HarmonyPatch(typeof(PieceTable), "GetSelectedPiece")]
  public class GetSelectedPiece {
    public static bool Prefix(ref Piece __result) {
      if (HammerCommand.GetHammerOverride()) {
        __result = HammerCommand.GetHammerOverride();
        return false;
      }
      return true;
    }
  }

  ///<summary>Enables removing the override by selecting a piece normally.</summary>
  [HarmonyPatch(typeof(Player), "SetSelectedPiece")]
  public class SetSelectedPiece {
    public static void Prefix(Player __instance) {
      HammerCommand.RemoveHammerOverride(__instance);
    }
  }

  ///<summary>Disables problematic scripts.</summary>
  [HarmonyPatch(typeof(Player), "SetupPlacementGhost")]
  public class SetupPlacementGhost {
    public static void Postfix(Player __instance) {
      var obj = __instance.m_placementGhost;
      if (!obj) return;
      var baseAI = obj.GetComponent<BaseAI>();
      var monsterAI = obj.GetComponent<MonsterAI>();
      var humanoid = obj.GetComponent<Humanoid>();
      var character = obj.GetComponent<Character>();
      if (baseAI) baseAI.enabled = false;
      if (monsterAI) monsterAI.enabled = false;
      if (humanoid) humanoid.enabled = false;
      if (character) character.enabled = false;
    }
  }
  ///<summary>Prevents script error by creature awake functions trying to do ZNetView stuff.</summary>
  [HarmonyPatch(typeof(BaseAI), "Awake")]
  public class BaseAIAwake {
    public static bool Prefix() => !ZNetView.m_forceDisableInit;
  }
  ///<summary>Prevents script error by creature awake functions trying to do ZNetView stuff.</summary>
  [HarmonyPatch(typeof(MonsterAI), "Awake")]
  public class MonsterAIAwake {
    public static bool Prefix() => !ZNetView.m_forceDisableInit;
  }
}