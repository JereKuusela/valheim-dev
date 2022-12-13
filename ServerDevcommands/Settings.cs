using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;
namespace ServerDevcommands;
#nullable disable
public static class Settings
{
  public static bool Cheats => (ZNet.instance && ZNet.instance.IsServer()) || (Console.instance.IsCheatsEnabled() && Admin.Enabled);
  public static ConfigEntry<bool> configMapCoordinates;
  public static bool MapCoordinates => Cheats && configMapCoordinates.Value;
  public static ConfigEntry<bool> configMiniMapCoordinates;
  public static bool MiniMapCoordinates => Cheats && configMiniMapCoordinates.Value;
  public static ConfigEntry<bool> configShowPrivatePlayers;
  public static bool ShowPrivatePlayers => Cheats && configShowPrivatePlayers.Value;
  public static ConfigEntry<bool> configAutoDevcommands;
  public static bool AutoDevcommands => configAutoDevcommands.Value;
  public static ConfigEntry<bool> configDebugModeFastTeleport;
  public static bool DebugModeFastTeleport => configDebugModeFastTeleport.Value;
  public static ConfigEntry<bool> configAutoDebugMode;
  public static bool AutoDebugMode => configAutoDebugMode.Value;
  public static ConfigEntry<bool> configAutoGodMode;
  public static bool AutoGodMode => configAutoGodMode.Value;
  public static ConfigEntry<bool> configDisableNoMap;
  public static bool DisableNoMap => Cheats && configDisableNoMap.Value;
  public static ConfigEntry<bool> configAutoGhostMode;
  public static bool AutoGhostMode => configAutoGhostMode.Value;
  public static ConfigEntry<bool> configAutomaticItemPickUp;
  public static bool AutomaticItemPickUp => configAutomaticItemPickUp.Value;
  public static ConfigEntry<bool> configAutoNoCost;
  public static bool AutoNoCost => configAutoNoCost.Value;
  public static ConfigEntry<bool> configDisableEvents;
  public static bool DisableEvents => Cheats && configDisableEvents.Value;
  public static ConfigEntry<bool> configDisableUnlockMessages;
  public static bool DisableUnlockMessages => configDisableUnlockMessages.Value;
  public static ConfigEntry<bool> configDisableDebugModeKeys;
  public static bool DisableDebugModeKeys => configDisableDebugModeKeys.Value;
  public static ConfigEntry<bool> configDebugConsole;
  public static bool DebugConsole => configDebugConsole.Value;
  public static ConfigEntry<bool> configAutoFly;
  public static bool AutoFly => configAutoFly.Value;
  public static ConfigEntry<bool> configDisableMessages;
  public static bool DisableMessages => configDisableMessages.Value;
  public static ConfigEntry<bool> configGodModeNoWeightLimit;
  public static bool GodModeNoWeightLimit => Cheats && configGodModeNoWeightLimit.Value;
  public static ConfigEntry<bool> configGodModeNoStamina;
  public static bool GodModeNoStamina => Cheats && configGodModeNoStamina.Value;
  public static ConfigEntry<bool> configGodModeNoEitr;
  public static bool GodModeNoEitr => Cheats && configGodModeNoEitr.Value;
  public static ConfigEntry<bool> configGodModeNoUsage;
  public static bool GodModeNoUsage => Cheats && configGodModeNoUsage.Value;
  public static ConfigEntry<bool> configGodModeAlwaysDodge;
  public static bool GodModeAlwaysDodge => Cheats && configGodModeAlwaysDodge.Value;
  public static ConfigEntry<bool> configGodModeAlwaysParry;
  public static bool GodModeAlwaysParry => Cheats && configGodModeAlwaysParry.Value;
  public static ConfigEntry<bool> configGodModeNoStagger;
  public static bool GodModeNoStagger => Cheats && configGodModeNoStagger.Value;
  public static ConfigEntry<bool> configHideShoutPings;
  public static bool HideShoutPings => Cheats && configHideShoutPings.Value;
  public static ConfigEntry<bool> configGodModeNoEdgeOfWorld;
  public static bool GodModeNoEdgeOfWorld => Cheats && configGodModeNoEdgeOfWorld.Value;
  public static ConfigEntry<bool> configDisableStartShout;
  public static bool DisableStartShout => configDisableStartShout.Value;
  public static ConfigEntry<bool> configDisableTutorials;
  public static bool DisableTutorials => configDisableTutorials.Value;
  public static ConfigEntry<bool> configAccessPrivateChests;
  public static bool AccessPrivateChests => Cheats && configAccessPrivateChests.Value;
  public static ConfigEntry<bool> configAccessWardedAreas;
  public static bool AccessWardedAreas => Cheats && configAccessWardedAreas.Value;
  public static ConfigEntry<bool> configFlyNoClip;
  public static bool FlyNoClip => Cheats && configFlyNoClip.Value;
  public static ConfigEntry<bool> configNoClipClearEnvironment;
  public static bool NoClipClearEnvironment => configNoClipClearEnvironment.Value;
  public static ConfigEntry<bool> configGodModeNoKnockback;
  public static bool GodModeNoKnockback => Cheats && configGodModeNoKnockback.Value;
  public static ConfigEntry<bool> configGodModeNoMist;
  public static bool GodModeNoMist => Cheats && configGodModeNoMist.Value;
  public static ConfigEntry<bool> configAliasing;
  public static bool Aliasing => configAliasing.Value;
  public static ConfigEntry<bool> configDisableParameterWarnings;
  public static bool DisableParameterWarnings => configDisableParameterWarnings.Value;
  public static ConfigEntry<string> configSubstitution;
  public static string Substitution => configSubstitution.Value;
  public static ConfigEntry<bool> configImprovedAutoComplete;
  public static bool ImprovedAutoComplete => configImprovedAutoComplete.Value;
  public static ConfigEntry<bool> configMultiCommand;
  public static bool MultiCommand => configMultiCommand.Value;
  public static ConfigEntry<bool> configGhostInvisibility;
  public static bool GhostInvisibility => Cheats && configGhostInvisibility.Value;
  public static ConfigEntry<string> configFlyUpKeys;
  public static string[] FlyUpKeys = new string[0];
  public static ConfigEntry<string> configFlyDownKeys;
  public static string[] FlyDownKeys = new string[0];
  public static ConfigEntry<bool> configNoDrops;
  public static bool NoDrops => Cheats && configNoDrops.Value;
  public static ConfigEntry<bool> configNoClipView;
  public static bool NoClipView => Cheats && configNoClipView.Value;
  public static ConfigEntry<string> configCommandAliases;
  public static ConfigEntry<KeyboardShortcut> configMouseWheelBindKey;
  public static KeyCode MouseWheelBindKey => configMouseWheelBindKey.Value.MainKey;
  public static ConfigEntry<string> configAutoExecBoot;
  public static string AutoExecBoot => configAutoExecBoot.Value;
  public static ConfigEntry<string> configAutoExec;
  public static string AutoExec => configAutoExec.Value;
  public static ConfigEntry<string> configAutoExecDevOn;
  public static string AutoExecDevOn => configAutoExecDevOn.Value;
  public static ConfigEntry<string> configAutoExecDevOff;
  public static string AutoExecDevOff => configAutoExecDevOff.Value;
  public static ConfigEntry<bool> configCommandDescriptions;
  public static bool CommandDescriptions => configCommandDescriptions.Value;
  public static ConfigEntry<bool> configBestCommandMatch;
  public static bool BestCommandMatch => configBestCommandMatch.Value;
  private static Dictionary<string, string> Aliases = new();
  public static string[] AliasKeys = new string[0];

