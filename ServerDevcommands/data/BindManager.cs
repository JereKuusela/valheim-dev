using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;

[HarmonyPatch]
public class BindManager
{
  public static string FileName = "binds.yaml";
  public static string FilePath = Path.Combine(Paths.ConfigPath, FileName);

  private static List<CommandBind> Binds = [];
  private static List<CommandBind> WheelBinds = [];
  private static readonly List<CommandBind> TemporaryBinds = [];
  public static void AddBind(string keys, string command)
  {
    BindData data = new()
    {
      key = keys,
      command = command,
    };
    var bind = FromData(data, false);
    if (bind.MouseWheel) WheelBinds.Add(bind);
    else Binds.Add(bind);
    ToBeSaved = true;
  }
  public static void RemoveBind(string key)
  {
    if (key == "wheel") WheelBinds.Clear();
    else if (TryParse(key, out var keyCode))
    {
      Binds.RemoveAll(bind => bind.Required != null && bind.Required.Contains(keyCode));
      WheelBinds.RemoveAll(bind => bind.Required != null && bind.Required.Contains(keyCode));
    }
    ToBeSaved = true;
  }
  public static void SetTemporaryBind(string keys, string mode, string command, string offCommand)
  {
    BindData data = new()
    {
      key = keys,
      modifiers = mode,
      command = command,
      offCommand = offCommand,
    };
    TemporaryBinds.RemoveAll(b => b.Command == command);
    WheelBinds.RemoveAll(b => b.Command == command);
    Binds.RemoveAll(b => b.Command == command);
    var bind = FromData(data, true);
    if (!bind.MouseWheel && bind.Required.Count == 0) return;
    TemporaryBinds.Add(bind);
    if (bind.MouseWheel) WheelBinds.Add(bind);
    else if (bind.Required.Count > 0) Binds.Add(bind);
  }
  public static void ClearBinds()
  {
    Binds.Clear();
    WheelBinds.Clear();
    ToBeSaved = true;
  }
  public static CommandBind FromData(BindData data, bool temporary)
  {
    CommandBind bind = new()
    {
      Command = data.command,
      OffCommand = data.offCommand,
      Keys = data.key,
      Temporary = temporary,
    };
    // Quite long to support old format too.
    if (data.keys != null)
    {
      var keys = Parse.Split(data.keys);
      foreach (var key in keys)
      {
        if (key == "wheel")
        {
          bind.MouseWheel = true;
          continue;
        }
        else if (key == "-wheel")
        {
          bind.MouseWheel = false;
          continue;
        }
        if (key.StartsWith("-"))
        {
          bind.Banned ??= [];
          if (TryParse(key.Substring(1), out var keyCode))
            bind.Banned.Add(keyCode);
        }
        else
        {
          bind.Required ??= [];
          if (TryParse(key, out var keyCode))
            bind.Required.Add(keyCode);
        }
      }
    }
    if (data.state != null)
    {
      var states = Parse.Split(data.state);
      foreach (var state in states)
      {
        if (state == "wheel")
        {
          bind.MouseWheel = true;
          continue;
        }
        else if (state == "-wheel")
        {
          bind.MouseWheel = false;
          continue;
        }
        if (state.StartsWith("-"))
        {
          bind.BannedState ??= [];
          bind.BannedState.Add(state.Substring(1));
        }
        else
        {
          bind.RequiredState ??= [];
          bind.RequiredState.Add(state);
        }
      }
    }
    if (data.key != null)
    {
      var keys = Parse.Split(data.key);
      foreach (var key in keys)
      {
        if (key == "wheel")
        {
          bind.MouseWheel = true;
          continue;
        }
        else if (key == "-wheel")
        {
          bind.MouseWheel = false;
          continue;
        }
        if (key.StartsWith("-"))
        {
          bind.Banned ??= [];
          if (Enum.TryParse<KeyCode>(key.Substring(1), true, out var keyCode))
            bind.Banned.Add(keyCode);
        }
        else
        {
          bind.Required ??= [];
          if (Enum.TryParse<KeyCode>(key, true, out var keyCode))
            bind.Required.Add(keyCode);
        }
      }
    }
    if (data.modifiers != null)
    {
      // Modifiers can be keys or build.
      var modifiers = Parse.Split(data.modifiers);
      foreach (var modifier in modifiers)
      {
        if (modifier == "wheel")
        {
          bind.MouseWheel = true;
          continue;
        }
        else if (modifier == "-wheel")
        {
          bind.MouseWheel = false;
          continue;
        }
        if (modifier.StartsWith("-"))
        {
          bind.Banned ??= [];
          if (Enum.TryParse<KeyCode>(modifier.Substring(1), true, out var keyCode))
            bind.Banned.Add(keyCode);
          else
          {
            bind.BannedState ??= [];
            bind.BannedState.Add(modifier.Substring(1));
          }
        }
        else
        {
          bind.Required ??= [];
          if (Enum.TryParse<KeyCode>(modifier, true, out var keyCode))
            bind.Required.Add(keyCode);
          else
          {
            bind.RequiredState ??= [];
            bind.RequiredState.Add(modifier);
          }
        }
      }
    }
    // Infinity Hammer has added binds with underscore, these should be temporary to not remain when the mod is removed.
    if (!temporary && bind.Command.StartsWith("_"))
      bind.Temporary = true;
    // Some old commands have "unbound" as state to indicate that they are not bound to anything.
    // These should be cleaned up.
    if (bind.RequiredState?.Contains("unbound") == true)
    {
      bind.MouseWheel = false;
      bind.Required = [];
      ToBeSaved = true;
    }
    return bind;
  }
  private static bool TryParse(string str, out KeyCode keyCode)
  {
    if (Enum.TryParse(str, true, out keyCode)) return true;
    ServerDevcommands.Log.LogWarning($"Failed to parse {str} as KeyCode.");
    return false;
  }
  public static BindData ToData(string command)
  {
    var args = command.Split(' ');
    BindData data = new()
    {
      key = args[0]
    };
    if (data.key.ToLower() == "none")
      data.key = "wheel";
    foreach (var arg in args)
    {
      var split = arg.Split('=');
      if (split.Length == 1) continue;
      if (split[0] == "keys")
        data.modifiers = split[1];
    }
    var cmd = args.Skip(1).Where(arg => !arg.StartsWith("keys=", StringComparison.OrdinalIgnoreCase));
    data.command = string.Join(" ", cmd);
    return data;
  }
  public static BindData ToData(CommandBind commandBind)
  {
    List<string> keys = [];
    if (commandBind.Required != null)
      keys.AddRange(commandBind.Required.Select(key => key.ToString().ToLower()));
    if (commandBind.Banned != null)
      keys.AddRange(commandBind.Banned.Select(key => "-" + key.ToString().ToLower()));
    if (commandBind.MouseWheel)
      keys.Add("wheel");
    List<string> states = [];
    if (commandBind.RequiredState != null)
      states.AddRange(commandBind.RequiredState.Select(state => state.ToString().ToLower()));
    if (commandBind.BannedState != null)
      states.AddRange(commandBind.BannedState.Select(state => "-" + state.ToString().ToLower()));
    BindData data = new()
    {
      keys = string.Join(", ", keys),
      state = string.Join(", ", states),
      command = commandBind.Command,
      offCommand = commandBind.OffCommand,
    };
    return data;
  }
  private static void ImportBinds()
  {
    var binds = Terminal.m_bindList.Select(ToData).ToArray();
    if (binds.Length == 0) return;
    var yaml = Yaml.Serializer().Serialize(binds);
    File.WriteAllText(FilePath, yaml);
    ServerDevcommands.Log.LogInfo($"Importing {binds.Length} bind data.");
  }
  [HarmonyPatch(typeof(Chat), nameof(Chat.Awake)), HarmonyPostfix]
  public static void ChatAwake()
  {
    if (File.Exists(FilePath))
      FromFile();
    else
      ImportBinds();
  }
  public static bool ToBeSaved = false;
  public static void ToFile()
  {
    ToBeSaved = false;
    List<BindData> data = [.. Binds.Where(b => !b.Temporary).Select(ToData), .. WheelBinds.Where(b => !b.Temporary).Select(ToData)];
    if (data.Count == 0)
    {
      if (File.Exists(FilePath)) File.Delete(FilePath);
      return;
    }
    var yaml = Yaml.Serializer().Serialize(data);
    File.WriteAllText(FilePath, yaml);
  }
  public static void FromFile()
  {
    try
    {
      Terminal.m_bindList.Clear();
      Terminal.m_binds.Clear();
      var data = Yaml.Read(FilePath, Yaml.Deserialize<List<BindData>>);
      var binds = data.Select(d => FromData(d, false)).Where(b => b.MouseWheel || b.Required.Count > 0).ToList();
      binds.AddRange(TemporaryBinds);
      Binds = [.. binds.Where(bind => !bind.MouseWheel)];
      WheelBinds = [.. binds.Where(bind => bind.MouseWheel)];
      ServerDevcommands.Log.LogInfo($"Reloading {binds.Count} bind data.");
    }
    catch (Exception e)
    {
      ServerDevcommands.Log.LogError(e.StackTrace);
    }
  }
  public static void SetupWatcher()
  {
    Yaml.SetupWatcher(FileName, FromFile);
  }

