using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneralExplosionScript : MonoBehaviour
{
    [Header("Core Settings")]
    [Tooltip("Who created this explosion? Determines targeting based on friendlyFire.")]
    public GameObject owner;
    [Tooltip("If true, affects all entities. If false, follows owner-based rules.")]
    public bool friendlyFire = false;
    [Tooltip("Base damage before owner stats/falloff.")]
    public int damage = 50;
    [Tooltip("Base knockback force.")]
    public float knockback = 10f;
    [Tooltip("The chance (0-1) for this explosion to trigger owner's OnHit effects.")]
    [Range(0f, 1f)]
    public float procCoefficient = 0.1f;
    [Tooltip("Optional status effect prefab to apply to affected entities.")]
    public GameObject statusEffectPrefab; // Renamed from onHitDebuff for clarity

    [Header("Timing & Duration")]
    [Tooltip("Delay in seconds after instantiation before the explosion effects trigger.")]
    [Min(0f)]
    public float activationDelay = 0.1f;
    [Tooltip("Total lifetime of the explosion GameObject in seconds.")]
    [Min(0f)]
    public float duration = 2f;
    public float particleLifetime = 8f; // Lifetime of the explosion particles

    [Header("Area of Effect")]
    [Tooltip("Radius within which damage is applied.")]
    public float damageRadius = 5f;
    [Tooltip("Radius within which knockback is applied.")]
    public float knockbackRadius = 7f;
    [Tooltip("How much knockback force is directed upwards (0 = horizontal, 1 = vertical).")]
    [Range(0f, 1f)]
    public float knockbackVerticalBias = 0.2f;

    [Header("Damage Falloff")]
    [Tooltip("Radius within which full damage is dealt.")]
    public float fullDamageRadius = 1f;
    [Tooltip("Minimum damage percentage dealt at the edge of the damageRadius.")]
    [Range(0f, 1f)]
    public float minFalloffPercent = 0.2f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField][Range(0f, 1f)] private float soundVolume = 1f;

    // Cached references
    private BuffManager _ownerBuffManager;
    private EntityStats _ownerStats;
    private float _critChance = 0f;
    private int _finalBaseDamage; // Base damage potentially modified by owner stats

    void Start()
    {
        // Cache owner components and stats
        if (owner != null)
        {
            _ownerBuffManager = owner.GetComponent<BuffManager>();
            _ownerStats = owner.GetComponent<EntityStats>();
            if (_ownerStats != null)
            {
                _critChance = _ownerStats.getCritChance();
                // Apply owner's damage mods to the base damage once
                _finalBaseDamage = Mathf.FloorToInt(_ownerStats.damageMod.ApplyModifier(damage));
            }
            else
            {
                _finalBaseDamage = damage; // Use base damage if no owner stats
            }
        }
        else
        {
            _finalBaseDamage = damage; // Use base damage if no owner
        }


        // Validate radii
        if (fullDamageRadius > damageRadius)
        {
            Debug.LogWarning("Full Damage Radius cannot be greater than Damage Radius. Clamping.", this);
            fullDamageRadius = damageRadius;
        }

        // Start the explosion process
        StartCoroutine(ExplosionSequence());

        // Start self-destruct timer
        Destroy(gameObject, duration);
    }

    IEnumerator ExplosionSequence()
    {
        // Initial effects (optional delay for these too if needed)
        if (explosionParticles != null)
        {
            explosionParticles.Play();
        }
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, soundVolume);
        }

        // Wait for the activation delay
        if (activationDelay > 0)
        {
            yield return new WaitForSeconds(activationDelay);
        }

        // --- Explosion Effects Trigger ---
        PerformExplosion();
    }

    void PerformExplosion()
    {
        // Find all colliders within the maximum effect radius (damage or knockback)
        float maxRadius = damageRadius;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, maxRadius);

        foreach (Collider hit in hitColliders)
        {
            // Attempt to get EntityStats from the hit collider's GameObject
            EntityStats targetStats = hit.GetComponent<EntityStats>();
            if (targetStats == null) continue; // Not an entity we can affect

            // --- Friendly Fire Check ---
            if (!ShouldAffectTarget(targetStats))
            {
                continue; // Skip this target based on friendly fire rules
            }

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            // --- Damage Calculation & Application ---
            if (distance <= damageRadius)
            {
                int calculatedDamage = CalculateFalloffDamage(distance);
                if (calculatedDamage > 0)
                {
                    // Create the Attack object
                    Attack attack = new Attack(owner, calculatedDamage, _critChance, 0, procCoefficient); // Knockback handled separately by AddExplosionForce

                    // Add status effect if assigned
                    if (statusEffectPrefab != null)
                    {
                        attack.debuffsToApply.Add(statusEffectPrefab);
                    }

                    // Trigger owner's OnHitEffects (if applicable) BEFORE applying the hit
                    if (procCoefficient > 0 && _ownerBuffManager != null)
                    {
                        // Pass the target's GameObject and the attack instance
                        _ownerBuffManager.TriggerOnHitEffects(hit.gameObject, attack);
                    }

                    // Apply the final attack to the target
                    targetStats.TakeHit(attack);
                }
            }

            // --- Knockback Application ---
            if (knockback > 0)
            {
                ApplyKnockback(hit.attachedRigidbody); // Pass Rigidbody for efficiency
            }
        }
    }

    int CalculateFalloffDamage(float distance)
    {
        // Check if target is outside explosion radius
        if (distance > damageRadius)
        {
            return 0; // No damage beyond the radius
        }
        // Check if target is within full damage range
        else if (distance <= fullDamageRadius)
        {
            return _finalBaseDamage; // Full damage
        }
        // Otherwise, calculate falloff damage
        else
        {
            // Calculate normalized distance (0-1) between fullDamageRadius and damageRadius
            float falloffRange = damageRadius - fullDamageRadius;
            // Avoid division by zero if radii are the same
            float normalizedDist = (falloffRange > 0) ? (distance - fullDamageRadius) / falloffRange : 1f;

            // Lerp between full damage and minimum damage
            return Mathf.FloorToInt(Mathf.Lerp(_finalBaseDamage, _finalBaseDamage * minFalloffPercent, normalizedDist));
        }
    }

    void ApplyKnockback(Rigidbody targetRigidbody)
    {
        if (targetRigidbody == null || targetRigidbody.isKinematic) return;

        // Use AddExplosionForce for physically realistic explosion knockback
        targetRigidbody.AddExplosionForce(knockback, transform.position, knockbackRadius, knockbackVerticalBias, ForceMode.Impulse);
    }

    // --- Friendly Fire Logic ---
    public bool ShouldAffectTarget(EntityStats targetStats)
    {
        if (friendlyFire)
        {
            return true; // Affect everyone if friendly fire is on
        }

        GameObject targetObject = targetStats.gameObject;

        // Case 1: No owner - Hazard affects everyone
        if (owner == null)
        {
            return true;
        }

        // --- Owner exists, check teams ---

        // Check if the target is the owner - owners never hit themselves unless FF is on
        if (targetObject == owner)
        {
            return false;
        }

        bool ownerIsPlayer = owner.GetComponent<PlayerControllerScript>() != null; // Assuming PlayerControllerScript marks the player
        bool targetIsPlayer = targetObject.CompareTag("Player") && targetObject.GetComponent<PlayerControllerScript>() != null; // Check tag and script

        // Case 2: Owner is an Enemy (No PlayerControllerScript) - Affects only the Player
        if (!ownerIsPlayer)
        {
            return targetIsPlayer;
        }

        // Case 3: Owner is the Player - Affects only non-Players
        if (ownerIsPlayer)
        {
            return !targetIsPlayer;
        }

        // Default case (shouldn't be reached if logic is sound)
        return true;
    }
}