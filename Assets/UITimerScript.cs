using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITimerScript : MonoBehaviour
{
    TMP_Text timerText;
    LogicManager logicManager;
    public Color escapeSequenceColor = Color.red;
    // Start is called before the first frame update
    void Start()
    {
        logicManager = GameObject.FindWithTag("LogicManager").GetComponent<LogicManager>();
        timerText = GetComponent<TMP_Text>();
        
    }

    // Update is called once per frame
    void Update()
    {
        float ptime = logicManager.playtime;
        //format the time below as 00:00
        int minutes = Mathf.FloorToInt(ptime / 60);
        int seconds = Mathf.FloorToInt(ptime % 60);
        timerText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
        if (logicManager.objectiveComplete)
        {
            timerText.color = escapeSequenceColor;
        }
        else if (logicManager.isTimeSlowed)
        {
            timerText.color = Color.yellow;
        }
        else
        {
            timerText.color = Color.white;
        }
    }
}
