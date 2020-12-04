using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles all matters relating to ModalTabbedUI
/// </summary>
public class ModalTabbedUI : MonoBehaviour
{
    [Header("Canvases")]
    public Canvas tabbedCanvasMain;

    [Header("Control Objects")]
    public GameObject tabbedObjectMain;

    [Header("Backgrounds")]
    public Image backgroundImage;

    [Header("Page Canvases")]
    public Canvas canvasTab0;
    public Canvas canvasTab1;
    public Canvas canvasTab2;
    public Canvas canvasTab3;
    public Canvas canvasTab4;
    public Canvas canvasTab5;
    public Canvas canvasTab6;

    [Header("Side Tabs")]
    public GameObject sideTab0;
    public GameObject sideTab1;
    public GameObject sideTab2;
    public GameObject sideTab3;

    [Header("Top Tabs")]
    public GameObject topTab0;
    public GameObject topTab1;
    public GameObject topTab2;
    public GameObject topTab3;
    public GameObject topTab4;
    public GameObject topTab5;
    public GameObject topTab6;

    [Header("Controller Buttons")]
    public Button buttonSubordinates;
    public Button buttonPlayer;
    public Button buttonHq;
    public Button buttonReserves;

    [Header("Controller Texts")]
    public TextMeshProUGUI textSubordinates;
    public TextMeshProUGUI textPlayer;
    public TextMeshProUGUI textHq;
    public TextMeshProUGUI textReserves;

    [Header("Help Buttons")]
    public Button buttonHelpClose;

    [Header("Button Interactions")]
    public ButtonInteraction interactClose;
    public ButtonInteraction interactSubordinates;
    public ButtonInteraction interactPlayer;
    public ButtonInteraction interactHq;
    public ButtonInteraction interactReserves;

    [Header("Texts -> Development Only")]
    public TextMeshProUGUI textActorName;
    public TextMeshProUGUI textPageMain;

    [Header("Canvas0 -> Main")]
    public TextMeshProUGUI tab0ActorName;
    public TextMeshProUGUI tab0ActorTitle;
    public TextMeshProUGUI tab0PowerText;
    public TextMeshProUGUI tab0Compatibility;
    public Image tab0ActorImage;
    public Image tab0PanelStatus;
    public Image tab0PanelConflicts;
    public Image tab0PanelFriends;
    public Image tab0PanelConditions;
    public Image tab0PanelCures;
    public TabbedSubHeaderInteraction tab0Header0;
    public TabbedSubHeaderInteraction tab0Header1;
    public TabbedSubHeaderInteraction tab0Header2;
    public TabbedSubHeaderInteraction tab0Header3;
    public TabbedSubHeaderInteraction tab0Header4;

    //help
    private GenericHelpTooltipUI helpClose;

    //tabs (side/top/actorSet)
    private int currentSideTabIndex = -1;                           //side tabs (top to bottom)
    private int currentTopTabIndex = -1;                            //top tabs (left to right)
    private int currentSetIndex = -1;                               //actor sets
    private int maxSideTabIndex;                                    //side tabs only (used for keeping pgUP/DOWN movement within HQ side tabs)
    private int maxTopTabIndex;                                     //top tabs only
    private int maxSetIndex;                                        //actor sets
    private int numOfSideTabs;                                      //keyed off enum.MetaTabSide
    private int numOfTopTabs;                                       //keyed off enum.MetaTabTop
    private int offset = 1;                                         //used with '(ActorHQ)index + offset' to account for the ActorHQ.enum having index 0 being 'None'
    private float sideTabAlpha = 0.50f;                             //alpha level of side tabs when inactive
    private float topTabAlpha = 0.50f;                              //alpha level of top tabs when inactive
    private bool isActive;                                          //true if UI open

    //Page0
    private int maxNumOfConditions = 0;                              //max number of conditions allowed in tab0/page0 subHeader3 'Conditions'
    private int maxNumOfCures = 0;                                   //max number of cures allowed in tab0/page0 subHeader4 'Cures'

    //help tooltips (I don't want this as a global, just a master private field)
    private int x_offset = 200;
    private int y_offset = 40;

    //colours
    private Color sideTabActiveColour;
    private Color sideTabDormantColour;
    private Color portraitColour;
    private Color textSideColour;                                       //generic global for toggling Side Tab text alpha settings for active/dormant
    private Color backgroundColour;
    private Color topTabDormantColour;
    private Color topTabActiveColour;
    private Color tabTextActiveColour;                                 //top tab and controller button text colours
    private Color tabTextDormantColour;                                //top tab and controller button text colours
    private Color tabSubHeaderColour;
    private Color tabSubHeaderTextActive;
    private Color tabSubHeaderTextDormant;
    private Color tabItemHelpActive;
    private Color tabItemHelpDormant;


    //Input data
    TabbedUIData inputData;
    //canvas collection
    private Canvas[] arrayOfCanvas;
    //sideTab collections
    private GameObject[] arrayOfSideTabObjects;
    private TabbedInteractionSide[] arrayOfSideTabItems;
    private TabbedSideTabUI[] arrayOfSideTabInteractions;
    //topTab collections
    private GameObject[] arrayOfTopTabObjects;
    private Image[] arrayOfTopTabImages;
    private TextMeshProUGUI[] arrayOfTopTabTitles;
    private TabbedTopTabUI[] arrayOfTopTabInteractions;
    private TabbedPage[] arrayOfPages;
    //cached Actor sets
    private Actor[] arrayOfActorsTemp;                              //holds all actors for the current page (excludes Player -> not in array)
    private Actor[] arrayOfSubordinates;
    private Actor[] arrayOfHq;
    private Actor[] arrayOfReserves;


    private static ModalTabbedUI modalTabbedUI;


    /// <summary>
    /// Static instance so the ModalTabbedUI can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalTabbedUI Instance()
    {
        if (!modalTabbedUI)
        {
            modalTabbedUI = FindObjectOfType(typeof(ModalTabbedUI)) as ModalTabbedUI;
            if (!modalTabbedUI)
            { Debug.LogError("There needs to be one active ModalTabbedUI script on a GameObject in your scene"); }
        }
        return modalTabbedUI;
    }

    /// <summary>
    /// Initialise
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.LoadGame:
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
                SubInitialiseAsserts();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                //do nothing
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseAsserts
    private void SubInitialiseAsserts()
    {
        //Asserts
        Debug.Assert(tabbedCanvasMain != null, "Invalid tabbedCanvasMain (Null)");
        Debug.Assert(tabbedObjectMain != null, "Invalid tabbedObjectMain (Null)");
        Debug.Assert(backgroundImage != null, "Invalid backgroundImage (Null)");
        //Canvases
        Debug.Assert(canvasTab0 != null, "Invalid canvasTab0 (Null)");
        Debug.Assert(canvasTab1 != null, "Invalid canvasTab1 (Null)");
        Debug.Assert(canvasTab2 != null, "Invalid canvasTab2 (Null)");
        Debug.Assert(canvasTab3 != null, "Invalid canvasTab3 (Null)");
        Debug.Assert(canvasTab4 != null, "Invalid canvasTab4 (Null)");
        Debug.Assert(canvasTab5 != null, "Invalid canvasTab5 (Null)");
        Debug.Assert(canvasTab6 != null, "Invalid canvasTab6 (Null)");
        //button interactions
        if (interactClose != null) { interactClose.SetButton(EventType.TabbedClose); }
        else { Debug.LogError("Invalid interactClose (Null)"); }
        if (interactSubordinates != null) { interactSubordinates.SetButton(EventType.TabbedSubordinates); }
        else { Debug.LogError("Invalid interactSubordinates (Null)"); }
        if (interactPlayer != null) { interactPlayer.SetButton(EventType.TabbedPlayer); }
        else { Debug.LogError("Invalid interactPlayer (Null)"); }
        if (interactHq != null) { interactHq.SetButton(EventType.TabbedHq); }
        else { Debug.LogError("Invalid interactHq (Null)"); }
        if (interactReserves != null) { interactReserves.SetButton(EventType.TabbedReserves); }
        else { Debug.LogError("Invalid interactReserves (Null)"); }
        //controller
        Debug.Assert(buttonSubordinates != null, "Invalid buttonSubordinates (Null)");
        Debug.Assert(buttonPlayer != null, "Invalid buttonPlayer (Null)");
        Debug.Assert(buttonHq != null, "Invalid buttonHq (Null)");
        Debug.Assert(buttonReserves != null, "Invalid buttonReserves (Null)");
        Debug.Assert(textSubordinates != null, "Invalid textSubordinates (Null)");
        Debug.Assert(textPlayer != null, "Invalid textPlayer (Null)");
        Debug.Assert(textHq != null, "Invalid textHq (Null)");
        Debug.Assert(textReserves != null, "Invalid textReserves (Null)");
        //help
        if (buttonHelpClose != null)
        {
            helpClose = buttonHelpClose.GetComponent<GenericHelpTooltipUI>();
            Debug.Assert(helpClose != null, "Invalid helpClose (Null)");
        }
        else { Debug.LogError("Invalid buttonHelpClose (Null)"); }
        //texts
        Debug.Assert(textActorName != null, "Invalid textActorName (Null)");
        Debug.Assert(textPageMain != null, "Invalid textPageMain (Null)");
        //tab0
        Debug.Assert(tab0ActorName != null, "Invalid tab0ActorName (Null)");
        Debug.Assert(tab0ActorTitle != null, "Invalid tab0ActorTitle (Null)");
        Debug.Assert(tab0PowerText != null, "Invalid tab0PowerText (Null)");
        Debug.Assert(tab0Compatibility != null, "Invalid tab0Compatibility (Null)");
        Debug.Assert(tab0ActorImage != null, "Invalid tab0ActorImage (Null)");
        Debug.Assert(tab0PanelStatus != null, "Invalid tab0PanelStatus (Null)");
        Debug.Assert(tab0PanelConflicts != null, "Invalid tab0PanelConflicts (Null)");
        Debug.Assert(tab0PanelFriends != null, "Invalid tab0PanelFriends (Null)");
        Debug.Assert(tab0PanelConditions != null, "Invalid tab0PanelConditions (Null)");
        Debug.Assert(tab0PanelCures != null, "Invalid tab0PanelCures (Null)");
        Debug.Assert(tab0Header0 != null, "Invalid tab0Header0 (Null)");
        Debug.Assert(tab0Header1 != null, "Invalid tab0Header1 (Null)");
        Debug.Assert(tab0Header2 != null, "Invalid tab0Header2 (Null)");
        if (tab0Header3 != null) { maxNumOfConditions = tab0Header3.listOfItems.Count; }
        else { Debug.LogError("Invalid tab0Header3 (Null)"); }
        if (tab0Header4 != null) { maxNumOfCures = tab0Header4.listOfItems.Count; }
        else { Debug.LogError("Invalid tab0Header4 (Null)"); }
    }
    #endregion

