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
    var key = PermissionData.PeerKey(Hostname, CharacterId);
    var entry = PermissionLoader.GetOrCreatePeerEntry(Hostname, CharacterId, DisplayName)
      ?? throw new InvalidOperationException("Unable to create permission entry.");
    var changed = update(entry);
    if (changed)
    {
      PermissionLoader.Save();
      PermissionLoader.SendPeerPermissions(Hostname, CharacterId);
    }

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
          HandlePlayerEdit(args, target, entry => PermissionLoader.Data.AddGroup(entry, group));
          return;
        }
      case "remove_group":
        {
          Helper.ArgsCheck(args, 4, "Usage: permissions remove_group <group> <character id or name>");
          var group = args[2].Trim();
          var target = JoinArgs(args, 3);
          if (target == "")
            throw new InvalidOperationException("Missing target character id or name.");
          HandlePlayerEdit(args, target, entry => PermissionLoader.Data.RemoveGroup(entry, group));
          return;
        }
      case "clear_groups":
        {
          Helper.ArgsCheck(args, 3, "Usage: permissions clear_groups <character id or name>");
          var target = JoinArgs(args, 2);
          if (target == "")
            throw new InvalidOperationException("Missing target character id or name.");
          HandlePlayerEdit(args, target, entry => PermissionLoader.Data.ClearGroups(entry));
          return;
        }
      case "add_command":
        {
          Helper.ArgsCheck(args, 4, "Usage: permissions add_command <command> <character id or name>");
          var target = JoinArgs(args, 3);
          HandlePlayerEdit(args, target, entry => PermissionLoader.Data.SetCommandPermission(entry, args[2], PermissionManager.FeaturePermission.Yes));
          return;
        }
      case "ban_command":
        {
          Helper.ArgsCheck(args, 4, "Usage: permissions ban_command <command> <character id or name>");
          var target = JoinArgs(args, 3);
          HandlePlayerEdit(args, target, entry => PermissionLoader.Data.SetCommandPermission(entry, args[2], PermissionManager.FeaturePermission.No));
          return;
        }
      case "clear_command":
        {
          Helper.ArgsCheck(args, 4, "Usage: permissions clear_command <command> <character id or name>");
          var target = JoinArgs(args, 3);
          HandlePlayerEdit(args, target, entry => PermissionLoader.Data.ClearCommand(entry, args[2]));
          return;
        }
      case "add_feature":
        {
          Helper.ArgsCheck(args, 5, "Usage: permissions add_feature <section> <feature> <character id or name>");
          var target = JoinArgs(args, 4);
          HandlePlayerEdit(args, target, entry => PermissionLoader.Data.SetFeaturePermission(entry, args[2], args[3], PermissionManager.FeaturePermission.Yes));
          return;
        }
      case "ban_feature":
        {
          Helper.ArgsCheck(args, 5, "Usage: permissions ban_feature <section> <feature> <character id or name>");
          var target = JoinArgs(args, 4);
          HandlePlayerEdit(args, target, entry => PermissionLoader.Data.SetFeaturePermission(entry, args[2], args[3], PermissionManager.FeaturePermission.No));
          return;
        }
      case "force_feature":
        {
          Helper.ArgsCheck(args, 5, "Usage: permissions force_feature <section> <feature> <character id or name>");
          var target = JoinArgs(args, 4);
          HandlePlayerEdit(args, target, entry => PermissionLoader.Data.SetFeaturePermission(entry, args[2], args[3], PermissionManager.FeaturePermission.Force));
          return;
        }
      case "clear_feature":
        {
          Helper.ArgsCheck(args, 5, "Usage: permissions clear_feature <section> <feature> <character id or name>");
          var target = JoinArgs(args, 4);
          HandlePlayerEdit(args, target, entry => PermissionLoader.Data.ClearFeature(entry, args[2], args[3]));
          return;
        }
      case "clear_all":
        {
          Helper.ArgsCheck(args, 3, "Usage: permissions clear_all <character id or name>");
          var target = JoinArgs(args, 2);
          HandlePlayerEdit(args, target, entry => PermissionLoader.Data.ClearAll(entry));
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
