namespace ServerDevcommands;
///<summary>New command for toggling HUD.</summary>
public class HUDCommand
{
  public HUDCommand()
  {
    new Terminal.ConsoleCommand("hud", "[value] -  Toggles or sets the HUD visibility.", (args) =>
    {
      if (!Hud.m_instance)
      {
        Helper.AddMessage(args.Context, "Error: No HUD instance.");
        return;
      }
      if (args.Length >= 2) Hud.m_instance.m_userHidden = args[1] != "1";
      else Hud.m_instance.m_userHidden = !Hud.m_instance.m_userHidden;
      var str = Hud.m_instance.m_userHidden ? "disabled" : "enabled";
      Helper.AddMessage(args.Context, $"Hud {str}.");
    });
    AutoComplete.Register("hud", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("1 = enable, 0 = disable, no value = toggle");
      return ParameterInfo.None;
    });
  }
}
