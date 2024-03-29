﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using packageAPI;

/// <summary>
/// handles mouse interactions for Faction level bar (right hand side) (2D polygon collider attached to game object)
/// </summary>
public class WidgetBarRightMouseUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip
    //private float mouseOverFade;                        //tooltip
    private Coroutine myCoroutine;
    private GlobalSide playerSide;

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
            GameManager.i.tooltipNodeScript.CloseTooltip("WidgetBarRightMouseUI.cs -> OnPointerEnter");
            //start tooltip routine
            myCoroutine = StartCoroutine("ShowTooltip");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click!");
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
            GameManager.i.tooltipGenericScript.CloseTooltip("WidgetBarRightMouseUI.cs -> OnPointerExit");
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
        playerSide = GameManager.i.sideScript.PlayerSide;
        //activate tool tip if mouse still over button
        if (onMouseFlag == true)
        {
            //do once
            Vector3 screenPos = transform.position;
            screenPos.y -= 55;
            while (GameManager.i.tooltipGenericScript.CheckTooltipActive() == false)
            {
                tooltipHeader = string.Format("{0} <size=115%>Approval</size>", GameManager.i.hqScript.GetHqName(playerSide));
                tooltipMain = GameManager.i.hqScript.GetHqApprovalLevel(playerSide);
                tooltipDetails = GameManager.i.hqScript.GetHqDetails(playerSide, true);
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
