using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;

/// <summary>
/// handles selective tooltip (Generic) for the actor sprites. Only shows in certain cases (Actor.tooltipStatus > ActorTooltip.None)
/// </summary>
public class ActorSpriteTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public string tooltipHeader;
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipEffect;

    [HideInInspector] public int actorSlotID;               //initialised in GUIManager.cs

    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;
    //data derived whenever parent sprite moused over (OnPointerEnter)
    private Actor actor;
    private GlobalSide side;

    //colours
    private string colourSide;
    private string colourRebel;
    private string colourAlert;
    private string colourAuthority;
    private string colourNeutral;
    private string colourNormal;
    private string colourEnd;


    /// <summary>
    /// constructor -> needs to be Start as GameManager hasn't initialised prior to this (UI elements initialise before normal gameObjects?)
    /// </summary>
    private void Start()
    {
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        mouseOverFade = GameManager.instance.tooltipScript.tooltipFade;
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "ActorSpriteTooltipUI");
    }

    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourRebel = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
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
                { StartCoroutine(ShowGenericTooltip()); }
            }
        }
        else
        { StartCoroutine(ShowVacantActorTooltip()); }
    }

    /// <summary>
    /// Mouse exit, close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseFlag = false;
        if (GameManager.instance.dataScript.CheckActorSlotStatus(actorSlotID, GameManager.instance.sideScript.PlayerSide) == true)
        { StopCoroutine("ShowGenericTooltip"); }
        else
        { StopCoroutine("ShowVacantActorTooltip"); }
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
                colourSide = colourRebel;
                if (side.level == GameManager.instance.globalScript.sideAuthority.level)
                { colourSide = colourAuthority; }

                switch (actor.tooltipStatus)
                {
                    case ActorTooltip.Breakdown:
                        tooltipHeader = string.Format("{0}{1}{2}{3}{4}", colourSide, actor.arc.name, colourEnd, "\n", actor.actorName);
                        tooltipMain = string.Format("{0}<size=120%>Currently having a {1}{2}BREAKDOWN (Stress){3}{4} and unavailable</size>{5}", colourNormal, colourEnd,
                            colourNeutral, colourEnd, colourNormal, colourEnd);
                        tooltipEffect = string.Format("{0} is expected to recover next turn", actor.actorName);
                        break;
                    case ActorTooltip.LieLow:
                        tooltipHeader = string.Format("{0}{1}{2}{3}{4}", colourSide, actor.arc.name, colourEnd, "\n", actor.actorName);
                        tooltipMain = string.Format("{0}<size=120%>Currently {1}{2}LYING LOW{3}{4} and unavailable</size>{5}", colourNormal, colourEnd,
                            colourNeutral, colourEnd, colourNormal, colourEnd);
                        tooltipEffect = string.Format("{0} will automatically reactivate once their invisibility recovers or you {1}ACTIVATE{2} them",
                            actor.actorName, colourNeutral, colourEnd);
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

    /// <summary>
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
                string headerText = "Position Vacant";
                string mainText = string.Format("{0}There is currently nobody acting in this position{1}", colourNormal, colourEnd);
                string detailText = string.Format("{0}Go to the {1}{2}Reserve Pool{3}{4} and click on a person for the option to recall them to active duty{5}",
                    colourAlert, colourEnd, colourNeutral, colourEnd, colourAlert, colourEnd);
                GameManager.instance.tooltipGenericScript.SetTooltip(mainText, transform.position, headerText, detailText);
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