    #region SubInitialiseSessionStart
    /// <summary>
    /// Session start
    /// </summary>
    private void SubInitialiseSessionStart()
    {
        isActive = false;
        int index;
        //Colours
        sideTabActiveColour = GameManager.i.uiScript.TabbedSideTabActive;
        sideTabDormantColour = GameManager.i.uiScript.TabbedSideTabDormant;
        topTabActiveColour = GameManager.i.uiScript.TabbedTopTabActive;
        topTabDormantColour = GameManager.i.uiScript.TabbedTopTabDormant;
        tabTextActiveColour = GameManager.i.uiScript.TabbedTextActive;
        tabTextDormantColour = GameManager.i.uiScript.TabbedTextDormant;
        tabSubHeaderColour = GameManager.i.uiScript.TabbedSubHeader;
        tabSubHeaderTextActive = GameManager.i.uiScript.TabbedSubHeaderText;
        Color tempColour = tabSubHeaderTextActive;
        tempColour.a = 0.60f;
        tabSubHeaderTextDormant = tempColour;
        tempColour = tab0Header0.listOfItems[0].image.color;
        tabItemHelpActive = tempColour;
        tempColour.a = 0.60f;
        tabItemHelpDormant = tempColour;
        //Tabs and indexes
        currentTopTabIndex = 0;
        numOfSideTabs = (int)TabbedUISide.Count;
        numOfTopTabs = (int)TabbedUITop.Count;
        //max tabs
        maxSideTabIndex = numOfSideTabs - 1;
        maxSetIndex = (int)TabbedUIWho.Count - 1;
        maxTopTabIndex = numOfTopTabs - 1;
        //initialise Canvas array
        arrayOfCanvas = new Canvas[numOfTopTabs];
        //initialise Top Tab Arrays
        arrayOfTopTabObjects = new GameObject[numOfTopTabs];
        arrayOfTopTabImages = new Image[numOfTopTabs];
        arrayOfTopTabTitles = new TextMeshProUGUI[numOfTopTabs];
        arrayOfTopTabInteractions = new TabbedTopTabUI[numOfTopTabs];
        arrayOfPages = new TabbedPage[numOfTopTabs];
        //initialise Side Tab Arrays
        arrayOfSideTabObjects = new GameObject[numOfSideTabs];
        arrayOfSideTabItems = new TabbedInteractionSide[numOfSideTabs];
        arrayOfSideTabInteractions = new TabbedSideTabUI[numOfSideTabs];
        arrayOfActorsTemp = new Actor[numOfSideTabs];
        //Canvas array
        arrayOfCanvas[0] = canvasTab0;
        arrayOfCanvas[1] = canvasTab1;
        arrayOfCanvas[2] = canvasTab2;
        arrayOfCanvas[3] = canvasTab3;
        arrayOfCanvas[4] = canvasTab4;
        arrayOfCanvas[5] = canvasTab5;
        arrayOfCanvas[6] = canvasTab6;
        //Top tab components
        index = 0;
        if (topTab0 != null) { arrayOfTopTabObjects[index++] = topTab0; } else { Debug.LogError("Invalid topTab0 (Null)"); }
        if (topTab1 != null) { arrayOfTopTabObjects[index++] = topTab1; } else { Debug.LogError("Invalid topTab1 (Null)"); }
        if (topTab2 != null) { arrayOfTopTabObjects[index++] = topTab2; } else { Debug.LogError("Invalid topTab2 (Null)"); }
        if (topTab3 != null) { arrayOfTopTabObjects[index++] = topTab3; } else { Debug.LogError("Invalid topTab3 (Null)"); }
        if (topTab4 != null) { arrayOfTopTabObjects[index++] = topTab4; } else { Debug.LogError("Invalid topTab4 (Null)"); }
        if (topTab5 != null) { arrayOfTopTabObjects[index++] = topTab5; } else { Debug.LogError("Invalid topTab5 (Null)"); }
        if (topTab6 != null) { arrayOfTopTabObjects[index++] = topTab6; } else { Debug.LogError("Invalid topTab6 (Null)"); }
        //initialise top tab arrays 
        for (int i = 0; i < numOfTopTabs; i++)
        {
            if (arrayOfTopTabObjects[i] != null)
            {
                //populate top tab component arrays
                TabbedInteractionTop interact = arrayOfTopTabObjects[i].GetComponent<TabbedInteractionTop>();
                if (interact != null)
                {
                    if (interact.topTab != null) { arrayOfTopTabImages[i] = interact.topTab; } else { Debug.LogWarningFormat("Invalid interact.topTab (Null) for arrayOfTopTabObjects[{0}]", i); }
                    if (interact.topTabUI != null)
                    {
                        arrayOfTopTabInteractions[i] = interact.topTabUI;
                        arrayOfTopTabInteractions[i].SetTabIndex(i, maxTopTabIndex);
                    }
                    else { Debug.LogWarningFormat("Invalid interact.topTabUI (Null) for arrayOfTopTabObjects[{0}]", i); }
                    if (interact.text != null) { arrayOfTopTabTitles[i] = interact.text; } else { Debug.LogWarningFormat("Invalid interact.text (Null) for arrayOfTopTabObjects[{0}]", i); }
                }
                else { Debug.LogErrorFormat("Invalid tabbedInteractionTop (Null) for arrayOfTopTabObjects[{0}]", i); }
            }
            else { Debug.LogErrorFormat("Invalid TopTabObject (Null) for arrayOfTopTabObjects[{0}]", i); }
        }
        //Side tab components
        index = 0;
        if (sideTab0 != null) { arrayOfSideTabObjects[index++] = sideTab0; } else { Debug.LogError("Invalid sideTab0 (Null)"); }
        if (sideTab1 != null) { arrayOfSideTabObjects[index++] = sideTab1; } else { Debug.LogError("Invalid sideTab1 (Null)"); }
        if (sideTab2 != null) { arrayOfSideTabObjects[index++] = sideTab2; } else { Debug.LogError("Invalid sideTab2 (Null)"); }
        if (sideTab3 != null) { arrayOfSideTabObjects[index++] = sideTab3; } else { Debug.LogError("Invalid sideTab3 (Null)"); }
        //initialise side tab arrays -> interaction
        for (int i = 0; i < numOfSideTabs; i++)
        {
            if (arrayOfSideTabObjects[i] != null)
            {
                TabbedInteractionSide tabbedInteract = arrayOfSideTabObjects[i].GetComponent<TabbedInteractionSide>();
                if (tabbedInteract != null)
                {
                    arrayOfSideTabItems[i] = tabbedInteract;
                    //tab interaction
                    TabbedSideTabUI sideTab = tabbedInteract.background.GetComponent<TabbedSideTabUI>();
                    if (sideTab != null)
                    {
                        arrayOfSideTabInteractions[i] = sideTab;
                        //identify tabs
                        sideTab.SetTabIndex(i, maxSideTabIndex);
                    }
                    else { Debug.LogErrorFormat("Invalid TabbedSideTabUI (Null) for arrayOfSideTabItems[{0}]", i); }
                }
                else { Debug.LogErrorFormat("Invalid TabbedInteraction (Null) for arrayOfSideTabObject[{0}]", i); }
            }
        }
        //Sub Header colours
        tab0PanelStatus.color = tabSubHeaderColour;
        tab0PanelConflicts.color = tabSubHeaderColour;
        tab0PanelFriends.color = tabSubHeaderColour;
        tab0PanelConditions.color = tabSubHeaderColour;
        tab0PanelCures.color = tabSubHeaderColour;
        //Initialisations
        InitialiseTooltips();
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.TabbedOpen, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedClose, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedSideTabOpen, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedSubordinates, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedPlayer, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedHq, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedReserves, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedUpArrow, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedDownArrow, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedRightArrow, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedLeftArrow, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedPageUp, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedPageDown, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedOpenPage, OnEvent, "ModalTabbedUI");
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
            case EventType.TabbedOpen:
                TabbedUIData details = Param as TabbedUIData;
                SetTabbedUI(details);
                break;
            case EventType.TabbedClose:
                CloseTabbedUI();
                break;
            case EventType.TabbedOpenPage:
                OpenPage((int)Param, true);
                break;
            case EventType.TabbedSideTabOpen:
                OpenSideTab((int)Param, true);
                break;
            case EventType.TabbedSubordinates:
                OpenActorSet(TabbedUIWho.Subordinates);
                break;
            case EventType.TabbedPlayer:
                OpenActorSet(TabbedUIWho.Player);
                break;
            case EventType.TabbedHq:
                OpenActorSet(TabbedUIWho.HQ);
                break;
            case EventType.TabbedReserves:
                OpenActorSet(TabbedUIWho.Reserves);
                break;
            case EventType.TabbedUpArrow:
                ExecuteUpArrow();
                break;
            case EventType.TabbedDownArrow:
                ExecuteDownArrow();
                break;
            case EventType.TabbedLeftArrow:
                ExecuteLeftArrow();
                break;
            case EventType.TabbedRightArrow:
                ExecuteRightArrow();
                break;
            case EventType.TabbedPageUp:
                ExecutePageUp();
                break;
            case EventType.TabbedPageDown:
                ExecutePageDown();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion


