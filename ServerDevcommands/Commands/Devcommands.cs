using HarmonyLib;
namespace ServerDevcommands;
///<summary>Replaces the default devcommands with an admin check.</summary>
public class DevcommandsCommand
{
  public static void Toggle(Terminal terminal)
  {
    var value = !Terminal.m_cheat;
    terminal?.AddString("Devcommands: " + value.ToString());
    Gogan.LogEvent("Cheat", "CheatsEnabled", value.ToString(), 0L);
    Set(value);
  }
  public static void Set(Terminal terminal, bool value)
  {
    terminal?.AddString("Devcommands: " + value.ToString());
    Gogan.LogEvent("Cheat", "CheatsEnabled", value.ToString(), 0L);
    Set(value);
  }
  public static void Set(bool value)
  {
    if (Terminal.m_cheat == value) return;
    Terminal.m_cheat = value;
    Console.instance.updateCommandList();
    Chat.instance.updateCommandList();
    var player = Player.m_localPlayer;
    if (Settings.AutoDebugMode)
      Player.m_debugMode = Terminal.m_cheat;
    if (player && Settings.AutoGodMode)
      player.SetGodMode(Terminal.m_cheat);
    if (player && Settings.AutoGhostMode)
      player.SetGhostMode(Terminal.m_cheat);
    if (player && Settings.AutoFly)
    {
      player.m_debugFly = Terminal.m_cheat;
      player.m_nview.GetZDO().Set("DebugFly", Terminal.m_cheat);
    }
    if (Settings.AutoNoCost && player)
      player.m_noPlacementCost = Terminal.m_cheat;
    if (value && Settings.AutoExecDevOn != "") Console.instance.TryRunCommand(Settings.AutoExecDevOn);
    if (!value && Settings.AutoExecDevOff != "") Console.instance.TryRunCommand(Settings.AutoExecDevOff);
  }

  public DevcommandsCommand()
  {
    new Terminal.ConsoleCommand("devcommands", "Toggles cheats", (args) =>
    {
      if (Terminal.m_cheat)
      {
        Set(args.Context, false);
      }
      else if (ZNet.instance && ZNet.instance.IsServer())
      {
        Toggle(args.Context);
      }
      else
      {
        args.Context.AddString("Authenticating for devcommands...");
        Admin.ManualCheck();
      }
    });
    AutoComplete.RegisterEmpty("devcommands");
  }
}
///<summary>Custom admin check to update devcommands.</summary>
public class DevCommandsAdmin : DefaultAdmin
{
  protected override void OnSuccess()
  {
    base.OnSuccess();
    DevcommandsCommand.Set(true);
    Console.instance.AddString("Authorized to use devcommands.");
  }
  protected override void OnFail()
  {
    base.OnFail();
    DevcommandsCommand.Set(false);
    Console.instance.AddString("Unauthorized to use devcommands.");
  }
  public override void AutomaticCheck()
  {
    if (!Settings.AutoDevcommands) return;
    base.AutomaticCheck();
  }
  public override void Reset()
  {
    base.Reset();
    DevcommandsCommand.Set(false);
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.IsCheatsEnabled))]
public class IsCheatsEnabledWithoutServerCheck
{
  static void Postfix(ref bool __result)
  {
    __result = Terminal.m_cheat || ZNet.instance?.IsDedicated() == true;
  }
}
[HarmonyPatch(typeof(Terminal.ConsoleCommand), nameof(Terminal.ConsoleCommand.IsValid))]
public class IsValidWithoutServerCheck
{
  static void Postfix(ref bool __result)
  {
    __result = __result || Terminal.m_cheat || ZNet.instance?.IsDedicated() == true;
  }
}
// Probably needed to provide autocomplete for the chat window.
[HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
public class AutoCompleteSecrets
{
  static void Postfix(ref bool ___m_autoCompleteSecrets)
  {
    ___m_autoCompleteSecrets = true;
  }
}
