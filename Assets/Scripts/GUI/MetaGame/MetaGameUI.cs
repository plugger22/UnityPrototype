using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [Tooltip("Main UI help at bottom right")]
    public Button buttonHelpMain;

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
    public TextMeshProUGUI renownAmount;        //use UpdateRenown() rather than do so directly for enhanced readability and code maintenance

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
    private float sideTabAlpha = 0.30f;                             //alpha level of side tabs when inactive
    private float topTabAlpha = 0.50f;                              //alpha level of top tabs when inactive

    //button script handlers
    private ButtonInteraction buttonInteractionReset;
    private ButtonInteraction buttonInteractionConfirm;
    private ButtonInteraction buttonInteractionRecommended;
    private ButtonInteraction buttonInteractionSelect;
    private ButtonInteraction buttonInteractionDeselect;

    //button help components                       
    private GenericHelpTooltipUI helpCentre;                    //item help button on RHS panel (where no 'Select' button exists)
    private GenericHelpTooltipUI helpCombined;                  //item help button on RHS panel (adjacent to 'Select' button
    private GenericHelpTooltipUI helpMain;                      //main UI help button down the bottom right
    private GenericHelpTooltipUI helpReset;                     //reset button
    private GenericHelpTooltipUI helpConfirm;                   //confirm button
    private GenericHelpTooltipUI helpRecommended;               //reset button
    private GenericHelpTooltipUI helpTabBoss;                   //top side tab
    private GenericHelpTooltipUI helpTabSubBoss1;               //second side tab from top
    private GenericHelpTooltipUI helpTabSubBoss2;               //third side tab from top
    private GenericHelpTooltipUI helpTabSubBoss3;               //fourth side tab from top
    private GenericHelpTooltipUI helpTabStatus;                 //top tab
    private GenericHelpTooltipUI helpTabSelected;               //top tab
    private GenericHelpTooltipUI helpRenown;                    //renown background

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
    private TextMeshProUGUI[] arrayOfTopTabTitles;
    private MetaTopTabUI[] arrayOfTopTabInteractions;
    private int[] arrayOfTopTabOptions;                            //count of how many ACTIVE options are available for this tab
    //Top arrays -> metaData
    private GameObject[] arrayOfTopMetaMain;
    private TextMeshProUGUI[] arrayOfTopMetaText;
    private Image[] arrayOfTopMetaIcon;
    private Image[] arrayOfTopMetaBorder;
    private Image[] arrayOfTopMetaCheckMark;
    private Image[] arrayOfTopMetaBackground;

    private MetaData defaultSelected;                               //default metaData item to display when nothing has been selected
    private List<MetaData> listOfRecommended;

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

    /*private bool isRunning;*/
    private bool isLastTabTop;                                       //true if last tab pressed was a top tab
    private int highlightIndex = -1;                                 //item index of currently highlighted item
    private int maxHighlightIndex = -1;
    private int numOfItemsTotal = 20;                                //hardwired Max number of items -> 30
    private int numOfVisibleItems = 10;                              //hardwired visible items in main page -> 10
    private int numOfItemsCurrent = -1;                              //count of items in current list / page
    private int numOfItemsPrevious = -1;                             //count of items in previous list / page
    private int renownCurrent;                                       //current amount of renown remaining available to spend
    private int numOfChoicesCurrent = 0;                             //how many choices the player has made (capped at numOfChoicesMax)

    //help tooltips (I don't want this as a global, just a master private field)
    private int x_offset = 200;
    private int y_offset = 40;

    //data needed to run UI
    private MetaInfoData metaInfoData;

    //fast access
    private int costLow = -1;                                        //renown cost for low priority metaOptions
    private int costMedium = -1;
    private int costHigh = -1;
    private int costExtreme = -1;
    private int numOfChoicesMax = -1;                               //max num of items you choose (good or bad)
    private int renownRecommendMin = -1;                            //min amount of renown required for Recommendation button to work

    //colours
    string colourDefault;
    string colourNeutral;
    string colourGrey;
    string colourAlert;
    /*string colourGood;
    string colourBlue;
    string colourNormal;
    string colourError;
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
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
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
        arrayOfTopTabTitles = new TextMeshProUGUI[numOfTopTabs];
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
        index = 0;
        if (textStatus != null) { arrayOfTopTabTitles[index++] = textStatus; } else { Debug.LogError("Invalid textStatus (Null)"); }
        if (textSelected != null) { arrayOfTopTabTitles[index++] = textSelected; } else { Debug.LogError("Invalid textSelected (Null)"); }
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
        Debug.Assert(buttonHelpMain != null, "Invalid buttonHelpMain (Null)");
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
        buttonInteractionRecommended.SetButton(EventType.MetaGameRecommended);
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
        helpMain = buttonHelpMain.GetComponent<GenericHelpTooltipUI>();
        helpReset = buttonReset.GetComponent<GenericHelpTooltipUI>();
        helpConfirm = buttonConfirm.GetComponent<GenericHelpTooltipUI>();
        helpRecommended = buttonRecommended.GetComponent<GenericHelpTooltipUI>();
        helpTabBoss = tabBoss.GetComponent<GenericHelpTooltipUI>();
        helpTabSubBoss1 = tabSubBoss1.GetComponent<GenericHelpTooltipUI>();
        helpTabSubBoss2 = tabSubBoss2.GetComponent<GenericHelpTooltipUI>();
        helpTabSubBoss3 = tabSubBoss3.GetComponent<GenericHelpTooltipUI>();
        helpTabStatus = tabStatus.GetComponent<GenericHelpTooltipUI>();
        helpTabSelected = tabSelected.GetComponent<GenericHelpTooltipUI>();
        helpRenown = renownBackground.GetComponent<GenericHelpTooltipUI>();
        Debug.Assert(helpCentre != null, "Invalid helpCentre (Null)");
        Debug.Assert(helpCombined != null, "Invalid helpCombined (Null)");
        Debug.Assert(helpMain != null, "Invalid helpMain (Null)");
        Debug.Assert(helpReset != null, "Invalid helpReset (Null)");
        Debug.Assert(helpConfirm != null, "Invalid helpConfirm (Null)");
        Debug.Assert(helpRecommended != null, "Invalid helpRecommended (Null)");
        Debug.Assert(helpTabBoss != null, "Invalid helpTabBoss (Null)");
        Debug.Assert(helpTabSubBoss1 != null, "Invalid helpTabSubBoss1 (Null)");
        Debug.Assert(helpTabSubBoss2 != null, "Invalid helpTabSubBoss2 (Null)");
        Debug.Assert(helpTabSubBoss3 != null, "Invalid helpTabSubBoss3 (Null)");
        Debug.Assert(helpTabStatus != null, "Invalid helpTabStatus (Null)");
        Debug.Assert(helpTabSelected != null, "Invalid helpTabSelected (Null)");
        Debug.Assert(helpRenown != null, "Invalid helpRenown (Null)");
        //backgrounds
        Debug.Assert(backgroundMain != null, "Invalid backgroundMain (Null)");
        Debug.Assert(backgroundLeft != null, "Invalid backgroundLeft (Null)");
        Debug.Assert(backgroundCentre != null, "Invalid backgroundCentre (Null)");
        Debug.Assert(backgroundRight != null, "Invalid backgroundRight (Null)");
        //assign backgrounds and active tab colours
        Color colour = GameManager.i.guiScript.colourMainBackground;
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
        InitialiseTooltips();
    }
    #endregion

    #region SubInitialiseFastAccess
    /// <summary>
    /// fast access
    /// </summary>
    private void SubInitialiseFastAccess()
    {
        //priority icons
        priorityHigh = GameManager.i.guiScript.priorityHighSprite;
        priorityMedium = GameManager.i.guiScript.priorityMediumSprite;
        priorityLow = GameManager.i.guiScript.priorityLowSprite;
        priorityInactive = GameManager.i.guiScript.priorityInactiveSprite;
        Debug.Assert(priorityHigh != null, "Invalid priorityHigh (Null)");
        Debug.Assert(priorityMedium != null, "Invalid priorityMedium (Null)");
        Debug.Assert(priorityLow != null, "Invalid priorityLow (Null)");
        Debug.Assert(priorityInactive != null, "Invalid priorityInactive (Null)");
        //cost levels for metaOptions
        costLow = GameManager.i.metaScript.costLowPriority;
        costMedium = GameManager.i.metaScript.costMediumPriority;
        costHigh = GameManager.i.metaScript.costHighPriority;
        costExtreme = GameManager.i.metaScript.costExtremePriority;
        Debug.Assert(costLow > -1, "Invalid costLow (-1)");
        Debug.Assert(costMedium > -1, "Invalid costMedium (-1)");
        Debug.Assert(costHigh > -1, "Invalid costHigh (-1)");
        Debug.Assert(costExtreme > -1, "Invalid costExtreme (-1)");
        numOfChoicesMax = GameManager.i.metaScript.numOfChoices;
        Debug.Assert(numOfChoicesMax > -1, "Invalid numOfChoicesMax (-1)");
        renownRecommendMin = GameManager.i.metaScript.renownRecommendMin;
        Debug.Assert(renownRecommendMin > -1, "Invalid renownRecommendMin (-1)");
    }
    #endregion

    #region SubInitialiseEvents
    /// <summary>
    /// events
    /// </summary>
    private void SubInitialiseEvents()
    {
        //listeners
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameOpen, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameClose, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameSideTabOpen, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameTopTabOpen, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameShowDetails, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameUpArrow, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameDownArrow, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameLeftArrow, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameRightArrow, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGamePageUp, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGamePageDown, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameSelect, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameDeselect, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameButton, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameReset, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameRecommended, OnEvent, "MetaGamesUI");
        EventManager.i.AddListener(EventType.MetaGameConfirm, OnEvent, "MetaGamesUI");
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
                SetMetaUI();
                break;
            case EventType.MetaGameShowDetails:
                ShowItemDetails((int)Param);
                break;
            case EventType.MetaGameSelect:
                ExecuteSelect((int)Param);
                break;
            case EventType.MetaGameDeselect:
                ExecuteDeselect((int)Param);
                break;
            case EventType.MetaGameButton:
                ExecuteButton((int)Param);
                break;
            case EventType.MetaGameReset:
                ExecuteReset();
                break;
            case EventType.MetaGameRecommended:
                ExecuteRecommended();
                break;
            case EventType.MetaGameConfirm:
                ExecuteConfirm();
                break;
            case EventType.MetaGameUpArrow:
                ExecuteUpArrow();
                break;
            case EventType.MetaGameDownArrow:
                ExecuteDownArrow();
                break;
            case EventType.MetaGameLeftArrow:
                ExecuteLeftArrow();
                break;
            case EventType.MetaGameRightArrow:
                ExecuteRightArrow();
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
        colourDefault = GameManager.i.colourScript.GetColour(ColourType.whiteText);
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourGrey = GameManager.i.colourScript.GetColour(ColourType.greyText);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        /*colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourBlue = GameManager.instance.colourScript.GetColour(ColourType.blueText)
        colourNormal = GameManager.i.colourScript.GetColour(ColourType.normalText);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourError = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.salmonText);*/
        colourCancel = GameManager.i.colourScript.GetColour(ColourType.moccasinText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
    }

    /// <summary>
    /// Input data required to populate UI
    /// </summary>
    /// <param name="data"></param>
    public void SetMetaInfoData(MetaInfoData data)
    {
        if (data != null)
        { metaInfoData = data; }
        else { Debug.LogError("Invalid metaInfoData (Null)"); }
    }

    /// <summary>
    /// run prior to every metaGameUI use. Run from SetMetaUI
    /// </summary>
    private void InitialiseMetaUI()
    {
        if (metaInfoData != null)
        {
            int count;
            //player renown
            int playerRenown = GameManager.i.playerScript.Renown;
            renownCurrent = playerRenown;
            UpdateRenown();
            Debug.LogFormat("[Met] MetaGameUI.cs -> InitialiseMetaUI: Player has {0} Renown{1}", playerRenown, "\n");
            Color portraitColor, backgroundColor;
            //default metaData for selected tab (when nothing has been selected)
            defaultSelected = metaInfoData.selectedDefault;
            Debug.Assert(defaultSelected != null, "Invalid defaultSelected (Null)");
            listOfRecommended = metaInfoData.listOfRecommended;
            Debug.Assert(listOfRecommended != null, "Invalid listOfRecommended (Null)");
            //initialise HQ side tabs'
            for (int index = 0; index < numOfSideTabs; index++)
            {
                backgroundColor = arrayOfSideTabItems[index].background.color;
                if (arrayOfSideTabItems[index] != null)
                {
                    //sprite
                    Actor actor = GameManager.i.dataScript.GetHqHierarchyActor((ActorHQ)(index + offset));
                    if (actor != null)
                    { arrayOfSideTabItems[index].portrait.sprite = actor.sprite; }
                    else
                    {
                        //default error sprite if a problem
                        Debug.LogWarningFormat("Invalid actor (Null) for ActorHQ \"{0}\"", (ActorHQ)(index + offset));
                        arrayOfSideTabItems[index].portrait.sprite = GameManager.i.guiScript.errorSprite;
                    }
                    portraitColor = arrayOfSideTabItems[index].portrait.color;
                    //title
                    arrayOfSideTabItems[index].title.text = GameManager.i.hqScript.GetHqTitle(actor.statusHQ);
                    //first tab (Boss) should be active on opening, rest passive
                    if (index == 0)
                    { portraitColor.a = 1.0f; backgroundColor.a = 1.0f; }
                    else
                    { portraitColor.a = sideTabAlpha; backgroundColor.a = sideTabAlpha; }
                    //set colors
                    arrayOfSideTabItems[index].portrait.color = portraitColor;
                    arrayOfSideTabItems[index].background.color = backgroundColor;
                    //set count of active items in each tab
                    count = 0;
                    for (int j = 0; j < metaInfoData.arrayOfMetaData[index].Count; j++)
                    {
                        if (metaInfoData.arrayOfMetaData[index][j].isActive == true)
                        { count++; }
                    }
                    arrayOfSideTabOptions[index] = count;
                }
                else { Debug.LogErrorFormat("Invalid tabItems[{0}] (Null)", index); }
            }
            //initialise top status tab count (selected tab is dynamic and resolved at time of DisplayTopItemPage
            arrayOfTopTabOptions[(int)MetaTabTop.Status] = metaInfoData.listOfStatusData.Count;
            //initialise top tabs (start inactive) -> status tab 
            backgroundColor = tabStatus.color;
            backgroundColor.a = topTabAlpha;
            tabStatus.color = backgroundColor;
            backgroundColor = textStatus.color;
            backgroundColor.a = topTabAlpha;
            textStatus.color = backgroundColor;
            //Selected tab
            backgroundColor = tabSelected.color;
            backgroundColor.a = topTabAlpha;
            tabSelected.color = backgroundColor;
            backgroundColor = textSelected.color;
            backgroundColor.a = topTabAlpha;
            textSelected.color = backgroundColor;
            //tab pressed default settings (starts with side tab index 0)
            isLastTabTop = false;
            currentTopTabIndex = 0;
            //Turn off checkmarks
            ResetCheckMarks();
            //clear out dict
            dictOfSelected.Clear();
            //
            // - - - dynamic tooltip help
            //
            //Boss tab
            List<HelpData> listOfHelp = GameManager.i.actorScript.GetHqTooltip(ActorHQ.Boss);
            if (listOfHelp != null)
            { helpTabBoss.SetHelpTooltip(listOfHelp, x_offset, y_offset); }
            else { Debug.LogWarning("Invalid listOfHelp for helpTabBoss (Null)"); }
            //subBoss1 tab
            listOfHelp = GameManager.i.actorScript.GetHqTooltip(ActorHQ.SubBoss1);
            if (listOfHelp != null)
            { helpTabSubBoss1.SetHelpTooltip(listOfHelp, x_offset, y_offset); }
            else { Debug.LogWarning("Invalid listOfHelp for helpTabSubBoss1 (Null)"); }
            //subBoss2 tab
            listOfHelp = GameManager.i.actorScript.GetHqTooltip(ActorHQ.SubBoss2);
            if (listOfHelp != null)
            { helpTabSubBoss2.SetHelpTooltip(listOfHelp, x_offset, y_offset); }
            else { Debug.LogWarning("Invalid listOfHelp for helpTabSubBoss2 (Null)"); }
            //subBoss1 tab
            listOfHelp = GameManager.i.actorScript.GetHqTooltip(ActorHQ.SubBoss3);
            if (listOfHelp != null)
            { helpTabSubBoss3.SetHelpTooltip(listOfHelp, x_offset, y_offset); }
            else { Debug.LogWarning("Invalid listOfHelp for helpTabSubBoss3 (Null)"); }
        }
        else { Debug.LogError("Invalid metaInfoData (Null)"); }
    }



    /// <summary>
    /// Initialise MetaData items
    /// </summary>
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
    /// Initialise fixed tooltips
    /// </summary>
    private void InitialiseTooltips()
    {
        List<HelpData> listOfHelp;
        //main help button
        listOfHelp = GameManager.i.helpScript.GetHelpData("metaGameUI_0", "metaGameUI_1", "metaGameUI_2", "metaGameUI_3");
        if (listOfHelp != null)
        { helpMain.SetHelpTooltip(listOfHelp, x_offset, y_offset); }
        else { Debug.LogWarning("Invalid listOfHelp for helpMain (Null)"); }
        //reset button
        listOfHelp = GameManager.i.helpScript.GetHelpData("metaGameUI_4", "metaGameUI_5");
        if (listOfHelp != null)
        { helpReset.SetHelpTooltip(listOfHelp, x_offset, y_offset); }
        else { Debug.LogWarning("Invalid listOfHelp for helpReset (Null)"); }
        //confirm button
        listOfHelp = GameManager.i.helpScript.GetHelpData("metaGameUI_6", "metaGameUI_7");
        if (listOfHelp != null)
        { helpConfirm.SetHelpTooltip(listOfHelp, x_offset + 25, y_offset); }
        else { Debug.LogWarning("Invalid listOfHelp for helpConfirm (Null)"); }
        //recommended button
        listOfHelp = GameManager.i.helpScript.GetHelpData("metaGameUI_8", "metaGameUI_9");
        if (listOfHelp != null)
        { helpRecommended.SetHelpTooltip(listOfHelp, x_offset, y_offset); }
        else { Debug.LogWarning("Invalid listOfHelp for helpRec0mmended (Null)"); }
        //Status tab
        listOfHelp = GameManager.i.helpScript.GetHelpData("metaGameUI_10", "metaGameUI_11", "metaGameUI_12");
        if (listOfHelp != null)
        { helpTabStatus.SetHelpTooltip(listOfHelp, x_offset, y_offset); }
        else { Debug.LogWarning("Invalid listOfHelp for helpTabStatus (Null)"); }
        //Selected tab
        listOfHelp = GameManager.i.helpScript.GetHelpData("metaGameUI_13", "metaGameUI_14");
        if (listOfHelp != null)
        { helpTabSelected.SetHelpTooltip(listOfHelp, x_offset, y_offset); }
        else { Debug.LogWarning("Invalid listOfHelp for helpTabSelected (Null)"); }
        //Renown
        listOfHelp = GameManager.i.helpScript.GetHelpData("metaGameUI_15", "metaGameUI_16");
        if (listOfHelp != null)
        { helpRenown.SetHelpTooltip(listOfHelp, x_offset, y_offset); }
        else { Debug.LogWarning("Invalid listOfHelp for helpRenown (Null)"); }
    }

    /// <summary>
    /// Display MetaGame Player options UI
    /// </summary>
    public void SetMetaUI()
    {
        if (metaInfoData != null)
        {
            //set blocked (failsafe)
            GameManager.i.guiScript.SetIsBlocked(true);
            //initialise
            InitialiseMetaUI();
            canvasMeta.gameObject.SetActive(true);
            // Populate data
            UpdateData(metaInfoData);
            // Display Boss page by default
            OpenSideTab(0);
            //select buttons, RHS, off by default
            buttonSelect.gameObject.SetActive(false);
            buttonDeselect.gameObject.SetActive(false);
            helpCentre.gameObject.SetActive(false);
            helpCombined.gameObject.SetActive(false);
            helpMain.gameObject.SetActive(true);
            /*//set game state
            isRunning = true;*/
            GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.MetaGame, metaState = ModalMetaSubState.PlayerOptions });
            Debug.LogFormat("[UI] MetaGameUI.cs -> SetMetaUI{0}", "\n");
        }
        else { Debug.LogWarning("Invalid MetaInfoData (Null)"); }
    }


    /// <summary>
    /// close Meta UI display
    /// </summary>
    private void CloseMetaUI()
    {
        GameManager.i.tooltipGenericScript.CloseTooltip("MainInfoUI.cs -> CloseMainInfo");
        GameManager.i.tooltipHelpScript.CloseTooltip("MainInfoUI.cs -> CloseMainInfo");
        canvasMeta.gameObject.SetActive(false);
        Debug.LogFormat("[UI] MainInfoUI.cs -> CloseMainInfo{0}", "\n");
        //update player renown
        GameManager.i.playerScript.Renown = renownCurrent;
        Debug.LogFormat("[Met] MetaGameUI.cs -> CloseMetaUI: Player carries over {0} Renown{1}", renownCurrent, "\n");
        //set state
        GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.MetaGame, metaState = ModalMetaSubState.EndScreen });
        /*
        //outcome
        ModalOutcomeDetails details = new ModalOutcomeDetails();
        details.side = GameManager.i.sideScript.PlayerSide;
        details.textTop = GameManager.Formatt("Prepare for Insertion", ColourType.neutralText);
        details.textBottom = string.Format("Are you ready for deployment, soldier? Are you willing to {0} for the Cause?", GameManager.Formatt("fight and die", ColourType.moccasinText));
        details.sprite = GameManager.i.guiScript.infoSprite;
        details.modalLevel = 2;
        details.modalState = ModalSubState.MetaGame;
        details.triggerEvent = EventType.CloseMetaOverall;
        //open outcome windown (will open MetaGameUI via triggerEvent once closed
        EventManager.i.PostNotification(EventType.OutcomeOpen, this, details, "TransitionUI.cs -> ExecuteClose");
        */

        //confirm window that will open new level on closing
        ModalConfirmDetails details = new ModalConfirmDetails();
        details.topText = string.Format("Prepare for deployment, soldier. It's time to do whatever it takes to further the Cause", GameManager.Formatt("assistance", ColourType.moccasinText));
        details.bottomText = "Are you Ready?";
        details.buttonFalse = "SAVE and EXIT";
        details.buttonTrue = "CONTINUE";
        details.eventFalse = EventType.SaveAndExit;
        details.eventTrue = EventType.CloseMetaOverall;
        details.modalState = ModalSubState.MetaGame;
        details.restorePoint = RestorePoint.MetaEnd;
        //open confirm
        EventManager.i.PostNotification(EventType.ConfirmOpen, this, details, "TransitionUI.cs -> ExecuteClose");

    }


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
            backgroundColor = arrayOfSideTabItems[index].background.color;
            //activate indicated tab and deactivate the rest
            if (index == tabIndex)
            {
                portraitColor.a = 1.0f;
                backgroundColor.a = 1.0f;
            }
            else
            {
                portraitColor.a = sideTabAlpha;
                backgroundColor.a = sideTabAlpha;
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
        //reset buttons
        buttonSelect.gameObject.SetActive(false);
        buttonDeselect.gameObject.SetActive(false);
        buttonHelpCombined.gameObject.SetActive(false);
        buttonHelpCentre.gameObject.SetActive(false);
        //redrawn main page
        DisplaySideItemPage(tabIndex);
        //update indexes
        highlightIndex = -1;
        currentSideTabIndex = tabIndex;
        //need to deselect last active top tab
        if (isLastTabTop == true)
        {
            Color color = arrayOfTopTabImages[currentTopTabIndex].color;
            color.a = topTabAlpha;
            arrayOfTopTabImages[currentTopTabIndex].color = color;
            color = arrayOfTopTabTitles[currentTopTabIndex].color;
            color.a = topTabAlpha;
            arrayOfTopTabTitles[currentTopTabIndex].color = color;
        }
        isLastTabTop = false;
    }

    /// <summary>
    /// Open the designated Top tab and close whatever is open
    /// </summary>
    /// <param name="tabIndex"></param>
    private void OpenTopTab(int tabIndex)
    {
        Debug.AssertFormat(tabIndex > -1 && tabIndex < numOfTopTabs, "Invalid tab index {0}", tabIndex);

        //reset Active tabs to reflect new status
        Color backgroundColor, textColor, portraitColor;
        for (int index = 0; index < numOfTopTabs; index++)
        {
            backgroundColor = arrayOfTopTabImages[index].color;
            textColor = arrayOfTopTabTitles[index].color;
            //activate indicated tab and deactivate the rest
            if (index == tabIndex)
            {
                backgroundColor.a = 1.0f;
                textColor.a = 1.0f;
            }
            else
            {
                backgroundColor.a = topTabAlpha;
                textColor.a = topTabAlpha;
            }
            arrayOfTopTabImages[index].color = backgroundColor;
            arrayOfTopTabTitles[index].color = textColor;
        }
        //assign default RHS values
        switch ((MetaTabTop)tabIndex)
        {
            case MetaTabTop.Status:
                rightTextTop.text = "Player Status Options";
                rightTextBottom.text = string.Format("Shows all <b>ongoing options</b> that will {0}<b>carry over</b>{1} to the <b>next city</b> if <b>not dealt with</b><br><br>Includes {2}<b>Conditions, " +
                    "Secrets, Investigations</b>{3} and <b>contacts</b> with {4}<b>Organisations</b>{5} (that <b>will be lost</b>)", colourCancel, colourEnd, colourCancel, colourEnd, colourCancel, colourEnd);
                break;
            case MetaTabTop.Selected:
                rightTextTop.text = "Selected Options";
                rightTextBottom.text = string.Format("A <b>summary</b> of <b>all options</b> that you have {0}<b>Selected</b>{1}<br><br>You can {2}<b>Deselect</b>{3} any option",
                    colourCancel, colourEnd, colourCancel, colourEnd);
                break;
            default:
                rightTextTop.text = "Unknown";
                rightTextBottom.text = "Unknown";
                break;
        }
        rightImage.sprite = rightImageDefault;
        //reset buttons
        buttonSelect.gameObject.SetActive(false);
        buttonDeselect.gameObject.SetActive(false);
        buttonHelpCombined.gameObject.SetActive(false);
        buttonHelpCentre.gameObject.SetActive(false);
        //redrawn main page
        DisplayTopItemPage(tabIndex);
        //update indexes
        highlightIndex = -1;
        currentTopTabIndex = tabIndex;
        //need do to deselect last active side tab
        if (isLastTabTop == false)
        {
            backgroundColor = arrayOfSideTabItems[currentSideTabIndex].background.color;
            backgroundColor.a = sideTabAlpha;
            arrayOfSideTabItems[currentSideTabIndex].background.color = backgroundColor;
            portraitColor = arrayOfSideTabItems[currentSideTabIndex].portrait.color;
            portraitColor.a = sideTabAlpha;
            arrayOfSideTabItems[currentSideTabIndex].portrait.color = portraitColor;
        }
        isLastTabTop = true;
    }


    /// <summary>
    /// sub Method to display a particular page drawing from cached data in dictOfItemData -> side tab
    /// </summary>
    /// <param name="tab"></param>
    private void DisplaySideItemPage(int tabIndex)
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
                        //checkmark
                        if (listOfCurrentPageSideMetaData[index].isSelected == true)
                        { arrayOfSideMetaCheckMark[index].gameObject.SetActive(true); }
                        else { arrayOfSideMetaCheckMark[index].gameObject.SetActive(false); }
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
    /// sub Method to display a particular page drawing from cached data in dictOfItemData -> top tab 
    /// </summary>
    /// <param name="tab"></param>
    private void DisplayTopItemPage(int tabIndex)
    {
        Debug.Assert(tabIndex > -1 && tabIndex < (int)ItemTab.Count, string.Format("Invalid tabIndex {0}", tabIndex));
        //page header (dynamic calc for 'Selected' tab)
        int count = 0;
        switch ((MetaTabTop)tabIndex)
        {
            case MetaTabTop.Selected: count = dictOfSelected.Count; break;
            default: count = arrayOfTopTabOptions[tabIndex]; break;
        }
        page_header.text = string.Format("{0} Option{1} available", count, count != 1 ? "s" : "");
        //clear out current data
        listOfCurrentPageTopMetaData.Clear();
        //Get data -> selected tab
        if (tabIndex == (int)MetaTabTop.Selected)
        {
            //nothing currently selected, show default
            if (dictOfSelected.Count == 0)
            { listOfCurrentPageTopMetaData.Add(defaultSelected); }
            else { listOfCurrentPageTopMetaData = dictOfSelected.Values.ToList(); }
        }
        else
        {
            //get data -> all other top tabs
            listOfCurrentPageTopMetaData.AddRange(arrayOfTopMetaData[tabIndex]);
        }
        //display routine
        if (listOfCurrentPageTopMetaData != null)
        {
            numOfItemsCurrent = listOfCurrentPageTopMetaData.Count;
            maxHighlightIndex = numOfItemsCurrent - 1;
            if (numOfItemsCurrent > 0)
            {
                //populate current messages for the main tab
                for (int index = 0; index < arrayOfTopMetaText.Length; index++)
                {
                    if (index < numOfItemsCurrent)
                    {
                        //populate text and set item to active
                        arrayOfTopMetaText[index].text = listOfCurrentPageTopMetaData[index].itemText;
                        arrayOfTopMetaMain[index].gameObject.SetActive(true);
                        //checkmarks
                        if (listOfCurrentPageTopMetaData[index].isSelected == true)
                        { arrayOfTopMetaCheckMark[index].gameObject.SetActive(true); }
                        else { arrayOfTopMetaCheckMark[index].gameObject.SetActive(false); }
                        //assign icon
                        if (listOfCurrentPageTopMetaData[index].isActive == true)
                        {
                            switch (listOfCurrentPageTopMetaData[index].priority)
                            {
                                case MetaPriority.Extreme:
                                case MetaPriority.High:
                                    arrayOfTopMetaIcon[index].sprite = priorityHigh;
                                    break;
                                case MetaPriority.Medium:
                                    arrayOfTopMetaIcon[index].sprite = priorityMedium;
                                    break;
                                case MetaPriority.Low:
                                    arrayOfTopMetaIcon[index].sprite = priorityLow;
                                    break;
                                default:
                                    Debug.LogWarningFormat("Invalid priority \"{0}\"", listOfCurrentPageTopMetaData[index].priority);
                                    break;
                            }
                        }
                        else { arrayOfTopMetaIcon[index].sprite = priorityInactive; }

                    }
                    else if (index < numOfItemsPrevious)
                    {
                        //efficient -> only disables items that were previously active, not the whole set
                        arrayOfTopMetaText[index].text = "";
                        arrayOfTopMetaMain[index].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //no data, blank previous items (not necessarily all), disable line
                for (int index = 0; index < numOfItemsPrevious; index++)
                {
                    arrayOfTopMetaText[index].text = "";
                    arrayOfTopMetaMain[index].gameObject.SetActive(false);
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
            arrayOfTopMetaData[(int)MetaTabTop.Status].Clear();
            //populate status top tab data
            for (int i = 0; i < data.listOfStatusData.Count; i++)
            {

                MetaData metaData = data.listOfStatusData[i];
                if (metaData != null)
                { arrayOfTopMetaData[(int)MetaTabTop.Status].Add(metaData); }
                else { Debug.LogWarningFormat("Invalid metaData (Null) for listOfStatusData[i]"); }
            }
        }
        else { Debug.LogError("Invalid data.listOfStatusData (Null)"); }
    }



    /// <summary>
    /// MetaData details
    /// </summary>
    /// <param name="itemIndex"></param>
    private void ShowItemDetails(int itemIndex)
    {
        MetaData data;
        //check top or side tab pressed
        if (isLastTabTop == true)
        { data = listOfCurrentPageTopMetaData[itemIndex]; }
        else { data = listOfCurrentPageSideMetaData[itemIndex]; }
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
                //inactive
                rightTextTop.text = string.Format("{0}<b>{1}</b>{2}", colourGrey, "Option Unavailable", colourEnd);
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
                {
                    if (isLastTabTop == true)
                    { arrayOfTopMetaText[highlightIndex].text = string.Format("{0}{1}{2}", colourDefault, listOfCurrentPageTopMetaData[highlightIndex].itemText, colourEnd); }
                    else { arrayOfSideMetaText[highlightIndex].text = string.Format("{0}{1}{2}", colourDefault, listOfCurrentPageSideMetaData[highlightIndex].itemText, colourEnd); }
                }
                highlightIndex = itemIndex;
                //highlight item -> yelllow if active, grey if not
                if (data.isActive == true)
                {
                    if (isLastTabTop == true)
                    { arrayOfTopMetaText[itemIndex].text = string.Format("{0}<b>{1}</b>{2}", colourNeutral, listOfCurrentPageTopMetaData[itemIndex].itemText, colourEnd); }
                    else { arrayOfSideMetaText[itemIndex].text = string.Format("{0}<b>{1}</b>{2}", colourNeutral, listOfCurrentPageSideMetaData[itemIndex].itemText, colourEnd); }
                }
                else
                {
                    if (isLastTabTop == true)
                    { arrayOfTopMetaText[itemIndex].text = string.Format("{0}<size=115%><b>{1}</size></b>{2}", colourDefault, listOfCurrentPageTopMetaData[itemIndex].itemText, colourEnd); }
                    else { arrayOfSideMetaText[itemIndex].text = string.Format("{0}<size=115%><b>{1}</size></b>{2}", colourDefault, listOfCurrentPageSideMetaData[itemIndex].itemText, colourEnd); }
                }
            }
        }
        else
        {
            Debug.LogWarningFormat("Invalid MetaData for listOfCurrentPageMetaData[{0}]", itemIndex);
        }
    }

    /// <summary>
    /// Assembles a list of HelpData for the item to pass onto the help components. Returns Empty if none.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private List<HelpData> GetItemHelpList(MetaData data)
    {
        string tag0, tag1, tag2, tag3;
        tag0 = tag1 = tag2 = tag3 = "";
        //help only valid if first tag present ('help0/tag0'
        if (string.IsNullOrEmpty(data.tag0) == false)
        {
            //item specified help
            tag0 = data.tag0;
            tag1 = data.tag1;
            tag2 = data.tag2;
            tag3 = data.tag3;
        }
        else if (data.isActive == true)
        {
            //active item default help
            tag0 = "metaActive_0";
            tag1 = "metaActive_1";
            tag2 = "metaActive_2";
        }
        else
        {
            //inactive item default help
            tag0 = "metaInactive_0";
            tag1 = "metaInactive_1";
            tag2 = "metaInactive_2";
        }
        return GameManager.i.helpScript.GetHelpData(tag0, tag1, tag2, tag3);
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
                //check top or side tab
                if (isLastTabTop == true)
                { OpenTopTab(currentTopTabIndex); }
                else { OpenSideTab(currentSideTabIndex); }
            }
        }
    }

    /// <summary>
    /// Page UP (next side tab up)
    /// </summary>
    private void ExecutePageUp()
    {
        //check if previously using top tab
        if (isLastTabTop == true)
        {
            Color color = arrayOfTopTabImages[currentTopTabIndex].color;
            color.a = topTabAlpha;
            arrayOfTopTabImages[currentTopTabIndex].color = color;
            color = arrayOfTopTabTitles[currentTopTabIndex].color;
            color.a = topTabAlpha;
            arrayOfTopTabTitles[currentTopTabIndex].color = color;
        }
        isLastTabTop = false;
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
    /// Page DOWN (next side tab down)
    /// </summary>
    private void ExecutePageDown()
    {
        //check if previously using top tab
        if (isLastTabTop == true)
        {
            Color color = arrayOfTopTabImages[currentTopTabIndex].color;
            color.a = topTabAlpha;
            arrayOfTopTabImages[currentTopTabIndex].color = color;
            color = arrayOfTopTabTitles[currentTopTabIndex].color;
            color.a = topTabAlpha;
            arrayOfTopTabTitles[currentTopTabIndex].color = color;
        }
        isLastTabTop = false;
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
    /// Left Arrow (next top tab across)
    /// </summary>
    private void ExecuteLeftArrow()
    {
        //check if previously using side tab
        if (isLastTabTop == false)
        {
            Color color = arrayOfSideTabItems[currentSideTabIndex].background.color;
            color.a = sideTabAlpha;
            arrayOfSideTabItems[currentSideTabIndex].background.color = color;
            color = arrayOfSideTabItems[currentSideTabIndex].portrait.color;
            color.a = sideTabAlpha;
            arrayOfSideTabItems[currentSideTabIndex].portrait.color = color;
        }
        isLastTabTop = true;
        if (currentTopTabIndex > -1)
        {
            if (currentTopTabIndex > 0)
            {
                currentTopTabIndex -= 1;
                OpenTopTab(currentTopTabIndex);
            }
            else
            {
                //roll over
                currentTopTabIndex = maxTopTabIndex;
                OpenTopTab(currentTopTabIndex);
            }
        }
    }

    /// <summary>
    /// Right Arrow (next top tab across)
    /// </summary>
    private void ExecuteRightArrow()
    {
        //check if previously using side tab
        if (isLastTabTop == false)
        {
            Color color = arrayOfSideTabItems[currentSideTabIndex].background.color;
            color.a = sideTabAlpha;
            arrayOfSideTabItems[currentSideTabIndex].background.color = color;
            color = arrayOfSideTabItems[currentSideTabIndex].portrait.color;
            color.a = sideTabAlpha;
            arrayOfSideTabItems[currentSideTabIndex].portrait.color = color;
        }
        isLastTabTop = true;
        if (currentTopTabIndex > -1)
        {
            if (currentTopTabIndex < maxTopTabIndex)
            {
                currentTopTabIndex += 1;
                OpenTopTab(currentTopTabIndex);
            }
            else
            {
                //roll over
                currentTopTabIndex = 0;
                OpenTopTab(currentTopTabIndex);
            }
        }
    }

    /// <summary>
    /// Select button pressed, RHS. ItemIndex parameter returned in case of itemInteraction mouse click, not the case for Select/Deselect button pressed on RHS (uses existing highlight for this)
    /// </summary>
    private void ExecuteSelect(int itemIndex = -1)
    {
        if (numOfChoicesCurrent < numOfChoicesMax)
        {
            MetaData metaData = GetMetaData(itemIndex);
            //process metaData
            if (metaData != null)
            {
                MetaOption metaOption = GameManager.i.dataScript.GetMetaOption(metaData.metaName);
                if (metaOption != null)
                {
                    //add to dictOfSelected
                    AddToSelected(metaData);
                    //adjust renown display
                    renownCurrent -= metaData.renownCost;
                    renownCurrent = Mathf.Max(0, renownCurrent);
                    UpdateRenown();
                    //set as selected
                    metaData.isSelected = true;
                    //switch buttons
                    buttonSelect.gameObject.SetActive(false);
                    buttonDeselect.gameObject.SetActive(true);
                    //checkmark
                    arrayOfSideMetaCheckMark[highlightIndex].gameObject.SetActive(true);
                    arrayOfTopMetaCheckMark[highlightIndex].gameObject.SetActive(true);
                    //switch top texts
                    rightTextTop.text = metaData.textDeselect;
                    //popUp floating text -> test
                    SetRenownPopUp(metaData.renownCost * -1);
                }
                else { Debug.LogWarningFormat("Invalid metaOption (Null) for metaData.metaName \"{0}\"", metaData.metaName); }
            }
            else { Debug.LogWarningFormat("Invalid metaData (Null) for listOfCurrentPageMetaData[{0}]", highlightIndex); }
        }
        else
        {
            //maxxed out your available choices
            ModalOutcomeDetails details = new ModalOutcomeDetails()
            {
                side = GameManager.i.sideScript.PlayerSide,
                textTop = string.Format("{0}LIMIT EXCEEDED{1}", colourAlert, colourEnd),
                textBottom = string.Format("You have selected {0}{1}{2} option{3}<br><br>You have reached the {4}allowable limit{5} (max {6}{7}{8})<br><br>{9}Deselect{10} to free up more choices",
                    colourNeutral, numOfChoicesCurrent, colourEnd, numOfChoicesCurrent != 1 ? "s" : "", colourNeutral, colourEnd, colourNeutral, numOfChoicesMax, colourEnd, colourNeutral, colourEnd),
                sprite = GameManager.i.guiScript.alertInformationSprite,
                modalLevel = 2,
                modalState = ModalSubState.MetaGame,
                reason = "Recommended pressed"
            };
            EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
        }
    }




    /// <summary>
    /// Deselect button pressed, RHS. ItemIndex parameter returned in case of itemInteraction mouse click, not the case for Select/Deselect button pressed on RHS (uses existing highlight for this)
    /// </summary>
    private void ExecuteDeselect(int itemIndex = -1)
    {
        MetaData metaData = GetMetaData(itemIndex);
        //process metaData
        if (metaData != null)
        {
            MetaOption metaOption = GameManager.i.dataScript.GetMetaOption(metaData.metaName);
            if (metaOption != null)
            {
                //remove from dictOfSelected
                RemoveFromSelected(metaData);
                //adjust renown display
                renownCurrent += metaData.renownCost;
                UpdateRenown();
                //set as deSelected
                metaData.isSelected = false;
                //switch buttons
                buttonDeselect.gameObject.SetActive(false);
                buttonSelect.gameObject.SetActive(true);
                //checkmark
                arrayOfSideMetaCheckMark[highlightIndex].gameObject.SetActive(false);
                arrayOfTopMetaCheckMark[highlightIndex].gameObject.SetActive(false);
                //popUp floating text -> test
                SetRenownPopUp(metaData.renownCost);
                //switch top texts
                rightTextTop.text = metaData.textSelect;
            }
            else { Debug.LogWarningFormat("Invalid metaOption (Null) for metaData.metaName \"{0}\"", metaData.metaName); }
        }
        else { Debug.LogWarningFormat("Invalid metaData (Null) for listOfCurrentPageMetaData[{0}]", highlightIndex); }
    }

    /// <summary>
    /// SubMethod to get metaData (different if side or top tab open) for ExecuteSelect/Deselect/Button. Returns null if a problem
    /// </summary>
    /// <param name="itemIndex"></param>
    /// <returns></returns>
    private MetaData GetMetaData(int itemIndex)
    {
        MetaData metaData = null;
        //update highlight index for itemInteraction right click only
        if (itemIndex > -1)
        { highlightIndex = itemIndex; }
        //metaData depends on which tab type has been selected (each has it's own records)
        if (isLastTabTop == true)
        { metaData = listOfCurrentPageTopMetaData[highlightIndex]; }
        else { metaData = listOfCurrentPageSideMetaData[highlightIndex]; }
        return metaData;
    }

    /// <summary>
    /// Initiate pop up text over renown display
    /// </summary>
    /// <param name="amount"></param>
    private void SetRenownPopUp(int amount)
    {
        PopUpDynamicData data = new PopUpDynamicData()
        {
            position = renownBackground.transform.position,
            x_offset = -50,
            y_offset = 60,
            text = string.Format("Renown {0}{1}", amount > 0 ? "+" : "", amount)
        };
        GameManager.i.popUpDynamicScript.ExecuteDynamic(data);
    }

    /// <summary>
    /// Generic keyboard shortcut for Select/Deselect button press (it is whatever button is active at the time). Does nothing if neither button valid
    /// </summary>
    private void ExecuteButton(int itemIndex = -1)
    {
        //done to handle space bar (uses whatever item is currently selected, ignores action if none)
        if (itemIndex == -1)
        {
            //space bar
            if (highlightIndex > -1)
            {
                MetaData data = GetMetaData(highlightIndex);
                if (data != null)
                {
                    //active option
                    if (data.isActive == true)
                    {
                        //currently selected / deselected
                        if (data.isSelected == false)
                        { ExecuteSelect(itemIndex); }
                        else
                        { ExecuteDeselect(itemIndex); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid metaData (Null) for highlightIndex \"{0}\"", highlightIndex); }
            }
        }
        else
        {
            //right click
            if (buttonSelect.gameObject.activeSelf == true)
            { ExecuteSelect(itemIndex); }
            else if (buttonDeselect.gameObject.activeSelf == true)
            { ExecuteDeselect(itemIndex); }
        }
    }

    /// <summary>
    /// Deselects all metaData options to give a clean slate. Displays a confirmation message by default
    /// </summary>
    private void ExecuteReset(bool isDisplayMsg = true)
    {
        if (dictOfSelected.Count > 0)
        {
            ResetCheckMarks();
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
            UpdateRenown();
            //empty dict
            dictOfSelected.Clear();

            //update current item
            if (highlightIndex > -1)
            {
                MetaData data;
                //check top or side tab
                if (isLastTabTop == true)
                { data = listOfCurrentPageTopMetaData[highlightIndex]; }
                else { data = listOfCurrentPageSideMetaData[highlightIndex]; }
                //process metaData
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
                side = GameManager.i.sideScript.PlayerSide,
                textTop = string.Format("{0}RESET{1}", colourNeutral, colourEnd),
                textBottom = "All available options are now ready for selection again",
                sprite = GameManager.i.guiScript.infoSprite,
                modalLevel = 2,
                modalState = ModalSubState.MetaGame,
                reason = "Reset pressed"
            };
            EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
        }
    }

    /// <summary>
    /// Recommeneded button pressed. All options reset and new ones auto selected based on listOfRecommendations (highest priority on down until run out of renown).
    /// Generates an outcome windown no matter what (no recommendations / insufficient renown / 'x' recommendations made
    /// Sets MetaGameUI display to top 'Selected' tab on complettion
    /// </summary>
    void ExecuteRecommended()
    {
        int cost;
        int count = listOfRecommended.Count;
        int numSelected = 0;
        int totalCost = 0;
        //availableRenown could include bonus renown from taking negative options
        int availableRenown = Mathf.Max(GameManager.i.playerScript.Renown, renownCurrent);
        ModalOutcomeDetails details = null;
        //must be items on recommended list
        if (count > 0)
        {
            //player must have a minimum amount of renown
            if (availableRenown >= renownRecommendMin)
            {
                //start with a clean slate
                ExecuteReset(false);
                //loop through list selecting until available renown expended
                for (int i = 0; i < count; i++)
                {
                    MetaData metaData = listOfRecommended[i];
                    if (metaData != null)
                    {
                        cost = metaData.renownCost;
                        //check affordability
                        if (renownCurrent >= cost)
                        {
                            //Select item
                            metaData.isSelected = true;
                            renownCurrent -= cost;
                            totalCost += cost;
                            numSelected++;
                            AddToSelected(metaData);
                        }
                        //run out of available renown -> exit
                        if (renownCurrent < renownRecommendMin)
                        { break; }
                        if (numSelected >= numOfChoicesMax)
                        { break; }
                    }
                    else { Debug.LogWarningFormat("Invalid metaData (Null) for listOfRecommended[{0}]", i); }
                }
                UpdateRenown();
                //show selected tab
                OpenTopTab((int)MetaTabTop.Selected);
                //Outcome message depends on whether any recommendations have been made
                if (numSelected > 0)
                {
                    //'x' recommendations selected
                    details = new ModalOutcomeDetails()
                    {
                        side = GameManager.i.sideScript.PlayerSide,
                        textTop = string.Format("{0}RECOMMENDED{1}", colourAlert, colourEnd),
                        textBottom = string.Format("{0}{1}{2} recommended option{3} have been automatically selected for a cost of {4}{5}{6} Renown<br><br>There is a {7}{8}{9} option {10}limit{11}",
                        colourNeutral, numSelected, colourEnd, numSelected != 1 ? "s" : "", colourNeutral, totalCost, colourEnd, colourNeutral, numOfChoicesMax, colourEnd, colourNeutral, colourEnd),
                        sprite = GameManager.i.guiScript.alertInformationSprite,
                        modalLevel = 2,
                        modalState = ModalSubState.MetaGame,
                        reason = "Recommended pressed"
                    };
                }
                else
                {
                    //couldn't afford any recommendations
                    details = new ModalOutcomeDetails()
                    {
                        side = GameManager.i.sideScript.PlayerSide,
                        textTop = string.Format("{0}RECOMMENDED{1}", colourAlert, colourEnd),
                        textBottom = string.Format("No recommendations have been selected as you had {0}insufficient renown{1} to pay for them", colourCancel, colourEnd),
                        sprite = GameManager.i.guiScript.alertInformationSprite,
                        modalLevel = 2,
                        modalState = ModalSubState.MetaGame,
                        reason = "Recommended pressed"
                    };
                }
            }
            else
            {
                //Insufficient renown
                details = new ModalOutcomeDetails()
                {
                    side = GameManager.i.sideScript.PlayerSide,
                    textTop = string.Format("{0}RECOMMENDED{1}", colourAlert, colourEnd),
                    textBottom = string.Format("No recommendations have been selected as you had {0}insufficient renown{1} to pay for them", colourCancel, colourEnd),
                    sprite = GameManager.i.guiScript.alertInformationSprite,
                    modalLevel = 2,
                    modalState = ModalSubState.MetaGame,
                    reason = "Recommended pressed"
                };
            }
        }
        else
        {
            //No Recommendation present
            details = new ModalOutcomeDetails()
            {
                side = GameManager.i.sideScript.PlayerSide,
                textTop = string.Format("{0}RECOMMENDED{1}", colourAlert, colourEnd),
                textBottom = string.Format("No recommendations have been selected as there are {0}no critical issues{1} that need your attention", colourCancel, colourEnd),
                sprite = GameManager.i.guiScript.alertInformationSprite,
                modalLevel = 2,
                modalState = ModalSubState.MetaGame,
                reason = "Recommended pressed"
            };
        }
        //outcome message
        if (details != null)
        { EventManager.i.PostNotification(EventType.OutcomeOpen, this, details); }
    }

    /// <summary>
    /// Confirm button pressed
    /// </summary>
    public void ExecuteConfirm()
    {
        //close popUpDynamic
        GameManager.i.popUpDynamicScript.Close();
        if (dictOfSelected.Count > 0)
        {
            int count;
            //data packages
            EffectDataReturn effectReturn = new EffectDataReturn();
            EffectDataInput effectInput = new EffectDataInput();
            StringBuilder builder = new StringBuilder();
            //stop renown coroutine
            GameManager.i.popUpDynamicScript.StopMyCoroutine();
            //
            // - - - Process Effects
            //
            //use Player node (still viable as new level not yet created)
            Node node = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
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
                            effectInput.data = data.Value.data;
                            for (int i = 0; i < count; i++)
                            {
                                Effect effect = data.Value.listOfEffects[i];
                                if (effect != null)
                                {
                                    effectReturn = GameManager.i.effectScript.ProcessEffect(effect, node, effectInput);
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
            GameManager.i.inputScript.SetModalState(modalData);
            //check options have been selected
            if (builder.Length > 0)
            {
                //add renown note
                builder.AppendFormat("{0}{1}{2}{3}{4} Renown will carry over", "\n", "\n", colourNeutral, renownCurrent, colourEnd);
                //confirmation outcome popup
                ModalOutcomeDetails details = new ModalOutcomeDetails()
                {
                    side = GameManager.i.sideScript.PlayerSide,
                    textTop = string.Format("{0}HQ Outcomes{1}", colourCancel, colourEnd),
                    textBottom = builder.ToString(),
                    sprite = GameManager.i.guiScript.infoSprite,
                    modalLevel = 2,
                    modalState = ModalSubState.MetaGame,
                    reason = "Effect Outcomes"
                };
                EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
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
            EventManager.i.PostNotification(EventType.ConfirmOpen, this, details);
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
        GameManager.i.guiScript.waitUntilDone = true;
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
        EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
        //will wait until ModalOutcome -> CloseModalOutcome resets flag
        yield return new WaitUntil(() => GameManager.i.guiScript.waitUntilDone == false);
    }

    /// <summary>
    /// Master coroutine for confirm dialogue
    /// </summary>
    /// <param name="details"></param>
    /// <returns></returns>
    IEnumerator CloseMetaGameConfirm(ModalConfirmDetails details)
    {
        GameManager.i.guiScript.waitUntilDone = true;
        //wait for confirm window to be closed
        yield return CloseConfirm(details);
        //check result -> close and continue or close and revert back to MetaGameUI
        if (GameManager.i.confirmScript.CheckResult() == true)
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
        EventManager.i.PostNotification(EventType.ConfirmOpen, this, details);
        //will wait until ModalConfirm -> True or False button pressed which resets flag
        yield return new WaitUntil(() => GameManager.i.guiScript.waitUntilDone == false);
    }

    //
    // - - - SubMethods
    //

    /// <summary>
    /// Sets all item checkmarks to OFF
    /// </summary>
    private void ResetCheckMarks()
    {
        //set checkmarks to false
        for (int index = 0; index < numOfItemsTotal; index++)
        {
            arrayOfSideMetaCheckMark[index].gameObject.SetActive(false);
            arrayOfTopMetaCheckMark[index].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// subMethod to update renown display with renownCurrent (use this for readability and enhanced code maintenance)
    /// </summary>
    /// <param name="newRenown"></param>
    private void UpdateRenown()
    { renownAmount.text = renownCurrent.ToString(); }

    /// <summary>
    /// subMethod to add a new entry to dictOfSelected
    /// </summary>
    /// <param name="metaData"></param>
    private void AddToSelected(MetaData metaData)
    {
        try
        {
            dictOfSelected.Add(metaData.metaName, metaData);
            numOfChoicesCurrent++;
            Debug.LogFormat("[Met] MetaGameUI.cs -> AddToSelected: metaOption \"{0}\" Selected at a cost of {1} Renown ({2} remaining){3}", metaData.metaName, metaData.renownCost, renownCurrent, "\n");
        }
        catch (ArgumentNullException)
        { Debug.LogError("Invalid metaData (Null)"); }
        catch (ArgumentException)
        { Debug.LogErrorFormat("Invalid metaName (duplicate) in dictOfSelected for \"{0}\"", metaData.metaName); }
    }

    /// <summary>
    /// subMethod to remove an entry from dictOfSelected
    /// </summary>
    /// <param name="metaData"></param>
    private void RemoveFromSelected(MetaData metaData)
    {
        if (dictOfSelected.ContainsKey(metaData.metaName) == true)
        {
            dictOfSelected.Remove(metaData.metaName);
            numOfChoicesCurrent--;
            Debug.LogFormat("[Met] MetaGameUI.cs -> RemoveFromSelected: metaOption \"{0}\" Deselected for +{1} Renown (now {2}){3}", metaData.metaName, metaData.renownCost, renownCurrent, "\n");
        }
        else { Debug.LogWarningFormat("metaData \"{0}\" not found in dictOfSelected (Not removed)", metaData.metaName); }
    }

    //
    // - - - Debug Methods
    //

    /// <summary>
    /// Debug display of dictOfSelected metaData
    /// </summary>
    /// <returns></returns>
    public string DebugDisplaySelected()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- MetaGameUI.cs -> dictOfSelected{0}{1}", "\n", "\n");
        if (GameManager.i.inputScript.GameState == GameState.MetaGame)
        {
            if (GameManager.i.inputScript.ModalMetaState == ModalMetaSubState.PlayerOptions)
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
