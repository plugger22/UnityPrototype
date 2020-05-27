using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// handles all gear related matters (Only the Resistance has gear)
/// </summary>
public class GearManager : MonoBehaviour
{
    [Range(0, 4)] public int maxNumOfGear = 3;
    [Tooltip("Whenever a random selection of gear is provided there is a chance * actor Ability of it being a Rare item, otherwise it's the standard Common")]
    [Range(1, 10)] public int chanceOfRareGear = 5;

    [Header("Compromised Gear")]
    [Tooltip("Chance gear will be compromised and be no longer of any benefit after each use")]
    [Range(25, 75)] public int chanceOfCompromise = 50;

    [Header("Swap Gear")]
    [Tooltip("Motivation boost/cost to give/take gear from an actor")]
    [Range(0, 3)] public int gearSwapBaseAmount = 1;
    [Tooltip("Motivation boost/cost to give/take PREFERRED gear from an actor")]
    [Range(0, 3)] public int gearSwapPreferredAmount = 1;
    [Tooltip("Base cost to save compromised gear (in Renown)")]
    [Range(0, 3)] public int gearSaveBaseCost = 0;

    [Header("Actor Gear")]
    [Tooltip("Number of turns after actor is given gear before it can be handed back and before it begins to be tested for being lost")]
    [Range(0, 10)] public int actorGearGracePeriod = 3;
    [Tooltip("% Chance, per turn, of actor losing gear in his possession. Only checked after actorGearGracePeriod number of turns")]
    [Range(0, 100)] public int actorGearLostChance = 20;

    [Header("Special Gear")]
    [Tooltip("Gear that gives movement rate of 2 districts instead of one and ignores all security")]
    public Gear gearSpecialMove;

    //used for quick reference  -> rarity
    [HideInInspector] public GearRarity gearCommon;
    [HideInInspector] public GearRarity gearRare;
    [HideInInspector] public GearRarity gearUnique;
    [HideInInspector] public GearRarity gearSpecial;
    //quick reference -> type
    [HideInInspector] public GearType typeHacking;
    [HideInInspector] public GearType typeInfiltration;
    [HideInInspector] public GearType typeInvisibility;
    [HideInInspector] public GearType typeKinetic;
    [HideInInspector] public GearType typeMovement;
    [HideInInspector] public GearType typeRecovery;
    [HideInInspector] public GearType typePersuasion;

    //cached gear picker choices
    private int selectionPlayerTurn;                                          //turn number of last choice for a Player gear selection
    private int selectionActorTurn;                                           //turn number of last choice for an Actor gear selection
    private GenericPickerDetails cachedPlayerDetails;                         //last player gear selection this action
    private GenericPickerDetails cachedActorDetails;                          //last actor gear selection this action
    private bool isNewActionPlayer;                                           //set to true after player makes a gear choice at own node
    private bool isNewActionActor;                                            //set to true after player makes a gear choice at an actor contact's node

    #region Save Data Compatible
    //compromised gear
    private int gearSaveCurrentCost;                                          //how much renown to save a compromised item of gear (increments +1 each time option used)
    private List<string> listOfCompromisedGear;                               //list cleared each turn that contains names of compromised gear (used for outcome dialogues)
    #endregion

    //fast access
    private int maxGenericOptions = -1;
    //fast access -> traits
    private string actorLoseGearHigh;
    private string actorLoseGearNone;
    //fast access -> sides
    private GlobalSide globalResistance;
    private GlobalSide globalAuthority;

    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourSide;
    private string colourGear;
    private string colourDefault;
    private string colourGrey;
    private string colourNormal;
    private string colourAlert;
    private string colourEnd;


