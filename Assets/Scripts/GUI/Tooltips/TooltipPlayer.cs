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
    public TextMeshProUGUI playerStats;
    public TextMeshProUGUI playerMulti_1;       //multipurpose section 1
    public TextMeshProUGUI playerMulti_2;       //multipurpose section 2
    public Image divider_1;                     //numbered from top to bottom
    public Image divider_2;
    public Image divider_3;
    public Image divider_4;
    public Image divider_5;
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
    private string colourAlert;
    private string colourGrey;
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
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "TooltipPlayer");
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "TooltipPlayer");
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
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }


    /// <summary>
    /// Initialise Player Tool tip
    /// </summary>
    /// <param name="pos">Position of tooltip originator -> note as it's a UI element transform will be in screen units, not world units</param>
    public void SetTooltip(Vector3 pos)
    {
        bool isStatus = false;
        bool isConditions = false;
        //open panel at start
        tooltipPlayerObject.SetActive(true);
        //set opacity to zero (invisible)
        SetOpacity(0f);
        //set state of all items in tooltip window (Status and Conditions are switched on later only if present)
        playerName.gameObject.SetActive(true);
        playerStatus.gameObject.SetActive(false);
        playerConditions.gameObject.SetActive(false);
        playerStats.gameObject.SetActive(true);
        playerMulti_1.gameObject.SetActive(true);
        playerMulti_2.gameObject.SetActive(true);
        divider_1.gameObject.SetActive(true);
        divider_2.gameObject.SetActive(false);
        divider_3.gameObject.SetActive(false);   //true if both Status and Conditions texts are present
        divider_4.gameObject.SetActive(true);
        divider_5.gameObject.SetActive(true);
        //
        // - - - Name - - -
        //
        playerName.text = string.Format("{0}<b>PLAYER</b>{1}{2}{3}{4}{5}", colourAlert, colourEnd, "\n", colourName, GameManager.instance.playerScript.PlayerName, colourEnd);
        //
        // - - - Status - - -
        //
        if (GameManager.instance.playerScript.status != ActorStatus.Active)
        {
            //activate UI components
            playerStatus.gameObject.SetActive(true);
            isStatus = true;
            switch (GameManager.instance.playerScript.status)
            {
                case ActorStatus.Inactive:
                    switch (GameManager.instance.playerScript.inactiveStatus)
                    {
                        case ActorInactive.LieLow:
                            int numOfTurns = GameManager.instance.actorScript.maxStatValue - GameManager.instance.playerScript.Invisibility;
                            if (GameManager.instance.playerScript.isLieLowFirstturn == true) { numOfTurns++; }
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
        //
        // - - - Conditions - - -
        //
        if (GameManager.instance.playerScript.CheckNumOfConditions() > 0)
        {
            List<Condition> listOfConditions = GameManager.instance.playerScript.GetListOfConditions();
            if (listOfConditions != null)
            {
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
                isConditions = true;
                playerConditions.text = builderCondition.ToString();
            }
            else { Debug.LogWarning("Invalid listOfConditions (Null)"); }
        }
        //
        // - - - Dividers - - -
        //
        if (isStatus == true && isConditions == true) { divider_2.gameObject.SetActive(true); }
        if (isStatus == true || isConditions == true) { divider_3.gameObject.SetActive(true); }
        //
        // - - - Stats - - -
        //
        switch (GameManager.instance.sideScript.PlayerSide.level)
        {
            case 1:
                //Authority
                StringBuilder builderStats = new StringBuilder();
                builderStats.AppendFormat("{0}Teams{1}{2}", colourAlert, colourEnd, "\n");
                builderStats.AppendFormat("On Map<pos=70%>{0}{1}", GameManager.instance.dataScript.CheckTeamPoolCount(TeamPool.OnMap), "\n");
                builderStats.AppendFormat("In Transit<pos=70%>{0}{1}", GameManager.instance.dataScript.CheckTeamPoolCount(TeamPool.InTransit), "\n");
                builderStats.AppendFormat("Reserves<pos=70%>{0}{1}", GameManager.instance.dataScript.CheckTeamPoolCount(TeamPool.Reserve), "\n");
                playerStats.text = builderStats.ToString();
                break;
            case 2:
                //Resistance
                int invisibility = GameManager.instance.playerScript.Invisibility;
                playerStats.text = string.Format("Invisibility<pos=70%>{0}{1}{2}", GameManager.instance.colourScript.GetValueColour(invisibility), invisibility, colourEnd);
                break;
        }
        //
        // - - - MultiPurpose 1 - - - 
        //
        switch (GameManager.instance.sideScript.PlayerSide.level)
        {
            case 1:
                //Authority
                playerMulti_1.text = "Placeholder";
                break;
            case 2:
                //Resistance -> Gear
                List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
                if (listOfGear != null && listOfGear.Count > 0)
                {
                    StringBuilder builderGear = new StringBuilder();
                    builderGear.AppendFormat("{0}Gear{1}", colourAlert, colourEnd);
                    //gear in inventory
                    foreach (int gearID in listOfGear)
                    {
                        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                        if (gear != null)
                        { builderGear.AppendFormat("<b>{0}{1}{2}{3}</b>", "\n", colourNeutral, gear.name, colourEnd); }
                    }
                    playerMulti_1.text = builderGear.ToString();
                }
                else { playerMulti_1.text = string.Format("{0}<size=95%>No Gear</size>{1}", colourGrey, colourEnd); }
                break;
        }

        //
        // - - - MultiPurpose 2 - - - 
        //
        switch (GameManager.instance.sideScript.PlayerSide.level)
        {
            case 1:
                //Authority
                playerMulti_2.text = "Placeholder";
                break;
            case 2:
                //Resistance -> Secrets
                playerMulti_2.text = string.Format("{0}<size=95%>No Secrets</size>{1}", colourGrey, colourEnd);
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
        Debug.LogFormat("[UI] TooltipPlayer.cs -> SetTooltip{0}", "\n");
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
    public void CloseTooltip(string callingMethod = "Unknown")
    {
        Debug.LogFormat("[UI] TooltipPlayer.cs -> CloseTooltip: calling by {0}{1}", callingMethod, "\n");
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
                divider_1.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                divider_2.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                divider_3.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                divider_4.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                divider_5.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                break;
            case "Resistance":
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundRebel;
                divider_1.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                divider_2.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                divider_3.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                divider_4.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                divider_5.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", side.name));
                break;
        }
    }


}
