using System;
using System.Collections.Generic;

namespace ServerDevcommands;

public class PermissionData
{
  private readonly List<PermissionEntry> _entries;
  private readonly Dictionary<string, PermissionEntry> _entriesByKey;
  private readonly Dictionary<string, HashSet<string>> _groupsCache;

  public static string PeerKey(string hostname, string characterId)
  {
    if (string.IsNullOrWhiteSpace(hostname) || string.IsNullOrWhiteSpace(characterId))
      return "";
    return $"{hostname}_{characterId}";
  }

  public PermissionData()
  {
    _entries = [];
    _entriesByKey = [];
    _groupsCache = [];
  }

  public PermissionData(List<PermissionEntry>? entries)
  {
    _entries = entries ?? [];
    _entriesByKey = ToDictionary(_entries);
    _groupsCache = [];
    RebuildGroupCache();
  }

  public List<PermissionEntry> Entries => _entries;

  public int Count => _entriesByKey.Count;

  public bool TryGetValue(string key, out PermissionEntry entry) => _entriesByKey.TryGetValue(key, out entry!);

  public PermissionEntry GetOrCreate(string key)
  {
    if (_entriesByKey.TryGetValue(key, out var entry))
      return entry;

    entry = new PermissionEntry() { name = key };


    _entries.Add(entry);
    _entriesByKey[key] = entry;
    _groupsCache[key] = [];
    return entry;
  }

  public bool AddGroup(PermissionEntry entry, string group)
  {
    if (!TryGetTrackedEntry(entry, out var key, out var tracked))
      return false;

    group = group?.Trim() ?? "";
    if (group == "")
      return false;

    tracked.groups ??= [];
    if (tracked.groups.Exists(raw => raw.Trim().Equals(group, StringComparison.OrdinalIgnoreCase)))
      return false;

    tracked.groups.Add(group);
    RefreshGroupCache(key);
    return true;
  }

  public bool RemoveGroup(PermissionEntry entry, string group)
  {
    if (!TryGetTrackedEntry(entry, out var key, out var tracked))
      return false;

    group = group?.Trim() ?? "";
    if (group == "")
      return false;

    if (tracked.groups == null)
      return false;

    var oldCount = tracked.groups.Count;
    tracked.groups.RemoveAll(raw => raw.Trim().Equals(group, StringComparison.OrdinalIgnoreCase));
    if (tracked.groups.Count == 0)
      tracked.groups = null;

    var changed = oldCount != (tracked.groups?.Count ?? 0);
    if (changed)
      RefreshGroupCache(key);

    return changed;
  }

  public bool ClearGroups(PermissionEntry entry)
  {
    if (!TryGetTrackedEntry(entry, out var key, out var tracked))
      return false;

    if (tracked.groups == null || tracked.groups.Count == 0)
      return false;

    tracked.groups = null;
    RefreshGroupCache(key);
    return true;
  }

  public bool SetFeaturePermission(PermissionEntry entry, string section, string feature, PermissionManager.FeaturePermission permission)
  {
    if (!TryGetTrackedEntry(entry, out _, out var tracked))
      return false;

    section = section?.Trim().ToLowerInvariant() ?? "";
    feature = feature?.Trim() ?? "";
    if (section == "" || feature == "")
      throw new InvalidOperationException("Missing section or feature name.");

    tracked.features ??= [];
    if (!tracked.features.TryGetValue(section, out var features))
    {
      features = [];
      tracked.features[section] = features;
    }

    var suffix = PermissionToSuffix(permission);
    var replacement = suffix == "yes" ? feature : $"{feature}: {suffix}";
    var already = features.Exists(raw => raw.Equals(replacement, StringComparison.OrdinalIgnoreCase));
    var oldCount = features.Count;
    features.RemoveAll(raw => IsMatchingFeature(raw, feature));
    features.Add(replacement);
    return !already || oldCount != features.Count;
  }

