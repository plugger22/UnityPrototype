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


    private GlobalSide _playerSide;

    //what side is the player
    public GlobalSide PlayerSide
    {
        get { return _playerSide; }
        set
        {
            _playerSide = value;
            //Post notification - Player side has been changed, update colours as well
            EventManager.instance.PostNotification(EventType.ChangeSide, this, _playerSide);
            EventManager.instance.PostNotification(EventType.ChangeColour, this);
            Debug.Log(string.Format("OptionManager -> Player Side now {0}{1}", _playerSide, "\n"));
        }
    }

    public void Initialise()
    {
        //set side
        PlayerSide = GameManager.instance.globalScript.sideResistance;
    }

}
