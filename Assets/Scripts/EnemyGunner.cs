using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGunner : Enemy
{
    private GameObject statsMenu;
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

    public Sound_Background soundGunner;

    float accuracy = 0.03f;

    DateTime startTime = DateTime.MinValue;

    new void Start()
    {
        statsMenu = GameObject.FindGameObjectWithTag("Menu");
        soundGunner = Camera.main.GetComponent<Sound_Background>();
        base.Start();
    }

    new void Update()
    {
        //attack(player.transform);
        base.Update();
    }
    public override void kill()
    {
        statsMenu.GetComponent<UI>().player.killGunner();
        base.kill();
    }
    Node attackNode = null;
    public override void attack(object tObj)
    {
        Transform t = (Transform)tObj;

        //if the attack node is null or invalid
        if (attackNode == null || !isValidAttackNode(attackNode, t))
        {
            //try to get a new one
            attackNode = navScript.TryGetNode(t.position, 5, 20, isValidAttackNode, t);
        }
        //otherwise, set the follower destination to the new attackNode.
        else
        {
            follower.destination = attackNode.location;
        }

        //if we have visual with the target, shoot
        if (!Physics.Linecast(transform.position, t.position))
        {
            shoot();
        }
    }

    public bool isValidAttackNode(Node n, object c)
    {
        if (n == null || c == null || !isValidPositionNode(n, c) || (DateTime.Now - n.lastCostTime).TotalSeconds < 3)
        {
            return false;
        }
        float distSqr = Vector3.SqrMagnitude(n.location - ((Transform)c).position);
        RaycastHit hit;
        Physics.Raycast(n.location, (n.location - ((Transform)c).position).normalized, out hit, Mathf.Infinity);

        if (hit.collider == null)
        {
            return !Physics.Linecast(n.location, ((Transform)c).position) && distSqr > 1.5 * 1.5 && distSqr < 2.5 * 2.5;
        }
        return !Physics.Linecast(n.location, ((Transform)c).position) && hit.collider.gameObject.layer != 12 && hit.collider.gameObject.layer != 13 && distSqr > 1.5 * 1.5 && distSqr < 2 * 2; ;
    }

    //shoots the lazers
    public void shoot()
    {
        if ((DateTime.Now - startTime).TotalSeconds >= reloadTimeSec)
        {
            startTime = DateTime.Now;
            foreach (Transform t in shotTransforms)
            {
                Vector3 rand = new Vector3(UnityEngine.Random.Range(-accuracy, accuracy), UnityEngine.Random.Range(-accuracy, accuracy), 0);
                Vector3 shotDir = (t.rotation * Vector3.forward + rand * 10).normalized;
                GameObject currLazer = Instantiate(lazer, t.position + rand, Quaternion.LookRotation(shotDir));
                currLazer.GetComponent<Lazer>().shoot(shotDir, shotSpeed, lazerDamage, gameObject);
                soundGunner.PlayGunnerLaser();
            }
        }
    }
}
