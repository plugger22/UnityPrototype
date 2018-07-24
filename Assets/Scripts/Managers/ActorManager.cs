using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using modalAPI;
using packageAPI;
using System;
using Random = UnityEngine.Random;
using System.Text;
using UnityEngine.Events;

/// <summary>
/// handles Actor related data and methods
/// </summary>
public class ActorManager : MonoBehaviour
{
    [Header("Basics")]
    [HideInInspector] public int numOfActiveActors;    //Actors who are OnMap and active, eg. not asleep or captured
    [Tooltip("Maxium number of actors (Active or Inactive) that can be onMap (eg. 'Onscreen') at any one time")]
    [Range(1, 4)] public int maxNumOfOnMapActors = 4;      //if you increase this then GUI elements and GUIManager will need to be changed to accomodate it, default value 4
    [Tooltip("Maximum number of actors that can be in the replacement pool (applies to both sides)")]
    [Range(1, 6)] public int maxNumOfReserveActors = 4;    //
    [Tooltip("The maximum number of stats (Qualities) that an actor can have")]
    [Range(2,4)] public int numOfQualities = 3;        //number of qualities actors have (different for each side), eg. "Connections, Invisibility" etc. Map to DataPoint0 -> DataPoint'x'
    [Tooltip("Maximum value of an actor datapoint stat")]
    [Range(2, 4)] public int maxStatValue = 3;
    [Tooltip("Minimum value of an actor datapoint stat")]
    [Range(2, 4)] public int minStatValue = 0;

    [Header("Management")]
    [Tooltip("Actor sent to Reserves and Player promises to recall them within this number of turns. Their unhappy timer will be set to this number of turns.")]
    [Range(1, 10)] public int promiseReserveTimer = 10;
    [Tooltip("Actor sent to Reserves and Player did NOT promise anything. Their unhappy timer will be set to this number of turns.")]
    [Range(1, 10)] public int noPromiseReserveTimer = 10;
    [Tooltip("Actor sent to Reserves to Rest. Their unhappy timer will be set to this number of turns.")]
    [Range(1, 10)] public int restReserveTimer = 10;
    [Tooltip("Actor Recruited and placed in Reserves. Their unhappy timer will be set to this number of turns.")]
    [Range(1, 10)] public int recruitedReserveTimer = 5;
    [Tooltip("Renown cost for threatening an actor in the Reserve Pool")]
    [Range(1, 3)] public int renownCostThreaten = 1;
    [Tooltip("Base Renown cost for carrying out Manage Reserve Actor actions")]
    [Range(1, 5)] public int manageReserveRenown = 1;
    [Tooltip("Base Renown cost for carrying out Manage Dismiss Actor actions")]
    [Range(1, 5)] public int manageDismissRenown = 2;
    [Tooltip("Base Renown cost for carrying out Manage Dispose Actor actions")]
    [Range(1, 5)] public int manageDisposeRenown = 3;
    [Tooltip("% Chance of an actor in the Reserve Pool becoming unhappy each turn once their unhappyTimer expires")]
    [Range(1, 50)] public int chanceOfUnhappy = 20;
    [Tooltip("Multiplier to the chanceOfUnhappy for an actor who has been promised that they will be recalled within a set time period")]
    [Range(1, 5)] public int unhappyPromiseFactor = 3;
    [Tooltip("Once actor has taken action as a result of being unhappy this is the number of turns warning period you get before they carry out their action")]
    [Range(1, 5)] public int unhappyWarningPeriod = 2;
    [Tooltip("Once actor is unhappy, the chance per turn (1d100) of losing motivation -1")]
    [Range(1, 99)] public int unhappyLoseMotivationChance = 50;
    [Tooltip("Once actor is unhappy and has motivation 0 the chance of them acting on their dissatisfaction / turn")]
    [Range(1, 99)] public int unhappyTakeActionChance = 25;
    [Tooltip("When an unhappy actor in the Reserve pool takes action this is the first check made (ignored if actor has no secrets")]
    [Range(1, 99)] public int unhappyRevealSecretChance = 50;
    [Tooltip("When an unhappy actor in the Reserve pool takes action this is the second check made. Double chance if actor has previously complained")]
    [Range(1, 99)] public int unhappyLeaveChance = 25;
    [Tooltip("When an unhappy actor in the Reserve pool takes action this is the third check made. An actor can only complain once")]
    [Range(1, 99)] public int unhappyComplainChance = 50;
    [Tooltip("Increase to the actor's Unhappy Timer after they have been Reassured")]
    [Range(1, 10)] public int unhappyReassureBoost = 5;
    [Tooltip("Increase to the actor's Unhappy Timer after they have been Threatened")]
    [Range(1, 10)] public int unhappyThreatenBoost = 5;
    [Tooltip("Amount of motivation lost when actor let go from reserve pool")]
    [Range(1, 3)] public int motivationLossLetGo = 1;
    [Tooltip("Amount of motivation lost when actor fired")]
    [Range(1, 3)] public int motivationLossFire = 2;
    [Tooltip("Amount of motivation gained when recalled from Reserves to Active Duty")]
    [Range(1, 3)] public int motivationGainActiveDuty = 2;

    [Header("Assorted")]
    [Tooltip("Renown cost for negating a bad gear use roll")]
    [Range(1, 3)] public int renownCostGear = 1;
    [Tooltip("Chance of a character with the Stressed condition having a breakdown and missing a turn")]
    [Range(1, 99)] public int breakdownChance = 5;
    [Tooltip("Chance, per turn, of a character resigning if the Player has a bad condition (corrupt/quesitonable/incompetent). Chance stacks for each bad condition present")]
    [Range(0, 10)] public int playerBadResignChance = 5;
 

    //cached recruit picker choices
    private int resistancePlayerTurn;                                           //turn number of last choice for a resistance Player Recruit selection
    private int resistanceActorTurn;                                            //turn number of last choice for an resistance Actor Recruit selection
    private int authorityTurn;                                                  //turn number of last choice for an authority Actor Recruit selection
    private GenericPickerDetails cachedResistancePlayerDetails;                 //last resistance player recruit selection this action
    private GenericPickerDetails cachedResistanceActorDetails;                  //last resistance actor recruit selection this action
    private GenericPickerDetails cachedAuthorityDetails;                        //last authority recruit selection this action
    private bool isNewActionResistancePlayer;                                   //set to true after resistance player makes a recruit choice at own node
    private bool isNewActionResistanceActor;                                    //set to true after resistance player makes a recruit choice at an actor contact's node
    private bool isNewActionAuthority;                                          //set to true after autority makes a recruit choice

    private static int actorIDCounter = 0;              //used to sequentially number actorID's

    //fast access fields
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;
    private Condition conditionStressed;
    private Condition conditionBlackmailer;
    private Condition conditionCorrupt;
    private Condition conditionIncompetent;
    private Condition conditionQuestionable;
    private TraitCategory actorCategory;
    private int secretBaseChance = -1;
    //cached TraitEffects
    private int actorBreakdownChanceHigh;
    private int actorBreakdownChanceLow;
    private int actorBreakdownChanceNone;
    private int actorNoActionsDuringSecurityMeasures;
    private int actorSecretChanceHigh;
    private int actorSecretChanceNone;
    private int actorSecretTellAll;
    private int actorAppeaseNone;
    private int actorConflictNoGoodOptions;
    private int actorConflictNone;
    private int actorNeverResigns;
    private int actorConflictKill;
    private int actorResignHigh;
    

    //gear
    private int maxNumOfGear;
    private int gearGracePeriod = -1;
    private int gearSwapBaseAmount = -1;
    private int gearSwapPreferredAmount = -1;
    //traits
    private int actorKeepGear = 1;
    //colour palette for Generic tool tip
    private string colourResistance;
    private string colourAuthority;
    private string colourCancel;
    private string colourInvalid;
    private string colourAlert;
    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourDefault;
    private string colourNormal;
    private string colourRecruit;
    private string colourArc;
    private string colourEnd;


    public void PreInitialiseActors()
    {
        //number of actors, default 4
        maxNumOfOnMapActors = maxNumOfOnMapActors == 4 ? maxNumOfOnMapActors : 4;
        numOfActiveActors = maxNumOfOnMapActors;
    }


