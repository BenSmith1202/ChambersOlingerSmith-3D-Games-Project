using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralExplosionScript : MonoBehaviour
{

    [Header("Explosion FX")]
    [SerializeField] ParticleSystem explosionParticles;
    [SerializeField] AudioClip explosionSound;
    [SerializeField] float soundVolume = 1;

    [Header("Explosion Settings")]
    [SerializeField] float duration; //time until object is destroyed
    [SerializeField] float delay; //time into the coroutine before the explosion happens
    [SerializeField] float radius; // radius of the explosion
    public int damage; // base damage of the explosion

    [Header("Explosion Proc Options")]
    [SerializeField] float procCoeff; // the chance to proc on hit effects
    [SerializeField] GameObject onHitDebuff; //the debuff to apply on hit

    [Header("Explosion Knockback Options")]
    [SerializeField] float knockback;
    [SerializeField] float knockbackVerticalBias;

    [Header("Explosion Falloff Options")]
    [SerializeField] float falloffMinPercent;
    [SerializeField] float falloffStartRad;

    [Header("Don't touch:")]
    public GameObject owner;
    BuffManager ownerBuffManager;
    EntityStats ownerStats;
    float critChance = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        if(owner != null)
        {
            ownerBuffManager = owner.GetComponent<BuffManager>();
            ownerStats = owner.GetComponent<EntityStats>();
        }
        if (ownerStats != null)
        {
            critChance = ownerStats.getCritChance();
            damage = Mathf.FloorToInt(ownerStats.damageMod.ApplyModifier(damage));
        }


        Explode();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Explode()
    {
        StartCoroutine(ExplodeCoroutine());
    }

    IEnumerator ExplodeCoroutine()
    {
        explosionParticles.Play();
        AudioSource.PlayClipAtPoint(explosionSound, transform.position, soundVolume);

        yield return new WaitForSeconds(delay);

        Collider[] hitEntities = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider c in hitEntities)
        {
            EntityStats enemyStats = c.GetComponent<EntityStats>();
            if (enemyStats != null)
            {

                //calculate distance from explosion center
                float dist = Vector3.Distance(transform.position, c.transform.position);
                //calculate damage
                               
                int damageDealt = CalculateDamage(dist);
                
                //ATTACK CALCULATIONS
                //create attack object
                Attack atk = new Attack(owner, damageDealt, critChance, 0, procCoeff);
                
                //If owner has a buffManager and this has a chance to proc
                if ( procCoeff > 0 && ownerBuffManager != null)
                {
                    //stack on hit effects
                    ownerBuffManager.TriggerOnHitEffects(c.gameObject, atk);
                }

                //if this has a special debuff to inflict
                if (onHitDebuff != null && ownerBuffManager != null)
                {
                    //add it to the attack
                    atk.debuffsToApply.Add(onHitDebuff);
                }

                //inflict attack
                enemyStats.TakeHit(atk);

                //apply knockback
                Vector3 knockbackDir = (c.transform.position - transform.position).normalized;

                //apply knockback
                ApplyKnockback(c);

                //duration seconds after this coroutine was started...
                yield return new WaitForSeconds(Mathf.Clamp(duration - delay, 0, duration));
                //destroy the explosion.
                Destroy(gameObject);
            }
        }
        
    }
    private int CalculateDamage(float dist)
    {
        //DAMAGE FALLOFF CALCULATIONS
        int damageDealt = 0;
        // Check if target is outside explosion radius
        if (dist > radius)
        {
            damageDealt = 0;  // No damage beyond the radius
        }
        // Check if target is within full damage range
        else if (dist <= falloffStartRad)
        {
            damageDealt = damage;  // Full damage
        }
        // Otherwise, calculate falloff damage
        else
        {
            // Calculate normalized distance (0-1) between falloffStartRad and radius
            float falloffRange = radius - falloffStartRad;
            float normalizedDist = (dist - falloffStartRad) / falloffRange;

            // Lerp between full damage and minimum damage
            damageDealt = Mathf.FloorToInt(Mathf.Lerp(damage, damage * falloffMinPercent, normalizedDist));
        }
        return damageDealt;
    }

    private void ApplyKnockback(Collider target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb == null || rb.isKinematic) return;

        rb.AddExplosionForce(knockback, transform.position, radius, knockbackVerticalBias, ForceMode.Impulse);
    }

}
