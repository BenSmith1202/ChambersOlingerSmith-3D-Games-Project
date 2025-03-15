using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerShootingScript : MonoBehaviour
{
    GameObject cam;
    CameraControllerScript cameraControllerScript;
    public float gunRange = 100f;
    public bool isShooting;

    [Header("Visuals")]
    public AudioClip gunshot;
    public AudioClip reloadSound;
    Animator animator;
    AudioSource audioSource;
    public GameObject bulletParticlePrefab;
    //public TMP_Text clipText;
    Image reloadRing;

    [Header("Shooting")]
    public float shootCooldownTime;
    public float shootCooldown = 0;
    public int clipSize;
    public int clip;
    public float reloadTime;
    public bool isReloading;

    public void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        cam = GameObject.FindWithTag("MainCamera");
        reloadRing = GameObject.Find("Reload Ring").GetComponent<Image>();
        cameraControllerScript = cam.GetComponent<CameraControllerScript>();
        clip = clipSize;
        //clipText.SetText("" + clip);
    }

    public void Update()
    {
        if (shootCooldown > 0)
        {
            shootCooldown -= Time.deltaTime;
        }
        if (isShooting)
        {
            ShootingCheck();
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isShooting = true;
        }
        else if (context.canceled)
        {
            isShooting = false;
        }
    }


    public void ShootingCheck()
    {
        //Write a debug message that outputs the values of the conditions below
        Debug.Log("Cooldown: " + shootCooldown + " Reloading: " + isReloading + " Clip: " + clip);
        if (shootCooldown < 0 && !isReloading && clip > 0)
        {
            Debug.Log("Shooting");
            Ray ray = new Ray(cam.transform.position, cam.transform.forward * gunRange);
            RaycastHit hitData;
            Physics.Raycast(ray, out hitData);
            Vector3 target = hitData.point;
            if (target == Vector3.zero) //if we shoot into the void
            {
                target = cam.transform.forward * gunRange; //shoot to our max range forward
            }
            else
            {
                Debug.Log("Hit: " + hitData.collider.name);
            }

            ShootGun();
            shootCooldown = shootCooldownTime;
        }
    }


    public void ShootGun()
    {
        audioSource.PlayOneShot(gunshot, 0.15f);

        // Create bullet particle
        Vector3 spawnOffset = cam.transform.right * 0.2f + cam.transform.up * - 0.25f + cam.transform.forward * -0.5f;
        GameObject bulletPart = Instantiate(
            bulletParticlePrefab,
            cam.transform.position + spawnOffset,
            Quaternion.LookRotation(cam.transform.forward, Vector3.up)
        );

        Destroy(bulletPart.gameObject, 2f);
        shootCooldown = shootCooldownTime;

        ChangeClipAmmo(-1);
        if (clip <= 0)
        {
            StartCoroutine(ReloadCoroutine(reloadTime));
        }
    }

    // ammo update function

    public void ChangeClipAmmo(int deltaAmmo)
    {
        clip += deltaAmmo;
        //clipText.SetText("" + clip);
    }

    IEnumerator ReloadCoroutine(float duration)
    {
        audioSource.PlayOneShot(reloadSound);
        if (clip < clipSize)
        {
            isReloading = true;
           
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                reloadRing.fillAmount = elapsedTime / duration;
                yield return new WaitForEndOfFrame();
                elapsedTime += Time.deltaTime;
            }

            int amountToLoad = clipSize - clip;
            ChangeClipAmmo(amountToLoad);

            isReloading = false;
            reloadRing.fillAmount = 0;
        }
    }

}
