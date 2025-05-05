using System; // Required for System.Action
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for SceneManager events

public class LogicManager : MonoBehaviour
{
    // --- Singleton Instance ---
    public static LogicManager Instance { get; private set; }

    // --- Events ---
    /// <summary>
    /// Event triggered after a new level scene (not loading screen/menu) has loaded.
    /// Actions queued via QueueActionForNextLevel will be invoked here.
    /// </summary>
    public event System.Action OnNextLevelLoaded;

    // --- Game State & Configuration ---
    public bool objectiveComplete = false;

    [Header("Game State")]
    public int currentStage = 1;
    public float difficultyLevel = 1;

    [Header("Playtime Tracking")]
    public float playtime = 0f;
    public bool isPlaytimePaused = false; // Controls the playtime coroutine

    [Header("Difficulty Scaling")]
    public float timePerDifficultyIncrease = 300f;
    public float timeSinceLastDifficultyIncrease = 0f;
    public int numTimeIncreaseEachTime;
    public float timeSinceLastEnemyLevelUp = 0f;
    public float timePerEnemyLevelUp = 60f;
    public int enemyLevel = 0;

    [Header("Game Over")]
    public float gameOverDelay = 3f;
    public string gameOverSceneName = "GameOverScene"; // Make scene name configurable

    [Header("Time Scale Settings")]
    // Durations are no longer used for instant change, but kept for potential future use
    [SerializeField] private float slowdownDuration = 2f;
    [SerializeField] private float targetTimeScale = 0.1f;
    [SerializeField] private float speedUpDuration = 1f;

    [Header("UI References (Assigned in Inspector or found)")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] ItemWindowScript itemWindowScript; // Assign in inspector if possible

    // Found dynamically after scene load
    InventoryDisplayUI inventoryDisplayUI;
    CameraControllerScript playerCameraController; // Example: Cache camera controller

    // --- Private State ---
    // private Coroutine currentTimeCoroutine; // No longer needed for instant change
    private Coroutine playtimeCoroutine; // Store reference to stop/start
    private Coroutine difficultyCoroutine; // Store reference to stop/start
    public bool isTimeSlowed = false;
    private bool isLogicPausedForLoading = false; // Flag to prevent logic during load


