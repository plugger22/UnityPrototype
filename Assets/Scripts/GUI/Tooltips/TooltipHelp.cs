using packageAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles explanatory help tooltips, text only. Attached to Panel Manager. Operates in Modal 1 state if required (due to position in Heirarchy).
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
    private int numOfTopics = 4;

    private GameObject[] arrayOfObjects;
    private TextMeshProUGUI[] arrayOfHeaders;
    private TextMeshProUGUI[] arrayOfTexts;


    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        rectTransform = tooltipHelpObject.GetComponent<RectTransform>();
        offset = GameManager.instance.tooltipScript.tooltipOffset;
        Debug.Assert(offset > 0, "Invalid offset (Zero)");
        Debug.Assert(tooltipHelpObject != null, "Invalid tooltipHelpOjbect (Null)");
        Debug.Assert(topicObject_0 != null, "Invalid topicOjbect_0 (Null)");
        Debug.Assert(topicObject_1 != null, "Invalid topicOjbect_1 (Null)");
        Debug.Assert(topicObject_2 != null, "Invalid topicOjbect_2 (Null)");
        Debug.Assert(topicObject_3 != null, "Invalid topicOjbect_3 (Null)");
        Debug.Assert(headerTopic_0 != null, "Invalid headerTopic_0 (Null)");
        Debug.Assert(headerTopic_1 != null, "Invalid headerTopic_1 (Null)");
        Debug.Assert(headerTopic_2 != null, "Invalid headerTopic_2 (Null)");
        Debug.Assert(headerTopic_3 != null, "Invalid headerTopic_3 (Null)");
        Debug.Assert(textTopic_0 != null, "Invalid textTopic_0 (Null)");
        Debug.Assert(textTopic_1 != null, "Invalid textTopic_1 (Null)");
        Debug.Assert(textTopic_2 != null, "Invalid textTopic_2 (Null)");
        Debug.Assert(textTopic_3 != null, "Invalid textTopic_3 (Null)");
        //arrrays
        arrayOfObjects = new GameObject[numOfTopics];
        arrayOfHeaders = new TextMeshProUGUI[numOfTopics];
        arrayOfTexts = new TextMeshProUGUI[numOfTopics];
        //populate arrays
        arrayOfObjects[0] = topicObject_0;
        arrayOfObjects[1] = topicObject_1;
        arrayOfObjects[2] = topicObject_2;
        arrayOfObjects[3] = topicObject_3;
        arrayOfHeaders[0] = headerTopic_0;
        arrayOfHeaders[1] = headerTopic_1;
        arrayOfHeaders[2] = headerTopic_2;
        arrayOfHeaders[3] = headerTopic_3;
        arrayOfTexts[0] = textTopic_0;
        arrayOfTexts[1] = textTopic_1;
        arrayOfTexts[2] = textTopic_2;
        arrayOfTexts[3] = textTopic_3;
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
    /// Initialise Help Tool tip. General Purpose. Can take one to four text topics and auto divides them as necessary. Topics are displayed in their list order
    /// Colours are set by the calling method
    /// </summary>
    public void SetTooltip(List<HelpData> listOfHelpData, Vector3 screenPos)
    {
        //exit any node tooltip that might be open
        GameManager.instance.tooltipNodeScript.CloseTooltip("TooltipHelp.cs -> OnPointerEnter");
        GameManager.instance.tooltipGenericScript.CloseTooltip("TooltipHelp.cs -> OnPointerEnter");
        //number of topics
        int count = listOfHelpData.Count;
        if (count > 0)
        {
            //can't have more than 4 topics
            count = Mathf.Min(4, count);
            //open panel at start
            tooltipHelpObject.SetActive(true);
            //populate topics where required
            for (int index = 0; index < numOfTopics; index++)
            {
                if (index < count)
                {
                    //activate topic
                    arrayOfObjects[index].SetActive(true);
                    //populate header and text
                    arrayOfHeaders[index].text = listOfHelpData[index].header;
                    arrayOfTexts[index].text = listOfHelpData[index].text;
                }
                else
                {
                    //deactivate topic
                    arrayOfObjects[index].SetActive(false);
                }
            }

            //update rectTransform to get a correct height as it changes every time with the dynamic menu resizing depending on number of buttons
            Canvas.ForceUpdateCanvases();
            rectTransform = tooltipHelpObject.GetComponent<RectTransform>();
            //get dimensions of dynamic tooltip
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;
            float halfWidth = width * 0.5f;
            //y pos
            if (screenPos.y + height >= Screen.height)
            { screenPos.y -= (screenPos.y + height - Screen.height) - offset; } //NOTE: change from '+' offset to '-' offset
            //x pos
            if (screenPos.x + width >= Screen.width)
            { screenPos.x -= (width * 2 + screenPos.x - Screen.width); }
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


    /// <summary>
    /// returns true if tooltip is active and visible
    /// </summary>
    /// <returns></returns>
    public bool CheckTooltipActive()
    { return tooltipHelpObject.activeSelf; }

    //new methods above here
}
