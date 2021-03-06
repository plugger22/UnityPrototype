﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles tooltip for player name in ActorUI -> REDUNDANT Oct'19 as player tooltip moved over to PlayerSpriteTooltipUI.cs
/// </summary>
public class PlayerTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;
    private GameObject parent;
    private Coroutine myCoroutine;


    private void Awake()
    {
        parent = transform.parent.gameObject;
    }


    /// <summary>
    /// constructor -> needs to be Start as GameManager hasn't initialised prior to this (UI elements initialise before normal gameObjects?)
    /// </summary>
    private void Start()
    {
        mouseOverDelay = GameManager.i.guiScript.tooltipDelay;
        mouseOverFade = GameManager.i.guiScript.tooltipFade;
        //halve fade in time as a canvas tool tip appears to be a lot slower than a scene one
        mouseOverFade /= 2;
    }


    /// <summary>
    /// Mouse Over event -> Shows Player tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        onMouseFlag = true;
        myCoroutine = StartCoroutine("ShowPlayerTooltip");
    }

    /// <summary>
    /// Mouse exit, close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseFlag = false;
        if (myCoroutine != null)
        { StopCoroutine(myCoroutine); }
        GameManager.i.tooltipPlayerScript.CloseTooltip("PlayerTooltipUI.cs -> OnPointerExit");
    }

    /// <summary>
    /// Player tooltip
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowPlayerTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over player text
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.i.tooltipPlayerScript.CheckTooltipActive() == false)
            {
                GameManager.i.tooltipPlayerScript.SetTooltip(parent.transform.position);
                yield return null;
            }
            //fade in
            float alphaCurrent;
            while (GameManager.i.tooltipPlayerScript.GetOpacity() < 1.0)
            {
                alphaCurrent = GameManager.i.tooltipPlayerScript.GetOpacity();
                alphaCurrent += Time.deltaTime / mouseOverFade;
                GameManager.i.tooltipPlayerScript.SetOpacity(alphaCurrent);
                yield return null;
            }
        }
    }



}
