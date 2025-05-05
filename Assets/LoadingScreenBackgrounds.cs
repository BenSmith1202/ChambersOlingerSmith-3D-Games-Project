using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenBackgrounds : MonoBehaviour
{

    public List<Sprite> loadingScreenBackgrounds;
    Image image;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        image.sprite = loadingScreenBackgrounds[Random.Range(0, loadingScreenBackgrounds.Count)];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
