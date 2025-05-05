using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DifficultyCounterScript : MonoBehaviour
{
    TMP_Text difText;
    LogicManager logicManager;
    // Start is called before the first frame update
    void Start()
    {
        logicManager = GameObject.FindWithTag("LogicManager").GetComponent<LogicManager>();
        difText = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (logicManager.objectiveComplete)
        {
            difText.text = "Difficulty: ESCAPE!";
        } else
        {
            difText.text = "Difficulty: " + logicManager.enemyLevel.ToString("F1");
        }
        
    }
}