    #region Initialisation...

    /// <summary>
    /// run prior to every TabbedUI use. Run from SetTabbedUI
    /// </summary>
    private void InitialiseTabbedUI(TabbedUIData data)
    {
        //Start with a clean slate -> empty out cached data
        if (arrayOfSubordinates != null) { Array.Clear(arrayOfSubordinates, 0, arrayOfSubordinates.Length); }
        if (arrayOfReserves != null) { Array.Clear(arrayOfReserves, 0, arrayOfReserves.Length); }
        if (arrayOfHq != null) { Array.Clear(arrayOfHq, 0, arrayOfHq.Length); }
    }


    #region InitialiseSideTabs
    /// <summary>
    /// Updates side tabs when necessary
    /// </summary>
    /// <param name=""></param>
    private void InitialiseSideTabs()
    {
        int index = 0;
        if (arrayOfSideTabItems[index] != null)
        {
            //clear out actor array
            Array.Clear(arrayOfActorsTemp, 0, arrayOfActorsTemp.Length);
            //Update sideTabs for required actor set
            switch (inputData.who)
            {
                case TabbedUIWho.Subordinates:
                    //Cached data available
                    if (arrayOfSubordinates != null)
                    {
                        numOfSideTabs = arrayOfSubordinates.Length;
                        if (numOfSideTabs > 0)
                        { InitialiseSubordinate(arrayOfSubordinates); }
                        else
                        { textActorName.text = ""; tab0ActorName.text = ""; }
                        /*Debug.LogFormat("[Tst] ModalTabbedUI.cs -> InitialiseSideTabs: Subordinates CACHED data used{0}", "\n");*/
                    }
                    else
                    {
                        //Generate Data -> check how many actors OnMap
                        arrayOfActorsTemp = GameManager.i.dataScript.GetCurrentActorsVariable(inputData.side);
                        numOfSideTabs = arrayOfActorsTemp.Length;
                        if (numOfSideTabs > 0)
                        {
                            InitialiseSubordinate(arrayOfActorsTemp);
                            //store Cached data
                            if (arrayOfSubordinates == null)
                            {
                                //initialise cached Subordinates array
                                arrayOfSubordinates = new Actor[numOfSideTabs];
                                Array.Copy(arrayOfActorsTemp, arrayOfSubordinates, numOfSideTabs);
                            }
                            /*Debug.LogFormat("[Tst] ModalTabbedUI.cs -> InitialiseSideTabs: Subordinates Generated data used{0}", "\n");*/
                        }
                        else
                        {
                            //clear out any residual actor name
                            textActorName.text = ""; tab0ActorName.text = "";
                        }
                    }

                    //disable empty tabs
                    if (numOfSideTabs <= maxSideTabIndex)
                    {
                        for (int i = numOfSideTabs; i <= maxSideTabIndex; i++)
                        { arrayOfSideTabObjects[i].SetActive(false); }
                    }
                    break;
                case TabbedUIWho.Reserves:
                    //Cached data available
                    if (arrayOfReserves != null)
                    {
                        numOfSideTabs = arrayOfReserves.Length;
                        if (numOfSideTabs > 0)
                        { InitialiseReserves(arrayOfReserves); }
                        else
                        { textActorName.text = ""; tab0ActorName.text = ""; }
                        /*Debug.LogFormat("[Tst] ModalTabbedUI.cs -> InitialiseSideTabs: Reserves CACHED data used{0}", "\n");*/
                    }
                    else
                    {
                        //Generate Data -> how many actors in reserve
                        numOfSideTabs = GameManager.i.dataScript.CheckNumOfActorsInReserve();
                        if (numOfSideTabs > 0)
                        {
                            List<int> listOfActors = GameManager.i.dataScript.GetListOfReserveActors(inputData.side);
                            if (listOfActors != null)
                            {
                                InitialiseReserves(listOfActors);
                                //Cached data
                                if (arrayOfReserves == null)
                                {
                                    //initialise cached Subordinates array
                                    arrayOfReserves = new Actor[numOfSideTabs];
                                    Array.Copy(arrayOfActorsTemp, arrayOfReserves, numOfSideTabs);
                                }
                                /*Debug.LogFormat("[Tst] ModalTabbedUI.cs -> InitialiseSideTabs: Reserves Generated data used{0}", "\n");*/
                            }
                            else { Debug.LogErrorFormat("Invalid listOfReserveActors (Null) for \"{0}\"", inputData.side); }
                        }
                        else
                        {
                            //clear out any residual actor name
                            textActorName.text = ""; tab0ActorName.text = "";
                        }
                    }
                    //disable empty tabs
                    if (numOfSideTabs <= maxSideTabIndex)
                    {
                        for (int i = numOfSideTabs; i <= maxSideTabIndex; i++)
                        { arrayOfSideTabObjects[i].SetActive(false); }
                    }
                    break;
                case TabbedUIWho.Player:
                    numOfSideTabs = 1;
                    index = 0;
                    textActorName.text = GameManager.i.playerScript.PlayerName;
                    //set up player tab
                    arrayOfSideTabItems[index].portrait.sprite = GameManager.i.playerScript.sprite;
                    arrayOfSideTabItems[index].title.text = GameManager.i.playerScript.FirstName;
                    //colors
                    textSideColour = arrayOfSideTabItems[index].title.color;
                    portraitColour = arrayOfSideTabItems[index].portrait.color;
                    backgroundColour = sideTabActiveColour;
                    portraitColour.a = 1.0f; backgroundColour.a = 1.0f; textSideColour.a = 1.0f;
                    //set colors
                    arrayOfSideTabItems[index].portrait.color = portraitColour;
                    arrayOfSideTabItems[index].background.color = backgroundColour;
                    arrayOfSideTabItems[index].title.color = textSideColour;
                    //activate tab
                    arrayOfSideTabObjects[index].SetActive(true);
                    currentSideTabIndex = index;
                    //disable inactive tabs
                    for (int i = numOfSideTabs; i <= maxSideTabIndex; i++)
                    { arrayOfSideTabObjects[i].SetActive(false); }
                    break;
                case TabbedUIWho.HQ:
                    //Cached data available
                    if (arrayOfHq != null)
                    {
                        numOfSideTabs = arrayOfHq.Length;
                        if (numOfSideTabs > 0)
                        { InitialiseHq(arrayOfHq); }
                        else
                        { textActorName.text = ""; tab0ActorName.text = ""; }
                        /*Debug.LogFormat("[Tst] ModalTabbedUI.cs -> InitialiseSideTabs: Hq CACHED data used{0}", "\n");*/
                    }
                    else
                    {
                        //Generate Data -> assumes a full compliment of HQ actors present
                        List<Actor> listOfActors = GameManager.i.dataScript.GetListOfHqHierarchy();
                        if (listOfActors != null)
                        {

                            /*numOfSideTabs = maxSideTabIndex + 1;*/

                            numOfSideTabs = listOfActors.Count;
                            arrayOfActorsTemp = listOfActors.ToArray();
                            InitialiseHq(arrayOfActorsTemp);
                            //Cached data
                            if (arrayOfHq == null)
                            {
                                //initialise cached Subordinates array
                                arrayOfHq = new Actor[numOfSideTabs];
                                Array.Copy(arrayOfActorsTemp, arrayOfHq, numOfSideTabs);
                            }
                            /*Debug.LogFormat("[Tst] ModalTabbedUI.cs -> InitialiseSideTabs: Hq Generated data used{0}", "\n");*/
                        }
                        else { Debug.LogError("Invalid listOfHqHierarchyActors (Null)"); }
                    }
                    break;
                default: Debug.LogWarningFormat("Unrecognised TabbedUIWho \"{0}\"", inputData.who); break;

            }

        }
        else { Debug.LogErrorFormat("Invalid tabItems[{0}] (Null)", index); }
    }
    #endregion


