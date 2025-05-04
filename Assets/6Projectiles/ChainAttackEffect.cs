using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainAttackEffect : MonoBehaviour
{
// Assign your Lightning Bolt Prefab here in the Inspector
    public GameObject lightningBoltPrefab;

    /// <summary>
    /// Creates a lightning burst effect between two GameObjects.
    /// </summary>
    /// <param name="startObject">The GameObject where the lightning originates.</param>
    /// <param name="endObject">The GameObject where the lightning terminates.</param>
    public void CreateLightningBurst(GameObject startObject, GameObject endObject)
    {
        if (lightningBoltPrefab == null || startObject == null || endObject == null)
        {
            Debug.LogError("Cannot create lightning burst: Prefab or target objects are null.");
            return;
        }

        // Instantiate the prefab at the start position (position doesn't strictly matter as Line Renderer uses world space)
        GameObject boltInstance = Instantiate(lightningBoltPrefab, startObject.transform.position, Quaternion.identity);

        // Get the LightningBolt script component from the instantiated object
        LightningBolt boltScript = boltInstance.GetComponent<LightningBolt>();

        if (boltScript != null)
        {
            // Initialize the bolt with the start and end positions
            Vector3 endpPos = endObject.transform.position;
            Collider endCollider = endObject.GetComponent<Collider>();
            if (endCollider != null)
            {
                endpPos = endCollider.ClosestPoint(startObject.transform.position);
            }
            boltScript.Initialize(startObject.transform.position, endpPos);
        }
        else
        {
            Debug.LogError("Lightning Bolt Prefab is missing the LightningBolt script component!", lightningBoltPrefab);
            Destroy(boltInstance); // Clean up if prefab is misconfigured
        }
    }

    // --- Example Usage ---
    // You might call this from another script based on some game event
    public GameObject objectA; // Assign in Inspector
    public GameObject objectB; // Assign in Inspector

    void Update()
    {
        // Example: Trigger lightning burst when Space key is pressed
        if (Input.GetKeyDown(KeyCode.L))
        {
             if (objectA != null && objectB != null)
             {
                 CreateLightningBurst(objectA, objectB);
             }
        }
    }
}
