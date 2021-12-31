using BepInEx;
using HarmonyLib;
using Service;

namespace DEV {
  [BepInPlugin("valheim.jerekuusela.dev", "DEV", "1.5.0.0")]
  [BepInDependency("m3to.mods.GizmoReloaded", BepInDependency.DependencyFlags.SoftDependency)]
  public class DEV : BaseUnityPlugin {
    public void Awake() {
      Harmony harmony = new Harmony("valheim.jerekuusela.dev");
      harmony.PatchAll();
      Admin.Instance = new DevAdmin();
    }
  }

  [HarmonyPatch(typeof(Terminal), "InitTerminal")]
  public class SetCommands {
    public static void Postfix() {
      new UndoSpawnCommand();
      new RedoSpawnCommand();
      new HammerCommand();
      new StartEventCommand();
      new SpawnLocationCommand();
      new SpawnObjectCommand();
      new ManipulateCommand();
      new ChangeEquipmentCommand();
    }
  }
}
