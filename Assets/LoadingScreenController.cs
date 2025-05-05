using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Optional: If you have a progress bar/text
using System.Collections;
using TMPro;

/// <summary>
/// Manages the loading screen, asynchronously loads the target level,
/// and optionally displays progress.
/// </summary>
public class LoadingScreenController : MonoBehaviour
{
    [Tooltip("Optional: Slider UI element to display loading progress.")]
    [SerializeField] private Slider progressBar;
    [Tooltip("Optional: Text UI element to display loading progress percentage.")]
    [SerializeField] private TMP_Text progressText;

    private void Start()
    {
        // Get the target scene name from the LevelManager
        string targetSceneName = LevelManager.GetTargetSceneName();

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("LoadingScreenController: Target scene name is missing! Cannot load next level.");
            // Optionally load a default scene like Main Menu here as a fallback
            // SceneManager.LoadScene("MainMenu");
            return;
        }

        // Initialize UI elements if they exist
        if (progressBar != null) progressBar.value = 0;
        if (progressText != null) progressText.text = "Loading... 0%";

        // Start the asynchronous loading process
        StartCoroutine(LoadSceneAsynchronously(targetSceneName));
    }

    private IEnumerator LoadSceneAsynchronously(string sceneName)
    {
        // Start loading the scene in the background
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // Prevent the scene from activating automatically until loading is complete
        operation.allowSceneActivation = false;

        // Update progress bar/text while loading
        while (!operation.isDone)
        {
            // operation.progress value goes from 0.0 to 0.9 when loading is almost complete.
            // It reaches 1.0 only when allowSceneActivation is true.
            // We clamp the value to simulate progress up to 100%.
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
            {
                progressBar.value = progress;
            }
            if (progressText != null)
            {
                progressText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
            }

            // Check if loading is complete (progress reaches 0.9)
            if (operation.progress >= 0.9f)
            {
                // You can add a small delay here if needed, or wait for player input
                // yield return new WaitForSeconds(1f); // Example delay

                // Update UI to 100% before activation
                if (progressBar != null) progressBar.value = 1f;
                if (progressText != null) progressText.text = "Loading... 100%";

                // Allow the scene to activate
                operation.allowSceneActivation = true;
            }

            yield return null; // Wait for the next frame
        }
    }
}
