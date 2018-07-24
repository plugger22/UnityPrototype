using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using gameAPI;
using packageAPI;
using TMPro;
using System;

/// <summary>
/// handles the main info App
/// </summary>
public class MainInfoUI : MonoBehaviour
{

    public GameObject mainInfoObject;

    [Header("Overall")]
    public Button buttonClose;

    [Header("RHS Miscellanous")]
    public TextMeshProUGUI page_header;

    [Header("RHS backgrounds")]
    //main tab -> parent backgrounds
    public Image main_issue_0;
    public Image main_issue_1;
    public Image main_issue_2;
    public Image main_issue_3;
    public Image main_issue_4;
    public Image main_issue_5;
    public Image main_issue_6;
    public Image main_issue_7;
    public Image main_issue_8;
    public Image main_issue_9;

    [Header("RHS texts")]
    //main tab -> text
    public TextMeshProUGUI main_text_0;
    public TextMeshProUGUI main_text_1;
    public TextMeshProUGUI main_text_2;
    public TextMeshProUGUI main_text_3;
    public TextMeshProUGUI main_text_4;
    public TextMeshProUGUI main_text_5;
    public TextMeshProUGUI main_text_6;
    public TextMeshProUGUI main_text_7;
    public TextMeshProUGUI main_text_8;
    public TextMeshProUGUI main_text_9;

    [Header("RHS borders")]
    //main tab -> border
    public Image main_border_1;
    public Image main_border_2;
    public Image main_border_3;
    public Image main_border_4;
    public Image main_border_5;
    public Image main_border_6;
    public Image main_border_7;
    public Image main_border_8;
    public Image main_border_9;

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

    private ButtonInteraction buttonInteractionClose;


    //hardwired lines in main page -> 10
    private int numOfLines = 10;
    private Image[] mainImageArray;
    private Image[] mainBorderArray;
    private TextMeshProUGUI[] mainTextArray;
    //hardwired tabs at top -> 6
    private int numOfTabs = 6;
    private Image[] tabActiveArray;
    private Image[] tabPassiveArray;
    //data sets (one per tab)
    private Dictionary<int, List<String>> dictOfData;                   //cached data, one entry for each page for current turn
    List<string> listOfCurrentPageData;                                     //current data for currently displayed page

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
        mainImageArray = new Image[numOfLines];
        mainBorderArray = new Image[numOfLines];
        mainTextArray = new TextMeshProUGUI[numOfLines];
        tabActiveArray = new Image[numOfTabs];
        tabPassiveArray = new Image[numOfTabs];
        dictOfData = new Dictionary<int, List<String>>();
        listOfCurrentPageData = new List<string>();
        //close button event
        buttonInteractionClose = buttonClose.GetComponent<ButtonInteraction>();
        if (buttonInteractionClose != null)
        { buttonInteractionClose.SetEvent(EventType.MainInfoClose); }
        else { Debug.LogError("Invalid buttonInteraction Cancel (Null)"); }
        //main background image array
        Debug.Assert(main_issue_0 != null, "Invalid issue_0 (Null)");
        Debug.Assert(main_issue_1 != null, "Invalid issue_1 (Null)");
        Debug.Assert(main_issue_2 != null, "Invalid issue_2 (Null)");
        Debug.Assert(main_issue_3 != null, "Invalid issue_3 (Null)");
        Debug.Assert(main_issue_4 != null, "Invalid issue_4 (Null)");
        Debug.Assert(main_issue_5 != null, "Invalid issue_5 (Null)");
        Debug.Assert(main_issue_6 != null, "Invalid issue_6 (Null)");
        Debug.Assert(main_issue_7 != null, "Invalid issue_7 (Null)");
        Debug.Assert(main_issue_8 != null, "Invalid issue_8 (Null)");
        Debug.Assert(main_issue_9 != null, "Invalid issue_9 (Null)");
        mainImageArray[0] = main_issue_0;
        mainImageArray[1] = main_issue_1;
        mainImageArray[2] = main_issue_2;
        mainImageArray[3] = main_issue_3;
        mainImageArray[4] = main_issue_4;
        mainImageArray[5] = main_issue_5;
        mainImageArray[6] = main_issue_6;
        mainImageArray[7] = main_issue_7;
        mainImageArray[8] = main_issue_8;
        mainImageArray[9] = main_issue_9;
        //main border image array
        Debug.Assert(main_border_1 != null, "Invalid main_border_1 (Null)");
        Debug.Assert(main_border_2 != null, "Invalid main_border_2 (Null)");
        Debug.Assert(main_border_3 != null, "Invalid main_border_3 (Null)");
        Debug.Assert(main_border_4 != null, "Invalid main_border_4 (Null)");
        Debug.Assert(main_border_5 != null, "Invalid main_border_5 (Null)");
        Debug.Assert(main_border_6 != null, "Invalid main_border_6 (Null)");
        Debug.Assert(main_border_7 != null, "Invalid main_border_7 (Null)");
        Debug.Assert(main_border_8 != null, "Invalid main_border_8 (Null)");
        Debug.Assert(main_border_9 != null, "Invalid main_border_9 (Null)");
        mainBorderArray[1] = main_border_1;
        mainBorderArray[2] = main_border_2;
        mainBorderArray[3] = main_border_3;
        mainBorderArray[4] = main_border_4;
        mainBorderArray[5] = main_border_5;
        mainBorderArray[6] = main_border_6;
        mainBorderArray[7] = main_border_7;
        mainBorderArray[8] = main_border_8;
        mainBorderArray[9] = main_border_9;
        //main text array
        Debug.Assert(main_text_0 != null, "Invalid text_0 (Null)");
        Debug.Assert(main_text_1 != null, "Invalid text_1 (Null)");
        Debug.Assert(main_text_2 != null, "Invalid text_2 (Null)");
        Debug.Assert(main_text_3 != null, "Invalid text_3 (Null)");
        Debug.Assert(main_text_4 != null, "Invalid text_4 (Null)");
        Debug.Assert(main_text_5 != null, "Invalid text_5 (Null)");
        Debug.Assert(main_text_6 != null, "Invalid text_6 (Null)");
        Debug.Assert(main_text_7 != null, "Invalid text_7 (Null)");
        Debug.Assert(main_text_8 != null, "Invalid text_8 (Null)");
        Debug.Assert(main_text_9 != null, "Invalid text_9 (Null)");
        mainTextArray[0] = main_text_0;
        mainTextArray[1] = main_text_1;
        mainTextArray[2] = main_text_2;
        mainTextArray[3] = main_text_3;
        mainTextArray[4] = main_text_4;
        mainTextArray[5] = main_text_5;
        mainTextArray[6] = main_text_6;
        mainTextArray[7] = main_text_7;
        mainTextArray[8] = main_text_8;
        mainTextArray[9] = main_text_9;
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
        EventManager.instance.AddListener(EventType.MainInfoOpen, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoClose, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoTabOpen, OnEvent, "MainInfoUI");
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
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
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

