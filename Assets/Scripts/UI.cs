using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    public Player player;

    public GameObject playerObj;

    public TextMesh gunnersKilledText;
    public TextMesh snipersKilledText;
    public TextMesh spinnersKilledText;
    public TextMesh seekersKilledText;

    public void Start()
    {
        player = new Player();
    }
    public void Update()
    {
        int[] botsKilled = player.returnBotsKilled();
        gunnersKilledText.text = botsKilled[0].ToString();
        snipersKilledText.text = botsKilled[1].ToString();
        spinnersKilledText.text = botsKilled[2].ToString();
        seekersKilledText.text = botsKilled[3].ToString();
        if(player.playerHealth <= 0)
        {
            Debug.Log("Player is dead");
            return;
        }
    }
}
