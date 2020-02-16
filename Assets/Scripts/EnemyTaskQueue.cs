using System.Collections.Generic;
using UnityEngine;

//handles the priority management for the enemy tasks
public class EnemyTaskQueue
{    
    //the list of tasks
    private List<EnemyTask> tasks = new List<EnemyTask>();

    //adds task to list of tasks, or assigns task to variable superTask
    //overrides tasks with same priority level - or overrides tasks with lower priority
    public bool addTask(EnemyTask newTask)
    {                
        int index = 0;
        //loop through the tasks
        foreach (EnemyTask e in tasks)
        {
            //if we find a priority that is less than or equal to ours
            if(e.taskPriority <= newTask.taskPriority)
            {
                //if we find a priority equal to ours
                if (e.taskPriority == newTask.taskPriority)
                {
                    if(e.task.Equals(newTask.task))
                    {
                        return false;
                    }
                    //remove the old task - we'll add ours and override it.
                    tasks.Remove(e);                 
                }

                //add the task and end iteration, we've placed the task at the right index.
                tasks.Insert(index, newTask);
                return true;
            }
            index++;
        }
        //if we didn't find any index where the task could go, add the task to the end of the list
        tasks.Add(newTask);
        return true;
    }
    

    //get the task at the top of the list
    public EnemyTask getTask()
    {
        if(tasks.Count > 0)
        {
            return tasks[0];
        }
        return null;
    }    

    //removes completed tasks
    public void removeCompletedTasks(Enemy e)
    {        
        for(int i = 0; i < tasks.Count; i++)
        {
            if(tasks[i].endCondition.evaluateCondition(e, tasks[i]))
            {
                Debug.Log("removed task " + tasks[i].action + " evaluate condition returned true");
                tasks.Remove(tasks[i]);
                i--;
            }
        }
    }

    //gets you the cost of adding this task (an estimate of how long it will take to get to this task)
    public int getTaskAdditionCost(EnemyTask newTask, Enemy e)
    {
        if(!addTask(newTask))
        {
            tasks.Remove(newTask);
            return -1;
        }

        //if the newTask can't be added
        if (newTask.endCondition.evaluateCondition(e, newTask))
        {
            tasks.Remove(newTask);
            return -1;
        }

        int totalCost = 0;
        int index = tasks.IndexOf(newTask);
        for(int i = 0; i < index; i++)
        {
            totalCost += tasks[i].taskCost;
        }

        tasks.Remove(newTask);
        return totalCost;        
    }

    public new string ToString()
    {
        string s = "";
        foreach(EnemyTask t in tasks)
        {
            s += t.action.ToString() + " ";
        }
        return s;
    }
}
