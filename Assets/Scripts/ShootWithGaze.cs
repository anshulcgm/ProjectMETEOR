using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System;
using UnityEngine.VR.WSA.Input;
public class ShootWithGaze : MonoBehaviour, IInputClickHandler
{
    public static bool inTestMode = false;

    public GameObject laserPrefab;
    public GameObject explosivePrefab;
    public GameObject smokePrefab;

    
    private Quaternion rotationVector;


    private GameObject instantiatedProjectile;

    private int projectileMode = 0;

    //public TextMesh projectileStatus; 

    private Weapons projectile; 
    GestureRecognizer gestureRecognizer;

    private float laserfireRate = 0.50f;
    private float explosivefireRate = 2.0f;
    private float smokeFireRate = 15.0f;

    private float laserNextFire = 0.0f;
    private float explosiveNextFire = 0.0f;
    private float smokeNextFire = 0.0f;
    /// <summary>
    /// Pushing the gameobject in the fallback input stack only works in 
    /// the Unity editor, so it has been kept there for testing purposes
    /// while a GestureRecognizer is used to detect airtaps in the Hololens
    /// </summary>
    private void Start()
    {
#if UNITY_EDITOR
        //InputManager.Instance.PushFallbackInputHandler(gameObject);
#else
        gestureRecognizer = new GestureRecognizer();
        gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap); //Sets airtap as a recognizable gesture
        
        StartGestureRecognizer();
        gestureRecognizer.TappedEvent += GestureRecognizer_TappedEvent; //Calls an event when airtap occurs similar to OnInputClicked
#endif
        //projectileModeChangeToLaser();
        //Sets preset mode to laser
        projectileModeChangeToLaser();

        
    }
    public void projectileModeChangeToLaser()
    {
        projectileMode = 1;
        Debug.Log("Projectile mode changed to laser");
        //projectileStatus.text = "Projectile Mode is Laser";
        //Changes projectileMode parameter so laser is fired
    }
    public void projectileModeChangeToExplosive()
    {
        projectileMode = 2;
        Debug.Log("Projectile mode changed to Explosive");
        //projectileStatus.text = "Projectile Mode is Explosive";
        //Changes projectileMode parameter so Explosive is fired
    }
    public void projectileModeChangeToSmoke()
    {
        projectileMode = 3;
        Debug.Log("Projectile mode changed to Smoke");
        //projectileStatus.text = "Projectile Mode is Smoke";
        //Changes projectileMode parameter so Smoke Bomb is fired  
    }
    private void GestureRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        actionOnAirTapClick();
    }
    private void StartGestureRecognizer()
    {
        gestureRecognizer.StartCapturingGestures();
    }
    private void setRotationVectorOfProjectile()
    {
        rotationVector = new Quaternion();
        rotationVector.SetLookRotation(Camera.main.transform.forward, Camera.main.transform.up); //Sets projectile Rotation to where user is looking
        rotationVector *= Quaternion.Euler(0, 90, 0); //Rotates projectile so tip is facing user

    }
    public virtual void OnInputClicked(InputClickedEventData eventData)
    {
        actionOnAirTapClick();
    }
    
    private void actionOnAirTapClick()
    {
            //if we're in the test mode
            if (inTestMode)
            {
                Debug.Log("airtap preformed");
                //call the airtap function on the object we're looking at.
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
                {
                    for (int i = 0; i < tapRequests.Count; i++)
                    {
                        tapRequests[0].complete(hit.point);
                        tapRequests.RemoveAt(0);
                    }

                    Debug.DrawLine(Camera.main.transform.position, hit.point, Color.blue, 20.0f);
                    Debug.Log("airtap hit: " + hit.collider.gameObject.name);
                    AirtapReactor airtapReactor = hit.collider.gameObject.GetComponent<AirtapReactor>();
                    if (airtapReactor != null)
                    {
                        Debug.Log("airtap action preformed");
                        airtapReactor.airtap();
                    }
                }


            }
            //otherwise, shoot the lazer
            else
            {

                if (projectileMode == 1)
                {
                    if(Time.time > laserNextFire)
                    {
                    //instantiate laser 
                      laserNextFire = Time.time + laserfireRate;
                      setRotationVectorOfProjectile();
                      instantiatedProjectile = Instantiate(laserPrefab, Camera.main.transform.position, rotationVector);
                }
                    
                }
                else if (projectileMode == 2)
                {
                    if(Time.time > explosiveNextFire)
                    {
                      //instantiate explosive
                      explosiveNextFire = Time.time + explosivefireRate;
                      setRotationVectorOfProjectile();
                      instantiatedProjectile = Instantiate(explosivePrefab, Camera.main.transform.position, rotationVector);
                    }
                    
                }
                else if (projectileMode == 3)
                {
                    if(Time.time > smokeNextFire)
                    {
                       //instantiate smoke
                       setRotationVectorOfProjectile();
                       instantiatedProjectile = Instantiate(smokePrefab, Camera.main.transform.position, rotationVector);
                    }
                     
                    
                }
            }  
    }

    List<TapRequest> tapRequests = new List<TapRequest>();

    public void addTapRequest(TapRequest t)
    {
        if(!tapRequests.Contains(t))
        {
            tapRequests.Add(t);
        }        
    }
}
public class TapRequest
{
    public object requester { get; private set; }
    public Vector3 tapPos { get; private set; }
    public bool tapReqComplete {get; private set;}

    public TapRequest(object requester)
    {
        tapPos = Vector3.negativeInfinity;
        tapReqComplete = false;
        this.requester = requester;
    }

    public void complete(Vector3 tapPos)
    {
        this.tapPos = tapPos;
        tapReqComplete = true;
    }

    public override bool Equals(object t)
    {
        if(t.GetType().Equals(this.GetType()))
        {
            return ((TapRequest)t).requester.Equals(requester);
        }
        return false;
    }
}
