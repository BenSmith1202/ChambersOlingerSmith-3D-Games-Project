using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Enemy that rotates while maintaining laser beams
/// </summary>
public class RotatingLaser : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 30f;
    [Tooltip("Rotation axis (normalized)")]
    public Vector3 rotationAxis = Vector3.up;
    [Tooltip("Should rotation reverse direction periodically")]
    public bool reverseDirection = false;
    [Tooltip("Time between direction changes if reversing")]
    public float reverseInterval = 5f;

    [Header("Laser Settings")]
    [Tooltip("Array of child LaserBeam components")]
    public LaserBeam[] lasers;

    private float currentSpeed;
    private float directionChangeTimer;

    private void Awake()
    {
        // Auto-populate lasers array if empty
        if (lasers == null || lasers.Length == 0)
        {
            lasers = GetComponentsInChildren<LaserBeam>();
        }

        currentSpeed = rotationSpeed;
        directionChangeTimer = reverseInterval;
    }

    private void Update()
    {
        if (reverseDirection)
        {
            UpdateDirection();
        }

        // Apply continuous rotation
        transform.Rotate(rotationAxis, currentSpeed * Time.deltaTime);
    }

    private void UpdateDirection()
    {
        directionChangeTimer -= Time.deltaTime;
        if (directionChangeTimer <= 0f)
        {
            currentSpeed *= -1;
            directionChangeTimer = reverseInterval;
        }
    }

    private void OnValidate()
    {
        // Ensure axis is normalized
        rotationAxis = rotationAxis.normalized;

        // Clamp values
        rotationSpeed = Mathf.Abs(rotationSpeed);
        reverseInterval = Mathf.Max(0.1f, reverseInterval);
    }
}