using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base abstract class for all item upgrades
public abstract class ItemInstance : ScriptableObject
{
    // Common properties for all items
    public string itemName;
    public string description;
    public Sprite icon;
    public Rarity rarity;


    //Methods that all items should implement
    #region Abstract Methods

    // every item has a trigger type that it must override this with
    public abstract TriggerType TriggerCategory { get; }

    // Abstract method. Each item must do something (or nothing) when triggered
    public abstract void OnTrigger(EntityStats stats, TriggerContext context);

    // Optional virtual methods that can be overridden
    // often used for stat boosts
    public virtual void OnAcquire(EntityStats stats) { }
    public virtual void OnRemove(EntityStats stats) { }

    #endregion
}


//Class Definitions like trigger types, rarities, etc.
#region ClassDefinitions

// Enum to define trigger types
public enum TriggerType
{
    StatBoost,
    OnHit,
    OnKill,
    OnAbilityUse,
    OnHP
    //, Constant //PLEASE Avoid implementing constantly triggered effects!
}


// Context class for passing relevant information during trigger events
public class TriggerContext
{
    public GameObject myself;
    public GameObject target;
    public float damage;
    public Attack atk;
    // Add other contextual information as needed
}


public class Rarity
{
    public string rarityName;
    public Color rarityColor;
    public float rarityDropChance;
}


public class Rarities
{
    public static Rarity common = new Rarity { rarityName = "Common", rarityColor = Color.white, rarityDropChance = 0.75f };
    public static Rarity rare = new Rarity { rarityName = "Rare", rarityColor = Color.blue, rarityDropChance = 0.23f };
    public static Rarity legendary = new Rarity { rarityName = "Legendary", rarityColor = Color.yellow, rarityDropChance = 0.02f };
}
#endregion