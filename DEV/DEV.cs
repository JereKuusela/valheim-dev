using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace DEV {
  [BepInPlugin("valheim.jerekuusela.dev", "DEV", "1.8.0.0")]
  public class DEV : BaseUnityPlugin {
    public static ManualLogSource Log;
    public void Awake() {
      Log = Logger;
      Harmony harmony = new Harmony("valheim.jerekuusela.dev");
      harmony.PatchAll();
      Admin.Instance = new DevAdmin();
      Settings.Init(Config);
      Console.SetConsoleEnabled(true);
    }
  }

  [HarmonyPatch(typeof(Terminal), "InitTerminal")]
  public class SetCommands {
    public static void Postfix() {
      new DevCommandsCommand();
      new ConfigCommand();
      new StartEventCommand();
      new PosCommand();
      new AliasCommand();
      new SearchCommand();
      new UndoRedoCommand();
      DefaultAutoComplete.Register();
      Settings.RegisterCommands();
    }
  }
}
