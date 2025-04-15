using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{

    private EntityStats stats;

    void Start()
    {
        stats = gameObject.GetComponent<EntityStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if(stats.currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }
}
