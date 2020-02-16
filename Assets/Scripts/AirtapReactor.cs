using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirtapReactor : MonoBehaviour
{
    //the function to call if there is an airtap.
    AirtapFunction onAirTap = null;

    //sets the airtap function.
    public void setAirtapFunction(AirtapFunction onAirTap)
    {
        this.onAirTap = onAirTap;
    }

    //calls the airtap function if it has been defined.
    public void airtap()
    {
        if(onAirTap != null){onAirTap(); Debug.Log("airTap action isn't null"); }
    }
}
public delegate void AirtapFunction();
