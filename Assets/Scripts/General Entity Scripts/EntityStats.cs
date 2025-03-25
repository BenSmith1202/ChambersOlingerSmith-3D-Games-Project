using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class EntityStats : MonoBehaviour
{
    [Header("Entity Stats")]
    public bool isDead;
    public int currentHP;
    public int clipSize; //does not really need a multiplier lol

    //Private Base Stats:
    // Should not be modified from outside of this class (except perhaps during a level up)
    [SerializeField] private int baseMaxHP = 100;
    [SerializeField] private float baseSpeed;     // Walking speed of the entity
    [SerializeField] private float baseJumpForce;     // Force applied when the entity jumpss
    [SerializeField] private float baseAttackDelay;
    [SerializeField] private int baseDamage; //base damage (gun deals 100% base damage, dash punch probably deals 250% base damage)
    [SerializeField] private float baseCritChance;
    [SerializeField] private float baseRange = 100f;
    [SerializeField] private float baseKB; //base knockback
    [SerializeField] private float baseReloadTime;

    [SerializeField] private float baseAttackRange;




    // Stats mostly used by the player/not likely to be used by enemies.
    // grouped here for convienience
    #region Player Ability Exlusive Stats
    [Header("Player Specific Stats")]
    public float crouchSpeed;   // Speed during crouching
    public float wallrunForce;  // effectively wall run speed
    public float maxWallTime;   // Maximum time in seconds that a player can run on walls before touching the ground
    public float wallrunSpeed;   // Speed during wallrun

    public float grappleRange;
    [SerializeField] private float baseGrappleCooldownTime;
    public float grappleForce;  // force of the ropes tension
    public float grappleStretch;    // divides the above, how stretchy the rope is
    public float grappleReelSpeed;  // how fast the max length of the grapple decreases (linearly)
    public float grappleYankPercentDistance; // percent of player's distance from the grapple point that the rope length is initialized to
    public float swingSpeed;                 // The speed at which the player influences their momentum with the movement keys
    public float maxSwingVelocity;           // the speed cap for grappling.

    [SerializeField] private float baseDashCooldownTime;
    public float dashDuration;
    public float midDashSpeed;
    public float postDashSpeedReduction;
    [SerializeField] private float dashLaunchSpeed;
    #endregion

    //Stat Multipliers:
    // Used for getters, changed by buffs and items.
    [Header("Stat Multipliers")]
    public float maxHPMult = 1f;

    public float speedMult = 1f;
    public float jumpMult = 1f;

    public float attackDelayMult = 1f;
    public float damageMult = 1f;
    public float critChanceMult = 1f;

    public float rangeMult = 1f;
    public float knockbackMult = 1f;
    public float reloadTimeMult = 1f;

    public float healingMult = 1f;
    public float damageReductionMult = 1f;

    public float dashDelayMult = 1f;
    public float grappleDelayMult = 1f;


    //References
    [Header("References")]
    public BuffManager buffManager;
    public HealthBarScript healthBarScript;
    public DamageNumbers damageNumbers;


    private void Start()
    {
        buffManager = GetComponent<BuffManager>();
        InitializeHealthBar();

        //It's ok if this is null
        damageNumbers = GetComponent<DamageNumbers>();

        SetMaxHP(baseMaxHP);
        SetHP(baseMaxHP);
    }


    // Universal takeHit function that takes an attack object and applies the damage, knockback,
    // and correctly modifies any relevant health bar object 
    public void TakeHit(Attack atk)
    {
        //If the attak is lethal, trigger OnKill effects
        if (atk.damage > currentHP)
        {
            BuffManager attackerBuffManager = atk.owner.GetComponent<BuffManager>();
            if (attackerBuffManager != null)
            {
                attackerBuffManager.TriggerOnKillEffects(gameObject, atk);
            }
        }

        InflictDamage(atk.damage);

        //TODI: apply knockback
    }


    public void InitializeHealthBar()
    {
        //check if i have a health canvas child
        Transform healthCanvas = transform.Find("HealthCanvas");
        if (healthCanvas != null)
        {
            healthBarScript = healthCanvas.gameObject.GetComponent<HealthBarScript>();
        }
        else
        {
            //Debug.Log("No enemy healthbar found, checking player");
            //check if i have a health bar myself
            // am i a player?
            PlayerHealthScript myhealth = GetComponent<PlayerHealthScript>();
            if (myhealth != null)
            {
                //if so, i can use the UI health Bar
                GameObject playerHealthBar = GameObject.FindWithTag("PlayerHealthBar");
                healthBarScript = playerHealthBar.GetComponent<HealthBarScript>();
                //Debug.Log("Player health bar found");
                // Initialize player health bar slider
                healthBarScript.healthBarSlider = playerHealthBar.GetComponent<Slider>();
            }

        }
    }

    // These are used to get the scaled versions of stats mid game.
    #region Stat Getters:

    public int getMaxHP()
    {
        return Mathf.FloorToInt(baseMaxHP * maxHPMult);
    }

    public float getSpeed()
    {
        return baseSpeed * speedMult;
    }

    public float getJumpForce()
    {
        return baseJumpForce * jumpMult;
    }

    public float getAtkDelay()
    {
        return baseAttackDelay * attackDelayMult;
    }

    public int getDamage()
    {
        return Mathf.FloorToInt(baseDamage * damageMult);
    }

    public float getCritChance()
    {
        return baseCritChance * critChanceMult;
    }

    public float getRange()
    {
        return baseRange * rangeMult;
    }

    public float getKnockback()
    {
        return baseKB * knockbackMult;
    }

    public float getReloadTime()
    {
        return baseReloadTime * reloadTimeMult;
    }

    public float getGrappleDelay()
    {
        return baseGrappleCooldownTime * grappleDelayMult;
    }

    public float getDashDelay()
    {
        return baseDashCooldownTime * dashDelayMult;
    }

    public float getDashLaunchSpeed()
    {
        return dashLaunchSpeed * speedMult;
    }

    public float getAttackRange()
    {
        return baseAttackRange;
    }
    #endregion


    public void SetHP(int newHP)
    {
        newHP = Mathf.Clamp(newHP, 0, baseMaxHP); //prevents overfilling or negative HP
        currentHP = newHP;

        //am i dead?
        if (currentHP <= 0)
        {
            isDead = true;
        }

        // Trigger any on HP effects
        if (buffManager != null)
        {
            buffManager.TriggerOnHPEffects(gameObject);
        }

        //Debug.Log("Setting HP to " + newHP);

        if (healthBarScript != null)
        {
            healthBarScript.SetHP(newHP); //update healthbar
        } else
        {
            //Debug.Log("No health bar found");
        }
        
    }

    public void SetMaxHP(int newMaxHP)
    {
        //Debug.Log("Setting HP to " + newMaxHP);
        if (newMaxHP < 1) //prevents division by zero
        {
            newMaxHP = 1;
        }
        int oldMax = baseMaxHP;
        baseMaxHP = newMaxHP;

        if (newMaxHP < currentHP) //if max hp is set lower than the current hp, set the player's current hp to the new max
        {
            SetHP(newMaxHP);
        } else
        {
            Heal(newMaxHP - oldMax); // heal for the amount gained.
        }


        if (healthBarScript != null)
        {
            healthBarScript.SetMaxHP(newMaxHP); //update healthbar
        }
        else
        {
            Debug.Log("No health bar found");
        }
        

    }

    public void IncreaseMaxHP(int amount)
    {
        SetMaxHP(getMaxHP() + amount);
    }

    public void Heal(int amount)
    {
        int scaledHealing = Mathf.FloorToInt(amount * healingMult);
        SetHP(currentHP + scaledHealing);
        if (damageNumbers != null && damageNumbers.showDamage)
        {
            
            damageNumbers.canvasScript.ShowDamageNumber(amount);
        }
    }

    public void DecreaseMaxHP(int amount)
    {
        SetMaxHP(getMaxHP() - amount);
    }

    public void InflictDamage(int amount)
    {
        int scaledDamage = Mathf.FloorToInt(amount * damageReductionMult);
        SetHP(currentHP - scaledDamage);
        if (damageNumbers != null && damageNumbers.showDamage)
        {
            damageNumbers.canvasScript.ShowDamageNumber(-scaledDamage);
        }
    }
}