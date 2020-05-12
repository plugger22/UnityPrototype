using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using packageAPI;
using gameAPI;
using modalAPI;

/// <summary>
/// handles UI tooltip for Actors
/// </summary>
public class ActorTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public int actorSlotID;
    [HideInInspector] public bool isText;                   //true if tooltip attached to text below sprite. Activates while actor inactive unlike sprite which doesn't

    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;
    private GameObject parent;
    private Coroutine myCoroutine;
    private Actor actor;
    private GlobalSide side;

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
    /// Mouse Over event -> Shows actor tooltip if one present and a generic, info, tooltip, if not
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter (PointerEventData eventData)
    {
        side = GameManager.i.sideScript.PlayerSide;
        onMouseFlag = true;
        if (GameManager.i.dataScript.CheckActorSlotStatus(actorSlotID, GameManager.i.sideScript.PlayerSide) == true)
        {
            actor = GameManager.i.dataScript.GetCurrentActor(actorSlotID, GameManager.i.sideScript.PlayerSide);
            if (actor != null)
            {
                //Don't want to clash with actor sprite tooltip
                if (actor.tooltipStatus == ActorTooltip.None)
                { myCoroutine = StartCoroutine("ShowActiveActorTooltip"); }
                else
                {
                    //InActive status, shows only if the text below sprite
                    if (isText == true)
                    { myCoroutine = StartCoroutine("ShowActiveActorTooltip"); }
                    else
                    { myCoroutine = StartCoroutine("ShowGenericTooltip"); }
                }
            }
        }
        else { myCoroutine = StartCoroutine("ShowVacantActorTooltip"); }
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
        GameManager.i.tooltipActorScript.CloseTooltip("ActorTooltipUI.cs -> OnPointerExit");
        GameManager.i.tooltipGenericScript.CloseTooltip("ActorTooltipUI.cs -> OnPointerExit");
    }

    /// <summary>
    /// Actor tooltip
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowActiveActorTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over actor text
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.i.tooltipActorScript.CheckTooltipActive() == false)
            {
                GlobalSide side = GameManager.i.sideScript.PlayerSide;
                Actor actor = GameManager.i.dataScript.GetCurrentActor(actorSlotID, side);
                if (actor != null)
                {
                    ActorTooltipData data = actor.GetTooltipData(parent.transform.position);
                    GameManager.i.tooltipActorScript.SetTooltip(data, actorSlotID);
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorSlotID {0}", actorSlotID); }
                yield return null;
            }
            //fade in
            float alphaCurrent;
            while (GameManager.i.tooltipActorScript.GetOpacity() < 1.0)
            {
                alphaCurrent = GameManager.i.tooltipActorScript.GetOpacity();
                alphaCurrent += Time.deltaTime / mouseOverFade;
                GameManager.i.tooltipActorScript.SetOpacity(alphaCurrent);
                yield return null;
            }
        }
    }

    //
    // - - - Inactive actor tooltips
    //

    IEnumerator ShowGenericTooltip()
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
                data = GameManager.i.actorScript.GetActorTooltip(actor, side);
                data.screenPos = transform.position;
                data.screenPos = AdjustTooltipPosition(data.screenPos);
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
            while (GameManager.i.tooltipGenericScript.CheckTooltipActive() == false)
            {
                data = GameManager.i.actorScript.GetVacantActorTooltip();
                if (data != null)
                {
                    data.screenPos = transform.position;
                    data.screenPos = AdjustTooltipPosition(data.screenPos);
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
    /// Adjusts tooltip position based on slotID
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private Vector3 AdjustTooltipPosition(Vector3 pos)
    {
        Vector3 adjustedPos = pos;
        switch (actorSlotID)
        {
            case 0:
            case 1:
                adjustedPos.x -= 250.0f;
                break;
            case 2:
            case 3:
                adjustedPos.x += 50.0f;
                break;
            default: Debug.LogWarningFormat("Unrecognised actorSlotID \"{0}\"", actorSlotID); break;
        }
        return adjustedPos;
    }



    //place methods above here
}
