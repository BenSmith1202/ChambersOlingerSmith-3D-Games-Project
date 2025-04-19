using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeed = 100f;
    public GameObject impactParticles;
    public int damage = 15;
    public float knockback = 7f;
    [Tooltip("Time to wait in place with 0 velocity before launching")]
    public float launchDelay = 0f; // New public variable for launch delay

    private Vector3 direction;
    private float speed;
    private EntityStats ownerStats;
    private bool hasCollided;
    private Vector3 randomRotationAxis;
   

    /// <summary>
    /// Initialize the thrown rock
    /// </summary>
    public void Initialize(Vector3 throwDirection, float throwSpeed, EntityStats owner)
    {
        direction = throwDirection.normalized;
        speed = throwSpeed;
        ownerStats = owner;
        randomRotationAxis = Random.onUnitSphere;

        // Destroy after 5 seconds if it doesn't hit anything
        Destroy(gameObject, 8f);

        
    }

    private void Update()
    {
        if (hasCollided) return;

        // Move in throw direction (only if not waiting)
        transform.position += direction * speed * Time.deltaTime;

        // Add random rotation
        transform.Rotate(randomRotationAxis, rotationSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;
        hasCollided = true;

        // Damage player if hit
        if (collision.gameObject.CompareTag("Player"))
        {
            EntityStats playerStats = collision.gameObject.GetComponent<EntityStats>();
            if (playerStats != null)
            {
                Attack rockAttack = new Attack(
                    ownerStats.gameObject,
                    damage,
                    0f,
                    knockback,
                    1f
                );
                playerStats.TakeHit(rockAttack);
            }
        }

        // Spawn impact effect
        if (impactParticles != null)
        {
            Instantiate(impactParticles, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);

        
    }
}