using System.Collections.Generic;

namespace ServerDevcommands;
///<summary>Adds duration and intensity.</summary>
public class AddStatusCommand {
  public AddStatusCommand() {
    AutoComplete.Register("addstatus", (int index) => {
      if (index == 0) return ParameterInfo.StatusEffects;
      if (index == 1) return ParameterInfo.Create("Effect duration in seconds.");
      if (index == 2) return ParameterInfo.Create("Effect intensity.");
      return ParameterInfo.None;
    });
    Helper.Command("addstatus", "[name] [duration] [intenstiy] - Adds a status effect.", (args) => {
      Helper.ArgsCheck(args, 2, "Missing status name");
      var player = Helper.GetPlayer();
      var hash = args[1].GetStableHashCode();
      player.GetSEMan().AddStatusEffect(hash, true);
      var effect = player.GetSEMan().GetStatusEffect(hash);
      if (effect == null) return;
      if (args.TryParameterFloat(2, out var duration))
        effect.m_ttl = duration;
      if (args.TryParameterFloat(3, out var intensity)) {
        if (effect is SE_Shield shield)
          shield.m_absorbDamage = intensity;
        if (effect is SE_Burning burning) {
          if (args[1] == "Burning") {
            burning.m_fireDamageLeft = 0;
            burning.AddFireDamage(intensity);
          } else {
            burning.m_spiritDamageLeft = 0;
            burning.AddSpiritDamage(intensity);
          }
        }
        if (effect is SE_Poison poison) {
          poison.m_damageLeft = intensity;
          poison.m_damagePerHit = intensity / effect.m_ttl * poison.m_damageInterval;
        }
      }
    });
  }
}

