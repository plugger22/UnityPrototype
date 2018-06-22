using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles tooltip for player name in ActorUI
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
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        mouseOverFade = GameManager.instance.tooltipScript.tooltipFade;
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
        GameManager.instance.tooltipPlayerScript.CloseTooltip("PlayerTooltipUI.cs -> OnPointerExit");
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
            while (GameManager.instance.tooltipPlayerScript.CheckTooltipActive() == false)
            {
                GameManager.instance.tooltipPlayerScript.SetTooltip(parent.transform.position);
                yield return null;
            }
            //fade in
            float alphaCurrent;
            while (GameManager.instance.tooltipPlayerScript.GetOpacity() < 1.0)
            {
                alphaCurrent = GameManager.instance.tooltipPlayerScript.GetOpacity();
                alphaCurrent += Time.deltaTime / mouseOverFade;
                GameManager.instance.tooltipPlayerScript.SetOpacity(alphaCurrent);
                yield return null;
            }
        }
    }



}
