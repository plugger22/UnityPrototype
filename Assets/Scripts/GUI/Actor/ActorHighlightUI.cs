using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;
using modalAPI;

/// <summary>
/// handles all Actor face image + text mouse interactions -> show highlighted nodes
/// Right click on face is ActorClickUI.cs & mouseover tooltip on text is ActorTooltipUI.cs
/// </summary>
public class ActorHighlightUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public int actorSlotID;               //0 to 3, assuming 4 actors
    private float mouseOverDelay;         //delay in seconds before mouseOver kicks in


    private void Start()
    {
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
    }

    
    /// <summary>
    /// Mouse Over event -> Only show active nodes if actor active
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter (PointerEventData eventData)
    {
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, GameManager.instance.sideScript.PlayerSide);
        if (actor != null && actor.Status == ActorStatus.Active)
        { StartCoroutine(ShowActiveNodes()); }
    }

    /// <summary>
    /// reset nodes back to normal
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        //StopAllCoroutines();
        StopCoroutine(ShowActiveNodes());
        EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.Reset);
    }
    

    /// <summary>
    /// After a set delay, highlight all active nodes for this actor
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowActiveNodes()
    {
        yield return new WaitForSeconds(mouseOverDelay);
        GameManager.instance.nodeScript.ShowActiveNodes(actorSlotID);
    }


}
