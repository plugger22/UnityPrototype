using modalAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using packageAPI;
using gameAPI;

/// <summary>
/// handles Generic tooltips. Text only. Attach to PanelManager
/// </summary>
public class TooltipGeneric : MonoBehaviour
{
    public TextMeshProUGUI genericHeader;               //header (optional)
    public TextMeshProUGUI genericText;                 //main text (Required)
    public TextMeshProUGUI genericDetail;               //specific detail (optional)
    public Image dividerTop;                            //use depends on text components
    public Image dividerBottom;                         //use depends on text components
    public GameObject tooltipGenericObject;

    private static TooltipGeneric tooltipGeneric;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private float fadeInTime;
    private int offset;                         //TooltipManager.cs -> Offset. Used to move tooltip down from screen top if required (otherwise sits right on edge)
    private GenericTooltipType tooltipType;

    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        canvasGroup = tooltipGenericObject.GetComponent<CanvasGroup>();
        rectTransform = tooltipGenericObject.GetComponent<RectTransform>();
        fadeInTime = GameManager.instance.guiScript.tooltipFade / 2;
        offset = GameManager.instance.guiScript.tooltipOffset;
        Debug.Assert(offset > 0, "Invalid vertical Offset (zero, or less)");
        Debug.Assert(genericHeader != null, "Invalid genericHeader (Null)");
        Debug.Assert(genericText != null, "Invalid genericText (Null)");
        Debug.Assert(genericDetail != null, "Invalid genericDetail (Null)");
        Debug.Assert(dividerTop != null, "Invalid dividerTop (Null)");
        Debug.Assert(dividerBottom != null, "Invalid dividerBottom (Null)");
        Debug.Assert(tooltipGenericObject != null, "Invalid tooltipGenericOjbect (Null)");

    }

    /// <summary>
    /// provide a static reference to the Generic Tooltip that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TooltipGeneric Instance()
    {
        if (!tooltipGeneric)
        {
            tooltipGeneric = FindObjectOfType(typeof(TooltipGeneric)) as TooltipGeneric;
            if (!tooltipGeneric)
            { Debug.LogError("There needs to be one active TooltipGeneric script on a GameObject in your scene"); }
        }
        return tooltipGeneric;
    }


    /// <summary>
    /// Initialise Generic Tool tip. General Purpose. Can take one to three text segments and auto divides them as necessary.
    /// Unique to the Generic tool tip, colours are set by the calling method
    /// </summary>
    public void SetTooltip(GenericTooltipData data)
    {
        //open panel at start
        tooltipGenericObject.SetActive(true);
        genericText.gameObject.SetActive(true);
        genericHeader.gameObject.SetActive(false);
        genericDetail.gameObject.SetActive(false);
        dividerTop.gameObject.SetActive(false);
        dividerBottom.gameObject.SetActive(false);
        /*//set opacity to zero (invisible)
        SetOpacity(0f);*/
        //set state of all items in tooltip window
        genericText.text = data.main;

        //header
        if (string.IsNullOrEmpty(data.header) == false)
        {
            genericHeader.text = data.header;
            genericHeader.gameObject.SetActive(true);
            dividerTop.gameObject.SetActive(true);
        }
        //details
        if (string.IsNullOrEmpty(data.details) == false)
        {
            genericDetail.text = data.details;
            genericDetail.gameObject.SetActive(true);
            dividerBottom.gameObject.SetActive(true);
        }
        //type
        tooltipType = data.tooltipType;
        //update rectTransform to get a correct height as it changes every time with the dynamic menu resizing depending on number of buttons
        Canvas.ForceUpdateCanvases();
        rectTransform = tooltipGenericObject.GetComponent<RectTransform>();
        //get dimensions of dynamic tooltip
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        float halfWidth = width * 0.5f;
        //place tooltip adjacent to the button
        data.screenPos.y -= height / 4;
        if (data.screenPos.y + height >= Screen.height)
        { data.screenPos.y -= (data.screenPos.y + height - Screen.height) + offset; }
        //to the right
        if (data.screenPos.x + width >= Screen.width)
        { data.screenPos.x -= (width * 2 + data.screenPos.x - Screen.width); }

        /*else if (screenPos.x - width <= 0)
        { screenPos.x += width - screenPos.x; }*/

        //minimum position of tooltip from Left Hand side is half width
        else if (data.screenPos.x <= halfWidth)
        { data.screenPos.x = halfWidth; }
        else { data.screenPos.x += width / 4; } //never applies, dead code

        //set new position
        tooltipGenericObject.transform.position = data.screenPos;
        Debug.LogFormat("[UI] TooltipGeneric.cs -> SetTooltip{0}", "\n");

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
    { return tooltipGenericObject.activeSelf; }

    /// <summary>
    /// close tool tip. Provide an optional string showing calling method. 'tooltipType' is the tooltip Type to close (default 'Any' but you can specify a particular type to close and to ignore the rest)
    /// </summary>
    public void CloseTooltip(string text = "Unknown", GenericTooltipType tooltipTypeToClose = GenericTooltipType.Any)
    {
        bool isProceed = true;
        Debug.LogFormat("[Tst] TooltipGeneric.cs -> CloseTooltip: \"{0}\", tooltipTypeToClose \"{1}\", current tooltipType \"{2}\"", text, tooltipTypeToClose, tooltipType);
        if (tooltipTypeToClose != GenericTooltipType.Any)
        {
            //only close if current tooltip is specified type
            if (tooltipType != tooltipTypeToClose)
            { isProceed = false; }
        }
        if (isProceed == true)
        {
            //close tooltip
            Debug.LogFormat("[UI] TooltipGeneric -> CloseTooltip: called by {0}{1}", text, "\n");
            tooltipGenericObject.SetActive(false);
        }
    }


}
