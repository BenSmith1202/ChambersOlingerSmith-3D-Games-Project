using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityIconScript : MonoBehaviour
{
    Slider cooldownSlider;
    public TMP_Text timerText;
    public int timeLeft;
    
    // Start is called before the first frame update
    void Start()
    {
        cooldownSlider = GetComponent<Slider>();
        timerText.SetText("");
        cooldownSlider.value = 0;
    }

    // Update is called once per frame


    public void StartCooldown(float cooldownTime)
    {
        StartCoroutine(CooldownCoroutine(cooldownTime));
    }

    IEnumerator CooldownCoroutine(float cooldownTime)
    {
        float timeLeft = cooldownTime;
        yield return null;
        while (timeLeft > 0)
        {
            yield return null;
            timeLeft -= Time.deltaTime;
            if (timeLeft > 0)
            {
                cooldownSlider.value = (timeLeft / cooldownTime);
            }
            timerText.SetText("" + Mathf.Ceil(timeLeft));
        }
        timerText.SetText("");
        cooldownSlider.value = 0;

        //End Cooldown
    }
}
