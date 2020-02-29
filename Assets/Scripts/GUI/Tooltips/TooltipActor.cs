using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using packageAPI;
using gameAPI;

/// <summary>
/// Actor based tooltip, reference static instance in GameManager
/// </summary>
public class TooltipActor : MonoBehaviour
{
    public TextMeshProUGUI actorName;
    public TextMeshProUGUI actorStatus;
    public TextMeshProUGUI actorQualities;
    public TextMeshProUGUI actorConditions;
    public TextMeshProUGUI actorStats;
    public TextMeshProUGUI actorTrait;
    public TextMeshProUGUI actorAction;
    public TextMeshProUGUI actorGear;
    public TextMeshProUGUI actorSecrets;
    public Image dividerTop;                   //Side specific sprites for tooltips
    public Image dividerMiddleUpper;
    public Image dividerMiddleLower;
    public Image dividerBottom;
    public Image dividerGear;
    public Image dividerSecrets;
    public GameObject tooltipActorObject;

    private Image background;
    private static TooltipActor tooltipActor;
    RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private float fadeInTime;
    private int offset;

    //fast access
    private int gracePeriod = -1;                    //actor gear grace period

    private string[] arrayOfIcons = new string[3];

    //colours
    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourName;
    /*private string colourQuality;*/
    private string colourAction;
    private string colourArc;
    private string colourAlert;
    private string colourGrey;
    private string colourEnd;

    /// <summary>
    /// needed for sequencing issues. Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                break;
            case GameState.FollowOnInitialisation:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseFastAccess
    /// <summary>
    /// Fast access submethod
    /// </summary>
    private void SubInitialiseFastAccess()
    {
        //node datapoint icons
        arrayOfIcons[0] = GameManager.instance.guiScript.connectionsIcon;
        arrayOfIcons[1] = GameManager.instance.guiScript.motivationIcon;
        arrayOfIcons[2] = GameManager.instance.guiScript.invisibilityIcon;
        Debug.Assert(arrayOfIcons[0] != null, "Invalid arrayOfIcons[0] (Null)");
        Debug.Assert(arrayOfIcons[1] != null, "Invalid arrayOfIcons[1] (Null)");
        Debug.Assert(arrayOfIcons[2] != null, "Invalid arrayOfIcons[2] (Null)");
    }
    #endregion

    #endregion

    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        canvasGroup = tooltipActorObject.GetComponent<CanvasGroup>();
        rectTransform = tooltipActorObject.GetComponent<RectTransform>();
        fadeInTime = GameManager.instance.guiScript.tooltipFade;
        offset = GameManager.instance.guiScript.tooltipOffset;
        //fast access
        gracePeriod = GameManager.instance.gearScript.actorGearGracePeriod;
        Debug.Assert(gracePeriod > -1, "Invalid gracePeriod (-1)");

