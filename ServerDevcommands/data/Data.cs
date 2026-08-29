
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Service;
using System.Linq;
using BepInEx;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization.NamingConventions;

namespace ServerDevcommands;

public class Yaml
{
  public static void SetupWatcher(string pattern, Action action) => SetupWatcher(Paths.ConfigPath, pattern, action);
  public static void SetupWatcher(string path, string pattern, string folder, Action action)
  {
    SetupWatcher(path, pattern, action);
    if (string.IsNullOrEmpty(folder)) return;
    var folderPath = Path.Combine(path, folder);
    if (Directory.Exists(folderPath))
      SetupWatcher(folderPath, "*", action);
  }
  public static void SetupWatcher(string path, string pattern, Action action)
  {
    FileSystemWatcher watcher = new(path, pattern);
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
    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults).WithTypeConverter(new FloatConverter()).WithEventEmitter(nextEmitter => new MultilineScalarFlowStyleEmitter(nextEmitter)).Build();

  public static T Deserialize<T>(string raw, string fileName) where T : new()
  {
    try
    {
      return Deserializer().Deserialize<T>(raw);
    }
    catch (Exception ex1)
    {
      Log.Error($"{fileName}: {ex1.Message}");
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

  public static void LoadDictFromDirectory<T>(string directory, string pattern, Action<string, string, T> action) =>
    LoadDictFromDirectory(directory, pattern, "", action);

  public static void LoadDictFromDirectory<T>(string directory, string pattern, string folder, Action<string, string, T> action)
  {
    if (!Directory.Exists(directory)) return;
    // Full search on top config directory could be really slow if some mod adds lots of files.
    // So just use it when operating inside some other folder.
    var search = directory == Paths.ConfigPath ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
    var paths = Directory.GetFiles(directory, pattern, search).AsEnumerable();
    if (!string.IsNullOrEmpty(folder))
    {
      var folderPath = Path.Combine(directory, folder);
      if (Directory.Exists(folderPath))
        paths = paths.Concat(Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)).Distinct();
    }
    foreach (var path in paths)
    {
      var file = Path.GetFileName(path);
      var data = Deserialize<Dictionary<string, T>>(File.ReadAllText(path), file);
      foreach (var kvp in data)
        action(file, kvp.Key, kvp.Value);
    }
  }

  public static void LoadListsFromDirectory<T>(string directory, string pattern, Action<string, T> action) where T : new() =>
    LoadListsFromDirectory(directory, pattern, "", action);

  public static void LoadListsFromDirectory<T>(string directory, string pattern, string folder, Action<string, T> action) where T : new()
  {
    if (!Directory.Exists(directory)) return;
    // Full search on top config directory could be really slow if some mod adds lots of files.
    // So just use it when operating inside some other folder.
    var search = directory == Paths.ConfigPath ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
    var paths = Directory.GetFiles(directory, pattern, search).AsEnumerable();
    if (!string.IsNullOrEmpty(folder))
    {
      var folderPath = Path.Combine(directory, folder);
      if (Directory.Exists(folderPath))
        paths = paths.Concat(Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)).Distinct();
    }
    foreach (var path in paths)
    {
      var file = Path.GetFileName(path);
      var data = Deserialize<List<T>>(File.ReadAllText(path), file);
      foreach (var item in data)
        action(file, item);
    }
  }
}

#nullable disable
public class FloatConverter : IYamlTypeConverter
{
  public bool Accepts(Type type) => type == typeof(float);

  public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
  {
    var scalar = (YamlDotNet.Core.Events.Scalar)parser.Current;
    var number = float.Parse(scalar.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
    parser.MoveNext();
    return number;
  }

  public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
  {
    var number = (float)value;
    emitter.Emit(new YamlDotNet.Core.Events.Scalar(number.ToString("0.###", CultureInfo.InvariantCulture)));
  }
}

public class MultilineScalarFlowStyleEmitter(IEventEmitter nextEmitter) : ChainedEventEmitter(nextEmitter)
{
  public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
  {

    if (typeof(string).IsAssignableFrom(eventInfo.Source.Type))
    {
      string value = eventInfo.Source.Value as string;
      if (!string.IsNullOrEmpty(value))
      {
        bool isMultiLine = value.IndexOfAny(['\r', '\n', '\x85', '\x2028', '\x2029']) >= 0;
        if (isMultiLine)
          eventInfo = new ScalarEventInfo(eventInfo.Source)
          {
            Style = ScalarStyle.Literal
          };
      }
    }

    nextEmitter.Emit(eventInfo, emitter);
  }
}
#nullable enable