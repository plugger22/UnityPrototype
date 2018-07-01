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
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        mouseOverFade = GameManager.instance.tooltipScript.tooltipFade;
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
        side = GameManager.instance.sideScript.PlayerSide;
        /*playerName = GameManager.instance.playerScript.PlayerName;*/
        //activate tooltip if there is a valid reason
        if (GameManager.instance.playerScript.tooltipStatus > ActorTooltip.None)
        { myTooltipCoroutine = StartCoroutine("ShowGenericTooltip"); }
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
        { StopCoroutine(myTooltipCoroutine); }
    }



    IEnumerator ShowGenericTooltip()
    {
        //delay before tooltip kicks in
        GenericTooltipData data = null;
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over button
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.instance.tooltipGenericScript.CheckTooltipActive() == false)
            {
                data = GameManager.instance.actorScript.GetPlayerTooltip(side);
                if (data != null)
                {
                    data.screenPos = transform.position;
                    GameManager.instance.tooltipGenericScript.SetTooltip(data);
                }
                yield return null;
            }
            //fade in
            if (data != null)
            {
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



    //new methods above here
}
