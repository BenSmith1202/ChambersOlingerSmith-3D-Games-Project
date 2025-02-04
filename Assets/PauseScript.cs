using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseScript : MonoBehaviour
{
    public bool timeWasStopped = false;
    public bool isPaused = false;
    public GameObject PauseMenuCanvas;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnPause(InputValue value)
    {
        if (value.isPressed)
        {
            

            if (!isPaused)
            {
                if (Time.timeScale > 0)
                {
                    timeWasStopped = false;
                }
                else
                {
                    timeWasStopped = true;
                }
                PauseGame();
            } else
            {
                UnpauseGame();
            }

        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        PauseMenuCanvas.SetActive(true);
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UnpauseGame()
    {
        if (!timeWasStopped) // in case some other method pauses time (item pickups etc.)
        {
            Time.timeScale = 1;
        }
        PauseMenuCanvas.SetActive(false);
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }
}
