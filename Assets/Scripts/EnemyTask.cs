using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EnemyTask
{
    //the new action we want the enemy to do
    public EnemyAction action;
    //the condition that would signal the completion of the task
    public Condition endCondition;
    //the task priority
    public int taskPriority;
    //the task
    public object task;
    //the task cost
    public int taskCost;

    public EnemyTask(EnemyAction action, Condition endCondition, int taskPriority, object task, int taskCost)
    {
        this.action = action;
        this.endCondition = endCondition;
        this.taskPriority = taskPriority;
        this.task = task;
        this.taskCost = taskCost;
    }

}





