using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[CreateAssetMenu(fileName = "SpeedBoost", menuName = "Items/StatBoosts/SpeedBoost")]
public class SpeedBoostItem : ItemInstance
{
    //% move speed increase
    public float speedBoost = 0.2f;

    public override TriggerType TriggerCategory => TriggerType.StatBoost;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        // This won't actually be called for stat boosts
    }

    public override void OnAcquire(EntityStats stats)
    {
        Debug.Log("Move Speed increased");
        stats.speedMod.percent += speedBoost; //linearly increases the move speed
    }

    // does the inverse of the method above
    public override void OnRemove(EntityStats stats)
    {
        stats.speedMod.percent -= speedBoost;// Reverses the move speed increase
    }
}
