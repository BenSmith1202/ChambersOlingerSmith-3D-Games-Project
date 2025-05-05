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
    public float timePerDifficultyIncrease = 300f; // Initial value
    private float initialTimePerDifficultyIncrease; // Store the initial value
    public float timeSinceLastDifficultyIncrease = 0f;
    public int numTimeIncreaseEachTime;
    public float timeSinceLastEnemyLevelUp = 0f;
    public float timePerEnemyLevelUp = 60f; // Initial value
    private float initialTimePerEnemyLevelUp; // Store the initial value
    public int enemyLevel = 0;

    [Header("Game Over")]
    public float gameOverDelay = 3f;
    public string gameOverSceneName = "GameOverScene"; // Make scene name configurable
    public string winScreenSceneName = "WinScreen"; // Make scene name configurable
    public string titleScreenSceneName = "Title"; // Make scene name configurable

    [Header("Time Scale Settings")]
    // Durations are no longer used for instant change, but kept for potential future use
    [SerializeField] private float slowdownDuration = 2f;
    [SerializeField] private float targetTimeScale = 0.1f;
    [SerializeField] private float speedUpDuration = 1f;

    [Header("UI References (Assigned in Inspector or found)")]
    [SerializeField] GameObject pauseMenu; // Assign in Inspector
    [SerializeField] ItemWindowScript itemWindowScript; // Assign in inspector if possible
    [SerializeField] GameObject player; // Found dynamically

    // Found dynamically after scene load
    InventoryDisplayUI inventoryDisplayUI;
    CameraControllerScript playerCameraController; // Example: Cache camera controller

    // --- Private State ---
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

            // Store initial difficulty values for reset
            initialTimePerDifficultyIncrease = timePerDifficultyIncrease;
            initialTimePerEnemyLevelUp = timePerEnemyLevelUp;
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
    }

    private void Start()
    {
        // Start persistent coroutines initially. HandleSceneLoaded will stop them
        // immediately if the first scene is a utility scene like the Title screen.
        StartPersistentCoroutines();
        // Ensure logic starts paused if the very first scene should be ignored
        // We'll rely on HandleSceneLoaded to manage this correctly after the first scene load.
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
        // Stop coroutines and clear references so they can be restarted later
        StopPersistentCoroutines();
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

        // Restart coroutines only if logic isn't paused
        if (!isLogicPausedForLoading)
        {
            StartPersistentCoroutines();
        }
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
        string loadingSceneName = GetLoadingSceneName();
        bool isUtilityScene = IsUtilityScene(scene, loadingSceneName);

        if (isUtilityScene)
        {
            Debug.Log($"Utility scene loaded ({scene.name}). Stopping persistent coroutines and pausing logic.");
            isLogicPausedForLoading = true; // Use this flag to prevent accidental resumption
            isPlaytimePaused = true; // Ensure playtime counter is paused

            // Explicitly STOP the coroutines when entering these scenes,
            // even if Start() just began them.
            StopPersistentCoroutines();

            // Reset relevant game state when returning to the Title screen
            if (scene.name == titleScreenSceneName) // Use variable for title screen name
            {
                ResetGameStateForNewRun();
            }

            // Ensure correct cursor state and time scale for menus/utility scenes
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            return; // Do nothing further for these scenes
        }

        // --- Step 2: It's a playable level scene - Resume Logic & Run Setup ---
        Debug.Log($"Running setup for level: {scene.name}");

        // Resume core logic now that the level is loaded (restarts coroutines)
        ResumeLogicAfterLoading();

        // Find essential scene-specific references
        FindSceneReferences(scene.name);

        // Load player state *after* finding player object
        LoadPlayerState();

        // Invoke and clear the queued actions
        InvokeQueuedActions();

        // Ensure correct initial game state for the level
        EnsureCorrectLevelStartState();

        playtime = 0f; // Reset playtime for the new level
        objectiveComplete = false; // Reset objective state
    }

    /// <summary>
    /// Gets the loading screen scene name from LevelManager safely.
    /// </summary>
    private string GetLoadingSceneName()
    {
        LevelManager levelManagerInstance = LevelManager.Instance;
        if (levelManagerInstance != null)
        {
            // Ensure LevelManager has a public way to get the loading screen name
            // Example: public string LoadingSceneName => loadingScreenSceneName;
            return levelManagerInstance.loadingScreenSceneName; // Access the field directly if public/internal, or use property
        }
        else if (Time.frameCount > 1) // Avoid warning on initial game launch
        {
            Debug.LogWarning("GetLoadingSceneName: LevelManager instance not found.");
        }
        return ""; // Return empty if not found
    }

    /// <summary>
    /// Checks if the loaded scene is a utility scene where game logic shouldn't run.
    /// </summary>
    private bool IsUtilityScene(Scene scene, string loadingSceneName)
    {
        // Add all scene names that are *not* playable levels
        return scene.name == titleScreenSceneName ||
               scene.name == "IntroSlide" || // Your specific intro scene name
               scene.name == winScreenSceneName ||
               scene.name == gameOverSceneName || // Using the variable name
               (!string.IsNullOrEmpty(loadingSceneName) && scene.name == loadingSceneName);
    }

    /// <summary>
    /// Finds references to objects specific to the current scene.
    /// </summary>
    private void FindSceneReferences(string sceneName)
    {
        inventoryDisplayUI = GameObject.FindGameObjectWithTag("InventoryDisplay")?.GetComponent<InventoryDisplayUI>();
        if (inventoryDisplayUI == null) Debug.LogWarning($"InventoryDisplayUI not found in scene {sceneName}. Check tag and component.");

        pauseMenu = inventoryDisplayUI.pauseMenu;

        player = GameObject.FindGameObjectWithTag("Player"); // Find dynamically
        if (player == null) Debug.LogWarning($"Player object not found in scene {sceneName}. Check tag.");

        // Find camera controller if needed (example)
        // GameObject playerCamObj = GameObject.FindGameObjectWithTag("MainCamera");
        // if (playerCamObj != null) playerCameraController = playerCamObj.GetComponent<CameraControllerScript>();
    }

    /// <summary>
    /// Loads the player state if the player object is found.
    /// </summary>
    private void LoadPlayerState()
    {
        if (player != null)
        {
            PlayerSavingScript savingScript = player.GetComponent<PlayerSavingScript>();
            if (savingScript != null)
            {
                savingScript.LoadPlayer();
                Debug.Log("Player state loaded.");
            }
            else
            {
                Debug.LogWarning("Player found, but PlayerSavingScript component is missing. Cannot load state.");
            }
        }
        // No warning if player is null here, FindSceneReferences already warned.
    }


    /// <summary>
    /// Invokes and clears actions queued for the next level load.
    /// </summary>
    private void InvokeQueuedActions()
    {
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
                OnNextLevelLoaded = null; // Clear the event listeners
                Debug.Log("Cleared OnNextLevelLoaded actions.");
            }
        }
        else
        {
            Debug.Log("No actions queued for OnNextLevelLoaded.");
        }
    }

    /// <summary>
    /// Sets the initial TimeScale and Cursor state for a playable level.
    /// </summary>
    private void EnsureCorrectLevelStartState()
    {
        bool isGamePausedByUser = pauseMenu != null && pauseMenu.activeSelf; // Check if pause menu was somehow active
        bool isItemWindowOpen = itemWindowScript != null && itemWindowScript.isOpen;

        // Only set to playing state if not intentionally paused or in slow-mo
        if (!isTimeSlowed && !isGamePausedByUser && !isItemWindowOpen)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (isGamePausedByUser || isItemWindowOpen)
        {
            // If loading into a state that should be paused
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (isTimeSlowed)
        {
            // If loading into slow-mo state
            Time.timeScale = targetTimeScale;
            Cursor.lockState = CursorLockMode.Locked; // Usually locked during gameplay
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// Resets game state variables for starting a new run from the title screen.
    /// </summary>
    private void ResetGameStateForNewRun()
    {
        Debug.Log("Resetting game state for new run.");
        playtime = 0f;
        timeSinceLastDifficultyIncrease = 0f;
        timeSinceLastEnemyLevelUp = 0f;
        difficultyLevel = 1; // Reset to base difficulty
        enemyLevel = 0; // Reset enemy level
        currentStage = 1; // Reset stage
        objectiveComplete = false;
        isTimeSlowed = false; // Ensure time isn't slowed

        // Reset difficulty timers to initial values
        timePerDifficultyIncrease = initialTimePerDifficultyIncrease;
        timePerEnemyLevelUp = initialTimePerEnemyLevelUp;

        // Clear any queued actions from a previous run
        OnNextLevelLoaded = null;
    }


    // --- Coroutines ---

    private void StartPersistentCoroutines()
    {
        // Only start if logic is not paused and they aren't already running
        if (!isLogicPausedForLoading)
        {
            if (playtimeCoroutine == null)
            {
                playtimeCoroutine = StartCoroutine(CountPlaytime());
            }
            if (difficultyCoroutine == null)
            {
                difficultyCoroutine = StartCoroutine(CheckDifficultyIncrease());
            }
        }
    }

    /// <summary>
    /// Stops the persistent coroutines and clears their references.
    /// </summary>
    private void StopPersistentCoroutines()
    {
        if (playtimeCoroutine != null)
        {
            StopCoroutine(playtimeCoroutine);
            playtimeCoroutine = null;
            Debug.Log("Playtime counting stopped.");
        }
        if (difficultyCoroutine != null)
        {
            StopCoroutine(difficultyCoroutine);
            difficultyCoroutine = null;
            Debug.Log("Difficulty checking stopped.");
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
        Time.timeScale = targetTimeScale; // Set time scale instantly
        Debug.Log($"Time slowed instantly to: {targetTimeScale}");
    }

    // Call this to start speeding time back up instantly
    public void StartTimeSpeedUp()
    {
        isTimeSlowed = false; // Reset the flag
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

    public void IncreaseEnemyLevel()
    {
        enemyLevel++; // Increase enemy level
        Debug.Log("Enemy Level increased to level " + enemyLevel);
    }
    #endregion

    // --- Pause Menu Logic ---
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

            if (pauseMenu != null) pauseMenu.SetActive(true); else Debug.LogWarning("Pause Menu reference not set or found!");
            // Ensure inventoryDisplayUI reference is valid before using
            if (inventoryDisplayUI != null) inventoryDisplayUI.ShowItemDisplay(); else Debug.LogWarning("InventoryDisplayUI reference not set in PauseGame!");
            ItemTooltipSystem.HideTooltip(); // Assuming static access

            // Unlock cursor for menu interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
        StopPersistentCoroutines(); // Stop playtime/difficulty counting
        StopMusic(); // Placeholder

        // Ensure time stops completely on game over
        Time.timeScale = 0f;

        StartCoroutine(LoadGameOverScreen(gameOverSceneName)); // Pass the specific scene name
    }

    // --- Win Condition ---
    public void WinGame() // Example method to call when player wins
    {
        Debug.Log("You Win!");
        PausePlaytime();
        isLogicPausedForLoading = true; // Stop updates
        StopPersistentCoroutines(); // Stop playtime/difficulty counting
        StopMusic(); // Placeholder

        // Ensure time stops completely on win
        Time.timeScale = 0f;

        StartCoroutine(LoadGameOverScreen(winScreenSceneName)); // Load the win screen
    }


    private void StopMusic() { Debug.Log("Music stopped."); /* Add music logic */ }

    private IEnumerator LoadGameOverScreen(string sceneToLoad) // Takes scene name as parameter
    {
        // Use Realtime delay as Time.timeScale might be 0
        yield return new WaitForSecondsRealtime(gameOverDelay);

        // Delete save file only on actual game over (death), not on win
        if (sceneToLoad == gameOverSceneName)
        {
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
        }

        // IMPORTANT: Reset time scale *before* loading the next scene,
        // otherwise the next scene might start with Time.timeScale = 0.
        Time.timeScale = 1f;

        // Load the specified scene (Game Over or Win Screen)
        SceneManager.LoadScene(sceneToLoad);
    }
    #endregion

    // --- Stage Management ---
    #region Stage Management
    public void AdvanceStage()
    {
        // Pause logic during the transition
        PauseLogicForLoading(); // Stops coroutines

        // Reset difficulty for the new stage
        difficultyLevel = 1; // Reset difficulty level
        timeSinceLastDifficultyIncrease = 0f; // Reset timer for next increase
        currentStage++;
        Debug.Log("Advanced to stage " + currentStage);

        // Save player state *before* loading next level
        // Ensure player reference is still valid (might be destroyed/recreated)
        player = GameObject.FindGameObjectWithTag("Player"); // Re-find just in case
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
