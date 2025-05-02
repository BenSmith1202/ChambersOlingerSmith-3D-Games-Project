using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainLightning : MonoBehaviour
{
    public GameObject lightningBoltPrefab;
    public GameObject firstTarget;

    // Start is called before the first frame update
    void Start()
    {
        if (firstTarget == null)
        {
            Debug.LogError("First target is not set. Please assign a target in the inspector.");
            Destroy(gameObject); // Destroy this object if no target is assigned

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
