using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpDamage : MonoBehaviour
{
    private GameObject statsMenu;
    private void Start()
    {
        statsMenu = GameObject.FindGameObjectWithTag("Menu");
        float radius = Vector3.Distance(transform.position, statsMenu.GetComponent<UI>().playerObj.gameObject.transform.position);
        float damageApplied = 100 / (radius + 1.0f);
        statsMenu.GetComponent<UI>().player.damagePlayer(damageApplied);
        Debug.Log("Player health is at " + statsMenu.GetComponent<UI>().player.playerHealth);
        Destroy(gameObject);
    }
}
