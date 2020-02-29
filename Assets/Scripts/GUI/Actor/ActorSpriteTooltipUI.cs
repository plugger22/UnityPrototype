using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;
using packageAPI;

/// <summary>
/// handles selective tooltip (Generic) for the actor sprites. Only shows in certain cases (Actor.tooltipStatus > ActorTooltip.None)
/// </summary>
public class ActorSpriteTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /*[HideInInspector] public string tooltipHeader;
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipDetails;*/

    [HideInInspector] public int actorSlotID;               //initialised in GUIManager.cs

    private Coroutine myCoroutine;
    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;
    //data derived whenever parent sprite moused over (OnPointerEnter)
    private Actor actor;
    private GlobalSide side;


    /// <summary>
    /// constructor -> needs to be Start as GameManager hasn't initialised prior to this (UI elements initialise before normal gameObjects?)
    /// </summary>
    private void Start()
    {
        mouseOverDelay = GameManager.instance.guiScript.tooltipDelay;
        mouseOverFade = GameManager.instance.guiScript.tooltipFade;
    }


    /// <summary>
    /// Mouse Over event -> show tooltip if actor present in slot and their tooltipStatus > ActorTooltip.None
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        onMouseFlag = true;
        side = GameManager.instance.sideScript.PlayerSide;
        if (GameManager.instance.dataScript.CheckActorSlotStatus(actorSlotID, side) == true)
        {
            actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, side);
            if (actor != null)
            {
                if (actor.tooltipStatus > ActorTooltip.None)
                { myCoroutine = StartCoroutine("ShowGenericTooltip"); }
            }
        }
        else
        { myCoroutine = StartCoroutine("ShowVacantActorTooltip"); }
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

        GameManager.instance.tooltipGenericScript.CloseTooltip("ActorSpriteTooltipUI.cs -> OnPointerExit");
    }


    IEnumerator ShowGenericTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        GenericTooltipData data = null;
        //activate tool tip if mouse still over button
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.instance.tooltipGenericScript.CheckTooltipActive() == false)
            {
                data = GameManager.instance.actorScript.GetActorTooltip(actor, side);
                data.screenPos = transform.position;
                if (data != null)
                { GameManager.instance.tooltipGenericScript.SetTooltip(data); }
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

    /// <summary>
    /// Position vacant generic info tooltip
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowVacantActorTooltip()
    {
        GenericTooltipData data = null;
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over 'vacant' text
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.instance.tooltipGenericScript.CheckTooltipActive() == false)
            {
                data = GameManager.instance.actorScript.GetVacantActorTooltip();
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

}
