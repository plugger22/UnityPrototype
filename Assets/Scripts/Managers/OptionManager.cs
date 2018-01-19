using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System.Text;

/// <summary>
/// handles all game option matters
/// </summary>
public class OptionManager : MonoBehaviour
{
    //game options
    [HideInInspector] public bool autoGearResolution = false;                     //if true then dice roller ignored whenever not enough renown to save gear

    //Backing fields (use underscore)
    
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






    /// <summary>
    /// Housekeep events
    /// </summary>
    public void OnDisable()
    {
        EventManager.instance.RemoveEvent(EventType.ChangeSide);
        EventManager.instance.RemoveEvent(EventType.ChangeColour);
    }

    /// <summary>
    /// Debug method
    /// </summary>
    /// <returns></returns>
    public string DisplayOptions()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(string.Format(" Current Option Settings{0}{1}", "\n", "\n"));
        builder.Append(string.Format(" Side -> {0}{1}{2}", GameManager.instance.sideScript.PlayerSide, "\n", "\n"));
        builder.Append(string.Format(" Auto Gear Resolution -> {0}{1}{2}", autoGearResolution, "\n", "\n"));
        return builder.ToString();
    }



    //place methods above here
}
