using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ground-based dragon enemy that charges at the player
/// </summary>
public class DragonGround : MonoBehaviour
{
    [Header("Player Tracking")]
    [Tooltip("How fast the enemy rotates to face player (degrees/sec)")]
    public float rotationSpeed = 180f;

    [Header("Charge Attack Settings")]
    [Tooltip("Range at which enemy will start charging")]
    public float chargeRange = 10f;
    [Tooltip("Time between charge attacks")]
    public float chargeCooldown = 5f;
    [Tooltip("Delay after choosing target before starting charge animation")]
    public float chargeWindupTime = 0.5f;
    [Tooltip("Delay after animation starts before movement begins")]
    public float chargeStartDelay = 0.3f;
    [Tooltip("How long the charge movement lasts")]
    public float chargeDuration = 1.5f;
    [Tooltip("Speed during charge movement")]
    public float chargeSpeed = 8f;
    [Tooltip("Damage dealt on collision during charge")]
    public int chargeDamage = 20;
    [Tooltip("Knockback force on hit")]
    public float knockback = 10f;
    [Tooltip("Cooldown after hitting player before can damage again")]
    public float damageCooldown = 0.5f;
    [Tooltip("Length of wall detection raycast during charge")]
    public float wallCheckDistance = 1f;
    [Tooltip("Layer mask for wall detection")]
    public LayerMask wallMask;

    [Header("References")]
    [Tooltip("Animator controller (needs 'Charge' trigger)")]
    public Animator animator;

    private GameObject player;
    private EntityStats stats;
    private bool isDead = false;
    private bool isCharging = false;
    private Vector3 chargeDirection;
    private float nextChargeTime = 0f;
    private Rigidbody rb;
    private bool canDamagePlayer = true;
    private float chargeTimer = 0f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        stats = GetComponent<EntityStats>();
        rb = GetComponent<Rigidbody>();
        nextChargeTime = Time.time + chargeCooldown; // Start with cooldown
    }

    private void Update()
    {
        if (isDead) return;

        // Death check
        if (stats.currentHP <= 0)
        {
            // Trigger charge animation
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }
            Invoke("Die", 1);
            return;
        }

        // Only face player when not charging
        if (!isCharging)
        {
            FacePlayer();
        }

        // Charge attack logic
        if (!isCharging && Time.time >= nextChargeTime && player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= chargeRange)
            {
                StartCoroutine(ChargeAttack());
            }
        }

        // Handle charge movement and wall detection
        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            
            // Check for walls or charge duration completion
            if (chargeTimer >= chargeDuration || CheckForWalls())
            {
                StopCharge();
            }
        }
    }

    private void FixedUpdate()
    {
        // Handle charge movement in FixedUpdate for physics consistency
        if (isCharging)
        {
            rb.MovePosition(transform.position + chargeDirection * chargeSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Checks for walls in front of the charging enemy
    /// </summary>
    private bool CheckForWalls()
    {
        RaycastHit hit;
        return Physics.Raycast(transform.position, chargeDirection, out hit, wallCheckDistance, wallMask);
    }

    /// <summary>
    /// Full charge attack sequence: windup, animation, movement, cooldown
    /// </summary>
    private IEnumerator ChargeAttack()
    {
        isCharging = true;
        chargeTimer = 0f;
        nextChargeTime = Time.time + chargeCooldown;

        // Store charge direction at start (won't change during charge)
        chargeDirection = (player.transform.position - transform.position).normalized;
        chargeDirection.y = 0;
        chargeDirection.Normalize();

        // Windup period before animation
        yield return new WaitForSeconds(chargeWindupTime);

        // Trigger charge animation
        if (animator != null)
        {
            animator.SetTrigger("Charge");
        }

        // Delay before movement starts
        yield return new WaitForSeconds(chargeStartDelay);
    }

    /// <summary>
    /// Immediately stops the charge and resets state
    /// </summary>
    private void StopCharge()
    {
        isCharging = false;
        rb.velocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only deal damage during active charge movement
        if (isCharging && canDamagePlayer && collision.gameObject.CompareTag("Player"))
        {
            Attack chargeAttack = new Attack(
                gameObject,
                chargeDamage,
                0f, // No crit chance
                knockback,
                1f // Full proc coefficient
            );
            collision.gameObject.GetComponent<EntityStats>().TakeHit(chargeAttack);
            StartCoroutine(DamageCooldown());
        }
    }

    /// <summary>
    /// Temporarily prevents multiple hits during charge
    /// </summary>
    private IEnumerator DamageCooldown()
    {
        canDamagePlayer = false;
        yield return new WaitForSeconds(damageCooldown);
        canDamagePlayer = true;
    }

    /// <summary>
    /// Makes the enemy face the player smoothly on Y axis only
    /// </summary>
    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.transform.position - transform.position).normalized;
        direction.y = 0;

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

    private void OnDrawGizmosSelected()
    {
        if (isCharging)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, chargeDirection * wallCheckDistance);
        }
    }
}