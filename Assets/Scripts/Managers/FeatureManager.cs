using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all togglable game features and related methods
/// </summary>
public class FeatureManager : MonoBehaviour
{

    [Header("Features")]
    [Tooltip("AI (both sides) on/off -> sets initial state only (can be adjusted by code later)")]
    public bool isAI;
    [Tooltip("Nemesis on/off (AI must be ON to use Nemesis) -> sets initial state only (can be adjusted by code later)")]
    public bool isNemesis;
    [Tooltip("Fog of War on/off -> sets initial state only (can be adjusted by code later)")]
    public bool isFOW;
    [Tooltip("Decisions on/off (all forms) -> sets initial state only (can be adjusted by code later)")]
    public bool isDecisions;
    [Tooltip("MainInfoApp on/off -> sets initial state only (can be adjusted by code later)")]
    public bool isMainInfoApp;
    [Tooltip("NPC on/off -> sets initial state only (can be adjusted by code later)")]
    public bool isNPC;
    [Tooltip("Subordinates on/off -> sets initial state only (can be adjusted by code later)")]
    public bool isSubordinates;
    [Tooltip("Review decisions on/off (auto off if isDecisions OFF) -> sets initial state only (can be adjusted by code later)")]
    public bool isReviews;
    [Tooltip("Objectives on/off (auto Off if Targets off) -> sets initial state only (can be adjusted by code later)")]
    public bool isObjectives;
    [Tooltip("Organisations on/off (auto Off if Targets off)-> sets initial state only (can be adjusted by code later)")]
    public bool isOrganisations;
    [Tooltip("Targets on/off (if off, sets Objectives and Organisations off automatically) -> sets initial state only (can be adjusted by code later)")]
    public bool isTargets;
    [Tooltip("Gear on/off -> sets initial state only (can be adjusted by code later)")]
    public bool isGear;
    [Tooltip("Recruit Actors on/off -> sets initial state only (can be adjusted by code later)")]
    public bool isRecruit;
    [Tooltip("If false connection security is ignored when moving (can be adjusted by code later)")]
    public bool isMoveSecurity;
    [Tooltip("If false actions aren't expended and a new turn is by request only, eg. press 'Enter'")]
    public bool isActions;


