using System.Collections;
using System.Collections.Generic;
using UnityEngine;




/// <summary>
/// Enemy that walks toward player and performs jumping slams
/// </summary>
public class JumpSlammer : MonoBehaviour
{
    [Header("Player Tracking")]
    [Tooltip("How fast the enemy rotates to face player (degrees/sec)")]
    public float rotationSpeed = 180f;
    [Tooltip("Only rotate on Y axis (for grounded enemies)")]
    public bool yAxisOnly = true;
    [Tooltip("How often to update player position (seconds)")]
    public float playerUpdateInterval = 0.2f;

    [Header("Movement Settings")]
    [Tooltip("Walking speed when approaching player")]
    public float walkSpeed = 2f;
    [Tooltip("Distance at which enemy stops walking and prepares to jump")]
    public float walkStopDistance = 5f;

    [Header("Jump Settings")]
    [Tooltip("Height of jump arc at max distance")]
    public float jumpHeight = 3f;
    [Tooltip("Maximum jump distance")]
    public float maxJumpDistance = 8f;
    [Tooltip("Minimum jump distance")]
    public float minJumpDistance = 2f;
    [Tooltip("Delay before jumping after deciding target")]
    public float jumpDelay = 0.5f;
    [Tooltip("Minimum clearance below ceiling")]
    public float ceilingClearance = 1f;
    [Tooltip("Time between jumps (seconds)")]
    public float jumpCooldown = 3f;
    [Tooltip("Custom gravity scale (base is half normal gravity)")]
    public float gravityScale = 0.5f;
    [Tooltip("Layer mask for ground/ceiling detection")]
    public LayerMask groundMask;

    [Header("Attack Settings")]
    [Tooltip("Range for shockwave damage on landing")]
    public float damageRange = 3f;
    [Tooltip("Damage dealt on landing")]
    public int slamDamage = 20;
    [Tooltip("Knockback force on landing")]
    public float knockback = 10f;
    [Tooltip("Prefab for ring laser effect")]
    public GameObject ringLaserPrefab;

    private GameObject player;
    private Vector3 lastPlayerPosition;
    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded = true;
    private bool isPerformingAction = false;
    private float ceilingHeight;
    private Vector3 jumpTarget;
    private Vector3 customGravity;
    private float currentGravity;
    private bool shouldWalk = false;


    private EntityStats stats;

    private void Start()
    {
        stats = gameObject.GetComponent<EntityStats>();

        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Disable Unity's gravity
        currentGravity = Physics.gravity.y * gravityScale;
        customGravity = new Vector3(0, currentGravity, 0);

        animator = GetComponentInChildren<Animator>();
        StartCoroutine(UpdatePlayerPositionRoutine());
        StartCoroutine(BehaviorCycleRoutine());
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
            animator.SetTrigger("Die");
            Invoke("Die", 1);
        }

        // Apply custom gravity
        if (!isGrounded)
        {
            rb.velocity += customGravity * Time.deltaTime;
        }

        // Smooth rotation to face player
        Vector3 lookDirection = lastPlayerPosition - transform.position;
        if (yAxisOnly) lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Ground check
        CheckGrounded();
    }

    private void FixedUpdate()
    {
        if (shouldWalk && isGrounded && !isPerformingAction)
        {
            WalkTowardPlayer();
        }
    }

    private void WalkTowardPlayer()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, lastPlayerPosition);
        if (distance > walkStopDistance)
        {
            Vector3 moveDirection = (lastPlayerPosition - transform.position).normalized;
            moveDirection.y = 0;
            rb.velocity = new Vector3(
                moveDirection.x * walkSpeed,
                rb.velocity.y,
                moveDirection.z * walkSpeed
            );

            if (animator != null)
            {
                animator.SetTrigger("Walk");
            }
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            if (animator != null)
            {
                animator.SetTrigger("Idle");
            }
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

    private IEnumerator BehaviorCycleRoutine()
    {
        while (true)
        {
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, lastPlayerPosition);

                if (!isPerformingAction && isGrounded)
                {
                    if (distance <= maxJumpDistance && distance >= minJumpDistance)
                    {
                        shouldWalk = false;
                        yield return StartCoroutine(JumpAction());
                        yield return new WaitForSeconds(jumpCooldown);
                        shouldWalk = true;
                    }
                    else
                    {
                        shouldWalk = true;
                    }
                }
            }
            yield return null;
        }
    }

    private IEnumerator JumpAction()
    {
        isPerformingAction = true;

        // Calculate ceiling height
        RaycastHit ceilingHit;
        if (Physics.Raycast(transform.position, Vector3.up, out ceilingHit, Mathf.Infinity, groundMask))
        {
            ceilingHeight = ceilingHit.point.y - ceilingClearance;
        }
        else
        {
            ceilingHeight = float.MaxValue;
        }

        // Set jump target with delay
        jumpTarget = lastPlayerPosition;
        yield return new WaitForSeconds(jumpDelay);

        // Calculate jump parameters with custom gravity
        Vector3 jumpDirection = (jumpTarget - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, jumpTarget);
        float clampedDistance = Mathf.Clamp(distance, minJumpDistance, maxJumpDistance);

        // Calculate initial velocity using custom gravity
        float maxHeight = Mathf.Min(jumpHeight, ceilingHeight - transform.position.y);
        float initialYVelocity = Mathf.Sqrt(2 * -currentGravity * maxHeight);
        float timeToPeak = initialYVelocity / -currentGravity;
        float initialXVelocity = clampedDistance / (2 * timeToPeak);

        // Combine velocities
        Vector3 jumpVelocity = new Vector3(
            jumpDirection.x * initialXVelocity,
            initialYVelocity,
            jumpDirection.z * initialXVelocity
        );

        // Execute jump
        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
        isGrounded = false;
        rb.velocity = jumpVelocity;


        yield return new WaitForSeconds(0.5f);


        // Wait until landing
        while (!isGrounded)
        {
            yield return null;
        }

        // Landing effects
        OnLand();
        isPerformingAction = false;
    }


    public float groundCheckDistance;
    private void CheckGrounded()
    {
        RaycastHit hit;
        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundMask);

        if (!wasGrounded && isGrounded)
        {
            if (animator != null)
            {
                animator.SetTrigger("Land");
            }
        }
    }

    private void OnLand()
    {

        rb.velocity = new Vector3(0, rb.velocity.y, 0);

        // Shockwave attack
        if (player != null && Vector3.Distance(transform.position, player.transform.position) <= damageRange)
        {
            Attack slamAttack = new Attack(
                gameObject,
                slamDamage,
                0f,
                knockback,
                1f
            );
            player.GetComponent<EntityStats>().TakeHit(slamAttack);
        }

        // Spawn ring laser
        if (ringLaserPrefab != null)
        {
            Instantiate(ringLaserPrefab, transform.position, Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minJumpDistance);
        Gizmos.DrawWireSphere(transform.position, maxJumpDistance);
        Gizmos.DrawWireSphere(transform.position, walkStopDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRange);
    }
}