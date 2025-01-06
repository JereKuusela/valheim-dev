namespace ServerDevcommands;
///<summary>Adds output when used without the parameter.</summary>
public class WindCommand
{
  public WindCommand()
  {
    new Terminal.ConsoleCommand("wind", "[angle] [intensity] - Prints or overrides the wind.", (args) =>
    {
      var em = EnvMan.instance;
      if (!em) return;
      if (args.Length < 3)
      {
        Helper.AddMessage(args.Context, $"Wind: {em.GetWindIntensity()}.");
        return;
      }
      var angle = Parse.Float(args[1], 0f);
      var intensity = Parse.Float(args[2], 0f);
      EnvMan.instance.SetDebugWind(angle, intensity);
    }, true, true);
    AutoComplete.Register("wind", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Angle", "a number (from -360.0 to 360.0)");
      if (index == 1) return ParameterInfo.Create("Intensity", "a positive number (from 0.0 to 1.0)");
      return ParameterInfo.None;
    });
  }
}
