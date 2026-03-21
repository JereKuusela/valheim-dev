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

    // Parse features from new format: Dictionary<string, List<string>>
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

    // Parse commands
    if (entry.commands != null && entry.commands.Count > 0)
    {
      _allowedCommands = [.. entry.commands.Select(c => c.ToLower())];
    }
    else
    {
      _allowedCommands = null; // All commands allowed
    }
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

  private HashSet<string> _allowedCommands = null; // null = all commands allowed

  /// <summary>
  /// Checks if a command is allowed to be executed.
  /// </summary>
  /// <param name="commandName">The name of the command (case-insensitive).</param>
  /// <returns>True if the command is allowed, false otherwise.</returns>
  public bool IsCommandAllowed(string commandName)
  {
    if (_allowedCommands == null)
      return true; // null = all commands allowed

    return _allowedCommands.Contains(commandName.ToLower());
  }

  /// <summary>
  /// Sets the list of allowed commands. If null, all commands are allowed.
  /// </summary>
  /// <param name="commands">List of allowed command names, or null to allow all.</param>
  public void SetAllowedCommands(HashSet<string> commands)
  {
    if (commands == null)
    {
      _allowedCommands = null;
    }
    else
    {
      _allowedCommands = new HashSet<string>();
      foreach (var cmd in commands)
        _allowedCommands.Add(cmd.ToLower());
    }
  }

  /// <summary>
  /// Adds a command to the allowed list.
  /// </summary>
  /// <param name="commandName">The name of the command to allow.</param>
  public void AllowCommand(string commandName)
  {
    if (_allowedCommands == null)
      _allowedCommands = new HashSet<string>();

    _allowedCommands.Add(commandName.ToLower());
  }

  /// <summary>
  /// Removes a command from the allowed list.
  /// </summary>
  /// <param name="commandName">The name of the command to disallow.</param>
  public void DisallowCommand(string commandName)
  {
    if (_allowedCommands == null)
      return;

    _allowedCommands.Remove(commandName.ToLower());
  }

  /// <summary>
  /// Gets a copy of the allowed commands set. Returns null if all commands are allowed.
  /// </summary>
  public HashSet<string> GetAllowedCommands()
  {
    if (_allowedCommands == null)
      return null;

    return new HashSet<string>(_allowedCommands);
  }

  /// <summary>
  /// Clears all command restrictions (allows all commands).
  /// </summary>
  public void AllowAllCommands()
  {
    _allowedCommands = null;
  }

  // ====================
  // Serialization for RPC
  // ====================

  /// <summary>
  /// Writes permissions to a ZPackage for network transmission.
  /// Format: [section count] for each section: [section] [feature count] [feature hash] [feature permission int]... [command count] [command names...]
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

    // Write allowed commands
    if (_allowedCommands == null)
    {
      pkg.Write(-1); // -1 means all commands allowed
    }
    else
    {
      pkg.Write(_allowedCommands.Count);
      foreach (var cmd in _allowedCommands)
        pkg.Write(cmd);
    }
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
    if (commandCount == -1)
    {
      _allowedCommands = null; // All commands allowed
    }
    else
    {
      _allowedCommands = new HashSet<string>();
      for (int i = 0; i < commandCount; i++)
      {
        _allowedCommands.Add(pkg.ReadString());
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
    _allowedCommands = null;
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
