﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using packageAPI;
using gameAPI;

/// <summary>
/// handles mouse interactions for top widget objective stars (2D polygon collider attached to game object)
/// </summary>
public class WidgetStarsMouseUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
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
    public void OnPointerEnter (PointerEventData eventData)
    {
        //check modal block isn't in place
        if (GameManager.i.guiScript.CheckIsBlocked() == false)
        {
            //Tool tip
            onMouseFlag = true;
            //exit any node tooltip that might be open
            GameManager.i.tooltipNodeScript.CloseTooltip("WidgetStarsMouseUI.cs -> OnPointerEnter");
            //start tooltip routine
            myCoroutine = StartCoroutine(ShowTooltip());
        }
    }

    public void OnPointerClick (PointerEventData eventData)
    {
        Debug.Log("Click!");
    }


    /// <summary>
    /// mouse over exit, shut down tooltip
    /// </summary>
    public void OnPointerExit (PointerEventData eventData)
    {
        if (GameManager.i.guiScript.CheckIsBlocked() == false)
        {
            onMouseFlag = false;
            if (myCoroutine != null)
            {
                StopCoroutine(myCoroutine);
                myCoroutine = null;
            }
            GameManager.i.tooltipGenericScript.CloseTooltip("WidgetStarsMouseUI.cs -> OnPointerExit");
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
            screenPos.y -= 125;
            while (GameManager.i.tooltipGenericScript.CheckTooltipActive() == false)
            {
                tooltipHeader = GameManager.i.objectiveScript.GetObjectivesTitle();
                tooltipMain = GameManager.i.objectiveScript.GetObjectivesSummary();
                tooltipDetails = GameManager.i.objectiveScript.GetObjectiveDetails();
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
