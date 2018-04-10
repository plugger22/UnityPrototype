using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// handles selective tooltip (Generic) for the actor sprites. Only shows in certain cases (Actor.tooltipStatus > ActorTooltip.None)
/// </summary>
public class ActorSpriteTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [HideInInspector] public string tooltipHeader;
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipEffect;

    [HideInInspector] public int actorSlotID;

    private int offset;
    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;
    private RectTransform rectTransform;
    private GameObject parent;
    //data derived whenever parent sprite moused over (OnPointerEnter)
    private Actor actor;
    private GlobalSide side;




    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parent = transform.parent.gameObject;
    }

    /// <summary>
    /// constructor -> needs to be Start as GameManager hasn't initialised prior to this (UI elements initialise before normal gameObjects?)
    /// </summary>
    private void Start()
    {
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        mouseOverFade = GameManager.instance.tooltipScript.tooltipFade;
        offset = GameManager.instance.tooltipScript.tooltipOffset;
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
                StartCoroutine(ShowGenericTooltip());
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
        StopCoroutine(ShowGenericTooltip());
        GameManager.instance.tooltipGenericScript.CloseTooltip();
    }

    /// <summary>
    /// Mouse click on button, don't forget to close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        onMouseFlag = false;
        StopCoroutine(ShowGenericTooltip());
        GameManager.instance.tooltipGenericScript.CloseTooltip();
    }


    IEnumerator ShowGenericTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over button
        if (onMouseFlag == true)
        {
            //do once

            while (GameManager.instance.tooltipGenericScript.CheckTooltipActive() == false)
            {
                switch(actor.tooltipStatus)
                {
                    case ActorTooltip.Breakdown:

                        break;
                    case ActorTooltip.LieLow:
                        tooltipHeader = string.Format("{0}{1}{2}", actor.actorName, "\n", actor.arc.name);
                        tooltipMain = string.Format("Currently Lying Low and unavailable");
                        tooltipEffect = string.Format("{0} will automatically reactivate once their invisibility recovers or your could ACTIVATE them before this", 
                            actor.arc.name);
                        break;
                    case ActorTooltip.Talk:

                        break;
                    default:
                        tooltipMain = "Unknown"; tooltipHeader = "Unknown"; tooltipEffect = "Unknown";
                        break;
                }
                GameManager.instance.tooltipGenericScript.SetTooltip(tooltipMain, transform.position, tooltipHeader, tooltipEffect);
                yield return null;
            }
            //fade in
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
