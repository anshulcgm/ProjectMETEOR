using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour {
    public GameObject projectile;
    public float projectileRadius;
    public Transform target;
    public float timeBetweenLaunches = 5.0f;
    //public GameObject redOrb;


    DateTime lastLaunch = DateTime.Now;

    public float dd = 0.1f;
    public float dtheta = 0.1f;
    bool activated = true;

    void beginShooting()
    {
        lastLaunch = DateTime.Now;
        activated = true;
    }

    void stopShooting()
    {
        lastLaunch = DateTime.Now;
        activated = false;
    }

    // Use this for initialization
    void Start ()
    {
        projectileRadius = projectile.GetComponent<SphereCollider>().radius;
        Physics.gravity = new Vector3(0, -1, 0);
    }


    float error = 0f;
    // Update is called once per frame
    void Update()
    {
        if (!activated)
        {
            return;
        }

        if ((DateTime.Now - lastLaunch).TotalSeconds >= timeBetweenLaunches)
        {
            Vector3 launchVector = getLaunchVector();
            if (launchVector != Vector3.zero)
            {
                Vector3 skewedLaunchVector = launchVector + new Vector3(UnityEngine.Random.Range(-error * launchVector.magnitude * 0.5f, error * launchVector.magnitude * 0.5f),
                                                                  UnityEngine.Random.Range(-error * launchVector.magnitude * 0.5f, error * launchVector.magnitude * 0.5f),
                                                                  UnityEngine.Random.Range(-error * launchVector.magnitude * 0.5f, error * launchVector.magnitude * 0.5f));
                shoot(projectile, skewedLaunchVector);
                lastLaunch = DateTime.Now;
            }
        }
    }

    void shoot(GameObject projectile, Vector3 launchVector)
    {
        GameObject projectileInstance = Instantiate(projectile, transform.position, Quaternion.identity);
        projectileInstance.GetComponent<Rigidbody>().velocity = launchVector;
    }

    Vector3 getLaunchVector()
    {        
        Vector3 transformFlat = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetFlat = new Vector3(target.position.x, 0, target.position.z);
        float df = Vector3.Distance(targetFlat, transformFlat);
        float angleXZ = Mathf.Atan2(targetFlat.z - transformFlat.z, targetFlat.x - transformFlat.x);
        float thetaMin = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x);
        float thetaMax = Mathf.PI / 2;
        float theta0 = (thetaMax + thetaMin) / 2.0f;
        float deltaTheta = 0;
        while (deltaTheta < (thetaMax - thetaMin)/2.0f)
        {
            float thetaHigh = theta0 + deltaTheta;
            float thetaLow = theta0 - deltaTheta;            
            float speedHighSqr = (-Physics.gravity.magnitude * df * df) / (2 * Mathf.Cos(thetaHigh) * Mathf.Cos(thetaHigh) * (target.position.y - transform.position.y - df * Mathf.Tan(thetaHigh)));
            float speedLowSqr = (-Physics.gravity.magnitude * df * df) / (2 * Mathf.Cos(thetaLow) * Mathf.Cos(thetaLow) * (target.position.y - transform.position.y - df * Mathf.Tan(thetaLow)));

            if ((thetaHigh - thetaLow) <= Mathf.Epsilon)
            {
                if(speedHighSqr > 0 && TrajectoryClear((float)Math.Sqrt(speedHighSqr), angleXZ, thetaHigh, df))
                {
                    return getVectorFromTrajectory(angleXZ, thetaHigh, (float)Math.Sqrt(speedHighSqr));
                }
            }
            else
            {
                if(speedHighSqr > 0 && TrajectoryClear((float)Math.Sqrt(speedHighSqr), angleXZ, thetaHigh, df))
                {
                    return getVectorFromTrajectory(angleXZ, thetaHigh, (float)Math.Sqrt(speedHighSqr));
                }
                if(speedLowSqr > 0 && TrajectoryClear((float)Math.Sqrt(speedLowSqr), angleXZ, thetaLow, df))
                {
                    return getVectorFromTrajectory(angleXZ, thetaLow, (float)Math.Sqrt(speedLowSqr));
                }
            }
            deltaTheta += dtheta;
        }
        return Vector3.zero;
    }

    Vector3 getVectorFromTrajectory (float angleXZ, float angleY, float speed)
    {
        return new Vector3(Mathf.Cos(angleXZ) * Mathf.Cos(angleY) * speed, Mathf.Sin(angleY) * speed, Mathf.Sin(angleXZ) * Mathf.Cos(angleY) * speed);
    }

    //checks if a trajectory is clear for this projectile
    bool TrajectoryClear(float speed, float angleXZ, float angleY, float df)
    {
        float speedX0 = Mathf.Cos(angleY) * speed;
        float speedY0 = Mathf.Sin(angleY) * speed;
        float d = 0;
        Vector3 posn = transform.position;
        int origionalLayer = gameObject.layer;
        gameObject.layer = 2;
        while (d < df)
        {
            Vector3 newPosn = Vector3.zero;            
            newPosn.x = Mathf.Cos(angleXZ) * d + transform.position.x;
            newPosn.y = -0.5f * Physics.gravity.magnitude * (d / speedX0) * (d / speedX0) + speedY0 / speedX0 * d + transform.position.y;
            newPosn.z = Mathf.Sin(angleXZ) * d + transform.position.z;            
            RaycastHit hit;
            if(Physics.SphereCast(posn, projectileRadius, (newPosn - posn).normalized, out hit, Vector3.Distance(posn,newPosn)))
            {
                gameObject.layer = origionalLayer;
                return false;
            }
            posn = newPosn;
            d += dd;
        }
        Debug.Log("found trajectory");
        gameObject.layer = origionalLayer;
        return true;
    }
}
