using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeathBlast", menuName = "Items/OnKill/DeathBlast")]
public class DeathBlast : ItemInstance
{
    public float chance = 0.33f; // 33% chance to trigger
    public float radius = 6f;
    //percent of killing attack's damage that this explosion will deal
    public float totalDamagePercent = 0.6f;
    public ParticleSystem explosionEffect;
    public override TriggerType TriggerCategory => TriggerType.OnKill;

    public override void OnTrigger(EntityStats myStats, TriggerContext context)
    {
        if (Random.value < chance)
        {
            //get a reference to the lethal attack
            Attack atk = context.atk;

            //deal a portion of the triggering damage
            int damage = Mathf.FloorToInt(atk.damage * totalDamagePercent);

            //make particle effect
            Destroy(Instantiate(explosionEffect, context.target.transform.position, Quaternion.Euler(90, 0, 0)).gameObject, 3f);

            //For everything hit by the blast
            Collider[] enemiesHit = Physics.OverlapSphere(context.target.transform.position, radius);
            foreach (Collider collider in enemiesHit)
            {
                //if it can be hit, and isnt the player
                EntityStats stats = collider.gameObject.GetComponent<EntityStats>();
                if (stats != null && !collider.gameObject.CompareTag("Player"))
                {
                    //inflict damage
                    stats.InflictDamage(damage);
                }
            }
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
