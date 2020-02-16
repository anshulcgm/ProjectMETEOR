using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestroy : MonoBehaviour {

    public float secondsToDestroy;
    DateTime start;
	// Use this for initialization
	void Start () {
        start = DateTime.Now;
	}
	
	// Update is called once per frame
	void Update () {
		if((DateTime.Now - start).TotalSeconds > secondsToDestroy)
        {
            Destroy(gameObject);
        }
	}
}
