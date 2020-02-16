using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

    // Attach to player (and bots, though bots will not require a healthbar display)

    public const int maxHealth = 100;
    public int currentHealth = maxHealth;
    public Transform healthBar;
    private void Start()
    {
        TakeDamage(38);
    }
    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth); // subtract damage, clamped between 0 and max
        DisplayHealth(); // unnecessary for bots

        if (currentHealth == 0)
        {
            Destroy(this); // if health reaches 0, this object is destroyed
        }
    }

    public void RemoveDamage(int amount) // healing
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        DisplayHealth(); // unnecessary for bots
    }

    public void DisplayHealth()
    {
        healthBar.position = new Vector3(healthBar.position.x - (maxHealth - currentHealth) * healthBar.localScale.x / 200, healthBar.position.y, healthBar.position.z);
        healthBar.localScale = new Vector3(healthBar.localScale.x * currentHealth/maxHealth, healthBar.localScale.y, healthBar.localScale.z);
       // healthBar.position = new Vector3(healthBar.position.x - (maxHealth - currentHealth) * healthBar.localScale.x / 200, healthBar.position.y, healthBar.position.z);
    }
}
