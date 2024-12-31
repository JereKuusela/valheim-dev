
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BepInEx;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization.NamingConventions;

namespace ServerDevcommands;

public class Yaml
{
  public static void SetupWatcher(string pattern, Action action) => SetupWatcher(Paths.ConfigPath, pattern, action);
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
    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults).WithTypeConverter(new FloatConverter()).WithEventEmitter(static nextEmitter => new MultilineScalarFlowStyleEmitter(nextEmitter)).Build();

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
  public static List<T> LoadList<T>(string file) where T : new()
  {
    if (!File.Exists(file)) return [];
    return Deserialize<List<T>>(File.ReadAllText(file), file);
  }
  public static T Load<T>(string file) where T : new()
  {
    if (!File.Exists(file)) return new T();
    return Deserialize<T>(File.ReadAllText(file), file);
  }
  public static T Read<T>(string file, Func<string, string, T> action) where T : new()
  {
    if (!File.Exists(file)) return new T();
    return action(File.ReadAllText(file), file);
  }
  public static Dictionary<string, List<T>> Read<T>(string pattern, Func<string, string, Dictionary<string, T[]>> action)
  {
    Dictionary<string, List<T>> result = [];
    foreach (var name in Directory.GetFiles(Paths.ConfigPath, pattern))
    {
      var data = action(File.ReadAllText(name), name);
      foreach (var kvp in data)
      {
        if (!result.TryGetValue(kvp.Key, out var list))
        {
          list = [];
          result[kvp.Key] = list;
        }
        list.AddRange(kvp.Value);
      }
    }
    return result;
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