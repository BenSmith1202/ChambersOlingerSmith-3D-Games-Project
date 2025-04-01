using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExampleOnHit", menuName = "Items/OnHit/ExampleOnHit")]
public class ExampleOnHitItem : ItemInstance
{
    public float chance = 0.33f; // 33% chance to trigger
    public override TriggerType TriggerCategory => TriggerType.OnHit;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        Attack atk = context.atk;
        if (Random.value < chance*atk.procCoef)
        {
            
            // Do something here
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
