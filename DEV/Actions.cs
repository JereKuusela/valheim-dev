using System;
using UnityEngine;
namespace DEV {
  public static class Actions {

    public static void SetTame(GameObject obj, bool tame) {
      SetTame(obj.GetComponent<Character>(), tame);
    }
    public static void SetTame(Character obj, bool tame) {
      if (!obj) return;
      obj.SetTamed(tame); // Ok to use (action sent to the owner).
      var AI = obj.GetComponent<BaseAI>();
      if (AI) {
        AI.SetAlerted(false);
        if (tame) {
          SetHunt(AI, false);
          AI.SetPatrolPoint();
        }
        AI.SetTargetInfo(ZDOID.None);
        var monster = obj.GetComponent<MonsterAI>();
        if (monster) {
          monster.m_targetCreature = null;
          monster.m_targetStatic = null;
          if (tame) {
            monster.SetDespawnInDay(false);
            monster.SetEventCreature(false);
          }
        }
        var animal = obj.GetComponent<AnimalAI>();
        if (animal) {
          animal.m_target = null;
        }
      }
    }
    public static void SetHunt(GameObject obj, bool hunt) {
      SetHunt(obj.GetComponent<BaseAI>(), hunt);
    }
    public static void SetHunt(BaseAI obj, bool hunt) {
      if (!obj) return;
      obj.m_huntPlayer = hunt;
      obj.m_nview.GetZDO().Set("huntplayer", hunt);
    }
    public static void SetSleeping(GameObject obj, bool sleep) {
      SetSleeping(obj.GetComponent<MonsterAI>(), sleep);
    }
    public static void SetSleeping(MonsterAI obj, bool sleep) {
      if (!obj) return;
      obj.m_sleeping = sleep;
      obj.m_nview.GetZDO().Set("sleeping", sleep);
    }
    public static void SetBaby(GameObject obj) {
      SetBaby(obj.GetComponent<Growup>());
    }
    public static void SetBaby(Growup obj) {
      if (!obj) return;
      obj.m_nview.GetZDO().Set("spawntime", DateTime.MaxValue.Ticks);
    }
    public static void SetLevel(GameObject obj, int level) {
      if (level < 1) return;
      SetLevel(obj.GetComponent<ItemDrop>(), level);
      SetLevel(obj.GetComponent<Character>(), level);
    }
    public static void SetLevel(Character obj, int level) {
      if (!obj) return;
      if (obj.GetLevel() != level)
        obj.SetLevel(level); // Ok to use (no owner check).
    }
    public static void SetLevel(ItemDrop obj, int level) {
      if (!obj) return;
      obj.m_itemData.m_quality = level;
      obj.m_nview.GetZDO().Set("quality", level);
    }
    public static float GetHealth(ZNetView obj) {
      var zdo = obj.GetZDO();
      var itemDrop = obj.GetComponent<ItemDrop>();
      if (itemDrop) return itemDrop.m_itemData.m_durability;
      var character = obj.GetComponent<Character>();
      if (character) return character.GetHealth();
      var wearNTear = obj.GetComponent<WearNTear>();
      if (wearNTear) return zdo.GetFloat("health", wearNTear.m_health);
      var destructible = obj.GetComponent<Destructible>();
      if (destructible) return zdo.GetFloat("health", destructible.m_health);
      var treeLog = obj.GetComponent<TreeLog>();
      if (treeLog) return zdo.GetFloat("health", treeLog.m_health);
      var treeBase = obj.GetComponent<TreeBase>();
      if (treeBase) return zdo.GetFloat("health", treeBase.m_health);
      return -1;
    }
    public static float SetHealth(GameObject obj, float health) {
      var itemDrop = obj.GetComponent<ItemDrop>();
      if (itemDrop) return SetHealth(itemDrop, health);
      var character = obj.GetComponent<Character>();
      if (character) return SetHealth(character, health);
      var wearNTear = obj.GetComponent<WearNTear>();
      if (wearNTear) return SetHealth(wearNTear, health);
      var treeLog = obj.GetComponent<TreeLog>();
      if (treeLog) return SetHealth(treeLog, health);
      var treeBase = obj.GetComponent<TreeBase>();
      if (treeBase) return SetHealth(treeBase, health);
      var destructible = obj.GetComponent<Destructible>();
      if (destructible) return SetHealth(destructible, health);
      return 0;
    }
    public static float SetHealth(Character obj, float health) {
      if (!obj) return 0f;
      var previous = obj.GetMaxHealth();
      if (health == 0) {
        obj.SetupMaxHealth();
        return previous;
      }
      obj.SetMaxHealth(health);
      // Max health resets on awake if health is equal to max.
      obj.SetHealth(health * 1.000001f);
      return previous;
    }
    public static float SetHealth(WearNTear obj, float health) {
      if (!obj) return 0f;
      if (health == 0) health = obj.m_health;
      var previous = obj.m_nview.GetZDO().GetFloat("health", obj.m_health);
      obj.m_nview.GetZDO().Set("health", health);
      return previous;
    }
    public static float SetHealth(TreeLog obj, float health) {
      if (!obj) return 0f;
      if (health == 0) health = obj.m_health;
      var previous = obj.m_nview.GetZDO().GetFloat("health", obj.m_health);
      obj.m_nview.GetZDO().Set("health", health);
      return previous;
    }
    public static float SetHealth(Destructible obj, float health) {
      if (!obj) return 0f;
      if (health == 0) health = obj.m_health;
      var previous = obj.m_nview.GetZDO().GetFloat("health", obj.m_health);
      obj.m_nview.GetZDO().Set("health", health);
      return previous;
    }
    public static float SetHealth(TreeBase obj, float health) {
      if (!obj) return 0f;
      if (health == 0) health = obj.m_health;
      var previous = obj.m_nview.GetZDO().GetFloat("health", obj.m_health);
      obj.m_nview.GetZDO().Set("health", health);
      return previous;
    }
    public static float SetHealth(ItemDrop obj, float health) {
      if (!obj) return 0f;
      if (health == 0) health = obj.m_itemData.GetMaxDurability();
      var previous = obj.m_itemData.m_durability;
      obj.m_itemData.m_durability = health;
      obj.m_nview.GetZDO().Set("durability", obj.m_itemData.m_durability);
      return previous;
    }
    public static void SetVariant(GameObject obj, int variant) {
      SetVariant(obj.GetComponent<ItemDrop>(), variant);
    }
    public static void SetVariant(ItemDrop obj, int variant) {
      if (!obj) return;
      obj.m_itemData.m_variant = variant;
      obj.m_nview.GetZDO().Set("variant", variant);
    }
    public static void SetName(GameObject obj, string name) {
      SetName(obj.GetComponent<Tameable>(), name);
      SetName(obj.GetComponent<ItemDrop>(), name);
    }
    public static void SetName(Tameable obj, string name) {
      if (!obj) return;
      obj.m_nview.GetZDO().Set("TamedName", name);
    }
    public static void SetName(ItemDrop obj, string name) {
      if (!obj) return;
      obj.m_itemData.m_crafterID = name == "" ? 0 : -1;
      obj.m_nview.GetZDO().Set("crafterID", obj.m_itemData.m_crafterID);
      obj.m_itemData.m_crafterName = name;
      obj.m_nview.GetZDO().Set("crafterName", name);
    }
    public static int SetStack(GameObject obj, int remaining) {
      if (remaining <= 0) return 0;
      var item = obj.GetComponent<ItemDrop>();
      if (!item) return 0;
      var stack = Math.Min(remaining, item.m_itemData.m_shared.m_maxStackSize);
      item.m_itemData.m_stack = stack;
      item.m_nview.GetZDO().Set("stack", stack);
      return stack;
    }
    public static void Remove(GameObject obj) {
      var netView = obj.GetComponent<ZNetView>();
      // Original code only fully destroyes owned zdos.
      if (netView && !netView.IsOwner())
        ZDOMan.instance.m_destroySendList.Add(netView.GetZDO().m_uid);
      ZNetScene.instance.Destroy(obj);
    }
    public static void SetRotation(GameObject obj, Quaternion rotation) {
      var view = obj.GetComponent<ZNetView>();
      if (view == null) return;
      if (rotation != Quaternion.identity) {
        view.GetZDO().SetRotation(rotation);
        obj.transform.rotation = rotation;
      }
    }
    public static void SetScale(GameObject obj, Vector3 scale) {
      var view = obj.GetComponent<ZNetView>();
      if (view == null) return;
      if (scale != Vector3.one && view.m_syncInitialScale)
        view.SetLocalScale(scale);
    }
    public static void SetVisual(GameObject obj, string item, int variant) {
      SetVisual(obj.GetComponent<ItemStand>(), item, variant);
    }
    public static void SetVisual(GameObject obj, VisSlot slot, string item, int variant) {
      SetVisual(obj.GetComponent<Character>(), slot, item, variant);
    }
    public static void SetVisual(ItemStand obj, string item, int variant) {
      if (!obj) return;
      obj.m_nview.GetZDO().Set("item", item);
      obj.m_nview.GetZDO().Set("variant", variant);
      obj.UpdateVisual();
    }
    public static void SetVisual(Character obj, VisSlot slot, string item, int variant) {
      if (!obj) return;
      var equipment = obj.GetComponent<VisEquipment>();
      if (equipment == null) return;
      equipment.SetItem(slot, item, variant);
    }
    public static void Move(ZNetView obj, Vector3 offset, string origin) {
      var zdo = obj.GetZDO();
      var position = obj.transform.position;
      var rotation = Player.m_localPlayer.transform.rotation;
      if (origin == "world")
        rotation = Quaternion.identity;
      if (origin == "object")
        rotation = obj.transform.rotation;
      position += rotation * Vector3.forward * offset.x;
      position += rotation * Vector3.right * offset.z;
      position += rotation * Vector3.up * offset.y;
      zdo.SetPosition(position);
      obj.transform.position = position;
    }

    public static void Rotate(ZNetView obj, Vector3 relative, string origin) {
      var zdo = obj.GetZDO();
      var originRotation = Player.m_localPlayer.transform.rotation;
      if (origin == "world")
        originRotation = Quaternion.identity;
      if (origin == "object")
        originRotation = obj.transform.rotation;
      var transform = obj.transform;
      var position = transform.position;
      transform.RotateAround(position, originRotation * Vector3.up, relative.y);
      transform.RotateAround(position, originRotation * Vector3.forward, relative.x);
      transform.RotateAround(position, originRotation * Vector3.right, relative.z);
      zdo.SetRotation(obj.transform.rotation);
    }
    public static void ResetRotation(ZNetView obj) {
      var zdo = obj.GetZDO();
      zdo.SetRotation(Quaternion.identity);
      obj.transform.rotation = Quaternion.identity;
    }
    public static void Scale(ZNetView obj, Vector3 scale) {
      if (obj.m_syncInitialScale)
        obj.SetLocalScale(scale);
    }
  }
}