    /// <summary>
    /// Initialisation. Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseLevelStart();
                SubInitailiseCachedGearData();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseLevelStart();
                SubInitailiseCachedGearData();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseLevelStart();
                SubInitailiseCachedGearData();
                SubInitialiseEvents();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //initialise fast access variables -> rarity
        List<GearRarity> listOfGearRarity = GameManager.i.dataScript.GetListOfGearRarity();
        if (listOfGearRarity != null)
        {
            foreach (GearRarity rarity in listOfGearRarity)
            {
                //pick out and assign the ones required for fast access, ignore the rest. 
                //Also dynamically assign gearRarity.level values (0/1/2). Do so here as lots of gear calc's depend on these and they need to be correct
                switch (rarity.name)
                {
                    case "Common":
                        gearCommon = rarity;
                        rarity.level = 0;
                        break;
                    case "Rare":
                        gearRare = rarity;
                        rarity.level = 1;
                        break;
                    case "Unique":
                        gearUnique = rarity;
                        rarity.level = 2;
                        break;
                    case "Special":
                        gearSpecial = rarity;
                        rarity.level = 3;
                        break;
                }
            }
            //error check
            if (gearCommon == null) { Debug.LogError("Invalid gearCommon (Null)"); }
            if (gearRare == null) { Debug.LogError("Invalid gearRare (Null)"); }
            if (gearUnique == null) { Debug.LogError("Invalid gearUnique (Null)"); }
            if (gearSpecial == null) { Debug.LogError("Invalid gearSpecial (Null)"); }
        }
        else { Debug.LogError("Invalid listOfGearRarity (Null)"); }
        //initialise fast access variables -> type
        List<GearType> listOfGearType = GameManager.i.dataScript.GetListOfGearType();
        Debug.Assert(listOfGearType != null, "Invalid listOfGearType (Null)");
        //fast access
        globalResistance = GameManager.i.globalScript.sideResistance;
        globalAuthority = GameManager.i.globalScript.sideAuthority;
        actorLoseGearHigh = "ActorLoseGearHigh";
        actorLoseGearNone = "ActorLoseGearNone";
        maxGenericOptions = GameManager.i.genericPickerScript.maxOptions;
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(maxGenericOptions != -1, "Invalid maxGenericOptions (-1)");
        if (listOfGearType != null)
        {
            foreach (GearType gearType in listOfGearType)
            {
                //pick out and assign the ones required for fast access, ignore the rest. 
                switch (gearType.name)
                {
                    case "Hacking":
                        typeHacking = gearType;
                        break;
                    case "Infiltration":
                        typeInfiltration = gearType;
                        break;
                    case "Invisibility":
                        typeInvisibility = gearType;
                        break;
                    case "Kinetic":
                        typeKinetic = gearType;
                        break;
                    case "Movement":
                        typeMovement = gearType;
                        break;
                    case "Recovery":
                        typeRecovery = gearType;
                        break;
                    case "Persuasion":
                        typePersuasion = gearType;
                        break;
                }
            }
            //error check
            if (typeHacking == null) { Debug.LogError("Invalid typeHacking (Null)"); }
            if (typeInfiltration == null) { Debug.LogError("Invalid typeInfiltration (Null)"); }
            if (typeInvisibility == null) { Debug.LogError("Invalid typeInvisibility (Null)"); }
            if (typeKinetic == null) { Debug.LogError("Invalid typeKinetic (Null)"); }
            if (typeMovement == null) { Debug.LogError("Invalid typeMovement (Null)"); }
            if (typeRecovery == null) { Debug.LogError("Invalid typeRecovery (Null)"); }
            if (typePersuasion == null) { Debug.LogError("Invalid typePersuasion (Null)"); }
        }
        else { Debug.LogError("Invalid listOfGearType (Null)"); }
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event Listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "GearManager");
        EventManager.instance.AddListener(EventType.GearAction, OnEvent, "GearManager");
        EventManager.instance.AddListener(EventType.GenericGearChoice, OnEvent, "GearManager");
        EventManager.instance.AddListener(EventType.GenericCompromisedGear, OnEvent, "GearManager");
        EventManager.instance.AddListener(EventType.InventorySetGear, OnEvent, "GearManager");
        EventManager.instance.AddListener(EventType.EndTurnEarly, OnEvent, "GearManager");
    }
    #endregion

    #region SubInitialiseCachedGearData
    private void SubInitailiseCachedGearData()
    {
        //cached
        selectionPlayerTurn = -1;
        selectionActorTurn = -1;
        cachedPlayerDetails = null;
        cachedActorDetails = null;
        isNewActionPlayer = true;
        isNewActionActor = true;
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        listOfCompromisedGear = new List<string>();
        //gear save cost
        gearSaveCurrentCost = gearSaveBaseCost;
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        //initialise gear lists
        Dictionary<string, Gear> dictOfGear = GameManager.i.dataScript.GetDictOfGear();
        if (dictOfGear != null)
        {
            //set up an array of Lists, with index corresponding to GearLevel enum, eg. Common / Rare / Unique
            List<string>[] arrayOfGearLists = new List<string>[GameManager.i.dataScript.GetNumOfGearRarity()];
            for (int i = 0; i < arrayOfGearLists.Length; i++)
            { arrayOfGearLists[i] = new List<string>(); }
            //loop dict and allocate gear to various lists
            foreach (var gearEntry in dictOfGear)
            {
                //reset stats (SO carries values over between sessions)
                gearEntry.Value.ResetStats();
                //assign to a list based on rarity
                int index = gearEntry.Value.rarity.level;
                //exclude MetaGame special gear
                if (index < 3)
                { arrayOfGearLists[index].Add(gearEntry.Value.name); }
            }
            //initialise dataManager lists with local lists
            for (int i = 0; i < arrayOfGearLists.Length; i++)
            {
                GearRarity indexRarity = GameManager.i.dataScript.GetGearRarity(i);
                if (indexRarity != null)
                { GameManager.i.dataScript.SetGearList(arrayOfGearLists[i], indexRarity); }
                else { Debug.LogError("Invalid indexRarity (Null)"); }
            }
        }
        else { Debug.LogError("Invalid dictOfGear (Null) -> Gear not initialised"); }

    }

    #endregion

    #endregion



    /// <summary>
    /// Called when an event happens
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect Event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.EndTurnEarly:
                EndTurnEarly();
                break;
            case EventType.GearAction:
                ModalActionDetails details = Param as ModalActionDetails;
                InitialiseGenericPickerGear(details);
                break;
            case EventType.GenericGearChoice:
                GenericReturnData returnDataGear = Param as GenericReturnData;
                ProcessGearChoice(returnDataGear);
                break;
            case EventType.GenericCompromisedGear:
                GenericReturnData returnDataCompromised = Param as GenericReturnData;
                ProcessCompromisedGear(returnDataCompromised);
                break;
            case EventType.InventorySetGear:
                InitialiseGearInventoryDisplay();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourGood = GameManager.i.colourScript.GetColour(ColourType.goodText);
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.badText);
        colourSide = GameManager.i.colourScript.GetColour(ColourType.blueText);
        colourDefault = GameManager.i.colourScript.GetColour(ColourType.whiteText);
        colourNormal = GameManager.i.colourScript.GetColour(ColourType.normalText);
        colourGrey = GameManager.i.colourScript.GetColour(ColourType.greyText);
        colourGear = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
    }

    /// <summary>
    /// End turn final events
    /// </summary>
    private void EndTurnEarly()
    {
        CheckForCompromisedGear();
        CheckActorGear();
    }


    /// <summary>
    /// run at turn end to check all gear (Player and actor, eg. target use) that has been used to see if it's compromised. Generates Generic Picker for player choice (use renown to keep gear) if so
    /// </summary>
    public void CheckForCompromisedGear()
    {
        int chance, rnd;
        bool isCompromisedGear = false;
        listOfCompromisedGear.Clear();
        //inclues all gear held by player and actors
        List<string> listOfGear = GameManager.i.dataScript.GetListOfCurrentGear();
        List<string> listOfPlayerGear = new List<string>();
        if (listOfGear != null)
        {
            if (listOfGear.Count > 0)
            {
                for (int i = 0; i < listOfGear.Count; i++)
                {
                    Gear gear = GameManager.i.dataScript.GetGear(listOfGear[i]);
                    if (gear != null)
                    {
                        //gear has been used?
                        if (gear.timesUsed > 0)
                        {
                            //check to see if compromised
                            chance = GetChanceOfCompromise(gear.name);
                            for (int j = 0; j < gear.timesUsed; j++)
                            {
                                rnd = Random.Range(0, 100);
                                if (rnd < chance)
                                {
                                    //gear COMPROMISED
                                    gear.isCompromised = true;
                                    gear.chanceOfCompromise = chance;
                                    //add to list (used for outcome dialogues)
                                    listOfCompromisedGear.Add(gear.tag.ToUpper());
                                    if (GameManager.i.playerScript.CheckGearPresent(gear.name) == true)
                                    {
                                        listOfPlayerGear.Add(gear.name);
                                        isCompromisedGear = true;
                                        GameManager.i.playerScript.isEndOfTurnGearCheck = true;
                                    }
                                    else { GameManager.i.actorScript.isGearCheckRequired = true; }
                                    //stat (before message)
                                    gear.statTimesCompromised++;
                                    //admin
                                    Debug.LogFormat("[Rnd] GearManager.cs -> CheckForCompromisedGear: {0} COMPROMISED, need < {1}, rolled {2}{3}", gear.tag, chance, rnd, "\n");
                                    Debug.LogFormat("[Gea] -> CheckForCompromisedGear: {0}, {1},  Compromised ({2}){3}", gear.tag, gear.type.name, gear.reasonUsed, "\n");
                                    string msgText = string.Format("Gear {0} COMPROMISED", gear.tag);
                                    GameManager.i.messageScript.GeneralRandom(msgText, "Compromised Gear", chance, rnd, true);
                                    break;
                                }
                                else
                                {
                                    //gear not compromised
                                    gear.chanceOfCompromise = chance;
                                    Debug.LogFormat("[Rnd] GearManager.cs -> CheckForCompromisedGear: {0} NOT compromised, need < {1}, rolled {2}{3}", gear.tag, chance, rnd, "\n");
                                    string msgText = string.Format("Gear {0} NOT Compromised", gear.tag);
                                    GameManager.i.messageScript.GeneralRandom(msgText, "Compromised Gear", chance, rnd, true);
                                }
                            }
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid gear (Null) for gearID {0}", listOfGear[i]); }
                }
                //any Player compromised gear? (Actor gear is automatically lost if compromised, handled by ActorManager.cs -> CheckActive/InactiveResistanceActorsHuman)
                if (isCompromisedGear == true)
                {
                    //check Player has enough renown to save gear
                    int playerRenown = GameManager.i.playerScript.Renown;
                    if (playerRenown >= gearSaveCurrentCost)
                    {
                        //initialise generic picker
                        InitialiseCompromisedGearPicker();
                    }
                    else
                    {
                        //not enough renown -> remove gear and reset stats
                        /*ProcessCompromisedGear(null);*/
                        GameManager.i.playerScript.UpdateGear();
                        //outcome dialogue
                        ModalOutcomeDetails details = new ModalOutcomeDetails();
                        StringBuilder builderTop = new StringBuilder();
                        builderTop.AppendFormat("{0}Gear used this turn has been {1}{2}COMPROMISED{3}{4}", colourNormal, colourEnd, colourBad, colourEnd, "\n");
                        builderTop.AppendFormat("{0}You have {1}{2}{3}Insufficient Renown{4}{5}", colourNormal, colourEnd, "\n", colourBad, colourEnd, "\n");
                        builderTop.AppendFormat("<size=85%>({0}{1}{2} needed)</size>{3}", colourNeutral, gearSaveCurrentCost, colourEnd, "\n");
                        builderTop.AppendFormat("{0}to {1}{2}Save{3}{4} any gear{5}", colourNormal, colourEnd, colourGood, colourEnd, colourNormal, colourEnd);
                        details.textTop = builderTop.ToString();
                        StringBuilder builderBottom = new StringBuilder();
                        foreach (string gearName in listOfCompromisedGear)
                        {
                            if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                            builderBottom.AppendFormat("{0}{1}{2}{3} has been LOST{4}", colourNeutral, gearName, colourEnd, colourBad, colourEnd);
                        }
                        details.textBottom = builderBottom.ToString();
                        /*EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "GearManager.cs -> ProcessCompromisedGear");*/

                        //add to start of turn info Pipeline
                        details.type = MsgPipelineType.CompromisedGear;
                        if (GameManager.i.guiScript.InfoPipelineAdd(details) == false)
                        { Debug.LogWarningFormat("Compromised Gear infoPipeline message FAILED to be added to dictOfPipeline"); }
                    }
                }
            }
        }
        else { Debug.LogError("Invalid listOfGear (Null)"); }
    }

    /// <summary>
    /// checks all ACTIVE actors with gear, increments timers and checks for lost gear
    /// </summary>
    private void CheckActorGear()
    {
        int rnd, chance;
        string traitName;
        //loop OnMap actors
        Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActors(globalResistance);
        foreach (Actor actor in arrayOfActors)
        {
            //if invalid actor or vacant position then ignore
            if (actor != null)
            {
                //only check Active actors
                if (actor.Status == ActorStatus.Active)
                {
                    if (string.IsNullOrEmpty(actor.GetGearName()) == false)
                    {
                        Gear gear = GameManager.i.dataScript.GetGear(actor.GetGearName());
                        if (gear != null)
                        {
                            actor.IncrementGearTimer();
                            int timer = actor.GetGearTimer();
                            timer -= 1;
                            //grace period about to be exceeded
                            if (timer == actorGearGracePeriod)
                            {
                                //let player know that gear will be available
                                string msgText = string.Format("{0} gear held by {1}, available next turn", gear.tag, actor.arc.name);
                                GameManager.i.messageScript.GearAvailable(msgText, gear, actor);
                            }
                            //grace period exceeded
                            if (timer > actorGearGracePeriod)
                            {
                                //check for Gear being LOST (only after grace period has expired)
                                rnd = Random.Range(0, 100);
                                chance = actorGearLostChance;
                                //trait check
                                if (actor.CheckTraitEffect(actorLoseGearHigh) == true)
                                {
                                    chance *= 3;
                                    traitName = string.Format(" ({0}) ", actor.GetTrait().tag);
                                    GameManager.i.actorScript.TraitLogMessage(actor, "for a Lose Gear check", "to TRIPLE chance of losing Gear");
                                }
                                else if (actor.CheckTraitEffect(actorLoseGearNone) == true)
                                {
                                    chance = 0;
                                    GameManager.i.actorScript.TraitLogMessage(actor, "for a Lose Gear check", "to AVOID losing Gear");
                                    traitName = string.Format(" ({0}) ", actor.GetTrait().tag);
                                }
                                else { traitName = ""; }
                                //roll die
                                if (rnd < chance)
                                {
                                    //Gear LOST
                                    Debug.LogFormat("[Rnd] GearManager.cs -> CheckActorGear: {0} LOST ({1}), need < {2}{3}, rolled {4}{5}",
                                        gear.tag, actor.arc.name, chance, traitName, rnd, "\n");
                                    //message
                                    string msgText = string.Format("{0} gear lost by {1}, {2}{3}", gear.tag, actor.actorName, actor.arc.name, traitName);
                                    GameManager.i.messageScript.GearLost(msgText, gear, actor);
                                    //random
                                    msgText = string.Format("Gear {0} LOST", gear.tag);
                                    GameManager.i.messageScript.GeneralRandom(msgText, "Gear Lost", chance, rnd, true);
                                    //remove gear AFTER message
                                    actor.RemoveGear(GearRemoved.Lost);
                                }
                                else
                                {
                                    //random
                                    string msgText = string.Format("{0} gear, {1}, not lost", actor.arc.name, gear.tag);
                                    GameManager.i.messageScript.GeneralRandom(msgText, "Gear Lost", chance, rnd, true);
                                }
                            }
                        }
                        else { Debug.LogWarningFormat("Invalid gear (Null) for gearID {0}{1}", actor.GetGearName(), "\n"); }
                    }
                }
            }
        }
    }

    /// <summary>
    /// sets up and triggers generic picker for end of turn compromised gear decision. Player gear only
    /// </summary>
    private void InitialiseCompromisedGearPicker()
    {
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        //Obtain Gear
        genericDetails.returnEvent = EventType.GenericCompromisedGear;
        genericDetails.side = globalResistance;
        genericDetails.textHeader = "Compromised Gear";
        genericDetails.isHaltExecution = true;
        //state
        genericDetails.subState = ModalGenericPickerSubState.CompromisedGear;
        //picker text
        genericDetails.textTop = string.Format("Select {0}ONE{1} Item of Gear to {2}SAVE{3} <size=70%>(optional)</size>", colourNeutral, colourEnd, colourGood, colourEnd);
        genericDetails.textMiddle = string.Format("{0}Any Gear NOT Saved will be Lost{1}", colourAlert, colourEnd);
        genericDetails.textBottom = "Click on an item to Select. Press CONFIRM to SAVE gear. Mouseover gear for more information.";
        //renown cost to save gear
        genericDetails.data = gearSaveCurrentCost;
        //get all compromised gear
        List<string> listOfGear = GameManager.i.playerScript.GetListOfGear();
        if (listOfGear != null && listOfGear.Count > 0)
        {
            //loop gear (max 'maxGenericOptions' pieces of gear / max 'maxGenericOptions' options)
            for (int index = 0; index < listOfGear.Count; index++)
            {
                if (index < maxGenericOptions)
                {
                    Gear gear = GameManager.i.dataScript.GetGear(listOfGear[index]);
                    if (gear != null)
                    {
                        //is gear compromised?
                        if (gear.isCompromised == true)
                        {
                            //generate an option for picker
                            GenericTooltipDetails tooltipDetails = GetCompromisedGearTooltip(gear);
                            if (tooltipDetails != null)
                            {
                                //option details
                                GenericOptionDetails optionDetails = new GenericOptionDetails();
                                /*optionDetails.optionID = gear.gearID;*/
                                optionDetails.optionName = gear.name;
                                optionDetails.text = gear.tag.ToUpper();
                                optionDetails.sprite = gear.sprite;
                                //add to master arrays
                                genericDetails.arrayOfOptions[index] = optionDetails;
                                genericDetails.arrayOfTooltips[index] = tooltipDetails;
                            }
                        }
                        else if (gear.timesUsed > 0)
                        {
                            //gear is Used but NOT Compromised -> faded & unselectable option
                            GenericTooltipDetails tooltipDetails = GetUsedGearTooltip(gear);
                            if (tooltipDetails != null)
                            {
                                //option details
                                GenericOptionDetails optionDetails = new GenericOptionDetails();
                                /*optionDetails.optionID = gear.gearID;*/
                                optionDetails.optionName = gear.name;
                                optionDetails.text = gear.tag.ToUpper();
                                optionDetails.sprite = gear.sprite;
                                optionDetails.isOptionActive = false;
                                //add to master arrays
                                genericDetails.arrayOfOptions[index] = optionDetails;
                                genericDetails.arrayOfTooltips[index] = tooltipDetails;
                            }
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid gear (Null) for gearID {0}", listOfGear[index]); }
                }
                else
                {
                    //can only have 3 options
                    Debug.LogWarningFormat("Max of {0} gear options available -> appears to be more", maxGenericOptions);
                    break;
                }
            }
            //deactivate back button
            GameManager.i.genericPickerScript.SetBackButton(EventType.None);
            //open picker
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails, "GearManager.cs -> InitialiseGenericPickerGear");
        }
        else
        { Debug.LogWarning("Invalid listOfGear (Null or Empty). Compromised Gear Picker cancelled"); }

    }


    /// <summary>
    /// Choose Gear (Resistance): sets up ModalGenericPicker class and triggers event: ModalGenericEvent.cs -> SetGenericPicker()
    /// </summary>
    /// <param name="details"></param>
    private void InitialiseGenericPickerGear(ModalActionDetails details)
    {
        //first Gear Pick this action
        bool errorFlag = false;
        bool isIgnoreCache = false;
        bool isPlayer = false;
        int index;
        string gearName;
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Node node = GameManager.i.dataScript.GetNode(details.nodeID);
        if (node != null)
        {

            #region CaptureCheck
            //check for player/actor being captured
            int actorID = GameManager.i.playerScript.actorID;
            if (node.nodeID != GameManager.i.nodeScript.GetPlayerNodeID())
            {
                Actor actor = GameManager.i.dataScript.GetCurrentActor(details.actorDataID, globalResistance);
                if (actor != null)
                { actorID = actor.actorID; }
                else { Debug.LogError(string.Format("Invalid actor (Null) fro details.ActorSlotID {0}", details.actorDataID)); errorFlag = true; }
            }
            else { isPlayer = true; }
            //check capture provided no errors
            if (errorFlag == false)
            {
                CaptureDetails captureDetails = GameManager.i.captureScript.CheckCaptured(node.nodeID, actorID);
                if (captureDetails != null)
                {
                    //capture happened, abort recruitment
                    captureDetails.effects = string.Format("{0}The contact wasn't there. Nor was the gear.{1}", colourNeutral, colourEnd);
                    EventManager.instance.PostNotification(EventType.Capture, this, captureDetails, "GearManager.cs -> InitialiseGenericPickerGear");
                    return;
                }
            }
            else
            {
                //reset flag to the default state prior to recruitments
                errorFlag = false;
            }
            #endregion

            //get a new selection once per action. If multiple attempts during an action used the cached results to ensure the same gear is available on each attempt
            if (isPlayer == true)
            {
                if (selectionPlayerTurn != GameManager.i.turnScript.Turn || isNewActionPlayer == true)
                { isIgnoreCache = true; }
            }
            else
            {
                //actor gear selection (can be different from Players, eg. better chance of rare gear)
                if (selectionActorTurn != GameManager.i.turnScript.Turn || isNewActionActor == true)
                { isIgnoreCache = true; }
            }
            //proceed with a new gear Selection
            if (isIgnoreCache == true)
            {
                #region gearSelection
                //Obtain Gear
                genericDetails.returnEvent = EventType.GenericGearChoice;
                genericDetails.textHeader = "Select Gear";
                genericDetails.side = globalResistance;
                genericDetails.nodeID = details.nodeID;
                genericDetails.actorSlotID = details.actorDataID;
                //picker text
                genericDetails.textTop = string.Format("{0}Gear{1} {2}available{3}", colourNeutral, colourEnd, colourNormal, colourEnd);
                genericDetails.textMiddle = string.Format("{0}Gear will be placed in your inventory{1}",
                    colourNormal, colourEnd);
                genericDetails.textBottom = "Click on an item to Select. Press CONFIRM to obtain gear. Mouseover gear for more information.";
                //generate temp list of gear to choose from
                List<string> tempCommonGear = new List<string>(GameManager.i.dataScript.GetListOfGear(gearCommon));
                List<string> tempRareGear = new List<string>(GameManager.i.dataScript.GetListOfGear(gearRare));
                //
                //select two items of gear for the picker
                //
                string[] arrayOfGear = new string[2];
                int countOfGear = 0;
                for (int i = 0; i < arrayOfGear.Length; i++)
                {
                    gearName = null;
                    //any rare gear available?
                    if (tempRareGear.Count > 0)
                    {
                        //chance of rare gear -> base chance * actor connections (or 1 if player)
                        int chance = chanceOfRareGear;
                        Actor actor = GameManager.i.dataScript.GetCurrentActor(details.actorDataID, globalResistance);
                        if (actor != null)
                        {
                            //if Player doing it then assumed to have an ability of 1, actor (Fixer) may have a higher ability.
                            if (isPlayer == false)
                            { chance *= actor.GetDatapoint(ActorDatapoint.Contacts0); }
                        }
                        else
                        {
                            chance = 0;
                            Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", details.actorDataID));
                        }
                        int rnd = Random.Range(0, 100);
                        if (rnd < chance)
                        {
                            index = Random.Range(0, tempRareGear.Count);
                            gearName = tempRareGear[index];
                            tempRareGear.RemoveAt(index);
                            Gear rareGear = GameManager.i.dataScript.GetGear(gearName);
                            if (rareGear != null)
                            {
                                Debug.LogFormat("[Rnd] GearManager.cs -> InitialiseGenericPickerGear: Rare gear ({0}) Success, need < {1} roll {2}{3}", rareGear.tag, chance, rnd, "\n");
                                GameManager.i.messageScript.GeneralRandom(string.Format("Gear Choice, Rare gear ({0}) SUCCESS", rareGear.tag), "Rare Gear", chance, rnd);
                            }
                            else
                            {
                                Debug.LogFormat("[Rnd] GearManager.cs -> InitialiseGenericPickerGear: Rare gear Success, need < {0} roll {1}{2}", chance, rnd, "\n");
                                GameManager.i.messageScript.GeneralRandom("Gear Choice, Rare gear SUCCESS", "Rare Gear", chance, rnd);
                                Debug.LogWarningFormat("Invalid rare gear (Null) for gear {0}", gearName);
                            }

                        }
                        else if (tempCommonGear.Count > 0)
                        {
                            //if failed chance for rare gear then need to get common
                            index = Random.Range(0, tempCommonGear.Count);
                            gearName = tempCommonGear[index];
                            tempCommonGear.RemoveAt(index);
                            Debug.LogFormat("[Rnd] GearManager.cs -> InitialiseGenericPickerGear: Rare gear FAIL, need < {0} roll {1}{2}", chance, rnd, "\n");
                            GameManager.i.messageScript.GeneralRandom("Gear Choice, Rare gear FAILED", "Rare Gear", chance, rnd);
                        }
                    }
                    //common gear
                    else
                    {
                        if (tempCommonGear.Count > 0)
                        {
                            index = Random.Range(0, tempCommonGear.Count);
                            gearName = tempCommonGear[index];
                            tempCommonGear.RemoveAt(index);
                        }
                    }
                    //found some gear?
                    if (string.IsNullOrEmpty(gearName) == false)
                    { arrayOfGear[i] = gearName; countOfGear++; }
                }
                //check there is at least one item of gear available
                if (countOfGear < 1)
                {
                    //OUTCOME -> No gear available
                    Debug.LogWarning("GearManager: No gear available in InitaliseGenericPickerGear");
                    errorFlag = true;
                }
                else
                {
                    //
                    //loop gearID's that have been selected and package up ready for ModalGenericPicker
                    //
                    for (int i = 0; i < countOfGear; i++)
                    {
                        Gear gear = GameManager.i.dataScript.GetGear(arrayOfGear[i]);
                        if (gear != null)
                        {
                            //tooltip 
                            GenericTooltipDetails tooltipDetails = GetGearTooltip(gear);
                            if (tooltipDetails != null)
                            {
                                //option details
                                GenericOptionDetails optionDetails = new GenericOptionDetails();
                                optionDetails.optionName = gear.name;
                                optionDetails.text = gear.tag.ToUpper();
                                optionDetails.sprite = gear.sprite;
                                //add to master arrays
                                genericDetails.arrayOfOptions[i] = optionDetails;
                                genericDetails.arrayOfTooltips[i] = tooltipDetails;
                            }
                            else { Debug.LogError(string.Format("Invalid tooltip Details (Null) for gearID {0}", arrayOfGear[i])); }
                        }
                        else { Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", arrayOfGear[i])); }
                    }
                }
                #endregion
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Node (null) for nodeID {0}", details.nodeID));
            errorFlag = true;
        }
        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = globalResistance;
            outcomeDetails.textTop = "There has been an error in communication and no gear can be sourced.";
            outcomeDetails.textBottom = "Heads will roll!";
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "GearManager.cs -> InitialiseGenericPickerGear");
        }
        else
        {
            //deactivate back button
            GameManager.i.genericPickerScript.SetBackButton(EventType.None);
            if (isIgnoreCache == true)
            {
                //activate Generic Picker window
                EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails, "GearManager.cs -> InitialiseGenericPickerGear");
                if (isPlayer == true)
                {
                    //cache details in case player attempts to access Player gear selection again this action
                    selectionPlayerTurn = GameManager.i.turnScript.Turn;
                    cachedPlayerDetails = genericDetails;
                    isNewActionPlayer = false;
                }
                else
                {
                    //cache details in case player attempts to access Actor gear selection again this action
                    selectionActorTurn = GameManager.i.turnScript.Turn;
                    cachedActorDetails = genericDetails;
                    isNewActionActor = false;
                }
            }
            //player accessing gear during the same action multiple times. Used cached details so he gets the same result.
            else
            {
                if (isPlayer == true)
                {
                    //Player gear selection
                    if (cachedPlayerDetails != null)
                    { EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, cachedPlayerDetails, "GearManager.cs -> InitialiseGenericPickerGear"); }
                    else
                    { Debug.LogWarning("Invalid cachedGenericDetails Player(Null)"); }
                }
                else
                {
                    //Actor gear selection
                    if (cachedActorDetails != null)
                    { EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, cachedActorDetails, "GearManager.cs -> InitialiseGenericPickerGear"); }
                    else
                    { Debug.LogWarning("Invalid cachedGenericDetails Actor (Null)"); }
                }
            }
        }
    }

    /// <summary>
    /// sets up all needed data for Resistance Player Gear Inventory and triggers ModalInventoryUI to display such
    /// </summary>
    private void InitialiseGearInventoryDisplay()
    {
        int numOfGear;
        string colourRarity;
        bool errorFlag = false;

        /*//close node tooltip -> safety check -> redundant as ModalInventory handles this
        GameManager.instance.tooltipNodeScript.CloseTooltip("GearManager.cs -> InitialiseGearInventoryDisplay");*/

        //only for Resistance
        if (GameManager.i.sideScript.PlayerSide == GameManager.i.globalScript.sideResistance)
        {
            numOfGear = GameManager.i.playerScript.CheckNumOfGear();
            //Check for presence of gear
            if (numOfGear > 0)
            {
                //At least one item of gear is present
                InventoryInputData data = new InventoryInputData();
                data.textHeader = "Gear Inventory";
                data.textTop = string.Format("{0}You have {1}{2}{3}{4}{5} out of {6}{7}{8}{9}{10} possible item{11} of Gear{12}", colourNeutral, colourEnd,
                    colourDefault, numOfGear, colourEnd, colourNeutral, colourEnd, colourDefault, maxNumOfGear, colourEnd, colourNeutral,
                    maxNumOfGear != 1 ? "s" : "", colourEnd);
                data.textBottom = string.Format("{0}LEFT CLICK{1}{2} Item for Info, {3}{4}RIGHT CLICK{5}{6} Item for Gear Options{7}", colourAlert, colourEnd,
                    colourDefault, colourEnd, colourAlert, colourEnd, colourDefault, colourEnd);
                data.side = GameManager.i.sideScript.PlayerSide;
                data.handler = RefreshGearInventory;
                data.state = ModalInventorySubState.Gear;
                //Loop Gear list and populate arrays
                List<string> listOfGear = GameManager.i.playerScript.GetListOfGear();
                if (listOfGear != null)
                {
                    for (int i = 0; i < numOfGear; i++)
                    {
                        Gear gear = GameManager.i.dataScript.GetGear(listOfGear[i]);
                        if (gear != null)
                        {
                            GenericTooltipDetails tooltipDetails = GetGearTooltip(gear);
                            if (tooltipDetails != null)
                            {
                                GenericOptionData optionData = new GenericOptionData();
                                optionData.sprite = gear.sprite;
                                optionData.textUpper = gear.tag.ToUpper();
                                //colour code Rarity
                                switch (gear.rarity.name)
                                {
                                    case "Common": colourRarity = colourBad; break;
                                    case "Rare": colourRarity = colourNeutral; break;
                                    case "Unique": colourRarity = colourGood; break;
                                    case "Special": colourRarity = colourGood; break;
                                    default: colourRarity = colourDefault; break;
                                }
                                optionData.textLower = string.Format("{0}{1}{2}{3}{4}{5}{6}", colourRarity, gear.rarity.name, colourEnd, "\n",
                                    colourDefault, gear.type.name, colourEnd);
                                /*optionData.optionID = gear.gearID;*/
                                optionData.optionName = gear.name;
                                //add to array
                                data.arrayOfOptions[i] = optionData;
                                data.arrayOfTooltipsSprite[i] = tooltipDetails;
                            }
                            else
                            { Debug.LogWarning(string.Format("Invalid gear (Null) for gear {0}", listOfGear[i])); }
                        }
                        else
                        { Debug.LogWarning(string.Format("Invalid gear (Null) for gear {0}", listOfGear[i])); }
                    }
                }
                else
                {
                    Debug.LogError("Invalid listOfGear (Null)");
                    errorFlag = true;
                }
                //data package has been populated, proceed if all O.K
                if (errorFlag == true)
                {
                    //error msg
                    ModalOutcomeDetails details = new ModalOutcomeDetails()
                    {
                        side = GameManager.i.sideScript.PlayerSide,
                        textTop = string.Format("{0}Something has gone wrong with your Inventory{1}", colourAlert, colourEnd),
                        textBottom = "Phone calls are being made. Lots of them.",
                        sprite = GameManager.i.guiScript.errorSprite,
                        isAction = false
                    };
                    EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "GearManager.cs -> InitialiseGearInventoryDisplay");
                }
                else
                {
                    //open Inventory UI
                    EventManager.instance.PostNotification(EventType.InventoryOpenUI, this, data, "GearManager.cs -> InitialiseGearInventoryDisplay");
                }
            }
            else
            {
                //No gear in inventory
                ModalOutcomeDetails details = new ModalOutcomeDetails()
                {
                    side = GameManager.i.sideScript.PlayerSide,
                    textTop = string.Format("{0}There is currently no gear in your inventory{1}", colourAlert, colourEnd),
                    textBottom = string.Format("You can have a maximum of {0} items of Gear in your inventtory", maxNumOfGear),
                    sprite = GameManager.i.guiScript.infoSprite,
                    isAction = false
                };
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "GearManager.cs -> InitialiseGearInventoryDisplay");
            }
        }
        else
        {
            //Authority message
            ModalOutcomeDetails details = new ModalOutcomeDetails()
            {
                side = GameManager.i.sideScript.PlayerSide,
                textTop = string.Format("{0}Gear is only available when you are playing the Resistance side{1}", colourAlert, colourEnd),
                sprite = GameManager.i.guiScript.infoSprite,
                isAction = false
            };
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "GearManager.cs -> InitialiseGearInventoryDisplay");
        }
    }

    /// <summary>
    /// Called as a delegate method -> Recalculates option data in InventoryUI after an action has been taken
    /// </summary>
    public InventoryInputData RefreshGearInventory()
    {
        int numOfGear;
        string colourRarity;
        InventoryInputData data = new InventoryInputData();
        //close node tooltip -> safety check
        GameManager.i.tooltipNodeScript.CloseTooltip("GearManager.cs -> RefreshGearInventory");
        //only for Resistance
        if (GameManager.i.sideScript.PlayerSide == GameManager.i.globalScript.sideResistance)
        {
            numOfGear = GameManager.i.playerScript.CheckNumOfGear();
            //At least one item of gear is present
            data.textTop = string.Format("{0}You have {1}{2}{3}{4}{5} out of {6}{7}{8}{9}{10} possible item{11} of Gear{12}", colourNeutral, colourEnd,
                colourDefault, numOfGear, colourEnd, colourNeutral, colourEnd, colourDefault, maxNumOfGear, colourEnd, colourNeutral,
                maxNumOfGear != 1 ? "s" : "", colourEnd);
            if (numOfGear > 0)
            {
                data.textBottom = string.Format("{0}LEFT CLICK{1}{2} Item for Info, {3}{4}RIGHT CLICK{5}{6} Item for Gear Options{7}", colourAlert, colourEnd,
                    colourDefault, colourEnd, colourAlert, colourEnd, colourDefault, colourEnd);
            }
            else { data.textBottom = ""; }
            data.handler = RefreshGearInventory;
            //Loop Gear list and populate arrays
            List<string> listOfGear = GameManager.i.playerScript.GetListOfGear();
            if (listOfGear != null)
            {
                for (int i = 0; i < numOfGear; i++)
                {
                    Gear gear = GameManager.i.dataScript.GetGear(listOfGear[i]);
                    if (gear != null)
                    {
                        GenericTooltipDetails tooltipDetails = GetGearTooltip(gear);
                        if (tooltipDetails != null)
                        {
                            GenericOptionData optionData = new GenericOptionData();
                            optionData.sprite = gear.sprite;
                            optionData.textUpper = gear.tag.ToUpper();
                            //colour code Rarity
                            switch (gear.rarity.name)
                            {
                                case "Common": colourRarity = colourBad; break;
                                case "Rare": colourRarity = colourNeutral; break;
                                case "Unique": colourRarity = colourGood; break;
                                case "Special": colourRarity = colourGood; break;
                                default: colourRarity = colourDefault; break;
                            }
                            optionData.textLower = string.Format("{0}{1}{2}{3}{4}{5}{6}", colourRarity, gear.rarity.name, colourEnd, "\n",
                                colourDefault, gear.type.name, colourEnd);
                            /*optionData.optionID = gear.gearID;*/
                            optionData.optionName = gear.name;
                            //add to array
                            data.arrayOfOptions[i] = optionData;
                            data.arrayOfTooltipsSprite[i] = tooltipDetails;
                        }
                        else
                        { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
                    }
                    else
                    { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
                }
            }
            else
            { Debug.LogError("Invalid listOfGear (Null)"); }
        }
        return data;
    }

    /// <summary>
    /// Process end of turn Compromised Gear choices (Player gear only)
    /// </summary>
    /// <param name="data"></param>
    public void ProcessCompromisedGear(GenericReturnData data)
    {
        if (data != null)
        {
            //deduct renown
            if (string.IsNullOrEmpty(data.optionName) == false)
            {
                int renown = GameManager.i.playerScript.Renown;
                renown -= gearSaveCurrentCost;
                if (renown < 0)
                {
                    Debug.LogWarning("Renown invalid (< 0)");
                    renown = 0;
                }
                GameManager.i.playerScript.Renown = renown;
                //retain saved gear, remove any unsaved gear
                GameManager.i.playerScript.UpdateGear(gearSaveCurrentCost, data.optionName);
                //stats
                Gear gear = GameManager.i.dataScript.GetGear(data.optionName);
                if (gear != null)
                {
                    gear.statTimesSaved++;
                    gear.statRenownSpent += gearSaveCurrentCost;
                }
                else { Debug.LogWarningFormat("Invalid gear (Null) for gear {0}", data.optionName); }
                //outcome -> one item saved, any other compromised gear lost
                ModalOutcomeDetails details = new ModalOutcomeDetails();
                StringBuilder builderTop = new StringBuilder();
                builderTop.AppendFormat("{0}Gear used this turn has been{1}{2}{3}COMPROMISED{4}{5}", colourNormal, colourEnd, "\n", colourBad, colourEnd, "\n");
                builderTop.AppendFormat("{0}You used {1}{2}{3}{4}{5} Renown to Save gear{6}", colourNormal, colourEnd, colourNeutral, gearSaveCurrentCost, colourEnd,
                    colourNormal, colourEnd);
                details.textTop = builderTop.ToString();
                StringBuilder builderBottom = new StringBuilder();
                foreach (string gearName in listOfCompromisedGear)
                {
                    //listOfCompromisedGear is gear.tag.ToUpper(), not gear.name
                    if (gearName.Equals(gear.tag, System.StringComparison.OrdinalIgnoreCase) == false)
                    {
                        //gear lost
                        if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                        builderBottom.AppendFormat("{0}{1}{2}{3} has been LOST{4}", colourNeutral, gearName, colourEnd, colourBad, colourEnd);
                    }
                    else
                    {
                        //gear saved
                        if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                        builderBottom.AppendFormat("{0}{1}{2}{3} has been SAVED{4}", colourNeutral, gearName, colourEnd, colourGood, colourEnd);
                    }
                }
                details.textBottom = builderBottom.ToString();
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "GearManager.cs -> ProcessCompromisedGear");
            }
        }
        else
        {
            //End of turn compromised Gear dialogue has been Cancelled
            GameManager.i.playerScript.UpdateGear();
            //outcome dialogue
            ModalOutcomeDetails details = new ModalOutcomeDetails();
            StringBuilder builderTop = new StringBuilder();
            builderTop.AppendFormat("{0}Gear used this turn has been {1}{2}COMPROMISED{3}{4}", colourNormal, colourEnd, colourBad, colourEnd, "\n");
            builderTop.AppendFormat("{0}You have {1}{2}{3}chosen NOT to save gear{4}{5}", colourNormal, colourEnd, "\n", colourAlert, colourEnd, "\n");
            details.textTop = builderTop.ToString();
            StringBuilder builderBottom = new StringBuilder();
            foreach (string gearName in listOfCompromisedGear)
            {
                if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                builderBottom.AppendFormat("{0}{1}{2}{3} has been LOST{4}", colourNeutral, gearName, colourEnd, colourBad, colourEnd);
            }
            details.textBottom = builderBottom.ToString();
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "GearManager.cs -> ProcessCompromisedGear");
        }
    }

    /// <summary>
    /// Processes choice of Gear
    /// </summary>
    /// <param name="returnDetails"></param>
    public void ProcessGearChoice(GenericReturnData data)
    {
        bool successFlag = true;
        bool isInvisibility = false;
        bool isPlayer = false;
        int turn = GameManager.i.turnScript.Turn;
        if (string.IsNullOrEmpty(data.optionName) == false)
        {
            //get currently selected node
            if (data.nodeID != -1)
            {
                Gear gear = GameManager.i.dataScript.GetGear(data.optionName);
                if (gear != null)
                {
                    //check whether it is invisible type gear (if so then invisibility -1 effect is ignored)
                    if (gear.type.name.Equals("Invisibility", System.StringComparison.Ordinal) == true)
                    { isInvisibility = true; }
                    Sprite sprite = gear.sprite;
                    Node node = GameManager.i.dataScript.GetNode(data.nodeID);
                    if (node != null)
                    {
                        if (GameManager.i.nodeScript.GetPlayerNodeID() == node.nodeID)
                        { isPlayer = true; }
                        Actor actor = GameManager.i.dataScript.GetCurrentActor(data.actorSlotID, GameManager.i.globalScript.sideResistance);
                        if (actor != null)
                        {
                            StringBuilder builderTop = new StringBuilder();
                            StringBuilder builderBottom = new StringBuilder();

                            if (GameManager.i.playerScript.AddGear(gear.name) == true)
                            {
                                //remove gear from pool
                                if (GameManager.i.dataScript.RemoveGearFromPool(gear) == false)
                                { Debug.LogWarningFormat("Invalid removal for \"{0}\"", gear.tag); }
                                //stat
                                gear.statTurnObtained = turn;
                                //gear successfully acquired
                                builderTop.Append(string.Format("{0}We have the goods!{1}", colourNormal, colourEnd));
                                builderBottom.Append(string.Format("{0}{1}{2}{3} is in our possession{4}", colourGear, gear.tag.ToUpper(), colourEnd,
                                    colourDefault, colourEnd));
                                //reset flags for gear caches
                                if (isPlayer == true)
                                { isNewActionPlayer = true; }
                                else { isNewActionActor = true; }
                                //message
                                string textMsg;
                                if (isPlayer == true)
                                {
                                    textMsg = string.Format("{0} ({1}) has been acquired (by PLAYER)", gear.tag, gear.type.name);
                                    GameManager.i.messageScript.GearObtained(textMsg, node, gear);
                                }
                                else
                                {
                                    textMsg = string.Format("{0} ({1}) has been acquired ( by {2})", gear.tag, gear.type.name, actor.arc.name);
                                    GameManager.i.messageScript.GearObtained(textMsg, node, gear, actor.actorID);
                                }
                                //Process any other effects, if acquisition was successfull, ignore otherwise
                                Action action = actor.arc.nodeAction;
                                EffectDataInput dataInput = new EffectDataInput();
                                dataInput.originText = "Gear";
                                List<Effect> listOfEffects = action.GetEffects();
                                if (listOfEffects.Count > 0)
                                {
                                    foreach (Effect effect in listOfEffects)
                                    {
                                        if (effect.ignoreEffect == false)
                                        {
                                            //ignore invisiblity effect in case of fixer/player getting invisibility gear 
                                            if (isInvisibility == true && effect.outcome.name.Equals("Invisibility", System.StringComparison.Ordinal) == true)
                                            {
                                                Debug.LogFormat("GearManager.cs -> ProcessGearChoice: {0} effect ignored due to Invisibility{1}",
                                                  effect.name, "\n");
                                            }
                                            else
                                            {
                                                //process effect normally
                                                EffectDataReturn effectReturn = GameManager.i.effectScript.ProcessEffect(effect, node, dataInput, actor);
                                                if (effectReturn != null)
                                                {
                                                    builderTop.AppendLine();
                                                    builderTop.Append(effectReturn.topText);
                                                    if (builderBottom.Length > 0)
                                                    { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                                    builderBottom.Append(effectReturn.bottomText);
                                                }
                                                else { Debug.LogError("Invalid effectReturn (Null)"); }
                                            }
                                        }
                                    }
                                }
                                //NodeActionData package
                                if (isPlayer == false)
                                {
                                    //actor action
                                    NodeActionData nodeActionData = new NodeActionData()
                                    {
                                        turn = turn,
                                        actorID = actor.actorID,
                                        nodeID = node.nodeID,
                                        dataName = gear.tag,
                                        nodeAction = NodeAction.ActorObtainGear
                                    };
                                    //add to actor's personal list
                                    actor.AddNodeAction(nodeActionData);
                                    Debug.LogFormat("[Tst] GearManager.cs -> ProcessGearChoice: nodeActionData added to {0}, {1}{2}", actor.actorName, actor.arc.name, "\n");
                                }
                                else
                                {
                                    //player action
                                    NodeActionData nodeActionData = new NodeActionData()
                                    {
                                        turn = turn,
                                        actorID = 999,
                                        nodeID = node.nodeID,
                                        dataName = gear.tag,
                                        nodeAction = NodeAction.PlayerObtainGear
                                    };
                                    //add to player's personal list
                                    GameManager.i.playerScript.AddNodeAction(nodeActionData);
                                    Debug.LogFormat("[Tst] GearManager.cs -> ProcessGearChoice: nodeActionData added to {0}, {1}{2}", GameManager.i.playerScript.PlayerName, "Player", "\n");
                                    //statistics
                                    GameManager.i.dataScript.StatisticIncrement(StatType.PlayerNodeActions);
                                }
                                //statistics
                                GameManager.i.dataScript.StatisticIncrement(StatType.NodeActionsResistance);
                            }
                            else
                            {
                                //Problem occurred, gear not acquired
                                builderTop.Append("Problem occured, gear NOT obtained");
                                builderBottom.Append("Who did this? B*stard!");
                                successFlag = false;
                            }
                            //OUTCOME Window
                            ModalOutcomeDetails details = new ModalOutcomeDetails();
                            details.textTop = builderTop.ToString();
                            details.textBottom = builderBottom.ToString();
                            details.sprite = sprite;
                            details.side = GameManager.i.globalScript.sideResistance;
                            if (successFlag == true)
                            {
                                details.isAction = true;
                                details.reason = "Select Gear";
                            }
                            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "GearManager.cs -> ProcessGearChoice");
                        }
                        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", data.actorSlotID)); }
                    }
                    else { Debug.LogError(string.Format("Invalid node (Null) for NodeID {0}", data.nodeID)); }
                }
                else { Debug.LogError(string.Format("Invalid Gear (Null) for gear {0}", data.optionName)); }
            }
            else { Debug.LogError("Highlighted node invalid (default '-1' value)"); }
        }
        else { Debug.LogError("Invalid gear (Null or Empty)"); }
    }

    /// <summary>
    /// returns chance of gear being compromised. Depends on gear rarity (Same for all). Returns '0' is a problem.
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public int GetChanceOfCompromise(string gearName)
    {
        int chance = 0;
        if (string.IsNullOrEmpty(gearName) == false)
        {
            Gear gear = GameManager.i.dataScript.GetGear(gearName);
            if (gear != null)
            {
                //chance of compromise same for all
                switch (gear.rarity.name)
                {
                    case "Special":
                    case "Unique":
                    case "Rare":
                    case "Common":
                        chance = chanceOfCompromise;
                        break;
                }
            }
            else
            {
                //problem. Default compromised.
                Debug.LogWarning(string.Format("Invalid gear (Null) for gear {0}", gearName));
            }
        }
        return chance;
    }

    /// <summary>
    /// call this whenever gear is used
    /// </summary>
    /// <param name="gear"></param>
    /// <param name="descriptor">In format 'gear name' used to .... ('start a Riot')</param>
    public void SetGearUsed(Gear gear, string descriptorUsedTo)
    {
        if (gear != null)
        {
            if (string.IsNullOrEmpty(descriptorUsedTo) == false)
            {
                gear.timesUsed++;
                gear.statTimesUsed++;
                gear.reasonUsed = string.Format("Used to {0}", descriptorUsedTo);
                //message
                string msgText = string.Format("{0} used to {1}", gear.tag, descriptorUsedTo);
                GameManager.i.messageScript.GearUsed(msgText, gear);
            }
            else { Debug.LogWarning("Invalid descriptorUsedTo parameter (Null)"); }
        }
        else { Debug.LogWarning("Invalid gear (Null)"); }
    }

    /*/// <summary>
    /// subMethod to handle admin for Player renown expenditure
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public string RenownUsed(Gear gear, Node node, int amount)
    {
        //update player renown
        int renown = GameManager.instance.playerScript.Renown;
        renown -= amount;
        GameManager.instance.playerScript.Renown = renown;
        //message
        string textMsg = string.Format("{0}, ID {1} has been compromised. Saved by using {2} Renown.", gear.tag, gear.gearID, amount);
        GameManager.instance.messageScript.RenownUsedPlayer(textMsg, string.Format("save {0} gear", gear.tag), amount, gear.gearID, node.nodeID);
        //return text string for builder
        return string.Format("{0}{1}{2}Gear saved, Renown -{3}{4}", "\n", "\n", colourBad, amount, colourEnd);
    }*/

    /// <summary>
    /// returns a data package of 3 formatted strings ready to slot into a gear tooltip. Null if a problem.
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public GenericTooltipDetails GetGearTooltip(Gear gear)
    {
        GenericTooltipDetails details = null;
        if (gear != null)
        {
            details = new GenericTooltipDetails();
            StringBuilder builderHeader = new StringBuilder();
            StringBuilder builderDetails = new StringBuilder();
            builderHeader.AppendFormat("{0}<size=110%>{1}</size>{2}", colourGear, gear.tag.ToUpper(), colourEnd);
            string colourGearEffect = colourNeutral;
            if (gear.data == 3) { colourGearEffect = colourGood; }
            else if (gear.data == 1) { colourGearEffect = colourBad; }
            //add a second line to the gear header tooltip to reflect the specific value of the gear, appropriate to it's type
            switch (gear.type.name)
            {
                case "Movement":
                    builderHeader.AppendFormat("{0}{1}defeats {2}{3}{4}{5}{6} security{7}", "\n", colourNormal, colourEnd, colourGearEffect, (ConnectionType)gear.data, colourEnd, colourNormal, colourEnd);
                    break;
            }
            //Node use
            builderHeader.AppendLine();
            switch (gear.type.name)
            {
                case "Hacking":
                case "Kinetic":
                case "Persuasion":
                    builderHeader.AppendFormat("{0}<size=90%>District use? Yes{1}", colourAlert, colourEnd);
                    break;
                default:
                    builderHeader.AppendFormat("{0}<size=90%>District use? No{1}", colourGrey, colourEnd);
                    break;
            }
            //gear use
            builderHeader.AppendLine();
            builderHeader.AppendFormat("{0}Gift use? Yes{1}", colourAlert, colourEnd);
            //personal use
            builderHeader.AppendLine();
            if (gear.listOfPersonalEffects != null && gear.listOfPersonalEffects.Count > 0)
            { builderHeader.AppendFormat("{0}Personal use? Yes{1}", colourAlert, colourEnd); }
            else
            { builderHeader.AppendFormat("{0}Personal use? No{1}", colourGrey, colourEnd); }
            //AI use
            builderHeader.AppendLine();
            if (gear.aiHackingEffect != null)
            { builderHeader.AppendFormat("{0}AI use? Yes{1}", colourAlert, colourEnd); }
            else
            { builderHeader.AppendFormat("{0}AI use? No{1}", colourGrey, colourEnd); }
            //move use
            builderHeader.AppendLine();
            if (gear.type.name.Equals("Movement", StringComparison.Ordinal) == true)
            { builderHeader.AppendFormat("{0}Move use? Yes{1}", colourAlert, colourEnd); }
            else
            { builderHeader.AppendFormat("{0}Move use? No{1}", colourGrey, colourEnd); }
            //has been used this turn
            if (gear.timesUsed > 0)
            {
                builderHeader.AppendLine();
                builderHeader.AppendFormat("</size>{0}Already USED this Turn{1}", colourBad, colourEnd);
            }
            //details
            string colourRarity = colourDefault;
            switch (gear.rarity.name)
            {
                case "Common": colourRarity = colourBad; break;
                case "Rare": colourRarity = colourNeutral; break;
                case "Unique": colourRarity = colourGood; break;
                case "Special": colourRarity = colourGood; break;
            }
            builderDetails.AppendFormat("{0}Rarity {1}{2}{3}{4}", colourNormal, colourEnd, colourRarity, gear.rarity.name, colourEnd);
            builderDetails.AppendLine();
            builderDetails.AppendFormat("{0}{1} gear{2}", colourSide, gear.type.name, colourEnd);

            //data package
            details.textHeader = builderHeader.ToString();
            details.textMain = string.Format("{0}{1}{2}", colourNormal, gear.description, colourEnd);
            details.textDetails = builderDetails.ToString();
        }
        return details;
    }

    /// <summary>
    /// returns a data package of 3 formatted strings ready to slot into a gear tooltip. Null if a problem. For gear that has been used and Compromised.
    /// </summary>
    /// <param name="gear"></param>
    /// <returns></returns>
    public GenericTooltipDetails GetCompromisedGearTooltip(Gear gear)
    {
        GenericTooltipDetails details = null;
        if (gear != null)
        {
            details = new GenericTooltipDetails();
            StringBuilder builderHeader = new StringBuilder();
            StringBuilder builderMain = new StringBuilder();
            StringBuilder builderDetails = new StringBuilder();
            builderHeader.AppendFormat("{0}<size=110%>{1}</size>{2}", colourGear, gear.tag.ToUpper(), colourEnd);
            string colourGearEffect = colourNeutral;
            if (gear.data == 3) { colourGearEffect = colourGood; }
            else if (gear.data == 1) { colourGearEffect = colourBad; }
            //add a second line to the gear header tooltip to reflect the specific value of the gear, appropriate to it's type
            switch (gear.type.name)
            {
                case "Movement":
                    builderHeader.AppendFormat("{0}{1}{2}{3}", "\n", colourGearEffect, (ConnectionType)gear.data, colourEnd);
                    break;
            }
            builderMain.AppendFormat("{0}<size=115%><cspace=0.5em>COMPROMISED</cspace></size>{1}{2}", colourBad, colourEnd, "\n");
            builderMain.AppendFormat("{0}<size=110%> {1} %</size>{2}   <size=90%>({3}{4}{5} time{6})</size>{7}", "<mark=#FFFFFF4D>", gear.chanceOfCompromise, "</mark>",
                colourNeutral, gear.timesUsed, colourEnd, gear.timesUsed != 1 ? "s" : "", "\n");
            builderMain.AppendFormat("{0}{1}{2}{3}", colourNormal, gear.reasonUsed, colourEnd, "\n");
            builderMain.AppendFormat("{0}LEFT CLICK to SAVE{1}{2}Cost {3}{4}{5} Renown", colourAlert, colourEnd, "\n", colourNeutral, gearSaveCurrentCost, colourEnd);
            //details
            builderDetails.AppendFormat("{0}{1}{2}", colourGood, gear.rarity.name, colourEnd);
            builderDetails.AppendLine();
            builderDetails.AppendFormat("{0}{1} gear{2}", colourSide, gear.type.name, colourEnd);

            //data package
            details.textHeader = builderHeader.ToString();
            details.textMain = builderMain.ToString();
            details.textDetails = builderDetails.ToString();
        }
        return details;
    }


    /// <summary>
    /// returns a data package of 3 formatted strings. Null if a problem. For gear that has been used but Not Compromised
    /// </summary>
    /// <param name="gear"></param>
    /// <returns></returns>
    public GenericTooltipDetails GetUsedGearTooltip(Gear gear)
    {
        GenericTooltipDetails details = null;
        if (gear != null)
        {
            details = new GenericTooltipDetails();
            StringBuilder builderHeader = new StringBuilder();
            StringBuilder builderMain = new StringBuilder();
            StringBuilder builderDetails = new StringBuilder();
            builderHeader.AppendFormat("{0}<size=110%>{1}</size>{2}", colourGear, gear.tag.ToUpper(), colourEnd);
            string colourGearEffect = colourNeutral;
            if (gear.data == 3) { colourGearEffect = colourGood; }
            else if (gear.data == 1) { colourGearEffect = colourBad; }
            //add a second line to the gear header tooltip to reflect the specific value of the gear, appropriate to it's type
            switch (gear.type.name)
            {
                case "Movement":
                    builderHeader.AppendFormat("{0}{1}{2}{3}", "\n", colourGearEffect, (ConnectionType)gear.data, colourEnd);
                    break;
            }
            builderMain.AppendFormat("{0}NOT COMPROMISED{1}{2}", colourGood, colourEnd, "\n");
            builderMain.AppendFormat("{0}<size=110%> {1} %</size>{2}{3}", "<mark=#FFFFFF4D>", gear.chanceOfCompromise, "</mark>", "\n");
            builderMain.AppendFormat("{0}{1}{2}{3}", colourNormal, gear.reasonUsed, colourEnd, "\n");
            builderMain.AppendFormat("{0}Gear is O.K{1}{2}{3}No need to Save{4}", colourAlert, colourEnd, "\n", colourNeutral, colourEnd);
            //details
            builderDetails.AppendFormat("{0}{1}{2}", colourGood, gear.rarity.name, colourEnd);
            builderDetails.AppendLine();
            builderDetails.AppendFormat("{0}{1} gear{2}", colourSide, gear.type.name, colourEnd);
            //data package
            details.textHeader = builderHeader.ToString();
            details.textMain = builderMain.ToString();
            details.textDetails = builderDetails.ToString();
        }
        return details;
    }


    public int GetGearSaveCurrentCost()
    { return gearSaveCurrentCost; }

    public void SetGearSaveCurrentCost(int cost)
    { gearSaveCurrentCost = cost; }

    public List<string> GetListOfCompromisedGear()
    { return listOfCompromisedGear; }

    /// <summary>
    /// clear and copy new data to listOfCompromisedGear (for Load saved game)
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfCompromisedGear(List<string> tempList)
    {
        if (tempList != null)
        {
            listOfCompromisedGear.Clear();
            listOfCompromisedGear.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    //new methods above here
}
