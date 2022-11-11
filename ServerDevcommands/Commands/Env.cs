using System.Linq;

namespace ServerDevcommands;
///<summary>Adds output when used without the parameter.</summary>
public class EnvCommand
{
  public EnvCommand()
  {
    Helper.Command("env", "[value] - Prints or overrides the environment.", (args) =>
    {
      var em = EnvMan.instance;
      if (!em) return;
      if (args.Length < 2)
      {
        Helper.AddMessage(args.Context, $"Environment: {em.GetCurrentEnvironment()}.");
        return;
      }
      var text = string.Join(" ", args.Args, 1, args.Args.Length - 1);
      if (!EnvMan.instance.m_environments.Any(env => env.m_name == text))
        text = text.Replace("_", " ");
      Helper.AddMessage(args.Context, $"Setting debug environment: {text}");
      em.m_debugEnv = text;
    }, () => ParameterInfo.Environments);
    AutoComplete.RegisterDefault("env");
  }
}
