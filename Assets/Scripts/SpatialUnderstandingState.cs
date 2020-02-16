using System;
using System.Collections;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine.VR.WSA.Input;
public class SpatialUnderstandingState : Singleton<SpatialUnderstandingState>, IInputClickHandler, ISourceStateHandler
{
    public float MinAreaForStats = 0.01f; //5
    public float MinAreaForComplete = 0.01f; //50
    public float MinHorizAreaForComplete = 0.01f; //25
    public float MinWallAreaForComplete = 0.01f; //10

    public TextMesh DebugDisplay;
    public TextMesh DebugSubDisplay;

    public GameObject appManager;
    public GameObject SpatialUnderstandingPrefab;
    public GameObject statsMenu;

    private bool _triggered;
    public bool HideText = false;

    private bool ready = false;
    private uint trackedHandsCount = 0;

    private Sound_Background sound;
    GestureRecognizer spatialComplete;

    private string _spaceQueryDescription;

    public string SpaceQueryDescription
    {
        get
        {
            return _spaceQueryDescription;
        }
        set
        {
            _spaceQueryDescription = value;
        }
    }



    public bool DoesScanMeetMinBarForCompletion
    {
        get
        {
            // Only allow this when we are actually scanning
            if ((SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Scanning) ||
                (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
            {
                return false;
            }

            // Query the current playspace stats
            IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
            if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
            {
                return false;
            }
            SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

            // Check our preset requirements
            if ((stats.TotalSurfaceArea > MinAreaForComplete) ||
                (stats.HorizSurfaceArea > MinHorizAreaForComplete) ||
                (stats.WallSurfaceArea > MinWallAreaForComplete))
            {
                return true;
            }
            return false;
        }
    }

    public string PrimaryText
    {
        get
        {
            if (HideText)
                return string.Empty;

            // Display the space and object query results (has priority)
            if (!string.IsNullOrEmpty(SpaceQueryDescription))
            {
                return SpaceQueryDescription;
            }

            // Scan state
            if (SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            {
                switch (SpatialUnderstanding.Instance.ScanState)
                {
                    case SpatialUnderstanding.ScanStates.Scanning:
                        // Get the scan stats
                        IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                        if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                        {
                            return "playspace stats query failed";
                        }

                        // The stats tell us if we could potentially finish
                        if (DoesScanMeetMinBarForCompletion)
                        {
                            return "When ready, air tap to finalize your playspace";
                        }
                        return "Walk around and scan in your playspace";
                    case SpatialUnderstanding.ScanStates.Finishing:
                        return "Finalizing scan (please wait)";
                    case SpatialUnderstanding.ScanStates.Done:
                        ActivateGameObjects(appManager);
                        //ActivateGameObjects(healthCanvas);
                        ActivateGameObjects(statsMenu);
                        //start up the navigation
                        GameObject navMapper = GameObject.FindGameObjectWithTag("navMapper");
                        if (navMapper != null)
                        {
                            navMapper.GetComponent<NavMapper>().Initialize();
                        }


                        //then start up the METEOR
                        GameObject meteor = GameObject.FindGameObjectWithTag("METEOR");
                        if (meteor != null)
                        {
                            meteor.GetComponent<EnemyManager>().Initialize();
                        }
                        //sound = Camera.main.GetComponent<Sound_Background>();
                        //sound.playSoundtrack();
#if WINDOWS_UWP
                        SpatialUnderstandingPrefab.SetActive(false);
#endif
                        Invoke("SetGameObjectInactive", 2);
                        return "Scan complete";
                    default:
                        return "ScanState = " + SpatialUnderstanding.Instance.ScanState;
                }
            }
            return string.Empty;
        }
    }
    private void SetGameObjectInactive()
    {
        gameObject.SetActive(false);
    }
    public Color PrimaryColor
    {
        get
        {
            ready = DoesScanMeetMinBarForCompletion;
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning)
            {
                if (trackedHandsCount > 0)
                {
                    return ready ? Color.green : Color.red;
                }
                return ready ? Color.yellow : Color.white;
            }

            // If we're looking at the menu, fade it out
            float alpha = 1.0f;

            // Special case processing & 
            return (!string.IsNullOrEmpty(SpaceQueryDescription)) ?
                (PrimaryText.Contains("processing") ? new Color(1.0f, 0.0f, 0.0f, 1.0f) : new Color(1.0f, 0.7f, 0.1f, alpha)) :
                new Color(1.0f, 1.0f, 1.0f, alpha);
        }
    }

    public string DetailsText
    {
        get
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.None)
            {
                return "";
            }

            // Scanning stats get second priority
            if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
                (SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
            {
                IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                {
                    return "Playspace stats query failed";
                }
                SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

                // Start showing the stats when they are no longer zero
                if (stats.TotalSurfaceArea > MinAreaForStats)
                {
                    SpatialMappingManager.Instance.DrawVisualMeshes = false;
                    string subDisplayText = string.Format("TotalArea={0:0.0}, HorizontalArea ={1:0.0}, WallArea={2:0.0}", stats.TotalSurfaceArea, stats.HorizSurfaceArea, stats.WallSurfaceArea);
                    subDisplayText += string.Format("\nNumber of Floor Cells ={0}, Number of Ceiling Cells={1}, Number of Platform Cells={2}", stats.NumFloor, stats.NumCeiling, stats.NumPlatform);
                    subDisplayText += string.Format("\npaintMode={0}, seenCells={1}, notSeen={2}", stats.CellCount_IsPaintMode, stats.CellCount_IsSeenQualtiy_Seen + stats.CellCount_IsSeenQualtiy_Good, stats.CellCount_IsSeenQualtiy_None);
                    return subDisplayText;
                }
                return "";
            }
            return "";
        }
    }

    private void Update_DebugDisplay()
    {
        // Basic checks
        if (DebugDisplay == null)
        {
            return;
        }

        // Update display text
        DebugDisplay.text = PrimaryText;
        DebugDisplay.color = PrimaryColor;
        DebugSubDisplay.text = DetailsText;
    }

    private void Start()
    {
#if UNITY_EDITOR
        InputManager.Instance.PushFallbackInputHandler(gameObject);
#else
        spatialComplete = new GestureRecognizer();
        spatialComplete.SetRecognizableGestures(GestureSettings.Tap);
        spatialComplete.StartCapturingGestures();
        spatialComplete.TappedEvent += SpatialComplete_TappedEvent;
#endif
        SurfaceMeshesToPlanes.Instance.MakePlanes();
        SurfaceMeshesToPlanes.Instance.MakePlanesComplete += Instance_MakePlanesComplete;
        
    }

    private void Instance_MakePlanesComplete(object source, EventArgs args)
    {
        Debug.Log("Planes completed");

        
    }

    private void SpatialComplete_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        if (ready &&
             (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
             !SpatialUnderstanding.Instance.ScanStatsReportStillWorking)
        {
            SpatialUnderstanding.Instance.RequestFinishScan();
            //sound = Camera.main.GetComponent<Sound_Background>();
            //sound.playSoundtrack();
        }
        spatialComplete.StopCapturingGestures();
        
    }

    // Update is called once per frame
    private void Update()
    {
        // Updates
        Update_DebugDisplay();

        if (!_triggered && SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
        {
            _triggered = true;
        }
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("OnInputClicked fired from SpatialUnderstandingState");
        if (ready &&
             (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
             !SpatialUnderstanding.Instance.ScanStatsReportStillWorking)
        {
            SpatialUnderstanding.Instance.RequestFinishScan();
        }
    }

    void ISourceStateHandler.OnSourceDetected(SourceStateEventData eventData)
    {
        trackedHandsCount++;
    }

    void ISourceStateHandler.OnSourceLost(SourceStateEventData eventData)
    {
        trackedHandsCount--;
    }
    private void ActivateGameObjects(GameObject toActivate)
    {
        toActivate.SetActive(true);
    }
}