    #region InitialiseTooltips
    /// <summary>
    /// Initialise fixed tooltips
    /// </summary>
    private void InitialiseTooltips()
    {
        List<HelpData> listOfHelp;
        //main help button
        listOfHelp = GameManager.i.helpScript.GetHelpData("metaGameUI_0", "metaGameUI_1", "metaGameUI_2", "metaGameUI_3");
        if (listOfHelp != null)
        { helpClose.SetHelpTooltip(listOfHelp, x_offset, y_offset); }
        else { Debug.LogWarning("Invalid listOfHelp for helpMain (Null)"); }
    }
    #endregion

    #endregion


    #region SetTabbedUI
    /// <summary>
    /// Initialise and open TabbedUI
    /// </summary>
    private void SetTabbedUI(TabbedUIData details)
    {
        if (isActive == false)
        {
            if (details != null)
            {
                bool errorFlag = false;
                //set modal status
                GameManager.i.guiScript.SetIsBlocked(true, details.modalLevel);
                //store data
                inputData = details;
                //initialise actorSetIndex
                currentSetIndex = (int)details.who;
                //initialise (order is important as InitialiseTabbedUI clears arrays that are filled with cached data in InitialiseSideTabs)
                InitialiseTabbedUI(details);
                //tooltips off
                GameManager.i.guiScript.SetTooltipsOff();
                //activate main panel
                tabbedObjectMain.SetActive(true);
                //Activate main canvas -> last
                tabbedCanvasMain.gameObject.SetActive(true);
                //Initialise selected set (do so AFTER main canvas activated, not before, otherwise controller button won't highlight for the specified actor set)
                OpenActorSetStart(details.who);
                //Initialise default main top tab
                OpenPage(0);
                //error outcome message if there is a problem
                if (errorFlag == true)
                {
                    tabbedObjectMain.SetActive(false);
                    //create an outcome window to notify player
                    ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
                    outcomeDetails.textTop = "There has been a hiccup and the information isn't available";
                    outcomeDetails.textBottom = "Sit tight, Thunderbirds are GO!";
                    outcomeDetails.side = details.side;
                    EventManager.i.PostNotification(EventType.OutcomeOpen, this, outcomeDetails, "ModalInventoryUI.cs -> SetInventoryUI");
                }
                else
                {
                    //all good, set modal status
                    isActive = true;
                    ModalStateData package = new ModalStateData() { mainState = ModalSubState.InfoDisplay, infoState = ModalInfoSubState.TabbedUI };
                    GameManager.i.inputScript.SetModalState(package);
                    Debug.LogFormat("[UI] ModalTabbedUI.cs -> SetTabbedUI{0}", "\n");
                }
            }
            else { Debug.LogError("Invalid TabbedUIData (Null)"); }
        }
        else { Debug.LogWarning("ModalTabbedUI.cs can't be opened as already ACTIVE -> Info only"); }
    }
    #endregion


    #region CloseTabbedUI
    /// <summary>
    /// Close TabbedUI
    /// </summary>
    private void CloseTabbedUI()
    {
        tabbedCanvasMain.gameObject.SetActive(false);
        tabbedObjectMain.SetActive(false);
        //null out cached data (otherwise arrays will be empty but still valid and new data won't be generated next time UI opened)
        arrayOfSubordinates = null;
        arrayOfReserves = null;
        arrayOfHq = null;
        //clear but not if sitting on top of another UI element, eg. InventoryUI (HQ / Reserve actors)
        GameManager.i.guiScript.SetIsBlocked(false, inputData.modalLevel);
        //close generic tooltip (safety check)
        GameManager.i.tooltipGenericScript.CloseTooltip("ModalTabbedUI.cs -> CloseTabbedUI");
        //close help tooltip
        GameManager.i.tooltipHelpScript.CloseTooltip("ModalTabbedUI.cs -> CloseTabbedUI");
        //set game state
        GameManager.i.inputScript.ResetStates(inputData.modalState);
        //set inactive
        isActive = false;
        Debug.LogFormat("[UI] ModalTabbedUI.cs -> CloseTabbedUI{0}", "\n");
    }
    #endregion


    #region Page and Tab Control... 
    //
    // - - - Page and Tab control
    //

    /// <summary>
    /// Open the designated Side tab and close whatever is open
    /// 'isMouseClick' true if user has clicked directly on the tab, false otherwise
    /// </summary>
    /// <param name="tabIndex"></param>
    private void OpenSideTab(int tabIndex, bool isMouseClick = false)
    {
        Debug.AssertFormat(tabIndex > -1 && tabIndex < numOfSideTabs, "Invalid tab index {0}", tabIndex);
        bool isProceed = true;
        //check not opening current side tab (mouse click only)
        if (isMouseClick == true && tabIndex == currentSideTabIndex)
        { isProceed = false; }
        if (isProceed == true)
        {
            //reset Active tabs to reflect new status
            for (int index = 0; index < numOfSideTabs; index++)
            {
                portraitColour = arrayOfSideTabItems[index].portrait.color;
                textSideColour = arrayOfSideTabItems[index].title.color;
                //activate indicated tab and deactivate the rest
                if (index == tabIndex)
                {
                    backgroundColour = sideTabActiveColour;
                    portraitColour.a = 1.0f;
                    backgroundColour.a = 1.0f;
                    textActorName.text = GetActorName(tabIndex);
                    textSideColour.a = 1.0f;
                }
                else
                {
                    backgroundColour = sideTabDormantColour;
                    portraitColour.a = sideTabAlpha;
                    backgroundColour.a = sideTabAlpha;
                    textSideColour.a = 0.5f;
                }
                arrayOfSideTabItems[index].portrait.color = portraitColour;
                arrayOfSideTabItems[index].background.color = backgroundColour;
                //update text colour (faded if inactive
                arrayOfSideTabItems[index].title.color = textSideColour;
            }
            //update index
            currentSideTabIndex = tabIndex;
            UpdatePage();
        }
        //do so regardless
        UpdateControllerButton(inputData.who);
    }

    /// <summary>
    /// Open a tab page. 'isMouseClick' true for when user directly clicking on a top tab to open a page (versus left/right arrow key activation)
    /// </summary>
    /// <param name="tabIndex"></param>
    private void OpenPage(int tabIndex, bool isMouseClick = false)
    {
        bool isProceed = true;
        if (tabIndex > -1 && tabIndex < numOfTopTabs)
        {
            //check you're not opening the current page (mouse click only)
            if (isMouseClick == true && tabIndex == currentTopTabIndex)
            { isProceed = false; }
            if (isProceed == true)
            {
                //Active/Dormant tabs
                UpdateTopTabs(tabIndex);
                //toggle canvases on/off
                for (int i = 0; i < arrayOfCanvas.Length; i++)
                {
                    if (i == tabIndex) { arrayOfCanvas[i].gameObject.SetActive(true); }
                    else { arrayOfCanvas[i].gameObject.SetActive(false); }
                }
                UpdatePage();
            }
        }
        else { Debug.LogErrorFormat("Invalid tabIndex \"{0}\" (should be > -1 and < {1})", tabIndex, numOfTopTabs); }
        UpdateControllerButton(inputData.who);
    }

