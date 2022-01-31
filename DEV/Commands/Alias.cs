using System.Linq;

namespace DEV {
  ///<summary>Add commands for managing command aliases.</summary>
  public class AliasCommand : BaseCommand {

    ///<summary>Adds an alias as an actual command so it works with autocomplete, etc.</summary>
    public static void AddCommand(string key, string value) {
      var baseCommand = Aliasing.Plain(value).Split(' ').First();
      if (Terminal.commands.TryGetValue(baseCommand, out var command))
        new Terminal.ConsoleCommand(key, command.Description, command.action, command.IsCheat, command.IsNetwork, command.OnlyServer, command.IsSecret, command.AllowInDevBuild, command.m_tabOptionsFetcher);
      else
        new Terminal.ConsoleCommand(key, "", delegate (Terminal.ConsoleEventArgs args) { });
    }

    public AliasCommand() {
      new Terminal.ConsoleCommand("alias", "[name] [command] - Sets a command alias.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          args.Context.AddString(string.Join("\n", Settings.AliasKeys.Select(key => key + " -> " + Settings.GetAlias(key))));
        } else if (args.Length < 3) {
          Settings.RemoveAlias(args[1]);
          if (Terminal.commands.ContainsKey(args[1])) Terminal.commands.Remove(args[1]);
          args.Context.updateCommandList();
        } else {
          var value = string.Join(" ", args.Args.Skip(2));
          Settings.AddAlias(args[1], value);
          AddCommand(args[1], value);
          args.Context.updateCommandList();
        }
      });
      AutoComplete.Register("alias", (int index) => {
        if (index == 0) return ParameterInfo.Create("Name of the new command");
        if (index == 1) return ParameterInfo.CommandNames;
        return ParameterInfo.Create("Command values");
      });
    }

  }

}
