using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HololensButton : MonoBehaviour
{
    public GameObject marker;

    public Transform currTarget = null;
    public ShootWithGaze guestureRec;
    //the distance from the delete pos at which the button is deleted.
    private static float distanceToDelete = 0.1f;
    //the transform we're anchored to
    private Transform anchor;
    //the relative Vector3 position to the transform anchor
    private Vector3 relPos;
    //the relative Vector3 position to reach when deleted, relative to the transform anchor
    private Vector3 relDeletePos;
    //whether the button has been initialized or not
    private bool initialized = false;
    //whether the button is open or closed
    public bool open = true;
    Rigidbody r;
    public bool clicked = false;
    TaskFunction taskToRun;
    TapRequest t = null;

    public void Initialize(Transform anchor, Vector3 relPos, Vector3 relDeletePos, TaskFunction taskToRun)
    {
        this.anchor = anchor;
        this.relPos = relPos;
        this.relDeletePos = relDeletePos;
        this.taskToRun = taskToRun;
        initialized = true;

        //the airtap reactor
        AirtapReactor a = gameObject.AddComponent<AirtapReactor>();
        //set the function to call when there is an airtap
        a.setAirtapFunction(new AirtapFunction(airtap));
        
    }

	// Use this for initialization
	void Start ()
    {
        r = GetComponent<Rigidbody>();
        guestureRec = GameObject.FindGameObjectWithTag("GuestureRecognizer").GetComponent<ShootWithGaze>();
    }

    void airtap()
    {
        currTarget = null;
        t = new TapRequest(this);
        guestureRec.addTapRequest(t);                
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(initialized)
        {
            //allways look towards the player. (but don't change your y-rotation)
            Vector3 lookPos = Camera.main.transform.position;
            lookPos.y = transform.position.y;
            lookAt(lookPos, 360.0f);

            
            if(open)
            {
                //if the button is open, smoothFollow to the correct position
                smoothFollowToTarget(anchor.position + relPos, 2.0f, 0.7f, 10.0f, r);

                if(t != null && t.tapReqComplete && currTarget == null)
                {
                    currTarget = Instantiate(marker, t.tapPos, Quaternion.identity).transform;      
                }

                if(currTarget != null)
                {
                    Debug.Log("running task");
                    taskToRun(currTarget);
                }
            }            
            else
            {
                //if the button is closed, smoothFollow to the delete position
                smoothFollowToTarget(anchor.position + relDeletePos, 5.0f, 2.5f, 10.0f, r);
                //if we're close enough to the delete position
                if(Vector3.Distance(transform.position, anchor.position + relDeletePos) <= distanceToDelete)
                {
                    //destroy yourself
                    Destroy(gameObject);
                }                
            }
        }
	}

    public void smoothFollowToTarget(Vector3 target, float accel, float deAccel, float maxMoveSpeed, Rigidbody r)
    {        
        //the direction we need to go in Vector3 format
        Vector3 dir = (target - transform.position).normalized;

        if(Vector3.Distance(target, transform.position) < 0.01f)
        {
            r.velocity = Vector3.zero;
            return;
        }

        //move towards target at maxMoveSpeed
        //if we are traveling at less than the max speed
        if (Mathf.Abs(r.velocity.magnitude) < maxMoveSpeed)
        {
            //add a force in the direction we want to go
            r.AddForce(dir * accel);
        }

        //add a force in the opposite direction that we are traveling - to slow the object down
        r.AddForce(-r.velocity.normalized * deAccel);
    }

    public void lookAt(Vector3 target, float rotationSpeed)
    {
        if (Vector3.Distance(target, transform.position) < 0.01)
        {
            return;
        }

        //the direction we need to look in Vector3 format
        Vector3 dir = (target - transform.position).normalized;
        //the direction we need to go in Quaternion format
        Quaternion rot = Quaternion.LookRotation(dir);

        //rotate towards the rotation we need to be at
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);
    }
}

public delegate void TaskFunction(object task);