    /// <summary>
    /// Updates data in current page for different side tab actor
    /// </summary>
    private void UpdatePage()
    {
        switch (currentTopTabIndex)
        {
            case 0:
                tab0ActorName.text = GetActorName(currentSideTabIndex);
                tab0ActorTitle.text = GetActorTitle(currentSideTabIndex);
                tab0ActorImage.sprite = arrayOfSideTabItems[currentSideTabIndex].portrait.sprite;
                //RHS panels
                switch (inputData.who)
                {
                    case TabbedUIWho.Subordinates:
                        tab0PanelStatus.gameObject.SetActive(true);
                        tab0PanelConflicts.gameObject.SetActive(true);
                        tab0PanelFriends.gameObject.SetActive(true);
                        tab0PanelConditions.gameObject.SetActive(true);
                        tab0PanelCures.gameObject.SetActive(false);
                        UpdateCompatibility();
                        UpdatePower();
                        UpdateStatus();
                        if (GetConflict() == true) { tab0Header1.text.color = tabSubHeaderTextActive; } else { tab0Header1.text.color = tabSubHeaderTextDormant; }
                        if (GetRelationship() == true) { tab0Header2.text.color = tabSubHeaderTextActive; } else { tab0Header2.text.color = tabSubHeaderTextDormant; }
                        if (GetConditions() == true) { tab0Header3.text.color = tabSubHeaderTextActive; } else { tab0Header3.text.color = tabSubHeaderTextDormant; }
                        break;
                    case TabbedUIWho.Player:
                        tab0PanelStatus.gameObject.SetActive(true);
                        tab0PanelConflicts.gameObject.SetActive(false);
                        tab0PanelFriends.gameObject.SetActive(false);
                        tab0PanelCures.gameObject.SetActive(true);
                        tab0PanelConditions.gameObject.SetActive(true);
                        UpdateCompatibility(true);
                        UpdatePower(true);
                        UpdateStatus(true);
                        if (GetCures() == true) { tab0Header4.text.color = tabSubHeaderTextActive; } else { tab0Header4.text.color = tabSubHeaderTextDormant; }
                        if (GetConditions(true) == true) { tab0Header3.text.color = tabSubHeaderTextActive; } else { tab0Header3.text.color = tabSubHeaderTextDormant; }
                        break;
                    case TabbedUIWho.HQ:
                        tab0PanelStatus.gameObject.SetActive(true);
                        tab0PanelConflicts.gameObject.SetActive(false);
                        tab0PanelFriends.gameObject.SetActive(false);
                        tab0PanelConditions.gameObject.SetActive(false);
                        tab0PanelCures.gameObject.SetActive(false);
                        UpdateCompatibility();
                        UpdatePower();
                        UpdateStatus();
                        break;
                    case TabbedUIWho.Reserves:
                        tab0PanelStatus.gameObject.SetActive(true);
                        tab0PanelConflicts.gameObject.SetActive(false);
                        tab0PanelFriends.gameObject.SetActive(false);
                        tab0PanelConditions.gameObject.SetActive(true);
                        tab0PanelCures.gameObject.SetActive(false);
                        UpdateCompatibility();
                        UpdatePower();
                        UpdateStatus();
                        if (GetConditions() == true) { tab0Header3.text.color = tabSubHeaderTextActive; } else { tab0Header3.text.color = tabSubHeaderTextDormant; }
                        break;
                    default: Debug.LogWarningFormat("Unrecognised inputData.who \"{0}\"", inputData.who); break;
                }
                break;
            case 1:

                break;
            case 2:

                break;
            case 3:

                break;
            case 4:

                break;
            case 5:

                break;
            case 6:

                break;
            default: Debug.LogWarningFormat("Unrecognised currentTopTabIndex \"{0}\"", currentTopTabIndex); break;
        }
    }


    /// <summary>
    /// Open a new actor set, eg. Subordinates/Player/HQ/Reserves. Assumed that TabbedUI is already open
    /// 'isResetSlotID' will set slotID to 0 if true and retain the original value, if false
    /// </summary>
    /// <param name="who"></param>
    private void OpenActorSet(TabbedUIWho who, bool isResetSlotID = true)
    {
        //check not opening current page
        if (who != inputData.who)
        {
            switch (who)
            {
                case TabbedUIWho.Subordinates:
                case TabbedUIWho.Player:
                case TabbedUIWho.HQ:
                case TabbedUIWho.Reserves:
                    inputData.who = who;
                    if (isResetSlotID == true)
                    { inputData.slotID = 0; }
                    currentSetIndex = (int)who;
                    InitialiseTopTabs(who);
                    InitialiseSideTabs();
                    break;
                default: Debug.LogWarningFormat("Unrecognised who \"{0}\"", who); break;
            }
            UpdatePage();
        }
        //do so regardless
        UpdateControllerButton(who);
    }

    /// <summary>
    /// Used at startup to open first actor set (ignores opening current page check)
    /// </summary>
    /// <param name="who"></param>
    /// <param name="isResetSlotID"></param>
    private void OpenActorSetStart(TabbedUIWho who)
    {
        switch (who)
        {
            case TabbedUIWho.Subordinates:
            case TabbedUIWho.Player:
            case TabbedUIWho.HQ:
            case TabbedUIWho.Reserves:
                inputData.who = who;
                currentSetIndex = (int)who;
                InitialiseTopTabs(who);
                InitialiseSideTabs();
                break;
            default: Debug.LogWarningFormat("Unrecognised who \"{0}\"", who); break;
        }
        UpdatePage();
        UpdateControllerButton(who);
    }

    /// <summary>
    /// Sets up top tabs and arrays whenever a new actorSet is opened
    /// </summary>
    /// <param name="who"></param>
    private void InitialiseTopTabs(TabbedUIWho who)
    {
        switch (who)
        {
            case TabbedUIWho.Subordinates:
                arrayOfPages[0] = TabbedPage.Main;
                arrayOfPages[1] = TabbedPage.Personality;
                arrayOfPages[2] = TabbedPage.Contacts;
                arrayOfPages[3] = TabbedPage.Secrets;
                arrayOfPages[4] = TabbedPage.Gear;
                arrayOfPages[5] = TabbedPage.History;
                arrayOfPages[6] = TabbedPage.Stats;
                maxTopTabIndex = 6;
                arrayOfTopTabObjects[4].SetActive(true);
                arrayOfTopTabObjects[5].SetActive(true);
                arrayOfTopTabObjects[6].SetActive(true);
                arrayOfTopTabTitles[0].text = "Main";
                arrayOfTopTabTitles[1].text = "Person";
                arrayOfTopTabTitles[2].text = "Contacts";
                arrayOfTopTabTitles[3].text = "Secrets";
                arrayOfTopTabTitles[4].text = "Gear";
                arrayOfTopTabTitles[5].text = "History";
                arrayOfTopTabTitles[6].text = "Stats";
                break;
            case TabbedUIWho.Player:
                arrayOfPages[0] = TabbedPage.Main;
                arrayOfPages[1] = TabbedPage.Personality;
                arrayOfPages[2] = TabbedPage.Likes;
                arrayOfPages[3] = TabbedPage.Secrets;
                arrayOfPages[4] = TabbedPage.Investigations;
                arrayOfPages[5] = TabbedPage.History;
                arrayOfPages[6] = TabbedPage.Stats;
                maxTopTabIndex = 6;
                arrayOfTopTabObjects[4].SetActive(true);
                arrayOfTopTabObjects[5].SetActive(true);
                arrayOfTopTabObjects[6].SetActive(true);
                arrayOfTopTabTitles[0].text = "Main";
                arrayOfTopTabTitles[1].text = "Person";
                arrayOfTopTabTitles[2].text = "Likes";
                arrayOfTopTabTitles[3].text = "Invest";
                arrayOfTopTabTitles[4].text = "Secrets";
                arrayOfTopTabTitles[5].text = "History";
                arrayOfTopTabTitles[6].text = "Stats";
                break;
            case TabbedUIWho.HQ:
                arrayOfPages[0] = TabbedPage.Main;
                arrayOfPages[1] = TabbedPage.Personality;
                arrayOfPages[2] = TabbedPage.History;
                arrayOfPages[3] = TabbedPage.Stats;
                arrayOfTopTabObjects[4].SetActive(false);
                arrayOfTopTabObjects[5].SetActive(false);
                arrayOfTopTabObjects[6].SetActive(false);
                maxTopTabIndex = 3;
                arrayOfTopTabTitles[0].text = "Main";
                arrayOfTopTabTitles[1].text = "Person";
                arrayOfTopTabTitles[2].text = "History";
                arrayOfTopTabTitles[3].text = "Stats";
                break;
            case TabbedUIWho.Reserves:
                arrayOfPages[0] = TabbedPage.Main;
                arrayOfPages[1] = TabbedPage.Personality;
                arrayOfPages[2] = TabbedPage.History;
                arrayOfPages[3] = TabbedPage.Stats;
                arrayOfTopTabObjects[4].SetActive(false);
                arrayOfTopTabObjects[5].SetActive(false);
                arrayOfTopTabObjects[6].SetActive(false);
                maxTopTabIndex = 3;
                arrayOfTopTabTitles[0].text = "Main";
                arrayOfTopTabTitles[1].text = "Person";
                arrayOfTopTabTitles[2].text = "History";
                arrayOfTopTabTitles[3].text = "Stats";
                break;
            default: Debug.LogWarningFormat("Unrecognised who \"{0}\"", who); break;
        }
        //default to first tab (current tab may no longer be present, so always go back to first on changing actorSet)
        currentTopTabIndex = 0;
        UpdateTopTabs(0);
    }

