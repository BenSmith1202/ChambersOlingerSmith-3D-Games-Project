using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ReapItem", menuName = "Items/OnCrit/Reap")]
public class ReapItem : ItemInstance
{
    // healing per crit
    public int healOnCrit = 5;
    

    public override TriggerType TriggerCategory => TriggerType.OnCrit;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        Attack atk = context.atk;

        if (atk.critLevel > 0) // On crit
        {
            stats.Heal(healOnCrit * atk.critLevel);
        }

    }

    public override void OnAcquire(EntityStats stats)
    {

    }

    // does the inverse of the method above
    public override void OnRemove(EntityStats stats)
    {

    }
}
