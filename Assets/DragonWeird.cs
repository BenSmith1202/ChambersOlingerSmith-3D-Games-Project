using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Flying dragon enemy that alternates between moving to an offset position and firing flames
/// </summary>
public class DragonWeird : MonoBehaviour
{
    [Header("Player Tracking")]
    [Tooltip("How fast the enemy rotates to face player (degrees/sec)")]
    public float rotationSpeed = 180f;

    [Header("Behavior Settings")]
    [Tooltip("Time between action cycles")]
    public float actionCycleTime = 3f;
    [Tooltip("Delay before starting first action")]
    public float initialDelay = 1f;

    [Header("Movement Settings")]
    [Tooltip("Speed during movement phase")]
    public float moveSpeed = 5f;
    [Tooltip("Vertical offset above player position")]
    public float verticalOffset = 2f;
    [Tooltip("Horizontal offset from player position")]
    public float horizontalOffset = 2f;
    [Tooltip("Distance threshold to consider position reached")]
    public float arrivalThreshold = 0.5f;
    [Tooltip("Animator trigger for move animation")]
    public string moveTrigger = "move";

    [Header("Attack Settings")]
    [Tooltip("Prefab for flame attack")]
    public GameObject flamesPrefab;
    [Tooltip("Position offset for flame spawn")]
    public Vector3 flameSpawnOffset = new Vector3(0, -0.5f, 1f);
    [Tooltip("Delay after animation starts before spawning flames")]
    public float flameSpawnDelay = 0.3f;
    [Tooltip("Animator trigger for fire animation")]
    public string fireTrigger = "fire";

    private GameObject player;
    private EntityStats stats;
    private Animator animator;
    private bool isDead = false;
    private bool isPerformingAction = false;
    private Vector3 targetPosition;
    private float nextActionTime;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        stats = GetComponentInChildren<EntityStats>();
        animator = GetComponent<Animator>();
        nextActionTime = Time.time + initialDelay;
    }

    private void Update()
    {
        if (isDead) return;

        // Death check
        if (stats.currentHP <= 0)
        {
            Die();
            return;
        }

        // Always face the player
        FacePlayer();

        // Action cycle logic
        if (!isPerformingAction && Time.time >= nextActionTime)
        {
            StartCoroutine(ActionCycle());
        }

        // Movement handling
        if (isPerformingAction)
        {
            MoveToTarget();
        }
    }

    /// <summary>
    /// Alternates between moving and attacking
    /// </summary>
    private IEnumerator ActionCycle()
    {
        isPerformingAction = true;

        // Movement phase
        CalculateOffsetPosition();
        animator.SetTrigger(moveTrigger);
        yield return new WaitUntil(() => Vector3.Distance(transform.position, targetPosition) <= arrivalThreshold);

        // Attack phase
        animator.SetTrigger(fireTrigger);
        yield return new WaitForSeconds(flameSpawnDelay);
        SpawnFlames();

        // Cooldown
        isPerformingAction = false;
        nextActionTime = Time.time + actionCycleTime;
    }

    /// <summary>
    /// Calculates target position with offset from player
    /// </summary>
    private void CalculateOffsetPosition()
    {
        if (player == null) return;

        // Get random side (left or right)
        float side = Random.value > 0.5f ? 1f : -1f;

        // Calculate offset position
        Vector3 playerForward = player.transform.forward;
        Vector3 playerRight = player.transform.right;
        targetPosition = player.transform.position +
                         (playerRight * horizontalOffset * side) +
                         (Vector3.up * verticalOffset);
    }

    /// <summary>
    /// Moves toward the calculated target position
    /// </summary>
    private void MoveToTarget()
    {
        if (Vector3.Distance(transform.position, targetPosition) > arrivalThreshold)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// Spawns flame projectile aimed at player
    /// </summary>
    private void SpawnFlames()
    {
        if (flamesPrefab == null || player == null) return;

        // Calculate spawn position with offset
        Vector3 spawnPosition = transform.position + transform.TransformDirection(flameSpawnOffset);

        // Instantiate and aim at player
        GameObject flames = Instantiate(flamesPrefab, spawnPosition, Quaternion.identity);
        Vector3 directionToPlayer = (player.transform.position - spawnPosition).normalized;
        flames.transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    /// <summary>
    /// Makes the enemy face the player smoothly
    /// </summary>
    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.transform.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// Standard death handling - notifies spawn system and destroys object
    /// </summary>
    private void Die()
    {
        isDead = true;

        SpawnDirector spawnDirector = GameObject.FindWithTag("SpawnDirector")?.GetComponent<SpawnDirector>();
        if (spawnDirector != null)
        {
            spawnDirector.RegisterKill(gameObject, 2); // Mid-tier enemy value
        }
        else
        {
            Destroy(gameObject);
        }
    }
}