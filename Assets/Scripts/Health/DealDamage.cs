using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour {
    
    // Attach this script to projectiles

    public int damage = 10;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hit = collision.gameObject;
        if(hit != null && (hit.name == "player" || hit.name == "bot")) // check if hit exists, and is a player or bot
        {
            hit.GetComponent<Health>().TakeDamage(damage);
        }
        
        // Destroy(this); <-- do this here or somewhere else
    }
}
