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
        Debug.LogFormat("[Tst] ActorTooltipUI.cs -> OnPointerEnter: actorSlotID 0 -> {0}{1}", actorSlotID, "\n");
        onMouseFlag = true;
        if (GameManager.instance.dataScript.CheckActorSlotStatus(actorSlotID, GameManager.instance.sideScript.PlayerSide) == true)
        {
            Debug.LogFormat("[Tst] ActorTooltipUI.cs -> OnPointerEnter: actorSlotID 1 -> {0}{1}", actorSlotID, "\n");
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, GameManager.instance.sideScript.PlayerSide);
            if (actor != null)
            {
                Debug.LogFormat("[Tst] ActorTooltipUI.cs -> OnPointerEnter: actorSlotID 2 -> {0}{1}", actorSlotID, "\n");
                //Don't want to clash with actor sprite tooltip
                if (actor.tooltipStatus == ActorTooltip.None)
                { myCoroutine = StartCoroutine("ShowActiveActorTooltip"); }
                else
                {
                    //InActive status, shows only if the text below sprite
                    if (isText == true)
                    { myCoroutine = StartCoroutine("ShowActiveActorTooltip"); }
                }
            }
        }
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
                    Debug.LogFormat("[Tst] ActorTooltipUI.cs -> ShowActiveActor: actorSlotID 4 -> {0}{1}", actorSlotID, "\n");

                    /*Gear gearActor = null;
                    if (actor.GetGearID() > -1)
                    { gearActor = GameManager.instance.dataScript.GetGear(actor.GetGearID()); }
                    ActorTooltipData data = new ActorTooltipData()
                    {
                        tooltipPos = parent.transform.position,
                        actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, side),
                        action = GameManager.instance.dataScript.GetActorAction(actorSlotID, side),
                        gear = gearActor,
                        listOfSecrets = actor.GetSecretsTooltipList(),
                        arrayOfQualities = GameManager.instance.dataScript.GetQualities(side),
                        arrayOfStats = GameManager.instance.dataScript.GetActorStats(actorSlotID, side)
                    };*/

                    ActorTooltipData data = actor.GetTooltipData(parent.transform.position);
                    Debug.LogFormat("[Tst] ActorTooltipUI.cs -> ShowActiveActor: actorSlotID 5 -> {0}, {1}, slotID {2}, actorSlotID {3}{4}", actor.actorName, actor.arc.name, actor.slotID, actorSlotID, "\n");
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






    //place methods above here
}
