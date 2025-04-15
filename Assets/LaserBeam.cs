using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    [Header("Visual Settings")]
    public float maxLength = 50f;
    public float beamWidth = 0.2f;
    public Color activeColor = Color.red;
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Semi-transparent gray

    [Header("Damage Settings")]
    public int damage = 10;
    public float damageInterval = 1f;
    public float knockback = 5f;

    [Header("State")]
    public bool isActive = true;

    private LineRenderer lineRenderer;
    private bool canDamage = true;
    private EntityStats playerStats;
    private Material laserMaterial;

    private void Awake()
    {
        // Proper LineRenderer setup
        lineRenderer = GetComponent<LineRenderer>();

        // Create a simple material if none exists
        if (lineRenderer.material == null)
        {
            laserMaterial = new Material(Shader.Find("Unlit/Color"));
            lineRenderer.material = laserMaterial;
        }
        else
        {
            laserMaterial = lineRenderer.material;
        }

        ConfigureLineRenderer();
        FindPlayer();
    }

    private void ConfigureLineRenderer()
    {
        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        UpdateLaserAppearance();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<EntityStats>();
        }
    }

    private void Update()
    {
        if (lineRenderer == null) return;

        if (isActive)
        {
            UpdateLaserPosition();
            CheckForPlayerHit();
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    private void UpdateLaserPosition()
    {
        lineRenderer.SetPosition(0, transform.position);

        Vector3 endPosition = transform.position + (transform.forward * maxLength);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxLength))
        {
            endPosition = hit.point;
        }

        lineRenderer.SetPosition(1, endPosition);
    }

    private void CheckForPlayerHit()
    {
        if (!canDamage || playerStats == null) return;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxLength))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Attack laserAttack = new Attack(
                    gameObject,
                    damage,
                    0f,
                    knockback,
                    1f
                );
                playerStats.TakeHit(laserAttack);
                StartCoroutine(DamageCooldown());
            }
        }
    }

    private IEnumerator DamageCooldown()
    {
        canDamage = false;
        yield return new WaitForSeconds(damageInterval);
        canDamage = true;
    }

    private void UpdateLaserAppearance()
    {
        if (laserMaterial != null)
        {
            laserMaterial.color = isActive ? activeColor : inactiveColor;
        }
    }

    public void ToggleLaser(bool active)
    {
        isActive = active;
        UpdateLaserAppearance();

        // Ensure line renderer is properly enabled/disabled
        if (lineRenderer != null)
        {
            lineRenderer.enabled = active;
        }
    }

    private void OnDisable()
    {
        // Proper cleanup when disabled
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    private void OnDestroy()
    {
        // Clean up material if we created it
        if (laserMaterial != null && lineRenderer != null && lineRenderer.material == laserMaterial)
        {
            Destroy(laserMaterial);
        }
    }
}