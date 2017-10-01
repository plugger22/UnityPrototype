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
    public TextMeshProUGUI genericText;
    public GameObject tooltipGenericObject;

    private static TooltipGeneric tooltipGeneric;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private float fadeInTime;
    private int offset;

    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        canvasGroup = tooltipGenericObject.GetComponent<CanvasGroup>();
        rectTransform = tooltipGenericObject.GetComponent<RectTransform>();
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade;
        offset = GameManager.instance.tooltipScript.tooltipOffset;

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
    /// Initialise node Tool tip
    /// </summary>
    /// <param name="tooltipText">Tool tip text (any length as autoresizes)</param>
    /// <param name="pos">Position of tooltip originator -> Use a transform position (it's screen units as it's derived from a UI element))</param>
    public void SetTooltip(string tooltipText, Vector3 screenPos)
    {
        //open panel at start
        tooltipGenericObject.SetActive(true);
        //set opacity to zero (invisible)
        SetOpacity(0f);
        //set state of all items in tooltip window
        genericText.gameObject.SetActive(true);
        genericText.text = tooltipText;

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
        if (screenPos.y + height / 2 < Screen.height)
        { screenPos.y += height / 2; }
        else { screenPos.y -= height / 2; }
        //screenPos.y += height / 2;

        screenPos.x -= width / 10;
        //width
        if (screenPos.x + width >= Screen.width)
        { screenPos.x -= width + screenPos.x - Screen.width; }
        else if (screenPos.x - width <= 0)
        { screenPos.x += width - screenPos.x; }


        /*
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
        { screenPos.x += width / 2 - screenPos.x; }*/

        //set new position
        tooltipGenericObject.transform.position = screenPos;

        Debug.Log("Node ID " + tooltipText + " Tooltip active" + "\n");
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
        Debug.Log("Close: TooltipGeneric Active? -> " + tooltipGenericObject.activeSelf + "\n");
        tooltipGenericObject.SetActive(false);
    }


}