  public bool ClearFeature(PermissionEntry entry, string section, string feature)
  {
    if (!TryGetTrackedEntry(entry, out _, out var tracked))
      return false;

    section = section?.Trim().ToLowerInvariant() ?? "";
    feature = feature?.Trim() ?? "";
    if (section == "" || feature == "")
      throw new InvalidOperationException("Missing section or feature name.");

    if (tracked.features == null)
      return false;
    if (!tracked.features.TryGetValue(section, out var features))
      return false;

    var oldCount = features.Count;
    features.RemoveAll(raw => IsMatchingFeature(raw, feature));
    if (features.Count == 0)
      tracked.features.Remove(section);
    if (tracked.features.Count == 0)
      tracked.features = null;

    return oldCount != features.Count;
  }

  public bool SetCommandPermission(PermissionEntry entry, string command, PermissionManager.FeaturePermission permission)
  {
    if (!TryGetTrackedEntry(entry, out _, out var tracked))
      return false;

    command = command?.Trim() ?? "";
    if (command == "")
      throw new InvalidOperationException("Missing command name.");

    tracked.commands ??= [];

    var suffix = PermissionToSuffix(permission);
    var replacement = suffix == "yes" ? command : $"{command}: {suffix}";
    var already = tracked.commands.Exists(raw => raw.Equals(replacement, StringComparison.OrdinalIgnoreCase));
    var oldCount = tracked.commands.Count;
    tracked.commands.RemoveAll(raw => IsMatchingCommand(raw, command));
    tracked.commands.Add(replacement);
    return !already || oldCount != tracked.commands.Count;
  }

  public bool ClearCommand(PermissionEntry entry, string command)
  {
    if (!TryGetTrackedEntry(entry, out _, out var tracked))
      return false;

    command = command?.Trim() ?? "";
    if (command == "")
      throw new InvalidOperationException("Missing command name.");

    if (tracked.commands == null)
      return false;

    var oldCount = tracked.commands.Count;
    tracked.commands.RemoveAll(raw => IsMatchingCommand(raw, command));
    var newCount = tracked.commands.Count;
    if (tracked.commands.Count == 0)
      tracked.commands = null;
    return oldCount != newCount;
  }

  public bool ClearAll(PermissionEntry entry)
  {
    if (!TryGetTrackedEntry(entry, out _, out var tracked))
      return false;

    var groupChanged = ClearGroups(tracked);
    var changed = groupChanged;
    if (tracked.commands != null)
    {
      tracked.commands = null;
      changed = true;
    }
    if (tracked.features != null)
    {
      tracked.features = null;
      changed = true;
    }

    return changed;
  }

  public bool HasGroup(string key, string group)
  {
    if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(group))
      return false;

    if (!_groupsCache.TryGetValue(key, out var groups))
      return false;

