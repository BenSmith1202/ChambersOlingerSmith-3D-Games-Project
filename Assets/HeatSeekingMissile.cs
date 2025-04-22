using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Missile that seeks target and explodes on impact
/// </summary>
public class HeatSeekingMissile : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base speed of missile")]
    public float speed = 8f;
    [Tooltip("How quickly missile can turn (degrees/sec)")]
    public float turnRate = 90f;
    [Tooltip("Lifetime before auto-destruct (seconds)")]
    public float lifetime = 5f;

  


    [Header("Effects")]
    [Tooltip("Explosion prefab to spawn on impact")]
    public GameObject explosionPrefab;
    [Tooltip("Sound played on impact")]
    public AudioClip explosionSound;

    private Transform target;
    private float spawnTime;

    private EntityStats stats;


    private void Start()
    {
        stats = gameObject.GetComponent<EntityStats>();
    }


    /// <summary>
    /// Initialize missile with target reference
    /// </summary>
    public void Initialize(Transform targetTransform)
    {
        target = targetTransform;
        spawnTime = Time.time;
        Destroy(gameObject, lifetime);
    }

    

    private void Update()
    {
        if (target == null) return;

        if(stats.currentHP <= 0)
        {
            Explode();
        }



        // Calculate direction to target
        Vector3 targetDirection = (target.position - transform.position).normalized;

        // Rotate toward target with -90 degree X-axis offset
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnRate * Time.deltaTime
        );

        // Move forward
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only explode on player or ground
        if (collision.gameObject.CompareTag("Player"))
        {
            Explode();
        }


        if (collision.gameObject.CompareTag("Ground"))
        {
            print("EXPLODING");
            Explode();
        }


    }

    private void Explode()
    {
        // Spawn explosion effect
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Play explosion sound
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        Destroy(gameObject);
    }
}