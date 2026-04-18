using System;
using System.Collections.Generic;
using System.Linq;
using Service;

namespace ServerDevcommands;

/// <summary>Utility command for editing runtime permission entries.</summary>
public class PermissionsCommand
{
  private static readonly List<string> Operations = [
    "add_group",
    "remove_group",
    "clear_groups",
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

  private static bool IsMatchingGroup(string raw, string group)
  {
    return raw.Trim().Equals(group.Trim(), StringComparison.OrdinalIgnoreCase);
  }

  private static bool AddGroup(PermissionEntry entry, string group)
  {
    group = group.Trim();
    if (group == "")
      throw new InvalidOperationException("Missing group name.");

    entry.groups ??= [];
    if (entry.groups.Any(raw => IsMatchingGroup(raw, group)))
      return false;
    entry.groups.Add(group);
    return true;
  }

  private static bool RemoveGroup(PermissionEntry entry, string group)
  {
    group = group.Trim();
    if (group == "")
      throw new InvalidOperationException("Missing group name.");
    if (entry.groups == null)
      return false;

    var oldCount = entry.groups.Count;
    entry.groups.RemoveAll(raw => IsMatchingGroup(raw, group));
    if (entry.groups.Count == 0)
      entry.groups = null;
    return oldCount != (entry.groups?.Count ?? 0);
  }

  private static bool ClearGroups(PermissionEntry entry)
  {
    if (entry.groups == null || entry.groups.Count == 0)
      return false;
    entry.groups = null;
    return true;
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

  private static string JoinArgs(Terminal.ConsoleEventArgs args, int startIndex)
  {
    if (args.Length <= startIndex)
      return "";
    return string.Join(" ", args.Args.Skip(startIndex)).Trim();
  }

  private static (string Hostname, string CharacterId, string DisplayName) ResolvePlayerTarget(string target)
  {
    var players = FindTargetPlayers(target);
    if (players.Count > 1)
      throw new InvalidOperationException($"Target '{target}' matched multiple online players.");
    if (players.Count == 1)
    {
      var player = players[0];
      var characterId = ZDOMan.instance.GetZDO(player.ZDOID)?.GetLong(ZDOVars.s_playerID).ToString();
      if (characterId == null)
        throw new InvalidOperationException($"Unable to resolve character id for player '{player.Name}'.");
      return (player.HostId, characterId, player.Name);
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
    var (Hostname, CharacterId, DisplayName) = ResolvePlayerTarget(target);
    var entry = PermissionLoader.GetOrCreatePeerEntry(Hostname, CharacterId, DisplayName)
      ?? throw new InvalidOperationException("Unable to create permission entry.");
    var changed = update(entry);
    if (changed)
    {
      PermissionLoader.Save();
      PermissionLoader.SendPeerPermissions(Hostname, CharacterId);
    }

    var key = PermissionData.PeerKey(Hostname, CharacterId);
    Helper.AddMessage(args.Context, changed
      ? $"Permissions updated for {DisplayName} ({key})."
      : $"No permission changes for {DisplayName} ({key}).");
  }

  private static void Handle(Terminal.ConsoleEventArgs args)
  {
    if (args.Length < 2)
      throw new InvalidOperationException("Missing operation. Use add_group, remove_group, clear_groups, add_command, ban_command, clear_command, add_feature, ban_feature, force_feature, clear_feature, clear_all.");

    if (!ZNet.instance.IsServer())
    {
      ServerExecution.Send(args);
      return;
    }

    var operation = args[1].ToLowerInvariant();
    switch (operation)
    {
      case "add_group":
        {
          Helper.ArgsCheck(args, 4, "Usage: permissions add_group <group> <character id or name>");
          var group = args[2].Trim();
          var target = JoinArgs(args, 3);
          if (target == "")
            throw new InvalidOperationException("Missing target character id or name.");
          HandlePlayerEdit(args, target, entry => AddGroup(entry, group));
          return;
        }
      case "remove_group":
        {
          Helper.ArgsCheck(args, 4, "Usage: permissions remove_group <group> <character id or name>");
          var group = args[2].Trim();
          var target = JoinArgs(args, 3);
          if (target == "")
            throw new InvalidOperationException("Missing target character id or name.");
          HandlePlayerEdit(args, target, entry => RemoveGroup(entry, group));
          return;
        }
      case "clear_groups":
        {
          Helper.ArgsCheck(args, 3, "Usage: permissions clear_groups <character id or name>");
          var target = JoinArgs(args, 2);
          if (target == "")
            throw new InvalidOperationException("Missing target character id or name.");
          HandlePlayerEdit(args, target, ClearGroups);
          return;
        }
      case "add_command":
        {
          Helper.ArgsCheck(args, 4, "Usage: permissions add_command <command> <character id or name>");
          var target = JoinArgs(args, 3);
          HandlePlayerEdit(args, target, entry => SetCommandPermission(entry, args[2], PermissionManager.FeaturePermission.Yes));
          return;
        }
      case "ban_command":
        {
          Helper.ArgsCheck(args, 4, "Usage: permissions ban_command <command> <character id or name>");
          var target = JoinArgs(args, 3);
          HandlePlayerEdit(args, target, entry => SetCommandPermission(entry, args[2], PermissionManager.FeaturePermission.No));
          return;
        }
      case "clear_command":
        {
          Helper.ArgsCheck(args, 4, "Usage: permissions clear_command <command> <character id or name>");
          var target = JoinArgs(args, 3);
          HandlePlayerEdit(args, target, entry => ClearCommand(entry, args[2]));
          return;
        }
      case "add_feature":
        {
          Helper.ArgsCheck(args, 5, "Usage: permissions add_feature <section> <feature> <character id or name>");
          var target = JoinArgs(args, 4);
          HandlePlayerEdit(args, target, entry => SetFeaturePermission(entry, args[2], args[3], PermissionManager.FeaturePermission.Yes));
          return;
        }
      case "ban_feature":
        {
          Helper.ArgsCheck(args, 5, "Usage: permissions ban_feature <section> <feature> <character id or name>");
          var target = JoinArgs(args, 4);
          HandlePlayerEdit(args, target, entry => SetFeaturePermission(entry, args[2], args[3], PermissionManager.FeaturePermission.No));
          return;
        }
      case "force_feature":
        {
          Helper.ArgsCheck(args, 5, "Usage: permissions force_feature <section> <feature> <character id or name>");
          var target = JoinArgs(args, 4);
          HandlePlayerEdit(args, target, entry => SetFeaturePermission(entry, args[2], args[3], PermissionManager.FeaturePermission.Force));
          return;
        }
      case "clear_feature":
        {
          Helper.ArgsCheck(args, 5, "Usage: permissions clear_feature <section> <feature> <character id or name>");
          var target = JoinArgs(args, 4);
          HandlePlayerEdit(args, target, entry => ClearFeature(entry, args[2], args[3]));
          return;
        }
      case "clear_all":
        {
          Helper.ArgsCheck(args, 3, "Usage: permissions clear_all <character id or name>");
          var target = JoinArgs(args, 2);
          HandlePlayerEdit(args, target, entry =>
          {
            if (entry.groups == null && entry.commands == null && entry.features == null)
              return false;
            entry.groups = null;
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

  private static List<string> GetOperationSpecificHints(int index, int _, string[] args)
  {
    if (index == 0)
      return Operations;

    var operation = args.Length > 0 ? args[0].Trim().ToLowerInvariant() : "";

    if (operation == "add_group" || operation == "remove_group")
    {
      if (index == 1) return ParameterInfo.Create("Group name");
      if (index >= 2) return ParameterInfo.PlayerNames;
      return ParameterInfo.None;
    }

    if (operation == "clear_groups" || operation == "clear_all")
    {
      if (index >= 1) return ParameterInfo.PlayerNames;
      return ParameterInfo.None;
    }

    if (operation == "add_command" || operation == "ban_command" || operation == "clear_command")
    {
      if (index == 1) return ParameterInfo.Create("Command name");
      if (index >= 2) return ParameterInfo.PlayerNames;
      return ParameterInfo.None;
    }

    if (operation == "add_feature" || operation == "ban_feature" || operation == "force_feature" || operation == "clear_feature")
    {
      if (index == 1) return ParameterInfo.Create("Feature section");
      if (index == 2) return ParameterInfo.Create("Feature name");
      if (index >= 3) return ParameterInfo.PlayerNames;
      return ParameterInfo.None;
    }

    return ParameterInfo.Create("Unknown operation. Use: add_group, remove_group, clear_groups, add_command, ban_command, clear_command, add_feature, ban_feature, force_feature, clear_feature, clear_all");
  }

  public PermissionsCommand()
  {
    Helper.Command("permissions", "[operation] - Manage player permission overrides.", Handle);
    AutoComplete.Register("permissions", GetOperationSpecificHints);
  }
}
