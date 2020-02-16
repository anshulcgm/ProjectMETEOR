using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class smokeEffectBehavior : MonoBehaviour
{

    List<Enemy> botsWithinSmokeDistance = new List<Enemy>();
    DateTime start;

    public float numSecondsToDelete = 5.0f;
    // Use this for initialization
    void Start()
    {
        start = DateTime.Now;
        List<Enemy> botsFound = GameObject.FindGameObjectWithTag("METEOR").GetComponent<EnemyManager>().enemies;

        foreach (Enemy bot in botsFound)
        {
            float distance = Vector3.Distance(transform.position, bot.transform.position);
            Debug.Log("The distance between the smoke and the bot is " + distance + " meters");
            if (distance <= 3f)
            {
                botsWithinSmokeDistance.Add(bot);
            }
        }
        Debug.Log("There are " + botsWithinSmokeDistance.Count + " bots found within 3 meters of the smoke");
        foreach (Enemy bot in botsWithinSmokeDistance)
        {
            bot.activated = false;
            bot.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if we reached the time, then enable the bots we disabled and destroy this gameobject
        if ((DateTime.Now - start).TotalSeconds > numSecondsToDelete)
        {
            foreach (Enemy bot in botsWithinSmokeDistance)
            {
                bot.activated = true;
            }
            Destroy(gameObject);
        }
    }
}

