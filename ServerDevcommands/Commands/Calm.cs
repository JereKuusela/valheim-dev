using UnityEngine;

namespace ServerDevcommands;
///<summary>Calms nearby creatures.</summary>
public class CalmCommand
{
  public CalmCommand()
  {
    AutoComplete.Register("calm", static (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Radius", "a positive integer");
      return ParameterInfo.None;
    });
    Helper.Command("calm", "[radius=20] - Calms creatures within given meters.", static (args) =>
    {
      var player = Helper.GetPlayer();
      var pos = player.transform.position;
      var radius = args.TryParameterFloat(1, 20f);
      var calmed = 0;
      foreach (BaseAI baseAI in BaseAI.Instances)
      {
        if (Vector3.Distance(pos, baseAI.transform.position) <= radius)
        {
          if (!baseAI.IsAlerted() && !baseAI.IsAggravated() && !baseAI.HaveTarget()) continue;
          baseAI.m_nview.ClaimOwnership();
          baseAI.SetAggravated(false, BaseAI.AggravatedReason.Building);
          baseAI.SetAlerted(false);
          baseAI.SetTargetInfo(ZDOID.None);
          if (baseAI is MonsterAI monsterAI)
          {
            monsterAI.m_targetCreature = null;
            monsterAI.m_targetStatic = null;
          }
          if (baseAI is AnimalAI animalAI)
            animalAI.m_target = null;
          calmed++;
        }
      }
      Helper.AddMessage(args.Context, $"Calmed {calmed} creatures.");
    });
  }
}

