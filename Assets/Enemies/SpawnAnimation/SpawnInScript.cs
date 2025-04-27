using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpawnInScript : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float spawnAnimationDuration = 1.5f;
    [SerializeField] private float rotationSpeed = 360f; // Degrees per second

    [SerializeField] GameObject spawnParticles;

    [Header("Components")]
    [SerializeField] private MonoBehaviour[] behaviorsToDisableDuringSpawn;

    [Header("Sound")]
    [SerializeField] AudioClip spawnSound;
    AudioSource audSource;

    [SerializeField] float extraHeightForSpawnParticles = 0;

    [SerializeField] bool debugging = false;

    private void OnEnable()
    {
        audSource = GetComponent<AudioSource>();
        if(debugging)
        {
            StartSpawnSequence();
        }
    }

    public void StartSpawnSequence()
    {
        StartCoroutine(SpawnSequence());
    }

    public IEnumerator SpawnSequence()
    {
        // Disable all enemy behaviors
        foreach (MonoBehaviour behavior in behaviorsToDisableDuringSpawn)
        {
            if (behavior != null)
            {
                behavior.enabled = false;
            }
        }

        audSource.PlayOneShot(spawnSound);
        Vector3 spawnPos = new Vector3(transform.position.x, (transform.position.y + GetComponent<Collider>().bounds.extents.y * 1.2f) + extraHeightForSpawnParticles, transform.position.z);
        GameObject p = Instantiate(spawnParticles, spawnPos, Quaternion.identity);

        float colliderWidth = Mathf.Max(GetComponent<Collider>().bounds.extents.z, GetComponent<Collider>().bounds.extents.x);
        print(colliderWidth);
        p.GetComponent<SpawnParticlesScript>().SetSize(colliderWidth);

        // Wait for the spawn animation to complete
        yield return StartCoroutine(SpawnAnim());

        // Re-enable all behaviors after spawn animation completes
        Destroy(p);
        foreach (MonoBehaviour behavior in behaviorsToDisableDuringSpawn)
        {
            if (behavior != null)
            {
                behavior.enabled = true;
            }
        }
    }

    public IEnumerator SpawnAnim()
    {
        Vector3 startSize = Vector3.zero;
        Vector3 endSize = transform.localScale;

        // Store original scale and rotation
        Vector3 originalScale = endSize;
        Quaternion originalRotation = transform.rotation;

        // Set starting scale to zero
        transform.localScale = startSize;

        float elapsedTime = 0f;

        while (elapsedTime < spawnAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / spawnAnimationDuration;

            // Handle scaling
            transform.localScale = Vector3.Lerp(startSize, originalScale, progress);

            // Handle rotation around Y axis
            float currentRotationY = rotationSpeed * elapsedTime;
            transform.rotation = Quaternion.Euler(originalRotation.eulerAngles.x,
                                                 originalRotation.eulerAngles.y + currentRotationY,
                                                 originalRotation.eulerAngles.z);

            yield return null;
        }

        // Ensure final scale is exactly the target scale
        transform.localScale = originalScale;

        // Optional: Reset rotation to original if you don't want it to remain rotated
        // transform.rotation = originalRotation;
    }
}