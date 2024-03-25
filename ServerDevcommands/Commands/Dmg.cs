using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerDevcommands;
///<summary>Applies damage or healing to nearby players.</summary>
public class DmgCommand
{
    public DmgCommand()
    {
        Helper.Command("dmg", "[target] [value] - Default value: 5. (Negative values heal the character).", (args) =>
        {
            if (Player.m_localPlayer == null)
            {
                Console.instance.Print("Error: Local player not found.");
                return;
            }

            string argName = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : "all";
            float argDmg = args.Length > 2 && float.TryParse(args[2].Trim(), out float parsedDmg) ? parsedDmg : 5f;

            float absDmg = Math.Abs(argDmg);
            string action = argDmg >= 0 ? " damage" : " healing";

            string damageMessage = $"{absDmg}{action} applied to: ";

            List<string> affectedPlayers = new List<string>();

            foreach (Player targetPlayer in Player.GetAllPlayers())
            {
                if (argName == "all" || (argName == "others" && targetPlayer != Player.m_localPlayer) || targetPlayer.GetPlayerName().Contains(argName))
                {
                    if (argDmg >= 0)
                    {
                        HitData hit = new HitData
                        {
                            m_damage = { m_damage = argDmg },
                            m_hitType = HitData.HitType.Self
                        };
                        targetPlayer.Damage(hit);
                    }
                    else
                    {
                        targetPlayer.Heal(absDmg);
                    }
                    affectedPlayers.Add(targetPlayer.GetPlayerName());
                }
            }

            if (affectedPlayers.Count == 0)
            {
                damageMessage = "No players affected.";
            }
            else
            {
                damageMessage += string.Join(" / ", affectedPlayers);
            }

            Console.instance.Print(damageMessage);
        });
        AutoComplete.Register("dmg", (int index) =>
        {
            if (index == 0) return new List<string> { "others", "all" }.Concat(ParameterInfo.PlayerNames).ToList();
            if (index == 1) return ParameterInfo.Create("Value", "Positive = damage / Negative = healing");
            return ParameterInfo.None;
        });
    }
}
