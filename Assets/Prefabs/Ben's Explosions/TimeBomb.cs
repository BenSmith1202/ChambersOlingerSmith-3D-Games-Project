using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBomb : MonoBehaviour
{

    public float timer = 5f; // Time in seconds before the bomb explodes
    public GameObject explosionPrefab; // Prefab for the explosion effect
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Explode());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Explode()
    {
        yield return new WaitForSeconds(timer);
        // Call the explosion function here
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject); // Destroy the bomb object after explosion
    }
}
