﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System.Text;

/// <summary>
/// handles all game option matters
/// </summary>
public class OptionManager : MonoBehaviour
{
    #region Save Data Compatible
    //game options
    [HideInInspector] public bool autoGearResolution = false;                     //if true then dice roller ignored whenever not enough renown to save gear
    [HideInInspector] public bool fogOfWar = false;                               //if true then one sides sees only the information that they should
    
    //Debug options
    [HideInInspector] public bool debugData = false;                              //if true debug data is displayed onscreen
    [HideInInspector] public bool noAI = false;                                   //if true AI is switched off for both sides (debug purposes)

    //UI options
    [HideInInspector] public bool showContacts = false;                           //if true node tooltips will show contact as well as Actor Arcs for nodes where actors have contacts
    [HideInInspector] public bool showRenown = true;                              //if true renown UI elements shown for actors and player
    [HideInInspector] public bool connectorTooltips = false;                      //if true then connectors have tooltips
    [HideInInspector] public bool fullMoodInfo = false;

    //Backing fields (use underscore)
    private ColourScheme _colourOption;
    #endregion


    //ColourManager.cs ColourScheme enum (eg. 0 -> normal, 1 -> colourblind)
    public ColourScheme ColourOption                             
    {
        get { return _colourOption; }
        set
        {
            _colourOption = value;
            //Post notification - colour scheme has been changed
            if (GameManager.instance.inputScript.GameState != GameState.LoadAtStart)
            {
                EventManager.instance.PostNotification(EventType.ChangeColour, this, null, "OptionManager.cs -> ColourOption");
                Debug.Log("OptionManager -> Colour Scheme: now " + _colourOption + "\n");
            }
        }
    }  


    /*/// <summary>
    /// Housekeep events
    /// </summary>
    public void OnDisable()
    {
        EventManager.instance.RemoveEvent(EventType.ChangeSide);
        EventManager.instance.RemoveEvent(EventType.ChangeColour);
    }*/

    /// <summary>
    /// Debug method
    /// </summary>
    /// <returns></returns>
    public string DisplayOptions()
    {
        return new StringBuilder()
            .AppendFormat(" Current Option Settings{0}{1}", "\n", "\n")
            .AppendFormat(" Side -> {0}{1}", GameManager.instance.sideScript.PlayerSide, "\n")
            .AppendFormat("{0}- Game Options{1}", "\n", "\n")
            .AppendFormat(" Fog Of War (Show from POV of Player) -> {0}{1}", fogOfWar, "\n")
            .AppendFormat(" Auto Gear (Dice ignored if not enough renown) -> {0}{1}", autoGearResolution, "\n")
            .AppendFormat("{0}- Debug Options{1}","\n", "\n")
            .AppendFormat(" Debug Data -> {0}{1}", debugData, "\n")
            .AppendFormat(" NO AI -> {0}{1}", noAI, "\n")
            .AppendFormat("{0}- UI Options{1}","\n", "\n")
            .AppendFormat(" Connector Tooltips -> {0}{1}", connectorTooltips, "\n")
            .AppendFormat(" Full Mood Information -> {0}{1}", fullMoodInfo, "\n")
            .AppendFormat(" Show Contacts -> {0}{1}", showContacts, "\n")
            .AppendFormat(" Show Renown -> {0}{1}", showRenown, "\n")
            .ToString();
    }

    /// <summary>
    /// Toggle option
    /// </summary>
    public void ToggleAutoGearResolution()
    {
        if (autoGearResolution == true) { autoGearResolution = false; }
        else { autoGearResolution = true; }
    }


    //place methods above here
}
