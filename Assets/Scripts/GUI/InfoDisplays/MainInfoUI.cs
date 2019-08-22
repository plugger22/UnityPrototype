﻿using gameAPI;
using packageAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles the main info App
/// </summary>
public class MainInfoUI : MonoBehaviour
{
    public Canvas mainInfoCanvas;
    public Canvas canvasTicker;
    public Canvas canvasFlasher;
    public Canvas canvasScroll;

    public GameObject mainInfoObject;

    [Header("Buttons")]
    public Button buttonClose;
    public Button buttonHelpInfo;                               //next to panel close button
    public Button buttonHome;
    public Button buttonBack;
    public Button buttonForward;
    public Button buttonHelpCentre;                         //bottom of RHS panel (centred below, NO 'Show Me' button present)
    public Button buttonHelpCombined;                       //bottom of RHS panel (bottom right, in line with 'Show Me' button)
    public Button buttonItem;                               //bottom of RHS panel, eg. 'Show Me'
    public TextMeshProUGUI buttonItemText;

    [Header("LHS Miscellanous")]
    public TextMeshProUGUI page_header;
    public GameObject scrollBarObject;
    public GameObject scrollBackground;         //needed to get scrollRect component in order to manually disable scrolling when not needed
    public Image flasher_requestTab;            //flashing alerts sitting over the top of their respective tabs
    public Image flasher_meetingTab;

    [Header("LHS Items")]
    //main tab -> parent backgrounds (full set of twenty, only a max of 10 shown at a time)
    public GameObject main_item_0;
    public GameObject main_item_1;
    public GameObject main_item_2;
    public GameObject main_item_3;
    public GameObject main_item_4;
    public GameObject main_item_5;
    public GameObject main_item_6;
    public GameObject main_item_7;
    public GameObject main_item_8;
    public GameObject main_item_9;
    public GameObject main_item_10;
    public GameObject main_item_11;
    public GameObject main_item_12;
    public GameObject main_item_13;
    public GameObject main_item_14;
    public GameObject main_item_15;
    public GameObject main_item_16;
    public GameObject main_item_17;
    public GameObject main_item_18;
    public GameObject main_item_19;
    public GameObject main_item_20;
    public GameObject main_item_21;
    public GameObject main_item_22;
    public GameObject main_item_23;
    public GameObject main_item_24;
    public GameObject main_item_25;
    public GameObject main_item_26;
    public GameObject main_item_27;
    public GameObject main_item_28;
    public GameObject main_item_29;

    [Header("LHS Active tabs")]
    //page tabs -> active
    public Image tab_active_0;
    public Image tab_active_1;
    public Image tab_active_2;
    public Image tab_active_3;
    public Image tab_active_4;
    public Image tab_active_5;

    public TextMeshProUGUI tab_active_text_0;
    public TextMeshProUGUI tab_active_text_1;
    public TextMeshProUGUI tab_active_text_2;
    public TextMeshProUGUI tab_active_text_3;
    public TextMeshProUGUI tab_active_text_4;
    public TextMeshProUGUI tab_active_text_5;

    [Header("LHS Passive tabs")]
    //page tabs -> passive
    public Image tab_passive_0;
    public Image tab_passive_1;
    public Image tab_passive_2;
    public Image tab_passive_3;
    public Image tab_passive_4;
    public Image tab_passive_5;

    public TextMeshProUGUI tab_passive_text_0;
    public TextMeshProUGUI tab_passive_text_1;
    public TextMeshProUGUI tab_passive_text_2;
    public TextMeshProUGUI tab_passive_text_3;
    public TextMeshProUGUI tab_passive_text_4;
    public TextMeshProUGUI tab_passive_text_5;

    [Header("RHS details")]
    public TextMeshProUGUI details_text_top;
    public TextMeshProUGUI details_text_bottom;
    public Image details_image;
    public Sprite details_image_sprite;

    [Header("Moving Flares")]
    public Image flare_SW;

    [Header("Ticker Text")]
    public GameObject tickerObject;
    public TextMeshProUGUI tickerText;
    public float tickerSpeed = 10;
    public float tickerSpeedMax = 20;
    public float tickerSpeedMin = 5;


    [Header("Globals")]
    [Tooltip("Change this at your peril (default 6) as data collections and indexes all flow from it")]
    public int numOfTabs = 6;
    [Tooltip("")]

    //button script handlers
    private ButtonInteraction buttonInteractionClose;
    private ButtonInteraction buttonInteractionHome;
    private ButtonInteraction buttonInteractionBack;
    private ButtonInteraction buttonInteractionForward;
    /*private ButtonInteraction buttonInteractionHelp;*/
    private ButtonInteraction buttonInteractionItem;

    //item help components
    private MainInfoItemHelpUI itemHelpCentre;                      //item help button on RHS panel (where no 'Show me' button exists)
    private MainInfoItemHelpUI itemHelpCombined;                    //item help button on RHS panel (adjacent to Show Me button
    private MainInfoItemHelpUI infoHelpTop;                         //info help button at top right of InfoApp (next to close button)


    private int highlightIndex = -1;                                 //item index of currently highlighted item
    private int maxHighlightIndex = -1;
    private int viewTurnNumber = -1;                                 //turn number of data being viewed
    private int currentTurn = -1;                                    //cached current turn # (SetMainInfo)
    private int numOfItemsTotal = 30;                                //hardwired Max number of items -> 30
    private int numOfVisibleItems = 10;                              //hardwired visible items in main page -> 10
    private int numOfItemsCurrent = -1;                              //count of items in current list / page
    private int numOfItemsPrevious = -1;                             //count of items in previous list / page
    private int currentItemNodeID = -1;
    private int currentItemConnID = -1;
    
    private GameObject[] arrayItemMain;
    private TextMeshProUGUI[] arrayItemText;
    private Image[] arrayItemIcon;
    private Image[] arrayItemBorder;
    private Image[] arrayItemBackground;
    private Sprite priorityHigh;
    private Sprite priorityMedium;
    private Sprite priorityLow;
    //scroll bar LHS
    private ScrollRect scrollRect;                                  //needed to manually disable scrolling when not needed
    private Scrollbar scrollBar;
    //flares
    private Coroutine myCoroutineFlareSW;
    /*private Vector3 startFlareLocalPosition;*/

    //tooltips
    private GenericTooltipUI itemButtonTooltip;
    //tabs
    private int currentTabIndex = -1;
    private int maxTabIndex;
    private Image[] tabActiveArray;
    private Image[] tabPassiveArray;
    private TextMeshProUGUI[] tabActiveTextArray;
    private TextMeshProUGUI[] tabPassiveTextArray;
    //ItemData
    private List<ItemData>[] arrayOfItemData = new List<ItemData>[(int)ItemTab.Count];       //One dataset for each tab (excluding Help tab)
    List<ItemData> listOfCurrentPageItemData;                                               //current data for currently displayed page
    //is MainINfoApp active?
    private bool isRunning;
    //flashers
    private bool isRequestFlasherOn;
    private bool isMeetingFlasherOn;
    private Coroutine myCoroutineRequest;
    private Coroutine myCoroutineMeeting;
    private float flashTimer = -1.0f;
    //ticker
    private TextMeshProUGUI cloneTickerText;
    private RectTransform tickerRectTransform;
    private RectTransform cloneRectTransform;
    private Coroutine myCoroutineTicker;

    //colours
    string colourDefault;
    string colourNeutral;
    string colourGrey;
    string colourAlert;
    string colourSide;
    /*string colourNormal;
    string colourGood;
    string colourError;
    string colourInvalid;
    string colourCancel;*/
    string colourEnd;

