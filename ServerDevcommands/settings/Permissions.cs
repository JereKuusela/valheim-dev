using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerDevcommands;

#nullable disable

/// <summary>
/// Manages client-side permissions for features and commands.
/// Permissions are synchronized from the server via RPC using ZPackage.
/// By default, all features are disabled and all commands are allowed.
/// Uses a generic string-based system for extensibility.
/// </summary>
public class PermissionManager
{
  public enum FeaturePermission
  {
    Unknown = -1,
    Yes = 0,
    No = 1,
    Force = 2,
  }

  private static PermissionManager _instance;
  public static PermissionManager Instance
  {
    get
    {
      if (_instance == null)
        _instance = new PermissionManager();
      return _instance;
    }
  }

  // ====================
  // Feature Permissions
  // ====================

  // Maps section name -> feature hash -> permission mode
  private Dictionary<string, Dictionary<int, FeaturePermission>> _featurePermissions = [];

  // Tracks whether permissions were received from the server
  private bool _permissionsReceived = false;

  public PermissionManager()
  {
    ResetToDefaults();
  }

  public PermissionManager(PermissionEntry entry)
  {
    ResetToDefaults();
    if (entry == null)
      return;

    if (entry.features != null)
    {
      foreach (var section in entry.features)
      {
        string sectionName = section.Key.ToLower();

        if (!_featurePermissions.ContainsKey(sectionName))
          _featurePermissions[sectionName] = [];

        foreach (var rawFeature in section.Value)
        {
          ParseFeature(rawFeature, out var featureName, out var permission);
          if (featureName == "")
            continue;

          int featureHash = featureName.ToLower().GetStableHashCode();
          _featurePermissions[sectionName][featureHash] = permission;
        }
      }
    }

    _allowedCommands.Clear();
    _bannedCommands.Clear();

    if (entry.commands != null)
    {
      Dictionary<string, FeaturePermission> commandPermissions = [];
      foreach (var rawCommand in entry.commands)
      {
        ParseFeature(rawCommand, out var commandName, out var permission);
        if (commandName == "")
          continue;

        commandPermissions[commandName.ToLower()] = permission;
      }


      foreach (var kvp in commandPermissions)
      {
        if (kvp.Value == FeaturePermission.No)
          _bannedCommands.Add(kvp.Key);
        else
          _allowedCommands.Add(kvp.Key);
      }
    }

    HandleFeatureCommands();
  }

  /// <summary>
  /// Checks if a specific feature is enabled for a section.
  /// </summary>
  /// <param name="section">The section/mod name (e.g., "ServerDevcommands")</param>
  /// <param name="feature">The feature name (e.g., "MapCoordinates")</param>
  /// <returns>True if the feature is enabled, false otherwise.</returns>
  public bool IsFeatureEnabled(string section, string feature) => IsFeatureEnabledByHash(section, feature.ToLower().GetStableHashCode());

  /// <summary>
  /// Checks if a specific feature is active using pre-calculated hashes and a local config value.
  /// </summary>
  /// <param name="section">The section/mod name (e.g., "ServerDevcommands")</param>
  /// <param name="featureHash">The feature hash code</param>
  /// <param name="localConfigValue">The local config value for the feature.</param>
  /// <returns>True if the feature is active, false otherwise.</returns>
  public bool IsFeatureEnabledByHash(string section, int featureHash, bool localConfigValue)
  {
    var permission = GetFeaturePermissionByHash(section, featureHash);
    return permission switch
    {
      FeaturePermission.No => false,
      FeaturePermission.Force => true,
      FeaturePermission.Yes => localConfigValue,
      _ => localConfigValue && Console.instance && Console.instance.IsCheatsEnabled(),
    };
  }

  /// <summary>
  /// Checks if a specific feature is enabled using pre-calculated hashes.
  /// Fallback: If on server or (cheats enabled AND no permissions received), allow all features.
  /// </summary>
  /// <param name="sectionHash">The section hash code</param>
  /// <param name="featureHash">The feature hash code</param>
  /// <returns>True if the feature is enabled, false otherwise.</returns>
  public bool IsFeatureEnabledByHash(string section, int featureHash)
  {
    return IsFeatureEnabledByHash(section, featureHash, true);
  }

  public FeaturePermission GetFeaturePermissionByHash(string section, int featureHash)
  {
    if (section == null)
      return FeaturePermission.Unknown;

    section = section.ToLower();
    if (!_permissionsReceived)
      return FeaturePermission.Unknown;

    if (!_featurePermissions.TryGetValue(section, out var features))
      return FeaturePermission.Unknown;

    return features.TryGetValue(featureHash, out var permission) ? permission : FeaturePermission.Unknown;
  }

  // ====================
  // Command Permissions
  // ====================

  private HashSet<string> _allowedCommands = [];
  private HashSet<string> _bannedCommands = [];

