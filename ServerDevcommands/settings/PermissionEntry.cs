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
  // Runtime map for feature sections. In YAML this is stored as flat top-level keys,
  // where each non-reserved key is treated as a feature section.
  public Dictionary<string, List<string>>? features = null;
  // Command has format "command: value" where value is yes/no/force (defaults to yes if omitted).
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
        mapped[section.Key] = section.Value;
      }
    }

    if (entry.commands != null && entry.commands.Count > 0)
      mapped["commands"] = entry.commands;

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
      commands = ReadStringList(raw, "commands")
    };

    Dictionary<string, List<string>> features = [];
    foreach (var kvp in raw)
    {
      var key = kvp.Key?.Trim() ?? "";
      if (key == "")
        continue;
      if (ReservedKeys.Contains(key.ToLowerInvariant()))
        continue;

      var parsed = ToStringList(kvp.Value);
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

  private static List<string>? ReadStringList(Dictionary<string, object> raw, string key)
  {
    if (!raw.TryGetValue(key, out var value))
      return null;
    return ToStringList(value);
  }

  private static List<string> ToStringList(object? value)
  {
    List<string> result = [];
    if (value == null)
      return result;

    if (value is string str)
    {
      var trimmed = str.Trim();
      if (trimmed != "")
        result.Add(trimmed);
      return result;
    }

    if (value is IEnumerable<object> list)
    {
      foreach (var item in list)
      {
        var itemValue = item?.ToString()?.Trim() ?? "";
        if (itemValue != "")
          result.Add(itemValue);
      }
      return result;
    }

    var single = value.ToString()?.Trim() ?? "";
    if (single != "")
      result.Add(single);
    return result;
  }
}
