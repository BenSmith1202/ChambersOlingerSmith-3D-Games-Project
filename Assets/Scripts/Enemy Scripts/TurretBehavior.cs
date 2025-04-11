using System.Collections;
using UnityEngine;

public class TurretBehavior : MonoBehaviour
{
    [Header("Targeting Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private float awarenessDistance = 30f;
    [SerializeField] private LayerMask obstructionLayers;
    [SerializeField] private Color aimingColor = Color.red;
    [SerializeField] private Color lockedOnColor = Color.green;

    [Header("Attack Settings")]
    [SerializeField] private float aimTime = 1.5f;
    [SerializeField] private float damagePerShot = 5f;
    [SerializeField] private float fireRate = 10f;
    [SerializeField] private float attackDuration = 3f;

    [Header("Visuals")]
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private Transform muzzlePoint;

    private EntityStats stats;
    private BuffManager buffManager;
    private Coroutine attackRoutine;
    private bool isAttacking = false;
    private bool hasLineOfSight = false;


    public GameObject deathExplosion;
    private SpawnDirector spawnDirector;

    private void Start()
    {
        spawnDirector = GameObject.FindWithTag("SpawnDirector").GetComponent<SpawnDirector>();

        stats = GetComponent<EntityStats>();
        buffManager = GetComponent<BuffManager>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        InitializeAimLine();
        StartCoroutine(TargetingRoutine());
    }



    private void Update()
    {
        if (stats.isDead) //TODO: Replace with messages or something
        {
            Die();
        }
    }



    void Die()
    {
        Instantiate(deathExplosion, transform.position, Quaternion.identity);

        if (spawnDirector != null)
        {
            spawnDirector.RegisterKill(gameObject, 3);

        }
        else
        {
            Destroy(gameObject);
        }
    }



    private void InitializeAimLine()
    {
        if (aimLine == null)
        {
            aimLine = gameObject.AddComponent<LineRenderer>();
            aimLine.startWidth = 0.05f;
            aimLine.endWidth = 0.05f;
            aimLine.material = new Material(Shader.Find("Unlit/Color")) { color = aimingColor };
            aimLine.positionCount = 2;
        }
    }

    private IEnumerator TargetingRoutine()
    {
        while (true)
        {
            if (player == null || stats.isDead)
            {
                aimLine.enabled = false;
                yield return null;
                continue;
            }

            // Calculate direction to player
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Face the player (y-axis only)
            transform.rotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));

            // Perform raycast check
            RaycastHit hit;
            bool canSeePlayer = Physics.Raycast(
                muzzlePoint.position,
                directionToPlayer,
                out hit,
                distanceToPlayer,
                obstructionLayers
            );

            // Update line of sight status
            hasLineOfSight = !canSeePlayer || hit.collider.transform == player;

            // Update aim line visuals
            UpdateAimLineVisuals(directionToPlayer, distanceToPlayer);

            // Handle attack state
            if (distanceToPlayer <= awarenessDistance && hasLineOfSight)
            {
                if (!isAttacking)
                {
                    attackRoutine = StartCoroutine(AttackSequence());
                }
            }
            else
            {
                if (isAttacking)
                {
                    StopCoroutine(attackRoutine);
                    isAttacking = false;
                }
            }

            yield return null;
        }
    }

    private void UpdateAimLineVisuals(Vector3 direction, float distance)
    {
        if (aimLine == null) return;

        aimLine.enabled = true;
        aimLine.SetPosition(0, muzzlePoint.position);

        if (hasLineOfSight)
        {
            aimLine.material.color = lockedOnColor;
            aimLine.SetPosition(1, muzzlePoint.position + direction * distance);
        }
        else
        {
            aimLine.material.color = aimingColor;
            RaycastHit hit;
            if (Physics.Raycast(muzzlePoint.position, direction, out hit, distance, obstructionLayers))
            {
                aimLine.SetPosition(1, hit.point);
            }
            else
            {
                aimLine.SetPosition(1, muzzlePoint.position + direction * distance);
            }
        }
    }

    private IEnumerator AttackSequence()
    {
        isAttacking = true;

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

        // Create attack instance
        Attack turretAttack = new Attack(
            gameObject,
            Mathf.FloorToInt(damagePerShot),
            stats.getCritChance(),
            stats.getKnockback(),
            1f
        );

        // Apply damage
        player.GetComponent<EntityStats>().TakeHit(turretAttack);
        buffManager.TriggerOnHitEffects(player.gameObject, turretAttack);

        // Visual feedback
        Debug.DrawLine(muzzlePoint.position, player.position, Color.yellow, 0.1f);
    }

    private void OnDisable()
    {
        if (aimLine != null)
        {
            aimLine.enabled = false;
        }
    }
}