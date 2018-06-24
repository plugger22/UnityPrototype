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
    public Image dividerTop;                   //Side specific sprites for tooltips
    public Image dividerMiddleUpper;
    public Image dividerMiddleLower;
    public Image dividerBottom;
    public GameObject tooltipActorObject;

    private Image background;
    private static TooltipActor tooltipActor;
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
    private string colourEnd;


    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        canvasGroup = tooltipActorObject.GetComponent<CanvasGroup>();
        rectTransform = tooltipActorObject.GetComponent<RectTransform>();
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade;
        offset = GameManager.instance.tooltipScript.tooltipOffset;
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
        colourQuality = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourAction = GameManager.instance.colourScript.GetColour(ColourType.actorAction);
        colourArc = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
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
        dividerTop.gameObject.SetActive(true);
        dividerMiddleUpper.gameObject.SetActive(false);
        dividerMiddleLower.gameObject.SetActive(false);
        dividerBottom.gameObject.SetActive(true);
        if (data.actor != null)
        {
            //Header
            actorName.text = string.Format("{0}<b>{1}</b>{2}{3}{4}{5}{6}", colourArc, data.actor.arc.name, colourEnd, "\n", colourName, data.actor.actorName, colourEnd);
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
                                int numOfTurns = GameManager.instance.actorScript.maxStatValue - data.actor.datapoint2;
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
                    actorConditions.text = builderCondition.ToString();
                }
                else { Debug.LogWarning("Invalid listOfConditions (Null)"); }
            }
            //Trait
            actorTrait.text = data.actor.GetTrait().tagFormatted;
        }
        else { Debug.LogWarning("Invalid Actor (Null)"); }
        //action
        if (data.action != null)
        { actorAction.text = string.Format("{0}{1}{2}", colourAction, data.action.name, colourEnd); }
        else { Debug.LogWarning(string.Format("Actor \"{0}\" has an invalid Action (Null)", data.actor.actorName)); }
        //qualities
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
        }

        //Stats -> only takes the first three Qualities, eg. "Connections, Motivation, Invisibility"
        int dataStats;
        if (data.arrayOfStats.Length > 0 || data.arrayOfStats.Length < numOfQualities)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < numOfQualities; i++)
            {
                dataStats = data.arrayOfStats[i];
                if (i > 0) { builder.AppendLine(); }

                builder.AppendFormat("{0}{1}{2}", GameManager.instance.colourScript.GetValueColour(dataStats), dataStats, colourEnd);
            }
            actorStats.text = builder.ToString();
        }
        //Coordinates -> You need to send World (object.transform) coordinates
        Vector3 worldPos = data.tooltipPos;
        //update required to get dimensions as tooltip is dynamic
        Canvas.ForceUpdateCanvases();
        rectTransform = tooltipActorObject.GetComponent<RectTransform>();
        float height = rectTransform.rect.height;
        float width = rectTransform.rect.width;
        //base y pos at zero (bottom of screen). Adjust up from there.
        worldPos.y +=  height + offset;
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
