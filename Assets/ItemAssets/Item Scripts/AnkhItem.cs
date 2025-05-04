using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[CreateAssetMenu(fileName = "AnkhItem", menuName = "Items/OnHP/Ankh")]
public class AnkhItem : ItemInstance
{
    //Heal % of max hp on break
    public float healPercent = 0.5f;
    public float breakThreshold = 0.5f; // The threshold at which the item will break
    public GameObject breakEffectPrefab; // Prefab for the heal effect
    public AudioClip breakSound; // Sound to play on break

    public override TriggerType TriggerCategory => TriggerType.OnHP;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        if ((float)stats.currentHP / stats.getMaxHP() < breakThreshold)
        {
            stats.Heal((int)(stats.getMaxHP() * healPercent));

            //play sound?
            if (breakSound != null)
            {
                AudioSource.PlayClipAtPoint(breakSound, context.myself.transform.position);
            }

            //play effect
            GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

            Instantiate(breakEffectPrefab, mainCamera.transform.position, Quaternion.identity, mainCamera.transform);
            stats.buffManager.RemoveItem(this); // remove the item from the player
            stats.buffManager.consumedItem = this; // set the consumed item to this item
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
