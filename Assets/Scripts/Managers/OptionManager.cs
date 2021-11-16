using gameAPI;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all Player togglable game option matters
/// </summary>
public class OptionManager : MonoBehaviour
{
    #region Save Data Compatible
    //game options
    [HideInInspector] public bool autoGearResolution = false;                     //if true then dice roller ignored whenever not enough power to save gear
    [HideInInspector] public bool billboard = true;                               //if true billboard shows between turns

    //Debug options
    //
    //NOTE: most of these are controlled by GameManager.cs -> InitialiseFeatures (set options in the GameManager.cs prefab under 'Features')
    //
    [HideInInspector] public bool debugData = false;                                //if true debug data is displayed onscreen
    [HideInInspector] public bool isFogOfWar = false;                               //if true then one sides sees only the information that they should
    [HideInInspector] public bool isAI = false;                                     //if false AI is switched off for both sides (debug purposes)
    [HideInInspector] public bool isNemesis = true;                                 //if false Nemesis is switched off
    [HideInInspector] public bool isDecisions = true;                               //if false Decisions are switched off
    [HideInInspector] public bool isMainInfoApp = true;                             //if false MainInfoApp is switched off
    [HideInInspector] public bool isObjectives = true;                              //if false Objectives are switched off
    [HideInInspector] public bool isOrganisations = true;                           //if false Organisations are switched off
    [HideInInspector] public bool isTargets = true;                                 //if false Targets are switched off
    [HideInInspector] public bool isNPC = true;                                     //if false NPC is switched off
    [HideInInspector] public bool isSubordinates = true;                            //if false Subordinates are switched off
    [HideInInspector] public bool isReviews = true;                                 //if false Performance reviews are switched off
    [HideInInspector] public bool isActorPool = true;                               //if false all actors are randomly generated, if true then ActorPoolFinal.SO actors are used (GameManager.cs -> isRandomActors)
    [HideInInspector] public bool isOnMapRandom = false;                            //if true OnMap actors will be selected randomly from actorPoolFinal.SO. Only applies if isActorPool true
    [HideInInspector] public bool isGear = true;                                    //if false Gear mechanics and UI are switch off
    [HideInInspector] public bool isRecruit = true;                                 //if false no actors can be recruited
    [HideInInspector] public bool isMoveSecurity = true;                            //if false, connection security is ignored when moving
    [HideInInspector] public bool isActions = true;                                 //if false there are no actions. New turn only on request

    //Help Messages
    [HideInInspector] public bool isHelpMessages = false;                           //if true helpMessages are generated

    //UI options
    [HideInInspector] public bool showContacts = false;                             //if true node tooltips will show contact as well as Actor Arcs for nodes where actors have contacts
    [HideInInspector] public bool showPower = true;                                 //if true power UI elements shown for actors and player, if false show compatibility instead
    [HideInInspector] public bool connectorTooltips = false;                        //if true then connectors have tooltips
    [HideInInspector] public bool fullMoodInfo = false;

    //GUI functionality
    [HideInInspector] public bool isActorLeftMenu = true;                           //left click actor menu
    [HideInInspector] public bool isActorRightMenu = true;                          //right click actor menu
    [HideInInspector] public bool isNodeLeftMenu = true;                            //left click node menu
    [HideInInspector] public bool isNodeRightMenu = true;                           //right click node menu
    [HideInInspector] public bool isPlayerLeftMenu = true;                          //left click player menu
    [HideInInspector] public bool isPlayerRightMenu = true;                         //right click player menu
    [HideInInspector] public bool isTopWidget = true;                               //top centre widget (turn, actions, support, etc)
    [HideInInspector] public bool isFinder = true;                                  //if true district finder (RHS tab) displayed

    //Backing fields (use underscore)
    private ColourScheme _colourOption;
    #endregion

    //Development -> special option for getting district prefabs up and running -> if true node faceText is switched off and districts are used instead of nodes
    public bool noNodes = false;

    //ColourManager.cs ColourScheme enum (eg. 0 -> normal, 1 -> colourblind)
    public ColourScheme ColourOption
    {
        get { return _colourOption; }
        set
        {
            _colourOption = value;
            //Post notification - colour scheme has been changed
            if (GameManager.i.inputScript.GameState != GameState.LoadAtStart)
            {
                EventManager.i.PostNotification(EventType.ChangeColour, this, null, "OptionManager.cs -> ColourOption");
                Debug.Log("OptionManager -> Colour Scheme: now " + _colourOption + "\n");
            }
        }
    }

