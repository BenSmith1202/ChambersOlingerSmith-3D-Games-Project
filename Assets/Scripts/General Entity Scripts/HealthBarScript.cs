using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    public Slider healthBarSlider;
    public Image sliderFill;
    public TMP_Text currentHealthText;
    public TMP_Text maxHealthText;
    public Gradient hpGradient;
    public GameObject vignetteImage; // The image for the vignette effect
    public VignetteScript vignetteScript;
    public float lowHealthThreshold = 0.3f; // The health value at which the low health vignette appears

    // Start is called before the first frame update
    void Start()
    {
        //I removed this line of code and manually set the slider because other code would run in start before getting the slider and would try to call slider methods
        //healthBarSlider = GetComponent<Slider>();
        if (vignetteImage != null)
        {
            vignetteScript = vignetteImage.GetComponent<VignetteScript>();

        }
    }

    public void SetHP(int hp)
    {
        if(healthBarSlider != null)
        {
            healthBarSlider.value = hp;
            sliderFill.color = hpGradient.Evaluate(hp / healthBarSlider.maxValue);
        }
        else
        {
            print("health bar slider not found");
        }
        
       
        if (healthBarSlider.value <= healthBarSlider.maxValue * lowHealthThreshold)
        {

            // Show the vignette effect
            if (vignetteScript != null)
            {
                vignetteScript.targetTransparency = Mathf.Clamp01(1 - (
                    (healthBarSlider.value /
                    (healthBarSlider.maxValue * lowHealthThreshold)
                    ))); // Set the target transparency to 1 (fully visible)
            }
        }
        else
        {
            // Hide the vignette effect
            if (vignetteScript != null)
            {
                vignetteScript.targetTransparency = 0; // Set the target transparency to 0 (fully transparent)
            }
        }

        if (currentHealthText != null)
        {
            currentHealthText.SetText("" + hp);
        }
    }

    public void SetMaxHP(int maxHP)
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHP;
            sliderFill.color = hpGradient.Evaluate(healthBarSlider.value / maxHP);
        }
        else
        {
            print("health bar slider not found");
        }
        
        if(maxHealthText != null)
        {
            maxHealthText.SetText("" + maxHP);
        }
    }

}
