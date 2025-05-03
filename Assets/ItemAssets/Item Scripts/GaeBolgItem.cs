using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GaeBolgItem", menuName = "Items/OnKill/GaeBolg")]
public class GaeBolgItem : ItemInstance
{
    public GameObject barbPrefab; //the prefab to spawn
    public float chance = 0.33f; // 33% chance to trigger
    public float radius = 6f;
    public int barbNumber = 10; //number of barbs to spawn
    public float barbSpeed = 10f; //speed of the barbs
    public float barbDamage = 0.5f; //damage of the barbs

    public override TriggerType TriggerCategory => TriggerType.OnKill;

    public override void OnTrigger(EntityStats myStats, TriggerContext context)
    {
        if (Random.value < chance)
        {
            //get a reference to the lethal attack
            Attack atk = context.atk;

            // create barbNum barbs in a 360 degree arc around the target each facing outwards
            for (int i = 0; i < barbNumber; i++)
            {
                //calculate the angle of the barb (in degrees)
                float angle = i * (360f / barbNumber);

                //calculate the outward direction vector on the horizontal plane (XZ)
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                // Calculate the rotation needed TO FACE the calculated direction
                // Use LookRotation: it creates a rotation that points Z-axis (forward) along the 'direction'
                Quaternion spawnRotation = Quaternion.LookRotation(direction);

                //spawn the barb at the center, oriented outwards
                BolgBarbScript bbs = Instantiate(barbPrefab, context.target.transform.position, spawnRotation).GetComponent<BolgBarbScript>();

                bbs.owner = context.target;
                bbs.speed = barbSpeed;
                bbs.damage = Mathf.FloorToInt(atk.damage * barbDamage);
                
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
