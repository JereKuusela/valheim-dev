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
  public static string Pattern = "alias*.yaml";
  public static string Folder = "alias";
  public const string DefaultFile = "alias.yaml";
  ///<summary>Tracks whether an alias came from the default file, so saving only affects the default file.</summary>
  private class AliasEntry
  {
    public string Value = "";
    public bool IsDefault;
  }
  private static readonly Dictionary<string, AliasEntry> Aliases = [];
  public static string[] AliasKeys => [.. Aliases.Keys.OrderBy(key => key)];

  private static string GetFolderPath()
  {
    var folderPath = Path.Combine(Paths.ConfigPath, Folder);
    return Directory.Exists(folderPath) ? folderPath : Paths.ConfigPath;
  }

  public static void Init()
  {
    Yaml.ConsolidateDefaultFile(Paths.ConfigPath, Folder, DefaultFile);
    FromFiles();
    Yaml.SetupWatcher(Paths.ConfigPath, Pattern, Folder, FromFiles);
  }
  public static bool ToBeSaved = false;
  public static void ToFile()
  {
    ToBeSaved = false;
    var data = Aliases.Where(kvp => kvp.Value.IsDefault).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);
    var yaml = Yaml.Serializer().Serialize(data);
    File.WriteAllText(Path.Combine(GetFolderPath(), DefaultFile), yaml);
  }
  public static void FromFiles()
  {
    foreach (var alias in Aliases.Keys)
      RemoveCommand(alias);
    Aliases.Clear();
    Yaml.LoadDictFromDirectory<string>(Paths.ConfigPath, Pattern, Folder, LoadAlias);
    Log.Info($"Reloading {Aliases.Count} alias data.");
  }

  public static string GetAliasValue(string key) => Aliases.TryGetValue(key, out var entry) ? entry.Value : "_";

  public static void LoadAlias(string file, string alias, string value)
  {
    if (Aliases.ContainsKey(alias))
      Log.Warning($"Duplicate alias '{alias}' in {file}. Overwriting previous value.");
    AddCommand(alias, value);
    Aliases[alias] = new AliasEntry { Value = value, IsDefault = Yaml.IsDefaultFile(file, Folder, DefaultFile) };
  }
  public static void AddAlias(string alias, string value)
  {
    Aliases[alias] = new AliasEntry { Value = value, IsDefault = true };
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
