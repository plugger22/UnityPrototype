using gameAPI;
using packageAPI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [HideInInspector] public HackingStatus hackingStatus;               //data passed in from AIManager.cs -> UpdateSideTabData
    [HideInInspector] public bool isActive;                             //true if in use (Resistance Player, Authority AI), false otherwise

    private static AISideTabUI aiSideTabUI;

    private bool isFading;
    private Color tempColour;
    private Coroutine myCoroutine;
    private GenericTooltipUI tooltip;


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


    /// <summary>
    /// Initialise. Conditional activiation depending on player side for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        //Resistance player only
        if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideResistance.level)
        {
            switch (state)
            {
                case GameState.TutorialOptions:
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
                    SubInitialiseFastAccess();
                    SubInitialiseSessionStart();
                    SubInitialiseEvents();
                    break;
                case GameState.FollowOnInitialisation:
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                    break;
            }
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //tooltip
        tooltip = sideTabImage.GetComponent<GenericTooltipUI>();
        Debug.Assert(tooltip != null, "Invalid GenericTooltipUI component tooltip (Null)");
        tooltip.isIgnoreClick = true;
        tooltip.testTag = "AISideTabUI";
        tooltip.tooltipMain = "We haven't yet broken the AI's Security systems";
        tooltip.tooltipDetails = "Resistance HQ expect to do so by <b>NEXT TURN</b>";
        //data
        topText.text = "AI";
        bottomText.text = "-";
        hackingStatus = HackingStatus.Initialising;
        myCoroutine = null;
        isFading = false;
        //set alert flasher to zero opacity
        tempColour = alertFlasher.color;
        tempColour.a = 0.0f;
        alertFlasher.color = tempColour;
        //set to Active
        isActive = true;
        //Set all components 
        SetAllStatus(isActive);
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        flashAlertTime = GameManager.i.guiScript.flashAlertTime;
        Debug.Assert(flashAlertTime > 0, "Invalid flashAlertTime (zero)");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.i.AddListener(EventType.AISideTabOpen, OnEvent, "AISideTabUI");
        EventManager.i.AddListener(EventType.AISideTabClose, OnEvent, "AISideTabUI");
        EventManager.i.AddListener(EventType.AISendSideData, OnEvent, "AISideTabUI");
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
    public void SetAllStatus(bool status)
    {
        sideTabImage.gameObject.SetActive(status);
        topText.gameObject.SetActive(status);
        bottomText.gameObject.SetActive(status);
    }

    /// <summary>
    /// make side tab visibile
    /// </summary>
    public void OpenSideTab()
    {
        sideTabImage.gameObject.SetActive(true);
        //alert flasher
        if (hackingStatus == HackingStatus.Possible)
        {
            if (myCoroutine == null)
            { myCoroutine = StartCoroutine("ShowAlertFlash"); }
        }
    }

    public void CloseSideTab()
    {
        sideTabImage.gameObject.SetActive(false);
        if (myCoroutine != null)
        {
            StopCoroutine(myCoroutine);
            myCoroutine = null;
        }
        //close tooltip if open
        GameManager.i.tooltipGenericScript.CloseTooltip("AISideTabUI.cs -> CloseSideTab");
    }


    /// <summary>
    /// update data on side tab (sent from AIManager.cs -> UpdateSideTabData) & GenericTooltipUI data
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
            //cost in Power
            if (string.IsNullOrEmpty(data.bottomText) == false)
            { bottomText.text = data.bottomText; }
            else { bottomText.text = "?"; }
            //hacking Status
            hackingStatus = data.status;
            //alert flasher
            if (hackingStatus == HackingStatus.Possible && sideTabImage.gameObject.activeSelf == true)
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
            //tooltip data
            if (string.IsNullOrEmpty(data.tooltipMain) == false)
            {
                tooltip.tooltipMain = data.tooltipMain;
                if (string.IsNullOrEmpty(data.tooltipDetails) == false)
                { tooltip.tooltipDetails = data.tooltipDetails; }
            }
            else { tooltip.tooltipMain = "Unknown Data"; }
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
