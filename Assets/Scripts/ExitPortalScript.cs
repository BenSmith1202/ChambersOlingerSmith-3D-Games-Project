using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitPortalScript : MonoBehaviour
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

    private void OnCollisionEnter(Collision collision)
    {
        if (logicManager.objectiveComplete && collision.gameObject.GetComponent<PlayerControllerScript>() != null)
        {
            Debug.Log("You win!!");
        }

    }
}
