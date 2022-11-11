using System.Linq;
namespace ServerDevcommands;
///<summary>Add commands for managing command aliases.</summary>
public class AliasCommand
{

  ///<summary>Adds an alias as an actual command so it works with autocomplete, etc.</summary>
  public static void AddCommand(string key, string value)
  {
    var plain = Aliasing.Plain(value);
    var baseCommand = plain.Split(' ').First();
    if (Terminal.commands.TryGetValue(baseCommand, out var command))
      new Terminal.ConsoleCommand(key, plain, command.action, command.IsCheat, command.IsNetwork, command.OnlyServer, command.IsSecret, command.AllowInDevBuild, command.m_tabOptionsFetcher);
    else
      new Terminal.ConsoleCommand(key, plain, (args) => { });
  }

  public AliasCommand()
  {
    new Terminal.ConsoleCommand("alias", "[name] [command] - Sets a command alias.", (args) =>
    {
      if (args.Length < 2)
      {
        args.Context.AddString(string.Join("\n", Settings.AliasKeys.Select(key => key + " -> " + Settings.GetAliasValue(key))));
      }
      else if (args.Length < 3)
      {
        Settings.RemoveAlias(args[1]);
        if (Terminal.commands.ContainsKey(args[1])) Terminal.commands.Remove(args[1]);
        args.Context.updateCommandList();
        AliasManager.ToBeSaved = true;
      }
      else
      {
        var value = string.Join(" ", args.Args.Skip(2));
        Settings.AddAlias(args[1], value);
        AddCommand(args[1], value);
        args.Context.updateCommandList();
        AliasManager.ToBeSaved = true;
      }
    });
    AutoComplete.Register("alias", (int index, int subIndex) =>
    {
      if (index == 0) return ParameterInfo.Create("Name of the alias.");
      return ParameterInfo.None;
    });
  }
}
