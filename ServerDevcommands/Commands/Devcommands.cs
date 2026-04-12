using System.Runtime.Serialization.Json;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
///<summary>Replaces the default devcommands with an admin check.</summary>
public class DevcommandsCommand
{
  public static void Set(Terminal terminal, bool value)
  {
    terminal.AddString("Devcommands: " + value.ToString());
    Set(value);
  }
  public static void EnableAutoFeatures()
  {
    if (!Terminal.m_cheat)
      return;
    var player = Player.m_localPlayer;
    if (player && player.m_nview.IsValid())
    {
      if (Settings.AutoGodMode && Settings.IsEnabled(PermissionHash.God, true))
        player.SetGodMode(true);
      if (Settings.AutoGhostMode && Settings.IsEnabled(PermissionHash.Ghost, true))
        player.SetGhostMode(true);
      if (Settings.AutoFly && Settings.IsEnabled(PermissionHash.Fly, true))
      {
        player.m_debugFly = true;
        player.m_nview.GetZDO().Set(ZDOVars.s_debugFly, true);
      }
      if (Settings.AutoNoCost && Settings.IsEnabled(PermissionHash.NoCost, true))
        player.m_noPlacementCost = true;
      if (Settings.AutoDebugMode)
        Player.m_debugMode = true;
    }

    if (EnvMan.instance)
    {
      if (Settings.AutoTod != "" && PermissionApi.IsCommandAllowed("tod"))
      {
        EnvMan.instance.m_debugTimeOfDay = true;
        EnvMan.instance.m_debugTime = Mathf.Clamp01(Parse.Float(Settings.AutoTod));
      }
      if (Settings.AutoEnv != "" && PermissionApi.IsCommandAllowed("env"))
      {
        EnvMan.instance.m_debugEnv = Settings.AutoEnv;
      }
    }
  }
  public static void DisableAutoFeatures()
  {
    if (Terminal.m_cheat)
      return;
    var player = Player.m_localPlayer;
    if (player && player.m_nview.IsValid())
    {
      if (Settings.AutoDebugMode)
        Player.m_debugMode = false;
      if (Settings.AutoGodMode)
        player.SetGodMode(false);
      if (Settings.AutoGhostMode)
        player.SetGhostMode(false);
      if (Settings.AutoFly)
      {
        player.m_debugFly = false;
        player.m_nview.GetZDO().Set(ZDOVars.s_debugFly, false);
      }
      if (Settings.AutoNoCost) player.m_noPlacementCost = false;
    }
    if (EnvMan.instance)
    {
      if (Settings.AutoTod != "")
        EnvMan.instance.m_debugTimeOfDay = false;
      if (Settings.AutoEnv != "")
        EnvMan.instance.m_debugEnv = "";
    }
  }
  public static void Set(bool value)
  {
    if (Terminal.m_cheat != value)
      Gogan.LogEvent("Cheat", "CheatsEnabled", value.ToString(), 0L);

    Terminal.m_cheat = value;
    Console.instance.updateCommandList();
    Chat.instance.updateCommandList();
    DisableAutoFeatures();
    EnableAutoFeatures();
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
        Set(args.Context, true);
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

[HarmonyPatch(typeof(Terminal), nameof(Terminal.IsCheatsEnabled))]
public class IsCheatsEnabledWithoutServerCheck
{
  static bool Postfix(bool result) => result || Terminal.m_cheat || ZNet.instance?.IsDedicated() == true;
}
[HarmonyPatch(typeof(Terminal.ConsoleCommand), nameof(Terminal.ConsoleCommand.IsValid))]
public class IsValidWithoutServerCheck
{
  static bool Postfix(bool result) => result || Terminal.m_cheat || ZNet.instance?.IsDedicated() == true;
}
// Probably needed to provide autocomplete for the chat window.
[HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
public class AutoCompleteSecrets
{
  static void Postfix(Terminal __instance)
  {
    __instance.m_autoCompleteSecrets = true;
  }
}