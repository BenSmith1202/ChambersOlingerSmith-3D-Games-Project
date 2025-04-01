using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[CreateAssetMenu(fileName = "CoolDownReduction", menuName = "Items/StatBoosts/CooldownReduction")]
public class CooldownReductionItem : ItemInstance
{
    //7.5% reduced cooldown time
    public float cooldownTime = 0.925f;

    public override TriggerType TriggerCategory => TriggerType.StatBoost;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        // This won't actually be called for stat boosts
    }

    public override void OnAcquire(EntityStats stats)
    {
       
        stats.grappleDelayMod.mult *= cooldownTime; // 7.5% logarithmically reduces cooldown time
        stats.dashDelayMod.mult *= cooldownTime;
    }

    // does the inverse of the method above
    public override void OnRemove(EntityStats stats)
    {
        stats.grappleDelayMod.mult /= cooldownTime; // undoes the above
        stats.dashDelayMod.mult /= cooldownTime;
    }
}
