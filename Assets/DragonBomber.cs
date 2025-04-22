using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Flying enemy that circles while periodically firing fireballs at the player
/// </summary>
public class DragonBomber : MonoBehaviour
{
    [Header("Circular Movement Settings")]
    [Tooltip("Radius of the circular flight path")]
    public float circleRadius = 5f;
    [Tooltip("Speed of circular movement (degrees/second)")]
    public float circleSpeed = 30f;
    [Tooltip("Center point of the circular movement (auto-calculated if null)")]
    public Transform circleCenter;
    [Tooltip("How fast the enemy rotates to face player (degrees/sec)")]
    public float rotationSpeed = 180f;

    [Header("Combat Settings")]
    [Tooltip("Range at which bomber starts attacking")]
    public float attackRange = 15f;
    [Tooltip("Time between attack cycles")]
    public float attackInterval = 4f;
    [Tooltip("Delay before firing after entering attack state")]
    public float attackWindup = 1f;
    [Tooltip("Number of fireballs per attack")]
    public int fireballCount = 3;
    [Tooltip("Delay between consecutive fireballs")]
    public float fireballDelay = 0.3f;
    [Tooltip("Fireball spawn position offset")]
    public Vector3 fireballSpawnOffset = new Vector3(0, -0.5f, 1f);

    [Header("References")]
    [Tooltip("Fireball prefab to spawn")]
    public GameObject fireballPrefab;
    [Tooltip("Animator controller (should have IsShooting bool parameter)")]
    public Animator animator;

    private GameObject player;
    private EntityStats stats;
    private float currentAngle;
    private float attackTimer;
    private Vector3 calculatedCenter;
    private bool isAttacking = false;
    private float initialYPosition; // Stores starting height

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        stats = GetComponentInChildren<EntityStats>();

        if (circleCenter == null)
        {
            calculatedCenter = new Vector3(transform.position.x, 0, transform.position.z);
        }
        else
        {
            calculatedCenter = new Vector3(circleCenter.position.x, 0, circleCenter.position.z);
        }

        attackTimer = attackInterval;
    }

    private bool dying = false;
    private void Update()
    {
        if (dying) return;

        if (stats != null && stats.currentHP <= 0)
        {
            dying = true;
            animator.SetTrigger("Die");
            Invoke("Die", 1.6f);
            return;
        }

        if (player == null) return;

        FacePlayer();
        UpdateCircularMovement();
        HandleAttackCycle();
    }

    /// <summary>
    /// Makes the dragon always face the player
    /// </summary>
    private void FacePlayer()
    {
        Vector3 lookDirection = (player.transform.position - transform.position).normalized;
        lookDirection.y = 0; // Only rotate on Y axis

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// Handles the circular flight pattern while maintaining initial height
    /// </summary>
    private void UpdateCircularMovement()
    {
        if (isAttacking) return;

        currentAngle += circleSpeed * Time.deltaTime;
        if (currentAngle > 360f) currentAngle -= 360f;

        Vector3 offset = new Vector3(
            Mathf.Sin(currentAngle * Mathf.Deg2Rad) * circleRadius,
            0,
            Mathf.Cos(currentAngle * Mathf.Deg2Rad) * circleRadius
        );

        // Maintain initial Y position
        Vector3 newPos = calculatedCenter + offset;
        newPos.y = initialYPosition;
        transform.position = newPos;
    }

    /// <summary>
    /// Manages the attack cycle timing
    /// </summary>
    private void HandleAttackCycle()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0 &&
            Vector3.Distance(transform.position, player.transform.position) <= attackRange)
        {
            StartCoroutine(AttackSequence());
            attackTimer = attackInterval;
        }
    }

    /// <summary>
    /// Full attack sequence including windup and firing
    /// </summary>
    private IEnumerator AttackSequence()
    {
        isAttacking = true;

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetBool("IsShooting", true);
        }

        // Windup period
        yield return new WaitForSeconds(attackWindup);

        // Fire projectiles
        for (int i = 0; i < fireballCount; i++)
        {
            FireFireball();
            if (i < fireballCount - 1) yield return new WaitForSeconds(fireballDelay);
        }

        // Return to idle animation
        if (animator != null)
        {
            animator.SetBool("IsShooting", false);
        }

        isAttacking = false;
    }

    /// <summary>
    /// Instantiates and launches a fireball at player
    /// </summary>
    private void FireFireball()
    {
        if (fireballPrefab == null || player == null) return;

        Vector3 spawnPosition = transform.position +
                               transform.TransformDirection(fireballSpawnOffset);

        GameObject fireball = Instantiate(
            fireballPrefab,
            spawnPosition,
            Quaternion.identity
        );

        FireballProjectile projectile = fireball.GetComponent<FireballProjectile>();
        if (projectile != null)
        {
            Vector3 fireDirection = (player.transform.position - spawnPosition).normalized;
            projectile.Initialize(fireDirection);
        }
    }

    /// <summary>
    /// Handles enemy death
    /// </summary>
    private void Die()
    {
        SpawnDirector spawnDirector = GameObject.FindWithTag("SpawnDirector")?.GetComponent<SpawnDirector>();
        if (spawnDirector != null)
        {
            spawnDirector.RegisterKill(gameObject, 3); // Higher value enemy
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw movement circle
        Vector3 center = circleCenter != null ? circleCenter.position : calculatedCenter;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, circleRadius);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}