  private static void ParseAliases(string value)
  {
    Aliases = value.Split('¤').Select(str => str.Split(' ')).ToDictionary(split => split[0], split => string.Join(" ", split.Skip(1)));
    Aliases = Aliases.Where(kvp => kvp.Key != "").ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    AliasKeys = Aliases.Keys.OrderBy(key => key).ToArray();
  }
  public static string GetAliasValue(string key) => Aliases.ContainsKey(key) ? Aliases[key] : "_";
  public static void RegisterCommands()
  {
    foreach (var alias in Aliases)
    {
      AliasCommand.AddCommand(alias.Key, alias.Value);
    }
    DisableCommands.UpdateCommands(configRootUsers.Value, configDisabledCommands.Value);
  }

  private static void SaveAliases()
  {
    var value = string.Join("¤", Aliases.Select(kvp => kvp.Key + " " + kvp.Value));
    configCommandAliases.Value = value;
  }

  public static void AddAlias(string alias, string value)
  {
    if (value == "")
    {
      RemoveAlias(alias);
    }
    else
    {
      Aliases[alias] = value;
      SaveAliases();
    }
  }

  public static void AddAlias(Dictionary<string, string> dict)
  {
    Aliases = dict;
    SaveAliases();
  }
  public static void RemoveAlias(string alias)
  {
    if (!Aliases.ContainsKey(alias)) return;
    Aliases.Remove(alias);
    SaveAliases();
  }

