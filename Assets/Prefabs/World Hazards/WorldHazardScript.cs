using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldHazardScript : MonoBehaviour
{
    [Header("Core Settings")]
    [Tooltip("Who created this hazard? Determines targeting based on friendlyFire.")]
    public GameObject owner;
    [Tooltip("If true, affects all entities. If false, follows owner-based rules.")]
    public bool friendlyFire = false;
    [Tooltip("Base damage dealt per tick/application.")]
    public int damagePerTick = 5;
    [Tooltip("Base knockback force applied on tick/entry.")]
    public float knockback = 0f;
    [Tooltip("The chance (0-1) for this hazard's ticks to trigger owner's OnHit effects.")]
    [Range(0f, 1f)]
    public float procCoefficient = 0f;
    [Tooltip("Optional status effect prefab to apply to affected entities.")]
    public GameObject statusEffectPrefab; // Renamed from debuffToApply

    [Header("Timing & Application")]
    [Tooltip("How long the hazard lasts in seconds. Set to 0 or less for infinite.")]
    public float lifetime = 10f;
    [Tooltip("Time in seconds between applying effects to entities inside.")]
    [Min(0.1f)] // Ensure refresh rate is reasonable
    public float effectRefreshRate = 1f; // Renamed from debuffRefreshRate
    [Tooltip("Apply effects immediately when an entity enters?")]
    public bool applyOnEntry = true;

    [Header("Area of Effect (Knockback)")]
    [Tooltip("Radius within which knockback is applied.")]
    public float knockbackRadius = 3f;
    [Tooltip("How much knockback force is directed upwards (0 = horizontal, 1 = vertical).")]
    [Range(0f, 1f)]
    public float knockbackVerticalBias = 0f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem hazardParticles;
    [SerializeField] private AudioSource hazardSound; // Use AudioSource for looping
    [SerializeField][Range(0f, 1f)] private float soundVolume = 0.5f;

    // State
    private HashSet<EntityStats> _entitiesInHazard = new HashSet<EntityStats>();
    private Coroutine _refreshCoroutine;
    private bool _isDestroying = false;

    // Cached references
    private BuffManager _ownerBuffManager;
    private EntityStats _ownerStats;
    private float _critChance = 0f;
    private int _finalTickDamage; // Tick damage potentially modified by owner stats


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
                // Apply owner's damage mods to the tick damage once
                _finalTickDamage = Mathf.FloorToInt(_ownerStats.damageMod.ApplyModifier(damagePerTick));
            }
            else
            {
                _finalTickDamage = damagePerTick; // Use base damage if no owner stats
            }
        }
        else
        {
            _finalTickDamage = damagePerTick; // Use base damage if no owner
        }


        // Start visual/audio effects
        if (hazardParticles != null) hazardParticles.Play();
        if (hazardSound != null)
        {
            hazardSound.volume = soundVolume;
            hazardSound.loop = true;
            hazardSound.Play();
        }

        // Start lifetime countdown if applicable
        if (lifetime > 0)
        {
            StartCoroutine(LifetimeCountdown());
        }

        // Start the periodic effect application
        _refreshCoroutine = StartCoroutine(EffectRefreshLoop());
    }

    void OnDestroy()
    {
        // Ensure coroutines are stopped if the object is destroyed externally
        _isDestroying = true; // Flag to prevent issues during shutdown
        if (_refreshCoroutine != null)
        {
            StopCoroutine(_refreshCoroutine);
        }
    }

    IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(lifetime);
        DestroyHazard();
    }

    IEnumerator EffectRefreshLoop()
    {
        // Wait a frame to ensure OnTriggerEnter might have been called
        yield return null;

        while (!_isDestroying)
        {
            // Use a temporary list to avoid issues if entities leave during iteration
            List<EntityStats> currentTargets = new List<EntityStats>(_entitiesInHazard);

            foreach (EntityStats targetStats in currentTargets)
            {
                // Double-check if the entity is still valid and in the set
                if (targetStats == null || !_entitiesInHazard.Contains(targetStats)) continue;

                // Check friendly fire rules *before* applying effects
                if (ShouldAffectTarget(targetStats))
                {
                    ApplyEffects(targetStats);
                }
            }

            yield return new WaitForSeconds(effectRefreshRate);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (_isDestroying) return; // Don't add if we're shutting down

        EntityStats targetStats = other.GetComponent<EntityStats>();
        if (targetStats != null)
        {
            bool added = _entitiesInHazard.Add(targetStats); // Add to set

            // Apply immediately on entry if configured and allowed by FF rules
            if (added && applyOnEntry && ShouldAffectTarget(targetStats))
            {
                ApplyEffects(targetStats);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (_isDestroying) return;

        EntityStats targetStats = other.GetComponent<EntityStats>();
        if (targetStats != null)
        {
            _entitiesInHazard.Remove(targetStats); // Remove from set
        }
    }

    void ApplyEffects(EntityStats targetStats)
    {
        if (targetStats == null || targetStats.isDead) return; // Don't affect null or dead entities

        // --- Damage/Status Effect Application ---
        if (_finalTickDamage > 0 || statusEffectPrefab != null)
        {
            // Create the Attack object for this tick
            Attack attack = new Attack(owner, _finalTickDamage, _critChance, 0, procCoefficient); // Knockback handled separately

            // Add status effect if assigned
            if (statusEffectPrefab != null)
            {
                attack.debuffsToApply.Add(statusEffectPrefab);
            }

            // Trigger owner's OnHitEffects (if applicable) BEFORE applying the hit
            if (procCoefficient > 0 && _ownerBuffManager != null)
            {
                // Pass the target's GameObject and the attack instance
                _ownerBuffManager.TriggerOnHitEffects(targetStats.gameObject, attack);
            }


            // Apply the final attack to the target
            targetStats.TakeHit(attack);
        }

        // --- Knockback Application ---
        if (knockback > 0)
        {
            ApplyKnockback(targetStats.GetComponent<Rigidbody>()); // Get Rigidbody
        }
    }

    void ApplyKnockback(Rigidbody targetRigidbody)
    {
        if (targetRigidbody == null || targetRigidbody.isKinematic) return;

        // Use AddExplosionForce for consistency, even if it's a hazard area
        targetRigidbody.AddExplosionForce(knockback, transform.position, knockbackRadius, knockbackVerticalBias, ForceMode.Impulse);
    }


    // --- Friendly Fire Logic (Identical to Explosion Script) ---
    bool ShouldAffectTarget(EntityStats targetStats)
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

    private void DestroyHazard()
    {
        if (_isDestroying) return; // Prevent double destruction
        _isDestroying = true;

        if (_refreshCoroutine != null) StopCoroutine(_refreshCoroutine);

        // Stop effects gracefully
        if (hazardParticles != null) hazardParticles.Stop();
        if (hazardSound != null) hazardSound.Stop();

        // Clear entities just in case
        _entitiesInHazard.Clear();

        Destroy(gameObject);
    }
}