    /// <summary>
    /// Set all GUI options to default settings for a new game
    /// </summary>
    public void SetAllGUIOptionsToDefault()
    {
        //gui functionality
        isActorLeftMenu = true;
        isActorRightMenu = true;
        isPlayerLeftMenu = true;
        isPlayerRightMenu = true;
        isNodeLeftMenu = true;
        isNodeRightMenu = true;
        isTopWidget = true;
        isFinder = true;
        GameManager.i.widgetTopScript.SetWidget(isTopWidget);
    }


    /// <summary>
    /// Debug method
    /// </summary>
    /// <returns></returns>
    public string DisplayOptions()
    {
        return new StringBuilder()
            .AppendFormat(" Current Option Settings{0}{1}", "\n", "\n")
            .AppendFormat(" Side -> {0}{1}", GameManager.i.sideScript.PlayerSide, "\n")
            .AppendFormat("{0}- Game Options{1}", "\n", "\n")
            .AppendFormat(" Auto Gear (Dice ignored if not enough Power) -> {0}{1}", autoGearResolution, "\n")
            .AppendFormat(" Billboards -> {0}{1}", billboard, "\n")
            .AppendFormat("{0}- Debug Options{1}", "\n", "\n")
            .AppendFormat(" Debug Data -> {0}{1}", debugData, "\n")
            .AppendFormat(" isFogOfWar -> {0}{1}", isFogOfWar, "\n")
            .AppendFormat(" isAI -> {0}{1}", isAI, "\n")
            .AppendFormat(" isNemesis -> {0}{1}", isNemesis, "\n")
            .AppendFormat(" isDecisions -> {0}{1}", isDecisions, "\n")
            .AppendFormat(" isMainInfoApp -> {0}{1}", isMainInfoApp, "\n")
            .AppendFormat(" isNPC -> {0}{1}", isNPC, "\n")
            .AppendFormat(" isSubordinates -> {0}{1}", isSubordinates, "\n")
            .AppendFormat(" isReviews -> {0}{1}", isReviews, "\n")
            .AppendFormat(" isObjectives -> {0}{1}", isObjectives, "\n")
            .AppendFormat(" isOrganisations -> {0}{1}", isOrganisations, "\n")
            .AppendFormat(" isTargets -> {0}{1}", isTargets, "\n")
            .AppendFormat(" isActorPool -> {0}{1}", isActorPool, "\n")
            .AppendFormat(" isOnMapRandom -> {0}{1}", isOnMapRandom, "\n")
            .AppendFormat(" IsGear -> {0}{1}", isGear, "\n")
            .AppendFormat(" IsRecruit -> {0}{1}", isRecruit, "\n")
            .AppendFormat(" IsMoveSecurity -> {0}{1}", isMoveSecurity, "\n")
            .AppendFormat(" isActions -> {0}{1}", isActions, "\n")
            .AppendFormat(" NO Nodes -> {0}{1}", noNodes, "\n")
            .AppendFormat("{0}- Help Messages{1}", "\n", "\n")
            .AppendFormat(" isHelpMessages -> {0}{1}", isHelpMessages, "\n")
            .AppendFormat("{0}- UI Options{1}", "\n", "\n")
            .AppendFormat(" Connector Tooltips -> {0}{1}", connectorTooltips, "\n")
            .AppendFormat(" Full Mood Information -> {0}{1}", fullMoodInfo, "\n")
            .AppendFormat(" Show Contacts -> {0}{1}", showContacts, "\n")
            .AppendFormat(" Show Power -> {0}{1}", showPower, "\n")
            .AppendFormat("{0}- GUI Functionality{1}", "\n", "\n")
            .AppendFormat(" Actor Left Click Menu -> {0}{1}", isActorLeftMenu, "\n")
            .AppendFormat(" Actor Right Click Menu -> {0}{1}", isActorRightMenu, "\n")
            .AppendFormat(" Player Left Click Menu -> {0}{1}", isPlayerLeftMenu, "\n")
            .AppendFormat(" Player Right Click Menu -> {0}{1}", isPlayerRightMenu, "\n")
            .AppendFormat(" Node Left Click Menu -> {0}{1}", isNodeLeftMenu, "\n")
            .AppendFormat(" Node Right Click Menu -> {0}{1}", isNodeRightMenu, "\n")
            .AppendFormat(" Top Centre Widget -> {0}{1}", isTopWidget, "\n")
            .AppendFormat(" District Finder -> {0}{1}", isFinder, "\n")
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
