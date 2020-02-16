using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    List<Transform> transformsToDestroy = new List<Transform>();

    List<EnemyRequest> requests = new List<EnemyRequest>();

    //the distribution of enemies that we want.
    ProjectList projects = new ProjectList();

    public static GameObject player;
    public static NavMapper navScript;

    public GameObject playerLoc;
    public NavMapper navScriptLoc;

    public List<Enemy> enemies = new List<Enemy>();

    public GameObject meteor;

    public Transform spawnTransform;
    public Transform parent;

    int searchPriority = 1;
    int movePriority = 2;
    int attackPriority = 3;

    int patrolPriority = 4;

    int playerSearchPriority = 5;
    int playerMovePriority = 6;
    int playerAttackPriority = 7;

    //the enemy templates
    public GameObject spinnerTemplate;
    public GameObject gunnerTemplate;
    public GameObject sniperTemplate;
    public GameObject seekerTemplate;

    public GameObject[] enemyTemplates;


    int counter = 0;

    bool firstPlayerLock = true;
    private bool playerPosLocked = false;


    private bool hasEnoughEnemiesForTask = true;

    int areaRadius = 20;

    Vector3 lastPlayerLock;

    static bool initialized = false;
    static int numTimesCalled = 0;
    int numTimesCalledLoc = 0;

    public Follower follower;

    public GameObject meteorSpawner;


    private Sound_Background sound;

    public MeshRenderer bodyMeshRenderer;
    private Material defaultMat;
    public Material brightRedMat;

    ProjectileLauncher launcher;

    private DateTime start;
    private DateTime soundTrack;
    public void Initialize()
    {
        sound = Camera.main.GetComponent<Sound_Background>();
        
        numTimesCalled++;
        numTimesCalledLoc++;
        if (initialized)
        {
            return;
        }
        Debug.Log("initialized enemy manager");

        player = playerLoc;
        navScript = navScriptLoc;

        //add the idle project
        projects.Add(new Project(EnemyAction.IDLE, null, 0, 0, ReturnTrue));
        //add the attack on sight project
        projects.Add(new Project(EnemyAction.ATTACK, player.transform, 11, 5, CanSeePlayer));


        initialized = true;

        enemies = new List<Enemy>();
        initialized = true;
        enemyTemplates = new GameObject[] { spinnerTemplate, gunnerTemplate, sniperTemplate, seekerTemplate };
        transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.0f;
        GameObject meteorClone = Instantiate(meteor, transform.position, transform.rotation, transform);
        bodyMeshRenderer = meteorClone.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>();
        defaultMat = bodyMeshRenderer.materials[0];

        //sound = Camera.main.GetComponent<Sound_Background>();
        //sound.playSoundtrack();


        follower.Initialize(navScript, 10, 5, 0.3f, 10, true);
        follower.destination = transform.position;

        GameObject launcherObj = Instantiate(meteorSpawner, transform.position + Vector3.up * 0.3f, Quaternion.identity);
        launcherObj.transform.parent = transform;
        launcherObj.GetComponent<ProjectileLauncher>().target = player.transform;
        launcher = launcherObj.GetComponent<ProjectileLauncher>();
        sound.playIntro();
    }



    bool areaSearched = true;

    Area playerSearchArea = new Area(new List<Node>());
    bool justAttacking = false;

    bool wasBlocked = true;
    bool wasPhasing = false;
    bool renderingRed = false;
    bool renderingDefault = false;

    Node attackNode = null;

    public bool isValidPositionNode(Node n, object o)
    {
        if (n == null)
        {
            return false;
        }

        foreach (Enemy e in enemies)
        {
            if (Vector3.SqrMagnitude(e.follower.destination - n.location) <= 0.5f * 0.5f)
            {
                return false;
            }
        }
        return true;
    }

    int[] triggers = new int[] { 0, 0, 0 };
    public bool isValidAttackNode(Node n, object c)
    {
        if (n == null || c == null || !isValidPositionNode(n, c) || (DateTime.Now - n.lastCostTime).TotalSeconds < 5)
        {
            triggers[0]++;
            return false;
        }

        if (!n.isPassableForMeteor)
        {
            triggers[1]++;
            return false;
        }

        triggers[2]++;
        float distSqr = Vector3.SqrMagnitude(n.location - ((Transform)c).position);
        return !Physics.Linecast(n.location, ((Transform)c).position, navScript.collisionLayer) && distSqr > 4 && distSqr < 9 && n.location.y < ((Transform)c).position.y + 1.0f;
    }

    Node closestVisibleNode = null;
    // Update is called once per frame
    void Update()
    {
        
        if (!initialized || !navScript.initialized)
        {
            return;
        }

        follower.lookAt(player.transform.position);
        //making enemies
        #region enemy Generation
        //determine if we have enough enemies for our tasks
        bool hasEnoughEnemies = transformsToDestroy.Count == 0;
        foreach (Project p in projects.projects)
        {
            if (!hasEnoughEnemies)
            {
                break;
            }

            if (p.enemiesOnProject.Count < p.optimalNumWorkers)
            {
                hasEnoughEnemies = false;
            }
        }


        //create bots at 1 per 3 seconds if there aren't enough bots, otherwise generate at 1 per 30 seconds.
        if ((reload <= 0 && enemies.Count < 5))
        {
            Enemy e = createEnemy();
            e.currProject = projects.projects[projects.projects.Count - 1];
            projects.projects[projects.projects.Count - 1].enemiesOnProject.Add(e);
            reload = 200;
        }
        else if (reload > 0)
        {
            reload--;
        }
        #endregion

        //if we're off map, find the closest valid node and move towards it
        if (navScript.getClosestNodeTo(transform.position) == null && closestVisibleNode == null)
        {
            Debug.Log("Inside if statement in EnemyManager");
            float minDistSqr = float.MaxValue;
            closestVisibleNode = null;
            foreach (Node n in navScript.allValidNodes)
            {
                float distSqr = Vector3.SqrMagnitude(n.location - transform.position);
                if (minDistSqr > distSqr)
                {
                    closestVisibleNode = n;
                    minDistSqr = distSqr;
                }                
            }
        }
        else if (navScript.getClosestNodeTo(transform.position) == null && closestVisibleNode != null)
        {
            Debug.Log("Inside else if in EnemyManager");
            follower.smoothFollowToTarget(closestVisibleNode.location);
        }
        else
        {
            if (Enemy.numEnemiesWithActiveBeacons > 0 || !Physics.Linecast(transform.position, player.transform.position))
            {
                if (attackNode == null || !isValidAttackNode(attackNode, player.transform))
                {
                    //try to get a new one
                    triggers = new int[] { 0, 0, 0 };
                    attackNode = navScript.TryGetNode(player.transform.position, 10, 20, isValidAttackNode, player.transform);
                    Debug.Log(triggers[0] + " " + triggers[1] + " " + triggers[2]);
                }
                //otherwise, set the follower destination to the new attackNode.
                else
                {
                    follower.destination = attackNode.location;
                }


                //if the places are different islands, then "phase"
                if (navScript.getClosestNodeTo(transform.position) == null || navScript.getClosestNodeTo(follower.destination).islandNum != navScript.getClosestNodeTo(transform.position).islandNum)
                {
                    follower.pathReciever.isMeteor = false;
                }
                //otherwise don't phase
                else
                {
                    follower.pathReciever.isMeteor = true;
                }                
            }
        }

        GetComponent<BoxCollider>().isTrigger = Physics.CheckBox(transform.position, new Vector3(0.6f, 0.3f, 0.5f), transform.rotation, navScript.collisionLayer);
        if (GetComponent<BoxCollider>().isTrigger && !renderingRed)
        {
            Debug.Log("rendering red");
            Material[] mats = bodyMeshRenderer.materials;
            mats[0] = brightRedMat;
            bodyMeshRenderer.materials = mats;
            renderingDefault = false;
            renderingRed = true;
        }
        else if (!GetComponent<BoxCollider>().isTrigger && !renderingDefault)
        {
            Debug.Log("rendering default");
            Material[] mats = bodyMeshRenderer.materials;
            mats[0] = defaultMat;
            bodyMeshRenderer.materials = mats;
            renderingDefault = true;
            renderingRed = false;
        }





        //basic enemy management
        for (int i = 0; i < enemies.Count; i++)
        {
            //remove null enemies
            if (enemies[i] == null)
            {
                enemies.RemoveAt(i);
                i--;
                continue;
            }
        }

        //here's the player attack, search and random search code
        #region Player Attack, Player Search, Random Search

        //if atleast one enemy sees the player
        if (Enemy.numEnemiesWithActiveBeacons > 0)
        {

            if (!justAttacking)
            {
                //remove all search projects
                List<Project> searchProjects = projects.getProjectsWithAction(EnemyAction.SEARCH);
                foreach (Project p in searchProjects)
                {
                    projects.Remove(p);
                }

                //make sure that there is an attackProject
                Project attackProj = new Project(EnemyAction.ATTACK, player.transform, 10, int.MaxValue, ReturnTrue);
                if (projects.Contains(attackProj) == null)
                {
                    //remove the attack on sight project
                    projects.Add(attackProj);
                }
            }
            justAttacking = true;
        }
        //otherwise
        else
        {
            //if we were just attacking...
            if (justAttacking)
            {
                //remove the attack project
                Project attackProj = new Project(EnemyAction.ATTACK, player.transform, 10, 5, ReturnTrue);
                if (projects.Contains(attackProj) != null)
                {
                    projects.Remove(attackProj);
                }

                //make sure there's a search project for the player area.
                Project searchProj = new Project(EnemyAction.SEARCH, playerSearchArea, 9, 3, ReturnTrue);
                if (projects.Contains(searchProj) == null)
                {
                    Node closestNode = navScript.getClosestNodeTo(player.transform.position);
                    if (closestNode != null)
                    {
                        playerSearchArea = new Area(navScript.floodFill(closestNode.location, 10));
                        projects.Add(new Project(EnemyAction.SEARCH, playerSearchArea, 9, 4, ReturnTrue));
                    }
                }
            }

            //if the player area has been completely searched
            if (playerSearchArea.areaComplete())
            {
                //remove the player search project, if it exists
                Project playerSearchProj = new Project(EnemyAction.SEARCH, playerSearchArea, 9, 3, ReturnTrue);
                if (projects.Contains(playerSearchProj) != null)
                {
                    projects.Remove(playerSearchProj);
                }

                //create search projects (up to 3 at a time), searching random areas in hopes of finding the player.
                List<Project> searchProjects = projects.getProjectsWithAction(EnemyAction.SEARCH);
                if (searchProjects.Count < 3)
                {
                    Debug.Log("making search projects");
                    Node randomNode = navScript.allValidNodes[UnityEngine.Random.Range(0, navScript.allValidNodes.Count - 1)];
                    if (randomNode == null)
                    {
                        Debug.Log("Random node is null");
                        return;
                    }

                    if (navScript.getClosestNodeTo(randomNode.location, false) != null)
                    {
                        List<Node> areaToSearch = navScript.floodFill(randomNode.location, 10);

                        if (areaToSearch == null)
                        {
                            Debug.Log("null area");
                        }
                        else
                        {
                            projects.Add(new Project(EnemyAction.SEARCH, new Area(areaToSearch), 5, 2, ReturnTrue));
                        }
                    }
                }

                //complete the search projects if their areas have been completely searched
                foreach (Project p in searchProjects)
                {
                    if (((Area)p.task).areaComplete())
                    {
                        projects.Remove(p);
                    }
                }
            }

            justAttacking = false;
        }

        #endregion

        //attack any threats. (Can include health packs, ammo drops, armor drops, or any other tools the player could use.)
        #region destroying threats other than the player
        List<Enemy> extraEnemies = projects.getExtraEnemies();
        while (extraEnemies.Count > 0 && transformsToDestroy.Count > 0)
        {
            //get the closest enemy to the transform we need to destroy
            float leastDist = float.MaxValue;
            Enemy best = null;
            int bestIndex = -1;
            for (int i = 0; i < extraEnemies.Count; i++)
            {
                float currDist = Vector3.Distance(transformsToDestroy[0].position, extraEnemies[i].transform.position);
                if (currDist < leastDist)
                {
                    leastDist = currDist;
                    best = extraEnemies[i];
                    bestIndex = i;
                }
            }
            Project attackThreat = new Project(EnemyAction.ATTACK, transformsToDestroy[0], 3, 1, ReturnTrue);
            attackThreat.enemiesOnProject.Add(best);
            extraEnemies.RemoveAt(bestIndex);
            transformsToDestroy.RemoveAt(0);
            projects.Add(attackThreat);
        }

        //end completed attack projects
        List<Project> attackProjects = projects.getProjectsWithAction(EnemyAction.ATTACK);
        foreach (Project p in attackProjects)
        {
            //if the transform has been destroyed, then end the attack project
            if (p.task == null)
            {
                projects.Remove(p);
            }
        }
        #endregion

        /*
        attackProjects = projects.getProjectsWithAction(EnemyAction.ATTACK);
        foreach(Project p in attackProjects)
        {
            foreach(Enemy e in p.enemiesOnProject)
            {
                //if the length of the path is greater than 5m, create a transport task, and attempt to get seekers to complete it.
                if(e.pathReciever.pathLength > 5 && !e.GetType().Equals(typeof(EnemySeeker)))
                {
                    Project transportProj = new Project(EnemyAction.TRANSPORT, new object[] {e, p.task}, 11, 1, typeof(EnemySeeker));
                    projects.Add(transportProj);
                }
            }
        }
        */


        

        foreach (Project p in projects.projects)
        {
            foreach (Enemy e in enemies)
            {
                if (e.currentAction == EnemyAction.IDLE && p.enemyValidator(e) && p.enemiesOnProject.Count < p.optimalNumWorkers)
                {
                    projects.SwitchProject(e, p);
                }
            }
        }
        //if ((DateTime.Now - start).TotalSeconds >= 5.0f)
        //{

        //    sound.playIntro();
        //    start = DateTime.MaxValue;
        //    soundTrack = DateTime.Now;
        //}
        //if ((DateTime.Now - soundTrack).TotalSeconds >= 173.0f)
        //{
        //    sound.playSoundtrack();
        //}
        if (GetComponent<Rigidbody>().velocity.magnitude > 0.0f)
        {
            if (!GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().volume = 0.2f;
                //sound.PlayBooster();

            }

        }
    }
    int reload = 200;

    //the functions used to determine whether tasks are complete or not.
    //all return booleans, all accept enemy and task objects
    //true = task complete, false = task ongoing.

    //makes a random enemy
    public Enemy createEnemy()
    {
        int random = UnityEngine.Random.Range(0, enemyTemplates.Length);
        while (random == 3 && EnemySeeker.numSeekers > 0)
        {
            random = UnityEngine.Random.Range(0, enemyTemplates.Length);
        }
        GameObject enemy = Instantiate(enemyTemplates[random], spawnTransform.position, Quaternion.identity);
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        enemyScript.navScript = navScript;
        enemyScript.player = player;
        enemies.Add(enemyScript);
        enemyScript.currProject = projects.getProjectsWithAction(EnemyAction.IDLE)[0];
        projects.getProjectsWithAction(EnemyAction.IDLE)[0].enemiesOnProject.Add(enemyScript);
        enemyScript.enemyManager = this;
        return enemyScript;
    }

    public static EnemyValidator ReturnTrue = new EnemyValidator(returnTrue);
    public static EnemyValidator CanSeePlayer = new EnemyValidator(canSeePlayer);
    public static EnemyValidator IsSeeker = new EnemyValidator(isSeeker);

    public static bool returnTrue(Enemy e)
    {
        return true;
    }

    public static bool canSeePlayer(Enemy e)
    {
        return !Physics.Linecast(e.transform.position, player.transform.position, navScript.collisionLayer);
    }

    public static bool isSeeker(Enemy e)
    {
        return e.GetType().Equals(typeof(EnemySeeker));
    }

    public void removeEnemy(Enemy e)
    {
        enemies.Remove(e);
        if (Enemy.numEnemiesWithActiveBeacons == 0)
        {
            playerSearchArea = new Area(navScript.floodFill(e.transform.position, 10));
        }
    }
}

public delegate bool EnemyValidator(Enemy e);