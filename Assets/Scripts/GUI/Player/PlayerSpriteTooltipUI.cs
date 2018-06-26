using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;
using packageAPI;

/// <summary>
/// handles player status tooltip
/// </summary>
public class PlayerSpriteTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public string tooltipHeader;
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipDetails;

    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;

    private Coroutine myTooltipCoroutine;


    //data derived whenever parent sprite moused over (OnPointerEnter)
    private GlobalSide side;
    private string playerName;


    /*private string colourSide;
    private string colourRebel;
    private string colourAuthority;
    private string colourNeutral;
    private string colourNormal;
    private string colourEnd;*/


    /// <summary>
    /// constructor -> needs to be Start as GameManager hasn't initialised prior to this (UI elements initialise before normal gameObjects?)
    /// </summary>
    private void Start()
    {
        //delays
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        mouseOverFade = GameManager.instance.tooltipScript.tooltipFade;
        /*//initialise colours at start (missed out otherwise)
        SetColours();
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent,"PlayerSpirteTooltipUI");*/
    }

    /*/// <summary>
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
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }*/

    /// <summary>
    /// Mouse Over event -> show tooltip if Player tooltipStatus > ActorTooltip.None
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        onMouseFlag = true;
        //
        // - - - Tooltip - - -
        //
        side = GameManager.instance.sideScript.PlayerSide;
        playerName = GameManager.instance.playerScript.PlayerName;
        //activate tooltip if there is a valid reason
        if (GameManager.instance.playerScript.tooltipStatus > ActorTooltip.None)
        { myTooltipCoroutine = StartCoroutine("ShowGenericTooltip"); }
    }

    /// <summary>
    /// Mouse exit, close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseFlag = false;
        //Tooltip
        if (myTooltipCoroutine != null)
        { StopCoroutine(myTooltipCoroutine); }
    }



    IEnumerator ShowGenericTooltip()
    {
        //delay before tooltip kicks in
        GenericTooltipData data = null;
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over button
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.instance.tooltipGenericScript.CheckTooltipActive() == false)
            {
                /*colourSide = colourRebel;
                if (side.level == GameManager.instance.globalScript.sideAuthority.level)
                { colourSide = colourAuthority; }
                switch (GameManager.instance.playerScript.tooltipStatus)
                {
                    case ActorTooltip.Breakdown:
                        tooltipHeader = string.Format("{0}PLAYER{1}{2}{3}", colourSide, colourEnd, "\n", playerName);
                        tooltipMain = string.Format("{0}<size=120%>Currently having a {1}{2}BREAKDOWN (Stress){3}{4} and unavailable</size>{5}", colourNormal, colourEnd,
                            colourNeutral, colourEnd, colourNormal, colourEnd);
                        tooltipDetails = string.Format("{0} is expected to recover next turn", playerName);
                        break;
                    case ActorTooltip.LieLow:
                        tooltipHeader = string.Format("{0}PLAYER{1}{2}{3}", colourSide, colourEnd, "\n", playerName);
                        tooltipMain = string.Format("{0}<size=120%>Currently {1}{2}LYING LOW{3}{4} and unavailable</size>{5}", colourNormal, colourEnd,
                            colourNeutral, colourEnd, colourNormal, colourEnd);
                        tooltipDetails = string.Format("{0} will automatically reactivate once their invisibility recovers or you {1}ACTIVATE{2} them",
                            playerName, colourNeutral, colourEnd);
                        break;
                    default:
                        tooltipMain = "Unknown"; tooltipHeader = "Unknown"; tooltipDetails = "Unknown";
                        break;
                }
                GenericTooltipData data = new GenericTooltipData() { screenPos = transform.position, main = tooltipMain, header = tooltipHeader, details = tooltipDetails };*/
                data = GameManager.instance.actorScript.GetPlayerTooltip(side);
                data.screenPos = transform.position;
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



    //new methods above here
}
