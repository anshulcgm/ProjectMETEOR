using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float playerHealth;
    private float playerRegenRate;
    private float playerMaxHealth;

    private int gunnersKilled;
    private int spinnersKilled;
    private int snipersKilled;
    private int seekersKilled;

    private int[] botsKilled;
    private float updateTime;
   
    public Player()
    {
        playerHealth = 500.0f;
        playerMaxHealth = 500.0f;
        playerRegenRate = 25.0f;

        gunnersKilled = 0;
        spinnersKilled = 0;
        snipersKilled = 0;
        seekersKilled = 0;

        botsKilled = new int[4];
    }

    public void regenPlayer()
    {

        if(playerHealth < playerMaxHealth)
        {
            playerHealth += playerRegenRate;
        }

        if(playerHealth > playerMaxHealth)
        {
            playerHealth = playerMaxHealth;
        }
    }

    public void damagePlayer(float healthToRemove)
    {
        playerHealth -= healthToRemove;
    }
    public void killGunner()
    {
        gunnersKilled++;
    }
    public void killSpinner()
    {
        spinnersKilled++;
    }
    public void killSniper()
    {
        snipersKilled++;
    }
    public void killSeeker()
    {
        seekersKilled++;
    }
    public int[] returnBotsKilled()
    {
        botsKilled[0] = gunnersKilled;
        botsKilled[1] = snipersKilled;
        botsKilled[2] = spinnersKilled;
        botsKilled[3] = seekersKilled;
        return botsKilled;
    }
    private void Update()
    {
        if(playerHealth < playerMaxHealth)
        {
            playerHealth += playerRegenRate * Time.time;
        }
    }
}
