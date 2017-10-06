using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all game option matters
/// </summary>
public class OptionManager : MonoBehaviour
{
    //ColourManager.cs ColourScheme enum (eg. 0 -> normal, 1 -> colourblind)
    public ColourScheme ColourOption                             
    {
        get { return _colourOption; }
        set
        {
            _colourOption = value;
            GameManager.instance.colourScript.ChangeColourPalettes();
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
            GameManager.instance.sideScript.SwapSides(_playerSide);
            Debug.Log("OptionManager -> Player Side now " + _playerSide + "\n");
        }
    }                        

    //Backing fields (use underscore)
    private Side _playerSide;
    private ColourScheme _colourOption;



    //place methods above here
}
