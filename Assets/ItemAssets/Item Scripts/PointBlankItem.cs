using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "PointBlankItem", menuName = "Items/OnHit/PointBlankItem")]
public class PointBlankItem : ItemInstance
{
    //20% atk increase in x meter radius
    public float radius = 8f;
    public float damageMult = 0.2f;

    public override TriggerType TriggerCategory => TriggerType.OnHit;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        GameObject target = context.target;
        Attack atk = context.atk;
        GameObject myself = atk.owner;

        //if target is within [radius] meters of the player, multiply damage by [damageMult].
        if (Vector3.Distance(myself.transform.position, target.transform.position) < radius)
        {
            
            atk.damage = Mathf.FloorToInt(atk.damage * damageMult);
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