  public bool IsCommandAllowed(string commandName)
  {
    var normalized = commandName.ToLower();
    if (_bannedCommands.Contains(normalized))
      return false;
    if (_allowedCommands.Count == 0)
      return true;
    return _allowedCommands.Contains(normalized);
  }

  // ====================
  // Serialization for RPC
  // ====================

  /// <summary>
  /// Writes permissions to a ZPackage for network transmission.
  /// Format: [section count] for each section: [section] [feature count] [feature hash] [feature permission int]... [allowed command count] [allowed command names...] [banned command count] [banned command names...]
  /// Uses hash codes to minimize network traffic while maintaining compatibility.
  /// </summary>
  public void Write(ZPackage pkg)
  {
    // Write feature sections
    pkg.Write(_featurePermissions.Count);
    foreach (var section in _featurePermissions)
    {
      pkg.Write(section.Key); // section
      pkg.Write(section.Value.Count); // feature count
      foreach (var feature in section.Value)
      {
        pkg.Write(feature.Key); // feature hash
        pkg.Write((int)feature.Value); // feature permission
      }
    }

    pkg.Write(_allowedCommands.Count);
    foreach (var cmd in _allowedCommands)
      pkg.Write(cmd);
    pkg.Write(_bannedCommands.Count);
    foreach (var cmd in _bannedCommands)
      pkg.Write(cmd);
  }

  /// <summary>
  /// Reads permissions from a ZPackage received via network.
  /// Supports hash-based feature system for extensibility.
  /// </summary>
  public void Read(ZPackage pkg)
  {
    // Reset all features to defaults first
    ResetToDefaults();
    // Mark that permissions were received
    _permissionsReceived = true;

    // Read feature sections
    int sectionCount = pkg.ReadInt();
    for (int i = 0; i < sectionCount; i++)
    {
      string sectionName = pkg.ReadString();
      int featureCount = pkg.ReadInt();

      if (!_featurePermissions.ContainsKey(sectionName))
        _featurePermissions[sectionName] = [];

      for (int j = 0; j < featureCount; j++)
      {
        int featureHash = pkg.ReadInt();
        var permission = (FeaturePermission)pkg.ReadInt();
        _featurePermissions[sectionName][featureHash] = permission;
      }
    }

    // Read allowed commands
    int commandCount = pkg.ReadInt();
    _allowedCommands.Clear();
    for (int i = 0; i < commandCount; i++)
      _allowedCommands.Add(pkg.ReadString());

    int bannedCommandCount = pkg.ReadInt();
    _bannedCommands.Clear();
    for (int i = 0; i < bannedCommandCount; i++)
      _bannedCommands.Add(pkg.ReadString());

    HandleFeatureCommands();
  }

  // Some features have console command that is used to toggle them.
  // To simplify configuring, features should also affect those commands.
  private void HandleFeatureCommands()
  {
    Dictionary<int, string> hashes = Terminal.commands.Select(c => c.Key.ToLower()).ToDictionary(h => h.GetStableHashCode(), h => h);
    foreach (var section in _featurePermissions)
    {
      foreach (var feature in section.Value)
      {
        if (!hashes.ContainsKey(feature.Key))
          continue;
        if (feature.Value == FeaturePermission.No)
        {
          // Removing allowed is not needed as banned has higher priority.
          // Removing might also remove the last allowed, which would then allow all commands.
          _bannedCommands.Add(hashes[feature.Key]);
        }
        else
        {
          // If all are allowed, adding anything would disallowed other commands.
          if (_allowedCommands.Count > 0)
            _allowedCommands.Add(hashes[feature.Key]);
          _bannedCommands.Remove(hashes[feature.Key]);
        }
      }
    }
  }

  /// <summary>
  /// Resets all permissions to default (all disabled for clients, enabled for servers).
  /// </summary>
  public void ResetToDefaults()
  {
    _permissionsReceived = false;
    _featurePermissions.Clear();
    _allowedCommands.Clear();
    _bannedCommands.Clear();
  }

  private static void ParseFeature(string rawFeature, out string featureName, out FeaturePermission permission)
  {
    featureName = rawFeature?.Trim() ?? "";
    permission = FeaturePermission.Yes;
    if (featureName == "")
      return;

    int separator = featureName.LastIndexOf(':');
    if (separator < 0)
      return;

    var suffix = featureName.Substring(separator + 1).Trim();
    if (!TryParsePermission(suffix, out var parsedPermission))
      return;

    featureName = featureName.Substring(0, separator).Trim();
    permission = parsedPermission;
  }

  private static bool TryParsePermission(string value, out FeaturePermission permission)
  {
    switch (value.ToLower())
    {
      case "yes":
        permission = FeaturePermission.Yes;
        return true;
      case "no":
        permission = FeaturePermission.No;
        return true;
      case "force":
        permission = FeaturePermission.Force;
        return true;
      default:
        permission = FeaturePermission.Yes;
        return false;
    }
  }
}
