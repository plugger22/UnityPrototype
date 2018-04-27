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
    public Sprite inventory_background_Authority;
    public Sprite inventory_background_Rebel;
    public Sprite header_background_Authority;
    public Sprite header_background_Rebel;
    public Sprite button_Authority;
    public Sprite button_highlight_Authority;
    public Sprite button_Rebel;
    public Sprite button_highlight_Rebel;
    public Sprite button_Click;

    [HideInInspector] public SideState resistanceCurrent;               //who's currently in charge, AI or player?
    [HideInInspector] public SideState authorityCurrent;
    [HideInInspector] public SideState resistanceOverall;                 //who's in charge overall (flows through throughout, constant)
    [HideInInspector] public SideState authorityOverall;


    //backing field
    private GlobalSide _playerSide;

    //what side is the player
    public GlobalSide PlayerSide
    {
        get { return _playerSide; }
        set
        {
            _playerSide = value;
            switch (value.level)
            {
                case 0:
                    //AI
                    resistanceCurrent = SideState.AI;
                    authorityCurrent = SideState.AI;
                    break;
                case 1:
                    //Authority
                    if (GameManager.instance.optionScript.noAI == false)
                    {
                        resistanceCurrent = SideState.AI;
                        authorityCurrent = SideState.Player;
                    }
                    else
                    {
                        //no AI debug mode
                        resistanceCurrent = SideState.Player;
                        authorityCurrent = SideState.Player;
                    }
                    break;
                case 2:
                    //Resistance
                    if (GameManager.instance.optionScript.noAI == false)
                    {
                        resistanceCurrent = SideState.Player;
                        authorityCurrent = SideState.AI;
                    }
                    else
                    {
                        //no AI debug mode
                        resistanceCurrent = SideState.Player;
                        authorityCurrent = SideState.Player;
                    }
                    break;
                default:
                    Debug.LogError(string.Format("Invalid side.level \"{0}\"", value.level));
                    break;
            }
            //Post notification - Player side has been changed, update colours as well
            EventManager.instance.PostNotification(EventType.ChangeSide, this, _playerSide);
            EventManager.instance.PostNotification(EventType.ChangeColour, this);
            Debug.Log(string.Format("OptionManager -> Player Side now {0}{1}", _playerSide, "\n"));
        }
    }

    public void Initialise()
    {
        //set default player as Resistance
        PlayerSide = GameManager.instance.globalScript.sideResistance;
        resistanceOverall = SideState.Player;
        authorityOverall = SideState.AI;
    }

    /// <summary>
    /// Returns true if interaction is possible for the current Player side given the overall & noAI settings.
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public bool CheckInteraction()
    {
        bool isPossible = true;
        if (GameManager.instance.optionScript.noAI == false)
        {
            switch(_playerSide.level)
            {
                case 0:
                    //AI
                    isPossible = false;
                    break;
                case 1:
                    //Authority
                    if (authorityOverall == SideState.AI)
                    { isPossible = false; }
                    break;
                case 2:
                    //Resistance
                    if (resistanceOverall == SideState.AI)
                    { isPossible = false; }
                    break;
                default:
                    Debug.LogError(string.Format("Invalid _playerSide.level \"{0}\"", _playerSide.level));
                    break;
            }
        }
        return isPossible;
    }

}
