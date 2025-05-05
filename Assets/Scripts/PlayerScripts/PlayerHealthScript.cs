using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Keep for potential future use, but not for direct death screen loading

public class PlayerHealthScript : MonoBehaviour
{
    // --- References ---
    [Tooltip("Assign the Health Bar UI GameObject here.")]
    [SerializeField] private GameObject healthBar; // Assign in Inspector

    // Components fetched at Start
    private EntityStats stats;
    private ForcesToRB forcesToRBScript; // Optional: If knockback is used
    private HealthBarScript healthBarScript; // Fetched from healthBar GameObject

    // --- State ---
    private bool isDead = false; // Prevent multiple death triggers

    // --- Unity Methods ---

    void Start()
    {
        // Get required components
        stats = GetComponent<EntityStats>();
        if (stats == null)
        {
            Debug.LogError("PlayerHealthScript: EntityStats component not found!", this);
        }

        // Get optional components
        forcesToRBScript = GetComponent<ForcesToRB>();
        // if (forcesToRBScript == null) Debug.LogWarning("PlayerHealthScript: ForcesToRB component not found (optional).", this);

        // Get HealthBarScript from the assigned GameObject
        if (healthBar != null)
        {
            healthBarScript = healthBar.GetComponent<HealthBarScript>();
            if (healthBarScript == null)
            {
                Debug.LogError("PlayerHealthScript: HealthBarScript component not found on the assigned healthBar GameObject!", healthBar);
            }
        }
        else
        {
            Debug.LogError("PlayerHealthScript: Health Bar GameObject is not assigned in the Inspector!", this);
        }
    }

    void Update()
    {
        // Check for death condition, only if not already dead
        if (!isDead && stats != null && stats.currentHP <= 0)
        {
            Die();
        }

        // --- DEBUG TOOLS ---
        // Consider wrapping debug tools in #if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        HandleDebugInput();
#endif
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore triggers if already dead
        if (isDead) return;

        // Check for damage triggers
        DamageTriggerScript damageTrigger = other.gameObject.GetComponent<DamageTriggerScript>();
        if (damageTrigger != null && stats != null)
        {
            stats.InflictDamage(damageTrigger.GetDamage());
            // Optional: Trigger visual/audio feedback for taking damage
        }

        // Check for knockback triggers (if using ForcesToRB)
        // KnockBack knockbackTrigger = other.gameObject.GetComponent<KnockBack>();
        // if (knockbackTrigger != null && forcesToRBScript != null)
        // {
        //     forcesToRBScript.KnockMeBack(knockbackTrigger.GetKnockBack(transform.position));
        // }
    }

    // --- Private Methods ---

    /// <summary>
    /// Handles the player's death sequence.
    /// </summary>
    private void Die()
    {
        isDead = true; // Set flag to prevent repeated calls
        Debug.Log("Player has died.");

        // Disable player input/movement components here if necessary
        // Example: GetComponent<PlayerMovement>().enabled = false;

        // Trigger death animation/effects here if desired

        // Call the LogicManager to handle the game over sequence
        if (LogicManager.Instance != null)
        {
            LogicManager.Instance.GameOver();
        }
        else
        {
            // Fallback if LogicManager is missing (shouldn't happen with proper setup)
            Debug.LogError("PlayerHealthScript: LogicManager instance not found! Cannot trigger GameOver sequence.");
            // As a last resort, you *could* load the scene directly, but it's not ideal:
            // SceneManager.LoadScene("DeathScreen"); // Or use LogicManager's gameOverSceneName
        }
    }

    /// <summary>
    /// Handles debug key inputs for testing health.
    /// </summary>
    private void HandleDebugInput()
    {
        if (stats == null) return; // Don't run debug if stats component is missing

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            stats.InflictDamage(5);
            Debug.Log($"Debug: Inflicted 5 damage. Current HP: {stats.currentHP}");
        }

        if (Input.GetKeyDown(KeyCode.Equals)) // Usually '=' key requires Shift, check KeyCode if needed
        {
            stats.Heal(5);
            Debug.Log($"Debug: Healed 5 HP. Current HP: {stats.currentHP}");
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            stats.DecreaseMaxHP(5);
            Debug.Log($"Debug: Decreased Max HP by 5. Current MaxHP: {stats.getMaxHP()}");
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            stats.IncreaseMaxHP(5);
            Debug.Log($"Debug: Increased Max HP by 5. Current MaxHP: {stats.getMaxHP()}");
        }
    }
}
