namespace DEV {
  public class ConfigCommand : BaseCommands {

    public ConfigCommand() {
      new Terminal.ConsoleCommand("dev_config", "[key] [value] - Toggles or sets config value.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        if (args.Length == 2)
          Settings.UpdateValue(args.Context, args[1], "");
        else
          Settings.UpdateValue(args.Context, args[1], args[2]);
      }, optionsFetcher: () => Settings.Options);
    }

  }
}