    public void Initialise()
    {
        //recruit actors cached fields
        resistancePlayerTurn = -1;
        resistanceActorTurn = -1;
        authorityTurn = -1;
        cachedResistancePlayerDetails = null;
        cachedResistanceActorDetails = null;
        cachedAuthorityDetails = null;
        isNewActionResistancePlayer = true;
        isNewActionResistanceActor = true;
        isNewActionAuthority = true;
        //fast acess fields
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        conditionStressed = GameManager.instance.dataScript.GetCondition("STRESSED");
        conditionBlackmailer = GameManager.instance.dataScript.GetCondition("BLACKMAILER");
        conditionCorrupt = GameManager.instance.dataScript.GetCondition("CORRUPT");
        conditionIncompetent = GameManager.instance.dataScript.GetCondition("INCOMPETENT");
        conditionQuestionable = GameManager.instance.dataScript.GetCondition("QUESTIONABLE");
        actorCategory = GameManager.instance.dataScript.GetTraitCategory("Actor");
        secretBaseChance = GameManager.instance.secretScript.secretLearnBaseChance;
        maxNumOfGear = GameManager.instance.gearScript.maxNumOfGear;
        gearGracePeriod = GameManager.instance.gearScript.actorGearGracePeriod;
        gearSwapBaseAmount = GameManager.instance.gearScript.gearSwapBaseAmount;
        gearSwapPreferredAmount = GameManager.instance.gearScript.gearSwapPreferredAmount;

        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(conditionStressed != null, "Invalid conditionStressed (Null)");
        Debug.Assert(conditionBlackmailer != null, "Invalid conditionBlackmailer (Null)");
        Debug.Assert(conditionCorrupt != null, "Invalid conditionCorrupt (Null)");
        Debug.Assert(conditionIncompetent != null, "Invalid conditionIncompetent (Null)");
        Debug.Assert(conditionQuestionable != null, "Invalid conditionQuestionable (Null)");
        Debug.Assert(actorCategory != null, "Invalid actorCategory (Null)");
        Debug.Assert(secretBaseChance > -1, "Invalid secretBaseChance");
        Debug.Assert(maxNumOfGear > 0, "Invalid maxNumOfGear (zero)");
        Debug.Assert(gearGracePeriod > -1, "Invalid gearGracePeriod (-1)");
        Debug.Assert(gearSwapBaseAmount > -1, "Invalid gearSwapBaseAmount (-1)");
        Debug.Assert(gearSwapPreferredAmount > -1, "Invalid gearSwapPreferredAmount (-1)");
        
        
        //cached TraitEffects
        actorBreakdownChanceHigh = GameManager.instance.dataScript.GetTraitEffectID("ActorBreakdownChanceHigh");
        actorBreakdownChanceLow = GameManager.instance.dataScript.GetTraitEffectID("ActorBreakdownChanceLow");
        actorBreakdownChanceNone = GameManager.instance.dataScript.GetTraitEffectID("ActorBreakdownChanceNone");
        actorNoActionsDuringSecurityMeasures = GameManager.instance.dataScript.GetTraitEffectID("ActorNoActionsSecurity");
        actorSecretChanceHigh = GameManager.instance.dataScript.GetTraitEffectID("ActorSecretChanceHigh");
        actorSecretChanceNone = GameManager.instance.dataScript.GetTraitEffectID("ActorSecretChanceNone");
        actorSecretTellAll = GameManager.instance.dataScript.GetTraitEffectID("ActorSecretTellAll");
        actorAppeaseNone = GameManager.instance.dataScript.GetTraitEffectID("ActorAppeaseNone");
        actorKeepGear = GameManager.instance.dataScript.GetTraitEffectID("ActorKeepGear");
        actorConflictNoGoodOptions = GameManager.instance.dataScript.GetTraitEffectID("ActorConflictGoodNone");
        actorConflictNone = GameManager.instance.dataScript.GetTraitEffectID("ActorConflictNone");
        actorConflictKill = GameManager.instance.dataScript.GetTraitEffectID("ActorConflictKill");
        actorNeverResigns = GameManager.instance.dataScript.GetTraitEffectID("ActorResignNone");
        actorResignHigh = GameManager.instance.dataScript.GetTraitEffectID("ActorResignHigh");
        Debug.Assert(actorBreakdownChanceHigh > -1, "Invalid actorBreakdownHigh (-1)");
        Debug.Assert(actorBreakdownChanceLow > -1, "Invalid actorBreakdownLow (-1)");
        Debug.Assert(actorBreakdownChanceNone > -1, "Invalid actorBreakdownNone (-1)");
        Debug.Assert(actorNoActionsDuringSecurityMeasures > -1, "Invalid actorNoActionsDuringSecurityMeasures (-1)");
        Debug.Assert(actorSecretChanceHigh > -1, "Invalid actorSecretChanceHigh (-1)");
        Debug.Assert(actorSecretChanceNone > -1, "Invalid actorSecretChanceNone (-1)");
        Debug.Assert(actorAppeaseNone > -1, "Invalid actorAppeaseNone (-1)");
        Debug.Assert(actorKeepGear > -1, "Invalid actorKeepGear (-1)");
        Debug.Assert(actorConflictNoGoodOptions > -1, "Invalid actorConflictNoGoodOptions (-1)");
        Debug.Assert(actorConflictNone > -1, "Invalid actorConflictNone (-1)");
        Debug.Assert(actorConflictKill > -1, "Invalid actorConflictKill (-1)");
        Debug.Assert(actorNeverResigns > -1, "Invalid actorNeverResigns (-1)");
        Debug.Assert(actorResignHigh > -1, "Invalid actorResignHigh (-1)");
        //event listener is registered in InitialiseActors() due to GameManager sequence.
        EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent, "ActorManager");
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "ActorManager");
        EventManager.instance.AddListener(EventType.RecruitAction, OnEvent, "ActorManager");
        EventManager.instance.AddListener(EventType.RecruitDecision, OnEvent, "ActorManager");
        EventManager.instance.AddListener(EventType.GenericRecruitActorResistance, OnEvent, "ActorManager");
        EventManager.instance.AddListener(EventType.GenericRecruitActorAuthority, OnEvent, "ActorManager");
        EventManager.instance.AddListener(EventType.InventorySetReserve, OnEvent, "ActorManager");
        //create active, OnMap actors
        InitialiseActors(maxNumOfOnMapActors, GameManager.instance.globalScript.sideResistance);
        InitialiseActors(maxNumOfOnMapActors, GameManager.instance.globalScript.sideAuthority);
        //create pool actors
        InitialisePoolActors();

    }

    /// <summary>
    /// handles events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.StartTurnLate:
                StartTurnLate();
                break;
            case EventType.RecruitAction:
                ModalActionDetails details = Param as ModalActionDetails;
                InitialiseGenericPickerRecruit(details);
                break;
            case EventType.RecruitDecision:
                RecruitActor((int)Param);
                break;
            case EventType.GenericRecruitActorResistance:
                GenericReturnData returnDataRecruitResistance = Param as GenericReturnData;
                ProcessRecruitChoiceResistance(returnDataRecruitResistance);
                break;
            case EventType.GenericRecruitActorAuthority:
                GenericReturnData returnDataRecruitAuthority = Param as GenericReturnData;
                ProcessRecruitChoiceAuthority(returnDataRecruitAuthority);
                break;
            case EventType.InventorySetReserve:
                InitialiseReservePoolInventory();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Pre turn processing
    /// </summary>
    private void StartTurnLate()
    {
        CheckPlayerStartLate();
        if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
        {
            //run for Resistance Player
            CheckInactiveResistanceActors();
            CheckActiveResistanceActors();    //needs to be AFTER CheckInactiveActors
            //end game checks
            GameManager.instance.factionScript.CheckFactionFirePlayer();
            GameManager.instance.cityScript.CheckCityLoyaltyAtLimit();
        }
        else
        {
            //run for Authority Player
            CheckInactiveAuthorityActors();
            CheckActiveAuthorityActors();    //needs to be AFTER CheckInactiveActors
            //end game checks
            GameManager.instance.factionScript.CheckFactionFirePlayer();
            GameManager.instance.cityScript.CheckCityLoyaltyAtLimit();
        }
        UpdateReserveActors();
    }


    /// <summary>
    /// deregisters events
    /// </summary>
    public void OnDisable()
    {
        EventManager.instance.RemoveEvent(EventType.NodeAction);
        EventManager.instance.RemoveEvent(EventType.TargetAction);
    }

    /// <summary>
    /// set colour palette for Generic Tool tip
    /// </summary>
    public void SetColours()
    {
        colourResistance = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourCancel = GameManager.instance.colourScript.GetColour(ColourType.cancelNormal);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.cancelHighlight);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourRecruit = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourArc = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }



    /// <summary>
    /// Set up number of required actors (minions supporting play)
    /// </summary>
    /// <param name="num"></param>
    public void InitialiseActors(int num, GlobalSide side)
    {
        if (num > 0)
        {
            //get a list of random actorArcs
            List<ActorArc> tempActorArcs = GameManager.instance.dataScript.GetRandomActorArcs(num, side);
            //Create actors
            for (int i = 0; i < num; i++)
            {
                Actor actor = CreateActor(side, tempActorArcs[i].ActorArcID, 1, ActorStatus.Active, i);
                if (actor != null)
                {
                    Debug.Log(string.Format("Actor added -> {0}, {1} {2}, {3} {4}, {5} {6}, level {7}{8}", actor.arc.actorName,
                        GameManager.instance.dataScript.GetQuality(side, 0), actor.datapoint0, 
                        GameManager.instance.dataScript.GetQuality(side, 1), actor.datapoint1, 
                        GameManager.instance.dataScript.GetQuality(side, 2), actor.datapoint2, actor.level, "\n"));
                }
                else { Debug.LogWarning("Actor not created"); }
            }
        }
        else { Debug.LogWarning("Invalid number of Actors (Zero, or less)"); }
    }

    /// <summary>
    /// populate the pools to recruit from (one full set of actor arcs for each side and each level)
    /// </summary>
    public void InitialisePoolActors()
    {
        int numOfArcs;
        //Authority
        List<ActorArc> listOfArcs = GameManager.instance.dataScript.GetActorArcs(globalAuthority);
        if (listOfArcs != null)
        {
            numOfArcs = listOfArcs.Count;
            for (int i = 0; i < numOfArcs; i++)
            {
                //level one actor
                Actor actorOne = CreateActor(globalAuthority, listOfArcs[i].ActorArcID, 1, ActorStatus.RecruitPool);
                if (actorOne != null)
                {
                    //NOTE: need to add to Dictionary BEFORE adding to Pool (Debug.Assert checks dictOfActors.Count in AddActorToPool)
                    GameManager.instance.dataScript.AddActorToDict(actorOne);
                    GameManager.instance.dataScript.AddActorToPool(actorOne.actorID, 1, globalAuthority);
                }
                else { Debug.LogWarning(string.Format("Invalid Authority actorOne (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
                //level two actor
                Actor actorTwo = CreateActor(globalAuthority, listOfArcs[i].ActorArcID, 2, ActorStatus.RecruitPool);
                if (actorTwo != null)
                {
                    GameManager.instance.dataScript.AddActorToDict(actorTwo);
                    GameManager.instance.dataScript.AddActorToPool(actorTwo.actorID, 2, globalAuthority);
                }
                else { Debug.LogWarning(string.Format("Invalid Authority actorTwo (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
                //level three actor
                Actor actorThree = CreateActor(globalAuthority, listOfArcs[i].ActorArcID, 3, ActorStatus.RecruitPool);
                if (actorThree != null)
                {
                    GameManager.instance.dataScript.AddActorToDict(actorThree);
                    GameManager.instance.dataScript.AddActorToPool(actorThree.actorID, 3, globalAuthority);
                }
                else { Debug.LogWarning(string.Format("Invalid Authority actorThree (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
            }
        }
        else { Debug.LogError("Invalid list of Authority Actor Arcs (Null)"); }
        //Resistance
        listOfArcs = GameManager.instance.dataScript.GetActorArcs(globalResistance);
        if (listOfArcs != null)
        {
            numOfArcs = listOfArcs.Count;
            for (int i = 0; i < numOfArcs; i++)
            {
                //level one actor
                Actor actorOne = CreateActor(globalResistance, listOfArcs[i].ActorArcID, 1, ActorStatus.RecruitPool);
                if (actorOne != null)
                {
                    //NOTE: need to add to Dictionary BEFORE adding to Pool (Debug.Assert checks dictOfActors.Count in AddActorToPool)
                    GameManager.instance.dataScript.AddActorToDict(actorOne);
                    GameManager.instance.dataScript.AddActorToPool(actorOne.actorID, 1, globalResistance);
                }
                else { Debug.LogWarning(string.Format("Invalid Resistance actorOne (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
                //level two actor
                Actor actorTwo = CreateActor(globalResistance, listOfArcs[i].ActorArcID, 2, ActorStatus.RecruitPool);
                if (actorTwo != null)
                {
                    GameManager.instance.dataScript.AddActorToDict(actorTwo);
                    GameManager.instance.dataScript.AddActorToPool(actorTwo.actorID, 2, globalResistance);
                }
                else { Debug.LogWarning(string.Format("Invalid Resistance actorTwo (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
                //level three actor
                Actor actorThree = CreateActor(globalResistance, listOfArcs[i].ActorArcID, 3, ActorStatus.RecruitPool);
                if (actorThree != null)
                {
                    GameManager.instance.dataScript.AddActorToDict(actorThree);
                    GameManager.instance.dataScript.AddActorToPool(actorThree.actorID, 3, globalResistance);
                }
                else { Debug.LogWarning(string.Format("Invalid Resistance actorThree (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
            }
        }
        else { Debug.LogError("Invalid list of Resistance Actor Arcs (Null)"); }
    }

    /// <summary>
    /// creates a new actor of the specified actor arc type, side and level (1 worst -> 3 best). SlotID (0 to 3) for current actor, default '-1' for reserve pool actor.
    /// adds actor to dataManager.cs ArrayOfActors if OnMap (slot ID > -1). NOT added to dictOfActors if slotID -1 (do so after actor is chosen in generic picker)
    /// returns null if a problem
    /// </summary>
    /// <param name="arc"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public Actor CreateActor(GlobalSide side, int actorArcID, int level, ActorStatus status, int slotID = -1)
    {
        Debug.Assert(level > 0 && level < 4, "Invalid level (must be between 1 and 3)");
        Debug.Assert(slotID >= -1 && slotID <= maxNumOfOnMapActors, "Invalid slotID (must be -1 (default) or between 1 and 3");
        //check a valid actor Arc parameter
        ActorArc arc = GameManager.instance.dataScript.GetActorArc(actorArcID);
        if (arc != null)
        {
            //check actor arc is the correct side
            if (arc.side.level == side.level)
            {
                //create new actor
                Actor actor = new Actor();
                //data
                actor.actorID = actorIDCounter++;
                actor.actorSlotID = slotID;                
                actor.level = level;
                actor.side = side;
                actor.arc = arc;
                actor.actorName = arc.actorName;
                actor.AddTrait(GameManager.instance.dataScript.GetRandomTrait(actorCategory, side));
                actor.Status = status;
                Debug.Assert(actor.GetTrait() != null, "Invalid actor.trait (Null)");
                //level -> range limits
                int limitLower = 1;
                if (level == 3) { limitLower = 2; }
                int limitUpper = Math.Min(4, level + 2);
                //level -> assign
                actor.datapoint0 = Random.Range(limitLower, limitUpper); //connections and influence
                actor.datapoint1 = Random.Range(limitLower, limitUpper); //motivation and support
                if (side.level == GameManager.instance.globalScript.sideResistance.level)
                {
                    //invisibility -> Level 3 100% Invis 3, level 2 25% Invis 2, 75% Invis 3, level 1 50% Invis 2, 50% Invis 3
                    switch(actor.level)
                    {
                        case 3: actor.datapoint2 = 3; break;
                        case 2: 
                            if (Random.Range(0,100) <= 25) { actor.datapoint2 = 2; }
                            else { actor.datapoint2 = 3; }
                            break;
                        case 1:
                            if (Random.Range(0, 100) <= 50) { actor.datapoint2 = 2; }
                            else { actor.datapoint2 = 3; }
                            break;
                    }
                    
                }
                else if (side.level == GameManager.instance.globalScript.sideAuthority.level)
                { actor.datapoint2 = Random.Range(limitLower, limitUpper); /*Ability*/}
                //OnMap actor
                if (slotID > -1)
                {
                    actor.Renown = 0;
                    //add to data collections
                    GameManager.instance.dataScript.AddCurrentActor(side, actor, slotID);
                    GameManager.instance.dataScript.AddActorToDict(actor);
                }
                //return actor
                return actor;
            }
            else { Debug.LogError(string.Format("ActorArc (\"{0}\") is the wrong side for actorArcID {1}", arc.name, actorArcID)); }
        }
        else { Debug.LogError(string.Format("Invalid ActorArc (Null) for actorArcID {0}", actorArcID)); }
        return null;
    }


    /// <summary>
    /// Returns a list of all relevant Actor Actions for the  node to enable a ModalActionMenu to be put together (one button per action). 
    /// Resistance -> Max 4 Actor + 1 Target actions with an additional 'Cancel' buttnn added last automatically -> total six buttons (hardwired into GUI design)
    /// Authority -> Insert and Recall Teams
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public List<EventButtonDetails> GetNodeActions(int nodeID)
    {
        string sideColour;
        string cancelText = null;
        string effectCriteria;
        bool proceedFlag;
        AuthoritySecurityState securityState = GameManager.instance.turnScript.authoritySecurityState;
        int actionID;
        Actor[] arrayOfActors;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"ss
        if ( playerSide.level == globalAuthority.level)
        { sideColour = colourAuthority; }
        else { sideColour = colourResistance; }
        List<EventButtonDetails> tempList = new List<EventButtonDetails>();
        //string builder for cancel button (handles all no go cases
        StringBuilder infoBuilder = new StringBuilder();
        //player's current node
        int playerID = GameManager.instance.nodeScript.nodePlayer;
        //Get Node
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            List<Effect> listOfEffects = new List<Effect>();
            Action tempAction;
            EventButtonDetails details;
            //
            // - - - Resistance - - -
            //
            if (playerSide.level == globalResistance.level)
            {
                //'Cancel' button tooltip)
                if (nodeID == playerID)
                { cancelText = "You are present at the node"; }
                else { cancelText = "You are NOT present at the node"; }
                //
                // - - -  Target - - -
                //
                //Does the Node have a target attached? -> added first
                if (node.targetID >= 0)
                {
                    Target target = GameManager.instance.dataScript.GetTarget(node.targetID);
                    if (target != null)
                    {
                        if (target.targetStatus == Status.Live)
                        {
                            bool targetProceed = false;
                            //can tackle target if Player at node or target specified actor is present in line-up
                            if (nodeID == playerID) { targetProceed = true; }
                            else if (GameManager.instance.dataScript.CheckActorArcPresent(target.actorArc, globalResistance) == true)
                            {
                                //check traits
                                int slotID = GameManager.instance.dataScript.CheckActorPresent(target.actorArc.ActorArcID, globalResistance);
                                Actor actorTemp = GameManager.instance.dataScript.GetCurrentActor(slotID, globalResistance);
                                if (actorTemp != null)
                                {
                                    //Not if Spooked trait and Security Measures in place
                                    if (actorTemp.CheckTraitEffect(actorNoActionsDuringSecurityMeasures) == true && securityState != AuthoritySecurityState.Normal)
                                    {
                                        targetProceed = false;
                                        infoBuilder.AppendFormat("{0} is {1}Spooked{2} (Trait) due to Security Measures", actorTemp.arc.name, colourNeutral, colourEnd);
                                        TraitLogMessage(actorTemp, "and is prevented from attempting a Target");
                                    }
                                    else { targetProceed = true; }
                                }
                                else
                                {
                                    Debug.LogWarningFormat("Invalid actor for target.actorArc.ActorArcID {0} and slotID {1}", target.actorArc.ActorArcID, slotID);
                                    targetProceed = false;
                                }
                            }
                            //target live and dancing
                            if (targetProceed == true)
                            {
                                string targetHeader = string.Format("{0}<size=110%>{1}</size>{2}{3}{4}{5}{6}", sideColour, target.name, colourEnd, "\n", colourDefault,
                                    target.description, colourEnd);
                                //button target details
                                EventButtonDetails targetDetails = new EventButtonDetails()
                                {
                                    buttonTitle = "Attempt Target",
                                    buttonTooltipHeader = targetHeader,
                                    buttonTooltipMain = GameManager.instance.targetScript.GetTargetFactors(node.targetID),
                                    buttonTooltipDetail = GameManager.instance.targetScript.GetTargetEffects(node.targetID),
                                    //use a Lambda to pass arguments to the action
                                    action = () => { EventManager.instance.PostNotification(EventType.TargetAction, this, nodeID, "ActorManager.cs -> GetNodeActions"); }
                                };
                                tempList.Add(targetDetails);
                            }
                            else
                            {
                                //invalid target (Player not present, specified actor Arc not in line up)
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                infoBuilder.AppendFormat("{0}Target invalid{1}{2}{3}(No Player, No {4}){5}",
                                    colourInvalid, "\n", colourEnd, colourBad, target.actorArc.name, colourEnd);
                            }
                        }
                        else
                        {
                            //target already completed
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.Append("Target completed");
                        }
                    }
                    else { Debug.LogError(string.Format("Invalid TargetID \"{0}\" (Null){1}", node.targetID, "\n")); }
                }
                //
                // - - - Actors - - - 
                //
                //loop actors currently in game -> get Node actions (1 per Actor, if valid criteria)
                arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
                foreach (Actor actor in arrayOfActors)
                {
                    //if invalid actor or vacant position then ignore
                    if (actor != null)
                    {
                        details = null;
                        //correct side?
                        if (actor.side.level == playerSide.level)
                        {
                            //actor active?
                            if (actor.Status == ActorStatus.Active)
                            {
                                proceedFlag = true;
                                //active node for actor or player at node
                                if (nodeID != playerID)
                                { 
                                    //check for actor connections at node
                                    if (GameManager.instance.levelScript.CheckNodeActive(node.nodeID, playerSide, actor.actorSlotID) == true)
                                    {
                                        //Not if actor has Spooked trait and Security Measures in place
                                        if (actor.CheckTraitEffect(actorNoActionsDuringSecurityMeasures ) == true && securityState != AuthoritySecurityState.Normal)
                                        {
                                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                            infoBuilder.AppendFormat("{0} is {1}Spooked{2} (Trait) due to Security Measures", actor.arc.name, colourNeutral, colourEnd);
                                            TraitLogMessage(actor, "and is prevented from carrying out a Node Action");
                                            proceedFlag = false;
                                        }
                                    }
                                    else
                                    {
                                        //actor has no connections at node
                                        if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                        infoBuilder.AppendFormat("{0} has no connections", actor.arc.name);
                                        proceedFlag = false;
                                    }
                                }
                                if (proceedFlag == true)
                                    { 
                                    //get node action
                                    tempAction = actor.arc.nodeAction;
                                    if (tempAction != null)
                                    {
                                        //effects
                                        StringBuilder builder = new StringBuilder();
                                        listOfEffects = tempAction.listOfEffects;
                                        if (listOfEffects.Count > 0)
                                        {
                                            string colourEffect = colourDefault;
                                            for (int i = 0; i < listOfEffects.Count; i++)
                                            {
                                                Effect effect = listOfEffects[i];
                                                //colour code effects according to type
                                                if (effect.typeOfEffect != null)
                                                {
                                                    switch (effect.typeOfEffect.name)
                                                    {
                                                        case "Good":
                                                            colourEffect = colourGood;
                                                            break;
                                                        case "Neutral":
                                                            colourEffect = colourNeutral;
                                                            break;
                                                        case "Bad":
                                                            colourEffect = colourBad;
                                                            break;
                                                    }
                                                }
                                                //check effect criteria is valid
                                                CriteriaDataInput criteriaInput = new CriteriaDataInput()
                                                {
                                                    nodeID = nodeID,
                                                    listOfCriteria = effect.listOfCriteria
                                                };
                                                effectCriteria = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                                                if (effectCriteria == null)
                                                {
                                                    //Effect criteria O.K -> tool tip text
                                                    if (builder.Length > 0) { builder.AppendLine(); }
                                                    if (effect.outcome.name.Equals("Renown") == false && effect.outcome.name.Equals("Invisibility") == false)
                                                    { builder.AppendFormat("{0}{1}{2}", colourEffect, effect.textTag, colourEnd); }
                                                    else
                                                    {
                                                        //handle renown & invisibility situations - players or actors?
                                                        if (nodeID == playerID)
                                                        {
                                                            //player affected (good for renown, bad for invisibility)
                                                            if (effect.outcome.name.Equals("Renown"))
                                                            { builder.AppendFormat("{0}Player {1}{2}", colourGood, effect.textTag, colourEnd); }
                                                            else
                                                            {
                                                                builder.AppendFormat("{0}Player {1}{2}", colourBad, effect.textTag, colourEnd);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //actor affected
                                                            builder.AppendFormat("{0}{1} {2}{3}", colourBad, actor.arc.name, effect.textTag, colourEnd);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //invalid effect criteria -> Action cancelled
                                                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                    infoBuilder.AppendFormat("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                        colourInvalid, actor.arc.name, "\n", colourEnd,
                                                        colourAuthority, effectCriteria, colourEnd);
                                                    proceedFlag = false;
                                                }
                                            }
                                        }
                                        else
                                        { Debug.LogWarning(string.Format("Action \"{0}\" has no effects", tempAction)); }
                                        if (proceedFlag == true)
                                        {
                                            //Details to pass on for processing via button click
                                            ModalActionDetails actionDetails = new ModalActionDetails() { };

                                            actionDetails.side = globalResistance;
                                            actionDetails.nodeID = nodeID;
                                            actionDetails.actorDataID = actor.actorSlotID;
                                            //pass all relevant details to ModalActionMenu via Node.OnClick()
                                            if (actor.arc.nodeAction.special == null)
                                            {
                                                details = new EventButtonDetails()
                                                {
                                                    buttonTitle = tempAction.name,
                                                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                    buttonTooltipMain = tempAction.tooltipText,
                                                    buttonTooltipDetail = builder.ToString(),
                                                    //use a Lambda to pass arguments to the action
                                                    action = () => { EventManager.instance.PostNotification(EventType.NodeAction, this, actionDetails, "ActorManager.cs -> GetNodeActions"); }
                                                };
                                            }
                                            //special case, Resistance node actions only
                                            else
                                            {
                                                switch (actor.arc.nodeAction.special.name)
                                                {
                                                    case "NeutraliseTeam":
                                                        details = new EventButtonDetails()
                                                        {
                                                            buttonTitle = tempAction.name,
                                                            buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                            buttonTooltipMain = tempAction.tooltipText,
                                                            buttonTooltipDetail = builder.ToString(),
                                                            action = () => { EventManager.instance.PostNotification(EventType.NeutraliseTeamAction, this, actionDetails, "ActorManager.cs -> GetNodeActions"); }
                                                        };
                                                        break;
                                                    case "GetGear":
                                                        details = new EventButtonDetails()
                                                        {
                                                            buttonTitle = tempAction.name,
                                                            buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                            buttonTooltipMain = tempAction.tooltipText,
                                                            buttonTooltipDetail = builder.ToString(),
                                                            action = () => { EventManager.instance.PostNotification(EventType.GearAction, this, actionDetails, "ActorManager.cs -> GetNodeActions"); }
                                                        };
                                                        break;
                                                    case "GetRecruit":
                                                        details = new EventButtonDetails()
                                                        {
                                                            buttonTitle = tempAction.name,
                                                            buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                            buttonTooltipMain = tempAction.tooltipText,
                                                            buttonTooltipDetail = builder.ToString(),
                                                            action = () => { EventManager.instance.PostNotification(EventType.RecruitAction, this, actionDetails, "ActorManager.cs -> GetNodeActions"); }
                                                        };
                                                        break;
                                                    default:
                                                        Debug.LogError(string.Format("Invalid actor.Arc.nodeAction.special \"{0}\"", actor.arc.nodeAction.special.name));
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                //actor isn't Active
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                switch (actor.Status)
                                {
                                    case ActorStatus.Inactive:
                                        infoBuilder.AppendFormat("{0} is lying low and unavailable", actor.arc.name);
                                        break;
                                    case ActorStatus.Captured:
                                        infoBuilder.AppendFormat("{0} has been captured", actor.arc.name);
                                        break;
                                    default:
                                        infoBuilder.AppendFormat("{0} is unavailable", actor.arc.name);
                                        break;
                                }
                            }

                            //add to list
                            if (details != null)
                            { tempList.Add(details); }
                        }
                    }
                }
            }
            //
            // - - - Authority - - -
            //
            else if (playerSide.level == globalAuthority.level)
            {
                int teamArcID;
                bool isAnyTeam;
                string tooltipMain;
                //'Cancel' button tooltip)
                int numOfTeams = node.CheckNumOfTeams();
                cancelText = string.Format("There {0} {1} team{2} present at the node", numOfTeams == 1 ? "is" : "are", numOfTeams,
                    numOfTeams != 1 ? "s" : "");
                //
                // - - -  Recall Team - - -
                //
                //Does the Node have any teams present? -> added first
                if (node.CheckNumOfTeams() > 0)
                {
                    //get list of teams
                    List<Team> listOfTeams = node.GetTeams();
                    if (listOfTeams != null)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach(Team team in listOfTeams)
                        {
                            if (builder.Length > 0) { builder.AppendLine(); }
                            builder.AppendFormat("{0}{1} {2}{3}", colourNeutral, team.arc.name, team.teamName, colourEnd);
                        }
                        //button details
                        EventButtonDetails recallDetails = new EventButtonDetails()
                        {
                            buttonTitle = "Recall Team",
                            buttonTooltipHeader = string.Format("{0}Recall Team{1}", sideColour, colourEnd),
                            buttonTooltipMain = "The following teams can be withdrawn early",
                            buttonTooltipDetail = builder.ToString(),
                            //use a Lambda to pass arguments to the action
                            action = () => { EventManager.instance.PostNotification(EventType.RecallTeamAction, this, node.nodeID, "ActorManager.cs -> GetNodeActions"); }
                        };
                        tempList.Add(recallDetails);
                    }
                    else { Debug.LogError(string.Format("Invalid listOfTeams (Null) for Node {0} \"{1}\", ID {2}", node.Arc.name, node.nodeName, node.nodeID)); }
                }

                //get a list pre-emptively as it's computationally expensive to do so on demand
                List<string> tempTeamList = GameManager.instance.dataScript.GetAvailableReserveTeams(node);
                arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalAuthority);
                //loop actors currently in game -> get Node actions (1 per Actor, if valid criteria)
                foreach (Actor actor in arrayOfActors)
                {
                    proceedFlag = true;
                    details = null;
                    isAnyTeam = false;
                    //correct side?
                    if (actor.side.level == playerSide.level)
                    {
                        //actor active?
                        if (actor.Status == ActorStatus.Active)
                        {
                            //assign preferred team as default (doesn't matter if actor has ANY Team action)
                            teamArcID = actor.arc.preferredTeam.TeamArcID;
                            tempAction = null;
                            //active node for actor
                            if (GameManager.instance.levelScript.CheckNodeActive(node.nodeID, GameManager.instance.sideScript.PlayerSide, actor.actorSlotID) == true)
                            {
                                //get ANY TEAM node action
                                actionID = GameManager.instance.dataScript.GetActionID("Any Team");
                                if (actionID > -1)
                                {
                                    tempAction = GameManager.instance.dataScript.GetAction(actionID);
                                    isAnyTeam = true;
                                }
                            }
                            //actor not live at node -> Preferred team
                            else
                            { tempAction = actor.arc.nodeAction; }
                            //default main tooltip text body
                            tooltipMain = tempAction.tooltipText;
                            //tweak if ANY TEAM
                            if (isAnyTeam == true)
                            { tooltipMain = string.Format("{0} as the {1} has Influence here", tooltipMain, GameManager.instance.metaScript.GetAuthorityTitle()); }
                            //valid action?
                            if (tempAction != null)
                            {
                                //effects
                                StringBuilder builder = new StringBuilder();
                                listOfEffects = tempAction.listOfEffects;
                                if (listOfEffects.Count > 0)
                                {
                                    for (int i = 0; i < listOfEffects.Count; i++)
                                    {
                                        Effect effect = listOfEffects[i];
                                        //check effect criteria is valid
                                        CriteriaDataInput criteriaInput = new CriteriaDataInput()
                                        {
                                            nodeID = nodeID,
                                            teamArcID = teamArcID,
                                            actorSlotID = actor.actorSlotID,
                                            listOfCriteria = effect.listOfCriteria
                                        };
                                        effectCriteria = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                                        if (effectCriteria == null)
                                        {
                                            //Effect criteria O.K -> tool tip text
                                            if (builder.Length > 0) { builder.AppendLine(); }
                                            if (effect.outcome.name.Equals("Renown") == false)
                                            {
                                                //sort out colour, remember effect.typeOfEffect is from POV of resistance so good will be bad for authority
                                                if (effect.typeOfEffect != null)
                                                {
                                                    switch (effect.typeOfEffect.name)
                                                    {
                                                        case "Good":
                                                            builder.AppendFormat("{0}{1}{2}", colourBad, effect.textTag, colourEnd);
                                                            break;
                                                        case "Neutral":
                                                            builder.AppendFormat("{0}{1}{2}", colourNeutral, effect.textTag, colourEnd);
                                                            break;
                                                        case "Bad":
                                                            builder.AppendFormat("{0}{1}{2}", colourGood, effect.textTag, colourEnd);
                                                            break;
                                                        default:
                                                            builder.AppendFormat("{0}{1}{2}", colourDefault, effect.textTag, colourEnd);
                                                            Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    builder.Append(effect.textTag);
                                                    Debug.LogWarning(string.Format("Invalid effect.typeOfEffect (Null) for \"{0}\"", effect.name));
                                                }
                                                //if an ANY TEAM action then display available teams
                                                if (isAnyTeam == true)
                                                {
                                                    foreach (string teamName in tempTeamList)
                                                    {
                                                        builder.AppendLine();
                                                        builder.AppendFormat("{0}{1}{2}", colourNeutral, teamName, colourEnd);
                                                    }
                                                }
                                            }
                                            //actor automatically accumulates renown for their faction
                                            else
                                            { builder.AppendFormat("{0}{1} {2}{3}", colourAuthority, actor.arc.name, effect.textTag, colourEnd); }

                                        }
                                        else
                                        {
                                            //invalid effect criteria -> Action cancelled
                                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                            infoBuilder.AppendFormat("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                colourInvalid, actor.arc.name, "\n", colourEnd,
                                                colourAuthority, effectCriteria, colourEnd);
                                            proceedFlag = false;
                                        }
                                    }
                                }
                                else
                                { Debug.LogWarning(string.Format("Action \"{0}\" has no effects", tempAction)); }
                                if (proceedFlag == true)
                                {
                                    //Details to pass on for processing via button click
                                    ModalActionDetails actionDetails = new ModalActionDetails() { };
                                    actionDetails.side = globalAuthority;
                                    actionDetails.nodeID = nodeID;
                                    actionDetails.actorDataID = actor.actorSlotID;
                                    //Node action is standard but other actions are possible
                                    UnityAction clickAction = null;
                                    //Team action
                                    if (isAnyTeam)
                                    { clickAction = () => { EventManager.instance.PostNotification(EventType.InsertTeamAction, this, actionDetails, "ActorManager.cs -> GetNodeActions"); };  }
                                    //Node action
                                    else
                                    { clickAction = () => { EventManager.instance.PostNotification(EventType.NodeAction, this, actionDetails, "ActorManager.cs -> GetNodeActions"); };  }
                                    //pass all relevant details to ModalActionMenu via Node.OnClick()
                                    details = new EventButtonDetails()
                                    {
                                        buttonTitle = tempAction.name,
                                        buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                        buttonTooltipMain = tooltipMain,
                                        buttonTooltipDetail = builder.ToString(),
                                        //use a Lambda to pass arguments to the action
                                        action = clickAction
                                    };
                                }
                            }
                            else
                            {
                                Debug.LogError(string.Format("{0}, slotID {1} has no valid action", actor.arc.name, actor.actorSlotID));
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                infoBuilder.AppendFormat("{0} is having a bad day", actor.arc.name);
                            }
                        }
                        else
                        {
                            //actor gone silent
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.AppendFormat("{0} is lying low and unavailale", actor.arc.name);
                        }
                        //add to list
                        if (details != null)
                        { tempList.Add(details); }
                    }
                }
            }
            //
            // - - - Cancel
            //
            //Cancel button is added last
            EventButtonDetails cancelDetails = null;
            if (infoBuilder.Length > 0)
            {
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = cancelText,
                    buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, infoBuilder.ToString(), colourEnd),
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetNodeActions"); }
                };
            }
            else
            {
                //necessary to prevent color tags triggering the bottom divider in TooltipGeneric
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = cancelText,
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetNodeActions"); }
                };
            }
            //add Cancel button to list
            tempList.Add(cancelDetails);
        }
        else { Debug.LogError(string.Format("Invalid Node (null), ID {0}{1}", nodeID, "\n")); }
        return tempList;
    }


    /// <summary>
    /// Returns a list of all relevant Actor Actions for the actor to enable a ModalActionMenu to be put together (one button per action). 
    /// Resistance only -> up to 3 x 'Give (or Take) Gear to Actor', 1 x'Activate' / Lie Low', 1 x 'Manage' and an automatic Cancel button (6 total)
    /// triggered when you Right Click an Actor's portrait
    /// </summary>
    /// <param name="actorSlotID"></param>
    /// <returns></returns>
    public List<EventButtonDetails> GetActorActions(int actorSlotID)
    {
        string sideColour, tooltipText, title;
        string cancelText = null;
        int benefit;
        int numOfTurns;
        bool isResistance;
        bool proceedFlag;
        bool isGearToGive = false;
        AuthoritySecurityState securityState = GameManager.instance.turnScript.authoritySecurityState;
        //return list of button details
        List<EventButtonDetails> tempList = new List<EventButtonDetails>();
        //Cancel button tooltip (handles all no go cases)
        StringBuilder infoBuilder = new StringBuilder();
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"ss
        if (playerSide.level == globalAuthority.level)
        { sideColour = colourAuthority; isResistance = false; }
        else { sideColour = colourResistance; isResistance = true; }
        //get actor
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, playerSide);
        //if actor is Null, a single button (Cancel) menu is still provided
        if (actor != null)
        {
            title = string.Format("{0}", isResistance ? "" : GameManager.instance.metaScript.GetAuthorityTitle().ToString() + " ");
            cancelText = string.Format("{0} {1}", actor.arc.name, actor.actorName);
            switch (actor.Status)
            {
                case ActorStatus.Active:
                    //
                    // - - - Manage (both sides) - - - 
                    //
                    ModalActionDetails manageActionDetails = new ModalActionDetails() { };
                    manageActionDetails.side = playerSide;
                    manageActionDetails.actorDataID = actor.actorSlotID;
                    tooltipText = string.Format("Select to choose what to do with {0} (send to the Reserve Pool, Dismiss or Dispose Off)", actor.actorName);
                    EventButtonDetails dismissDetails = new EventButtonDetails()
                    {
                        buttonTitle = "MANAGE",
                        buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                        buttonTooltipMain = string.Format("Unfortunately {0}{1}{2} {3}{4}  isn't working out", colourNeutral, actor.arc.name, colourEnd,
                        title, actor.actorName),
                        buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                        //use a Lambda to pass arguments to the action
                        action = () => { EventManager.instance.PostNotification(EventType.ManageActorAction, this, manageActionDetails, "ActorManager.cs -> GetActorActions"); }
                    };
                    //add Manage button to list
                    tempList.Add(dismissDetails);
                    //
                    // - - - Resistance - - -
                    //
                    if (isResistance == true)
                    {
                        //
                        // - - - Lie Low - - -
                        //
                        //Invisibility must be less than max
                        if (actor.datapoint2 < maxStatValue)
                        {
                            //can't lie low if a Surveillance Crackdown is in place
                            if (securityState != AuthoritySecurityState.SurveillanceCrackdown)
                            {
                                ModalActionDetails lielowActionDetails = new ModalActionDetails() { };
                                lielowActionDetails.side = playerSide;
                                lielowActionDetails.actorDataID = actor.actorSlotID;
                                if (actor.isLieLowFirstturn == true)
                                { numOfTurns = 4 - actor.datapoint2; }
                                else { numOfTurns = 3 - actor.datapoint2; }
                                tooltipText = string.Format("{0} will regain Invisibility and automatically reactivate in {1}{2} FULL turn{3}{4}", actor.actorName,
                                    colourNeutral, numOfTurns, numOfTurns != 1 ? "s" : "", colourEnd);
                                EventButtonDetails lielowDetails = new EventButtonDetails()
                                {
                                    buttonTitle = "Lie Low",
                                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                                    buttonTooltipMain = string.Format("{0} {1} will be asked to keep a low profile and stay out of sight", actor.arc.name, actor.actorName),
                                    buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                                    //use a Lambda to pass arguments to the action
                                    action = () => { EventManager.instance.PostNotification(EventType.LieLowActorAction, this, lielowActionDetails, "ActorManager.cs -> GetActorActions"); }
                                };
                                //add Lie Low button to list
                                tempList.Add(lielowDetails);
                            }
                            else
                            { infoBuilder.Append("Can't Lie Low while a Surveillance Crackdown is in force"); }
                        }
                        else
                        {
                            //actor invisiblity at max
                            infoBuilder.AppendFormat("{0} Invisibility at Max and can't Lie Low", actor.arc.name);
                        }
                        //
                        // - - - Give Gear - - -
                        //
                        int numOfGear = GameManager.instance.playerScript.CheckNumOfGear();
                        if (numOfGear > 0)
                        {
                            List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
                            if (listOfGear != null)
                            {
                                //loop gear and create a button for each
                                for (int i = 0; i < numOfGear; i++)
                                {
                                    Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                                    if (gear != null)
                                    {
                                        //can't give gear if it's already been used this turn
                                        if (gear.timesUsed == 0)
                                        {
                                            StringBuilder builderTooltip = new StringBuilder();
                                            ModalActionDetails gearActionDetails = new ModalActionDetails() { };
                                            gearActionDetails.side = playerSide;
                                            gearActionDetails.actorDataID = actor.actorSlotID;
                                            gearActionDetails.gearID = gear.gearID;
                                            //get actor's preferred gear
                                            GearType preferredGear = actor.arc.preferredGear;
                                            if (preferredGear != null)
                                            {
                                                benefit = gearSwapBaseAmount;
                                                if (preferredGear.name.Equals(gear.type.name) == true)
                                                {
                                                    benefit += gearSwapPreferredAmount;
                                                    builderTooltip.AppendFormat("Preferred Gear for {0}{1}{2}{3} Motivation +{4}{5}",
                                                      actor.arc.name, "\n", colourGood, actor.arc.name, benefit, colourEnd);
                                                }
                                                else
                                                {
                                                    builderTooltip.AppendFormat("NOT Preferred Gear (prefers {0}{1}{2}){3}{4}{5} Motivation +{6}{7}", colourDefault,
                                                      preferredGear.name, colourEnd, "\n", colourGood, actor.arc.name, benefit, colourEnd);
                                                }
                                            }
                                            else
                                            {
                                                builderTooltip.Append("Unknown Preferred Gear");
                                                Debug.LogError(string.Format("Invalid preferredGear (Null) for actor Arc {0}", actor.arc.name));
                                            }

                                            //traits
                                            if (actor.CheckTraitEffect(actorKeepGear) == true)
                                            {
                                                builderTooltip.AppendFormat("{0}{1} has {2}{3}{4} trait{5}{6}(Refuses to return Gear){7}", "\n", actor.arc.name, colourNeutral,
                                                    actor.GetTrait().tag, colourEnd, "\n", colourBad, colourEnd);
                                            }

                                            //existing gear
                                            if (actor.GetGearID() > -1)
                                            {
                                                Gear gearOld = GameManager.instance.dataScript.GetGear(actor.GetGearID());
                                                if (gearOld != null)
                                                { builderTooltip.AppendFormat("{0}{1}{2}{3}{4} will be Lost{5}", "\n", colourNeutral, gearOld.name, colourEnd, colourBad, colourEnd); }
                                                else { Debug.LogWarningFormat("Invalid gearOld (Null) for gearID {0}", actor.GetGearID()); }
                                            }
                                            EventButtonDetails gearDetails = new EventButtonDetails()
                                            {
                                                buttonTitle = string.Format("Give {0}", gear.name),
                                                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                                                buttonTooltipMain = string.Format("Give {0}{1}{2} ({3}) to {4}{5}{6}, {7}", colourNeutral, gear.name, colourEnd, gear.type.name,
                                                 colourCancel, actor.arc.name, colourEnd, actor.actorName),
                                                buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, builderTooltip.ToString(), colourEnd),
                                                //use a Lambda to pass arguments to the action
                                                action = () => { EventManager.instance.PostNotification(EventType.GiveGearAction, this, gearActionDetails, "ActorManager.cs -> GetActorActions"); }
                                            };
                                            //add give gear button to list
                                            tempList.Add(gearDetails);
                                            isGearToGive = true;
                                        }
                                        else
                                        {
                                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                            infoBuilder.AppendFormat("Can't gift {0}{1}{2}{3}{4}(used this turn){5}", colourNeutral, gear.name, colourEnd, "\n", colourBad, colourEnd);
                                        }
                                    }
                                    else { Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
                                }
                            }
                            else { Debug.LogError("Invalid listOfGear (Null)"); }
                        }
                        //no gear to give
                        if (isGearToGive == false)
                        {
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.Append("You have no Gear to give");
                        }
                        //
                        // - - - Take Gear - - -
                        //
                        int actorGearID = actor.GetGearID();
                        //Player must have at least one free slot
                        if (numOfGear < maxNumOfGear)
                        {
                            //actor must have an item of gear
                            if (actorGearID > -1)
                            {
                                //traits
                                if (actor.CheckTraitEffect(actorKeepGear) == false)
                                {
                                    //grace period must have expired
                                    if (actor.GetGearTimer() > gearGracePeriod)
                                    {
                                        Gear gearActor = GameManager.instance.dataScript.GetGear(actorGearID);
                                        if (gearActor != null)
                                        {
                                            benefit = gearSwapBaseAmount;
                                            StringBuilder builder = new StringBuilder();
                                            //data package
                                            ModalActionDetails gearActionDetails = new ModalActionDetails() { };
                                            gearActionDetails.side = playerSide;
                                            gearActionDetails.actorDataID = actor.actorSlotID;
                                            gearActionDetails.gearID = gearActor.gearID;
                                            //get actor's preferred gear
                                            GearType preferredGear = actor.arc.preferredGear;
                                            if (preferredGear != null)
                                            {
                                                if (preferredGear.name.Equals(gearActor.type.name) == true)
                                                {
                                                    benefit += gearSwapPreferredAmount;
                                                    builder.AppendFormat("Preferred Gear for {0}{1}{2}{3} Motivation -{4}{5}",
                                                      actor.arc.name, "\n", colourBad, actor.arc.name, benefit, colourEnd);
                                                }
                                                else
                                                {
                                                    builder.AppendFormat("NOT Preferred Gear (prefers {0}{1}{2}){3}{4}{5} Motivation -{6}{7}", colourNeutral,
                                                      preferredGear.name, colourEnd, "\n", colourBad, actor.arc.name, benefit, colourEnd);
                                                }
                                            }
                                            else
                                            {
                                                builder.Append("Unknown Preferred Gear");
                                                Debug.LogError(string.Format("Invalid preferredGear (Null) for actor Arc {0}", actor.arc.name));
                                            }
                                            //relationship conflict
                                            if (actor.datapoint1 < benefit)
                                            {
                                                builder.AppendFormat("{0}{1} Motivation too Low!{2}", "\n", colourAlert, colourEnd);
                                                builder.AppendFormat("{0}{1}RELATIONSHIP CONFLICT{2}", "\n", colourBad, colourEnd);
                                                builder.AppendFormat("{0}{1}If you Take Gear{2}", "\n", colourAlert, colourEnd);
                                            }
                                            //button package
                                            EventButtonDetails gearDetails = new EventButtonDetails()
                                            {
                                                buttonTitle = string.Format("<b>TAKE</b> {0}", gearActor.name),
                                                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                                                buttonTooltipMain = string.Format("<b>TAKE</b> {0}{1}{2} ({3}) from {4}{5}{6}, {7}", colourNeutral, gearActor.name, colourEnd, gearActor.type.name,
                                                 colourCancel, actor.arc.name, colourEnd, actor.actorName),
                                                buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, builder.ToString(), colourEnd),
                                                //use a Lambda to pass arguments to the action
                                                action = () => { EventManager.instance.PostNotification(EventType.TakeGearAction, this, gearActionDetails, "ActorManager.cs -> GetActorActions"); }
                                            };
                                            //add give gear button to list
                                            tempList.Add(gearDetails);
                                            isGearToGive = true;
                                        }
                                        else { Debug.LogWarningFormat("Invalid gearActor (Null) for gearID {0}", actorGearID); }
                                    }
                                    else
                                    {
                                        //grace period still in force
                                        if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                        infoBuilder.AppendFormat("Can't TAKE gear{0}{1}(within Grace Period){2}", "\n", colourBad, colourEnd);
                                    }
                                }
                                else
                                {
                                    //actor has trait (Pack Rat) causing him to refuse to hand over gear
                                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                    infoBuilder.AppendFormat("Can't TAKE gear{0}{1}({2}{3} {4}{5}{6}{7} trait){8}", "\n", colourBad, actor.arc.name, colourEnd,
                                        colourNeutral, actor.GetTrait().tag, colourEnd, colourBad, colourEnd);
                                    TraitLogMessage(actor, "to Refuse to handback Gear");
                                }
                            }
                            else
                            {
                                //actor has no gear to give
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                { infoBuilder.AppendFormat("{0} has no gear to Take", actor.arc.name); }
                            }
                        }
                        else
                        {
                            //player has no spare slot to put gear
                            if (actorGearID > -1)
                            {
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                { infoBuilder.AppendFormat("Can't Take Gear{0}{1}(No space){2}", "\n", colourBad, colourEnd); }
                            }
                        }

                    }
                    break;
                //
                // - - - Actor Inactive - - -
                //
                case ActorStatus.Inactive:
                    //
                    // - - - Resistance - - -
                    //
                    if (isResistance == true)
                    {
                        proceedFlag = true;
                        //won't activate if spooked & security measures in place
                        if (actor.CheckTraitEffect(actorNoActionsDuringSecurityMeasures) == true && securityState != AuthoritySecurityState.Normal)
                        {
                            proceedFlag = false;
                            TraitLogMessage(actor, "and is prevented from Activating (Lying Low)");
                        }
                        if (proceedFlag == true)
                        {
                            //
                            // - - - Activate - - -
                            //
                            ModalActionDetails activateActionDetails = new ModalActionDetails() { };
                            activateActionDetails.side = playerSide;
                            activateActionDetails.actorDataID = actor.actorSlotID;
                            if (actor.isLieLowFirstturn == true)
                            { numOfTurns = 4 - actor.datapoint2; }
                            else { numOfTurns = 3 - actor.datapoint2; }
                            tooltipText = string.Format("{0} is Lying Low and will automatically return in {1} turn{2} if not Activated", actor.actorName, numOfTurns,
                                numOfTurns != 1 ? "s" : "");
                            EventButtonDetails activateDetails = new EventButtonDetails()
                            {
                                buttonTitle = "Activate",
                                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                                buttonTooltipMain = string.Format("{0} {1} will be Immediately Recalled", actor.arc.name, actor.actorName),
                                buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                                //use a Lambda to pass arguments to the action
                                action = () => { EventManager.instance.PostNotification(EventType.ActivateActorAction, this, activateActionDetails, "ActorManager.cs -> GetActorActions"); }
                            };
                            //add Activate button to list
                            tempList.Add(activateDetails);
                        }
                        else
                        { infoBuilder.AppendFormat("Won't Activate as {0}Spooked{1} (Trait) due to Security Measures", colourNeutral, colourEnd); }
                    }
                    break;
                //
                // - - - Actor Captured or other - - -
                //
                default:
                    cancelText = string.Format("{0}Actor is \"{1}\" and out of contact{2}", colourBad, actor.Status, colourEnd);
                    break;
            }
        }
        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", actorSlotID)); }

        //Debug
        if (string.IsNullOrEmpty(cancelText)) { cancelText = "Unknown"; }
        //
        // - - - Cancel - - - (both sides)
        //
        //Cancel button is added last
        EventButtonDetails cancelDetails = null;
        if (actor != null)
        {
            if (infoBuilder.Length > 0)
            {
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = cancelText,
                    buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, infoBuilder.ToString(), colourEnd),
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetActorActions"); }
                };
            }
            else
            {
                //necessary to prevent color tags triggering the bottom divider in TooltipGeneric
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = cancelText,
                    buttonTooltipDetail = string.Format("{0}Press Cancel to exit{1}", colourCancel, colourEnd),
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetActorActions"); }
                };
            }
        }
        else
        {
            //Null actor -> invalid menu creation
            cancelDetails = new EventButtonDetails()
            {
                buttonTitle = "CANCEL",
                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                buttonTooltipMain = string.Format("{0}Invalid Actor{1}",colourBad, colourEnd),
                //use a Lambda to pass arguments to the action
                action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetActorActions"); }
            };
        }
        //add Cancel button to list
        tempList.Add(cancelDetails);

        return tempList;
    }

    /// <summary>
    /// Right click Player sprite Action menu
    /// </summary>
    /// <returns></returns>
    public List<EventButtonDetails> GetPlayerActions()
    {
        string sideColour, colourEffect, tooltipText, effectCriteria;
        string playerName = GameManager.instance.playerScript.PlayerName;
        int numOfTurns;
        int invis = GameManager.instance.playerScript.Invisibility;
        bool isResistance;
        bool proceedFlag = false;
        AuthoritySecurityState securityState = GameManager.instance.turnScript.authoritySecurityState;
        //return list of button details
        List<EventButtonDetails> tempList = new List<EventButtonDetails>();
        //Cancel button tooltip (handles all no go cases)
        StringBuilder infoBuilder = new StringBuilder();
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"ss
        if (playerSide.level == globalAuthority.level)
        { sideColour = colourAuthority; isResistance = false; }
        else { sideColour = colourResistance; isResistance = true; }
        //
        // - - - Resistance - - -
        //
        if (isResistance == true)
        {
            switch (GameManager.instance.playerScript.status)
            {
                case ActorStatus.Active:
                    //
                    // - - - Use Gear (Personal) - - -
                    //
                    List<int> listOfGearID = GameManager.instance.playerScript.GetListOfGear();
                    if (listOfGearID != null)
                    {
                        //has gear?
                        if (listOfGearID.Count > 0)
                        {
                            for (int index = 0; index < listOfGearID.Count; index++)
                            {
                                Gear gear = GameManager.instance.dataScript.GetGear(listOfGearID[index]);
                                if (gear != null)
                                {
                                    //Personal Use gear?
                                    if (gear.listOfPersonalEffects != null && gear.listOfPersonalEffects.Count > 0)
                                    {
                                        //create a menu option to use gear
                                        List<Effect> listOfEffects = gear.listOfPersonalEffects;
                                        if (listOfEffects != null)
                                        {
                                            //effects
                                            StringBuilder builder = new StringBuilder();
                                            if (listOfEffects.Count > 0)
                                            {
                                                for (int i = 0; i < listOfEffects.Count; i++)
                                                {
                                                    proceedFlag = true;
                                                    colourEffect = colourDefault;
                                                    Effect effect = listOfEffects[i];
                                                    //colour code effects according to type
                                                    if (effect.typeOfEffect != null)
                                                    {
                                                        switch (effect.typeOfEffect.name)
                                                        {
                                                            case "Good":
                                                                colourEffect = colourGood;
                                                                break;
                                                            case "Neutral":
                                                                colourEffect = colourNeutral;
                                                                break;
                                                            case "Bad":
                                                                colourEffect = colourBad;
                                                                break;
                                                        }
                                                    }
                                                    //check effect criteria is valid
                                                    CriteriaDataInput criteriaInput = new CriteriaDataInput()
                                                    {
                                                        listOfCriteria = effect.listOfCriteria
                                                    };
                                                    effectCriteria = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                                                    if (effectCriteria == null)
                                                    {
                                                        //Effect criteria O.K -> tool tip text
                                                        if (builder.Length > 0) { builder.AppendLine(); }
                                                        builder.AppendFormat("{0}{1}{2}", colourEffect, effect.textTag, colourEnd);
                                                        //chance of compromise
                                                        int compromiseChance = GameManager.instance.gearScript.GetChanceOfCompromise(gear.gearID);
                                                        builder.AppendFormat("{0}{1}Chance of Gear being Compromised {2}{3}{4}%{5}", "\n", colourAlert, colourEnd,
                                                            colourNeutral, compromiseChance, colourEnd);
                                                    }
                                                    else
                                                    {
                                                        proceedFlag = false;
                                                        //invalid effect criteria -> Action cancelled
                                                        if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                        infoBuilder.AppendFormat("{0}USE {1} invalid{2}{3}{4}({5}){6}{7}",
                                                            colourInvalid, gear.name, colourEnd, "\n", colourBad, effectCriteria, colourEnd, "\n");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                proceedFlag = false;
                                                infoBuilder.AppendFormat("USE Action Invalid{0}{1}(None for this Gear){2}", "\n", colourBad, colourEnd);
                                            }
                                            //button 
                                            if (proceedFlag == true)
                                            {
                                                ModalActionDetails gearActionDetails = new ModalActionDetails() { };
                                                gearActionDetails.side = globalResistance;
                                                gearActionDetails.gearID = gear.gearID;
                                                EventButtonDetails gearDetails = new EventButtonDetails()
                                                {
                                                    buttonTitle = string.Format("Use {0}", gear.name),
                                                    buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, "INFO", colourEnd),
                                                    buttonTooltipMain = string.Format("Use {0} (Player)", gear.name),
                                                    /*buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, builder.ToString(), colourEnd),*/
                                                    buttonTooltipDetail = builder.ToString(),
                                                    //use a Lambda to pass arguments to the action
                                                    action = () => { EventManager.instance.PostNotification(EventType.UseGearAction, this, gearActionDetails, "ActorManager.cs -> GetPlayerAction"); }
                                                };
                                                //add USE to list
                                                tempList.Add(gearDetails);
                                            }
                                        }
                                        else
                                        { infoBuilder.AppendFormat("USE Action Invalid{0}{1}(None for this Gear){2}", "\n", colourBad, colourEnd); }
                                    }
                                }
                                else { Debug.LogWarningFormat("Invalid gear (Null) for gearID {0}", listOfGearID[index]); }
                            }
                        }
                    }
                    else { Debug.LogWarning("Invalid listOfGearID (Null)"); }

                    //
                    // - - - Lie Low - - -
                    //
                    //Invisibility must be less than max
                    if (invis < maxStatValue)
                    {
                        //can't lie low if a Surveillance Crackdown is in place
                        if (securityState != AuthoritySecurityState.SurveillanceCrackdown)
                        {
                            ModalActionDetails lielowActionDetails = new ModalActionDetails() { };
                            lielowActionDetails.side = playerSide;
                            lielowActionDetails.actorDataID = GameManager.instance.playerScript.actorID;
                            numOfTurns = 3 - invis;
                            tooltipText = string.Format("{0} will regain Invisibility and automatically reactivate in {1}{2} FULL turn{3}{4}", playerName,
                                colourNeutral, numOfTurns, numOfTurns != 1 ? "s" : "", colourEnd);
                            EventButtonDetails lielowDetails = new EventButtonDetails()
                            {
                                buttonTitle = "Lie Low",
                                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                                buttonTooltipMain = string.Format("{0} will keep a low profile and stay out of sight", playerName),
                                buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                                //use a Lambda to pass arguments to the action
                                action = () => { EventManager.instance.PostNotification(EventType.LieLowPlayerAction, this, lielowActionDetails, "ActorManager.cs -> GetPlayerActions"); }
                            };
                            //add Lie Low button to list
                            tempList.Add(lielowDetails);
                        }
                        else
                        { infoBuilder.AppendFormat("{0}Surveillance Crackdown{1}{2}{3}(can't Lie Low){4}", colourAlert, colourEnd, "\n", colourBad, colourEnd); }
                    }
                    else
                    {
                        //actor invisiblity at max
                        infoBuilder.AppendFormat("{0}Can't Lie Low{1}{2}{3}(Invisibility at Max){4}", colourAlert, colourEnd, "\n", colourBad, colourEnd);
                    }
                    break;
                case ActorStatus.Inactive:
                    //
                    // - - - Activate - - -
                    //
                    //can't activate if a Surveillance Crackdown is in place
                    if (securityState != AuthoritySecurityState.SurveillanceCrackdown)
                    {
                        ModalActionDetails activateActionDetails = new ModalActionDetails() { };
                        activateActionDetails.side = playerSide;
                        activateActionDetails.actorDataID = GameManager.instance.playerScript.actorID;
                        numOfTurns = 3 - invis;
                        tooltipText = string.Format("{0} is Lying Low and will automatically return in {1} turn{2} if not Activated", playerName, numOfTurns,
                            numOfTurns != 1 ? "s" : "");
                        EventButtonDetails activateDetails = new EventButtonDetails()
                        {
                            buttonTitle = "Activate",
                            buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                            buttonTooltipMain = string.Format("{0} will be Immediately Recalled", playerName),
                            buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                            //use a Lambda to pass arguments to the action
                            action = () => { EventManager.instance.PostNotification(EventType.ActivatePlayerAction, this, activateActionDetails, "ActorManager.cs -> GetPlayerActions"); }
                        };
                        //add Activate button to list
                        tempList.Add(activateDetails);
                    }
                    else
                    { infoBuilder.AppendFormat("{0}Surveillance Crackdown{1}{2}{3}(can't Activate){4}", colourAlert, colourEnd, "\n", colourBad, colourEnd); }
                    break;
            }
        }
        //
        // - - - Cancel - - - (both sides)
        //
        //Cancel button is added last
        EventButtonDetails cancelDetails = null;
        if (infoBuilder.Length > 0)
        {
            cancelDetails = new EventButtonDetails()
            {
                buttonTitle = "CANCEL",
                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                buttonTooltipMain = playerName,
                buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, infoBuilder.ToString(), colourEnd),
                //use a Lambda to pass arguments to the action
                action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetPlayerActions"); }
            };
        }
        else
        {
            //necessary to prevent color tags triggering the bottom divider in TooltipGeneric
            cancelDetails = new EventButtonDetails()
            {
                buttonTitle = "CANCEL",
                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                buttonTooltipMain = playerName,
                buttonTooltipDetail = string.Format("{0}Press Cancel to exit{1}", colourCancel, colourEnd),
                //use a Lambda to pass arguments to the action
                action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetPlayerActions"); }
            };
        }
        //add Cancel button to list
        tempList.Add(cancelDetails);
        return tempList;
    }


    /// <summary>
    /// Returns a list of all relevant actions for Gear in the player's Inventory (right click gear sprite in inventory)
    /// Resistance only -> up to 4 x 'Give Gear to Actor', 1 x 'Use' (if there is a viable use gear action), 1 x Cancel
    /// </summary>
    public List<EventButtonDetails> GetGearInventoryActions(int gearID)
    {            
        //return list of button details
        List<EventButtonDetails> eventList = new List<EventButtonDetails>();            
        //Cancel button tooltip (handles all no go cases)
        StringBuilder infoBuilder = new StringBuilder();
        string effectCriteria, colourEffect;
        string cancelText = null;
        bool proceedFlag = false;
        int benefit;
        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
        if (gear != null)
        {
            //
            // - - - Use - - -
            //
            List<Effect> listOfEffects = gear.listOfPersonalEffects;
            if (listOfEffects != null)
            {
                //effects
                StringBuilder builder = new StringBuilder();
                if (listOfEffects.Count > 0)
                {
                    for (int i = 0; i < listOfEffects.Count; i++)
                    {
                        proceedFlag = true;
                        colourEffect = colourDefault;
                        Effect effect = listOfEffects[i];
                        //colour code effects according to type
                        if (effect.typeOfEffect != null)
                        {
                            switch (effect.typeOfEffect.name)
                            {
                                case "Good":
                                    colourEffect = colourGood;
                                    break;
                                case "Neutral":
                                    colourEffect = colourNeutral;
                                    break;
                                case "Bad":
                                    colourEffect = colourBad;
                                    break;
                            }
                        }
                        //check effect criteria is valid
                        CriteriaDataInput criteriaInput = new CriteriaDataInput()
                        {
                            listOfCriteria = effect.listOfCriteria
                        };
                        effectCriteria = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                        if (effectCriteria == null)
                        {
                            //Effect criteria O.K -> tool tip text
                            if (builder.Length > 0) { builder.AppendLine(); }
                            builder.AppendFormat("{0}{1}{2}", colourEffect, effect.textTag, colourEnd);
                            //chance of compromise
                            int compromiseChance = GameManager.instance.gearScript.GetChanceOfCompromise(gear.gearID);
                            builder.AppendFormat("{0}{1}Chance of Gear being Compromised {2}{3}{4}%{5}", "\n", colourAlert, colourEnd, 
                                colourNeutral, compromiseChance, colourEnd);
                        }
                        else
                        {
                            proceedFlag = false;
                            //invalid effect criteria -> Action cancelled
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.AppendFormat("{0}USE action invalid{1}{2}{3}({4}){5}",
                                colourInvalid, colourEnd, "\n", colourBad, effectCriteria, colourEnd);
                        }
                    }
                }
                else
                {
                    proceedFlag = false;
                    infoBuilder.AppendFormat("USE Action Invalid{0}{1}(None for this Gear){2}", "\n", colourBad, colourEnd);
                }


                //button 
                if (proceedFlag == true)
                {
                    ModalActionDetails gearActionDetails = new ModalActionDetails() { };
                    gearActionDetails.side = globalResistance;
                    gearActionDetails.gearID = gear.gearID;
                    gearActionDetails.modalLevel = 2;
                    gearActionDetails.modalState = ModalState.Inventory;
                    gearActionDetails.handler = GameManager.instance.inventoryScript.RefreshInventoryUI;
                    EventButtonDetails gearDetails = new EventButtonDetails()
                    {
                        buttonTitle = "Use",
                        buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, "INFO", colourEnd),
                        buttonTooltipMain = string.Format("Use {0} (Player)", gear.name),
                        /*buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, builder.ToString(), colourEnd),*/
                        buttonTooltipDetail = builder.ToString(),
                        //use a Lambda to pass arguments to the action
                        action = () => { EventManager.instance.PostNotification(EventType.UseGearAction, this, gearActionDetails, "ActorManager.cs -> GetGearInventory"); }
                    };
                    //add USE to list
                    eventList.Add(gearDetails);
                }
            }
            else
            { infoBuilder.AppendFormat("USE Action Invalid{0}{1}(None for this Gear){2}", "\n", colourBad, colourEnd); }
            //
            // - - - Give to - - -
            //
            cancelText = string.Format("{0} {1}", gear.type.name.ToUpper(), gear.name);
            if (gear.timesUsed == 0)
            {
                //Loop current, onMap actors
                Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
                if (arrayOfActors != null)
                {
                    for (int i = 0; i < arrayOfActors.Length; i++)
                    {
                        Actor actor = arrayOfActors[i];
                        if (actor != null)
                        {
                            //actor must be active
                            if (actor.Status == ActorStatus.Active)
                            {
                                StringBuilder builderTooltip = new StringBuilder();
                                ModalActionDetails gearActionDetails = new ModalActionDetails() { };
                                gearActionDetails.side = globalResistance;
                                gearActionDetails.actorDataID = actor.actorSlotID;
                                gearActionDetails.gearID = gear.gearID;
                                gearActionDetails.modalLevel = 2;
                                gearActionDetails.modalState = ModalState.Inventory;
                                gearActionDetails.handler = GameManager.instance.inventoryScript.RefreshInventoryUI;
                                //get actor's preferred gear
                                GearType preferredGear = actor.arc.preferredGear;
                                if (preferredGear != null)
                                {
                                    benefit = gearSwapBaseAmount;
                                    if (preferredGear.name.Equals(gear.type.name) == true)
                                    {
                                        benefit += gearSwapPreferredAmount;
                                        builderTooltip.AppendFormat("{0}Preferred Gear for {1}{2}{3}{4}{5} motivation +{6}{7}",
                                          colourNeutral, actor.arc.name, colourEnd, "\n", colourGood, actor.arc.name, benefit, colourEnd);
                                    }
                                    else
                                    {
                                        builderTooltip.AppendFormat("NOT Preferred Gear (prefers {0}{1}{2}){3}{4}{5} Motivation +{6}{7}", colourNeutral,
                                          preferredGear.name, colourEnd, "\n", colourGood, actor.arc.name, benefit, colourEnd);
                                    }
                                }
                                else
                                {
                                    builderTooltip.Append("Unknown Preferred Gear");
                                    Debug.LogError(string.Format("Invalid preferredGear (Null) for actor Arc {0}", actor.arc.name));
                                }
                                //existing gear
                                if (actor.GetGearID() > -1)
                                {
                                    Gear gearOld = GameManager.instance.dataScript.GetGear(actor.GetGearID());
                                    if (gearOld != null)
                                    { builderTooltip.AppendFormat("{0}{1}{2}{3}{4} will be Lost{5}", "\n", colourNeutral, gearOld.name, colourEnd, colourBad, colourEnd); }
                                    else { Debug.LogWarningFormat("Invalid gearOld (Null) for gearID {0}", actor.GetGearID()); }
                                }
                                EventButtonDetails gearDetails = new EventButtonDetails()
                                {
                                    buttonTitle = string.Format("Give to {0}", actor.arc.name),
                                    buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, "INFO", colourEnd),
                                    buttonTooltipMain = string.Format("Give {0} ({1}{2}{3}) to {4} {5}", gear.name, colourNeutral, gear.type.name,
                                    colourEnd, actor.arc.name, actor.actorName),
                                    buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, builderTooltip.ToString(), colourEnd),
                                    //use a Lambda to pass arguments to the action
                                    action = () => { EventManager.instance.PostNotification(EventType.GiveGearAction, this, gearActionDetails, "ActorManager.cs -> GetGearInventoryActions"); }
                                };
                                //add Lie Low button to list
                                eventList.Add(gearDetails);
                            }
                        }
                        else { Debug.LogError(string.Format("Invalid actor (Null) in arrayOfActors[{0}]", i)); }
                    }
                }
                else
                { Debug.LogError("Invalid arrayOfActors (Null)"); }
            }
            else
            {
                //gear has already been used this turn
                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                infoBuilder.AppendFormat("{0}{1}{2} can't be gifted{3}{4}(Used this turn){5}", colourNeutral, gear.name, colourEnd, "\n", colourBad, colourEnd);
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", gearID));
        }
        //Debug
        if (string.IsNullOrEmpty(cancelText)) { cancelText = "Unknown"; }
        //
        // - - - Cancel - - - (both sides)
        //
        //Cancel button is added last
        EventButtonDetails cancelDetails = null;
        if (gear != null)
        {
            if (infoBuilder.Length > 0)
            {
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}INFO{1}", colourResistance, colourEnd),
                    buttonTooltipMain = cancelText,
                    buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, infoBuilder.ToString(), colourEnd),
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetGearInventoryActions"); }
                };
            }
            else
            {
                //necessary to prevent color tags triggering the bottom divider in TooltipGeneric
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}INFO{1}", colourResistance, colourEnd),
                    buttonTooltipMain = cancelText,
                    buttonTooltipDetail = string.Format("{0}Press Cancel to Exit{1}", colourCancel, colourEnd),
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetGearInventoryActions"); }
                };
            }
        }
        else
        {
            //Null gear -> invalid menu creation
            cancelDetails = new EventButtonDetails()
            {
                buttonTitle = "CANCEL",
                buttonTooltipHeader = string.Format("{0}{1}{2}", globalResistance, "INFO", colourEnd),
                buttonTooltipMain = string.Format("{0}Invalid Gear{1}", colourBad, colourEnd),
                //use a Lambda to pass arguments to the action
                action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetGearInventoryActions"); }
            };
        }
        //add Cancel button to list
        eventList.Add(cancelDetails);
        return eventList;
    }


    /// <summary>
    /// Returns a list of all relevant actions an Actor in the Reserve Pool (right click actor sprite in inventory)
    /// Both sides -> 1 x Active Duty, 1 x Let Go / Fire (unhappy), 1 x Reassure,  1 x Cancel
    /// </summary>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public List<EventButtonDetails> GetReservePoolActions(int actorID)
    {
        //return list of button details
        List<EventButtonDetails> eventList = new List<EventButtonDetails>();
        //Cancel button tooltip (handles all no go cases)
        StringBuilder infoBuilder = new StringBuilder();
        string tooltipText, sideColour;
        string cancelText = null;
        int playerRenown = GameManager.instance.playerScript.Renown;
        int renownCost = 0;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"ss
        if (playerSide.level == globalAuthority.level)
        { sideColour = colourAuthority; }
        else { sideColour = colourResistance; }
        //data package for Action Menu
        ModalActionDetails actorActionDetails = new ModalActionDetails() { };
        actorActionDetails.side = playerSide;
        actorActionDetails.actorDataID = actorID;
        actorActionDetails.modalLevel = 2;
        actorActionDetails.modalState = ModalState.Inventory;
        actorActionDetails.handler = GameManager.instance.inventoryScript.RefreshInventoryUI;
        //actor
        Actor actor = GameManager.instance.dataScript.GetActor(actorID);
        if (actor != null)
        {
            cancelText = string.Format("{0} {1}", actor.arc.name, actor.actorName);
            //
            // - - - Active Duty - - -
            //
            int actorSlotID = GameManager.instance.dataScript.CheckForSpareActorSlot(playerSide);
            if (actorSlotID > -1)
            {
                //can't have 2 actors of the same type OnMap
                if (GameManager.instance.dataScript.CheckActorArcPresent(actor.arc, playerSide, true) == false)
                {
                    StringBuilder builderActive = new StringBuilder();
                    builderActive.AppendFormat("{0}{1} Motivation +{2}{3}{4}", colourGood, actor.actorName, motivationGainActiveDuty, colourEnd, "\n");
                    builderActive.AppendFormat("{0}{1} joins others On Map{2}{3}", colourNeutral, actor.actorName, colourEnd, "\n");
                    builderActive.AppendFormat("{0}{1} will no longer Threaten or be Unhappy{2}{3}", colourGood, actor.actorName, colourEnd, "\n");
                    if (playerSide.level == globalAuthority.level)
                    {
                        //will or won't bring a team with them
                        if (GameManager.instance.aiScript.CheckNewTeamPossible() == true)
                        {
                            //only if a new actor
                            if (actor.isNewRecruit == true)
                            { builderActive.AppendFormat("{0}Will bring a {1} team{2}", colourNeutral, actor.arc.preferredTeam.name, colourEnd); }
                            else
                            { builderActive.AppendFormat("{0}Only new actors can bring a team{1}", colourAlert, colourEnd); }
                        }
                        else { builderActive.AppendFormat("{0}Can't bring a team as roster is full{1}", colourAlert, colourEnd); }
                    }
                    EventButtonDetails actorDetails = new EventButtonDetails()
                    {
                        buttonTitle = "Active Duty",
                        buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                        buttonTooltipMain = string.Format(string.Format("Inform {0} that they are required immediately for Active Duty", actor.actorName)),
                        buttonTooltipDetail = builderActive.ToString(),
                        //use a Lambda to pass arguments to the action
                        action = () => { EventManager.instance.PostNotification(EventType.InventoryActiveDuty, this, actorActionDetails, "ActorManager.cs -> GetReservePoolActions"); },

                    };
                    //add Active Duty button to list
                    eventList.Add(actorDetails);
                }
                else
                {
                    //can't have duplicate actor types OnMap
                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                    infoBuilder.AppendFormat("{0}Active Duty not possible as duplicate Actor types aren't allowed{1}", colourCancel, colourEnd);
                }
            }
            else
            {
                //there are no spare OnMap positions availabel
                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                infoBuilder.AppendFormat("{0}Active Duty not possible as there are no vacancies.{1}", colourCancel, colourEnd);
            }
            //
            // - - - Reassure - - -
            //
            if (actor.unhappyTimer > 0)
            {
                if (actor.isReassured == false)
                {
                    tooltipText = string.Format("{0}{1}'s Unhappy Timer +{2}{3}{4}{5}Can only be Reassured once{6}", colourGood, actor.actorName,
                        unhappyReassureBoost, colourEnd, "\n", colourNeutral, colourEnd);
                    EventButtonDetails actorDetails = new EventButtonDetails()
                    {
                        buttonTitle = "Reassure",
                        buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                        buttonTooltipMain = string.Format(string.Format("Reassure {0} that they will be the next person called for active duty", actor.actorName)),
                        buttonTooltipDetail = tooltipText,
                        //use a Lambda to pass arguments to the action
                        action = () => { EventManager.instance.PostNotification(EventType.InventoryReassure, this, actorActionDetails, "ActorManager.cs -> GetReservePoolActions"); },

                    };
                    //add Lie Low button to list
                    eventList.Add(actorDetails);
                }
                else
                {
                    //actor has already been reassured (once only effect)
                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                    infoBuilder.AppendFormat("{0} has already been Reassured", actor.actorName);
                }
            }
            else
            {
                //can't reassure somebody who is already unhappy
                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                infoBuilder.AppendFormat("{0}Can't Reassure if Unhappy.{1}", colourBad, colourEnd);
            }
            //
            // - - - Let go - - -
            //
            if (actor.unhappyTimer > 0)
            {
                if (actor.isNewRecruit == true)
                {
                    tooltipText = string.Format("{0}{1}'s{2}Motivation -1{3}{4}{5}Can be recruited again{6}", colourBad, actor.actorName, "\n",
                        colourEnd, "\n", colourNeutral, colourEnd);
                    EventButtonDetails actorDetails = new EventButtonDetails()
                    {
                        buttonTitle = "Let Go",
                        buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                        buttonTooltipMain = string.Format(string.Format("You don't want to but unfortunately you're going to have to let {0} go", actor.actorName)),
                        buttonTooltipDetail = tooltipText,
                        //use a Lambda to pass arguments to the action
                        action = () => { EventManager.instance.PostNotification(EventType.InventoryLetGo, this, actorActionDetails, "ActorManager.cs -> GetReservePoolActions"); },

                    };
                    //add Lie Low button to list
                    eventList.Add(actorDetails);
                }
                else
                {
                    //can't let go somebody you've already sent to the reserves, only new recruits
                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                    infoBuilder.AppendFormat("{0}Can only Let Go new recruits.{1}", colourCancel, colourEnd);
                }
            }
            else
            {
                //can't reassure somebody who is already unhappy
                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                infoBuilder.AppendFormat("{0}Can't Let Go if Unhappy.{1}", colourBad, colourEnd);
            }
            //
            // - - - Threaten - - -
            //
            if (actor.unhappyTimer > 0)
            {
                renownCost = renownCostThreaten;
                if (playerRenown >= renownCost)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("{0}{1}'s Unhappy Timer +{2}{3}", colourGood, actor.actorName, unhappyThreatenBoost, colourEnd);
                    builder.AppendLine();
                    builder.AppendFormat("{0}Player Renown -{1}{2}", colourBad, renownCostThreaten, colourEnd);
                    builder.AppendLine();
                    builder.AppendFormat("{0}Can be Threatened again provided not Unhappy{1}", colourNeutral, colourEnd);
                    EventButtonDetails actorDetails = new EventButtonDetails()
                    {
                        buttonTitle = "Threaten",
                        buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                        buttonTooltipMain = string.Format("You eyeball {0} and threaten that if they don't stop complaining you'll speak to HQ", actor.actorName),
                        buttonTooltipDetail = builder.ToString(),
                        //use a Lambda to pass arguments to the action
                        action = () => { EventManager.instance.PostNotification(EventType.InventoryThreaten, this, actorActionDetails, "ActorManager.cs -> GetReservePoolActions"); },

                    };
                    //add Lie Low button to list
                    eventList.Add(actorDetails);
                }
                else
                {
                    //not enough renown
                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                    infoBuilder.AppendFormat("{0}Insufficient Renown to Threaten (need {1}, currently have {2}){3}", colourBad, renownCost, playerRenown, colourEnd);
                }
            }
            else
            {
                //can't threaten somebody who is already unhappy
                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                infoBuilder.AppendFormat("{0}Can't Threaten if Unhappy.{1}", colourBad, colourEnd);
            }

            //
            // - - - Fire - - -
            //
            //generic tooltip (depends if actor is threatening or not)
            StringBuilder builderTooltip = new StringBuilder();
            builderTooltip.AppendFormat("{0}{1}'s Motivation -{2}{3}", colourBad, actor.actorName, motivationLossFire, colourEnd);
            builderTooltip.AppendLine();
            builderTooltip.AppendFormat("{0}Can be recruited again{1}", colourNeutral, colourEnd);
            builderTooltip.AppendLine();
            //double renown cost if actor threatening to take action against player
            if (actor.isThreatening == false)
            {
                renownCost = manageDismissRenown;
                builderTooltip.AppendFormat("{0}Player Renown -{1}{2}", colourBad, renownCost, colourEnd);
            }
            else
            {
                renownCost = manageDismissRenown * 2;
                builderTooltip.AppendFormat("{0}Player Renown -{1}{2}", colourBad, renownCost, colourEnd);
                builderTooltip.AppendLine();
                builderTooltip.AppendFormat("{0}Double Renown cost as {1} is Threatening you{2}", colourCancel, actor.actorName, colourEnd);
            }
            //only show button if player has enough renown to cover the cost of firing
            if (playerRenown >= renownCost)
            {
                EventButtonDetails actorDetails = new EventButtonDetails()
                {
                    buttonTitle = "FIRE",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = string.Format(string.Format("You inform {0} that their services are no longer needed, or desired", actor.actorName)),
                    buttonTooltipDetail = builderTooltip.ToString(),
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.InventoryFire, this, actorActionDetails, "ActorManager.cs -> GetReservePoolActions"); },
                };
                //add Fire button to list
                eventList.Add(actorDetails);
            }
            else
            {
                //not enough renown
                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                infoBuilder.AppendFormat("{0}Insufficient Renown to Fire (need {1}, currently have {2}){3}", colourBad, renownCost, playerRenown, colourEnd);
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid actor (Null) for actorID {0}", actorID));
        }
        //Debug
        if (string.IsNullOrEmpty(cancelText)) { cancelText = "Unknown"; }

        //
        // - - - Cancel - - - (both sides)
        //
        //Cancel button is added last
        EventButtonDetails cancelDetails = null;
        if (actor != null)
        {
            if (infoBuilder.Length > 0)
            {
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = cancelText,
                    buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, infoBuilder.ToString(), colourEnd),
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetReservePoolActions"); }
                };
            }
            else
            {
                //necessary to prevent color tags triggering the bottom divider in TooltipGeneric
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = cancelText,
                    buttonTooltipDetail = string.Format("{0}Press Cancel to Exit{1}", colourCancel, colourEnd),
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetReservePoolActions"); }
                };
            }
        }
        else
        {
            //Null gear -> invalid menu creation
            cancelDetails = new EventButtonDetails()
            {
                buttonTitle = "CANCEL",
                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                buttonTooltipMain = string.Format("{0}Invalid Actor{1}", colourBad, colourEnd),
                buttonTooltipDetail = string.Format("{0}Press Cancel to Exit{1}", colourCancel, colourEnd),
                //use a Lambda to pass arguments to the action
                action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetReservePoolActions"); }
            };
        }
        //add Cancel button to list
        eventList.Add(cancelDetails);
        return eventList;
    }


    /// <summary>
    /// call this to recruit an actor of a specified level (1 to 3) from a Decision
    /// </summary>
    /// <param name="level"></param>
    public void RecruitActor(int level = 0)
    {
        Debug.Assert(level > 0 && level <= 3, string.Format("Invalid level {0}", level));
        ModalActionDetails details = new ModalActionDetails();
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        //ignore node and actorSlotID
        details.side = side;
        details.level = level;
        details.nodeID = -1;
        details.actorDataID = -1;
        //get event
        switch (side.name)
        {
            case "Authority":
                details.eventType = EventType.GenericRecruitActorAuthority;
                break;
            case "Resistance":
                details.eventType = EventType.GenericRecruitActorResistance;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", side.name));
                break;
        }
        InitialiseGenericPickerRecruit(details);
    }

    /// <summary>
    /// Choose Recruit (Both sides): sets up ModalGenericPicker class and triggers event: ModalGenericEvent.cs -> SetGenericPicker()
    /// </summary>
    /// <param name="details"></param>
    private void InitialiseGenericPickerRecruit(ModalActionDetails details)
    {
        bool errorFlag = false;
        bool isIgnoreCache = false;
        bool isResistance = false;
        bool isResistancePlayer = false;
        int index, countOfRecruits;
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Node node = null;

        #region CaptureCheckResistance
        //Capture check
        if (details.side.level == GameManager.instance.globalScript.sideResistance.level)
        {
            isResistance = true;
            node = GameManager.instance.dataScript.GetNode(details.nodeID);
            if (node != null)
            {
                //check for player/actor being captured
                int actorID = GameManager.instance.playerScript.actorID;
                if (node.nodeID != GameManager.instance.nodeScript.nodePlayer)
                {
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, globalResistance);
                    if (actor != null)
                    { actorID = actor.actorID; }
                    else
                    {
                        Debug.LogError(string.Format("Invalid actor (Null) for details.ActorSlotID {0}", details.actorDataID));
                        errorFlag = true;
                    }
                }
                else { isResistancePlayer = true; }
                //check capture provided no errors
                if (errorFlag == false)
                {
                    CaptureDetails captureDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, actorID);
                    if (captureDetails != null)
                    {
                        //capture happened, abort recruitment
                        captureDetails.effects = string.Format("{0}The Recruiting mission was a wipe{1}", colourNeutral, colourEnd);
                        EventManager.instance.PostNotification(EventType.Capture, this, captureDetails, "ActorManager.cs -> InitialiseGenericPickerRecruit");
                        return;
                    }
                }
                else
                {
                    //reset flag to the default state prior to recruitments
                    errorFlag = false;
                }
            }
        }
        #endregion

        //get a new selection once per action. If multiple attempts during an action used the cached results to ensure the same recruit is available on each attempt
        if (isResistance == true)
        {
            if (isResistancePlayer == true)
            {
                if (resistancePlayerTurn != GameManager.instance.turnScript.Turn || isNewActionResistancePlayer == true)
                { isIgnoreCache = true; }
            }
            else
            {
                //actor gear selection (can be different from Players, eg. better chance of rare gear)
                if (resistanceActorTurn != GameManager.instance.turnScript.Turn || isNewActionResistanceActor == true)
                { isIgnoreCache = true; }
            }
        }
        else
        {
            //authority
            if (authorityTurn != GameManager.instance.turnScript.Turn || isNewActionAuthority == true)
            { isIgnoreCache = true; }
        }

        //proceed with a new Recruit Selection
        if (isIgnoreCache == true)
        {
            #region RecruitActorBothSides
            //Recruit actor (both sides)
            if (details.side.level == globalAuthority.level || node != null)
            {
                if (details.side.level == globalResistance.level) { genericDetails.returnEvent = EventType.GenericRecruitActorResistance; }
                else if (details.side.level == globalAuthority.level) { genericDetails.returnEvent = EventType.GenericRecruitActorAuthority; }
                else { Debug.LogError(string.Format("Invalid side \"{0}\"", details.side)); }
                genericDetails.side = details.side;
                genericDetails.textHeader = "Recruit Subordinate";
                if (details.side.level == globalResistance.level)
                { genericDetails.nodeID = details.nodeID; }
                else { genericDetails.nodeID = -1; }
                genericDetails.actorSlotID = details.actorDataID;
                //picker text
                genericDetails.textTop = string.Format("{0}Recruits{1} {2}available{3}", colourNeutral, colourEnd, colourNormal, colourEnd);
                genericDetails.textMiddle = string.Format("{0}Recruit will be assigned to your reserve list{1}",
                    colourNormal, colourEnd);
                genericDetails.textBottom = "Click on a Recruit to Select. Press CONFIRM to hire Recruit. Mouseover recruit for more information.";
                //
                //generate temp list of Recruits to choose from and a list of ones for the picker
                //
                int numOfOptions;
                List<int> listOfPoolActors = new List<int>();
                List<int> listOfPickerActors = new List<int>();
                List<int> listOfCurrentArcIDs = new List<int>(GameManager.instance.dataScript.GetAllCurrentActorArcIDs(details.side));
                //selection methodology varies for each side -> need to populate 'listOfPoolActors'
                if (details.side.level == globalResistance.level)
                {
                    if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                    {
                        //player at node, select from 3 x level 1 options, different from current OnMap actor types
                        listOfPoolActors.AddRange(GameManager.instance.dataScript.GetActorRecruitPool(1, details.side));
                        //loop backwards through pool of actors and remove any that match the curent OnMap types
                        for (int i = listOfPoolActors.Count - 1; i >= 0; i--)
                        {
                            Actor actor = GameManager.instance.dataScript.GetActor(listOfPoolActors[i]);
                            if (actor != null)
                            {
                                if (listOfCurrentArcIDs.Exists(x => x == actor.arc.ActorArcID))
                                { listOfPoolActors.RemoveAt(i); }
                            }
                            else { Debug.LogWarning(string.Format("Invalid actor (Null) for actorID {0}", listOfPoolActors[i])); }
                        }
                    }
                    else
                    {
                        //actor at node, select from 3 x level 2 options (random types, could be the same as currently OnMap)
                        listOfPoolActors.AddRange(GameManager.instance.dataScript.GetActorRecruitPool(2, details.side));
                    }
                }
                //Authority
                else if (details.side.level == globalAuthority.level)
                {
                    //placeholder -> select from 3 x specified level options (random types, could be the same as currently OnMap)
                    listOfPoolActors.AddRange(GameManager.instance.dataScript.GetActorRecruitPool(details.level, details.side));
                }
                else { Debug.LogError(string.Format("Invalid side \"{0}\"", details.side)); }
                //
                //select three actors for the picker
                //
                numOfOptions = Math.Min(3, listOfPoolActors.Count);
                for (int i = 0; i < numOfOptions; i++)
                {
                    index = Random.Range(0, listOfPoolActors.Count);
                    listOfPickerActors.Add(listOfPoolActors[index]);
                    //remove actor from pool to prevent duplicate types
                    listOfPoolActors.RemoveAt(index);
                }
                //check there is at least one recruit available
                countOfRecruits = listOfPickerActors.Count;
                if (countOfRecruits < 1)
                {
                    //OUTCOME -> No recruits available
                    Debug.LogWarning("ActorManager: No recruits available in InitaliseGenericPickerRecruits");
                    errorFlag = true;
                }
                else
                {
                    //
                    //loop actorID's that have been selected and package up ready for ModalGenericPicker
                    //
                    for (int i = 0; i < countOfRecruits; i++)
                    {
                        Actor actor = GameManager.instance.dataScript.GetActor(listOfPickerActors[i]);
                        if (actor != null)
                        {
                            //option details
                            GenericOptionDetails optionDetails = new GenericOptionDetails();
                            optionDetails.optionID = actor.actorID;
                            optionDetails.text = actor.arc.name;
                            optionDetails.sprite = actor.arc.baseSprite;
                            //tooltip
                            GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                            //arc type and name
                            tooltipDetails.textHeader = string.Format("{0}{1}{2}{3}{4}{5}{6}", colourRecruit, actor.arc.name, colourEnd,
                                "\n", colourNormal, actor.actorName, colourEnd);
                            //stats
                            string[] arrayOfQualities = GameManager.instance.dataScript.GetQualities(details.side);
                            StringBuilder builder = new StringBuilder();
                            if (arrayOfQualities.Length > 0)
                            {
                                builder.AppendFormat("{0}  {1}{2}{3}{4}", arrayOfQualities[0], GameManager.instance.colourScript.GetValueColour(actor.datapoint0),
                                    actor.datapoint0, colourEnd, "\n");
                                builder.AppendFormat("{0}  {1}{2}{3}{4}", arrayOfQualities[1], GameManager.instance.colourScript.GetValueColour(actor.datapoint1),
                                    actor.datapoint1, colourEnd, "\n");
                                builder.AppendFormat("{0}  {1}{2}{3}", arrayOfQualities[2], GameManager.instance.colourScript.GetValueColour(actor.datapoint2),
                                    actor.datapoint2, colourEnd);
                                tooltipDetails.textMain = string.Format("{0}{1}{2}", colourNormal, builder.ToString(), colourEnd);
                            }
                            //trait and action
                            tooltipDetails.textDetails = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "<font=\"Bangers SDF\">", "<cspace=0.6em>", actor.GetTrait().tagFormatted,
                                "</cspace>", "</font>", "\n", colourNormal, actor.arc.nodeAction.name, colourEnd);
                            //add to master arrays
                            genericDetails.arrayOfOptions[i] = optionDetails;
                            genericDetails.arrayOfTooltips[i] = tooltipDetails;
                        }
                        else { Debug.LogError(string.Format("Invalid actor (Null) for gearID {0}", listOfPickerActors[i])); }
                    }
                }
            }
            else
            {
                Debug.LogError(string.Format("Invalid Node (null) for nodeID {0}", details.nodeID));
                errorFlag = true;
            }
            #endregion
        }

        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = globalResistance;
            outcomeDetails.textTop = "There has been a Snafu in communication and no recruits can be found.";
            outcomeDetails.textBottom = "Heads will roll!";
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActorManager.cs -> InitialiseGenericPickerRecruit");
        }
        else
        {
            if (isIgnoreCache == true)
            {
                //activate Generic Picker window
                EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails, "ActorManager.cs -> InitialiseGenericPickerRecruit");
                if (isResistance == true)
                {
                    if (isResistancePlayer == true)
                    {
                        //cache details in case player attempts to access Player recruit selection again this action
                        resistancePlayerTurn = GameManager.instance.turnScript.Turn;
                        cachedResistancePlayerDetails = genericDetails;
                        isNewActionResistancePlayer = false;
                    }
                    else
                    {
                        //cache details in case player attempts to access Actor recruit selection again this action
                        resistanceActorTurn = GameManager.instance.turnScript.Turn;
                        cachedResistanceActorDetails = genericDetails;
                        isNewActionResistanceActor = false;
                    }
                }
                else
                {
                    //Authority cache details in case authority player attempts to access  recruit selection again this action
                    authorityTurn = GameManager.instance.turnScript.Turn;
                    cachedAuthorityDetails = genericDetails;
                    isNewActionAuthority = false;
                }
            }
            //player accessing recruit during the same action multiple times. Used cached details so he gets the same result.
            else
            {
                if (isResistance == true)
                {
                    if (isResistancePlayer == true)
                    {
                        //Player recruit selection
                        if (cachedResistancePlayerDetails != null)
                        { EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, cachedResistancePlayerDetails, "ActorManager.cs -> InitialiseGenericPickerRecruit"); }
                        else
                        { Debug.LogWarning("Invalid cachedGenericDetails Resistance Player(Null)"); }
                    }
                    else
                    {
                        //Actor recruit selection
                        if (cachedResistanceActorDetails != null)
                        { EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, cachedResistanceActorDetails, "ActorManager.cs -> InitialiseGenericPickerRecruit"); }
                        else
                        { Debug.LogWarning("Invalid cachedGenericDetails Resistance Actor (Null)"); }
                    }
                }
                else
                {
                    //Authority
                    if (cachedAuthorityDetails != null)
                    { EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, cachedAuthorityDetails, "ActorManager.cs -> InitialiseGenericPickerRecruit"); }
                    else
                    { Debug.LogWarning("Invalid cachedGenericDetails Authority (Null)"); }
                }
            }
        }
     
    }

    /// <summary>
    /// sets up all needed data for Reserve Actor pool and triggers ModalInventoryUI to display such
    /// </summary>
    private void InitialiseReservePoolInventory()
    {
        int numOfActors;
        string unhappySituation;
        bool errorFlag = false;
        //close node tooltip -> safety check
        GameManager.instance.tooltipNodeScript.CloseTooltip("ActorManager.cs -> InitialiseReservePoolInventory");
        numOfActors = GameManager.instance.dataScript.CheckNumOfActorsInReserve(); 
        //check for presence of actors in reserve pool
        if (numOfActors > 0)
        {
            //At least one actor in reserve
            InventoryInputData data = new InventoryInputData();
            data.side = GameManager.instance.sideScript.PlayerSide;
            data.textHeader = "Reserve Actor Pool";
            data.textTop = string.Format("{0}You have {1}{2}{3}{4}{5} out of {6}{7}{8}{9}{10} possible Actor{11} in your Reserve pool{12}", colourNeutral, colourEnd,
                colourDefault, numOfActors, colourEnd, colourNeutral, colourEnd, colourDefault, maxNumOfReserveActors, colourEnd, colourNeutral, 
                maxNumOfReserveActors != 1 ? "s" : "", colourEnd);
            data.textBottom = string.Format("{0}LEFT CLICK{1}{2} Actor for Info, {3}{4}RIGHT CLICK{5}{6} Actor for Options{7}", colourAlert, colourEnd, colourDefault, 
                colourEnd, colourAlert, colourEnd, colourDefault, colourEnd);
            data.handler = RefreshReservePool;
            data.state = InventoryState.ReservePool;
            //Loop Actor list and populate arrays
            List<int> listOfActors = GameManager.instance.dataScript.GetActorList(data.side, ActorList.Reserve);
            if (listOfActors != null)
            {
                Condition conditionUnhappy = GameManager.instance.dataScript.GetCondition("UNHAPPY");
                if (conditionUnhappy != null)
                {
                    for (int i = 0; i < listOfActors.Count; i++)
                    {
                        Actor actor = GameManager.instance.dataScript.GetActor(listOfActors[i]);
                        if (actor != null)
                        {
                            InventoryOptionData optionData = new InventoryOptionData();
                            optionData.sprite = actor.arc.baseSprite;
                            optionData.textUpper = actor.arc.name;
                            //unhappy situation
                            if (actor.CheckConditionPresent(conditionUnhappy) == true)
                            { unhappySituation = string.Format("{0}{1}{2}", colourBad, conditionUnhappy.name, colourEnd); }
                            else
                            { unhappySituation = string.Format("{0}Unhappy in {1} turn{2}{3}", colourDefault, actor.unhappyTimer, 
                                actor.unhappyTimer != 1 ? "s" : "", colourEnd); }
                            //combined text string
                            optionData.textLower = string.Format("{0}{1}{2}", actor.GetTrait().tagFormatted, "\n", unhappySituation);
                            optionData.optionID = actor.actorID;
                            //tooltip
                            GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                            //arc type and name
                            tooltipDetails.textHeader = string.Format("{0}{1}{2}{3}{4}{5}{6}", colourRecruit, actor.arc.name, colourEnd,
                                "\n", colourNormal, actor.actorName, colourEnd);
                            //stats
                            string[] arrayOfQualities = GameManager.instance.dataScript.GetQualities(data.side);
                            StringBuilder builder = new StringBuilder();
                            if (arrayOfQualities.Length > 0)
                            {
                                builder.AppendFormat("{0}  {1}{2}{3}{4}", arrayOfQualities[0], GameManager.instance.colourScript.GetValueColour(actor.datapoint0),
                                    actor.datapoint0, colourEnd, "\n");
                                builder.AppendFormat("{0}  {1}{2}{3}{4}", arrayOfQualities[1], GameManager.instance.colourScript.GetValueColour(actor.datapoint1),
                                    actor.datapoint1, colourEnd, "\n");
                                builder.AppendFormat("{0}  {1}{2}{3}", arrayOfQualities[2], GameManager.instance.colourScript.GetValueColour(actor.datapoint2),
                                    actor.datapoint2, colourEnd);
                                tooltipDetails.textMain = string.Format("{0}{1}{2}", colourNormal, builder.ToString(), colourEnd);
                            }
                            //trait and action
                            tooltipDetails.textDetails = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "<font=\"Bangers SDF\">", "<cspace=0.6em>", 
                                actor.GetTrait().tagFormatted, "</cspace>", "</font>", "\n", colourNormal, actor.arc.nodeAction.name, colourEnd);
                            //add to arrays
                            data.arrayOfOptions[i] = optionData;
                            data.arrayOfTooltips[i] = tooltipDetails;
                        }
                        else
                        { Debug.LogWarning(string.Format("Invalid Actor (Null) for actorID {0}", listOfActors[i])); }
                    }
                }
                else
                {
                    Debug.LogError("Invalid conditionUnhappy (Null)");
                    errorFlag = true;
                }
            }
            else
            {
                Debug.LogError("Invalid listOfActors (Null)");
                errorFlag = true;
            }
            //data package has been populated, proceed if all O.K
            if (errorFlag == true)
            {
                //error msg
                ModalOutcomeDetails details = new ModalOutcomeDetails()
                {
                    side = GameManager.instance.sideScript.PlayerSide,
                    textTop = string.Format("{0}Something has gone pear shaped with your Administration{1}", colourAlert, colourEnd),
                    textBottom = "Phone calls are being made. Lots of them.",
                    sprite = GameManager.instance.guiScript.errorSprite,
                    isAction = false
                };
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "ActorManager.cs -> InitialiseReservePoolInventory");
            }
            else
            {
                //open Inventory UI
                EventManager.instance.PostNotification(EventType.InventoryOpenUI, this, data, "ActorManager.cs -> InitialiseReservePoolInventory");
            }

        }
        else
        {
            //no actor in reserve
            ModalOutcomeDetails details = new ModalOutcomeDetails()
            {
                side = GameManager.instance.sideScript.PlayerSide,
                textTop = string.Format("{0}There are currently no Actors in your Reserve Pool{1}", colourInvalid, colourEnd),
                textBottom = string.Format("You can have a maximum of {0} actors in your Reserves", maxNumOfReserveActors),
                sprite = GameManager.instance.guiScript.infoSprite,
                isAction = false
            };
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "ActorManager.cs -> InitialiseReservePoolInventory");
        }
    }


    public InventoryInputData RefreshReservePool()
    {
        int numOfActors;
        string unhappySituation;
        InventoryInputData data = new InventoryInputData();
        numOfActors = GameManager.instance.dataScript.CheckNumOfActorsInReserve();
        data.side = GameManager.instance.sideScript.PlayerSide;
        data.textTop = string.Format("{0}You have {1}{2}{3}{4}{5} out of {6}{7}{8}{9}{10} possible Actor{11} in your Reserve pool{12}", colourNeutral, colourEnd,
            colourDefault, numOfActors, colourEnd, colourNeutral, colourEnd, colourDefault, maxNumOfReserveActors, colourEnd, colourNeutral,
            maxNumOfReserveActors != 1 ? "s" : "", colourEnd);
        if (numOfActors > 0)
        {
            data.textBottom = string.Format("{0}LEFT CLICK{1}{2} Actor for Info, {3}{4}RIGHT CLICK{5}{6} Actor for Options{7}", colourAlert, colourEnd, colourDefault,
            colourEnd, colourAlert, colourEnd, colourDefault, colourEnd);
        }
        else { data.textBottom = ""; }
        //Loop Actor list and populate arrays
        List<int> listOfActors = GameManager.instance.dataScript.GetActorList(data.side, ActorList.Reserve);
        if (listOfActors != null)
        {
            Condition conditionUnhappy = GameManager.instance.dataScript.GetCondition("UNHAPPY");
            if (conditionUnhappy != null)
            {
                for (int i = 0; i < listOfActors.Count; i++)
                {
                    Actor actor = GameManager.instance.dataScript.GetActor(listOfActors[i]);
                    if (actor != null)
                    {
                        InventoryOptionData optionData = new InventoryOptionData();
                        optionData.sprite = actor.arc.baseSprite;
                        optionData.textUpper = actor.arc.name;
                        //unhappy situation
                        if (actor.CheckConditionPresent(conditionUnhappy) == true)
                        { unhappySituation = string.Format("{0}{1}{2}", colourBad, conditionUnhappy.name, colourEnd); }
                        else
                        {
                            unhappySituation = string.Format("{0}Unhappy in {1} turn{2}{3}", colourDefault, actor.unhappyTimer,
                              actor.unhappyTimer != 1 ? "s" : "", colourEnd);
                        }
                        //combined text string
                        optionData.textLower = string.Format("{0}{1}{2}", actor.GetTrait().tagFormatted, "\n", unhappySituation);
                        optionData.optionID = actor.actorID;
                        //tooltip
                        GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                        //arc type and name
                        tooltipDetails.textHeader = string.Format("{0}{1}{2}{3}{4}{5}{6}", colourRecruit, actor.arc.name, colourEnd,
                            "\n", colourNormal, actor.actorName, colourEnd);
                        //stats
                        string[] arrayOfQualities = GameManager.instance.dataScript.GetQualities(data.side);
                        StringBuilder builder = new StringBuilder();
                        if (arrayOfQualities.Length > 0)
                        {
                            builder.AppendFormat("{0}  {1}{2}{3}{4}", arrayOfQualities[0], GameManager.instance.colourScript.GetValueColour(actor.datapoint0),
                                actor.datapoint0, colourEnd, "\n");
                            builder.AppendFormat("{0}  {1}{2}{3}{4}", arrayOfQualities[1], GameManager.instance.colourScript.GetValueColour(actor.datapoint1),
                                actor.datapoint1, colourEnd, "\n");
                            builder.AppendFormat("{0}  {1}{2}{3}", arrayOfQualities[2], GameManager.instance.colourScript.GetValueColour(actor.datapoint2),
                                actor.datapoint2, colourEnd);
                            tooltipDetails.textMain = string.Format("{0}{1}{2}", colourNormal, builder.ToString(), colourEnd);
                        }
                        //trait and action
                        tooltipDetails.textDetails = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "<font=\"Bangers SDF\">", "<cspace=1em>", actor.GetTrait().tagFormatted, 
                            "</cspace>", "</font>", "\n", colourNormal, actor.arc.nodeAction.name, colourEnd);
                        //add to arrays
                        data.arrayOfOptions[i] = optionData;
                        data.arrayOfTooltips[i] = tooltipDetails;
                    }
                    else
                    { Debug.LogWarning(string.Format("Invalid Actor (Null) for actorID {0}", listOfActors[i])); }
                }
            }
            else
            { Debug.LogError("Invalid conditionUnhappy (Null)"); }
        }
        else
        { Debug.LogError("Invalid listOfActors (Null)"); }
        return data;
    }

    /// <summary>
    /// Processes choice of Recruit Resistance Actor
    /// </summary>
    /// <param name="returnDetails"></param>
    private void ProcessRecruitChoiceResistance(GenericReturnData data)
    {
        bool successFlag = true;
        StringBuilder builderTop = new StringBuilder();
        StringBuilder builderBottom = new StringBuilder();
        Sprite sprite = GameManager.instance.guiScript.errorSprite;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        if (data != null)
        {
            if (data.optionID > -1)
            {
                //find actor
                Actor actorCurrent = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, playerSide);
                if (actorCurrent != null)
                {
                    Actor actorRecruited = GameManager.instance.dataScript.GetActor(data.optionID);
                    if (actorRecruited != null)
                    {
                        //add actor to reserve pool
                        if (GameManager.instance.dataScript.AddActorToReserve(actorRecruited.actorID, playerSide) == true)
                        {
                            //change actor's status
                            actorRecruited.Status = ActorStatus.ReservePool;
                            //remove actor from appropriate pool list
                            GameManager.instance.dataScript.RemoveActorFromPool(actorRecruited.actorID, actorRecruited.level, playerSide);
                            //sprite of recruited actor
                            sprite = actorRecruited.arc.baseSprite;
                            //initiliase unhappy timer
                            actorRecruited.unhappyTimer = recruitedReserveTimer;
                            actorRecruited.isNewRecruit = true;
                            //actor successfully recruited
                            builderTop.AppendFormat("{0}The interview went well!{1}", colourNormal, colourEnd);
                            builderBottom.AppendFormat("{0}{1}{2}, {3}\"{4}\", has been recruited and is available in the Reserve List{5}", colourArc,
                                actorRecruited.arc.name, colourEnd, colourNormal, actorRecruited.actorName, colourEnd);


                            //message
                            string textMsg = string.Format("{0}, {1}, ID {2} has been recruited", actorRecruited.actorName, actorRecruited.arc.name,
                                actorRecruited.actorID);
                            Message message = GameManager.instance.messageScript.ActorRecruited(textMsg, data.nodeID, actorRecruited.actorID,
                                GameManager.instance.globalScript.sideResistance);
                            if (message != null) { GameManager.instance.dataScript.AddMessage(message); }
                            //Process any other effects, if acquisition was successfull, ignore otherwise
                            Action action = actorCurrent.arc.nodeAction;
                            Node node = GameManager.instance.dataScript.GetNode(data.nodeID);
                            if (node != null)
                            {
                                //reset recruit actor cache flags
                                if (GameManager.instance.nodeScript.nodePlayer == node.nodeID)
                                { isNewActionResistancePlayer = true; }
                                else { isNewActionResistanceActor = true; }
                                //loop effects
                                List<Effect> listOfEffects = action.GetEffects();
                                if (listOfEffects.Count > 0)
                                {
                                    EffectDataInput dataInput = new EffectDataInput();
                                    dataInput.textOrigin = "Recruit Actor";
                                    foreach (Effect effect in listOfEffects)
                                    {
                                        if (effect.ignoreEffect == false)
                                        {
                                            EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actorCurrent);
                                            if (effectReturn != null)
                                            {
                                                builderTop.AppendLine();
                                                builderTop.Append(effectReturn.topText);
                                                builderBottom.AppendLine();
                                                builderBottom.AppendLine();
                                                builderBottom.Append(effectReturn.bottomText);
                                                //exit effect loop on error
                                                if (effectReturn.errorFlag == true) { break; }
                                            }
                                            else { Debug.LogError("Invalid effectReturn (Null)"); }
                                        }
                                    }

                                }
                            }
                            else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", data.nodeID)); }
                        }
                        else
                        {
                            //some issue prevents actor being added to reserve pool (full? -> probably not as a criteria checks this)
                            successFlag = false;
                        }
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("Invalid Recruited Actor (Null) for actorID {0}", data.optionID));
                        successFlag = false;
                    }
                }
                else
                {
                    Debug.LogWarning(string.Format("Invalid Current Actor (Null) for actorSlotID {0}", data.actorSlotID));
                    successFlag = false;
                }
            }
            else
            { Debug.LogWarning(string.Format("Invalid optionID {0}", data.optionID)); }
        }
        else
        { Debug.LogError("Invalid GenericReturnData (Null)"); successFlag = false; }
        //failed outcome
        if (successFlag == false)
        {
            builderTop.Append("Something has gone wrong. Our Recruit has gone missing");
            builderBottom.Append("This is a matter of concern");
        }
        //
        // - - - Outcome - - - 
        //
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        outcomeDetails.textTop = builderTop.ToString();
        outcomeDetails.textBottom = builderBottom.ToString();
        outcomeDetails.sprite = sprite;
        outcomeDetails.side = playerSide;
        //action expended automatically for recruit actor
        if (successFlag == true)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Recruit Actor";
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActorManager.cs -> ProcessRecruitChoiceResistance");
    }

    /// <summary>
    /// Processes choice of Recruit Authority Actor
    /// </summary>
    /// <param name="returnDetails"></param>
    private void ProcessRecruitChoiceAuthority(GenericReturnData data)
    {
        bool successFlag = true;
        StringBuilder builderTop = new StringBuilder();
        StringBuilder builderBottom = new StringBuilder();
        Sprite sprite = GameManager.instance.guiScript.errorSprite;
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        if (data != null)
        {
            if (data.optionID > -1)
            {
                //find actor
                Actor actorRecruited = GameManager.instance.dataScript.GetActor(data.optionID);
                if (actorRecruited != null)
                {
                    //add actor to reserve pool
                    if (GameManager.instance.dataScript.AddActorToReserve(actorRecruited.actorID, side) == true)
                    {
                        //change actor's status
                        actorRecruited.Status = ActorStatus.ReservePool;
                        //remove actor from appropriate pool list
                        GameManager.instance.dataScript.RemoveActorFromPool(actorRecruited.actorID, actorRecruited.level, side);
                        //sprite of recruited actor
                        sprite = actorRecruited.arc.baseSprite;
                        //initiliase unhappy timer
                        actorRecruited.unhappyTimer = recruitedReserveTimer;
                        actorRecruited.isNewRecruit = true;
                        //message
                        string textMsg = string.Format("{0}, {1}, ID {2} has been recruited", actorRecruited.actorName, actorRecruited.arc.name,
                            actorRecruited.actorID);
                        Message message = GameManager.instance.messageScript.ActorRecruited(textMsg, data.nodeID, actorRecruited.actorID,
                            GameManager.instance.globalScript.sideAuthority);
                        if (message != null) { GameManager.instance.dataScript.AddMessage(message); }
                        //actor successfully recruited
                        builderTop.AppendFormat("{0}The interview went well!{1}", colourNormal, colourEnd);
                        builderBottom.AppendFormat("{0}{1}{2}, {3}\"{4}\", has been recruited and is available in the Reserve List{5}", colourArc,
                            actorRecruited.arc.name, colourEnd, colourNormal, actorRecruited.actorName, colourEnd);
                        //reset cached recruit actor flag
                        isNewActionAuthority = true;
                    }
                    else
                    {
                        //some issue prevents actor being added to reserve pool (full? -> probably not as a criteria checks this)
                        successFlag = false;
                    }
                }
                else
                {
                    Debug.LogWarning(string.Format("Invalid Recruited Actor (Null) for actorID {0}", data.optionID));
                    successFlag = false;
                }
            }
            else
            { Debug.LogWarning(string.Format("Invalid optionID {0}", data.optionID)); }
        }
        else
        { Debug.LogError("Invalid GenericDataReturn (Null)"); successFlag = false; }
        //failed outcome
        if (successFlag == false)
        {
            builderTop.Append("Something has gone wrong. Our Recruit has gone missing");
            builderBottom.Append("This is a matter of concern");
        }
        //
        // - - - Outcome - - - 
        //
        ModalOutcomeDetails details = new ModalOutcomeDetails();
        details.textTop = builderTop.ToString();
        details.textBottom = builderBottom.ToString();
        details.sprite = sprite;
        details.side = side;
        if (successFlag == true)
        {
            details.isAction = true;
            details.reason = "Recruit Actor";
        }
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "ActorManager.cs -> ProcessRecruitChoiceAuthority");
    }

    /// <summary>
    /// When an actor's motivation drops below zero (before mincapping) they suffer a relationship conflict with the player. Returns two line text outcome (as well as applying effect).
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    public string ProcessActorConflict(Actor actor)
    {
        bool proceedFlag;
        StringBuilder builder = new StringBuilder();
        string outputMsg = "Unknown";
        if (actor != null)
        {
            //trait -> Team Player (avoids conflicts)
            if (actor.CheckTraitEffect(actorConflictNone) == false)
            {
                GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
                GlobalType typeGood = GameManager.instance.globalScript.typeGood;
                List<ActorConflict> listSelectionPool = new List<ActorConflict>();
                Dictionary<int, ActorConflict> dictOfActorConflicts = GameManager.instance.dataScript.GetDictOfActorConflicts();
                if (dictOfActorConflicts != null)
                {
                    if (dictOfActorConflicts.Count > 0)
                    {
                        //loop dict checking every actorConflict to see if they are applicable
                        foreach (var conflict in dictOfActorConflicts)
                        {
                            if (conflict.Value != null)
                            {
                                proceedFlag = true;
                                //data package
                                CriteriaDataInput criteriaData = new CriteriaDataInput();
                                criteriaData.listOfCriteria = conflict.Value.listOfCriteria;
                                //actorSlot determines whether criteria is tested against Player or actor
                                switch (conflict.Value.who.level)
                                {
                                    case 0:
                                        //player
                                        criteriaData.actorSlotID = -1;
                                        break;
                                    case 1:
                                        //actor
                                        criteriaData.actorSlotID = actor.actorSlotID;
                                        break;
                                    default:
                                        Debug.LogWarningFormat("Invalid conflict.who \"{0}\"", conflict.Value.who);
                                        break;
                                }
                                //
                                // - - - Criteria
                                //
                                if (criteriaData.listOfCriteria != null)
                                {
                                    if (GameManager.instance.effectScript.CheckCriteria(criteriaData) != null)
                                    { proceedFlag = false; }
                                }
                                //check side (if not 'both')
                                if (proceedFlag == true && conflict.Value.side.level != 3)
                                {
                                    if (conflict.Value.side.level != playerSide.level)
                                    { proceedFlag = false; }
                                }
                                //traits -> Thin Skinned (excludes all good conflict options)
                                if (proceedFlag == true && conflict.Value.type == typeGood && actor.CheckTraitEffect(actorConflictNoGoodOptions) == true)
                                {
                                    proceedFlag = false;
                                    TraitLogMessage(actor, "to eliminate Good options in a Conflict");
                                }
                                //test flag
                                if (proceedFlag == true && conflict.Value.isTestOff == true)
                                { proceedFlag = false; }
                                //
                                // - - - Pool - - -
                                //
                                if (proceedFlag == true)
                                {
                                    //place into pool, number of entries according to chance of being selected
                                    int numOfEntries;
                                    switch (conflict.Value.chance.level)
                                    {
                                        case 0:
                                            //low
                                            numOfEntries = 1;
                                            for (int i = 0; i < numOfEntries; i++) { listSelectionPool.Add(conflict.Value); }
                                            /*Debug.LogFormat("[Tst] {0} PASSED Criteria check -> {1} Entry in Pool", conflict.Value.name, numOfEntries);*/
                                            break;
                                        case 1:
                                            //medium
                                            numOfEntries = 2;
                                            for (int i = 0; i < numOfEntries; i++) { listSelectionPool.Add(conflict.Value); }
                                            /*Debug.LogFormat("[Tst] {0} PASSED Criteria check -> {1} Entries in Pool", conflict.Value.name, numOfEntries);*/
                                            break;
                                        case 2:
                                            //high
                                            numOfEntries = 3;
                                            for (int i = 0; i < numOfEntries; i++) { listSelectionPool.Add(conflict.Value); }
                                            /*Debug.LogFormat("[Tst] {0} PASSED Criteria check -> {1} Entries in Pool", conflict.Value.name, numOfEntries);*/
                                            break;
                                        case 3:
                                            //extreme
                                            numOfEntries = 5;
                                            for (int i = 0; i < numOfEntries; i++) { listSelectionPool.Add(conflict.Value); }
                                            /*Debug.LogFormat("[Tst] {0} PASSED Criteria check -> {1} Entries in Pool", conflict.Value.name, numOfEntries);*/
                                            break;
                                        default:
                                            Debug.LogWarningFormat("Invalid ActorConflict.chance.level {0}", conflict.Value.chance.level);
                                            break;
                                    }
                                }
                                else { /*Debug.LogFormat("[Tst] {0} Failed Criteria check", conflict.Value.name);*/ }
                            }
                            else { Debug.LogWarningFormat("Invalid actorConflict (Null) for actBreakID {0}", conflict.Key); }
                        }
                        //select from pool
                        ActorConflict actorConflict = null;
                        EffectDataReturn effectReturn = null;
                        if (listSelectionPool.Count > 0)
                        {
                            int index = Random.Range(0, listSelectionPool.Count);
                            actorConflict = listSelectionPool[index];
                            /*Debug.LogFormat("[Tst] Pool size {0} Entries ->  Selected {1}", listSelectionPool.Count, actorConflict.name);*/
                            //
                            // - - - Special Trait cases - - -
                            //
                            switch (actorConflict.name)
                            {
                                case "Actor Resigns":
                                    if (actor.CheckTraitEffect(actorNeverResigns) == true)
                                    { outputMsg = string.Format("{0}does NOT Resign{1}", colourGood, colourEnd); }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(actorConflict.outcomeText) == false)
                                        { outputMsg = string.Format("{0}{1}{2}", colourAlert, actorConflict.outcomeText, colourEnd); }
                                    }
                                    break;
                                default:
                                    //normally use ActorConflict.outcomeText for top line
                                    if (string.IsNullOrEmpty(actorConflict.outcomeText) == false)
                                    { outputMsg = string.Format("{0}{1}{2}", colourAlert, actorConflict.outcomeText, colourEnd); }
                                    break;
                            }
                            //Implement effect
                            if (actorConflict.effect != null)
                            {
                                //data packages
                                effectReturn = new EffectDataReturn();
                                EffectDataInput effectInput = new EffectDataInput();
                                effectInput.textOrigin = "Relationship Conflict";

                                //message
                                string msgText = string.Format("{0} Relationship Conflict ({1})", actor.arc.name, outputMsg);
                                Message message = GameManager.instance.messageScript.ActorConflict(msgText, actor.actorID, actorConflict.conflictID, GameManager.instance.sideScript.PlayerSide);
                                GameManager.instance.dataScript.AddMessage(message);
                                //
                                // - - - Effect - - -
                                //
                                int nodeID = GameManager.instance.nodeScript.nodePlayer;

                                switch (actorConflict.who.level)
                                {
                                    case 0:
                                        //player
                                        Node nodePlayer = GameManager.instance.dataScript.GetNode(nodeID);
                                        if (nodePlayer != null)
                                        { effectReturn = GameManager.instance.effectScript.ProcessEffect(actorConflict.effect, nodePlayer, effectInput); }
                                        else { Debug.LogWarningFormat("Invalid player Node (Null) for playerNodeID {0}", GameManager.instance.nodeScript.nodePlayer); }
                                        break;
                                    case 1:
                                        //actor -> can't use player node, need a different one (use nodeID 0 or 1 as safe options)
                                        if (nodeID == 0) { nodeID = 1; } else { nodeID = 0; }
                                        Node nodeOther = GameManager.instance.dataScript.GetNode(nodeID);
                                        effectReturn = GameManager.instance.effectScript.ProcessEffect(actorConflict.effect, nodeOther, effectInput, actor);
                                        break;
                                    default:
                                        Debug.LogWarningFormat("Invalid actorConflict.who \"{0}\"", actorConflict.who);
                                        break;
                                }
                            }
                        }
                        //
                        // - - - Output - - -
                        //
                        //build return string -> (2 lines) ActorConflict.outcomeText at top, effectReturn.bottomText underneath
                        if (actorConflict != null)
                        {
                            builder.AppendFormat("{0}{1}{2} {3}", colourAlert, actor.arc.name, colourEnd, outputMsg);
                            //bottom line
                            if (actorConflict.effect != null)
                            {
                                if (string.IsNullOrEmpty(effectReturn.bottomText) == false)
                                {
                                    if (builder.Length > 0) { builder.AppendLine(); }
                                    builder.Append(effectReturn.bottomText);
                                }
                            }
                            else
                            {
                                //default error message (keep it in character)
                                builder.AppendFormat("{0}{1}Nothing happens{2}", "\n", colourGood, colourEnd);
                                //message
                                string msgText = string.Format("{0} Relationship Conflict ({1}Nothing Happens{2})", actor.arc.name, colourGood, colourEnd);
                                Message message = GameManager.instance.messageScript.ActorConflict(msgText, actor.actorID, actorConflict.conflictID, GameManager.instance.sideScript.PlayerSide);
                                GameManager.instance.dataScript.AddMessage(message);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("ActorManager.cs -> GetActorConflict: The selection Pool has no entries in it");
                            builder.AppendFormat("{0}{1} stamps their feet{2}", colourAlert, actor.arc.name, colourEnd);
                            builder.AppendFormat("{0}{1} Nothing happens{2}", "\n", colourGood, colourEnd);
                            //message
                            string msgText = string.Format("{0} Relationship Conflict ({1}Nothing Happens{2})", actor.arc.name, colourGood, colourEnd);
                            Message message = GameManager.instance.messageScript.ActorConflict(msgText, actor.actorID, 0, GameManager.instance.sideScript.PlayerSide);
                            GameManager.instance.dataScript.AddMessage(message);
                        }
                    }
                    else { Debug.LogWarning("No records in dictOfActorConflicts"); }
                }
                else { Debug.LogWarning("Invalid dictOFActorConflicts (Null)"); }
            }
            else
            {
                //trait actorConflictNone (Team Player)
                TraitLogMessage(actor, "to avoid a Relationship Conflict");
                builder.AppendFormat("{0}{1} has {2}{3}{4}{5} trait{6}", colourNormal, actor.arc.name, colourNeutral, actor.GetTrait().tag, colourEnd, colourNormal, colourEnd);
                builder.AppendFormat("{0}{1}Nothing happens{2}", "\n", colourGood, colourEnd);
                //message
                string msgText = string.Format("{0} Relationship Conflict ({1}Nothing Happens{2})", actor.arc.name, colourGood, colourEnd);
                Message message = GameManager.instance.messageScript.ActorConflict(msgText, actor.actorID, 0, GameManager.instance.sideScript.PlayerSide);
                GameManager.instance.dataScript.AddMessage(message);
            }
        }
        else { Debug.LogWarning("Invalid actor (Null)"); }
        //return two line outcome
        return builder.ToString();
    }


    /// <summary>
    /// ActorKiller murders a random OnMap actor (in your line-up). Returns a formatted output string (one line). Psychopath trait
    /// NOTE: actorKiller checked for Null by calling method (EffectManager.cs -> ResolveManageData
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    public string ProcessKillRandomActor(Actor actorKiller)
    {
        string outputMsg = "Unknown";
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(side);
        if (arrayOfActors != null)
        {
            List<Actor> listOfActors = new List<Actor>();
            //loop actors and add any that are present (active / inactive) to the list (excluding actorKiller)
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, side) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    {
                        if (actor.actorID != actorKiller.actorID)
                        { listOfActors.Add(actor); }
                    }
                    else { Debug.LogWarningFormat("Invalid actor (Null) in arrayOfActors[{0}]", i); }
                }
            }
            //randomly choose an actor from list
            int numOfActors = listOfActors.Count;
            if (numOfActors > 0)
            {
                Actor actorVictim = listOfActors[Random.Range(0, numOfActors)];
                //kill actor
                if (GameManager.instance.dataScript.RemoveCurrentActor(side, actorVictim, ActorStatus.Killed) == true)
                {
                    //actor should have trait but check just to be sure as there could, perhaps, be another reason for the random killing
                    if (actorKiller.CheckTraitEffect(actorConflictKill) == true)
                    {
                        //add trait to output
                        outputMsg = string.Format("{0}{1} killed by {2}{3}{4}{5}{6} {7}{8}  ", colourBad, actorVictim.arc.name, colourEnd, colourNeutral, actorKiller.GetTrait().tag, 
                            colourEnd, colourBad, actorKiller.arc.name, colourEnd);
                        TraitLogMessage(actorKiller, "to kill a friend");
                    }
                    else
                    { outputMsg = string.Format("{0}{1} killed by {2}{3}", colourBad, actorVictim.arc.name, actorKiller.arc.name, colourEnd); }
                    //victim message
                    string msgText = string.Format("{0} has been killed by {1}", actorVictim.arc.name, actorKiller.arc.name);
                    Message message = GameManager.instance.messageScript.ActorStatus(msgText, actorVictim.actorID, side, true);
                    GameManager.instance.dataScript.AddMessage(message);
                }
                else { outputMsg = string.Format("{0}{1} failed to kill somebody{2}", colourNeutral, actorKiller.arc.name, colourEnd); }
            }
            else
            {
                //no actors in pool
                outputMsg = string.Format("{0}{1} unable to find a victim{2}", colourNeutral, actorKiller.arc.name, colourEnd);
            }
        }
        else { Debug.LogWarning("Invalid arrayOfActors (Null)"); }
        return outputMsg;
    }

    /// <summary>
    /// Debug method to display actor pools (both sides)
    /// </summary>
    /// <returns></returns>
    public string DisplayPools()
    {
        StringBuilder builder = new StringBuilder();
        //Resistance
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorRecruitPool(1, globalResistance), " ResistanceActorPoolLevelOne"));
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorRecruitPool(2, globalResistance), " ResistanceActorPoolLevelTwo"));
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorRecruitPool(3, globalResistance), " ResistanceActorPoolLevelThree"));
        //Authority
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorRecruitPool(1, globalAuthority), " AuthorityActorPoolLevelOne"));
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorRecruitPool(2, globalAuthority), " AuthorityActorPoolLevelTwo"));
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorRecruitPool(3, globalAuthority), " AuthorityActorPoolLevelThree"));
        return builder.ToString();
    }

    /// <summary>
    /// submethod for DisplayPools
    /// </summary>
    /// <param name="listOfActors"></param>
    /// <param name="header"></param>
    /// <returns></returns>
    private string DisplaySubPool(List<int> listOfActors, string header)
    {
        StringBuilder subBuilder = new StringBuilder();
        subBuilder.Append(header);
        subBuilder.AppendLine();
        for (int i = 0; i < listOfActors.Count; i++)
        {
            Actor actor = GameManager.instance.dataScript.GetActor(listOfActors[i]);
            subBuilder.AppendFormat(" actID {0}, {1}, L{2}, {3}-{4}-{5}{6}", actor.actorID, actor.arc.name.ToLower(), actor.level,
                actor.datapoint0, actor.datapoint1, actor.datapoint2, "\n");
        }
        subBuilder.AppendLine();
        return subBuilder.ToString();
    }

    /// <summary>
    /// Debug method to add a condition to an actor (debug input). Both sides.
    /// </summary>
    /// <param name="what"></param>
    /// <param name="who"></param>
    /// <returns></returns>
    public string DebugAddCondition(string what, string who)
    {
        Debug.Assert(String.IsNullOrEmpty(what) == false && String.IsNullOrEmpty(who) == false, "Invalid input parameters (Who or What are Null or empty");
        string text = "";
        //Condition
        Condition condition = GameManager.instance.dataScript.GetCondition(what.ToUpper());
        if (condition != null)
        {
            //Who to? (0 to 3 actorSlotID's and 'p' or 'P' for Player
            switch (who)
            {
                case "0":
                case "1":
                case "2":
                case "3":
                    text = DebugAddConditionToActor(Convert.ToInt32(who), condition);
                    break;
                case "p":
                case "P":
                    text = DebugAddConditionToPlayer(condition);
                    break;
            }
        }
        else { text = "Input Condition is INVALID and is NOT added"; }
        return text;
    }

    /// <summary>
    /// subMethod for DebugAddCondition to add a condition to the player. Returns a string indicating success, or otherwise
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    private string DebugAddConditionToPlayer(Condition condition)
    {
        Debug.Assert(condition != null, "Invalid Condition (Null)");
        string text = "Unknown";
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        //does actor already have the condition?
        if (GameManager.instance.playerScript.CheckConditionPresent(condition) == false)
        {
            //add condition
            GameManager.instance.playerScript.AddCondition(condition);
            text = string.Format("Condition {0} added to Player", condition.name);
        }
        else { text = string.Format("Player already has Condition {0}", condition.name); }
        return string.Format("{0}{1}Press ESC to Exit", text, "\n");
    }

    /// <summary>
    /// subMethod for DebugAddCondition to add a condition to an actor and return a string indicating success, or otherwise
    /// </summary>
    /// <param name="slotID"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    private string DebugAddConditionToActor(int actorSlotID, Condition condition)
    {
        Debug.Assert(actorSlotID > -1 && actorSlotID < maxNumOfOnMapActors, string.Format("Invalid actorSlotID {0}", actorSlotID));
        Debug.Assert(condition != null, "Invalid Condition (Null)");
        string text = "Unknown";
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        //Get actor
        if (GameManager.instance.dataScript.CheckActorSlotStatus(actorSlotID, side) == true)
        {
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, side);
            if (actor != null)
            {
                //does actor already have the condition?
                if (actor.CheckConditionPresent(condition) == false)
                {
                    //add condition
                    if (actor.AddCondition(condition) == true)
                    { text = string.Format("Condition {0} added to {1}, {2}", condition.name, actor.arc.name, actor.actorName); }
                    else { text = string.Format("Condition {0} NOT added", condition.name); }
                }
                else { text = string.Format("{0} already has Condition {1}", actor.arc.name, condition.name); }
            }
            else { text = string.Format("There is no valid Actor (Null) in Slot {0}", actorSlotID); }
        }
        else { text = string.Format("There is no Actor Present in Slot {0}", actorSlotID); }
        return string.Format("{0}{1}Press ESC to Exit", text, "\n");
    }

    /// <summary>
    /// Debug method to add a trait to an actor (debug input). Both sides.
    /// </summary>
    /// <param name="what"></param>
    /// <param name="who"></param>
    public string DebugAddTrait(string what, string who)
    {
        Debug.Assert(String.IsNullOrEmpty(what) == false && String.IsNullOrEmpty(who) == false, "Invalid input parameters (Who or What are Null or empty");
        string text = "";
        int i;
        if (int.TryParse(who, out i) == true)
        {
            
            int actorSlotID = Convert.ToInt32(who);
            //Trait
            Trait trait = GameManager.instance.dataScript.GetTrait(what);
            if (trait != null)
            {
                GlobalSide side = GameManager.instance.sideScript.PlayerSide;

                //Get actor
                if (GameManager.instance.dataScript.CheckActorSlotStatus(actorSlotID, side) == true)
                {
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, side);
                    if (actor != null)
                    {
                        //add trait
                        actor.AddTrait(trait);
                        text = string.Format("Trait {0} added to {1}, {2}", trait.tag, actor.arc.name, actor.actorName);
                    }
                    else { text = string.Format("There is no valid Actor (Null) in Slot {0}", actorSlotID); }
                }
                else { text = string.Format("There is no valid Actor Present in Slot {0}", actorSlotID); }
            }
            else { text = "Input Trait is INVALID and is NOT added"; }
        }
        else { text = "Invalid numeric input for 'who'"; }
        return string.Format("{0}{1}Press ESC to Exit", text, "\n");
    }



    /// <summary>
    /// Checks all OnMap Inactive Resistance actors, increments invisibility and returns any at max value back to Active status
    /// </summary>
    private void CheckInactiveResistanceActors()
    {
        // Resistance actors only
        Actor[] arrayOfActorsResistance = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
        if (arrayOfActorsResistance != null)
        {
            for (int i = 0; i < arrayOfActorsResistance.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                {
                    Actor actor = arrayOfActorsResistance[i];
                    if (actor != null)
                    {
                        if (actor.Status == ActorStatus.Inactive)
                        {
                            switch (actor.inactiveStatus)
                            {
                                case ActorInactive.LieLow:
                                    int invis = actor.datapoint2;
                                    //increment invisibility (not the first turn)
                                    if (actor.isLieLowFirstturn == false)
                                    { invis++; }
                                    else { actor.isLieLowFirstturn = false; }
                                    if (invis >= maxStatValue)
                                    {
                                        //actor has recovered from lying low, needs to be activated
                                        actor.datapoint2 = Mathf.Min(maxStatValue, actor.datapoint2);
                                        actor.Status = ActorStatus.Active;
                                        actor.inactiveStatus = ActorInactive.None;
                                        actor.tooltipStatus = ActorTooltip.None;
                                        GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.actorSlotID, GameManager.instance.guiScript.alphaActive);
                                        //message -> status change
                                        string text = string.Format("{0} {1} has automatically reactivated", actor.arc.name, actor.actorName);
                                        Message message = GameManager.instance.messageScript.ActorStatus(text, actor.actorID, globalResistance, true);
                                        GameManager.instance.dataScript.AddMessage(message);
                                        //check if actor has stressed condition
                                        if (actor.CheckConditionPresent(conditionStressed) == true)
                                        {
                                            if (actor.RemoveCondition(conditionStressed) == true)
                                            {
                                                //message -> condition change
                                                text = string.Format("{0}, {1}, is no longer Stressed (Lie Low)", actor.arc.name, actor.actorName);
                                                message = GameManager.instance.messageScript.ActorCondition(text, actor.actorID, globalResistance, true);
                                                GameManager.instance.dataScript.AddMessage(message);
                                            }
                                        }
                                    }
                                    else
                                    { actor.datapoint2 = invis; }
                                    break;
                                case ActorInactive.Breakdown:
                                    //restore actor (one stress turn only)
                                    actor.Status = ActorStatus.Active;
                                    actor.inactiveStatus = ActorInactive.None;
                                    actor.tooltipStatus = ActorTooltip.None;
                                    GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.actorSlotID, GameManager.instance.guiScript.alphaActive);
                                    string textBreakdown = string.Format("{0}, {1}, has recovered from their Breakdown", actor.arc.name, actor.actorName);
                                    Message messageBreakdown = GameManager.instance.messageScript.ActorStatus(textBreakdown, actor.actorID, globalResistance, true);
                                    GameManager.instance.dataScript.AddMessage(messageBreakdown);
                                    break;
                            }
                        }
                    }
                    else { Debug.LogError(string.Format("Invalid Resistance actor (Null), index {0}", i)); }
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfActorsResistance (Null)"); }
    }

    /// <summary>
    /// Checks all active resistance actors (run AFTER checkInactiveResistanceActors). No checks are made if Player is not Active
    /// </summary>
    private void CheckActiveResistanceActors()
    {
        //no checks are made if player is not Active
        if (GameManager.instance.playerScript.status == ActorStatus.Active)
        {
            Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
            if (arrayOfActors != null)
            {

                bool isSecrets = false;
                if (GameManager.instance.playerScript.CheckNumOfSecrets() > 0) { isSecrets = true; }
                int chanceBreakdown = breakdownChance;
                //base chance of nervous breakdown doubled during a surveillance crackdown
                if (GameManager.instance.turnScript.authoritySecurityState == AuthoritySecurityState.SurveillanceCrackdown)
                { chanceBreakdown *= 2; }
                //secrets
                int chanceSecret = secretBaseChance;
                List<Secret> listOfSecrets = GameManager.instance.playerScript.GetListOfSecrets();
                //compatibility (actors with player)
                List<Condition> listOfBadConditions = GameManager.instance.playerScript.GetNumOfBadConditionPresent();
                if (listOfBadConditions.Count > 0)
                {
                    //warning message
                    Message message = GameManager.instance.messageScript.GeneralWarning("Your subordinates are considering resigning (over your status)");
                    GameManager.instance.dataScript.AddMessage(message);
                }
                //
                // - - - loop Active actors - - -
                //
                for (int i = 0; i < arrayOfActors.Length; i++)
                {
                    //check actor is present in slot (not vacant)
                    if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                    {
                        Actor actor = arrayOfActors[i];
                        if (actor != null)
                        {
                            if (actor.Status == ActorStatus.Active)
                            {
                                //
                                // - - - Stress Condition - - -
                                //
                                if (actor.CheckConditionPresent(conditionStressed) == true)
                                { ProcessStress(actor, chanceBreakdown); }
                                //
                                // - - - Learn Secrets - - -
                                //
                                if (isSecrets == true)
                                { ProcessSecrets(actor, listOfSecrets, chanceSecret); }
                                //
                                // - - - Blackmailing - - -
                                //
                                if (actor.blackmailTimer > 0)
                                {
                                    string blackmailOutcome = ProcessBlackmail(actor);
                                    if (string.IsNullOrEmpty(blackmailOutcome) == false)
                                    {
                                        //TO DO -> Notification to player of black mail outcome (start turn) -> already done via Messages -> improve Notification system needed
                                    }
                                }
                                //
                                // - - - Compatibility - - -
                                //
                                if (listOfBadConditions.Count > 0)
                                { ProcessCompatibility(actor, listOfBadConditions); }
                            }
                        }
                        else { Debug.LogError(string.Format("Invalid Resistance actor (Null), index {0}", i)); }
                    }
                }
            }
            else { Debug.LogError("Invalid arrayOfActors (Resistance) (Null)"); }
        }
    }

    /// <summary>
    /// Checks all active authority actors (run AFTER checkInactiveAuthorityActors). No checks are made if the player is not Active
    /// </summary>
    private void CheckActiveAuthorityActors()
    {
        //no checks are made if player is not Active
        if (GameManager.instance.playerScript.status == ActorStatus.Active)
        {
            Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalAuthority);
            if (arrayOfActors != null)
            {
                int chanceBreakdown = breakdownChance;
                //base chance of nervous breakdown doubled during a surveillance crackdown
                if (GameManager.instance.turnScript.authoritySecurityState == AuthoritySecurityState.SurveillanceCrackdown)
                { chanceBreakdown *= 2; }
                //secrets
                int chanceSecret = secretBaseChance;
                List<Secret> listOfSecrets = GameManager.instance.playerScript.GetListOfSecrets();
                bool isSecrets = false;
                if (GameManager.instance.playerScript.CheckNumOfSecrets() > 0) { isSecrets = true; }
                //compatibility (actors with player)
                List<Condition> listOfBadConditions = GameManager.instance.playerScript.GetNumOfBadConditionPresent();
                if (listOfBadConditions.Count > 0)
                {
                    //warning message
                    Message message = GameManager.instance.messageScript.GeneralWarning("Your subordinates are considering resigning (over your status)");
                    GameManager.instance.dataScript.AddMessage(message);
                }
                //loop actors
                for (int i = 0; i < arrayOfActors.Length; i++)
                {
                    //check actor is present in slot (not vacant)
                    if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalAuthority) == true)
                    {
                        Actor actor = arrayOfActors[i];
                        if (actor != null)
                        {
                            if (actor.Status == ActorStatus.Active)
                            {
                                //
                                // - - - Stress Condition - - -
                                //
                                if (actor.CheckConditionPresent(conditionStressed) == true)
                                { ProcessStress(actor, chanceBreakdown); }
                                //
                                // - - - Learn Secrets - - -
                                //
                                if (isSecrets == true)
                                { ProcessSecrets(actor, listOfSecrets, chanceSecret); }
                                //
                                // - - - Blackmailing - - -
                                //
                                if (actor.blackmailTimer > 0)
                                {
                                    string blackmailOutcome = ProcessBlackmail(actor);
                                    if (string.IsNullOrEmpty(blackmailOutcome) == false)
                                    {
                                        //TO DO -> Notification to player of black mail outcome (start turn)
                                    }
                                }
                                //
                                // - - - Compatibility - - -
                                //
                                if (listOfBadConditions.Count > 0)
                                { ProcessCompatibility(actor, listOfBadConditions); }
                            }
                        }
                        else { Debug.LogError(string.Format("Invalid Authority actor (Null), index {0}", i)); }
                    }
                }
            }
            else { Debug.LogError("Invalid arrayOfActors (Authority) (Null)"); }
        }
    }


    /// <summary>
    /// sub Method to check Stress condition each turn. Called by CheckActive(Resistance/Authority)Actors.
    /// NOTE: Actor has been checked for null by calling method
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    private void ProcessStress(Actor actor, int chance)
    {
        //enforces a minimum one turn gap between successive breakdowns
        if (actor.isBreakdown == false)
        {
            //Trait Check
            if (actor.CheckTraitEffect(actorBreakdownChanceHigh) == true)
            { chance *= 2; TraitLogMessage(actor, "for a Nervous Breakdown check (doubled)"); }
            else if (actor.CheckTraitEffect(actorBreakdownChanceLow) == true)
            { chance /= 2; TraitLogMessage(actor, " for a Nervous Breakdown check (halved)"); }
            else if (actor.CheckTraitEffect(actorBreakdownChanceNone) == true)
            { chance = 0; TraitLogMessage(actor, "to prevent a Nervous Breakdown"); }
            //test
            int rnd = Random.Range(0, 100);
            if (rnd < chance)
            {
                //actor suffers a breakdown
                ActorBreakdown(actor, globalResistance);
                Debug.LogFormat("[Rnd] ActorManager.cs -> CheckActiveResistanceActors: Stress check FAILED -> need < {0}, rolled {1}{2}",
                    chance, rnd, "\n");
                string text = string.Format("{0}, {1}, Stress check FAILED need < {2}, rolled {3}", actor.actorName, actor.arc.name, chance, rnd);
                Message message = GameManager.instance.messageScript.GeneralRandom(text, chance, rnd);
                GameManager.instance.dataScript.AddMessage(message);
            }
        }
        else { actor.isBreakdown = false; }
    }


    /// <summary>
    /// sub Method to check Blackmail condition each turn. Called by CheckActive(Resistance/Authority)Actors. Returns builder string of effects or null if none.
    /// NOTE: Actor has been checked for null by calling method
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    private string ProcessBlackmail(Actor actor)
    {
        string text = null;
        bool isResolved = false;
        //decrement timer
        actor.blackmailTimer--;
        if (actor.datapoint1 == maxStatValue)
        {
            //trait Vindictive (won't be appeased)
            if (actor.CheckTraitEffect(actorAppeaseNone) == false)
            {
                //Motivation at max value, Blackmailer condition cancelled
                actor.RemoveCondition(conditionBlackmailer);
                isResolved = true;
                //message
                string msgText = string.Format("{0} has full Motivation and has dropped their threat", actor.arc.name);
                Message message = GameManager.instance.messageScript.ActorBlackmail(msgText, actor.actorID);
                GameManager.instance.dataScript.AddMessage(message);
            }
            else
            { TraitLogMessage(actor, "to avoid being bought-off"); }
        }
        if (isResolved == false)
        {
            if (actor.blackmailTimer == 0)
            {
                //
                // - - - Actor REVEALS secret - - -
                //
                Secret secret = actor.GetSecret();
                if (secret != null)
                {
                    secret.revealedWho = actor.actorID;
                    secret.revealedWhen = GameManager.instance.turnScript.Turn;
                    secret.status = GameManager.instance.secretScript.secretStatusRevealed;
                    StringBuilder builder = new StringBuilder();
                    //message
                    string msgText = string.Format("{0} reveals your secret (\"{1}\")", actor.arc.name, secret.tag);
                    Message message = GameManager.instance.messageScript.ActorBlackmail(msgText, actor.actorID, secret.secretID);
                    GameManager.instance.dataScript.AddMessage(message);
                    //carry out effects
                    if (secret.listOfEffects != null)
                    {
                        //data packages
                        EffectDataReturn effectReturn = new EffectDataReturn();
                        EffectDataInput effectInput = new EffectDataInput();
                        effectInput.textOrigin = "Reveal Secret";
                        Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                        if (node != null)
                        {
                            //loop effects
                            foreach (Effect effect in secret.listOfEffects)
                            {
                                effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, effectInput);
                                if (builder.Length > 0) { builder.AppendLine(); builder.AppendLine(); }
                                builder.AppendFormat("{0}{1}{2}", effectReturn.topText, "\n", effectReturn.bottomText);
                                //temp message
                                if (string.IsNullOrEmpty(effectReturn.bottomText) == false)
                                {
                                    string textSecret = string.Format("Secret Revealed ({0})", effectReturn.bottomText);
                                    Message messageSecret = GameManager.instance.messageScript.ActorBlackmail(textSecret, actor.actorID, secret.secretID);
                                    GameManager.instance.dataScript.AddMessage(messageSecret);
                                }
                            }
                        }
                        else { Debug.LogWarning("Invalid player node (Null)"); }
                        //return builder output (all effects colour formatted, two lines each and a double space betweeen
                        text = builder.ToString();
                    }
                    //Motivation at max value, Blackmailer condition cancelled
                    actor.RemoveCondition(conditionBlackmailer);
                    //remove secret from all actors and player
                    GameManager.instance.secretScript.RemoveSecretFromAll(secret.secretID);
                }
                else { Debug.LogWarning("Invalid Secret (Null) -> Not revealed"); }
            }
            else
            {
                //warning message
                string textWarning = string.Format("{0} is Blackmailing you and will reveal your secret in {1} turn{2}", actor.arc.name, actor.blackmailTimer,
                    actor.blackmailTimer != 1 ? "s" : "");
                Message message = GameManager.instance.messageScript.GeneralWarning(textWarning);
                GameManager.instance.dataScript.AddMessage(message);
            }
        }
        return text;
    }

    /// <summary>
    /// sub Method to check if actors learn of Player Secrets each turn. Called by CheckActive(Resistance/Authority)Actors.
    /// NOTE: Actor has been checked for null by calling method
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>  
    private void ProcessSecrets(Actor actor, List<Secret> listOfSecrets, int chance)
    {
        if (actor != null)
        {
            int rnd;
            bool isProceed;
            //actor already knows any secrets
            bool knowSecret = false;
            if (actor.CheckNumOfSecrets() > 0) { knowSecret = true; }
            //trait Detective
            if (actor.CheckTraitEffect(actorSecretChanceHigh) == true)
            {
                chance *= 3;
                TraitLogMessage(actor, "triple chance of learning Secrets");
            }
            //trait Blind
            if (actor.CheckTraitEffect(actorSecretChanceNone) == false)
            {
                //loop through Player secrets
                for (int i = 0; i < listOfSecrets.Count; i++)
                {
                    isProceed = true;
                    Secret secret = listOfSecrets[i];
                    if (secret != null)
                    {
                        //does actor already know the secret
                        if (knowSecret == true)
                        {
                            if (secret.CheckActorPresent(actor.actorID) == true)
                            { isProceed = false; }
                        }
                        if (isProceed == true)
                        {
                            //does actor learn of secret
                            rnd = Random.Range(0, 100);
                            if (rnd < chance)
                            {
                                //actor learns of secret
                                actor.AddSecret(secret);
                                secret.AddActor(actor.actorID);
                                //Admin
                                Debug.LogFormat("[Rnd] PlayerManager.cs -> CheckForSecrets: {0} learned SECRET need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                                string text = string.Format("{0}, {1}, learned SECRET need < {2}, rolled {3}", actor.actorName, actor.arc.name, chance, rnd);
                                Message message = GameManager.instance.messageScript.GeneralRandom(text, chance, rnd);
                                GameManager.instance.dataScript.AddMessage(message);
                                //trait Blabbermouth
                                if (actor.CheckTraitEffect(actorSecretTellAll) == true)
                                {
                                    //actor passes secret onto all other actors
                                    int numTold = ProcessSecretTellAll(secret, actor);
                                    TraitLogMessage(actor, string.Format("tell {0} other {1} about the secret", numTold, numTold != 1 ? "people" : "person"));
                                }
                            }
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid secret (Null) in listOFSecrets[{0}]", i); }
                }
            }
            else { TraitLogMessage(actor, "to not learn Secrets"); }
        }
        else { Debug.LogWarning("Invalid actor (Null)"); }
    }

    /// <summary>
    /// subMethod for ProcessSecrets to handle trait Blabbermouth where an actor tells all other actors about the secret they have learned. Actor is the actor who learned of secret
    /// NOTE: actorTrait & secret checked for null by calling method. Actors are told of secret regardless whether they are active or inactive or have 'Blind' trait
    /// </summary>
    /// <param name="secret"></param>
    /// <returns></returns>
    private int ProcessSecretTellAll(Secret secret, Actor actorTrait)
    {
        int numTold = 0;
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
        //loop all actors on Map
        for (int i = 0; i < arrayOfActors.Length; i++)
        {
            //check actor is present in slot (not vacant)
            if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
            {
                Actor actor = arrayOfActors[i];
                if (actor != null)
                {
                    //exclude actor with the trait who is telling everybody else
                    if (actor.actorID != actorTrait.actorID)
                    {
                        //actor doesn't already know the secret
                        if (secret.CheckActorPresent(actor.actorID) == false)
                        {
                            //actor learns of secret
                            actor.AddSecret(secret);
                            secret.AddActor(actor.actorID);
                            numTold++;
                        }
                    }
                }
            }
        }
        return numTold;
    }

    /// <summary>
    /// subMethod to check if actors resign due to the Player having a bad condition (Corrupt/Incompetent/Questionable)
    /// NOTE: Actor checked for null by calling method
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="numOfBadConditions"></param>
    private void ProcessCompatibility(Actor actor, List<Condition> listOfBadConditions)
    {
        if (listOfBadConditions != null)
        {
            int numOfBadConditions = listOfBadConditions.Count;
            if (numOfBadConditions > 0)
            {
                string msgText = "";
                GlobalSide side = GameManager.instance.sideScript.PlayerSide;
                //proceed only if actor doesn't have do not resign trait
                if (actor.CheckTraitEffect(actorNeverResigns) == false)
                {
                    int chance = playerBadResignChance * numOfBadConditions;
                    //trait check -> 'Ethical' (triple chance of resigning)
                    if (actor.CheckTraitEffect(actorResignHigh) == true)
                    { chance *= 3; }
                    int rnd = Random.Range(0, 100);
                    if (rnd < chance)
                    {
                        Debug.LogFormat("[Rnd] ActorManager.cs -> ProcessCompatibility: RESIGNS need < {0}, rolled {1}{2}", chance, rnd, "\n");
                        string text = string.Format("{0}, {1}, RESIGNS need < {2}, rolled {3}", actor.actorName, actor.arc.name, chance, rnd);
                        Message message = GameManager.instance.messageScript.GeneralRandom(text, chance, rnd);
                        GameManager.instance.dataScript.AddMessage(message);
                        //actor resigns
                        if (GameManager.instance.dataScript.RemoveCurrentActor(side, actor, ActorStatus.Resigned) == true)
                        {

                            //choose a random condition that actor is upset about
                            Condition conditionUpsetOver = listOfBadConditions[Random.Range(0, numOfBadConditions)];
                            if (conditionUpsetOver != null)
                            {
                                if (String.IsNullOrEmpty(conditionUpsetOver.resignTag) == false)
                                { msgText = string.Format("{0} Resigns (over Player's {1})", actor.arc.name, conditionUpsetOver.resignTag); }
                                else { Debug.LogWarning("Invalid conditionUpsetOver.resignTag (Null or empty)"); }
                            }
                            else { Debug.LogWarning("Invalid conditionUpsetOver (Null)"); }
                        }

                        //message
                        if (String.IsNullOrEmpty(msgText) == false)
                        {
                            message = GameManager.instance.messageScript.ActorStatus(msgText, actor.actorID, side, true);
                            GameManager.instance.dataScript.AddMessage(message);
                        }
                    }
                }
                else
                {
                    //trait actorResignNone "Loyal"
                    GameManager.instance.actorScript.TraitLogMessage(actor, "to decline Resigning");
                }
            }
            else { Debug.LogWarning("Invalid listOfBadConditions (empty)"); }
        }
        else { Debug.LogWarning("Invalid listOfBadConditions (Null)"); }
    }

    /// <summary>
    /// Checks all OnMap Inactive Authority actors, increments invisibility and returns any at max value back to Active status
    /// </summary>
    private void CheckInactiveAuthorityActors()
    {
        // Authority actors only
        Actor[] arrayOfActorsAuthority = GameManager.instance.dataScript.GetCurrentActors(globalAuthority);
        if (arrayOfActorsAuthority != null)
        {
            for (int i = 0; i < arrayOfActorsAuthority.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalAuthority) == true)
                {
                    Actor actor = arrayOfActorsAuthority[i];
                    if (actor != null)
                    {
                        if (actor.Status == ActorStatus.Inactive)
                        {
                            switch (actor.inactiveStatus)
                            {
                                case ActorInactive.Breakdown:
                                    //restore actor (one stress turn only)
                                    actor.Status = ActorStatus.Active;
                                    actor.inactiveStatus = ActorInactive.None;
                                    actor.tooltipStatus = ActorTooltip.None;
                                    GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.actorSlotID, GameManager.instance.guiScript.alphaActive);
                                    string textBreakdown = string.Format("{0}, {1}, has recovered from their Breakdown", actor.arc.name, actor.actorName);
                                    Message messageBreakdown = GameManager.instance.messageScript.ActorStatus(textBreakdown, actor.actorID, globalAuthority, true);
                                    GameManager.instance.dataScript.AddMessage(messageBreakdown);
                                    break;
                            }
                        }
                    }
                    else { Debug.LogError(string.Format("Invalid Authority actor (Null), index {0}", i)); }
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfActorsAuthority (Null)"); }
    }

    /// <summary>
    /// handles all admin for an Actor having a breakdown as a result of having the Stressed condition
    /// </summary>
    /// <param name="actor"></param>
    private void ActorBreakdown(Actor actor, GlobalSide side)
    {
        if (actor != null)
        {
            actor.Status = ActorStatus.Inactive;
            actor.inactiveStatus = ActorInactive.Breakdown;
            actor.tooltipStatus = ActorTooltip.Breakdown;
            actor.isBreakdown = true;
            //change alpha of actor to indicate inactive status
            GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.actorSlotID, GameManager.instance.guiScript.alphaInactive);
            //message (public)
            string text = string.Format("{0}, {1}, has suffered a Breakdown (Stressed)", actor.actorName, actor.arc.name);
            Message message = GameManager.instance.messageScript.ActorStatus(text, actor.actorID, side, true);
            GameManager.instance.dataScript.AddMessage(message);
        }
        else { Debug.LogError("Invalid actor (Null)"); }
    }

    /// <summary>
    /// runs all start late turn player checks
    /// </summary>
    private void CheckPlayerStartLate()
    {
        int rnd;
        string playerName = GameManager.instance.playerScript.PlayerName;
        //check for Stress Nervous breakdown -> both sides
        switch (GameManager.instance.playerScript.status)
        {
            case ActorStatus.Inactive:
                switch (GameManager.instance.playerScript.inactiveStatus)
                {
                    case ActorInactive.Breakdown:
                        //restore player (one stress turn only)
                        GameManager.instance.playerScript.status = ActorStatus.Active;
                        GameManager.instance.playerScript.inactiveStatus = ActorInactive.None;
                        GameManager.instance.playerScript.tooltipStatus = ActorTooltip.None;
                        GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaActive);
                        string textBreakdown = string.Format("{0} has recovered from their Breakdown", playerName);
                        Message messageBreakdown = GameManager.instance.messageScript.ActorStatus(textBreakdown, GameManager.instance.playerScript.actorID, GameManager.instance.sideScript.PlayerSide, true);
                        GameManager.instance.dataScript.AddMessage(messageBreakdown);
                        //update AI side tab status
                        GameManager.instance.aiScript.UpdateSideTabData();
                        break;
                    case ActorInactive.LieLow:
                        int invis = GameManager.instance.playerScript.Invisibility;
                        //increment invisibility (not the first turn)
                        if (GameManager.instance.playerScript.isLieLowFirstturn == false)
                        { invis++; }
                        else { GameManager.instance.playerScript.isLieLowFirstturn = false; }
                        if (invis >= maxStatValue)
                        {
                            //player has recovered from lying low, needs to be activated
                            GameManager.instance.playerScript.Invisibility = Mathf.Min(maxStatValue, invis);
                            GameManager.instance.playerScript.status = ActorStatus.Active;
                            GameManager.instance.playerScript.inactiveStatus = ActorInactive.None;
                            GameManager.instance.playerScript.tooltipStatus = ActorTooltip.None;
                            GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaActive);
                            //message -> status change
                            string text = string.Format("{0} has automatically reactivated", playerName);
                            Message message = GameManager.instance.messageScript.ActorStatus(text, GameManager.instance.playerScript.actorID, globalResistance, true);
                            GameManager.instance.dataScript.AddMessage(message);
                            //check if Player has stressed condition
                            if (GameManager.instance.playerScript.CheckConditionPresent(conditionStressed) == true)
                            {
                                if (GameManager.instance.playerScript.RemoveCondition(conditionStressed) == true)
                                {
                                    //message -> condition change
                                    text = string.Format("{0} is no longer Stressed (Lie Low)", playerName);
                                    message = GameManager.instance.messageScript.ActorCondition(text, GameManager.instance.playerScript.actorID, globalResistance, true);
                                    GameManager.instance.dataScript.AddMessage(message);
                                }
                            }
                        }
                        else
                        {  GameManager.instance.playerScript.Invisibility = invis; }
                        break;
                }
                break;
            case ActorStatus.Active:
                {
                    //check any actors with the stressed condition for a breakdown
                    if (GameManager.instance.playerScript.CheckConditionPresent(conditionStressed) == true)
                    {
                        //enforces a minimum one turn gap between successive breakdowns
                        if (GameManager.instance.playerScript.isBreakdown == false)
                        {
                            rnd = Random.Range(0, 100);
                            if ( rnd < breakdownChance)
                            {
                                //player Breakdown
                                GameManager.instance.playerScript.status = ActorStatus.Inactive;
                                GameManager.instance.playerScript.inactiveStatus = ActorInactive.Breakdown;
                                GameManager.instance.playerScript.tooltipStatus = ActorTooltip.Breakdown;
                                GameManager.instance.playerScript.isBreakdown = true;
                                //change alpha of actor to indicate inactive status
                                GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaInactive);
                                //message (public)
                                string text = "Player has suffered a Breakdown (Stressed)";
                                Message message = GameManager.instance.messageScript.ActorStatus(text, GameManager.instance.playerScript.actorID, GameManager.instance.sideScript.PlayerSide, true);
                                GameManager.instance.dataScript.AddMessage(message);
                                Debug.LogFormat("[Rnd] ActorManager.cs -> CheckPlayerStartlate: Stress check FAILED -> need < {0}, rolled {1}{2}",
                                    breakdownChance, rnd, "\n");
                                text = string.Format("Player Stress check FAILED -> need < {0}, rolled {1}", breakdownChance, rnd);
                                message = GameManager.instance.messageScript.GeneralRandom(text, breakdownChance, rnd);
                                GameManager.instance.dataScript.AddMessage(message);
                                //update AI side tab status
                                GameManager.instance.aiScript.UpdateSideTabData();
                            }
                        }
                        else { GameManager.instance.playerScript.isBreakdown = false; }
                    }
                }
                break;
                //NO Default case here, only check for what you are interested in
        }
    }

    /// <summary>
    /// Checks all reserve pool actors (both sides), decrements unhappy timers and takes appropriate action if any have reached zero
    /// </summary>
    private void UpdateReserveActors()
    {
        List<int> listOfActors = null;
        int chance;
        //
        // - - - Resistance - - -
        //
        listOfActors = GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Reserve);
        if (listOfActors != null)
        {
            //loop list of reserve actors
            for (int i = 0; i < listOfActors.Count; i++)
            {
                Actor actor = GameManager.instance.dataScript.GetActor(listOfActors[i]);
                if (actor != null)
                {
                    //Decrement unhappy timer if not yet zero
                    if (actor.unhappyTimer > 0)
                    {
                        actor.unhappyTimer--;
                        Debug.Log(string.Format("CheckReserveActors: Resistance {0} {1} unhappy timer now {2}{3}", actor.arc.name, actor.actorName, actor.unhappyTimer, "\n"));
                        //if timer now zero, gain condition "Unhappy"
                        if (actor.unhappyTimer == 0)
                        {
                            Condition condition = GameManager.instance.dataScript.GetCondition("UNHAPPY");
                            actor.AddCondition(condition);
                        }
                    }
                    else
                    {
                        //unhappy timer has reached zero. Is actor's motivation > 0?
                        if (actor.datapoint1 > 0)
                        {
                            //chance of decrementing motivation each turn till it reaches zero
                            chance = unhappyLoseMotivationChance;
                            //chance is 100% if actor was promised
                            if (actor.isPromised == true)
                            { chance = 100; }
                            if (Random.Range(0, 100) < chance)
                            {
                                actor.datapoint1--;
                                Debug.Log(string.Format("CheckReserveActors: Resistance {0} {1} UNHAPPY, Motivation now {2}{3}", actor.arc.name, actor.actorName, 
                                    actor.datapoint1, "\n"));
                            }
                        }
                        else
                        {
                            //actor is Unhappy and has 0 motivation. Do they take action?
                            chance = unhappyTakeActionChance;
                            //if actor has previously been reassured, double chance of action
                            if (actor.isReassured == true)
                            { chance *= 2; }
                            if (Random.Range(0, 100) < chance)
                            {
                                Debug.Log(string.Format("CheckReserveActors: Resistance {0} {1} takes ACTION {2}", actor.arc.name, actor.actorName, "\n"));
                                TakeAction(actor);
                            }
                        }
                    }
                }
                else { Debug.LogError(string.Format("Invalid Resitance actor (Null) for actorID {0}", listOfActors[i])); }
            }
        }
        else { Debug.LogError("Invalid listOfActors -> Resistance (Null)"); }
        //
        // - - - Authority - - -
        //
        listOfActors = GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Reserve);
        if (listOfActors != null)
        {
            //loop list of reserve actors
            for (int i = 0; i < listOfActors.Count; i++)
            {
                Actor actor = GameManager.instance.dataScript.GetActor(listOfActors[i]);
                if (actor != null)
                {
                    //Decrement unhappy timer if not yet zero
                    if (actor.unhappyTimer > 0)
                    {
                        actor.unhappyTimer--;
                        Debug.Log(string.Format("CheckReserveActors: Authority {0} {1} unhappy timer now {2}{3}", actor.arc.name, actor.actorName, actor.unhappyTimer, "\n"));
                        //if timer now zero, gain condition "Unhappy"
                        if (actor.unhappyTimer == 0)
                        {
                            Condition condition = GameManager.instance.dataScript.GetCondition("UNHAPPY");
                            actor.AddCondition(condition);
                        }
                    }
                    else
                    {
                        //unhappy timer has reached zero. Is actor's motivation > 0?
                        if (actor.datapoint1 > 0)
                        {
                            //chance of decrementing motivation each turn till it reaches zero
                            chance = unhappyLoseMotivationChance;
                            //chance is 100% if actor was promised
                            if (actor.isPromised == true)
                            { chance = 100; }
                            if (Random.Range(0, 100) < chance)
                            {
                                actor.datapoint1--;
                                Debug.Log(string.Format("CheckReserveActors: Authority {0} {1} UNHAPPY, Motivation now {2}, chance {3}{4}", actor.arc.name, 
                                    actor.actorName, actor.datapoint1, chance, "\n"));
                            }
                        }
                        else
                        {
                            //actor is Unhappy and has 0 motivation. Do they take action?
                            chance = unhappyTakeActionChance;
                            //if actor has previously been reassured, double chance of action
                            if (actor.isReassured == true)
                            { chance *= 2; }
                            if (Random.Range(0, 100) < chance)
                            {
                                Debug.Log(string.Format("CheckReserveActors: Authority {0} {1} takes ACTION, chance {2}{3}", actor.arc.name, actor.actorName, 
                                    chance, "\n"));
                                TakeAction(actor);
                            }
                        }
                    }
                }
                else { Debug.LogError(string.Format("Invalid Authority actor (Null) for actorID {0}", listOfActors[i])); }
            }
        }
        else { Debug.LogError("Invalid listOfActors -> Authority (Null)"); }
    }

    /// <summary>
    /// An unhappy actor takes action (both sides)
    /// NOTE: Actor is assumed to be Null checked by the calling method
    /// </summary>
    /// <param name="actor"></param>
    private void TakeAction(Actor actor)
    {
        //Check for secrets first
        if (actor.CheckNumOfSecrets() > 0)
        {
            if (Random.Range(0, 100) < unhappyRevealSecretChance)
            {

                //TO DO
                Debug.Log(string.Format("Unhappy Actor: {0} {1} Threatens to reveal a Secret{2}", actor.arc.name, actor.actorName, "\n"));
                return;
            }
        }
        //Check for Leaving second
        if (actor.isComplaining == false)
        {
            if (Random.Range(0, 100) < (unhappyLeaveChance))
            {

                //TO DO
                Debug.Log(string.Format("Unhappy Actor: {0} {1} Threatens to Leave{2}", actor.arc.name, actor.actorName, "\n"));
                return;
            }
        }
        else
        {
            //double chance if actor has already complained
            if (Random.Range(0, 100) < (unhappyLeaveChance * 2))
            {

                //TO DO
                Debug.Log(string.Format("Unhappy Actor: {0} {1} Threatens to Leave{2}", actor.arc.name, actor.actorName, "\n"));
                return;
            }
        }
        //Check for Complaint third (skip check if actor has already complained)
        if (actor.isComplaining == false)
        {
            if (Random.Range(0, 100) < unhappyComplainChance)
            {

                //TO DO
                Debug.Log(string.Format("Unhappy Actor: {0} {1} Threatens to Complain{2}", actor.arc.name, actor.actorName, "\n"));
                actor.isComplaining = true;
                return;
            }
        }


    }

    /// <summary>
    /// Generates a log message indicating use of a trait. Input skillCheck as "Actor uses x trait ...... (for a Nervous Breakdown check / to prevent Stressed)"
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="skillCheck"></param>
    public void TraitLogMessage(Actor actor, string skillCheck)
    {
        if (actor != null)
        {
            if (string.IsNullOrEmpty(skillCheck) == false)
            { Debug.LogFormat("[Trt] {0}, {1}, uses \"{2}\" trait {3}", actor.actorName, actor.arc.name, actor.GetTrait().tag, skillCheck); }
            else { Debug.LogError("Invalid skillCheck parameter (Null or empty)"); }
        }
        else { Debug.LogError("Invalid actor (Null)"); }
    }


    //
    // - - - Tooltips - - -
    //

    /// <summary>
    /// tooltip data package for ActorSpriteTooltipUI.cs
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    public GenericTooltipData GetActorTooltip(Actor actor, GlobalSide side)
    {
        GenericTooltipData data = new GenericTooltipData();
        if (actor != null)
        {
            string colourSide = colourResistance;
            if (side.level == GameManager.instance.globalScript.sideAuthority.level)
            { colourSide = colourAuthority; }

            switch (actor.tooltipStatus)
            {
                case ActorTooltip.Breakdown:
                    data.header = string.Format("{0}{1}{2}{3}{4}", colourSide, actor.arc.name, colourEnd, "\n", actor.actorName);
                    data.main = string.Format("{0}<size=120%>Currently having a {1}{2}BREAKDOWN (Stress){3}{4} and unavailable</size>{5}", colourNormal, colourEnd,
                        colourNeutral, colourEnd, colourNormal, colourEnd);
                    data.details = string.Format("{0} is expected to recover next turn", actor.actorName);
                    break;
                case ActorTooltip.LieLow:
                    data.header = string.Format("{0}{1}{2}{3}{4}", colourSide, actor.arc.name, colourEnd, "\n", actor.actorName);
                    data.main = string.Format("{0}<size=120%>Currently {1}{2}LYING LOW{3}{4} and unavailable</size>{5}", colourNormal, colourEnd,
                        colourNeutral, colourEnd, colourNormal, colourEnd);
                    data.details = string.Format("{0} will automatically reactivate once their invisibility recovers or you {1}ACTIVATE{2} them",
                        actor.actorName, colourNeutral, colourEnd);
                    break;
                case ActorTooltip.Talk:

                    break;
                default:
                    data.main = "Unknown"; data.header = "Unknown"; data.details = "Unknown";
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Invalid actor (Null)");
            data = null;
        }
        return data;
    }

    /// <summary>
    /// fixed colour formatted tooltip
    /// </summary>
    /// <returns></returns>
    public GenericTooltipData GetVacantActorTooltip()
    {
        GenericTooltipData data = new GenericTooltipData();
        data.header = "Position Vacant";
        data.main = string.Format("{0}There is currently nobody acting in this position{1}", colourNormal, colourEnd);
        data.details = string.Format("{0}Go to the {1}{2}Reserve Pool{3}{4} and click on a person for the option to recall them to active duty{5}",
            colourAlert, colourEnd, colourNeutral, colourEnd, colourAlert, colourEnd);
        return data;
    }

    /// <summary>
    /// fixed colour formatted tooltip
    /// </summary>
    /// <returns></returns>
    public GenericTooltipData GetRenownActorTooltip()
    {
        GenericTooltipData data = new GenericTooltipData();
        data.header = "Renown";
        data.main = string.Format("{0}Is a measure of how well known a person is and their level support{1}", colourNormal, colourEnd);
        data.details = string.Format("{0}Somebody with high renown gains {1}{2}influential friends{3}{4} and is harder to {5}{6}Fire{7}{8} or {9}{10}Dispose Off{11}",
            colourAlert, colourEnd, colourNeutral, colourEnd, colourAlert, colourEnd, colourBad, colourEnd, colourAlert, colourEnd, colourBad, colourEnd);
        return data;
    }

    /// <summary>
    /// fixed colour formatted tooltip
    /// </summary>
    /// <returns></returns>
    public GenericTooltipData GetRenownPlayerTooltip()
    {
        GenericTooltipData data = new GenericTooltipData();
        data.header = "Renown";
        data.main = string.Format("{0}The more the better{1}", colourNeutral, colourEnd);
        data.details = string.Format("{0}Renown is the currency you use to do many things such as Hack the AI, Manage your subordinates, make decision, etc.{1}",
            colourNormal, colourEnd);
        return data;
    }

    /// <summary>
    /// tooltip data package for PlayerSpriteTooltipUI.cs
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public GenericTooltipData GetPlayerTooltip(GlobalSide side)
    {
        GenericTooltipData data = new GenericTooltipData();
        string playerName = GameManager.instance.playerScript.PlayerName;
        string colourSide = colourResistance;
        if (side.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; }
        switch (GameManager.instance.playerScript.tooltipStatus)
        {
            case ActorTooltip.Breakdown:
                data.header = string.Format("{0}PLAYER{1}{2}{3}", colourSide, colourEnd, "\n", playerName);
                data.main = string.Format("{0}<size=120%>Currently having a {1}{2}BREAKDOWN (Stress){3}{4} and unavailable</size>{5}", colourNormal, colourEnd,
                    colourNeutral, colourEnd, colourNormal, colourEnd);
                data.details = string.Format("{0} is expected to recover next turn", playerName);
                break;
            case ActorTooltip.LieLow:
                data.header = string.Format("{0}PLAYER{1}{2}{3}", colourSide, colourEnd, "\n", playerName);
                data.main = string.Format("{0}<size=120%>Currently {1}{2}LYING LOW{3}{4} and unavailable</size>{5}", colourNormal, colourEnd,
                    colourNeutral, colourEnd, colourNormal, colourEnd);
                data.details = string.Format("{0} will automatically reactivate once their invisibility recovers or you {1}ACTIVATE{2} them",
                    playerName, colourNeutral, colourEnd);
                break;
            default:
                data.main = "Unknown"; data.header = "Unknown"; data.details = "Unknown";
                break;
        }
        return data;
    }

    //new methods above here
}