        //event listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "TooltipActor");
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "TooltipActor");
    }

    /// <summary>
    /// provide a static reference to tooltipActor that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TooltipActor Instance()
    {
        if (!tooltipActor)
        {
            tooltipActor = FindObjectOfType(typeof(TooltipActor)) as TooltipActor;
            if (!tooltipActor)
            { Debug.LogError("There needs to be one active TooltipActor script on a GameObject in your scene"); }
        }
        return tooltipActor;
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
        /*colourQuality = GameManager.instance.colourScript.GetColour(ColourType.whiteText);*/
        colourAction = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourArc = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }


    /// <summary>
    /// Initialise actor Tool tip
    /// </summary>
    /// <param name="name">Name of Node, eg. 'Downtown Manhattan'</param>
    /// <param name="arrayOfStats">Give stats as Ints[3] in order Stability - Support - Security</param>
    /// <param name="trait">place target info here, a blank list if none</param>
    /// <param name="pos">Position of tooltip originator -> note as it's a UI element transform will be in screen units, not world units</param>
    public void SetTooltip(ActorTooltipData data)
    {
        bool isResistance = true;
        if (GameManager.instance.sideScript.PlayerSide.level == 1) { isResistance = false; }
        //open panel at start
        tooltipActorObject.SetActive(true);
        //set opacity to zero (invisible)
        SetOpacity(0f);
        //set state of all items in tooltip window
        actorName.gameObject.SetActive(true);
        actorStatus.gameObject.SetActive(false);
        actorConditions.gameObject.SetActive(false);
        actorStats.gameObject.SetActive(true);
        actorQualities.gameObject.SetActive(true);
        actorTrait.gameObject.SetActive(true);
        actorAction.gameObject.SetActive(true);
        actorSecrets.gameObject.SetActive(true);
        dividerTop.gameObject.SetActive(true);
        dividerMiddleUpper.gameObject.SetActive(false);
        dividerMiddleLower.gameObject.SetActive(false);
        dividerBottom.gameObject.SetActive(true);
        if (isResistance == true)
        { dividerGear.gameObject.SetActive(true); }
        else { dividerGear.gameObject.SetActive(false); }
        dividerSecrets.gameObject.SetActive(true);
        if (data.actor != null)
        {
            //Header
            actorName.text = string.Format("{0}<b><size=120%>{1}</size>{2}{3}{4}{5}</b>{6}", colourArc, data.actor.arc.name, colourEnd, "\n", colourName, data.actor.actorName, colourEnd);
            //Status (ignore for the default 'Active' Condition)
            if (data.actor.Status != ActorStatus.Active)
            {
                //activate UI components
                actorStatus.gameObject.SetActive(true);
                dividerMiddleUpper.gameObject.SetActive(true);
                switch (data.actor.Status)
                {
                    case ActorStatus.Inactive:
                        switch (data.actor.inactiveStatus)
                        {
                            case ActorInactive.LieLow:
                                int numOfTurns = GameManager.instance.actorScript.maxStatValue - data.actor.GetDatapoint(ActorDatapoint.Invisibility2);
                                if (data.actor.isLieLowFirstturn == true)
                                { numOfTurns++; }
                                actorStatus.text = string.Format("{0}<b>LYING LOW</b>{1}{2}Back in {3} turn{4}", colourNeutral, colourEnd, "\n", numOfTurns,
                                    numOfTurns != 1 ? "s" : "");
                                break;
                            case ActorInactive.Breakdown:
                                actorStatus.text = string.Format("{0}<b>BREAKDOWN (Stress)</b>{1}{2}Back next turn", colourNeutral, colourEnd, "\n");
                                break;
                        }
                        break;
                    case ActorStatus.Captured:
                        actorStatus.text = string.Format("{0}<b>CAPTURED</b>{1}{2}Whereabouts unknown", colourBad, colourEnd, "\n");
                        break;
                    default:
                        actorStatus.text = string.Format("{0}<b>{1}</b>{2}", colourNeutral, data.actor.Status.ToString().ToUpper(), colourEnd);
                        break;
                }
            }
            //Conditions
            if (data.actor.CheckNumOfConditions() > 0)
            {
                List<Condition> listOfConditions = data.actor.GetListOfConditions();
                if (listOfConditions != null)
                {
                    dividerMiddleLower.gameObject.SetActive(true);
                    actorConditions.gameObject.SetActive(true);
                    StringBuilder builderCondition = new StringBuilder();
                    foreach(Condition condition in listOfConditions)
                    {
                        if (builderCondition.Length > 0) { builderCondition.AppendLine(); }
                        switch(condition.type.name)
                        {
                            case "Good":
                                builderCondition.AppendFormat("{0}{1}{2}", colourGood, condition.tag, colourEnd);
                                break;
                            case "Bad":
                                builderCondition.AppendFormat("{0}{1}{2}", colourBad, condition.tag, colourEnd);
                                break;
                            case "Neutral":
                                builderCondition.AppendFormat("{0}{1}{2}", colourNeutral, condition.tag, colourEnd);
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid condition.type.name \"{0}\"", condition.type.name));
                                break;
                        }

                        /*//Blackmail condition has a timer
                        switch(condition.tag)
                        {
                            case "BLACKMAILER":
                                builderCondition.AppendFormat("{0} {1}{2}", colourBad, data.actor.blackmailTimer, colourEnd);
                                break;
                        }*/

                    }
                    actorConditions.text = builderCondition.ToString();
                }
                else { Debug.LogWarning("Invalid listOfConditions (Null)"); }
            }
            //Trait
            actorTrait.text = string.Format("<size=120%>{0}</size>", data.actor.GetTrait().tagFormatted);
        }
        else { Debug.LogWarning("Invalid Actor (Null)"); }
        //action
        if (data.action != null)
        { actorAction.text = string.Format("{0}<b>{1}</b>{2}", colourAction, data.action.tag, colourEnd); }
        else { Debug.LogWarning(string.Format("Actor \"{0}\" has an invalid Action (Null)", data.actor.actorName)); }

        /*//qualities -> EDIT no longer used as replaced by a single text field for both qualities and stars combined (below)
        int numOfQualities = GameManager.instance.actorScript.numOfQualities;
        if (data.arrayOfQualities.Length > 0)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < numOfQualities; i++)
            {
                if (i > 0) { builder.AppendLine(); }
                builder.AppendFormat("{0}{1}{2}", colourQuality, data.arrayOfQualities[i], colourEnd);
            }
            actorQualities.text = builder.ToString();
        }*/

        //Stats -> only takes the first three Qualities, eg. "Connections, Motivation, Invisibility"
        int dataStats;
        int numOfQualities = GameManager.instance.actorScript.numOfQualities;
        if (data.arrayOfStats.Length > 0 || data.arrayOfStats.Length < numOfQualities)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < numOfQualities; i++)
            {
                dataStats = data.arrayOfStats[i];
                if (i > 0) { builder.AppendLine(); }
                builder.AppendFormat("{0} {1}<pos=57%>{2}", arrayOfIcons[i], data.arrayOfQualities[i], GameManager.instance.guiScript.GetStars(dataStats));
            }
            actorStats.text = builder.ToString();
        }
        //Gear
        if (isResistance == true)
        {
            if (data.gear != null)
            {
                //if gear held for <= grace period then show as Grey (can't be requested), otherwise yellow
                if (data.actor.GetGearTimer() <= gracePeriod)
                { actorGear.text = string.Format("{0}Gear{1}{2}{3}{4}{5}", colourAlert, colourEnd, "\n", colourGrey, data.gear.tag, colourEnd); }
                else
                { actorGear.text = string.Format("<b>{0}Gear{1}{2}{3}{4}{5}</b>", colourAlert, colourEnd, "\n", colourNeutral, data.gear.tag, colourEnd); }
            }
            else { actorGear.text = string.Format("{0}<size=95%>No Gear</size>{1}", colourGrey, colourEnd); }
        }
        //Secrets
        if (data.listOfSecrets != null && data.listOfSecrets.Count > 0)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}Secrets{1}", colourAlert, colourEnd);
            foreach (string secret in data.listOfSecrets)
            { builder.AppendFormat("{0}<b>{1}{2}{3}</b>", "\n", colourNeutral, secret, colourEnd); }
            actorSecrets.text = builder.ToString();
        }
        else { actorSecrets.text = string.Format("{0}<size=95%>No Secrets</size>{1}", colourGrey, colourEnd); }
        //Coordinates -> You need to send World (object.transform) coordinates
        Vector3 worldPos = data.tooltipPos;
        //update required to get dimensions as tooltip is dynamic
        Canvas.ForceUpdateCanvases();
        rectTransform = tooltipActorObject.GetComponent<RectTransform>();
        float height = rectTransform.rect.height;
        float width = rectTransform.rect.width;
        //base y pos at zero (bottom of screen). Adjust up from there.
        worldPos.y +=  height + offset + 100;
        worldPos.x -= width / 10;
        //width
        if (worldPos.x + width / 2 >= Screen.width)
        { worldPos.x -= width / 2 + worldPos.x - Screen.width; }
        else if (worldPos.x - width / 2 <= 0)
        { worldPos.x += width / 2 - worldPos.x; }
        //set new position
        tooltipActorObject.transform.position = worldPos;
        Debug.LogFormat("[UI] TooltipActor.cs -> SetTooltip{0}", "\n");
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
    { return tooltipActorObject.activeSelf; }

    /// <summary>
    /// close tool tip
    /// </summary>
    public void CloseTooltip(string callingMethod = "Unknown")
    {
        Debug.LogFormat("[UI] TooltipActor.cs -> CloseTooltip: called by {0}{1}",  callingMethod, "\n");
        tooltipActorObject.SetActive(false);
    }


    /// <summary>
    /// set up sprites on actor tooltip for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    public void InitialiseTooltip(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = tooltipActorObject.GetComponent<Image>();
        //assign side specific sprites
        switch (side.level)
        {
            case 1:
                //Authority
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundAuthority;
                dividerTop.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerMiddleUpper.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerMiddleLower.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerBottom.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerGear.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerSecrets.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                break;
            case 2:
                //Resistance
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundRebel;
                dividerTop.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerMiddleUpper.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerMiddleLower.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerBottom.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerGear.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerSecrets.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", side.name));
                break;
        }
    }

}
