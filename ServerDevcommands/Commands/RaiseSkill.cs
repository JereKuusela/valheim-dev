namespace ServerDevcommands;
///<summary>Adds wildcard for all skills.</summary>
public class RaiseSkillCommand {
  public RaiseSkillCommand() {
    AutoComplete.Register("raiseskill", (int index) => {
      if (index == 0) return ParameterInfo.SkillsWithWildcard;
      if (index == 1) return ParameterInfo.Create("Amount of skill levels gained or lost (if negative).");
      return ParameterInfo.None;
    });
    Helper.Command("raiseskill", "[skill] [amount] - Raises or lowers a skill level.", (args) => {
      var player = Helper.GetPlayer();
      Helper.ArgsCheck(args, 2, "Missing the skill.");
      Helper.ArgsCheck(args, 3, "Missing the amount.");
      var amount = Parse.TryFloat(args[2], 0f);
      if (args[1] == "*") {
        foreach (var skill in ParameterInfo.SkillsWithWildcard) {
          if (skill == "*") continue;
          player.GetSkills().CheatRaiseSkill(skill, amount);
        }
      } else {
        player.GetSkills().CheatRaiseSkill(args[1], amount);
      }
    }, () => ParameterInfo.SkillsWithWildcard);
  }
}
