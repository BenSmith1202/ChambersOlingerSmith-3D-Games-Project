using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controls a child laser's on/off state with timed intervals
/// </summary>
public class OnOffLaser : MonoBehaviour
{
    [Header("Timing Settings")]
    [Tooltip("Time in seconds between toggles")]
    public float toggleInterval = 3f;
    [Tooltip("Initial delay before first toggle")]
    public float initialDelay = 0f;
    [Tooltip("Random variation in toggle timing (+/- this value)")]
    public float timingVariation = 0.5f;

    [Header("Laser Reference")]
    [Tooltip("Reference to child LaserBeam component")]
    public LaserBeam controlledLaser;

    private float nextToggleTime;
    private bool isActive;

    private void Awake()
    {
        // Auto-find laser if not assigned
        if (controlledLaser == null)
        {
            controlledLaser = GetComponentInChildren<LaserBeam>();
        }

        // Initialize timing
        nextToggleTime = Time.time + initialDelay;
        isActive = controlledLaser != null ? controlledLaser.isActive : false;
    }

    private void Update()
    {
        if (Time.time >= nextToggleTime && controlledLaser != null)
        {
            ToggleLaser();
            SetNextToggleTime();
        }
    }

    private void ToggleLaser()
    {
        isActive = !isActive;
        controlledLaser.ToggleLaser(isActive);
    }

    private void SetNextToggleTime()
    {
        float variation = Random.Range(-timingVariation, timingVariation);
        nextToggleTime = Time.time + Mathf.Max(0.1f, toggleInterval + variation);
    }

    private void OnValidate()
    {
        // Clamp values in editor
        toggleInterval = Mathf.Max(0.1f, toggleInterval);
        timingVariation = Mathf.Clamp(timingVariation, 0f, toggleInterval * 0.9f);
    }
}