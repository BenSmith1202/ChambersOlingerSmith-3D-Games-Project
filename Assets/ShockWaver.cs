using System.Collections;
using System.Collections.Generic;
using UnityEngine;




/// <summary>
/// Spawns expanding ring lasers at specified positions with customizable timing
/// </summary>
public class ShockWaver : MonoBehaviour
{
    [Header("Ring Spawn Settings")]
    [Tooltip("Prefab of the ring laser to spawn")]
    public RingLaser ringLaserPrefab;
    [Tooltip("Time between ring spawn waves")]
    public float spawnInterval = 2f;
    [Tooltip("Number of rings to spawn per wave")]
    public int ringsPerWave = 3;
    [Tooltip("Delay between individual rings in a wave")]
    public float ringDelay = 0.3f;
    [Tooltip("Should spawn positions be randomized each wave")]
    public bool randomizeOrder = true;

    [Header("Spawn Positions")]
    [Tooltip("Array of spawn positions for the rings")]
    public Transform[] spawnPositions;

    [Header("Ring Configuration")]
    [Tooltip("Initial radius for spawned rings")]
    public float ringInitialRadius = 1f;
    [Tooltip("Maximum radius for spawned rings")]
    public float ringMaxRadius = 10f;
    [Tooltip("Expansion speed for spawned rings")]
    public float ringExpansionSpeed = 2f;
    [Tooltip("Lifetime for spawned rings")]
    public float ringLifetime = 3f;

    private Coroutine spawnRoutine;

    /// <summary>
    /// Starts the ring spawning process when enabled
    /// </summary>
    private void OnEnable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }
        spawnRoutine = StartCoroutine(SpawnRingsRoutine());
    }

    /// <summary>
    /// Stops the ring spawning process when disabled
    /// </summary>
    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }
    }

    /// <summary>
    /// Main coroutine that handles the wave spawning pattern
    /// </summary>
    private IEnumerator SpawnRingsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            yield return StartCoroutine(SpawnSingleWave());
        }
    }

    /// <summary>
    /// Spawns a complete wave of rings with delays between them
    /// </summary>
    private IEnumerator SpawnSingleWave()
    {
        // Create a list of positions to use for this wave
        int[] positionIndices = GetSpawnPositionIndices();

        for (int i = 0; i < Mathf.Min(ringsPerWave, spawnPositions.Length); i++)
        {
            SpawnRingAtPosition(spawnPositions[positionIndices[i]]);
            yield return new WaitForSeconds(ringDelay);
        }
    }

    /// <summary>
    /// Gets the spawn position indices, potentially randomized
    /// </summary>
    private int[] GetSpawnPositionIndices()
    {
        int[] indices = new int[spawnPositions.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = i;
        }

        if (randomizeOrder)
        {
            // Fisher-Yates shuffle
            for (int i = indices.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = indices[i];
                indices[i] = indices[j];
                indices[j] = temp;
            }
        }

        return indices;
    }

    /// <summary>
    /// Instantiates and configures a single ring at the specified position
    /// </summary>
    private void SpawnRingAtPosition(Transform spawnPosition)
    {
        if (ringLaserPrefab == null || spawnPosition == null) return;

        RingLaser newRing = Instantiate(
            ringLaserPrefab,
            spawnPosition.position,
            Quaternion.identity
        );

        // Configure the ring properties
        newRing.initialRadius = ringInitialRadius;
        newRing.maxRadius = ringMaxRadius;
        newRing.expansionSpeed = ringExpansionSpeed;
        newRing.lifetime = ringLifetime;
    }

    /// <summary>
    /// Draws gizmos in the editor to visualize spawn positions
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (spawnPositions != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform pos in spawnPositions)
            {
                if (pos != null)
                {
                    Gizmos.DrawWireSphere(pos.position, 0.5f);
                }
            }
        }
    }
}