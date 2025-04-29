using UnityEngine;
using System.Collections.Generic; // Optional, if using NonAlloc versions later

/// <summary>
/// Creates a gravitational pull effect towards this object's center,
/// affecting Rigidbodies within a specified radius.
/// </summary>
public class BlackholeScript: MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The radius within which objects will be affected.")]
    [Min(0.1f)] // Ensure radius is positive
    public float radius = 15.0f;

    [Tooltip("The base strength of the pull towards the center.")]
    public float pullForce = 100.0f;

    [Tooltip("Which physics layers should be affected by the black hole?")]
    public LayerMask affectedLayers = -1; // -1 means 'Everything' by default

    [Header("Options")]
    [Tooltip("Apply force that increases significantly as objects get closer (Inverse Square Law-like)? If false, uses a more constant force (still scales slightly with distance).")]
    public bool useInverseSquareLikeFalloff = true;

    [Tooltip("Minimum distance used for force calculation, especially when Inverse Square is true, to prevent extremely high forces very close to the center.")]
    [Min(0.01f)]
    public float minDistanceClamp = 0.5f;

    WorldHazardScript hazardScript;

    // --- Private Variables ---
    private List<Rigidbody> rigidbodiesInRange = new List<Rigidbody>(); // To potentially track objects if needed, though OverlapSphere is often sufficient per-frame.

    //start
    void Start()
    {
        hazardScript = GetComponent<WorldHazardScript>();
    }

    // Use FixedUpdate for physics calculations for consistency
    void FixedUpdate()
    {
        FindAndPullObjects();
    }

    void FindAndPullObjects()
    {
        // Find all colliders within the specified radius and layer mask
        // Physics.OverlapSphere is efficient for this kind of check.
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, affectedLayers, QueryTriggerInteraction.Ignore); // Ignore triggers

        foreach (Collider col in colliders)
        {
            EntityStats stats = col.GetComponent<EntityStats>();

            if (hazardScript != null && stats != null && !hazardScript.ShouldAffectTarget(stats))
            {
                continue; // Skip if the entity is not affected by this hazard
            }
            // Attempt to get a Rigidbody from the detected collider.
            // Use attachedRigidbody; it correctly finds the Rigidbody even if the collider is on a child object.
            Rigidbody rb = col.attachedRigidbody;

            // Check if it's a valid Rigidbody we should affect:
            // 1. It exists (rb != null)
            // 2. It's not kinematic (kinematic bodies aren't controlled by physics forces)
            // 3. It's not the Rigidbody attached to this black hole object itself (rb.gameObject != this.gameObject)
            if (rb != null && !rb.isKinematic && rb.gameObject != this.gameObject)
            {
                // Calculate the direction vector from the object towards the black hole's center
                Vector3 direction = transform.position - rb.position;

                // Calculate the distance to the center
                float distance = direction.magnitude;

                // Prevent issues if the object is exactly at the center (or extremely close)
                if (distance < 0.01f)
                {
                    continue; // Skip force application for this object
                }

                // Calculate the force magnitude
                float forceMagnitude;

                if (useInverseSquareLikeFalloff)
                {
                    // Force increases dramatically as distance decreases (like gravity)
                    // F is proportional to 1 / distance^2
                    // We clamp the distance using minDistanceClamp to prevent infinite forces at the center.
                    float effectiveDistance = Mathf.Max(distance, minDistanceClamp);
                    forceMagnitude = pullForce / (effectiveDistance * effectiveDistance);
                }
                else
                {
                    // Simpler approach: Force is somewhat constant, maybe slightly stronger when closer.
                    // Example: linear falloff (or just use the base pullForce)
                    // forceMagnitude = pullForce * (1.0f - Mathf.Clamp01(distance / radius)); // Stronger closer, zero at edge
                    forceMagnitude = pullForce; // Simplest: Constant force regardless of distance (within radius)
                }

                // Apply the force:
                // - Normalize the direction vector to get only the direction (length 1).
                // - Multiply by the calculated force magnitude.
                // - Use ForceMode.Force: Applies force over time, taking mass into account ( F = ma ). Good for continuous forces like gravity/pull.
                rb.AddForce(direction.normalized * forceMagnitude, ForceMode.Force);
            }
        }
    }

    // Draw a helpful yellow wire sphere gizmo in the Scene view
    // when the black hole object is selected, showing the radius.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}