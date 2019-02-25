using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using modalAPI;
using gameAPI;
using packageAPI;
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
    [Tooltip("Base cost to save compromised gear (in Renown) at level start. Cost increases +1 each time the option is used")]
    [Range(0, 3)] public int gearSaveBaseCost = 0;

    [Header("Actor Gear")]
    [Tooltip("Number of turns after actor is given gear before it can be handed back and before it begins to be tested for being lost")]
    [Range(0, 10)] public int actorGearGracePeriod = 3;
    [Tooltip("% Chance, per turn, of actor losing gear in his possession. Only checked after actorGearGracePeriod number of turns")]
    [Range(0, 100)] public int actorGearLostChance = 20;

    //used for quick reference  -> rarity
    [HideInInspector] public GearRarity gearCommon;
    [HideInInspector] public GearRarity gearRare;
    [HideInInspector] public GearRarity gearUnique;
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
    //compromised gear
    private int gearSaveCurrentCost;                                          //how much renown to save a compromised item of gear (increments +1 each time option used)
    private List<string> listOfCompromisedGear;                               //list cleared each turn that contains names of compromised gear (used for outcome dialogues)
    private int maxGenericOptions = -1;
    //fast access -> traits
    private int actorLoseGearHigh = -1;
    private int actorLoseGearNone = -1;
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
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        listOfCompromisedGear = new List<string>();
        //initialise fast access variables -> rarity
        List<GearRarity> listOfGearRarity = GameManager.instance.dataScript.GetListOfGearRarity();
        if (listOfGearRarity != null)
        {
            foreach(GearRarity rarity in listOfGearRarity)
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
                }
            }
            //error check
            if (gearCommon == null) { Debug.LogError("Invalid gearCommon (Null)"); }
            if (gearRare == null) { Debug.LogError("Invalid gearRare (Null)"); }
            if (gearUnique == null) { Debug.LogError("Invalid gearUnique (Null)"); }
        }
        else { Debug.LogError("Invalid listOfGearRarity (Null)"); }
        //gear save cost
        gearSaveCurrentCost = gearSaveBaseCost;
        //cached
        selectionPlayerTurn = -1;
        selectionActorTurn = -1;
        cachedPlayerDetails = null;
        cachedActorDetails = null;
        isNewActionPlayer = true;
        isNewActionActor = true;
        //initialise fast access variables -> type
        List<GearType> listOfGearType = GameManager.instance.dataScript.GetListOfGearType();
        Debug.Assert(listOfGearType != null, "Invalid listOfGearType (Null)");
        //fast access
        globalResistance = GameManager.instance.globalScript.sideResistance;
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        actorLoseGearHigh = GameManager.instance.dataScript.GetTraitEffectID("ActorLoseGearHigh");
        actorLoseGearNone = GameManager.instance.dataScript.GetTraitEffectID("ActorLoseGearNone");
        maxGenericOptions = GameManager.instance.genericPickerScript.maxOptions;
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(actorLoseGearHigh > -1, "Invalid actorLoseGearHigh (-1)");
        Debug.Assert(actorLoseGearNone > -1, "Invalid actorLoseGearNone (-1)");
        Debug.Assert(maxGenericOptions != -1, "Invalid maxGenericOptions (-1)");
        if (listOfGearType != null)
        {
            foreach (GearType gearType in listOfGearType)
            {
                //pick out and assign the ones required for fast acess, ignore the rest. 
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

        //initialise gear lists
        Dictionary<int, Gear> dictOfGear = GameManager.instance.dataScript.GetDictOfGear();
        if (dictOfGear != null)
        {
            int gameLevel = GameManager.instance.metaScript.metaLevel.level;
            //set up an array of Lists, with index corresponding to GearLevel enum, eg. Common / Rare / Unique
            List<int>[] arrayOfGearLists = new List<int>[(int)GameManager.instance.dataScript.GetNumOfGearRarity()];
            for (int i = 0; i < arrayOfGearLists.Length; i++)
            { arrayOfGearLists[i] = new List<int>(); }
            //loop dict and allocate gear to various lists
            foreach (var gearEntry in dictOfGear)
            {
                //check appropriate for metaLevel (same level or 'none', both are acceptable)
                if (gearEntry.Value.metaLevel == null || gearEntry.Value.metaLevel.level == gameLevel)
                {
                    //reset stats (SO carries values over between sessions)
                    gearEntry.Value.ResetStats();
                    //assign to a list based on rarity
                    int index = gearEntry.Value.rarity.level;
                    arrayOfGearLists[index].Add(gearEntry.Key);
                }
            }
            //initialise dataManager lists with local lists
            for (int i = 0; i < arrayOfGearLists.Length; i++)
            {
                GearRarity indexRarity = GameManager.instance.dataScript.GetGearRarity(i);
                if (indexRarity != null)
                { GameManager.instance.dataScript.SetGearList(arrayOfGearLists[i], indexRarity); }
                else { Debug.LogError("Invalid indexRarity (Null)"); }
            }
        }
        else { Debug.LogError("Invalid dictOfGear (Null) -> Gear not initialised"); }
        //event Listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "GearManager");
        EventManager.instance.AddListener(EventType.GearAction, OnEvent, "GearManager");
        EventManager.instance.AddListener(EventType.GenericGearChoice, OnEvent, "GearManager");
        EventManager.instance.AddListener(EventType.GenericCompromisedGear, OnEvent, "GearManager");
        EventManager.instance.AddListener(EventType.InventorySetGear, OnEvent, "GearManager");
        EventManager.instance.AddListener(EventType.EndTurnEarly, OnEvent, "GearManager");
        
    }


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
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourSide = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourGear = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
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
        List<int> listOfGear = GameManager.instance.dataScript.GetListOfCurrentGear();
        List<int> listOfPlayerGear = new List<int>();
        if (listOfGear != null)
        {
            if (listOfGear.Count > 0)
            {
                for (int i = 0; i < listOfGear.Count; i++)
                {
                    Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                    if (gear != null)
                    {
                        //gear has been used?
                        if (gear.timesUsed > 0)
                        {
                            //check to see if compromised
                            chance = GetChanceOfCompromise(gear.gearID);
                            for (int j = 0; j < gear.timesUsed; j++)
                            {
                                rnd = Random.Range(0, 100);
                                if ( rnd < chance)
                                {
                                    //gear COMPROMISED
                                    gear.isCompromised = true;
                                    gear.chanceOfCompromise = chance;
                                    //add to list (used for outcome dialogues)
                                    listOfCompromisedGear.Add(gear.name.ToUpper());
                                    if (GameManager.instance.playerScript.CheckGearPresent(gear.gearID) == true)
                                    {
                                        listOfPlayerGear.Add(gear.gearID);
                                        isCompromisedGear = true;
                                        GameManager.instance.playerScript.isEndOfTurnGearCheck = true;
                                    }
                                    else { GameManager.instance.actorScript.isGearCheckRequired = true; }
                                    //stat (before message)
                                    gear.statTimesCompromised++;
                                    //admin
                                    Debug.LogFormat("[Rnd] GearManager.cs -> CheckForCompromisedGear: {0} COMPROMISED, need < {1}, rolled {2}{3}", gear.name, chance, rnd, "\n");
                                    Debug.LogFormat("[Gea] -> CheckForCompromisedGear: {0}, {1}, ID {2}, Compromised ({3}){4}", gear.name, gear.type.name, gear.gearID, gear.reasonUsed, "\n");
                                    string msgText = string.Format("Gear {0} COMPROMISED", gear.name);
                                    GameManager.instance.messageScript.GeneralRandom(msgText, "Compromised Gear", chance, rnd, true);
                                    break;
                                }
                                else
                                {
                                    //gear not compromised
                                    gear.chanceOfCompromise = chance;
                                    Debug.LogFormat("[Rnd] GearManager.cs -> CheckForCompromisedGear: {0} NOT compromised, need < {1}, rolled {2}{3}", gear.name, chance, rnd, "\n");
                                    string msgText = string.Format("Gear {0} NOT Compromised", gear.name);
                                    GameManager.instance.messageScript.GeneralRandom(msgText, "Compromised Gear", chance, rnd, true);
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
                    int playerRenown = GameManager.instance.playerScript.Renown;
                    if (playerRenown >= gearSaveCurrentCost)
                    {
                        //initialise generic picker
                        InitialiseCompromisedGearPicker();
                    }
                    else
                    {
                        //not enough renown -> remove gear and reset stats
                        ProcessCompromisedGear(null);
                        //outcome dialogue
                        ModalOutcomeDetails details = new ModalOutcomeDetails();
                        StringBuilder builderTop = new StringBuilder();
                        builderTop.AppendFormat("{0}Gear used this turn has been {1}{2}COMPROMISED{3}{4}", colourNormal, colourEnd, colourBad, colourEnd, "\n");
                        builderTop.AppendFormat("{0}You have {1}{2}{3}Insufficient Renown{4}{5}", colourNormal, colourEnd, "\n", colourBad, colourEnd, "\n");
                        builderTop.AppendFormat("<size=85%>({0}{1}{2} needed)</size>{3}", colourNeutral, gearSaveCurrentCost, colourEnd, "\n");
                        builderTop.AppendFormat("{0}to {1}{2}Save{3}{4} any gear{5}", colourNormal, colourEnd, colourGood, colourEnd, colourNormal, colourEnd);
                        details.textTop = builderTop.ToString();
                        StringBuilder builderBottom = new StringBuilder();
                        foreach(string gearName in listOfCompromisedGear)
                        { 
                            if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                            builderBottom.AppendFormat("{0}{1}{2}{3} has been LOST{4}", colourNeutral, gearName, colourEnd, colourBad, colourEnd);
                        }
                        details.textBottom = builderBottom.ToString();
                        //will overlay InfoAPP so needs to handle this
                        details.modalLevel = 2;
                        details.modalState = ModalState.InfoDisplay;
                        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "GearManager.cs -> CheckForCompromisedGear");
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
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
        foreach (Actor actor in arrayOfActors)
        {
            //if invalid actor or vacant position then ignore
            if (actor != null)
            {
                //only check Active actors
                if (actor.Status == ActorStatus.Active)
                {
                    if (actor.GetGearID() > -1)
                    {
                        Gear gear = GameManager.instance.dataScript.GetGear(actor.GetGearID());
                        if (gear != null)
                        {
                            actor.IncrementGearTimer();
                            int timer = actor.GetGearTimer();
                            timer -= 1;
                            //grace period about to be exceeded
                            if (timer == actorGearGracePeriod)
                                {
                                    //let player know that gear will be available
                                    string msgText = string.Format("{0} gear held by {1}, available next turn", gear.name, actor.arc.name);
                                    GameManager.instance.messageScript.GearAvailable(msgText, gear, actor);
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
                                    GameManager.instance.actorScript.TraitLogMessage(actor, "for a Lose Gear check", "to TRIPLE chance of losing Gear");
                                }
                                else if (actor.CheckTraitEffect(actorLoseGearNone) == true)
                                {
                                    chance = 0;
                                    GameManager.instance.actorScript.TraitLogMessage(actor, "for a Lose Gear check", "to AVOID losing Gear");
                                    traitName = string.Format(" ({0}) ", actor.GetTrait().tag);
                                }
                                else { traitName = ""; }
                                //roll die
                                if (rnd < chance)
                                {
                                    //Gear LOST
                                    Debug.LogFormat("[Rnd] GearManager.cs -> CheckActorGear: {0} LOST ({1}), need < {2}{3}, rolled {4}{5}",
                                        gear.name, actor.arc.name,  chance, traitName, rnd, "\n");
                                    //message
                                    string msgText = string.Format("{0} gear lost by {1}, {2}{3}", gear.name, actor.actorName, actor.arc.name, traitName);
                                    GameManager.instance.messageScript.GearLost(msgText, gear, actor);
                                    //random
                                    msgText = string.Format("Gear {0} LOST", gear.name);
                                    GameManager.instance.messageScript.GeneralRandom(msgText, "Gear Lost", chance, rnd, true);
                                    //remove gear AFTER message
                                    actor.RemoveGear(GearRemoved.Lost);

                                }
                                else
                                {
                                    //random
                                    string msgText = string.Format("{0} gear, {1}, retained", actor.arc.name, gear.name);
                                    GameManager.instance.messageScript.GeneralRandom(msgText, "Gear Lost", chance, rnd, true);
                                }
                            }
                        }
                        else { Debug.LogWarningFormat("Invalid gear (Null) for gearID {0}{1}", actor.GetGearID(), "\n"); }
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
        List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
        if (listOfGear != null && listOfGear.Count > 0)
        {
            //loop gear (max 'maxGenericOptions' pieces of gear / max 'maxGenericOptions' options)
            for (int index = 0; index < listOfGear.Count; index++)
            {
                if (index < maxGenericOptions)
                {
                    Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[index]);
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
                                optionDetails.optionID = gear.gearID;
                                optionDetails.text = gear.name.ToUpper();
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
                                optionDetails.optionID = gear.gearID;
                                optionDetails.text = gear.name.ToUpper();
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
        int gearID, index;
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Node node = GameManager.instance.dataScript.GetNode(details.nodeID);
        if (node != null)
        {

            #region CaptureCheck
            //check for player/actor being captured
            int actorID = GameManager.instance.playerScript.actorID;
            if (node.nodeID != GameManager.instance.nodeScript.nodePlayer)
            {
                Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, globalResistance);
                if (actor != null)
                { actorID = actor.actorID; }
                else { Debug.LogError(string.Format("Invalid actor (Null) fro details.ActorSlotID {0}", details.actorDataID)); errorFlag = true; }
            }
            else { isPlayer = true; }
            //check capture provided no errors
            if (errorFlag == false)
            {
                CaptureDetails captureDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, actorID);
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
                if (selectionPlayerTurn != GameManager.instance.turnScript.Turn || isNewActionPlayer == true)
                { isIgnoreCache = true; }
            }
            else
            {
                //actor gear selection (can be different from Players, eg. better chance of rare gear)
                if (selectionActorTurn != GameManager.instance.turnScript.Turn || isNewActionActor == true)
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
                List<int> tempCommonGear = new List<int>(GameManager.instance.dataScript.GetListOfGear(gearCommon));
                List<int> tempRareGear = new List<int>(GameManager.instance.dataScript.GetListOfGear(gearRare));
                //
                //select two items of gear for the picker
                //
                int[] arrayOfGear = new int[2];
                int countOfGear = 0;
                for (int i = 0; i < arrayOfGear.Length; i++)
                {
                    gearID = -1;
                    //any rare gear available?
                    if (tempRareGear.Count > 0)
                    {
                        //chance of rare gear -> base chance * actor ability (or 1 if player)
                        int chance = chanceOfRareGear;
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, globalResistance);
                        if (actor != null)
                        {
                            //if Player doing it then assumed to have an ability of 1, actor (Fixer) may have a higher ability.
                            if (isPlayer == false)
                            { chance *= actor.datapoint0; }
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
                            gearID = tempRareGear[index];
                            tempRareGear.RemoveAt(index);
                            Gear rareGear = GameManager.instance.dataScript.GetGear(gearID);
                            if (rareGear != null)
                            {
                                Debug.LogFormat("[Rnd] GearManager.cs -> InitialiseGenericPickerGear: Rare gear ({0}) Success, need < {1} roll {2}{3}", rareGear.name, chance, rnd, "\n");
                                GameManager.instance.messageScript.GeneralRandom(string.Format("Gear Choice, Rare gear ({0}) SUCCESS", rareGear.name), "Rare Gear", chance, rnd);
                            }
                            else
                            {
                                Debug.LogFormat("[Rnd] GearManager.cs -> InitialiseGenericPickerGear: Rare gear Success, need < {0} roll {1}{2}", chance, rnd, "\n");
                                GameManager.instance.messageScript.GeneralRandom("Gear Choice, Rare gear SUCCESS", "Rare Gear", chance, rnd);
                               Debug.LogWarningFormat("Invalid rare gear (Null) for gearID {0}", gearID);
                            }
                            
                        }
                        else if (tempCommonGear.Count > 0)
                        {
                            //if failed chance for rare gear then need to get common
                            index = Random.Range(0, tempCommonGear.Count);
                            gearID = tempCommonGear[index];
                            tempCommonGear.RemoveAt(index);
                            Debug.LogFormat("[Rnd] GearManager.cs -> InitialiseGenericPickerGear: Rare gear FAIL, need < {0} roll {1}{2}", chance, rnd, "\n");
                            GameManager.instance.messageScript.GeneralRandom("Gear Choice, Rare gear FAILED", "Rare Gear", chance, rnd);
                        }
                    }
                    //common gear
                    else
                    {
                        if (tempCommonGear.Count > 0)
                        {
                            index = Random.Range(0, tempCommonGear.Count);
                            gearID = tempCommonGear[index];
                            tempCommonGear.RemoveAt(index);
                        }
                    }
                    //found some gear?
                    if (gearID > -1)
                    { arrayOfGear[i] = gearID; countOfGear++; }
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
                        Gear gear = GameManager.instance.dataScript.GetGear(arrayOfGear[i]);
                        if (gear != null)
                        {
                            //tooltip 
                            GenericTooltipDetails tooltipDetails = GetGearTooltip(gear);
                            if (tooltipDetails != null)
                            {
                                //option details
                                GenericOptionDetails optionDetails = new GenericOptionDetails();
                                optionDetails.optionID = gear.gearID;
                                optionDetails.text = gear.name.ToUpper();
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
            if (isIgnoreCache == true)
            {
                //activate Generic Picker window
                EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails, "GearManager.cs -> InitialiseGenericPickerGear");
                if (isPlayer == true)
                {
                    //cache details in case player attempts to access Player gear selection again this action
                    selectionPlayerTurn = GameManager.instance.turnScript.Turn;
                    cachedPlayerDetails = genericDetails;
                    isNewActionPlayer = false;
                }
                else
                {
                    //cache details in case player attempts to access Actor gear selection again this action
                    selectionActorTurn = GameManager.instance.turnScript.Turn;
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
        //close node tooltip -> safety check
        GameManager.instance.tooltipNodeScript.CloseTooltip("GearManager.cs -> InitialiseGearInventoryDisplay");
        //only for Resistance
        if (GameManager.instance.sideScript.PlayerSide == GameManager.instance.globalScript.sideResistance)
        {
            numOfGear = GameManager.instance.playerScript.CheckNumOfGear();
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
                data.side = GameManager.instance.sideScript.PlayerSide;
                data.handler = RefreshGearInventory;
                data.state = InventoryState.Gear;
                //Loop Gear list and populate arrays
                List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
                if (listOfGear != null)
                {
                    for (int i = 0; i < numOfGear; i++)
                    {
                        Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                        if (gear != null)
                        {
                            GenericTooltipDetails tooltipDetails = GetGearTooltip(gear);
                            if (tooltipDetails != null)
                            {
                                InventoryOptionData optionData = new InventoryOptionData();
                                optionData.sprite = gear.sprite;
                                optionData.textUpper = gear.name.ToUpper();
                                //colour code Rarity
                                switch (gear.rarity.name)
                                {
                                    case "Common": colourRarity = colourBad; break;
                                    case "Rare": colourRarity = colourNeutral; break;
                                    case "Unique": colourRarity = colourGood; break;
                                    default: colourRarity = colourDefault; break;
                                }
                                optionData.textLower = string.Format("{0}{1}{2}{3}{4}{5}{6}", colourRarity, gear.rarity.name, colourEnd, "\n",
                                    colourDefault, gear.type.name, colourEnd);
                                optionData.optionID = gear.gearID;
                                //add to array
                                data.arrayOfOptions[i] = optionData;
                                data.arrayOfTooltips[i] = tooltipDetails;
                            }
                            else
                            { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
                        }
                        else
                        { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
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
                        side = GameManager.instance.sideScript.PlayerSide,
                        textTop = string.Format("{0}Something has gone wrong with your Inventory{1}", colourAlert, colourEnd),
                        textBottom = "Phone calls are being made. Lots of them.",
                        sprite = GameManager.instance.guiScript.errorSprite,
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
                    side = GameManager.instance.sideScript.PlayerSide,
                    textTop = string.Format("{0}There is currently no gear in your inventory{1}", colourAlert, colourEnd),
                    textBottom = string.Format("You can have a maximum of {0} items of Gear in your inventtory", maxNumOfGear),
                    sprite = GameManager.instance.guiScript.infoSprite,
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
                side = GameManager.instance.sideScript.PlayerSide,
                textTop = string.Format("{0}Gear is only available when you are playing the Resistance side{1}", colourAlert, colourEnd),
                sprite = GameManager.instance.guiScript.infoSprite,
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
        GameManager.instance.tooltipNodeScript.CloseTooltip("GearManager.cs -> RefreshGearInventory");
        //only for Resistance
        if (GameManager.instance.sideScript.PlayerSide == GameManager.instance.globalScript.sideResistance)
        {
            numOfGear = GameManager.instance.playerScript.CheckNumOfGear();
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
            List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
            if (listOfGear != null)
            {
                for (int i = 0; i < numOfGear; i++)
                {
                    Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                    if (gear != null)
                    {
                        GenericTooltipDetails tooltipDetails = GetGearTooltip(gear);
                        if (tooltipDetails != null)
                        {
                            InventoryOptionData optionData = new InventoryOptionData();
                            optionData.sprite = gear.sprite;
                            optionData.textUpper = gear.name.ToUpper();
                            //colour code Rarity
                            switch (gear.rarity.name)
                            {
                                case "Common": colourRarity = colourBad; break;
                                case "Rare": colourRarity = colourNeutral; break;
                                case "Unique": colourRarity = colourGood; break;
                                default: colourRarity = colourDefault; break;
                            }
                            optionData.textLower = string.Format("{0}{1}{2}{3}{4}{5}{6}", colourRarity, gear.rarity.name, colourEnd, "\n",
                                colourDefault, gear.type.name, colourEnd);
                            optionData.optionID = gear.gearID;
                            //add to array
                            data.arrayOfOptions[i] = optionData;
                            data.arrayOfTooltips[i] = tooltipDetails;
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
            if (data.optionID > -1)
            {
                int renown = GameManager.instance.playerScript.Renown;
                renown -= gearSaveCurrentCost;
                if (renown < 0)
                {
                    Debug.LogWarning("Renown invalid (< 0)");
                    renown = 0;
                }
                GameManager.instance.playerScript.Renown = renown;
                //retain saved gear, remove any unsaved gear
                string gearSavedName = GameManager.instance.playerScript.UpdateGear(gearSaveCurrentCost, data.optionID);
                //stats
                Gear gear = GameManager.instance.dataScript.GetGear(data.optionID);
                if (gear != null)
                {
                    gear.statTimesSaved++;
                    gear.statRenownSpent += gearSaveCurrentCost;
                }
                else { Debug.LogWarningFormat("Invalid gear (Null) for gearID {0}", data.optionID); }
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
                    if (gearName.Equals(gearSavedName) == false)
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
            GameManager.instance.playerScript.UpdateGear();
            //outcome -> all gear lost
            ModalOutcomeDetails details = new ModalOutcomeDetails();
            StringBuilder builderTop = new StringBuilder();
            builderTop.AppendFormat("{0}Gear used this turn has been{1}{2}{3}COMPROMISED{4}{5}", colourNormal, colourEnd, "\n", colourBad, colourEnd, "\n");
            builderTop.AppendFormat("{0}You can {1}{2}Save{3}{4} gear with Renown{5}", colourNormal, colourEnd, colourGood, colourEnd, colourNormal, colourEnd);
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
        if (data.optionID > -1)
        {
            //get currently selected node
            if (data.nodeID != -1)
            {
                Gear gear = GameManager.instance.dataScript.GetGear(data.optionID);
                if (gear != null)
                {
                    //check whether it is invisible type gear (if so then invisibility -1 effect is ignored)
                    if (gear.type.name.Equals("Invisibility") == true)
                    { isInvisibility = true; }
                    Sprite sprite = gear.sprite;
                    Node node = GameManager.instance.dataScript.GetNode(data.nodeID);
                    if (node != null)
                    {
                        if (GameManager.instance.nodeScript.nodePlayer == node.nodeID)
                        { isPlayer = true; }
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, GameManager.instance.globalScript.sideResistance);
                        if (actor != null)
                        {
                            StringBuilder builderTop = new StringBuilder();
                            StringBuilder builderBottom = new StringBuilder();

                            if (GameManager.instance.playerScript.AddGear(gear.gearID) == true)
                            {
                                //remove gear from pool
                                if (GameManager.instance.dataScript.RemoveGearFromPool(gear) == false)
                                { Debug.LogWarningFormat("Invalid removal for \"{0}\", ID {1}", gear.name, gear.gearID); }
                                //stat
                                gear.statTurnObtained = GameManager.instance.turnScript.Turn;
                                //gear successfully acquired
                                builderTop.Append(string.Format("{0}We have the goods!{1}", colourNormal, colourEnd));
                                builderBottom.Append(string.Format("{0}{1}{2}{3} is in our possession{4}", colourGear, gear.name.ToUpper(), colourEnd,
                                    colourDefault, colourEnd));
                                //reset flags for gear caches
                                if (isPlayer == true)
                                { isNewActionPlayer = true; }
                                else { isNewActionActor = true; }
                                //message
                                string textMsg;
                                if (isPlayer == true)
                                {
                                    textMsg = string.Format("{0} ({1}) has been acquired (by PLAYER)", gear.name, gear.type.name);
                                    GameManager.instance.messageScript.GearObtained(textMsg, node, gear);
                                }
                                else
                                {
                                    textMsg = string.Format("{0} ({1}) has been acquired ( by {2})", gear.name, gear.type.name, actor.arc.name);
                                    GameManager.instance.messageScript.GearObtained(textMsg, node, gear, actor.actorID);
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
                                            if (isInvisibility == true && effect.outcome.name.Equals("Invisibility") == true)
                                            { Debug.LogFormat("GearManager.cs -> ProcessGearChoice: {0} effect ignored due to Invisibility{1}", 
                                                effect.name, "\n"); }
                                            else
                                            {
                                                //process effect normally
                                                EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actor);
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
                            details.side = GameManager.instance.globalScript.sideResistance;
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
                else { Debug.LogError(string.Format("Invalid Gear (Null) for teamID {0}", data.optionID)); }
            }
            else { Debug.LogError("Highlighted node invalid (default '-1' value)"); }
        }
        else { Debug.LogError("Invalid gearID (default '-1')"); }
    }

    /// <summary>
    /// returns chance of gear being compromised. Depends on gear rarity (Same for all). Returns '0' is a problem.
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public int GetChanceOfCompromise(int gearID)
    {
        int chance = 0;
        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
        if (gear != null)
        {
            //chance of compromise same for all
            switch (gear.rarity.name)
            {
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
            Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", gearID));
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
                string msgText = string.Format("{0} used to {1}", gear.name, descriptorUsedTo);
                GameManager.instance.messageScript.GearUsed(msgText, gear);
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
        string textMsg = string.Format("{0}, ID {1} has been compromised. Saved by using {2} Renown.", gear.name, gear.gearID, amount);
        GameManager.instance.messageScript.RenownUsedPlayer(textMsg, string.Format("save {0} gear", gear.name), amount, gear.gearID, node.nodeID);
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
            builderHeader.AppendFormat("{0}<size=110%>{1}</size>{2}", colourGear, gear.name.ToUpper(), colourEnd);
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
            //Node use
            builderHeader.AppendLine();
            switch(gear.type.name)
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
            //has been used this turn
            if (gear.timesUsed > 0)
            {
                builderHeader.AppendLine();
                builderHeader.AppendFormat("</size>{0}USED this Turn{1}Can't be gifted{2}", colourBad, "\n", colourEnd);
            }
            //details
            builderDetails.AppendFormat("{0}{1}{2}", colourGood, gear.rarity.name, colourEnd);
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
            builderHeader.AppendFormat("{0}<size=110%>{1}</size>{2}", colourGear, gear.name.ToUpper(), colourEnd);
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
            builderHeader.AppendFormat("{0}<size=110%>{1}</size>{2}", colourGear, gear.name.ToUpper(), colourEnd);
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

    //new methods above here
}
