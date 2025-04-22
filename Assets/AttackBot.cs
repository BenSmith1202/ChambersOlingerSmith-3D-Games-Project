using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Grounded combat bot that alternates between moving and shooting heat-seeking missiles
/// </summary>
public class AttackBot : MonoBehaviour
{
    [Header("Player Tracking")]
    [Tooltip("How fast the bot rotates to face player (degrees/sec)")]
    public float rotationSpeed = 120f;
    [Tooltip("Only rotate on Y axis (for grounded enemies)")]
    public bool yAxisOnly = true;
    [Tooltip("How often to update player position (seconds)")]
    public float playerUpdateInterval = 0.2f;

    [Header("Combat Settings")]
    [Tooltip("Range at which bot becomes active")]
    public float activationRange = 15f;
    [Tooltip("Distance at which bot will retreat from player")]
    public float retreatDistance = 5f;
    [Tooltip("Time between actions (seconds)")]
    public float actionDelay = 1f;

    [Header("Movement")]
    [Tooltip("Movement speed when retreating")]
    public float moveSpeed = 3f;
    [Tooltip("Distance to check behind for obstacles")]
    public float obstacleCheckDistance = 2f;
    [Tooltip("Layer mask for obstacle detection")]
    public LayerMask obstacleMask;

    [Header("Shooting")]
    [Tooltip("Missile prefab to spawn")]
    public GameObject missilePrefab;
    
    [Tooltip("Position references for missile spawn points")]
    public Transform[] missileSpawnPoints; // Should have 2 points minimum

    private GameObject player;
    private Vector3 lastPlayerPosition;
    private bool isShootingPhase = false;
    private bool isPerformingAction = false;

    private EntityStats stats;


    private void Start()
    {
        stats = gameObject.GetComponent<EntityStats>();


        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(UpdatePlayerPositionRoutine());
        StartCoroutine(ActionCycleRoutine());
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

    private void Update()
    {
        if (player == null) return;

        if (stats.isDead)
        {
            Die();
        }

        // Smooth rotation to face player with 90 degree offset
        Vector3 lookDirection = lastPlayerPosition - transform.position;
        if (yAxisOnly) lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            // Apply 90 degree Y-axis offset to neutral rotation
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection) * Quaternion.Euler(0, -90, 0);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private IEnumerator UpdatePlayerPositionRoutine()
    {
        while (true)
        {
            if (player != null)
            {
                lastPlayerPosition = player.transform.position;
            }
            yield return new WaitForSeconds(playerUpdateInterval);
        }
    }

    private IEnumerator ActionCycleRoutine()
    {
        while (true)
        {
            if (!isPerformingAction && player != null &&
                Vector3.Distance(transform.position, player.transform.position) <= activationRange)
            {
                if (isShootingPhase)
                {
                    yield return StartCoroutine(ShootAction());
                }
                else
                {
                    yield return StartCoroutine(MoveAction());
                }

                isShootingPhase = !isShootingPhase;
                yield return new WaitForSeconds(actionDelay);
            }
            yield return null;
        }
    }

    private IEnumerator MoveAction()
    {
        isPerformingAction = true;

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // Only move if player is too close
            if (distanceToPlayer < retreatDistance)
            {
                // Check for obstacles behind
                RaycastHit hit;
                bool obstacleBehind = Physics.Raycast(
                    transform.position,
                    -transform.forward,
                    out hit,
                    obstacleCheckDistance,
                    obstacleMask
                );

                // Move backward if no obstacle
                if (!obstacleBehind)
                {
                    float moveTimer = 0f;
                    float moveDuration = actionDelay;

                    while (moveTimer < moveDuration)
                    {
                        transform.position -= transform.forward * moveSpeed * Time.deltaTime;
                        moveTimer += Time.deltaTime;
                        yield return null;
                    }
                }
            }
        }

        isPerformingAction = false;
    }

    private IEnumerator ShootAction()
    {
        isPerformingAction = true;

        if (missilePrefab != null && missileSpawnPoints.Length >= 2)
        {
            // Fire from both shoulder cannons
            for (int i = 0; i < 2; i++)
            {
                Vector3 spawnPos = missileSpawnPoints[i].position;
                Quaternion spawnRot = missileSpawnPoints[i].rotation;

                GameObject missile = Instantiate(missilePrefab, spawnPos, spawnRot);
                HeatSeekingMissile missileScript = missile.GetComponent<HeatSeekingMissile>();

                if (missileScript != null && player != null)
                {
                    missileScript.Initialize(player.transform);
                }

                //// Add small delay between missile spawns
                //if (i < 1) // Only wait after first missile
                //{
                //    yield return new WaitForSeconds(0.1f); // Small delay between shots
                //}
            }
        }

        isPerformingAction = false;

        // Ensure coroutine yields at least once
        yield return null;
    }

    // Visualize obstacle check in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position - transform.forward * obstacleCheckDistance);
    }
}