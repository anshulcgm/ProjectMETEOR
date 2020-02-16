using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavMapper : MonoBehaviour
{
    //things in the default and spatial layers will be mapped. Everything else will not.
    public LayerMask collisionLayer;

    public int zBound = 300;
    public int xBound = 300;
    public int yBound = 50;
    //the map - has all of the node objects (the nodes store position and their neighbor references)
    public Node[][][] map;

    //the spacing between nodes (meters)
    public float spaceBetweenNodes;

    //the height of the node above the "floor" in meters (which is whatever the raycast detects when it raycasts straight down)
    public float heightAboveFloor;

    //the time between pathfinding refreshes in seconds. Setting this to 0 will result in continuous pathfinding 
    //which depending on the time taken to pathfind to the player can either be acceptable or trash. 
    //Don't be stupid and set the time to a negative value.
    public float millisecondsBetweenPathfindingCalls;

    //the maxumum time spent pathfinding before giving up and returning an incomplete path.
    public double millisecondsSpentPathfinding;

    //the player
    public GameObject player;

    //the marker object - this object is used for debugging 
    //(it is created at all the explored nodes to show the nodes explored. 
    //Set it to null if you don't want to see the nodes evaluated)
    public GameObject marker;

    public GameObject nodeMarker;
    public GameObject pathMarker;

    //public PillarSpawner pillarSpawner;

    private Sound_Background sound;
    // Use this for initialization

    public bool initialized = false;
    bool waitingForCreateMap = false;
    float compareMS = 0;
    public void Initialize()
    {
        sound = Camera.main.GetComponent<Sound_Background>();
        if (waitingForCreateMap)
        {
            return;
        }

        lagForMapCreation = DateTime.Now;
        waitingForCreateMap = true;

        lagForMapCreation = DateTime.Now;
        waitingForCreateMap = true;

        initialized = true;
        Debug.Log("initialized map");
        //make all the pillars at the start of the game.
        //pillarSpawner.Initialize(xBound * spaceBetweenNodes, zBound * spaceBetweenNodes);
        //makes the map at the start of the game and start the taskCompleter
        createMap();
        // drawMap();
        Debug.Log("Map length is " + map.Length);
        compareMS = millisecondsBetweenPathfindingCalls;
    }

    public void drawMap()
    {
        for (int i = 0; i < map.Length; i++)
        {
            for (int i1 = 0; i1 < map[i].Length; i1++)
            {
                for (int i2 = 0; i2 < map[i][i1].Length; i2++)
                {
                    if (map[i][i1][i2] != null)
                    {
                        foreach (Node n in map[i][i1][i2].connectedNodes)
                        {
                            if (n != null)
                            {
                                Debug.DrawLine(map[i][i1][i2].location, n.location, Color.cyan, 100000000000.0f);
                            }
                        }
                    }
                }
            }
        }

    }

    DateTime lastTaskCompleterCall = DateTime.MinValue;
    DateTime lagForMapCreation = DateTime.Now;
    float lagTime = 2.0f;

    public List<Node> allValidNodes;
    public void Update()
    {
        if (waitingForCreateMap && (DateTime.Now - lagForMapCreation).TotalSeconds >= lagTime)
        {
            //makes the map at the start of the game, starts the task completer.
            createMap();

            //make all the pillars at the start of the game.
            //pillarSpawner.Initialize(this);

            compareMS = millisecondsBetweenPathfindingCalls;
            waitingForCreateMap = false;
            initialized = true;

        }

        if (!initialized)
        {
            return;
        }

        //call the taskCompleter function every "millisecondsBetweenPathfindingCalls" milliseconds.
        //we're doing this instead of Corutines because the Corutine garbage collecter is actually garbage.
        //I got 15 FPS with Unity Corutines.

        if ((DateTime.Now - lastTaskCompleterCall).TotalMilliseconds > compareMS)
        {
            //complets as many tasks as you can before maxTimeSpentPathfinding
            PathState pathState = taskCompleter();
            lastTaskCompleterCall = DateTime.Now;

            //if the pathState is incomplete, continue pathfinding during the next update
            if (pathState == PathState.INCOMPLETE)
            {
                compareMS = 0;
            }
            //if the pathState is complete, wait "millisecondsBetweenPathfindingCalls" before calling this function again.
            else
            {
                compareMS = millisecondsBetweenPathfindingCalls;
            }
        }
    }



    #region taskManagement

    PriorityTree savedNodeQueue = null;
    List<PathTask> tasks = new List<PathTask>();
    public void addTask(PathTask pt)
    {
        tasks.Add(pt);
    }

    public PathState taskCompleter()
    {
        //get the start time
        DateTime startTime = DateTime.Now;

        //set the pathState to null
        PathState pathState = PathState.NULL;


        int numIter = 0;
        //as long as we haven't exceeded time, and we have tasks to do, keep pathfinding
        while (pathState != PathState.INCOMPLETE && tasks.Count > 0)
        {
            numIter++;
            if (tasks[0] == null)
            {
                tasks.Remove(tasks[0]);
                pathState = PathState.COMPLETE;
                continue;
            }


            //holds the length of the path
            float pathLength;
            //holds the path
            List<Vector3> path;
            //holds the nodeQueue
            PriorityTree nodeQueue;

            //get the path (in terms of nodes)
            List<Node> nodePath = findPath(tasks[0].start, tasks[0].finish, startTime, savedNodeQueue, out pathState, out pathLength, out path, out nodeQueue, tasks[0].isMeteor);


            //if we haven't run out of time
            if (pathState != PathState.INCOMPLETE)
            {
                //finish the pathTask
                tasks[0].finishTask(nodePath, path, pathLength);
                //remove the task from the list
                tasks.RemoveAt(0);
                //don't save this node queue
                savedNodeQueue = null;
                //clear the map
                clearNodes(nodesToClear);
            }
            //if the state is incomplete
            else
            {
                //save the node queue as well as the state of the map for the next pathfinding.
                savedNodeQueue = nodeQueue;
            }
        }

        return pathState;
    }
    #endregion

    //functions that help create the "map" (list of nodes) at the start of the game
    #region mapMakingFunctions
    //makes a map of the surroundings, starting with the center.    
    void createMap()
    {
        Node.distanceBetweenNodes = spaceBetweenNodes;
        map = new Node[zBound][][];
        //go through the map of nodes
        for (int i = 0; i < map.Length; i++)
        {
            map[i] = new Node[xBound][];
            //get the current Z.
            float currentZ = (i - map.Length / 2.0f) * spaceBetweenNodes;
            for (int i1 = 0; i1 < map[i].Length; i1++)
            {
                map[i][i1] = new Node[yBound];
                //calculate current X.
                float currentX = (i1 - map[i].Length / 2.0f) * spaceBetweenNodes;

                //if there is something at this x,z position
                if (Physics.Raycast(new Vector3(currentX, transform.position.y, currentZ), Vector3.down, Mathf.Infinity, collisionLayer) || Physics.Raycast(new Vector3(currentX, transform.position.y, currentZ), Vector3.up, Mathf.Infinity, collisionLayer))
                {
                    //make all the y-nodes
                    for (int i2 = 0; i2 < map[i][i1].Length; i2++)
                    {
                        //calculate current Y.
                        float currentY = (i2 - map[i][i1].Length / 2.0f) * spaceBetweenNodes;

                        if (!Physics.CheckSphere(new Vector3(currentX, currentY, currentZ), Enemy.colliderRadius * 0.3f, collisionLayer))
                        {
                            map[i][i1][i2] = new Node(new Vector3(currentX, currentY, currentZ) + transform.position);
                        }

                        if (!Physics.CheckSphere(new Vector3(currentX, currentY, currentZ), Enemy.colliderRadius * 0.3f, collisionLayer))
                        {
                            map[i][i1][i2] = new Node(new Vector3(currentX, currentY, currentZ) + transform.position);
                        }
                        else
                        {
                            map[i][i1][i2] = null;
                        }

                    }
                }
                //if there's nothing there
                else
                {
                    //set all the y-nodes to null.
                    for (int i2 = 0; i2 < map[i][i1].Length; i2++)
                    {
                        map[i][i1][i2] = null;
                    }
                }
            }
        }

        //set all the connections between nodes
        setAllNodeConnections();
        //smooth all connections between nodes
        smoothAllConnections();
        //find all the nodes that can actually be reached. (very rapid flood fill)
        allValidNodes = superFillFromSelf();
        //make the nodes that cannot be reached null.
        for (int i = 0; i < map.Length; i++)
        {
            for (int i1 = 0; i1 < map[i].Length; i1++)
            {
                for (int i2 = 0; i2 < map[i][i1].Length; i2++)
                {
                    if (map[i][i1][i2] != null && !map[i][i1][i2].navFill)
                    {
                        map[i][i1][i2] = null;
                    }
                }
            }
        }
        getPassablePointsForMeteor();
        createMeteorIslands();
    }

    private void getPassablePointsForMeteor()
    {
        float mag = new Vector3(0.3f, 0.5f, 0.6f).magnitude + 0.1f;
        foreach (Node n in allValidNodes)
        {
            n.isPassableForMeteor = !Physics.CheckSphere(n.location, mag);
        }
    }

    private void createMeteorIslands()
    {
        int islandNum = 1;
        foreach (Node n in allValidNodes)
        {
            List<Node> currentBoundary = new List<Node>();
            if (n == null || !n.isPassableForMeteor || n.islandNum != 0)
            {
                continue;
            }
            currentBoundary.Add(n);
            while (currentBoundary.Count > 0)
            {
                Node curr = currentBoundary[0];
                foreach (Node nc in curr.connectedNodes)
                {
                    if (nc != null && nc.isPassableForMeteor && nc.islandNum == 0)
                    {
                        currentBoundary.Add(n);
                        nc.islandNum = islandNum;
                    }
                }
                currentBoundary.RemoveAt(0);
            }
            islandNum++;
        }
    }


    //fills every possible node from start position
    private List<Node> superFillFromSelf()
    {
        List<Node> currentBoundary = new List<Node>();
        List<Node> volume = new List<Node>();
        currentBoundary.Add(getClosestNodeTo(transform.position, true));
        if (currentBoundary[0] == null)
        {
            Debug.Log("Cannot find node at transform.position");
            return new List<Node>();
        }
        currentBoundary[0].navFill = true;
        while (currentBoundary.Count > 0)
        {
            Node curr = currentBoundary[0];

            foreach (Node n in curr.connectedNodes)
            {
                if (n != null && !n.navFill)
                {
                    currentBoundary.Add(n);
                    n.navFill = true;
                }
            }
            currentBoundary.RemoveAt(0);
            volume.Add(curr);
        }
        return volume;
    }

    //sets the node connections after the map has been made - only called once at the start.
    void setAllNodeConnections()
    {
        for (int i = 0; i < map.Length; i++)
        {
            for (int i1 = 0; i1 < map[i].Length; i1++)
            {
                for (int i2 = 0; i2 < map[i][i1].Length; i2++)
                {
                    setNodeConnections(i, i1, i2);
                }
            }
        }
    }

    //smooths connections between nodes (eliminates one-way paths)
    void smoothAllConnections()
    {
        for (int i = 0; i < map.Length; i++)
        {
            for (int i1 = 0; i1 < map[i].Length; i1++)
            {
                for (int i2 = 0; i2 < map[i][i1].Length; i2++)
                {
                    if (map[i][i1][i2] != null)
                    {
                        map[i][i1][i2].smoothConnections();
                    }
                }
            }
        }
    }

    //gets a node from the map
    public Node getNodeFromMap(int x, int y, int z)
    {
        //returns a node, unless the index is out of bounds, in which case it returns null
        if (z < map.Length && z >= 0)
        {
            if (x < map[z].Length && x >= 0)
            {
                if (y < map[z][x].Length && y >= 0)
                {
                    return map[z][x][y];
                }
            }
        }
        return null;
    }
    #endregion

    //functions that help find paths to points
    #region pathFindingFunctions

    //gets a path from a position to another position
    public List<Node> findPath(Vector3 start, Vector3 finish, DateTime startTime, PriorityTree savedNodeQueue, out PathState pathState, out float pathLength, out List<Vector3> path, out PriorityTree nodeQueue, bool isMeteor)
    {
        Node startNode = getClosestNodeTo(start);
        Node finishNode = getClosestNodeTo(finish);

        if (finishNode == null || startNode == null)
        {
            Debug.Log("start or finish is null");
            pathState = PathState.NO_PATH;
            pathLength = -1;
            path = null;
            nodeQueue = null;
            return null;
        }

        //if the start is the finish
        if (startNode.Equals(finishNode))
        {
            //the path is complete
            pathState = PathState.COMPLETE;
            //the pathlength is just the distance from where you are to the node to the finish.
            pathLength = Vector3.Distance(start, startNode.location) + Vector3.Distance(finish, startNode.location);
            path = new List<Vector3>();
            //add each of the locations to the path list
            path.Add(start); path.Add(startNode.location); path.Add(finish);
            //the path via nodes
            List<Node> nodePath = new List<Node>();
            nodePath.Add(startNode);
            nodeQueue = null;
            return nodePath;
        }

        //search the nodes for a path.
        pathState = search(startNode, finishNode, startTime, savedNodeQueue, out nodeQueue, isMeteor);

        //if there is a path
        if (pathState == PathState.COMPLETE)
        {
            //get the path by unwinding parent references. 
            //(Each node has a parent node exept for the start, if you trace back the parent nodes from the finish, 
            //you will get back to the starting node - provided that there is a valid path.)            
            List<Node> nodePath = new List<Node>();
            //the path
            path = new List<Vector3>();
            //the current node
            Node currNode = finishNode;
            //if the parentNode is null, then you have reached the start, so keep getting the parent reference until then
            while (currNode != null)
            {
                //insert the parentNode at the beginning of the list (since we're going backwards from finish to start)
                nodePath.Insert(0, currNode);
                path.Insert(0, currNode.location);
                //get the parent of the currentNode, make that the new current
                currNode = currNode.parentNode;
            }
            //insert the start pos at the beginning of the list
            path.Insert(0, start);
            //insert the end pos at the end of the list
            path.Add(finish);

            //set the pathlength
            pathLength = finishNode.pathLength + Vector3.Distance(start, startNode.location) + Vector3.Distance(finish, finishNode.location);

            //return the node path
            return nodePath;
        }
        //if we couldn't complete the path or if there is no path, return null.
        pathLength = -1;
        path = null;
        return null;
    }


    List<Node> nodesToClear = new List<Node>();
    //searches from a node for a finish
    private PathState search(Node start, Node finish, DateTime startTime, PriorityTree savedNodeQueue, out PriorityTree nodeQueue, bool isMeteor)
    {
        int numNodesSearched = 0;

        //if the player is in an unmapped area
        if (start == null || finish == null)
        {
            //don't path
            nodeQueue = null;
            return PathState.NO_PATH;
        }

        //if we don't have any previous work to go off of...
        if (savedNodeQueue == null)
        {
            //make a new heap to store the nodes we're looking at. Give it a starting size of 1.
            nodeQueue = new PriorityTree(1);

            //calculate g,h, and f values for the start node
            start.gVal = 0;
            start.pathLength = 0;
            start.hVal = heuristic(start.location, finish.location);
            start.fVal = start.gVal + start.hVal;

            //set the state of the start node to open
            start.state = NodeState.OPEN;
            //add the start node to the heap
            nodeQueue.Add(start);
            nodesToClear = new List<Node>();
            nodesToClear.Add(start);
        }
        else
        {
            //otherwise, go off of what we have
            nodeQueue = savedNodeQueue;
        }

        //as long as we still have nodes to examine
        while (!nodeQueue.isEmpty)
        {
            //if we're over our allotted time           
            if ((DateTime.Now - startTime).TotalMilliseconds > millisecondsSpentPathfinding)
            {
                //abort
                return PathState.INCOMPLETE;
            }

            numNodesSearched++;
            //get the "most promising" node from the top of the heap. (nodes are sorted by f value)
            Node currentNode = nodeQueue.Remove();

            //if this node is the finish
            if (currentNode.Equals(finish))
            {
                //we're done, return.
                return PathState.COMPLETE;
            }

            //set the state of this node to closed
            currentNode.state = NodeState.CLOSED;

            Vector3 currDir;

            if (currentNode.parentNode != null)
            {
                currDir = currentNode.location - currentNode.parentNode.location;
            }
            else
            {
                currDir = finish.location - start.location;
            }

            int dir = 0;
            foreach (Node node in currentNode.connectedNodes)
            {
                if (node == null || (isMeteor && !node.isPassableForMeteor))
                {
                    continue;
                }

                Vector3 newDir = node.location - currentNode.location;
                if (node.state == NodeState.UNTESTED)
                {
                    node.parentNode = currentNode;
                    node.state = NodeState.OPEN;
                    node.gVal = currentNode.gVal + Vector3.Distance(node.location, currentNode.location) * node.cost;
                    node.pathLength = currentNode.pathLength + Vector3.Distance(node.location, currentNode.location);
                    //distance-based heuristic
                    node.hVal = heuristic(node.location, finish.location);
                    node.fVal = node.gVal + node.hVal;
                    nodeQueue.Add(node);
                    nodesToClear.Add(node);
                }
                else if (node.state == NodeState.OPEN)
                {
                    //calculate the possible cost of doing this path
                    double transversalCost = (Vector3.Distance(node.location, currentNode.location)) * node.cost;
                    double possibleG = currentNode.gVal + transversalCost;
                    //only change the parent to your own node and add this node to the list if it will result in a faster path
                    if (possibleG < node.gVal)
                    {
                        node.parentNode = currentNode;
                        node.gVal = possibleG;
                        node.pathLength = currentNode.pathLength + Vector3.Distance(node.location, currentNode.location);
                        node.fVal = node.gVal + node.hVal;
                        nodeQueue.Bubble(node);
                    }
                }


                dir++;
            }
        }
        nodeQueue = null;
        return PathState.NO_PATH;
    }


    float diff(int currDirPointer, int newDir)
    {
        if (currDirPointer == -1)
        {
            return 0;
        }
        int firstDiff = Math.Abs(newDir - currDirPointer);
        int secondDiff = Math.Abs(firstDiff - 8);
        return Mathf.Min(firstDiff, secondDiff);
    }

    public float heuristic(Vector3 a, Vector3 b)
    {
        //Differences in x, y, and z
        float dx = Mathf.Abs(a.x - b.x);
        float dy = Mathf.Abs(a.y - b.y);
        float dz = Mathf.Abs(a.z - b.z);

        float playerDetterrant = 0;
        if (Vector3.Magnitude(player.transform.position - a) < 2)
        {
            playerDetterrant = 2 / Vector3.Magnitude(player.transform.position - a);
        }

        //add manhattan distance
        return ((dx + dy + dz)
               //subtract benefits of taking diagonal paths
               - (0.586f) * Mathf.Min(dx, dz)) + playerDetterrant;
    }

    //gets the closest node to a Vector3 position
    public Node getClosestNodeTo(Vector3 pos)
    {
        return getClosestNodeTo(pos, true);
    }

    //the maximum time we spend searching before returning null
    double maxMillisecondsToSearch = 20;
    public Node getClosestNodeTo(Vector3 pos, bool spread)
    {
        Vector3 start = new Vector3(transform.position.x - xBound * spaceBetweenNodes * 0.5f, transform.position.y - yBound * spaceBetweenNodes * 0.5f, transform.position.z - zBound * spaceBetweenNodes * 0.5f);
        int zNode = Mathf.RoundToInt(((pos - start) / spaceBetweenNodes).z);
        int yNode = Mathf.RoundToInt(((pos - start) / spaceBetweenNodes).y);
        int xNode = Mathf.RoundToInt(((pos - start) / spaceBetweenNodes).x);

        Node bestNode = getNodeFromMap(xNode, yNode, zNode);
        if (bestNode != null && !Physics.Linecast(pos, bestNode.location, collisionLayer))
        {
            return bestNode;
        }
        if (!spread)
        {
            return null;
        }

        triples = new List<Triple>();
        removedNodes = new List<Node>();
        triples.Add(new Triple(xNode, yNode, zNode));
        return getClosestValidNode(new isValidNode(closestNodeValidator), pos);
    }

    //whether this node is valid or not.
    bool closestNodeValidator(Node n, object pos)
    {
        return n != null && !Physics.Linecast(n.location, (Vector3)pos, collisionLayer);
    }

    public Node getClosestValidNodeTo(Vector3 pos, isValidNode isValid, object c)
    {
        Vector3 start = new Vector3(transform.position.x - xBound * spaceBetweenNodes * 0.5f, transform.position.y - yBound * spaceBetweenNodes * 0.5f, transform.position.z - zBound * spaceBetweenNodes * 0.5f);
        int zNode = Mathf.RoundToInt(((pos - start) / spaceBetweenNodes).z);
        int yNode = Mathf.RoundToInt(((pos - start) / spaceBetweenNodes).y);
        int xNode = Mathf.RoundToInt(((pos - start) / spaceBetweenNodes).x);

        triples = new List<Triple>();
        removedNodes = new List<Node>();
        triples.Add(new Triple(xNode, yNode, zNode));
        return getClosestValidNode(isValid, c);
    }


    //searches from closest to farthest, returns the first node that is valid.
    List<Triple> triples = new List<Triple>();
    List<Node> removedNodes = new List<Node>();
    public Node getClosestValidNode(isValidNode isValid, object c)
    {
        DateTime start = DateTime.Now;
        while (true)
        {
            //if we've exceeded the maximum time, return null.
            if ((DateTime.Now - start).TotalMilliseconds > maxMillisecondsToSearch)
            {
                //Debug.Log("over time");
                return null;
            }

            //as long as there are triples that we've already checked, remove them.
            while (triples.Count > 0 && (getNodeFromMap(triples[0].x, triples[0].y, triples[0].z) == null || getNodeFromMap(triples[0].x, triples[0].y, triples[0].z).used))
            {
                triples.RemoveAt(0);
            }

            //if there are no triples left to analyze, return null.
            if (triples.Count == 0)
            {
                //Debug.Log("searched all");
                return null;
            }

            //add the current triple to the removed list, remove it from the evaluation list.
            int x = triples[0].x;
            int y = triples[0].y;
            int z = triples[0].z;

            //if the node is valid, return it.
            Node current = getNodeFromMap(x, y, z);
            current.used = true;
            removedNodes.Add(current);
            triples.RemoveAt(0);
            /*
            foreach (Node n in current.connectedNodes)
            {
                if (n != null)
                {
                    Debug.DrawLine(n.location, current.location, Color.blue);
                }
            }
            */
            if (isValid(current, c))
            {
                foreach (Node n in removedNodes)
                {
                    n.used = false;
                }
                //Debug.Log("ACHIEVED!");
                /*
                foreach(Node n in current.connectedNodes)
                {
                    if(n != null)
                    {
                        Debug.DrawLine(n.location, current.location, Color.red, 1.0f);
                    }
                }
                */

                return current;
            }

            //otherwise add all triples, in the order we want to evaluate them.
            triples.Add(new Triple(x + 1, y, z));
            triples.Add(new Triple(x - 1, y, z));
            triples.Add(new Triple(x, y, z + 1));
            triples.Add(new Triple(x, y, z - 1));
            triples.Add(new Triple(x + 1, y, z + 1));
            triples.Add(new Triple(x + 1, y, z - 1));
            triples.Add(new Triple(x - 1, y, z + 1));
            triples.Add(new Triple(x - 1, y, z - 1));
            triples.Add(new Triple(x, y + 1, z));
            triples.Add(new Triple(x, y - 1, z));
        }
    }

    class Triple
    {
        public int x;
        public int y;
        public int z;
        public Triple(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override bool Equals(object t)
        {
            if (t.GetType().Equals(GetType()))
            {
                return ((Triple)t).x == x && ((Triple)t).y == y && ((Triple)t).z == z;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (x + "" + y + "" + z).GetHashCode();
        }
    }

    public List<Node> getNodesInArea(float xDist, float yDist, float zDist, Vector3 pos)
    {
        Vector3 start = new Vector3(transform.position.x - xBound * spaceBetweenNodes * 0.5f, transform.position.y - yBound * spaceBetweenNodes * 0.5f, transform.position.z - zBound * spaceBetweenNodes * 0.5f);

        Vector3 startPos = new Vector3(pos.x - xDist / 2.0f, pos.y - yDist / 2.0f, pos.z - zDist / 2.0f);
        int zStart = Mathf.RoundToInt(((startPos - start) / spaceBetweenNodes).z);
        int yStart = Mathf.RoundToInt(((startPos - start) / spaceBetweenNodes).y);
        int xStart = Mathf.RoundToInt(((startPos - start) / spaceBetweenNodes).x);

        Vector3 endPos = new Vector3(pos.x + xDist / 2.0f, pos.y + yDist / 2.0f, pos.z + zDist / 2.0f);
        int zEnd = Mathf.RoundToInt(((startPos - start) / spaceBetweenNodes).z);
        int yEnd = Mathf.RoundToInt(((startPos - start) / spaceBetweenNodes).y);
        int xEnd = Mathf.RoundToInt(((startPos - start) / spaceBetweenNodes).x);

        List<Node> nodesInArea = new List<Node>();
        for (int z = zStart; z <= zEnd; z++)
        {
            for (int y = yStart; y <= yEnd; y++)
            {
                for (int x = xStart; x <= xEnd; x++)
                {
                    nodesInArea.Add(getNodeFromMap(x, y, z));
                }
            }
        }
        return nodesInArea;
    }


    public List<Node> getNodesInArea(float area, Vector3 pos)
    {
        float dist = Mathf.Pow(area, 1.0f / 3.0f);
        return getNodesInArea(dist, dist, dist, pos);
    }


    void clearNodes(List<Node> nodesToClear)
    {
        if (nodesToClear != null)
        {
            foreach (Node n in nodesToClear)
            {
                n.parentNode = null;
                n.pathLength = 0;
                n.state = NodeState.UNTESTED;
            }
        }
    }
    #endregion

    public bool canGoToNode(Node n)
    {
        return (((n.reservers.Count == 0) || (n.reservers.Count == 1 && n.reservers[0].Equals(this))) && (n.claimer == null || n.claimer.Equals(this)));
    }

    //sets the node connections for one node
    void setNodeConnections(int i, int i1, int i2)
    {
        if (map[i][i1][i2] == null)
        {
            return;
        }

        //get all the adjacent nodes
        Node[] adjacentNodes = new Node[10];
        adjacentNodes[0] = getNodeFromMap(i1 + 1, i2, i);
        adjacentNodes[1] = getNodeFromMap(i1 + 1, i2, i - 1);
        adjacentNodes[2] = getNodeFromMap(i1, i2, i - 1);
        adjacentNodes[3] = getNodeFromMap(i1 - 1, i2, i - 1);
        adjacentNodes[4] = getNodeFromMap(i1 - 1, i2, i);
        adjacentNodes[5] = getNodeFromMap(i1 - 1, i2, i + 1);
        adjacentNodes[6] = getNodeFromMap(i1, i2, i + 1);
        adjacentNodes[7] = getNodeFromMap(i1 + 1, i2, i + 1);
        adjacentNodes[8] = getNodeFromMap(i1, i2 + 1, i);
        adjacentNodes[9] = getNodeFromMap(i1, i2 - 1, i);

        //set the connected nodes of this node
        map[i][i1][i2].setConnectedNodes(adjacentNodes, collisionLayer);
    }

    //flood fills and returns the nodes filled. Used for identifying areas. Calls the recursive function.
    //very expensive, don't use this function regularly.
    public List<Node> floodFill(Vector3 start, int numTimesToFlood)
    {
        List<Node> currentBoundary = new List<Node>();
        List<Node> volume = new List<Node>();
        currentBoundary.Add(getClosestNodeTo(start, true));
        if (getClosestNodeTo(start, true) == null)
        {
            Debug.Log("START IS NULL!");
            return new List<Node>();
        }
        int counter = 0;
        while (currentBoundary.Count > 0 && counter <= numTimesToFlood)
        {
            Node curr = currentBoundary[0];

            foreach (Node n in curr.connectedNodes)
            {
                if (n != null && !n.navFill)
                {
                    currentBoundary.Add(n);
                }
            }
            currentBoundary.RemoveAt(0);
            volume.Add(curr);
            counter++;
        }
        return volume;
    }

    private List<Node> superFill(Vector3 start)
    {
        List<Node> currentBoundary = new List<Node>();
        List<Node> volume = new List<Node>();
        currentBoundary.Add(getClosestNodeTo(start, true));
        if (start == null)
        {
            Debug.Log("START IS NULL!");
            return new List<Node>();
        }
        currentBoundary[0].navFill = true;
        while (currentBoundary.Count > 0)
        {
            Node curr = currentBoundary[0];

            foreach (Node n in curr.connectedNodes)
            {
                if (n != null && !n.navFill)
                {
                    currentBoundary.Add(n);
                    n.navFill = true;
                }
            }
            currentBoundary.RemoveAt(0);
            volume.Add(curr);
        }
        return volume;
    }

    public Node TryGetNode(Vector3 position, int rStart, int rEnd, isValidNode isValid, object o)
    {
        Node start = getClosestNodeTo(position);
        if (start == null)
        {
            return null;
        }

        if (rStart == rEnd)
        {
            return getValidNodeInSphere(start, rStart, isValid, o);
        }

        int delta = (rEnd - rStart) / Math.Abs(rEnd - rStart);
        for (int r = rStart; r != rEnd; r += delta)
        {
            Node validNode = getValidNodeInSphere(start, r, isValid, o);
            if (validNode != null)
            {
                return validNode;
            }
        }
        return null;
    }

    Node getValidNodeInSphere(Node n, int radius, isValidNode isValid, object o)
    {
        Vector3 center = getMapLocFromPosition(n.location);
        int x0 = (int)center.x;
        int y0 = (int)center.y;
        int z0 = (int)center.z;

        List<Node> sphere = new List<Node>();
        for (int h = 0; h <= radius; h++)
        {
            int currRad = (int)Math.Sqrt(radius * radius - h * h);
            Node validNode = getValidNodeInCircle(x0, y0 + h, z0, currRad, isValid, o);
            if (validNode != null)
            {
                return validNode;
            }

            if (h != 0)
            {
                validNode = getValidNodeInCircle(x0, y0 - h, z0, currRad, isValid, o);
                if (validNode != null)
                {
                    return validNode;
                }
            }
        }
        return null;
    }
    Node getValidNodeInCircle(int x0, int y0, int z0, int radius, isValidNode isValid, object o)
    {
        int x = radius - 1;
        int z = 0;
        int dx = 1;
        int dz = 1;
        int err = dx - (radius << 1);
        while (x >= z)
        {
            List<Node> circlePart = new List<Node>();
            circlePart.Add(getNodeFromMap(x0 + x, y0, z0 + z));
            circlePart.Add(getNodeFromMap(x0 + z, y0, z0 + x));
            circlePart.Add(getNodeFromMap(x0 - z, y0, z0 + x));
            circlePart.Add(getNodeFromMap(x0 - x, y0, z0 + z));
            circlePart.Add(getNodeFromMap(x0 - x, y0, z0 - z));
            circlePart.Add(getNodeFromMap(x0 - z, y0, z0 - x));
            circlePart.Add(getNodeFromMap(x0 + z, y0, z0 - x));
            circlePart.Add(getNodeFromMap(x0 + x, y0, z0 - z));

            foreach (Node n in circlePart)
            {
                if (n != null)
                {
                    foreach (Node conn in n.connectedNodes)
                    {
                        if (conn != null)
                        {
                            Debug.DrawLine(n.location, conn.location, Color.red, 1.0f);
                        }
                    }
                }
                if (isValid(n, o))
                {
                    return n;
                }
            }


            if (err <= 0)
            {
                z++;
                err += dz;
                dz += 2;
            }

            if (err > 0)
            {
                x--;
                dx += 2;
                err += dx - (radius << 1);
            }
        }
        return null;
    }
    public Vector3 getMapLocFromPosition(Vector3 pos)
    {
        Vector3 start = new Vector3(transform.position.x - xBound * spaceBetweenNodes * 0.5f, transform.position.y - yBound * spaceBetweenNodes * 0.5f, transform.position.z - zBound * spaceBetweenNodes * 0.5f);
        int zNode = Mathf.RoundToInt(((pos - start) / spaceBetweenNodes).z);
        int yNode = Mathf.RoundToInt(((pos - start) / spaceBetweenNodes).y);
        int xNode = Mathf.RoundToInt(((pos - start) / spaceBetweenNodes).x);
        return new Vector3(xNode, yNode, zNode);
    }
}

public enum PathState { COMPLETE, NO_PATH, INCOMPLETE, NULL };

//returns whether a node is valid or not.
public delegate bool isValidNode(Node n, object measure);