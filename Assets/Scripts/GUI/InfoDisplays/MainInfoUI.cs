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

    public Button buttonClose;

    [Header("Main Tab")]
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

    private ButtonInteraction buttonInteractionClose;

    private Image[] mainImageArray = new Image[10];
    private Image[] mainBorderArray = new Image[10];
    private TextMeshProUGUI[] mainTextArray = new TextMeshProUGUI[10];

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
    }

    public void Start()
    {
        //event listener
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoOpen, OnEvent, "MainInfoUI");
        EventManager.instance.AddListener(EventType.MainInfoClose, OnEvent, "MainInfoUI");
    }

    public void Initialise()
    { }

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
            
            if (data.listOfMainText != null)
            {
                int numOfMessages = data.listOfMainText.Count;
                if (numOfMessages > 0)
                {
                    //populate current messages for the main tab
                    for (int index = 0; index < mainTextArray.Length; index++)
                    {
                        if (index < numOfMessages)
                        {
                            mainImageArray[index].gameObject.SetActive(true);
                            mainTextArray[index].text = data.listOfMainText[index];
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
                }
                else
                {
                    //no data, blank all items, disable line
                    if (data.listOfMainText.Count > 0)
                    {
                        for (int index = 0; index < mainTextArray.Length; index++)
                        {
                            mainTextArray[index].text = "";
                            mainImageArray[index].gameObject.SetActive(false);
                            if (index > 0)
                            { mainBorderArray[index].gameObject.SetActive(false); }
                        }
                    }
                }
            }
            else { Debug.LogWarning("Invalid MainInofData.listOfMainText (Null)"); }
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
