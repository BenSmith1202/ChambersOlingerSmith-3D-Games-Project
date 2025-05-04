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
    public int maxBounces = 5; // Maximum number of reflections

    [Header("Damage Settings")]
    public int damage = 10;
    public float damageInterval = 1f;
    public float knockback = 5f;

    [Header("State")]
    public bool isActive = true;

    [Header("Effects")]
    public GameObject laserHitEffect; // Effect shown at the final hit point

    private LineRenderer lineRenderer;
    private bool canDamage = true;
    private EntityStats playerStats;
    private Material laserMaterial;
    private List<Vector3> laserPoints = new List<Vector3>(); // Store all points for the line renderer

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Setup Material (same as before, consider assigning one in inspector)
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
        FindPlayer(); // Okay for Awake, consider alternatives for larger projects
    }

    private void ConfigureLineRenderer()
    {
        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;
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
        else
        {
            Debug.LogWarning("LaserBeam: Player object not found (missing tag 'Player'?).");
        }
    }

    private void Update()
    {
        if (lineRenderer == null) return;

        if (isActive)
        {
            lineRenderer.enabled = true;
            UpdateLaserPath(); // Renamed for clarity
            // Effect handling moved inside UpdateLaserPath
        }
        else
        {
            lineRenderer.enabled = false;
            if (laserHitEffect != null)
            {
                laserHitEffect.SetActive(false);
            }
        }
    }

    private void UpdateLaserPath()
    {
        laserPoints.Clear();
        laserPoints.Add(transform.position); // Start point

        Vector3 currentPosition = transform.position;
        Vector3 currentDirection = transform.forward;
        float remainingLength = maxLength;
        int bouncesLeft = maxBounces;
        RaycastHit hit = default; // Store the last hit info

        for (int i = 0; i <= maxBounces; i++) // Loop for initial segment + bounces
        {
            bool hitSomething = Physics.Raycast(currentPosition, currentDirection, out hit, remainingLength);

            if (hitSomething)
            {
                laserPoints.Add(hit.point);
                remainingLength -= Vector3.Distance(currentPosition, hit.point);
                currentPosition = hit.point;

                // Check for Player Hit *on this segment* before reflecting
                if (CheckForPlayerHit(hit))
                {
                    bouncesLeft = 0; // Stop bouncing if we hit the player
                }

                // Check if we should bounce
                if (bouncesLeft > 0 && remainingLength > 0.01f) // Check remainingLength to avoid tiny segments
                {
                    // Apply a slight offset away from the surface to avoid immediate self-collision on next raycast
                    currentPosition += hit.normal * 0.01f;
                    currentDirection = Vector3.Reflect(currentDirection, hit.normal);
                    bouncesLeft--;
                }
                else // Hit something, but no more bounces or length left
                {
                    // This is the final hit point
                    ActivateHitEffect(hit.point);
                    break; // Exit loop, laser terminates here
                }
            }
            else // Raycast didn't hit anything
            {
                laserPoints.Add(currentPosition + currentDirection * remainingLength);
                // No final hit effect if it goes into infinity
                DeactivateHitEffect();
                break; // Exit loop, laser goes to max length
            }
            // Safety break if remaining length is negligible
            if (remainingLength <= 0.01f)
            {
                ActivateHitEffect(currentPosition); // End point is the last hit point
                break;
            }
        }

        // Update the Line Renderer
        lineRenderer.positionCount = laserPoints.Count;
        lineRenderer.SetPositions(laserPoints.ToArray());
    }


    private bool CheckForPlayerHit(RaycastHit hit) // Now takes hit info as parameter
    {
        if (playerStats == null) return false;

        // Check if the specific collider we hit is the player
        if (hit.collider.CompareTag("Player"))
        {
            if (canDamage != false)
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
            
            return true;
        }
        return false;
    }

    private void ActivateHitEffect(Vector3 position)
    {
        if (laserHitEffect != null)
        {
            laserHitEffect.transform.position = position;
            // Optional: Orient effect to surface normal: laserHitEffect.transform.rotation = Quaternion.LookRotation(hit.normal);
            laserHitEffect.SetActive(true);
        }
    }

    private void DeactivateHitEffect()
    {
        if (laserHitEffect != null)
        {
            laserHitEffect.SetActive(false);
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
        // Apply width changes if needed
        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;
    }

    public void ToggleLaser(bool active)
    {
        isActive = active;
        UpdateLaserAppearance(); // Update color immediately

        // Enabling/disabling is handled in Update now based on isActive flag
        if (!active)
        {
            // Explicitly disable components when toggled off
            lineRenderer.enabled = false;
            DeactivateHitEffect();
        }
    }

    private void OnDisable()
    {
        // Ensure everything is off when the component/GameObject is disabled
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
        DeactivateHitEffect();
        // Stop coroutines tied to this script instance
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        // Clean up material if we created it
        if (laserMaterial != null && lineRenderer != null && lineRenderer.material == laserMaterial)
        {
            // Check if the material instance we created is still the one being used
            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(laserMaterial); // Use DestroyImmediate if cleaning up in editor mode outside play
            }
            else
            {
                Destroy(laserMaterial); // Use Destroy in play mode or builds
            }
        }
    }
}