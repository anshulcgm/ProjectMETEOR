using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity.InputModule;
public class LaserFire : MonoBehaviour {

    public GameObject cursor;

    private Vector3 initialPosition; //where the laser starts firing from
    private Vector3 destination;  //Where the laser will go before bouncing or getting destroyed
    private Vector3 laserDirection; //The direction in which the laser will travel

    private Sound_Background sound;
    
    public float speed; 

    private int bounceCount; //The number of times the laser has bounced

    private bool laserFired; //Checks if the laser is in motion

    public float particleShiftX;
    public float particleShiftY;
    public float particleShiftZ;

    //public ParticleSystem particleEffect; 
    private Rigidbody rb;
    private void Awake()
    {
        initialPosition = Camera.main.transform.position; //As the laser fires from the user's head the initial position of the laser 
        //is the position of the HoloLens Camera
        destination = GazeManager.Instance.HitPosition; //The laser will go to the position of the cursor, which is at where the user is looking at
        rb = GetComponent<Rigidbody>();
        laserDirection = (destination - initialPosition).normalized; //Gets the direction as vector with a magnitude of 1
        bounceCount = 0;
        laserFired = true;
        rb.velocity = laserDirection * speed;
        //Debug.Log("Final destination is " + destination);   
        sound = Camera.main.GetComponent<Sound_Background>();
        sound.PlayLaser();
    }

    private void FixedUpdate()
    {
    }
    private void Update()
    {
        if (Vector3.Distance(initialPosition, transform.position) >= 15.0f) //Checks if laser has gone 15 or meters from starting spot, if so the laser is destroyed
        {
            Debug.Log("Laser destroyed due to max distance");
            Destroy(gameObject);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<Enemy>() != null)
        {
            collision.gameObject.GetComponent<Enemy>().damage(50);
        }

        if (collision.gameObject.layer != 31)
             return;

        laserFired = false;
        Debug.Log("Collision detected with GameObject " +collision.gameObject.name);
        //The current direction vector is reflected to give a new direction vector in which the laser will now travel
        laserDirection = Vector3.Reflect((collision.contacts[0].point - initialPosition).normalized, collision.contacts[0].normal).normalized; 
        Debug.Log("New laser direction is " + laserDirection);
        initialPosition = collision.contacts[0].point; //The initial position of the laser is now where the laser originally collided
        bounceCount++;
        rb.velocity = laserDirection * speed;
        // Instantiate particle system
        //ParticleSystem genParticleEffect = (ParticleSystem)Instantiate(particleEffect,
                //new Vector3(collision.contacts[0].point.x + particleShiftX, collision.contacts[0].point.y + particleShiftY,
                //collision.contacts[0].point.z + particleShiftZ), collision.gameObject.transform.rotation);
        if (bounceCount > 1)
        {
            Debug.Log("Laser destroyed due to more than 1 bounces");
            Destroy(gameObject);
        }
        else
        {
            laserFired = true;
        }
    }
}


