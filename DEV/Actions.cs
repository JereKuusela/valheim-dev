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
    public static void SetHealth(GameObject obj, float health) {
      SetHealth(obj.GetComponent<ItemDrop>(), health);
      if (health == 0) return;
      SetHealth(obj.GetComponent<Character>(), health);
      SetHealth(obj.GetComponent<WearNTear>(), health);
    }
    public static void SetHealth(Character obj, float health) {
      if (!obj) return;
      obj.SetMaxHealth(health); // Ok to use (no owner check).
      obj.m_nview.GetZDO().Set("health", obj.GetMaxHealth());
    }
    public static void SetHealth(WearNTear obj, float health) {
      if (!obj) return;
      obj.m_nview.GetZDO().Set("health", health);
      obj.m_health = health;
    }
    public static void SetHealth(ItemDrop obj, float health) {
      if (!obj) return;
      obj.m_itemData.m_durability = health == 0 ? obj.m_itemData.GetMaxDurability() : health;
      obj.m_nview.GetZDO().Set("durability", obj.m_itemData.m_durability);
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
    public static void SetVisual(ItemStand obj, string item, int variant) {
      if (!obj) return;
      obj.m_nview.GetZDO().Set("item", item);
      obj.m_nview.GetZDO().Set("variant", variant);
      obj.UpdateVisual();
    }
  }
}