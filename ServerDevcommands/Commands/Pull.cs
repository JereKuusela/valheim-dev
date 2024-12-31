using UnityEngine;

namespace ServerDevcommands;

public class PullCommand
{
  public PullCommand()
  {
    Helper.Command("pull", "[radius] - Pulls all items within the radius.", static (args) =>
    {
      var player = Helper.GetPlayer();
      var range = args.TryParameterFloat(1, player.m_autoPickupRange);
      Vector3 vector = player.transform.position + Vector3.up;
      foreach (Collider collider in Physics.OverlapSphere(vector, range, player.m_autoPickupMask))
      {
        if (!collider.attachedRigidbody) continue;
        var item = collider.attachedRigidbody.GetComponent<ItemDrop>();
        if (!item) continue;
        item.m_autoPickup = true;
        item.transform.position = player.transform.position + Vector3.up * 0.5f;
      }
      Helper.AddMessage(args.Context, $"Pulled items within {range:F0} meters.");
    });
    AutoComplete.Register("pull", static (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Radius", "a positive integer");
      return ParameterInfo.None;
    });
  }
}
