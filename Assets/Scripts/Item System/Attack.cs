using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to store the information of an attack
public class Attack
{
    public float critMult = 1.5f; // critical hit damage multiplier
    public GameObject owner; //the entity that owns this attack
    public float procCoef = 0f; //multiplier for chance based item effects (0.0-1.0)

    //damage is initialized to the base damage of the entity, but can be modified by items, buffs, etc.
    public int damage = 0;
    //same with knockback
    public float knockback = 0f;

    //Hyper-Crit System like warframe. Crit chances over 100% give a chance to crit multiple times.
    public float critChance = 0f;
    public int critLevel = 0;  //This is the number of times this attack has crit.

    //The debuffs (aka status effects) that this attack inflicts
    public List<BuffInstance> debuffsToApply; //technically these will usually be debuffs, even though the class is called BuffInstance

    //items that cannot apply to this attack. Used to avoid infinite loops and for balancing purposes.
    public List<ItemInstance> blacklist; 

    //TODO: for items that need to add to the blacklist mid trigger, they need to put them in a ToBeBlacklisted list so they dont blacklist later stacks of themselves mid trigger.


    //constructor
    public Attack(GameObject owner, int damage, float critChance, float knockback, float procCoeff)
    {
        debuffsToApply = new List<BuffInstance>();
        blacklist = new List<ItemInstance>();
        this.owner = owner;
        this.damage = damage;
        this.critChance = critChance;
        this.knockback = knockback;
        this.procCoef = procCoeff; //multiplies ALL "chance to trigger" item effects. 
        checkCrit();
    }

    //called at the end of an onHit trigger to apply the crit after all other effects have been applied
    public void checkCrit()
    {
        while (critChance > 0) //while theres a chance for a crit to happen
        {
            //attempt a crit
            //Debug.Log("Attempting Crit. Chance: " + critChance);
            if (UnityEngine.Random.value < critChance)
            {
                //on success, multiply the damage, increment the crit level, and lower the crit chance
                damage = Mathf.FloorToInt(damage * critMult); 
                critLevel++;
                //Debug.Log("CRIT!!! LEVEL: " + critLevel);
                critChance -= 1f; //if crit chance is over 100%, subtract 100% to get chance for a hyper-crit
                // TODO: in the future, some kind of feedback for hyper-crits would be nice
            }
            else
            {
                //if the crit fails, break the loop
                break;
            }
        }
        
    }
}
