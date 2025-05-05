using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mech enemy that maintains distance while firing missiles at player
/// </summary>
public class MechScript : MonoBehaviour
{
    [Header("Player Tracking")]
    [Tooltip("How fast the mech rotates to face player (degrees/sec)")]
    public float rotationSpeed = 30f;
    [Tooltip("Only rotate on Y axis (for grounded enemies)")]
    public bool yAxisOnly = true;
    [Tooltip("How often to update player position (seconds)")]
    public float playerUpdateInterval = 0.2f;

    [Header("Combat Settings")]
    [Tooltip("Range at which mech becomes active")]
    public float activationRange = 30f;
    [Tooltip("Ideal distance to maintain from player")]
    public float desiredDistance = 25f;
    [Tooltip("Buffer distance around ideal distance")]
    public float distanceBuffer = 1f;
    [Tooltip("Base time between missile volleys")]
    public float fireRate = 3f;
    [Tooltip("Random variation added to fire rate")]
    public float fireRateVariation = 1f;
    [Tooltip("Delay between missiles in same volley")]
    public float missileDelay = 0.5f;
    [Tooltip("Random variation added to missile delay")]
    public float missileDelayVariation = 0.3f;
    [Tooltip("Rotation threshold to start firing (degrees)")]
    public float firingAngleThreshold = 10f;

    [Header("References")]
    [Tooltip("Missile prefab to spawn")]
    public GameObject missilePrefab;
    [Tooltip("Primary missile spawn point")]
    public Transform missileLaunchSpot;
    [Tooltip("Secondary missile spawn point")]
    public Transform missileLaunchSpot2;
    [Tooltip("Explosion effect on death")]
    public GameObject explosionEffect;

    private GameObject player;
    private Rigidbody rb;
    private EntityStats stats;
    private float nextFireTime = 0;
    private SpawnDirector spawnDirector;
    private Vector3 lastPlayerPosition;

    private void Start()
    {
        spawnDirector = GameObject.FindWithTag("SpawnDirector")?.GetComponent<SpawnDirector>();
        stats = GetComponent<EntityStats>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindWithTag("Player");
        StartCoroutine(UpdatePlayerPositionRoutine());
        StartCoroutine(Behavior());
    }

    private void FixedUpdate()
    {
        if (stats != null && stats.isDead)
        {
            Die();
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

    private IEnumerator Behavior()
    {
        while (true)
        {
            if (GetDistanceToPlayer() < activationRange)
            {
                // Smooth rotation to face player
                Quaternion targetRotation = Quaternion.LookRotation(GetXZDirectionToPlayer());
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );

                // Check if facing player enough to fire
                float angleToPlayer = Quaternion.Angle(transform.rotation, targetRotation);
                bool canFire = angleToPlayer < firingAngleThreshold;

                // Movement based on distance
                float currentDistance = GetDistanceToPlayer();
                if (canFire)
                {
                    if (currentDistance > desiredDistance + distanceBuffer)
                    {
                        rb.velocity = transform.forward * stats.getSpeed();
                    }
                    else if (currentDistance < desiredDistance - distanceBuffer)
                    {
                        rb.velocity = -transform.forward * stats.getSpeed();
                    }
                    else
                    {
                        rb.velocity = Vector3.zero;
                    }

                    // Firing logic
                    if (nextFireTime < Time.time)
                    {
                        FireMissile();
                    }
                }
                else
                {
                    rb.velocity = Vector3.zero;
                }
            }
            yield return null;
        }
    }

    private void FireMissile()
    {
        nextFireTime = Time.time + fireRate + Random.Range(0f, fireRateVariation);
        Instantiate(missilePrefab, missileLaunchSpot.position, GetRotationToPlayer());
        Invoke("FireSecondaryMissile", missileDelay + Random.Range(0f, missileDelayVariation));
    }

    private void FireSecondaryMissile()
    {
        Instantiate(missilePrefab, missileLaunchSpot2.position, GetRotationToPlayer());
    }

    private float GetDistanceToPlayer()
    {
        return Vector3.Distance(transform.position, lastPlayerPosition);
    }

    private Quaternion GetRotationToPlayer()
    {
        return Quaternion.LookRotation(GetXZDirectionToPlayer());
    }

    private Vector3 GetXZDirectionToPlayer()
    {
        Vector3 direction = (lastPlayerPosition - transform.position).normalized;
        if (yAxisOnly) direction.y = 0;
        return direction;
    }

    private void Die()
    {
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        if (spawnDirector != null)
        {
            spawnDirector.RegisterKill(gameObject, 3);
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw activation range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        // Draw ideal distance range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, desiredDistance);
    }
}