  private static HashSet<string> ParseList(string value) => Parse.Split(value).Select(s => s.ToLower()).ToHashSet();
  public static ConfigEntry<string> configServerCommands;
  public static HashSet<string> ServerCommands => ParseList(configServerCommands.Value);
  public static bool IsServerCommand(string command) => ServerCommands.Contains(command.ToLower());
  public static ConfigEntry<string> configDisabledCommands;
  public static ConfigEntry<string> configRootUsers;
  public static ConfigEntry<string> configDisabledGlobalKeys;
  public static ConfigEntry<string> configUndoLimit;
  public static int UndoLimit => Parse.Int(configUndoLimit.Value, 50);
  public static HashSet<string> DisabledGlobalKeys => ParseList(configDisabledGlobalKeys.Value);
  public static bool IsGlobalKeyDisabled(string key) => DisabledGlobalKeys.Contains(key.ToLower());
  public static void Init(ConfigFile config)
  {
    var section = "1. General";
    configGhostInvisibility = config.Bind(section, "Invisible to other players with ghost mode", false, "");
    configNoDrops = config.Bind(section, "No creature drops", false, "Disables drops from creatures (if you control the zone), intended to fix high star enemies crashing the game.");
    configNoClipView = config.Bind(section, "No clip view", false, "Removes collision check for the camera.");
    configAutoDebugMode = config.Bind(section, "Automatic debug mode", false, "Automatically enables debug mode when enabling devcommands.");
    configAutoFly = config.Bind(section, "Automatic fly mode", false, "Automatically enables fly mode when enabling devcommands.");
    configAutoNoCost = config.Bind(section, "Automatic no cost mode", false, "Automatically enables no cost mode when enabling devcommands.");
    configAutoGodMode = config.Bind(section, "Automatic god mode", false, "Automatically enables god mode when enabling devcommands.");
    configAutoGhostMode = config.Bind(section, "Automatic ghost mode", false, "Automatically enables ghost mode when enabling devcommands.");
    configAutomaticItemPickUp = config.Bind(section, "Automatic item pick up", true, "Sets the default value for the automatic item pick up feature.");
    configAutomaticItemPickUp.SettingChanged += (s, e) =>
    {
      if (Player.m_localPlayer) Player.m_localPlayer.m_enableAutoPickup = AutomaticItemPickUp;
    };
    configAutoDevcommands = config.Bind(section, "Automatic devcommands", true, "Automatically enables devcommands when joining servers.");
    configDebugModeFastTeleport = config.Bind(section, "Debug mode fast teleport", true, "All teleporting is much faster with the debug mode.");
    configDisableNoMap = config.Bind(section, "Disable no map", false, "Disables no map having effect.");
    configGodModeNoStamina = config.Bind(section, "No stamina usage with god mode", true, "");
    configGodModeNoEitr = config.Bind(section, "No eitr usage with god mode", true, "");
    configGodModeNoUsage = config.Bind(section, "No item usage with god mode", true, "");
    configGodModeNoWeightLimit = config.Bind(section, "No weight limit with god mode", false, "");
    configGodModeAlwaysDodge = config.Bind(section, "Always dodge with god mode", false, "");
    configGodModeAlwaysParry = config.Bind(section, "Always parry with god mode (when not blocking)", false, "");
    configGodModeNoStagger = config.Bind(section, "No staggering with god mode", true, "");
    configHideShoutPings = config.Bind(section, "Hide shout pings", false, "Forces shout pings at the world center.");
    configGodModeNoEdgeOfWorld = config.Bind(section, "No edge of world pull with god mode", true, "");
    configDisableStartShout = config.Bind(section, "Disable start shout", false, "Removes the initial shout message when joining the server.");
    configDisableTutorials = config.Bind(section, "Disable tutorials", false, "Prevents the raven from appearing.");
    configAccessPrivateChests = config.Bind(section, "Access private chests", true, "Allows opening private chests.");
    configAccessWardedAreas = config.Bind(section, "Access warded areas", true, "Allows accessing warded areas.");
    configFlyNoClip = config.Bind(section, "No clip with fly mode", false, "");
    configNoClipClearEnvironment = config.Bind(section, "No clip clears forced environments", true, "Removes any forced environments when the noclip is enabled. This disables any dark dungeon environments and prevents them from staying on when exiting the dungeon.");
    configGodModeNoKnockback = config.Bind(section, "No knockback with god mode", true, "");
    configGodModeNoMist = config.Bind(section, "No Mistlands mist with god mode", false, "");
    configMapCoordinates = config.Bind(section, "Show map coordinates", true, "The map shows coordinates on hover.");
    configMiniMapCoordinates = config.Bind(section, "Show minimap coordinates", false, "The minimap shows player coordinates.");
    configShowPrivatePlayers = config.Bind(section, "Show private players", false, "The map shows private players.");
    configDisableEvents = config.Bind(section, "Disable random events", false, "Disables random events (server side setting).");
    configDisableUnlockMessages = config.Bind(section, "Disable unlock messages", false, "Disables messages about new pieces and items.");
    configDisableDebugModeKeys = config.Bind(section, "Disable debug mode keys", false, "Removes debug mode key bindings for killall, removedrops, fly and no cost.");
    configDisabledGlobalKeys = config.Bind(section, "Disabled global keys", "", "Global keys separated by , that won't be set (server side setting).");
    configDisabledGlobalKeys.SettingChanged += (s, e) => DisableGlobalKeys.RemoveDisabled();
    configUndoLimit = config.Bind(section, "Max undo steps", "50", "How many undo actions are stored.");
    configUndoLimit.SettingChanged += (s, e) => UndoManager.MaxSteps = UndoLimit;
    UndoManager.MaxSteps = UndoLimit;
    section = "2. Console";
    configBestCommandMatch = config.Bind(section, "Best command match", true, "Executes only the commands with the most modifiers keys pressed.");
    configDisableMessages = config.Bind(section, "Disable messages", false, "Prevents messages from commands.");
    configServerCommands = config.Bind(section, "Server side commands", "randomevent,stopevent,genloc,sleep,skiptime", "Command names separated by , that should be executed server side.");
    configMouseWheelBindKey = config.Bind(section, "Mouse wheel bind key", new KeyboardShortcut(KeyCode.None), "The simulated key code when scrolling the wheel.");
    configAutoExecBoot = config.Bind(section, "Auto exec boot", "", "Executes the given command when starting the game.");
    configAutoExecDevOn = config.Bind(section, "Auto exec dev on", "", "Executes the given command when enabling devcommands.");
    configAutoExecDevOff = config.Bind(section, "Auto exec dev off", "", "Executes the given command when disabling devcommands.");
    configAutoExec = config.Bind(section, "Auto exec", "", "Executes the given command when joining a server (before admin is checked).");
    configCommandDescriptions = config.Bind(section, "Command descriptions", true, "Shows command descriptions as autocomplete.");
    configAliasing = config.Bind(section, "Alias system", true, "Enables the command aliasing system (allows creating new commands).");
    configImprovedAutoComplete = config.Bind(section, "Improved autocomplete", true, "Enables parameter info or options for every parameter.");
    configCommandAliases = config.Bind(section, "Command aliases", "", "Internal data for aliases.");
    configMultiCommand = config.Bind(section, "Multiple commands per line", true, "Enables multiple commands when separated with ;.");
    configSubstitution = config.Bind(section, "Substitution", "$$", "Enables the command parameter substitution system (substitution gets replaced with the next free parameter).");
    configDebugConsole = config.Bind(section, "Debug console", false, "Extra debug information about aliasing.");
    configDisableParameterWarnings = config.Bind(section, "Disable parameter warnings", false, "Removes warning texts from some command parameter descriptions.");
    configCommandAliases.SettingChanged += (s, e) => ParseAliases(configCommandAliases.Value);
    configRootUsers = config.Bind(section, "Root users", "", "Steam IDs separated by , that can execute blacklisted commands. Server side setting.");
    configRootUsers.SettingChanged += (s, e) =>
    {
      DisableCommands.UpdateCommands(configRootUsers.Value, configDisabledCommands.Value);
      RootUsers.Update();
    };
    configDisabledCommands = config.Bind(section, "Disabled commands", "dev_config disable_command", "Command names separated by , that can't be executed.");
    configDisabledCommands.SettingChanged += (s, e) => DisableCommands.UpdateCommands(configRootUsers.Value, configDisabledCommands.Value);
    configFlyUpKeys = config.Bind(section, "Key for fly up", "Space", "Key codes separated by ,");
    configFlyUpKeys.SettingChanged += (s, e) => FlyUpKeys = Parse.Split(configFlyUpKeys.Value);
    FlyUpKeys = Parse.Split(configFlyUpKeys.Value);
    configFlyDownKeys = config.Bind(section, "Key for fly down", "LeftControl", "Key codes separated by ,");
    configFlyDownKeys.SettingChanged += (s, e) => FlyDownKeys = Parse.Split(configFlyDownKeys.Value);
    FlyDownKeys = Parse.Split(configFlyDownKeys.Value);
    ParseAliases(configCommandAliases.Value);
  }


