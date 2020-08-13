using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles Story tooltips (explanatory background info)
/// </summary>
public class TooltipStory : MonoBehaviour
{
    public Canvas storyCanvas;

    public GameObject storyObject;
    public Image storyImage;

    public TextMeshProUGUI textHeader;
    public TextMeshProUGUI textTop;
    public TextMeshProUGUI textMiddle;
    public TextMeshProUGUI textBottom;

    private static TooltipStory tooltipStory;

    private RectTransform rectTransform;
    private int offset;

    /// <summary>
    /// provide a static reference to the Story Tooltip that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TooltipStory Instance()
    {
        if (!tooltipStory)
        {
            tooltipStory = FindObjectOfType(typeof(TooltipStory)) as TooltipStory;
            if (!tooltipStory)
            { Debug.LogError("There needs to be one active TooltipStory script on a GameObject in your scene"); }
        }
        return tooltipStory;
    }

    /// <summary>
    /// Initialisation
    /// </summary>
    private void Start()
    {
        rectTransform = storyObject.GetComponent<RectTransform>();
        offset = GameManager.i.guiScript.tooltipOffset;
        Debug.Assert(offset > 0, "Invalid offset (Zero)");
        Debug.Assert(storyCanvas != null, "Invalid storyCanvas (Null)");
        Debug.Assert(storyObject != null, "Invalid storyObject (Null)");
        Debug.Assert(storyImage != null, "Invalid storyImage (Null)");
        Debug.Assert(textHeader != null, "Invalid textHeader (Null)");
        Debug.Assert(textTop != null, "Invalid textTop (Null)");
        Debug.Assert(textMiddle != null, "Invalid textMiddle (Null)");
        Debug.Assert(textBottom != null, "Invalid textBottom (Null)");
        //deactive storyObject at start otherwise tooltip won't fire
        storyObject.SetActive(false);
    }

    #region SetTooltip
    /// <summary>
    /// Initialise Help Tool tip. General Purpose. Can take one to four text topics and auto divides them as necessary. Topics are displayed in their list order
    /// Colours are set by the calling method
    /// </summary>
    public void SetTooltip(StoryHelp storyHelp, Vector3 screenPos)
    {
        //exit any tooltip that might be open
        GameManager.i.tooltipGenericScript.CloseTooltip("TooltipHelp.cs -> OnPointerEnter");
        if (storyHelp != null)
        {
            //open panel at start
            storyCanvas.gameObject.SetActive(true);
            storyObject.SetActive(true);
            //populate texts if valid data present
            textHeader.text = GameManager.Formatt(storyHelp.tag, ColourType.salmonText);
            //top
            if (storyHelp.textTop != null && storyHelp.textTop.Length > 0)
            {
                textTop.gameObject.SetActive(true);
                textTop.text = storyHelp.textTop;
            }
            //middle
            if (storyHelp.textMiddle != null && storyHelp.textMiddle.Length > 0)
            {
                textMiddle.gameObject.SetActive(true);
                textMiddle.text = storyHelp.textMiddle;
            }
            //bottom
            if (storyHelp.textBottom != null && storyHelp.textBottom.Length > 0)
            {
                textBottom.gameObject.SetActive(true);
                textBottom.text = storyHelp.textBottom;
            }
            //sprite
            storyImage.sprite = storyHelp.sprite;
            //update rectTransform to get a correct height as it changes every time with the dynamic menu resizing depending on number of buttons
            Canvas.ForceUpdateCanvases();
            rectTransform = storyObject.GetComponent<RectTransform>();
            //get dimensions of dynamic tooltip
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;
            float difference_y = screenPos.y - halfHeight;
            //y pos (from top of screen)
            if (screenPos.y + halfHeight >= Screen.height)
            { screenPos.y -= (screenPos.y + halfHeight - Screen.height) - offset; } //NOTE: change from '+' offset to '-' offset
            //y pos (from bottom of screen)
            else if (difference_y <= 0)
            { screenPos.y += difference_y * -1 + offset; }
            //x pos
            if (screenPos.x + width >= Screen.width)
            { screenPos.x -= (width * 2 + screenPos.x - Screen.width); }
            //minimum position of tooltip from Left Hand side is half width
            else if (screenPos.x <= halfWidth)
            { screenPos.x = halfWidth; }
            else { screenPos.x += width / 4; } //never applies, dead code

            //set new position
            storyObject.transform.position = screenPos;
            Debug.LogFormat("[UI] TooltipStoryHelp.cs -> SetTooltip{0}", "\n");
        }
        else { Debug.LogWarning("Invalid storyHelp (Empty)"); }
    }
    #endregion

    /// <summary>
    /// close tool tip. Provide an optional string showing calling method
    /// </summary>
    public void CloseTooltip(string text = "Unknown")
    {
        Debug.LogFormat("[UI] TooltipStoryHelp -> CloseTooltip: called by {0}{1}", text, "\n");
        storyObject.SetActive(false);
        storyCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// returns true if tooltip is active and visible
    /// </summary>
    /// <returns></returns>
    public bool CheckTooltipActive()
    { return storyObject.activeSelf; }


    //new methods above here
}
