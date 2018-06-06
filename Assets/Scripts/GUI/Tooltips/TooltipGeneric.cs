using modalAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        canvasGroup = tooltipGenericObject.GetComponent<CanvasGroup>();
        rectTransform = tooltipGenericObject.GetComponent<RectTransform>();
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade / 2;
        offset = GameManager.instance.tooltipScript.tooltipOffset;
        //debug
        Debug.Assert(offset > 0, "Invalid vertical Offset (zero, or less)");
        //offset = GameManager.instance.tooltipScript.tooltipOffset;

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
    /// Initialise node Tool tip. General Purpose. Can take one to three text segments and auto divides them as necessary.
    /// Unique to the Generic tool tip, colours are set by the calling method
    /// </summary>
    /// <param name="tooltipText">Tool tip text (any length as autoresizes). Required</param>
    /// <param name="pos">Position of tooltip originator -> Use a transform position (it's screen units as it's derived from a UI element))</param>
    /// <param name="headerText">Optional</param>
    /// <param name="detailText">Optional</param>
    public void SetTooltip(string mainText, Vector3 screenPos, string headerText = null, string detailText = null)
    {
        //open panel at start
        tooltipGenericObject.SetActive(true);
        genericText.gameObject.SetActive(true);
        genericHeader.gameObject.SetActive(false);
        genericDetail.gameObject.SetActive(false);
        dividerTop.gameObject.SetActive(false);
        dividerBottom.gameObject.SetActive(false);
        //set opacity to zero (invisible)
        SetOpacity(0f);
        //set state of all items in tooltip window
        genericText.text = mainText;

        //header
        if (String.IsNullOrEmpty(headerText) == false)
        {
            genericHeader.text = headerText;
            genericHeader.gameObject.SetActive(true);
            dividerTop.gameObject.SetActive(true);
        }
        //details
        if (String.IsNullOrEmpty(detailText) == false)
        {
            genericDetail.text = detailText;
            genericDetail.gameObject.SetActive(true);
            dividerBottom.gameObject.SetActive(true);
        }
        //update rectTransform to get a correct height as it changes every time with the dynamic menu resizing depending on number of buttons
        Canvas.ForceUpdateCanvases();
        rectTransform = tooltipGenericObject.GetComponent<RectTransform>();
        //get dimensions of dynamic tooltip
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        //place tooltip adjacent to the button
        screenPos.y -= height / 4;
        if (screenPos.y + height >= Screen.height)
        { screenPos.y -= (screenPos.y + height - Screen.height) + offset; }
        //to the right
        if (screenPos.x + width >= Screen.width)
        { screenPos.x -= (width * 2 + screenPos.x - Screen.width); }
        else if (screenPos.x - width <= 0)
        { screenPos.x += width - screenPos.x; }
        else { screenPos.x += width / 4; }

        //set new position
        tooltipGenericObject.transform.position = screenPos;
        Debug.Log("UI: Open -> TooltipGeneric" + "\n");

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
    /// close tool tip
    /// </summary>
    public void CloseTooltip()
    {
        Debug.Log("UI: Close -> TooltipGeneric" + "\n");
        tooltipGenericObject.SetActive(false);
    }


}
