using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[CreateAssetMenu(fileName = "HPOnAbility", menuName = "Items/OnAbilityUse/HpOnAbility")]
public class HPOnAbilityItem : ItemInstance
{
    //Heal 20% of max hp on ability use
    public float healPercent = 0.2f;

    public override TriggerType TriggerCategory => TriggerType.OnAbilityUse;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        // Heal the entity by a percentage of their max HP
        Debug.Log(stats.name + "Dash Healed for " + (int)(stats.getMaxHP() * healPercent));
        stats.Heal((int)(stats.getMaxHP() * healPercent));
    }

    public override void OnAcquire(EntityStats stats)
    {

    }

    // does the inverse of the method above
    public override void OnRemove(EntityStats stats)
    {

    }
}
