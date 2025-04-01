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
    public float commonDropChance = 0.2f;
    public float rareDropChance = 0.07f;
    public float legendaryDropChance = 0.02f;
    // Start is called before the first frame update

    public void AttemptLootDrop()
    {
        //make Cumulative distribution function
        //float commonThresh = commonDropChance;
        //float rareThresh = commonThresh + rareDropChance;
        //float legendaryThresh = rareThresh + legendaryDropChance;

        float roll = Random.value; // Rolls a number between 0 and 1

        if (roll < legendaryDropChance)
        {
            Instantiate(legendaryPickup, transform.position, Quaternion.identity);
        }
        else if (roll < legendaryDropChance + rareDropChance)
        {
            Instantiate(rarePickup, transform.position, Quaternion.identity);
        }
        else if (roll < legendaryDropChance + rareDropChance + commonDropChance)
        {
            Instantiate(commonPickup, transform.position, Quaternion.identity);
        }

    }
}