    //static reference
    private static MainInfoUI mainInfoUI;


    /// <summary>
    /// provide a static reference to MainInfoUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static MainInfoUI Instance()
    {
        if (!mainInfoUI)
        {
            mainInfoUI = FindObjectOfType(typeof(MainInfoUI)) as MainInfoUI;
            if (!mainInfoUI)
            { Debug.LogError("There needs to be one active MainInfoUI script on a GameObject in your scene"); }
        }
        return mainInfoUI;
    }


    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadGame:
            case GameState.FollowOnInitialisation:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //collections
        arrayItemMain = new GameObject[numOfItemsTotal];
        arrayItemIcon = new Image[numOfItemsTotal];
        arrayItemBorder = new Image[numOfItemsTotal];
        arrayItemBackground = new Image[numOfItemsTotal];
        arrayItemText = new TextMeshProUGUI[numOfItemsTotal];
        tabActiveArray = new Image[numOfTabs];
        tabPassiveArray = new Image[numOfTabs];
        tabActiveTextArray = new TextMeshProUGUI[numOfTabs];
        tabPassiveTextArray = new TextMeshProUGUI[numOfTabs];
        for (int i = 0; i < (int)ItemTab.Count; i++)
        { arrayOfItemData[i] = new List<ItemData>(); }
        listOfCurrentPageItemData = new List<ItemData>();
        //max tabs
        maxTabIndex = numOfTabs - 1;
        //main
        Debug.Assert(mainInfoCanvas != null, "Invalid mainInfoCanvas (Null)");
        Debug.Assert(canvasFlasher != null, "Invalid canvasFlasher (Null)");
        Debug.Assert(canvasTicker != null, "Invalid canvasTicker (Null)");
        Debug.Assert(canvasScroll != null, "Invalid canvasScroll (Null)");
        Debug.Assert(mainInfoObject != null, "Invalid mainInfoObject (Null)");
        mainInfoObject.SetActive(true);
        //buttons
        Debug.Assert(buttonClose != null, "Invalid buttonClose (Null)");
        Debug.Assert(buttonHelpInfo != null, "Invalid buttonInfo (Null)");
        Debug.Assert(buttonHome != null, "Invalid buttonHome (Null)");
        Debug.Assert(buttonBack != null, "Invalid buttonBack (Null)");
        Debug.Assert(buttonForward != null, "Invalid buttonForward (Null)");
        Debug.Assert(buttonHelpCentre != null, "Invalid buttonHelp (Null)");
        Debug.Assert(buttonHelpCombined != null, "Invalid buttonHelpCombined (Null)");
        Debug.Assert(buttonItem != null, "Invalid buttonDecisions (Null)");
        Debug.Assert(buttonItemText != null, "Invalid buttonItemText (Null)");
        //set button interaction events
        buttonInteractionClose = buttonClose.GetComponent<ButtonInteraction>();
        buttonInteractionHome = buttonHome.GetComponent<ButtonInteraction>();
        buttonInteractionBack = buttonBack.GetComponent<ButtonInteraction>();
        buttonInteractionForward = buttonForward.GetComponent<ButtonInteraction>();
        /*buttonInteractionHelp = buttonHelp.GetComponent<ButtonInteraction>();*/
        buttonInteractionItem = buttonItem.GetComponent<ButtonInteraction>();
        Debug.Assert(buttonInteractionClose != null, "Invalid buttonInteractionClose (Null)");
        Debug.Assert(buttonInteractionHome != null, "Invalid buttonInteractionHome (Null)");
        Debug.Assert(buttonInteractionBack != null, "Invalid buttonInteractionBack (Null)");
        Debug.Assert(buttonInteractionForward != null, "Invalid buttonInteractionForward (Null)");
        /*Debug.Assert(buttonInteractionHelp != null, "Invalid buttonInteractionHelp (Null)");*/
        Debug.Assert(buttonInteractionItem != null, "Invalid buttonInteractionDecision (Null)");
        buttonInteractionClose.SetButton(EventType.MainInfoClose);
        buttonInteractionHome.SetButton(EventType.MainInfoHome);
        buttonInteractionBack.SetButton(EventType.MainInfoBack);
        buttonInteractionForward.SetButton(EventType.MainInfoForward);
        //item help button
        itemHelpCentre = buttonHelpCentre.GetComponent<MainInfoItemHelpUI>();
        itemHelpCombined = buttonHelpCombined.GetComponent<MainInfoItemHelpUI>();
        infoHelpTop = buttonHelpInfo.GetComponent<MainInfoItemHelpUI>();
        Debug.Assert(itemHelpCentre != null, "Invalid itemHelpCentre (Null)");
        Debug.Assert(itemHelpCombined != null, "Invalid itemHelpCombined (Null)");
        Debug.Assert(infoHelpTop != null, "Invalid infoHelpTop (Null)");
        //tooltips
        itemButtonTooltip = buttonItem.GetComponent<GenericTooltipUI>();
        Debug.Assert(itemButtonTooltip != null, "Invalid itemButtonTooltip (Null)");
        //scrollRect & ScrollBar
        Debug.Assert(scrollBackground != null, "Invalid scrollBackground (Null)");
        Debug.Assert(scrollBarObject != null, "Invalid scrollBarObject (Null)");
        scrollRect = scrollBackground.GetComponent<ScrollRect>();
        scrollBar = scrollBarObject.GetComponent<Scrollbar>();
        Debug.Assert(scrollRect != null, "Invalid scrollRect (Null)");
        Debug.Assert(scrollBar != null, "Invalid scrollBar (Null)");
        //main background image array
        Debug.Assert(main_item_0 != null, "Invalid item_0 (Null)");
        Debug.Assert(main_item_1 != null, "Invalid item_1 (Null)");
        Debug.Assert(main_item_2 != null, "Invalid item_2 (Null)");
        Debug.Assert(main_item_3 != null, "Invalid item_3 (Null)");
        Debug.Assert(main_item_4 != null, "Invalid item_4 (Null)");
        Debug.Assert(main_item_5 != null, "Invalid item_5 (Null)");
        Debug.Assert(main_item_6 != null, "Invalid item_6 (Null)");
        Debug.Assert(main_item_7 != null, "Invalid item_7 (Null)");
        Debug.Assert(main_item_8 != null, "Invalid item_8 (Null)");
        Debug.Assert(main_item_9 != null, "Invalid item_9 (Null)");
        Debug.Assert(main_item_10 != null, "Invalid item_10 (Null)");
        Debug.Assert(main_item_11 != null, "Invalid item_11 (Null)");
        Debug.Assert(main_item_12 != null, "Invalid item_12 (Null)");
        Debug.Assert(main_item_13 != null, "Invalid item_13 (Null)");
        Debug.Assert(main_item_14 != null, "Invalid item_14 (Null)");
        Debug.Assert(main_item_15 != null, "Invalid item_15 (Null)");
        Debug.Assert(main_item_16 != null, "Invalid item_16 (Null)");
        Debug.Assert(main_item_17 != null, "Invalid item_17 (Null)");
        Debug.Assert(main_item_18 != null, "Invalid item_18 (Null)");
        Debug.Assert(main_item_19 != null, "Invalid item_19 (Null)");
        Debug.Assert(main_item_20 != null, "Invalid item_20 (Null)");
        Debug.Assert(main_item_21 != null, "Invalid item_21 (Null)");
        Debug.Assert(main_item_22 != null, "Invalid item_22 (Null)");
        Debug.Assert(main_item_23 != null, "Invalid item_23 (Null)");
        Debug.Assert(main_item_24 != null, "Invalid item_24 (Null)");
        Debug.Assert(main_item_25 != null, "Invalid item_25 (Null)");
        Debug.Assert(main_item_26 != null, "Invalid item_26 (Null)");
        Debug.Assert(main_item_27 != null, "Invalid item_27 (Null)");
        Debug.Assert(main_item_28 != null, "Invalid item_28 (Null)");
        Debug.Assert(main_item_29 != null, "Invalid item_29 (Null)");
        arrayItemMain[0] = main_item_0;
        arrayItemMain[1] = main_item_1;
        arrayItemMain[2] = main_item_2;
        arrayItemMain[3] = main_item_3;
        arrayItemMain[4] = main_item_4;
        arrayItemMain[5] = main_item_5;
        arrayItemMain[6] = main_item_6;
        arrayItemMain[7] = main_item_7;
        arrayItemMain[8] = main_item_8;
        arrayItemMain[9] = main_item_9;
        arrayItemMain[10] = main_item_10;
        arrayItemMain[11] = main_item_11;
        arrayItemMain[12] = main_item_12;
        arrayItemMain[13] = main_item_13;
        arrayItemMain[14] = main_item_14;
        arrayItemMain[15] = main_item_15;
        arrayItemMain[16] = main_item_16;
        arrayItemMain[17] = main_item_17;
        arrayItemMain[18] = main_item_18;
        arrayItemMain[19] = main_item_19;
        arrayItemMain[20] = main_item_20;
        arrayItemMain[21] = main_item_21;
        arrayItemMain[22] = main_item_22;
        arrayItemMain[23] = main_item_23;
        arrayItemMain[24] = main_item_24;
        arrayItemMain[25] = main_item_25;
        arrayItemMain[26] = main_item_26;
        arrayItemMain[27] = main_item_27;
        arrayItemMain[28] = main_item_28;
        arrayItemMain[29] = main_item_29;
        //active tab array
        Debug.Assert(tab_active_0 != null, "Invalid tab_active_0 (Null)");
        Debug.Assert(tab_active_1 != null, "Invalid tab_active_1 (Null)");
        Debug.Assert(tab_active_2 != null, "Invalid tab_active_2 (Null)");
        Debug.Assert(tab_active_3 != null, "Invalid tab_active_3 (Null)");
        Debug.Assert(tab_active_4 != null, "Invalid tab_active_4 (Null)");
        Debug.Assert(tab_active_5 != null, "Invalid tab_active_5 (Null)");
        tabActiveArray[0] = tab_active_0;
        tabActiveArray[1] = tab_active_1;
        tabActiveArray[2] = tab_active_2;
        tabActiveArray[3] = tab_active_3;
        tabActiveArray[4] = tab_active_4;
        tabActiveArray[5] = tab_active_5;
        Debug.Assert(tab_active_text_0 != null, "Invalid tab_active_text_0 (Null)");
        Debug.Assert(tab_active_text_1 != null, "Invalid tab_active_text_1 (Null)");
        Debug.Assert(tab_active_text_2 != null, "Invalid tab_active_text_2 (Null)");
        Debug.Assert(tab_active_text_3 != null, "Invalid tab_active_text_3 (Null)");
        Debug.Assert(tab_active_text_4 != null, "Invalid tab_active_text_4 (Null)");
        Debug.Assert(tab_active_text_5 != null, "Invalid tab_active_text_5 (Null)");
        tabActiveTextArray[0] = tab_active_text_0;
        tabActiveTextArray[1] = tab_active_text_1;
        tabActiveTextArray[2] = tab_active_text_2;
        tabActiveTextArray[3] = tab_active_text_3;
        tabActiveTextArray[4] = tab_active_text_4;
        tabActiveTextArray[5] = tab_active_text_5;
        //passive tab array
        Debug.Assert(tab_passive_0 != null, "Invalid tab_passive_0 (Null)");
        Debug.Assert(tab_passive_1 != null, "Invalid tab_passive_1 (Null)");
        Debug.Assert(tab_passive_2 != null, "Invalid tab_passive_2 (Null)");
        Debug.Assert(tab_passive_3 != null, "Invalid tab_passive_3 (Null)");
        Debug.Assert(tab_passive_4 != null, "Invalid tab_passive_4 (Null)");
        Debug.Assert(tab_passive_5 != null, "Invalid tab_passive_5 (Null)");
        tabPassiveArray[0] = tab_passive_0;
        tabPassiveArray[1] = tab_passive_1;
        tabPassiveArray[2] = tab_passive_2;
        tabPassiveArray[3] = tab_passive_3;
        tabPassiveArray[4] = tab_passive_4;
        tabPassiveArray[5] = tab_passive_5;
        Debug.Assert(tab_passive_text_0 != null, "Invalid tab_passive_text_0 (Null)");
        Debug.Assert(tab_passive_text_1 != null, "Invalid tab_passive_text_1 (Null)");
        Debug.Assert(tab_passive_text_2 != null, "Invalid tab_passive_text_2 (Null)");
        Debug.Assert(tab_passive_text_3 != null, "Invalid tab_passive_text_3 (Null)");
        Debug.Assert(tab_passive_text_4 != null, "Invalid tab_passive_text_4 (Null)");
        Debug.Assert(tab_passive_text_5 != null, "Invalid tab_passive_text_5 (Null)");
        tabPassiveTextArray[0] = tab_passive_text_0;
        tabPassiveTextArray[1] = tab_passive_text_1;
        tabPassiveTextArray[2] = tab_passive_text_2;
        tabPassiveTextArray[3] = tab_passive_text_3;
        tabPassiveTextArray[4] = tab_passive_text_4;
        tabPassiveTextArray[5] = tab_passive_text_5;
        //RHS
        Debug.Assert(details_image != null, "Invalid details_image (Null)");
        Debug.Assert(details_image_sprite != null, "Invalid details_image_sprite (Null)");
        Debug.Assert(details_text_top != null, "Invalid details_text_top (Null)");
        Debug.Assert(details_text_bottom != null, "Invalid details_text_bottom (Null)");
        //LHS
        Debug.Assert(flasher_requestTab != null, "Invalid flashing_requestTab (Null)");
        Debug.Assert(flasher_meetingTab != null, "Invalid flashing_meetingTab (Null)");
        //Moving Flares
        Debug.Assert(flare_SW != null, "Invalid flare_SW (Null");
        /*startFlareLocalPosition = flare_SW.transform.localPosition;*/
        //Ticker
        tickerRectTransform = tickerText.GetComponent<RectTransform>();
        cloneTickerText = Instantiate(tickerText) as TextMeshProUGUI;
        cloneRectTransform = cloneTickerText.GetComponent<RectTransform>();
        Debug.Assert(tickerRectTransform != null, "Invalid tickerRectTransform (Null)");
        Debug.Assert(cloneTickerText != null, "Invalid cloneTextTickerObject (Null)");
        Debug.Assert(cloneRectTransform != null, "Invalid cloneRectTransform (Null)");
        cloneRectTransform.SetParent(tickerRectTransform);
        cloneRectTransform.anchorMin = new Vector2(1, 0.5f);
        cloneRectTransform.anchorMax = new Vector2(1, 0.5f);
        cloneRectTransform.localScale = new Vector3(1, 1, 1);
        cloneRectTransform.position = new Vector3(tickerRectTransform.position.x, tickerRectTransform.position.y, tickerRectTransform.position.z);
        tickerObject.SetActive(true);
        cloneTickerText.gameObject.SetActive(true);
        //initiliase Active tabs
        for (int index = 0; index < tabActiveArray.Length; index++)
        {
            //deactivate Active tabs except default first one
            if (index == 0)
            { tabActiveArray[index].gameObject.SetActive(true); }
            else { tabActiveArray[index].gameObject.SetActive(false); }
            //initialise tabIndex fields for Passive Tabs
            MainInfoRightTabUI tab = tabPassiveArray[index].GetComponent<MainInfoRightTabUI>();
            if (tab != null)
            { tab.SetTabIndex(index); }
            else { Debug.LogWarningFormat("Invalid MainInfoRightTabUI component (Null) for tabActiveArray[{0}]", index); }
            //initialise Tab texts
            tabActiveTextArray[index].text = Convert.ToString((ItemTab)index);
            tabPassiveTextArray[index].text = Convert.ToString((ItemTab)index);
        }
        //initialise items & populate arrays
        for (int index = 0; index < arrayItemMain.Length; index++)
        {
            GameObject itemObject = arrayItemMain[index];
            if (itemObject != null)
            {
                //get child components -> Image
                var childrenImage = itemObject.GetComponentsInChildren<Image>();
                foreach (var child in childrenImage)
                {
                    if (child.name.Equals("background", StringComparison.Ordinal) == true)
                    {
                        arrayItemBackground[index] = child;
                        //attached interaction script
                        MainInfoRightItemUI itemScript = child.GetComponent<MainInfoRightItemUI>();
                        if (itemScript != null)
                        { itemScript.SetItemIndex(index, numOfItemsTotal); }
                        else { Debug.LogWarningFormat("Invalid MainInfoRightItemUI component (Null) for mainItemArray[{0}]", index); }
                    }
                    else if (child.name.Equals("icon", StringComparison.Ordinal) == true)
                    { arrayItemIcon[index] = child; }
                    else if (child.name.Equals("border", StringComparison.Ordinal) == true)
                    { arrayItemBorder[index] = child; }
                }
                //child components -> Text
                var childrenText = itemObject.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var child in childrenText)
                {
                    if (child.name.Equals("text", StringComparison.Ordinal) == true)
                    {
                        TextMeshProUGUI itemText = child.GetComponent<TextMeshProUGUI>();
                        if (itemText != null)
                        { arrayItemText[index] = itemText; }
                        else { Debug.LogWarningFormat("Invalid TextMeshProUGUI component (Null) for mainItemArray[{0}]", index); }
                    }
                }
            }
            else { Debug.LogWarningFormat("Invalid GameObject (Null) for mainItemArray[{0}]", index); }
        }
        //flashing alert over tabs
        flashTimer = GameManager.instance.guiScript.flashInfoTabTime;
        Debug.Assert(flashTimer > -1.0f, "Invalid flashTimer (-1f)");
        //Set starting Initialisation states
        SetColours();
        InitialiseCanvases();
        InitialiseItems();
        InitialiseTooltips();
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //priority icons
        priorityHigh = GameManager.instance.guiScript.priorityHighSprite;
        priorityMedium = GameManager.instance.guiScript.priorityMediumSprite;
        priorityLow = GameManager.instance.guiScript.priorityLowSprite;
        Debug.Assert(priorityHigh != null, "Invalid priorityHigh (Null)");
        Debug.Assert(priorityMedium != null, "Invalid priorityMedium (Null)");
        Debug.Assert(priorityLow != null, "Invalid priorityLow (Null)");

    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoOpen, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoOpenInterim, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoClose, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoTabOpen, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoShowDetails, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoHome, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoEnd, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoBack, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoForward, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoUpArrow, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoDownArrow, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoLeftArrow, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoRightArrow, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoShowMe, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoRestore, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoTickerFaster, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoTickerSlower, OnEvent, "MainInfoUI");
    }
    #endregion

    #endregion


    private void InitialiseItems()
    {
        for (int index = 0; index < numOfItemsTotal; index++)
        {
            //main game objects off
            arrayItemMain[index].SetActive(false);
            //all other child objects on
            arrayItemIcon[index].gameObject.SetActive(true);
            arrayItemText[index].gameObject.SetActive(true);
            arrayItemBorder[index].gameObject.SetActive(true);
            arrayItemBackground[index].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// set subsidiary canvases to active at start as they may have been accidentally left off
    /// </summary>
    private void InitialiseCanvases()
    {
        canvasTicker.gameObject.SetActive(true);
        canvasFlasher.gameObject.SetActive(true);
        canvasScroll.gameObject.SetActive(true);
    }

    private void InitialiseTooltips()
    {
        //guiManager.cs provides a standard ShowMe tooltip implementation
        Tuple<string, string, string> texts = GameManager.instance.guiScript.GetShowMeTooltip();
        //itemButton
        itemButtonTooltip.tooltipHeader = texts.Item1;
        itemButtonTooltip.tooltipMain = texts.Item2;
        itemButtonTooltip.tooltipDetails = texts.Item3;
        itemButtonTooltip.x_offset = 125;
        //initialise butotnHelpInfo data
        infoHelpTop.listOfHelp = GetInfoHelpList();
    }

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
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.ChangeSide:
                ChangeSides((GlobalSide)Param);
                break;
            case EventType.MainInfoOpen:
                MainInfoData data = Param as MainInfoData;
                SetMainInfo(data);
                break;
            case EventType.MainInfoOpenInterim:
                OpenMainInfoBetweenTurns();
                break;
            case EventType.MainInfoClose:
                CloseMainInfo();
                break;
            case EventType.MainInfoTabOpen:
                OpenTab((int)Param);
                break;
            case EventType.MainInfoShowDetails:
                ShowItemDetails((int)Param);
                break;
            case EventType.MainInfoHome:
                ExecuteButtonHome();
                break;
            case EventType.MainInfoEnd:
                ExecuteButtonEnd();
                break;
            case EventType.MainInfoBack:
                ExecuteButtonBack();
                break;
            case EventType.MainInfoForward:
                ExecuteButtonForward();
                break;
            case EventType.MainInfoUpArrow:
                ExecuteUpArrow();
                break;
            case EventType.MainInfoDownArrow:
                ExecuteDownArrow();
                break;
            case EventType.MainInfoLeftArrow:
                ExecuteLeftArrow();
                break;
            case EventType.MainInfoRightArrow:
                ExecuteRightArrow();
                break;
            case EventType.MainInfoShowMe:
                ExecuteShowMe();
                break;
            case EventType.MainInfoRestore:
                ExecuteRestore();
                break;
            case EventType.MainInfoTickerFaster:
                ExecuteTickerFaster();
                break;
            case EventType.MainInfoTickerSlower:
                ExecuteTickerSlower();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Set colour palette for tooltip
    /// </summary>
    public void SetColours()
    {
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.whiteText);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        if (GameManager.instance.sideScript.PlayerSide.level == 1)
        { colourSide = GameManager.instance.colourScript.GetColour(ColourType.badText); }
        else { colourSide = GameManager.instance.colourScript.GetColour(ColourType.blueText); }
        /*colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourError = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourCancel = GameManager.instance.colourScript.GetColour(ColourType.moccasinText);*/
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }


    /// <summary>
    /// Open Main Info display at start of turn
    /// </summary>
    private void SetMainInfo(MainInfoData data)
    {
        if (data != null)
        {
            //close any Alert Message
            GameManager.instance.alertScript.CloseAlertUI(true);
            currentTurn = GameManager.instance.turnScript.Turn;
            viewTurnNumber = currentTurn;
            // Populate data
            UpdateData(data);
            //exit any generic or node tooltips
            GameManager.instance.tooltipGenericScript.CloseTooltip("MainInfoUI.cs -> SetMainInfo");
            GameManager.instance.tooltipNodeScript.CloseTooltip("MainInfoUI.cs -> SetMainInfo");
            // Display Main page by default
            OpenTab(0);
            // Navigation buttons
            UpdateNavigationStatus();
            // GUI
            mainInfoCanvas.gameObject.SetActive(true);
            //Reset scrollRect to prevent previous position carrying over
            scrollRect.verticalNormalizedPosition = 1.0f;
            //flashers -> Request & Meeting tabs
            SetTabFlashers();
            /*//flares -> moving inwards towards App
            SetFlares();*/
            //ticker tap
            SetTicker(data.tickerText);
            //set modal status
            GameManager.instance.guiScript.SetIsBlocked(true);
            //set game state
            isRunning = true;
            ModalStateData package = new ModalStateData();
            package.mainState = ModalSubState.InfoDisplay;
            package.infoState = ModalInfoSubState.MainInfo;
            GameManager.instance.inputScript.SetModalState(package);
            Debug.LogFormat("[UI] MainInfoUI.cs -> SetMainInfo{0}", "\n");
        }
        else { Debug.LogWarning("Invalid MainInfoData package (Null)"); }
    }


    /// <summary>
    /// Updates cached data in dictionary
    /// NOTE: data checked for null by the calling procedure
    /// </summary>
    /// <param name="data"></param>
    private void UpdateData(MainInfoData data)
    {
        for (int i = 0; i < (int)ItemTab.Count; i++)
        {
            arrayOfItemData[i].Clear();
            arrayOfItemData[i].AddRange(data.arrayOfItemData[i]);
        }
    }

    /// <summary>
    /// switch flashers on/off depending on contents of respective tabs (Request & Meeting, ON for both if items present)
    /// </summary>
    private void SetTabFlashers()
    {
        //request tab
        if (arrayOfItemData[(int)ItemTab.Request].Count > 0)
        {
            flasher_requestTab.gameObject.SetActive(true);
            //commence flashing
            myCoroutineRequest = StartCoroutine("FlashRequestTab");
        }
        else { flasher_requestTab.gameObject.SetActive(false); }
        //meeting tab
        if (arrayOfItemData[(int)ItemTab.Meeting].Count > 0)
        {
            flasher_meetingTab.gameObject.SetActive(true);
            //commence flashing
            myCoroutineMeeting = StartCoroutine("FlashMeetingTab");
        }
        else { flasher_meetingTab.gameObject.SetActive(false); }
    }

    /// <summary>
    /// coroutine to flash alert (white ball) icon above request tab
    /// </summary>
    /// <returns></returns>
    IEnumerator FlashRequestTab()
    {
        while (true)
        {
            if (isRequestFlasherOn == false)
            {
                flasher_requestTab.gameObject.SetActive(true);
                isRequestFlasherOn = true;
                yield return new WaitForSecondsRealtime(flashTimer);
            }
            else
            {
                flasher_requestTab.gameObject.SetActive(false);
                isRequestFlasherOn = false;
                yield return new WaitForSecondsRealtime(flashTimer);
            }
        }
    }

    /// <summary>
    /// coroutine to flash alert (white ball) icon above meeting tab
    /// </summary>
    /// <returns></returns>
    IEnumerator FlashMeetingTab()
    {
        while (true)
        {
            if (isMeetingFlasherOn == false)
            {
                flasher_meetingTab.gameObject.SetActive(true);
                isMeetingFlasherOn = true;
                yield return new WaitForSecondsRealtime(flashTimer);
            }
            else
            {
                flasher_meetingTab.gameObject.SetActive(false);
                isMeetingFlasherOn = false;
                yield return new WaitForSecondsRealtime(flashTimer);
            }
        }
    }

    /*/// <summary>
    /// Start flares moving inwards towards app in a continuous loop while ever app is open
    /// </summary>
    private void SetFlares()
    {
        //reset to start position off screen
        flare_SW.transform.localPosition = startFlareLocalPosition;
        //start movement loop
        myCoroutineFlareSW = StartCoroutine("FlareSW");
    }

    /// <summary>
    /// Stop flare coroutines
    /// </summary>
    private void StopFlares()
    {
        if (myCoroutineFlareSW != null)
        { StopCoroutine(myCoroutineFlareSW); }
    }

    /// <summary>
    /// moves flare up SW line towards APP in a continuous loop
    /// </summary>
    /// <returns></returns>
    IEnumerator FlareSW()
    {
        //NOTE: need Local Position, not 'position' as flare object is a child of a parent object with relative positions to the parent
        Vector3 position = flare_SW.transform.localPosition;
        //endless loop
        while (true)
        {
            if (position.y < 74)
            {
                //moving up vertical
                position.y += 3;
            }
            else
            {
                //moving along horizontal
                if (position.x < 30)
                {
                    //still moving
                    position.x += 3;
                }
                else
                {
                    //reached end point, reset
                    position = startFlareLocalPosition;
                }
            }
            //reassign flare to new position
            flare_SW.transform.localPosition = position;
            yield return null;
        }
    }*/

    /// <summary>
    /// sub Method to display a particular page drawing from cached data in dictOfItemData
    /// </summary>
    /// <param name="tab"></param>
    private void DisplayItemPage(int tabIndex)
    {
        Debug.Assert(tabIndex > -1 && tabIndex < (int)ItemTab.Count, string.Format("Invalid tabIndex {0}", tabIndex));
        //clear out current data
        listOfCurrentPageItemData.Clear();
        //get data
        listOfCurrentPageItemData.AddRange(arrayOfItemData[tabIndex]);
        //display routine
        if (listOfCurrentPageItemData != null)
        {
            numOfItemsCurrent = listOfCurrentPageItemData.Count;
            maxHighlightIndex = numOfItemsCurrent - 1;
            if (numOfItemsCurrent > 0)
            {
                /*//update max number of items
                numOfMaxItem = numOfItemsCurrent;*/
                //populate current messages for the main tab
                for (int index = 0; index < arrayItemText.Length; index++)
                {
                    if (index < numOfItemsCurrent)
                    {
                        //populate text and set item to active
                        arrayItemText[index].text = listOfCurrentPageItemData[index].itemText;
                        arrayItemMain[index].gameObject.SetActive(true);
                        //assign icon
                        switch (listOfCurrentPageItemData[index].priority)
                        {
                            case ItemPriority.High:
                                arrayItemIcon[index].sprite = priorityHigh;
                                break;
                            case ItemPriority.Medium:
                                arrayItemIcon[index].sprite = priorityMedium;
                                break;
                            case ItemPriority.Low:
                                arrayItemIcon[index].sprite = priorityLow;
                                break;
                            default:
                                Debug.LogWarningFormat("Invalid priority \"{0}\"", listOfCurrentPageItemData[index].priority);
                                break;
                        }

                    }
                    else if (index < numOfItemsPrevious)
                    {
                        //efficient -> only disables items that were previously active, not the whole set
                        arrayItemText[index].text = "";
                        arrayItemMain[index].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //no data, blank previous items (not necessarily all), disable line
                for (int index = 0; index < numOfItemsPrevious; index++)
                {
                    arrayItemText[index].text = "";
                    arrayItemMain[index].gameObject.SetActive(false);
                }

            }
            //update previous count to current
            numOfItemsPrevious = numOfItemsCurrent;
            //set header
            SetPageHeader(numOfItemsCurrent);
        }
        else { Debug.LogWarning("Invalid MainInfoData.listOfMainText (Null)"); }
        //manually activate / deactivate scrollBar as needed (because you've got daactivated objects in the scroll list the bar shows regardless unless you override here)
        if (numOfItemsCurrent <= numOfVisibleItems)
        {
            scrollRect.verticalScrollbar = null;
            scrollBarObject.SetActive(false);
        }
        else
        {
            scrollBarObject.SetActive(true);
            scrollRect.verticalScrollbar = scrollBar;
        }
    }

    /// <summary>
    /// handles formatting of page header string in a constant format
    /// </summary>
    /// <param name="numOfItems"></param>
    /// <returns></returns>
    private void SetPageHeader(int numOfItems)
    {
        string text = "Unknown";
        string colourDay = colourGrey;
        if (viewTurnNumber == currentTurn) { colourDay = colourDefault; }
        if (numOfItems > 0)
        { text = string.Format("{0}Day {1}, 2033, there {2} {3} item{4}{5}", colourDay, viewTurnNumber, numOfItems != 1 ? "are" : "is", numOfItems, numOfItems != 1 ? "s" : "", colourEnd); }
        else
        { text = string.Format("{0}Day {1}, 2033, there are 0 items{2}", colourDay, viewTurnNumber, colourEnd); }
        page_header.text = text;
    }

    /// <summary>
    /// close Main Info display
    /// </summary>
    private void CloseMainInfo()
    {
        GameManager.instance.tooltipGenericScript.CloseTooltip("MainInfoUI.cs -> CloseMainInfo");
        GameManager.instance.tooltipHelpScript.CloseTooltip("MainInfoUI.cs -> CloseMainInfo");
        mainInfoCanvas.gameObject.SetActive(false);
        GameManager.instance.guiScript.SetIsBlocked(false);
        //switch off coroutines
        if (myCoroutineRequest != null)
        { StopCoroutine(myCoroutineRequest); }
        if (myCoroutineMeeting != null)
        { StopCoroutine(myCoroutineMeeting); }
        if (myCoroutineTicker != null)
        { StopCoroutine(myCoroutineTicker); }
        //switch of AlertUI 
        GameManager.instance.alertScript.CloseAlertUI();
        /*StopFlares();*/
        //set game state
        isRunning = false;
        GameManager.instance.inputScript.ResetStates();
        Debug.LogFormat("[UI] MainInfoUI.cs -> CloseMainInfo{0}", "\n");
    }


    /// <summary>
    /// Open the designated tab and close whatever is open
    /// </summary>
    /// <param name="tabIndex"></param>
    private void OpenTab(int tabIndex)
    {
        Debug.Assert(tabIndex > -1 && tabIndex < 6, string.Format("Invalid tab index {0}", tabIndex));
        //reset Active tabs to reflect new status
        for (int index = 0; index < tabActiveArray.Length; index++)
        {
            //activate indicated tab and deactivate the rest
            if (index == tabIndex)
            { tabActiveArray[index].gameObject.SetActive(true); }
            else { tabActiveArray[index].gameObject.SetActive(false); }
            //display tab help in RHS panel
            Tuple<string, string> results = GetTabHelp(tabIndex);
            details_text_top.text = results.Item1;
            details_text_bottom.text = results.Item2;
        }
        //hide both RHS buttons (help and decision)
        buttonHelpCentre.gameObject.SetActive(false);
        buttonHelpCombined.gameObject.SetActive(false);
        buttonItem.gameObject.SetActive(false);
        //redrawn main page
        DisplayItemPage(tabIndex);
        //assign default info icon
        details_image.gameObject.SetActive(true);
        details_image.sprite = details_image_sprite;
        //update indexes
        highlightIndex = -1;
        currentTabIndex = tabIndex;
    }

    /// <summary>
    /// ItemData details
    /// </summary>
    /// <param name="itemIndex"></param>
    private void ShowItemDetails(int itemIndex)
    {
        ItemData data = listOfCurrentPageItemData[itemIndex];
        if (data != null)
        {
            //main item data
            details_text_top.text = data.topText;
            details_text_bottom.text = data.bottomText;
            if (data.sprite != null)
            {
                details_image.sprite = data.sprite;
                details_image.gameObject.SetActive(true);
            }
            //if no sprite, switch off default info sprite and leave blak
            else { details_image.gameObject.SetActive(false); }

            /*//display button if event & data present
            if (data.buttonEvent > EventType.None && data.buttonData != -1)
            {
                //set button event
                buttonInteractionDecision.SetButton(data.buttonEvent, data.buttonData);
                //hide help and make button active
                buttonDecision.gameObject.SetActive(true);
                buttonHelp.gameObject.SetActive(false);
            }*/

            //Show me data
            currentItemNodeID = data.nodeID;
            currentItemConnID = data.connID;
            if (data.nodeID > -1 || data.connID > -1)
            {
                //hide help and make button active
                buttonInteractionItem.SetButton(EventType.MainInfoShowMe);
                buttonItemText.text = "Show Me";
                buttonItem.gameObject.SetActive(true);
                buttonHelpCentre.gameObject.SetActive(false);
                if (data.help > -1)
                {
                    buttonHelpCombined.gameObject.SetActive(true);
                    itemHelpCombined.listOfHelp = GetItemHelpList(data);
                }
            }
            else
            {
                //hide button and display help button

                //set tooltip interaction -> TO DO

                buttonItem.gameObject.SetActive(false);
                buttonHelpCombined.gameObject.SetActive(false);
                //display help only if available
                if (data.help > -1)
                {
                    buttonHelpCentre.gameObject.SetActive(true);
                    itemHelpCentre.listOfHelp = GetItemHelpList(data);
                }
                else { buttonHelpCentre.gameObject.SetActive(false); }
            }
            //remove highlight
            if (highlightIndex != itemIndex)
            {
                //reset currently highlighted back to default
                if (highlightIndex > -1)
                { arrayItemText[highlightIndex].text = string.Format("{0}{1}{2}", colourDefault, listOfCurrentPageItemData[highlightIndex].itemText, colourEnd); }
                highlightIndex = itemIndex;
                //highlight item -> show as yellow
                arrayItemText[itemIndex].text = string.Format("{0}<b>{1}</b>{2}", colourNeutral, listOfCurrentPageItemData[itemIndex].itemText, colourEnd);
            }
        }
        else
        {
            Debug.LogWarningFormat("Invalid ItemData for listOfCurrentPageItemData[{0}]", itemIndex);
            //set default values for showMe data so data doesn't carry over
            currentItemNodeID = -1;
            currentItemConnID = -1;
        }
    }

    /// <summary>
    /// Assembles a list of HelpData for the item to pass onto the help components. Returns empty if none.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private List<HelpData> GetItemHelpList(ItemData data)
    {
        string tag0, tag1, tag2, tag3;
        //Debug
        if (string.IsNullOrEmpty(data.tag0) == true)
        {
            //default data
            tag0 = "test0";
            tag1 = ""; tag2 = ""; tag3 = "";
        }
        else
        {
            //item specified help
            tag0 = data.tag0;
            tag1 = data.tag1;
            tag2 = data.tag2;
            tag3 = data.tag3;
        }
        return GetHelpData(tag0, tag1, tag2, tag3);
    }

    /// <summary>
    /// hard wired help for Main info app info
    /// </summary>
    /// <returns></returns>
    private List<HelpData> GetInfoHelpList()
    { return GetHelpData("info_app_0", "info_app_1", "info_app_2", "info_app_3"); }

    /// <summary>
    /// sub method to pull data from dict and put list together
    /// </summary>
    /// <returns></returns>
    public List<HelpData> GetHelpData(string tag0, string tag1, string tag2, string tag3)
    {
        List<HelpData> listOfHelp = new List<HelpData>();
        //first topic, skip if null
        if (string.IsNullOrEmpty(tag0) == false)
        {
            HelpData help0 = GameManager.instance.dataScript.GetHelpData(tag0);
            if (help0 != null)
            { listOfHelp.Add(help0); }
            else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag0 \"{0}\"", tag0); }
        }
        //second topic, skip if null
        if (string.IsNullOrEmpty(tag1) == false)
        {
            HelpData help1 = GameManager.instance.dataScript.GetHelpData(tag1);
            if (help1 != null)
            { listOfHelp.Add(help1); }
            else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag1 \"{0}\"", tag1); }
        }
        //thrid topic, skip if null
        if (string.IsNullOrEmpty(tag2) == false)
        {
            HelpData help2 = GameManager.instance.dataScript.GetHelpData(tag2);
            if (help2 != null)
            { listOfHelp.Add(help2); }
            else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag2 \"{0}\"", tag2); }
        }
        //fourth topic, skip if null
        if (string.IsNullOrEmpty(tag3) == false)
        {
            HelpData help3 = GameManager.instance.dataScript.GetHelpData(tag3);
            if (help3 != null)
            { listOfHelp.Add(help3); }
            else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag3 \"{0}\"", tag3); }
        }
        return listOfHelp;
    }


    /*/// <summary>
    /// Special method for the last tab, Help (hard wired info, not dynamic)
    /// </summary>
    private void DisplayHelpPage()
    {
        //TO DO
    }*/

    /// <summary>
    /// provides pre-formatted help text for RHS detail panel when tabs are first opened
    /// </summary>
    /// <param name="tabIndex"></param>
    /// <returns></returns>
    private Tuple<string, string> GetTabHelp(int tabIndex)
    {
        string textTop = "";
        string textBottom = "";
        StringBuilder builder = new StringBuilder();
        switch ((ItemTab)tabIndex)
        {
            case ItemTab.ALERTS:
                textTop = "Incoming Messages";
                builder.AppendFormat("{0}<b>Click</b>{1} on an {2}<b>Item</b>{3}{4}for more information", colourNeutral, colourEnd, colourNeutral, colourEnd, "\n");
                builder.AppendLine(); builder.AppendLine();
                builder.Append("Items are ordered by priority");
                builder.AppendLine(); builder.AppendLine();
                builder.AppendFormat("{0}<b>No action required</b>{1}", colourNeutral, colourEnd);
                break;
            case ItemTab.Request:
                textTop = "Make a Request";
                builder.AppendFormat("You can request a {0}<b>Meeting</b>{1}{2}{3}<b>OTHER PARTIES</b>{4} can request a meeting with you{5}{6}", colourNeutral, colourEnd, "\n",
                    colourAlert, colourEnd, "\n", "\n");
                builder.AppendFormat("You can, <i>if you wish</i>,{0}select {1}<b>ONE</b>{2} request", "\n", colourNeutral, colourEnd);
                builder.AppendFormat("{0}{1}The meeting will be at the {2}<b>Start</b>{3}{4}of the {5}<b>Next</b>{6} day", "\n", "\n",
                    colourNeutral, colourEnd, "\n", colourNeutral, colourEnd);
                break;
            case ItemTab.Meeting:
                textTop = "Resolve a Meeting";
                builder.AppendFormat("During your Meeting you can{0}choose {1}<b>ONE</b>{2} option", "\n", colourNeutral, colourEnd);
                builder.AppendLine(); builder.AppendLine();
                builder.AppendFormat("If you decide to do nothing{0}the {1}<b>DEFAULT OPTION</b>{2}{3}will be chosen for you", "\n", colourAlert, colourEnd, "\n");
                builder.AppendLine(); builder.AppendLine();
                builder.AppendFormat("There will be a {0}<b>Cooldown period</b>{1}{2}before you can meet this{3}person, faction or organisation{4}again", colourNeutral, colourEnd,
                    "\n", "\n", "\n");
                break;
            case ItemTab.Effects:
                textTop = "Ongoing Effects";
                builder.AppendFormat("<b>All effects currently impacting{0}the game</b>", "\n");
                builder.AppendLine(); builder.AppendLine();
                builder.AppendFormat("Sorted by type{0}{1}Mouseover {2}<b>Icons</b>{3} to see type", "\n", "\n", colourNeutral, colourEnd);
                break;
            case ItemTab.Traits:
                textTop = "Traits Used";
                builder.AppendFormat("{0}<b>All traits used by Subordinates</b>{1}{2}{3}at the start of this or the previous day", colourNeutral, colourEnd, "\n", "\n");
                break;
            case ItemTab.Random:
                textTop = "Random Outcomes";
                builder.AppendFormat("{0}<b>All important events that required{1}{2}a random roll are shown here</b>", colourNeutral, colourEnd, "\n");
                builder.AppendLine(); builder.AppendLine();
                builder.Append("Events are for the start of this or the Previous day");
                builder.AppendLine(); builder.AppendLine();
                builder.AppendFormat("All rolls use a{0}{1}<b>Percentage</b>{2}{3}1d100 die", "\n", colourNeutral, colourEnd, "\n");
                break;
            default:
                //Mystery tab, doesn't exist
                textTop = "Unknown";
                break;
        }
        textBottom = builder.ToString();
        return new Tuple<string, string>(textTop, textBottom);
    }

    /// <summary>
    /// Open MainInfoApp between turns 
    /// </summary>
    private void OpenMainInfoBetweenTurns()
    {
        //can't do on first turn (needs to be initialised at least once) or if already running
        if (currentTurn > 0 && isRunning == false)
        {
            //only if player is active
            if (GameManager.instance.playerScript.status == ActorStatus.Active)
            {
                MainInfoData data = GameManager.instance.dataScript.GetCurrentInfoData();
                if (data != null)
                {
                    SetMainInfo(data);
                }
                else { Debug.LogWarning("Invalid MainInfoData (Null)"); }
            }
            else
            {
                //outcome message explaining why not
                GameManager.instance.guiScript.SetAlertMessage(AlertType.PlayerStatus);
            }
        }
    }

    /// <summary>
    /// press Home button -> jump to current turn
    /// </summary>
    private void ExecuteButtonHome()
    {
        //get & update data
        MainInfoData data = GameManager.instance.dataScript.GetNotifications();
        if (data != null)
        {
            viewTurnNumber = currentTurn;
            UpdateData(data);
            //Open last used tab
            if (currentTabIndex > -1)
            { OpenTab(currentTabIndex); }
            else
            {
                //default to main tab -> should never be used
                Debug.Assert(currentTabIndex < 0, "Invalid currentTabIndex (< 0)");
                OpenTab(0);
            }
            //update button status
            UpdateNavigationStatus();
        }
        else { Debug.LogWarning("Invalid data (Null)"); }
    }

    /// <summary>
    /// press End shortcut key -> jump to first turn
    /// </summary>
    private void ExecuteButtonEnd()
    {
        //get & update data
        viewTurnNumber = 0;
        MainInfoData data = GameManager.instance.dataScript.GetNotifications(viewTurnNumber);
        if (data != null)
        {
            UpdateData(data);
            //Open last used tab
            if (currentTabIndex > -1)
            { OpenTab(currentTabIndex); }
            else
            {
                //default to main tab -> should never be used
                Debug.Assert(currentTabIndex < 0, "Invalid currentTabIndex (< 0)");
                OpenTab(0);
            }
            //update button status
            UpdateNavigationStatus();
        }
        else { Debug.LogWarning("Invalid data (Null)"); }
    }

    /// <summary>
    /// press Back button -> go back one turn (until start)
    /// </summary>
    private void ExecuteButtonBack()
    {
        //get & update data
        viewTurnNumber -= 1;
        viewTurnNumber = Mathf.Max(0, viewTurnNumber);
        MainInfoData data = GameManager.instance.dataScript.GetNotifications(viewTurnNumber);
        if (data != null)
        {
            UpdateData(data);
            //Open last used tab
            if (currentTabIndex > -1)
            { OpenTab(currentTabIndex); }
            else
            {
                //default to main tab -> should never be used
                Debug.Assert(currentTabIndex < 0, "Invalid currentTabIndex (< 0)");
                OpenTab(0);
            }
            //update button status
            UpdateNavigationStatus();
        }
        else { Debug.LogWarning("Invalid data (Null)"); }
    }

    /// <summary>
    /// press Forward button -> go forward one turn (until current)
    /// </summary>
    private void ExecuteButtonForward()
    {
        //get & update data
        viewTurnNumber += 1;
        viewTurnNumber = Mathf.Min(currentTurn, viewTurnNumber);
        MainInfoData data = GameManager.instance.dataScript.GetNotifications(viewTurnNumber);
        if (data != null)
        {
            UpdateData(data);
            //Open last used tab
            if (currentTabIndex > -1)
            { OpenTab(currentTabIndex); }
            else
            {
                //default to main tab -> should never be used
                Debug.Assert(currentTabIndex < 0, "Invalid currentTabIndex (< 0)");
                OpenTab(0);
            }
            //update button status
            UpdateNavigationStatus();
        }
        else { Debug.LogWarning("Invalid data (Null)"); }
    }

    /// <summary>
    /// Down arrow (down next item on page)
    /// </summary>
    private void ExecuteDownArrow()
    {
        if (highlightIndex > -1)
        {
            if (highlightIndex < (maxHighlightIndex))
            { ShowItemDetails(highlightIndex + 1); }
        }
        else if (maxHighlightIndex > -1)
        {
            //at tab, jump to first entry if present
            ShowItemDetails(0);
        }
    }

    /// <summary>
    /// Up arrow (up next item on page)
    /// </summary>
    private void ExecuteUpArrow()
    {
        if (highlightIndex > -1)
        {
            if (highlightIndex > 0)
            { ShowItemDetails(highlightIndex - 1); }
            else
            {
                //at top of page, go to tab
                highlightIndex = -1;
                OpenTab(currentTabIndex);
            }
        }
    }

    /// <summary>
    /// Left arrow (left to next tab)
    /// </summary>
    private void ExecuteLeftArrow()
    {
        //change tab
        if (currentTabIndex > -1)
        {
            if (currentTabIndex > 0)
            {
                currentTabIndex -= 1;
                OpenTab(currentTabIndex);
            }
        }
    }

    /// <summary>
    /// Right arrow (right to next tab)
    /// </summary>
    private void ExecuteRightArrow()
    {
        if (currentTabIndex > -1)
        {
            if (currentTabIndex < maxTabIndex)
            {
                currentTabIndex += 1;
                OpenTab(currentTabIndex);
            }
        }
    }

    /// <summary>
    /// 'Show Me' a node or connection in an item, switch off infoApp and highlight onmap
    /// </summary>
    private void ExecuteShowMe()
    {
        //only do so if there is something to show
        if (currentItemNodeID > -1 || currentItemConnID > -1)
        {
            //turn off infoApp
            mainInfoCanvas.gameObject.SetActive(false);
            //pass data package to GUIManager.cs
            ShowMeData data = new ShowMeData();
            data.restoreEvent = EventType.MainInfoRestore;
            data.nodeID = currentItemNodeID;
            data.connID = currentItemConnID;
            GameManager.instance.guiScript.SetShowMe(data);
        }
    }

    /// <summary>
    /// Restore InfoApp after a 'ShowMe'
    /// </summary>
    private void ExecuteRestore()
    {
        //set game state
        ModalStateData package = new ModalStateData();
        package.mainState = ModalSubState.InfoDisplay;
        package.infoState = ModalInfoSubState.MainInfo;
        GameManager.instance.inputScript.SetModalState(package);
        //alert message
        GameManager.instance.alertScript.CloseAlertUI();
        //turn infoApp back on
        mainInfoCanvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// Increase speed of MainInfo Ticker
    /// </summary>
    private void ExecuteTickerFaster()
    {
        tickerSpeed++;
        tickerSpeed = Mathf.Min(tickerSpeedMax, tickerSpeed);
        //feedback to player
        GameManager.instance.alertScript.SetAlertUI(string.Format("{0}<b>News Ticker Speed increased to {1}</b>{2}", colourNeutral, tickerSpeed, colourEnd));
    }

    /// <summary>
    /// Decrease speed of MainInfo Ticker
    /// </summary>
    private void ExecuteTickerSlower()
    {
        tickerSpeed--;
        tickerSpeed = Mathf.Max(tickerSpeedMin, tickerSpeed);
        //feedback to player
        GameManager.instance.alertScript.SetAlertUI(string.Format("</b>News Ticker Speed decreased to {0}</b>", tickerSpeed));
    }

    /// <summary>
    /// subMethod to update status of three navigation buttons. Contains all necessary logic to auto set buttons
    /// </summary>
    /// <param name="isHome"></param>
    /// <param name="isBack"></param>
    /// <param name="isForward"></param>
    private void UpdateNavigationStatus()
    {
        //home button -> only if not at current turn
        if (currentTurn != viewTurnNumber)
        { buttonHome.gameObject.SetActive(true); }
        else { buttonHome.gameObject.SetActive(false); }
        //back button
        if (viewTurnNumber > 1)
        { buttonBack.gameObject.SetActive(true); }
        else { buttonBack.gameObject.SetActive(false); }
        //forward button
        if (viewTurnNumber < currentTurn)
        { buttonForward.gameObject.SetActive(true); }
        else { buttonForward.gameObject.SetActive(false); }
    }

       
    /// <summary>
    /// Set background image and cancel button for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    private void ChangeSides(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        switch (side.level)
        {
            case 1:

                break;
            case 2:

                break;
        }
    }

    /// <summary>
    /// start ticker tape each turn provided there is news to display
    /// </summary>
    /// <param name="text"></param>
    private void SetTicker(string text)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            string sourceText = text.ToUpper();
            tickerText.text = sourceText;
            //lossyScale prevents scrolling jitters
            float width = tickerText.preferredWidth * tickerRectTransform.lossyScale.x;
            //need to calc width before populating clone as width takes into account length of both (clone is a child of tickerText)
            cloneTickerText.text = sourceText;
            myCoroutineTicker = StartCoroutine("TickerTape", width);
        }
        else { Debug.LogWarning("Invalid ticker text (Null or Empty)"); }
    }


    /// <summary>
    /// Run ticker tape across the top of the info App
    /// </summary>
    /// <returns></returns>
    private IEnumerator TickerTape(float width)
    {
        float scrollPosition = 0;
        Vector3 startPosition = tickerRectTransform.position;
        while (true)
        {
            //scroll the text across the screen by moving the RectTransform (everytime the scrollPosition gets bigger than the width, it'll warp back to zero, giving us our loop)
            tickerRectTransform.position = new Vector3(-scrollPosition % width, startPosition.y, startPosition.z);
            if (scrollPosition >= width) { scrollPosition = 0; }
            /*Debug.LogFormat("[Tst] scrollPosition -> {0}, width -> {1}, scrollPosition % width -> {2}{3}", scrollPosition, width, scrollPosition % width, "\n");*/
            scrollPosition += tickerSpeed * 10 * Time.deltaTime;
            yield return null;
        }
    }




    //new methods above here
}
