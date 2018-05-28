using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// handles mouse interactions for City Info tab (far left) (2D polygon collider attached to game object)
/// </summary>
public class WidgetCityMouseUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip
    private float mouseOverFade;                        //tooltip
    private Coroutine myCoroutine;

    [HideInInspector] public string tooltipHeader;
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipDetails;


    public void Start()
    {
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        mouseOverFade = GameManager.instance.tooltipScript.tooltipFade;
    }

    /// <summary>
    /// Mouse over generic tooltip
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //check modal block isn't in place
        if (GameManager.instance.guiScript.CheckIsBlocked() == false)
        {
            //Tool tip
            onMouseFlag = true;
            //exit any node tooltip that might be open
            /*StopCoroutine("ShowTooltip"); */
            GameManager.instance.tooltipNodeScript.CloseTooltip();
            //start tooltip routine
            myCoroutine = StartCoroutine("ShowTooltip");
        }
    }


    /// <summary>
    /// mouse over exit, shut down tooltip
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameManager.instance.guiScript.CheckIsBlocked() == false)
        {
            onMouseFlag = false;
            if (myCoroutine != null)
            { StopCoroutine(myCoroutine); }
            GameManager.instance.tooltipGenericScript.CloseTooltip();
        }
    }

    /// <summary>
    /// Mouse click -> Right: Actor Action Menu
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
            case PointerEventData.InputButton.Right:
                if (GameManager.instance.guiScript.CheckIsBlocked() == false)
                {
                    //activate city Info Display
                    EventManager.instance.PostNotification(EventType.CityInfoOpen, this, GameManager.instance.cityScript.GetCity());
                }
                break;
            default:
                Debug.LogError("Unknown InputButton");
                break;
        }
    }

    /// <summary>
    /// Show generic tooltip
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over button
        if (onMouseFlag == true)
        {
            //do once
            Vector3 screenPos = transform.position;
            screenPos.x -= 280;
            screenPos.y -= 90;
            while (GameManager.instance.tooltipGenericScript.CheckTooltipActive() == false && GameManager.instance.guiScript.CheckIsBlocked() == false)
            {
                tooltipHeader = string.Format("{0}{1}", GameManager.instance.cityScript.GetCityName(), GameManager.instance.cityScript.GetCityArc());
                tooltipMain = GameManager.instance.cityScript.GetCityDescription();
                GameManager.instance.tooltipGenericScript.SetTooltip(tooltipMain, screenPos, tooltipHeader);
                yield return null;
            }
            //fade in
            float alphaCurrent;
            while (GameManager.instance.tooltipGenericScript.GetOpacity() < 1.0)
            {
                alphaCurrent = GameManager.instance.tooltipGenericScript.GetOpacity();
                alphaCurrent += Time.deltaTime / mouseOverFade;
                GameManager.instance.tooltipGenericScript.SetOpacity(alphaCurrent);
                yield return null;
            }
        }
    }

}
