namespace ServerDevcommands;
///<summary>REsetting a skill</summary>
public class ResetSkillCommand {
  public ResetSkillCommand() {
    AutoComplete.Register("resetskill", (int index) => {
      if (index == 0) return ParameterInfo.SkillsWithWildcard;
      return ParameterInfo.None;
    });
    Helper.Command("resetskill", "[skill] - Resets a skill level.", (args) => {
      var player = Helper.GetPlayer();
      Helper.ArgsCheck(args, 2, "Missing the skill.");
      if (args[1] == "*") {
        foreach (var skill in ParameterInfo.SkillsWithWildcard) {
          if (skill == "*") continue;
          player.GetSkills().CheatResetSkill(skill);
        }
      } else {
        player.GetSkills().CheatResetSkill(args[1]);
      }
    }, () => ParameterInfo.SkillsWithWildcard);
  }
}
