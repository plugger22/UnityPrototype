using gameAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles the city info display (static class)
/// </summary>
public class CityInfoUI : MonoBehaviour
{
    public GameObject cityInfoObject;
    public Image backgroundPanel;
    public Image cityImage;
    public Image subPanelLeft;
    public Image subPanelMiddle;
    public Image subPanelRight;
    public TextMeshProUGUI cityName;
    public TextMeshProUGUI cityArc;
    public TextMeshProUGUI cityDescription;
    //left panel -> Districts
    public TextMeshProUGUI districtNames;
    public TextMeshProUGUI districtTotals;

    public Button cancelButton;



    private City city;                              //current city level

    private static CityInfoUI cityInfoUI;


    /// <summary>
    /// provide a static reference to cityInfoUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static CityInfoUI Instance()
    {
        if (!cityInfoUI)
        {
            cityInfoUI = FindObjectOfType(typeof(CityInfoUI)) as CityInfoUI;
            if (!cityInfoUI)
            { Debug.LogError("There needs to be one active CityInfoUI script on a GameObject in your scene"); }
        }
        return cityInfoUI;
    }


    public void Initialise()
    {
        UpdateCachedData();
        SetAllToActive();
    }


    public void Start()
    {
        //event listener
        EventManager.instance.AddListener(EventType.ChangeLevel, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent);
        EventManager.instance.AddListener(EventType.OpenCityInfo, OnEvent);
        EventManager.instance.AddListener(EventType.CloseCityInfo, OnEvent);
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
            case EventType.ChangeLevel:
                ChangeLevel();
                break;
            case EventType.ChangeSide:
                ChangeSides((GlobalSide)Param);
                break;
            case EventType.OpenCityInfo:
                SetCityInfo();
                break;
            case EventType.CloseCityInfo:
                CloseCityInfo();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Change level event
    /// </summary>
    private void ChangeLevel()
    {
        UpdateCachedData();
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
                backgroundPanel.sprite = GameManager.instance.sideScript.info_background_Authority;
                cancelButton.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Authority;
                break;
            case 2:
                backgroundPanel.sprite = GameManager.instance.sideScript.info_background_Resistance;
                cancelButton.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Resistance;
                break;
        }
    }

    /// <summary>
    /// updates and assigns all cached data for city info screen when a new level (City) is loaded
    /// </summary>
    private void UpdateCachedData()
    {
        //city details
        city = GameManager.instance.cityScript.GetCity();
        Debug.Assert(city != null, "Invalid city (Null)");
        cityName.text = city.name;
        cityArc.text = city.Arc.name;
        cityDescription.text = city.descriptor;
        //district details
        districtNames.text = GameManager.instance.nodeScript.GetNodeArcNames();
        districtTotals.text = GameManager.instance.nodeScript.GetNodeArcNumbers();
    }

    /// <summary>
    /// set all UI components (apart from main) to active. Run at level start to ensure no problems (something hasn't been switched off in the editor)
    /// </summary>
    private void SetAllToActive()
    {
        backgroundPanel.gameObject.SetActive(true);
        cityImage.gameObject.SetActive(true);
        cityName.gameObject.SetActive(true);
        cityArc.gameObject.SetActive(true);
        cityDescription.gameObject.SetActive(true);
        districtNames.gameObject.SetActive(true);
        districtTotals.gameObject.SetActive(true);
        subPanelLeft.gameObject.SetActive(true);
        subPanelMiddle.gameObject.SetActive(true);
        subPanelRight.gameObject.SetActive(true);
    }

    /// <summary>
    /// Open City Info display
    /// </summary>
    private void SetCityInfo()
    {        
        //set modal status
        GameManager.instance.guiScript.SetIsBlocked(true);
        //activate main panel
        cityInfoObject.SetActive(true);
        //set game state
        GameManager.instance.inputScript.SetModalState(ModalState.GenericPicker);
        Debug.Log("UI: Open -> Open City Info Display" + "\n");
    }

    /// <summary>
    /// close city Info display
    /// </summary>
    private void CloseCityInfo()
    {
        cityInfoObject.SetActive(false);
        GameManager.instance.guiScript.SetIsBlocked(false);
        //set game state
        GameManager.instance.inputScript.ResetStates();
        Debug.Log("UI: Close -> CityInfoDisplay" + "\n");
    }


    //new methods above here
}
