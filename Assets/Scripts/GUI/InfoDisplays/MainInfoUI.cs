using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles the main info App
/// </summary>
public class MainInfoUI : MonoBehaviour
{

    public GameObject mainInfoObject;

    [Header("Buttons")]
    public Button buttonClose;
    public Button buttonInfo;
    public Button buttonHome;
    public Button buttonBack;
    public Button buttonForward;

    [Header("RHS Miscellanous")]
    public TextMeshProUGUI page_header;
    public GameObject scrollBarObject;
    public GameObject scrollBackground;         //needed to get scrollRect component in order to manually disable scrolling when not needed

    [Header("RHS Items")]
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

    [Header("RHS Active tabs")]
    //page tabs -> active
    public Image tab_active_0;
    public Image tab_active_1;
    public Image tab_active_2;
    public Image tab_active_3;
    public Image tab_active_4;
    public Image tab_active_5;

    [Header("RHS Passive tabs")]
    //page tabs -> passive
    public Image tab_passive_0;
    public Image tab_passive_1;
    public Image tab_passive_2;
    public Image tab_passive_3;
    public Image tab_passive_4;
    public Image tab_passive_5;

    [Header("LHS details")]
    public TextMeshProUGUI details_text_top;
    public TextMeshProUGUI details_text_bottom;
    public Image details_image;

    //button script handlers
    private ButtonInteraction buttonInteractionClose;
    private ButtonInteraction buttonInteractionHome;
    private ButtonInteraction buttonInteractionBack;
    private ButtonInteraction buttonInteractionForward;


    private int highlightIndex = -1;                                 //item index of currently highlighted item
    
    private int numOfItemsTotal = 20;                                //hardwired Max number of items -> 20
    private int numOfVisibleItems = 10;                              //hardwired visible items in main page -> 10
    private int numOfItemsCurrent = -1;                              //count of items in current list / page
    private int numOfItemsPrevious = -1;                             //count of items in previous list / page
    private int numOfMaxItem = -1;                                   // (max) count of items in current list / page
    private GameObject[] arrayItemMain;
    private Image[] arrayItemIcon;
    private Image[] arrayItemBorder;
    private Image[] arrayItemBackground;
    private TextMeshProUGUI[] arrayItemText;
    private ScrollRect scrollRect;                                  //needed to manually disable scrolling when not needed
    private Scrollbar scrollBar;
    //hardwired tabs at top -> 6
    private int numOfTabs = 6;
    private Image[] tabActiveArray;
    private Image[] tabPassiveArray;
    //data sets (one per tab)
    private Dictionary<int, List<String>> dictOfData;                   //cached data, one entry for each page for current turn
    List<string> listOfCurrentPageData;                                     //current data for currently displayed page

