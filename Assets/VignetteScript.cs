using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VignetteScript : MonoBehaviour
{
    public float targetTransparency = 0;
    public float lerpSpeed = 2f; // Speed at which the transparency changes
    Image vignetteImage; // The image for the vignette effect
    // Start is called before the first frame update
    void Start()
    {
        vignetteImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        //use math.lerp to lerp the transparency value of the image from its current value to its target
        Color color = vignetteImage.color;
        color.a = Mathf.Lerp(color.a, targetTransparency, Time.deltaTime * lerpSpeed);
        vignetteImage.color = color;
    }
}
