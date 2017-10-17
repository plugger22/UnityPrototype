using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using gameAPI;

/// <summary>
/// Actor based tooltip, reference static instance in GameManager
/// </summary>
public class TooltipActor : MonoBehaviour
{
    public TextMeshProUGUI actorName;
    public TextMeshProUGUI actorStats;
    public TextMeshProUGUI actorTrait;
    public Image dividerTop;                   //Side specific sprites for tooltips
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
    private string colourTrait;
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
        EventManager.instance.AddListener(EventType.ChangeColour, this.OnEvent);
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent);
    }

    /// <summary>
    /// provide a static reference to tooltipNode that can be accessed from any script
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
                InitialiseTooltip((Side)Param);
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
        colourTrait = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }


    /// <summary>
    /// Initialise node Tool tip
    /// </summary>
    /// <param name="name">Name of Node, eg. 'Downtown Manhattan'</param>
    /// <param name="arrayOfStats">Give stats as Ints[3] in order Stability - Support - Security</param>
    /// <param name="trait">place target info here, a blank list if none</param>
    /// <param name="pos">Position of tooltip originator -> note as it's a UI element transform will be in screen units, not world units</param>
    public void SetTooltip(string name, int[] arrayOfStats, string trait, Vector3 screenPos, float width, float height)
    {

        //open panel at start
        tooltipActorObject.SetActive(true);
        //set opacity to zero (invisible)
        SetOpacity(0f);
        //set state of all items in tooltip window
        actorName.gameObject.SetActive(true);
        actorStats.gameObject.SetActive(true);
        actorTrait.gameObject.SetActive(true);
        dividerTop.gameObject.SetActive(true);
        dividerBottom.gameObject.SetActive(true);
        actorName.text = string.Format("{0}{1}{2}", colourName, name, colourEnd);
        actorTrait.text = string.Format("{0}{1}{2}", colourTrait, trait, colourEnd);


        //set up stats -> only takes the first three Connections / Motivation / Invisibility
        int checkCounter = 0;
        int data;
        if (arrayOfStats.Length > 0)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < arrayOfStats.Length; i++)
            {
                data = arrayOfStats[i];
                if (i > 0) { builder.AppendLine(); }
                switch (data)
                {
                    case 3:
                        //good -> green
                        builder.Append(string.Format(string.Format("{0}{1}{2}", colourGood, data, colourEnd)));
                        break;
                    case 2:
                        //average -> yellow
                        builder.Append(string.Format(string.Format("{0}{1}{2}", colourNeutral, data, colourEnd)));
                        break;
                    case 1:
                        //bad -> red (Security runs in reverse so that level 1 security is the highest)
                        builder.Append(string.Format(string.Format("{0}{1}{2}", colourBad, data, colourEnd)));
                        break;
                    default:
                        builder.Append(data);
                        break;
                }
                //idiot check to handle case of being too many stats
                checkCounter++;
                if (checkCounter >= 3) { break; }
            }
            actorStats.text = builder.ToString();
        }

        //Debug.Log("ScreenPos " + screenPos.x + " : " + screenPos.y + "  W: " + width + " H: " + height + "\n");

        //calculate offset - height (default above)
        screenPos.y += height / 2 + offset * 3;
        screenPos.x -= width / 10;
        //width
        if (screenPos.x + width / 2 >= Screen.width)
        { screenPos.x -= width / 2 + screenPos.x - Screen.width; }
        else if (screenPos.x - width / 2 <= 0)
        { screenPos.x += width / 2 - screenPos.x; }
        //set new position
        tooltipActorObject.transform.position = screenPos;

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
    { return tooltipActorObject.activeSelf; }

    /// <summary>
    /// close tool tip
    /// </summary>
    public void CloseTooltip()
    {
        Debug.Log("UI: Close -> TooltipActor" + "\n");
        tooltipActorObject.SetActive(false);
    }


    /// <summary>
    /// set up sprites on actor tooltip for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    public void InitialiseTooltip(Side side)
    {
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = tooltipActorObject.GetComponent<Image>();
        //assign side specific sprites
        switch (side)
        {
            case Side.Authority:
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundAuthority;
                dividerTop.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerBottom.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                break;
            case Side.Rebel:
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundRebel;
                dividerTop.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerBottom.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                break;
        }
    }

}
