using gameAPI;
using packageAPI;
using System.Collections.Generic;
using TMPro;
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
    public List<Button> listOfButtons;                              //index linked
    public List<ButtonInteraction> listOfInteractions;              //index linked
    public List<TextMeshProUGUI> listOfTexts;                       //index linked

    private bool isActive;
    private string colourEnd;

    //Scrolling
    private int maxNumOfScrollItems;                                //max number of items in scrollable list
    private int numOfScrollItemsVisible = 21;                       //max number of items visible at any one time
    private int numOfScrollItemsCurrent;                            //number of active items
    private int scrollHighlightIndex = -1;                          //current highlight index (doesn't matter if shown as highlighted or not)
    private int scrollMaxHighlightIndex = -1;                       //numOfScrollItemsCurrent - 1

    //collections
    List<FindButtonData> listOfStartData = new List<FindButtonData>();          //start menu list
    List<FindButtonData> listOfDistrictData = new List<FindButtonData>();       //district list


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
        Debug.Assert(listOfButtons.Count > 0, "Invalid listOfButtons (Empty)");
        Debug.Assert(listOfInteractions.Count > 0, "Invalid listOfInteractions (Empty)");
        Debug.Assert(listOfTexts.Count > 0, "Invalid listOfTexts (Empty)");
        Debug.AssertFormat(listOfButtons.Count == listOfInteractions.Count, "Count Mismatch: listOfButtons {0} records, listOfInteractions {1} records", listOfButtons.Count, listOfInteractions.Count);
        Debug.AssertFormat(listOfButtons.Count == listOfTexts.Count, "Count Mismatch: listOfButtons {0} records, listOfTexts {1} records", listOfButtons.Count, listOfTexts.Count);
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //fast access
        colourEnd = GameManager.i.colourScript.GetEndTag();
        //set to False
        isActive = false;
        finderImage.gameObject.SetActive(true);
        finderObject.SetActive(true);
        finderCanvas.gameObject.SetActive(false);
        //Initialisations
        InitialiseFinder();
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

    public void InitialiseFinder()
    {
        //start list
        listOfStartData.Add(new FindButtonData() { descriptor = "Districts", colour = ColourType.neutralText, eventType = EventType.FinderDistricts});
        listOfStartData.Add(new FindButtonData()
        { descriptor = string.Format("{0}   Corporate", GameManager.i.guiScript.corporateChar), colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc0});
        listOfStartData.Add(new FindButtonData()
        { descriptor = string.Format("{0}   Gated", GameManager.i.guiScript.gatedChar), colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc1});
        listOfStartData.Add(new FindButtonData()
        { descriptor = string.Format("{0}   Government", GameManager.i.guiScript.governmentChar), colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc2});
        listOfStartData.Add(new FindButtonData()
        { descriptor = string.Format("{0}   Industrial", GameManager.i.guiScript.industrialChar), colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc3});
        listOfStartData.Add(new FindButtonData()
        { descriptor = string.Format("{0}   Research", GameManager.i.guiScript.researchChar), colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc4});
        listOfStartData.Add(new FindButtonData()
        { descriptor = string.Format("{0}   Sprawl", GameManager.i.guiScript.sprawlChar), colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc5});
        listOfStartData.Add(new FindButtonData()
        { descriptor = string.Format("{0}   Utility", GameManager.i.guiScript.utilityChar), colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc6});
        listOfStartData.Add(new FindButtonData() { descriptor = "Targets", colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.ShowTargets});
        listOfStartData.Add(new FindButtonData() { descriptor = "Spiders", colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.ShowSpiders});
        listOfStartData.Add(new FindButtonData() { descriptor = "Tracers", colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.ShowTracers});
        listOfStartData.Add(new FindButtonData() { descriptor = "Teams", colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.ShowTeams});
        listOfStartData.Add(new FindButtonData() { descriptor = "Contacts", colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.ShowContacts});
        listOfStartData.Add(new FindButtonData() { descriptor = "Crisis Districts", colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.CrisisNodes});
        listOfStartData.Add(new FindButtonData() { descriptor = "Cures", colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.CureNodes});
        listOfStartData.Add(new FindButtonData() { descriptor = "Spotted", colour = ColourType.cyberText, eventType = EventType.NodeDisplay, nodeType = NodeUI.PlayerKnown});
        //district list
    }

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
        //initialise Buttons
        SetFinderButtons();

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

    #region SetFinderButtons
    /// <summary>
    /// Set, or Update, finder buttons (depends on whether at start menu or districts menu)
    /// </summary>
    private void SetFinderButtons(bool isStart = true)
    {
        int numOfButtons;
        if (isStart == true)
        {
            //Start Menu
            numOfButtons = Mathf.Min(listOfButtons.Count, listOfStartData.Count);
            for (int i = 0; i < numOfButtons; i++)
            {
                listOfInteractions[i].SetButton(listOfStartData[i].eventType);
                listOfTexts[i].text = string.Format("{0}{1}{2}", GameManager.i.colourScript.GetColour( listOfStartData[i].colour), listOfStartData[i].descriptor, colourEnd);
            }
        }
        else
        {
            //District menu

        }
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
