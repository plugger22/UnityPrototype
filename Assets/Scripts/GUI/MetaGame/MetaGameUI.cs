using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles MetaGame UI
/// </summary>
public class MetaGameUI : MonoBehaviour
{
    [Header("Main")]
    public Canvas canvasMeta;
    public Canvas canvasScroll;
    public GameObject metaObject;

    [Header("Backgrounds")]
    public Image backgroundMain;
    public Image backgroundLeft;
    public Image backgroundCentre;
    public Image backgroundRight;

    [Header("Buttons")]
    public Button buttonReset;
    public Button buttonConfirm;
    public Button buttonRecommended;
    [Tooltip("Right Hand side")]
    public Button buttonHelpCentre;
    [Tooltip("Right Hand side")]
    public Button buttonHelpCombined;
    [Tooltip("Right Hand side")]
    public Button buttonSelect;
    [Tooltip("Right Hand Side")]
    public Button buttonDeselect;

    [Header("HQ Tabs")]
    public GameObject tabBoss;
    public GameObject tabSubBoss1;
    public GameObject tabSubBoss2;
    public GameObject tabSubBoss3;

    [Header("Top Tabs")]
    public Image tabStatus;
    public Image tabSelected;
    public TextMeshProUGUI textStatus;
    public TextMeshProUGUI textSelected;

    [Header("Top")]
    public Image renownBackground;
    public TextMeshProUGUI renownAmount;

    [Header("Centre Miscellanous")]
    public TextMeshProUGUI page_header;
    public GameObject scrollBarObject;
    public GameObject scrollBackground;         //needed to get scrollRect component in order to manually disable scrolling when not needed

    [Header("MetaOption Items")]
    public GameObject meta_item_0;
    public GameObject meta_item_1;
    public GameObject meta_item_2;
    public GameObject meta_item_3;
    public GameObject meta_item_4;
    public GameObject meta_item_5;
    public GameObject meta_item_6;
    public GameObject meta_item_7;
    public GameObject meta_item_8;
    public GameObject meta_item_9;
    public GameObject meta_item_10;
    public GameObject meta_item_11;
    public GameObject meta_item_12;
    public GameObject meta_item_13;
    public GameObject meta_item_14;
    public GameObject meta_item_15;
    public GameObject meta_item_16;
    public GameObject meta_item_17;
    public GameObject meta_item_18;
    public GameObject meta_item_19;

    [Header("RHS details")]
    public TextMeshProUGUI rightTextTop;
    public TextMeshProUGUI rightTextBottom;
    public Image rightImage;
    public Sprite rightImageDefault;                     //default sprite

    //tabs
    private int currentSideTabIndex = -1;                           //side tabs (top to bottom)
    private int currentTopTabIndex = -1;                            //top tabs (left to right)
    private int maxSideTabIndex;                                    //side tabs only (used for keeping pgUP/DOWN movement within HQ side tabs)
    private int maxTopTabIndex;                                     //top tabs only
    private int numOfSideTabs;                                      //keyed off enum.MetaTabSide
    private int numOfTopTabs;                                       //keyed off enum.MetaTabTop
    private int offset = 1;                                         //used with '(ActorHQ)index + offset' to account for the ActorHQ.enum having index 0 being 'None'

    //button script handlers
    private ButtonInteraction buttonInteractionReset;
    private ButtonInteraction buttonInteractionConfirm;
    private ButtonInteraction buttonInteractionRecommended;
    private ButtonInteraction buttonInteractionSelect;
    private ButtonInteraction buttonInteractionDeselect;

    //button help components                       
    private GenericHelpTooltipUI helpCentre;                    //item help button on RHS panel (where no 'Select' button exists)
    private GenericHelpTooltipUI helpCombined;                  //item help button on RHS panel (adjacent to 'Select' button

    //Side arrays -> tabs
    private GameObject[] arrayOfSideTabObjects;
    private MetaInteraction[] arrayOfSideTabItems;
    private MetaSideTabUI[] arrayOfSideTabInteractions;
    private int[] arrayOfSideTabOptions;                            //count of how many ACTIVE options are available for this tab
    //Side arrays -> metaData
    private GameObject[] arrayOfSideMetaMain;
    private TextMeshProUGUI[] arrayOfSideMetaText;
    private Image[] arrayOfSideMetaIcon;
    private Image[] arrayOfSideMetaBorder;
    private Image[] arrayOfSideMetaCheckMark;
    private Image[] arrayOfSideMetaBackground;

    //Top arrays -> tabs
    private Image[] arrayOfTopTabImages;
    private MetaTopTabUI[] arrayOfTopTabInteractions;

    private int[] arrayOfTopTabOptions;                            //count of how many ACTIVE options are available for this tab
    //Top arrays -> metaData
    private GameObject[] arrayOfTopMetaMain;
    private TextMeshProUGUI[] arrayOfTopMetaText;
    private Image[] arrayOfTopMetaIcon;
    private Image[] arrayOfTopMetaBorder;
    private Image[] arrayOfTopMetaCheckMark;
    private Image[] arrayOfTopMetaBackground;

    //item priority sprites
    private Sprite priorityHigh;
    private Sprite priorityMedium;
    private Sprite priorityLow;
    private Sprite priorityInactive;

    //MetaData -> side tabs
    private List<MetaData>[] arrayOfSideMetaData = new List<MetaData>[(int)MetaTabSide.Count];      //One dataset for each side tab (excluding Help tab)
    List<MetaData> listOfCurrentPageSideMetaData;                                                   //current data for currently displayed page (side tabs)
    //MetaData -> top tabs
    private List<MetaData>[] arrayOfTopMetaData = new List<MetaData>[(int)MetaTabTop.Count];        //One dataset for each top tab (excluding Help tab)
    List<MetaData> listOfCurrentPageTopMetaData;                                                    //current data for currently displayed page (top tabs)
    //Player selected metaData (dynamic, dict for speed)
    Dictionary<string, MetaData> dictOfSelected = new Dictionary<string, MetaData>();               //selected items, key is metaData.metaName, value metaData

    //scroll bar LHS
    private ScrollRect scrollRect;                                   //needed to manually disable scrolling when not needed
    private Scrollbar scrollBar;

    private bool isRunning;
    private bool isStatusData;                                          //true if Player status data (metaOption.isPlayerStatus true) present, false otherwise
    private int highlightIndex = -1;                                 //item index of currently highlighted item
    private int maxHighlightIndex = -1;
    private int numOfItemsTotal = 20;                                //hardwired Max number of items -> 30
    private int numOfVisibleItems = 10;                              //hardwired visible items in main page -> 10
    private int numOfItemsCurrent = -1;                              //count of items in current list / page
    private int numOfItemsPrevious = -1;                             //count of items in previous list / page
    private int renownCurrent;                                       //current amount of renown remaining available to spend

    //fast access
    private int costLow = -1;                                        //renown cost for low priority metaOptions
    private int costMedium = -1;
    private int costHigh = -1;
    private int costExtreme = -1;

    //colours
    string colourDefault;
    string colourNeutral;
    string colourGrey;
    string colourAlert;
    /*string colourGood;
    string colourBlue;*/
    string colourNormal;
    /*string colourError;
    string colourInvalid;*/
    string colourCancel;
    string colourEnd;


    //static reference
    private static MetaGameUI metaGameUI;