    /// <summary>
    /// selects specified button (shows as highlighted). Other button automatically deselected
    /// </summary>
    /// <param name="who"></param>
    private void UpdateControllerButton(TabbedUIWho who)
    {
        switch (who)
        {
            case TabbedUIWho.Subordinates:
                buttonSubordinates.Select();
                textSubordinates.color = tabTextActiveColour;
                textPlayer.color = tabTextDormantColour;
                textHq.color = tabTextDormantColour;
                textReserves.color = tabTextDormantColour;
                break;
            case TabbedUIWho.Player:
                buttonPlayer.Select();
                textSubordinates.color = tabTextDormantColour;
                textPlayer.color = tabTextActiveColour;
                textHq.color = tabTextDormantColour;
                textReserves.color = tabTextDormantColour;
                break;
            case TabbedUIWho.HQ:
                buttonHq.Select();
                textSubordinates.color = tabTextDormantColour;
                textPlayer.color = tabTextDormantColour;
                textHq.color = tabTextActiveColour;
                textReserves.color = tabTextDormantColour;
                break;
            case TabbedUIWho.Reserves:
                buttonReserves.Select();
                textSubordinates.color = tabTextDormantColour;
                textPlayer.color = tabTextDormantColour;
                textHq.color = tabTextDormantColour;
                textReserves.color = tabTextActiveColour;
                break;
            default: Debug.LogWarningFormat("Unrecognised who \"{0}\"", who); break;
        }
    }

    /// <summary>
    /// selects specified tab (shows as active). Other tabs set to dormant
    /// </summary>
    /// <param name="top"></param>
    private void UpdateTopTabs(int tabIndex)
    {
        if (tabIndex > -1)
        {
            //Update active/dormant tabs
            for (int i = 0; i < numOfTopTabs; i++)
            {
                if (i == tabIndex)
                {
                    //active
                    arrayOfTopTabImages[i].color = topTabActiveColour;
                    arrayOfTopTabTitles[i].color = tabTextActiveColour;
                }
                else
                {
                    //dormant
                    arrayOfTopTabImages[i].color = topTabDormantColour;
                    arrayOfTopTabTitles[i].color = tabTextDormantColour;
                }
            }
            //update index (required for when player directly clicks on a tab)
            currentTopTabIndex = tabIndex;
            //main page text
            textPageMain.text = Convert.ToString(arrayOfPages[tabIndex]);
            //actorSet
            UpdateControllerButton((TabbedUIWho)currentSetIndex);
        }
        else { Debug.LogWarning("Invalid tabIndex (-1)"); }
    }

    #endregion


    #region Movement...

    //
    // - - - Movement
    //

    /// <summary>
    /// Up arrow -> Side Tabs
    /// </summary>
    private void ExecuteUpArrow()
    {
        //change tab
        if (currentSideTabIndex > -1 && numOfSideTabs > 1)
        {
            if (currentSideTabIndex > 0)
            {
                currentSideTabIndex -= 1;
                OpenSideTab(currentSideTabIndex);
            }
            else
            {
                //roll over
                currentSideTabIndex = numOfSideTabs - 1;
                OpenSideTab(currentSideTabIndex);
            }
            //highlight actorSet
            UpdateControllerButton(inputData.who);
            //page
            UpdatePage();
        }
    }


    /// <summary>
    /// Down arrow -> Side Tabs
    /// </summary>
    private void ExecuteDownArrow()
    {
        if (currentSideTabIndex > -1 && numOfSideTabs > 1)
        {
            if (currentSideTabIndex < numOfSideTabs - 1)
            {
                currentSideTabIndex += 1;
                OpenSideTab(currentSideTabIndex);
            }
            else
            {
                //roll over
                currentSideTabIndex = 0;
                OpenSideTab(currentSideTabIndex);
            }
            //highlight actorSet
            UpdateControllerButton(inputData.who);
            //page
            UpdatePage();
        }
    }

    /// <summary>
    /// Right arrow
    /// </summary>
    private void ExecuteRightArrow()
    {
        if (currentTopTabIndex > -1)
        {
            if (currentTopTabIndex < maxTopTabIndex)
            {
                currentTopTabIndex += 1;
                OpenPage(currentTopTabIndex);
            }
            else
            {
                //roll over
                currentTopTabIndex = 0;
                OpenPage(currentTopTabIndex);
            }
        }
    }


    /// <summary>
    /// Left Arrow
    /// </summary>
    private void ExecuteLeftArrow()
    {
        if (currentTopTabIndex > -1)
        {
            if (currentTopTabIndex > 0)
            {
                currentTopTabIndex -= 1;
                OpenPage(currentTopTabIndex);
            }
            else
            {
                //roll over
                currentTopTabIndex = maxTopTabIndex;
                OpenPage(currentTopTabIndex);
            }
        }
    }

    /// <summary>
    /// Page Up -> next actor Set to the Left
    /// </summary>
    private void ExecutePageUp()
    {
        if (currentSetIndex > -1)
        {
            if (currentSetIndex > 0)
            {
                currentSetIndex -= 1;
                OpenActorSet((TabbedUIWho)currentSetIndex);
            }
            else
            {
                //roll over
                currentSetIndex = maxSetIndex;
                OpenActorSet((TabbedUIWho)currentSetIndex);
            }
        }
    }

    /// <summary>
    /// Page Down -> next actor Set to the Right
    /// </summary>
    private void ExecutePageDown()
    {
        if (currentSetIndex > -1)
        {
            if (currentSetIndex < maxSetIndex)
            {
                currentSetIndex += 1;
                OpenActorSet((TabbedUIWho)currentSetIndex);
            }
            else
            {
                //roll over
                currentSetIndex = 0;
                OpenActorSet((TabbedUIWho)currentSetIndex); ;
            }
        }
    }

    #endregion


    #region SubMethods...
    //
    // - - - SubMethods
    //

    /// <summary>
    /// Returns actorName (normal or HQ) for header. Runs all needed checks
    /// </summary>
    /// <param name="tabIndex"></param>
    /// <returns></returns>
    private string GetActorName(int tabIndex)
    {
        string actorName = "Unknown";
        switch (inputData.who)
        {
            case TabbedUIWho.Player:
                actorName = GameManager.i.playerScript.PlayerName;
                break;
            default:
                if (tabIndex <= maxSideTabIndex)
                {
                    if (arrayOfActorsTemp[tabIndex] != null)
                    { actorName = arrayOfActorsTemp[tabIndex].actorName; }
                    else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfActorsTemp[{0}]", tabIndex); }
                }
                else { Debug.LogWarningFormat("Invalid tabIndex (is {0}, should be <= {1})", tabIndex, maxSideTabIndex); }
                break;
        }
        return actorName;
    }

    /// <summary>
    /// Updates tabbedItem in Tab0 subHeader 'Status' for current actor
    /// </summary>
    private void UpdateStatus(bool isPlayer = false)
    {
        if (isPlayer == true)
        {
            //Player
            tab0Header0.listOfItems[0].descriptor.text = GetStatus(GameManager.i.playerScript.status, GameManager.i.playerScript.inactiveStatus);

        }
        else
        {
            //Actor
            Actor actor = arrayOfActorsTemp[currentSideTabIndex];
            if (actor != null)
            { tab0Header0.listOfItems[0].descriptor.text = GetStatus(actor.Status, actor.inactiveStatus); }
            else { Debug.LogErrorFormat("Invalid actor (Null) for arrayOfActorsTemp[{0}]", currentSideTabIndex); }
        }
    }

