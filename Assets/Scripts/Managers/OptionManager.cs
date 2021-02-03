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
    [HideInInspector] public bool isfogOfWar = false;                               //if true then one sides sees only the information that they should
    [HideInInspector] public bool isAI = false;                                     //if false AI is switched off for both sides (debug purposes)
    [HideInInspector] public bool isNemesis = true;                                 //if false Nemesis is switched off
    [HideInInspector] public bool isDecisions = true;                               //if false Decisions are switched off
    [HideInInspector] public bool isMainInfoApp = true;                             //if false MainInfoApp is switched off
    [HideInInspector] public bool isNPC = true;                                     //if false NPC is switched off
    [HideInInspector] public bool isSubordinates = true;                            //if false Subordinates are switched off
    [HideInInspector] public bool isReviews = true;                                 //if false Performance reviews are switched off
    [HideInInspector] public bool isActorPool = true;                               //if false all actors are randomly generated, if true then ActorPoolFinal.SO actors are used (GameManager.cs -> isRandomActors)

    //UI options
    [HideInInspector] public bool showContacts = false;                           //if true node tooltips will show contact as well as Actor Arcs for nodes where actors have contacts
    [HideInInspector] public bool showPower = true;                              //if true power UI elements shown for actors and player, if false show compatibility instead
    [HideInInspector] public bool connectorTooltips = false;                      //if true then connectors have tooltips
    [HideInInspector] public bool fullMoodInfo = false;

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
            .AppendFormat(" Side -> {0}{1}", GameManager.i.sideScript.PlayerSide, "\n")
            .AppendFormat("{0}- Game Options{1}", "\n", "\n")
            .AppendFormat(" Auto Gear (Dice ignored if not enough Power) -> {0}{1}", autoGearResolution, "\n")
            .AppendFormat(" Billboards -> {0}{1}", billboard, "\n")
            .AppendFormat("{0}- Debug Options{1}", "\n", "\n")
            .AppendFormat(" Debug Data -> {0}{1}", debugData, "\n")
            .AppendFormat(" isFogOfWar -> {0}{1}", isfogOfWar, "\n")
            .AppendFormat(" isAI -> {0}{1}", isAI, "\n")
            .AppendFormat(" isNemesis -> {0}{1}", isNemesis, "\n")
            .AppendFormat(" isDecisions -> {0}{1}", isDecisions, "\n")
            .AppendFormat(" isMainInfoApp -> {0}{1}", isMainInfoApp, "\n")
            .AppendFormat(" isNPC -> {0}{1}", isNPC, "\n")
            .AppendFormat(" isSubordinates -> {0}{1}", isSubordinates, "\n")
            .AppendFormat(" isReviews -> {0}{1}", isReviews, "\n")
            .AppendFormat(" isActorPool -> {0}{1}", isActorPool, "\n")
            .AppendFormat(" NO Nodes -> {0}{1}", noNodes, "\n")
            .AppendFormat("{0}- UI Options{1}", "\n", "\n")
            .AppendFormat(" Connector Tooltips -> {0}{1}", connectorTooltips, "\n")
            .AppendFormat(" Full Mood Information -> {0}{1}", fullMoodInfo, "\n")
            .AppendFormat(" Show Contacts -> {0}{1}", showContacts, "\n")
            .AppendFormat(" Show Power -> {0}{1}", showPower, "\n")
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
