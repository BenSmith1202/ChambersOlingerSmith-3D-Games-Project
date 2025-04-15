using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldHazardScript : MonoBehaviour
{
    [Header("Hazard Settings")]
    [SerializeField] private float lifetime = 10f; // Set to <=0 for infinite
    [SerializeField] private float debuffRefreshRate = 2f; // Time between debuff applications
    [SerializeField] private GameObject debuffToApply; // The debuff prefab to apply
    [SerializeField] private float procCoeff = 0f; // Chance of onhit effects triggering
    [SerializeField] private int damage = 0; // Base damage of the hazard (if applicable)
    [SerializeField] private float knockback = 0f; // Knockback amount (if applicable)

    [Header("Effects")]
    [SerializeField] private ParticleSystem hazardParticles;
    [SerializeField] private AudioSource hazardSound; // Using AudioSource for looping
    [SerializeField] private float soundVolume = 1f;

    [Header("Owner")]
    public GameObject owner; // Optional: who created this hazard?
    private BuffManager ownerBuffManager;
    private EntityStats ownerStats;
    private float critChance = 0f;

    private HashSet<GameObject> entitiesInHazard = new HashSet<GameObject>(); // Track entities inside
    private Coroutine refreshCoroutine;

    private void Start()
    {
        // Initialize effects
        if (hazardParticles != null) hazardParticles.Play();
        if (hazardSound != null)
        {
            hazardSound.volume = soundVolume;
            hazardSound.loop = true;
            hazardSound.Play();
        }

        // Set up owner references (if owner exists)
        if (owner != null)
        {
            ownerBuffManager = owner.GetComponent<BuffManager>();
            ownerStats = owner.GetComponent<EntityStats>();
            if (ownerStats != null)
            {
                critChance = ownerStats.getCritChance();
                damage = Mathf.FloorToInt(ownerStats.damageMod.ApplyModifier(damage));
            }

        }

        // Start lifetime countdown (if not infinite)
        if (lifetime > 0) StartCoroutine(LifetimeCountdown());

        // Start debuff refresh loop
        refreshCoroutine = StartCoroutine(DebuffRefreshLoop());
    }

    private IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(lifetime);
        DestroyHazard();
    }

    private IEnumerator DebuffRefreshLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(debuffRefreshRate);
            ApplyDebuffToAllInHazard();
        }
    }

    private void ApplyDebuffToAllInHazard()
    {
        foreach (GameObject entity in entitiesInHazard)
        {
            if (entity == null) continue; // Skip if entity was destroyed

            BuffManager targetBuffManager = entity.GetComponent<BuffManager>();
            EntityStats targetStats = entity.GetComponent<EntityStats>();

            if (targetStats != null)
            {
                // Create an attack to apply the debuff
                Attack debuffAttack = new Attack(
                    owner,
                    damage, // Optional: damage if hazard should also deal damage
                    critChance,
                    knockback, // knockback DEPRECATED
                    procCoeff
                );

                // Add debuff if configured
                if (debuffToApply != null)
                {
                    debuffAttack.debuffsToApply.Add(debuffToApply);
                }

                // Trigger owner's on-hit effects (if owner exists)
                if (procCoeff > 0 && ownerBuffManager != null)
                {
                    ownerBuffManager.TriggerOnHitEffects(entity, debuffAttack);
                }

                // Apply debuff
                targetStats.TakeHit(debuffAttack);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EntityStats>() != null)
        {
            entitiesInHazard.Add(other.gameObject);
            ApplyDebuffToEntity(other.gameObject); // Apply immediately on entry
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (entitiesInHazard.Contains(other.gameObject))
        {
            entitiesInHazard.Remove(other.gameObject);
        }
    }

    private void ApplyDebuffToEntity(GameObject entity)
    {
        // Same logic as in ApplyDebuffToAllInHazard, but for a single entity
        BuffManager targetBuffManager = entity.GetComponent<BuffManager>();
        EntityStats targetStats = entity.GetComponent<EntityStats>();

        if (targetStats != null)
        {
            Attack debuffAttack = new Attack(owner, damage, critChance, knockback, procCoeff);
            if (debuffToApply != null) debuffAttack.debuffsToApply.Add(debuffToApply);
            if (procCoeff > 0 && ownerBuffManager != null) ownerBuffManager.TriggerOnHitEffects(entity, debuffAttack);
            targetStats.TakeHit(debuffAttack);
        }
    }

    private void DestroyHazard()
    {
        if (refreshCoroutine != null) StopCoroutine(refreshCoroutine);
        if (hazardParticles != null) hazardParticles.Stop();
        if (hazardSound != null) hazardSound.Stop();
        Destroy(gameObject);
    }
}