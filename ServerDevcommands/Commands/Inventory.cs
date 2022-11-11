using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerDevcommands;
public class InventoryCommand
{
  private static List<string> Operations = new() { "repair", "upgrade" };
  public InventoryCommand()
  {
    Helper.Command("inventory", "[repair/upgrade] [amount or max level if not given] - Modifies the inventory.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing repair or upgrade.");
      var player = Helper.GetPlayer();
      var inventory = player.GetInventory();
      var items = inventory.GetAllItems();
      var operation = args[1];
      if (operation == "repair")
      {
        var toRepair = items.Where(item => item.m_durability < item.GetMaxDurability()).ToArray();
        foreach (var item in toRepair)
          item.m_durability = item.GetMaxDurability();

        Helper.AddMessage(args.Context, $"{toRepair.Length} items repaired.");
      }
      else if (operation == "upgrade")
      {
        var amount = Parse.Int(args.Args, 2, 0);
        var toUpgrade = items.Where(item =>
          item.m_shared.m_maxQuality > 1
          && (amount != 0 || item.m_quality != item.m_shared.m_maxQuality)
        ).ToArray();
        foreach (var item in toUpgrade)
        {
          if (amount == 0)
            item.m_quality = item.m_shared.m_maxQuality;
          else if (item.m_shared.m_maxQuality > 1)
            item.m_quality += amount;
          item.m_durability = item.GetMaxDurability();
        }
        Helper.AddMessage(args.Context, $"{toUpgrade.Length} items upgraded.");
      }
      else throw new InvalidOperationException("Invalid operation. Use repair or upgrade.");
      inventory.Changed();
    }, () => Operations);
    AutoComplete.Register("inventory", (int index) =>
    {
      if (index == 0) return Operations;
      if (index == 1) return ParameterInfo.Create("Amount", "How many levels to upgrade.");
      return ParameterInfo.None;
    });
  }
}
