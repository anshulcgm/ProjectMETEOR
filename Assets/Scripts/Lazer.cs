using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lazer : MonoBehaviour
{
    GameObject origin;
    float damage = 0.0f;
    float timedDeleteSec = 3.0f;
    //shoots this lazer
    private GameObject statsMenu;

   
    private void Awake()
    {
        GetComponent<AudioSource>().pitch = Random.Range(0.8f, 1.2f);
        statsMenu = GameObject.FindGameObjectWithTag("Menu");
    }
    public void shoot (Vector3 direction, float speed, float damage, GameObject origin)
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
        if (!coll.gameObject.Equals(statsMenu.GetComponent<UI>().playerObj)){
            return;
        }
        if(gameObject.name == "laser")
        {
            statsMenu.GetComponent<UI>().player.damagePlayer(30f);
            Debug.Log("Applying regular damage to the player");
        }
        else if(gameObject.name == "SniperLaser")
        {
            statsMenu.GetComponent<UI>().player.damagePlayer(150f);
        }
        Destroy(gameObject);
    }
}
