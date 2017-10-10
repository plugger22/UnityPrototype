using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// handles all Actor image related mouse interaction
/// </summary>
public class ActorInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int actorSlotID;               //0 to 3, assuming 4 actors
    private float mouseOverDelay;         //delay in seconds before mouseOver kicks in


    private void Start()
    {
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
    }

    /// <summary>
    /// Mouse Over event
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter (PointerEventData eventData)
    {
        StartCoroutine(ShowActiveNodes());
    }

    /// <summary>
    /// reset nodes back to normal
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        GameManager.instance.levelScript.ResetNodes();
    }

    /// <summary>
    /// After a set delay, highlight all active nodes for this actor
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowActiveNodes()
    {
        yield return new WaitForSeconds(mouseOverDelay);
        GameManager.instance.levelScript.ShowActiveNodes(actorSlotID);
    }
}
