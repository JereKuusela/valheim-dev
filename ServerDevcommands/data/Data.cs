
using System;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ServerDevcommands;

public class Data : MonoBehaviour
{
  public static void SetupWatcher(string pattern, Action action)
  {
    FileSystemWatcher watcher = new(Paths.ConfigPath, pattern);
    watcher.Created += (s, e) => action();
    watcher.Changed += (s, e) => action();
    watcher.Renamed += (s, e) => action();
    watcher.Deleted += (s, e) => action();
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }
  public static IDeserializer Deserializer() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
  public static IDeserializer DeserializerUnSafe() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
  .IgnoreUnmatchedProperties().Build();
  public static ISerializer Serializer() => new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).DisableAliases()
    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults).Build();

  public static T Deserialize<T>(string raw, string fileName) where T : new()
  {
    try
    {
      return Deserializer().Deserialize<T>(raw);
    }
    catch (Exception ex1)
    {
      ServerDevcommands.Log.LogError($"{fileName}: {ex1.Message}");
      try
      {
        return DeserializerUnSafe().Deserialize<T>(raw);
      }
      catch (Exception)
      {
        return new();
      }
    }
  }

  public static T Read<T>(string file, Func<string, string, T> action) where T : new()
  {
    if (!File.Exists(file)) return new T();
    return action(File.ReadAllText(file), file);
  }
}

