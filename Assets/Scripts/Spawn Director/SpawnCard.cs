using UnityEngine;

public class SpawnCard : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float weight = 1f;
    public float creditCost = 1f;
    public int minStage = 1;

    [Header("Enemy Type")]
    public bool isFlyer = false;
    public bool isRanged = false;

    [Header("Legacy Spawn Settings")]
    public float innerSpawnRadius;
    public float outerSpawnRadius;
}