using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// handles all game option matters
/// </summary>
public class OptionManager : MonoBehaviour
{   
    //Backing fields (use underscore)
    private Side _playerSide;
    private ColourScheme _colourOption;

    //ColourManager.cs ColourScheme enum (eg. 0 -> normal, 1 -> colourblind)
    public ColourScheme ColourOption                             
    {
        get { return _colourOption; }
        set
        {
            _colourOption = value;
            //Post notification - colour scheme has been changed
            EventManager.instance.PostNotification(EventType.ChangeColour, this);
            Debug.Log("OptionManager -> Colour Scheme now " + _colourOption + "\n");
        }
    }  

    //what side is the player
    public Side PlayerSide                                       
    {
        get { return _playerSide; }
        set
        {
            _playerSide = value;
            //POst notification - Player side has been changed
            EventManager.instance.PostNotification(EventType.ChangeSide, this, _playerSide);
            //GameManager.instance.sideScript.SwapSides(_playerSide);
            Debug.Log("OptionManager -> Player Side now " + _playerSide + "\n");
        }
    }


    /// <summary>
    /// Housekeep events
    /// </summary>
    public void OnDisable()
    {
        EventManager.instance.RemoveEvent(EventType.ChangeSide);
        EventManager.instance.RemoveEvent(EventType.ChangeColour);
    }



    //place methods above here
}
