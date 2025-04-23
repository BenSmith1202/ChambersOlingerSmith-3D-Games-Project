using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A grounded enemy that fires spread patterns of bullets at the player
/// </summary>
public class ShotgunPolice : MonoBehaviour
{
    [Header("Combat Settings")]
    [Tooltip("Time between attacks")]
    public float attackCooldown = 3f;
    [Tooltip("Delay after aiming before firing")]
    public float attackWindup = 0.5f;
    [Tooltip("Range at which enemy will start attacking")]
    public float attackRange = 10f;
    [Tooltip("Bullet prefab to spawn")]
    public GameObject bulletPrefab;
    [Tooltip("Position where bullets spawn")]
    public Transform bulletSpawnPoint;

    [Header("Shotgun Spread Settings")]
    [Tooltip("Minimum number of bullets per shot")]
    public int minBullets = 3;
    [Tooltip("Maximum number of bullets per shot")]
    public int maxBullets = 6;
    [Tooltip("Maximum position offset for each bullet")]
    public float positionSpread = 0.2f;
    [Tooltip("Maximum angle offset for each bullet (degrees)")]
    public float angleSpread = 10f;

    [Header("Effects")]
    [Tooltip("Sound played when shooting")]
    public AudioClip shootSound;
    [Tooltip("Particle effect played when shooting")]
    public ParticleSystem shootEffect;

    private GameObject player;
    private float attackTimer;
    private AudioSource audioSource;
    private EntityStats stats;

    /// <summary>
    /// Initializes references
    /// </summary>
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        audioSource = GetComponent<AudioSource>();
        stats = GetComponent<EntityStats>();
        attackTimer = attackCooldown;
    }

    /// <summary>
    /// Handles death check and timers
    /// </summary>
    private void Update()
    {
        if (stats != null && stats.isDead)
        {
            Die();
            return;
        }

        attackTimer -= Time.deltaTime;

        // Always face player
        if (player != null)
        {
            Vector3 lookDirection = player.transform.position - transform.position;
            lookDirection.y = 0; // Only rotate on Y axis
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        // Attack logic
        if (player != null &&
            Vector3.Distance(transform.position, player.transform.position) <= attackRange &&
            attackTimer <= 0f)
        {
            StartCoroutine(AttackRoutine());
            attackTimer = attackCooldown;
        }
    }

    /// <summary>
    /// Attack sequence: windup -> fire spread pattern
    /// </summary>
    private IEnumerator AttackRoutine()
    {
        // Windup period
        yield return new WaitForSeconds(attackWindup);

        // Play effects if they exist
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        if (shootEffect != null)
        {
            shootEffect.Play();
        }

        // Fire bullet spread if prefab exists
        if (bulletPrefab != null && bulletSpawnPoint != null)
        {
            FireBulletSpread();
        }
    }

    /// <summary>
    /// Creates a randomized spread pattern of bullets
    /// </summary>
    private void FireBulletSpread()
    {
        int bulletCount = Random.Range(minBullets, maxBullets + 1);
        Vector3 baseDirection = (player.transform.position - bulletSpawnPoint.position).normalized;

        for (int i = 0; i < bulletCount; i++)
        {
            // Calculate random position offset
            Vector3 positionOffset = new Vector3(
                Random.Range(-positionSpread, positionSpread),
                Random.Range(-positionSpread, positionSpread),
                Random.Range(-positionSpread, positionSpread)
            );

            // Calculate random angle offset
            Vector3 angleOffset = new Vector3(
                Random.Range(-angleSpread, angleSpread),
                Random.Range(-angleSpread, angleSpread),
                Random.Range(-angleSpread, angleSpread)
            );

            // Create bullet with offsets
            Vector3 spawnPosition = bulletSpawnPoint.position + positionOffset;
            Quaternion spawnRotation = Quaternion.LookRotation(baseDirection) * Quaternion.Euler(angleOffset);

            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, spawnRotation);
            ShotgunBullet bulletScript = bullet.GetComponent<ShotgunBullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(stats);
            }
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
            spawnDirector.RegisterKill(gameObject, 2);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}