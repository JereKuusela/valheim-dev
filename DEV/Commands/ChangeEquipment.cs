namespace DEV {
  public class ChangeEquipmentCommand : BaseCommands {
    private static void ChangeHelmet(Character obj, string item) {
      if (obj == null) return;
      var equipment = obj.GetComponent<VisEquipment>();
      if (equipment == null) return;
      equipment.SetHelmetItem(item);
    }
    private static void ChangeLeftHand(Character obj, string item, int variant) {
      if (obj == null) return;
      var equipment = obj.GetComponent<VisEquipment>();
      if (equipment == null) return;
      equipment.SetLeftItem(item, variant);
    }
    private static void ChangeRightHand(Character obj, string item) {
      if (obj == null) return;
      var equipment = obj.GetComponent<VisEquipment>();
      if (equipment == null) return;
      equipment.SetRightItem(item);
    }
    private static void ChangeChest(Character obj, string item) {
      if (obj == null) return;
      var equipment = obj.GetComponent<VisEquipment>();
      if (equipment == null) return;
      equipment.SetChestItem(item);
    }
    private static void ChangeBeard(Character obj, string item) {
      if (obj == null) return;
      var equipment = obj.GetComponent<VisEquipment>();
      if (equipment == null) return;
      equipment.SetBeardItem(item);
    }
    public ChangeEquipmentCommand() {
      new Terminal.ConsoleCommand("check_equipment", "Checks some equipment slots.", delegate (Terminal.ConsoleEventArgs args) {
        if (Player.m_localPlayer == null) return;
        var creature = Player.m_localPlayer.GetHoverCreature();
        if (creature == null) return;
        var equipment = creature.GetComponent<VisEquipment>();
        if (equipment == null) return;
        if (equipment.m_rightHand)
          args.Context.AddString("Right hand: " + equipment.m_rightItem);
        if (equipment.m_leftHand)
          args.Context.AddString("Left hand: " + equipment.m_leftItem);
        if (equipment.m_helmet)
          args.Context.AddString("Helmet: " + equipment.m_helmetItem);
        args.Context.AddString("Beard: " + equipment.m_beardItem);
      }, true, true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("change_helmet", "[item name] - Changes visual helmet of hovered creature.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        if (Player.m_localPlayer == null) return;
        ChangeHelmet(Player.m_localPlayer.GetHoverCreature(), args[1]);
      }, true, true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("change_left_hand", "[item name] [variant = 0] - Changes visual let hand item of hovered creature.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        if (Player.m_localPlayer == null) return;
        ChangeLeftHand(Player.m_localPlayer.GetHoverCreature(), args[1], TryParameterInt(args.Args, 2, 0));
      }, true, true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("change_right_hand", "[item name] - Changes own visual left hand item.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        if (Player.m_localPlayer == null) return;
        ChangeRightHand(Player.m_localPlayer.GetHoverCreature(), args[1]);
      }, true, true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
      new Terminal.ConsoleCommand("change_chest", "[item name] - Changes visual chest armor of hovered creature..", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        if (Player.m_localPlayer == null) return;
        ChangeChest(Player.m_localPlayer.GetHoverCreature(), args[1]);
      }, true, true, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
    }
  }
}