  public static List<CommandBind> GetBestKeyCommands() =>
    GetBestCommands(Binds);
  public static List<CommandBind> GetBestWheelCommands() =>
    GetBestCommands(WheelBinds);
  private static List<CommandBind> GetBestCommands(List<CommandBind> binds)
  {
    var maxKeys = 0;
    List<CommandBind> commands = [];
    foreach (var bind in binds)
    {
      if (!IsValid(bind)) continue;
      var keys = bind.Required.Count;
      if (keys < maxKeys) continue;
      if (keys > maxKeys) commands.Clear();
      maxKeys = keys;
      commands.Add(bind);
    }
    return commands;
  }
  public static int GetBestKeyCount() =>
    GetKeyCount(Binds);
  public static int GetBestWheelCount() =>
    GetKeyCount(WheelBinds);
  private static int GetKeyCount(List<CommandBind> binds)
  {
    var maxKeys = 0;
    foreach (var bind in binds)
    {
      if (!IsValid(bind)) continue;
      var keys = bind.Required == null ? 0 : bind.Required.Count;
      if (keys > maxKeys) maxKeys = keys;
    }
    return maxKeys;
  }

  public static bool CouldKeyExecute() =>
    CouldExecute(Binds);
  public static bool CouldWheelExecute() =>
    CouldExecute(WheelBinds);
  private static bool CouldExecute(List<CommandBind> binds)
  {
    foreach (var bind in binds)
    {
      if (IsValid(bind)) return true;
    }
    return false;
  }

