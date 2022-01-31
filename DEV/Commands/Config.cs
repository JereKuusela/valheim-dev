namespace DEV {
  //<summary>Adds commands for changing the client and server configuration.</summary>
  public class ConfigCommand : BaseCommand {
    private void RegisterAutoComplete(string command) {
      AutoComplete.Register(command, (int index) => {
        if (index == 0) return Settings.Options;
        return null;
      });
    }
    public ConfigCommand() {
      new Terminal.ConsoleCommand("dev_config", "[key] [value] - Toggles or sets config value.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        if (args.Length == 2)
          Settings.UpdateValue(args.Context, args[1], "");
        else
          Settings.UpdateValue(args.Context, args[1], args[2]);
      }, optionsFetcher: () => Settings.Options);
      RegisterAutoComplete("dev_config");
      new Terminal.ConsoleCommand("dev_server_config", "[key] [value] - Toggles or sets config value for server.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        if (ZNet.instance.IsServer()) {
          if (args.Length == 2)
            Settings.UpdateValue(args.Context, args[1], "");
          else
            Settings.UpdateValue(args.Context, args[1], args[2]);
        } else ServerCommand.Send(args.Args);
      }, optionsFetcher: () => Settings.Options);
      RegisterAutoComplete("dev_server_config");
    }
  }
}
