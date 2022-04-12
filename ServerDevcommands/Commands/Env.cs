namespace ServerDevcommands;
///<summary>Adds output when used without the parameter.</summary>
public class EnvCommand {
  public EnvCommand() {
    new Terminal.ConsoleCommand("env", "[value] - Prints or overrides the environment.", (Terminal.ConsoleEventArgs args) => {
      var em = EnvMan.instance;
      if (!em) return;
      if (args.Length < 2) {
        Helper.AddMessage(args.Context, $"Environment: {em.GetCurrentEnvironment()}.");
        return;
      }
      var text = string.Join(" ", args.Args, 1, args.Args.Length - 1);
      Helper.AddMessage(args.Context, $"Setting debug enviornment: {text}");
      em.m_debugEnv = text;
    }, true, true, optionsFetcher: () => ParameterInfo.Environments);
    AutoComplete.RegisterDefault("env");
  }
}
