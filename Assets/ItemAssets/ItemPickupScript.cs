using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickupScript : MonoBehaviour
{
    public ParticleSystem sparks;
    public ParticleSystem beams;
    public ParticleSystem glow;
    public ParticleSystem coreLight;

    public int rarity = 0;
    ItemWindowScript itemWindowScript;


    private void OnTriggerEnter(Collider other)
    {
        GameObject itemWindow = GameObject.Find("ItemWindow");
        if (itemWindow == null)
        {
            Debug.LogWarning("ItemWindow not found");
            return;
        }

        itemWindowScript = itemWindow.GetComponent<ItemWindowScript>();

        if (itemWindowScript == null)
        {
            Debug.LogWarning("ItemWindowScript not found");
            return;
        }

        if (other.gameObject.CompareTag("Player") && Time.timeScale == 1f)
        {
            itemWindowScript.OpenWindow(rarity);
            Destroy(gameObject);
        }

        //BuffManager bman = other.gameObject.GetComponent<BuffManager>();
        //EntityStats stats = other.gameObject.GetComponent<EntityStats>();

        //if (bman != null && stats != null)
        //{
        //    Debug.Log("attempting to pick up Item: " + item.name);
        //    bman.AddItem(item);
        //    Destroy(gameObject);
        //}
    }

}