  private static string Mode = "";
  public static void SetMode(string mode)
  {
    Mode = mode;
  }
  private static bool IsValid(CommandBind bind)
  {
    var mode = Mode == "" ? Player.m_localPlayer?.InPlaceMode() == true ? "build" : "" : Mode;

    if (!bind.MouseWheel && (bind.Required == null || bind.Required.Count == 0)) return false;

    if (bind.Required.Any(key => !GetKey(key))) return false;
    if (bind.Banned != null && bind.Banned.Any(GetKey)) return false;
    if (bind.RequiredState != null && bind.RequiredState.All(state => state != mode)) return false;
    if (bind.BannedState != null && bind.BannedState.Any(state => state == mode)) return false;
    return true;
  }
  public static bool GetKey(KeyCode key)
  {
    // Mouse 5+ are not supported by Valheim.
    // Mouse 3 and 4 are swapped in the game.
    if (key == KeyCode.Mouse3) return ZInput.GetKey(KeyCode.Mouse4);
    if (key == KeyCode.Mouse4) return ZInput.GetKey(KeyCode.Mouse3);
    if (key == KeyCode.Mouse5) return Input.GetKey(KeyCode.Mouse5);
    if (key == KeyCode.Mouse6) return Input.GetKey(KeyCode.Mouse6);
    return ZInput.GetKey(key);
  }
  public static void PrintBinds(Terminal terminal)
  {
    if (Binds.Count == 0 && WheelBinds.Count == 0)
    {
      terminal.AddString("No binds found.");
      return;
    }
    if (Binds.Count > 0)
      terminal.AddString("Binds:");
    foreach (var bind in Binds) terminal.AddString(PrintBind(bind));
    if (WheelBinds.Count > 0)
      terminal.AddString("Wheel binds:");
    foreach (var bind in WheelBinds) terminal.AddString(PrintBind(bind));
  }
  private static string PrintBind(CommandBind bind)
  {
    List<string> keys = [];
    if (bind.Required != null)
      keys.AddRange(bind.Required.Select(key => key.ToString()));
    if (bind.Banned != null)
      keys.AddRange(bind.Banned.Select(key => "-" + key.ToString()));
    if (bind.RequiredState != null)
      keys.AddRange(bind.RequiredState.Select(state => state.ToString()));
    if (bind.BannedState != null)
      keys.AddRange(bind.BannedState.Select(state => "-" + state.ToString()));
    var input = string.Join(",", keys);
    return $"{input}: {bind.Command}";
  }

  public static List<CommandBind> GetOffBinds()
  {
    List<CommandBind> binds = [];
    foreach (var bind in Binds)
    {
      if (bind.Executed)
      {
        bind.WasExecuted = true;
        bind.Executed = false;
      }
      else if (bind.WasExecuted)
      {
        binds.Add(bind);
      }
    }
    return binds;
  }
}
