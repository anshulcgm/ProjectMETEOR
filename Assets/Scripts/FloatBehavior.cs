using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatBehavior : MonoBehaviour {
    public float maxFloatSpeed;
    public float floatAccel;
    bool up = true;
    float floatingFactor = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (up)
        {
            floatingFactor += floatAccel;
            if (floatingFactor > maxFloatSpeed)
            {
                up = false;
            }
        }
        else
        {
            floatingFactor -= floatAccel;
            if (floatingFactor < -maxFloatSpeed)
            {
                up = true;
            }
        }
        transform.position += new Vector3(0, floatingFactor * Time.deltaTime, 0);
    }
}
