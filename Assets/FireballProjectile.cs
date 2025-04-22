using System.Collections;
using System.Collections.Generic;
using UnityEngine;




/// <summary>
/// Projectile fired by DragonBomber with configurable behavior
/// </summary>
public class FireballProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Initial speed of fireball")]
    public float speed = 10f;
    [Tooltip("Acceleration per second")]
    public float acceleration = 2f;
    [Tooltip("Maximum lifetime before auto-destruct")]
    public float maxLifetime = 5f;
    [Tooltip("Rotation speed for visual effect")]
    public float visualRotationSpeed = 90f;

    [Header("Impact Settings")]
    [Tooltip("Damage dealt on impact")]
    public int damage = 15;
    [Tooltip("Radius for splash damage")]
    public float splashRadius = 2f;
    [Tooltip("Knockback force on direct hit")]
    public float knockback = 5f;
    [Tooltip("Prefab to spawn on impact")]
    public GameObject explosionEffect;
    [Tooltip("Sound to play on impact")]
    public AudioClip impactSound;

    private Vector3 movementDirection;
    private float currentSpeed;
    private float lifetimeTimer;

    /// <summary>
    /// Initialize fireball with movement direction
    /// </summary>
    public void Initialize(Vector3 direction)
    {
        movementDirection = direction.normalized;
        currentSpeed = speed;
        lifetimeTimer = maxLifetime;
        transform.rotation = Quaternion.LookRotation(movementDirection);
    }

    private void Update()
    {
        // Movement
        currentSpeed += acceleration * Time.deltaTime;
        transform.position += movementDirection * currentSpeed * Time.deltaTime;

        // Visual rotation
        transform.Rotate(Vector3.right, visualRotationSpeed * Time.deltaTime);

        // Lifetime check
        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

   
    private void OnCollisionEnter(Collision collision)
    {
        // Only explode on player or ground
        if (collision.gameObject.CompareTag("Player"))
        {
            CreateImpactEffects();
            Destroy(gameObject);
        }


        if (collision.gameObject.CompareTag("Ground"))
        {
            CreateImpactEffects();
            Destroy(gameObject);
        }


    }

    /// <summary>
    /// Handles damage application to player
    /// </summary>
    private void DealDamage(GameObject player)
    {
        EntityStats playerStats = player.GetComponent<EntityStats>();
        if (playerStats != null)
        {
            Attack fireballAttack = new Attack(
                null, // No owner
                damage,
                0f, // No crit
                knockback,
                1f // Full proc coefficient
            );
            playerStats.TakeHit(fireballAttack);
        }
    }

    /// <summary>
    /// Creates visual and audio impact effects
    /// </summary>
    private void CreateImpactEffects()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position);
        }
    }
}