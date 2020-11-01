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
    private Coroutine myCoroutine;

    private void Start()
    {
        mouseOverDelay = GameManager.i.guiScript.tooltipDelay;
    }

    
    /// <summary>
    /// Mouse Over event -> Only show active nodes if actor active
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter (PointerEventData eventData)
    {
        Actor actor = GameManager.i.dataScript.GetCurrentActor(actorSlotID, GameManager.i.sideScript.PlayerSide);
        if (actor != null && actor.Status == ActorStatus.Active)
        { myCoroutine = StartCoroutine("ShowActiveNodes"); }
    }

    /// <summary>
    /// reset nodes back to normal
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (myCoroutine != null)
        {
            StopCoroutine(myCoroutine);
            EventManager.i.PostNotification(EventType.FlashNodesStop, this, null, "ActorHighlightUI.cs -> OnPointerExit");
        }
        /*EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.Reset, "ActorHighlightUI.cs -> OnPointerExit");*/
    }
    

    /// <summary>
    /// After a set delay, highlight all active nodes for this actor
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowActiveNodes()
    {
        yield return new WaitForSeconds(mouseOverDelay);
        List<Node> listOfNodes = GameManager.i.nodeScript.ShowActiveNodes(actorSlotID);
        EventManager.i.PostNotification(EventType.FlashNodesStart, this, listOfNodes, "ActorHighlightUI.cs -> OnPointerEnter");
    }


}
