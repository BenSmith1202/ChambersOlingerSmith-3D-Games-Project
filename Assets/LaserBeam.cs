using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Versatile laser beam system that can damage players with configurable timing
/// </summary>
public class LaserBeam : MonoBehaviour
{
    [Header("Visual Settings")]
    [Tooltip("Maximum length of the laser beam")]
    public float maxLength = 50f;
    [Tooltip("Width of the laser beam")]
    public float beamWidth = 0.2f;
    [Tooltip("Color when active")]
    public Color activeColor = Color.red;
    [Tooltip("Color when inactive")]
    public Color inactiveColor = Color.gray;

    [Header("Damage Settings")]
    [Tooltip("Damage dealt per hit")]
    public int damage = 10;
    [Tooltip("Minimum time between damage ticks (seconds)")]
    public float damageInterval = 1f;
    [Tooltip("Knockback force applied")]
    public float knockback = 5f;

    [Header("State")]
    [Tooltip("Whether the laser is currently active")]
    public bool isActive = true;

    // Components
    private LineRenderer lineRenderer;
    private bool canDamage = true;
    private EntityStats playerStats;

    /// <summary>
    /// Initializes the laser beam components
    /// </summary>
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        ConfigureLineRenderer();
        FindPlayer();
    }

    /// <summary>
    /// Sets up the LineRenderer with our visual settings
    /// </summary>
    private void ConfigureLineRenderer()
    {
        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        UpdateLaserColor();
    }

    /// <summary>
    /// Finds the player's EntityStats component
    /// </summary>
    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<EntityStats>();
        }
    }

    /// <summary>
    /// Updates the laser beam each frame
    /// </summary>
    private void Update()
    {
        UpdateLaserPosition();
        if (isActive)
        {
            CheckForPlayerHit();
        }
    }

    /// <summary>
    /// Updates the laser's start and end positions
    /// </summary>
    private void UpdateLaserPosition()
    {
        // Set start position at the object's position
        lineRenderer.SetPosition(0, transform.position);

        // Calculate end position
        Vector3 endPosition = transform.position + transform.forward * maxLength;

        // Raycast to find obstacles
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxLength))
        {
            endPosition = hit.point;
        }

        lineRenderer.SetPosition(1, endPosition);
    }

    /// <summary>
    /// Checks if the laser is hitting the player and applies damage
    /// </summary>
    private void CheckForPlayerHit()
    {
        if (!isActive || !canDamage || playerStats == null) return;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxLength))
        {
            if (hit.collider.CompareTag("Player"))
            {
                ApplyDamageToPlayer();
            }
        }
    }

    /// <summary>
    /// Applies damage to the player and starts cooldown
    /// </summary>
    private void ApplyDamageToPlayer()
    {
        // Create attack object
        Attack laserAttack = new Attack(
            gameObject, // owner
            damage, // damage
            0f, // crit chance (lasers don't crit)
            knockback, // knockback
            1f // proc coefficient
        );

        // Apply damage
        playerStats.TakeHit(laserAttack);

        // Start damage cooldown
        StartCoroutine(DamageCooldown());
    }

    /// <summary>
    /// Temporarily disables damage after hitting player
    /// </summary>
    private IEnumerator DamageCooldown()
    {
        canDamage = false;
        yield return new WaitForSeconds(damageInterval);
        canDamage = true;
    }

    /// <summary>
    /// Toggles the laser beam on/off
    /// </summary>
    public void ToggleLaser(bool active)
    {
        isActive = active;
        UpdateLaserColor();
    }

    /// <summary>
    /// Updates the laser color based on active state
    /// </summary>
    private void UpdateLaserColor()
    {
        lineRenderer.startColor = isActive ? activeColor : inactiveColor;
        lineRenderer.endColor = isActive ? activeColor : inactiveColor;
    }
}