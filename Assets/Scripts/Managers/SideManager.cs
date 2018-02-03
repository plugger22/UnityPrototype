using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using gameAPI;


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
    public Sprite picker_background_Authority;
    public Sprite picker_background_Rebel;
    public Sprite button_Authority;
    public Sprite button_highlight_Authority;
    public Sprite button_Rebel;
    public Sprite button_highlight_Rebel;
    public Sprite button_Click;

    private Side _playerSide;

    //what side is the player
    public Side PlayerSide
    {
        get { return _playerSide; }
        set
        {
            _playerSide = value;
            //Post notification - Player side has been changed, update colours as well
            EventManager.instance.PostNotification(EventType.ChangeSide, this, _playerSide);
            EventManager.instance.PostNotification(EventType.ChangeColour, this);
            Debug.Log("OptionManager -> Player Side now " + _playerSide + "\n");
        }
    }

    public void Initialise()
    {
        //set side
        PlayerSide = Side.Resistance;        
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
