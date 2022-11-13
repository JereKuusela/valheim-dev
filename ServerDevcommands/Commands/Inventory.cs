using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerDevcommands;
public class InventoryCommand
{
  private static List<string> Operations = new() { "clear", "level", "refill", "repair", "upgrade" };
  private static List<string> Targets = new() { "all", "hand", "worn" };
  private static HashSet<ItemDrop.ItemData.ItemType> HandTypes = new() {
    ItemDrop.ItemData.ItemType.Bow, ItemDrop.ItemData.ItemType.OneHandedWeapon, ItemDrop.ItemData.ItemType.Shield,
    ItemDrop.ItemData.ItemType.Torch,ItemDrop.ItemData.ItemType.Tool ,ItemDrop.ItemData.ItemType.TwoHandedWeapon
  };
  private static HashSet<ItemDrop.ItemData.ItemType> ArmorTypes = new() {
    ItemDrop.ItemData.ItemType.Chest, ItemDrop.ItemData.ItemType.Hands, ItemDrop.ItemData.ItemType.Helmet,
    ItemDrop.ItemData.ItemType.Legs,ItemDrop.ItemData.ItemType.Shoulder
  };
  private static bool Valid(ItemDrop.ItemData item) => item.m_shared.m_maxQuality > 1 || HandTypes.Contains(item.m_shared.m_itemType) || ArmorTypes.Contains(item.m_shared.m_itemType);
  public InventoryCommand()
  {
    Helper.Command("inventory", "[level/repair/upgrade] [hand/worn/all] [amount or max level if not given] - Modifies items in the inventory.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing clear, level, refill, repair or upgrade.");
      var amount = Parse.IntNull(args.Args, 2);
      var target = Parse.String(args.Args, 3, "all");
      if (amount == null)
      {
        target = Parse.String(args.Args, 2, "all");
        amount = Parse.IntNull(args.Args, 3);
      }
      var player = Helper.GetPlayer();
      var inventory = player.GetInventory();
      var items = inventory.GetAllItems();
      if (target == "hand" || target == "worn")
        items = inventory.GetEquipedtems();
      if (target == "hand")
        items = items.Where(item => HandTypes.Contains(item.m_shared.m_itemType)).ToList();
      var operation = args[1];
      if (operation == "repair")
      {
        var toRepair = items.Where(item => item.m_durability < item.GetMaxDurability()).ToArray();
        foreach (var item in toRepair)
          item.m_durability = item.GetMaxDurability();

        Helper.AddMessage(args.Context, $"{toRepair.Length} items repaired.");
      }
      else if (operation == "refill")
      {
        var toFill = items.Where(item => item.m_stack < item.m_shared.m_maxStackSize).ToArray();
        foreach (var item in toFill)
          item.m_stack = item.m_shared.m_maxStackSize;

        Helper.AddMessage(args.Context, $"{toFill.Length} items filled.");
      }
      else if (operation == "clear")
      {
        player.UnequipAllItems();
        Helper.AddMessage(args.Context, $"{items.Count} items cleared.");
        inventory.RemoveAll();
      }
      else if (operation == "level")
      {
        if (amount == null) throw new InvalidOperationException("Missing the level.");
        var toUpgrade = items.Where(Valid).ToArray();
        foreach (var item in toUpgrade)
        {
          item.m_quality = amount.Value;
          item.m_durability = item.GetMaxDurability();
        }
        Helper.AddMessage(args.Context, $"{toUpgrade.Length} items upgraded.");
      }
      else if (operation == "upgrade")
      {
        var toUpgrade = items.Where(item => Valid(item)
          && (amount.HasValue || item.m_quality != item.m_shared.m_maxQuality)
        ).ToArray();
        foreach (var item in toUpgrade)
        {
          if (amount.HasValue)
            item.m_quality = amount.Value;
          else
            item.m_quality = item.m_shared.m_maxQuality;
          item.m_durability = item.GetMaxDurability();
        }
        Helper.AddMessage(args.Context, $"{toUpgrade.Length} items upgraded.");
      }
      else throw new InvalidOperationException("Invalid operation. Use level, repair or upgrade.");
      inventory.Changed();
    }, () => Operations);
    AutoComplete.Register("inventory", (int index) =>
    {
      if (index == 0) return Operations;
      if (index == 1) return Targets;
      if (index == 2) return ParameterInfo.Create("Amount", "How many levels to upgrade.");
      return ParameterInfo.None;
    });
  }
}
