using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all debug only graphics. Attached to PanelManager.cs
/// </summary>
public class DebugGraphics : MonoBehaviour
{

    [Tooltip("Debug plane to show central region as specified by AIManager.cs -> nodeGeographicCentre")]
    public GameObject centrePlane;

    private static DebugGraphics debugGraphics;


    /// <summary>
    /// provide a static reference to the Debug Graphic that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static DebugGraphics Instance()
    {
        if (!debugGraphics)
        {
            debugGraphics = FindObjectOfType(typeof(DebugGraphics)) as DebugGraphics;
            if (!debugGraphics)
            { Debug.LogError("There needs to be one active DebugGraphic script on a GameObject in your scene"); }
        }
        return debugGraphics;
    }


    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.Tutorial:
            case GameState.NewInitialisation:
                SubInitialiseSessionStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseSessionStart();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //adjust scale of centre Plane
        float scaleFactor = GameManager.i.aiScript.nodeGeographicCentre;
        float newScale = 1.1f * scaleFactor / 100f;
        centrePlane.transform.localScale = new Vector3(newScale, 1.15f, newScale);
    }
    #endregion

    #endregion

    /// <summary>
    /// set debug centre pane on/off depending on status
    /// </summary>
    /// <param name="status"></param>
    public void SetCentrePane(bool status)
    { centrePlane.gameObject.SetActive(status); }
}
