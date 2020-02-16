using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Project
{
    //the action we want the enemy to do
    public EnemyAction action { get; private set; }

    
    //the task
    public object task { get; private set; }
    //the enemies cooperating to complete the project
    public List<Enemy> enemiesOnProject = new List<Enemy>();
    //the "priority" of the project (how important is this project relative to others?)
    public int projectPriority = 0;

    public int optimalNumWorkers = 0;

    public EnemyValidator enemyValidator;
    

    public Project(EnemyAction action, object task, int projectPriority, int optimalNumWorkers, EnemyValidator enemyValidator)
    {
        this.action = action;
        this.task = task;
        this.projectPriority = projectPriority;
        this.optimalNumWorkers = optimalNumWorkers;
        this.enemyValidator = enemyValidator;
        if(this.projectPriority < 0)
        {
            this.projectPriority = 0;
        }
    }
    

    //returns true if the action and task are the same, otherwise returns false.
    public override bool Equals(object o)
    {
        if(o.GetType().Equals(typeof(Project)))
        {           
            if((((Project)o).task == null && task != null) || (((Project)o).enemyValidator == null && enemyValidator != null))
            {
                return false;
            }

            return ((Project)o).action.Equals(action) && 
                   ((((Project)o).task == null && task == null) || (((Project)o).task.Equals(task))) &&
                   ((((Project)o).enemyValidator == null && enemyValidator == null) || ((Project)o).enemyValidator.Equals(enemyValidator));
        }
        return false;
    }
}

//
public class Area
{
    public List<Node> area = null;
    private int numNodesSearched = 0;
    private int numRealNodes = 0;

    public Area (List<Node> area)
    {
        if(area == null)
        {
            Debug.Log("WTF! Ur area is null!");
        }
        this.area = area;
        int index = 0;
        foreach(Node n in area)
        {
            if(n == null)
            {
                continue;
            }

            if(n.area != null)
            {
                n.area.numRealNodes--;
            }

            n.searchIndex = index;
            n.searched = false;
            n.area = this;
            index++;
            numRealNodes++;
        }
    }

    public void search(Node n)
    {
        if(inArea(n) && !area[n.searchIndex].searched)
        {
            area[n.searchIndex].searched = true;
            numNodesSearched++;
        }
    }

    public bool inArea(Node n)
    {
        if(n == null || n.area == null)
        {
            return false;
        }
        return n.area.Equals(this);
    }

    public bool areaComplete()
    {
        return numNodesSearched == numRealNodes;
    }
}





