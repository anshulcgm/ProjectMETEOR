using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Condition
{
    //the condition delegate
    private ConditionFunction [] conditions;
    private EndFunction endFunction;

    public Condition(ConditionFunction[] conditions, EndFunction endFunction)
    {
        this.conditions = conditions;
        this.endFunction = endFunction;
    }

    //evaluates the condition
    public bool evaluateCondition(Enemy e, EnemyTask task)
    {
        //if any one of the conditions are true
        foreach (ConditionFunction c in conditions)
        {
            bool result = c(e, task);
            if (result)
            {
                //call the ending function for this condition - if it exists
                if(endFunction != null)
                { 
                    endFunction();
                }
                //the total condition is true
                return true;
            }
        }
        //otherwise, it's false
        return false;
    }
}

public delegate bool ConditionFunction(Enemy e, EnemyTask task);
public delegate void EndFunction();
