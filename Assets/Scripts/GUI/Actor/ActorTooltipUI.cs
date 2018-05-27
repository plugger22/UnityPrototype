using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using packageAPI;
using modalAPI;

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

    /*private string colourNeutral;
    private string colourAlert;
    private string colourEnd;*/


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
        /*//event listener is registered in InitialiseActors() due to GameManager sequence.
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "ActorTooltipUI");*/
    }


    /*/// <summary>
    /// handles events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }*/


    /*/// <summary>
    /// set colour palette for Generic Tool tip
    /// </summary>
    public void SetColours()
    {
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }*/




    /// <summary>
    /// Mouse Over event -> Shows actor tooltip if one present and a generic, info, tooltip, if not
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter (PointerEventData eventData)
    {
        onMouseFlag = true;
        /*if (GameManager.instance.dataScript.CheckActorSlotStatus(actorSlotID, GameManager.instance.sideScript.PlayerSide) == true)
        { StartCoroutine(ShowActiveActorTooltip()); }
        else
        { StartCoroutine(ShowVacantActorTooltip()); }*/
        if (GameManager.instance.dataScript.CheckActorSlotStatus(actorSlotID, GameManager.instance.sideScript.PlayerSide) == true)
        { StartCoroutine(ShowActiveActorTooltip()); }
    }

    /// <summary>
    /// Mouse exit, close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseFlag = false;
        /*if (GameManager.instance.dataScript.CheckActorSlotStatus(actorSlotID, GameManager.instance.sideScript.PlayerSide) == true)
        {
            StopCoroutine(ShowActiveActorTooltip());
            GameManager.instance.tooltipActorScript.CloseTooltip();
        }
        else
        {
            StopCoroutine(ShowVacantActorTooltip());
            GameManager.instance.tooltipGenericScript.CloseTooltip();
        }*/

        StopCoroutine(ShowActiveActorTooltip());
        GameManager.instance.tooltipActorScript.CloseTooltip();
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

    /*/// <summary>
    /// Position vacant generic info tooltip
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowVacantActorTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over 'vacant' text
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.instance.tooltipGenericScript.CheckTooltipActive() == false)
            {
                GlobalSide side = GameManager.instance.sideScript.PlayerSide;

                Vector3 tooltipPos = parent.transform.position;
                tooltipPos.x -= 75;
                tooltipPos.y += 75;
                string headerText = string.Format("{0}Position Vacant{1}", colourNeutral, colourEnd);
                string mainText = "There is currently nobody acting in this position.";
                string detailText = string.Format("{0}Go to the Reserve Pool and click on a person for the option to recall them to active duty{1}", 
                    colourAlert, colourEnd);
                GameManager.instance.tooltipGenericScript.SetTooltip(mainText, tooltipPos, headerText, detailText);
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
    }*/



    //place methods above here
}
