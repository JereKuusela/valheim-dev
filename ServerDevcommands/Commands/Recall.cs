namespace ServerDevcommands;
///<summary>Uses tp command to work better.</summary>
public class RecallCommand
{
  public RecallCommand()
  {
    AutoComplete.Register("recall", (int index) =>
    {
      if (index == 0) return ParameterInfo.PublicPlayerNames; ;
      return ParameterInfo.None;
    });
    Helper.Command("recall", "[players] - Teleports players to you", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing players");
      args.Context.TryRunCommand($"tp {args[1]}");
    });
  }
}

