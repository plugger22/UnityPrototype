using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
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

    [Header("HQ Tabs")]
    public GameObject tabBoss;
    public GameObject tabSubBoss1;
    public GameObject tabSubBoss2;
    public GameObject tabSubBoss3;

    [Header("LHS Miscellanous")]
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



    private int numOfTabs = 4;    //NOTE: Change this at your peril (default 4 which is number of HQ actors) as data collections and indexes all flow from it
    private int offset = 1;       //used with '(ActorHQ)index + offset' to account for the ActorHQ.enum having index 0 being 'None'

    //button script handlers
    private ButtonInteraction buttonInteractionReset;
    private ButtonInteraction buttonInteractionConfirm;
    private ButtonInteraction buttonInteractionRecommended;

    //arrays
    private GameObject[] tabObjects;
    private MetaInteraction[] tabItems;
    private MetaHqTabUI[] tabInteractions;

    //metaItems collections
    private GameObject[] arrayMetaMain;
    private TextMeshProUGUI[] arrayMetaText;
    private Image[] arrayMetaIcon;
    private Image[] arrayMetaBorder;
    private Image[] arrayMetaBackground;
    //item priority sprites
    private Sprite priorityHigh;
    private Sprite priorityMedium;
    private Sprite priorityLow;

    //ItemData
    private List<MetaData>[] arrayOfMetaData = new List<MetaData>[(int)MetaTab.Count];       //One dataset for each tab (excluding Help tab)
    List<MetaData> listOfCurrentPageMetaData;                                                //current data for currently displayed page

    //scroll bar LHS
    private ScrollRect scrollRect;                                   //needed to manually disable scrolling when not needed
    private Scrollbar scrollBar;

    private bool isRunning;
    private int highlightIndex = -1;                                 //item index of currently highlighted item
    private int maxHighlightIndex = -1;
    private int numOfItemsTotal = 20;                                //hardwired Max number of items -> 30
    private int numOfVisibleItems = 10;                              //hardwired visible items in main page -> 10
    private int numOfItemsCurrent = -1;                              //count of items in current list / page
    private int numOfItemsPrevious = -1;                             //count of items in previous list / page
    //tabs
    private int currentTabIndex = -1;
    private int maxTabIndex;


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
        //initialise Arrays -> tabs
        tabObjects = new GameObject[numOfTabs];
        tabItems = new MetaInteraction[numOfTabs];
        tabInteractions = new MetaHqTabUI[numOfTabs];
        //initialise Arrays -> items
        arrayMetaMain = new GameObject[numOfItemsTotal];
        arrayMetaIcon = new Image[numOfItemsTotal];
        arrayMetaBorder = new Image[numOfItemsTotal];
        arrayMetaBackground = new Image[numOfItemsTotal];
        arrayMetaText = new TextMeshProUGUI[numOfItemsTotal];
        //other collections
        listOfCurrentPageMetaData = new List<MetaData>();
        //canvas
        canvasScroll.gameObject.SetActive(true);
        //max tabs
        maxTabIndex = numOfTabs - 1;
        //tab components
        if (tabBoss != null)  { tabObjects[index++] = tabBoss; }  else { Debug.LogError("Invalid tabBoss (Null)"); }
        if (tabSubBoss1 != null)  { tabObjects[index++] = tabSubBoss1; }  else { Debug.LogError("Invalid tabSubBoss1 (Null)"); }
        if (tabSubBoss2 != null)  { tabObjects[index++] = tabSubBoss2; }  else { Debug.LogError("Invalid tabSubBoss2 (Null)"); }
        if (tabSubBoss3 != null)  { tabObjects[index++] = tabSubBoss3; }  else { Debug.LogError("Invalid tabSubBoss3 (Null)"); }
        //initialise tab Interaction array
        for (int i = 0; i < tabObjects.Length; i++)
        {
            if (tabObjects[i] != null)
            {
                MetaInteraction metaInteract = tabObjects[i].GetComponent<MetaInteraction>();
                if (metaInteract != null)
                {
                    tabItems[i] = metaInteract;
                    //tab interaction
                    MetaHqTabUI hqTab = metaInteract.background.GetComponent<MetaHqTabUI>();
                    if (hqTab != null)
                    {
                        tabInteractions[i] = hqTab;
                        //identify tabs
                        hqTab.SetTabIndex(i);
                    }
                    else { Debug.LogErrorFormat("Invalid MetaHqTabUI (Null) for tabItems[{0}]", i); }
                }
                else { Debug.LogErrorFormat("Invalid MetaInteraction (Null) for tabObject[{0}]", i); }
            }
        }
        //main
        Debug.Assert(canvasMeta != null, "Invalid canvasMeta (Null)");
        Debug.Assert(canvasScroll != null, "Invalid canvasScroll (Null)");
        Debug.Assert(metaObject != null, "Invalid metaObject (Null)");
        //buttons
        buttonInteractionReset = buttonReset.GetComponent<ButtonInteraction>();
        buttonInteractionConfirm = buttonConfirm.GetComponent<ButtonInteraction>();
        buttonInteractionRecommended = buttonRecommended.GetComponent<ButtonInteraction>();
        Debug.Assert(buttonInteractionReset != null, "Invalid buttonInteractionReset (Null)");
        Debug.Assert(buttonInteractionConfirm != null, "Invalid buttonInteractionConfirm (Null)");
        Debug.Assert(buttonInteractionRecommended != null, "Invalid buttonInteractionRecommended (Null)");
        buttonInteractionConfirm.SetButton(EventType.MetaGameClose);
        //scrollRect & ScrollBar
        Debug.Assert(scrollBackground != null, "Invalid scrollBackground (Null)");
        Debug.Assert(scrollBarObject != null, "Invalid scrollBarObject (Null)");
        scrollRect = scrollBackground.GetComponent<ScrollRect>();
        scrollBar = scrollBarObject.GetComponent<Scrollbar>();
        Debug.Assert(scrollRect != null, "Invalid scrollRect (Null)");
        Debug.Assert(scrollBar != null, "Invalid scrollBar (Null)");
        //RHS
        Debug.Assert(rightImage != null, "Invalid rightImage (Null)");
        Debug.Assert(rightImageDefault != null, "Invalid rightImageDefault (Null)");
        Debug.Assert(rightTextTop != null, "Invalid rightTextTop (Null)");
        Debug.Assert(rightTextBottom != null, "Invalid rightTextBottom (Null)");
        //backgrounds
        Debug.Assert(backgroundMain != null, "Invalid backgroundMain (Null)");
        Debug.Assert(backgroundLeft != null, "Invalid backgroundLeft (Null)");
        Debug.Assert(backgroundCentre != null, "Invalid backgroundCentre (Null)");
        Debug.Assert(backgroundRight != null, "Invalid backgroundRight (Null)");
        //assign backgrounds and active tab colours
        Color colour = GameManager.instance.guiScript.colourMainBackground;
        /*backgroundMain.color = new Color(colour.r, colour.g, colour.b, 0.35f);*/
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
        //assign items
        arrayMetaMain[0] = meta_item_0;
        arrayMetaMain[1] = meta_item_1;
        arrayMetaMain[2] = meta_item_2;
        arrayMetaMain[3] = meta_item_3;
        arrayMetaMain[4] = meta_item_4;
        arrayMetaMain[5] = meta_item_5;
        arrayMetaMain[6] = meta_item_6;
        arrayMetaMain[7] = meta_item_7;
        arrayMetaMain[8] = meta_item_8;
        arrayMetaMain[9] = meta_item_9;
        arrayMetaMain[10] = meta_item_10;
        arrayMetaMain[11] = meta_item_11;
        arrayMetaMain[12] = meta_item_12;
        arrayMetaMain[13] = meta_item_13;
        arrayMetaMain[14] = meta_item_14;
        arrayMetaMain[15] = meta_item_15;
        arrayMetaMain[16] = meta_item_16;
        arrayMetaMain[17] = meta_item_17;
        arrayMetaMain[18] = meta_item_18;
        arrayMetaMain[19] = meta_item_19;
        //initialise metaItems & populate arrays
        for (index = 0; index < arrayMetaMain.Length; index++)
        {
            GameObject metaObject = arrayMetaMain[index];
            if (metaObject != null)
            {
                //get child components -> Image
                var childrenImage = metaObject.GetComponentsInChildren<Image>();
                foreach (var child in childrenImage)
                {
                    if (child.name.Equals("background", StringComparison.Ordinal) == true)
                    {
                        arrayMetaBackground[index] = child;
                        //attached interaction script
                        MainInfoRightItemUI itemScript = child.GetComponent<MainInfoRightItemUI>();
                        if (itemScript != null)
                        {
                            itemScript.SetItemIndex(index, numOfItemsTotal);
                            itemScript.SetUIType(MajorUI.MetaGameUI);
                        }
                        else { Debug.LogWarningFormat("Invalid MainInfoRightItemUI component (Null) for arrayMetaMain[{0}]", index); }
                    }
                    else if (child.name.Equals("icon", StringComparison.Ordinal) == true)
                    { arrayMetaIcon[index] = child; }
                    else if (child.name.Equals("border", StringComparison.Ordinal) == true)
                    { arrayMetaBorder[index] = child; }
                }
                //child components -> Text
                var childrenText = metaObject.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var child in childrenText)
                {
                    if (child.name.Equals("text", StringComparison.Ordinal) == true)
                    {
                        TextMeshProUGUI metaText = child.GetComponent<TextMeshProUGUI>();
                        if (metaText != null)
                        { arrayMetaText[index] = metaText; }
                        else { Debug.LogWarningFormat("Invalid TextMeshProUGUI component (Null) for arrayMetaMain[{0}]", index); }
                    }
                }
            }
            else { Debug.LogWarningFormat("Invalid GameObject (Null) for mainItemArray[{0}]", index); }
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
        Debug.Assert(priorityHigh != null, "Invalid priorityHigh (Null)");
        Debug.Assert(priorityMedium != null, "Invalid priorityMedium (Null)");
        Debug.Assert(priorityLow != null, "Invalid priorityLow (Null)");
    }
    #endregion

    #region SubInitialiseEvents
    /// <summary>
    /// events
    /// </summary>
    private void SubInitialiseEvents()
    {
        //listeners
        EventManager.instance.AddListener(EventType.MetaGameClose, OnEvent, "MetaGamesUI");
        EventManager.instance.AddListener(EventType.MetaGameTabOpen, OnEvent, "MetaGamesUI");
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

            case EventType.MetaGameClose:
                CloseMetaUI();
                break;
            case EventType.MetaGameTabOpen:
                OpenTab((int)Param);
                break;
            case EventType.MetaGameOpen:
                MetaInfoData data = Param as MetaInfoData;
                SetMetaUI(data);
                break;
            /*case EventType.MetaGameShowDetails:
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
                break;*/
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// run prior to every metaGameUI use. Run from SetMetaUI
    /// </summary>
    private void InitialiseMetaUI()
    {
        //initialise HQ tabs'
        Color portraitColor, backgroundColor;
        for (int i = 0; i < tabItems.Length; i++)
        {
            backgroundColor = tabItems[i].background.color;
            if (tabItems[i] != null)
            {
                //sprite
                Actor actor = GameManager.instance.dataScript.GetHQHierarchyActor((ActorHQ)(i + offset));
                if (actor != null)
                {
                    tabItems[i].portrait.sprite = actor.sprite;
                }
                else
                {
                    //default error sprite if a problem
                    Debug.LogWarningFormat("Invalid actor (Null) for ActorHQ \"{0}\"", (ActorHQ)(i + offset));
                    tabItems[i].portrait.sprite = GameManager.instance.guiScript.errorSprite;
                }
                portraitColor = tabItems[i].portrait.color;
                //title
                tabItems[i].title.text = GameManager.instance.hqScript.GetHqTitle(actor.statusHQ);
                //first tab (Boss) should be active on opening, rest passive
                if (i == 0)
                { portraitColor.a = 1.0f; backgroundColor.a = 1.0f; }
                else
                { portraitColor.a = 0.25f; backgroundColor.a = 0.25f; }
                //set colors
                tabItems[i].portrait.color = portraitColor;
                tabItems[i].background.color = backgroundColor;
            }
            else { Debug.LogErrorFormat("Invalid tabItems[{0}] (Null)", i); }
            
        }
    }


    private void InitialiseItems()
    {
        for (int index = 0; index < numOfItemsTotal; index++)
        {
            //main game objects off
            arrayMetaMain[index].SetActive(false);
            //all other child objects on
            arrayMetaIcon[index].gameObject.SetActive(true);
            arrayMetaText[index].gameObject.SetActive(true);
            arrayMetaBorder[index].gameObject.SetActive(true);
            arrayMetaBackground[index].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Display MetaGame Player options UI
    /// </summary>
    public void SetMetaUI(MetaInfoData data)
    {
        if (data != null)
        {
            InitialiseMetaUI();
            canvasMeta.gameObject.SetActive(true);
            // Display Boss page by default
            OpenTab(0);
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
        //show top bar UI at completion of meta game
        EventManager.instance.PostNotification(EventType.TopBarShow, this, null, "MetaGameUI.cs -> Show TopBarUI");
    }


    /// <summary>
    /// Open the designated tab and close whatever is open
    /// </summary>
    /// <param name="tabIndex"></param>
    private void OpenTab(int tabIndex)
    {
        Debug.Assert(tabIndex > -1 && tabIndex < numOfTabs, string.Format("Invalid tab index {0}", tabIndex));
        //reset Active tabs to reflect new status
        Color portraitColor, backgroundColor;
        for (int index = 0; index < tabItems.Length; index++)
        {
            portraitColor = tabItems[index].portrait.color;
            backgroundColor = tabItems[index].background.color;
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
            tabItems[index].portrait.color = portraitColor;
            tabItems[index].background.color = backgroundColor;
        }
        /*//reset scrollbar
        scrollRect.verticalNormalizedPosition = 1.0f;
        //redrawn main page
        DisplayItemPage(tabIndex);*/

        //assign default info icon
        rightImage.sprite = rightImageDefault;
        //redrawn main page
        DisplayItemPage(tabIndex);
        //update indexes
        highlightIndex = -1;
        currentTabIndex = tabIndex;
    }


    /// <summary>
    /// sub Method to display a particular page drawing from cached data in dictOfItemData
    /// </summary>
    /// <param name="tab"></param>
    private void DisplayItemPage(int tabIndex)
    {
        Debug.Assert(tabIndex > -1 && tabIndex < (int)ItemTab.Count, string.Format("Invalid tabIndex {0}", tabIndex));
        //clear out current data
        listOfCurrentPageMetaData.Clear();
        //get data
        listOfCurrentPageMetaData.AddRange(arrayOfMetaData[tabIndex]);
        //display routine
        if (listOfCurrentPageMetaData != null)
        {
            numOfItemsCurrent = listOfCurrentPageMetaData.Count;
            maxHighlightIndex = numOfItemsCurrent - 1;
            if (numOfItemsCurrent > 0)
            {
                /*//update max number of items
                numOfMaxItem = numOfItemsCurrent;*/

                //populate current messages for the main tab
                for (int index = 0; index < arrayMetaText.Length; index++)
                {
                    if (index < numOfItemsCurrent)
                    {
                        //populate text and set item to active
                        arrayMetaText[index].text = listOfCurrentPageMetaData[index].itemText;
                        arrayMetaMain[index].gameObject.SetActive(true);
                        //assign icon
                        switch (listOfCurrentPageMetaData[index].priority)
                        {
                            case MetaPriority.Extreme:
                            case MetaPriority.High:
                                arrayMetaIcon[index].sprite = priorityHigh;
                                break;
                            case MetaPriority.Medium:
                                arrayMetaIcon[index].sprite = priorityMedium;
                                break;
                            case MetaPriority.Low:
                                arrayMetaIcon[index].sprite = priorityLow;
                                break;
                            default:
                                Debug.LogWarningFormat("Invalid priority \"{0}\"", listOfCurrentPageMetaData[index].priority);
                                break;
                        }

                    }
                    else if (index < numOfItemsPrevious)
                    {
                        //efficient -> only disables items that were previously active, not the whole set
                        arrayMetaText[index].text = "";
                        arrayMetaMain[index].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //no data, blank previous items (not necessarily all), disable line
                for (int index = 0; index < numOfItemsPrevious; index++)
                {
                    arrayMetaText[index].text = "";
                    arrayMetaMain[index].gameObject.SetActive(false);
                }

            }
            //update previous count to current
            numOfItemsPrevious = numOfItemsCurrent;
            
            /*//set header
            SetPageHeader(numOfItemsCurrent);*/
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


    //new methods above here
}
