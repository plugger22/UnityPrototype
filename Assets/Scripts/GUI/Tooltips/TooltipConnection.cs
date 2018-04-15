using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using gameAPI;
using packageAPI;

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
    private string colourDefault;
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
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent);
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
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
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
        //open panel at start
        tooltipConnectionObject.SetActive(true);
        //set opacity to zero (invisible)
        SetOpacity(0f);
        //set state of all items in tooltip window
        textTop.gameObject.SetActive(true);
        textBottom.gameObject.SetActive(false);
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
        string connText;
        switch (securityLevel)
        {
            case ConnectionType.HIGH:
                connText = string.Format("{0}<b>HIGH</b>{1}", colourBad, colourEnd);
                break;
            case ConnectionType.MEDIUM:
                connText = string.Format("{0}<b>MEDIUM</b>{1}", colourNeutral, colourEnd);
                break;
            case ConnectionType.LOW:
                connText = string.Format("{0}<b>LOW</b>{1}", colourGood, colourEnd);
                break;
            case ConnectionType.None:
                connText = string.Format("<b>NONE</b>");
                break;
            default:
                connText = string.Format("<b>Unknown</b>");
                break;
        }
        textTop.text = string.Format("{0}Connection{1}{2}{3}{4} Security{5}", colourArc, colourEnd, "\n", connText, securityLevel, colourEnd);

        //convert coordinates
        Vector3 screenPos = Camera.main.WorldToScreenPoint(tooltipPos);
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
        Debug.Log(string.Format("UI: Open -> TooltipConnection connID {0}{1}", connID, "\n"));
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
    public void CloseTooltip()
    {
        Debug.Log("UI: Close -> TooltipActor" + "\n");
        tooltipConnectionObject.SetActive(false);
    }


    /// <summary>
    /// set up sprites on actor tooltip for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    public void InitialiseTooltip(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = tooltipConnectionObject.GetComponent<Image>();
        //assign side specific sprites
        switch (side.name)
        {
            case "Authority":
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundAuthority;
                dividerTop.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerBottom.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                break;
            case "Resistance":
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
