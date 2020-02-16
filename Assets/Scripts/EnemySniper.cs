using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EnemySniper : Enemy
{
    Vector3[] orgShieldPositions;
    Quaternion orgShieldParentAngle;
    Quaternion[] orgShieldAngles;

    public GameObject shieldParent;
    //the shields
    public GameObject[] shields;
    //the places to shoot from
    public Transform[] shotTransforms;
    //the speed to shoot with
    public float shotSpeed;
    //the lazer gameObject
    public GameObject lazer;
    //the damage on the lazer
    public float lazerDamage;
    //time between shots seconds
    public float reloadTimeSec;

    public float shieldRotationSpeed = 120.0f;
    private GameObject statsMenu;
    DateTime startTime = DateTime.MinValue;

    new void Start()
    {
        statsMenu = GameObject.FindGameObjectWithTag("Menu");
        base.Start();

        orgShieldAngles = new Quaternion[shields.Length];
        orgShieldPositions = new Vector3[shields.Length];
        orgShieldParentAngle = shieldParent.transform.rotation;

        for (int i = 0; i < shields.Length; i++)
        {
            orgShieldPositions[i] = shields[i].transform.position;
            orgShieldAngles[i] = shields[i].transform.rotation;
        }
    }

    new void Update()
    {
        //attack(player.transform);
        base.Update();
    }
    public override void kill()
    {
        statsMenu.GetComponent<UI>().player.killSniper();
        base.kill();
    }
    Node retreatNode = null;
    Node attackNode = null;

    public override void attack(object tObj)
    {
        if (attackNode != null)
        {
            foreach (Node n in attackNode.connectedNodes)
            {
                if (n != null)
                {
                    Debug.DrawLine(n.location, attackNode.location, Color.red, 1.0f);
                }
            }
        }

        Transform t = (Transform)tObj;
        //if we have visual with the target, shoot
        foreach (Transform shotTransform in shotTransforms)
        {
            if (!Physics.Linecast(shotTransform.position, t.position))
            {
                shoot();
            }
        }

        //if we're below 30% health, and there are other bots tracking the player position
        if (health < maxHealth * 0.3 && (numEnemiesWithActiveBeacons > 1 || numEnemiesWithActiveBeacons == 1 && !beaconActive))
        {
            rotateShields();
            //if the retreat node is null or invalid
            if (retreatNode == null || !isValidRetreatNode(retreatNode, t))
            {
                //try to get a new one
                retreatNode = navScript.TryGetNode(t.position, 30, 0, isValidRetreatNode, t);
            }
            //otherwise, set the follower destination to the new retreatNode.
            else
            {
                follower.destination = retreatNode.location;
            }
            return;
        }

        //if we are far enough from the player, restore the shields
        if (Vector3.Distance(t.position, transform.position) > 2.5)
        {
            restoreShields();
        }
        //if we are too close to the player, begin rotating shields.
        else
        {
            rotateShields();
        }

        //if the attack node is null or invalid
        if (attackNode == null || !isValidAttackNode(attackNode, t))
        {
            //try to get a new one

            attackNode = navScript.TryGetNode(t.position, 30, 0, isValidAttackNode, t);

        }
        //otherwise, set the follower destination to the new attackNode.
        else
        {
            follower.destination = attackNode.location;
        }
    }


    public void rotateShields()
    {
        Quaternion rot = Quaternion.Euler(0, 0, 0);
        foreach (GameObject shield in shields)
        {
            if (Quaternion.Angle(shield.transform.localRotation, rot) > 1)
                shield.transform.localRotation = Quaternion.Lerp(transform.rotation, rot, shieldRotationSpeed * Time.deltaTime);
        }
        shieldParent.transform.Rotate(new Vector3(0, shieldRotationSpeed * Time.deltaTime, 0));
    }

    public void restoreShields()
    {
        for (int i = 0; i < shields.Length; i++)
        {
            if (Quaternion.Angle(shields[i].transform.localRotation, orgShieldAngles[i]) > 1)
                shields[i].transform.localRotation = Quaternion.Lerp(transform.rotation, orgShieldAngles[i], shieldRotationSpeed * Time.deltaTime);
        }
        if (Quaternion.Angle(shieldParent.transform.localRotation, orgShieldParentAngle) > 1)
            shieldParent.transform.localRotation = Quaternion.Lerp(transform.rotation, orgShieldParentAngle, shieldRotationSpeed * Time.deltaTime);
    }

    public bool isValidAttackNode(Node n, object c)
    {
        if (n == null || c == null || !isValidPositionNode(n, c) || (DateTime.Now - n.lastCostTime).TotalSeconds < 10)
        {
            return false;
        }

        float distSqr = Vector3.SqrMagnitude(n.location - ((Transform)c).position);
        RaycastHit hit;
        Physics.Raycast(n.location, (n.location - ((Transform)c).position).normalized, out hit, Mathf.Infinity);

        if (hit.collider == null)
        {
            return !Physics.Linecast(n.location, ((Transform)c).position) && distSqr > 1.5 * 1.5;
        }
        return !Physics.Linecast(n.location, ((Transform)c).position) && hit.collider.gameObject.layer != 12 && hit.collider.gameObject.layer != 13 && distSqr > 1.5 * 1.5;
    }

    public bool isValidRetreatNode(Node n, object c)
    {
        if (n == null || c == null || !enemyManager.isValidPositionNode(n, c) || (DateTime.Now - n.lastCostTime).TotalSeconds < 10)
        {
            return false;
        }

        float dist = Vector3.Distance(n.location, ((Transform)c).position);
        return Physics.Linecast(n.location, ((Transform)c).position, navScript.collisionLayer);
    }

    //shoots the lazers
    public void shoot()
    {
        if ((DateTime.Now - startTime).TotalSeconds >= reloadTimeSec)
        {
            startTime = DateTime.Now;
            foreach (Transform t in shotTransforms)
            {
                Vector3 shotDir = t.rotation * Vector3.forward;
                GameObject currLazer = Instantiate(lazer, t.position, t.rotation);
                currLazer.GetComponent<Lazer>().shoot(shotDir, shotSpeed, lazerDamage, gameObject);
            }
        }
    }
}



