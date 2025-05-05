using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManagerScript : MonoBehaviour
{
    // Start is called before the first frame update
    LogicManager lm;
    AudioSource audioSource;
    public AudioClip escapeMusic;
    bool escaping;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        escaping = false;
        lm = GameObject.FindWithTag("LogicManager").GetComponent<LogicManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!escaping && lm.objectiveComplete)
        {
            escaping = true;
            audioSource.Stop();
            audioSource.clip = escapeMusic;
            audioSource.Play();
        }
    }
}
