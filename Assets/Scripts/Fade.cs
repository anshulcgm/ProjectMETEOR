using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{

    // [Range(0f, 500f)]
    public float duration;
    private float time;
    // Use this for initialization
    void Start()
    {
        float time = duration;
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime; 
        Debug.Log("There's " +time.ToString()+ " seconds left in particle effect");
        if (time == 0)
        {
            Destroy(this.gameObject);
        }
    }
}
