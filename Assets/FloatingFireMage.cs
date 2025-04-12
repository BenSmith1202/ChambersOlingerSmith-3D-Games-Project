using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A floating enemy that moves in circular patterns and shoots fireballs at the player.
/// Modified version that only moves upward to adjust height, never downward.
/// </summary>
public class FloatingFireMage : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Radius of the circular movement path")]
    public float circleRadius = 3f;
    [Tooltip("Speed of circular movement")]
    public float circleSpeed = 1f;
    [Tooltip("Height above ground the enemy floats at")]
    public float floatHeight = 3f;
    [Tooltip("How quickly the enemy adjusts height upward")]
    public float heightAdjustSpeed = 2f;

    [Header("Combat Settings")]
    [Tooltip("Time between fireball attacks")]
    public float attackCooldown = 3f;
    [Tooltip("Delay after facing player before shooting")]
    public float attackWindup = 0.5f;
    [Tooltip("Range at which enemy will start attacking")]
    public float attackRange = 10f;
    [Tooltip("Prefab for the fireball projectile")]
    public GameObject fireballPrefab;
    [Tooltip("Position where fireballs spawn")]
    public Transform fireballSpawnPoint;

    [Header("Effects")]
    [Tooltip("Sound played when shooting")]
    public AudioClip shootSound;
    [Tooltip("Particle effect played when shooting")]
    public ParticleSystem shootEffect;

    // Private variables
    private GameObject player;
    private float angle = 0f;
    private Vector3 circleCenter;
    private float attackTimer;
    private AudioSource audioSource;
    private EntityStats stats;
    private float currentHeight; // Track height separately

    /// <summary>
    /// Initializes references and sets up starting position
    /// </summary>
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        audioSource = GetComponent<AudioSource>();
        stats = GetComponent<EntityStats>();

        // Set initial circle center at current position
        circleCenter = transform.position;
        circleCenter.y = 0; // Keep circle on horizontal plane

        attackTimer = attackCooldown; // Start ready to attack
        currentHeight = transform.position.y; // Initialize height
    }

    /// <summary>
    /// Handles death check and timers
    /// </summary>
    private void Update()
    {
        if (stats != null && stats.isDead)
        {
            Die();
            return;
        }

        attackTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Handles circular movement and attacking logic
    /// </summary>
    private void FixedUpdate()
    {
        if (player == null) return;

        // Circular movement
        angle += circleSpeed * Time.fixedDeltaTime;
        Vector3 targetPosition = circleCenter + new Vector3(
            Mathf.Cos(angle) * circleRadius,
            0,
            Mathf.Sin(angle) * circleRadius
        );

        // Height adjustment - only moves upward if needed
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, floatHeight * 2f))
        {
            float desiredHeight = hit.point.y + floatHeight;

            // Only adjust height if we're below desired height
            if (currentHeight < desiredHeight)
            {
                currentHeight = Mathf.Lerp(currentHeight, desiredHeight, heightAdjustSpeed * Time.fixedDeltaTime);
            }
        }

        targetPosition.y = currentHeight;

        // Apply movement
        transform.position = targetPosition;

        // Always face player
        Vector3 lookDirection = player.transform.position - transform.position;
        lookDirection.y = 0; // Only rotate on Y axis
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // Attack logic
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= attackRange && attackTimer <= 0f)
        {
            StartCoroutine(AttackRoutine());
            attackTimer = attackCooldown;
        }
    }






    /// <summary>
    /// Coroutine that handles the attack windup and firing
    /// </summary>
    private IEnumerator AttackRoutine()
    {
        // Windup period
        yield return new WaitForSeconds(attackWindup);

        // Play effects if they exist
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        if (shootEffect != null)
        {
            shootEffect.Play();
        }

        // Spawn fireball if prefab exists
        if (fireballPrefab != null && fireballSpawnPoint != null)
        {
            Vector3 toPlayer = (player.transform.position - fireballSpawnPoint.position);
            float distance = toPlayer.magnitude;

            // Calculate initial launch angle (slightly upward)
            Vector3 horizontalDirection = new Vector3(toPlayer.x, 0, toPlayer.z).normalized;
            float verticalAngle = Mathf.Clamp(distance * 0.1f, 5f, 45f); // Dynamic angle based on distance

            // Create fireball with arc trajectory
            GameObject fireball = Instantiate(
                fireballPrefab,
                fireballSpawnPoint.position,
                Quaternion.LookRotation(toPlayer)
            );

            // Initialize arc trajectory
            FireballProjectile projectile = fireball.GetComponent<FireballProjectile>();
            if (projectile != null)
            {
                projectile.InitializeArc(
                    horizontalDirection,
                    verticalAngle,
                    distance
                );
            }
        }
    }





    /// <summary>
    /// Handles enemy death and notifies spawn system
    /// </summary>
    private void Die()
    {
        SpawnDirector spawnDirector = GameObject.FindWithTag("SpawnDirector")?.GetComponent<SpawnDirector>();
        if (spawnDirector != null)
        {
            spawnDirector.RegisterKill(gameObject, 2); // Higher value than basic enemies
        }
        else
        {
            Destroy(gameObject);
        }
    }
}