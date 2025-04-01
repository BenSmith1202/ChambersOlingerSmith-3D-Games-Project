using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[CreateAssetMenu(fileName = "RegenBoost", menuName = "Items/StatBoosts/RegenBoost")]
public class RegenBoostItem : ItemInstance
{
    //+1 hp per second
    public int regenBoost = 1;

    public override TriggerType TriggerCategory => TriggerType.StatBoost;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        // This won't actually be called for stat boosts
    }

    public override void OnAcquire(EntityStats stats)
    {
        
        stats.regenMod.flat += regenBoost; // increases regen by a flat amount per tick
    }

    // does the inverse of the method above
    public override void OnRemove(EntityStats stats)
    {
        stats.regenMod.flat -= regenBoost; // Reverses the 20 hp increase
    }
}