    //colours
    string colourDefault;
    string colourHighlight;
    /*string colourNormal;
    string colourAlert;
    string colourHighlight;
    string colourResistance;
    string colourBad;
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

    private void Awake()
    {
        //collections
        arrayItemMain = new GameObject[numOfItemsTotal];
        arrayItemIcon = new Image[numOfItemsTotal];
        arrayItemBorder = new Image[numOfItemsTotal];
        arrayItemBackground = new Image[numOfItemsTotal];
        arrayItemText = new TextMeshProUGUI[numOfItemsTotal];
        tabActiveArray = new Image[numOfTabs];
        tabPassiveArray = new Image[numOfTabs];
        dictOfData = new Dictionary<int, List<String>>();
        listOfCurrentPageData = new List<string>();
        //buttons
        Debug.Assert(buttonClose != null, "Invalid buttonClose (Null)");
        Debug.Assert(buttonInfo != null, "Invalid buttonInfo (Null)");
        Debug.Assert(buttonHome != null, "Invalid buttonHome (Null)");
        Debug.Assert(buttonBack != null, "Invalid buttonBack (Null)");
        Debug.Assert(buttonForward != null, "Invalid buttonForward (Null)");
        //set button interaction events
        buttonInteractionClose = buttonClose.GetComponent<ButtonInteraction>();
        buttonInteractionHome = buttonHome.GetComponent<ButtonInteraction>();
        buttonInteractionBack = buttonBack.GetComponent<ButtonInteraction>();
        buttonInteractionForward = buttonForward.GetComponent<ButtonInteraction>();
        Debug.Assert(buttonInteractionClose != null, "Invalid buttonInteractionClose (Null)");
        Debug.Assert(buttonInteractionHome != null, "Invalid buttonInteractionHome (Null)");
        Debug.Assert(buttonInteractionBack != null, "Invalid buttonInteractionBack (Null)");
        Debug.Assert(buttonInteractionForward != null, "Invalid buttonInteractionForward (Null)");
        buttonInteractionClose.SetEvent(EventType.MainInfoClose);
        buttonInteractionHome.SetEvent(EventType.MainInfoHome);
        buttonInteractionBack.SetEvent(EventType.MainInfoBack);
        buttonInteractionForward.SetEvent(EventType.MainInfoForward);
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
        Debug.Assert(main_item_10 != null, "Invalid item_0 (Null)");
        Debug.Assert(main_item_11 != null, "Invalid item_1 (Null)");
        Debug.Assert(main_item_12 != null, "Invalid item_2 (Null)");
        Debug.Assert(main_item_13 != null, "Invalid item_3 (Null)");
        Debug.Assert(main_item_14 != null, "Invalid item_4 (Null)");
        Debug.Assert(main_item_15 != null, "Invalid item_5 (Null)");
        Debug.Assert(main_item_16 != null, "Invalid item_6 (Null)");
        Debug.Assert(main_item_17 != null, "Invalid item_7 (Null)");
        Debug.Assert(main_item_18 != null, "Invalid item_8 (Null)");
        Debug.Assert(main_item_19 != null, "Invalid item_9 (Null)");
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
    }

    public void Start()
    {
        //event listener
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoOpen, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoClose, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoTabOpen, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoShowDetails, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoHome, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoBack, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoForward, OnEvent, "MainInfoUI");
    }

    public void Initialise()
    {
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
                    if (child.name.Equals("background") == true)
                    {
                        arrayItemBackground[index] = child;
                        //attached interaction script
                        MainInfoRightItemUI itemScript = child.GetComponent<MainInfoRightItemUI>();
                        if (itemScript != null)
                        { itemScript.SetItemIndex(index); }
                        else { Debug.LogWarningFormat("Invalid MainInfoRightItemUI component (Null) for mainItemArray[{0}]", index); }
                    }
                    else if (child.name.Equals("icon") == true)
                    { arrayItemIcon[index] = child; }
                    else if (child.name.Equals("border") == true)
                    { arrayItemBorder[index] = child; }
                }
                //child components -> Text
                var childrenText = itemObject.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var child in childrenText)
                {
                    if (child.name.Equals("text") == true)
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
        //Set starting Initialisation states
        InitialiseItems();
    }

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
            case EventType.MainInfoClose:
                CloseMainInfo();
                break;
            case EventType.MainInfoTabOpen:
                OpenTab((int)Param);
                break;
            case EventType.MainInfoShowDetails:
                ShowDetails((int)Param);
                break;
            case EventType.MainInfoHome:

                break;
            case EventType.MainInfoBack:

                break;
            case EventType.MainInfoForward:

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
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourHighlight = GameManager.instance.colourScript.GetColour(ColourType.actionEffect);
        /*colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourHighlight = GameManager.instance.colourScript.GetColour(ColourType.nodeActive);
        colourResistance = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourError = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.cancelHighlight);
        colourCancel = GameManager.instance.colourScript.GetColour(ColourType.cancelNormal);*/
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }


    /// <summary>
    /// Open Main Info display
    /// </summary>
    private void SetMainInfo(MainInfoData data)
    {
        if (data != null)
        {
            //exit any generic or node tooltips
            GameManager.instance.tooltipGenericScript.CloseTooltip("MainInfoUI.cs -> SetMainInfo");
            GameManager.instance.tooltipNodeScript.CloseTooltip("MainInfoUI.cs -> SetMainInfo");
            //close any Alert Message
            GameManager.instance.alertScript.CloseAlertUI(true);
            // Populate data
            UpdateData(data);
            // Display Main page by default
            OpenTab(0);
            // GUI
            mainInfoObject.SetActive(true);
            //set modal status
            GameManager.instance.guiScript.SetIsBlocked(true);
            //set game state
            ModalStateData package = new ModalStateData();
            package.mainState = ModalState.InfoDisplay;
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
        //clear out dictionary
        dictOfData.Clear();
        //populate new data (excludes help)
        if (data.listOfData_0 != null)
        { dictOfData.Add(0, data.listOfData_0); }
        else { Debug.LogWarning("Invaid data.listOfData_0 (Null)"); }
        if (data.listOfData_1 != null)
        { dictOfData.Add(1, data.listOfData_1); }
        else { Debug.LogWarning("Invaid data.listOfData_1 (Null)"); }
        if (data.listOfData_2 != null)
        { dictOfData.Add(2, data.listOfData_2); }
        else { Debug.LogWarning("Invaid data.listOfData_2 (Null)"); }
        if (data.listOfData_3 != null)
        { dictOfData.Add(3, data.listOfData_3); }
        else { Debug.LogWarning("Invaid data.listOfData_3 (Null)"); }
        if (data.listOfData_4 != null)
        { dictOfData.Add(4, data.listOfData_4); }
        else { Debug.LogWarning("Invaid data.listOfData_4 (Null)"); }
    }


    /// <summary>
    /// sub Method to display a particular page drawing from cached data in dictOfData
    /// </summary>
    /// <param name="tab"></param>
    private void DisplayPage(int tabIndex)
    {
        int turn = GameManager.instance.turnScript.Turn;
        //clear out current dat
        listOfCurrentPageData.Clear();
        //get data
        if (dictOfData.ContainsKey(tabIndex) == true)
        { listOfCurrentPageData.AddRange(dictOfData[tabIndex]); }
        //display routine
        if (listOfCurrentPageData != null)
        {
            numOfItemsCurrent = listOfCurrentPageData.Count;
            if (numOfItemsCurrent > 0)
            {
                //update max number of items
                numOfMaxItem = numOfItemsCurrent;
                //populate current messages for the main tab
                for (int index = 0; index < arrayItemText.Length; index++)
                {
                    if (index < numOfItemsCurrent)
                    {
                        //populate text and set item to active
                        arrayItemText[index].text = listOfCurrentPageData[index];
                        arrayItemMain[index].gameObject.SetActive(true);
                    }
                    else if (index < numOfItemsPrevious)
                    {
                        //efficient -> only disables items that were previously active, not the whole set
                        arrayItemText[index].text = "";
                        arrayItemMain[index].gameObject.SetActive(false);
                    }
                }
                //set header
                page_header.text = string.Format("Day {0}, 2033, there {1} {2} item{3}", turn,  numOfItemsCurrent != 1 ? "are" : "is", numOfItemsCurrent, numOfItemsCurrent != 1 ? "s" : "");
            }
            else
            {
                //no data, blank previous items (not necessarily all), disable line
                    for (int index = 0; index < numOfItemsPrevious; index++)
                    {
                        arrayItemText[index].text = "";
                        arrayItemMain[index].gameObject.SetActive(false);
                    }
                //set header
                page_header.text = string.Format("Day {0}, 2033, there are 0 items", turn);
            }
            //update previous count to current
            numOfItemsPrevious = numOfItemsCurrent;
        }
        else { Debug.LogWarning("Invalid MainInofData.listOfMainText (Null)"); }
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
    /// close Main Info display
    /// </summary>
    private void CloseMainInfo()
    {
        GameManager.instance.tooltipGenericScript.CloseTooltip("MainInfoUI.cs -> CloseMainInfo");
        mainInfoObject.SetActive(false);
        GameManager.instance.guiScript.SetIsBlocked(false);
        //set game state
        GameManager.instance.inputScript.ResetStates();
        Debug.LogFormat("[UI] MainInfoUI.cs -> CloseMainInfo{0}", "\n");
    }


    /// <summary>
    /// Open the designated tab and close whatever is open
    /// </summary>
    /// <param name="tabIndex"></param>
    private void OpenTab(int tabIndex)
    {
        //reset Active tabs to reflect new status
        for (int index = 0; index < tabActiveArray.Length; index++)
        {
            //activate indicated tab and deactivate the rest
            if (index == tabIndex)
            { tabActiveArray[index].gameObject.SetActive(true); }
            else  { tabActiveArray[index].gameObject.SetActive(false); }
            //blank LHS
            details_text_top.text = "";
            details_text_bottom.text = "";
        }
        //redrawn main page
        DisplayPage(tabIndex);
        //reset highlightIndex to default
        highlightIndex = -1;
    }

    /// <summary>
    /// called by clicking on an item on RHS which will then show details of the item on the LHS
    /// </summary>
    /// <param name="itemIndex"></param>
    private void ShowDetails(int itemIndex)
    {
        //debug
        int rnd = UnityEngine.Random.Range(0, 1000);
        details_text_top.text = string.Format("itemIndex {0}, Random {1}", itemIndex, rnd);
        //remove highlight
        if (highlightIndex != itemIndex)
        {
            //reset currently highlighted back to default
            if (highlightIndex > -1)
            { arrayItemText[highlightIndex].text = string.Format("{0}{1}{2}", colourDefault, listOfCurrentPageData[itemIndex], colourEnd); }
            highlightIndex = itemIndex;
            //highlight item -> show as yellow
            arrayItemText[itemIndex].text = string.Format("{0}<b>{1}</b>{2}", colourHighlight, listOfCurrentPageData[itemIndex], colourEnd);
        }
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

    //new methods above here
}
