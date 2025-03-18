using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickupScript : MonoBehaviour
{
    public ParticleSystem sparks;
    public ParticleSystem beams;
    public ParticleSystem glow;
    public ParticleSystem coreLight;

    public ItemInstance item; // currently each pickup only has one item inside


    private void OnCollisionEnter(Collision collision)
    {
        BuffManager bman = collision.gameObject.GetComponent<BuffManager>();
        EntityStats stats = collision.gameObject.GetComponent<EntityStats>();

        if (bman != null && stats != null)
        {
            Debug.Log("attempting to pick up Item: " + item.name);
            bman.AddItem(item);
            Destroy(gameObject);
        }
    }

}
