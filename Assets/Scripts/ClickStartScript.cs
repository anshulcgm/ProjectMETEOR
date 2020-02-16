using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine.VR.WSA.Input;
public class ClickStartScript : MonoBehaviour, IInputClickHandler {
    public GameObject spatial;
    public GameObject spatialBillboard;

    public GameObject cursor;

    public Sound_Background sound;
    GestureRecognizer airTap;
    private void Start()
    {
        if (spatial != null && spatialBillboard != null)
        {
            Debug.Log("Spatial and spatialBillboard game objects found");
        }
        if (spatial == null || spatialBillboard == null)
        {
            Debug.Log("Either spatial or spatialBillboard objects were not found");
        }
        sound = Camera.main.GetComponent<Sound_Background>();
        InputManager.Instance.PushFallbackInputHandler(gameObject);
        airTap = new GestureRecognizer();
        airTap.SetRecognizableGestures(GestureSettings.Tap);
        airTap.StartCapturingGestures();
        airTap.TappedEvent += Gesture_Recognizer_TappedEvent;
    }

    private void Gesture_Recognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        Debug.Log("Gesture captured from ClickStartScript");
        if(Vector3.Distance(transform.position, cursor.transform.position) <= 0.15f)
        {
            AirTapAction();
        }
        
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("OnInputClicked detected on play button");
        AirTapAction();
    }
    private void AirTapAction()
    {
        sound.playLaunch();
        
        spatial.SetActive(true);
        spatialBillboard.SetActive(true);
        transform.parent.gameObject.SetActive(false);
    }
}
