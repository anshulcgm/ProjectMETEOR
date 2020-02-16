using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperLazer : MonoBehaviour {

    GameObject origin;
    float damage = 0.0f;
    float timedDeleteSec = 3.0f;
    //shoots this lazer

    private void Awake()
    {
        GetComponent<AudioSource>().pitch = 1.0f;
    }
    public void shoot(Vector3 direction, float speed, float damage, GameObject origin)
    {
        this.damage = damage;
        Vector3 normalizedDirection = Vector3.Normalize(direction);
        Rigidbody r = GetComponent<Rigidbody>();
        r.velocity = normalizedDirection * speed;
        this.origin = origin;
    }

    void OnCollisionEnter(Collision coll)
    {
        //if colliding with the player

        Destroy(gameObject);
    }
}
