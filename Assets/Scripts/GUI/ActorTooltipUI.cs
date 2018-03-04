using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using packageAPI;

/// <summary>
/// handles UI tooltip for Actors
/// </summary>
public class ActorTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private int offset;
    [HideInInspector] public int actorSlotID;
    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;
    private RectTransform rectTransform;
    private GameObject parent;


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
        //halve fade in time as a canvas tool tip appears to be a lot slower than a scene one
        mouseOverFade /= 2; 
        offset = GameManager.instance.tooltipScript.tooltipOffset;
        //actorSlotID = parent.GetComponent<ActorHighlightUI>().actorSlotID;
    }

    /// <summary>
    /// Mouse Over event
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter (PointerEventData eventData)
    {
        onMouseFlag = true;
        if (GameManager.instance.dataScript.CheckActorSlotStatus(actorSlotID, GameManager.instance.sideScript.PlayerSide) == true)
        { StartCoroutine(ShowActorTooltip()); }
    }

    /// <summary>
    /// Mouse exit, close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseFlag = false;
        StopCoroutine(ShowActorTooltip());
        GameManager.instance.tooltipActorScript.CloseTooltip();
    }

    IEnumerator ShowActorTooltip()
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
                ActorTooltipData data = new ActorTooltipData()
                {
                    tooltipPos = parent.transform.position,
                    actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, side),
                    action = GameManager.instance.dataScript.GetActorAction(actorSlotID, side),
                    arrayOfQualities = GameManager.instance.dataScript.GetQualities(side),
                    arrayOfStats = GameManager.instance.dataScript.GetActorStats(actorSlotID, side)
                };
                GameManager.instance.tooltipActorScript.SetTooltip(data);
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
}