    #region Initialisation...
    /// <summary>
    /// Master method that creates a level (city)
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
            case GameState.FollowOnInitialisation:
                SubInitialiseToggle();
                break;
            case GameState.LoadAtStart:
            case GameState.LoadGame:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }
    #endregion


    #region InitialiseSubmethods

    #region SubInitialiseToggle
    /// <summary>
    /// selectively toggles features on/off as required at new or followOn levels. Last in GameManager.cs start sequence
    /// </summary>
    private void SubInitialiseToggle()
    {
        //if Resistance, depends on isAI setting, if Authority, auto off
        if (GameManager.i.sideScript.PlayerSide.level == 2)
        { ToggleAISideWidget(isAI); }
        else { ToggleAISideWidget(false); }
    }
    #endregion

    #endregion

    #region InitialiseFeatures
    /// <summary>
    /// sets togglable features in optionManager.cs prior to running start up sequences
    /// </summary>
    public void InitialiseFeatures()
    {
        GameManager.i.optionScript.isAI = isAI;
        GameManager.i.optionScript.isNemesis = isNemesis;
        GameManager.i.optionScript.isFogOfWar = isFOW;
        GameManager.i.optionScript.isDecisions = isDecisions;
        GameManager.i.optionScript.isMainInfoApp = isMainInfoApp;
        GameManager.i.optionScript.isNPC = isNPC;
        GameManager.i.optionScript.isSubordinates = isSubordinates;
        GameManager.i.optionScript.isReviews = isReviews;
        GameManager.i.optionScript.isObjectives = isObjectives;
        GameManager.i.optionScript.isOrganisations = isOrganisations;
        GameManager.i.optionScript.isTargets = isTargets;
        GameManager.i.optionScript.isGear = isGear;
        GameManager.i.optionScript.isRecruit = isRecruit;
        GameManager.i.optionScript.isMoveSecurity = isMoveSecurity;
        GameManager.i.optionScript.isActions = isActions;
        if (isTargets == false)
        {
            //automatically disable Objectives and Organisations if targets are also disabled
            GameManager.i.optionScript.isObjectives = false;
            GameManager.i.optionScript.isOrganisations = false;
        }
        //set button texts in DebugGUI.cs
        if (isSubordinates == true)
        { GameManager.i.debugScript.optionSubordinates = "Subordinates OFF"; }
        else { GameManager.i.debugScript.optionSubordinates = "Subordinates ON"; }
        if (isAI == true)
        { GameManager.i.debugScript.optionNoAI = "AI OFF"; }
        else { GameManager.i.debugScript.optionNoAI = "AI ON"; }
        if (isFOW == true)
        { GameManager.i.debugScript.optionFogOfWar = "FOW OFF"; }
        else { GameManager.i.debugScript.optionFogOfWar = "FOW ON"; }
        if (isDecisions == true)
        { GameManager.i.debugScript.optionDecisions = "Decisions OFF"; }
        else { GameManager.i.debugScript.optionDecisions = "Decisions ON"; }
        if (isMainInfoApp == true)
        { GameManager.i.debugScript.optionMainInfoApp = "InfoApp OFF"; }
        else { GameManager.i.debugScript.optionMainInfoApp = "InfoApp ON"; }
        if (isReviews == true)
        { GameManager.i.debugScript.optionReviews = "Reviews OFF"; }
        else { GameManager.i.debugScript.optionReviews = "Reviews ON"; }
        if (isNemesis == true)
        { GameManager.i.debugScript.optionNemesis = "Nemesis OFF"; }
        else { GameManager.i.debugScript.optionNemesis = "Nemesis ON"; }
        if (isNPC == true)
        { GameManager.i.debugScript.optionNPC = "NPC OFF"; }
        else { GameManager.i.debugScript.optionNPC = "NPC ON"; }
        if (isObjectives == true)
        { GameManager.i.debugScript.optionObjectives = "Objectives OFF"; }
        else { GameManager.i.debugScript.optionObjectives = "Objectives ON"; }
        if (isOrganisations == true)
        { GameManager.i.debugScript.optionOrganisations = "Organisations OFF"; }
        else { GameManager.i.debugScript.optionOrganisations = "Organisations ON"; }
        if (isTargets == true)
        { GameManager.i.debugScript.optionTargets = "Targets OFF"; }
        else { GameManager.i.debugScript.optionTargets = "Targets ON"; }
        if (isGear == true)
        { GameManager.i.debugScript.optionGear = "Gear OFF"; }
        else { GameManager.i.debugScript.optionGear = "Gear ON"; }
        if (isRecruit == true)
        { GameManager.i.debugScript.optionRecruit = "Recruiting OFF"; }
        else { GameManager.i.debugScript.optionRecruit = "Recruiting ON"; }
        if (isMoveSecurity == true)
        { GameManager.i.debugScript.optionMoveSecurity = "Move Sec OFF"; }
        else { GameManager.i.debugScript.optionMoveSecurity = "Move Sec ON"; }
        if (isActions == true)
        { GameManager.i.debugScript.optionActions = "Actions OFF"; }
        else { GameManager.i.debugScript.optionActions = "Actions ON"; }
    }
    #endregion

    #region ToggleOnMapActors
    /// <summary>
    /// OnMap actors on/off -> includes toggling 'Actor' topics 
    /// </summary>
    /// <param name="isActive"></param>
    public void ToggleOnMapActors(bool isActive)
    {
        if (isActive == true)
        {
            //activate onMap actors
            GameManager.i.optionScript.isSubordinates = true;
            GameManager.i.dataScript.ToggleTopicType("Actor", false);
        }
        else
        {
            //disable onMap actors
            GameManager.i.optionScript.isSubordinates = false;
            GameManager.i.dataScript.ToggleTopicType("Actor");
        }
        GameManager.i.actorScript.ToggleOnMapActors(isActive);
    }
    #endregion

    #region ToggleAISideWidget
    /// <summary>
    /// toggle AI side tab and display
    /// </summary>
    /// <param name="isActive"></param>
    public void ToggleAISideWidget(bool isActive)
    {
        //switch on
        if (isActive == true)
        {
            //only need to switch on if already switched off
            if (GameManager.i.aiDisplayScript.isActive == false)
            {
                GameManager.i.aiDisplayScript.isActive = true;
                GameManager.i.aiDisplayScript.SetAllStatus(true);
            }
            if (GameManager.i.aiSideTabScript.isActive == false)
            {
                GameManager.i.aiSideTabScript.isActive = true;
                GameManager.i.aiSideTabScript.SetAllStatus(true);
            }
        }
        else
        {
            //switch off -> only if already switched on
            if (GameManager.i.aiDisplayScript.isActive == true)
            {
                GameManager.i.aiDisplayScript.isActive = false;
                GameManager.i.aiDisplayScript.SetAllStatus(false);
            }
            if (GameManager.i.aiSideTabScript.isActive == true)
            {
                GameManager.i.aiSideTabScript.isActive = false;
                GameManager.i.aiSideTabScript.SetAllStatus(false);
            }
        }
    }
    #endregion



    //new methods above here
}
