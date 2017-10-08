using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Handles change of Sides, eg. Rebel / Authority, admin
/// </summary>
public class SideManager : MonoBehaviour
{
    //node tooltip
    public Sprite toolTip_backgroundAuthority;
    public Sprite toolTip_backgroundRebel;
    public Sprite toolTip_dividerAuthority;
    public Sprite toolTip_dividerRebel;
    public Sprite outcome_backgroundAuthority;
    public Sprite outcome_backgroundRebel;

    public void Initialise()
    {
        //event Listener
        //EventManager.instance.AddListener(EventType.ChangeSide, OnEvent);
    }


    /*/// <summary>
    /// Called when an event happens
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect Event type
        switch(eventType)
        {
            case EventType.ChangeSide:
                SwapSides((Side)Param);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Swaps gfx around for UI elements
    /// </summary>
    /// <param name="side"></param>
    public void SwapSides(Side side)
    {
        GameManager.instance.tooltipNodeScript.InitialiseTooltip(side);
        GameManager.instance.tooltipActorScript.InitialiseTooltip(side);
        GameManager.instance.outcomeScript.InitialiseOutcome(side);
    }*/
}
