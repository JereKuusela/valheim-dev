using System.Linq;
namespace ServerDevcommands;
///<summary>Add commands for managing command aliases.</summary>
public class AliasCommand
{
  public AliasCommand()
  {
    new Terminal.ConsoleCommand("alias", "[name] [command] - Sets a command alias.", (args) =>
    {
      if (args.Length < 2)
      {
        args.Context.AddString(string.Join("\n", AliasManager.AliasKeys.Select(key => key + " -> " + AliasManager.GetAliasValue(key))));
      }
      else if (args.Length < 3)
      {
        AliasManager.RemoveAlias(args[1]);
        if (Terminal.commands.ContainsKey(args[1])) Terminal.commands.Remove(args[1]);
        args.Context.updateCommandList();
      }
      else
      {
        var value = string.Join(" ", args.Args.Skip(2));
        AliasManager.AddAlias(args[1], value);
        args.Context.updateCommandList();
      }
    });
    AutoComplete.Register("alias", (index, subIndex) =>
    {
      if (index == 0) return ParameterInfo.Create("Name of the alias.");
      return ParameterInfo.None;
    });
    AutoComplete.Offsets["alias"] = 1;
  }
}
