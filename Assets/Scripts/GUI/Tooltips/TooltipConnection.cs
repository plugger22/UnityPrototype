using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using gameAPI;
using packageAPI;
using System.Text;
using System;

/// <summary>
/// Connection tooltips, static reference in GameManager
/// </summary>
public class TooltipConnection : MonoBehaviour
{
    public TextMeshProUGUI textTop;
    public TextMeshProUGUI textMiddle;
    public TextMeshProUGUI textBottom;
    public Image dividerTop;                   //Side specific sprites for tooltips
    public Image dividerBottom;
    public GameObject tooltipConnectionObject;

    private Image background;
    private static TooltipConnection tooltipConnection;
    RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private float fadeInTime;
    private int offset;
    //colours
    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourArc;
    private string colourNormal;
    private string colourEnd;


    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        canvasGroup = tooltipConnectionObject.GetComponent<CanvasGroup>();
        rectTransform = tooltipConnectionObject.GetComponent<RectTransform>();
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade;
        offset = GameManager.instance.tooltipScript.tooltipOffset;
        //event listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "TooltipConnection");
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "TooltipConnection");
    }

    /// <summary>
    /// provide a static reference to tooltipConnection that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TooltipConnection Instance()
    {
        if (!tooltipConnection)
        {
            tooltipConnection = FindObjectOfType(typeof(TooltipConnection)) as TooltipConnection;
            if (!tooltipConnection)
            { Debug.LogError("There needs to be one active TooltipConnection script on a GameObject in your scene"); }
        }
        return tooltipConnection;
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
        colourArc = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }


    /// <summary>
    /// Initialise connection tooltip
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="securityLevel"></param>
    /// <param name="textBottom"></param>
    /// <param name="textMiddle"></param>
    public void SetTooltip(Vector3 tooltipPos, int connID, ConnectionType securityLevel, List<EffectDataOngoing> listOfOngoingEffects, string textMain = null)
    {
        Debug.LogFormat("[UI] TooltipConnection -> SetTooltip{0}", "\n");
        //open panel at start
        tooltipConnectionObject.SetActive(true);
        //set opacity to zero (invisible)
        SetOpacity(0f);
        //set state of all items in tooltip window
        textTop.gameObject.SetActive(true);
        textBottom.gameObject.SetActive(true);
        dividerTop.gameObject.SetActive(true);
        //display main text only if present
        if (string.IsNullOrEmpty(textMain) == false)
        {
            dividerBottom.gameObject.SetActive(true);
            textMiddle.gameObject.SetActive(true);
        }
        else
        {
            dividerBottom.gameObject.SetActive(false);
            textMiddle.gameObject.SetActive(false);
        }
        //security level
        string fullItem;
        //top text
        string debugID = "";
        //show connID if debug data on
        if (GameManager.instance.optionScript.debugData == true)
        { debugID = string.Format("<font=\"LiberationSans SDF\"> ID {0}</font>", connID); }
        //context sensitive depending on ActivityUI
        switch (GameManager.instance.nodeScript.activityState)
        {
            case ActivityUI.None:
                switch (securityLevel)
                {
                    case ConnectionType.HIGH: fullItem = string.Format("{0}<b>Security {1}</b>{2}", colourBad, securityLevel, colourEnd); break;
                    case ConnectionType.MEDIUM: fullItem = string.Format("{0}<b>Security {1}</b>{2}", colourNeutral, securityLevel, colourEnd); break;
                    case ConnectionType.LOW: fullItem = string.Format("{0}<b>Security {1}</b>{2}", colourGood, securityLevel, colourEnd); break;
                    case ConnectionType.None: fullItem = string.Format("Security None"); break;
                    default: fullItem = string.Format("<b>Unknown</b>"); break;
                }
                textTop.text = string.Format("{0}Connection{1}{2}{3}{4}", colourArc, colourEnd, debugID, "\n", fullItem);
                break;
            case ActivityUI.Time:
                switch (securityLevel)
                {
                    case ConnectionType.HIGH: fullItem = string.Format("{0}<b>RECENT</b>{1}", colourBad, colourEnd); break;
                    case ConnectionType.MEDIUM: fullItem = string.Format("{0}<b>TWO Turns Ago</b>{1}", colourNeutral, colourEnd); break;
                    case ConnectionType.LOW: fullItem = string.Format("{0}<b>THREE OR MORE Turns Ago</b>{1}", colourGood, colourEnd); break;
                    case ConnectionType.None: fullItem = "No Activity"; break;
                    default: fullItem = string.Format("<b>NONE</b>"); break;
                }
                textTop.text = string.Format("{0}Resistance Activity{1}{2}{3}{4}", colourArc, colourEnd, debugID, "\n", fullItem);
                break;
            case ActivityUI.Count:
                switch (securityLevel)
                {
                    case ConnectionType.HIGH: fullItem = string.Format("{0}<b>THREE OR MORE Incidents</b>{1}", colourBad, colourEnd); break;
                    case ConnectionType.MEDIUM: fullItem = string.Format("{0}<b>TWO Incidents</b>{1}", colourNeutral, colourEnd); break;
                    case ConnectionType.LOW: fullItem = string.Format("{0}<b>ONE Incident</b>{1}", colourGood, colourEnd); break;
                    case ConnectionType.None: fullItem = "No Activity"; break;
                    default: fullItem = string.Format("<b>NONE</b>"); break;
                }
                textTop.text = string.Format("{0}Resistance Activity{1}{2}{3}{4}", colourArc, colourEnd, debugID, "\n", fullItem);
                break;
            default:
                textTop.text = "Unknown";
                break;
        }

        
        //Ongoing effects -> Bottom text
        StringBuilder builderOngoing = new StringBuilder();
        if (listOfOngoingEffects != null & listOfOngoingEffects.Count > 0)
        {
            foreach (EffectDataOngoing effect in listOfOngoingEffects)
            {
                if (builderOngoing.Length > 0) { builderOngoing.AppendLine(); }
                if (string.IsNullOrEmpty(effect.text) == false)
                { builderOngoing.AppendFormat("{0}effect.text{1}{2}", colourNeutral, colourEnd, "\n"); }
                builderOngoing.AppendFormat("Security {0}{1}{2}{3}{4}", effect.value > 0 ? colourGood : colourBad, effect.value > 0 ? "+" : "", effect.value,
                    colourEnd, "\n");
                builderOngoing.AppendFormat("For {0}{1}{2} more turns", colourNeutral, effect.timer, colourEnd);
            }
        }
        else { builderOngoing.Append("<size=90%>No Ongoing effects present</size>"); }
        
        textBottom.text = builderOngoing.ToString();
        //debug or Activity data -> Middle text
        textMiddle.text = string.Format("{0}{1}{2}", colourNormal, textMain, colourEnd);
        //convert coordinates
        Vector3 screenPos = Camera.main.WorldToScreenPoint(tooltipPos);
        screenPos.x += 25;
        screenPos.y -= 100;
        Canvas.ForceUpdateCanvases();
        rectTransform = tooltipConnectionObject.GetComponent<RectTransform>();
        //get dimensions of dynamic tooltip
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        //calculate offset - height (default above)
        if (screenPos.y + height + offset < Screen.height)
        { screenPos.y += height / 2 + offset; }
        else { screenPos.y -= height / 2 + offset; }
        //width - default centred
        if (screenPos.x + width / 2 >= Screen.width)
        { screenPos.x -= width / 2 + screenPos.x - Screen.width; }
        else if (screenPos.x - width / 2 <= 0)
        { screenPos.x += width / 2 - screenPos.x; }
        //set new position
        tooltipConnectionObject.transform.position = screenPos;
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
    { return tooltipConnectionObject.activeSelf; }

    /// <summary>
    /// close tool tip
    /// </summary>
    public void CloseTooltip(string callingMethod = "Unknown")
    {
        Debug.LogFormat("[UI] TooltipConnection -> CloseTooltip: called by {0}{1}", callingMethod, "\n");
        tooltipConnectionObject.SetActive(false);
    }


    /// <summary>
    /// set up sprites on Connection tooltip for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    public void InitialiseTooltip(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = tooltipConnectionObject.GetComponent<Image>();
        //assign side specific sprites
        switch (side.level)
        {
            case 1:
                //Authority
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundAuthority;
                dividerTop.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerBottom.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                break;
            case 2:
                //Resistance
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundRebel;
                dividerTop.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerBottom.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", side.name));
                break;
        }
    }

}
