using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using gameAPI;

/// <summary>
/// Handles AI side tab data and logic matters (mouse interactions are handled by AISideTabMouseUI.cs)
/// </summary>
public class AISideTabUI : MonoBehaviour
{

    public Image sideTabImage;
    public Image alertFlasher;
    public TextMeshProUGUI topText;
    public TextMeshProUGUI bottomText;

    private float flashAlertTime;

    [HideInInspector] public string tooltipHeader;
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipDetails;

    [HideInInspector] public HackingStatus hackingStatus;             //data passed in from AIManager.cs -> UpdateSideTabData

    private static AISideTabUI aiSideTabUI;

    private bool isFading;
    private Color tempColour;
    private Coroutine myCoroutine;


    /// <summary>
    /// provide a static reference to AISideTabUI that can be accessed from any script
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
        flashAlertTime = GameManager.instance.guiScript.flashAlertTime;
        Debug.Assert(flashAlertTime > 0, "Invalid flashAlertTime (zero)");
        topText.text = "AI";
        bottomText.text = "-";
        hackingStatus = HackingStatus.Initialising;
        myCoroutine = null;
        isFading = false;
        //set alert flasher to zero opacity
        tempColour = alertFlasher.color;
        tempColour.a = 0.0f;
        alertFlasher.color = tempColour;
        //set all sub compoponents to Active
        SetAllToActive();
    }

    public void Start()
    {
        //event listener
        EventManager.instance.AddListener(EventType.AISideTabOpen, OnEvent, "AISideTabUI");
        EventManager.instance.AddListener(EventType.AISideTabClose, OnEvent, "AISideTabUI");
        EventManager.instance.AddListener(EventType.AISendSideData, OnEvent, "AISideTabUI");
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
            case EventType.AISendSideData:
                AISideTabData data = Param as AISideTabData;
                UpdateSideTab(data);
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


    /// <summary>
    /// update data on side tab (sent from AIManager.cs -> UpdateSideTabData)
    /// </summary>
    /// <param name="data"></param>
    private void UpdateSideTab(AISideTabData data)
    {
        if (data != null)
        {
            //'A.I'
            if (string.IsNullOrEmpty(data.topText) == false)
            { topText.text = data.topText; }
            else { topText.text = "?"; }
            //cost in renown
            if (string.IsNullOrEmpty(data.bottomText) == false)
            { bottomText.text = data.bottomText; }
            else { bottomText.text = "?"; }
            //hacking Status
            hackingStatus = data.status;
            //alert flasher
            if (hackingStatus == HackingStatus.Possible)
            {
                if (myCoroutine == null)
                { myCoroutine = StartCoroutine("ShowAlertFlash"); }
            }
            else
            {
                if (myCoroutine != null)
                {
                    StopCoroutine(myCoroutine);
                    myCoroutine = null;
                    isFading = false;
                    //reset opacity back to zero
                    tempColour = alertFlasher.color;
                    tempColour.a = 0.0f;
                    alertFlasher.color = tempColour;
                }
            }
        }
        else { Debug.LogWarning("Invalid AISideTabData (Null)"); }
    }

    /// <summary>
    /// coroutine to flash the white alert 'dot' whenever the side tab is ready to be hacked
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowAlertFlash()
    {
        //infinite while loop
        while (true)
        {
            tempColour = alertFlasher.color;
            if (isFading == false)
            {
                tempColour.a += Time.deltaTime / flashAlertTime;
                if (tempColour.a >= 1.0f)
                { isFading = true; }
            }
            else
            {
                tempColour.a -= Time.deltaTime / flashAlertTime;
                if (tempColour.a <= 0.0f)
                { isFading = false; }
            }
            alertFlasher.color = tempColour;
            yield return null;
        }
    }

    //new methods above here
}
