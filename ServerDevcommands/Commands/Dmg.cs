using System;
using System.Linq;
using Service;

namespace ServerDevcommands;
///<summary>Applies damage or healing to players.</summary>
public class DmgCommand
{
    public DmgCommand()
    {
        Helper.Command("dmg", "[target] [amount] - (Negative values heal the character).", (args) =>
        {
            Helper.ArgsCheck(args, 2, "Missing target.");
            Helper.ArgsCheck(args, 3, "Missing amount.");
            
            var name = args.Args[1];
            var dmg = Parse.Float(args.Args[2], 0f);

            var absDmg = Math.Abs(dmg);
            var action = dmg >= 0 ? " damage" : " healing";

            var targets = PlayerInfo.FindPlayers([name]);

            foreach (var target in targets)
            {
                if (dmg >= 0)
                {
                    HitData hit = new HitData
                    {
                        m_damage = { m_damage = dmg },
                        m_hitType = HitData.HitType.Self,
                    };
                    ZRoutedRpc.instance.InvokeRoutedRPC(0, target.ZDOID, "Damage", [hit]);
                }
                else
                {
                    ZRoutedRpc.instance.InvokeRoutedRPC(0, target.ZDOID, "Heal", [absDmg, true]);
                }
            }

            var msg = $"{absDmg}{action} applied to: {string.Join(", ", targets.Select(p => p.Name))}";
            args.Context.AddString(msg);
        });
        AutoComplete.Register("dmg", (int index) =>
        {
            if (index == 0) return ["others", "all", .. ParameterInfo.PlayerNames];
            if (index == 1) return ParameterInfo.Create("Value", "Positive = damage / Negative = healing");
            return ParameterInfo.None;
        });
    }
}