  public static List<string> Options = new() {
    "access_private_chests",
    "access_warded_areas",
    "map_coordinates",
    "private_players",
    "auto_devcommands",
    "auto_debugmode",
    "auto_fly",
    "god_no_stagger",
    "auto_nocost",
    "auto_ghost",
    "auto_god",
    "debug_console",
    "no_drops",
    "aliasing",
    "god_no_stamina",
    "substitution",
    "improved_autocomplete",
    "disable_events",
    "disable_warnings",
    "multiple_commands",
    "god_no_knockback",
    "ghost_invibisility",
    "auto_exec_dev_on",
    "auto_exec_dev_off",
    "auto_exec_boot",
    "auto_exec",
    "command_descriptions",
    "server_commands",
    "fly_no_clip",
    "disable_command",
    "minimap_coordinates",
    "disable_global_key",
    "disable_debug_mode_keys",
    "god_always_parry",
    "god_always_dodge",
    "fly_up_key",
    "fly_down_key",
    "disable_start_shout",
    "mouse_wheel_bind_key",
    "disable_tutorials",
    "god_no_weight_limit",
    "automatic_item_pick_up",
    "disable_messages",
    "god_no_edge",
    "no_clip_clear_environment",
    "max_undo_steps",
    "best_command_match",
    "debug_fast_teleport",
    "disable_no_map",
    "hide_shout_pings",
    "disable_unlock_messages",
    "no_clip_view",
    "god_no_eitr",
    "god_no_item"
  };
  private static string State(bool value) => value ? "enabled" : "disabled";
  private static string Flag(bool value) => value ? "Removed" : "Added";

