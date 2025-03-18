using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[CreateAssetMenu(fileName = "AtkSpdBoost", menuName = "Items/StatBoosts/AtkSpdBoost")]
public class AtkSpdBoostItem : ItemInstance
{
    //20% atk speed increase
    public float speedBoost = 1.5f;

    public override TriggerType TriggerCategory => TriggerType.StatBoost;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        // This won't actually be called for stat boosts
    }

    public override void OnAcquire(EntityStats stats)
    {
        Debug.Log("Attack Speed increased");
        stats.attackCooldownTime /= speedBoost; // 20% logarithmically increases the attack speed
    }

    // does the inverse of the method above
    public override void OnRemove(EntityStats stats)
    {
        stats.attackCooldownTime *= speedBoost; // Reverses the 20% attack speed increase
    }
}
