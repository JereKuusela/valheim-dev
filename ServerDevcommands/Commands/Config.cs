using System.Linq;
namespace ServerDevcommands;
//<summary>Adds commands for changing the configuration.</summary>
public class ConfigCommand
{
  private void RegisterAutoComplete(string command)
  {
    AutoComplete.Register(command, (int index) =>
    {
      if (index == 0) return Settings.Options;
      return ParameterInfo.Create("Value");
    });
  }
  public ConfigCommand()
  {
    new Terminal.ConsoleCommand("dev_config", "[key] [value] - Toggles or sets config value.", (args) =>
    {
      if (args.Length < 2) return;
      if (args.Length == 2)
        Settings.UpdateValue(args.Context, args[1], "");
      else
        Settings.UpdateValue(args.Context, args[1], string.Join(" ", args.Args.Skip(2)));
    }, optionsFetcher: () => Settings.Options);
    RegisterAutoComplete("dev_config");
  }
}
