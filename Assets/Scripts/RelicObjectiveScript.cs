using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicObjectiveScript : MonoBehaviour
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
        if (collision.gameObject.GetComponent<PlayerControllerScript>() != null)
        {
            logicManager.objectiveComplete = true;

            Debug.Log("MacGuffin obtained!");
            //play sounds or particle effects and enrage enemies
            Destroy(gameObject);
        }
    }
}
