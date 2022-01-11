using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Service;

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
      new UndoSpawnCommand();
      new RedoSpawnCommand();
      new StartEventCommand();
      new SpawnLocationCommand();
      new SpawnObjectCommand();
      new ManipulateCommand();
      new ChangeEquipmentCommand();
      new TerrainCommand();
      new PosCommand();
    }
  }
}
