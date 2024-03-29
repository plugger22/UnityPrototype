﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using packageAPI;

/// <summary>
/// tooltip for actor Power display
/// </summary>
public class ActorRenownTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private Coroutine myCoroutine;
    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;
    //data derived whenever parent sprite moused over (OnPointerEnter)
    /*private GlobalSide side;*/


    /// <summary>
    /// constructor -> needs to be Start as GameManager hasn't initialised prior to this (UI elements initialise before normal gameObjects?)
    /// </summary>
    private void Start()
    {
        myCoroutine = null;
        mouseOverDelay = GameManager.i.guiScript.tooltipDelay;
        mouseOverFade = GameManager.i.guiScript.tooltipFade;
    }


    /// <summary>
    /// Mouse Over event -> show tooltip 
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        onMouseFlag = true;
        /*side = GameManager.instance.sideScript.PlayerSide;*/
        if (myCoroutine == null)
        { myCoroutine = StartCoroutine("ShowRenownTooltip"); }
    }

    /// <summary>
    /// Mouse exit, close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseFlag = false;
        if (myCoroutine != null)
        {
            StopCoroutine(myCoroutine);
            myCoroutine = null;
        }

        GameManager.i.tooltipGenericScript.CloseTooltip("ActorRenownTooltipUI.cs -> OnPointerExit");
    }


    IEnumerator ShowRenownTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        GenericTooltipData data = null;
        //activate tool tip if mouse still over button
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.i.tooltipGenericScript.CheckTooltipActive() == false)
            {
                data = GameManager.i.actorScript.GetPowerActorTooltip();
                data.screenPos = transform.position;
                if (data != null)
                { GameManager.i.tooltipGenericScript.SetTooltip(data); }
                yield return null;
            }
            //fade in
            if (data != null)
            {
                float alphaCurrent;
                while (GameManager.i.tooltipGenericScript.GetOpacity() < 1.0)
                {
                    alphaCurrent = GameManager.i.tooltipGenericScript.GetOpacity();
                    alphaCurrent += Time.deltaTime / mouseOverFade;
                    GameManager.i.tooltipGenericScript.SetOpacity(alphaCurrent);
                    yield return null;
                }
            }
        }
    }



}
