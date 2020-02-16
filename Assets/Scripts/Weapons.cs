using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
public class Weapons : MonoBehaviour {
    private GameObject projectile;
    private GameObject instantiatedProjectile;
    
    private Vector3 initialPosition;
    private Vector3 destination;
    private Vector3 projectileDirection;

    public float speed;

    private int bounceCount; //this field will only be used for the laser as that's the only bouncing projectile

    private bool projectileFired;

    private Rigidbody rb;
    public Weapons(int typeOfProjectile, GameObject shot)
    {
        //1 is laser
        //2 is explosive
        //3 is smoke
        projectile = shot;
        InstantiateProjectile();
        //if(typeOfProjectile == 1)
        //{
        //    InstantiateLaser();
        //}
        //else if(typeOfProjectile == 2)
        //{
        //    InstantiateExplosive();
        //}
        //else
        //{
        //    InstantiateSmoke();
        //}
        initialPosition = Camera.main.transform.position;
        destination = GazeManager.Instance.HitPosition; //The laser will go to the position of the cursor, which is at where the user is looking at
        rb = GetComponent<Rigidbody>();
        projectileDirection = (destination - initialPosition).normalized; //Gets the direction as vector with magnitude of 1
        bounceCount = 0;
        projectileFired = true;
        
    }
    private void InstantiateProjectile()
    {
        Quaternion rotationVector = new Quaternion();
        rotationVector.SetLookRotation(Camera.main.transform.forward, Camera.main.transform.up); //Sets projectile rotation to where user is looking
        rotationVector *= Quaternion.Euler(0, 90, 0); //Rotates projectile so tip is facing user
        instantiatedProjectile = Instantiate(projectile, Camera.main.transform.position, rotationVector);
    }
    private void FixedUpdate()
    {
        if (projectileFired)
        {
            rb.velocity = projectileDirection * speed;
        }
    }
    private void Update()
    {
        if(Vector3.Distance(initialPosition, transform.position) >= 15.0f)
        {
            Debug.Log("Laser destroyed due to max distance");
             
        }
    }
    private void InstantiateExplosive()
    {
        //code to instantiate explosive
    }
    private void InstantiateSmoke()
    {
        //code to instantiate smoke bomb 
    }
}
