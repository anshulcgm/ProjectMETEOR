using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//script that controls the bot's movement and look rotation.
public class Follower : MonoBehaviour
{
    public bool disabled = false;

    public bool autoInit = false;

    //the position to go to
    public Vector3 destination;

    //the pathReciever, recieves paths from navScript
    public PathReciever pathReciever;

    //the navScript, set by component outside of this script
    public NavMapper navScript;

    //the rigidbody
    public Rigidbody r;

    public List<Vector3> currentPath;

    //the position on the path
    private int posOnPath = 1;


    //whether this script was initialized or not.
    private bool initialized = false;

    float accel;
    float deAccel;
    float maxMoveSpeed;
    float rotationSpeed;
    void Start()
    {
        if (autoInit)
        {
            Initialize(navScript, 50, 25, 1, 30, false);
        }
    }

    public void Initialize(NavMapper nav, float accel, float deAccel, float maxMoveSpeed, float rotationSpeed, bool isMeteor)
    {
        navScript = nav;
        pathReciever = new PathReciever(navScript, gameObject, 10, isMeteor);
        this.accel = accel;
        this.deAccel = deAccel;
        this.maxMoveSpeed = maxMoveSpeed;
        this.rotationSpeed = rotationSpeed;
        r = GetComponent<Rigidbody>();
        initialized = true;
        Debug.Log("initialized follower");
    }

    bool havePathToDest = true;
    bool pathingOverriden = false;

    void Update()
    {
        if (!initialized)
        {
            return;
        }

        if (disabled)
        {
            return;
        }

        if (Vector3.Distance(destination, transform.position) < 0.05)
        {
            transform.position = destination;
        }

        //if the path updates
        if (pathReciever.tryUpdatePath())
        {
            currentPath = pathReciever.path;
            //check if we have a path to the destination
            havePathToDest = pathReciever.hasPathTo(destination);
            posOnPath = 2;
        }

        //follow the path
        followPath(pathReciever.path);


        //keep updating the path task.
        pathReciever.updatePathTask(transform.position, destination);


        if (pathReciever.path == null)
        {
            return;
        }






    }


    //allows the enemy to smoothly go to the target, while using physics instead of position setting 
    //(because the latter method ignores collisions)
    private void smoothFollowToTarget(Vector3 target, float accel, float deAccel, float maxMoveSpeed)
    {
        //the direction we need to go in Vector3 format
        Vector3 dir = (target - transform.position).normalized;


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

    public void smoothFollowToTarget(Vector3 target)
    {
        smoothFollowToTarget(target, accel, deAccel, maxMoveSpeed);
    }

    public void smoothFollowOverridePathFinding(Vector3 target, float accel, float deAccel, float maxMoveSpeed)
    {
        pathingOverriden = true;
        destination = target;
        smoothFollowToTarget(target, accel, deAccel, maxMoveSpeed);
    }

    public void smoothFollowOverridePathFinding(Vector3 target)
    {
        pathingOverriden = true;
        destination = target;
        smoothFollowToTarget(target, accel, deAccel, maxMoveSpeed);
    }

    //look at a position. rotates to position based on rotation speed (deg/sec)
    public void lookAt(Vector3 target)
    {
        if (Vector3.Distance(target, transform.position) == 0 || disabled)
        {
            return;
        }


        //the direction we need to look in Vector3 format
        Vector3 dir = (target - transform.position).normalized;
        //the direction we need to go in Quaternion format
        Quaternion rot = Quaternion.LookRotation(dir);

        //rotate towards the rotation we need to be at
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, 0.5f);
    }

    //follows the current path. Returns if the enemy has reached the end of the path or not.



    public bool followPath(List<Vector3> path)
    {
        if (path == null || posOnPath >= path.Count)
        {
            if (path == null) { return true; }
            if (posOnPath >= path.Count)
            {
                posOnPath = 1;
            }

            r.velocity = Vector3.zero;
            return true;
        }

        if (posOnPath < path.Count - 1 && Vector3.Distance(path[posOnPath], transform.position) <= navScript.spaceBetweenNodes / 2.0f)
        {
            posOnPath++;
        }
        smoothFollowToTarget(path[posOnPath]);

        return false;
    }

}
