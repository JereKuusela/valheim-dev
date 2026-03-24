using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ServerDevcommands;

public class PermissionEntry
{
  [DefaultValue("")]
  public string id = "";
  [DefaultValue("")]
  public string name = "";
  [DefaultValue("")]
  public string character = "";
  [DefaultValue("")]
  public string group = "";
  // Runtime map for feature sections. In YAML each non-reserved top-level key is a
  // map of feature -> yes/no/force.
  public Dictionary<string, List<string>>? features = null;
  // Runtime list where each item is "command" or "command: no/force".
  // In YAML this is stored as a map of command -> yes/no/force.
  public List<string>? commands = null;
}
public static class PermissionYaml
{
  private static readonly HashSet<string> ReservedKeys = ["id", "name", "character", "group", "commands"];

  public static string SerializeEntries(List<PermissionEntry> entries)
  {
    List<Dictionary<string, object>> mapped = [];
    foreach (var entry in entries)
      mapped.Add(SerializeEntry(entry));
    return Yaml.Serializer().Serialize(mapped);
  }

  public static List<PermissionEntry> DeserializeEntries(string value)
  {
    try
    {
      var rows = Yaml.Deserializer().Deserialize<List<Dictionary<string, object>>>(value);
      if (rows == null) return [];
      return rows.Select(ParseEntry).ToList();
    }
    catch (Exception ex)
    {
      ServerDevcommands.Log.LogError($"permissions: {ex.Message}");
      return [];
    }
  }

  private static Dictionary<string, object> SerializeEntry(PermissionEntry entry)
  {
    Dictionary<string, object> mapped = [];

    if (!string.IsNullOrWhiteSpace(entry.id)) mapped["id"] = entry.id;
    if (!string.IsNullOrWhiteSpace(entry.name)) mapped["name"] = entry.name;
    if (!string.IsNullOrWhiteSpace(entry.character)) mapped["character"] = entry.character;
    if (!string.IsNullOrWhiteSpace(entry.group)) mapped["group"] = entry.group;

    if (entry.features != null)
    {
      foreach (var section in entry.features.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
      {
        if (section.Value.Count == 0)
          continue;
        var featureMap = ToPermissionMap(section.Value);
        if (featureMap.Count > 0)
          mapped[section.Key] = featureMap;
      }
    }

    if (entry.commands != null && entry.commands.Count > 0)
    {
      var commandMap = ToPermissionMap(entry.commands);
      if (commandMap.Count > 0)
        mapped["commands"] = commandMap;
    }

    return mapped;
  }

  private static PermissionEntry ParseEntry(Dictionary<string, object> raw)
  {
    PermissionEntry entry = new()
    {
      id = ReadScalar(raw, "id"),
      name = ReadScalar(raw, "name"),
      character = ReadScalar(raw, "character"),
      group = ReadScalar(raw, "group"),
      commands = ReadPermissionList(raw, "commands")
    };

    Dictionary<string, List<string>> features = [];
    foreach (var kvp in raw)
    {
      var key = kvp.Key?.Trim() ?? "";
      if (key == "")
        continue;
      if (ReservedKeys.Contains(key.ToLowerInvariant()))
        continue;

      var parsed = ToPermissionList(kvp.Value);
      if (parsed.Count == 0)
        continue;
      features[key.ToLowerInvariant()] = parsed;
    }

    if (features.Count > 0)
      entry.features = features;

    if (entry.commands != null && entry.commands.Count == 0)
      entry.commands = null;

    return entry;
  }

  private static string ReadScalar(Dictionary<string, object> raw, string key)
  {
    if (!raw.TryGetValue(key, out var value))
      return "";
    return value?.ToString()?.Trim() ?? "";
  }

  private static List<string>? ReadPermissionList(Dictionary<string, object> raw, string key)
  {
    if (!raw.TryGetValue(key, out var value))
      return null;
    return ToPermissionList(value);
  }

  private static List<string> ToPermissionList(object? value)
  {
    Dictionary<string, string> map = [];
    if (value is IDictionary<object, object> objectMap)
    {
      foreach (var kvp in objectMap)
      {
        var key = kvp.Key?.ToString()?.Trim() ?? "";
        if (key == "")
          continue;
        var permission = kvp.Value?.ToString()?.Trim() ?? "";
        map[key] = permission;
      }
    }
    else if (value is IDictionary<string, object> stringMap)
    {
      foreach (var kvp in stringMap)
      {
        var key = kvp.Key?.Trim() ?? "";
        if (key == "")
          continue;
        var permission = kvp.Value?.ToString()?.Trim() ?? "";
        map[key] = permission;
      }
    }

    List<string> result = [];
    foreach (var kvp in map)
    {
      var key = kvp.Key.Trim();
      if (key == "")
        continue;
      var permission = kvp.Value.Trim();
      if (permission == "" || permission.Equals("yes", StringComparison.OrdinalIgnoreCase))
        result.Add(key);
      else
        result.Add($"{key}: {permission}");
    }
    return result;
  }

  private static Dictionary<string, string> ToPermissionMap(List<string> values)
  {
    Dictionary<string, string> mapped = [];
    foreach (var value in values)
    {
      var split = Parse.Kvp(value, ':');
      var key = split.Key.Trim();
      if (key == "")
        continue;

      var permission = split.Value.Trim().ToLowerInvariant();
      if (permission == "")
        permission = "yes";
      mapped[key] = permission;
    }
    return mapped;
  }
}
