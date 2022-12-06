namespace ServerDevcommands;
///<summary>Adds wildcard for all skills. Bit convoluted to support mods adding new skills.</summary>
public class RaiseSkillCommand
{
  public RaiseSkillCommand()
  {
    AutoComplete.Register("raiseskill", (int index) =>
    {
      var fetcher = Terminal.commands["raiseskill"].m_tabOptionsFetcher;
      if (index == 0) return fetcher();
      if (index == 1) return ParameterInfo.Create("Amount of skill levels gained or lost (if negative).");
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
    Helper.Command("raiseskill", "[skill] [amount] - Raises or lowers a skill level.", (args) =>
    {
      var player = Helper.GetPlayer();
      Helper.ArgsCheck(args, 2, "Missing the skill.");
      Helper.ArgsCheck(args, 3, "Missing the amount.");
      var amount = Parse.Float(args[2], 0f);
      if (args[1] == "*")
      {
        var skills = Terminal.commands["raiseskill"].m_tabOptionsFetcher();
        foreach (var skill in skills)
        {
          if (skill == "*") continue;
          player.GetSkills().CheatRaiseSkill(skill, amount);
        }
      }
      else
      {
        player.GetSkills().CheatRaiseSkill(args[1], amount);
      }
    }, () => newFetcher());
  }
}
