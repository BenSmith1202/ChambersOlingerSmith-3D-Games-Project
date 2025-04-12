using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles behavior of individual shotgun bullets
/// </summary>
public class ShotgunBullet : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Initial speed of the bullet")]
    public float initialSpeed = 15f;
    [Tooltip("Acceleration per second (0 for constant speed)")]
    public float acceleration = 0f;
    [Tooltip("Lifetime in seconds before auto-destruct")]
    public float lifetime = 2f;

    [Header("Damage Settings")]
    [Tooltip("Base damage dealt by this bullet")]
    public int baseDamage = 5;
    [Tooltip("Knockback force applied")]
    public float knockback = 5f;

    private float currentSpeed;
    private EntityStats ownerStats;

    /// <summary>
    /// Initializes the bullet with owner's stats
    /// </summary>
    public void Initialize(EntityStats owner)
    {
        ownerStats = owner;
        currentSpeed = initialSpeed;
        if (lifetime > 0) Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Handles movement each frame
    /// </summary>
    private void Update()
    {
        // Apply acceleration if enabled
        if (acceleration != 0)
        {
            currentSpeed += acceleration * Time.deltaTime;
        }

        // Move forward
        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    /// <summary>
    /// Handles collision with objects
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Create attack object
            Attack bulletAttack = new Attack(
                ownerStats.gameObject,
                baseDamage,
                ownerStats.getCritChance(),
                knockback,
                1f // Proc coefficient
            );

            // Apply damage
            collision.gameObject.GetComponent<EntityStats>().TakeHit(bulletAttack);
        }

        // Destroy on any collision
        Destroy(gameObject);
    }
}