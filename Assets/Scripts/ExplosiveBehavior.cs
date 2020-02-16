using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
public class ExplosiveBehavior : MonoBehaviour
{
    private Vector3 initialPosition;
    private Vector3 destination;
    private Vector3 explosiveDirection;

    public float speed;

    private GameObject meteor;

    public GameObject particleEffectPrefab;

    public Sound_Background sound;

    public List<Enemy> botsFound;
    private Rigidbody rb;

    private bool explosiveFired;

    public float explosionForce = 1000f;
    private void Awake()
    {
        initialPosition = Camera.main.transform.position;
        destination = GazeManager.Instance.HitPosition;
        rb = GetComponent<Rigidbody>();
        explosiveDirection = (destination - initialPosition).normalized;
        explosiveFired = true;
        botsFound = new List<Enemy>();
        meteor = GameObject.FindGameObjectWithTag("METEOR");
        sound = Camera.main.GetComponent<Sound_Background>();
        sound.PlayExplosive();
    }
    private void FixedUpdate()
    {
        if (explosiveFired)
        {
            rb.velocity = explosiveDirection * speed;
        }
    }
    private void Update()
    {
        if (Vector3.Distance(initialPosition, transform.position) >= 15.0f)
        {
            Debug.Log("Explosive destroyed due to max distance");
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //botsFound = Physics.RaycastAll(transform.position, )
        Debug.Log("Collision Detected");
        List<Enemy> botsWithinExplosionDistance = new List<Enemy>();
        Debug.Log("Collision detected with " + collision.gameObject.name);
        botsFound = meteor.GetComponent<EnemyManager>().enemies;
        Debug.Log("Bots found list has " + botsFound.Count + " elements");
        //Attach damage function
        //Damage is inversely proportional to radius
        //100/((r + 0.01)^2) is the damage done to each bot within 2.5 meters
        foreach (Enemy bot in botsFound)
        {
            float distance = Vector3.Distance(collision.transform.position, bot.transform.position);
            Debug.Log("The distance between the explosive and the bot is " + distance + " meters");
            if (distance <= 2.5f)
            {
                botsWithinExplosionDistance.Add(bot);
            }
        }
        Debug.Log("There are " + botsWithinExplosionDistance.Count + " bots found within 2.5 meters of the explosion");
        foreach (Enemy bot in botsWithinExplosionDistance)
        {
            float radius = Vector3.Distance(collision.transform.position, bot.transform.position);
            float damageApplied = 100 / (radius + 0.5f);
            Rigidbody rb = bot.GetComponent<Rigidbody>();
            Vector3 force = (bot.transform.position - transform.position).normalized * explosionForce/(radius);
            rb.AddForce(force);
            bot.damage(damageApplied);
        }
       GameObject particleEffect = Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
        sound.PlayExplosion();
        Destroy(gameObject);
    }
}
