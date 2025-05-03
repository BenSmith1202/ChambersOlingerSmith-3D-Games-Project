using UnityEngine;
using System.Collections;


public class BolgBarbScript : MonoBehaviour
{
    [Header("References")]
    public GameObject owner; // Assign who caused this projectile

    [Header("Stats")]
    public float speed = 20f; // How fast the projectile moves
    public int damage = 10; // Base damage of the projectile

    [Header("Collision")]
    [Tooltip("Layers this projectile can collide with.")]
    public LayerMask hittableLayers; // IMPORTANT: Configure this in the Inspector
    [Tooltip("How far ahead to check for collision each frame (adjust based on speed).")]
    public float collisionCheckDistance = 0.1f; // A small buffer beyond frame movement

    [Header("Lifetime")]
    [Tooltip("How long the projectile lives before being destroyed automatically.")]
    public float maxLifetime = 5.0f;

    private EntityStats ownerStats; // Cached owner stats
    public GameObject impactEffectPrefab; // Optional prefab for impact effects

    void Start()
    {
        // Cache owner stats for efficiency and validity check
        if (owner != null)
        {
            ownerStats = owner.GetComponent<EntityStats>();
            if (ownerStats == null)
            {
                Debug.LogError($"Projectile owner '{owner.name}' is missing EntityStats component!", owner);
                // Optionally destroy projectile if owner setup is invalid
                // Destroy(gameObject);
            }
        }
        else
        {
            
        }

        // Schedule automatic destruction after maxLifetime
        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        // --- Movement ---
        float distanceToMove = speed * Time.deltaTime;
        transform.Translate(transform.forward * distanceToMove, Space.World);

        // --- Collision Check ---
        RaycastHit hitData;
        // Cast a ray forward for the distance it will move this frame + a small buffer
        // Use the configured LayerMask
        bool hitDetected = Physics.Raycast(transform.position - transform.forward * distanceToMove, // Start slightly behind current pos
                                          transform.forward,
                                          out hitData,
                                          distanceToMove + collisionCheckDistance, // Check path covered + buffer
                                          hittableLayers);

        // Visualize the raycast path for debugging
        Debug.DrawRay(transform.position - transform.forward * distanceToMove, transform.forward * (distanceToMove + collisionCheckDistance), Color.red);


        if (hitDetected)
        {
            // Prevent hitting the owner immediately after spawn (LayerMask should ideally handle this too)
            if (hitData.collider.gameObject == owner)
            {
                return; // Ignore hit if it's the owner
            }

            // --- Process Hit ---
            HandleHit(hitData);

            // Destroy the projectile since it hit something valid
            Destroy(gameObject);
        }
        // --- No Hit: Projectile continues moving (handled by Translate above) ---
    }

    void HandleHit(RaycastHit hitData)
    {
        //Debug.Log($"Projectile hit: {hitData.collider.name} at {hitData.point}");

        // Try to get stats from the hit object
        EntityStats targetStats = hitData.collider.GetComponent<EntityStats>();

        // Apply damage/effects only if the target has stats AND the owner's stats are valid
        if (targetStats != null && ownerStats != null && targetStats != ownerStats)
        {
            // Calculate damage/crit locally
            
            float calculatedCritChance = ownerStats.getCritChance(); // Ensure method names match EntityStats

            // Create the attack data structure
            // Consider if Attack needs owner GameObject or just owner stats/ID
            Attack barbHit = new Attack(owner, damage, calculatedCritChance, 0, 0.1f); // Adjust params as needed

            // Trigger OnHit effects (Check if BuffManager exists)
            BuffManager ownerBuffManager = owner.GetComponent<BuffManager>();
            if (ownerBuffManager != null)
            {
                ownerBuffManager.TriggerOnHitEffects(hitData.collider.gameObject, barbHit);
            }
            else
            {
                Debug.LogWarning($"Owner '{owner.name}' is missing BuffManager component.", owner);
            }

            // Apply the hit damage/effects to the target
            targetStats.TakeHit(barbHit);

            //Debug.Log($"Applied hit to {hitData.collider.name} with {calculatedDamage} base damage.");
        }
        else if (targetStats == null)
        {
            // Optional: Log if we hit something without stats (like a wall)
            //Debug.Log($"Projectile hit {hitData.collider.name}, which has no EntityStats.");
        }

        // Optional: Instantiate an impact particle effect at the hit point
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, hitData.point, Quaternion.LookRotation(hitData.normal));
        }

        // Projectile is destroyed in Update after HandleHit returns
    }
}