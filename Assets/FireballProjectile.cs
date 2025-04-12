using System.Collections;
using System.Collections.Generic;
using UnityEngine;




/// <summary>
/// Handles behavior of arcing fireball projectiles with ballistic trajectory
/// </summary>
public class FireballProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base speed of the fireball")]
    public float baseSpeed = 10f;
    [Tooltip("Initial upward angle in degrees")]
    public float initialAngle = 25f;
    [Tooltip("Gravity force affecting the projectile")]
    public float gravity = 9.8f;
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
    private Vector3 horizontalDirection;
    private float verticalSpeed;
    private float currentVerticalSpeed;
    private float horizontalSpeed;

    /// <summary>
    /// Initializes the arc trajectory
    /// </summary>
    /// <param name="direction">Horizontal direction to target</param>
    /// <param name="angle">Launch angle in degrees</param>
    /// <param name="distance">Initial distance to target</param>
    public void InitializeArc(Vector3 direction, float angle, float distance)
    {
        horizontalDirection = direction.normalized;
        float angleRad = angle * Mathf.Deg2Rad;

        // Calculate initial velocities
        horizontalSpeed = baseSpeed * Mathf.Cos(angleRad);
        verticalSpeed = baseSpeed * Mathf.Sin(angleRad);
        currentVerticalSpeed = verticalSpeed;

        spawnTime = Time.time;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (lifetime > 0) Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Updates the projectile's ballistic trajectory
    /// </summary>
    private void Update()
    {
        // Apply gravity
        currentVerticalSpeed -= gravity * Time.deltaTime;

        // Calculate movement
        Vector3 movement = (horizontalDirection * horizontalSpeed) +
                          (Vector3.up * currentVerticalSpeed);

        // Apply movement
        transform.position += movement * Time.deltaTime;

        // Rotate to face movement direction
        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(movement);
        }
    }







    /// <summary>
    /// Handles collision with objects
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {

        // Get the collider's GameObject
        GameObject other = collision.gameObject;

        // Only collide with ground or player
        if (!other.CompareTag("Ground") && !other.CompareTag("Player")) return;


        // Damage player if hit
        if (other.CompareTag("Player"))
        {
            EntityStats playerStats = other.GetComponent<EntityStats>();
            if (playerStats != null)
            {
                playerStats.InflictDamage(damage);
            }
        }

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