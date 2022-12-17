using UnityEngine;

namespace ServerDevcommands;
///<summary>Calms nearby creatures.</summary>
public class CalmCommand
{
  public CalmCommand()
  {
    AutoComplete.RegisterEmpty("calm");
    Helper.Command("calm", "- Calms creatures within 20 meters.", (args) =>
    {
      var player = Helper.GetPlayer();
      var pos = player.transform.position;
      var calmed = 0;
      foreach (BaseAI baseAI in BaseAI.GetAllInstances())
      {
        baseAI.m_nview.ClaimOwnership();
        if (Vector3.Distance(pos, baseAI.transform.position) <= 20f)
        {
          if (!baseAI.IsAlerted() && !baseAI.IsAggravated()) continue;
          baseAI.SetAggravated(false, BaseAI.AggravatedReason.Building);
          baseAI.SetAlerted(false);
          calmed++;
        }
      }
      Helper.AddMessage(args.Context, $"Calmed {calmed} creatures.");
    });
  }
}

