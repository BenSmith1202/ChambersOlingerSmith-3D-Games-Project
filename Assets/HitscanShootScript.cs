using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitscanShootScript : MonoBehaviour
{
    public int gunDamage = 1;                                            // Set the number of hitpoints that this gun will take away from shot objects with a health script
    public float fireRate = 0.25f;                                        // Number in seconds which controls how often the player can fire
    public float weaponRange = 50f;                                        // Distance in Unity units over which the player can fire
    public float hitForce = 100f;                                        // Amount of force which will be added to objects with a rigidbody shot by the player

    private Camera cam;                                                // Holds a reference to the first person camera
    private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);    // WaitForSeconds object used by our ShotEffect coroutine, determines time laser line will remain visible
    private AudioSource gunAudio;                                        // Reference to the audio source which will play our shooting sound effect
    private float nextFire = 0;                                                // Float to store the time the player will be allowed to fire again, after firing



    public AudioClip shotSound;

    void Start()
    {

        // Get and store a reference to our AudioSource component
        gunAudio = GetComponent<AudioSource>();

        // Get and store a reference to our Camera by searching this GameObject and its parents
        cam = Camera.main;
    }


    public void OnShoot()
    {
        // Check if the player has pressed the fire button and if enough time has elapsed since they last fired
        if (Time.time > nextFire)
        {
            // Update the time when our player can fire next
            nextFire = Time.time + fireRate;



            // Start our ShotEffect coroutine to turn our laser line on and off
            StartCoroutine(ShotEffect());




            // Create a vector at the center of our camera's viewport
            Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));





            // Declare a raycast hit to store information about what our raycast has hit
            RaycastHit hit;





            // Check if our raycast has hit anything
            if (Physics.Raycast(rayOrigin, cam.transform.forward, out hit, weaponRange))
            {

                // Get a reference to a health script attached to the collider we hit
                ShootableBox health = hit.collider.GetComponent<ShootableBox>();




                // If there was a health script attached (should let us make this universal)
                if (health != null)
                {
                    // Call the damage function of that script, passing in our gunDamage variable
                    health.Damage(gunDamage);
                }




                // Check if the object we hit has a rigidbody attached (only if we want to deal knockback to enemies)
                if (hit.rigidbody != null)
                {
                    // Add force to the rigidbody we hit, in the direction from which it was hit
                    hit.rigidbody.AddForce(-hit.normal * hitForce);
                }




            }
            
        }





        //draw the line
        // Create a vector at the center of our camera's viewport
        Vector3 lineOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));

        // Draw a line in the Scene View  from the point lineOrigin in the direction of fpsCam.transform.forward * weaponRange, using the color green
        Debug.DrawRay(lineOrigin, cam.transform.forward * weaponRange, Color.green);


    }


    private IEnumerator ShotEffect()
    {
        // Play the shooting sound effect when we have one
        AudioSource.PlayClipAtPoint(shotSound, transform.position);


        //Wait for .07 seconds
        yield return shotDuration;

    }
}
