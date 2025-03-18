using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to store the information of an attack
public class Attack
{
    //damage is initialized to the base damage of the entity, but can be modified by items, buffs, etc.
    public float damage;

    //same with knockback
    public float knockback;

    //The debuffs (aka status effects) that this attack inflicts
    public List<BuffInstance> debuffsToApply; //technically these will usually be debuffs, even though the class is called BuffInstance


    public Attack(float damage, float knockback)
    {
        this.damage = damage;
        this.knockback = knockback;
    }
}
