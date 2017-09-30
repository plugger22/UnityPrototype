using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Node based tooltip
/// </summary>
public class TooltipNode : MonoBehaviour
{
    public TextMeshProUGUI nodeName;
    public TextMeshProUGUI nodeType;
    public TextMeshProUGUI nodeActive;
    public TextMeshProUGUI nodeStats;
    public TextMeshProUGUI nodeTarget;

    public Image dividerTop;                   //Side specific sprites for tooltips
    public Image dividerMiddle;
    public Image dividerBottom;

    public GameObject tooltipNodeObject;

    private Image background;
    private static TooltipNode tooltipNode;
    RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private float fadeInTime;
    private int offset;

    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        canvasGroup = tooltipNodeObject.GetComponent<CanvasGroup>();
        rectTransform = tooltipNodeObject.GetComponent<RectTransform>();
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade;
        offset = GameManager.instance.tooltipScript.tooltipOffset;

    }

    /// <summary>
    /// provide a static reference to tooltipNode that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TooltipNode Instance()
    {
        if (!tooltipNode)
        {
            tooltipNode = FindObjectOfType(typeof(TooltipNode)) as TooltipNode;
            if (!tooltipNode)
            { Debug.LogError("There needs to be one active TooltipNode script on a GameObject in your scene"); }
        }
        return tooltipNode;
    }

    /// <summary>
    /// Initialise node Tool tip
    /// </summary>
    /// <param name="name">Name of Node, eg. 'Downtown Manhattan'</param>
    /// <param name="type">Type of Node, eg. Sprawl, government, corporate (auto converted to CAPS)</param>
    /// <param name="listOfActive">place actor type here if node is active for them, eg. 'Hacker'. No limit to actors</param>
    /// <param name="arrayOfStats">Give stats as Ints[3] in order Stability - Support - Security</param>
    /// <param name="listOfTarget">place target info here, a blank list if none</param>
    /// <param name="pos">Position of tooltip originator -> Use a transform position (world units), not an Input.MousePosition (screen units)</param>
    public void SetTooltip(string name, string type, List<string> listOfActive, int[] arrayOfStats, List<string> listOfTarget, Vector3 pos)
    {
        //open panel at start
        tooltipNodeObject.SetActive(true);
        //set opacity to zero (invisible)
        SetOpacity(0f);
        //set state of all items in tooltip window
        nodeName.gameObject.SetActive(true);
        nodeType.gameObject.SetActive(true);
        nodeStats.gameObject.SetActive(true);
        dividerTop.gameObject.SetActive(true);
        //show only if node active for at least one actor
        nodeActive.gameObject.SetActive(false);
        dividerMiddle.gameObject.SetActive(false);
        //show only if node has a target
        nodeTarget.gameObject.SetActive(false);
        dividerBottom.gameObject.SetActive(false);
        //set up tooltipNode object
        nodeName.text = name;
        nodeType.text = type.ToUpper();

        //list of Actors for whom the is node is Active
        if (listOfActive.Count > 0)
        {
            //set tooltip elements to show
            nodeActive.gameObject.SetActive(true);
            dividerMiddle.gameObject.SetActive(true);
            //build string
            StringBuilder builder = new StringBuilder();
            foreach(string active in listOfActive)
            {
                if (builder.Length > 0) { builder.AppendLine(); }
                builder.Append(string.Format("{0}{1}{2}", "<color=yellow>", active, "</color>"));
            }
            nodeActive.text = builder.ToString();
        }

        //Node has an identified target
        if (listOfTarget.Count > 0)
        {
            //set tooltip elements to show
            nodeTarget.gameObject.SetActive(true);
            dividerBottom.gameObject.SetActive(true);
            //build string
            StringBuilder builder = new StringBuilder();
            foreach (string target in listOfTarget)
            {
                if (builder.Length > 0) { builder.AppendLine(); }
                builder.Append(target);
            }
            nodeTarget.text = builder.ToString();
        }

        //set up stats -> only takes the first three Stability - Support - Security
        int checkCounter = 0;
        int data;
        if (arrayOfStats.Length > 0)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < arrayOfStats.Length; i++)
            {
                data = arrayOfStats[i];
                if (i > 0) { builder.AppendLine(); }
                switch(data)
                {
                    case 3:
                        //good -> green
                        builder.Append(string.Format(string.Format("{0}{1}{2}", "<color=#0bff01>", data, "</color>")));
                        break;
                    case 2:
                        //average -> yellow
                        builder.Append(string.Format(string.Format("{0}{1}{2}", "<color=#fdfe02>", data, "</color>")));
                        break;
                    case 1:
                        //bad -> red (Security runs in reverse so that level 1 security is the highest)
                        builder.Append(string.Format(string.Format("{0}{1}{2}", "<color=#fe0000>", data, "</color>")));
                        break;
                    default:
                        builder.Append(data);
                        break;
                }
                //idiot check to handle case of being too many stats
                checkCounter++;
                if (checkCounter >= 3) { break; }
            }
            nodeStats.text = builder.ToString();
        }

        //convert coordinates
        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        //get dimensions of dynamic tooltip
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        //height showing zero due to vertical layout group for first call
        if (height == 0)
        {
            Canvas.ForceUpdateCanvases();
            height = rectTransform.rect.height;
        }
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
        tooltipNodeObject.transform.position = screenPos;

        Debug.Log("Node ID " + name + " Tooltip active" + "\n");
    }

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
    { return tooltipNodeObject.activeSelf; }

    /// <summary>
    /// close tool tip
    /// </summary>
    public void CloseTooltip()
    {
        Debug.Log("Close: TooltipNode Active? -> " + tooltipNodeObject.activeSelf + "\n");
        tooltipNodeObject.SetActive(false);
    }


    public void SetOpacity(float opacity)
    { canvasGroup.alpha = opacity; }

    public float GetOpacity()
    { return canvasGroup.alpha; }


    /// <summary>
    /// set up sprites on node tooltip for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    public void InitialiseTooltip(Side side)
    {
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = tooltipNodeObject.GetComponent<Image>();
        //assign side specific sprites
        switch (side)
        {
            case Side.Authority:
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundAuthority;
                dividerTop.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerMiddle.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerBottom.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                break;
            case Side.Rebel:
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundRebel;
                dividerTop.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerMiddle.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerBottom.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                break;
        }
    }

}
