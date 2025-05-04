using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple grounded enemy that faces the player and fires single bullets.
/// Uses the ShotgunBullet prefab for its projectile.
/// </summary>
[RequireComponent(typeof(EntityStats))] // Ensures EntityStats is attached
[RequireComponent(typeof(AudioSource))] // Ensures AudioSource is attached for sound effects
public class GunPolice : MonoBehaviour
{
    [Header("Player Tracking")]
    [Tooltip("How fast the enemy rotates to face the player (degrees/sec)")]
    public float rotationSpeed = 180f;
    [Tooltip("Restrict rotation to the Y axis only")]
    public bool yAxisOnly = true;

    [Header("Combat Settings")]
    [Tooltip("Time between consecutive attacks (seconds)")]
    public float attackCooldown = 2f;
    [Tooltip("Delay after aiming before the shot is fired (seconds)")]
    public float attackWindup = 0.5f;
    [Tooltip("Maximum distance at which the enemy will start attacking")]
    public float attackRange = 15f;
    [Tooltip("Value assigned for score/spawning system upon kill")]
    public int killValue = 1; // Value used in the Die() method

    [Header("Projectile")]
    [Tooltip("The bullet prefab to spawn (Must have ShotgunBullet script attached)")]
    public GameObject bulletPrefab; // Expects the ShotgunBullet prefab
    [Tooltip("Transform defining the exact position and initial forward direction for bullet spawn")]
    public Transform bulletSpawnPoint;

    [Header("Effects")]
    [Tooltip("Sound effect played when shooting (Optional)")]
    public AudioClip shootSound;
    [Tooltip("Particle effect played at the spawn point when shooting (Optional)")]
    public ParticleSystem shootEffect;

    // --- Private Variables ---
    private GameObject player;        // Reference to the player object
    private EntityStats stats;        // Reference to this enemy's EntityStats
    private AudioSource audioSource;  // Reference to the AudioSource for sound effects
    private float attackTimer;        // Timer to manage attack cooldown
    private bool isDead = false;      // Flag to prevent actions after death

    /// <summary>
    /// Finds the Player, gets required components, and initializes the attack timer.
    /// </summary>
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        stats = GetComponent<EntityStats>();
        audioSource = GetComponent<AudioSource>();

        // Start ready to attack if player is immediately in range
        attackTimer = 0f;
    }

    /// <summary>
    /// Handles death check, player tracking, and attack timing each frame.
    /// </summary>
    void Update()
    {
        // Stop updates if dead or player is missing
        if (isDead || player == null) return;

        // Check the death condition from EntityStats
        if (stats.isDead && !isDead)
        {
            Die();
            return; // Exit Update if dead
        }

        // Orient towards the player
        FacePlayer();

        // Manage attack cooldown
        attackTimer -= Time.deltaTime;

        // Check distance and cooldown before attacking
        if (Vector3.Distance(transform.position, player.transform.position) <= attackRange && attackTimer <= 0f)
        {
            StartCoroutine(AttackRoutine());
            attackTimer = attackCooldown; // Reset cooldown timer
        }
    }

    /// <summary>
    /// Rotates the enemy to face the player smoothly.
    /// </summary>
    private void FacePlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;

        // Optionally flatten the direction vector for Y-axis only rotation
        if (yAxisOnly)
        {
            direction.y = 0;
        }

        // Rotate only if the direction is valid
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime // Apply rotation speed
            );
        }
    }

    /// <summary>
    /// Handles the attack sequence: waits for windup, plays effects, fires bullet.
    /// </summary>
    private IEnumerator AttackRoutine()
    {
        // Wait for the windup duration
        yield return new WaitForSeconds(attackWindup);

        // Play sound effect if assigned
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Play particle effect if assigned
        if (shootEffect != null)
        {
            shootEffect.Play(); // Assumes the ParticleSystem is set up to play on command
        }

        // Fire the bullet
        FireSingleBullet();
    }

    /// <summary>
    /// Instantiates the bullet prefab and initializes its ShotgunBullet script.
    /// </summary>
    private void FireSingleBullet()
    {
        // Check if essential references are set
        if (bulletPrefab == null || bulletSpawnPoint == null || player == null)
        {
            Debug.LogWarning($"{gameObject.name}: Missing Bullet Prefab, Spawn Point, or Player reference.", this);
            return;
        }

        // Determine the direction from the spawn point to the player
        Vector3 fireDirection = (player.transform.position - bulletSpawnPoint.position).normalized;

        // Create the bullet instance facing the calculated direction
        GameObject bulletInstance = Instantiate(
            bulletPrefab,
            bulletSpawnPoint.position,
            Quaternion.LookRotation(fireDirection)
        );

        // Get the ShotgunBullet script from the instantiated prefab
        ShotgunBullet bulletScript = bulletInstance.GetComponent<ShotgunBullet>();
        if (bulletScript != null)
        {
            // Initialize the bullet, passing this enemy's stats
            bulletScript.Initialize(stats);
        }
        else
        {
            // Log an error if the prefab is missing the required script
            Debug.LogError($"{gameObject.name}: Bullet Prefab '{bulletPrefab.name}' is missing the ShotgunBullet script.", bulletPrefab);
            Destroy(bulletInstance); // Destroy the incorrectly configured bullet
        }
    }

    /// <summary>
    /// Handles the enemy's death process.
    /// </summary>
    private void Die()
    {
        isDead = true; // Set flag to prevent further actions

        // Notify the SpawnDirector if it exists in the scene
        SpawnDirector spawnDirector = GameObject.FindWithTag("SpawnDirector")?.GetComponent<SpawnDirector>();
        if (spawnDirector != null)
        {
            spawnDirector.RegisterKill(gameObject, killValue);
            // Assuming SpawnDirector might handle destroying the object.
            // If it doesn't, the object needs to be destroyed here or by another system.
        }
        else
        {
            // If no SpawnDirector, destroy the enemy object immediately
            Destroy(gameObject);
        }

        // If SpawnDirector exists but does NOT handle destruction, uncomment the following line:
        // Destroy(gameObject);
    }
}
