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

    [Header("Side Tabs")]
    public GameObject sideTab0;
    public GameObject sideTab1;
    public GameObject sideTab2;
    public GameObject sideTab3;

    [Header("Controller Buttons")]
    public Button buttonSubordinates;
    public Button buttonPlayer;
    public Button buttonHq;
    public Button buttonReserves;

    [Header("Help Buttons")]
    public Button buttonHelpClose;

    [Header("Button Interactions")]
    public ButtonInteraction innteractCancel;
    public ButtonInteraction interactSubordinates;
    public ButtonInteraction interactPlayer;
    public ButtonInteraction interactHq;
    public ButtonInteraction interactReserves;

    [Header("Texts")]
    public TextMeshProUGUI textActorName;

    //help
    private GenericHelpTooltipUI helpClose;

    //tabs
    private int currentSideTabIndex = -1;                           //side tabs (top to bottom)
    private int currentTopTabIndex = -1;                            //top tabs (left to right)
    private int maxSideTabIndex;                                    //side tabs only (used for keeping pgUP/DOWN movement within HQ side tabs)
    private int maxTopTabIndex;                                     //top tabs only
    private int numOfSideTabs;                                      //keyed off enum.MetaTabSide
    private int numOfTopTabs;                                       //keyed off enum.MetaTabTop
    private int offset = 1;                                         //used with '(ActorHQ)index + offset' to account for the ActorHQ.enum having index 0 being 'None'
    private float sideTabAlpha = 0.50f;                             //alpha level of side tabs when inactive
    private float topTabAlpha = 0.50f;                              //alpha level of top tabs when inactive

    //help tooltips (I don't want this as a global, just a master private field)
    private int x_offset = 200;
    private int y_offset = 40;

    //colours
    private Color sideTabActiveColour;
    private Color sideTabDormantColour;

    //Input data
    TabbedUIData inputData;

    //sideTab collections
    private GameObject[] arrayOfSideTabObjects;
    private TabbedInteraction[] arrayOfSideTabItems;
    private TabbedSideTabUI[] arrayOfSideTabInteractions;
    private Actor[] arrayOfActorsTemp;                              //holds all actors for the current page (excludes Player -> not in array)



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
        //button interactions
        if (innteractCancel != null) { innteractCancel.SetButton(EventType.TabbedClose); }
        else { Debug.LogError("Invalid interactCancal (Null)"); }
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
        //help
        if (buttonHelpClose != null)
        {
            helpClose = buttonHelpClose.GetComponent<GenericHelpTooltipUI>();
            Debug.Assert(helpClose != null, "Invalid helpClose (Null)");
        }
        else { Debug.LogError("Invalid buttonHelpClose (Null)"); }
        //texts
        Debug.Assert(textActorName != null, "Invalid textActorName (Null)");
    }
    #endregion

    #region SubInitialiseSessionStart
    /// <summary>
    /// Session start
    /// </summary>
    private void SubInitialiseSessionStart()
    {
        int index = 0;
        numOfSideTabs = (int)TabbedUISide.Count;
        //max tabs
        maxSideTabIndex = numOfSideTabs - 1;
        //initialise Side Arrays -> tabs
        arrayOfSideTabObjects = new GameObject[numOfSideTabs];
        arrayOfSideTabItems = new TabbedInteraction[numOfSideTabs];
        arrayOfSideTabInteractions = new TabbedSideTabUI[numOfSideTabs];
        arrayOfActorsTemp = new Actor[numOfSideTabs];
        //Side tab components
        if (sideTab0 != null) { arrayOfSideTabObjects[index++] = sideTab0; } else { Debug.LogError("Invalid sideTab0 (Null)"); }
        if (sideTab1 != null) { arrayOfSideTabObjects[index++] = sideTab1; } else { Debug.LogError("Invalid sideTab1 (Null)"); }
        if (sideTab2 != null) { arrayOfSideTabObjects[index++] = sideTab2; } else { Debug.LogError("Invalid sideTab2 (Null)"); }
        if (sideTab3 != null) { arrayOfSideTabObjects[index++] = sideTab3; } else { Debug.LogError("Invalid sideTab3 (Null)"); }
        //initialise side tab arrays -> interaction
        for (int i = 0; i < numOfSideTabs; i++)
        {
            if (arrayOfSideTabObjects[i] != null)
            {
                TabbedInteraction tabbedInteract = arrayOfSideTabObjects[i].GetComponent<TabbedInteraction>();
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
        //Miscellaneous
        sideTabActiveColour = GameManager.i.uiScript.TabbedSideTabActive;
        sideTabDormantColour = GameManager.i.uiScript.TabbedSideTabDormant;
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
    }
    #endregion

    #endregion


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
            case EventType.TabbedSideTabOpen:
                OpenSideTab((int)Param);
                break;
            case EventType.TabbedSubordinates:
                inputData.who = TabbedUIWho.Subordinates;
                inputData.slotID = 0;
                InitialiseSideTabs(inputData);
                break;
            case EventType.TabbedPlayer:
                inputData.who = TabbedUIWho.Player;
                InitialiseSideTabs(inputData);
                break;
            case EventType.TabbedHq:
                inputData.who = TabbedUIWho.HQ;
                inputData.slotID = 0;
                InitialiseSideTabs(inputData);
                break;
            case EventType.TabbedReserves:
                inputData.who = TabbedUIWho.Reserves;
                inputData.slotID = 0;
                InitialiseSideTabs(inputData);
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


    /// <summary>
    /// run prior to every TabbedUI use. Run from SetTabbedUI
    /// </summary>
    private void InitialiseTabbedUI(TabbedUIData data)
    {

    }


    #region InitialiseSideTabs
    /// <summary>
    /// Updates side tabs when necessary
    /// </summary>
    /// <param name=""></param>
    private void InitialiseSideTabs(TabbedUIData data)
    {
        int index = 0;
        Actor actor;
        Color portraitColor, backgroundColor;

        if (arrayOfSideTabItems[index] != null)
        {
            //clear out actor array
            Array.Clear(arrayOfActorsTemp, 0, arrayOfActorsTemp.Length);
            //Update sideTabs for required actor set
            switch (data.who)
            {
                case TabbedUIWho.Subordinates:
                    //check how many actors OnMap
                    Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActorsVariable(data.side);
                    numOfSideTabs = arrayOfActors.Length;
                    if (numOfSideTabs > 0)
                    {
                        for (index = 0; index < numOfSideTabs; index++)
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
                                portraitColor = arrayOfSideTabItems[index].portrait.color;
                                //first tab should be active on opening, rest passive
                                if (index == data.slotID)
                                {
                                    backgroundColor = sideTabActiveColour;
                                    portraitColor.a = 1.0f; backgroundColor.a = 1.0f;
                                    textActorName.text = actor.actorName;
                                    currentSideTabIndex = index;
                                }
                                else
                                {
                                    backgroundColor = sideTabDormantColour;
                                    portraitColor.a = sideTabAlpha; backgroundColor.a = sideTabAlpha;
                                }
                                //set colors
                                arrayOfSideTabItems[index].portrait.color = portraitColor;
                                arrayOfSideTabItems[index].background.color = backgroundColor;
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
                    else
                    {
                        //clear out any residual actor name
                        textActorName.text = "";
                    }
                    //disable empty tabs
                    if (numOfSideTabs <= maxSideTabIndex)
                    {
                        for (int i = numOfSideTabs; i <= maxSideTabIndex; i++)
                        { arrayOfSideTabObjects[i].SetActive(false); }
                    }
                    break;
                case TabbedUIWho.Reserves:
                    numOfSideTabs = GameManager.i.dataScript.CheckNumOfActorsInReserve();
                    if (numOfSideTabs > 0)
                    {
                        List<int> listOfActors = GameManager.i.dataScript.GetListOfReserveActors(data.side);
                        if (listOfActors != null)
                        {
                            for (index = 0; index < numOfSideTabs; index++)
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
                                    portraitColor = arrayOfSideTabItems[index].portrait.color;
                                    //first tab should be active on opening, rest passive
                                    if (index == data.slotID)
                                    {
                                        backgroundColor = sideTabActiveColour;
                                        portraitColor.a = 1.0f; backgroundColor.a = 1.0f;
                                        textActorName.text = actor.actorName;
                                        currentSideTabIndex = index;
                                    }
                                    else
                                    {
                                        backgroundColor = sideTabDormantColour;
                                        portraitColor.a = sideTabAlpha; backgroundColor.a = sideTabAlpha;
                                    }
                                    //set colors
                                    arrayOfSideTabItems[index].portrait.color = portraitColor;
                                    arrayOfSideTabItems[index].background.color = backgroundColor;
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
                        else { Debug.LogErrorFormat("Invalid listOfReserveActors (Null) for \"{0}\"", data.side); }
                    }
                    else
                    {
                        //clear out any residual actor name
                        textActorName.text = "";
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
                    portraitColor = arrayOfSideTabItems[index].portrait.color;
                    backgroundColor = sideTabActiveColour;
                    portraitColor.a = 1.0f; backgroundColor.a = 1.0f;
                    //set colors
                    arrayOfSideTabItems[index].portrait.color = portraitColor;
                    arrayOfSideTabItems[index].background.color = backgroundColor;
                    //activate tab
                    arrayOfSideTabObjects[index].SetActive(true);
                    currentSideTabIndex = index;
                    //disable inactive tabs
                    for (int i = numOfSideTabs; i <= maxSideTabIndex; i++)
                    { arrayOfSideTabObjects[i].SetActive(false); }
                    break;
                case TabbedUIWho.HQ:
                    //assumes a full compliment of HQ actors present
                    numOfSideTabs = maxSideTabIndex + 1;
                    for (index = 0; index < numOfSideTabs; index++)
                    {
                        actor = GameManager.i.dataScript.GetHqHierarchyActor((ActorHQ)(index + offset));
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
                        portraitColor = arrayOfSideTabItems[index].portrait.color;
                        //first tab should be active on opening, rest passive
                        if (index == data.slotID)
                        {
                            backgroundColor = sideTabActiveColour;
                            portraitColor.a = 1.0f; backgroundColor.a = 1.0f;
                            textActorName.text = actor.actorName;
                            currentSideTabIndex = index;
                        }
                        else
                        {
                            backgroundColor = sideTabDormantColour;
                            portraitColor.a = sideTabAlpha; backgroundColor.a = sideTabAlpha;
                        }
                        //set colors
                        arrayOfSideTabItems[index].portrait.color = portraitColor;
                        arrayOfSideTabItems[index].background.color = backgroundColor;
                    }
                    break;
                default: Debug.LogWarningFormat("Unrecognised TabbedUIWho \"{0}\"", data.who); break;

            }

        }
        else { Debug.LogErrorFormat("Invalid tabItems[{0}] (Null)", index); }
    }
    #endregion


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


    #region SetTabbedUI
    /// <summary>
    /// Initialise and open TabbedUI
    /// </summary>
    private void SetTabbedUI(TabbedUIData details)
    {
        if (details != null)
        {
            bool errorFlag = false;
            //set modal status
            GameManager.i.guiScript.SetIsBlocked(true, details.modalLevel);
            //store data
            inputData = details;
            //initialise
            InitialiseSideTabs(details);
            InitialiseTabbedUI(details);
            //tooltips off
            GameManager.i.guiScript.SetTooltipsOff();
            //activate main panel
            tabbedObjectMain.SetActive(true);


            //Activate main canvas -> last
            tabbedCanvasMain.gameObject.SetActive(true);


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
                //all good, inventory window displayed
                ModalStateData package = new ModalStateData() { mainState = ModalSubState.InfoDisplay, infoState = ModalInfoSubState.TabbedUI };
                GameManager.i.inputScript.SetModalState(package);
                Debug.LogFormat("[UI] ModalTabbedUI.cs -> SetTabbedUI{0}", "\n");
            }
        }
        else { Debug.LogError("Invalid TabbedUIData (Null)"); }
    }
    #endregion



    /// <summary>
    /// Close TabbedUI
    /// </summary>
    private void CloseTabbedUI()
    {
        tabbedCanvasMain.gameObject.SetActive(false);
        tabbedObjectMain.SetActive(false);
        //clear but not if sitting on top of another UI element, eg. InventoryUI (HQ / Reserve actors)
        GameManager.i.guiScript.SetIsBlocked(false, inputData.modalLevel);
        //close generic tooltip (safety check)
        GameManager.i.tooltipGenericScript.CloseTooltip("ModalTabbedUI.cs -> CloseTabbedUI");
        //close help tooltip
        GameManager.i.tooltipHelpScript.CloseTooltip("ModalTabbedUI.cs -> CloseTabbedUI");
        //set game state
        GameManager.i.inputScript.ResetStates(inputData.modalState);
        Debug.LogFormat("[UI] ModalTabbedUI.cs -> CloseTabbedUI{0}", "\n");
    }

    #region Movement... 

    //
    // - - - Movement
    //

    /// <summary>
    /// Open the designated Side tab and close whatever is open
    /// </summary>
    /// <param name="tabIndex"></param>
    private void OpenSideTab(int tabIndex)
    {
        Debug.AssertFormat(tabIndex > -1 && tabIndex < numOfSideTabs, "Invalid tab index {0}", tabIndex);
        //reset Active tabs to reflect new status
        Color portraitColor, backgroundColor;
        for (int index = 0; index < numOfSideTabs; index++)
        {
            portraitColor = arrayOfSideTabItems[index].portrait.color;
            //activate indicated tab and deactivate the rest
            if (index == tabIndex)
            {
                backgroundColor = sideTabActiveColour;
                portraitColor.a = 1.0f;
                backgroundColor.a = 1.0f;
                //textActorName.text = actor.actorName;
                textActorName.text = GetActorName(tabIndex);
            }
            else
            {
                backgroundColor = sideTabDormantColour;
                portraitColor.a = sideTabAlpha;
                backgroundColor.a = sideTabAlpha;
            }
            arrayOfSideTabItems[index].portrait.color = portraitColor;
            arrayOfSideTabItems[index].background.color = backgroundColor;
        }
        //update index
        currentSideTabIndex = tabIndex;
    }

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
        }
    }

    /// <summary>
    /// Right arrow
    /// </summary>
    private void ExecuteRightArrow()
    {

    }


    /// <summary>
    /// Left Arrow
    /// </summary>
    private void ExecuteLeftArrow()
    {

    }

    /// <summary>
    /// Page Up
    /// </summary>
    private void ExecutePageUp()
    {

    }

    /// <summary>
    /// Page Down
    /// </summary>
    private void ExecutePageDown()
    {

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

    #endregion


    //new methods above here
}
