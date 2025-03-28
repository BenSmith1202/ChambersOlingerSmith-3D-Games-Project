using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CritBoost", menuName = "Items/StatBoosts/CritBoost")]
public class CritBoostItem : ItemInstance
{
    //20% crit chance increase
    public float critBoost = 0.2f;

    public override TriggerType TriggerCategory => TriggerType.StatBoost;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        // This won't actually be called for stat boosts
    }

    public override void OnAcquire(EntityStats stats)
    {
        stats.critChanceMod.percent += critBoost; // increases crit chance linearly
        Debug.Log("Crit chance increased to " + stats.critChanceMod);
    }

    // does the inverse of the method above
    public override void OnRemove(EntityStats stats)
    {
        stats.critChanceMod.percent -= critBoost; ; // Reverses the crit chance increase
    }
}
