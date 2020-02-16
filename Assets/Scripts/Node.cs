using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IComparable<Node>
{
    public Enemy claimer = null;

    public static double costIncrease = 5.0;
    //number of nodes
    public static int numNodes = 0;

    public bool used = false;
    //the location of the node
    public Vector3 location;
    //the list of connected nodes
    public Node[] connectedNodes;
    //the list of extended connected nodes (after redundancies have been dealt with)
    public Node[] extendedConnectedNodes;
    //the parent node
    public Node parentNode;
    //the node id - different for each node
    public int NODE_ID;
    //the state of the node
    public NodeState state;
    public static float distanceBetweenNodes;
    public double cost = 1.0;
    public double gVal;
    public double hVal;
    public double fVal;
    public bool deactivated;
    public bool occupied;

    public bool navFill = false;

    public bool searched = false;

    public bool addedToLine = false;

    public List<Enemy> reservers = new List<Enemy>();

    public int pointerDir = -1;

    public int index = -1;

    public int searchIndex = -1;

    public float pathLength = 0.0f;

    public Area area = null;
    public DateTime lastCostTime;
    public int islandNum;
    internal bool isPassableForMeteor;

    public bool claim(Enemy e)
    {
        if (claimer == null || claimer.Equals(e))
        {
            claimer = e;
            return true;
        }
        return false;
    }

    public void unClaim()
    {
        claimer = null;
    }

    //call this when the node is made
    public Node(Vector3 location)
    {
        //set the location, parentNode,connectedNodes,Node ID, and state of the node
        this.location = location;
        parentNode = null;
        NODE_ID = numNodes;
        state = NodeState.UNTESTED;
        deactivated = false;
        //increment the number of existing nodes
        numNodes++;
    }

    //attempts to get rid of one-way connections.
    //connects Nodes both ways that are only connected one way.
    public void smoothConnections()
    {
        //go through each of the nodes
        int i = 0;
        foreach (Node n in connectedNodes)
        {
            //if we are connected to n, but n is not connected to us
            if (n != null && isConnectedTo(n) && !n.isConnectedTo(this))
            {
                //connect n to us.
                connectedNodes[i].connectedNodes[(i + 4) % 8] = this;
            }
            i++;
        }
    }

    //returns if this node is directly connected to another node 
    public bool isConnectedTo(Node n)
    {
        foreach (Node currentNode in connectedNodes)
        {
            if (currentNode != null && currentNode.Equals(n))
            {
                return true;
            }
        }
        return false;
    }

    //given the adjacent nodes, gets all the connected nodes (the nodes that are adjacent and accessible)
    public void setConnectedNodes(Node[] adjacentNodes, LayerMask collisionLayer)
    {
        int i = 0;
        connectedNodes = new Node[adjacentNodes.Length];
        foreach (Node currentNode in adjacentNodes)
        {
            if (currentNode != null)
            {
                //if there is nothing between this point and the adjacent point
                if (!Physics.Linecast(location, currentNode.location, collisionLayer))
                {
                    //Debug.DrawLine(location, currentNode.location, Color.cyan, 10.0f);
                    //add the Node to the connected Nodes list
                    connectedNodes[i] = currentNode;
                }
                else
                {
                    connectedNodes[i] = null;
                }
            }
            else
            {
                connectedNodes[i] = null;
            }
            i++;
        }
        extendedConnectedNodes = connectedNodes;
    }

    //iterates back through the "list" of nodes via parentNode - essentially gets the length of the path
    public double getGVal(Node start)
    {
        if (parentNode != null && !parentNode.Equals(start))
        {
            return parentNode.getGVal(start) + Vector3.Distance(parentNode.location, location);
        }
        return 0;
    }

    //gets the distance to the finish node
    public double getHVal(Node finish)
    {
        return 0;
    }

    //the measure of how "good" a path is
    public double getFVal(Node start, Node finish)
    {
        gVal = getGVal(start);
        hVal = getHVal(finish);
        fVal = gVal + hVal;
        return fVal;
    }

    //tests if one node is equal to another
    public bool Equals(Node n)
    {
        if (n == null)
        {
            return false;
        }
        return n.NODE_ID == NODE_ID;
    }

    public int CompareTo(Node other)
    {
        if (other.fVal > fVal)
            return -1;
        if (other.fVal == fVal)
            return 0;
        return 1;
    }
}


public class RetreatNode
{
    public Node node;
    public bool inLineOfSight;

    public RetreatNode(Node node, bool inLineOfSight)
    {
        this.node = node;
        this.inLineOfSight = inLineOfSight;
    }
}



