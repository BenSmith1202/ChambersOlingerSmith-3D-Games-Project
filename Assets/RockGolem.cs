using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class RockGolem : MonoBehaviour
{
    [Header("Combat Settings")]
    public float detectionRange = 10f;
    public float rotationSpeed = 5f;
    public float moveSpeed = 2f;
    public float moveDistance = 3f;

    [Header("Attack Weights")]
    public float moveWeight = 1f;
    public float throwRockWeight = 1f;
    public float spawnRocksWeight = 1f;
    [Tooltip("Extra weight when player is far away")]
    public float throwRockDistanceBonus = 0.5f;

    [Header("Throw Rock Attack")]
    public GameObject rockProjectilePrefab;
    public Transform handBone;
    public float throwForce = 10f;
    public float throwWindupTime = 1f;
    public float throwCooldown = 3f;

    [Header("Spawn Rocks Attack")]
    public GameObject fallingRockPrefab;
    public float spawnHeight = 3f;
    public float spawnDistance = 2f;
    public float rockHangTime = 1f;
    public float rockFallSpeed = 5f;
    public float rockLifetime = 5f;
    public float spawnCooldown = 4f;

    [Header("Animation Parameters")]
    public string moveAnimBool = "IsMoving";
    public string throwAnimBool = "IsThrowing";
    public string spawnAnimBool = "IsSpawning";

    private Animator animator;
    private GameObject player;
    private EntityStats stats;
    private bool isAttacking;
    private float nextAttackTime;
    private Vector3[] cardinalDirections = {
        Vector3.forward, Vector3.back, Vector3.right, Vector3.left
    };

    private void Awake()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        stats = GetComponent<EntityStats>();
    }

    private void Update()
    {
        if(stats.currentHP <= 0)
        {
            Die();
        }

        // Always try to face player smoothly
        FacePlayer();

        if (isAttacking) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Check if player is in range and cooldown is over
        if (distanceToPlayer <= detectionRange && Time.time >= nextAttackTime)
        {
            ChooseAttack(distanceToPlayer);
        }
    }

    /// <summary>
    /// Smoothly rotates the golem to face the player
    /// </summary>
    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.transform.position - transform.position).normalized;
        direction.y = 0; // Only rotate on Y axis

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

    /// <summary>
    /// Randomly selects an attack based on weights and conditions
    /// </summary>
    private void ChooseAttack(float distanceToPlayer)
    {
        // Calculate weights with distance modifier
        float throwWeight = throwRockWeight;
        if (distanceToPlayer > detectionRange * 0.7f)
        {
            throwWeight += throwRockDistanceBonus;
        }

        float totalWeight = moveWeight + throwWeight + spawnRocksWeight;
        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue <= moveWeight)
        {
            StartCoroutine(MoveAttack());
        }
        else if (randomValue <= moveWeight + throwWeight)
        {
            StartCoroutine(ThrowRockAttack());
        }
        else
        {
            StartCoroutine(SpawnRocksAttack());
        }
    }

    /// <summary>
    /// Move towards player for a short distance
    /// </summary>
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
        nextAttackTime = Time.time + 1f; // Short cooldown for move
    }

    /// <summary>
    /// Perform rock throwing attack
    /// </summary>
    private IEnumerator ThrowRockAttack()
    {
        isAttacking = true;
        animator.SetBool(throwAnimBool, true);

        // Wait for windup animation
        yield return new WaitForSeconds(throwWindupTime);

        // Spawn rock at hand position
        GameObject rock = Instantiate(
            rockProjectilePrefab,
            handBone.position,
            Quaternion.identity
        );

        // Initialize rock with throw direction
        RockProjectile rockScript = rock.GetComponent<RockProjectile>();
        if (rockScript != null)
        {
            Vector3 throwDirection = (player.transform.position - handBone.position).normalized;
            rockScript.Initialize(throwDirection, throwForce, stats);
        }

        // Finish animation
        yield return new WaitForSeconds(0.5f);
        animator.SetBool(throwAnimBool, false);
        isAttacking = false;
        nextAttackTime = Time.time + throwCooldown;
    }

    /// <summary>
    /// Perform rock spawning attack
    /// </summary>
    private IEnumerator SpawnRocksAttack()
    {
        isAttacking = true;
        animator.SetBool(spawnAnimBool, true);

        // Wait for spawn animation to start
        yield return new WaitForSeconds(0.5f);

        // Spawn rocks in 4 directions
        foreach (Vector3 direction in cardinalDirections)
        {
            Vector3 spawnPos = transform.position + (direction * spawnDistance);
            spawnPos.y += spawnHeight;

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

        // Finish animation
        yield return new WaitForSeconds(1f);
        animator.SetBool(spawnAnimBool, false);
        isAttacking = false;
        nextAttackTime = Time.time + spawnCooldown;
    }

    private void Die()
    {
        // Handle death
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