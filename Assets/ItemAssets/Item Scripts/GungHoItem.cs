using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "GungHoItem", menuName = "Items/OnHit/GungHo")]
public class GungHoItem : ItemInstance
{
    public float damageMult = 2f;

    public override TriggerType TriggerCategory => TriggerType.OnHit;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        GameObject target = context.target;
        Attack atk = context.atk;
        GameObject myself = atk.owner;

        //if target is within [radius] meters of the player, multiply damage by [damageMult].
        SlidingScript slidingScript = myself.GetComponent<SlidingScript>();
        if (slidingScript == null)
        {
            return;
        }
        // if sliding faster than crouch speed
        if (slidingScript.isCrouching && myself.GetComponent<Rigidbody>().velocity.magnitude > stats.crouchSpeed + 0.1f)
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
