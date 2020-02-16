using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IComparable<Enemy>
{

    #region fields
    public Project currProject = null;
    //the buttons that will be instantiated
    public GameObject[] buttonTemplates;
    //the actual buttons
    public GameObject[] buttons = new GameObject[2];

    //whether the button menu is open or not.
    public bool buttonsOpen = false;

    //whether the bot is being controlled or not.
    public Enemy controller = null;

    //the size of the sphere collider for the bots
    public static float colliderRadius = 0.5f;
    //the number of enemies who have active beacons
    public static int numEnemiesWithActiveBeacons = 0;

    public static int numEnemiesAttackingPlayer = 0;

    public static DateTime lastSeenTime;
    //the last seen pos of the player
    public static Vector3 playerLastSeen = new Vector3(0, 0, 0);
    //if the player pos has been locked by any one of the bots
    public static bool playerPosLocked = false;
    //the number of enemies
    public static int numEnemies = 0;

    DateTime personalLastSeenTime;
    Vector3 personalPlayerLastSeen = new Vector3(0, 0, 0);
    //the player, set by component outside of this script
    public GameObject player;

    //the number that identifies the enemy
    public int enemyID;

    public Sound_Background sound;
    //has the player been seen
    public bool playerSeen;

    //values that determine movement/rotation speeds and accels
    public float maxMoveSpeed;
    public float accel;
    public float deAccel;
    public float rotationSpeed;
    public float scanningRotationSpeed;

    //angle of sight in degrees
    public float angleOfSightDeg = 60;

    public float health = 100;
    public float maxHealth = 100;
    public float regen = 0.1f;

    //time until beacon is sent to all bots
    public float timeTillBeaconIsSentSeconds = 3;
    //the starting time at which the player was seen.
    DateTime startSeen = DateTime.MaxValue;

    //is this bot's beacon active?
    public bool beaconActive = false;

    //the current position on the path it
    public int posOnPath = 1;
    bool pathJustUpdated = false;

    //the rigidbody of the gameObject that contains this component, or a child of this component
    public Rigidbody r = null;

    public Follower follower;

    public PathReciever pathReciever;

    public NavMapper navScript;

    public EnemyAction currentAction = EnemyAction.IDLE;

    public EnemyManager enemyManager;
    #endregion

    public void Start()
    {
        //get the rigidbody
        r = gameObject.GetComponent<Rigidbody>();

        //set the enemyID (make it unique)
        enemyID = numEnemies;
        numEnemies++;

        //the airtap reactor
        AirtapReactor a = gameObject.AddComponent<AirtapReactor>();
        //set the function to call when there is an airtap
        a.setAirtapFunction(new AirtapFunction(switchButtons));
        sound = Camera.main.GetComponent<Sound_Background>();
        follower.Initialize(navScript, accel, deAccel, maxMoveSpeed, rotationSpeed, false);
    }

    //switches the buttons from out to in.
    public void switchButtons()
    {
        Debug.Log("switching buttons");
        if (buttonsOpen)
        {
            Debug.Log("destroying buttons");
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].GetComponent<HololensButton>().open = false;
                buttons[i] = null;
            }
        }
        else
        {
            Debug.Log("creating buttons");
            buttons = new GameObject[buttonTemplates.Length];
            buttons[0] = Instantiate(buttonTemplates[0], transform.position, Quaternion.identity);
            HololensButton button = buttons[0].GetComponent<HololensButton>();
            button.Initialize(transform, new Vector3(0, 0.2f, 0), new Vector3(0, 0, 0), new TaskFunction(attack));
            button.open = true;

            buttons[1] = Instantiate(buttonTemplates[1], transform.position, Quaternion.identity);
            HololensButton button1 = buttons[1].GetComponent<HololensButton>();
            button1.Initialize(transform, new Vector3(0, 0.5f, 0), new Vector3(0, 0, 0), new TaskFunction(move));
            button1.open = true;
        }
        buttonsOpen = !buttonsOpen;
    }


    bool switchedStates = false;
    bool sentLastSeen = true;
    bool firstSighting = true;

    bool added = false;
    Node prevNode = null;

    public bool activated = true;

    Node closestVisibleNode = null;
    public void Update()
    {
        if (!activated)
        {
            return;
        }

        //if we're off map, find the closest valid node and move towards it
        if (navScript.getClosestNodeTo(transform.position) == null && closestVisibleNode == null)
        {
            float minDistSqr = float.MaxValue;
            closestVisibleNode = null;
            foreach (Node n in navScript.allValidNodes)
            {
                if (!Physics.Linecast(transform.position, n.location, navScript.collisionLayer))
                {
                    float distSqr = Vector3.SqrMagnitude(n.location - transform.position);
                    if (minDistSqr > distSqr)
                    {
                        closestVisibleNode = n;
                        minDistSqr = distSqr;
                    }
                }
            }
            return;
        }
        else if (navScript.getClosestNodeTo(transform.position) == null && closestVisibleNode != null)
        {
            follower.smoothFollowOverridePathFinding(closestVisibleNode.location);
            return;
        }


        //always look at the player
        follower.lookAt(player.transform.position);



        //if someone else is directly controlling this bot
        if (controller != null)
        {
            //don't do any of the behaviors
            return;
        }
        //if we're in test mode, don't execute normal behaviors.
        if (ShootWithGaze.inTestMode)
        {
            return;
        }

        invalidateSelfNode();


        //if we're on a reserved node, set the destination to the closest non-reserved node
        Node currentNode = navScript.getClosestNodeTo(transform.position, true);
        /*            
        if (currentNode != null && currentNode.reservers != null && ((currentNode.reservers.Count > 1)||(currentNode.reservers.Count > 0 && !currentNode.reservers.Contains(this))))
        {
            follower.destination = navScript.getClosestValidNodeTo(transform.position, new isValidNode(isNotReserved), null).location;
        }
        */
        healthHandler();
        beaconHandler();
        actionManager();
        currentAction = currProject.action;
        prevNode = currentNode;

        if (Vector3.SqrMagnitude(transform.position) > 100 * 100)
        {
            kill();
        }
    }

    bool isNotReserved(Node n, object c)
    {
        return n != null && !n.addedToLine;
    }

    #region stateManagerAndBehaviors

    public void actionManager()
    {
        if (currProject == null)
        {
            return;
        }
        if (currentAction != currProject.action && currentAction == EnemyAction.SEARCH)
        {
            endSearch();
        }

        if (currProject.action == EnemyAction.IDLE)
        {
            idle();
        }
        if (currProject.action == EnemyAction.ATTACK)
        {
            attack(currProject.task);
        }
        if (currProject.action == EnemyAction.SEARCH)
        {
            search(currProject.task);
        }
    }

    //attacks the transform
    public virtual void attack(object t)
    {
        //do the attack
    }

    public virtual void move(object task)
    {
        follower.destination = ((Transform)task).position;
    }

    Node closestNode = null;
    Node currentNode = null;
    bool reachedArea = false;
    //searches a list of Nodes
    public virtual void search(object areaObj)
    {
        Area area = (Area)areaObj;

        if (area == null)
        {
            return;
        }

        //search the nodes!

        //if we don't know the closest node, get it.       
        if (closestNode == null)
        {
            closestNode = findClosestNode(area, false);
        }

        //couldn't find the closest Node...
        if (closestNode == null)
        {
            return;
        }

        //if we haven't reached the area yet
        if (!reachedArea)
        {
            follower.destination = closestNode.location;
            //if we're close enough to the closest node
            if (Vector3.Distance(transform.position, closestNode.location) < navScript.spaceBetweenNodes)
            {
                //we've reached the area
                r.velocity = Vector3.zero;
                reachedArea = true;
                currentNode = closestNode;
                area.search(currentNode);
            }
        }
        else
        {
            //if all our neighbors are searched, find the closest node that hasn't been searched
            if (currentNode == null)
            {
                currentNode = findClosestNode(area, true);
                r.velocity = Vector3.zero;
                return;
            }

            //if our currentNode is not null (i.e. we found a place to go to)
            if (currentNode != null)
            {
                //go to it and declare it searched so that nobody else goes to it.
                area.search(currentNode);
                follower.destination = currentNode.location;
            }

            //if we can see the current Node
            if (!Physics.Linecast(transform.position, currentNode.location) && Vector3.SqrMagnitude(transform.position - currentNode.location) < 4)
            {
                //check if our neighbors are not searched and in the bounds of the area
                Node currentNodeTemp = currentNode;
                currentNode = null;
                foreach (Node n in currentNodeTemp.connectedNodes)
                {
                    if (n != null && !n.searched && area.inArea(n))
                    {
                        currentNode = n;
                        break;
                    }
                }
            }

        }

    }

    public Node findClosestNode(Area area, bool ignoreSearchedNodes)
    {
        Node closestNode = null;
        float minDist = float.MaxValue;
        foreach (Node n in area.area)
        {
            if (n == null || (ignoreSearchedNodes && n.searched))
            {
                continue;
            }

            float currDist = Vector3.Distance(n.location, transform.position);
            if (currDist < minDist)
            {
                minDist = currDist;
                closestNode = n;
            }
        }
        return closestNode;
    }

    //stay out of the way of any bots, do nothing.
    public void idle()
    {
        bool isClear = Vector3.SqrMagnitude(enemyManager.follower.destination - follower.destination) > 0.5f * 0.5f;
        foreach (Enemy e in enemyManager.enemies)
        {
            if (e.Equals(this)) { continue; }
            if (Vector3.SqrMagnitude(e.follower.destination - follower.destination) < 0.3f * 0.3f)
            {
                isClear = false;
                break;
            }
        }

        //if you're within 0.3m of another bot's reservation, then find a new place to stay.
        if (!isClear)
        {
            Node idleNode = navScript.TryGetNode(transform.position, 5, 20, enemyManager.isValidPositionNode, null);
            if (idleNode != null)
            {
                follower.destination = idleNode.location;
            }
            else
            {
                Debug.Log("we know it's wrong, but we can't find anything");
            }
        }
        r.AddTorque(-r.angularVelocity * 10);
    }

    public void endSearch()
    {
        Debug.Log("completed search");
        reachedArea = false;
        closestNode = null;
        currentNode = null;
    }
    #endregion

    #region scanning behaviors
    Quaternion left;
    Quaternion right;
    bool movingLeft;

    bool startingScan = true;
    internal float cost;
    internal bool primed;
    internal bool active;

    //starts scanning the area
    public void scan()
    {

    }


    #endregion

    #region healthAndDamage
    public void healthHandler()
    {
        if (health <= 0)
        {

            kill();
        }

        if (health < maxHealth)
        {
            health += regen;
        }

        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    public virtual void kill()
    {
        if (beaconActive)
        {
            beaconActive = false;
            numEnemiesWithActiveBeacons--;
        }
        enemyManager.removeEnemy(this);
        currProject.enemiesOnProject.Remove(this);
        Destroy(gameObject);
    }

    public void damage(float damage)
    {
        health -= damage;
    }
    #endregion

    #region beaconAndPathHandling
    public void beaconHandler()
    {
        //set the boolean indicating whether the player is in sight or not.
        bool playerInLineOfSight = !Physics.Linecast(transform.position, player.transform.position);

        //the player is seen if it's in the line of sight and if they aren't in stealth mode

        playerSeen = playerInLineOfSight;
        //if the player has been seen and the player is in the angle of sight and this is the first sighting
        if (playerSeen && firstSighting)
        {
            //set the startTime to now
            startSeen = DateTime.Now;
            //make sure that this isn't called again until sight is lost and regained
            firstSighting = false;
        }
        //otherwise if the player was just lost
        else if (!playerSeen && !firstSighting)
        {
            //if the beacon is currently running
            if (beaconActive)
            {
                //disconnect the beacon, since we lost sight.
                numEnemiesWithActiveBeacons--;
                beaconActive = false;
            }

            //set the start time to an invalid value
            startSeen = DateTime.MaxValue;
            firstSighting = true;
        }

        //if the beacon is not on and the start time is not invalid and we are at or have exceeded the time to send the beacon
        if (!beaconActive && (DateTime.Now - startSeen).TotalSeconds >= timeTillBeaconIsSentSeconds)
        {
            //activate the beacon
            beaconActive = true;
            numEnemiesWithActiveBeacons++;
        }
    }

    //blips the beacon on and off
    public IEnumerable blipBeacon(DateTime start)
    {
        //wait to send the beacon.
        while ((DateTime.Now - start).TotalSeconds >= timeTillBeaconIsSentSeconds)
        {
            yield return null;
        }

        //send the beacon
        beaconActive = true;
        numEnemiesWithActiveBeacons++;
        yield return null;

        //unsend the beacon
        beaconActive = false;
        numEnemiesWithActiveBeacons--;
        yield return null;
    }

    #endregion

    float secondsTillInvalidation = 3;
    DateTime lastInvalidation = DateTime.Now;
    void invalidateSelfNode()
    {
        if (Vector3.Distance(transform.position, follower.destination) > 0f)
        {
            lastInvalidation = DateTime.Now;
        }
        if ((DateTime.Now - lastInvalidation).TotalSeconds > secondsTillInvalidation)
        {
            List<Node> invalidatedNodes = navScript.floodFill(navScript.getClosestNodeTo(transform.position).location, 3);
            foreach (Node n in invalidatedNodes)
            {
                n.lastCostTime = DateTime.Now;
            }
            secondsTillInvalidation = UnityEngine.Random.Range(2, 8);
            lastInvalidation = DateTime.Now;
        }
    }

    public bool isValidPositionNode(Node n, object o)
    {
        if (n == null)
        {
            return false;
        }

        foreach (Enemy e in enemyManager.enemies)
        {
            if (e.Equals(this)) { continue; }
            if (Vector3.SqrMagnitude(e.follower.destination - n.location) <= 0.5f * 0.5f)
            {
                return false;
            }
        }
        return true;
    }

    public int CompareTo(Enemy other)
    {
        return (int)(cost - other.cost);
    }

    float maxSpeedToKill = 3.0f;
    public virtual void onColl(Collision c)
    {
        if (r.velocity.magnitude > maxSpeedToKill)
        {
            kill();
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        sound.PlayBotCollide();
        onColl(collision);
    }
}

public enum EnemyAction { ATTACK, MOVE, SEARCH, IDLE, TRANSPORT }