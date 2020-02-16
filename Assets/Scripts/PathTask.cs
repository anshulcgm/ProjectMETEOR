using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTask
{
    //holds the start and finish positions
    public Vector3 start;
    public Vector3 finish;

    //holds the path info
    public PathHolder pathHolder { get; private set; }

    //holds the old path info
    public PathHolder oldPathHolder { get; private set; }

    public bool finishedTask { get; private set; }

    public bool isMeteor = false;


    int costIncrease = 0;

    //initialization
    public PathTask(Vector3 start, Vector3 finish, PathHolder oldPathHolder, int costIncrease, bool isMeteor)
    {
        this.isMeteor = isMeteor;
        this.start = start;
        this.finish = finish;
        pathHolder = null;
        this.oldPathHolder = oldPathHolder;
        this.costIncrease = costIncrease;
        finishedTask = false;
        if (costIncrease == 0)
        {
            return;
        }

        //decrease costs for old path
        if (oldPathHolder != null && oldPathHolder.nodePath != null)
        {
            foreach (Node n in oldPathHolder.nodePath)
            {
                if (n == null) { continue; }
                n.cost -= costIncrease;
            }
        }
    }

    //completes the task. If the task has already been completed, it does nothing.
    public void finishTask(List<Node> nodePath, List<Vector3> path, float pathLength)
    {
        //if we haven't already finished this task
        if (!finishedTask)
        {

            //make a new pathHolder
            pathHolder = new PathHolder();
            //give it all these characteristicts
            pathHolder.nodePath = nodePath;
            pathHolder.path = path;
            pathHolder.pathLength = pathLength;
            //make sure you can't finish the task again.
            finishedTask = true;

            if (costIncrease == 0)
            {
                return;
            }

            //increase costs for current path, draws path.
            if (nodePath != null)
            {
                Node prev = null;
                foreach (Node n in nodePath)
                {
                    if (n == null) { continue; }
                    n.cost += costIncrease;
                    if (prev != null)
                        //Debug.DrawLine(prev.location, n.location,Color.green, 1.0f);

                        prev = n;
                }
            }
        }
    }
}

//holds the node path, the Vector 3 path, and the length of the path
public class PathHolder
{
    public List<Node> nodePath;
    public List<Vector3> path;
    public float pathLength;
}
