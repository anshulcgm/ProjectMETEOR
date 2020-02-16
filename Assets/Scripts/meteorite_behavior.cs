using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meteorite_behavior : MonoBehaviour {
    float secondsToDeleteAfterHit = 0.3f;
    DateTime hitTime = DateTime.Now;
    bool hit = false;
    public GameObject explosive;
    public Sound_Background sound;

    //public GameObject statsMenu;

    public GameObject EnemyExplosion;
	// Use this for initialization
	void Start ()
    {
        sound = Camera.main.GetComponent<Sound_Background>();		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if((DateTime.Now - hitTime).TotalSeconds >= secondsToDeleteAfterHit && hit)
        {
            Destroy(gameObject);
        }
	}

    void OnCollisionEnter(Collision coll)
    {
        //ANSHUL PUT AN EXPLOSION HERE
        GameObject particleEffect = Instantiate(explosive, transform.position, Quaternion.identity);
        GameObject exp = Instantiate(EnemyExplosion, transform.position, Quaternion.identity);
        
        sound.PlayExplosion();
        //upon colliding with something, kill the child sphere and stop the particle effect. Delete the gameObject after 1 second.
        GetComponent<ParticleSystem>().Stop();
        hit = true;
        hitTime = DateTime.Now;
        
    }
}
