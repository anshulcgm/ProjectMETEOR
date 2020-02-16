using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
public class airtaptest : MonoBehaviour {
    public virtual void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("Airtap Detected");
    }
}
