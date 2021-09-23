﻿using gameAPI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles RHS Finder (district finder) UI
/// </summary>
public class FinderUI : MonoBehaviour
{

    public Canvas finderCanvas;
    public GameObject finderObject;
    public Image finderImage;

    private bool isActive;

    //Scrolling
    private int maxNumOfScrollItems;                                //max number of items in scrollable list
    private int numOfScrollItemsVisible = 21;                       //max number of items visible at any one time
    private int numOfScrollItemsCurrent;                            //number of active items
    private int scrollHighlightIndex = -1;                          //current highlight index (doesn't matter if shown as highlighted or not)
    private int scrollMaxHighlightIndex = -1;                       //numOfScrollItemsCurrent - 1

    #region Static Instance...

    private static FinderUI finderUI;

    /// <summary>
    /// provide a static reference to FinderUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static FinderUI Instance()
    {
        if (!finderUI)
        {
            finderUI = FindObjectOfType(typeof(FinderUI)) as FinderUI;
            if (!finderUI)
            { Debug.LogError("There needs to be one active FinderUI script on a GameObject in your scene"); }
        }
        return finderUI;
    }

    #endregion

    #region Initialise
    /// <summary>
    /// Initialise. Conditional activiation depending on player side for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {

        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
                SubInitialiseAssert();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadAtStart:
                SubInitialiseAssert();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadGame:
                SubInitialiseAssert();
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

    #region Initialise SubMethods...

    #region SubInitialiseAssert
    private void SubInitialiseAssert()
    {
        Debug.Assert(finderCanvas != null, "Invalid finderCanvas (Null)");
        Debug.Assert(finderObject != null, "Invalid finderCanvas (Null)");
        Debug.Assert(finderImage != null, "Invalid finderImage (Null)");
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //set to False
        isActive = false;
        finderImage.gameObject.SetActive(true);
        finderObject.SetActive(true);
        finderCanvas.gameObject.SetActive(false);
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.FinderOpen, OnEvent, "FinderUI.cs");
        EventManager.i.AddListener(EventType.FinderClose, OnEvent, "FinderUI.cs");
    }
    #endregion

    #endregion


    #region OnEvent
    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.FinderOpen:
                SetFinder();
                break;
            case EventType.FinderClose:
                CloseFinder();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion


    #region SetFinder
    /// <summary>
    /// Display finder UI
    /// </summary>
    private void SetFinder()
    {
        //toggle off finder UI
        EventManager.i.PostNotification(EventType.FinderSideTabClose, this, null, "FinderUI.cs -> SetFinder");
        //open finder UI
        finderCanvas.gameObject.SetActive(true);
        //set modal status
        GameManager.i.guiScript.SetIsBlocked(true);
        //turn off any alert message
        GameManager.i.alertScript.CloseAlertUI(true);
        //set game state
        ModalStateData package = new ModalStateData();
        package.mainState = ModalSubState.InfoDisplay;
        package.infoState = ModalInfoSubState.Finder;
        GameManager.i.inputScript.SetModalState(package);
        Debug.LogFormat("[UI] FinderUI.cs -> SetFinder{0}", "\n");
    }
    #endregion

    #region CloseFinder
    /// <summary>
    /// Close Finder UI
    /// </summary>
    private void CloseFinder()
    {
        //close finder UI
        finderCanvas.gameObject.SetActive(false);
        //toggle on finder UI
        EventManager.i.PostNotification(EventType.FinderSideTabOpen, this, null, "FinderUI.cs -> CloseFinder");
        //unblock
        GameManager.i.guiScript.SetIsBlocked(false);
        //set game state
        GameManager.i.inputScript.ResetStates();
        Debug.LogFormat("[UI] FinderUI.cs -> CloseFinder{0}", "\n");
    }
    #endregion
}
