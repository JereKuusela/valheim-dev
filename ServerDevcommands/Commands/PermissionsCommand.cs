using System;
using System.Collections.Generic;
using System.Linq;
using Service;

namespace ServerDevcommands;

/// <summary>Utility command for editing runtime permission entries.</summary>
public class PermissionsCommand
{
  private static readonly List<string> Operations = [
    "set_group",
    "clear_group",
    "add_command",
    "ban_command",
    "clear_command",
    "add_feature",
    "ban_feature",
    "force_feature",
    "clear_feature",
    "clear_all"
  ];

  private static string PermissionToSuffix(PermissionManager.FeaturePermission permission)
  {
    return permission switch
    {
      PermissionManager.FeaturePermission.No => "no",
      PermissionManager.FeaturePermission.Force => "force",
      _ => "yes"
    };
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

  private static bool SetFeaturePermission(PermissionEntry entry, string section, string feature, PermissionManager.FeaturePermission permission)
  {
    section = section.Trim().ToLowerInvariant();
    feature = feature.Trim();
    if (section == "" || feature == "")
      throw new InvalidOperationException("Missing section or feature name.");

    entry.features ??= [];
    if (!entry.features.TryGetValue(section, out var features))
    {
      features = [];
      entry.features[section] = features;
    }

    var suffix = PermissionToSuffix(permission);
    var replacement = suffix == "yes" ? feature : $"{feature}: {suffix}";
    var already = features.Any(raw => raw.Equals(replacement, StringComparison.OrdinalIgnoreCase));
    var oldCount = features.Count;
    features.RemoveAll(raw => IsMatchingFeature(raw, feature));
    features.Add(replacement);
    return !already || oldCount != features.Count;
  }

  private static bool ClearFeature(PermissionEntry entry, string section, string feature)
  {
    section = section.Trim().ToLowerInvariant();
    feature = feature.Trim();
    if (section == "" || feature == "")
      throw new InvalidOperationException("Missing section or feature name.");
    if (entry.features == null)
      return false;
    if (!entry.features.TryGetValue(section, out var features))
      return false;

    var oldCount = features.Count;
    features.RemoveAll(raw => IsMatchingFeature(raw, feature));
    if (features.Count == 0)
      entry.features.Remove(section);
    if (entry.features.Count == 0)
      entry.features = null;

    return oldCount != features.Count;
  }

  private static bool SetCommandPermission(PermissionEntry entry, string command, PermissionManager.FeaturePermission permission)
  {
    command = command.Trim();
    if (command == "")
      throw new InvalidOperationException("Missing command name.");

    entry.commands ??= [];
    var suffix = PermissionToSuffix(permission);
    var replacement = suffix == "yes" ? command : $"{command}: {suffix}";
    var already = entry.commands.Any(raw => raw.Equals(replacement, StringComparison.OrdinalIgnoreCase));
    var oldCount = entry.commands.Count;
    entry.commands.RemoveAll(raw => IsMatchingCommand(raw, command));
    entry.commands.Add(replacement);
    return !already || oldCount != entry.commands.Count;
  }

  private static bool ClearCommand(PermissionEntry entry, string command)
  {
    command = command.Trim();
    if (command == "")
      throw new InvalidOperationException("Missing command name.");
    if (entry.commands == null)
      return false;

    var oldCount = entry.commands.Count;
    entry.commands.RemoveAll(raw => IsMatchingCommand(raw, command));
    var newCount = entry.commands.Count;
    if (entry.commands.Count == 0)
      entry.commands = null;
    return oldCount != newCount;
  }

  private static List<PlayerInfo> FindTargetPlayers(string target)
  {
    try
    {
      return PlayerInfo.FindPlayers([target]);
    }
    catch
    {
      return [];
    }
  }

  private static (string Hostname, string CharacterId, string DisplayName) ResolvePlayerTarget(string target)
  {
    var players = FindTargetPlayers(target);
    if (players.Count > 1)
      throw new InvalidOperationException($"Target '{target}' matched multiple online players.");
    if (players.Count == 1)
    {
      var player = players[0];
      return (player.HostId, player.PeerId.ToString(), player.Name);
    }

    var matches = PermissionLoader.Data.Entries
      .Where(entry => entry.character == target || entry.name.Equals(target, StringComparison.OrdinalIgnoreCase))
      .Where(entry => !string.IsNullOrWhiteSpace(entry.id) && !string.IsNullOrWhiteSpace(entry.character))
      .ToList();

    if (matches.Count == 0)
      throw new InvalidOperationException($"Unable to find player by id/name '{target}'.");
    if (matches.Count > 1)
      throw new InvalidOperationException($"Target '{target}' is ambiguous. Use a unique character id.");

    var match = matches[0];
    var displayName = string.IsNullOrWhiteSpace(match.name) ? match.character : match.name;
    return (match.id, match.character, displayName);
  }

  private static void HandlePlayerEdit(Terminal.ConsoleEventArgs args, string target, Func<PermissionEntry, bool> update)
  {
    var resolved = ResolvePlayerTarget(target);
    var entry = PermissionLoader.GetOrCreatePeerEntry(resolved.Hostname, resolved.CharacterId, resolved.DisplayName)
      ?? throw new InvalidOperationException("Unable to create permission entry.");
    var changed = update(entry);
    if (changed)
    {
      PermissionLoader.Save();
      PermissionLoader.SendPeerPermissions(resolved.Hostname, resolved.CharacterId);
    }

    var key = PermissionData.PeerKey(resolved.Hostname, resolved.CharacterId);
    Helper.AddMessage(args.Context, changed
      ? $"Permissions updated for {resolved.DisplayName} ({key})."
      : $"No permission changes for {resolved.DisplayName} ({key}).");
  }

  private static void Handle(Terminal.ConsoleEventArgs args)
  {
    if (args.Length < 2)
      throw new InvalidOperationException("Missing operation. Use set_group, clear_group, add_command, ban_command, clear_command, add_feature, ban_feature, force_feature, clear_feature, clear_all.");

    if (!ZNet.instance.IsServer())
    {
      ServerExecution.Send(args);
      return;
    }

    var operation = args[1].ToLowerInvariant();
    switch (operation)
    {
      case "set_group":
        {
          Helper.ArgsCheck(args, 4, "Usage: permissions set_group <character id or name> <group>");
          var target = args[2];
          var group = args[3].Trim();
          HandlePlayerEdit(args, target, entry =>
          {
            if (entry.group == group) return false;
            entry.group = group;
            return true;
          });
          return;
        }
      case "clear_group":
        {
          Helper.ArgsCheck(args, 3, "Usage: permissions clear_group <character id or name>");
          HandlePlayerEdit(args, args[2], entry =>
          {
            if (entry.group == "") return false;
            entry.group = "";
            return true;
          });
          return;
        }
      case "add_command":
        {
          Helper.ArgsCheck(args, 4, "Usage: permissions add_command <command> <character id or name>");
          HandlePlayerEdit(args, args[3], entry => SetCommandPermission(entry, args[2], PermissionManager.FeaturePermission.Yes));
          return;
        }
      case "ban_command":
        {
          Helper.ArgsCheck(args, 4, "Usage: permissions ban_command <command> <character id or name>");
          HandlePlayerEdit(args, args[3], entry => SetCommandPermission(entry, args[2], PermissionManager.FeaturePermission.No));
          return;
        }
      case "clear_command":
        {
          Helper.ArgsCheck(args, 4, "Usage: permissions clear_command <command> <character id or name>");
          HandlePlayerEdit(args, args[3], entry => ClearCommand(entry, args[2]));
          return;
        }
      case "add_feature":
        {
          Helper.ArgsCheck(args, 5, "Usage: permissions add_feature <section> <feature> <character id or name>");
          HandlePlayerEdit(args, args[4], entry => SetFeaturePermission(entry, args[2], args[3], PermissionManager.FeaturePermission.Yes));
          return;
        }
      case "ban_feature":
        {
          Helper.ArgsCheck(args, 5, "Usage: permissions ban_feature <section> <feature> <character id or name>");
          HandlePlayerEdit(args, args[4], entry => SetFeaturePermission(entry, args[2], args[3], PermissionManager.FeaturePermission.No));
          return;
        }
      case "force_feature":
        {
          Helper.ArgsCheck(args, 5, "Usage: permissions force_feature <section> <feature> <character id or name>");
          HandlePlayerEdit(args, args[4], entry => SetFeaturePermission(entry, args[2], args[3], PermissionManager.FeaturePermission.Force));
          return;
        }
      case "clear_feature":
        {
          Helper.ArgsCheck(args, 5, "Usage: permissions clear_feature <section> <feature> <character id or name>");
          HandlePlayerEdit(args, args[4], entry => ClearFeature(entry, args[2], args[3]));
          return;
        }
      case "clear_all":
        {
          Helper.ArgsCheck(args, 3, "Usage: permissions clear_all <character id or name>");
          HandlePlayerEdit(args, args[2], entry =>
          {
            if (entry.commands == null && entry.features == null)
              return false;
            entry.commands = null;
            entry.features = null;
            return true;
          });
          return;
        }
      default:
        throw new InvalidOperationException($"Unknown operation '{operation}'.");
    }
  }

  public PermissionsCommand()
  {
    Helper.Command("permissions", "[operation] - Manage player permission overrides.", Handle);
    AutoComplete.Register("permissions", index =>
    {
      if (index == 0) return Operations;
      if (index == 1) return ParameterInfo.Create("Target / Value");
      if (index == 2) return ParameterInfo.Create("Value / Target");
      if (index == 3) return ParameterInfo.Create("Target");
      return ParameterInfo.None;
    });
  }
}
