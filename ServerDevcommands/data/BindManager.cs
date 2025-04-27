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
  public static void AddBind(string keys, string command)
  {
    BindData data = new()
    {
      key = keys,
      command = command,
    };
    var bind = FromData(data);
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
  public static void UpdateBind(string keys, string mode, string command, string offCommand)
  {
    BindData data = new()
    {
      key = keys,
      modifiers = mode,
      command = command,
      offCommand = offCommand,
    };
    // Simply needed to avoid unnecessary updates.
    if (WheelBinds.Any(b => b.Command == command && b.Keys == keys)) return;
    if (Binds.Any(b => b.Command == command && b.Keys == keys)) return;
    WheelBinds = [.. WheelBinds.Where(b => b.Command != command)];
    Binds = [.. Binds.Where(b => b.Command != command)];
    var bind = FromData(data);
    if (bind.Required.Count > 0)
    {
      if (bind.MouseWheel) WheelBinds.Add(bind);
      else Binds.Add(bind);
    }
    ToBeSaved = true;
  }
  public static void ClearBinds()
  {
    Binds.Clear();
    WheelBinds.Clear();
    ToBeSaved = true;
  }
  public static CommandBind FromData(BindData data)
  {
    CommandBind bind = new()
    {
      Command = data.command,
      OffCommand = data.offCommand,
      Keys = data.key
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
      keys.AddRange(commandBind.Required.Select(key => key.ToString()));
    if (commandBind.Banned != null)
      keys.AddRange(commandBind.Banned.Select(key => "-" + key.ToString()));
    if (commandBind.MouseWheel)
      keys.Add("wheel");
    List<string> states = [];
    if (commandBind.RequiredState != null)
      states.AddRange(commandBind.RequiredState.Select(state => state.ToString()));
    if (commandBind.BannedState != null)
      states.AddRange(commandBind.BannedState.Select(state => "-" + state.ToString()));
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
    List<BindData> data = [.. Binds.Select(ToData), .. WheelBinds.Select(ToData)];
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
      var binds = data.Select(FromData).Where(b => b.Required.Count > 0).ToList();
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
    var commands = new List<CommandBind>();
    foreach (var bind in binds)
    {
      if (!IsValid(bind)) continue;
      var keys = bind.Required == null ? 0 : bind.Required.Count;
      if (keys > maxKeys)
      {
        maxKeys = keys;
        commands.Clear();
      }
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
      if (!IsValid(bind)) continue;
      var keys = bind.Required == null ? 0 : bind.Required.Count;
      if (keys > 0) return true;
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
    var inBuildMode = Player.m_localPlayer?.InPlaceMode() ?? false;
    if (bind.Required == null || bind.Required.Count == 0) return false;
    if (bind.Required.Any(key => !ZInput.GetKey(key))) return false;
    if (bind.Banned != null && bind.Banned.Count > 0)
    {
      if (bind.Banned.Any(key => ZInput.GetKey(key))) return false;
    }
    if (bind.RequiredState != null && bind.RequiredState.Count > 0)
    {
      foreach (var state in bind.RequiredState)
      {
        if (state == "build" && !inBuildMode) continue;
        else if (state != Mode) continue;
      }
    }
    if (bind.BannedState != null && bind.BannedState.Count > 0)
    {
      foreach (var state in bind.BannedState)
      {
        if (state == "build" && inBuildMode) continue;
        else if (state == Mode) continue;
      }
    }
    return true;
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
