using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles the main info App
/// </summary>
public class MainInfoUI : MonoBehaviour
{

    public GameObject mainInfoObject;

    public Button buttonClose;


    private ButtonInteraction buttonInteractionClose;

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
                SetMainInfo();
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
    /// Open Main Info display
    /// </summary>
    private void SetMainInfo()
    {
        //exit any generic or node tooltips
        GameManager.instance.tooltipGenericScript.CloseTooltip("MainInfoUI.cs -> SetMainInfo");
        GameManager.instance.tooltipNodeScript.CloseTooltip("MainInfoUI.cs -> SetMainInfo");
        //close any Alert Message
        GameManager.instance.alertScript.CloseAlertUI(true);

        //populate data

        //activate main panel
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


    //new methods above here
}
