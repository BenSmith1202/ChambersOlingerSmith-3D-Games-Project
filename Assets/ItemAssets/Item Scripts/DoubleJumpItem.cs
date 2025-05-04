using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "DoubleJumpItem", menuName = "Items/StatBoosts/DoubleJump")]
public class DoubleJumpItem : ItemInstance
{
    //adds an extra jump to the player
    public int numJumpsToAdd = 1;

    public override TriggerType TriggerCategory => TriggerType.StatBoost;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        // This won't actually be called for stat boosts
    }

    public override void OnAcquire(EntityStats stats)
    {
       
        stats.extraJumps += numJumpsToAdd; // increases clip size
    }

    // does the inverse of the method above
    public override void OnRemove(EntityStats stats)
    {
        stats.extraJumps -= numJumpsToAdd; // decreases clip size

    }
}
