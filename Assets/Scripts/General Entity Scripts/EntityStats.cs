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
    public int level;


    //stats and modifiers initialized here
    #region Stat Initialization

    //Private Base Stats:
    // Should not be modified from outside of this class (except perhaps during a level up)
    [SerializeField] private int baseMaxHP = 100;
    [SerializeField] private float baseRegen = 0f; //health regen per second
    [SerializeField] private float baseSpeed;     // Walking speed of the entity
    [SerializeField] private float baseJumpForce;     // Force applied when the entity jumpss
    [SerializeField] private float baseAttackDelay;
    [SerializeField] private int baseDamage; //base damage (gun deals 100% base damage, dash punch probably deals 250% base damage)
    [SerializeField] private float baseCritChance;
    [SerializeField] private float baseRange = 100f;
    [SerializeField] private float baseKB; //base knockback
    [SerializeField] private float baseReloadTime;

    [SerializeField] private float baseAttackRange;
    public float regenDelay = 3f;
    private float regenCountdown = 0f;




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

    //Stat Modifiers:
    
    // Used for getters, changed by buffs and items.
    [Header("Stat Multipliers")]
    public StatModifier maxHPMod =          new StatModifier();
    public StatModifier regenMod =          new StatModifier();

    public StatModifier speedMod =          new StatModifier();
    public StatModifier jumpMod =           new StatModifier();

    public StatModifier attackDelayMod =    new StatModifier();
    public StatModifier damageMod =         new StatModifier();
    public StatModifier critChanceMod =     new StatModifier();

    public StatModifier rangeMod =          new StatModifier();
    public StatModifier knockbackMod =      new StatModifier();
    public StatModifier reloadTimeMod =     new StatModifier();

    public StatModifier healingMod =        new StatModifier();
    public StatModifier damageReductionMod= new StatModifier();

    public StatModifier dashDelayMod =      new StatModifier();
    public StatModifier grappleDelayMod =   new StatModifier();

    #endregion


    //References
    [Header("References")]
    public BuffManager buffManager;
    public HealthBarScript healthBarScript;
    public DamageNumbers damageNumbers;
    LogicManager logic;


    private LootPool loot;


    private void Start()
    {

        loot = gameObject.GetComponent<LootPool>();

        logic = GameObject.FindGameObjectWithTag("LogicManager").GetComponent<LogicManager>();
        buffManager = GetComponent<BuffManager>();
        InitializeHealthBar();

        //It's ok if this is null
        damageNumbers = GetComponent<DamageNumbers>();

        SetMaxHP(baseMaxHP);
        SetHP(baseMaxHP);

        
    }

    private void Update()
    {
        if (regenCountdown > regenDelay)
        {
            regenCountdown = 0;
            Heal(getRegen());
        } else
        {
            regenCountdown += Time.deltaTime;
        }

        if (!gameObject.CompareTag("Player")) //if i am an enemy
        {
            if (level < logic.enemyLevel)
            {
                LevelUp(logic.enemyLevel - level);
            }
        }
    }


    //functions specific to combat
    #region combat
    // Universal takeHit function that takes an attack object and applies the damage, knockback,
    // and correctly modifies any relevant health bar object 
    public void TakeHit(Attack atk)
    {
        //If the attak is lethal, trigger OnKill effects
        if (atk.damage >= currentHP)
        {
            if (atk.owner != null)
            {
                BuffManager attackerBuffManager = atk.owner.GetComponent<BuffManager>();
                if (attackerBuffManager != null)
                {
                    attackerBuffManager.TriggerOnKillEffects(gameObject, atk);
                }
            }
            
        }

        if (atk.debuffsToApply != null)
        {
            // apply each debuff
            foreach (var buffObject in atk.debuffsToApply)
            {
                buffManager.AddBuff(buffObject);
            }
        }
        

        InflictDamage(atk.damage);

       
    }
    #endregion

    // These are used to get the scaled versions of stats mid game.
    #region Stat Getters:

    public int getMaxHP()
    {
        return Mathf.FloorToInt(maxHPMod.ApplyModifier(baseMaxHP));
    }

    public int getRegen()
    {
        return Mathf.FloorToInt(regenMod.ApplyModifier(baseRegen));
    }

    public float getSpeed()
    {
        return speedMod.ApplyModifier(baseSpeed);
    }

    public float getJumpForce()
    {
        return jumpMod.ApplyModifier(baseJumpForce);
    }

    public float getAtkDelay()
    {
        return attackDelayMod.ApplyModifier(baseAttackDelay);
    }

    public int getDamage()
    {
        return Mathf.FloorToInt(damageMod.ApplyModifier(baseDamage));
    }

    public float getCritChance()
    {
        return critChanceMod.ApplyModifier(baseCritChance);
    }

    public float getRange()
    {
        return rangeMod.ApplyModifier(baseRange);
    }

    public float getKnockback()
    {
        return knockbackMod.ApplyModifier(baseKB);
    }

    public float getReloadTime()
    {
        return reloadTimeMod.ApplyModifier(baseReloadTime);
    }

    public float getGrappleDelay()
    {
        return grappleDelayMod.ApplyModifier(baseGrappleCooldownTime);
    }

    public float getDashDelay()
    {
        return dashDelayMod.ApplyModifier(baseDashCooldownTime);
    }

    public float getDashLaunchSpeed()
    { 
        return speedMod.ApplyModifier(dashLaunchSpeed);
    }

    public float getAttackRange()
    {
        return rangeMod.ApplyModifier(baseAttackRange);
    }
    #endregion


    //health initialization, getters, and setters
    #region Health Management


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


    public void SetHP(int newHP)
    {
        newHP = Mathf.Clamp(newHP, 0, baseMaxHP); //prevents overfilling or negative HP
        currentHP = newHP;

        //am i dead?
        if (currentHP <= 0)
        {
            if(loot != null)
            {
                loot.AttemptLootDrop();
            }
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
           // Debug.Log("No health bar found");
        }
        

    }


    public void IncreaseMaxHP(int amount)
    {
        SetMaxHP(getMaxHP() + amount);
    }


    public void Heal(int amount)
    {
        int scaledHealing = Mathf.FloorToInt(healingMod.ApplyModifier(amount));
        SetHP(currentHP + scaledHealing);
        //if (damageNumbers != null && damageNumbers.showDamage)
        //{
            
        //    damageNumbers.canvasScript.ShowDamageNumber(amount);
        //}
    }


    public void DecreaseMaxHP(int amount)
    {
        SetMaxHP(getMaxHP() - amount);
    }


    public void InflictDamage(int amount)
    {
        int scaledDamage = Mathf.FloorToInt(damageReductionMod.ApplyModifier(amount));
        SetHP(currentHP - scaledDamage);
        if (damageNumbers != null && damageNumbers.showDamage)
        {
            damageNumbers.canvasScript.ShowDamageNumber(-scaledDamage);
        }
    }

    #endregion


    // LEVEL UP
    #region Leveling Up
    [Header("Level Scaling")]
    public int levelUpHealth = 15;
    public int levelUpDamage = 3;
    public float levelUpRegen = 0f;
    public void LevelUp(int amountOfLevels)
    {
        for (int i = 0; i < amountOfLevels; i++)
        {
            level++;
            baseDamage += levelUpDamage;
            baseMaxHP += levelUpHealth;
            SetMaxHP(getMaxHP()); //update the health bar
            Heal(levelUpHealth); //heal for the amount gained
            baseRegen += levelUpRegen;

        }
    }
    #endregion
}


/**
     *
     * Stores a flat, percent, and multiplicative damage boost.
     * for non-scaling linear growth, use flat
     * for scaling linear growth, use percent
     * for exponential or logarithmic growth, use mult
     *
     **/
public class StatModifier
{
    public float flat = 0f;
    public float percent = 0f;
    public float mult = 1f;

    public StatModifier(float flat, float percent, float mult)
    {
        this.flat = flat;
        this.percent = percent;
        this.mult = mult;
    }

    public StatModifier()
    {
        //empty constructor for now
    }

    // applies modifiers like so:
    public float ApplyModifier(float baseStat)
    {
        float totalStat = (baseStat + flat); //apply flat bonuses first
        totalStat += percent * totalStat; //apply percent bonuses second
        totalStat *= mult; //apply multiplicative bonuses last
        return totalStat;
    }

}