using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Service;
using UnityEngine;

namespace DEV {
  [BepInPlugin("valheim.jerekuusela.dev", "DEV", "1.5.0.0")]
  public class DEV : BaseUnityPlugin {
    public static ManualLogSource Log;
    public void Awake() {
      Log = Logger;
      Harmony harmony = new Harmony("valheim.jerekuusela.dev");
      harmony.PatchAll();
      Admin.Instance = new DevAdmin();
      Settings.Init(Config);
    }
  }

  [HarmonyPatch(typeof(Terminal), "InitTerminal")]
  public class SetCommands {
    public static void Postfix() {
      new ConfigCommand();
      new StartEventCommand();
      new PosCommand();
      new UndoSpawnCommand();
      new RedoSpawnCommand();
      new SpawnLocationCommand();
      new SpawnObjectCommand();
      new ObjectCommand();
      new TerrainCommand();
      new DevCommandsCommand();
      new AliasCommand();
      CommandParameters.RegisterBaseGameFetchers();
      Settings.RegisterCommands();
    }
  }

  [HarmonyPatch(typeof(CharacterDrop), "GenerateDropList")]
  public class GenerateDropList {
    public static bool Prefix(ref List<KeyValuePair<GameObject, int>> __result) {
      if (Settings.NoDrops) {
        __result = new List<KeyValuePair<GameObject, int>>();
        return false;
      }
      return true;
    }
  }
}