    /// <summary>
    /// Returns a user friendly string for actor/player status
    /// </summary>
    /// <param name="status"></param>
    /// <param name="inactive"></param>
    /// <returns></returns>
    private string GetStatus(ActorStatus status, ActorInactive inactive)
    {
        string descriptor = "Unknown";
        switch (status)
        {
            case ActorStatus.Active:
                if (inputData.who != TabbedUIWho.Player) { descriptor = "Available"; }
                else { descriptor = "On the Job"; }
                break;
            case ActorStatus.Reserve: descriptor = "Awaiting Assignment"; break;
            case ActorStatus.HQ: descriptor = "At HQ"; break;
            case ActorStatus.Captured: descriptor = "Captured"; break;
            case ActorStatus.Inactive:
                switch (inactive)
                {
                    case ActorInactive.Breakdown: descriptor = "Undergoing a Nervous Breakdown"; break;
                    case ActorInactive.LieLow: descriptor = "Lying Low"; break;
                    case ActorInactive.StressLeave: descriptor = "On Stress Leave"; break;
                    default: Debug.LogWarningFormat("Unrecognised Inactive status \"{0}\"", inactive); break;
                }
                break;
            default: Debug.LogWarningFormat("Unrecognised ActorStatus \"{0}\"", status); break;
        }
        return descriptor;
    }

    /// <summary>
    /// Updates Power in Tab0 for current actor
    /// </summary>
    private void UpdatePower(bool isPlayer = false)
    {
        if (isPlayer == true)
        { tab0PowerText.text = Convert.ToString(GameManager.i.playerScript.Power); }
        else
        { tab0PowerText.text = Convert.ToString(arrayOfActorsTemp[currentSideTabIndex].Power); }
    }

    /// <summary>
    /// Updates Compatibility with Player in Tab0 for current actor
    /// </summary>
    private void UpdateCompatibility(bool isPlayer = false)
    {
        if (isPlayer == true)
        { tab0Compatibility.text = ""; }
        else
        {
            int compatibility = arrayOfActorsTemp[currentSideTabIndex].GetPersonality().GetCompatibilityWithPlayer();
            tab0Compatibility.text = GameManager.i.guiScript.GetCompatibilityStars(compatibility);
        }
    }

    /// <summary>
    /// If Subordinate actorSet opinion 0 highlights actor at risk of conflict by populating subHeader1 item.tag
    /// </summary>
    /// <returns></returns>
    private bool GetConflict()
    {
        bool isPossibleConflict = false;
        switch (arrayOfActorsTemp[currentSideTabIndex].GetDatapoint(ActorDatapoint.Opinion1))
        {
            case 0:
                tab0Header1.listOfItems[0].descriptor.text = "ON THE EDGE";
                tab0Header1.listOfItems[0].image.color = tabItemHelpActive;
                isPossibleConflict = true;
                break;
            case 1:
                tab0Header1.listOfItems[0].descriptor.text = "<alpha=#AA>Unlikely";
                tab0Header1.listOfItems[0].image.color = tabItemHelpDormant;
                break;
            default:
                tab0Header1.listOfItems[0].descriptor.text = "<alpha=#AA>Happy in the Service";
                tab0Header1.listOfItems[0].image.color = tabItemHelpDormant;
                break;
        }
        return isPossibleConflict;
    }

    /// <summary>
    /// Populates subHeader2 item.tag for friend/Enemy relationship. Returns true if a relationship exists, false otherwise
    /// </summary>
    /// <returns></returns>
    private bool GetRelationship()
    {
        bool isRelationship = false;
        RelationshipData data = GameManager.i.dataScript.GetRelationshipData(arrayOfActorsTemp[currentSideTabIndex].slotID);
        if (data.relationship != ActorRelationship.None)
        {
            Actor actor = GameManager.i.dataScript.GetActor(data.actorID);
            if (actor != null)
            {
                tab0Header2.listOfItems[0].descriptor.text = string.Format("{0} {1}", data.relationship == ActorRelationship.Friend ? "Friends with " : "Enemies with ", actor.arc.name);
                isRelationship = true;
            }
            else
            {
                Debug.LogWarningFormat("Invalid actor (Null) for RelationshipData.actorID {0}", data.actorID);
                tab0Header2.listOfItems[0].descriptor.text = "<alpha=#AA>None";
                tab0Header2.listOfItems[0].image.color = tabItemHelpDormant;
            }
        }
        else
        {
            tab0Header2.listOfItems[0].descriptor.text = "<alpha=#AA>None";
            tab0Header2.listOfItems[0].image.color = tabItemHelpDormant;
        }
        return isRelationship;
    }

    /// <summary>
    /// Gets tabbedItems in Tab0 subHeader 'Conditions' for current actor. NOTE: Max of 10 conditions allowed
    /// Returns true if any conditions, false if none
    /// </summary>
    /// <param name="isPlayer"></param>
    private bool GetConditions(bool isPlayer = false)
    {
        int numOfItems;
        List<Condition> listOfConditions;
        numOfItems = tab0Header3.listOfItems.Count;
        if (isPlayer == true)
        {
            //Player
            listOfConditions = GameManager.i.playerScript.GetListOfConditions(inputData.side);
        }
        else
        {
            //Actor
            listOfConditions = arrayOfActorsTemp[currentSideTabIndex].GetListOfConditions();
        }
        return UpdateConditions(listOfConditions);
    }

