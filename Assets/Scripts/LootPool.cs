using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LootPool : MonoBehaviour
{
    public GameObject commonPickup;
    public GameObject rarePickup;
    public GameObject legendaryPickup;

    [Header("Drop chances (must sum to < 1.0)")]
    public float commonDropChance = 0.09f;
    public float rareDropChance = 0.04f;
    public float legendaryDropChance = 0.01f;
    // Start is called before the first frame update

    public void AttemptLootDrop()
    {
        //make Cumulative distribution function
        float commonThresh = commonDropChance;
        float rareThresh = commonThresh + rareDropChance;
        float legendaryThresh = rareThresh + legendaryDropChance;
        
        //pick a random float
        float val = Random.value;
        if (val < commonThresh)
        {
            Instantiate(commonPickup, transform.position, Quaternion.identity);
        }
        else if (val < rareThresh)
        {
            Instantiate(rarePickup, transform.position, Quaternion.identity);
        }
        else if (val > legendaryThresh)
        {
            Instantiate(legendaryPickup, transform.position, Quaternion.identity);
        } 
        else
        {
            //drop nothing
        }

    }
}
