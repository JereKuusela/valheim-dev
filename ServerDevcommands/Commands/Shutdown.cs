namespace ServerDevcommands;
///<summary>New command for toggling HUD.</summary>
public class ShutdownCommand
{
  public ShutdownCommand()
  {
    Helper.Command("shutdown", "- Closes the game.", static (args) =>
    {
      Helper.AddMessage(args.Context, "Shutting down...");
      Menu.instance.OnQuitYes();
    });
    AutoComplete.RegisterEmpty("shutdown");
  }
}
