using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySeeker : Enemy
{
    public static int numSeekers = 0;
    bool addedToNumSeekers = false;
    Enemy currentTransport = null;
    Transform target = null;
    Vector3 tartgetPos = Vector3.zero;
    public AudioClip playerFoundNotification;
    private AudioSource source;
    private GameObject statsMenu;
    public static List<EnemySeeker> seekers = new List<EnemySeeker>();

    new void Start()
    {
        statsMenu = GameObject.FindGameObjectWithTag("Menu");
        base.Start();
        seekers.Add(this);
        timeTillBeaconIsSentSeconds = 0.5f;
        source = GetComponent<AudioSource>();
        numSeekers++;
    }

    public bool isValidAttackNode(Node n, object c)
    {
        if (n == null || c == null || !isValidPositionNode(n, c) || (DateTime.Now - n.lastCostTime).TotalSeconds < 2)
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
        return !Physics.Linecast(n.location, ((Transform)c).position) && hit.collider.gameObject.layer != 12 && hit.collider.gameObject.layer != 13 && distSqr > 1.5 * 1.5 && (n.location.y > ((Transform)c).position.y + 0.6f);
    }

    bool soundPlayed = false;
    new void Update()
    {
        base.Update();
        if (beaconActive && !soundPlayed && numEnemiesWithActiveBeacons == 1)
        {
            source.PlayOneShot(playerFoundNotification);
            soundPlayed = true;
        }

        if (!beaconActive)
        {
            soundPlayed = false;
        }
    }

    public override void kill()
    {
        statsMenu.GetComponent<UI>().player.killSeeker();
        numSeekers--;
        base.kill();
    }

    Node attackNode = null;
    public override void attack(object t)
    {
        //if the attack node is null or invalid
        if (attackNode == null || !isValidAttackNode(attackNode, t))
        {
            //try to get a new one
            attackNode = navScript.TryGetNode(((Transform)t).position, 5, 20, isValidAttackNode, t);
        }
        //otherwise, set the follower destination to the new attackNode.
        else
        {
            follower.destination = attackNode.location;
        }
    }

    public void transport(object task)
    {
        //if the task is not an array, or the task array length is not 2
        if (!task.GetType().IsArray || ((object[])task).Length != 2)
        {
            //input invalid, return.
            return;
        }

        object[] tasks = (object[])task;
        Enemy enemyToTransport = null;
        Vector3 targetPos = Vector3.zero;

        //if the task type is an enemy
        if (tasks[0].GetType().Equals(typeof(Enemy)))
        {
            //set the enemy to transport variable
            enemyToTransport = (Enemy)tasks[0];
        }
        else
        {
            //otherwise, return
            return;
        }

        if (tasks[1].GetType().Equals(typeof(Transform)))
        {
            transport(enemyToTransport, ((Transform)tasks[1]));
        }
        else if (tasks[1].GetType().Equals(typeof(Vector3)))
        {
            transport(enemyToTransport, ((Vector3)tasks[1]));
        }
    }

    void transport(Enemy enemyToTransport, Transform transToSend)
    {
        transport(enemyToTransport, transToSend.position);
    }

    Node transportNode = null;
    bool reachedTransportNode = false;
    bool reachedEnemy = false;
    void transport(Enemy enemyToTransport, Vector3 locToSend)
    {
        //if we're trying to transport a null enemy, return.
        if (enemyToTransport == null)
        {
            return;
        }

        //if the controller has not been assigned or is assigned to someone else, assign it to yourself.
        if (enemyToTransport.controller == null || !enemyToTransport.controller.Equals(this))
        {
            enemyToTransport.controller = this;
            return;
        }

        //if the enemy can see the location, end the controller and the transport.
        if (!Physics.Linecast(enemyToTransport.transform.position, locToSend))
        {
            return;
        }

        //if the transport node is null, get the closest valid transport node
        if (transportNode == null)
        {
            transportNode = navScript.getClosestValidNodeTo(enemyToTransport.transform.position, new isValidNode(isValid), enemyToTransport.transform.position);
            enemyToTransport.follower.destination = transportNode.location;
            return;
        }

        //make the enemy to transport move to the transport node.
        if (Vector3.Distance(enemyToTransport.transform.position, transportNode.location) < 0.1f)
        {
            reachedTransportNode = true;
            enemyToTransport.transform.position = transportNode.location;
            enemyToTransport.r.velocity = Vector3.zero;
            enemyToTransport.follower.disabled = true;
        }

        if (reachedTransportNode && !reachedEnemy)
        {
            Vector3 loc = enemyToTransport.transform.position;
            loc.y += 0.5f;
            follower.destination = loc;
            reachedEnemy = Vector3.Distance(enemyToTransport.transform.position, transportNode.location) < 0.6f;
        }

        if (reachedEnemy)
        {
            Vector3 loc = transform.position;
            loc.y -= 0.5f;
            enemyToTransport.transform.position = loc;
            follower.destination = locToSend;
        }
    }

    void endTransport(Enemy e)
    {
        transportNode = null;
        e.controller = null;
        e.follower.disabled = false;
        reachedTransportNode = false;
        reachedEnemy = false;
        follower.destination = transform.position;
    }

    //determines whether this node is valid or not.
    public bool isValid(Node n, object pos)
    {
        return n != null && !Physics.Raycast(n.location, Vector3.up, 0.75f);
    }
}