    return groups.Contains(group.Trim().ToLowerInvariant());
  }

  private static bool IsMatchingFeature(string raw, string feature)
  {
    var split = Parse.Kvp(raw, ':');
    return split.Key.Trim().Equals(feature.Trim(), StringComparison.OrdinalIgnoreCase);
  }

  private static bool IsMatchingCommand(string raw, string command)
  {
    var split = Parse.Kvp(raw, ':');
    return split.Key.Trim().Equals(command.Trim(), StringComparison.OrdinalIgnoreCase);
  }

  private static string PermissionToSuffix(PermissionManager.FeaturePermission permission)
  {
    return permission switch
    {
      PermissionManager.FeaturePermission.No => "no",
      PermissionManager.FeaturePermission.Force => "force",
      _ => "yes"
    };
  }

  public void RebuildGroupCache()
  {
    _groupsCache.Clear();
    foreach (var key in _entriesByKey.Keys)
      RefreshGroupCache(key);
  }

  public void RefreshGroupCache(string key)
  {
    if (string.IsNullOrWhiteSpace(key))
      return;

    if (!_entriesByKey.ContainsKey(key))
    {
      _groupsCache.Remove(key);
      return;
    }

    HashSet<string> resolved = new(StringComparer.OrdinalIgnoreCase);
    HashSet<string> visitedPath = new(StringComparer.OrdinalIgnoreCase);
    AddResolvedGroups(key, resolved, visitedPath);
    _groupsCache[key] = resolved;
  }

  private void AddResolvedGroups(string key, HashSet<string> resolved, HashSet<string> visitedPath)
  {
    if (!visitedPath.Add(key))
      return;

    if (!_entriesByKey.TryGetValue(key, out var entry) || entry.groups == null)
    {
      visitedPath.Remove(key);
      return;
    }

    foreach (var rawGroup in entry.groups)
    {
      var group = rawGroup.Trim().ToLowerInvariant();
      if (group == "")
        continue;

      resolved.Add(group);
      AddResolvedGroups(group, resolved, visitedPath);
    }

    visitedPath.Remove(key);
  }


  public bool UpdatePeer(string hostname, string characterId, string playerName)
  {
    var key = PeerKey(hostname, characterId);
    if (key == "")
      return false;

    var entry = GetOrCreate(key);
    var updated = false;
    updated |= UpdateString(entry, e => e.name, (e, value) => e.name = value, playerName);
    updated |= UpdateString(entry, e => e.id, (e, value) => e.id = value, hostname);
    updated |= UpdateString(entry, e => e.character, (e, value) => e.character = value, characterId);
    return updated;
  }

  public PermissionManager Resolve(string hostname, string characterId)
  {
    bool isAdmin = ZNet.instance == null || ZNet.instance.IsServer() || ZNet.instance.IsAdmin(hostname);
    var resolved = new PermissionManager(isAdmin);
    var key = PeerKey(hostname, characterId);

    var chain = BuildResolutionChain(key);
    for (int i = 0; i < chain.Count; ++i)
      resolved.AddEntry(chain[i]);
    return resolved;
  }

  private List<PermissionEntry> BuildResolutionChain(string playerKey)
  {
    List<PermissionEntry> result = [];
    HashSet<string> visitedPath = [];
    HashSet<string> addedEntries = [];
    AddEntryWithParents("Everyone", result, visitedPath, addedEntries);
    AddEntryWithParents(playerKey, result, visitedPath, addedEntries);
    return result;
  }

  private void AddEntryWithParents(string key, List<PermissionEntry> result, HashSet<string> visitedPath, HashSet<string> addedEntries)
  {
    if (!_entriesByKey.TryGetValue(key, out var entry))
      return;

    AddGroupChains(entry.groups, result, visitedPath, addedEntries);

    if (addedEntries.Contains(key))
      return;

    addedEntries.Add(key);
    result.Add(entry);
  }

  private void AddGroupChains(List<string>? groupNames, List<PermissionEntry> result, HashSet<string> visitedPath, HashSet<string> addedEntries)
  {
    if (groupNames == null)
      return;

    foreach (var groupName in groupNames)
      AddGroupChain(groupName, result, visitedPath, addedEntries);
  }

  private void AddGroupChain(string groupName, List<PermissionEntry> result, HashSet<string> visitedPath, HashSet<string> addedEntries)
  {
    groupName = groupName.Trim();
    if (string.IsNullOrWhiteSpace(groupName))
      return;

    if (visitedPath.Contains(groupName))
      return;

    if (!_entriesByKey.TryGetValue(groupName, out var groupEntry))
      return;

    visitedPath.Add(groupName);
    AddGroupChains(groupEntry.groups, result, visitedPath, addedEntries);

    if (!addedEntries.Contains(groupName))
    {
      addedEntries.Add(groupName);
      result.Add(groupEntry);
    }

    visitedPath.Remove(groupName);
  }

  private static bool UpdateString(PermissionEntry entry, Func<PermissionEntry, string> getter, Action<PermissionEntry, string> setter, string value)
  {
    if (getter(entry) == value)
      return false;
    setter(entry, value);
    return true;
  }

  private static string EntryKey(PermissionEntry entry)
  {
    var key = PeerKey(entry.id, entry.character);
    if (key != "")
      return key;
    return entry.name;
  }

  private bool TryGetTrackedEntry(PermissionEntry? entry, out string key, out PermissionEntry tracked)
  {
    key = "";
    tracked = null!;
    if (entry == null)
      return false;

    key = EntryKey(entry);
    if (string.IsNullOrWhiteSpace(key))
      return false;

    if (!_entriesByKey.TryGetValue(key, out tracked!))
      return false;

    return true;
  }

  private static Dictionary<string, PermissionEntry> ToDictionary(List<PermissionEntry> entries)
  {
    Dictionary<string, PermissionEntry> result = [];
    foreach (var entry in entries)
    {
      var key = EntryKey(entry);
      if (string.IsNullOrWhiteSpace(key))
        continue;
      result[key] = entry;
    }
    return result;
  }

  private static bool IsNullOrEmpty(List<string>? value) => value == null || value.Count == 0;
  private static bool IsNullOrEmpty(Dictionary<string, List<string>>? value) => value == null || value.Count == 0;

  private static bool EqualStringList(List<string>? a, List<string>? b)
  {
    if (IsNullOrEmpty(a) && IsNullOrEmpty(b)) return true;
    if (a == null || b == null) return false;
    if (a.Count != b.Count) return false;
    for (int i = 0; i < a.Count; ++i)
    {
      if (a[i] != b[i]) return false;
    }
    return true;
  }

  private static bool EqualFeatures(Dictionary<string, List<string>>? a, Dictionary<string, List<string>>? b)
  {
    if (IsNullOrEmpty(a) && IsNullOrEmpty(b)) return true;
    if (a == null || b == null) return false;
    if (a.Count != b.Count) return false;
    foreach (var kvp in a)
    {
      if (!b.TryGetValue(kvp.Key, out var other)) return false;
      if (!EqualStringList(kvp.Value, other)) return false;
    }
    return true;
  }

  private static bool EqualEntry(PermissionEntry? a, PermissionEntry? b)
  {
    if (ReferenceEquals(a, b)) return true;
    if (a == null || b == null) return false;
    if (a.id != b.id) return false;
    if (a.name != b.name) return false;
    if (a.character != b.character) return false;
    if (!EqualStringList(a.groups, b.groups)) return false;
    if (!EqualFeatures(a.features, b.features)) return false;
    if (!EqualStringList(a.commands, b.commands)) return false;
    return true;
  }

  public static HashSet<string> ChangedKeys(Dictionary<string, PermissionEntry> oldData, Dictionary<string, PermissionEntry> newData)
  {
    HashSet<string> changed = [];
    foreach (var kvp in oldData)
    {
      if (!newData.TryGetValue(kvp.Key, out var next) || !EqualEntry(kvp.Value, next))
        changed.Add(kvp.Key);
    }
    foreach (var key in newData.Keys)
    {
      if (!oldData.ContainsKey(key))
        changed.Add(key);
    }
    return changed;
  }

  public static HashSet<string> ChangedKeys(PermissionData oldData, PermissionData newData)
  {
    return ChangedKeys(oldData._entriesByKey, newData._entriesByKey);
  }

  public static bool HasGroupChanges(HashSet<string> changedKeys, Dictionary<string, PermissionEntry> oldData, Dictionary<string, PermissionEntry> newData)
  {
    foreach (var key in changedKeys)
    {
      if (IsGroupEntry(SelectEntry(key, oldData, newData)))
        return true;
    }
    return false;
  }

  public static bool HasGroupChanges(HashSet<string> changedKeys, PermissionData oldData, PermissionData newData)
  {
    return HasGroupChanges(changedKeys, oldData._entriesByKey, newData._entriesByKey);
  }

  private static PermissionEntry? SelectEntry(string key, Dictionary<string, PermissionEntry> oldData, Dictionary<string, PermissionEntry> newData)
  {
    if (newData.TryGetValue(key, out var next))
      return next;
    if (oldData.TryGetValue(key, out var previous))
      return previous;
    return null;
  }

  private static bool IsGroupEntry(PermissionEntry? entry)
  {
    if (entry == null)
      return false;
    var isPeer = !string.IsNullOrWhiteSpace(entry.id) && !string.IsNullOrWhiteSpace(entry.character);
    if (isPeer)
      return false;
    return !string.IsNullOrWhiteSpace(entry.name);
  }
}
