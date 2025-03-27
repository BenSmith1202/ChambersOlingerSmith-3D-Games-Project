using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogicManager : MonoBehaviour
{
    // Singleton instance
    public static LogicManager Instance { get; private set; }

    public bool objectiveComplete = false;

    [Header("Game State")]
    public int currentStage = 1; // Current stage of the game
    public float difficultyLevel = 1f; // Current difficulty level

    [Header("Playtime Tracking")]
    private float playtime = 0f; // Total playtime in seconds
    private bool isPlaytimePaused = false; // Whether the playtime counter is paused

    [Header("Difficulty Scaling")]
    public float timePerDifficultyIncrease = 300f; // Time (in seconds) before difficulty increases
    private float timeSinceLastDifficultyIncrease = 0f; // Tracks time since last difficulty increase

    [Header("Game Over")]
    public float gameOverDelay = 3f; // Time to wait before loading the game over screen


    private void Awake()
    {
        // Ensure only one instance of LogicManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    private void Start()
    {
        StartCoroutine(CountPlaytime()); // Start tracking playtime
        StartCoroutine(CheckDifficultyIncrease()); // Start checking for difficulty increases

    }









    // TIME
    #region




    // Coroutine to count playtime
    private IEnumerator CountPlaytime()
    {
        while (true)
        {
            if (!isPlaytimePaused)
            {
                playtime += Time.deltaTime; // Increment playtime
            }
            yield return null; // Wait for the next frame
        }
    }


    // Pause the playtime counter
    public void PausePlaytime()
    {
        isPlaytimePaused = true;
        Debug.Log("Playtime paused.");
    }

    // Resume the playtime counter
    public void ResumePlaytime()
    {
        isPlaytimePaused = false;
        Debug.Log("Playtime resumed.");
    }

    // Get the total playtime in seconds
    public float GetPlaytime()
    {
        return playtime;
    }

    // Format playtime into a readable string (HH:MM:SS)
    public string GetFormattedPlaytime()
    {
        int hours = (int)(playtime / 3600);
        int minutes = (int)((playtime % 3600) / 60);
        int seconds = (int)(playtime % 60);
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    // Increase the current stage
    public void AdvanceStage()
    {
        currentStage++;
        Debug.Log("Advanced to stage " + currentStage);
        //SceneManager.LoadScene(currentStage + 1);
    }



    #endregion


















    // Difficulty
    #region

    // Coroutine to check for difficulty increases
    private IEnumerator CheckDifficultyIncrease()
    {
        while (true)
        {
            if (timeSinceLastDifficultyIncrease >= timePerDifficultyIncrease)
            {
                IncreaseDifficulty();
                timeSinceLastDifficultyIncrease = 0f; // Reset the timer
            }
            yield return null; // Wait for the next frame
        }
    }

    // Increase the difficulty level
    private void IncreaseDifficulty()
    {
        difficultyLevel++;
        Debug.Log("Difficulty increased to level " + difficultyLevel);
    }


    #endregion















    //GameOver
    #region

    // Handle game over
    public void GameOver()
    {
        Debug.Log("Game Over!");
        PausePlaytime(); // Pause playtime tracking
        StopMusic(); // Stop the music (placeholder for now)
        StartCoroutine(LoadGameOverScreen()); // Load the game over screen after a delay
    }

    // Stop the music (placeholder for now)
    private void StopMusic()
    {
        Debug.Log("Music stopped.");
        // Add music stopping logic here later
    }

    // Coroutine to load the game over screen after a delay
    private IEnumerator LoadGameOverScreen()
    {
        yield return new WaitForSeconds(gameOverDelay);
        SceneManager.LoadScene("GameOverScene"); // Replace with your game over scene name
    }
    #endregion

















}
