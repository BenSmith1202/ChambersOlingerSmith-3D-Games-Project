using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RingLaser : MonoBehaviour
{
    [Header("Ring Settings")]
    public float initialRadius = 1f;
    public float maxRadius = 10f;
    public float expansionSpeed = 2f;
    public float lifetime = 5f;

    [Header("Vertical Detection")]
    [Tooltip("Base Y position for damage detection")]
    public float baseYPosition = 0f;
    [Tooltip("Vertical range above baseY for damage (total height)")]
    public float verticalDetectionRange = 1f;
    [Tooltip("Vertical offset from baseY (can be positive or negative)")]
    public float verticalOffset = 0.5f;

    [Header("Visual Settings")]
    public float beamWidth = 0.2f;
    public Color activeColor = Color.red;

    [Header("Damage Settings")]
    public int damage = 10;
    public float damageInterval = 1f;
    public float knockback = 5f;

    private LineRenderer lineRenderer;
    private Material laserMaterial;
    private float currentRadius;
    private float timer;
    private bool canDamage = true;
    private EntityStats playerStats;
    private float effectiveYMin;
    private float effectiveYMax;

    private void Awake()
    {
        InitializeComponents();
        FindPlayer();
    }

    private void InitializeComponents()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Create material
        laserMaterial = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material = laserMaterial;
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;

        UpdateRingVisual();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<EntityStats>();
        }
    }

    private void OnEnable()
    {
        currentRadius = initialRadius;
        timer = 0f;
        canDamage = true;
        UpdateVerticalDetectionRange();
        UpdateRingVisual();
    }

    private void Update()
    {
        if (timer >= lifetime)
        {
            gameObject.SetActive(false);
            return;
        }

        // Expand the ring
        currentRadius += expansionSpeed * Time.deltaTime;
        timer += Time.deltaTime;

        UpdateRingVisual();
        CheckPlayerCollision();
    }

    private void UpdateVerticalDetectionRange()
    {
        effectiveYMin = baseYPosition + verticalOffset - (verticalDetectionRange * 0.5f);
        effectiveYMax = baseYPosition + verticalOffset + (verticalDetectionRange * 0.5f);
    }

    private void UpdateRingVisual()
    {
        // Update material color
        laserMaterial.color = activeColor;

        // Create circle points
        int segments = Mathf.Max(20, Mathf.FloorToInt(currentRadius * 4f));
        lineRenderer.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * (2f * Mathf.PI / segments);
            Vector3 point = new Vector3(
                Mathf.Cos(angle) * currentRadius,
                0,
                Mathf.Sin(angle) * currentRadius
            );
            lineRenderer.SetPosition(i, point);
        }
    }

    private void CheckPlayerCollision()
    {
        if (!canDamage || playerStats == null) return;

        Vector3 playerPos = playerStats.transform.position;
        float playerDistance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(playerPos.x, 0, playerPos.z)
        );

        // Check if player is within damage range horizontally AND vertically
        bool inHorizontalRange = playerDistance > currentRadius - beamWidth &&
                               playerDistance < currentRadius + beamWidth;
        bool inVerticalRange = playerPos.y >= effectiveYMin &&
                             playerPos.y <= effectiveYMax;

        if (inHorizontalRange && inVerticalRange)
        {
            Attack ringAttack = new Attack(
                gameObject,
                damage,
                0f,
                knockback,
                1f
            );
            playerStats.TakeHit(ringAttack);
            StartCoroutine(DamageCooldown());
        }
    }

    private IEnumerator DamageCooldown()
    {
        canDamage = false;
        yield return new WaitForSeconds(damageInterval);
        canDamage = true;
    }

    private void OnDisable()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (laserMaterial != null)
        {
            Destroy(laserMaterial);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw vertical detection range
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Vector3 center = transform.position + Vector3.up * (baseYPosition + verticalOffset);
        Vector3 size = new Vector3(maxRadius * 2, verticalDetectionRange, maxRadius * 2);
        Gizmos.DrawCube(center, size);
    }
}