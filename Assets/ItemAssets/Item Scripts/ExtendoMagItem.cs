using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ExtendoMag", menuName = "Items/StatBoosts/ExtendoMag")]
public class ExtendoMagItem : ItemInstance
{
    //Clip size up
    public int clipSizeBoost = 6;
    //but reloads take longer
    public float reloadTimeMult = 0.4f;

    public override TriggerType TriggerCategory => TriggerType.StatBoost;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        // This won't actually be called for stat boosts
    }

    public override void OnAcquire(EntityStats stats)
    {
       
        stats.clipSize += clipSizeBoost; // increases clip size
        stats.reloadTimeMod.percent += this.reloadTimeMult; //+40% reload time stacking linearly
    }

    // does the inverse of the method above
    public override void OnRemove(EntityStats stats)
    {
        stats.clipSize -= clipSizeBoost; // decreases clip size
        stats.reloadTimeMod.percent -= this.reloadTimeMult;
    }
}
