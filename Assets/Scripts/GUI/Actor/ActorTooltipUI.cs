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
        mouseOverDelay = GameManager.instance.guiScript.tooltipDelay;
        mouseOverFade = GameManager.instance.guiScript.tooltipFade;
        //halve fade in time as a canvas tool tip appears to be a lot slower than a scene one
        mouseOverFade /= 2;
    }

    

    /// <summary>
    /// Mouse Over event -> Shows actor tooltip if one present and a generic, info, tooltip, if not
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter (PointerEventData eventData)
    {
        side = GameManager.instance.sideScript.PlayerSide;
        onMouseFlag = true;
        if (GameManager.instance.dataScript.CheckActorSlotStatus(actorSlotID, GameManager.instance.sideScript.PlayerSide) == true)
        {
            actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, GameManager.instance.sideScript.PlayerSide);
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
        GameManager.instance.tooltipActorScript.CloseTooltip("ActorTooltipUI.cs -> OnPointerExit");
        GameManager.instance.tooltipGenericScript.CloseTooltip("ActorTooltipUI.cs -> OnPointerExit");
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
            while (GameManager.instance.tooltipActorScript.CheckTooltipActive() == false)
            {
                GlobalSide side = GameManager.instance.sideScript.PlayerSide;
                Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, side);
                if (actor != null)
                {
                    ActorTooltipData data = actor.GetTooltipData(parent.transform.position);
                    GameManager.instance.tooltipActorScript.SetTooltip(data, actorSlotID);
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorSlotID {0}", actorSlotID); }
                yield return null;
            }
            //fade in
            float alphaCurrent;
            while (GameManager.instance.tooltipActorScript.GetOpacity() < 1.0)
            {
                alphaCurrent = GameManager.instance.tooltipActorScript.GetOpacity();
                alphaCurrent += Time.deltaTime / mouseOverFade;
                GameManager.instance.tooltipActorScript.SetOpacity(alphaCurrent);
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
            while (GameManager.instance.tooltipGenericScript.CheckTooltipActive() == false)
            {
                data = GameManager.instance.actorScript.GetActorTooltip(actor, side);
                data.screenPos = transform.position;
                data.screenPos = AdjustTooltipPosition(data.screenPos);
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
                    data.screenPos = AdjustTooltipPosition(data.screenPos);
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
