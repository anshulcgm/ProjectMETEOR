using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarRise : MonoBehaviour
{
    public GameObject PillarMesh;
    public float netRiseSpeed = 0.0f;
    public float actualRiseSpeed = 0.0f;
    public DateTime lastRandomChange = DateTime.MinValue;
    public float secondsBetweenRandomChanges = 0.2f;
	// Use this for initialization
	void Start ()
    {
        //get a random net rising speed
        netRiseSpeed = UnityEngine.Random.Range(0.25f, 1.0f);
        //get the actual rise speed
        actualRiseSpeed = UnityEngine.Random.Range(-netRiseSpeed * 0.5f, netRiseSpeed * 0.5f) + netRiseSpeed;
        //set the last time we updated the speed to now.
        lastRandomChange = DateTime.Now;
	}
	


	// Update is called once per frame
	void Update ()
    {
        //if the pillar is at the right position, set it to the right position and return.
        if (PillarMesh.transform.position.y == transform.position.y)
        {
            PillarMesh.transform.position = transform.position;
            return;
        }

        //if enough time has elapsed, set the actual rise speed
        if((DateTime.Now - lastRandomChange).TotalSeconds > secondsBetweenRandomChanges)
        {
            actualRiseSpeed = UnityEngine.Random.Range(-netRiseSpeed * 0.5f, netRiseSpeed * 0.5f) + netRiseSpeed;
            lastRandomChange = DateTime.Now;
        }

        //if we haven't reached the correct position yet, continue to rise. Otherwise, stop.
        if (PillarMesh.transform.position.y < transform.position.y)
        {
            PillarMesh.transform.position = new Vector3(transform.position.x, PillarMesh.transform.position.y + actualRiseSpeed * Time.deltaTime, transform.position.z);
        }
        else
        {
            PillarMesh.transform.position = transform.position;
        }
	}

   
}
