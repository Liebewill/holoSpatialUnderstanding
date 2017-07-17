using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;

public class SpatialUnderstandingState : Singleton<SpatialUnderstandingState> {

    public float MinAreaForStats = 5.0f;

    public TextMesh DebugDisplay;
    public TextMesh DebugSubDisplay;

    private bool _triggered;
    public bool HideText = false;

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

            //Scan state
            if (SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            {
                switch (SpatialUnderstanding.Instance.ScanState)
                {
                    case SpatialUnderstanding.ScanStates.Scanning:
                        // get the scan states
                        System.IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                        if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                        {
                            return "playspace stats query failed";
                        }
                        return "Walk around and scan your playspace";
                    case SpatialUnderstanding.ScanStates.Finishing:
                        return "Finalizing scan (please wait)";
                    case SpatialUnderstanding.ScanStates.Done:
                        return "Scan Complete";
                    default:
                        return "ScanState = " + SpatialUnderstanding.Instance.ScanState;
                }
            }
            return string.Empty;
        }
    }

    public Color PrimaryColor
    {
        get
        {
            return Color.white;
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

            // Scanning stats gett second priority
            if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
                (SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
            {
                System.IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                {
                    return "Playspace stats query failed";
                }
                SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

                // Start showing the stats when they are no longer zero
                if (stats.TotalSurfaceArea > MinAreaForStats)
                {
                    SpatialMappingManager.Instance.DrawVisualMeshes = false;
                    string subDisplayText = string.Format("totalArea={0:0.0}, horiz={1:0.0}, wall={2:0.0}", stats.TotalSurfaceArea, stats.HorizSurfaceArea, stats.WallSurfaceArea);
                    subDisplayText += string.Format("\nnumFloorCells={0}, numCeilingCells={1}, numPlatformCells={2}", stats.NumFloor, stats.NumCeiling, stats.NumPlatform);
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
        // basic checks
        if (DebugDisplay == null)
        {
            return;
        }
        // update display text
        DebugDisplay.text = PrimaryText;
        DebugDisplay.color = PrimaryColor;
        DebugSubDisplay.text = DetailsText;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        // Updates
        Update_DebugDisplay();
	}
}
