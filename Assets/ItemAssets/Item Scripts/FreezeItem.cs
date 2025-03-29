using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FreezeItem", menuName = "Items/OnHit/Freeze!")]
public class FreezeItem : ItemInstance
{
    public GameObject frozenDebuff;
    public float chance = 0.33f; // chance to trigger
    public override TriggerType TriggerCategory => TriggerType.OnHit;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        Attack atk = context.atk;
        if (Random.value < chance*atk.procCoef)
        {
            Debug.Log("\"Freeze!\" triggered!");
            atk.debuffsToApply.Add(frozenDebuff);
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