  private static HashSet<string> Truthies = new() {
    "1",
    "true",
    "yes",
    "on"
  };
  private static bool IsTruthy(string value) => Truthies.Contains(value);
  private static HashSet<string> Falsies = new() {
    "0",
    "false",
    "no",
    "off"
  };
  private static bool IsFalsy(string value) => Falsies.Contains(value);
  private static void Toggle(Terminal context, ConfigEntry<bool> setting, string name, string value, bool reverse = false)
  {
    if (value == "") setting.Value = !setting.Value;
    else if (IsTruthy(value)) setting.Value = true;
    else if (IsFalsy(value)) setting.Value = false;
    Helper.AddMessage(context, $"{name} {State(reverse ? !setting.Value : setting.Value)}.");
  }
  private static void ToggleFlag(Terminal context, ConfigEntry<string> setting, string name, string value)
  {
    if (value == "")
    {
      Helper.AddMessage(context, $"{name}: {setting.Value}.");
      return;
    }
    var list = ParseList(setting.Value);
    var newList = ParseList(value);
    foreach (var flag in newList)
    {
      var remove = list.Contains(flag);
      if (remove) list.Remove(flag);
      else list.Add(flag);
      setting.Value = string.Join(",", list);
      Helper.AddMessage(context, $"{name}: {Flag(remove)} {flag}.");
    }
  }
  private static void SetValue(Terminal context, ConfigEntry<string> setting, string name, string value)
  {
    if (value == "")
    {
      Helper.AddMessage(context, $"{name}: {setting.Value}.");
      return;
    }
    setting.Value = value;
    Helper.AddMessage(context, $"{name} set to {value}.");
  }
  private static void SetKey(Terminal context, ConfigEntry<KeyboardShortcut> setting, string name, string value)
  {
    if (value == "")
    {
      Helper.AddMessage(context, $"{name}: {setting.Value}.");
      return;
    }
    if (!Enum.TryParse<KeyCode>(value, true, out var keyCode))
      throw new InvalidOperationException("'" + value + "' is not a valid UnityEngine.KeyCode.");
    setting.Value = new(keyCode);
    Helper.AddMessage(context, $"{name} set to {value}.");
  }
  public static void UpdateValue(Terminal context, string key, string value)
  {
    if (key == "no_clip_view") Toggle(context, configNoClipView, key, value);
    if (key == "fly_up_key") SetValue(context, configFlyUpKeys, key, value);
    if (key == "fly_down_key") SetValue(context, configFlyDownKeys, key, value);
    if (key == "max_undo_steps") SetValue(context, configUndoLimit, key, value);
    if (key == "auto_exec_dev_on") SetValue(context, configAutoExecDevOn, key, value);
    if (key == "auto_exec_dev_off") SetValue(context, configAutoExecDevOff, key, value);
    if (key == "auto_exec_boot") SetValue(context, configAutoExecBoot, key, value);
    if (key == "auto_exec") SetValue(context, configAutoExec, key, value);
    if (key == "substitution") SetValue(context, configSubstitution, key, value);
    if (key == "mouse_wheel_bind_key") SetKey(context, configMouseWheelBindKey, "Mouse wheel bind key", value);
    if (key == "debug_fast_teleport") Toggle(context, configDebugModeFastTeleport, key, value);
    if (key == "best_command_match") Toggle(context, configBestCommandMatch, key, value);
    if (key == "access_private_chests") Toggle(context, configAccessPrivateChests, key, value);
    if (key == "access_warded_areas") Toggle(context, configAccessWardedAreas, key, value);
    if (key == "no_clip_clear_environment") Toggle(context, configNoClipClearEnvironment, key, value);
    if (key == "disable_messages") Toggle(context, configDisableMessages, "Command messages", value, true);
    if (key == "automatic_item_pick_up") Toggle(context, configAutomaticItemPickUp, "Automatic item pick up", value);
    if (key == "command_descriptions") Toggle(context, configCommandDescriptions, "Command descriptions", value);
    if (key == "map_coordinates") Toggle(context, configMapCoordinates, "Map coordinates", value);
    if (key == "minimap_coordinates") Toggle(context, configMiniMapCoordinates, "Minimap coordinates", value);
    if (key == "private_players") Toggle(context, configShowPrivatePlayers, "Private players", value);
    if (key == "auto_devcommands") Toggle(context, configAutoDevcommands, "Automatic devcommands", value);
    if (key == "auto_debugmode") Toggle(context, configAutoDebugMode, "Automatic debug mode", value);
    if (key == "auto_fly") Toggle(context, configAutoFly, "Automatic fly", value);
    if (key == "auto_nocost") Toggle(context, configAutoNoCost, "Automatic no cost", value);
    if (key == "auto_god") Toggle(context, configAutoGodMode, "Automatic god mode", value);
    if (key == "auto_ghost") Toggle(context, configAutoGhostMode, "Automatic ghost mode", value);
    if (key == "debug_console") Toggle(context, configDebugConsole, "Debug console", value);
    if (key == "no_drops") Toggle(context, configNoDrops, "Creature drops", value, true);
    if (key == "aliasing") Toggle(context, configAliasing, "Command aliasing", value);
    if (key == "improved_autocomplete") Toggle(context, configImprovedAutoComplete, "Improved autocomplete", value);
    if (key == "disable_unlock_messages") Toggle(context, configDisableUnlockMessages, "Unlock messages", value, true);
    if (key == "disable_events") Toggle(context, configDisableEvents, "Random events", value, true);
    if (key == "disable_warnings") Toggle(context, configDisableParameterWarnings, "Command parameter warnings", value, true);
    if (key == "multiple_commands") Toggle(context, configMultiCommand, "Multiple commands per line", value);
    if (key == "god_no_stamina") Toggle(context, configGodModeNoStamina, "Stamina usage with god mode", value, true);
    if (key == "god_no_eitr") Toggle(context, configGodModeNoEitr, "Eitr usage with god mode", value, true);
    if (key == "god_no_item") Toggle(context, configGodModeNoUsage, "Item usage with god mode", value, true);
    if (key == "god_no_weight_limit") Toggle(context, configGodModeNoWeightLimit, "Weight limit with god mode", value, true);
    if (key == "god_no_stagger") Toggle(context, configGodModeNoStagger, "Staggering with god mode", value, true);
    if (key == "hide_shout_pings") Toggle(context, configHideShoutPings, "Shout pings", value, true);
    if (key == "god_no_edge") Toggle(context, configGodModeNoEdgeOfWorld, "Edge of world pull with god mode", value, true);
    if (key == "god_no_knockback") Toggle(context, configGodModeNoKnockback, "Knockback with god mode", value, true);
    if (key == "god_no_mist") Toggle(context, configGodModeNoMist, "Mist with god mode", value, true);
    if (key == "fly_no_clip") Toggle(context, configFlyNoClip, "No clip with fly mode", value);
    if (key == "ghost_invibisility") Toggle(context, configGhostInvisibility, "Invisibility with ghost mode", value);
    if (key == "server_commands") ToggleFlag(context, configServerCommands, "Server commands", value);
    if (key == "disable_command") ToggleFlag(context, configDisabledCommands, "Disabled commands", value);
    if (key == "disable_global_key") ToggleFlag(context, configDisabledGlobalKeys, "Disabled global keys", value);
    if (key == "disable_debug_mode_keys") Toggle(context, configDisableDebugModeKeys, "Debug mode key bindings", value, true);
    if (key == "god_always_parry") Toggle(context, configGodModeAlwaysParry, "Always parry with god mode", value);
    if (key == "god_always_dodge") Toggle(context, configGodModeAlwaysDodge, "Always dodge with god mode", value);
    if (key == "disable_start_shout") Toggle(context, configDisableStartShout, "Start shout", value, true);
    if (key == "disable_tutorials") Toggle(context, configDisableTutorials, "Tutorials", value, true);
    if (key == "disable_no_map") Toggle(context, configDisableNoMap, "Disable no map", value);
  }
}
