using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Service;
namespace ServerDevcommands;

[HarmonyPatch]
public class AliasManager
{
  public static string FileName = "alias.yaml";
  public static string FilePath = Path.Combine(Paths.ConfigPath, FileName);
  private static Dictionary<string, string> Aliases = [];
  public static string[] AliasKeys => [.. Aliases.Keys.OrderBy(key => key)];

  public static void Init()
  {
    if (File.Exists(FilePath))
      FromFile();
    else
      ToFile();

    Yaml.SetupWatcher(FileName, FromFile);
  }
  public static bool ToBeSaved = false;
  public static void ToFile()
  {
    ToBeSaved = false;
    if (Aliases.Count == 0)
    {
      if (File.Exists(FilePath)) File.Delete(FilePath);
      return;
    }
    var yaml = Yaml.Serializer().Serialize(Aliases);
    File.WriteAllText(FilePath, yaml);
  }
  public static void FromFile()
  {
    foreach (var alias in Aliases.Keys)
      RemoveCommand(alias);
    Aliases.Clear();
    Yaml.LoadDictFromDirectory<string>(Paths.ConfigPath, "alias*.yaml", LoadAlias);
    Log.Info($"Reloading {Aliases.Count} alias data.");
  }

  public static string GetAliasValue(string key) => Aliases.TryGetValue(key, out var value) ? value : "_";

  public static void LoadAlias(string file, string alias, string value)
  {
    if (Aliases.ContainsKey(alias))
      Log.Warning($"Duplicate alias '{alias}' in {file}. Overwriting previous value.");
    AddCommand(alias, value);
    Aliases[alias] = value;
  }
  public static void AddAlias(string alias, string value)
  {
    Aliases[alias] = value;
    AddCommand(alias, value);
    ToBeSaved = true;
  }

  public static void RemoveAlias(string alias)
  {
    Aliases.Remove(alias);
    RemoveCommand(alias);
    ToBeSaved = true;
  }

  ///<summary>Adds an alias as an actual command so it works with autocomplete, etc.</summary>
  public static void AddCommand(string key, string value)
  {
    var plain = Aliasing.Plain(value);
    var baseCommand = plain.Split(' ').First();
    if (Terminal.commands.TryGetValue(baseCommand, out var command))
      new Terminal.ConsoleCommand(key, plain, command.action, command.IsCheat, command.IsNetwork, command.OnlyServer, command.IsSecret, command.AllowInDevBuild, command.m_tabOptionsFetcher);
    else
      new Terminal.ConsoleCommand(key, plain, (args) => { });
  }
  public static void RemoveCommand(string key)
  {
    if (Terminal.commands.ContainsKey(key))
      Terminal.commands.Remove(key);
  }

}
