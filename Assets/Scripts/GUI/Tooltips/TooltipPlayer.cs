using gameAPI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player based tooltip, static reference instance in GameManager
/// </summary>
public class TooltipPlayer : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerStatus;
    public TextMeshProUGUI playerConditions;
    public TextMeshProUGUI playerQualities;
    public TextMeshProUGUI playerStats;
    public TextMeshProUGUI playerBottom;       //gear for rebels
    public Image dividerTop;                   //Side specific sprites for tooltips
    public Image dividerMiddleUpper;
    public Image dividerMiddleLower;
    public Image dividerBottom;
    public GameObject tooltipPlayerObject;

    private Image background;
    private static TooltipPlayer tooltipPlayer;
    RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private float fadeInTime;
    private int offset;
    //colours
    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourName;
    private string colourQuality;
    private string colourAction;
    private string colourArc;
    private string colourDefault;
    private string colourEnd;


    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        canvasGroup = tooltipPlayerObject.GetComponent<CanvasGroup>();
        rectTransform = tooltipPlayerObject.GetComponent<RectTransform>();
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade;
        offset = GameManager.instance.tooltipScript.tooltipOffset;
        //event listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent);
    }

    /// <summary>
    /// provide a static reference to tooltipPlayer that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TooltipPlayer Instance()
    {
        if (!tooltipPlayer)
        {
            tooltipPlayer = FindObjectOfType(typeof(TooltipPlayer)) as TooltipPlayer;
            if (!tooltipPlayer)
            { Debug.LogError("There needs to be one active TooltipPlayer script on a GameObject in your scene"); }
        }
        return tooltipPlayer;
    }


    //Called when events happen
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.ChangeSide:
                InitialiseTooltip((GlobalSide)Param);
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
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.dataNeutral);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourName = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourQuality = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourAction = GameManager.instance.colourScript.GetColour(ColourType.actorAction);
        colourArc = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }


    /// <summary>
    /// Initialise Player Tool tip
    /// </summary>
    /// <param name="pos">Position of tooltip originator -> note as it's a UI element transform will be in screen units, not world units</param>
    public void SetTooltip(Vector3 pos)
    {
        //open panel at start
        tooltipPlayerObject.SetActive(true);
        //set opacity to zero (invisible)
        SetOpacity(0f);
        //set state of all items in tooltip window
        playerName.gameObject.SetActive(true);
        playerStatus.gameObject.SetActive(false);
        playerConditions.gameObject.SetActive(false);
        playerStats.gameObject.SetActive(true);
        playerQualities.gameObject.SetActive(true);
        playerBottom.gameObject.SetActive(true);
        dividerTop.gameObject.SetActive(true);
        dividerMiddleUpper.gameObject.SetActive(false);
        dividerMiddleLower.gameObject.SetActive(false);
        dividerBottom.gameObject.SetActive(true);
        //Header
        playerName.text = string.Format("{0}<b>PLAYER</b>{1}{2}{3}{4}{5}", colourArc, colourEnd, "\n", colourName, GameManager.instance.playerScript.PlayerName, colourEnd);
        //Status (ignore for the default 'Active' Condition)
        if (GameManager.instance.playerScript.status != ActorStatus.Active)
        {
            //activate UI components
            playerStatus.gameObject.SetActive(true);
            dividerMiddleUpper.gameObject.SetActive(true);
            switch (GameManager.instance.playerScript.status)
            {
                case ActorStatus.Inactive:
                    switch (GameManager.instance.playerScript.inactiveStatus)
                    {
                        case ActorInactive.LieLow:
                            int numOfTurns = GameManager.instance.actorScript.maxStatValue + 1 - GameManager.instance.playerScript.invisibility;
                            playerStatus.text = string.Format("{0}<b>LYING LOW</b>{1}{2}Back in {3} turn{4}", colourNeutral, colourEnd, "\n", numOfTurns,
                                numOfTurns != 1 ? "s" : "");
                            break;
                        case ActorInactive.Breakdown:
                            playerStatus.text = string.Format("{0}<b>BREAKDOWN (Stress)</b>{1}{2}Back next turn", colourNeutral, colourEnd, "\n");
                            break;
                    }
                    break;
                case ActorStatus.Captured:
                    playerStatus.text = string.Format("{0}<b>CAPTURED</b>{1}{2}Whereabouts unknown", colourBad, colourEnd, "\n");
                    break;
                default:
                    playerStatus.text = string.Format("{0}<b>{1}</b>{2}", colourNeutral, GameManager.instance.playerScript.status.ToString().ToUpper(), colourEnd);
                    break;
            }
        }
        //Conditions
        if (GameManager.instance.playerScript.CheckNumOfConditions() > 0)
        {
            List<Condition> listOfConditions = GameManager.instance.playerScript.GetListOfConditions();
            if (listOfConditions != null)
            {
                dividerMiddleLower.gameObject.SetActive(true);
                playerConditions.gameObject.SetActive(true);
                StringBuilder builderCondition = new StringBuilder();
                foreach (Condition condition in listOfConditions)
                {
                    if (builderCondition.Length > 0) { builderCondition.AppendLine(); }
                    switch (condition.type.name)
                    {
                        case "Good":
                            builderCondition.AppendFormat("{0}{1}{2}", colourGood, condition.name, colourEnd);
                            break;
                        case "Bad":
                            builderCondition.AppendFormat("{0}{1}{2}", colourBad, condition.name, colourEnd);
                            break;
                        case "Neutral":
                            builderCondition.AppendFormat("{0}{1}{2}", colourNeutral, condition.name, colourEnd);
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid condition.type.name \"{0}\"", condition.type.name));
                            break;
                    }
                }
                playerConditions.text = builderCondition.ToString();
            }
            else { Debug.LogWarning("Invalid listOfConditions (Null)"); }
        }
        //context sensitive at bottom
        StringBuilder builderStats = new StringBuilder();
        int renown = GameManager.instance.playerScript.Renown;
        switch (GameManager.instance.sideScript.PlayerSide.level)
        {
            case 1:
                //Authority Stats and Qualities
                playerQualities.text = string.Format("{0}OnMap{1}{2}Intransit{3}{4}Reserve{5}", colourNeutral, colourEnd, "\n", "\n", colourNeutral, colourEnd);
                int teamsOnMap = GameManager.instance.dataScript.CheckTeamPoolCount(TeamPool.OnMap);
                int teamsInTransit = GameManager.instance.dataScript.CheckTeamPoolCount(TeamPool.InTransit);
                int teamsReserve = GameManager.instance.dataScript.CheckTeamPoolCount(TeamPool.Reserve);
                playerStats.text = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", colourNeutral, teamsOnMap, colourEnd, "\n", teamsInTransit, "\n", colourNeutral, 
                    teamsReserve, colourEnd);
                //Authority Bottom
                playerBottom.text = string.Format("Renown  {0}{1}{2}", GameManager.instance.colourScript.GetValueColour(renown), renown, colourEnd);
                break;               
            case 2:
                //Resistance gear
                List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
                if (listOfGear != null && listOfGear.Count > 0)
                {
                    StringBuilder builderGear = new StringBuilder();
                    builderGear.Append("Gear");
                    //gear in inventory
                    foreach (int gearID in listOfGear)
                    {
                        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                        if (gear != null)
                        { builderGear.AppendFormat("<b>{0}{1}{2}{3}</b>", "\n", colourNeutral, gear.name, colourEnd); }
                    }
                    playerBottom.text = builderGear.ToString();
                }
                else { playerBottom.text = string.Format("{0}<size=90%>No Gear in Inventory</size>{1}", colourArc, colourEnd); }
                //Resistance Stats and Qualities -> Renown / Secrets / Invisibility
                
                builderStats.AppendFormat("{0}{1}{2}{3}", GameManager.instance.colourScript.GetValueColour(renown), renown, colourEnd, "\n");
                builderStats.Append("0");
                builderStats.AppendLine();
                int invisibility = GameManager.instance.playerScript.invisibility;
                builderStats.AppendFormat("{0}{1}{2}{3}", GameManager.instance.colourScript.GetValueColour(invisibility), invisibility, colourEnd, "\n");
                playerStats.text = builderStats.ToString();
                //Resistance Qualities
                playerQualities.text = string.Format("Renown{0}Secrets{1}Invisibility", "\n", "\n");
                break;
        }
        //Coordinates -> You need to send World (object.transform) coordinates
        Vector3 worldPos = pos;
        //update required to get dimensions as tooltip is dynamic
        Canvas.ForceUpdateCanvases();
        rectTransform = tooltipPlayerObject.GetComponent<RectTransform>();
        float height = rectTransform.rect.height;
        float width = rectTransform.rect.width;
        //base y pos at zero (bottom of screen). Adjust up from there.
        worldPos.y += height + offset;
        worldPos.x -= width / 10;
        //width
        if (worldPos.x + width / 2 >= Screen.width)
        { worldPos.x -= width / 2 + worldPos.x - Screen.width; }
        else if (worldPos.x - width / 2 <= 0)
        { worldPos.x += width / 2 - worldPos.x; }

        //set new position
        tooltipPlayerObject.transform.position = worldPos;
        Debug.Log("UI: Open -> TooltipActor" + "\n");
    }


    public void SetOpacity(float opacity)
    { canvasGroup.alpha = opacity; }

    public float GetOpacity()
    { return canvasGroup.alpha; }

    /// <summary>
    /// fade in tooltip over time
    /// </summary>
    /// <returns></returns>
    IEnumerator FadeInTooltip()
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime / fadeInTime;
            yield return null;
        }
    }

    /// <summary>
    /// returns true if tooltip is active and visible
    /// </summary>
    /// <returns></returns>
    public bool CheckTooltipActive()
    { return tooltipPlayerObject.activeSelf; }

    /// <summary>
    /// close tool tip
    /// </summary>
    public void CloseTooltip()
    {
        Debug.Log("UI: Close -> TooltipActor" + "\n");
        tooltipPlayerObject.SetActive(false);
    }


    /// <summary>
    /// set up sprites on actor tooltip for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    public void InitialiseTooltip(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = tooltipPlayerObject.GetComponent<Image>();
        //assign side specific sprites
        switch (side.name)
        {
            case "Authority":
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundAuthority;
                dividerTop.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerMiddleUpper.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerMiddleLower.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerBottom.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                break;
            case "Resistance":
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundRebel;
                dividerTop.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerMiddleUpper.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerMiddleLower.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerBottom.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", side.name));
                break;
        }
    }


}
