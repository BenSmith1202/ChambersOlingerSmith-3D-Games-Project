using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehavior : MonoBehaviour
{
    [Header("Targeting Settings")]
    [SerializeField] private GameObject player;
    [SerializeField] private float awarenessDistance = 30f;
    [SerializeField] private LayerMask obstructionLayers;
    [SerializeField] private Color laserSightColor = new Color(1f, 0f, 0f, 0.6f); // Reduced opacity to 0.6

    [Header("Attack Settings")]
    [SerializeField] private float aimTime = 1.5f;
    [SerializeField] private float damagePerShot = 5f;
    [SerializeField] private float fireRate = 10f;
    [SerializeField] private float attackDuration = 3f;

    [Header("Effects")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private AudioClip shootingSound;
    [SerializeField] private GameObject deathExplosion;
    [SerializeField] private Transform muzzlePoint;

    // Component references
    private EntityStats entityStats;
    private BuffManager buffManager;
    private AudioSource audioSource;
    private SpawnDirector spawnDirector;
    private LineRenderer laserSight;

    // State tracking
    private Coroutine attackRoutine;
    private bool isAttacking = false;
    private bool hasLineOfSight = false;

    private void Start()
    {
        entityStats = GetComponent<EntityStats>();
        buffManager = GetComponent<BuffManager>();
        audioSource = GetComponent<AudioSource>();

        if (player == null) player = GameObject.FindWithTag("Player");

        GameObject spawnDirectorObj = GameObject.FindWithTag("SpawnDirector");
        if (spawnDirectorObj != null)
        {
            spawnDirector = spawnDirectorObj.GetComponent<SpawnDirector>();
        }

        // Setup laser sight
        laserSight = gameObject.AddComponent<LineRenderer>();
        laserSight.startWidth = 0.05f;
        laserSight.endWidth = 0.05f;
        laserSight.material = new Material(Shader.Find("Unlit/Color"))
        {
            color = laserSightColor,
            renderQueue = 3000 // Make it render on top
        };
        laserSight.positionCount = 2;
        laserSight.enabled = false;

        StartCoroutine(TargetingRoutine());
    }

    private void FixedUpdate()
    {
        if (entityStats != null && entityStats.isDead)
        {
            Die();
        }
    }

    private IEnumerator TargetingRoutine()
    {
        while (true)
        {
            if (player == null || entityStats.isDead)
            {
                laserSight.enabled = false;
                yield return null;
                continue;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            bool playerInRange = distanceToPlayer <= awarenessDistance;

            if (playerInRange)
            {
                // Always face the player, whether attacking or not
                Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

                if (!isAttacking)
                {
                    // Check line of sight only when not attacking
                    RaycastHit hit;
                    bool canSeePlayer = Physics.Raycast(
                        muzzlePoint.position,
                        directionToPlayer,
                        out hit,
                        distanceToPlayer,
                        obstructionLayers
                    );

                    hasLineOfSight = !canSeePlayer || hit.collider.gameObject == player;

                    // Update laser sight
                    laserSight.enabled = hasLineOfSight;
                    if (hasLineOfSight)
                    {
                        laserSight.SetPosition(0, muzzlePoint.position);
                        laserSight.SetPosition(1, player.transform.position);
                    }

                    // Start attack if we have line of sight
                    if (hasLineOfSight)
                    {
                        attackRoutine = StartCoroutine(AttackSequence());
                    }
                }
            }
            else
            {
                laserSight.enabled = false;
            }

            yield return null;
        }
    }

    private IEnumerator AttackSequence()
    {
        isAttacking = true;
        laserSight.enabled = false; // Disable laser when attacking

        // Aiming phase
        float aimTimer = 0f;
        while (aimTimer < aimTime && hasLineOfSight)
        {
            aimTimer += Time.deltaTime;
            yield return null;
        }

        // If we still have line of sight after aiming, start firing
        if (hasLineOfSight)
        {
            float attackTimer = 0f;
            float shotInterval = 1f / fireRate;
            float nextShotTime = 0f;

            while (attackTimer < attackDuration && hasLineOfSight)
            {
                attackTimer += Time.deltaTime;

                if (Time.time >= nextShotTime)
                {
                    FireAtPlayer();
                    nextShotTime = Time.time + shotInterval;
                }

                yield return null;
            }
        }

        isAttacking = false;
    }

    private void FireAtPlayer()
    {
        if (player == null) return;

        // Create visual effect
        if (bulletPrefab != null)
        {
            Vector3 directionToPlayer = (player.transform.position - muzzlePoint.position).normalized;
            GameObject bullet = Instantiate(
                bulletPrefab,
                muzzlePoint.position + directionToPlayer,
                Quaternion.LookRotation(directionToPlayer)
            );
            Destroy(bullet, 2f);
        }

        // Play sound
        if (shootingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootingSound);
        }

        // Create attack instance
        Attack turretAttack = new Attack(
            gameObject,
            Mathf.FloorToInt(damagePerShot),
            entityStats.getCritChance(),
            entityStats.getKnockback(),
            1f
        );

        // Apply damage
        player.GetComponent<EntityStats>().TakeHit(turretAttack);
        buffManager.TriggerOnHitEffects(player, turretAttack);
    }

    private void Die()
    {
        if (deathExplosion != null)
        {
            Instantiate(deathExplosion, transform.position, Quaternion.identity);
        }

        if (spawnDirector != null)
        {
            spawnDirector.RegisterKill(gameObject, 3);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        if (laserSight != null)
        {
            laserSight.enabled = false;
        }
    }
}