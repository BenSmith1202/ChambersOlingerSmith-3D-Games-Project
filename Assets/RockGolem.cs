using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockGolem : MonoBehaviour
{
    [Header("Combat Settings")]
    [Tooltip("Maximum distance at which the golem can detect the player")]
    public float detectionRange = 10f;
    [Tooltip("Speed at which the golem rotates to face the player (degrees/sec)")]
    public float rotationSpeed = 5f;
    [Tooltip("Movement speed during attacks")]
    public float moveSpeed = 2f;
    [Tooltip("Distance the golem moves forward during move attacks")]
    public float moveDistance = 3f;

    [Header("Attack Weights")]
    [Tooltip("Relative likelihood of choosing move attack")]
    public float moveWeight = 1f;
    [Tooltip("Relative likelihood of choosing rock throw attack")]
    public float throwRockWeight = 1f;
    [Tooltip("Relative likelihood of choosing rock spawn attack")]
    public float spawnRocksWeight = 1f;
    [Tooltip("Extra weight when player is far away")]
    public float throwRockDistanceBonus = 0.5f;

    [Header("Throw Rock Attack")]
    [Tooltip("Prefab for the thrown rock projectile")]
    public GameObject rockProjectilePrefab;
    [Tooltip("Transform representing the golem's hand position for throwing")]
    public Transform handBone;
    [Tooltip("Initial force applied to thrown rocks")]
    public float throwForce = 10f;
    [Tooltip("Time spent winding up before throwing")]
    public float throwWindupTime = 1f;
    [Tooltip("Cooldown time after throwing")]
    public float throwCooldown = 3f;

    [Header("Spawn Rocks Attack")]
    [Tooltip("Prefab for falling rocks")]
    public GameObject fallingRockPrefab;
    [Tooltip("Height above golem where rocks spawn")]
    public float spawnHeight = 3f;
    [Tooltip("Horizontal distance from golem where rocks spawn")]
    public float spawnDistance = 2f;
    [Tooltip("Time rocks hover before falling")]
    public float rockHangTime = 1f;
    [Tooltip("Speed at which rocks fall downward")]
    public float rockFallSpeed = 5f;
    [Tooltip("Time before unused rocks disappear")]
    public float rockLifetime = 5f;
    [Tooltip("Cooldown time after spawning rocks")]
    public float spawnCooldown = 4f;

    [Header("Animation Parameters")]
    [Tooltip("Animator boolean parameter for move animation")]
    public string moveAnimBool = "IsMoving";
    [Tooltip("Animator boolean parameter for throw animation")]
    public string throwAnimBool = "IsThrowing";
    [Tooltip("Animator boolean parameter for spawn animation")]
    public string spawnAnimBool = "IsSpawning";

    private Animator animator;
    private GameObject player;
    private EntityStats stats;
    private bool isAttacking;
    private float nextAttackTime;
    private Vector3[] cardinalDirections = {
        Vector3.forward, Vector3.back, Vector3.right, Vector3.left
    };

    // SUGGESTION: Consider adding [SerializeField] to private fields you might want to debug in Inspector
    // SUGGESTION: Add a rigidbody reference if you plan to add physics-based movement later

    private void Awake()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        stats = GetComponent<EntityStats>();
    }


    private bool dieing = false;

    private bool JustRocked = false;
    private void Update()
    {
        if (dieing)
        {
            return;
        }
        if (stats.currentHP <= 0 && !dieing)
        {
            animator.SetBool("Die", true);
            dieing = true;

            Invoke("Die", 1);
           
        }

        // SUGGESTION: Consider adding null check for player here
        FacePlayer();

        if (isAttacking) return;


        // SUGGESTION: Consider adding a "isPlayerInRange" bool to avoid distance calc every frame
        if (Time.time >= nextAttackTime)
        {
            ChooseAttack();
        }
    }

    /// <summary>
    /// Smoothly rotates the golem to face the player
    /// </summary>
    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.transform.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // SUGGESTION: Consider breaking this into smaller methods for better readability
    private void ChooseAttack()
    {

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);


        float throwWeight = throwRockWeight;
        if (distanceToPlayer > detectionRange * 0.7f)
        {
            throwWeight += throwRockDistanceBonus;
        }

        float totalWeight = moveWeight + throwWeight + spawnRocksWeight;
        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue <= moveWeight)
        {
            JustRocked = false;
            StartCoroutine(MoveAttack());
        }
        else if (randomValue <= moveWeight + throwWeight)
        {
            JustRocked = false;
            StartCoroutine(ThrowRockAttack());
        }
        else if (!JustRocked)
        {
            StartCoroutine(SpawnRocksAttack());
        }
        else
        {
            JustRocked = false;

            StartCoroutine(ThrowRockAttack());
        }
    }

    // SUGGESTION: Consider using Rigidbody.MovePosition for smoother physics-based movement
    private IEnumerator MoveAttack()
    {
        isAttacking = true;
        animator.SetBool(moveAnimBool, true);

        Vector3 startPos = transform.position;
        Vector3 direction = (player.transform.position - transform.position).normalized;
        direction.y = 0;

        float distanceMoved = 0f;
        while (distanceMoved < moveDistance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
            distanceMoved = Vector3.Distance(startPos, transform.position);
            yield return null;
        }

        animator.SetBool(moveAnimBool, false);
        isAttacking = false;
        nextAttackTime = Time.time + 1f;
    }

    public float handOffset;

    private IEnumerator ThrowRockAttack()
    {
        isAttacking = true;
        animator.SetBool(throwAnimBool, true);

        yield return new WaitForSeconds(throwWindupTime);

        Vector3 pos = handBone.position;

        pos = new Vector3(pos.x, pos.y + handOffset, pos.z);

        // SUGGESTION: Add null checks for handBone and prefab
        GameObject rock = Instantiate(
            rockProjectilePrefab,
            pos,
            Quaternion.identity
        );

        RockProjectile rockScript = rock.GetComponent<RockProjectile>();
        if (rockScript != null && player != null) // Added player null check
        {
            Vector3 throwDirection = (player.transform.position - handBone.position).normalized;
            rockScript.Initialize(throwDirection, throwForce, stats);
        }

        yield return new WaitForSeconds(0.5f);
        animator.SetBool(throwAnimBool, false);
        isAttacking = false;
        nextAttackTime = Time.time + throwCooldown;
    }

    private IEnumerator SpawnRocksAttack()
    {
        JustRocked = true;
        isAttacking = true;
        animator.SetBool(spawnAnimBool, true);

        yield return new WaitForSeconds(0.5f);

        foreach (Vector3 direction in cardinalDirections)
        {
            Vector3 spawnPos = transform.position + (direction * spawnDistance);
            spawnPos.y += spawnHeight;

            // SUGGESTION: Add null check for prefab
            GameObject rock = Instantiate(
                fallingRockPrefab,
                spawnPos,
                Quaternion.identity
            );

            FallingRock rockScript = rock.GetComponent<FallingRock>();
            if (rockScript != null)
            {
                rockScript.Initialize(rockHangTime, rockFallSpeed, rockLifetime, stats);
            }
        }

        yield return new WaitForSeconds(1f);
        animator.SetBool(spawnAnimBool, false);
        isAttacking = false;
        nextAttackTime = Time.time + spawnCooldown;
    }

    private void Die()
    {
        SpawnDirector spawnDirector = GameObject.FindWithTag("SpawnDirector")?.GetComponent<SpawnDirector>();
        if (spawnDirector != null)
        {
            spawnDirector.RegisterKill(gameObject, 3);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}