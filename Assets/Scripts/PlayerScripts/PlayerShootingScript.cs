using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerShootingScript : MonoBehaviour
{
    GameObject cam;
    GameObject gun;
    GameObject player;
    PlayerControllerScript playerControllerScript;
    CameraControllerScript cameraControllerScript;
    EntityStats stats;
    BuffManager bman;
    public bool isShooting;

    [Header("Visuals")]
    public AudioClip gunshot;
    public AudioClip reloadSound;
    Animator gunAnimator;
    AudioSource audioSource;
    public GameObject bulletParticlePrefab;
    //public TMP_Text clipText;
    Image reloadRing;

    [Header("Shooting")]
    public float gunProcCoeff = 1.0f;
    float shootCooldown = -0.1f;
    
    int clip;
    
    public bool isReloading;

    

    public void Start()
    {
        stats = GetComponent<EntityStats>();
        bman = GetComponent<BuffManager>();
        audioSource = GetComponent<AudioSource>();
        cam = GameObject.FindWithTag("MainCamera");
        gun = GameObject.FindWithTag("PlayerGun");
        gunAnimator = gun.GetComponent<Animator>();
        reloadRing = GameObject.Find("Reload Ring").GetComponent<Image>();
        cameraControllerScript = cam.GetComponent<CameraControllerScript>();
        player = GameObject.FindWithTag("Player");
        playerControllerScript = player.GetComponent<PlayerControllerScript>();
        clip = stats.clipSize;
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
        AnimationCheck();
    }


    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started && !playerControllerScript.inputPaused)
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
        
        
        if (shootCooldown < 0 && !isReloading && clip > 0)
        {
            
            Ray ray = new Ray(cam.transform.position, cam.transform.forward * stats.getRange());
            RaycastHit hitData;
            Physics.Raycast(ray, out hitData);
            Vector3 target = hitData.point;
            if (target == Vector3.zero) //if we shoot into the void
            {
                target = cam.transform.forward * stats.getRange(); //shoot to our max range forward
            }
            else
            {
                //not shooting into void so hitting something

                if(hitData.collider.gameObject.CompareTag("Enemy"))
                {
                    // make a new attack object
                    Attack bulletHit = new Attack(gameObject, stats.getDamage(), 
                        stats.getCritChance(), stats.getKnockback(), gunProcCoeff); //Proc Coefficient of 1 means use default item odds.

                    // trigger on hit effects and multipliers
                    bman.TriggerOnHitEffects(hitData.collider.gameObject, bulletHit);

                    // apply damage
                    Debug.Log("Damage: " + bulletHit.damage);
                    hitData.collider.gameObject.GetComponent<EntityStats>().InflictDamage(bulletHit.damage);

                    //TODO: apply knockback

                }
            }

            ShootGun(target);
            
        }
    }


    // Switch animation states
    public void AnimationCheck()
    {
        if (playerControllerScript.movementState == PlayerControllerScript.MovementState.wallrunning ||
            playerControllerScript.movementState == PlayerControllerScript.MovementState.running)
        {
            if (playerControllerScript._rbody.velocity.magnitude > 0.1f)
            {
                gunAnimator.SetBool("running", true);
            }
            else
            {
                gunAnimator.SetBool("running", false);
            }
        }
        else
        {
            gunAnimator.SetBool("running", false);
        }
    }


    // plays gun Audio/Visual effects and handles ammo
    public void ShootGun(Vector3 target)
    {
        gunAnimator.SetTrigger("shoot");
        audioSource.PlayOneShot(gunshot, 0.15f);

        // Calculate the direction to the target point
        Vector3 directionToTarget = target - gun.transform.position;

        // Create bullet particle
        GameObject bulletPart = Instantiate(
            bulletParticlePrefab,
            gun.transform.position,
            Quaternion.LookRotation(directionToTarget)
        );

        Destroy(bulletPart.gameObject, 2f);
        shootCooldown = stats.getAtkDelay();

        ChangeClipAmmo(-1);
        if (clip <= 0)
        {
            StartCoroutine(ReloadCoroutine(stats.getReloadTime()));
        }
    }
    

    // ammo amount change function
    public void ChangeClipAmmo(int deltaAmmo)
    {
        clip += deltaAmmo;
        //clipText.SetText("" + clip);
    }


    // fills mag in [duration] seconds
    IEnumerator ReloadCoroutine(float duration)
    {
        audioSource.PlayOneShot(reloadSound);
        if (clip < stats.clipSize)
        {
            isReloading = true;
           
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                reloadRing.fillAmount = elapsedTime / duration;
                yield return new WaitForEndOfFrame();
                elapsedTime += Time.deltaTime;
            }

            int amountToLoad = stats.clipSize - clip;
            ChangeClipAmmo(amountToLoad);

            isReloading = false;
            reloadRing.fillAmount = 0;
        }
    }

}
