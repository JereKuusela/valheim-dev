using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerDevcommands;

public class PermissionApi
{
  public static event Action? PermissionsUpdated;

  public static bool IsFeatureEnabled(string section, string feature, bool localConfigValue) => IsFeatureEnabledByHash(section, feature.ToLower().GetStableHashCode(), localConfigValue);
  public static bool IsFeatureEnabledByHash(string section, int featureHash, bool localConfigValue)
  {
    return PermissionManager.Instance.IsFeatureEnabledByHash(section, featureHash, localConfigValue);
  }
  public static bool IsCommandAllowed(string commandName)
  {
    var kvp = Parse.Kvp(commandName, ' ');
    var cmdName = kvp.Key.ToLower();
    var cmd = Terminal.commands.FirstOrDefault(c => c.Key.Equals(cmdName, StringComparison.OrdinalIgnoreCase)).Value;
    if (cmd == null)
      return false;
    return PermissionManager.Instance.IsCommandAllowed(cmd, commandName);
  }

  public static void Subscribe(Action handler)
  {
    PermissionsUpdated += handler;
  }

  public static void Unsubscribe(Action handler)
  {
    PermissionsUpdated -= handler;
  }

  internal static void Notify()
  {
    PermissionsUpdated?.Invoke();
  }
}

public class PermissionManager
{
  public enum FeaturePermission
  {
    Unknown = -1,
    Yes = 0,
    No = 1,
    Force = 2,
  }
#nullable disable
  private static PermissionManager _instance;
#nullable enable
  public static PermissionManager Instance
  {
    get
    {
      if (_instance == null)
        _instance = new PermissionManager(false);
      return _instance;
    }
  }

  private Dictionary<string, Dictionary<int, FeaturePermission>> _featurePermissions = [];
  private bool _isAdmin = false;
  private bool CanCheat => _isAdmin || !Helper.IsClient();


  public PermissionManager(bool isAdmin)
  {
    _isAdmin = isAdmin;

  }
  public void SetAdmin(bool isAdmin)
  {
    _isAdmin = isAdmin;
    PermissionApi.Notify();
  }

  public void AddEntry(PermissionEntry entry)
  {
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

    if (entry.commands != null)
    {
      foreach (var rawCommand in entry.commands)
      {
        ParseFeature(rawCommand, out var commandName, out var permission);
        if (commandName == "")
          continue;
        var cmd = NormalizeCommand(commandName);
        if (permission == FeaturePermission.No)
          _bannedCommands.Add(cmd);
        else
          _allowedCommands.Add(cmd);
      }
    }
  }

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
      _ => localConfigValue && CanCheat,
    };
  }

  public FeaturePermission GetFeaturePermissionByHash(string section, int featureHash)
  {
    section = section.ToLower();

    if (!_featurePermissions.TryGetValue(section, out var features))
      return FeaturePermission.Unknown;

    return features.TryGetValue(featureHash, out var permission) ? permission : FeaturePermission.Unknown;
  }

  // ====================
  // Command Permissions
  // ====================

  private List<string> _allowedCommands = [];
  private List<string> _bannedCommands = [];

  private static string NormalizeCommand(string commandName) => commandName?.Trim().ToLowerInvariant() ?? "";


  private static bool StartsWithAny(List<string> commands, string cmd)
    => commands.Any(check => cmd.StartsWith(check, StringComparison.Ordinal));

  public bool IsCommandAllowed(Terminal.ConsoleCommand cmd, string commandName)
  {
    var normalized = NormalizeCommand(commandName);
    if (normalized == "")
      return false;

    if (StartsWithAny(_bannedCommands, normalized))
      return false;

    // Reguar valid check to allow non-cheat commands or all commands for admins.
    // Unless explicitly banned.
    if (cmd.IsValid(Console.instance))
      return true;

    if (StartsWithAny(_allowedCommands, normalized))
      return true;

    return CanCheat;
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
    pkg.Write(_isAdmin);

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

    _isAdmin = pkg.ReadBool();
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
      _allowedCommands.Add(NormalizeCommand(pkg.ReadString()));

    int bannedCommandCount = pkg.ReadInt();
    _bannedCommands.Clear();
    for (int i = 0; i < bannedCommandCount; i++)
      _bannedCommands.Add(NormalizeCommand(pkg.ReadString()));

    HandleFeatureCommands();
    PermissionApi.Notify();

  }

  // Some features have console command that is used to toggle them.
  // To simplify configuring, features should also affect those commands.
  public void HandleFeatureCommands()
  {
    Dictionary<int, string> hashes = Terminal.commands.Select(c => c.Key.ToLower()).ToDictionary(h => h.GetStableHashCode(), h => h);
    foreach (var section in _featurePermissions)
    {
      foreach (var feature in section.Value)
      {
        if (!hashes.ContainsKey(feature.Key))
          continue;
        var cmd = NormalizeCommand(hashes[feature.Key]);
        if (feature.Value == FeaturePermission.No)
        {
          _allowedCommands.Remove(cmd);
          _bannedCommands.Add(cmd);
        }
        else
        {
          _allowedCommands.Add(cmd);
          _bannedCommands.Remove(cmd);
        }
      }
    }
  }

  public void ResetToDefaults()
  {
    _isAdmin = false;
    _featurePermissions.Clear();
    _allowedCommands.Clear();
    _bannedCommands.Clear();
  }

  private static void ParseFeature(string rawFeature, out string featureName, out FeaturePermission permission)
  {
    var kvp = Parse.Kvp(rawFeature, ':');
    featureName = kvp.Key.Trim();
    permission = ParsePermission(kvp.Value);
  }

  private static FeaturePermission ParsePermission(string value)
  {
    return value.ToLowerInvariant() switch
    {
      "yes" => FeaturePermission.Yes,
      "no" => FeaturePermission.No,
      "force" => FeaturePermission.Force,
      _ => FeaturePermission.Yes,
    };
  }
}
