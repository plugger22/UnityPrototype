using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;
using packageAPI;

/// <summary>
/// handles player status tooltip
/// </summary>
public class PlayerSpriteTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /*[HideInInspector] public string tooltipHeader;
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipDetails;*/

    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;

    private Coroutine myTooltipCoroutine;


    //data derived whenever parent sprite moused over (OnPointerEnter)
    private GlobalSide side;
    /*private string playerName;*/


    /// <summary>
    /// constructor -> needs to be Start as GameManager hasn't initialised prior to this (UI elements initialise before normal gameObjects?)
    /// </summary>
    private void Start()
    {
        //delays
        mouseOverDelay = GameManager.i.guiScript.tooltipDelay;
        mouseOverFade = GameManager.i.guiScript.tooltipFade;
    }

    /// <summary>
    /// Mouse Over event -> show tooltip if Player tooltipStatus > ActorTooltip.None
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        onMouseFlag = true;
        //
        // - - - Tooltip - - -
        //
        side = GameManager.i.sideScript.PlayerSide;
        //activate tooltip if there is a valid reason
        if (GameManager.i.playerScript.tooltipStatus > ActorTooltip.None)
        { myTooltipCoroutine = StartCoroutine("ShowSpecialTooltip"); }
        else
        {
            //normal status -> show standard player tooltip
            myTooltipCoroutine = StartCoroutine("ShowPlayerTooltip");
        }
    }

    /// <summary>
    /// Mouse exit, close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseFlag = false;
        //Tooltip
        if (myTooltipCoroutine != null)
        {
            StopCoroutine(myTooltipCoroutine);
            //close regardless, even if special tooltip in play
            GameManager.i.tooltipPlayerScript.CloseTooltip("PlayerTooltipUI.cs -> OnPointerExit");
        }
    }


    /// <summary>
    /// Generic tooltip to handle special conditions, eg. Lie Low, Captured, etc
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowSpecialTooltip()
    {
        //delay before tooltip kicks in
        GenericTooltipData data = null;
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over button
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.i.tooltipGenericScript.CheckTooltipActive() == false)
            {
                data = GameManager.i.actorScript.GetPlayerTooltip(side);
                if (data != null)
                {
                    data.screenPos = transform.position;
                    data.screenPos.x += 100;
                    data.screenPos.y -= 100;
                    GameManager.i.tooltipGenericScript.SetTooltip(data);
                }
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

    /// <summary>
    /// Player standard tooltip
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
                GameManager.i.tooltipPlayerScript.SetTooltip(transform.position);
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


    //new methods above here
}
