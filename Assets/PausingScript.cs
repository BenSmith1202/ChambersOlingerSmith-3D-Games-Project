using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PausingScript : MonoBehaviour
{
    LogicManager logicManager;
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            logicManager = GameObject.FindWithTag("LogicManager").GetComponent<LogicManager>();
        }
        catch(Exception e)
        {
            logicManager = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if(logicManager != null)
        {
            logicManager.PauseGame(true);
        }
    }
}
