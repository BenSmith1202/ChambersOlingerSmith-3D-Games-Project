using System.Collections;
using System.Collections.Generic;
using UnityEngine;




/// <summary>
/// Handles behavior of arcing fireball projectiles with ballistic trajectory
/// </summary>
public class FireballProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed the fireball travels")]
    public float speed = 10f;
    [Tooltip("Lifetime in seconds before auto-destruct")]
    public float lifetime = 5f;

    [Header("Impact Settings")]
    [Tooltip("Damage dealt to player on hit")]
    public int damage = 20;
    [Tooltip("Explosion prefab to spawn on impact")]
    public GameObject explosionPrefab;
    [Tooltip("Sound played on impact")]
    public AudioClip impactSound;

    // Private variables
    private float spawnTime;
    private AudioSource audioSource;
    private Vector3 movementDirection; // Stores initial firing direction

    /// <summary>
    /// Initializes the projectile and sets destruction timer
    /// </summary>
    private void Start()
    {
        spawnTime = Time.time;
        audioSource = GetComponent<AudioSource>();
        movementDirection = transform.forward; // Store initial facing direction
    }

    /// <summary>
    /// Moves the projectile in its initial direction each frame
    /// </summary>
    private void Update()
    {
        // Move in the initial fired direction
        transform.position += movementDirection * speed * Time.deltaTime;

        // Destroy after lifetime expires
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }





    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            

            Explode();
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            
        }
        else
        {
            Explode();
        }
        
    }



    public void Explode()
    {
        // Spawn explosion if prefab exists
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Play sound if available
        if (impactSound != null && audioSource != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position);
        }

        Destroy(gameObject);
    }





}







