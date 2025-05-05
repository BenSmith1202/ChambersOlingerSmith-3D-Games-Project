using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Required for OrderBy

/// <summary>
/// Manages loading levels within ordered worlds, randomizing level order within each world.
/// Includes functionality for showing a loading screen between levels.
/// </summary>
public class LevelManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    private static LevelManager _instance;
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find an existing instance in the scene
                _instance = FindObjectOfType<LevelManager>();

                // If not found, create a new GameObject and add the component
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("LevelManager");
                    _instance = singletonObject.AddComponent<LevelManager>();
                }
            }
            return _instance;
        }
    }

    // --- Inspector Variables ---
    [Tooltip("The name of the scene to use as a loading screen.")]
    [SerializeField] public string loadingScreenSceneName = "LoadingScreen"; // Make sure this scene exists and is in Build Settings

    [Tooltip("List of worlds. Order matters. Levels within each world will be randomized.")]
    [SerializeField] private List<WorldData> worlds = new List<WorldData>();

    [Tooltip("Optional: Scene to load when all worlds are completed (e.g., Main Menu, Credits). Leave empty to do nothing.")]
    [SerializeField] private string completionSceneName = "MainMenu";

    // --- State Variables ---
    private int currentWorldIndex = -1;
    private List<string> currentShuffledLevelQueue = new List<string>();
    private static string nextSceneToLoad = ""; // Static to pass scene name to the loading screen

    // --- World Data Structure ---
    [System.Serializable]
    public class WorldData
    {
        public string worldName = "New World"; // Just for organization in the Inspector
        [Tooltip("List of scene names for levels in this world.")]
        public List<string> levelSceneNames = new List<string>();
    }

    // --- Unity Lifecycle Methods ---
    private void Awake()
    {
        // Enforce Singleton Pattern & Persist Across Scenes
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Duplicate LevelManager found. Destroying this one.");
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject); // Keep the LevelManager alive between scenes
    }

    // --- Public Methods ---

    /// <summary>
    /// Starts the game by loading the first level of the first world.
    /// Call this from your Main Menu or initial setup.
    /// </summary>
    public void StartGame()
    {
        currentWorldIndex = -1; // Reset world index
        currentShuffledLevelQueue.Clear(); // Clear any previous queue
        LoadNextLevel();
    }

    /// <summary>
    /// Loads the next level in the sequence (randomized within the current world, then moves to the next world).
    /// Call this when a level is successfully completed.
    /// </summary>
    public void LoadNextLevel()
    {
        GameObject.FindWithTag("Player")?.GetComponent<PlayerSavingScript>()?.SavePlayerToFile();
        
        string sceneToLoad = GetNextLevelSceneName();

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            // Store the target scene name statically so the loading screen can access it
            nextSceneToLoad = sceneToLoad;
            // Load the loading screen first
            SceneManager.LoadScene(loadingScreenSceneName);
        }
        else
        {
            // No more levels or worlds left
            Debug.Log("All worlds completed!");
            LoadCompletionScene();
        }
    }

    /// <summary>
    /// Static method for the LoadingScreenController to call to get the target scene.
    /// </summary>
    public static string GetTargetSceneName()
    {
        return nextSceneToLoad;
    }

    // --- Private Helper Methods ---

    /// <summary>
    /// Determines the scene name of the next level to load based on current state.
    /// Handles world progression and level randomization.
    /// </summary>
    /// <returns>The scene name of the next level, or null if finished.</returns>
    private string GetNextLevelSceneName()
    {
        // If the current queue is empty, try to move to the next world
        if (currentShuffledLevelQueue == null || currentShuffledLevelQueue.Count == 0)
        {
            currentWorldIndex++; // Move to the next world index

            // Check if there are more worlds
            if (currentWorldIndex < worlds.Count)
            {
                // Check if the current world has any levels defined
                if (worlds[currentWorldIndex].levelSceneNames == null || worlds[currentWorldIndex].levelSceneNames.Count == 0)
                {
                    Debug.LogWarning($"World '{worlds[currentWorldIndex].worldName}' (Index: {currentWorldIndex}) has no levels defined. Skipping.");
                    // Recursively call to try the *next* world immediately
                    return GetNextLevelSceneName();
                }

                // Shuffle the levels for the new world
                ShuffleCurrentWorldLevels();
                Debug.Log($"Starting World: {worlds[currentWorldIndex].worldName} (Index: {currentWorldIndex})");
            }
            else
            {
                // No more worlds left
                return null; // Signal completion
            }
        }

        // If after potentially moving to a new world, the queue is still empty (e.g., skipped an empty world and reached the end)
        if (currentShuffledLevelQueue == null || currentShuffledLevelQueue.Count == 0)
        {
            return null; // Should ideally not happen if worlds have levels, but good safeguard.
        }

        // Get the next level from the front of the shuffled queue
        string nextLevel = currentShuffledLevelQueue[0];
        currentShuffledLevelQueue.RemoveAt(0); // Remove it from the queue

        Debug.Log($"Loading next level: {nextLevel} from World: {worlds[currentWorldIndex].worldName}");
        return nextLevel;
    }

    /// <summary>
    /// Shuffles the list of level scene names for the current world.
    /// Uses Fisher-Yates shuffle algorithm.
    /// </summary>
    private void ShuffleCurrentWorldLevels()
    {
        if (currentWorldIndex < 0 || currentWorldIndex >= worlds.Count)
        {
            Debug.LogError("Cannot shuffle levels: Invalid world index.");
            currentShuffledLevelQueue = new List<string>(); // Ensure it's an empty list
            return;
        }

        // Create a copy to shuffle
        currentShuffledLevelQueue = new List<string>(worlds[currentWorldIndex].levelSceneNames);

        // Fisher-Yates Shuffle
        System.Random rng = new System.Random();
        int n = currentShuffledLevelQueue.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            string value = currentShuffledLevelQueue[k];
            currentShuffledLevelQueue[k] = currentShuffledLevelQueue[n];
            currentShuffledLevelQueue[n] = value;
        }

        Debug.Log($"Shuffled levels for World {currentWorldIndex}: {string.Join(", ", currentShuffledLevelQueue)}");
    }

    /// <summary>
    /// Loads the designated completion scene (if defined).
    /// </summary>
    private void LoadCompletionScene()
    {
        if (!string.IsNullOrEmpty(completionSceneName))
        {
            // Reset state before going back (optional, depends on your game structure)
            currentWorldIndex = -1;
            currentShuffledLevelQueue.Clear();
            nextSceneToLoad = completionSceneName; // Use the same loading screen mechanism
            SceneManager.LoadScene(loadingScreenSceneName);
            // Or load directly if you don't want a loading screen before the final scene:
            // SceneManager.LoadScene(completionSceneName);
        }
        else
        {
            Debug.Log("No completion scene defined.");
            // Optionally, quit the application or return to a default state
            // Application.Quit(); // Example
        }
    }
}
