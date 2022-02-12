namespace ServerDevcommands {

  ///<summary>Adds support for directly setting the value and makes it work without needing the raven appear first.</summary>
  public class TutorialToggleCommand {
    public TutorialToggleCommand() {
      new Terminal.ConsoleCommand("tutorialtoggle", "[value] -  Toggles or sets Hugin hints.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length >= 2) Raven.m_tutorialsEnabled = args[1] == "1";
        else Raven.m_tutorialsEnabled = !Raven.m_tutorialsEnabled;
        var str = Raven.m_tutorialsEnabled ? "enabled" : "disabled";
        Helper.AddMessage(args.Context, $"Tutorials {str}.");
      });
      AutoComplete.Register("tutorialtoggle", (int index) => {
        if (index == 0) return ParameterInfo.Create("1 = enable, 0 = disable, no value = toggle");
        return null;
      });
    }
  }
}
