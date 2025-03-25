using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[CreateAssetMenu(fileName = "HealthBoost", menuName = "Items/StatBoosts/HealthBoost")]
public class HealthBoostItem : ItemInstance
{
    //20% atk speed increase
    public int hpBoost = 20;

    public override TriggerType TriggerCategory => TriggerType.StatBoost;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        // This won't actually be called for stat boosts
    }

    public override void OnAcquire(EntityStats stats)
    {
        Debug.Log("Max Health increased");
        stats.IncreaseMaxHP(20); // increases max hp by 20
    }

    // does the inverse of the method above
    public override void OnRemove(EntityStats stats)
    {
        stats.DecreaseMaxHP(hpBoost); // Reverses the 20 hp increase
    }
}
