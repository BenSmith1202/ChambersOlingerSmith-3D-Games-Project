using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    Slider healthBarSlider;
    public Image sliderFill;
    public TMP_Text currentHealthText;
    public TMP_Text maxHealthText;
    public Gradient hpGradient;

    

    // Start is called before the first frame update
    void Start()
    {
        healthBarSlider = GetComponent<Slider>();
    }

    public void SetHP(int hp)
    {
        healthBarSlider.value = hp;
        sliderFill.color = hpGradient.Evaluate(hp / healthBarSlider.maxValue);
        currentHealthText.SetText("" + hp);
    }

    public void SetMaxHP(int maxHP)
    {
        healthBarSlider.maxValue = maxHP;
        sliderFill.color = hpGradient.Evaluate(healthBarSlider.value / maxHP);
        maxHealthText.SetText("" + maxHP);
    }

}
