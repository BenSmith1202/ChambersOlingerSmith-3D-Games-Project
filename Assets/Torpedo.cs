using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Standard torpedo projectile that moves straight and explodes on impact
/// </summary>
public class Torpedo : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed at which torpedo travels")]
    public float speed = 10f;
    [Tooltip("Lifetime in seconds before auto-destruct")]
    public float lifetime = 5f;

    [Header("Effects")]
    [Tooltip("Explosion prefab to spawn on impact")]
    public GameObject explosionPrefab;
    [Tooltip("Napalm to spawn on impact")]
    public GameObject napalmPrefab;
    [Tooltip("Sound played on impact")]
    public AudioClip impactSound;

    private AudioSource audioSource;
    private float spawnTime;

    private void Start()
    {
        spawnTime = Time.time;
        audioSource = GetComponent<AudioSource>();
        Destroy(gameObject, lifetime); // Auto-destruct after lifetime
    }

    private void Update()
    {
        // Move forward constantly
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("Player"))
        {
            Explode();
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            Explode();
        }

    }

    /// <summary>
    /// Spawns explosion effects and destroys torpedo
    /// </summary>
    private void Explode()
    {
        // Spawn explosion if prefab exists
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Spawn explosion if prefab exists
        if (napalmPrefab != null)
        {
            Instantiate(napalmPrefab, transform.position, Quaternion.identity);
        }

        // Play sound if available
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position);
        }

        Destroy(gameObject);
    }
}