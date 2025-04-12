using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuScript : MonoBehaviour
{

    LogicManager logicManager;
    // Start is called before the first frame update
    void Start()
    {
        logicManager = GameObject.FindWithTag("LogicManager").GetComponent<LogicManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResumeGame()
    {
        logicManager.PauseGame(false);
    }

    public void QuitToTitle()
    {
        Time.timeScale = 1f;
        // Load the title scene (assuming it's named "TitleScene")
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
