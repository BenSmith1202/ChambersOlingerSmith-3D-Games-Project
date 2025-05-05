using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO; // Required for File operations

public class MenuManagerScript : MonoBehaviour
{
    // Reference to the scene name stored in LogicManager (optional but recommended)
    // You might need to ensure LogicManager exists before calling ReturnToMenu
    // or just stick to loading by index 0 if that's reliable for you.
    // private string titleSceneName => LogicManager.Instance != null ? LogicManager.Instance.titleScreenSceneName : "Title"; // Example
    public string specificSceneOptional = "";
    void Start()
    {
        // Ensure cursor is visible and unlocked in the menu
        Time.timeScale = 1f; // Ensure time scale is normal in menus
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Optional: Ensure LogicManager is paused if returning from gameplay
        // This is mostly handled by LogicManager's HandleSceneLoaded now,
        // but an extra check here doesn't hurt.
        if (LogicManager.Instance != null)
        {
            // LogicManager.HandleSceneLoaded should take care of stopping coroutines
            // based on the scene name check.
        }
    }

    /// <summary>
    /// Called by the "Start Game" button.
    /// Deletes any existing save file and starts the game flow via LevelManager.
    /// </summary>
    public void StartGameFlow() // Renamed to avoid confusion if you had other start methods
    {
        Debug.Log("StartGameFlow called.");
        // Optional: Delete player save for a fresh run
        string savePath = Path.Combine(Application.persistentDataPath, "playerSave.json");
        try
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("Deleted previous player save file.");
            }
        }
        catch (IOException e)
        {
            Debug.LogError($"Error deleting save file at {savePath}: {e.Message}");
        }

        // Start the game using the Level Manager
        if (LevelManager.Instance != null)
        {
            // LevelManager.StartGame() will handle resetting its state
            // and loading the first level via the loading screen.
            // LogicManager's HandleSceneLoaded will detect the level load
            // and resume/start its coroutines.
            LevelManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("MenuManagerScript: LevelManager instance not found! Cannot start game.");
            // Handle error: maybe show a message to the user or load a fallback scene?
        }
    }


    public void LoadSpecificScene()
    {
        if (!string.IsNullOrEmpty(specificSceneOptional))
        {
            Debug.Log($"Loading specific scene: {specificSceneOptional}");
            SceneManager.LoadScene(specificSceneOptional);
        }
        else
        {
            Debug.LogError("LoadSpecificScene: sceneName is null or empty!");
        }
    }


    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting application...");
        Application.Quit();

        // If running in the Unity Editor, Application.Quit() might not work as expected.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    /// Returns to the main menu scene (typically build index 0 or a specific name).
    /// Called from Pause Menu, Game Over Screen, Win Screen etc.
    /// </summary>
    public void ReturnToMenu()
    {
        Debug.Log("Returning to Title Screen...");
        // Option 1: Load by index (Simpler if index 0 is always Title)
        SceneManager.LoadScene(0);

        // Option 2: Load by name (More robust, uses name from LogicManager)
        // if (LogicManager.Instance != null)
        // {
        //     SceneManager.LoadScene(LogicManager.Instance.titleScreenSceneName);
        // }
        // else
        // {
        //     Debug.LogError("ReturnToMenu: LogicManager instance not found. Loading scene index 0 as fallback.");
        //     SceneManager.LoadScene(0); // Fallback
        // }
    }

    // Removed the problematic NextScene() method that used buildIndex + 1
    // Removed the duplicate empty StartGame() method
}
