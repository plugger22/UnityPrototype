using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using packageAPI;

/// <summary>
/// handles mouse interactions for City Info tab (far left) (2D polygon collider attached to game object)
/// </summary>
public class WidgetCityMouseUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip
    //private float mouseOverFade;                        //tooltip
    private Coroutine myCoroutine;

    [HideInInspector] public string tooltipHeader;
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipDetails;


    public void Start()
    {
        mouseOverDelay = GameManager.i.guiScript.tooltipDelay;
        //mouseOverFade = GameManager.instance.tooltipScript.tooltipFade;
    }

    /// <summary>
    /// Mouse over generic tooltip
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //check modal block isn't in place
        if (GameManager.i.guiScript.CheckIsBlocked() == false)
        {
            //Tool tip
            onMouseFlag = true;
            //exit any node tooltip that might be open
            GameManager.i.tooltipNodeScript.CloseTooltip("WidgetCityMouseUI.cs -> OnPointerEnter");
            //start tooltip routine
            myCoroutine = StartCoroutine("ShowTooltip");
        }
    }


    /// <summary>
    /// mouse over exit, shut down tooltip
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameManager.i.guiScript.CheckIsBlocked() == false)
        {
            onMouseFlag = false;
            if (myCoroutine != null)
            {
                StopCoroutine(myCoroutine);
                myCoroutine = null;
            }
            GameManager.i.tooltipGenericScript.CloseTooltip("WidgetCityMouseUI.cs -> OnPointerExit");
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
                if (GameManager.i.guiScript.CheckIsBlocked() == false)
                {
                    //activate city Info Display
                    EventManager.instance.PostNotification(EventType.CityInfoOpen, this, GameManager.i.cityScript.GetCity(), "WidgetCityMouseUI.cs -> OnPointerClick");
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
        if (onMouseFlag == true && GameManager.i.inputScript.ModalState == ModalState.Normal)
        {
            //do once
            Vector3 screenPos = transform.position;
            screenPos.x -= 280;
            screenPos.y -= 90;
            while (GameManager.i.tooltipGenericScript.CheckTooltipActive() == false && GameManager.i.guiScript.CheckIsBlocked() == false)
            {
                tooltipHeader = string.Format("{0}{1}", GameManager.i.cityScript.GetCityNameFormatted(), GameManager.i.cityScript.GetCityArcFormatted());
                tooltipMain = GameManager.i.cityScript.GetCityDescriptionFormatted();
                tooltipDetails = GameManager.i.cityScript.GetCityDetails();
                GenericTooltipData data = new GenericTooltipData() { screenPos = screenPos, main = tooltipMain, header = tooltipHeader, details = tooltipDetails };
                GameManager.i.tooltipGenericScript.SetTooltip(data);
                yield return null;
            }
            /*//fade in
            float alphaCurrent;
            while (GameManager.instance.tooltipGenericScript.GetOpacity() < 1.0)
            {
                alphaCurrent = GameManager.instance.tooltipGenericScript.GetOpacity();
                alphaCurrent += Time.deltaTime / mouseOverFade;
                GameManager.instance.tooltipGenericScript.SetOpacity(alphaCurrent);
                yield return null;
            }*/
        }
    }

}
