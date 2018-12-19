using packageAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles explanatory help tooltips, text only. Attached to Panel Manager
/// </summary>
public class TooltipHelp : MonoBehaviour
{
    public GameObject tooltipHelpObject;
    public GameObject topicObject_0;
    public GameObject topicObject_1;
    public GameObject topicObject_2;
    public GameObject topicObject_3;

    public TextMeshProUGUI headerTopic_0;
    public TextMeshProUGUI headerTopic_1;
    public TextMeshProUGUI headerTopic_2;
    public TextMeshProUGUI headerTopic_3;

    public TextMeshProUGUI textTopic_0;
    public TextMeshProUGUI textTopic_1;
    public TextMeshProUGUI textTopic_2;
    public TextMeshProUGUI textTopic_3;

    private static TooltipHelp tooltipHelp;
    private RectTransform rectTransform;
    private int offset;


    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        rectTransform = tooltipHelpObject.GetComponent<RectTransform>();
        offset = GameManager.instance.tooltipScript.tooltipOffset;
        Debug.Assert(offset > 0, "Invalid offset (Zero)");

    }

    /// <summary>
    /// provide a static reference to the Generic Tooltip that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TooltipHelp Instance()
    {
        if (!tooltipHelp)
        {
            tooltipHelp = FindObjectOfType(typeof(TooltipHelp)) as TooltipHelp;
            if (!tooltipHelp)
            { Debug.LogError("There needs to be one active TooltipHelp script on a GameObject in your scene"); }
        }
        return tooltipHelp;
    }

    /// <summary>
    /// Initialise Help Tool tip. General Purpose. Can take one to three text segments and auto divides them as necessary.
    /// Unique to the Generic tool tip, colours are set by the calling method
    /// </summary>
    public void SetTooltip(List<HelpData> listOfHelpData, Vector3 screenPos)
    {
        int count = listOfHelpData.Count;
        if (count > 0)
        {
            //can't have more than 4 topics
            count = Mathf.Min(4, count);
            //open panel at start
            tooltipHelpObject.SetActive(true);
            //populate topics where required
            for (int i = 0; i < count; i++)
            {
                //activate topic

            }

            //update rectTransform to get a correct height as it changes every time with the dynamic menu resizing depending on number of buttons
            Canvas.ForceUpdateCanvases();
            rectTransform = tooltipHelpObject.GetComponent<RectTransform>();
            //get dimensions of dynamic tooltip
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;
            float halfWidth = width * 0.5f;
            //place tooltip adjacent to the button
            screenPos.y -= height / 4;
            if (screenPos.y + height >= Screen.height)
            { screenPos.y -= (screenPos.y + height - Screen.height) + offset; }
            //to the right
            if (screenPos.x + width >= Screen.width)
            { screenPos.x -= (width * 2 + screenPos.x - Screen.width); }

            /*else if (screenPos.x - width <= 0)
            { screenPos.x += width - screenPos.x; }*/

            //minimum position of tooltip from Left Hand side is half width
            else if (screenPos.x <= halfWidth)
            { screenPos.x = halfWidth; }
            else { screenPos.x += width / 4; } //never applies, dead code

            //set new position
            tooltipHelpObject.transform.position = screenPos;
            Debug.LogFormat("[UI] TooltipHelp.cs -> SetTooltip{0}", "\n");
        }
        else { Debug.LogWarning("Invalid listOfHelpData (Empty)"); }
    }


    /// <summary>
    /// close tool tip. Provide an optional string showing calling method
    /// </summary>
    public void CloseTooltip(string text = "Unknown")
    {
        Debug.LogFormat("[UI] TooltipHelp -> CloseTooltip: called by {0}{1}", text, "\n");
        tooltipHelpObject.SetActive(false);
    }

    //new methods above here
}
