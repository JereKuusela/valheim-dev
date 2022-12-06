namespace ServerDevcommands;
///<summary>Resetting a skill. Bit convoluted to support mods adding new skills.</summary>
public class ResetSkillCommand
{
  public ResetSkillCommand()
  {
    AutoComplete.Register("resetskill", (int index) =>
    {
      var fetcher = Terminal.commands["resetskill"].m_tabOptionsFetcher;
      if (index == 0) return fetcher();
      return ParameterInfo.None;
    });
    var originalFetcher = Terminal.commands["raiseskill"].m_tabOptionsFetcher;
    var newFetcher = () =>
    {
      var options = originalFetcher();
      if (!options.Contains("*"))
        options.Add("*");
      return options;
    };
    Helper.Command("resetskill", "[skill] - Resets a skill level.", (args) =>
    {
      var player = Helper.GetPlayer();
      Helper.ArgsCheck(args, 2, "Missing the skill.");
      if (args[1] == "*")
      {
        var skills = Terminal.commands["raiseskill"].m_tabOptionsFetcher();
        foreach (var skill in skills)
        {
          if (skill == "*") continue;
          player.GetSkills().CheatResetSkill(skill);
        }
      }
      else
      {
        player.GetSkills().CheatResetSkill(args[1]);
      }
    }, () => newFetcher());
  }
}
