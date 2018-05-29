using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles AI side tab matters (mouse interactions are handled by AISideTabMouseUI.cs)
/// </summary>
public class AISideTabUI : MonoBehaviour
{

    public Image sideTabImage;
    public TextMeshProUGUI topText;
    public TextMeshProUGUI bottomText;



    [HideInInspector] public string tooltipHeader;
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipDetails;

    private static AISideTabUI aiSideTabUI;


    /// <summary>
    /// provide a static reference to AIDisplayUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static AISideTabUI Instance()
    {
        if (!aiSideTabUI)
        {
            aiSideTabUI = FindObjectOfType(typeof(AISideTabUI)) as AISideTabUI;
            if (!aiSideTabUI)
            { Debug.LogError("There needs to be one active aiSideTabUI script on a GameObject in your scene"); }
        }
        return aiSideTabUI;
    }

    public void Initialise()
    {
        //set all sub compoponents to Active
        SetAllToActive();
        topText.text = "AI";
        bottomText.text = "2";    //PLACEHOLDER
    }

    public void Start()
    {
        //event listener
        EventManager.instance.AddListener(EventType.AISideTabOpen, OnEvent, "AISideTabUI");
        EventManager.instance.AddListener(EventType.AISideTabClose, OnEvent, "AISideTabUI");
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
            case EventType.AISideTabOpen:
                OpenSideTab();
                break;
            case EventType.AISideTabClose:
                CloseSideTab();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// set all UI components (apart from main) to active. Run at level start to ensure no problems (something hasn't been switched off in the editor)
    /// </summary>
    private void SetAllToActive()
    {
        sideTabImage.gameObject.SetActive(true);
        topText.gameObject.SetActive(true);
        bottomText.gameObject.SetActive(true);
    }



    /// <summary>
    /// make side tab visibile
    /// </summary>
    public void OpenSideTab()
    { sideTabImage.gameObject.SetActive(true); }

    public void CloseSideTab()
    { sideTabImage.gameObject.SetActive(false); }
}