    /// <summary>
    /// Updates condition processing for GetConditions. Returns true if anyconditions, returns false if 'None'
    /// </summary>
    /// <param name="listOfConditions"></param>
    private bool UpdateConditions(List<Condition> listOfConditions)
    {
        bool isConditions = true;
        Condition condition;
        int count = listOfConditions.Count;
        if (count > 0)
        {
            for (int i = 0; i < maxNumOfConditions; i++)
            {
                if (i < count)
                {
                    //condition present
                    condition = listOfConditions[i];
                    if (condition != null)
                    {
                        tab0Header3.listOfItems[i].gameObject.SetActive(true);
                        tab0Header3.listOfItems[i].descriptor.text = condition.tag;
                        //turn on help
                        tab0Header3.listOfItems[0].image.gameObject.SetActive(true);
                        tab0Header3.listOfItems[0].image.color = tabItemHelpActive;
                    }
                    else
                    {
                        Debug.LogWarningFormat("Invalid condition (Null) for listOfConditions[{0}]", i);
                        tab0Header3.listOfItems[i].descriptor.text = "Unknown";
                        tab0Header3.listOfItems[0].image.color = tabItemHelpDormant;
                    }
                }
                else
                {
                    //disable all other items
                    tab0Header3.listOfItems[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            //No Conditions present
            tab0Header3.listOfItems[0].descriptor.text = "<alpha=#AA>None";
            tab0Header3.listOfItems[0].image.color = tabItemHelpDormant;
            isConditions = false;
            //disable all other items
            for (int i = 1; i < maxNumOfConditions; i++)
            { tab0Header3.listOfItems[i].gameObject.SetActive(false); }
        }
        return isConditions;
    }

    /// <summary>
    /// Player specific info -> Available Cures for active conditions, returns true if any cures, false if 'None'
    /// </summary>
    /// <returns></returns>
    private bool GetCures()
    {
        bool isCure = false;
        int numOfItems;
        string cureName;
        //Get cures
        List<Node> listOfCures = GameManager.i.dataScript.GetListOfCureNodes();
        numOfItems = tab0Header4.listOfItems.Count;
        int count = listOfCures.Count;
        if (count > 0)
        {
            isCure = true;
            for (int i = 0; i < maxNumOfCures; i++)
            {
                if (i < count)
                {
                    //cure present
                    cureName = string.Format("{0} at {1}", listOfCures[i].cure.cureName, listOfCures[i].nodeName);
                    if (string.IsNullOrEmpty(cureName) == false)
                    {
                        tab0Header4.listOfItems[i].gameObject.SetActive(true);
                        tab0Header4.listOfItems[i].descriptor.text = cureName;
                        //turn on help
                        tab0Header4.listOfItems[0].image.gameObject.SetActive(true);
                        tab0Header4.listOfItems[0].image.color = tabItemHelpActive;
                    }
                    else
                    {
                        Debug.LogWarningFormat("Invalid cure (Null or Empty) for listOfCures[{0}]", i);
                        tab0Header4.listOfItems[i].descriptor.text = "Unknown";
                        tab0Header4.listOfItems[0].image.color = tabItemHelpDormant;
                    }
                }
                else
                {
                    //disable all other items
                    tab0Header4.listOfItems[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            //No Cures present
            tab0Header4.listOfItems[0].descriptor.text = "<alpha=#AA>None";
            tab0Header4.listOfItems[0].image.color = tabItemHelpDormant;
            //disable all other items
            for (int i = 1; i < maxNumOfCures; i++)
            { tab0Header4.listOfItems[i].gameObject.SetActive(false); }
        }
        return isCure;
    }

    /// <summary>
    /// Returns actorTitle (arc name / Player / HQ title)
    /// </summary>
    /// <param name="tabIndex"></param>
    /// <returns></returns>
    private string GetActorTitle(int tabIndex)
    {
        string actorTitle = "Unknown";
        if (tabIndex > -1 && tabIndex <= maxSideTabIndex)
        {
            switch (inputData.who)
            {
                case TabbedUIWho.Subordinates:
                case TabbedUIWho.Reserves:
                    actorTitle = arrayOfActorsTemp[tabIndex].arc.name;
                    break;
                case TabbedUIWho.Player:
                    actorTitle = "Player";
                    break;
                case TabbedUIWho.HQ:
                    actorTitle = GameManager.i.hqScript.GetHqTitle((ActorHQ)(tabIndex + 1));
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised inputData.who \"{0}\"", inputData.who); break;
            }
        }
        else { Debug.LogWarningFormat("Invalid currentSideIndex \"{0}\" (should be > -1 and <= {1})", tabIndex, maxSideTabIndex); }
        return actorTitle;
    }


    #endregion


    #region Initialisation SubMethods...
    //
    // - - - Initialisation SubMethods
    //

    /// <summary>
    /// Takes an arrayOfActors and initialises data for Subordinates actor set
    /// </summary>
    /// <param name="numOfSideTabs"></param>
    /// <param name="arrayOfActors"></param>
    private void InitialiseSubordinate(Actor[] arrayOfActors)
    {
        Actor actor;
        for (int index = 0; index < numOfSideTabs; index++)
        {
            actor = arrayOfActors[index];
            if (actor != null)
            {
                //populate array
                arrayOfActorsTemp[index] = actor;
                //sprite and arc
                arrayOfSideTabItems[index].portrait.sprite = actor.sprite;
                arrayOfSideTabItems[index].title.text = actor.arc.name;
                //baseline colours
                textSideColour = arrayOfSideTabItems[index].title.color;
                portraitColour = arrayOfSideTabItems[index].portrait.color;
                //first tab should be active on opening, rest passive
                if (index == inputData.slotID)
                {
                    backgroundColour = sideTabActiveColour;
                    portraitColour.a = 1.0f; backgroundColour.a = 1.0f;
                    textActorName.text = actor.actorName;
                    currentSideTabIndex = index;
                    textSideColour.a = 1.0f;
                }
                else
                {
                    backgroundColour = sideTabDormantColour;
                    portraitColour.a = sideTabAlpha; backgroundColour.a = sideTabAlpha;
                    textSideColour.a = 0.5f;
                }
                //set colors
                arrayOfSideTabItems[index].portrait.color = portraitColour;
                arrayOfSideTabItems[index].background.color = backgroundColour;
                arrayOfSideTabItems[index].title.color = textSideColour;
                //activate tab
                arrayOfSideTabObjects[index].SetActive(true);
            }
            else
            {
                //missing Actor -> shouldn't occur
                Debug.LogWarningFormat("Invalid actor (Null) for arrayOfActors[{0}]", index);
                arrayOfSideTabObjects[index].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Takes a listOfActorID's and initialises data for Reserves actor set
    /// </summary>
    /// <param name="numOfSideTabs"></param>
    /// <param name="listOfActors"></param>
    private void InitialiseReserves(List<int> listOfActors)
    {
        Actor actor;
        int limit = listOfActors.Count;
        if (limit != numOfSideTabs)
        {
            limit = Mathf.Min(numOfSideTabs, limit);
            Debug.LogWarningFormat("Mismatch between numOfSideTabs {0} and listOfActors.Count {1} (Should be identical)", numOfSideTabs, limit);
        }
        for (int index = 0; index < limit; index++)
        {
            actor = GameManager.i.dataScript.GetActor(listOfActors[index]);
            if (actor != null)
            {
                //populate array
                arrayOfActorsTemp[index] = actor;
                //sprite and arc
                arrayOfSideTabItems[index].portrait.sprite = actor.sprite;
                arrayOfSideTabItems[index].title.text = actor.arc.name;
                //baseline colours
                portraitColour = arrayOfSideTabItems[index].portrait.color;
                textSideColour = arrayOfSideTabItems[index].title.color;
                //first tab should be active on opening, rest passive
                if (index == inputData.slotID)
                {
                    backgroundColour = sideTabActiveColour;
                    portraitColour.a = 1.0f; backgroundColour.a = 1.0f;
                    textActorName.text = actor.actorName;
                    currentSideTabIndex = index;
                    textSideColour.a = 1.0f;
                }
                else
                {
                    backgroundColour = sideTabDormantColour;
                    portraitColour.a = sideTabAlpha; backgroundColour.a = sideTabAlpha;
                    textSideColour.a = 0.5f;
                }
                //set colors
                arrayOfSideTabItems[index].portrait.color = portraitColour;
                arrayOfSideTabItems[index].background.color = backgroundColour;
                arrayOfSideTabItems[index].title.color = textSideColour;
                //activate tab
                arrayOfSideTabObjects[index].SetActive(true);
            }
            else
            {
                //missing Actor -> shouldn't occur
                Debug.LogWarningFormat("Invalid actor (Null) for listOfActors[{0}], actorID \"{1}\"", index, listOfActors[index]);
                arrayOfSideTabObjects[index].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Overloaded -> Takes an arrayOfActors and initialises data for Reserves actor set (used for cached data)
    /// </summary>
    /// <param name="numOfSideTabs"></param>
    /// <param name="listOfActors"></param>
    private void InitialiseReserves(Actor[] arrayOfActors)
    {
        Actor actor;
        for (int index = 0; index < numOfSideTabs; index++)
        {
            actor = arrayOfActors[index];
            if (actor != null)
            {
                //populate array
                arrayOfActorsTemp[index] = actor;
                //sprite and arc
                arrayOfSideTabItems[index].portrait.sprite = actor.sprite;
                arrayOfSideTabItems[index].title.text = actor.arc.name;
                //baseline colours
                portraitColour = arrayOfSideTabItems[index].portrait.color;
                textSideColour = arrayOfSideTabItems[index].title.color;
                //first tab should be active on opening, rest passive
                if (index == inputData.slotID)
                {
                    backgroundColour = sideTabActiveColour;
                    portraitColour.a = 1.0f; backgroundColour.a = 1.0f;
                    textActorName.text = actor.actorName;
                    currentSideTabIndex = index;
                    textSideColour.a = 1.0f;
                }
                else
                {
                    backgroundColour = sideTabDormantColour;
                    portraitColour.a = sideTabAlpha; backgroundColour.a = sideTabAlpha;
                    textSideColour.a = 0.5f;
                }
                //set colors
                arrayOfSideTabItems[index].portrait.color = portraitColour;
                arrayOfSideTabItems[index].background.color = backgroundColour;
                arrayOfSideTabItems[index].title.color = textSideColour;
                //activate tab
                arrayOfSideTabObjects[index].SetActive(true);
            }
            else
            {
                //missing Actor -> shouldn't occur
                Debug.LogWarningFormat("Invalid actor (Null) for arrayOfActors[{0}]", index);
                arrayOfSideTabObjects[index].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Initialises Hq data for Hq actor set
    /// </summary>
    /// <param name="numOfSideTabs"></param>
    private void InitialiseHq(Actor[] arrayOfActors)
    {
        Actor actor;
        for (int index = 0; index < numOfSideTabs; index++)
        {
            actor = arrayOfActors[index];
            if (actor != null)
            {
                //populate array
                arrayOfActorsTemp[index] = actor;
                //sprite
                arrayOfSideTabItems[index].portrait.sprite = actor.sprite;
            }
            else
            {
                //default error sprite if a problem
                Debug.LogWarningFormat("Invalid actor (Null) for ActorHQ \"{0}\"", (ActorHQ)(index + offset));
                //deactivate tab
                arrayOfSideTabObjects[index].SetActive(false);
            }
            //title
            arrayOfSideTabItems[index].title.text = GameManager.i.hqScript.GetHqTitle(actor.statusHQ);
            //activate tab
            arrayOfSideTabObjects[index].SetActive(true);
            //baseline colours
            portraitColour = arrayOfSideTabItems[index].portrait.color;
            textSideColour = arrayOfSideTabItems[index].title.color;
            //first tab should be active on opening, rest passive
            if (index == inputData.slotID)
            {
                backgroundColour = sideTabActiveColour;
                portraitColour.a = 1.0f; backgroundColour.a = 1.0f;
                textActorName.text = actor.actorName;
                currentSideTabIndex = index;
                textSideColour.a = 1.0f;
            }
            else
            {
                backgroundColour = sideTabDormantColour;
                portraitColour.a = sideTabAlpha; backgroundColour.a = sideTabAlpha;
                textSideColour.a = 0.5f;
            }
            //set colors
            arrayOfSideTabItems[index].portrait.color = portraitColour;
            arrayOfSideTabItems[index].background.color = backgroundColour;
            arrayOfSideTabItems[index].title.color = textSideColour;
        }
    }



    #endregion


    //new methods above here
}
