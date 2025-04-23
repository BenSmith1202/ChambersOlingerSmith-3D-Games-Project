using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAtPlayer : MonoBehaviour
{
    [Header("Tracking Settings")]
    [Tooltip("How fast the object rotates to face the player (degrees per second)")]
    public float rotationSpeed = 90f;
    [Tooltip("How often the player's position is updated (seconds)")]
    public float updateInterval = 0.2f;
    [Tooltip("Whether to only rotate on the Y axis (for ground-based enemies)")]
    public bool yAxisOnly = true;

    private GameObject player;
    private Vector3 lastPlayerPosition;
    private Coroutine updateCoroutine;

    private void Start()
    {
        // Find player on start
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("No GameObject with tag 'Player' found in scene!");
            return;
        }

        // Start the position update routine
        updateCoroutine = StartCoroutine(UpdatePlayerPositionRoutine());
    }

    private void OnDisable()
    {
        // Clean up coroutine if disabled
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Calculate target direction
        Vector3 targetDirection = lastPlayerPosition - transform.position;

        // Optionally flatten to Y axis only
        if (yAxisOnly)
        {
            targetDirection.y = 0;
        }

        // Only rotate if we have a valid direction
        if (targetDirection != Vector3.zero)
        {
            // Calculate target rotation
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // Smoothly rotate towards target
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// Coroutine that updates the player's position at regular intervals
    /// </summary>
    private IEnumerator UpdatePlayerPositionRoutine()
    {
        while (true)
        {
            if (player != null)
            {
                lastPlayerPosition = player.transform.position;
            }
            yield return new WaitForSeconds(updateInterval);
        }
    }
}