            //
            // - - - Populate data
            //
            UpdateData(data);
            //
            // - - - Display Main page by default
            //
            OpenTab(0);
            //
            // - - - GUI - - -
            //
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
            int numOfItems = listOfCurrentPageData.Count;
            if (numOfItems > 0)
            {
                //populate current messages for the main tab
                for (int index = 0; index < mainTextArray.Length; index++)
                {
                    if (index < numOfItems)
                    {
                        mainImageArray[index].gameObject.SetActive(true);
                        mainTextArray[index].text = listOfCurrentPageData[index];
                        if (index > 0)
                        { mainBorderArray[index].gameObject.SetActive(true); }
                    }
                    else
                    {
                        mainTextArray[index].text = "";
                        mainImageArray[index].gameObject.SetActive(false);
                        if (index > 0)
                        { mainBorderArray[index].gameObject.SetActive(false); }
                    }
                }
                //set header
                page_header.text = string.Format("Day {0}, 2033, there {1} {2} item{3}", turn,  numOfItems != 1 ? "are" : "is", numOfItems, numOfItems != 1 ? "s" : "");
            }
            else
            {
                //no data, blank all items, disable line
                    for (int index = 0; index < mainTextArray.Length; index++)
                    {
                        mainTextArray[index].text = "";
                        mainImageArray[index].gameObject.SetActive(false);
                        if (index > 0)
                        { mainBorderArray[index].gameObject.SetActive(false); }
                    }
                //set header
                page_header.text = string.Format("Day {0}, 2033, there are 0 items", turn);
            }
        }
        else { Debug.LogWarning("Invalid MainInofData.listOfMainText (Null)"); }
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
            //redrawn main page
            DisplayPage(tabIndex);
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
