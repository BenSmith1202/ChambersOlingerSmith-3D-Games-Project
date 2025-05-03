using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BuffManager : MonoBehaviour
{
    public EntityStats myStats;

    // Lists for each trigger category
    public List<ItemInstance> allItems = new List<ItemInstance>();
    public List<ItemInstance> statBoosts = new List<ItemInstance>();
    public List<ItemInstance> onHitEffects = new List<ItemInstance>();
    public List<ItemInstance> onKillEffects = new List<ItemInstance>();
    public List<ItemInstance> onAbilityEffects = new List<ItemInstance>();
    public List<ItemInstance> onHPEffects = new List<ItemInstance>();
    public List<ItemInstance> onCritEffects = new List<ItemInstance>();
    //public List<ItemInstance> constantEffects = new List<ItemInstance>();


    //Buffs list
    public List<BuffInstance> buffs = new List<BuffInstance>();

    public ItemInstance consumedItem;

    private void Awake()
    {
        myStats = GetComponent<EntityStats>();
    }

    //Update method and buff management
    #region Update and Buffs

    private void Update()
    {
        List<BuffInstance> deadBuffs = new List<BuffInstance>();
        foreach (var buff in buffs)
        {
            //count up this buff's time
            buff.currentDuration += Time.deltaTime;
            buff.currentTickDelay += Time.deltaTime;

            //call tick method once every [tickdelay]
            if (buff.currentTickDelay > buff.maxTickDelay)
            {
                buff.currentTickDelay = 0;
                buff.OnTick();
            }
            //remove buff when it's time is up
            if (buff.currentDuration > buff.maxDuration)
            {
                deadBuffs.Add(buff);
            }
        }
        foreach (var buff in deadBuffs)
        {
            RemoveBuff(buff);
        }
        deadBuffs.Clear();

    }

    //applies a buff prefab to this entity
    public void AddBuff(GameObject buffObject)
    {
        //Attatch the buff to the entity
        GameObject instantiatedBuff = Instantiate(buffObject, gameObject.transform);
        instantiatedBuff.transform.localPosition = Vector3.zero;
        //get a reference to the buff script
        BuffInstance buffInstance = instantiatedBuff.GetComponent<BuffInstance>();
        //add it to the list and trigger application effects.
        buffs.Add(buffInstance);
        buffInstance.OnApply();
    }

    public void RemoveBuff(BuffInstance buff)
    {
        buffs.Remove(buff);
        buff.OnRemove();
    }
#endregion

    //Methods to add/Remove items from the inventory
    #region ItemManagement
    public void AddItem(ItemInstance item)
    {
        if (item == null) // handle null pickups
        {
            Debug.LogWarning("No item to add :(");
            return;
        }

        //TODO: Stacking logic

        allItems.Add(item);
        Debug.Log("Added " + item.itemName + " to inventory");
        Debug.Log("Item count: " + allItems.Count);
        //PrintInventory();

        // Add to appropriate category list
        switch (item.TriggerCategory)
        {
            case TriggerType.StatBoost:
                statBoosts.Add(item);
                break;
            case TriggerType.OnHit:
                onHitEffects.Add(item);
                break;
            case TriggerType.OnKill:
                onKillEffects.Add(item);
                break;
            case TriggerType.OnAbilityUse:
                onAbilityEffects.Add(item);
                break;
            case TriggerType.OnHP:
                onHPEffects.Add(item);
                break;
            case TriggerType.OnCrit:
                onCritEffects.Add(item);
                break;

                // Add other cases
        }

        // Call OnAcquire to apply initial effects
        item.OnAcquire(myStats);
    }

    public void RemoveItem(ItemInstance itemInstance)
    {
        if (!allItems.Contains(itemInstance))
        {
            Debug.LogWarning("Item not found in inventory");
            return;
        }
        if (itemInstance == consumedItem) //dont double consume items. Reset at end of iteration
        {
            return;
        }

        //Remove from main list
        allItems.Remove(itemInstance);

        // Remove from appropriate category list
        switch (itemInstance.TriggerCategory)
        {
            case TriggerType.StatBoost:
                statBoosts.Remove(itemInstance);
                break;
            case TriggerType.OnHit:
                onHitEffects.Remove(itemInstance);
                break;
            case TriggerType.OnKill:
                onKillEffects.Remove(itemInstance);
                break;
            case TriggerType.OnAbilityUse:
                onAbilityEffects.Remove(itemInstance);
                break;
            case TriggerType.OnHP:
                onHPEffects.Remove(itemInstance);
                break;
            case TriggerType.OnCrit:
                onCritEffects.Remove(itemInstance);
                break;

                // hopefully these removals dont happen if there's no item in the list because these should be perfectly synced with the main item list.
        }

        // Call OnRemove to de-apply initial effects
        itemInstance.OnRemove(myStats);
    }
#endregion


    // Methods to trigger different effect types
    #region TriggerMethods

    //Triggered whenever an attack hits an enemy
    public void TriggerOnHitEffects(GameObject target, Attack atk) //TODO: may need some more parameters
    {
        var context = new TriggerContext {target = target, atk = atk };
        foreach (var item in onHitEffects)
        {
            if (atk.blacklist.Contains(item)) continue; //skip this item if it's blacklisted
            item.OnTrigger(myStats, context);
        }
    }


    //Triggered after an attack that hit an enemy kills it
    public void TriggerOnKillEffects(GameObject target, Attack atk) //atk is the attack that killed the target
    {
        var context = new TriggerContext {target = target, atk = atk };
        foreach (var item in onKillEffects)
        {
            if (atk.blacklist.Contains(item)) continue; //skip this item if it's blacklisted
            item.OnTrigger(myStats, context);
        }
    }


    //triggered whenever an ability is used. As of right now, this likely only applies to the player's dash ability.
    public void TriggerOnAbilityEffects(GameObject myself) //Doesn't need an attack because it's not an attack
    {
        var context = new TriggerContext { myself = myself};
        foreach (var item in onAbilityEffects)
        {
            item.OnTrigger(myStats, context);
        }
    }


    //Triggered whenever an entity's HP changes.
    public void TriggerOnHPEffects(GameObject myself) //TODO: may need some more parameters
    {
        var context = new TriggerContext { myself = myself};
        foreach (var item in onHPEffects)
        {
            if (item == consumedItem) continue; //skip this item if it's blacklisted
            item.OnTrigger(myStats, context);
        }
        consumedItem = null; //reset consumed item at the end of the iteration
    }

    //Triggered whenever an attack crits on an enemy
    public void TriggerOnCritEffects(GameObject target, Attack atk) //TODO: may need some more parameters
    {
        var context = new TriggerContext { target = target, atk = atk };
        foreach (var item in onCritEffects)
        {
            if (atk.blacklist.Contains(item)) continue; //skip this item if it's blacklisted
            item.OnTrigger(myStats, context);
        }
    }

    #endregion


    // print formatted list of items in each of the above lists
    public void PrintInventory()
    {
        print("Inventory:");

        print("\nStat Boosts:");
        foreach (var item in statBoosts)
        {
            print(item.itemName);
        }

        print("\nOn Hit Effects:");
        foreach (var item in onHitEffects)
        {
            print(item.itemName);
        }

        print("\nOn Kill Effects:");
        foreach (var item in onKillEffects)
        {
            print(item.itemName);
        }

        print("\nOn Ability Effects:");
        foreach (var item in onAbilityEffects)
        {
            print(item.itemName);
        }

        print("\nOn HP Effects:");
        foreach (var item in onHPEffects)
        {
            print(item.itemName);
        }

        print("\nOn Crit Effects:");
        foreach (var item in onCritEffects)
        {
            print(item.itemName);
        }
    }
}
