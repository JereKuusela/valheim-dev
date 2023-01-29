namespace ServerDevcommands;
///<summary>Resetting a skill. Bit convoluted to support mods adding new skills.</summary>
public class ResetSkillCommand
{
  public ResetSkillCommand()
  {
    var originalFetcher = Terminal.commands["resetskill"].m_tabOptionsFetcher;
    var newFetcher = () =>
    {
      var options = originalFetcher();
      if (!options.Contains(Skills.SkillType.All.ToString()))
        options.Add(Skills.SkillType.All.ToString());
      return options;
    };
    Helper.Command("resetskill", "[skill] - Resets a skill level.", (args) =>
    {
      var player = Helper.GetPlayer();
      Helper.ArgsCheck(args, 2, "Missing the skill.");
      if (args[1].ToLower() == Skills.SkillType.All.ToString().ToLower())
      {
        var skills = Terminal.commands["resetskill"].m_tabOptionsFetcher();
        foreach (var skill in skills)
        {
          if (skill == Skills.SkillType.All.ToString()) continue;
          player.GetSkills().CheatResetSkill(skill);
        }
      }
      else
      {
        player.GetSkills().CheatResetSkill(args[1]);
      }
    }, () => newFetcher());
    AutoComplete.RegisterDefault("resetskill");
  }
}
