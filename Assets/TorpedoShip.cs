using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Alien ship that moves randomly and fires torpedoes at the player
/// </summary>
public class TorpedoShip : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed when moving randomly")]
    public float moveSpeed = 3f;
    [Tooltip("Duration of random movement in seconds")]
    public float moveDuration = 2f;
    [Tooltip("Minimum time between actions in seconds")]
    public float minActionDelay = 1f;
    [Tooltip("Maximum time between actions in seconds")]
    public float maxActionDelay = 3f;
    [Tooltip("Speed of hovering up/down movement")]
    public float hoverSpeed = 0.5f;
    [Tooltip("Amount of hovering up/down movement")]
    public float hoverAmount = 0.3f;

    [Header("Combat Settings")]
    [Tooltip("Range at which ship will start attacking")]
    public float attackRange = 15f;
    [Tooltip("Prefab for normal torpedo")]
    public GameObject torpedoPrefab;
    [Tooltip("Prefab for fire torpedo")]
    public GameObject fireTorpedoPrefab;
    [Tooltip("Position where torpedoes spawn")]
    public Transform torpedoSpawnPoint;
    [Tooltip("Angle spread between normal torpedoes in degrees")]
    public float torpedoSpreadAngle = 15f;

    private GameObject player;
    private EntityStats stats;
    private Vector3 originalPosition;
    private float hoverOffset;
    private bool isPerformingAction;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        stats = GetComponent<EntityStats>();
        originalPosition = transform.position;
        StartCoroutine(ActionRoutine());
        StartCoroutine(HoverRoutine());
    }

    private void Update()
    {
        if (stats != null && stats.isDead)
        {
            Die();
            return;
        }

        // Always face player on X/Z plane
        if (player != null)
        {
            Vector3 lookDirection = player.transform.position - transform.position;
            lookDirection.y = 0; // Only rotate on Y axis
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    /// <summary>
    /// Handles continuous up/down hovering movement
    /// </summary>
    private IEnumerator HoverRoutine()
    {
        while (true)
        {
            // Calculate hover offset using sine wave for smooth movement
            hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;

            // Apply hover offset while maintaining X/Z position
            transform.position = new Vector3(
                transform.position.x,
                originalPosition.y + hoverOffset,
                transform.position.z
            );

            yield return null;
        }
    }

    /// <summary>
    /// Main action loop that randomly selects behaviors
    /// </summary>
    private IEnumerator ActionRoutine()
    {
        while (true)
        {
            if (!isPerformingAction && player != null &&
                Vector3.Distance(transform.position, player.transform.position) <= attackRange)
            {
                // Randomly select an action
                int action = Random.Range(0, 3);

                switch (action)
                {
                    case 0:
                        StartCoroutine(MoveAction());
                        break;
                    case 1:
                        StartCoroutine(ShootTorpedoesAction());
                        break;
                    case 2:
                        StartCoroutine(ShootFireTorpedoAction());
                        break;
                }
            }

            // Wait random delay between actions
            yield return new WaitForSeconds(Random.Range(minActionDelay, maxActionDelay));
        }
    }

    /// <summary>
    /// Action: Move in a random X/Z direction for set duration
    /// </summary>
    private IEnumerator MoveAction()
    {
        isPerformingAction = true;

        // Pick random X/Z direction
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            0,
            Random.Range(-1f, 1f)
        ).normalized;

        float timer = 0f;

        while (timer < moveDuration)
        {
            // Move in random direction while maintaining hover
            transform.position += randomDirection * moveSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        isPerformingAction = false;
    }

    /// <summary>
    /// Action: Fire 3 torpedoes in spread pattern at player
    /// </summary>
    private IEnumerator ShootTorpedoesAction()
    {
        isPerformingAction = true;

        // Fire 3 torpedoes with spread
        for (int i = 0; i < 3; i++)
        {
            if (torpedoPrefab != null && torpedoSpawnPoint != null && player != null)
            {
                // Calculate direction with spread angle
                Vector3 baseDirection = (player.transform.position - torpedoSpawnPoint.position).normalized;
                float angle = (i - 1) * torpedoSpreadAngle; // -15, 0, +15 degrees
                Vector3 spreadDirection = Quaternion.Euler(0, angle, 0) * baseDirection;

                // Create torpedo
                GameObject torpedo = Instantiate(
                    torpedoPrefab,
                    torpedoSpawnPoint.position,
                    Quaternion.LookRotation(spreadDirection)
                );
            }
        }

        isPerformingAction = false;
        yield return null;
    }

    /// <summary>
    /// Action: Fire 1 powerful fire torpedo at player
    /// </summary>
    private IEnumerator ShootFireTorpedoAction()
    {
        isPerformingAction = true;

        if (fireTorpedoPrefab != null && torpedoSpawnPoint != null && player != null)
        {
            Vector3 fireDirection = (player.transform.position - torpedoSpawnPoint.position).normalized;

            Instantiate(
                fireTorpedoPrefab,
                torpedoSpawnPoint.position,
                Quaternion.LookRotation(fireDirection)
            );
        }

        isPerformingAction = false;
        yield return null;
    }

    /// <summary>
    /// Handles enemy death
    /// </summary>
    private void Die()
    {
        SpawnDirector spawnDirector = GameObject.FindWithTag("SpawnDirector")?.GetComponent<SpawnDirector>();
        if (spawnDirector != null)
        {
            spawnDirector.RegisterKill(gameObject, 2); // Medium value enemy
        }
        else
        {
            Destroy(gameObject);
        }
    }
}