    /// <summary>
    /// provide a static reference to ModalMetaUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static MetaGameUI Instance()
    {
        if (!metaGameUI)
        {
            metaGameUI = FindObjectOfType(typeof(MetaGameUI)) as MetaGameUI;
            if (!metaGameUI)
            { Debug.LogError("There needs to be one active MetaGameUI script on a GameObject in your scene"); }
        }
        return metaGameUI;
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
    /// <summary>
    /// Session start
    /// </summary>
    private void SubInitialiseSessionStart()
    {
        int index = 0;
        numOfSideTabs = (int)MetaTabSide.Count;
        numOfTopTabs = (int)MetaTabTop.Count;
        //initialise Side Arrays -> tabs
        arrayOfSideTabObjects = new GameObject[numOfSideTabs];
        arrayOfSideTabItems = new MetaInteraction[numOfSideTabs];
        arrayOfSideTabInteractions = new MetaSideTabUI[numOfSideTabs];
        arrayOfSideTabOptions = new int[numOfSideTabs];
        //initialise  Side Arrays -> items
        arrayOfSideMetaMain = new GameObject[numOfItemsTotal];
        arrayOfSideMetaIcon = new Image[numOfItemsTotal];
        arrayOfSideMetaBorder = new Image[numOfItemsTotal];
        arrayOfSideMetaCheckMark = new Image[numOfItemsTotal];
        arrayOfSideMetaBackground = new Image[numOfItemsTotal];
        arrayOfSideMetaText = new TextMeshProUGUI[numOfItemsTotal];
        //initialise Top Arrays -> tabs
        arrayOfTopTabImages = new Image[numOfTopTabs];
        arrayOfTopTabInteractions = new MetaTopTabUI[numOfTopTabs];
        arrayOfTopTabOptions = new int[numOfTopTabs];
        //initialise  Top Arrays -> items
        arrayOfTopMetaMain = new GameObject[numOfItemsTotal];
        arrayOfTopMetaIcon = new Image[numOfItemsTotal];
        arrayOfTopMetaBorder = new Image[numOfItemsTotal];
        arrayOfTopMetaCheckMark = new Image[numOfItemsTotal];
        arrayOfTopMetaBackground = new Image[numOfItemsTotal];
        arrayOfTopMetaText = new TextMeshProUGUI[numOfItemsTotal];
        //current pages side and top
        listOfCurrentPageSideMetaData = new List<MetaData>();
        listOfCurrentPageTopMetaData = new List<MetaData>();
        //initialise metaData array lists (side and top tabs)
        for (int i = 0; i < (int)MetaTabSide.Count; i++)
        { arrayOfSideMetaData[i] = new List<MetaData>(); }
        for (int i = 0; i < (int)MetaTabTop.Count; i++)
        { arrayOfTopMetaData[i] = new List<MetaData>(); }
        //canvas
        canvasScroll.gameObject.SetActive(true);
        //max tabs
        maxSideTabIndex = numOfSideTabs - 1;
        maxTopTabIndex = numOfTopTabs - 1;
        //tab components
        if (tabBoss != null) { arrayOfSideTabObjects[index++] = tabBoss; } else { Debug.LogError("Invalid tabBoss (Null)"); }
        if (tabSubBoss1 != null) { arrayOfSideTabObjects[index++] = tabSubBoss1; } else { Debug.LogError("Invalid tabSubBoss1 (Null)"); }
        if (tabSubBoss2 != null) { arrayOfSideTabObjects[index++] = tabSubBoss2; } else { Debug.LogError("Invalid tabSubBoss2 (Null)"); }
        if (tabSubBoss3 != null) { arrayOfSideTabObjects[index++] = tabSubBoss3; } else { Debug.LogError("Invalid tabSubBoss3 (Null)"); }
        //initialise side tab arrays -> interaction
        for (int i = 0; i < numOfSideTabs; i++)
        {
            if (arrayOfSideTabObjects[i] != null)
            {
                MetaInteraction metaInteract = arrayOfSideTabObjects[i].GetComponent<MetaInteraction>();
                if (metaInteract != null)
                {
                    arrayOfSideTabItems[i] = metaInteract;
                    //tab interaction
                    MetaSideTabUI sideTab = metaInteract.background.GetComponent<MetaSideTabUI>();
                    if (sideTab != null)
                    {
                        arrayOfSideTabInteractions[i] = sideTab;
                        //identify tabs
                        sideTab.SetTabIndex(i, maxSideTabIndex);
                    }
                    else { Debug.LogErrorFormat("Invalid MetaSideTabUI (Null) for arrayOfSideTabItems[{0}]", i); }
                }
                else { Debug.LogErrorFormat("Invalid MetaInteraction (Null) for arrayOfSideTabObject[{0}]", i); }
            }
        }
        //top tabs
        index = 0;
        if (tabStatus != null) { arrayOfTopTabImages[index++] = tabStatus; } else { Debug.LogError("Invalid tabStatus (Null)"); }
        if (tabStatus != null) { arrayOfTopTabImages[index++] = tabSelected; } else { Debug.LogError("Invalid tabSelected (Null)"); }
        //initialise top tab arrays -> interaction
        for (int i = 0; i < numOfTopTabs; i++)
        {
            if (arrayOfTopTabImages[i] != null)
            {
                MetaTopTabUI topTab = arrayOfTopTabImages[i].GetComponent<MetaTopTabUI>();
                if (topTab != null)
                {
                    arrayOfTopTabInteractions[i] = topTab;
                    //identify tabs
                    topTab.SetTabIndex(i, maxTopTabIndex);
                }
                else { Debug.LogErrorFormat("Invalid MetaTopTabUI (Null) for arrayOfTopTabItems[{0}]", i); }
            }
            else { Debug.LogErrorFormat("Invalid MetaInteraction (Null) for arrayOfTopTabImages[{0}]", i); }
        }

        Debug.Assert(textStatus != null, "Invalid textStatus (Null)");
        Debug.Assert(textSelected != null, "Invalid textSelected (Null)");
        //main
        Debug.Assert(canvasMeta != null, "Invalid canvasMeta (Null)");
        Debug.Assert(canvasScroll != null, "Invalid canvasScroll (Null)");
        Debug.Assert(metaObject != null, "Invalid metaObject (Null)");
        //buttons
        Debug.Assert(buttonReset != null, "Invalid buttonReset (Null)");
        Debug.Assert(buttonConfirm != null, "Invalid buttonConfirm (Null)");
        Debug.Assert(buttonRecommended != null, "Invalid buttonRecommended (Null)");
        Debug.Assert(buttonSelect != null, "Invalid buttonSelect (Null)");
        Debug.Assert(buttonDeselect != null, "Invalid buttonDeselect (Null)");
        //buttons -> get interaction components
        buttonInteractionReset = buttonReset.GetComponent<ButtonInteraction>();
        buttonInteractionConfirm = buttonConfirm.GetComponent<ButtonInteraction>();
        buttonInteractionRecommended = buttonRecommended.GetComponent<ButtonInteraction>();
        buttonInteractionSelect = buttonSelect.GetComponent<ButtonInteraction>();
        buttonInteractionDeselect = buttonDeselect.GetComponent<ButtonInteraction>();
        Debug.Assert(buttonInteractionReset != null, "Invalid buttonInteractionReset (Null)");
        Debug.Assert(buttonInteractionConfirm != null, "Invalid buttonInteractionConfirm (Null)");
        Debug.Assert(buttonInteractionRecommended != null, "Invalid buttonInteractionRecommended (Null)");
        Debug.Assert(buttonInteractionSelect != null, "Invalid buttonInteractionSelect (Null)");
        Debug.Assert(buttonInteractionDeselect != null, "Invalid buttonInteractionDeselect (Null)");
        //buttons -> assign button events
        buttonInteractionConfirm.SetButton(EventType.MetaGameConfirm);
        buttonInteractionSelect.SetButton(EventType.MetaGameSelect);
        buttonInteractionDeselect.SetButton(EventType.MetaGameDeselect);
        buttonInteractionReset.SetButton(EventType.MetaGameReset);
        //miscellaneous
        Debug.Assert(page_header != null, "Invalid page_Header (Null)");
        //scrollRect & ScrollBar
        Debug.Assert(scrollBackground != null, "Invalid scrollBackground (Null)");
        Debug.Assert(scrollBarObject != null, "Invalid scrollBarObject (Null)");
        scrollRect = scrollBackground.GetComponent<ScrollRect>();
        scrollBar = scrollBarObject.GetComponent<Scrollbar>();
        Debug.Assert(scrollRect != null, "Invalid scrollRect (Null)");
        Debug.Assert(scrollBar != null, "Invalid scrollBar (Null)");
        //top
        Debug.Assert(renownBackground != null, "Invalid renownBackground (Null)");
        Debug.Assert(renownAmount != null, "Invalid renownAmount (Null)");
        //RHS
        Debug.Assert(rightImage != null, "Invalid rightImage (Null)");
        Debug.Assert(rightImageDefault != null, "Invalid rightImageDefault (Null)");
        Debug.Assert(rightTextTop != null, "Invalid rightTextTop (Null)");
        Debug.Assert(rightTextBottom != null, "Invalid rightTextBottom (Null)");
        //Help 
        helpCentre = buttonHelpCentre.GetComponent<GenericHelpTooltipUI>();
        helpCombined = buttonHelpCombined.GetComponent<GenericHelpTooltipUI>();
        Debug.Assert(helpCentre != null, "Invalid helpCentre (Null)");
        Debug.Assert(helpCombined != null, "Invalid helpCombined (Null)");
        //backgrounds
        Debug.Assert(backgroundMain != null, "Invalid backgroundMain (Null)");
        Debug.Assert(backgroundLeft != null, "Invalid backgroundLeft (Null)");
        Debug.Assert(backgroundCentre != null, "Invalid backgroundCentre (Null)");
        Debug.Assert(backgroundRight != null, "Invalid backgroundRight (Null)");
        //assign backgrounds and active tab colours
        Color colour = GameManager.instance.guiScript.colourMainBackground;
        backgroundCentre.color = new Color(colour.r, colour.g, colour.b);
        backgroundRight.color = new Color(colour.r, colour.g, colour.b);
        //items
        Debug.Assert(meta_item_0 != null, "Invalid item_0 (Null)");
        Debug.Assert(meta_item_1 != null, "Invalid item_1 (Null)");
        Debug.Assert(meta_item_2 != null, "Invalid item_2 (Null)");
        Debug.Assert(meta_item_3 != null, "Invalid item_3 (Null)");
        Debug.Assert(meta_item_4 != null, "Invalid item_4 (Null)");
        Debug.Assert(meta_item_5 != null, "Invalid item_5 (Null)");
        Debug.Assert(meta_item_6 != null, "Invalid item_6 (Null)");
        Debug.Assert(meta_item_7 != null, "Invalid item_7 (Null)");
        Debug.Assert(meta_item_8 != null, "Invalid item_8 (Null)");
        Debug.Assert(meta_item_9 != null, "Invalid item_9 (Null)");
        Debug.Assert(meta_item_10 != null, "Invalid item_10 (Null)");
        Debug.Assert(meta_item_11 != null, "Invalid item_11 (Null)");
        Debug.Assert(meta_item_12 != null, "Invalid item_12 (Null)");
        Debug.Assert(meta_item_13 != null, "Invalid item_13 (Null)");
        Debug.Assert(meta_item_14 != null, "Invalid item_14 (Null)");
        Debug.Assert(meta_item_15 != null, "Invalid item_15 (Null)");
        Debug.Assert(meta_item_16 != null, "Invalid item_16 (Null)");
        Debug.Assert(meta_item_17 != null, "Invalid item_17 (Null)");
        Debug.Assert(meta_item_18 != null, "Invalid item_18 (Null)");
        Debug.Assert(meta_item_19 != null, "Invalid item_19 (Null)");
        //assign items -> side array
        arrayOfSideMetaMain[0] = meta_item_0;
        arrayOfSideMetaMain[1] = meta_item_1;
        arrayOfSideMetaMain[2] = meta_item_2;
        arrayOfSideMetaMain[3] = meta_item_3;
        arrayOfSideMetaMain[4] = meta_item_4;
        arrayOfSideMetaMain[5] = meta_item_5;
        arrayOfSideMetaMain[6] = meta_item_6;
        arrayOfSideMetaMain[7] = meta_item_7;
        arrayOfSideMetaMain[8] = meta_item_8;
        arrayOfSideMetaMain[9] = meta_item_9;
        arrayOfSideMetaMain[10] = meta_item_10;
        arrayOfSideMetaMain[11] = meta_item_11;
        arrayOfSideMetaMain[12] = meta_item_12;
        arrayOfSideMetaMain[13] = meta_item_13;
        arrayOfSideMetaMain[14] = meta_item_14;
        arrayOfSideMetaMain[15] = meta_item_15;
        arrayOfSideMetaMain[16] = meta_item_16;
        arrayOfSideMetaMain[17] = meta_item_17;
        arrayOfSideMetaMain[18] = meta_item_18;
        arrayOfSideMetaMain[19] = meta_item_19;
        //assign items -> top array
        arrayOfTopMetaMain[0] = meta_item_0;
        arrayOfTopMetaMain[1] = meta_item_1;
        arrayOfTopMetaMain[2] = meta_item_2;
        arrayOfTopMetaMain[3] = meta_item_3;
        arrayOfTopMetaMain[4] = meta_item_4;
        arrayOfTopMetaMain[5] = meta_item_5;
        arrayOfTopMetaMain[6] = meta_item_6;
        arrayOfTopMetaMain[7] = meta_item_7;
        arrayOfTopMetaMain[8] = meta_item_8;
        arrayOfTopMetaMain[9] = meta_item_9;
        arrayOfTopMetaMain[10] = meta_item_10;
        arrayOfTopMetaMain[11] = meta_item_11;
        arrayOfTopMetaMain[12] = meta_item_12;
        arrayOfTopMetaMain[13] = meta_item_13;
        arrayOfTopMetaMain[14] = meta_item_14;
        arrayOfTopMetaMain[15] = meta_item_15;
        arrayOfTopMetaMain[16] = meta_item_16;
        arrayOfTopMetaMain[17] = meta_item_17;
        arrayOfTopMetaMain[18] = meta_item_18;
        arrayOfTopMetaMain[19] = meta_item_19;
        //initialise metaItems & populate arrays -> Side arrays
        for (index = 0; index < arrayOfSideMetaMain.Length; index++)
        {
            GameObject metaObject = arrayOfSideMetaMain[index];
            if (metaObject != null)
            {
                //get child components -> Image
                var childrenImage = metaObject.GetComponentsInChildren<Image>();
                foreach (var child in childrenImage)
                {
                    if (child.name.Equals("background", StringComparison.Ordinal) == true)
                    {
                        arrayOfSideMetaBackground[index] = child;
                        //attached interaction script
                        ItemInteractionUI itemScript = child.GetComponent<ItemInteractionUI>();
                        if (itemScript != null)
                        {
                            itemScript.SetItemIndex(index, numOfItemsTotal);
                            itemScript.SetUIType(MajorUI.MetaGameUI);
                        }
                        else { Debug.LogWarningFormat("Invalid ItemInteractionUI component (Null) for arrayOfSideMetaMain[{0}]", index); }
                    }
                    else if (child.name.Equals("icon", StringComparison.Ordinal) == true)
                    { arrayOfSideMetaIcon[index] = child; }
                    else if (child.name.Equals("border", StringComparison.Ordinal) == true)
                    { arrayOfSideMetaBorder[index] = child; }
                    else if (child.name.Equals("checkmark", StringComparison.Ordinal) == true)
                    { arrayOfSideMetaCheckMark[index] = child; }
                }
                //child components -> Text
                var childrenText = metaObject.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var child in childrenText)
                {
                    if (child.name.Equals("text", StringComparison.Ordinal) == true)
                    {
                        TextMeshProUGUI metaText = child.GetComponent<TextMeshProUGUI>();
                        if (metaText != null)
                        { arrayOfSideMetaText[index] = metaText; }
                        else { Debug.LogWarningFormat("Invalid TextMeshProUGUI component (Null) for arrayOfSideMetaMain[{0}]", index); }
                    }
                }
            }
            else { Debug.LogWarningFormat("Invalid GameObject (Null) for arrayOfSideMetaMain[{0}]", index); }
        }
        //initialise metaItems & populate arrays -> Top arrays
        for (index = 0; index < arrayOfTopMetaMain.Length; index++)
        {
            GameObject metaObject = arrayOfTopMetaMain[index];
            if (metaObject != null)
            {
                //get child components -> Image
                var childrenImage = metaObject.GetComponentsInChildren<Image>();
                foreach (var child in childrenImage)
                {
                    if (child.name.Equals("background", StringComparison.Ordinal) == true)
                    {
                        arrayOfTopMetaBackground[index] = child;
                        //attached interaction script
                        ItemInteractionUI itemScript = child.GetComponent<ItemInteractionUI>();
                        if (itemScript != null)
                        {
                            itemScript.SetItemIndex(index, numOfItemsTotal);
                            itemScript.SetUIType(MajorUI.MetaGameUI);
                        }
                        else { Debug.LogWarningFormat("Invalid ItemInteractionUI component (Null) for arrayOfTopMetaMain[{0}]", index); }
                    }
                    else if (child.name.Equals("icon", StringComparison.Ordinal) == true)
                    { arrayOfTopMetaIcon[index] = child; }
                    else if (child.name.Equals("border", StringComparison.Ordinal) == true)
                    { arrayOfTopMetaBorder[index] = child; }
                    else if (child.name.Equals("checkmark", StringComparison.Ordinal) == true)
                    { arrayOfTopMetaCheckMark[index] = child; }
                }
                //child components -> Text
                var childrenText = metaObject.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var child in childrenText)
                {
                    if (child.name.Equals("text", StringComparison.Ordinal) == true)
                    {
                        TextMeshProUGUI metaText = child.GetComponent<TextMeshProUGUI>();
                        if (metaText != null)
                        { arrayOfTopMetaText[index] = metaText; }
                        else { Debug.LogWarningFormat("Invalid TextMeshProUGUI component (Null) for arrayOfTopMetaMain[{0}]", index); }
                    }
                }
            }
            else { Debug.LogWarningFormat("Invalid GameObject (Null) for arrayOfTopMetaMain[{0}]", index); }
        }
        //Set starting Initialisation states
        InitialiseItems();
    }
    #endregion

    #region SubInitialiseFastAccess
    /// <summary>
    /// fast access
    /// </summary>
    private void SubInitialiseFastAccess()
    {
        //priority icons
        priorityHigh = GameManager.instance.guiScript.priorityHighSprite;
        priorityMedium = GameManager.instance.guiScript.priorityMediumSprite;
        priorityLow = GameManager.instance.guiScript.priorityLowSprite;
        priorityInactive = GameManager.instance.guiScript.priorityInactiveSprite;
        Debug.Assert(priorityHigh != null, "Invalid priorityHigh (Null)");
        Debug.Assert(priorityMedium != null, "Invalid priorityMedium (Null)");
        Debug.Assert(priorityLow != null, "Invalid priorityLow (Null)");
        Debug.Assert(priorityInactive != null, "Invalid priorityInactive (Null)");
        //cost levels for metaOptions
        costLow = GameManager.instance.metaScript.costLowPriority;
        costMedium = GameManager.instance.metaScript.costMediumPriority;
        costHigh = GameManager.instance.metaScript.costHighPriority;
        costExtreme = GameManager.instance.metaScript.costExtremePriority;
        Debug.Assert(costLow > -1, "Invalid costLow (-1)");
        Debug.Assert(costMedium > -1, "Invalid costMedium (-1)");
        Debug.Assert(costHigh > -1, "Invalid costHigh (-1)");
        Debug.Assert(costExtreme > -1, "Invalid costExtreme (-1)");
    }
    #endregion

    #region SubInitialiseEvents
    /// <summary>
    /// events
    /// </summary>
    private void SubInitialiseEvents()
    {
        //listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGameClose, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGameSideTabOpen, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGameTopTabOpen, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGameShowDetails, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGameUpArrow, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGameDownArrow, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGamePageUp, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGamePageDown, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGameSelect, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGameDeselect, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGameButton, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGameReset, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGameConfirm, OnEvent, "MetaGamesUI");
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
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.MetaGameClose:
                CloseMetaUI();
                break;
            case EventType.MetaGameSideTabOpen:
                OpenSideTab((int)Param);
                break;
            case EventType.MetaGameTopTabOpen:
                OpenTopTab((int)Param);
                break;
            case EventType.MetaGameOpen:
                MetaInfoData data = Param as MetaInfoData;
                SetMetaUI(data);
                break;
            case EventType.MetaGameShowDetails:
                ShowItemDetails((int)Param);
                break;
            case EventType.MetaGameSelect:
                ExecuteSelect();
                break;
            case EventType.MetaGameDeselect:
                ExecuteDeselect();
                break;
            case EventType.MetaGameButton:
                ExecuteButton();
                break;
            case EventType.MetaGameReset:
                ExecuteReset();
                break;
            case EventType.MetaGameConfirm:
                ExecuteConfirm();
                break;
            /*case EventType.MainInfoHome:
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
                break;*/
            case EventType.MetaGameUpArrow:
                ExecuteUpArrow();
                break;
            case EventType.MetaGameDownArrow:
                ExecuteDownArrow();
                break;
            case EventType.MetaGamePageUp:
                ExecutePageUp();
                break;
            case EventType.MetaGamePageDown:
                ExecutePageDown();
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
        /*colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourBlue = GameManager.instance.colourScript.GetColour(ColourType.blueText)*/
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        /*colourBad = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourError = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.salmonText);*/
        colourCancel = GameManager.instance.colourScript.GetColour(ColourType.moccasinText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// run prior to every metaGameUI use. Run from SetMetaUI
    /// </summary>
    private void InitialiseMetaUI(MetaInfoData data)
    {
        int count;
        //player renown
        int playerRenown = GameManager.instance.playerScript.Renown;
        renownAmount.text = playerRenown.ToString();
        renownCurrent = playerRenown;
        Debug.LogFormat("[Met] MetaGameUI.cs -> InitialiseMetaUI: Player has {0} Renown{1}", playerRenown, "\n");
        Color portraitColor, backgroundColor;
        //initialise HQ side tabs'
        for (int index = 0; index < numOfSideTabs; index++)
        {
            backgroundColor = arrayOfSideTabItems[index].background.color;
            if (arrayOfSideTabItems[index] != null)
            {
                //sprite
                Actor actor = GameManager.instance.dataScript.GetHQHierarchyActor((ActorHQ)(index + offset));
                if (actor != null)
                {
                    arrayOfSideTabItems[index].portrait.sprite = actor.sprite;
                }
                else
                {
                    //default error sprite if a problem
                    Debug.LogWarningFormat("Invalid actor (Null) for ActorHQ \"{0}\"", (ActorHQ)(index + offset));
                    arrayOfSideTabItems[index].portrait.sprite = GameManager.instance.guiScript.errorSprite;
                }
                portraitColor = arrayOfSideTabItems[index].portrait.color;
                //title
                arrayOfSideTabItems[index].title.text = GameManager.instance.hqScript.GetHqTitle(actor.statusHQ);
                //first tab (Boss) should be active on opening, rest passive
                if (index == 0)
                { portraitColor.a = 1.0f; backgroundColor.a = 1.0f; }
                else
                { portraitColor.a = 0.25f; backgroundColor.a = 0.25f; }
                //set colors
                arrayOfSideTabItems[index].portrait.color = portraitColor;
                arrayOfSideTabItems[index].background.color = backgroundColor;
                //set count of active items in each tab
                count = 0;
                for (int j = 0; j < data.arrayOfMetaData[index].Count; j++)
                {
                    if (data.arrayOfMetaData[index][j].isActive == true)
                    { count++; }
                }
                arrayOfSideTabOptions[index] = count;
            }
            else { Debug.LogErrorFormat("Invalid tabItems[{0}] (Null)", index); }
        }
        //initialise top tabs -> selected tab (inactive as nothing has yet been selected)
        backgroundColor = tabSelected.color;
        backgroundColor.a = 0.25f;
        tabSelected.color = backgroundColor;
        backgroundColor = textSelected.color;
        backgroundColor.a = 0.25f;
        textSelected.color = backgroundColor;
    }

    /// <summary>
    /// Tab initialised at start and doesn't ever change. Status data (metaOption.isPlayerStatus true) either present or not
    /// </summary>
    private void InitialiseTopStatusTab()
    {
        Color color;
        if (isStatusData == true)
        {
            //data present
            color = tabSelected.color;
            color.a = 1.0f;
            tabSelected.color = color;
            color = textSelected.color;
            color.a = 1.0f;
            textSelected.color = color;
        }
        else
        {
            //no data
            color = tabSelected.color;
            color.a = 0.25f;
            tabSelected.color = color;
            color = textSelected.color;
            color.a = 0.25f;
            textSelected.color = color;
        }
    }


    private void InitialiseItems()
    {
        for (int index = 0; index < numOfItemsTotal; index++)
        {
            //main game objects off -> Side
            arrayOfSideMetaMain[index].SetActive(false);
            //all other child objects on -> Side
            arrayOfSideMetaIcon[index].gameObject.SetActive(true);
            arrayOfSideMetaText[index].gameObject.SetActive(true);
            arrayOfSideMetaBorder[index].gameObject.SetActive(true);
            arrayOfSideMetaCheckMark[index].gameObject.SetActive(false);
            arrayOfSideMetaBackground[index].gameObject.SetActive(true);
            //main game objects off -> top
            arrayOfTopMetaMain[index].SetActive(false);
            //all other child objects on -> top
            arrayOfTopMetaIcon[index].gameObject.SetActive(true);
            arrayOfTopMetaText[index].gameObject.SetActive(true);
            arrayOfTopMetaBorder[index].gameObject.SetActive(true);
            arrayOfTopMetaCheckMark[index].gameObject.SetActive(false);
            arrayOfTopMetaBackground[index].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Display MetaGame Player options UI
    /// </summary>
    public void SetMetaUI(MetaInfoData data)
    {
        if (data != null)
        {
            InitialiseMetaUI(data);
            canvasMeta.gameObject.SetActive(true);
            // Populate data
            UpdateData(data);
            //initialise top status tab (active if data present) -> once off
            InitialiseTopStatusTab();
            // Display Boss page by default
            OpenSideTab(0);
            //select buttons, RHS, off by default
            buttonSelect.gameObject.SetActive(false);
            buttonDeselect.gameObject.SetActive(false);
            helpCentre.gameObject.SetActive(false);
            helpCombined.gameObject.SetActive(false);
            //set game state
            isRunning = true;
            GameManager.instance.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.MetaGame, metaState = ModalMetaSubState.PlayerOptions });
            Debug.LogFormat("[UI] MetaGameUI.cs -> SetMetaUI{0}", "\n");
        }
        else { Debug.LogWarning("Invalid MetaInfoData (Null)"); }
    }


    /// <summary>
    /// close Meta UI display
    /// </summary>
    private void CloseMetaUI()
    {
        GameManager.instance.tooltipGenericScript.CloseTooltip("MainInfoUI.cs -> CloseMainInfo");
        GameManager.instance.tooltipHelpScript.CloseTooltip("MainInfoUI.cs -> CloseMainInfo");
        canvasMeta.gameObject.SetActive(false);
        //set game state
        isRunning = false;
        GameManager.instance.guiScript.SetIsBlocked(false);
        GameManager.instance.inputScript.ResetStates();
        Debug.LogFormat("[UI] MainInfoUI.cs -> CloseMainInfo{0}", "\n");
        //update player renown
        GameManager.instance.playerScript.Renown = renownCurrent;
        Debug.LogFormat("[Met] MetaGameUI.cs -> CloseMetaUI: Player carries over {0} Renown{1}", renownCurrent, "\n");
        //show top bar UI at completion of meta game
        EventManager.instance.PostNotification(EventType.TopBarShow, this, null, "MetaGameUI.cs -> Show TopBarUI");
    }


    /// <summary>
    /// Open the designated Side tab and close whatever is open
    /// </summary>
    /// <param name="tabIndex"></param>
    private void OpenSideTab(int tabIndex)
    {
        Debug.Assert(tabIndex > -1 && tabIndex < numOfSideTabs, string.Format("Invalid tab index {0}", tabIndex));
        //reset Active tabs to reflect new status
        Color portraitColor, backgroundColor;
        for (int index = 0; index < numOfSideTabs; index++)
        {
            portraitColor = arrayOfSideTabItems[index].portrait.color;
            backgroundColor = arrayOfSideTabItems[index].background.color;
            //activate indicated tab and deactivate the rest
            if (index == tabIndex)
            {
                portraitColor.a = 1.0f;
                backgroundColor.a = 1.0f;
            }
            else
            {
                portraitColor.a = 0.25f;
                backgroundColor.a = 0.25f;
            }
            arrayOfSideTabItems[index].portrait.color = portraitColor;
            arrayOfSideTabItems[index].background.color = backgroundColor;
        }
        /*//reset scrollbar
        scrollRect.verticalNormalizedPosition = 1.0f;
        //redrawn main page
        DisplayItemPage(tabIndex);*/

        //assign default RHS values
        string leader = arrayOfSideTabItems[tabIndex].title.text.ToUpper();
        rightImage.sprite = rightImageDefault;
        rightTextTop.text = $"{leader} Options";
        rightTextBottom.text = string.Format("Shows any HQ options on offer from your<br><br><b>{0}{1}{2}</b>", colourCancel, leader, colourEnd);
        //redrawn main page
        DisplayItemPage(tabIndex);
        //update indexes
        highlightIndex = -1;
        currentSideTabIndex = tabIndex;
    }

    /// <summary>
    /// Open the designated Top tab and close whatever is open
    /// </summary>
    /// <param name="tabIndex"></param>
    private void OpenTopTab(int tabIndex)
    {

    }


    /// <summary>
    /// sub Method to display a particular page drawing from cached data in dictOfItemData
    /// </summary>
    /// <param name="tab"></param>
    private void DisplayItemPage(int tabIndex)
    {
        Debug.Assert(tabIndex > -1 && tabIndex < (int)ItemTab.Count, string.Format("Invalid tabIndex {0}", tabIndex));
        //page header
        page_header.text = string.Format("{0} Option{1} available", arrayOfSideTabOptions[tabIndex], arrayOfSideTabOptions[tabIndex] != 1 ? "s" : "");
        //clear out current data
        listOfCurrentPageSideMetaData.Clear();
        //get data
        listOfCurrentPageSideMetaData.AddRange(arrayOfSideMetaData[tabIndex]);
        //display routine
        if (listOfCurrentPageSideMetaData != null)
        {
            numOfItemsCurrent = listOfCurrentPageSideMetaData.Count;
            maxHighlightIndex = numOfItemsCurrent - 1;
            if (numOfItemsCurrent > 0)
            {
                //populate current messages for the main tab
                for (int index = 0; index < arrayOfSideMetaText.Length; index++)
                {
                    if (index < numOfItemsCurrent)
                    {
                        //populate text and set item to active
                        arrayOfSideMetaText[index].text = listOfCurrentPageSideMetaData[index].itemText;
                        arrayOfSideMetaMain[index].gameObject.SetActive(true);
                        //assign icon
                        if (listOfCurrentPageSideMetaData[index].isActive == true)
                        {
                            switch (listOfCurrentPageSideMetaData[index].priority)
                            {
                                case MetaPriority.Extreme:
                                case MetaPriority.High:
                                    arrayOfSideMetaIcon[index].sprite = priorityHigh;
                                    break;
                                case MetaPriority.Medium:
                                    arrayOfSideMetaIcon[index].sprite = priorityMedium;
                                    break;
                                case MetaPriority.Low:
                                    arrayOfSideMetaIcon[index].sprite = priorityLow;
                                    break;
                                default:
                                    Debug.LogWarningFormat("Invalid priority \"{0}\"", listOfCurrentPageSideMetaData[index].priority);
                                    break;
                            }
                        }
                        else { arrayOfSideMetaIcon[index].sprite = priorityInactive; }

                    }
                    else if (index < numOfItemsPrevious)
                    {
                        //efficient -> only disables items that were previously active, not the whole set
                        arrayOfSideMetaText[index].text = "";
                        arrayOfSideMetaMain[index].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //no data, blank previous items (not necessarily all), disable line
                for (int index = 0; index < numOfItemsPrevious; index++)
                {
                    arrayOfSideMetaText[index].text = "";
                    arrayOfSideMetaMain[index].gameObject.SetActive(false);
                }

            }
            //update previous count to current
            numOfItemsPrevious = numOfItemsCurrent;
        }
        else { Debug.LogWarning("Invalid MetaInfoData.listOfMainText (Null)"); }
        //manually activate / deactivate scrollBar as needed (because you've got deactivated objects in the scroll list the bar shows regardless unless you override here)
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
    /// Updates cached data (side tabs)
    /// NOTE: data checked for null by the calling procedure
    /// </summary>
    /// <param name="data"></param>
    private void UpdateData(MetaInfoData data)
    {
        if (data.arrayOfMetaData != null)
        {
            //populate all side tab data
            for (int i = 0; i < (int)MetaTabSide.Count; i++)
            {
                arrayOfSideMetaData[i].Clear();
                arrayOfSideMetaData[i].AddRange(data.arrayOfMetaData[i]);
            }
        }
        else { Debug.LogError("Invalid data.arrayOfMetaData (Null)"); }
        if (data.listOfStatusData != null)
        {
            //populate status top tab data
            for (int i = 0; i < data.listOfStatusData.Count; i++)
            {
                MetaData metaData = data.listOfStatusData[i];
                if (metaData != null)
                { arrayOfTopMetaData[(int)MetaTabTop.Status].Add(metaData); }
                else { Debug.LogWarningFormat("Invalid metaData (Null) for listOfStatusData[i]"); }
            }
            //set flag to indicate status data present -> it's a constant from time of initialisation (items may be selected, or not, but they will either be there at the start or they won't
            if (arrayOfTopMetaData[(int)MetaTabTop.Status].Count > 0)
            { isStatusData = true; }
            else { isStatusData = false; }
        }
        else { Debug.LogError("Invalid data.listOfStatusData (Null)"); }
    }


    /// <summary>
    /// MetaData details
    /// </summary>
    /// <param name="itemIndex"></param>
    private void ShowItemDetails(int itemIndex)
    {
        MetaData data = listOfCurrentPageSideMetaData[itemIndex];
        if (data != null)
        {
            //Right Hand side text varies depending on 'isActive' status
            if (data.isActive == true)
            {
                //header text varies if already selected or not
                if (data.isSelected == false)
                { rightTextTop.text = data.textSelect; }
                else { rightTextTop.text = data.textDeselect; }
                //bottom text same regardless
                rightTextBottom.text = data.bottomText;
            }
            else
            {
                rightTextTop.text = string.Format("{0}{1}{2}", colourGrey, "Option Unavailable", colourEnd);
                if (data.isCriteria == true)
                { rightTextBottom.text = data.inactiveText; }
                else
                { rightTextBottom.text = data.bottomText; }
            }
            //sprite
            if (data.sprite != null)
            {
                rightImage.sprite = data.sprite;
                rightImage.gameObject.SetActive(true);
            }
            //if no sprite, switch off default info sprite and leave blak
            else { rightImage.gameObject.SetActive(false); }

            //display Select button if active
            if (data.isActive == true)
            {
                //hide help and make button active (if hasn't already been selected)
                if (data.isSelected == false)
                {
                    //hasn't been selected
                    buttonDeselect.gameObject.SetActive(false);
                    //enough renown for option
                    if (data.renownCost <= renownCurrent)
                    { buttonSelect.gameObject.SetActive(true); }
                    else
                    {
                        //insufficient renown
                        buttonSelect.gameObject.SetActive(false);
                        rightTextTop.text = data.textInsufficient;
                    }
                }
                else
                {
                    //already Selected
                    buttonSelect.gameObject.SetActive(false);
                    buttonDeselect.gameObject.SetActive(true);
                }


                if (data.help > -1)
                {

                    List<HelpData> listOfHelp = GetItemHelpList(data);
                    if (listOfHelp != null)
                    { helpCombined.SetHelpTooltip(listOfHelp, -400, 0); }
                    else { Debug.LogWarning("Invalid listOfHelp (Null)"); }
                    if (data.renownCost <= renownCurrent)
                    {
                        //enough renown
                        buttonHelpCentre.gameObject.SetActive(false);
                        buttonHelpCombined.gameObject.SetActive(true);
                    }
                    else
                    {
                        //insufficient renown
                        buttonHelpCombined.gameObject.SetActive(false);
                        buttonHelpCentre.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                //inactive item, hide Select item and display help
                buttonSelect.gameObject.SetActive(false);
                buttonDeselect.gameObject.SetActive(false);
                buttonHelpCombined.gameObject.SetActive(false);
                //display help only if available
                if (data.help > -1)
                {
                    buttonHelpCentre.gameObject.SetActive(true);
                    List<HelpData> listOfHelp = GetItemHelpList(data);
                    if (listOfHelp != null)
                    { helpCentre.SetHelpTooltip(listOfHelp, -400, 0); }
                    else { Debug.LogWarning("Invalid listOfHelp (Null)"); }
                }
                else { buttonHelpCentre.gameObject.SetActive(false); }
            }

            //remove highlight
            if (highlightIndex != itemIndex)
            {
                //reset currently highlighted back to default
                if (highlightIndex > -1)
                { arrayOfSideMetaText[highlightIndex].text = string.Format("{0}{1}{2}", colourDefault, listOfCurrentPageSideMetaData[highlightIndex].itemText, colourEnd); }
                highlightIndex = itemIndex;
                //highlight item -> yelllow if active, grey if not
                if (data.isActive == true)
                { arrayOfSideMetaText[itemIndex].text = string.Format("{0}<b>{1}</b>{2}", colourNeutral, listOfCurrentPageSideMetaData[itemIndex].itemText, colourEnd); }
                else { arrayOfSideMetaText[itemIndex].text = string.Format("{0}<size=115%><b>{1}</size></b>{2}", colourDefault, listOfCurrentPageSideMetaData[itemIndex].itemText, colourEnd); }
            }
        }
        else
        {
            Debug.LogWarningFormat("Invalid MetaData for listOfCurrentPageMetaData[{0}]", itemIndex);
        }
    }

    /// <summary>
    /// Assembles a list of HelpData for the item to pass onto the help components. Returns empty if none.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private List<HelpData> GetItemHelpList(MetaData data)
    {
        string tag0, tag1, tag2, tag3;
        tag0 = tag1 = tag2 = tag3 = "";
        //Debug
        if (string.IsNullOrEmpty(data.tag0) == true)
        {
            //default data
            tag0 = "test0";
        }
        else
        {
            //item specified help
            tag0 = data.tag0;
            tag1 = data.tag1;
            tag2 = data.tag2;
            tag3 = data.tag3;
        }
        return GameManager.instance.helpScript.GetHelpData(tag0, tag1, tag2, tag3);
    }

    /// <summary>
    /// Down arrow (down next item on page)
    /// </summary>
    private void ExecuteDownArrow()
    {
        if (highlightIndex > -1)
        {
            if (highlightIndex < maxHighlightIndex)
            {
                ShowItemDetails(highlightIndex + 1);

                //if outside scroll view move scrollRect down one item
                if (highlightIndex >= numOfVisibleItems)
                {
                    float scrollPos = 1.0f - (float)highlightIndex / maxHighlightIndex;
                    scrollRect.verticalNormalizedPosition = scrollPos;
                }
            }
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
            {
                ShowItemDetails(highlightIndex - 1);
                //adjust scrolling
                if (scrollRect.verticalNormalizedPosition != 1)
                {
                    float scrollPos = 1.0f - (float)highlightIndex / maxHighlightIndex;
                    scrollRect.verticalNormalizedPosition = scrollPos;
                }
            }
            else
            {
                //at top of page, go to tab
                highlightIndex = -1;
                OpenSideTab(currentSideTabIndex);
            }
        }
    }

    /// <summary>
    /// Page UP (next tab up)
    /// </summary>
    private void ExecutePageUp()
    {
        //change tab
        if (currentSideTabIndex > -1)
        {
            if (currentSideTabIndex > 0)
            {
                currentSideTabIndex -= 1;
                OpenSideTab(currentSideTabIndex);
            }
            else
            {
                //roll over
                currentSideTabIndex = maxSideTabIndex;
                OpenSideTab(currentSideTabIndex);
            }
        }
    }

    /// <summary>
    /// Page DOWN (next tab down)
    /// </summary>
    private void ExecutePageDown()
    {
        if (currentSideTabIndex > -1)
        {
            if (currentSideTabIndex < maxSideTabIndex)
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
    /// Select button pressed, RHS
    /// </summary>
    private void ExecuteSelect()
    {
        MetaData metaData = listOfCurrentPageSideMetaData[highlightIndex];
        if (metaData != null)
        {
            MetaOption metaOption = GameManager.instance.dataScript.GetMetaOption(metaData.metaName);
            if (metaOption != null)
            {
                //add to dictOfSelected
                try
                { dictOfSelected.Add(metaData.metaName, metaData); }
                catch (ArgumentException)
                { Debug.LogErrorFormat("Invalid metaName (duplicate) in dictOfSelected for \"{0}\"", metaData.metaName); }
                //adjust renown display
                renownCurrent -= metaData.renownCost;
                renownCurrent = Mathf.Max(0, renownCurrent);
                renownAmount.text = renownCurrent.ToString();
                //set as selected
                metaData.isSelected = true;
                //switch buttons
                buttonSelect.gameObject.SetActive(false);
                buttonDeselect.gameObject.SetActive(true);
                //checkmark
                arrayOfSideMetaCheckMark[highlightIndex].gameObject.SetActive(true);
                //top tab
                if (dictOfSelected.Count == 1)
                {
                    //make top selectedTab active
                    Color backgroundColor = tabSelected.color;
                    backgroundColor.a = 1.0f;
                    tabSelected.color = backgroundColor;
                    backgroundColor = textSelected.color;
                    backgroundColor.a = 1.0f;
                    textSelected.color = backgroundColor;


                }
                //switch top texts
                rightTextTop.text = metaData.textDeselect;
                Debug.LogFormat("[Met] MetaGameUI.cs -> ExecuteSelect: metaOption \"{0}\" Selected at a cost of {1} Renown ({2} remaining){3}", metaData.metaName, metaData.renownCost, renownCurrent, "\n");
            }
            else { Debug.LogWarningFormat("Invalid metaOption (Null) for metaData.metaName \"{0}\"", metaData.metaName); }
        }
        else { Debug.LogWarningFormat("Invalid metaData (Null) for listOfCurrentPageMetaData[{0}]", highlightIndex); }
    }

    /// <summary>
    /// Deselect button pressed, RHS
    /// </summary>
    private void ExecuteDeselect()
    {
        MetaData metaData = listOfCurrentPageSideMetaData[highlightIndex];
        if (metaData != null)
        {
            MetaOption metaOption = GameManager.instance.dataScript.GetMetaOption(metaData.metaName);
            if (metaOption != null)
            {
                //remove from dictOfSelected
                if (dictOfSelected.ContainsKey(metaData.metaName) == true)
                { dictOfSelected.Remove(metaData.metaName); }
                else { Debug.LogWarningFormat("metaData \"{0}\" not found in dictOfSelected (Not removed)", metaData.metaName); }
                //adjust renown display
                renownCurrent += metaData.renownCost;
                renownAmount.text = renownCurrent.ToString();
                //set as deSelected
                metaData.isSelected = false;
                //switch buttons
                buttonDeselect.gameObject.SetActive(false);
                //checkmark
                arrayOfSideMetaCheckMark[highlightIndex].gameObject.SetActive(false);
                //top tab
                if (dictOfSelected.Count == 0)
                {
                    //make top selectedTab inactive
                    Color backgroundColor = tabSelected.color;
                    backgroundColor.a = 0.25f;
                    tabSelected.color = backgroundColor;
                    backgroundColor = textSelected.color;
                    backgroundColor.a = 0.25f;
                    textSelected.color = backgroundColor;

                }
                //switch top texts
                rightTextTop.text = metaData.textSelect;
                buttonSelect.gameObject.SetActive(true);
                Debug.LogFormat("[Met] MetaGameUI.cs -> ExecuteDeselect: metaOption \"{0}\" Deselected for +{1} Renown (now {2}){3}", metaData.metaName, metaData.renownCost, renownCurrent, "\n");
            }
            else { Debug.LogWarningFormat("Invalid metaOption (Null) for metaData.metaName \"{0}\"", metaData.metaName); }
        }
        else { Debug.LogWarningFormat("Invalid metaData (Null) for listOfCurrentPageMetaData[{0}]", highlightIndex); }
    }

    /// <summary>
    /// Generic keyboard shortcut for Select/Deselect button press (it is whatever button is active at the time). Does nothing if neither button valid
    /// </summary>
    private void ExecuteButton()
    {
        if (buttonSelect.gameObject.activeSelf == true)
        { ExecuteSelect(); }
        else if (buttonDeselect.gameObject.activeSelf == true)
        { ExecuteDeselect(); }
    }

    /// <summary>
    /// Deselects all metaData options to give a clean slate
    /// </summary>
    private void ExecuteReset()
    {
        if (dictOfSelected.Count > 0)
        {
            //set all selected metaData to isSelected false
            foreach (var data in dictOfSelected)
            {
                if (data.Value != null)
                {
                    data.Value.isSelected = false;
                    //update renown
                    renownCurrent += data.Value.renownCost;
                    Debug.LogFormat("[Met] MetaGameUI.cs -> ExecuteReset: metaData \"{0}\" Deselected (Reset button pressed){1}", data.Key, "\n");
                }
                else { Debug.LogWarningFormat("Invalid metaData (Null) for \"{0}\" in dictOfSelected", data.Key); }
            }
            renownAmount.text = renownCurrent.ToString();
            //empty dict
            dictOfSelected.Clear();

            //update current item
            if (highlightIndex > -1)
            {
                MetaData data = listOfCurrentPageSideMetaData[highlightIndex];
                if (data != null)
                {

                    if (buttonDeselect.gameObject.activeSelf == true)
                    {
                        //deselected option
                        rightTextTop.text = data.textSelect;
                        buttonDeselect.gameObject.SetActive(false);
                        buttonSelect.gameObject.SetActive(true);
                    }
                    else if (buttonSelect.gameObject.activeSelf == false)
                    {
                        if (renownCurrent >= data.renownCost)
                        {
                            //unaffordable option
                            rightTextTop.text = data.textInsufficient;
                            buttonHelpCentre.gameObject.SetActive(false);
                            buttonHelpCombined.gameObject.SetActive(true);
                            buttonSelect.gameObject.SetActive(true);
                        }
                    }
                }
            }
            //confirmation outcome popup
            ModalOutcomeDetails details = new ModalOutcomeDetails()
            {
                side = GameManager.instance.sideScript.PlayerSide,
                textTop = string.Format("{0}RESET{1}", colourNeutral, colourEnd),
                textBottom = "All available options are now ready for selection again",
                sprite = GameManager.instance.guiScript.infoSprite,
                modalLevel = 2,
                modalState = ModalSubState.MetaGame,
                reason = "Reset pressed"
            };
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
        }
    }

    /// <summary>
    /// Confirm button pressed
    /// </summary>
    public void ExecuteConfirm()
    {
        if (dictOfSelected.Count > 0)
        {
            int count;
            //data packages
            EffectDataReturn effectReturn = new EffectDataReturn();
            EffectDataInput effectInput = new EffectDataInput();
            StringBuilder builder = new StringBuilder();
            //
            // - - - Process Effects
            //
            //use Player node (still viable as new level not yet created)
            Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.GetPlayerNodeID());
            if (node != null)
            {
                //loop dict and add all to effects
                foreach (var data in dictOfSelected)
                {
                    if (data.Value != null)
                    {
                        count = data.Value.listOfEffects.Count;
                        if (count > 0)
                        {
                            effectInput.originText = data.Key;
                            effectInput.dataName = data.Value.dataName;
                            for (int i = 0; i < count; i++)
                            {
                                Effect effect = data.Value.listOfEffects[i];
                                if (effect != null)
                                {
                                    effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, effectInput);
                                    if (builder.Length > 0) { builder.AppendLine(); builder.AppendLine(); }
                                    if (string.IsNullOrEmpty(effectReturn.topText) == false)
                                    { builder.AppendFormat("{0}{1}{2}", effectReturn.topText, "\n", effectReturn.bottomText); }
                                    else { builder.Append(effectReturn.bottomText); }
                                }
                                else { Debug.LogWarningFormat("metaData \"{0}\" effect invalid (Null) for listOfEffects[{1}]", data.Key, i); }
                            }
                        }
                        else { Debug.LogWarningFormat("metaData \"{0}\" has no Effects (listOfEffects empty)", data.Key); }
                    }
                    else { Debug.LogWarningFormat("Invalid metaData (Null) in dictOfSelected for \"{0}\"", data.Key); }
                }
            }
            else { Debug.LogError("Invalid playerNode (Null), metaData effects not process"); }
            //
            // - - - Effects Outcome
            //
            ModalStateData modalData = new ModalStateData()
            {
                mainState = ModalSubState.MetaGame,
                metaState = ModalMetaSubState.OptionsConfirm
            };
            GameManager.instance.inputScript.SetModalState(modalData);
            //check options have been selected
            if (builder.Length > 0)
            {
                //add renown note
                builder.AppendFormat("{0}{1}{2}{3}{4} Renown will carry over", "\n", "\n", colourNeutral, renownCurrent, colourEnd);
                //confirmation outcome popup
                ModalOutcomeDetails details = new ModalOutcomeDetails()
                {
                    side = GameManager.instance.sideScript.PlayerSide,
                    textTop = string.Format("{0}HQ Outcomes{1}", colourCancel, colourEnd),
                    textBottom = builder.ToString(),
                    sprite = GameManager.instance.guiScript.infoSprite,
                    modalLevel = 2,
                    modalState = ModalSubState.MetaGame,
                    reason = "Effect Outcomes"
                };
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
                //need a coroutine to handle execution to prevent metaGameUI closing prematurely
                StartCoroutine(CloseMetaGameOutcome(details));
            }
        }
        else
        {
            //nothing selected -> Confirm continue?
            ModalConfirmDetails details = new ModalConfirmDetails()
            {
                topText = string.Format("You haven't selected any options<br>{0}<b>{1}</b>{2} Renown will carry over", colourNeutral, renownCurrent, colourEnd),
                buttonFalse = "BACK",
                buttonTrue = "CONTINUE",
                modalLevel = 2,
                modalState = ModalSubState.MetaGame
            };
            EventManager.instance.PostNotification(EventType.OpenConfirmWindow, this, details);
            //need a coroutine to handle execution to prevent metaGameUI closing prematurely
            StartCoroutine(CloseMetaGameConfirm(details));
        }
    }

    /// <summary>
    /// Master coroutine for outcome window
    /// </summary>
    /// <param name="details"></param>
    /// <returns></returns>
    IEnumerator CloseMetaGameOutcome(ModalOutcomeDetails details)
    {
        GameManager.instance.guiScript.waitUntilDone = true;
        //wait for outcome window to be closed
        yield return CloseOutcome(details);
        //close metaGameUI
        CloseMetaUI();
    }

    /// <summary>
    /// handles outcome window showing result of effects of selected metaOptions
    /// </summary>
    /// <param name="details"></param>
    /// <returns></returns>
    IEnumerator CloseOutcome(ModalOutcomeDetails details)
    {
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
        //will wait until ModalOutcome -> CloseModalOutcome resets flag
        yield return new WaitUntil(() => GameManager.instance.guiScript.waitUntilDone == false);
    }

    /// <summary>
    /// Master coroutine for confirm dialogue
    /// </summary>
    /// <param name="details"></param>
    /// <returns></returns>
    IEnumerator CloseMetaGameConfirm(ModalConfirmDetails details)
    {
        GameManager.instance.guiScript.waitUntilDone = true;
        //wait for confirm window to be closed
        yield return CloseConfirm(details);
        //check result -> close and continue or close and revert back to MetaGameUI
        if (GameManager.instance.confirmScript.CheckResult() == true)
        {
            //True button pressed - > close metaGameUI
            CloseMetaUI();
            Debug.LogFormat("[Met] MetaGameUI.cs -> CloseMetaGameConfirm: Nooptions selected{0}", "\n");
        }
    }

    /// <summary>
    /// handles confirm window asking are you sure you want to continue (didn't select any options)
    /// </summary>
    /// <param name="details"></param>
    /// <returns></returns>
    IEnumerator CloseConfirm(ModalConfirmDetails details)
    {
        EventManager.instance.PostNotification(EventType.OpenConfirmWindow, this, details);
        //will wait until ModalConfirm -> True or False button pressed which resets flag
        yield return new WaitUntil(() => GameManager.instance.guiScript.waitUntilDone == false);
    }


    /// <summary>
    /// Debug display of dictOfSelected metaData
    /// </summary>
    /// <returns></returns>
    public string DebugDisplaySelected()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- MetaGameUI.cs -> dictOfSelected{0}{1}", "\n", "\n");
        if (GameManager.instance.inputScript.GameState == GameState.MetaGame)
        {
            if (GameManager.instance.inputScript.ModalMetaState == ModalMetaSubState.PlayerOptions)
            {
                if (dictOfSelected.Count > 0)
                {
                    foreach (var data in dictOfSelected)
                    {
                        if (data.Value != null)
                        {
                            builder.AppendFormat(" {0}, cost {1} R, dataN {2}, dataT {3}{4}", data.Key, data.Value.renownCost,
                                string.IsNullOrEmpty(data.Key) ? "n.a" : data.Value.dataName, string.IsNullOrEmpty(data.Value.dataTag) ? "n.a" : data.Value.dataTag, "\n");
                        }
                        else { Debug.LogWarningFormat("Invalid metaData in dictOfSelected[{0}]", data.Key); }
                    }
                }
                else { builder.AppendFormat(" Nothing selected"); }
            }
            else { builder.AppendFormat(" Player options UI isn't active"); }
        }
        else { builder.AppendFormat(" Only works during MetaGame Player Options UI"); }
        return builder.ToString();
    }


    //new methods above here
}
