using gameAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles start of turns advertisements
/// </summary>
public class AdvertUI : MonoBehaviour
{
    [Header("Main Components")]
    public Canvas advertCanvas;
    public GameObject advertObject;
    public Image innerPanel;
    public Image outerPanel;
    public Image logo;
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textTop;
    public TextMeshProUGUI textBottom;

    //private
    private bool isFading;
    private Color outerColour;
    private float flashBorder;
    private float panelOffset;                   //offset distance to get panels off screen during development
    private int maxNameChars;                    //max number of chars in playerName before it's swapped to a default text to prevent overflowing
    private string colourBlue;
    private string colourRed;
    private string endTag;
    private string sizeLarge;

    private float pos_x;
    private float pos_y;

    #region Static Instance
    private static AdvertUI advertUI;

    /// <summary>
    /// Static instance so the advertUI can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static AdvertUI Instance()
    {
        if (!advertUI)
        {
            advertUI = FindObjectOfType(typeof(AdvertUI)) as AdvertUI;
            if (!advertUI)
            { Debug.LogError("There needs to be one active advertUI script on a GameObject in your scene"); }
        }
        return advertUI;
    }
    #endregion

    #region Initialise
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.StartUp:
            case GameState.NewInitialisation:
                SubInitialiseAsserts();
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadAtStart:
                SubInitialiseAsserts();
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }
    #endregion

    #region Initialise SubMethods

    #region SubInitialiseAsserts
    private void SubInitialiseAsserts()
    {
        Debug.Assert(advertCanvas != null, "Invalid advertCanvas (Null)");
        Debug.Assert(advertObject != null, "Invalid advertObject (Null)");
        Debug.Assert(innerPanel != null, "Invalid innerPanel (Null)");
        Debug.Assert(outerPanel != null, "Invalid outerPanel (Null)");
        Debug.Assert(logo != null, "Invalid logo (Null)");
        Debug.Assert(textName != null, "Invalid textName (Null)");
        Debug.Assert(textTop != null, "Invalid textTop (Null)");
        Debug.Assert(textBottom != null, "Invalid textBottom (Null)");
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {

    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {

        //colours
        colourRed = "<color=#f71735>";
        colourBlue = "<color=#55C1FF>";
        endTag = "</color></size>";
        sizeLarge = "<size=130%>";
        //positions
        pos_x = Screen.width / 2;
        pos_y = Screen.height / 2;
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.i.AddListener(EventType.AdvertClose, OnEvent, "AdvertUI");
    }
    #endregion

    #endregion

    /// <summary>
    /// Event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.AdvertClose:
                CloseAdvert();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Initialise and open AdvertUI
    /// </summary>
    public void InitialiseAdvert()
    {
        SetAdvertUI();
    }

    /// <summary>
    /// Open Advert
    /// </summary>
    private void SetAdvertUI()
    {
        //Fixed position at screen centre
        advertObject.transform.position = new Vector3(pos_x, pos_y);
        //set states
        ModalStateData package = new ModalStateData() { mainState = ModalSubState.Advert };
        GameManager.i.inputScript.SetModalState(package);
        GameManager.i.guiScript.SetIsBlocked(true);
        Debug.LogFormat("[UI] AdvertUI.cs -> OpenAdvert{0}", "\n");
        //open canvas
        advertCanvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// Close advert UI
    /// </summary>
    private void CloseAdvert()
    {
        advertCanvas.gameObject.SetActive(false);
        //close advertUI and go straight to MainInfoApp
        GameManager.i.guiScript.waitUntilDone = false;
    }

    //new methods above here
}