    // --- Unity Lifecycle Callbacks ---

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return; // Exit early if duplicate
        }
    }

    private void OnEnable()
    {
        // Subscribe to the sceneLoaded event when this object becomes active
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe when this object is disabled or destroyed to prevent memory leaks
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        // Optional: Reset time scale if the manager is destroyed unexpectedly
        // if (Instance == this) // Only reset if this *is* the singleton instance
        // {
        //     Time.timeScale = 1f;
        // }
    }

    private void Start()
    {
        // Start persistent coroutines only once
        // These might be paused/resumed during loading via Pause/ResumeLogicForLoading
        StartPersistentCoroutines();

        // Note: Finding scene-specific objects like InventoryDisplayUI is now done
        // in HandleSceneLoaded after a relevant scene loads.
    }

    // --- Public Methods ---

    /// <summary>
    /// Queues an action (method) to be executed once the *next* game level finishes loading.
    /// </summary>
    /// <param name="action">The method to execute.</param>
    public void QueueActionForNextLevel(System.Action action)
    {
        if (action != null)
        {
            OnNextLevelLoaded += action;
            Debug.Log($"Action '{action.Method.Name}' queued for next level load.");
        }
    }

    /// <summary>
    /// Call this method before initiating the loading sequence (e.g., from LevelManager).
    /// Pauses time-based logic and prevents updates during loading.
    /// </summary>
    public void PauseLogicForLoading()
    {
        isLogicPausedForLoading = true;
        isPlaytimePaused = true; // Explicitly pause playtime counter
        // Stop coroutines that shouldn't run during loading or rely on Time.deltaTime heavily
        // Check if coroutines exist before stopping
        if (playtimeCoroutine != null)
        {
            StopCoroutine(playtimeCoroutine);
            playtimeCoroutine = null; // Clear reference
        }
        if (difficultyCoroutine != null)
        {
            StopCoroutine(difficultyCoroutine);
            difficultyCoroutine = null; // Clear reference
        }
        // Add any other logic pausing needed
        Debug.Log("LogicManager paused for loading.");
    }

    /// <summary>
    /// Called internally after a new level scene loads to resume logic.
    /// </summary>
    private void ResumeLogicAfterLoading()
    {
        isLogicPausedForLoading = false;
        // Resume playtime ONLY if the game wasn't paused by the player beforehand
        // You might need more sophisticated state management for pause states
        bool isGamePausedByUser = pauseMenu != null && pauseMenu.activeSelf;
        bool isItemWindowOpen = itemWindowScript != null && itemWindowScript.isOpen;

        if (!isGamePausedByUser && !isItemWindowOpen)
        {
            isPlaytimePaused = false;
        }
        else
        {
            isPlaytimePaused = true; // Ensure playtime remains paused if game UI is open
        }

        // Restart coroutines
        StartPersistentCoroutines();
        Debug.Log("LogicManager resumed after loading.");
    }


    // --- Scene Loading Handler ---

    /// <summary>
    /// Called automatically by Unity after any scene finishes loading.
    /// </summary>
    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name} (Mode: {mode})");

        // --- Step 1: Check if it's a scene where logic should be paused/ignored ---

        string loadingSceneName = "";
        if (LevelManager.Instance != null)
        {
            // Ensure LevelManager has a public way to get the loading screen name
            // Example: public string LoadingSceneName => loadingScreenSceneName;
            loadingSceneName = LevelManager.Instance.loadingScreenSceneName;
        }
        else
        {
            // Only log warning if this is not the very first scene load potentially
            if (Time.frameCount > 1) // Avoid warning on initial game launch before LevelManager might exist
            {
                Debug.LogWarning("HandleSceneLoaded: LevelManager instance not found. Cannot check for loading screen.");
            }
            // Assume it's not the loading screen if LevelManager isn't found yet
        }


        // Ignore the loading screen itself and potentially other scenes like Main Menu
        // Ensure loadingSceneName is not empty before comparing
        if ((!string.IsNullOrEmpty(loadingSceneName) && scene.name == loadingSceneName) || scene.name == "MainMenu") // Add other non-level scenes if needed
        {
            Debug.Log($"Ignoring scene load event for utility scene: {scene.name}");
            // Ensure logic remains paused if we just loaded the loading screen
            if (!string.IsNullOrEmpty(loadingSceneName) && scene.name == loadingSceneName)
            {
                isLogicPausedForLoading = true; // Ensure it stays paused
                isPlaytimePaused = true;
                // Ensure coroutines are stopped if they somehow restarted
                if (playtimeCoroutine != null) { StopCoroutine(playtimeCoroutine); playtimeCoroutine = null; }
                if (difficultyCoroutine != null) { StopCoroutine(difficultyCoroutine); difficultyCoroutine = null; }
            }
            return; // Do nothing further for these scenes
        }

        // --- Step 2: It's a playable level scene - Resume Logic & Run Setup ---

        Debug.Log($"Running setup for level: {scene.name}");

        // Resume core logic now that the level is loaded
        ResumeLogicAfterLoading();

        // Find essential scene-specific references ONLY now
        // Use null-conditional operator ?. for safer access
        inventoryDisplayUI = GameObject.FindGameObjectWithTag("InventoryDisplay")?.GetComponent<InventoryDisplayUI>();
        if (inventoryDisplayUI == null) Debug.LogWarning($"InventoryDisplayUI not found in scene {scene.name}. Check tag and component.");

        // Example: Find player camera controller
        GameObject playerCamObj = GameObject.FindGameObjectWithTag("MainCamera"); // Or however you find it
        if (playerCamObj != null)
        {
            playerCameraController = playerCamObj.GetComponent<CameraControllerScript>();
            if (playerCameraController == null) Debug.LogWarning($"CameraControllerScript component not found on MainCamera in scene {scene.name}.");
        }
        else
        {
            Debug.LogWarning($"MainCamera object not found in scene {scene.name}. Check tag.");
        }


        // --- Step 3: Invoke and clear the queued actions ---
        if (OnNextLevelLoaded != null)
        {
            Debug.Log("Invoking OnNextLevelLoaded actions...");
            try
            {
                OnNextLevelLoaded.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error invoking OnNextLevelLoaded action: {e}");
            }
            finally
            {
                // Clear the event listeners *after* invoking, even if errors occurred
                OnNextLevelLoaded = null;
                Debug.Log("Cleared OnNextLevelLoaded actions.");
            }
        }
        else
        {
            Debug.Log("No actions queued for OnNextLevelLoaded.");
        }

        // --- Step 4: Ensure correct initial game state for the level ---
        bool isGamePausedByUser = pauseMenu != null && pauseMenu.activeSelf;
        bool isItemWindowOpen = itemWindowScript != null && itemWindowScript.isOpen;

        // Only reset time scale and cursor if the game is not supposed to be paused
        if (!isTimeSlowed && !isGamePausedByUser && !isItemWindowOpen)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // Example: Re-enable camera control if needed
            // if (playerCameraController != null) playerCameraController.camLock = false;
        }
        else if (isGamePausedByUser || isItemWindowOpen)
        {
            // If loading into a state that should be paused (e.g., returning to game with pause menu open)
            Time.timeScale = 0f; // Ensure time is paused
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // if (playerCameraController != null) playerCameraController.camLock = true;
        }
        // If isTimeSlowed is true, StartTimeSlowdown would have already set the timescale.
    }


    // --- Coroutines ---

    private void StartPersistentCoroutines()
    {
        // Start or restart coroutines if they aren't already running
        if (playtimeCoroutine == null)
        {
            playtimeCoroutine = StartCoroutine(CountPlaytime());
        }
        if (difficultyCoroutine == null)
        {
            difficultyCoroutine = StartCoroutine(CheckDifficultyIncrease());
        }
    }

    // Coroutine to count playtime
    private IEnumerator CountPlaytime()
    {
        Debug.Log("Playtime counting started/resumed.");
        while (true)
        {
            // Use unscaledDeltaTime if playtime should advance even when Time.timeScale is 0 (paused)
            // float dt = Time.unscaledDeltaTime;
            float dt = Time.deltaTime; // Affected by Time.timeScale

            if (!isPlaytimePaused && !isLogicPausedForLoading) // Check both flags
            {
                playtime += dt;
                timeSinceLastDifficultyIncrease += dt;
                timeSinceLastEnemyLevelUp += dt;
            }
            yield return null; // Wait for the next frame
        }
    }

    // Coroutine to check for difficulty increases
    private IEnumerator CheckDifficultyIncrease()
    {
        Debug.Log("Difficulty checking started/resumed.");
        while (true)
        {
            if (!isPlaytimePaused && !isLogicPausedForLoading) // Check flags
            {
                if (timeSinceLastDifficultyIncrease >= timePerDifficultyIncrease)
                {
                    IncreaseDifficulty();
                    timeSinceLastDifficultyIncrease = 0f; // Reset timer
                    timePerDifficultyIncrease += numTimeIncreaseEachTime;
                }
                if (timeSinceLastEnemyLevelUp >= timePerEnemyLevelUp)
                {
                    IncreaseEnemyLevel();
                    timeSinceLastEnemyLevelUp = 0f; // Reset timer
                }
            }
            yield return null; // Wait for the next frame
        }
    }

    // SmoothTimeScale coroutine is no longer used for instant changes
    /*
    private IEnumerator SmoothTimeScale(float start, float end, float duration)
    {
        // Ensure duration is positive to avoid division by zero
        if (duration <= 0)
        {
            Time.timeScale = end;
            yield break; // Exit if duration is invalid
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Use unscaledDeltaTime because Time.timeScale is being changed
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration); // Calculate interpolation factor
            Time.timeScale = Mathf.Lerp(start, end, t); // Apply the interpolated time scale
            yield return null; // Wait for the next frame
        }
        Time.timeScale = end; // Ensure the final value is set precisely
        // currentTimeCoroutine = null; // Clear the coroutine reference
    }
    */


    // --- Time Control ---
    #region Time Control
    public void PausePlaytime() { isPlaytimePaused = true; Debug.Log("Playtime paused."); }
    public void ResumePlaytime() { isPlaytimePaused = false; Debug.Log("Playtime resumed."); }
    public float GetPlaytime() { return playtime; }
    public string GetFormattedPlaytime()
    {
        int hours = (int)(playtime / 3600);
        int minutes = (int)((playtime % 3600) / 60);
        int seconds = (int)(playtime % 60);
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    // Call this to start slowing time instantly
    public void StartTimeSlowdown()
    {
        isTimeSlowed = true; // Set the flag to indicate time is slowed
        // Stop any previous time scale coroutine if it was somehow running (safety check)
        // if (currentTimeCoroutine != null) StopCoroutine(currentTimeCoroutine);
        Time.timeScale = targetTimeScale; // Set time scale instantly
        Debug.Log($"Time slowed instantly to: {targetTimeScale}");
    }

    // Call this to start speeding time back up instantly
    public void StartTimeSpeedUp()
    {
        isTimeSlowed = false; // Reset the flag
        // Stop any previous time scale coroutine if it was somehow running (safety check)
        // if (currentTimeCoroutine != null) StopCoroutine(currentTimeCoroutine);
        Time.timeScale = 1f; // Set time scale instantly back to normal
        Debug.Log("Time restored instantly to normal (1.0)");
    }
    #endregion

    // --- Difficulty ---
    #region Difficulty
    private void IncreaseDifficulty()
    {
        difficultyLevel++;
        Debug.Log("Difficulty Level increased to level " + difficultyLevel);
    }

    private void IncreaseEnemyLevel()
    {
        enemyLevel++; // Increase enemy level
        Debug.Log("Enemy Level increased to level " + enemyLevel);
    }
    #endregion

    // --- Pause Menu Logic ---
    // Consider refactoring this into a dedicated UIManager or Game State Manager later
    #region Pause
    public void PauseGame(bool pause)
    {
        if (pause)
        {
            // Don't allow pausing if logic is already paused for loading
            if (isLogicPausedForLoading)
            {
                Debug.LogWarning("Cannot pause game while loading is in progress.");
                return;
            }

            Time.timeScale = 0f; // Hard pause
            PausePlaytime(); // Pause playtime tracking

            if (pauseMenu != null) pauseMenu.SetActive(true); else Debug.LogWarning("Pause Menu reference not set!");
            // Ensure inventoryDisplayUI reference is valid before using
            if (inventoryDisplayUI != null) inventoryDisplayUI.ShowItemDisplay(); else Debug.LogWarning("InventoryDisplayUI reference not set in PauseGame!");
            ItemTooltipSystem.HideTooltip(); // Assuming static access

            // Unlock cursor for menu interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // Lock camera if applicable
            // if (playerCameraController != null) playerCameraController.camLock = true;
            Debug.Log("Game Paused.");
        }
        else // Unpausing
        {
            if (pauseMenu != null) pauseMenu.SetActive(false);
            // Ensure inventoryDisplayUI reference is valid before using
            if (inventoryDisplayUI != null) inventoryDisplayUI.HideItemDisplay();

            ItemTooltipSystem.HideTooltip();

            // Determine if time should resume based on other UI elements or states
            bool shouldResumeTime = (itemWindowScript == null || !itemWindowScript.isOpen);

            if (shouldResumeTime)
            {
                ResumePlaytime(); // Resume playtime tracking *before* setting timescale > 0

                // Restore time scale based on whether slow-mo was active before pausing
                if (isTimeSlowed)
                {
                    Time.timeScale = targetTimeScale; // Restore to slow-mo scale
                    Debug.Log($"Game Unpaused. Time scale restored to slow-mo: {targetTimeScale}");
                }
                else
                {
                    Time.timeScale = 1f; // Resume normal time
                    Debug.Log("Game Unpaused. Time scale restored to normal (1.0).");
                }

                // Relock cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                // Unlock camera if applicable
                // if (playerCameraController != null) playerCameraController.camLock = false;
            }
            else
            {
                // If another window (like item window) is open, keep time paused (Time.timeScale = 0f)
                // and cursor unlocked. Playtime remains paused.
                Debug.Log("Game Unpaused, but time remains 0 due to other UI elements (e.g., Item Window).");
            }
        }
    }
    #endregion

    // --- Game Over ---
    #region GameOver
    public void GameOver()
    {
        Debug.Log("Game Over!");
        PausePlaytime();
        isLogicPausedForLoading = true; // Stop updates
        StopMusic(); // Placeholder
        // Consider stopping other game systems here (e.g., player input, enemy AI)

        // Ensure time stops completely on game over
        Time.timeScale = 0f;

        StartCoroutine(LoadGameOverScreen());
    }

    private void StopMusic() { Debug.Log("Music stopped."); /* Add music logic */ }

    private IEnumerator LoadGameOverScreen()
    {
        // Use Realtime delay as Time.timeScale is 0
        yield return new WaitForSecondsRealtime(gameOverDelay);

        // Delete save file
        string savePath = System.IO.Path.Combine(Application.persistentDataPath, "playerSave.json");
        try
        {
            if (System.IO.File.Exists(savePath))
            {
                System.IO.File.Delete(savePath);
                Debug.Log("Deleted player save file.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deleting save file at {savePath}: {e.Message}");
        }

        // Reset singleton state before leaving? Generally not recommended unless
        // you have a specific reason and handle re-initialization carefully.
        // Instance = null;

        // IMPORTANT: Reset time scale *before* loading the next scene,
        // otherwise the GameOverScene might start with Time.timeScale = 0.
        Time.timeScale = 1f;

        // Load the game over scene
        SceneManager.LoadScene(gameOverSceneName);
    }
    #endregion

    // --- Stage Management ---
    #region Stage Management
    public void AdvanceStage()
    {
        // Pause logic during the transition
        PauseLogicForLoading(); // Use the same pause mechanism

        // Reset difficulty for the new stage? Seems intended.
        difficultyLevel = 1; // Reset difficulty level
        timeSinceLastDifficultyIncrease = 0f; // Reset timer for next increase
        // Keep enemy level? Or reset? Depends on design.
        // enemyLevel = 0; // Optional: Reset enemy level too

        currentStage++;
        Debug.Log("Advanced to stage " + currentStage);

        // Save player state *before* loading next level
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerSavingScript savingScript = player.GetComponent<PlayerSavingScript>();
            if (savingScript != null)
            {
                savingScript.SavePlayerToFile();
                Debug.Log("Player state saved.");
            }
            else
            {
                Debug.LogWarning("Player found, but PlayerSavingScript component is missing.");
            }
        }
        else
        {
            Debug.LogWarning("Player object not found with tag 'Player'. Cannot save state.");
        }

        // Use the LevelManager to load the next level in the sequence
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
        else
        {
            Debug.LogError("LevelManager instance not found! Cannot advance stage properly via LevelManager.");
            // Fallback? Maybe load a scene directly, but that bypasses randomization/loading screen
            // SceneManager.LoadScene("SomeFallbackScene");
            // Consider re-enabling logic if loading fails?
            ResumeLogicAfterLoading(); // Or handle error state appropriately
        }
    }
    #endregion
}
