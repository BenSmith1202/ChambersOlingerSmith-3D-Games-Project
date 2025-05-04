using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "FractureItem", menuName = "Items/OnHit/FractureItem")]
public class FractureItem : ItemInstance
{
    //+ damageMod% damage to enemies above 80% health
    public float hpThreshold = 0.8f;
    public float damageMod = 0.5f;

    public override TriggerType TriggerCategory => TriggerType.OnHit;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        GameObject target = context.target;
        Attack atk = context.atk;
        GameObject myself = atk.owner;

        EntityStats targetStats = target.GetComponent<EntityStats>();
        //if hp is above threshhold
        if (targetStats != null && (targetStats.currentHP/targetStats.getMaxHP() > hpThreshold))
        {
            // + x% damage
            atk.damage += Mathf.FloorToInt(stats.getDamage() * damageMod);
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
