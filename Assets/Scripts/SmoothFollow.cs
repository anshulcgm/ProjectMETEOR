using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour {
    private Rigidbody r;
    public Transform finalPos;
	// Use this for initialization
	void Start () {
        r = GetComponent<Rigidbody>();
	}
	// Update is called once per frame
	void Update () {
        smoothFollowToTarget(finalPos.position, 1.75f, 1.75f, 1.0f);
	}
    public void smoothFollowToTarget(Vector3 target, float accel, float deAccel, float maxMoveSpeed)
    {
        //the direction we need to go in Vector3 format
        Vector3 dir = (target - transform.position).normalized;


        //move towards target at maxMoveSpeed
        //if we are traveling at less than the max speed
        if (Mathf.Abs(r.velocity.magnitude) < maxMoveSpeed)
        {
            //add a force in the direction we want to go
            r.velocity = dir * accel;
        }

        //add a force in the opposite direction that we are traveling - to slow the object down
        r.AddForce(-r.velocity.normalized * deAccel);
    }
}
