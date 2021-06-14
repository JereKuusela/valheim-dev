using BepInEx;
using HarmonyLib;

namespace DEV
{
  [BepInPlugin("valheim.jerekuusela.dev", "DEV", "1.0.0.0")]
  public class ESP : BaseUnityPlugin
  {
    void Awake()
    {
      var harmony = new Harmony("valheim.jerekuusela.dev");
      harmony.PatchAll();
    }
  }
  public class Cheats
  {
    public static bool CheckingAdmin = false;
    public static bool enabled = false;
    public static bool Enabled
    {
      get => enabled;
      set
      {
        if (value == enabled) return;
        enabled = value;
        Console.instance.Print("Dev commands: " + value.ToString());
        Console.instance.Print("WARNING: using any dev commands is not recommended and is done on your own risk.");
        Gogan.LogEvent("Cheat", "CheatsEnabled", value.ToString(), 0L);
      }
    }
  }
  [HarmonyPatch(typeof(ZNet), "RPC_RemotePrint")]
  public class ZNet_RPC_RemotePrint
  {
    public static bool Prefix(string text)
    {
      if (Cheats.CheckingAdmin)
      {
        Cheats.CheckingAdmin = false;
        if (text == "Banning user admintest")
          Cheats.Enabled = true;
        else
          Console.instance.Print("Unauthorized to use devcommands.");
        return false;
      }
      return true;
    }
  }

  [HarmonyPatch(typeof(Console), "InputText")]
  public class Console_InputText_CheckAdmin
  {
    public static bool Prefix(Console __instance)
    {
      var text = __instance.m_input.text;
      var array = text.Split(' ');
      if (array[0] != "devcommands") return true;
      if (Cheats.Enabled)
      {
        Cheats.Enabled = false;
      }
      else
      {
        if (ZNet.instance && ZNet.instance.IsServer())
        {
          Cheats.Enabled = true;
        }
        else if (ZNet.instance)
        {
          Cheats.CheckingAdmin = true;
          ZNet.instance.Ban("admintest");
        }
      }
      return false;
    }
  }

  // Cheats must be disabled when joining servers (so that locally enabling doesn't work).
  [HarmonyPatch(typeof(ZNet), "Start")]
  public class ZNet_Start
  {
    public static void Postfix()
    {
      Cheats.enabled = false;
    }
  }

  [HarmonyPatch(typeof(Console), "IsConsoleEnabled")]
  public class Console_IsConsoleEnabled
  {
    public static bool Prefix(ref bool __result)
    {
      __result = true;
      return false;
    }
  }
  [HarmonyPatch(typeof(Console), "IsCheatsEnabled")]
  public class Console_IsCheatsEnabled
  {
    public static bool Prefix(ref bool __result)
    {
      __result = Cheats.Enabled;
      return false;
    }
  }
}
