using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

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
    [Range(1, 6)] public int maxNumOfReserveActors = 4;

    [Header("Stats")]
    [Tooltip("Maximum value of an actor datapoint stat")]
    [Range(2, 4)] public int maxStatValue = 3;
    [Tooltip("Minimum value of an actor datapoint stat")]
    [Range(0, 4)] public int minStatValue = 0;
    [Tooltip("value of a datapoint stat that is considered 'neutral'")]
    [Range(2, 4)] public int neutralStatValue = 2;

    [HideInInspector] public int numOfQualities;            //initialised in LoadManager.cs -> numOfQualities (need to do this for sequencing reasons)

    [Header("Management")]
    [Tooltip("OnMap actor sent to Reserves. Their unhappy timer will be set to the this number of turns")]
    [Range(5, 15)] public int currentReserveTimer = 10;
    [Tooltip("Actor Recruited and placed in Reserves. Their unhappy timer will be set to this number of turns.")]
    [Range(1, 10)] public int recruitedReserveTimer = 5;
    [Tooltip("Renown cost for threatening an actor in the Reserve Pool")]
    [Range(1, 3)] public int renownCostThreaten = 1;
    [Tooltip("Base Renown cost for carrying out Manage Reserve Actor actions")]
    [Range(1, 5)] public int manageReserveRenown = 1;
    [Tooltip("Base Renown cost for carrying out Manage Dismiss Actor actions")]
    [Range(1, 5)] public int manageDismissRenown = 2;
    [Tooltip("Extra renown cost to dismiss or displose per secret known, added to base  costs before other modifiers")]
    [Range(0, 3)] public int manageSecretRenown = 2;
    [Tooltip("Base Renown cost for carrying out Manage Dispose Actor actions")]
    [Range(1, 5)] public int manageDisposeRenown = 3;
    [Tooltip("Once actor is unhappy, the chance per turn (1d100) of losing motivation -1")]
    [Range(1, 99)] public int unhappyLoseMotivationChance = 40;
    [Tooltip("Once actor is unhappy and has motivation 0 the chance of them acting on their dissatisfaction / turn")]
    [Range(1, 99)] public int unhappyTakeActionChance = 30;
    [Tooltip("When an unhappy actor in the Reserve pool takes action this is the first check made (ignored if actor has no secrets")]
    [Range(1, 99)] public int unhappyRevealSecretChance = 50;
    [Tooltip("When an unhappy actor in the Reserve pool takes action this is the second check made. Double chance if actor has previously complained")]
    [Range(1, 99)] public int unhappyResignChance = 25;
    [Tooltip("When an unhappy actor in the Reserve pool takes action this is the third check made. An actor can only complain once")]
    [Range(1, 99)] public int unhappyComplainChance = 60;
    [Tooltip("Increase to the actor's Unhappy Timer after they have been Reassured")]
    [Range(1, 10)] public int unhappyReassureBoost = 5;
    [Tooltip("Increase to the actor's Unhappy Timer after they have been Bullied")]
    [Range(1, 10)] public int unhappyBullyBoost = 5;
    [Tooltip("Amount of motivation lost when actor let go from reserve pool")]
    [Range(1, 3)] public int motivationLossLetGo = 1;
    [Tooltip("Amount of motivation lost when actor fired")]
    [Range(1, 3)] public int motivationLossFire = 2;
    [Tooltip("Amount of motivation gained when recalled from Reserves to Active Duty")]
    [Range(1, 3)] public int motivationGainActiveDuty = 2;
    [Tooltip("Base chance (low priority) of an AI leader dismissing a Questionable actor (adds AITasks into pool) during an autorun (Med priority x 2 chance, High x 3)")]
    [Range(1, 30)] public int dismissQuestionableChance = 15;

    [Header("Assorted")]
    [Tooltip("Renown cost for negating a bad gear use roll")]
    [Range(1, 3)] public int renownCostGear = 1;

    [Header("Condition Related")]
    [Tooltip("Chance of a character with the Stressed condition having a breakdown and missing a turn")]
    [Range(1, 99)] public int breakdownChance = 5;
    [Tooltip("Chance of a breakdown when Player becomes Stressed when already Stressed. Note that this is a fake chance as the breakdown is forced but it's needed for the fake random roll msg")]
    [Range(1, 99)] public int superStressChance = 80;
    [Tooltip("Chance per turn of the player, with the IMAGED condition, being picked up by a facial recognition scan and losing a level of invisibility")]
    [Range(0, 50)] public int playerRecognisedChance = 20;
    [Tooltip("Chance per turn of the player, with the QUESTIONABLE condition, losing one notch of Rebel HQ approval")]
    [Range(0, 100)] public int playerQuestionableChance = 30;
    [Tooltip("Chance of an actor, who has been captured, becoming a traitor (secretly) on release. Chance is multiplied by the # of times they have been captured.")]
    [Range(0, 100)] public int actorTraitorChance = 30;
    [Tooltip("Chance, per turn, of a character resigning if the Player has a bad condition (corrupt/quesitonable/incompetent). Chance stacks for each bad condition present")]
    [Range(0, 10)] public int actorResignChance = 5;
    [Tooltip("Base chance of a traitor revealing the Player's location each turn (could be RebelHQ or Actors). Actual chance -> base chance * # of traitors")]
    [Range(0, 10)] public int traitorActiveChance = 5;
    [Tooltip("Initial value of counterdown doomTimer for Nemesis Kill damage -> DOOM condition")]
    [Range(1, 10)] public int playerDoomTimerValue = 8;
    [Tooltip("Doom timer value when a cure will automatically be provided, if none already")]
    [Range(1, 10)] public int playerDoomFailSafeValue = 4;
    [Tooltip("Chance of an Addicted Player/Actor needing to spend renown to buy supplies of Dust to feed their addiction, % per turn")]
    [Range(0, 100)] public int playerAddictedChance = 20;
    [Tooltip("Amount of Renown player needs to spend to feed their addiction every time chance comes up true. If not enough renown available then -1 HQ support")]
    [Range(1, 10)] public int playerAddictedRenownCost = 2;
    [Tooltip("Initial amount of time (# of turns) that an addicted player will be immune from stress after taking a shot of the drug")]
    [Range(1, 10)] public int playerAddictedImmuneStart = 6;
    [Tooltip("Minimum amount of time (# of turns) that taking the drug will provide immunity from stress for")]
    [Range(1, 3)] public int playerAddictedImmuneMin = 1;
    [Tooltip("Number of turns when a player first becomes Addicted that they are exempt from Feeding their addiction")]
    [Range(0, 5)] public int playerAddictedExempt = 2;

    [Header("Stress Leave")]
    [Tooltip("Renown cost for Authority player or actor to take stress leave")]
    [Range(0, 5)] public int stressLeaveRenownCostAuthority = 2;
    [Tooltip("Renown cost for Resistance player or actor to take stress leave")]
    [Range(0, 5)] public int stressLeaveRenownCostResistance = 2;
    [Tooltip("Stress leave can only be taken with the approval of HQ (default true). Human player side only (AI ignores)")]
    public bool stressLeaveHQApproval = true;

    [Header("Lie Low")]
    [Tooltip("Lying Low has a global cooldown period. Once it has been used by either an actor or the player, it isn't available until the cooldown timer has expired")]
    [Range(1, 10)] public int lieLowCooldownPeriod = 5;

    [Header("MetaGame")]
    [Tooltip("Chance of Promoted actor being sent to HQ at end of level")]
    [Range(0, 100)] public int chanceOfPromotedToHQ = 100;
    [Tooltip("Chance of Resigned actor being sent to HQ at end of level")]
    [Range(0, 100)] public int chanceOfResignedToHQ = 40;
    [Tooltip("Chance of Dismissed actor being sent to HQ at end of level")]
    [Range(0, 100)] public int chanceOfDismissedToHQ = 40;
    [Tooltip("Chance of OnMap actor being sent to HQ at end of level")]
    [Range(0, 100)] public int chanceOfOnMapToHQ = 40;
    [Tooltip("Chance of Reserves actor being sent to HQ at end of level")]
    [Range(0, 100)] public int chanceOfReservesToHQ = 40;
    [Tooltip("Maximum number of actors that can be sent to HQ at the end of a level")]
    [Range(0, 10)] public int maxActorsToHQ = 5;
    [Tooltip("Maximum number of actors that can leave HQ at the end of a level (during MetaGame)")]
    [Range(0, 10)] public int maxActorsLeaveHQ = 5;

    [Header("Actor to Actor Relations")]
    [Tooltip("Relationships can't be changed while ever timer > 0 (counts down every turn)")]
    [Range(0, 1000)] public int timerRelations = 999;
    [Tooltip("Chance of a motivational shift occuring when a friend/enemy relationship exists (will only ever be +/-1 regardless of how big the initial shift in the originating actor")]
    [Range(0, 100)] public int chanceRelationShift = 100;

    #region Save Compatible Data
    [HideInInspector] public int lieLowTimer;                                   //Lying low can't be used unless timer is 0. Reset to lieLowCooldownPeriod whenever used. Decremented each turn.
    [HideInInspector] public int doomTimer;                                     //countdown doom timer set when resistance player gains the DOOMED condition (infected with a slow acting lethal virus)
    [HideInInspector] public int captureTimerPlayer;                            //countdown timer which determines how long the Resistance player is inactive while captured
    [HideInInspector] public bool isGearCheckRequired;                          //GearManager.cs -> used to flag that actors need to reset their gear
    [HideInInspector] public NameSet nameSet;                                   //used for naming actors. Takes nameSet from city
    [HideInInspector] public int actorIDCounter = 0;                            //used to sequentially number actorID's
    [HideInInspector] public int hqIDCounter = 0;                               //used to sequentially number hqID's
    #endregion

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

    //fast access fields
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;
    private Condition conditionStressed;
    private Condition conditionBlackmailer;
    private Condition conditionCorrupt;
    private Condition conditionIncompetent;
    private Condition conditionQuestionable;
    private Condition conditionDoomed;
    private Condition conditionImaged;
    private Condition conditionAddicted;
    private TraitCategory actorCategory;
    private Action actionAnyTeam;
    private int secretBaseChance = -1;
    //cached TraitEffects
    private string actorBreakdownChanceHigh;
    private string actorBreakdownChanceLow;
    private string actorBreakdownChanceNone;
    private string actorNoActionsDuringSecurityMeasures;
    private string actorSecretChanceHigh;
    private string actorSecretChanceNone;
    private string actorSecretTellAll;
    private string actorAppeaseNone;
    private string actorConflictNoGoodOptions;
    private string actorConflictNone;
    private string actorNeverResigns;
    private string actorConflictKill;
    private string actorResignHigh;
    private string actorReserveTimerDoubled;
    private string actorReserveTimerHalved;
    private string actorReserveActionNone;
    private string actorReserveActionDoubled;
    private string actorRemoveActionDoubled;
    private string actorRemoveActionHalved;
    private string actorKeepGear;
    //generic picker
    private int maxGenericOptions = -1;
    //gear
    private int maxNumOfGear;
    private int gearGracePeriod = -1;
    private int gearSwapBaseAmount = -1;
    private int gearSwapPreferredAmount = -1;
    //colour palette for Generic tool tip
    private string colourResistance;
    private string colourAuthority;
    /*private string colourGrey;*/
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


    public void PreInitialiseActors(GameState state)
    {
        //number of actors, default 4
        maxNumOfOnMapActors = maxNumOfOnMapActors == 4 ? maxNumOfOnMapActors : 4;
        numOfActiveActors = maxNumOfOnMapActors;
    }

    /// <summary>
    /// Not called for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseRecruitActorCachedFields();
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                SubInitialiseLevelStart();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseRecruitActorCachedFields();
                SubInitialiseLevelStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseRecruitActorCachedFields();
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                SubInitialiseLevelStart();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }

    /// <summary>
    /// Late initialisation done after DataManager.cs -> InitialiseLate
    /// Not for GameState.LoadGame
    /// </summary>
    public void InitialiseLate(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseAllLate();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseAllLate();
                break;
            case GameState.LoadAtStart:
                //do nothing
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }

    #region Initialisation SubMethods

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener is registered in InitialiseActors() due to GameManager sequence.
        EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent, "ActorManager");
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "ActorManager");
        EventManager.instance.AddListener(EventType.RecruitAction, OnEvent, "ActorManager");
        EventManager.instance.AddListener(EventType.RecruitDecision, OnEvent, "ActorManager");
        EventManager.instance.AddListener(EventType.GenericRecruitActorResistance, OnEvent, "ActorManager");
        EventManager.instance.AddListener(EventType.GenericRecruitActorAuthority, OnEvent, "ActorManager");
        EventManager.instance.AddListener(EventType.InventorySetReserve, OnEvent, "ActorManager");
        EventManager.instance.AddListener(EventType.InventorySetHQ, OnEvent, "ActorManager");
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access fields
        numOfQualities = GameManager.instance.preloadScript.numOfQualities;
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        conditionStressed = GameManager.instance.dataScript.GetCondition("STRESSED");
        conditionBlackmailer = GameManager.instance.dataScript.GetCondition("BLACKMAILER");
        conditionCorrupt = GameManager.instance.dataScript.GetCondition("CORRUPT");
        conditionIncompetent = GameManager.instance.dataScript.GetCondition("INCOMPETENT");
        conditionQuestionable = GameManager.instance.dataScript.GetCondition("QUESTIONABLE");
        conditionImaged = GameManager.instance.dataScript.GetCondition("IMAGED");
        conditionDoomed = GameManager.instance.dataScript.GetCondition("DOOMED");
        conditionAddicted = GameManager.instance.dataScript.GetCondition("ADDICTED");
        actorCategory = GameManager.instance.dataScript.GetTraitCategory("Actor");
        secretBaseChance = GameManager.instance.secretScript.secretLearnBaseChance;
        maxNumOfGear = GameManager.instance.gearScript.maxNumOfGear;
        gearGracePeriod = GameManager.instance.gearScript.actorGearGracePeriod;
        gearSwapBaseAmount = GameManager.instance.gearScript.gearSwapBaseAmount;
        gearSwapPreferredAmount = GameManager.instance.gearScript.gearSwapPreferredAmount;
        maxGenericOptions = GameManager.instance.genericPickerScript.maxOptions;
        actionAnyTeam = GameManager.instance.dataScript.GetAction("AnyTeam");
        Debug.Assert(numOfQualities > 0, "Invalid numOfQualities (zero or less)");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(conditionStressed != null, "Invalid conditionStressed (Null)");
        Debug.Assert(conditionBlackmailer != null, "Invalid conditionBlackmailer (Null)");
        Debug.Assert(conditionCorrupt != null, "Invalid conditionCorrupt (Null)");
        Debug.Assert(conditionIncompetent != null, "Invalid conditionIncompetent (Null)");
        Debug.Assert(conditionQuestionable != null, "Invalid conditionQuestionable (Null)");
        Debug.Assert(conditionImaged != null, "Invalid conditionImaged (Null)");
        Debug.Assert(conditionDoomed != null, "Invalid conditionDoomed (Null)");
        Debug.Assert(conditionAddicted != null, "Invalid conditionAddicted (Null)");
        Debug.Assert(actorCategory != null, "Invalid actorCategory (Null)");
        Debug.Assert(secretBaseChance > -1, "Invalid secretBaseChance");
        Debug.Assert(maxNumOfGear > 0, "Invalid maxNumOfGear (zero)");
        Debug.Assert(gearGracePeriod > -1, "Invalid gearGracePeriod (-1)");
        Debug.Assert(gearSwapBaseAmount > -1, "Invalid gearSwapBaseAmount (-1)");
        Debug.Assert(gearSwapPreferredAmount > -1, "Invalid gearSwapPreferredAmount (-1)");
        Debug.Assert(maxGenericOptions > -1, "Invalid maxGenericOptions (-1)");
        Debug.Assert(actionAnyTeam != null, "Invalid actionAnyTeam (Null)");
        //cached TraitEffects
        actorBreakdownChanceHigh = "ActorBreakdownChanceHigh";
        actorBreakdownChanceLow = "ActorBreakdownChanceLow";
        actorBreakdownChanceNone = "ActorBreakdownChanceNone";
        actorNoActionsDuringSecurityMeasures = "ActorNoActionsSecurity";
        actorSecretChanceHigh = "ActorSecretChanceHigh";
        actorSecretChanceNone = "ActorSecretChanceNone";
        actorSecretTellAll = "ActorSecretTellAll";
        actorAppeaseNone = "ActorAppeaseNone";
        actorKeepGear = "ActorKeepGear";
        actorConflictNoGoodOptions = "ActorConflictGoodNone";
        actorConflictNone = "ActorConflictNone";
        actorConflictKill = "ActorConflictKill";
        actorNeverResigns = "ActorResignNone";
        actorResignHigh = "ActorResignHigh";
        actorReserveTimerDoubled = "ActorReserveTimerDoubled";
        actorReserveTimerHalved = "ActorReserveTimerHalved";
        actorReserveActionNone = "ActorReserveActionNone";
        actorReserveActionDoubled = "ActorReserveActionDoubled";
        actorRemoveActionDoubled = "ActorRemoveActionDoubled";
        actorRemoveActionHalved = "ActorRemoveActionHalved";
    }
    #endregion

    #region SubInitialiseRecruitActorCachedFields
    private void SubInitialiseRecruitActorCachedFields()
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
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        //Name set
        nameSet = GameManager.instance.cityScript.GetNameSet();
        if (nameSet == null)
        { Debug.LogError("Invalid nameSet (Null)"); }
        //create active, OnMap actors
        InitialiseActors(maxNumOfOnMapActors, GameManager.instance.globalScript.sideResistance);
        InitialiseActors(maxNumOfOnMapActors, GameManager.instance.globalScript.sideAuthority);
        //create pool actors
        InitialisePoolActors();
        //Debug settings
        DebugTest();
        //set actor alpha to active for all onMap slots
        GameManager.instance.actorPanelScript.SetActorsAlphaActive();
    }
    #endregion

    #region SubInitialiseAllLate
    private void SubInitialiseAllLate()
    {
        //initialise actor contacts
        InitialiseActorContacts(globalAuthority);
        InitialiseActorContacts(globalResistance);
    }
    #endregion

    #endregion

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
            case EventType.InventorySetHQ:
                InitialiseHqHierarchyInventory();
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
        bool isPlayer;
        if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
        {
            //run for Resistance Player
            switch (GameManager.instance.sideScript.resistanceOverall)
            {
                case SideState.Human:
                    CheckPlayerHuman();
                    CheckInactiveResistanceActorsHuman();
                    //needs to be AFTER CheckInactiveActors
                    CheckActiveResistanceActorsHuman();

                    //end game checks
                    GameManager.instance.factionScript.CheckFactionFirePlayer();
                    GameManager.instance.cityScript.CheckCityLoyaltyAtLimit();
                    break;
                case SideState.AI:
                    if (GameManager.instance.autoRunTurns > 0)
                    {
                        //Both sides AI (autorun) -> Resistance first
                        isPlayer = true;
                        if (GameManager.instance.sideScript.PlayerSide.level != globalResistance.level) { isPlayer = false; }
                        CheckPlayerResistanceAI(isPlayer);
                        CheckInactiveResistanceActorsAI(isPlayer);
                        CheckActiveResistanceActorsAI(isPlayer);
                        //Authority -> sequence is Player / Inactive / Active
                        isPlayer = true;
                        if (GameManager.instance.sideScript.PlayerSide.level != globalAuthority.level) { isPlayer = false; }
                        CheckPlayerAuthorityAI(isPlayer);
                        CheckInactiveAuthorityActorsAI(isPlayer);
                        CheckActiveAuthorityActorsAI(isPlayer);
                    }
                    else
                    {
                        //Resistance AI only
                        isPlayer = false;
                        CheckPlayerResistanceAI(isPlayer);
                        CheckActiveResistanceActorsAI(isPlayer);
                        CheckInactiveResistanceActorsAI(isPlayer);
                    }
                    break;
                default:
                    Debug.LogWarningFormat("Invalid sideState (Unrecognised) for authorityOverall \"{0}\"", GameManager.instance.sideScript.authorityOverall);
                    break;
            }
        }
        else
        {
            //run for Authority Player
            switch (GameManager.instance.sideScript.authorityOverall)
            {
                case SideState.Human:
                    CheckPlayerHuman();
                    CheckInactiveAuthorityActorsHuman();
                    //needs to be AFTER CheckInactiveActors
                    CheckActiveAuthorityActorsHuman();
                    //end game checks
                    GameManager.instance.factionScript.CheckFactionFirePlayer();
                    GameManager.instance.cityScript.CheckCityLoyaltyAtLimit();
                    break;
                case SideState.AI:
                    if (GameManager.instance.autoRunTurns > 0)
                    {
                        //Both sides AI (autorun) -> Resistance first
                        isPlayer = true;
                        if (GameManager.instance.sideScript.PlayerSide.level != globalResistance.level) { isPlayer = false; }
                        CheckPlayerResistanceAI(isPlayer);
                        CheckInactiveResistanceActorsAI(isPlayer);
                        CheckActiveResistanceActorsAI(isPlayer);
                        //Authority-> sequence is Player / Inactive / Active
                        isPlayer = true;
                        if (GameManager.instance.sideScript.PlayerSide.level != globalAuthority.level) { isPlayer = false; }
                        CheckPlayerAuthorityAI(isPlayer);
                        CheckInactiveAuthorityActorsAI(isPlayer);
                        CheckActiveAuthorityActorsAI(isPlayer);
                    }
                    else
                    {
                        //Authority AI only -> sequence is Player / Inactive / Active
                        isPlayer = false;
                        CheckPlayerAuthorityAI(isPlayer);
                        CheckInactiveAuthorityActorsAI(isPlayer);
                        CheckActiveAuthorityActorsAI(isPlayer);

                    }
                    break;
                default:
                    Debug.LogWarningFormat("Invalid sideState (Unrecognised) for authorityOverall \"{0}\"", GameManager.instance.sideScript.authorityOverall);
                    break;
            }
        }
        //count down relation timers
        GameManager.instance.dataScript.CheckRelations();
        UpdateRelationMessages();
        //Reserve actors
        UpdateReserveActors();
        GameManager.instance.statScript.UpdateRatios();
        //Lie Low cooldown timer
        if (lieLowTimer > 0)
        {
            lieLowTimer--;
            //Resistance player (even if autorun)
            if (GameManager.instance.sideScript.PlayerSide.level == 2)
            {
                //lie low timer message (InfoApp 'Effects' tab)
                string text = string.Format("Lie Low Timer {0}", lieLowTimer);
                GameManager.instance.messageScript.ActorLieLowOngoing(text, lieLowTimer);
            }
        }
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
        colourResistance = GameManager.instance.colourScript.GetColour(ColourType.blueText);
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.badText);
        /*colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);*/
        colourCancel = GameManager.instance.colourScript.GetColour(ColourType.moccasinText);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.whiteText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourRecruit = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourArc = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// Reset data prior to a new level
    /// </summary>
    public void ResetCounter()
    { actorIDCounter = 0; }

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
                Actor actor = CreateActor(side, tempActorArcs[i].name, 1, ActorStatus.Active, i);
                if (actor != null)
                {
                    Debug.LogFormat("Actor added -> {0}, {1} {2}, {3} {4}, {5} {6}, level {7}{8}", actor.arc.actorName,
                        GameManager.instance.dataScript.GetQuality(side, 0), actor.GetDatapoint(ActorDatapoint.Datapoint0),
                        GameManager.instance.dataScript.GetQuality(side, 1), actor.GetDatapoint(ActorDatapoint.Datapoint1),
                        GameManager.instance.dataScript.GetQuality(side, 2), actor.GetDatapoint(ActorDatapoint.Datapoint2),
                        actor.level, "\n");
                }
                else { Debug.LogWarning("Actor not created"); }
            }
        }
        else { Debug.LogWarning("Invalid number of Actors (Zero, or less)"); }
    }

    /// <summary>
    /// Initialise actor contacts for both sides at game start
    /// </summary>
    public void InitialiseActorContacts(GlobalSide side)
    {
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(side);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                Actor actor = arrayOfActors[i];
                if (actor != null)
                { GameManager.instance.contactScript.SetActorContacts(actor); }
                else { Debug.LogErrorFormat("Invalid actor (Null) for arrayOfActors[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
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
                Actor actorOne = CreateActor(globalAuthority, listOfArcs[i].name, 1, ActorStatus.RecruitPool);
                if (actorOne != null)
                {
                    //NOTE: need to add to Dictionary BEFORE adding to Pool (Debug.Assert checks dictOfActors.Count in AddActorToPool)
                    GameManager.instance.dataScript.AddActor(actorOne);
                    GameManager.instance.dataScript.AddActorToPool(actorOne.actorID, 1, globalAuthority);
                }
                else { Debug.LogWarning(string.Format("Invalid Authority actorOne (Null) for actorArc \"{0}\"", listOfArcs[i].name)); }
                //level two actor
                Actor actorTwo = CreateActor(globalAuthority, listOfArcs[i].name, 2, ActorStatus.RecruitPool);
                if (actorTwo != null)
                {
                    GameManager.instance.dataScript.AddActor(actorTwo);
                    GameManager.instance.dataScript.AddActorToPool(actorTwo.actorID, 2, globalAuthority);
                }
                else { Debug.LogWarning(string.Format("Invalid Authority actorTwo (Null) for actorArc \"{0}\"", listOfArcs[i].name)); }
                //level three actor
                Actor actorThree = CreateActor(globalAuthority, listOfArcs[i].name, 3, ActorStatus.RecruitPool);
                if (actorThree != null)
                {
                    GameManager.instance.dataScript.AddActor(actorThree);
                    GameManager.instance.dataScript.AddActorToPool(actorThree.actorID, 3, globalAuthority);
                }
                else { Debug.LogWarning(string.Format("Invalid Authority actorThree (Null) for actorArc \"{0}\"", listOfArcs[i].name)); }
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
                Actor actorOne = CreateActor(globalResistance, listOfArcs[i].name, 1, ActorStatus.RecruitPool);
                if (actorOne != null)
                {
                    //NOTE: need to add to Dictionary BEFORE adding to Pool (Debug.Assert checks dictOfActors.Count in AddActorToPool)
                    GameManager.instance.dataScript.AddActor(actorOne);
                    GameManager.instance.dataScript.AddActorToPool(actorOne.actorID, 1, globalResistance);
                }
                else { Debug.LogWarning(string.Format("Invalid Resistance actorOne (Null) for actorArc \"{0}\"", listOfArcs[i].name)); }
                //level two actor
                Actor actorTwo = CreateActor(globalResistance, listOfArcs[i].name, 2, ActorStatus.RecruitPool);
                if (actorTwo != null)
                {
                    GameManager.instance.dataScript.AddActor(actorTwo);
                    GameManager.instance.dataScript.AddActorToPool(actorTwo.actorID, 2, globalResistance);
                }
                else { Debug.LogWarning(string.Format("Invalid Resistance actorTwo (Null) for actorArc \"{0}\"", listOfArcs[i].name)); }
                //level three actor
                Actor actorThree = CreateActor(globalResistance, listOfArcs[i].name, 3, ActorStatus.RecruitPool);
                if (actorThree != null)
                {
                    GameManager.instance.dataScript.AddActor(actorThree);
                    GameManager.instance.dataScript.AddActorToPool(actorThree.actorID, 3, globalResistance);
                }
                else { Debug.LogWarning(string.Format("Invalid Resistance actorThree (Null) for actorArc \"{0}\"", listOfArcs[i].name)); }
            }
        }
        else { Debug.LogError("Invalid list of Resistance Actor Arcs (Null)"); }
    }

    /// <summary>
    /// Initialises required number of actors to populate HQ hierarchy for player side
    /// </summary>
    public void InitialiseHQActors()
    {
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        int numOfArcs, level;
        int numOfActors = GameManager.instance.factionScript.numOfActorsHQ;
        int renownFactor = GameManager.instance.factionScript.renownFactor;
        int[] arrayOfLevels = new int[] { 3, 3, 3, 3, 2, 2, 2, 1, 1 }; //weighted towards higher calibre actors
        List<ActorArc> listOfArcs;
        List<Actor> listOfActors = new List<Actor>();
        switch (playerSide.level)
        {
            case 1:
                //Authority
                listOfArcs = new List<ActorArc>(GameManager.instance.dataScript.GetActorArcs(globalAuthority));
                if (listOfArcs != null)
                {
                    numOfArcs = listOfArcs.Count;
                    for (int i = 0; i < numOfActors; i++)
                    {
                        //Get random arc
                        ActorArc arc = listOfArcs[Random.Range(0, listOfArcs.Count)];
                        //Get random level
                        level = arrayOfLevels[Random.Range(0, arrayOfLevels.Length)];
                        switch (level)
                        {
                            case 1:
                                //level one actor
                                Actor actorOne = CreateActor(globalAuthority, arc.name, 1, ActorStatus.HQ);
                                if (actorOne != null)
                                {
                                    //NOTE: need to add to Dictionary BEFORE adding to Pool (Debug.Assert checks dictOfActors.Count in AddActorToPool)
                                    GameManager.instance.dataScript.AddHQActor(actorOne);
                                    GameManager.instance.dataScript.AddActorToHQPool(actorOne.hqID);
                                    listOfActors.Add(actorOne);
                                }
                                else { Debug.LogWarning(string.Format("Invalid Authority actorOne (Null) for actorArc \"{0}\"", arc.name)); }
                                break;
                            case 2:
                                //level two actor
                                Actor actorTwo = CreateActor(globalAuthority, arc.name, 2, ActorStatus.HQ);
                                if (actorTwo != null)
                                {
                                    GameManager.instance.dataScript.AddHQActor(actorTwo);
                                    GameManager.instance.dataScript.AddActorToHQPool(actorTwo.hqID);
                                    listOfActors.Add(actorTwo);
                                }
                                else { Debug.LogWarning(string.Format("Invalid Authority actorTwo (Null) for actorArc \"{0}\"", arc.name)); }
                                break;
                            case 3:
                                //level three actor
                                Actor actorThree = CreateActor(globalAuthority, arc.name, 3, ActorStatus.HQ);
                                if (actorThree != null)
                                {
                                    GameManager.instance.dataScript.AddHQActor(actorThree);
                                    GameManager.instance.dataScript.AddActorToHQPool(actorThree.hqID);
                                    listOfActors.Add(actorThree);
                                }
                                else { Debug.LogWarning(string.Format("Invalid Authority actorThree (Null) for actorArc \"{0}\"", arc.name)); }
                                break;
                            default: Debug.LogWarningFormat("Unrecognised level {0}", level); break;
                        }
                    }
                }
                else { Debug.LogError("Invalid list of Authority Actor Arcs (Null)"); }
                break;
            case 2:
                //Resistance
                listOfArcs = new List<ActorArc>(GameManager.instance.dataScript.GetActorArcs(globalResistance));
                if (listOfArcs != null)
                {
                    numOfArcs = listOfArcs.Count;
                    for (int i = 0; i < numOfActors; i++)
                    {
                        //Get random arc
                        ActorArc arc = listOfArcs[Random.Range(0, listOfArcs.Count)];
                        //Get random level
                        level = arrayOfLevels[Random.Range(0, arrayOfLevels.Length)];
                        switch (level)
                        {
                            case 1:
                                //level one actor
                                Actor actorOne = CreateActor(globalResistance, arc.name, 1, ActorStatus.HQ);
                                if (actorOne != null)
                                {
                                    //NOTE: need to add to Dictionary BEFORE adding to Pool (Debug.Assert checks dictOfActors.Count in AddActorToPool)
                                    GameManager.instance.dataScript.AddHQActor(actorOne);
                                    GameManager.instance.dataScript.AddActorToHQPool(actorOne.hqID);
                                    listOfActors.Add(actorOne);
                                }
                                else { Debug.LogWarning(string.Format("Invalid Resistance actorOne (Null) for actorArc \"{0}\"", arc.name)); }
                                break;
                            case 2:
                                //level two actor
                                Actor actorTwo = CreateActor(globalResistance, arc.name, 2, ActorStatus.HQ);
                                if (actorTwo != null)
                                {
                                    GameManager.instance.dataScript.AddHQActor(actorTwo);
                                    GameManager.instance.dataScript.AddActorToHQPool(actorTwo.hqID);
                                    listOfActors.Add(actorTwo);
                                }
                                else { Debug.LogWarning(string.Format("Invalid Resistance actorTwo (Null) for actorArc \"{0}\"", arc.name)); }
                                break;
                            case 3:
                                //level three actor
                                Actor actorThree = CreateActor(globalResistance, arc.name, 3, ActorStatus.HQ);
                                if (actorThree != null)
                                {
                                    GameManager.instance.dataScript.AddHQActor(actorThree);
                                    GameManager.instance.dataScript.AddActorToHQPool(actorThree.hqID);
                                    listOfActors.Add(actorThree);
                                }
                                else { Debug.LogWarning(string.Format("Invalid Resistance actorThree (Null) for actorArc \"{0}\"", arc.name)); }
                                break;
                            default: Debug.LogWarningFormat("Unrecognised level {0}", level); break;
                        }
                    }
                }
                else { Debug.LogError("Invalid list of Resistance Actor Arcs (Null)"); }
                break;
            default: Debug.LogWarningFormat("Unrecognised playerSide \"{0}\"", playerSide.name); break;
        }
        //assign actors to positions at HQ
        if (listOfActors.Count > 0)
        {
            int counter = 0;
            Actor[] arrayOfActorsHQ = GameManager.instance.dataScript.GetArrayOfActorsHQ();
            if (arrayOfActorsHQ != null)
            {
                foreach (Actor actor in listOfActors)
                {
                    //increment counter first
                    counter++;
                    //add to array
                    arrayOfActorsHQ[counter] = actor;
                    //do after incrementing counter as ActorHQ enum[0] is 'None'
                    ActorHQ statusHQ = (ActorHQ)counter;
                    //fail safe to prevent any overflow always being workers
                    if (statusHQ == ActorHQ.Worker)
                    { counter--; }
                    //assign to actor
                    actor.statusHQ = statusHQ;
                    //assign renown (Boss has highest, rest get progressively less, closer to the boss you are the more important the position)
                    actor.Renown = (numOfActors + 2 - counter) * renownFactor;
                }
            }
            else { Debug.LogError("Invalid arrayOfActorsHQ (Null)"); }
        }
        else { Debug.LogError("Invalid listOfActors (Empty)"); }
    }


    /// <summary>
    /// creates a new actor of the specified actor arc type, side and level (1 worst -> 3 best). SlotID (0 to 3) for current actor, default '-1' for reserve pool actor.
    /// adds actor to dataManager.cs ArrayOfActors if OnMap (slot ID > -1). NOT added to dictOfActors if slotID -1 (do so after actor is chosen in generic picker)
    /// returns null if a problem
    /// </summary>
    /// <param name="arc"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public Actor CreateActor(GlobalSide side, string arcName, int level, ActorStatus status, int slotID = -1)
    {
        Debug.Assert(level > 0 && level < 4, "Invalid level (must be between 1 and 3)");
        Debug.Assert(slotID >= -1 && slotID <= maxNumOfOnMapActors, "Invalid slotID (must be -1 (default) or between 1 and 3");
        Debug.Assert(string.IsNullOrEmpty(arcName) == false, "Invalid arcName (Null)");
        //check a valid actor Arc parameter
        ActorArc arc = GameManager.instance.dataScript.GetActorArc(arcName);
        if (arc != null)
        {
            //check actor arc is the correct side
            if (arc.side.level == side.level)
            {
                //create new actor
                Actor actor = new Actor();
                //data
                actor.actorID = actorIDCounter++;
                actor.slotID = slotID;
                actor.level = level;
                actor.side = side;
                actor.arc = arc;
                actor.AddTrait(GameManager.instance.dataScript.GetRandomTrait(actorCategory, side));
                actor.GetPersonality().SetFactors(GameManager.instance.personScript.SetPersonalityFactors(actor.GetTrait().GetArrayOfCriteria()));
                actor.Status = status;
                //hq actor
                if (actor.Status == ActorStatus.HQ)
                {
                    actor.hqID = hqIDCounter++;
                    //temporary assignment
                    actor.statusHQ = ActorHQ.Worker;
                }
                //TO DO -> currently actor sprite is derived from actorArc sprite
                actor.sprite = arc.sprite;
                actor.spriteName = arc.sprite.name;
                Debug.Assert(actor.GetTrait() != null, "Invalid actor.trait (Null)");
                //name
                if (Random.Range(0, 100) < 50)
                {
                    actor.sex = ActorSex.Male;
                    actor.firstName = nameSet.firstMaleNames.GetRandomRecord();
                }
                else
                {
                    actor.sex = ActorSex.Female;
                    actor.firstName = nameSet.firstFemaleNames.GetRandomRecord();
                }
                actor.actorName = string.Format("{0} {1}", actor.firstName, nameSet.lastNames.GetRandomRecord());
                //level -> range limits
                int limitLower = 1;
                if (level == 3) { limitLower = 2; }
                int limitUpper = Math.Min(4, level + 2);
                //level -> assign
                actor.SetDatapoint(ActorDatapoint.Datapoint0, Random.Range(limitLower, limitUpper)); //connections and influence
                actor.SetDatapoint(ActorDatapoint.Datapoint1, Random.Range(limitLower, limitUpper)); //motivation and support
                if (side.level == GameManager.instance.globalScript.sideResistance.level)
                {
                    //invisibility -> Level 3 100% Invis 3, level 2 25% Invis 2, 75% Invis 3, level 1 50% Invis 2, 50% Invis 3
                    switch (actor.level)
                    {
                        case 3: actor.SetDatapoint(ActorDatapoint.Invisibility2, 3); break;
                        case 2:
                            if (Random.Range(0, 100) <= 25) { actor.SetDatapoint(ActorDatapoint.Invisibility2, 2); }
                            else { actor.SetDatapoint(ActorDatapoint.Invisibility2, 3); }
                            break;
                        case 1:
                            if (Random.Range(0, 100) <= 50) { actor.SetDatapoint(ActorDatapoint.Invisibility2, 2); }
                            else { actor.SetDatapoint(ActorDatapoint.Invisibility2, 3); }
                            break;
                    }

                }
                else if (side.level == GameManager.instance.globalScript.sideAuthority.level)
                { actor.SetDatapoint(ActorDatapoint.Ability2, Random.Range(limitLower, limitUpper)); }
                //OnMap actor (pool actors already in dictionary)
                if (slotID > -1)
                {
                    actor.Renown = 0;
                    //add to data collections
                    GameManager.instance.dataScript.AddCurrentActor(side, actor, slotID);
                    GameManager.instance.dataScript.AddActor(actor);
                }
                //return actor
                return actor;
            }
            else { Debug.LogError(string.Format("ActorArc (\"{0}\") is the wrong side ({1})", arc.name, side)); }
        }
        else { Debug.LogError(string.Format("Invalid ActorArc (Null) for actorArc {0}", arcName)); }
        return null;
    }

    #region GetNodeActions
    /// <summary>
    /// Returns a list of all relevant Actor Actions for the  node to enable a ModalActionMenu to be put together (one button per action). 
    /// Resistance -> Max 4 Actor + 1 Target actions with an additional 'Cancel' buttnn added last automatically -> total six buttons (hardwired into GUI design)
    /// Authority -> Insert and Recall Teams
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public List<EventButtonDetails> GetNodeActions(int nodeID)
    {

        #region SetUp
        string sideColour;
        string cancelText = null;
        string effectCriteria;
        bool proceedFlag;
        bool isPlayerAction = false;
        AuthoritySecurityState securityState = GameManager.instance.turnScript.authoritySecurityState;
        Actor[] arrayOfActors;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"ss
        if (playerSide.level == globalAuthority.level)
        { sideColour = colourAuthority; }
        else { sideColour = colourResistance; }
        List<EventButtonDetails> tempList = new List<EventButtonDetails>();
        //string builder for cancel button (handles all no go cases
        StringBuilder infoBuilder = new StringBuilder();
        //player's current node
        int playerID = GameManager.instance.nodeScript.nodePlayer;
        #endregion

        //Get Node
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            List<Effect> listOfEffects = new List<Effect>();
            Action tempAction;
            EventButtonDetails details;

            #region Resistance
            //
            // - - - Resistance - - -
            //
            if (playerSide.level == globalResistance.level)
            {
                //'Cancel' button tooltip)
                if (nodeID == playerID)
                {
                    cancelText = "You are present at the node";
                    isPlayerAction = true;
                }
                else { cancelText = "You are NOT present at the node"; }

                #region target
                //
                // - - -  Target - - -
                //
                //Does the Node have a target attached? -> added first
                if (string.IsNullOrEmpty(node.targetName) == false)
                {
                    Target target = GameManager.instance.dataScript.GetTarget(node.targetName);
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
                                int slotID = GameManager.instance.dataScript.CheckActorPresent(target.actorArc.name, globalResistance);
                                Actor actorTemp = GameManager.instance.dataScript.GetCurrentActor(slotID, globalResistance);
                                if (actorTemp != null)
                                {
                                    //Not if Spooked trait and Security Measures in place
                                    if (actorTemp.CheckTraitEffect(actorNoActionsDuringSecurityMeasures) == true && securityState != AuthoritySecurityState.Normal)
                                    {
                                        targetProceed = false;
                                        infoBuilder.AppendFormat("{0} is {1}{2}{3} (Trait) due to Security Measures", actorTemp.arc.name, colourNeutral, actorTemp.GetTrait().tag, colourEnd);
                                        TraitLogMessage(actorTemp, "for an attempt on a Target", "to CANCEL attempt due to Security Measures");
                                    }
                                    else { targetProceed = true; }
                                }
                                else
                                {
                                    Debug.LogWarningFormat("Invalid actor for target.actorArc.ActorArc {0} and slotID {1}", target.actorArc.name, slotID);
                                    targetProceed = false;
                                }
                            }
                            //target live and dancing
                            if (targetProceed == true)
                            {
                                string targetHeader = string.Format("{0}<size=110%>{1}</size>{2}{3}{4}{5}{6}", sideColour, target.targetName, colourEnd, "\n", colourDefault,
                                    target.descriptorResistance, colourEnd);
                                //button target details
                                EventButtonDetails targetDetails = new EventButtonDetails()
                                {
                                    buttonTitle = "Attempt Target",
                                    buttonTooltipHeader = targetHeader,
                                    buttonTooltipMain = GameManager.instance.targetScript.GetTargetFactors(node.targetName),
                                    buttonTooltipDetail = GameManager.instance.targetScript.GetTargetEffects(node.targetName, isPlayerAction),
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
                    else { Debug.LogError(string.Format("Invalid target \"{0}\" (Null){1}", node.targetName, "\n")); }
                }
                #endregion

                //
                // - - - Actors - - - 
                //
                //loop actors currently in game -> get Node actions (1 per Actor, if valid criteria)
                arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
                if (GameManager.instance.dataScript.CheckNumOfActiveActors(globalResistance) > 0)
                {
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
                                        if (GameManager.instance.dataScript.CheckForActorContactActive(actor, nodeID) == true)
                                        {
                                            //Not if actor has Spooked trait and Security Measures in place
                                            if (actor.CheckTraitEffect(actorNoActionsDuringSecurityMeasures) == true && securityState != AuthoritySecurityState.Normal)
                                            {
                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                infoBuilder.AppendFormat("{0} is {1}Spooked{2} (Trait) due to Security Measures", actor.arc.name, colourNeutral, colourEnd);
                                                TraitLogMessage(actor, "for a district action", "to CANCEL action due to Security Measures");
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
                                                        if (effect.outcome.name.Equals("Renown", StringComparison.Ordinal) == false && effect.outcome.name.Equals("Invisibility", StringComparison.Ordinal) == false)
                                                        { builder.AppendFormat("{0}{1}{2}", colourEffect, effect.description, colourEnd); }
                                                        else
                                                        {
                                                            //handle renown & invisibility situations - players or actors?
                                                            if (nodeID == playerID)
                                                            {
                                                                //player affected (good for renown, bad for invisibility)
                                                                if (effect.outcome.name.Equals("Renown", StringComparison.Ordinal))
                                                                { builder.AppendFormat("{0}Player {1}{2}", colourGood, effect.description, colourEnd); }
                                                                else
                                                                {
                                                                    builder.AppendFormat("{0}Player {1}{2}", colourBad, effect.description, colourEnd);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                //actor affected
                                                                builder.AppendFormat("{0}{1} {2}{3}", colourBad, actor.arc.name, effect.description, colourEnd);
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
                                                actionDetails.actorDataID = actor.slotID;
                                                //pass all relevant details to ModalActionMenu via Node.OnClick()
                                                if (actor.arc.nodeAction.special == null)
                                                {
                                                    details = new EventButtonDetails()
                                                    {
                                                        buttonTitle = tempAction.tag,
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
                                                                buttonTitle = tempAction.tag,
                                                                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                                buttonTooltipMain = tempAction.tooltipText,
                                                                buttonTooltipDetail = builder.ToString(),
                                                                action = () => { EventManager.instance.PostNotification(EventType.NeutraliseTeamAction, this, actionDetails, "ActorManager.cs -> GetNodeActions"); }
                                                            };
                                                            break;
                                                        case "GetGear":
                                                            //only if HQ not Relocating
                                                            if (GameManager.instance.factionScript.isHqRelocating == false)
                                                            {
                                                                details = new EventButtonDetails()
                                                                {
                                                                    buttonTitle = tempAction.tag,
                                                                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                                    buttonTooltipMain = tempAction.tooltipText,
                                                                    buttonTooltipDetail = builder.ToString(),
                                                                    action = () => { EventManager.instance.PostNotification(EventType.GearAction, this, actionDetails, "ActorManager.cs -> GetNodeActions"); }
                                                                };
                                                            }
                                                            else
                                                            {
                                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                                infoBuilder.AppendFormat("Gear unavailable as HQ Relocating");
                                                            }
                                                            break;
                                                        case "GetRecruit":
                                                            //only if HQ not Relocating
                                                            if (GameManager.instance.factionScript.isHqRelocating == false)
                                                            {
                                                                details = new EventButtonDetails()
                                                                {
                                                                    buttonTitle = tempAction.tag,
                                                                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                                    buttonTooltipMain = tempAction.tooltipText,
                                                                    buttonTooltipDetail = builder.ToString(),
                                                                    action = () => { EventManager.instance.PostNotification(EventType.RecruitAction, this, actionDetails, "ActorManager.cs -> GetNodeActions"); }
                                                                };
                                                            }
                                                            else
                                                            {
                                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                                infoBuilder.AppendFormat("Recruits unavailable as HQ Relocating");
                                                            }
                                                            break;
                                                        case "TargetInfo":
                                                            details = new EventButtonDetails()
                                                            {
                                                                buttonTitle = tempAction.tag,
                                                                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                                buttonTooltipMain = tempAction.tooltipText,
                                                                buttonTooltipDetail = builder.ToString(),
                                                                action = () => { EventManager.instance.PostNotification(EventType.TargetInfoAction, this, actionDetails, "ActorManager.cs -> GetNodeActions"); }
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
                                            switch (actor.inactiveStatus)
                                            {
                                                case ActorInactive.LieLow:
                                                    infoBuilder.AppendFormat("{0} is lying low and unavailable", actor.arc.name);
                                                    break;
                                                case ActorInactive.Breakdown:
                                                    infoBuilder.AppendFormat("{0} is having a Stress Breakdown", actor.arc.name);
                                                    break;
                                            }
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
                else
                {
                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                    infoBuilder.Append("No Subordinates present");
                }
            }
            #endregion

            #region Authority
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
                    List<Team> listOfTeams = node.GetListOfTeams();
                    if (listOfTeams != null)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (Team team in listOfTeams)
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
                if (GameManager.instance.dataScript.CheckNumOfActiveActors(globalAuthority) > 0)
                {
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
                                /*if (GameManager.instance.dataScript.CheckActorContactPresent(actor.actorID, nodeID) == true)*/
                                if (GameManager.instance.dataScript.CheckForActorContactActive(actor, nodeID) == true)
                                {
                                    //temporary action "Any Team"
                                    tempAction = actionAnyTeam;
                                    isAnyTeam = true;
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
                                                actorSlotID = actor.slotID,
                                                listOfCriteria = effect.listOfCriteria
                                            };
                                            effectCriteria = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                                            if (effectCriteria == null)
                                            {
                                                //Effect criteria O.K -> tool tip text
                                                if (builder.Length > 0) { builder.AppendLine(); }
                                                if (effect.outcome.name.Equals("Renown", StringComparison.Ordinal) == false)
                                                {
                                                    //sort out colour, remember effect.typeOfEffect is from POV of resistance so good will be bad for authority
                                                    if (effect.typeOfEffect != null)
                                                    {
                                                        switch (effect.typeOfEffect.name)
                                                        {
                                                            case "Good":
                                                                builder.AppendFormat("{0}{1}{2}", colourBad, effect.description, colourEnd);
                                                                break;
                                                            case "Neutral":
                                                                builder.AppendFormat("{0}{1}{2}", colourNeutral, effect.description, colourEnd);
                                                                break;
                                                            case "Bad":
                                                                builder.AppendFormat("{0}{1}{2}", colourGood, effect.description, colourEnd);
                                                                break;
                                                            default:
                                                                builder.AppendFormat("{0}{1}{2}", colourDefault, effect.description, colourEnd);
                                                                Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                                                                break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        builder.Append(effect.description);
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
                                                { builder.AppendFormat("{0}{1} {2}{3}", colourAuthority, actor.arc.name, effect.description, colourEnd); }

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
                                        actionDetails.actorDataID = actor.slotID;
                                        //Node action is standard but other actions are possible
                                        UnityAction clickAction = null;
                                        //Team action
                                        if (isAnyTeam)
                                        { clickAction = () => { EventManager.instance.PostNotification(EventType.InsertTeamAction, this, actionDetails, "ActorManager.cs -> GetNodeActions"); }; }
                                        //Node action
                                        else
                                        { clickAction = () => { EventManager.instance.PostNotification(EventType.NodeAction, this, actionDetails, "ActorManager.cs -> GetNodeActions"); }; }
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
                                    Debug.LogError(string.Format("{0}, slotID {1} has no valid action", actor.arc.name, actor.slotID));
                                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                    infoBuilder.AppendFormat("{0} is having a bad day", actor.arc.name);
                                }
                            }
                            else
                            {
                                //Inactive actor
                                switch (actor.inactiveStatus)
                                {
                                    case ActorInactive.Breakdown:
                                        if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                        infoBuilder.AppendFormat("{0} is having a Stress Breakdown", actor.arc.name);
                                        break;
                                }
                            }
                            //add to list
                            if (details != null)
                            { tempList.Add(details); }
                        }
                    }
                }
                else
                {
                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                    infoBuilder.Append("No Ministers present");
                }
            }
            #endregion

            #region Cancel
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
            #endregion

        }
        else { Debug.LogError(string.Format("Invalid Node (null), ID {0}{1}", nodeID, "\n")); }
        return tempList;
    }
    #endregion


    #region GetActorActions
    /// <summary>
    /// Returns a list of all relevant Actor Actions for the actor to enable a ModalActionMenu to be put together (one button per action). 
    /// Resistance only -> up to 3 x 'Give (or Take) Gear to Actor', 1 x'Activate' / Lie Low', 1 x 'Manage' and an automatic Cancel button (6 total)
    /// triggered when you Right Click an Actor's portrait
    /// </summary>
    /// <param name="actorSlotID"></param>
    /// <returns></returns>
    public List<EventButtonDetails> GetActorActions(int actorSlotID)
    {

        #region SetUp
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
        #endregion

        //if actor is Null, a single button (Cancel) menu is still provided
        if (actor != null)
        {
            title = string.Format("{0}", isResistance ? "" : GameManager.instance.metaScript.GetAuthorityTitle().ToString() + " ");
            cancelText = string.Format("{0} {1}", actor.arc.name, actor.actorName);
            switch (actor.Status)
            {
                case ActorStatus.Active:

                    #region Manage (both sides)
                    //
                    // - - - Manage (both sides) - - - 
                    //
                    ModalActionDetails manageActionDetails = new ModalActionDetails() { };
                    manageActionDetails.side = playerSide;
                    manageActionDetails.actorDataID = actor.slotID;
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
                    #endregion

                    #region Resistance
                    //
                    // - - - Resistance - - -
                    //
                    if (isResistance == true)
                    {
                        //
                        // - - - Lie Low - - -
                        //
                        //Invisibility must be less than max
                        if (actor.GetDatapoint(ActorDatapoint.Invisibility2) < maxStatValue)
                        {
                            //can't lie low if a Surveillance Crackdown is in place
                            if (securityState != AuthoritySecurityState.SurveillanceCrackdown)
                            {
                                if (lieLowTimer == 0)
                                {
                                    if (GameManager.instance.factionScript.isHqRelocating == false)
                                    {
                                        ModalActionDetails lielowActionDetails = new ModalActionDetails() { };
                                        lielowActionDetails.side = playerSide;
                                        lielowActionDetails.actorDataID = actor.slotID;
                                        if (actor.isLieLowFirstturn == true)
                                        { numOfTurns = 4 - actor.GetDatapoint(ActorDatapoint.Invisibility2); }
                                        else { numOfTurns = 3 - actor.GetDatapoint(ActorDatapoint.Invisibility2); }
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
                                    { infoBuilder.AppendFormat("Lie Low unavailable as HQ Relocating"); }
                                }
                                else
                                { infoBuilder.AppendFormat("Lie Low unavailable for {0} turn{1}", lieLowTimer, lieLowTimer != 1 ? "s" : ""); }
                            }
                            else
                            { infoBuilder.AppendFormat("{0}Surveillance Crackdown{1}{2}{3}(can't Lie Low){4}", colourAlert, colourEnd, "\n", colourBad, colourEnd); }
                            //stress leave
                            if (actor.CheckConditionPresent(conditionStressed) == true)
                            {
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                infoBuilder.AppendFormat("{0}Stress Leave only possible if MAX Invisibility{1}", colourAlert, colourEnd);
                            }
                        }
                        else
                        {
                            //actor invisiblity at max
                            infoBuilder.AppendFormat("{0} Invisibility at Max and can't Lie Low", actor.arc.name);

                            //
                            // - - - Stress Leave - - -
                            //
                            if (actor.CheckConditionPresent(conditionStressed) == true)
                            {
                                //must be stressed and have enough renown
                                if (GameManager.instance.playerScript.Renown >= stressLeaveRenownCostResistance)
                                {
                                    //can't take Stress Leave if a Surveillance Crackdown is in place
                                    if (securityState != AuthoritySecurityState.SurveillanceCrackdown)
                                    {
                                        if (stressLeaveHQApproval == true)
                                        {
                                            if (GameManager.instance.factionScript.isHqRelocating == false)
                                            {
                                                ModalActionDetails leaveActionDetails = new ModalActionDetails() { };
                                                leaveActionDetails.side = playerSide;
                                                leaveActionDetails.actorDataID = actor.actorID;
                                                leaveActionDetails.renownCost = stressLeaveRenownCostResistance;
                                                tooltipText = "Stress is a debilitating condition which can chew a person up into little pieces if left untreated";
                                                EventButtonDetails leaveDetails = new EventButtonDetails()
                                                {
                                                    buttonTitle = "Stress Leave",
                                                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                                                    buttonTooltipMain = string.Format("{0}, {1}, will recover from their Stress", actor.actorName, actor.arc.name),
                                                    buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                                                    //use a Lambda to pass arguments to the action
                                                    action = () => { EventManager.instance.PostNotification(EventType.LeaveActorAction, this, leaveActionDetails, "ActorManager.cs -> GetActorActions"); }
                                                };
                                                //add Activate button to list
                                                tempList.Add(leaveDetails);
                                            }
                                            else
                                            {
                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                infoBuilder.AppendFormat("{0}Stress Leave unavailable as HQ Relocating{1}", colourAlert, colourEnd);
                                            }
                                        }
                                        else
                                        {
                                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                            infoBuilder.AppendFormat("{0}HQ has banned all Stress Leave{1}", colourAlert, colourEnd);
                                        }
                                    }
                                    else
                                    {
                                        if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                        infoBuilder.AppendFormat("{0}Stress Leave not possible during a Surveillance Crackdown{1}", colourAlert, colourEnd);
                                    }
                                }
                                else
                                {
                                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                    infoBuilder.AppendFormat("{0}Insufficient Renown for Stress Leave (need {1}{2}{3}{4}{5}){6}", colourAlert, colourEnd, colourNeutral,
                                        stressLeaveRenownCostResistance, colourEnd, colourAlert, colourEnd);
                                }
                            }
                        }

                        //
                        // - - - Give Gear - - -
                        //
                        int numOfGear = GameManager.instance.playerScript.CheckNumOfGear();
                        if (numOfGear > 0)
                        {
                            List<string> listOfGear = GameManager.instance.playerScript.GetListOfGear();
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
                                            gearActionDetails.actorDataID = actor.slotID;
                                            gearActionDetails.gearName = gear.name;
                                            //get actor's preferred gear
                                            GearType preferredGear = actor.arc.preferredGear;
                                            if (preferredGear != null)
                                            {
                                                benefit = gearSwapBaseAmount;
                                                if (preferredGear.name.Equals(gear.type.name, StringComparison.Ordinal) == true)
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
                                            //mood info
                                            string moodText = GameManager.instance.personScript.GetMoodTooltip(MoodType.GiveGear, actor.arc.name);
                                            builderTooltip.AppendFormat("{0}{1}", "\n", moodText);
                                            //traits
                                            if (actor.CheckTraitEffect(actorKeepGear) == true)
                                            {
                                                builderTooltip.AppendFormat("{0}{1} has {2}{3}{4} trait{5}{6}(Refuses to return Gear){7}", "\n", actor.arc.name, colourNeutral,
                                                    actor.GetTrait().tag, colourEnd, "\n", colourBad, colourEnd);
                                                TraitLogMessage(actor, "for Returning Gear", "to AVOID doing so");
                                            }

                                            //existing gear
                                            if (string.IsNullOrEmpty(actor.GetGearName()) == false)
                                            {
                                                Gear gearOld = GameManager.instance.dataScript.GetGear(actor.GetGearName());
                                                if (gearOld != null)
                                                { builderTooltip.AppendFormat("{0}{1}{2} will be Lost{3}", "\n", colourBad, gearOld.name, colourEnd); }
                                                else { Debug.LogWarningFormat("Invalid gearOld (Null) for gearID {0}", actor.GetGearName()); }
                                            }
                                            EventButtonDetails gearDetails = new EventButtonDetails()
                                            {
                                                buttonTitle = string.Format("Give {0}", gear.tag),
                                                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                                                buttonTooltipMain = string.Format("Give {0}{1}{2} ({3}) to {4}{5}{6}, {7}", colourNeutral, gear.tag, colourEnd, gear.type.name,
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
                                            infoBuilder.AppendFormat("Can't gift {0}{1}{2}{3}{4}(used this turn){5}", colourNeutral, gear.tag, colourEnd, "\n", colourBad, colourEnd);
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
                        string actorGearName = actor.GetGearName();
                        //Player must have at least one free slot
                        if (numOfGear < maxNumOfGear)
                        {
                            //actor must have an item of gear
                            if (string.IsNullOrEmpty(actorGearName) == false)
                            {
                                //traits
                                if (actor.CheckTraitEffect(actorKeepGear) == false)
                                {
                                    //grace period must have expired
                                    if (actor.GetGearTimer() > gearGracePeriod)
                                    {
                                        Gear gearActor = GameManager.instance.dataScript.GetGear(actorGearName);
                                        if (gearActor != null)
                                        {
                                            benefit = gearSwapBaseAmount;
                                            StringBuilder builder = new StringBuilder();
                                            //data package
                                            ModalActionDetails gearActionDetails = new ModalActionDetails() { };
                                            gearActionDetails.side = playerSide;
                                            gearActionDetails.actorDataID = actor.slotID;
                                            gearActionDetails.gearName = gearActor.name;
                                            //get actor's preferred gear
                                            GearType preferredGear = actor.arc.preferredGear;
                                            if (preferredGear != null)
                                            {
                                                if (preferredGear.name.Equals(gearActor.type.name, StringComparison.Ordinal) == true)
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
                                            //mood info
                                            string moodText = GameManager.instance.personScript.GetMoodTooltip(MoodType.TakeGear, actor.arc.name);
                                            builder.AppendFormat("{0}{1}", "\n", moodText);
                                            //relationship conflict
                                            if (actor.GetDatapoint(ActorDatapoint.Motivation1) < benefit)
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
                                        else { Debug.LogWarningFormat("Invalid gearActor (Null) for gear {0}", actorGearName); }
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
                                    TraitLogMessage(actor, "for a return of Gear", "to REFUSE to handback Gear");
                                }
                            }
                            else
                            {
                                //actor has no gear to give
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                { infoBuilder.AppendFormat("{0}{1} has no gear to Take{2}", colourAlert, actor.arc.name, colourEnd); }
                            }
                        }
                        else
                        {
                            //player has no spare slot to put gear
                            if (string.IsNullOrEmpty(actorGearName) == false)
                            {
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                { infoBuilder.AppendFormat("Can't Take Gear{0}{1}(No space){2}", "\n", colourBad, colourEnd); }
                            }
                        }
                    }
                    #endregion

                    #region Authority
                    //
                    // - - - Authority - - -
                    //
                    else
                    {
                        if (actor.CheckConditionPresent(conditionStressed) == true)
                        {
                            if (GameManager.instance.playerScript.Renown >= stressLeaveRenownCostAuthority)
                            {
                                //
                                // - - - Stress Leave - - -
                                //
                                ModalActionDetails leaveActionDetails = new ModalActionDetails() { };
                                leaveActionDetails.side = playerSide;
                                leaveActionDetails.actorDataID = actor.actorID;
                                leaveActionDetails.renownCost = stressLeaveRenownCostAuthority;
                                tooltipText = "Stress is a debilitating condition which can chew a person up into little pieces if left untreated";
                                EventButtonDetails leaveDetails = new EventButtonDetails()
                                {
                                    buttonTitle = "Stress Leave",
                                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                                    buttonTooltipMain = string.Format("{0}, {1}, will recover from their Stress", actor.actorName, actor.arc.name),
                                    buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                                    //use a Lambda to pass arguments to the action
                                    action = () => { EventManager.instance.PostNotification(EventType.LeaveActorAction, this, leaveActionDetails, "ActorManager.cs -> GetActorActions"); }
                                };
                                //add Activate button to list
                                tempList.Add(leaveDetails);
                            }
                            else
                            { infoBuilder.AppendFormat("Stress Leave requires {0}{1}{2} Renown", colourNeutral, stressLeaveRenownCostAuthority, colourEnd); }
                        }
                        else
                        { infoBuilder.AppendFormat("Need to be {0}Stressed{1} in order to take Leave", colourNeutral, colourEnd); }
                    }
                    #endregion

                    break;
                //
                // - - - Actor Inactive - - -
                //
                case ActorStatus.Inactive:

                    #region Resistance
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
                            TraitLogMessage(actor, "for becoming Active after Lying Low", "to CANCEL Activation due to Security Measures");
                        }
                        if (proceedFlag == true)
                        {
                            //
                            // - - - Activate - - -
                            //
                            ModalActionDetails activateActionDetails = new ModalActionDetails() { };
                            activateActionDetails.side = playerSide;
                            activateActionDetails.actorDataID = actor.slotID;
                            if (actor.isLieLowFirstturn == true)
                            { numOfTurns = 4 - actor.GetDatapoint(ActorDatapoint.Invisibility2); }
                            else { numOfTurns = 3 - actor.GetDatapoint(ActorDatapoint.Invisibility2); }
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
                    #endregion

                    break;

                default:

                    #region Actor Captured or other
                    //
                    // - - - Actor Captured or other - - -
                    //
                    cancelText = string.Format("{0}Actor is \"{1}\" and out of contact{2}", colourBad, actor.Status, colourEnd);
                    #endregion

                    break;
            }
        }
        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", actorSlotID)); }

        #region Cancel
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
                buttonTooltipMain = string.Format("{0}Invalid Actor{1}", colourBad, colourEnd),
                //use a Lambda to pass arguments to the action
                action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "ActorManager.cs -> GetActorActions"); }
            };
        }
        //add Cancel button to list
        tempList.Add(cancelDetails);
        #endregion

        return tempList;
    }
    #endregion


    #region GetPlayerActions
    /// <summary>
    /// Right click Player sprite Action menu
    /// </summary>
    /// <returns></returns>
    public List<EventButtonDetails> GetPlayerActions()
    {
        #region SetUp
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
        #endregion

        #region Resistance
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
                    List<string> listOfGear = GameManager.instance.playerScript.GetListOfGear();
                    if (listOfGear != null)
                    {
                        //has gear?
                        if (listOfGear.Count > 0)
                        {
                            for (int index = 0; index < listOfGear.Count; index++)
                            {
                                Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[index]);
                                if (gear != null)
                                {
                                    //Personal Use gear?
                                    if (gear.listOfPersonalEffects != null && gear.listOfPersonalEffects.Count > 0)
                                    {
                                        //create a menu option to use gear
                                        List<Effect> listOfEffects = gear.listOfPersonalEffects;
                                        if (listOfEffects != null)
                                        {
                                            StringBuilder builder = new StringBuilder();
                                            //can't use gear twice in the one turn
                                            if (gear.timesUsed == 0)
                                            {
                                                //effects
                                                if (listOfEffects.Count > 0)
                                                {
                                                    proceedFlag = true;
                                                    for (int i = 0; i < listOfEffects.Count; i++)
                                                    {
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
                                                        if (effectCriteria == null && proceedFlag == true)
                                                        {
                                                            if (i == 0)
                                                            {
                                                                //chance of compromise
                                                                int compromiseChance = GameManager.instance.gearScript.GetChanceOfCompromise(gear.name);
                                                                builder.AppendFormat("{0}Chance of Gear being Compromised {1}{2}{3}%{4}", colourAlert, colourEnd,
                                                                    colourNeutral, compromiseChance, colourEnd);
                                                            }
                                                            //Effect criteria O.K -> tool tip text
                                                            builder.AppendLine();
                                                            builder.AppendFormat("{0}{1}{2}", colourEffect, effect.description, colourEnd);
                                                            //mood change
                                                            if (effect.isMoodEffect == true)
                                                            {
                                                                /*if (builder.Length > 0) { builder.AppendLine(); }*/
                                                                string moodText = GameManager.instance.personScript.GetMoodTooltip(effect.belief, "Player");
                                                                builder.Append(moodText);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //nneded because of possible mood effect that would cause a repeat of the first effect
                                                            if (effectCriteria != null)
                                                            {
                                                                proceedFlag = false;
                                                                //invalid effect criteria -> Action cancelled
                                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                                infoBuilder.AppendFormat("{0}USE {1} invalid{2}{3}{4}({5}){6}{7}",
                                                                    colourInvalid, gear.tag, colourEnd, "\n", colourBad, effectCriteria, colourEnd, "\n");
                                                            }
                                                        }
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
                                                gearActionDetails.gearName = gear.name;
                                                EventButtonDetails gearDetails = new EventButtonDetails()
                                                {
                                                    buttonTitle = string.Format("Use {0}", gear.tag),
                                                    buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, "INFO", colourEnd),
                                                    buttonTooltipMain = string.Format("Use {0} (Player)", gear.tag),
                                                    buttonTooltipDetail = builder.ToString(),
                                                    //use a Lambda to pass arguments to the action
                                                    action = () => { EventManager.instance.PostNotification(EventType.UseGearAction, this, gearActionDetails, "ActorManager.cs -> GetPlayerAction"); }
                                                };
                                                //add USE to list
                                                tempList.Add(gearDetails);
                                            }
                                            else
                                            {
                                                //gear has already been used this turn
                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                infoBuilder.AppendFormat("{0}{1}{2} can't be Used{3}{4}(Used this turn){5}", colourNeutral, gear.tag, colourEnd, "\n", colourBad, colourEnd);
                                            }
                                        }
                                        else
                                        { infoBuilder.AppendFormat("USE Action Invalid{0}{1}(None for this Gear){2}", "\n", colourBad, colourEnd); }
                                    }
                                }
                                else { Debug.LogWarningFormat("Invalid gear (Null) for gearID {0}", listOfGear[index]); }
                            }
                        }
                        else
                        {
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.Append("No Gear to USE");
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
                            if (lieLowTimer == 0)
                            {
                                if (GameManager.instance.factionScript.isHqRelocating == false)
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
                                {
                                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                    infoBuilder.AppendFormat("{0}Lie Low unavailable as HQ Relocating{1}", colourAlert, colourEnd);
                                }
                            }
                            else
                            {
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                infoBuilder.AppendFormat("Lie Low unavailable for {0} turn{1}", lieLowTimer, lieLowTimer != 1 ? "s" : "");
                            }
                        }
                        else
                        {
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.AppendFormat("{0}Surveillance Crackdown{1}{2}{3}(can't Lie Low){4}", colourAlert, colourEnd, "\n", colourBad, colourEnd);
                        }
                    }
                    else
                    {
                        //actor invisiblity at max
                        if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                        infoBuilder.AppendFormat("{0}Can't Lie Low{1}{2}{3}(Invisibility at Max){4}", colourAlert, colourEnd, "\n", colourBad, colourEnd);
                        //
                        // - - - Stress Leave - - -
                        //
                        if (GameManager.instance.playerScript.CheckConditionPresent(conditionStressed, playerSide) == true)
                        {
                            //must be stressed and have enough renown
                            if (GameManager.instance.playerScript.Renown >= stressLeaveRenownCostResistance)
                            {
                                //can't take Stress Leave if a Surveillance Crackdown is in place
                                if (securityState != AuthoritySecurityState.SurveillanceCrackdown)
                                {
                                    if (stressLeaveHQApproval == true)
                                    {
                                        if (GameManager.instance.factionScript.isHqRelocating == false)
                                        {
                                            ModalActionDetails leaveActionDetails = new ModalActionDetails();
                                            leaveActionDetails.side = playerSide;
                                            leaveActionDetails.actorDataID = GameManager.instance.playerScript.actorID;
                                            leaveActionDetails.renownCost = stressLeaveRenownCostResistance;
                                            tooltipText = "It's a wise person who knows when to step back for a moment and gather their thoughts";
                                            EventButtonDetails leaveDetails = new EventButtonDetails()
                                            {
                                                buttonTitle = "Stress Leave",
                                                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "Player Action", colourEnd),
                                                buttonTooltipMain = "Recover from your Stress",
                                                buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                                                //use a Lambda to pass arguments to the action
                                                action = () => { EventManager.instance.PostNotification(EventType.LeavePlayerAction, this, leaveActionDetails, "ActorManager.cs -> GetPlayerActions"); }
                                            };
                                            //add Activate button to list
                                            tempList.Add(leaveDetails);
                                        }
                                        else
                                        {
                                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                            infoBuilder.AppendFormat("{0}Stress Leave unavailable as HQ Relocating{1}", colourAlert, colourEnd);
                                        }
                                    }
                                    else
                                    {
                                        if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                        infoBuilder.AppendFormat("{0}HQ has banned all Stress Leave{1}", colourAlert, colourEnd);
                                    }
                                }
                                else
                                {
                                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                    infoBuilder.AppendFormat("{0}Stress Leave not possible during a Surveillance Crackdown{1}", colourAlert, colourEnd);
                                }
                            }
                            else
                            {
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                infoBuilder.AppendFormat("{0}Insufficient Renown for Stress Leave (need {1}{2}{3}{4}{5}){6}", colourAlert, colourEnd, colourNeutral, stressLeaveRenownCostResistance,
                                    colourEnd, colourAlert, colourEnd);
                            }
                        }
                        else
                        {
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.Append("Stress Leave only if STRESSED");
                        }
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
        #endregion

        #region Authority
        //
        // - - - Authority actions - - -
        //
        else
        {
            //
            // - - - Recruit - - -
            //
            int numOfActors = GameManager.instance.dataScript.CheckNumOfActorsInReserve();
            //spare space for a new recruit?
            if (numOfActors < maxNumOfReserveActors)
            {
                if (GameManager.instance.factionScript.isHqRelocating == false)
                {
                    ModalActionDetails recruitActionDetails = new ModalActionDetails() { };
                    recruitActionDetails.side = playerSide;
                    recruitActionDetails.actorDataID = GameManager.instance.playerScript.actorID;
                    recruitActionDetails.level = 2;
                    tooltipText = "There's a job to be done and you're the person to find the people to do it";
                    EventButtonDetails activateDetails = new EventButtonDetails()
                    {
                        buttonTitle = "Recruit",
                        buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "Mayoral Action", colourEnd),
                        buttonTooltipMain = "Recruit a subordinate",
                        buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                        //use a Lambda to pass arguments to the action
                        action = () => { EventManager.instance.PostNotification(EventType.RecruitAction, this, recruitActionDetails, "ActorManager.cs -> GetPlayerActions"); }
                    };
                    //add Activate button to list
                    tempList.Add(activateDetails);
                }
                else
                {
                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                    infoBuilder.AppendFormat("{0}Lie Low unavailable as HQ Relocating{1}", colourAlert, colourEnd);
                }
            }
            else
            {
                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                infoBuilder.AppendFormat("{0}Can't Recruit as Maxxed out{1}", colourAlert, colourEnd);
            }
            //
            // - - - Stress Leave - - -
            //
            //Player is stressed
            if (GameManager.instance.playerScript.CheckConditionPresent(conditionStressed, GameManager.instance.sideScript.PlayerSide) == true)
            {
                //Player has enough renown
                if (GameManager.instance.playerScript.Renown >= stressLeaveRenownCostAuthority)
                {
                    if (GameManager.instance.factionScript.isHqRelocating == false)
                    {
                        ModalActionDetails leaveActionDetails = new ModalActionDetails();
                        leaveActionDetails.side = playerSide;
                        leaveActionDetails.actorDataID = GameManager.instance.playerScript.actorID;
                        leaveActionDetails.renownCost = stressLeaveRenownCostAuthority;
                        tooltipText = "It's a wise person who knows when to step back for a moment and gather their thoughts";
                        EventButtonDetails leaveDetails = new EventButtonDetails()
                        {
                            buttonTitle = "Stress Leave",
                            buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "Mayoral Action", colourEnd),
                            buttonTooltipMain = "Recover from your Stress",
                            buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                            //use a Lambda to pass arguments to the action
                            action = () => { EventManager.instance.PostNotification(EventType.LeavePlayerAction, this, leaveActionDetails, "ActorManager.cs -> GetPlayerActions"); }
                        };
                        //add Activate button to list
                        tempList.Add(leaveDetails);
                    }
                    else
                    {
                        if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                        infoBuilder.AppendFormat("{0}Lie Low unavailable as HQ Relocating{1}", colourAlert, colourEnd);
                    }
                }
                else { infoBuilder.AppendFormat("{0}Stress Leave requires {1} Renown{2}", colourAlert, stressLeaveRenownCostAuthority, colourEnd); }
            }
            else
            { infoBuilder.AppendFormat("{0}No Leave possible as not Stressed{1}", colourAlert, colourEnd); }

        }
        #endregion

        #region Cancel
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
        #endregion

        return tempList;
    }
    #endregion


    #region GetGearInventoryActions
    /// <summary>
    /// Returns a list of all relevant actions for Gear in the player's Inventory (right click gear sprite in inventory)
    /// Resistance only -> up to 4 x 'Give Gear to Actor', 1 x 'Use' (if there is a viable use gear action), 1 x Cancel
    /// </summary>
    public List<EventButtonDetails> GetGearInventoryActions(string gearName)
    {
        //return list of button details
        List<EventButtonDetails> eventList = new List<EventButtonDetails>();
        if (string.IsNullOrEmpty(gearName) == false)
        {
            //Cancel button tooltip (handles all no go cases)
            StringBuilder infoBuilder = new StringBuilder();
            string effectCriteria, colourEffect;
            string cancelText = null;
            bool proceedFlag = false;
            int benefit;
            Gear gear = GameManager.instance.dataScript.GetGear(gearName);
            if (gear != null)
            {
                #region Use
                //
                // - - - Use - - -
                //
                List<Effect> listOfEffects = gear.listOfPersonalEffects;
                if (listOfEffects != null)
                {
                    if (gear.timesUsed == 0)
                    {
                        //effects
                        StringBuilder builder = new StringBuilder();
                        if (listOfEffects.Count > 0)
                        {
                            proceedFlag = true;
                            for (int i = 0; i < listOfEffects.Count; i++)
                            {

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
                                { listOfCriteria = effect.listOfCriteria };
                                effectCriteria = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                                if (effectCriteria == null && proceedFlag == true)
                                {
                                    if (i == 0)
                                    {
                                        //chance of compromise
                                        int compromiseChance = GameManager.instance.gearScript.GetChanceOfCompromise(gear.name);
                                        builder.AppendFormat("{0}Chance of Gear being Compromised {1}{2}{3}%{4}", colourAlert, colourEnd,
                                            colourNeutral, compromiseChance, colourEnd);
                                    }
                                    //Effect criteria O.K -> tool tip text
                                    if (builder.Length > 0) { builder.AppendLine(); }
                                    builder.AppendFormat("{0}{1}{2}", colourEffect, effect.description, colourEnd);
                                    //mood info
                                    if (effect.isMoodEffect == true)
                                    {
                                        string moodText = GameManager.instance.personScript.GetMoodTooltip(effect.belief, "Player");
                                        builder.Append(moodText);
                                    }
                                }
                                else
                                {
                                    //needed because of possible mood effect that would cause a repeat of the first effect
                                    if (effectCriteria != null)
                                    {
                                        proceedFlag = false;
                                        //invalid effect criteria -> Action cancelled
                                        if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                        infoBuilder.AppendFormat("{0}USE action invalid{1}{2}{3}({4}){5}",
                                            colourInvalid, colourEnd, "\n", colourBad, effectCriteria, colourEnd);
                                    }
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
                            gearActionDetails.gearName = gear.name;
                            gearActionDetails.modalLevel = 2;
                            gearActionDetails.modalState = ModalSubState.Inventory;
                            gearActionDetails.handler = GameManager.instance.inventoryScript.RefreshInventoryUI;
                            EventButtonDetails gearDetails = new EventButtonDetails()
                            {
                                buttonTitle = "Use",
                                buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, "INFO", colourEnd),
                                buttonTooltipMain = string.Format("Use {0} (Player)", gear.tag),
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
                    {
                        //gear has already been used this turn
                        if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                        infoBuilder.AppendFormat("{0}{1}{2} can't be Used{3}{4}(Used this turn){5}", colourNeutral, gear.tag, colourEnd, "\n", colourBad, colourEnd);
                    }
                }
                else
                { infoBuilder.AppendFormat("USE Action Invalid{0}{1}(None for this Gear){2}", "\n", colourBad, colourEnd); }
                #endregion

                #region Give To
                //
                // - - - Give to - - -
                //
                cancelText = string.Format("{0} {1}", gear.type.name.ToUpper(), gear.tag);
                if (gear.timesUsed == 0)
                {
                    //Loop current, onMap actors
                    Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
                    if (arrayOfActors != null)
                    {
                        for (int i = 0; i < arrayOfActors.Length; i++)
                        {
                            //check actor present in slot
                            if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
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
                                        gearActionDetails.actorDataID = actor.slotID;
                                        gearActionDetails.gearName = gear.name;
                                        gearActionDetails.modalLevel = 2;
                                        gearActionDetails.modalState = ModalSubState.Inventory;
                                        gearActionDetails.handler = GameManager.instance.inventoryScript.RefreshInventoryUI;
                                        //get actor's preferred gear
                                        GearType preferredGear = actor.arc.preferredGear;
                                        if (preferredGear != null)
                                        {
                                            benefit = gearSwapBaseAmount;
                                            if (preferredGear.name.Equals(gear.type.name, StringComparison.Ordinal) == true)
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
                                        //mood info
                                        string moodText = GameManager.instance.personScript.GetMoodTooltip(MoodType.GiveGear, actor.arc.name);
                                        builderTooltip.AppendFormat("{0}{1}", "\n", moodText);
                                        //existing gear
                                        if (string.IsNullOrEmpty(actor.GetGearName()) == false)
                                        {
                                            Gear gearOld = GameManager.instance.dataScript.GetGear(actor.GetGearName());
                                            if (gearOld != null)
                                            { builderTooltip.AppendFormat("{0}{1}{2} will be Lost{3}", "\n", colourBad, gearOld.name, colourEnd); }
                                            else { Debug.LogWarningFormat("Invalid gearOld (Null) for gearID {0}", actor.GetGearName()); }
                                        }
                                        EventButtonDetails gearDetails = new EventButtonDetails()
                                        {
                                            buttonTitle = string.Format("Give to {0}", actor.arc.name),
                                            buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, "INFO", colourEnd),
                                            buttonTooltipMain = string.Format("Give {0} ({1}{2}{3}) to {4} {5}", gear.tag, colourNeutral, gear.type.name,
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
                    }
                    else
                    { Debug.LogError("Invalid arrayOfActors (Null)"); }
                }
                else
                {
                    //gear has already been used this turn
                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                    infoBuilder.AppendFormat("{0}{1}{2} can't be gifted{3}{4}(Used this turn){5}", colourNeutral, gear.tag, colourEnd, "\n", colourBad, colourEnd);
                }
                #endregion
            }
            else
            {
                Debug.LogError(string.Format("Invalid gear (Null) for gear {0}", gearName));
            }

            #region Cancel
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
            #endregion
        }
        else { Debug.LogError("Invalid gearName (Null)"); }
        return eventList;
    }
    #endregion


    #region GetReservePoolActions
    /// <summary>
    /// Returns a list of all relevant actions an Actor in the Reserve Pool (right click actor sprite in inventory)
    /// Both sides -> 1 x Active Duty, 1 x Let Go / Fire (unhappy), 1 x Reassure,  1 x Cancel
    /// </summary>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public List<EventButtonDetails> GetReservePoolActions(int actorID)
    {
        #region SetUp
        //return list of button details
        List<EventButtonDetails> eventList = new List<EventButtonDetails>();
        //Cancel button tooltip (handles all no go cases)
        StringBuilder infoBuilder = new StringBuilder();
        string sideColour;
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
        actorActionDetails.modalState = ModalSubState.Inventory;
        actorActionDetails.handler = GameManager.instance.inventoryScript.RefreshInventoryUI;
        #endregion

        //actor
        Actor actor = GameManager.instance.dataScript.GetActor(actorID);
        if (actor != null)
        {
            cancelText = string.Format("{0} {1}", actor.arc.name, actor.actorName);

            #region ActiveDuty
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
                    builderActive.AppendFormat("{0}{1} will no longer be Unhappy or Complain{2}{3}", colourGood, actor.actorName, colourEnd, "\n");
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
            #endregion

            #region Reassure
            //
            // - - - Reassure - - -
            //
            if (actor.unhappyTimer > 0)
            {
                if (actor.isReassured == false)
                {
                    //trait Cranky
                    if (actor.CheckTraitEffect(actorReserveActionNone) == false)
                    {
                        StringBuilder builder = new StringBuilder();
                        if (actor.CheckTraitEffect(actorReserveActionDoubled) == false)
                        {
                            builder.AppendFormat("{0}{1}'s Unhappy Timer +{2}{3}{4}{5}Can only be Reassured once{6}", colourGood, actor.actorName,
                              unhappyReassureBoost, colourEnd, "\n", colourNeutral, colourEnd);
                        }
                        else
                        {
                            builder.AppendFormat("{0}{1}'s Unhappy Timer +{2} ({3}){4}{5}{6}Can only be Reassured once{7}", colourGood, actor.actorName,
                              unhappyReassureBoost * 2, colourEnd, actor.GetTrait().tag, "\n", colourNeutral, colourEnd);
                        }
                        //mood info
                        string moodText = GameManager.instance.personScript.GetMoodTooltip(MoodType.ReserveReassure, actor.arc.name);
                        builder.AppendFormat("{0}{1}", "\n", moodText);
                        EventButtonDetails actorDetails = new EventButtonDetails()
                        {
                            buttonTitle = "Reassure",
                            buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                            buttonTooltipMain = string.Format(string.Format("Reassure {0} that they will be the next person called for active duty", actor.actorName)),
                            buttonTooltipDetail = builder.ToString(),
                            //use a Lambda to pass arguments to the action
                            action = () => { EventManager.instance.PostNotification(EventType.InventoryReassure, this, actorActionDetails, "ActorManager.cs -> GetReservePoolActions"); },

                        };
                        //add Lie Low button to list
                        eventList.Add(actorDetails);
                    }
                    else
                    {
                        //not allowed due to trait (Cranky)
                        if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                        infoBuilder.AppendFormat("{0} can't be Reassured ({1} trait)", actor.actorName, actor.GetTrait().tag);
                        TraitLogMessage(actor, "for, possibly, being Reassured", "to PREVENT a Reassurance attempt");
                    }
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
            #endregion

            #region Let Go
            //
            // - - - Let go - - -
            //
            if (actor.unhappyTimer > 0)
            {
                if (actor.isNewRecruit == true)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("{0}{1}'s{2}Motivation -1{3}{4}{5}Can be recruited again{6}", colourBad, actor.actorName, "\n",
                        colourEnd, "\n", colourNeutral, colourEnd);
                    //mood info
                    string moodText = GameManager.instance.personScript.GetMoodTooltip(MoodType.ReserveLetGo, actor.arc.name);
                    builder.AppendFormat("{0}{1}", "\n", moodText);
                    EventButtonDetails actorDetails = new EventButtonDetails()
                    {
                        buttonTitle = "Let Go",
                        buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                        buttonTooltipMain = string.Format(string.Format("You don't want to but unfortunately you're going to have to let {0} go", actor.actorName)),
                        buttonTooltipDetail = builder.ToString(),
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
            #endregion

            #region Bully
            //
            // - - - Bully - - -
            //
            if (actor.unhappyTimer > 0)
            {
                if (actor.CheckTraitEffect(actorReserveActionNone) == false)
                {
                    renownCost = actor.numOfTimesBullied + 1;
                    if (playerRenown >= renownCost)
                    {

                        StringBuilder builder = new StringBuilder();
                        builder.AppendFormat("{0}{1}'s Unhappy Timer +{2}{3}", colourGood, actor.actorName, unhappyBullyBoost, colourEnd);
                        builder.AppendLine();
                        builder.AppendFormat("{0}Player Renown -{1}{2}", colourBad, renownCost, colourEnd);
                        builder.AppendLine();
                        builder.AppendFormat("{0}Can be Bullied again{1}{2}{3}(Renown cost +1){4}", colourNeutral, colourEnd, "\n", colourBad, colourEnd);
                        //mood info
                        string moodText = GameManager.instance.personScript.GetMoodTooltip(MoodType.ReserveBully, actor.arc.name);
                        builder.AppendFormat("{0}{1}", "\n", moodText);
                        EventButtonDetails actorDetails = new EventButtonDetails()
                        {
                            buttonTitle = "Bully",
                            buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                            buttonTooltipMain = string.Format("You eyeball {0} and tell them that if they don't stop complaining you'll speak to HQ", actor.actorName),
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
                        infoBuilder.AppendFormat("{0}Insufficient Renown to Bully (need {1}, currently have {2}{3})", colourBad, renownCost, playerRenown, colourEnd);
                    }
                }
                else
                {
                    //not allowed due to trait (Cranky)
                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                    infoBuilder.AppendFormat("{0} can't be Bullied ({1} trait)", actor.actorName, actor.GetTrait().tag);
                    TraitLogMessage(actor, "for, possibly, being Bullied", "to PREVENT being Bullied");
                }
            }
            else
            {
                //can't threaten somebody who is already unhappy
                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                infoBuilder.AppendFormat("{0}Can't Bully if Unhappy.{1}", colourBad, colourEnd);
            }
            #endregion

            #region Fire/Dismiss
            //
            // - - - Fire (Dismiss) - - -
            //
            //only show button if player has enough renown to cover the cost of firing
            if (playerRenown >= renownCost)
            {
                //generic tooltip (depends if actor is threatening or not)
                StringBuilder builderTooltip = new StringBuilder();
                //allow for secrets and threats
                ManageRenownCost manageRenownCost = GetManageRenownCost(actor, manageDismissRenown);
                renownCost = manageRenownCost.renownCost;
                //tooltip
                builderTooltip.AppendFormat("{0}Player Renown -{1}{2}", colourBad, renownCost, colourEnd);
                if (string.IsNullOrEmpty(manageRenownCost.tooltip) == false)
                { builderTooltip.Append(manageRenownCost.tooltip); }
                //mood info
                string moodText = GameManager.instance.personScript.GetMoodTooltip(MoodType.ReserveFire, actor.arc.name);
                builderTooltip.AppendFormat("{0}{1}", "\n", moodText);
                //pass through renown cost
                actorActionDetails.renownCost = renownCost;
                //action button
                EventButtonDetails actorDetails = new EventButtonDetails()
                {
                    buttonTitle = "DISMISS",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = string.Format(string.Format("You inform {0} that their services are no longer needed, or desired", actor.actorName)),
                    buttonTooltipDetail = builderTooltip.ToString(),
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.InventoryFire, this, actorActionDetails, "ActorManager.cs -> GetReservePoolActions"); },
                };
                //add Dismiss button to list
                eventList.Add(actorDetails);
            }
            else
            {
                //not enough renown
                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                infoBuilder.AppendFormat("{0}Insufficient Renown to Dismiss (need {1}, currently have {2}{3})", colourBad, renownCost, playerRenown, colourEnd);
            }
            #endregion

        }
        else
        { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", actorID); }

        #region Cancel
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
        #endregion

        return eventList;
    }
    #endregion


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

        #region cachedSelection
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
        #endregion

        //proceed with a new Recruit Selection
        if (isIgnoreCache == true)
        {
            #region RecruitActorBothSides
            //Recruit actor (both sides)
            if (details.side.level == globalAuthority.level || node != null)
            {

                #region setupAdmin
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
                List<string> listOfCurrentArcs = new List<string>(GameManager.instance.dataScript.GetAllCurrentActorArcs(details.side));
                #endregion

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
                                if (listOfCurrentArcs.Exists(x => x == actor.arc.name))
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
                numOfOptions = Math.Min(maxGenericOptions, listOfPoolActors.Count);
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
                            optionDetails.sprite = actor.sprite;
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
                                builder.AppendFormat("{0}  {1}{2}{3}{4}", arrayOfQualities[0], GameManager.instance.colourScript.GetValueColour(actor.GetDatapoint(ActorDatapoint.Datapoint0)),
                                    actor.GetDatapoint(ActorDatapoint.Datapoint0), colourEnd, "\n");
                                builder.AppendFormat("{0}  {1}{2}{3}{4}", arrayOfQualities[1], GameManager.instance.colourScript.GetValueColour(actor.GetDatapoint(ActorDatapoint.Datapoint1)),
                                    actor.GetDatapoint(ActorDatapoint.Datapoint1), colourEnd, "\n");
                                builder.AppendFormat("{0}  {1}{2}{3}", arrayOfQualities[2], GameManager.instance.colourScript.GetValueColour(actor.GetDatapoint(ActorDatapoint.Datapoint2)),
                                    actor.GetDatapoint(ActorDatapoint.Datapoint2), colourEnd);
                                tooltipDetails.textMain = string.Format("{0}{1}{2}", colourNormal, builder.ToString(), colourEnd);
                            }
                            //trait and action
                            tooltipDetails.textDetails = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "<font=\"Bangers SDF\">", "<cspace=0.6em>", actor.GetTrait().tagFormatted,
                                "</cspace>", "</font>", "\n", colourNormal, actor.arc.nodeAction.tag, colourEnd);
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
            //deactivate back button
            GameManager.instance.genericPickerScript.SetBackButton(EventType.None);
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
                        {
                            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, cachedResistancePlayerDetails, "ActorManager.cs -> InitialiseGenericPickerRecruit");
                        }
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
    /// sets up all needed data for HQ Hierarchy only Actors and triggers ModalInventoryUI to display such
    /// </summary>
    private void InitialiseHqHierarchyInventory()
    {
        int numOfActors = 0;
        int offset = 1;             //accomodates fact that first entry in arrayOfActorsHQ is 'None'
        int lengthOfArray;
        int motivation;
        bool errorFlag = false;
        bool isBoss;
        string title;
        //close all modal 0 tooltips
        GameManager.instance.guiScript.SetTooltipsOff();
        //get HQ actors
        Actor[] arrayOfHqActors = GameManager.instance.dataScript.GetArrayOfActorsHQ();
        lengthOfArray = arrayOfHqActors.Length;
        if (arrayOfHqActors != null)
        {
            //tally up actors (ignore non-hierarchy actors)
            for (int i = 0; i < lengthOfArray; i++)
            { if (arrayOfHqActors[i] != null) { numOfActors++; } }
            //should be a full compliment at all times (ignore first and last entries, 'None' and 'Worker', 'LeftHQ' & 'Count')
            if (numOfActors != arrayOfHqActors.Length - 3)
            { Debug.LogWarningFormat("Mismatch for arrayOfHqActors (has {0} actors, should have {1}){2}", numOfActors, lengthOfArray - 2, "\n"); }
            if (numOfActors > 0)
            {
                //At least one actor in HQ
                InventoryInputData data = new InventoryInputData();
                data.side = GameManager.instance.sideScript.PlayerSide;
                data.textHeader = "HQ Hierarchy";
                data.textTop = string.Format("This is the {0}{1}{2} HQ Hierarchy", colourNeutral, data.side.name, colourEnd);
                data.textBottom = string.Format("{0}LEFT CLICK{1} portrait for more information", colourAlert, colourEnd);
                data.state = InventoryState.HQ;
                //loop actors and populate data packages (only want hierarchy entries)
                for (int i = offset; i < offset + GameManager.instance.factionScript.numOfActorsHQ; i++)
                {
                    Actor actor = arrayOfHqActors[i];
                    if (actor != null)
                    {
                        motivation = actor.GetDatapoint(ActorDatapoint.Datapoint1);
                        title = GameManager.instance.campaignScript.GetHqTitle((ActorHQ)i);
                        if ((ActorHQ)i == ActorHQ.Boss) { isBoss = true; } else { isBoss = false; }
                        InventoryOptionData optionData = new InventoryOptionData();
                        optionData.sprite = actor.sprite;
                        optionData.textUpper = string.Format("{0}{1}{2}", isBoss == true ? colourNeutral : colourAlert, title, colourEnd);
                        //motivation stars
                        optionData.textLower = GameManager.instance.guiScript.GetStars(motivation);
                        /*optionData.textLower = actor.actorName;*/
                        optionData.optionID = actor.actorID;
                        //tooltip -> sprite
                        GenericTooltipDetails tooltipDetailsSprite = new GenericTooltipDetails();
                        tooltipDetailsSprite.textHeader = string.Format("{0}{1}{2}<size=120%>{3}{4}", actor.actorName, "\n", isBoss == true ? colourNeutral : colourAlert, title.ToUpper(), colourEnd);
                        tooltipDetailsSprite.textMain = new StringBuilder()
                           .AppendFormat("{0}  {1}{2}{3}{4}", "Motivation", GameManager.instance.colourScript.GetValueColour(motivation),
                                   actor.GetDatapoint(ActorDatapoint.Datapoint1), colourEnd, "\n")
                           .AppendFormat("{0}  {1}{2}{3}", "Renown", colourNeutral, actor.Renown, colourEnd)
                           .ToString();
                        if (isBoss == true)
                        { tooltipDetailsSprite.textDetails = string.Format("Opinion of your{0}Decisions{1}{2}", "\n", "\n", GameManager.instance.factionScript.GetBossOpinionFormatted()); }
                        //tooltip -> stars (bottom text, motivation -> same for all)
                        GenericTooltipDetails tooltipDetailsStars = new GenericTooltipDetails();
                        tooltipDetailsStars.textHeader = string.Format("{0}'s{1}{2}<size=120%>MOTIVATION{3}", actor.actorName, "\n", colourNeutral, colourEnd);
                        tooltipDetailsStars.textMain = string.Format("A measure of the {0}{1}{2}'s{3}{4}{5}willingness to help you{6}", "\n", colourAlert, title, colourEnd, "\n", colourNeutral, colourEnd);
                        tooltipDetailsStars.textDetails = string.Format("0 to 3 stars{0}{1}Higher the better{2}", "\n", colourAlert, colourEnd);
                        //add to arrays
                        data.arrayOfOptions[i - offset] = optionData;
                        data.arrayOfTooltipsSprite[i - offset] = tooltipDetailsSprite;
                        data.arrayOfTooltipsStars[i - offset] = tooltipDetailsStars;
                    }
                    else { Debug.LogWarningFormat("Invalid actor (Null) in arrayOfHqActors[{0}]", i); }
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
                    EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "ActorManager.cs -> InitialiseHqActorsInventory");
                }
                else
                {
                    //open Inventory UI
                    EventManager.instance.PostNotification(EventType.InventoryOpenUI, this, data, "ActorManager.cs -> InitialiseHqActorsInventory");
                }
            }
            else
            {
                //no actor in HQ
                ModalOutcomeDetails details = new ModalOutcomeDetails()
                {
                    side = GameManager.instance.sideScript.PlayerSide,
                    textTop = string.Format("{0}HQ is currently unmanned{1}", colourInvalid, colourEnd),
                    textBottom = "Frantic phone calls are being made",
                    sprite = GameManager.instance.guiScript.infoSprite,
                    isAction = false
                };
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "ActorManager.cs -> InitialiseHqActorsInventory");
            }
        }
        else { Debug.LogError("Invalid arrayOfHQActors (Null)"); }
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
            data.textHeader = "Reserves";
            data.textTop = string.Format("{0}You have {1}{2}{3}{4}{5} out of {6}{7}{8}{9}{10} possible Actor{11} in your Reserve pool{12}", colourNeutral, colourEnd,
                colourDefault, numOfActors, colourEnd, colourNeutral, colourEnd, colourDefault, maxNumOfReserveActors, colourEnd, colourNeutral,
                maxNumOfReserveActors != 1 ? "s" : "", colourEnd);
            data.textBottom = string.Format("{0}LEFT CLICK{1}{2} portrait for Info, {3}{4}RIGHT CLICK{5}{6} portrait for Options{7}", colourAlert, colourEnd, colourDefault,
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
                            optionData.sprite = actor.sprite;
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
                                builder.AppendFormat("{0}  {1}{2}{3}{4}", arrayOfQualities[0], GameManager.instance.colourScript.GetValueColour(actor.GetDatapoint(ActorDatapoint.Datapoint0)),
                                    actor.GetDatapoint(ActorDatapoint.Datapoint0), colourEnd, "\n");
                                builder.AppendFormat("{0}  {1}{2}{3}{4}", arrayOfQualities[1], GameManager.instance.colourScript.GetValueColour(actor.GetDatapoint(ActorDatapoint.Datapoint1)),
                                    actor.GetDatapoint(ActorDatapoint.Datapoint1), colourEnd, "\n");
                                builder.AppendFormat("{0}  {1}{2}{3}", arrayOfQualities[2], GameManager.instance.colourScript.GetValueColour(actor.GetDatapoint(ActorDatapoint.Datapoint2)),
                                    actor.GetDatapoint(ActorDatapoint.Datapoint2), colourEnd);
                                tooltipDetails.textMain = string.Format("{0}{1}{2}", colourNormal, builder.ToString(), colourEnd);
                            }
                            //trait and action
                            StringBuilder builderDetails = new StringBuilder();
                            builderDetails.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "<font=\"Bangers SDF\">", "<cspace=0.6em>",
                                actor.GetTrait().tagFormatted, "</cspace>", "</font>", "\n", colourNormal, actor.arc.nodeAction.tag, colourEnd);
                            //secrets
                            int numOfSecrets = actor.CheckNumOfSecrets();
                            if (numOfSecrets > 0)
                            {
                                builderDetails.AppendFormat("{0}{1}Secrets{2}", "\n", colourAlert, colourEnd);
                                List<string> listOfSecrets = actor.GetSecretsTooltipList();
                                if (listOfSecrets != null)
                                {
                                    foreach (string secret in listOfSecrets)
                                    { builderDetails.AppendFormat("{0}{1}<b>{2}</b>{3}", "\n", colourNeutral, secret, colourEnd); }
                                }
                                else { Debug.LogWarning("Invalid listOfSecrets (Null)"); }

                            }
                            //gear
                            string gearName = actor.GetGearName();
                            if (string.IsNullOrEmpty(gearName) == false)
                            {
                                builderDetails.AppendFormat("{0}{1}Gear{2}", "\n", colourAlert, colourEnd);
                                Gear gear = GameManager.instance.dataScript.GetGear(gearName);
                                if (gear != null)
                                { builderDetails.AppendFormat("{0}{1}<b>{2}</b>{3}", "\n", colourNeutral, gear.tag, colourEnd); }
                                else { Debug.LogWarningFormat("Invalid Gear (Null) for gear {0}", gearName); }
                            }

                            tooltipDetails.textDetails = builderDetails.ToString();
                            //add to arrays
                            data.arrayOfOptions[i] = optionData;
                            data.arrayOfTooltipsSprite[i] = tooltipDetails;
                        }
                        else
                        { Debug.LogWarningFormat("Invalid Actor (Null) for actorID {0}", listOfActors[i]); }
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
                        optionData.sprite = actor.sprite;
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
                            builder.AppendFormat("{0}  {1}{2}{3}{4}", arrayOfQualities[0], GameManager.instance.colourScript.GetValueColour(actor.GetDatapoint(ActorDatapoint.Datapoint0)),
                                actor.GetDatapoint(ActorDatapoint.Datapoint0), colourEnd, "\n");
                            builder.AppendFormat("{0}  {1}{2}{3}{4}", arrayOfQualities[1], GameManager.instance.colourScript.GetValueColour(actor.GetDatapoint(ActorDatapoint.Datapoint1)),
                                actor.GetDatapoint(ActorDatapoint.Datapoint1), colourEnd, "\n");
                            builder.AppendFormat("{0}  {1}{2}{3}", arrayOfQualities[2], GameManager.instance.colourScript.GetValueColour(actor.GetDatapoint(ActorDatapoint.Datapoint2)),
                                actor.GetDatapoint(ActorDatapoint.Datapoint2), colourEnd);
                            tooltipDetails.textMain = string.Format("{0}{1}{2}", colourNormal, builder.ToString(), colourEnd);
                        }
                        //trait and action
                        tooltipDetails.textDetails = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "<font=\"Bangers SDF\">", "<cspace=1em>", actor.GetTrait().tagFormatted,
                            "</cspace>", "</font>", "\n", colourNormal, actor.arc.nodeAction.tag, colourEnd);
                        //add to arrays
                        data.arrayOfOptions[i] = optionData;
                        data.arrayOfTooltipsSprite[i] = tooltipDetails;
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
        bool isPlayer = false;
        int unhappyTimer = recruitedReserveTimer;
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
                            //traits that affect unhappy timer
                            string traitText = "";
                            if (actorRecruited.CheckTraitEffect(actorReserveTimerDoubled) == true)
                            {
                                unhappyTimer *= 2; traitText = string.Format(" ({0})", actorRecruited.GetTrait().tag);
                                TraitLogMessage(actorRecruited, "for their willingness to wait", "to DOUBLE Reserve Unhappy Timer");
                            }
                            else if (actorRecruited.CheckTraitEffect(actorReserveTimerHalved) == true)
                            {
                                unhappyTimer /= 2; unhappyTimer = Mathf.Max(1, unhappyTimer); traitText = string.Format(" ({0})", actorRecruited.GetTrait().tag);
                                TraitLogMessage(actorRecruited, "for their reluctance to wait", "to HALVE Reserve Unhappy Timer");
                            }
                            //change actor's status
                            actorRecruited.Status = ActorStatus.Reserve;
                            //remove actor from appropriate pool list
                            GameManager.instance.dataScript.RemoveActorFromPool(actorRecruited.actorID, actorRecruited.level, playerSide);
                            //sprite of recruited actor
                            sprite = actorRecruited.sprite;
                            //initiliase unhappy timer
                            actorRecruited.unhappyTimer = unhappyTimer;
                            actorRecruited.isNewRecruit = true;
                            //actor successfully recruited
                            builderTop.AppendFormat("{0}The interview went well!{1}", colourNormal, colourEnd);
                            builderBottom.AppendFormat("{0}, {1}{2}{3}, {4}has been recruited and is available in the Reserve List{5}", actorRecruited.actorName, colourArc,
                                actorRecruited.arc.name, colourEnd, colourNormal, colourEnd);
                            builderBottom.AppendFormat("{0}{1}{2}{3} will become Unhappy in {4} turn{5}{6}{7}", "\n", "\n", colourNeutral, actorRecruited.arc.name, unhappyTimer, unhappyTimer != 1 ? "s" : "",
                                traitText, colourEnd);
                            //message
                            string textMsg = string.Format("{0}, {1}, ID {2} has been recruited", actorRecruited.actorName, actorRecruited.arc.name,
                                actorRecruited.actorID);
                            GameManager.instance.messageScript.ActorRecruited(textMsg, data.nodeID, actorRecruited, actorRecruited.unhappyTimer);
                            //stats
                            GameManager.instance.dataScript.StatisticIncrement(StatType.ActorsRecruited);
                            //Process any other effects, if acquisition was successfull, ignore otherwise
                            Action action = actorCurrent.arc.nodeAction;
                            Node node = GameManager.instance.dataScript.GetNode(data.nodeID);
                            if (node != null)
                            {
                                //reset recruit actor cache flags
                                if (GameManager.instance.nodeScript.nodePlayer == node.nodeID)
                                {
                                    isPlayer = true;
                                    isNewActionResistancePlayer = true;
                                }
                                else { isNewActionResistanceActor = true; }
                                //loop effects
                                List<Effect> listOfEffects = action.GetEffects();
                                if (listOfEffects.Count > 0)
                                {
                                    EffectDataInput dataInput = new EffectDataInput();
                                    dataInput.originText = "Recruit Actor";
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
                                //NodeActionData package
                                if (isPlayer == false)
                                {
                                    //actor action
                                    NodeActionData nodeActionData = new NodeActionData()
                                    {
                                        turn = GameManager.instance.turnScript.Turn,
                                        actorID = actorCurrent.actorID,
                                        nodeID = node.nodeID,
                                        dataName = actorRecruited.actorName,
                                        nodeAction = NodeAction.ActorRecruitActor
                                    };
                                    //add to actor's personal list
                                    actorCurrent.AddNodeAction(nodeActionData);
                                    Debug.LogFormat("[Tst] ActorManager.cs -> ProcessRecruitChoiceResistance: nodeActionData added to {0}, {1}{2}", actorCurrent.actorName, actorCurrent.arc.name, "\n");
                                }
                                else
                                {
                                    //player action
                                    NodeActionData nodeActionData = new NodeActionData()
                                    {
                                        turn = GameManager.instance.turnScript.Turn,
                                        actorID = 999,
                                        nodeID = GameManager.instance.nodeScript.nodePlayer,
                                        dataName = actorRecruited.actorName,
                                        nodeAction = NodeAction.PlayerRecruitActor
                                    };
                                    //add to player's personal list
                                    GameManager.instance.playerScript.AddNodeAction(nodeActionData);
                                    Debug.LogFormat("[Tst] ActorManager.cs -> ProcessRecruitChoiceResistance: nodeActionData added to {0}, {1}{2}", GameManager.instance.playerScript.PlayerName, "Player", "\n");
                                    //statistics
                                    GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerNodeActions);
                                }
                                //statistics
                                GameManager.instance.dataScript.StatisticIncrement(StatType.NodeActionsResistance);
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
        int unhappyTimer = recruitedReserveTimer;
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
                        //traits that affect unhappy timer
                        string traitText = "";
                        if (actorRecruited.CheckTraitEffect(actorReserveTimerDoubled) == true)
                        {
                            unhappyTimer *= 2; traitText = string.Format(" ({0})", actorRecruited.GetTrait().tag);
                            TraitLogMessage(actorRecruited, "for their willingness to wait", "to DOUBLE Reserve Unhappy Timer");
                        }
                        else if (actorRecruited.CheckTraitEffect(actorReserveTimerHalved) == true)
                        {
                            unhappyTimer /= 2; unhappyTimer = Mathf.Max(1, unhappyTimer); traitText = string.Format(" ({0})", actorRecruited.GetTrait().tag);
                            TraitLogMessage(actorRecruited, "for their reluctance to wait", "to HALVE Reserve Unhappy Timer");
                        }
                        //change actor's status
                        actorRecruited.Status = ActorStatus.Reserve;
                        //remove actor from appropriate pool list
                        GameManager.instance.dataScript.RemoveActorFromPool(actorRecruited.actorID, actorRecruited.level, side);
                        //sprite of recruited actor
                        sprite = actorRecruited.sprite;
                        //initialise unhappy timer
                        actorRecruited.unhappyTimer = unhappyTimer;
                        actorRecruited.isNewRecruit = true;
                        //message
                        string textMsg = string.Format("{0}, {1}, ID {2} has been recruited", actorRecruited.actorName, actorRecruited.arc.name,
                            actorRecruited.actorID);
                        GameManager.instance.messageScript.ActorRecruited(textMsg, data.nodeID, actorRecruited, actorRecruited.unhappyTimer);
                        //actor successfully recruited
                        builderTop.AppendFormat("{0}The interview went well!{1}", colourNormal, colourEnd);
                        builderBottom.AppendFormat("{0}{1}{2}, {3}\"{4}\", has been recruited and is available in the Reserve List{5}", colourArc,
                            actorRecruited.arc.name, colourEnd, colourNormal, actorRecruited.actorName, colourEnd);
                        builderBottom.AppendFormat("{0}{1}{2}{3} will become Unhappy in {4} turn{5}{6}{7}", "\n", "\n", colourNeutral, actorRecruited.arc.name, unhappyTimer, unhappyTimer != 1 ? "s" : "",
                            traitText, colourEnd);
                        //NodeActionData package
                        NodeActionData nodeActionData = new NodeActionData()
                        {
                            turn = GameManager.instance.turnScript.Turn,
                            actorID = 999,
                            nodeID = GameManager.instance.nodeScript.nodePlayer,
                            dataName = actorRecruited.actorName,
                            nodeAction = NodeAction.PlayerRecruitActor
                        };
                        //add to player's personal list
                        GameManager.instance.playerScript.AddNodeAction(nodeActionData);
                        Debug.LogFormat("[Tst] ActorManager.cs -> ProcessRecruitChoiceAuthority: nodeActionData added to {0}, {1}{2}", GameManager.instance.playerScript.PlayerName, "Player", "\n");
                        //stats
                        GameManager.instance.dataScript.StatisticIncrement(StatType.ActorsRecruited);
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
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        string outputMsg = "Unknown";
        string threatMsg = "Unknown";
        if (actor != null)
        {
            //trait -> Team Player (avoids conflicts)
            if (actor.CheckTraitEffect(actorConflictNone) == false)
            {
                GlobalType typeGood = GameManager.instance.globalScript.typeGood;
                List<ActorConflict> listSelectionPool = new List<ActorConflict>();
                Dictionary<string, ActorConflict> dictOfActorConflicts = GameManager.instance.dataScript.GetDictOfActorConflicts();
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
                                        criteriaData.actorSlotID = actor.slotID;
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
                                    TraitLogMessage(actor, "for determining the type of Relationship Conflict", "to ELIMINATE any Good options");
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
                                case "Resigns":
                                    if (actor.CheckTraitEffect(actorNeverResigns) == true)
                                    {
                                        outputMsg = string.Format("{0}does NOT Resign{1}", colourGood, colourEnd);
                                        threatMsg = "does NOT Resign";
                                        TraitLogMessage(actor, "for a Resign conflict outcome", "to AVOID Resigning");
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(actorConflict.outcomeText) == false)
                                        {
                                            outputMsg = string.Format("{0}{1}{2}", colourAlert, actorConflict.outcomeText, colourEnd);
                                            threatMsg = actorConflict.outcomeText;
                                        }
                                    }
                                    break;
                                default:
                                    //normally use ActorConflict.outcomeText for top line
                                    if (string.IsNullOrEmpty(actorConflict.outcomeText) == false)
                                    {
                                        outputMsg = string.Format("{0}{1}{2}", colourAlert, actorConflict.outcomeText, colourEnd);
                                        threatMsg = actorConflict.outcomeText;
                                    }
                                    break;
                            }
                            //Statistics (counts conflict regardless of outcome of conflict)
                            GameManager.instance.dataScript.StatisticIncrement(StatType.ActorConflicts);
                            actor.numOfTimesConflict++;
                            //Implement effect (if any, no effect for a 'do nothing')
                            if (actorConflict.effect != null)
                            {
                                //data packages
                                effectReturn = new EffectDataReturn();
                                EffectDataInput effectInput = new EffectDataInput();
                                effectInput.originText = "Relationship Conflict";
                                //message
                                string msgText = string.Format("{0} Relationship Conflict ({1})", actor.arc.name, threatMsg);
                                GameManager.instance.messageScript.ActorConflict(msgText, actor, actorConflict);
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
                                GameManager.instance.messageScript.ActorConflict(msgText, actor, null, "DO NOTHING");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("ActorManager.cs -> ProcessActorConflict: The selection Pool has no entries in it");
                            builder.AppendFormat("{0}{1} stamps their feet{2}", colourAlert, actor.arc.name, colourEnd);
                            builder.AppendFormat("{0}{1} Nothing happens{2}", "\n", colourGood, colourEnd);
                            //message
                            string msgText = string.Format("{0} Relationship Conflict ({1}Nothing Happens{2})", actor.arc.name, colourGood, colourEnd);
                            GameManager.instance.messageScript.ActorConflict(msgText, actor, null, "DO NOTHING");
                        }
                    }
                    else { Debug.LogWarning("No records in dictOfActorConflicts"); }
                }
                else { Debug.LogWarning("Invalid dictOFActorConflicts (Null)"); }
            }
            else
            {
                //trait actorConflictNone (Team Player)
                TraitLogMessage(actor, "for a Relationship Conflict check", "to AVOID any type of Conflict");
                builder.AppendFormat("{0}{1} has {2}{3}{4}{5} trait{6}", colourNormal, actor.arc.name, colourNeutral, actor.GetTrait().tag, colourEnd, colourNormal, colourEnd);
                builder.AppendFormat("{0}{1}Nothing happens{2}", "\n", colourGood, colourEnd);
                //message
                string msgText = string.Format("{0} Relationship Conflict (Nothing Happens)", actor.arc.name, colourGood, colourEnd);
                GameManager.instance.messageScript.ActorConflict(msgText, actor, null, "Do Nothing because they are a TEAM PLAYER");
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
                        TraitLogMessage(actorKiller, "for a Relationship Conflict outcome", "to kill a fellow Subordinate");
                    }
                    else
                    { outputMsg = string.Format("{0}{1} killed by {2}{3}", colourBad, actorVictim.arc.name, actorKiller.arc.name, colourEnd); }
                    //victim message
                    string msgText = string.Format("{0} has been killed by {1}", actorVictim.arc.name, actorKiller.arc.name);
                    GameManager.instance.messageScript.ActorStatus(msgText, "Killed", string.Format("has been killed by {0}", actorKiller.arc.name), actorVictim.actorID, side);
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

    //
    // - - - Debug - - -
    //


    /// <summary>
    /// Debug method to display actor pools (both sides)
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayPools()
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
            subBuilder.AppendFormat(" {0}, ID {1}, {2}, L{3}, {4}-{5}-{6}{7}", actor.actorName, actor.actorID, actor.arc.name, actor.level,
                actor.GetDatapoint(ActorDatapoint.Datapoint0), actor.GetDatapoint(ActorDatapoint.Datapoint1), actor.GetDatapoint(ActorDatapoint.Datapoint2), "\n");
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
        Debug.Assert(string.IsNullOrEmpty(what) == false && string.IsNullOrEmpty(who) == false, "Invalid input parameters (Who or What are Null or empty");
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
    /// Debug method to remove a condition to an actor (debug input). Both sides.
    /// </summary>
    /// <param name="what"></param>
    /// <param name="who"></param>
    /// <returns></returns>
    public string DebugRemoveCondition(string what, string who)
    {
        Debug.Assert(string.IsNullOrEmpty(what) == false && string.IsNullOrEmpty(who) == false, "Invalid input parameters (Who or What are Null or empty");
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
                    text = DebugRemoveConditionToActor(Convert.ToInt32(who), condition);
                    break;
                case "p":
                case "P":
                    text = DebugRemoveConditionToPlayer(condition);
                    break;
            }
        }
        else { text = "Input Condition is INVALID and is NOT removed"; }
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
        if (GameManager.instance.playerScript.CheckConditionPresent(condition, side) == false)
        {
            //add condition
            GameManager.instance.playerScript.AddCondition(condition, side, "Debug action");
            text = string.Format("Condition {0} added to Player", condition.tag);
        }
        else { text = string.Format("Player already has Condition {0}", condition.tag); }
        return string.Format("{0}{1}Press ESC to Exit", text, "\n");
    }

    /// <summary>
    /// subMethod for DebugRemoveCondition to remove a condition to the player. Returns a string indicating success, or otherwise
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    private string DebugRemoveConditionToPlayer(Condition condition)
    {
        Debug.Assert(condition != null, "Invalid Condition (Null)");
        string text = "Unknown";
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        //does actor already have the condition?
        if (GameManager.instance.playerScript.CheckConditionPresent(condition, side) == true)
        {
            //remove condition
            GameManager.instance.playerScript.RemoveCondition(condition, side, "Debug action");
            text = string.Format("Condition {0} removed from Player", condition.tag);
        }
        else { text = string.Format("Player DOES NOT already have Condition {0}", condition.tag); }
        return string.Format("{0}{1}Press ESC to Exit", text, "\n");
    }

    /// <summary>
    /// subMethod for DebugAddCondition to Remove a condition to an actor and return a string indicating success, or otherwise
    /// </summary>
    /// <param name="slotID"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    private string DebugRemoveConditionToActor(int actorSlotID, Condition condition)
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
                if (actor.CheckConditionPresent(condition) == true)
                {
                    //remove condition
                    if (actor.RemoveCondition(condition, "Debug Action") == true)
                    { text = string.Format("Condition {0} removed from {1}, {2}", condition.tag, actor.arc.name, actor.actorName); }
                    else { text = string.Format("Condition {0} NOT removed", condition.tag); }
                }
                else { text = string.Format("{0} DOES NOT already have Condition {1}", actor.arc.name, condition.tag); }
            }
            else { text = string.Format("There is no valid Actor (Null) in Slot {0}", actorSlotID); }
        }
        else { text = string.Format("There is no Actor Present in Slot {0}", actorSlotID); }
        return string.Format("{0}{1}Press ESC to Exit", text, "\n");
    }

    /// <summary>
    /// subMethod for DebugRemoveCondition to remove a condition to an actor and return a string indicating success, or otherwise
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
                    if (actor.AddCondition(condition, "Debug Action") == true)
                    { text = string.Format("Condition {0} added to {1}, {2}", condition.tag, actor.arc.name, actor.actorName); }
                    else { text = string.Format("Condition {0} NOT added", condition.tag); }
                }
                else { text = string.Format("{0} already has Condition {1}", actor.arc.name, condition.tag); }
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
        Debug.Assert(string.IsNullOrEmpty(what) == false && string.IsNullOrEmpty(who) == false, "Invalid input parameters (Who or What are Null or empty");
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
                        //redo personality as trait may have changed things
                        actor.GetPersonality().SetFactors(GameManager.instance.personScript.SetPersonalityFactors(trait.GetArrayOfCriteria()));
                        GameManager.instance.personScript.SetIndividualPersonality(actor.GetPersonality());
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
    /// Provides a random motivational shift (+1/-1), 50% chance, for the actor for testing purposes
    /// NOTE: actor checked for Null by calling method
    /// </summary>
    /// <param name="actor"></param>
    private void DebugRandomMotivationShift(Actor actor)
    {
        //50% chance of happening
        if (Random.Range(0, 100) < 50)
        {
            int shift = 0;
            int motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
            switch (motivation)
            {
                case 3: shift = -1; break;
                case 2:
                case 1:
                    shift = 1;
                    if (Random.Range(0, 100) < 50) { shift = -1; }
                    break;
                case 0: shift = 1; break;
                default: Debug.LogErrorFormat("Unrecognised motivation \"{0}\"", motivation); break;
            }
            motivation += shift;
            actor.SetDatapoint(ActorDatapoint.Motivation1, motivation, "Debug Purposes");
        }
    }

    /// <summary>
    /// creates false Resistance node action data for testing purposes, after AutoRun, based on current actor line up (so more accurate that AI data). NumOfRecords is how many records per actor will be generated
    /// </summary>
    public void DebugCreateNodeActionResistanceData(int numOfActionsPerActor = 4)
    {
        int turn;
        //loop OnMap actors
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
        NameSet nameSet = GameManager.instance.cityScript.GetNameSet();
        if (nameSet != null)
        {
            if (arrayOfActors != null)
            {
                for (int i = 0; i < arrayOfActors.Length; i++)
                {
                    //check actor is present in slot (not vacant)
                    if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                    {
                        Actor actor = arrayOfActors[i];
                        if (actor != null)
                        {
                            NodeAction nodeAction = NodeAction.None;
                            //Get appropriate NodeAction enum
                            switch (actor.arc.name)
                            {
                                case "ANARCHIST": nodeAction = NodeAction.ActorBlowStuffUp; break;
                                case "BLOGGER": nodeAction = NodeAction.ActorSpreadFakeNews; break;
                                case "FIXER": nodeAction = NodeAction.ActorObtainGear; break;
                                case "HACKER": nodeAction = NodeAction.ActorHackSecurity; break;
                                case "HEAVY": nodeAction = NodeAction.ActorCreateRiots; break;
                                case "OBSERVER": nodeAction = NodeAction.ActorInsertTracer; break;
                                case "OPERATOR": nodeAction = NodeAction.ActorNeutraliseTeam; break;
                                case "PLANNER": nodeAction = NodeAction.ActorGainTargetInfo; break;
                                case "RECRUITER": nodeAction = NodeAction.ActorRecruitActor; break;
                                default: Debug.LogWarningFormat("Unrecognised actor.arc \"{0}\"", actor.arc.name); break;
                            }
                            if (nodeAction != NodeAction.None)
                            {
                                //test purposes only 
                                if (nodeAction == NodeAction.ActorRecruitActor)
                                {
                                    //loop for every required nodeAction
                                    for (int j = 0; j < numOfActionsPerActor; j++)
                                    {
                                        //get random node
                                        Node node = GameManager.instance.dataScript.GetRandomNode();
                                        if (node != null)
                                        {
                                            NodeActionData data = new NodeActionData();
                                            turn = GameManager.instance.turnScript.Turn;
                                            if (turn > 0)
                                            {
                                                //go back a set number of turns, minCap 0
                                                turn -= j;
                                                turn = Mathf.Max(0, turn);
                                            }
                                            data.turn = turn;
                                            data.actorID = actor.actorID;
                                            data.nodeID = node.nodeID;
                                            data.nodeAction = nodeAction;
                                            //special cases
                                            switch (actor.arc.name)
                                            {
                                                case "RECRUITER":
                                                    //need a name for an imaginery recruited actor
                                                    if (Random.Range(0, 100) < 50)
                                                    {
                                                        //Male
                                                        data.dataName = string.Format("{0} {1}", nameSet.firstMaleNames.GetRandomRecord(), nameSet.lastNames.GetRandomRecord());
                                                    }
                                                    else
                                                    { data.dataName = string.Format("{0} {1}", nameSet.firstFemaleNames.GetRandomRecord(), nameSet.lastNames.GetRandomRecord()); }
                                                    break;
                                                case "FIXER":
                                                    //need a name for imaginery gear
                                                    data.dataName = GameManager.instance.dataScript.DebugGetRandomGearName();
                                                    break;
                                                case "PLANNER":
                                                    //get random target name
                                                    List<Target> listOfTargets = GameManager.instance.dataScript.GetTargetPool(Status.Live);
                                                    if (listOfTargets != null)
                                                    { data.dataName = listOfTargets[Random.Range(0, listOfTargets.Count)].targetName; }
                                                    break;
                                                case "OPERATOR":
                                                    //get random team name
                                                    data.dataName = string.Format("{0} {1}", GameManager.instance.dataScript.DebugGetRandomTeamArc(), "Team Alpha");
                                                    break;
                                                case "ANARCHIST":
                                                    //get random Location name
                                                    data.dataName = GameManager.instance.topicScript.textlistGenericLocation.GetRandomRecord();
                                                    break;
                                            }
                                            actor.AddNodeAction(data);
                                        }
                                        else { Debug.LogWarningFormat("Invalid random node (Null)"); }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid arrayOfActors (Null)"); }
        }
        else { Debug.LogError("Invalid City nameSet (Null)"); }
    }


    //
    // - - - Inactive Actors - - -
    //


    /// <summary>
    /// Checks all OnMap Inactive Resistance actors, increments invisibility and returns any at max value back to Active status
    /// </summary>
    private void CheckInactiveResistanceActorsHuman()
    {
        string gearName;
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
                        switch (actor.Status)
                        {
                            case ActorStatus.Inactive:
                                string text = "Unknown";
                                switch (actor.inactiveStatus)
                                {
                                    case ActorInactive.LieLow:
                                        int invis = actor.GetDatapoint(ActorDatapoint.Invisibility2);
                                        //increment invisibility (not the first turn)
                                        if (actor.isLieLowFirstturn == false)
                                        { invis++; }
                                        else { actor.isLieLowFirstturn = false; }
                                        if (invis >= maxStatValue)
                                        {
                                            //actor has recovered from lying low, needs to be activated
                                            actor.SetDatapoint(ActorDatapoint.Invisibility2, Mathf.Min(maxStatValue, invis));
                                            actor.Status = ActorStatus.Active;
                                            actor.inactiveStatus = ActorInactive.None;
                                            actor.tooltipStatus = ActorTooltip.None;
                                            GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, GameManager.instance.guiScript.alphaActive);
                                            //message -> status change
                                            text = string.Format("{0} {1} has automatically reactivated", actor.arc.name, actor.actorName);
                                            GameManager.instance.messageScript.ActorStatus(text, "is now Active", "has finished Lying Low", actor.actorID, globalResistance);
                                            Debug.LogFormat("[Ply] ActorManager.cs -> CheckInactiveResistanceActorsHuman: {0}, {1}, id {2} is no longer LYING LOW{3}", actor.actorName,
                                                actor.arc.name, actor.actorID, "\n");
                                            //check if actor has stressed condition
                                            if (actor.CheckConditionPresent(conditionStressed) == true)
                                            { actor.RemoveCondition(conditionStressed, "Lying Low removes Stress"); }

                                            /*//update contacts
                                            GameManager.instance.contactScript.UpdateNodeContacts();*/
                                        }
                                        else
                                        {
                                            actor.SetDatapoint(ActorDatapoint.Invisibility2, invis);
                                            GameManager.instance.dataScript.StatisticIncrement(StatType.LieLowDaysTotal);
                                            actor.numOfDaysLieLow++;
                                        }
                                        break;
                                    case ActorInactive.Breakdown:
                                        //restore actor (one stress turn only)
                                        actor.Status = ActorStatus.Active;
                                        actor.inactiveStatus = ActorInactive.None;
                                        actor.tooltipStatus = ActorTooltip.None;
                                        GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, GameManager.instance.guiScript.alphaActive);
                                        text = string.Format("{0}, {1}, has recovered from their Breakdown", actor.arc.name, actor.actorName);
                                        GameManager.instance.messageScript.ActorStatus(text, "has Recovered", "has recovered from their Breakdown", actor.actorID, globalResistance);
                                        break;
                                    case ActorInactive.StressLeave:
                                        if (actor.isStressLeave == false)
                                        {
                                            //restore actor (one Stress Leave turn only)
                                            actor.Status = ActorStatus.Active;
                                            actor.inactiveStatus = ActorInactive.None;
                                            actor.tooltipStatus = ActorTooltip.None;
                                            GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, GameManager.instance.guiScript.alphaActive);
                                            text = string.Format("{0}, {1}, has returned from their Stress Leave", actor.actorName, actor.arc.name);
                                            GameManager.instance.messageScript.ActorStatus(text, "has Returned", "has returned from their Stress Leave", actor.actorID, globalResistance);
                                            actor.RemoveCondition(conditionStressed, "due to Stress Leave");
                                            actor.numOfTimesStressLeave++;
                                        }
                                        else { actor.isStressLeave = false; }
                                        break;
                                }
                                break;
                            case ActorStatus.Captured:
                                actor.captureTimer--;
                                if (actor.captureTimer <= 0)
                                { GameManager.instance.captureScript.ReleaseActor(actor); }
                                break;
                        }
                        //
                        // - - - Inactive Info App - - -
                        //
                        if (actor.inactiveStatus != ActorInactive.None)
                        {
                            string text = string.Format("{0}, {1}, contact network unavailable", actor.actorName, actor.arc.name);
                            string topText = "Contact Network";
                            string detailsTop = string.Format("{0}{1} is currently unavailable</b>{2}", colourAlert, actor.actorName, colourEnd);
                            string detailsBottom = string.Format("<b>Their network of contacts will become available once they are {0}back in touch</b>{1}", colourNeutral, colourEnd);
                            GameManager.instance.messageScript.ActiveEffect(text, topText, detailsTop, detailsBottom, actor.sprite, actor.actorID);
                        }
                        //
                        // - - - Gear - - -
                        //
                        gearName = actor.GetGearName();
                        if (string.IsNullOrEmpty(gearName) == false)
                        {
                            if (isGearCheckRequired == true)
                            {
                                Gear gear = GameManager.instance.dataScript.GetGear(gearName);
                                if (gear != null)
                                {
                                    if (gear.isCompromised == true)
                                    {
                                        //gear automatically lost after Target use (other uses are taken care elsewhere)
                                        actor.RemoveGear(GearRemoved.Compromised);
                                        //message
                                        string msgText = string.Format("{0} gear Compromised (Target attempt) by {1}, {2}", gear.tag, actor.actorName, actor.arc.name);
                                        GameManager.instance.messageScript.GearLost(msgText, gear, actor);
                                    }
                                    else { actor.ResetGearItem(gear); }

                                }
                                else { Debug.LogErrorFormat("Invalid gear (Null) for gear {0}", gearName); }
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
    /// Checks all OnMap Inactive AI Resistance actors, increments invisibility and returns any at max value back to Active status
    /// </summary>
    private void CheckInactiveResistanceActorsAI(bool isPlayer)
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

                        /*if (actor.Status == ActorStatus.Inactive)*/
                        switch (actor.Status)
                        {
                            case ActorStatus.Inactive:
                                switch (actor.inactiveStatus)
                                {
                                    case ActorInactive.LieLow:
                                        int invis = actor.GetDatapoint(ActorDatapoint.Invisibility2);
                                        //increment invisibility (not the first turn)
                                        if (actor.isLieLowFirstturn == false)
                                        { invis++; }
                                        else { actor.isLieLowFirstturn = false; }
                                        if (invis >= maxStatValue)
                                        {
                                            //actor has recovered from lying low, needs to be activated
                                            invis = Mathf.Min(maxStatValue, invis);
                                            actor.SetDatapoint(ActorDatapoint.Invisibility2, invis);
                                            actor.Status = ActorStatus.Active;
                                            actor.inactiveStatus = ActorInactive.None;
                                            actor.tooltipStatus = ActorTooltip.None;
                                            if (isPlayer == true)
                                            {
                                                //message -> status change
                                                string text = string.Format("{0} {1} has automatically reactivated", actor.arc.name, actor.actorName);
                                                GameManager.instance.messageScript.ActorStatus(text, "is now Active", "has finished Lying Low", actor.actorID, globalResistance);
                                            }
                                            Debug.LogFormat("[Ply] ActorManager.cs -> CheckInactiveResistanceActorsAI: {0}, {1}, id {2} is no longer LYING LOW{3}", actor.actorName,
                                                actor.arc.name, actor.actorID, "\n");
                                            //check if actor has stressed condition
                                            if (actor.CheckConditionPresent(conditionStressed) == true)
                                            { actor.RemoveCondition(conditionStressed, "Lying Low removes Stress"); }
                                            GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, GameManager.instance.guiScript.alphaActive);
                                        }
                                        else
                                        {
                                            actor.SetDatapoint(ActorDatapoint.Invisibility2, invis);
                                            //stats
                                            GameManager.instance.dataScript.StatisticIncrement(StatType.LieLowDaysTotal);
                                            actor.numOfDaysLieLow++;
                                        }
                                        break;
                                    case ActorInactive.Breakdown:
                                        //restore actor (one stress turn only)
                                        actor.Status = ActorStatus.Active;
                                        actor.inactiveStatus = ActorInactive.None;
                                        actor.tooltipStatus = ActorTooltip.None;
                                        if (isPlayer == true)
                                        {
                                            string textBreakdown = string.Format("{0}, {1}, has recovered from their Breakdown", actor.arc.name, actor.actorName);
                                            GameManager.instance.messageScript.ActorStatus(textBreakdown, "has Recovered", "has recovered from their Breakdown", actor.actorID, globalResistance);
                                        }
                                        Debug.LogFormat("[Rim] ActorManager.cs -> CheckInactiveResistanceActorsAI: {0}, {1}, id {2} has RECOVERED from their Breakdown{3}", actor.actorName,
                                            actor.arc.name, actor.actorID, "\n");
                                        GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, GameManager.instance.guiScript.alphaActive);
                                        break;
                                }
                                break;
                            case ActorStatus.Captured:
                                actor.captureTimer--;
                                if (actor.captureTimer <= 0)
                                { GameManager.instance.captureScript.ReleaseActor(actor); }
                                break;
                        }
                        //
                        // - - - Lie Low Info App - - -
                        //
                        if (actor.inactiveStatus == ActorInactive.LieLow)
                        {
                            if (isPlayer == true)
                            {
                                string text = string.Format("{0}, {1}, is LYING LOW", actor.actorName, actor.arc.name);
                                string topText = "Out of Contact";
                                string detailsTop = string.Format("{0}{1} is deliberately keeping a <b>Low Profile</b>{2}", colourAlert, actor.actorName, colourEnd);
                                string detailsBottom = string.Format("{0}<b>{1} can't take actions or access their connections</b>{2}", colourBad, actor.arc.name, colourEnd);
                                GameManager.instance.messageScript.ActiveEffect(text, topText, detailsTop, detailsBottom, actor.sprite, actor.actorID);
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
    private void CheckActiveResistanceActorsHuman()
    {
        string gearName;
        int numOfTraitors = 0;
        string text, topText, detailsTop, detailsBottom;
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
                List<Condition> listOfBadConditions = GameManager.instance.playerScript.GetNumOfBadConditionPresent(globalResistance);
                if (listOfBadConditions.Count > 0)
                { SetReputationWarningMessage(listOfBadConditions); }
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
                            //Checks all actors -> can inform on Player regardless of their status
                            if (actor.isTraitor == true)
                            { numOfTraitors++; }
                            //Active actors only
                            if (actor.Status == ActorStatus.Active)
                            {
                                //
                                // - - - Stress Condition - - -
                                //
                                if (actor.CheckConditionPresent(conditionStressed) == true)
                                { ProcessStress(actor, chanceBreakdown, true); }
                                //
                                // - - - Learn Secrets - - -
                                //
                                if (isSecrets == true)
                                { ProcessSecrets(actor, listOfSecrets, chanceSecret, true); }
                                //
                                // - - - Blackmailing - - -
                                //
                                if (actor.blackmailTimer > 0)
                                {
                                    ProcessBlackmail(actor);

                                    /*string blackmailOutcome = ProcessBlackmail(actor);
                                    if (string.IsNullOrEmpty(blackmailOutcome) == false)
                                    { }*/
                                }
                                //
                                // - - - Compatibility (Resign) - - -
                                //
                                if (listOfBadConditions.Count > 0)
                                { ProcessCompatibility(actor, listOfBadConditions); }
                                //
                                // - - - Invisibility Zero warning - - -
                                //
                                if (actor.GetDatapoint(ActorDatapoint.Invisibility2) == 0)
                                { ProcessInvisibilityWarning(actor); }
                                //
                                // - - - Motivation Warning - - -
                                //
                                if (actor.GetDatapoint(ActorDatapoint.Motivation1) == 0)
                                { ProcessMotivationWarning(actor); }
                                //
                                // - - - Info App conditions (any)
                                //
                                if (actor.CheckNumOfConditions() > 0)
                                {
                                    List<Condition> listOfConditions = actor.GetListOfConditions();
                                    foreach (Condition condition in listOfConditions)
                                    {
                                        if (condition != null)
                                        {
                                            text = string.Format("{0} has the {1} condition", actor.arc.name, condition.tag);
                                            topText = "Condition present";
                                            switch (condition.type.level)
                                            {
                                                case 0: detailsTop = string.Format("{0}{1} {2}{3}", colourAlert, actor.actorName, condition.topText, colourEnd); break;
                                                case 1: detailsTop = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, condition.topText, colourEnd); break;
                                                case 2: detailsTop = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, condition.topText, colourEnd); break;
                                                default: detailsTop = "Unknown"; break;
                                            }
                                            switch (condition.bottomTextTypeActor.level)
                                            {
                                                case 0: detailsBottom = string.Format("{0}<b>{1}</b>{2}", colourBad, condition.bottomTextActor, colourEnd); break;
                                                case 1: detailsBottom = string.Format("{0}{1}{2}", colourNeutral, condition.bottomTextActor, colourEnd); break;
                                                case 2: detailsBottom = string.Format("{0}{1}{2}", colourGood, condition.bottomTextActor, colourEnd); break;
                                                default: detailsBottom = "Unknown"; break;
                                            }
                                            GameManager.instance.messageScript.ActiveEffect(text, topText, detailsTop, detailsBottom, actor.sprite, actor.actorID, null, condition);
                                        }
                                        else { Debug.LogWarningFormat("Invalid condition (Null) for {0}, {1}, ID {2}", actor.actorName, actor.arc.name, actor.actorID); }
                                    }
                                }
                                //
                                // - - - Gear - - -
                                //
                                gearName = actor.GetGearName();
                                if (string.IsNullOrEmpty(gearName) == false)
                                {
                                    if (isGearCheckRequired == true)
                                    {
                                        Gear gear = GameManager.instance.dataScript.GetGear(gearName);
                                        if (gear != null)
                                        {
                                            if (gear.isCompromised == true)
                                            {
                                                //gear automatically lost after Target use (other uses are taken care elsewhere)
                                                actor.RemoveGear(GearRemoved.Compromised);
                                                //message
                                                string msgText = string.Format("{0} gear Compromised (Target attempt) by {1}, {2}", gear.tag, actor.actorName, actor.arc.name);
                                                GameManager.instance.messageScript.GearLost(msgText, gear, actor);
                                            }
                                            else { actor.ResetGearItem(gear); }

                                        }
                                        else { Debug.LogErrorFormat("Invalid gear (Null) for gearID {0}", gearName); }
                                    }
                                }
                            }
                        }
                        else { Debug.LogError(string.Format("Invalid Resistance actor (Null), index {0}", i)); }
                    }
                }
                //reset gear check flag as all active and inactive resistance actors have had their gear checked by now
                isGearCheckRequired = false;
                //Check for a betrayal
                CheckForBetrayal(numOfTraitors);
            }
            else { Debug.LogError("Invalid arrayOfActors (Resistance) (Null)"); }
        }
    }




    /// <summary>
    /// Checks all active resistance AI actors (run AFTER checkInactiveResistanceActors). No checks are made if Player is not Active
    /// </summary>
    private void CheckActiveResistanceActorsAI(bool isPlayer)
    {
        int numOfTraitors = 0;
        string text, topText, detailsTop, detailsBottom;
        bool isProceed = true;
        bool isSecrets = false;
        //Player active?
        if (isPlayer == true)
        {
            //human resistance player (after autorun)
            if (GameManager.instance.playerScript.status != ActorStatus.Active)
            { isProceed = false; }
        }
        else
        {
            //ai resistance player
            if (GameManager.instance.aiRebelScript.status != ActorStatus.Active)
            { isProceed = false; }
        }
        //no checks are made if player is not Active
        if (isProceed == true)
        {
            Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
            if (arrayOfActors != null)
            {
                //secrets only if reverts to human player
                if (isPlayer == true)
                { if (GameManager.instance.playerScript.CheckNumOfSecrets() > 0) { isSecrets = true; } }
                int chanceBreakdown = breakdownChance;
                //base chance of nervous breakdown doubled during a surveillance crackdown
                if (GameManager.instance.turnScript.authoritySecurityState == AuthoritySecurityState.SurveillanceCrackdown)
                { chanceBreakdown *= 2; }
                //secrets
                int chanceSecret = secretBaseChance;
                List<Secret> listOfSecrets = GameManager.instance.playerScript.GetListOfSecrets();
                //compatibility (actors with player)
                List<Condition> listOfBadConditions = GameManager.instance.playerScript.GetNumOfBadConditionPresent(globalResistance);
                if (listOfBadConditions.Count > 0 && isPlayer == true)
                { SetReputationWarningMessage(listOfBadConditions); }
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
                            /*//DEBUG
                            DebugRandomMotivationShift(actor);*/

                            //Checks all actors -> can inform on Player regardless of their status
                            if (actor.isTraitor == true)
                            { numOfTraitors++; }
                            if (actor.Status == ActorStatus.Active)
                            {
                                //
                                // - - - Compatibility - - -
                                //
                                if (listOfBadConditions.Count > 0)
                                { ProcessCompatibility(actor, listOfBadConditions, globalResistance); }
                                //
                                // - - - Stress Condition - - -
                                //
                                if (actor.CheckConditionPresent(conditionStressed) == true)
                                { ProcessStress(actor, chanceBreakdown, isPlayer); }
                                //
                                // - - - Learn Secrets - - -
                                //
                                if (isPlayer == true)
                                {
                                    if (isSecrets == true)
                                    { ProcessSecrets(actor, listOfSecrets, chanceSecret, isPlayer); }
                                }
                                //
                                // - - - Info App conditions (any)
                                //
                                if (actor.CheckNumOfConditions() > 0)
                                {
                                    if (isPlayer == true)
                                    {
                                        List<Condition> listOfConditions = actor.GetListOfConditions();
                                        foreach (Condition condition in listOfConditions)
                                        {
                                            if (condition != null)
                                            {
                                                text = string.Format("{0}, {1} has the {2} condition", actor.actorName, actor.arc.name, condition.tag);
                                                topText = "Condition present";
                                                switch (condition.type.level)
                                                {
                                                    case 0: detailsTop = string.Format("{0}{1} {2}{3}", colourAlert, actor.actorName, condition.topText, colourEnd); break;
                                                    case 1: detailsTop = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, condition.topText, colourEnd); break;
                                                    case 2: detailsTop = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, condition.topText, colourEnd); break;
                                                    default: detailsTop = "Unknown"; break;
                                                }
                                                switch (condition.bottomTextTypeActor.level)
                                                {
                                                    case 0: detailsBottom = string.Format("{0}<b>{1}</b>{2}", colourBad, condition.bottomTextActor, colourEnd); break;
                                                    case 1: detailsBottom = string.Format("{0}{1}{2}", colourNeutral, condition.bottomTextActor, colourEnd); break;
                                                    case 2: detailsBottom = string.Format("{0}{1}{2}", colourGood, condition.bottomTextActor, colourEnd); break;
                                                    default: detailsBottom = "Unknown"; break;
                                                }
                                                GameManager.instance.messageScript.ActiveEffect(text, topText, detailsTop, detailsBottom, actor.sprite, actor.actorID);
                                            }
                                            else { Debug.LogWarningFormat("Invalid condition (Null) for {0}, {1}, ID {2}", actor.actorName, actor.arc.name, actor.actorID); }
                                        }
                                    }
                                }
                            }
                        }
                        else { Debug.LogError(string.Format("Invalid Resistance actor (Null), index {0}", i)); }
                    }
                }
                //Check for a betrayal
                CheckForBetrayal(numOfTraitors);
            }
            else { Debug.LogError("Invalid arrayOfActors (Resistance) (Null)"); }
        }
    }

    /// <summary>
    /// Checks all active human player authority actors (run AFTER checkInactiveAuthorityActors). No checks are made if the player is not Active
    /// </summary>
    private void CheckActiveAuthorityActorsHuman()
    {
        string text, topText, detailsTop, detailsBottom;
        //no checks are made if player is not Active
        if (GameManager.instance.playerScript.status == ActorStatus.Active)
        {
            Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalAuthority);
            if (arrayOfActors != null)
            {
                //secrets
                int chanceSecret = secretBaseChance;
                List<Secret> listOfSecrets = GameManager.instance.playerScript.GetListOfSecrets();
                bool isSecrets = false;
                if (GameManager.instance.playerScript.CheckNumOfSecrets() > 0) { isSecrets = true; }
                //compatibility (actors with player)
                List<Condition> listOfBadConditions = GameManager.instance.playerScript.GetNumOfBadConditionPresent(globalAuthority);
                if (listOfBadConditions.Count > 0)
                { SetReputationWarningMessage(listOfBadConditions); }
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
                                { ProcessStress(actor, breakdownChance, true); }
                                //
                                // - - - Learn Secrets - - -
                                //
                                if (isSecrets == true)
                                { ProcessSecrets(actor, listOfSecrets, chanceSecret, true); }
                                //
                                // - - - Blackmailing - - -
                                //
                                if (actor.blackmailTimer > 0)
                                {
                                    ProcessBlackmail(actor);

                                    /*string blackmailOutcome = ProcessBlackmail(actor);
                                    if (string.IsNullOrEmpty(blackmailOutcome) == false)
                                    { }*/
                                }
                                //
                                // - - - Compatibility - - -
                                //
                                if (listOfBadConditions.Count > 0)
                                { ProcessCompatibility(actor, listOfBadConditions); }
                                //
                                // - - - Motivation Warning - - -
                                //
                                if (actor.GetDatapoint(ActorDatapoint.Motivation1) == 0)
                                { ProcessMotivationWarning(actor); }
                                //
                                // - - - Info App conditions (any)
                                //
                                if (actor.CheckNumOfConditions() > 0)
                                {
                                    List<Condition> listOfConditions = actor.GetListOfConditions();
                                    foreach (Condition condition in listOfConditions)
                                    {
                                        if (condition != null)
                                        {
                                            text = string.Format("{0}, {1} has the {2} condition", actor.actorName, actor.arc.name, condition.tag);
                                            topText = "Condition present";
                                            switch (condition.type.level)
                                            {
                                                case 0: detailsTop = string.Format("{0}{1} {2}{3}", colourAlert, actor.actorName, condition.topText, colourEnd); break;
                                                case 1: detailsTop = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, condition.topText, colourEnd); break;
                                                case 2: detailsTop = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, condition.topText, colourEnd); break;
                                                default: detailsTop = "Unknown"; break;
                                            }
                                            switch (condition.bottomTextTypeActor.level)
                                            {
                                                case 0: detailsBottom = string.Format("{0}<b>{1}</b>{2}", colourBad, condition.bottomTextActor, colourEnd); break;
                                                case 1: detailsBottom = string.Format("{0}{1}{2}", colourNeutral, condition.bottomTextActor, colourEnd); break;
                                                case 2: detailsBottom = string.Format("{0}{1}{2}", colourGood, condition.bottomTextActor, colourEnd); break;
                                                default: detailsBottom = "Unknown"; break;
                                            }
                                            GameManager.instance.messageScript.ActiveEffect(text, topText, detailsTop, detailsBottom, actor.sprite, actor.actorID, null, condition);
                                        }
                                        else { Debug.LogWarningFormat("Invalid condition (Null) for {0}, {1}, ID {2}", actor.actorName, actor.arc.name, actor.actorID); }
                                    }
                                }
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
    /// Checks all active Authority AI actors (run AFTER checkInactiveAuthorityActors). No checks are made if Player is not Active
    /// </summary>
    private void CheckActiveAuthorityActorsAI(bool isPlayer)
    {
        string text, topText, detailsTop, detailsBottom;
        bool isSecrets = false;
        bool isProceed = true;
        //Player active?
        if (isPlayer == true)
        {
            //human authority player (after autorun)
            if (GameManager.instance.playerScript.status != ActorStatus.Active)
            { isProceed = false; }
        }
        else
        {
            //ai authority player
            if (GameManager.instance.aiScript.status != ActorStatus.Active)
            { isProceed = false; }
        }
        //no checks are made if player is not Active
        if (isProceed == true)
        {
            Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalAuthority);
            if (arrayOfActors != null)
            {
                //secrets only if reverts to human control
                if (isPlayer == true)
                { if (GameManager.instance.playerScript.CheckNumOfSecrets() > 0) { isSecrets = true; } }
                //secrets
                int chanceSecret = secretBaseChance;
                List<Secret> listOfSecrets = GameManager.instance.playerScript.GetListOfSecrets();
                //compatibility (actors with player)
                List<Condition> listOfBadConditions = GameManager.instance.playerScript.GetNumOfBadConditionPresent(globalAuthority);
                if (listOfBadConditions.Count > 0 && isPlayer == true)
                { SetReputationWarningMessage(listOfBadConditions); }
                //
                // - - - loop Active actors - - -
                //
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
                                // - - - Compatibility - - -
                                //
                                if (listOfBadConditions.Count > 0)
                                { ProcessCompatibility(actor, listOfBadConditions, globalAuthority); }
                                //
                                // - - - Stress Condition - - -
                                //
                                if (actor.CheckConditionPresent(conditionStressed) == true)
                                { ProcessStress(actor, breakdownChance, isPlayer); }
                                //
                                // - - - Learn Secrets - - -
                                //
                                if (isPlayer == true)
                                {
                                    if (isSecrets == true)
                                    { ProcessSecrets(actor, listOfSecrets, chanceSecret, isPlayer); }
                                }
                                //
                                // - - - Info App conditions (any)
                                //
                                if (actor.CheckNumOfConditions() > 0)
                                {
                                    if (isPlayer == true)
                                    {
                                        List<Condition> listOfConditions = actor.GetListOfConditions();
                                        foreach (Condition condition in listOfConditions)
                                        {
                                            if (condition != null)
                                            {
                                                text = string.Format("{0}, {1} has the {2} condition", actor.actorName, actor.arc.name, condition.tag);
                                                topText = "Condition present";
                                                switch (condition.type.level)
                                                {
                                                    case 0: detailsTop = string.Format("{0}{1} {2}{3}", colourAlert, actor.actorName, condition.topText, colourEnd); break;
                                                    case 1: detailsTop = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, condition.topText, colourEnd); break;
                                                    case 2: detailsTop = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, condition.topText, colourEnd); break;
                                                    default: detailsTop = "Unknown"; break;
                                                }
                                                switch (condition.bottomTextTypeActor.level)
                                                {
                                                    case 0: detailsBottom = string.Format("{0}<b>{1}</b>{2}", colourBad, condition.bottomTextActor, colourEnd); break;
                                                    case 1: detailsBottom = string.Format("{0}{1}{2}", colourNeutral, condition.bottomTextActor, colourEnd); break;
                                                    case 2: detailsBottom = string.Format("{0}{1}{2}", colourGood, condition.bottomTextActor, colourEnd); break;
                                                    default: detailsBottom = "Unknown"; break;
                                                }
                                                GameManager.instance.messageScript.ActiveEffect(text, topText, detailsTop, detailsBottom, actor.sprite, actor.actorID);
                                            }
                                            else { Debug.LogWarningFormat("Invalid condition (Null) for {0}, {1}, ID {2}", actor.actorName, actor.arc.name, actor.actorID); }
                                        }
                                    }
                                }
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
    /// subMtethod to handle a warning message. NOTE: it's assumed that the calling method has isPlayer set to true (for AI versions only)
    /// </summary>
    private void SetReputationWarningMessage(List<Condition> listOfBadConditions)
    {
        if (listOfBadConditions != null)
        {
            bool isQuestionable = false;
            StringBuilder builder = new StringBuilder();
            foreach (Condition condition in listOfBadConditions)
            {
                //check for questionable condition
                if (condition.tag.Equals(conditionQuestionable.name, StringComparison.Ordinal) == true)
                { isQuestionable = true; }
                if (builder.Length > 0) { builder.Append(", "); }
                builder.Append(condition.tag);
            }
            //warning message
            string msgText = string.Format("Your subordinates are considering resigning over your Reputation, {0} bad Conditions present", listOfBadConditions.Count);
            string itemText = "Your Reputation is poor";
            string reason = string.Format("{0}You are {1}<b>{2}</b>{3}{4}", "\n", colourBad, builder.ToString(), colourEnd, "\n");
            string warning = string.Format("{0}Your Subordinates may resign{1}", colourAlert, colourEnd);
            if (isQuestionable == true)
            { warning = string.Format("{0}Your Subordinates may resign{1}HQ APPROVAL may fall{2}", colourAlert, "\n", colourEnd); }
            GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Dubious Reputation", reason, warning);
        }
        else { Debug.LogError("Invalid listOfBadConditions (Null)"); }
    }


    /// <summary>
    /// Checks all OnMap Inactive AI Authority actors
    /// </summary>
    private void CheckInactiveAuthorityActorsAI(bool isPlayer)
    {
        //Authority actors only
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalAuthority);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalAuthority) == true)
                {
                    Actor actor = arrayOfActors[i];
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
                                    if (isPlayer == true)
                                    {
                                        string textBreakdown = string.Format("{0}, {1}, has recovered from their Breakdown", actor.arc.name, actor.actorName);
                                        GameManager.instance.messageScript.ActorStatus(textBreakdown, "has Recovered", "has recovered from their Breakdown", actor.actorID, globalAuthority);
                                    }
                                    GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, GameManager.instance.guiScript.alphaActive);
                                    Debug.LogFormat("[Rim] ActorManager.cs -> CheckInactiveAuthorityActorsAI: {0}, {1}, id {2} has RECOVERED from their Breakdown{3}", actor.actorName,
                                        actor.arc.name, actor.actorID, "\n");
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
    /// sub Method to check Stress condition each turn. Called by CheckActive(Resistance/Authority)Actors.
    /// NOTE: Actor has been checked for null by calling method
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    private void ProcessStress(Actor actor, int chance, bool isPlayer)
    {
        //stats
        actor.numOfDaysStressed++;
        //enforces a minimum one turn gap between successive breakdowns
        if (actor.isBreakdown == false)
        {
            //Trait Check
            if (actor.CheckTraitEffect(actorBreakdownChanceHigh) == true)
            {
                chance *= 2;
                if (isPlayer == true)
                { TraitLogMessage(actor, "for a Nervous Breakdown check", "to DOUBLE the chance of a breakdown"); }
            }
            else if (actor.CheckTraitEffect(actorBreakdownChanceLow) == true)
            {
                chance /= 2;
                if (isPlayer == true)
                { TraitLogMessage(actor, "for a Nervous Breakdown check", "to HALVE the chance of a breakdown"); }
            }
            else if (actor.CheckTraitEffect(actorBreakdownChanceNone) == true)
            {
                chance = 0;
                if (isPlayer == true)
                { TraitLogMessage(actor, "for a Nervous Breakdown check", "to PREVENT a breakdown"); }
            }
            //test
            int rnd = Random.Range(0, 100);
            if (rnd < chance)
            {
                //breakdown
                ActorBreakdown(actor, actor.side);
                Debug.LogFormat("[Rnd] ActorManager.cs -> CheckActiveActors: Stress check SUCCESS -> need < {0}, rolled {1}{2}",
                    chance, rnd, "\n");
                if (isPlayer == true)
                {
                    string text = string.Format("{0}, {1}, Stress check SUCCESS", actor.actorName, actor.arc.name);
                    GameManager.instance.messageScript.GeneralRandom(text, "Stress Breakdown", chance, rnd, true);
                }
                Debug.LogFormat("[Rim] ActorManager.cs -> ProcessStress: {0}, {1}, id {2} suffers STRESS BREAKDOWN{3}", actor.actorName, actor.arc.name, actor.actorID, "\n");
            }
            else
            {
                //passed test
                if (isPlayer == true)
                {
                    string text = string.Format("{0}, {1}, Stress check FAILED", actor.actorName, actor.arc.name);
                    GameManager.instance.messageScript.GeneralRandom(text, "Stress Breakdown", chance, rnd, true);
                }
            }
        }
        else { actor.isBreakdown = false; }
    }


    /// <summary>
    /// sub Method to check Blackmail condition each turn. Called by CheckActive(Resistance/Authority)Actors. 
    /// NOTE: Actor has been checked for null by calling method
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    private void ProcessBlackmail(Actor actor)
    {
        string msgText, reason;
        bool isResolved = false;
        //decrement timer
        actor.blackmailTimer--;
        if (actor.GetDatapoint(ActorDatapoint.Motivation1) == maxStatValue)
        {
            //trait Vindictive (won't be appeased)
            if (actor.CheckTraitEffect(actorAppeaseNone) == false)
            {
                //Motivation at max value, Blackmailer condition cancelled
                actor.RemoveCondition(conditionBlackmailer, string.Format("{0} has Maximum Motivation", actor.arc.name));
                isResolved = true;
                //message
                msgText = string.Format("{0} has full Motivation and has dropped their threat", actor.arc.name);
                reason = string.Format("{0} has regained MAXIMUM Motivation", actor.arc.name);
                GameManager.instance.messageScript.ActorBlackmail(msgText, actor, null, true, reason);
            }
            else
            { TraitLogMessage(actor, "for a Stop Blackmail check", "to REFUSE being bought-off"); }
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
                    secret.revealedID = actor.actorID;
                    secret.revealedWho = string.Format("{0}, {1}", actor.actorName, actor.arc.name);
                    secret.revealedWhen = GameManager.instance.turnScript.Turn;
                    secret.status = gameAPI.SecretStatus.Revealed;
                    /*StringBuilder builder = new StringBuilder();*/
                    //message
                    msgText = string.Format("{0} reveals your secret (\"{1}\")", actor.arc.name, secret.tag);
                    GameManager.instance.messageScript.ActorBlackmail(msgText, actor, secret);
                    //carry out effects
                    if (secret.listOfEffects != null)
                    {
                        //data packages

                        /*EffectDataReturn effectReturn = new EffectDataReturn();*/

                        EffectDataInput effectInput = new EffectDataInput();
                        effectInput.originText = "Reveal Secret";
                        effectInput.dataName = secret.org.name;
                        Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                        if (node != null)
                        {
                            //loop effects
                            foreach (Effect effect in secret.listOfEffects)
                            {
                                GameManager.instance.effectScript.ProcessEffect(effect, node, effectInput);

                                /*effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, effectInput);
                                if (builder.Length > 0) { builder.AppendLine(); }
                                builder.AppendFormat("{0}{1}{2}", effectReturn.topText, "\n", effectReturn.bottomText);*/
                            }
                        }
                        else { Debug.LogWarning("Invalid player node (Null)"); }

                    }
                    //message detailing effects
                    GameManager.instance.messageScript.ActorRevealSecret(msgText, actor, secret, "Carries out Blackmail threat");
                    //attempt executed, Blackmailer condition cancelled
                    actor.RemoveCondition(conditionBlackmailer, string.Format("{0} has carried out their Threat", actor.arc.name));
                    //remove secret from all actors and player
                    GameManager.instance.secretScript.RemoveSecretFromAll(secret.name);
                }
                else { Debug.LogWarning("Invalid Secret (Null) -> Not revealed"); }
            }
            else
            {
                //warning message
                msgText = string.Format("{0} is Blackmailing you and will reveal your secret in {1} turn{2}", actor.arc.name, actor.blackmailTimer,
                    actor.blackmailTimer != 1 ? "s" : "");
                string itemText = string.Format("{0} is Blackmailing you", actor.arc.name);
                reason = string.Format("This is a result of a conflict between you and {0}, {1}", actor.actorName, actor.arc.name);
                string warning = string.Format("<b>{0} will reveal your Secret in {1} turn{2}</b>", actor.arc.name, actor.blackmailTimer, actor.blackmailTimer != 1 ? "s" : "");
                GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Blackmail", reason, warning);
            }
        }
    }

    /// <summary>
    /// sub Method to check if actors learn of Player Secrets each turn. Called by CheckActive(Resistance/Authority)Actors.
    /// NOTE: Actor has been checked for null by calling method
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>  
    private void ProcessSecrets(Actor actor, List<Secret> listOfSecrets, int chance, bool isPlayer)
    {
        if (actor != null)
        {
            int rnd;
            bool isProceed;
            //actor already knows any secrets
            bool knowSecret = false;
            int actorSecrets = actor.CheckNumOfSecrets();
            if (actorSecrets > 0) { knowSecret = true; }
            //trait Detective
            if (actor.CheckTraitEffect(actorSecretChanceHigh) == true)
            {
                chance *= 3;
                //only do trait message if there is a secret to learn to avoid message spam (can't learn secret on first turn as player doesn't gain it until after in sequence)
                if (GameManager.instance.turnScript.Turn > 1 && actorSecrets < listOfSecrets.Count)
                {
                    if (isPlayer == true)
                    { TraitLogMessage(actor, "for a Learn Secret check", "to TRIPLE chance of learning a Secret"); }
                }
            }
            //trait Bedazzled
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
                            //secret can only be learned one turn AFTER player gains secret (the '+1' is to get around the messaging system)
                            if ((secret.gainedWhen + 1) < GameManager.instance.turnScript.Turn)
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
                                    if (isPlayer == true)
                                    {
                                        string text = string.Format("{0}, {1}, learnt SECRET", actor.actorName, actor.arc.name);
                                        GameManager.instance.messageScript.GeneralRandom(text, "Learnt Secret", chance, rnd, true);
                                    }
                                    GameManager.instance.dataScript.StatisticIncrement(StatType.ActorLearntSecret);
                                    //trait Blabbermouth
                                    if (actor.CheckTraitEffect(actorSecretTellAll) == true)
                                    {
                                        //actor passes secret onto all other actors
                                        ProcessSecretTellAll(secret, actor);
                                        if (isPlayer == true)
                                        { TraitLogMessage(actor, "for passing on secrets", "to TELL ALL about secret"); }
                                    }
                                }
                                else
                                {
                                    if (isPlayer == true)
                                    {
                                        string text = string.Format("{0}, {1}, didn't learn Secret", actor.actorName, actor.arc.name);
                                        GameManager.instance.messageScript.GeneralRandom(text, "Learn Secret", chance, rnd, true);
                                    }
                                }
                            }
                            /*else { Debug.LogFormat("[Tst] ActorManager.cs -> ProcessSecrets: Can't learn secret, gained turn {0}, current turn {1}", secret.gainedWhen, GameManager.instance.turnScript.Turn); }*/
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid secret (Null) in listOFSecrets[{0}]", i); }
                }
            }
            else
            {
                if (isPlayer == true)
                { TraitLogMessage(actor, "for a Learn Secret check", "to AVOID learning any secrets"); }
            }
        }
        else { Debug.LogWarning("Invalid actor (Null)"); }
    }

    /// <summary>
    /// subMethod for ProcessSecrets to handle trait Blabbermouth where an actor tells all other actors about the secret they have learned. Actor is the actor who learned of secret. Default current side.
    /// NOTE: actorTrait & secret checked for null by calling method. Actors are told of secret regardless whether they are active or inactive or have 'Bedazzled' trait
    /// </summary>
    /// <param name="secret"></param>
    /// <returns></returns>
    private int ProcessSecretTellAll(Secret secret, Actor actorTrait)
    {
        int numTold = 0;
        GlobalSide sideCurrent = GameManager.instance.turnScript.currentSide;
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(sideCurrent);
        //loop all actors on Map
        for (int i = 0; i < arrayOfActors.Length; i++)
        {
            //check actor is present in slot (not vacant)
            if (GameManager.instance.dataScript.CheckActorSlotStatus(i, sideCurrent) == true)
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
                            GameManager.instance.dataScript.StatisticIncrement(StatType.ActorLearntSecret);
                        }
                    }
                }
            }
        }
        return numTold;
    }

    /// <summary>
    /// subMethod to check if actors resign due to the Player having a bad condition (Corrupt/Incompetent/Questionable). Leave side as Null for default player side, specify otherwise
    /// NOTE: Actor checked for null by calling method
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="numOfBadConditions"></param>
    public void ProcessCompatibility(Actor actor, List<Condition> listOfBadConditions, GlobalSide side = null)
    {
        int chance, rnd;
        string itemText, reason, warning;
        string msgText = "";
        if (listOfBadConditions != null)
        {
            int numOfBadConditions = listOfBadConditions.Count;
            if (numOfBadConditions > 0)
            {
                chance = actorResignChance * numOfBadConditions;
                rnd = Random.Range(0, 100);
                //default player side if not specified
                if (side == null)
                { side = GameManager.instance.sideScript.PlayerSide; }
                //proceed only if actor doesn't have do not resign trait
                if (actor.CheckTraitEffect(actorNeverResigns) == false)
                {
                    //trait check -> 'Ethical' (triple chance of resigning)
                    if (actor.CheckTraitEffect(actorResignHigh) == true)
                    {
                        chance *= 3;
                        TraitLogMessage(actor, "for a Resignation check", "to TRIPLE chance of Resigning");
                    }
                    if (rnd < chance)
                    {
                        //Actor RESIGNS -> random message
                        Debug.LogFormat("[Rnd] ActorManager.cs -> ProcessCompatibility: Resign check SUCCESS need < {0}, rolled {1}{2}", chance, rnd, "\n");
                        string text = string.Format("{0} Resign attempt SUCCESS", actor.arc.name);
                        GameManager.instance.messageScript.GeneralRandom(text, "Resignation", chance, rnd, true);
                        //resignation
                        if (GameManager.instance.dataScript.RemoveCurrentActor(side, actor, ActorStatus.Resigned) == true)
                        {
                            //choose a random condition that actor is upset about
                            Condition conditionUpsetOver = listOfBadConditions[Random.Range(0, numOfBadConditions)];
                            if (conditionUpsetOver != null)
                            {
                                if (string.IsNullOrEmpty(conditionUpsetOver.resignTag) == false)
                                {
                                    msgText = string.Format("{0} Resigns (over Player's {1})", actor.arc.name, conditionUpsetOver.resignTag);
                                    Debug.LogFormat("[Ply] ActorManager.cs -> ProcessCompatibility: {0}, {1}, Resigns in disgust{2}", actor.actorName, actor.arc.name, "\n");
                                }
                                else { Debug.LogWarning("Invalid conditionUpsetOver.resignTag (Null or empty)"); }
                            }
                            else { Debug.LogWarning("Invalid conditionUpsetOver (Null)"); }
                            //autoRun
                            if (GameManager.instance.turnScript.CheckIsAutoRun() == true)
                            {
                                string textAutoRun = string.Format("{0}{1}{2}, {3}Resigns{4}", colourAlert, actor.arc.name, colourEnd, colourBad, colourEnd);
                                GameManager.instance.dataScript.AddHistoryAutoRun(textAutoRun);
                            }
                            //statistics
                            if (side.level == globalResistance.level)
                            { GameManager.instance.dataScript.StatisticIncrement(StatType.ActorsResignedResistance); }
                            else { GameManager.instance.dataScript.StatisticIncrement(StatType.ActorsResignedAuthority); }
                        }
                        //message
                        if (string.IsNullOrEmpty(msgText) == false)
                        { GameManager.instance.messageScript.ActorStatus(msgText, "Resigned", "has resigned because of Player reputation", actor.actorID, side); }
                    }
                    else
                    {
                        //random message
                        Debug.LogFormat("[Rnd] ActorManager.cs -> ProcessCompatibility: Resign check FAILED need < {0}, rolled {1}{2}", chance, rnd, "\n");
                        string text = string.Format("{0} Resign attempt FAILED", actor.arc.name);
                        GameManager.instance.messageScript.GeneralRandom(text, "Resignation", chance, rnd, true);
                    }
                }
                else
                {
                    //only generate message if the actor was about to resign
                    if (rnd < chance)
                    {
                        //trait actorResignNone "Loyal"
                        TraitLogMessage(actor, "for a Resignation check", "to AVOID Resigning");
                        //General Info message
                        msgText = string.Format("{0} considered Resigning but didn't because they are LOYAL", actor.arc.name);
                        itemText = string.Format("{0} almost RESIGNS", actor.arc.name);
                        reason = string.Format("<b>{0}, {1}{2}{3}{4}{5}considered Resigning</b>{6}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", colourBad, colourEnd);
                        warning = string.Format("But didn't because they are <b>{0}</b>", actor.GetTrait().tag);
                        GameManager.instance.messageScript.GeneralInfo(msgText, itemText, "Resignation", reason, warning, false);
                    }
                }
            }
            else { Debug.LogWarning("Invalid listOfBadConditions (empty)"); }
        }
        else { Debug.LogWarning("Invalid listOfBadConditions (Null)"); }
    }

    /// <summary>
    /// Warning if actor Invisibility Zero
    /// </summary>
    /// <param name="actor"></param>
    public void ProcessInvisibilityWarning(Actor actor)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        string itemText = string.Format("{0} at risk of Capture", actor.arc.name);
        string detailsTop = string.Format("{0}<b>{1}, {2}{3}{4}</b>{5}{6}<b>Invisibility</b> at {7}<b>Zero</b>{8}", "\n", actor.actorName, colourAlert, actor.arc.name, colourEnd,
            "\n", "\n", colourNeutral, colourEnd);
        string detailsBottom = string.Format("{0}<b>Can be CAPTURED</b>{1}", colourAlert, colourEnd);
        GameManager.instance.messageScript.ActorWarningOngoing(itemText, detailsTop, detailsBottom, actor.sprite, actor.actorID);
    }

    /// <summary>
    /// Warning if actor Motivation Zero, InfoApp effects tab
    /// </summary>
    /// <param name="actor"></param>
    public void ProcessMotivationWarning(Actor actor)
    {
        string msgText = string.Format("{0} at risk of a Relationship Conflict", actor.arc.name);
        string reason = string.Format("<b>{0}, {1}{2}{3}{4}{5}{6}{7}Motivation at {8}Zero{9}{10}</b>", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n",
            colourNormal, colourEnd, colourNeutral, colourEnd, "\n");
        string warning = string.Format("{0}<b>Can develop a RELATIONSHIP CONFLICT</b>{1}", colourBad, colourEnd);
        List<string> listOfHelp = new List<string>() { "conflict_0", "conflict_1", "conflict_2" };
        GameManager.instance.messageScript.ActorWarningOngoing(msgText, reason, warning, actor.sprite, actor.actorID, listOfHelp);
    }

    /// <summary>
    /// Checks all OnMap Inactive Authority actors, and returns any at max value back to Active status
    /// </summary>
    private void CheckInactiveAuthorityActorsHuman()
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
                            string text;
                            switch (actor.inactiveStatus)
                            {
                                case ActorInactive.Breakdown:
                                    //restore actor (one stress turn only)
                                    actor.Status = ActorStatus.Active;
                                    actor.inactiveStatus = ActorInactive.None;
                                    actor.tooltipStatus = ActorTooltip.None;
                                    GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, GameManager.instance.guiScript.alphaActive);
                                    text = string.Format("{0}, {1}, has recovered from their Breakdown", actor.arc.name, actor.actorName);
                                    GameManager.instance.messageScript.ActorStatus(text, "has Recovered", "has recovered from their Breakdown", actor.actorID, globalAuthority);
                                    break;
                                case ActorInactive.StressLeave:
                                    if (actor.isStressLeave == false)
                                    {
                                        //restore actor (one Stress Leave turn only)
                                        actor.Status = ActorStatus.Active;
                                        actor.inactiveStatus = ActorInactive.None;
                                        actor.tooltipStatus = ActorTooltip.None;
                                        GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, GameManager.instance.guiScript.alphaActive);
                                        text = string.Format("{0}, {1}, has returned from their Stress Leave", actor.actorName, actor.arc.name);
                                        GameManager.instance.messageScript.ActorStatus(text, "has Returned", "has returned from their Stress Leave", actor.actorID, globalAuthority);
                                        actor.RemoveCondition(conditionStressed, "due to Stress Leave");
                                        actor.numOfTimesStressLeave++;
                                    }
                                    else { actor.isStressLeave = false; }
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
            actor.numOfTimesBreakdown++;
            //change alpha of actor to indicate inactive status
            GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, GameManager.instance.guiScript.alphaInactive);
            //message (public)
            string text = string.Format("{0}, {1}, has suffered a Breakdown (Stressed)", actor.actorName, actor.arc.name);
            string itemText = "has suffered a Breakdown";
            string reason = "has suffered a Nervous Breakdown due to being <b>STRESSED</b>";
            string details = string.Format("{0}<b>Unavailable but will recover next turn</b>{1}", colourNeutral, colourEnd);
            GameManager.instance.messageScript.ActorStatus(text, itemText, reason, actor.actorID, side, details);
        }
        else { Debug.LogError("Invalid actor (Null)"); }
    }

    /// <summary>
    /// runs all start late turn Human player checks, both sides
    /// </summary>
    private void CheckPlayerHuman()
    {
        int rnd;
        int playerID = GameManager.instance.playerScript.actorID;
        string text, itemText, topText, detailsTop, detailsBottom;
        string reason = "Unknown";
        string warning = "Unknown";
        string playerName = GameManager.instance.playerScript.PlayerName;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //doom timer
        if (doomTimer > 0)
        {
            //decrement timer
            doomTimer--;
            //warning message
            text = string.Format("Resistance Player doomTimer now {0}. Death is imminent", doomTimer);
            if (playerSide.level == globalResistance.level)
            {
                topText = "You are Dying";
                itemText = string.Format("{0}'s DOOM TIMER is ticking down", playerName);
                reason = string.Format("{0}The deadly <b>gene tailored virus</b> is spreading throughout your body{1}{2}You need to find a <b>CURE</b>", "\n", "\n", "\n");
                warning = string.Format("You have {0} day{1} left to live", doomTimer, doomTimer != 1 ? "s" : "");
            }
            else
            {
                topText = string.Format("{0} is Dying", GameManager.instance.playerScript.GetPlayerNameResistance());
                itemText = string.Format("{0}'s DOOM TIMER is ticking down", GameManager.instance.playerScript.GetPlayerNameResistance());
                reason = string.Format("{0}The deadly <b>gene tailored virus</b> is spreading throughout their body{1}{2}They have to find a <b>CURE</b>", "\n", "\n", "\n");
                warning = string.Format("They have {0} day{1} left to live", doomTimer, doomTimer != 1 ? "s" : "");
            }
            GameManager.instance.messageScript.GeneralWarning(text, itemText, topText, reason, warning);
            //timer expired, Authority wins
            if (doomTimer == 0)
            {
                if (playerSide.level == globalResistance.level)
                {
                    detailsTop = string.Format("You have DIED from the {0}gene tailored virus{1} within your body", colourNeutral, colourEnd);
                    detailsBottom = string.Format("{0}Authority wins{1}", colourBad, colourEnd);
                }
                else
                {
                    detailsTop = string.Format("The Resistance head has DIED from the Nemesis administered {0}gene tailored virus{1} within their body", colourNeutral, colourEnd);
                    detailsBottom = string.Format("{0}You win{1}", colourBad, colourEnd);
                }
                GameManager.instance.turnScript.SetWinStateCampaign(WinStateCampaign.Authority, WinReasonCampaign.DoomTimerMin, detailsTop, detailsBottom);
            }
            else if (doomTimer == playerDoomFailSafeValue)
            {
                if (conditionDoomed.cure.isActive == false)
                {
                    //fail safe code to provide a cure automatically for doomed condition at a certain point
                    GameManager.instance.dataScript.SetCureNodeStatus(conditionDoomed.cure, true, false);
                }
            }
        }
        //Stats -> check for Stress Condition (do now as it encompasses both breakdown and stressed case statements below)
        if (GameManager.instance.playerScript.isStressed == true)
        { GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerStressedDays); }
        //check for Status -> both sides
        switch (GameManager.instance.playerScript.status)
        {
            case ActorStatus.Captured:
                //decrement timer
                captureTimerPlayer--;
                if (captureTimerPlayer <= 0)
                {
                    //special case of player captured and losing game from being permanently locked up
                    if (GameManager.instance.turnScript.winReasonCampaign != WinReasonCampaign.Innocence)
                    { GameManager.instance.captureScript.ReleasePlayer(true); }
                }
                else
                { GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerCapturedDays); }
                break;
            case ActorStatus.Inactive:
                //
                // - - - Inactive actor (Any) Info App (needs to be at the top for sequencing reasons) - - -
                //
                if (GameManager.instance.playerScript.inactiveStatus != ActorInactive.None)
                {
                    text = "HQ Support Unavailable";
                    topText = "Support Unavailable";
                    detailsTop = string.Format("{0}<b>You are out of contact</b>{1}", colourAlert, colourEnd);
                    detailsBottom = string.Format("<b>HQ will consider providing support once you are {0}back in contact</b>{1}", colourNeutral, colourEnd);
                    GameManager.instance.messageScript.ActiveEffect(text, topText, detailsTop, detailsBottom, GameManager.instance.playerScript.sprite, playerID);
                }
                switch (GameManager.instance.playerScript.inactiveStatus)
                {
                    case ActorInactive.Breakdown:
                        //restore player (one stress turn only)
                        GameManager.instance.playerScript.status = ActorStatus.Active;
                        GameManager.instance.playerScript.inactiveStatus = ActorInactive.None;
                        GameManager.instance.playerScript.tooltipStatus = ActorTooltip.None;
                        GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaActive);
                        string textBreakdown = string.Format("{0} has recovered from their Breakdown", playerName);
                        GameManager.instance.messageScript.ActorStatus(textBreakdown, "has Recovered", "has recovered from their Breakdown",
                            playerID, playerSide, null, HelpType.PlayerBreakdown);
                        Debug.LogFormat("[Ply] ActorManager.cs -> CheckPlayerHuman: {0}, Player, has recovered from their stress induced Breakdown{1}",
                            GameManager.instance.playerScript.GetPlayerName(playerSide), "\n");
                        if (playerSide.level == globalResistance.level)
                        {
                            //update AI side tab status
                            GameManager.instance.aiScript.UpdateSideTabData();
                        }
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
                            //check if Player has stressed condition -> Player.RemoveCondition handles mood change
                            if (GameManager.instance.playerScript.isStressed == true)
                            { GameManager.instance.playerScript.RemoveCondition(conditionStressed, playerSide, "Lying Low removes Stress"); }
                            //improve mood
                            GameManager.instance.playerScript.ChangeMood(GameManager.instance.playerScript.moodReset, "Finished Lying Low", "n.a");
                            //message -> status change
                            text = string.Format("{0} has automatically reactivated", playerName);
                            GameManager.instance.messageScript.ActorStatus(text, "is now Active", "has finished Lying Low", playerID, globalResistance, null, HelpType.LieLow);
                            Debug.LogFormat("[Ply] ActorManager.cs -> CheckPlayerHuman: {0}, Player, is no longer Lying Low{1}", GameManager.instance.playerScript.GetPlayerName(playerSide), "\n");
                        }
                        else
                        {
                            //statistic
                            GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerLieLowDays);
                            GameManager.instance.dataScript.StatisticIncrement(StatType.LieLowDaysTotal);
                            GameManager.instance.playerScript.Invisibility = invis;
                        }
                        break;
                    case ActorInactive.StressLeave:
                        if (GameManager.instance.playerScript.isStressLeave == false)
                        {
                            //restore actor (one Stress Leave turn only)
                            GameManager.instance.playerScript.status = ActorStatus.Active;
                            GameManager.instance.playerScript.inactiveStatus = ActorInactive.None;
                            GameManager.instance.playerScript.tooltipStatus = ActorTooltip.None;
                            GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaActive);
                            text = string.Format("{0}, Player, has returned from their Stress Leave", GameManager.instance.playerScript.GetPlayerName(globalAuthority));
                            GameManager.instance.messageScript.ActorStatus(text, "has Returned", "has returned from their Stress Leave", playerID, globalAuthority, null, HelpType.StressLeave);
                            GameManager.instance.playerScript.RemoveCondition(conditionStressed, playerSide, "Finished Stress Leave");
                            Debug.LogFormat("[Ply] ActorManager.cs -> CheckPlayerHuman: {0}, Player, returns from Stress Leave{1}", GameManager.instance.playerScript.GetPlayerName(playerSide), "\n");
                        }
                        else { GameManager.instance.playerScript.isStressLeave = false; }
                        break;
                }
                //
                // - - - Lie Low Info App - - -
                //
                if (GameManager.instance.playerScript.inactiveStatus == ActorInactive.LieLow)
                {
                    text = string.Format("{0}, PLAYER, is LYING LOW", playerName);
                    topText = "Lying Low";
                    detailsTop = "You are deliberately keeping a <b>Low Profile</b>";
                    detailsBottom = string.Format("{0}<b>You can't take ANY actions while Lying Low</b>{1}", colourAlert, colourEnd);
                    GameManager.instance.messageScript.ActiveEffect(text, topText, detailsTop, detailsBottom, GameManager.instance.playerScript.sprite, playerID);
                }
                break;
            case ActorStatus.Active:
                {
                    //
                    // - - - STRESSED condition
                    //
                    if (GameManager.instance.playerScript.isStressed == true)
                    {
                        //enforces a minimum one turn gap between successive breakdowns
                        if (GameManager.instance.playerScript.isBreakdown == false)
                        {
                            //check player with the stressed condition for a breakdown
                            rnd = Random.Range(0, 100);
                            if (rnd < breakdownChance)
                            {
                                // - - - BREAKDOWN
                                GameManager.instance.playerScript.status = ActorStatus.Inactive;
                                GameManager.instance.playerScript.inactiveStatus = ActorInactive.Breakdown;
                                GameManager.instance.playerScript.tooltipStatus = ActorTooltip.Breakdown;
                                GameManager.instance.playerScript.isBreakdown = true;
                                GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerBreakdown);
                                //change alpha of actor to indicate inactive status
                                GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaInactive);
                                //message (public)
                                text = "Player has suffered a Breakdown (Stressed)";
                                itemText = "has suffered a BREAKDOWN";
                                reason = "has suffered a Nervous Breakdown due to being <b>STRESSED</b>";
                                string details = string.Format("{0}<b>Unavailable but will recover next turn</b>{1}", colourNeutral, colourEnd);
                                GameManager.instance.messageScript.ActorStatus(text, itemText, reason, playerID, playerSide, details, HelpType.PlayerBreakdown);
                                Debug.LogFormat("[Rnd] ActorManager.cs -> CheckPlayerStartlate: Stress check SUCCESS -> need < {0}, rolled {1}{2}",
                                    breakdownChance, rnd, "\n");
                                Debug.LogFormat("[Ply] ActorManager.cs -> CheckPlayerHuman: {0}, Player, undergoes a Stress BREAKDOWN{1}", GameManager.instance.playerScript.GetPlayerName(playerSide), "\n");
                                GameManager.instance.messageScript.GeneralRandom("Player Stress check SUCCESS", "Stress Breakdown", breakdownChance, rnd, true);
                                if (playerSide.level == globalResistance.level)
                                {
                                    //update AI side tab status
                                    GameManager.instance.aiScript.UpdateSideTabData();
                                }
                            }
                            else
                            {
                                if (GameManager.instance.playerScript.numOfSuperStress < 1)
                                {
                                    // - - - NO Breakdown
                                    Debug.LogFormat("[Rnd] ActorManager.cs -> CheckPlayerStartlate: Stress check FAILED -> need < {0}, rolled {1}{2}",
                                        breakdownChance, rnd, "\n");
                                    GameManager.instance.messageScript.GeneralRandom("Player Stress check FAILED", "Stress Breakdown", breakdownChance, rnd, true);
                                }
                                else
                                {
                                    // - - - FORCE a Breakdown, player SuperStressed
                                    GameManager.instance.playerScript.status = ActorStatus.Inactive;
                                    GameManager.instance.playerScript.inactiveStatus = ActorInactive.Breakdown;
                                    GameManager.instance.playerScript.tooltipStatus = ActorTooltip.Breakdown;
                                    GameManager.instance.playerScript.isBreakdown = true;
                                    GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerBreakdown);
                                    //reduce superstress counter
                                    GameManager.instance.playerScript.numOfSuperStress--;
                                    //change alpha of actor to indicate inactive status
                                    GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaInactive);
                                    //message (public)
                                    text = "Player has suffered a Breakdown";
                                    itemText = "has suffered a BREAKDOWN";
                                    reason = "has suffered a Nervous Breakdown due to being <b>STRESSED</b>";
                                    string details = string.Format("{0}<b>Unavailable but will recover next turn</b>{1}", colourNeutral, colourEnd);
                                    GameManager.instance.messageScript.ActorStatus(text, itemText, reason, playerID, playerSide, details, HelpType.PlayerBreakdown);
                                    Debug.LogFormat("[Ply] ActorManager.cs -> CheckPlayerHuman: {0}, Player, undergoes a SUPER Stress BREAKDOWN{1}",
                                        GameManager.instance.playerScript.GetPlayerName(playerSide), "\n");
                                    //fake a successful roll (chance is superStressChance to make it appear as there are higher odds but in reality it is a forced breakdown)
                                    rnd = Random.Range(0, superStressChance);
                                    GameManager.instance.messageScript.GeneralRandom("Player Stress check SUCCESS", "Stress Breakdown", superStressChance, rnd, true);
                                    if (playerSide.level == globalResistance.level)
                                    {
                                        //update AI side tab status
                                        GameManager.instance.aiScript.UpdateSideTabData();
                                    }
                                }
                            }
                        }
                        else { GameManager.instance.playerScript.isBreakdown = false; }
                    }
                    //
                    // - - - ADDICTED condition - - - 
                    //
                    if (GameManager.instance.playerScript.isAddicted == true)
                    {
                        //days addicted (this instance)
                        GameManager.instance.playerScript.addictedTally++;
                        //immunity period must be zero, eg. effect of previous dose has worn off (there is an initial 2 turn buffer after becoming addicted)
                        if (GameManager.instance.playerScript.stressImmunityCurrent == 0 && GameManager.instance.playerScript.addictedTally > playerAddictedExempt)
                        {
                            int renownCost = 0;
                            int approvalCost = 0;
                            //chance of having to spend renown to buy supplies
                            rnd = Random.Range(0, 100);
                            if (rnd < playerAddictedChance)
                            {
                                //random message
                                text = string.Format("[Rnd] ActorManager.cs -> CheckPlayerHuman: Addiction check SUCCEEDED, need < {0}, rolled {1}{2}", playerAddictedChance, rnd, "\n");
                                GameManager.instance.messageScript.GeneralRandom("Player ADDICTION check SUCCEEDED", "Addiction", playerAddictedChance, rnd, true, "rand_2");
                                //need to spend renown
                                int renown = GameManager.instance.playerScript.Renown;
                                if (renown < playerAddictedRenownCost)
                                {
                                    //insufficient renown, HQ support -1
                                    GameManager.instance.factionScript.ChangeFactionApproval(-1, playerSide, "Player Addiction");
                                    //feed the need text
                                    text = string.Format("[Msg] ActorManager.cs -> CheckPlayerHuman: Player has to FEED the NEED, Insufficient Renown, -1 HQ Approval{0}", "\n");
                                    approvalCost = 1;
                                }
                                else
                                {
                                    //enough renown, lose some to pay for addiction
                                    renown -= playerAddictedRenownCost;
                                    GameManager.instance.playerScript.Renown = renown;
                                    //feed the need text
                                    text = string.Format("[Msg] ActorManager.cs -> CheckPlayerHuman: Player has to FEED the NEED and pays {0} Renown (now {1}) for drugs{2}", playerAddictedRenownCost,
                                        renown, "\n");
                                    renownCost = playerAddictedRenownCost;
                                }
                                //take the drugs
                                GameManager.instance.playerScript.TakeDrugs();
                                //feed the need message
                                GameManager.instance.messageScript.PlayerAddicted(text, renownCost, approvalCost, GameManager.instance.playerScript.stressImmunityCurrent - 1);
                            }
                            else
                            {
                                //random message
                                text = string.Format("[Rnd] ActorManager.cs -> CheckPlayerHuman: Addiction check FAILED, need < {0}, rolled {1}{2}", playerAddictedChance, rnd, "\n");
                                GameManager.instance.messageScript.GeneralRandom("Player ADDICTION check FAILED", "Addiction", playerAddictedChance, rnd, true, "rand_2");
                            }
                            //stats
                            GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerAddictedDays);
                        }
                        else
                        {
                            Debug.LogFormat("[Tst] ActorManager.cs -> CheckPlayerHuman: Addiction Feed the Need EXEMPT (immunity {0}, addictedTally {1}){2}",
                                GameManager.instance.playerScript.stressImmunityCurrent, GameManager.instance.playerScript.addictedTally, "\n");
                        }
                    }
                    //
                    // - - - On Drugs (may, or may not, be addicted) - - -
                    //

                    if (GameManager.instance.playerScript.stressImmunityCurrent > 0)
                    {
                        //decrement immune period
                        GameManager.instance.playerScript.stressImmunityCurrent--;
                        //message
                        int immuneCurrent = GameManager.instance.playerScript.stressImmunityCurrent;
                        int immuneStart = GameManager.instance.playerScript.stressImmunityStart;
                        bool isAddicted = GameManager.instance.playerScript.isAddicted;
                        if (GameManager.instance.playerScript.stressImmunityCurrent > 0)
                        {
                            text = string.Format("[Msg] Player Immune to stress for {0} more days, isAddicted {1}{2}", immuneCurrent, immuneStart, isAddicted, "\n");
                            GameManager.instance.messageScript.PlayerImmuneEffect(text, immuneCurrent, immuneStart, isAddicted);
                        }

                    }
                    //
                    // - - - IMAGED condition - - -
                    //
                    if (GameManager.instance.playerScript.CheckConditionPresent(conditionImaged, playerSide) == true)
                    {
                        //Player has IMAGED condition. Random chance of them being picked up by facial recognition software
                        rnd = Random.Range(0, 100);
                        Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                        if (rnd < playerRecognisedChance)
                        {
                            text = "Player SPOTTED by Facial Recognition Scan";
                            detailsTop = string.Format("<b>Due to being {0}IMAGED{1}, Facial Recognition Scans have identified you</b>", colourNeutral, colourEnd);
                            //Player loses a level of invisibility
                            int invis = GameManager.instance.playerScript.Invisibility;
                            if (invis > 0)
                            {
                                invis--;
                                GameManager.instance.playerScript.Invisibility = invis;
                                detailsBottom = string.Format("{0}<b>Invisibility -1{1}", colourBad, colourEnd);
                            }
                            else
                            {
                                //player location known immediately
                                detailsBottom = string.Format("{0}<b>Invisibility -1{1}{2}Authority knows IMMEDIATELY</b>{3}", colourBad, "\n", "\n", colourEnd);
                                GameManager.instance.aiScript.immediateFlagResistance = true;
                                if (node != null)
                                {
                                    GameManager.instance.messageScript.AINodeActivity("Resistance Activity \"IMAGED\" (Player)", node, playerID, 0);
                                    //AI Immediate message
                                    GameManager.instance.messageScript.AIImmediateActivity("Immediate Activity \"IMAGED\" (Player)", "Facial Recognition Scan", node.nodeID, -1, playerID);
                                }
                                else { Debug.LogErrorFormat("Invalid node (Null) for playerNodeID {0}", GameManager.instance.nodeScript.nodePlayer); }
                            }
                            //messages
                            GameManager.instance.messageScript.PlayerSpotted(text, detailsTop, detailsBottom, node);
                            GameManager.instance.messageScript.GeneralRandom("Player IMAGED Recognition check SUCCESS", "Facial Recognition Scan", playerRecognisedChance, rnd, true);
                        }
                        else { GameManager.instance.messageScript.GeneralRandom("Player IMAGED Recognition check FAILED", "Facial Recognition Scan", playerRecognisedChance, rnd, true); }

                    }
                    //
                    // - - - QUESTIONABLE condition. 
                    //
                    if (GameManager.instance.playerScript.CheckConditionPresent(conditionQuestionable, globalResistance) == true)
                    {
                        //Random chance of losing HQ approval
                        rnd = Random.Range(0, 100);
                        if (rnd < playerQuestionableChance)
                        { GameManager.instance.factionScript.ChangeFactionApproval(-1, globalResistance, "due to QUESTIONABLE loyalty"); }

                    }
                }
                break;
                //NO Default case here, only check for what you are interested in
        }
        //
        // - - - Info App conditions (any)
        //
        if (GameManager.instance.playerScript.CheckNumOfConditions(playerSide) > 0)
        {
            List<Condition> listOfConditions = GameManager.instance.playerScript.GetListOfConditions(playerSide);
            foreach (Condition condition in listOfConditions)
            {
                if (condition != null)
                {
                    text = string.Format("{0}, PLAYER, has the {1} condition", playerName, condition.tag);
                    topText = "Condition present";
                    switch (condition.type.level)
                    {
                        case 0: detailsTop = string.Format("{0}{1} {2}{3}", colourAlert, playerName, condition.topText, colourEnd); break;
                        case 1: detailsTop = string.Format("{0}{1} {2}{3}", colourNeutral, playerName, condition.topText, colourEnd); break;
                        case 2: detailsTop = string.Format("{0}{1} {2}{3}", colourNeutral, playerName, condition.topText, colourEnd); break;
                        default: detailsTop = "Unknown"; break;
                    }
                    switch (condition.bottomTextTypePlayer.level)
                    {
                        case 0: detailsBottom = string.Format("{0}{1}{2}", colourBad, condition.bottomTextPlayer, colourEnd); break;
                        case 1: detailsBottom = string.Format("{0}{1}{2}", colourNeutral, condition.bottomTextPlayer, colourEnd); break;
                        case 2: detailsBottom = string.Format("{0}{1}{2}", colourGood, condition.bottomTextPlayer, colourEnd); break;
                        default: detailsBottom = "Unknown"; break;
                    }
                    GameManager.instance.messageScript.ActiveEffect(text, topText, detailsTop, detailsBottom, GameManager.instance.playerScript.sprite, GameManager.instance.playerScript.actorID, null, condition);
                }
                else { Debug.LogWarningFormat("Invalid condition (Null) for {0}, Player, ID {1}", playerName, GameManager.instance.playerScript.actorID); }
            }
            //cures
            GameManager.instance.dataScript.CheckCures();
        }
        //
        // - - - Action Adjustments
        //
        if (GameManager.instance.dataScript.CheckNumOfActionAdjustments(playerSide) > 0)
        {
            List<ActionAdjustment> listOfActionAdjustments = GameManager.instance.dataScript.GetListOfActionAdjustments();
            if (listOfActionAdjustments != null)
            {
                for (int i = 0; i < listOfActionAdjustments.Count; i++)
                {
                    ActionAdjustment adjustment = listOfActionAdjustments[i];
                    if (adjustment.sideLevel == playerSide.level)
                    {
                        //valid adjustment present, create an InfoApp effect message
                        text = "Your available ACTIONS have changed";
                        topText = "Actions Adjustment";
                        if (adjustment.value > 0)
                        {
                            detailsTop = string.Format("{0}<b>{1}</b>{2}{3}{4}gives {5}<b>{6}{7} EXTRA</b> Action{8}", colourGood, adjustment.descriptor, colourEnd, "\n", "\n", colourNeutral, adjustment.value, colourEnd,
                              adjustment.value != 1 ? "s" : "");
                        }
                        else
                        {
                            detailsTop = string.Format("{0}{1}{2}{3}{4}gives {5}<b>{6}{7} LESS</b> Action{8}", colourBad, adjustment.descriptor, colourEnd, "\n", "\n", colourNeutral, adjustment.value, colourEnd,
                              adjustment.value != 1 ? "s" : "");
                        }
                        detailsBottom = string.Format("Lasts for {0}<b>{1}{2} turn</b>{3}", colourNeutral, adjustment.timer, colourEnd, adjustment.timer != 1 ? "s" : "");
                        GameManager.instance.messageScript.ActiveEffect(text, topText, detailsTop, detailsBottom, GameManager.instance.guiScript.alarmSprite, 999);
                    }
                }
            }
            else { Debug.LogError("Invalid listOfActionAdjustments (Null)"); }
        }
    }


    /// <summary>
    /// run all late start turn AI Resistance player checks
    /// </summary>
    private void CheckPlayerResistanceAI(bool isPlayer)
    {
        int rnd;
        string text, topText, detailsTop, detailsBottom;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        int playerID = GameManager.instance.playerScript.actorID;
        string playerName = GameManager.instance.playerScript.PlayerName;
        //
        // - - - doom timer
        //
        if (doomTimer > 0)
        {
            //decrement timer
            doomTimer--;
            if (isPlayer == true)
            {
                //warning message
                text = string.Format("Player doomTimer now {0}. Death is imminent", doomTimer);
                string itemText = string.Format("{0}'s DOOM TIMER is ticking down", playerName);
                topText = "You are Dying";
                string reason = string.Format("{0}The deadly <b>gene tailored virus</b> is spreading throughout your body{1}{2}You need to find a <b>CURE</b>", "\n", "\n", "\n");
                string warning = string.Format("You have {0} day{1} left to live", doomTimer, doomTimer != 1 ? "s" : "");
                GameManager.instance.messageScript.GeneralWarning(text, itemText, topText, reason, warning);
            }
            Debug.LogFormat("[Rim] ActorManager.cs -> CheckPlayerResistanceAI: Player DOOMED, {0} day{1} left to live{2}", doomTimer, doomTimer != 1 ? "s" : "", "\n");
            //timer expired, Authority wins
            if (doomTimer == 0)
            {
                if (isPlayer == true)
                {
                    detailsTop = string.Format("You have DIED from the {0}gene tailored virus{1} within your body", colourNeutral, colourEnd);
                    detailsBottom = string.Format("{0}Authority wins{1}", colourBad, colourEnd);
                }
                else
                {
                    detailsTop = string.Format("The Resistance head has DIED from the Nemesis administered {0}gene tailored virus{1} within their body", colourNeutral, colourEnd);
                    detailsBottom = string.Format("{0}You win{1}", colourBad, colourEnd);
                }
                GameManager.instance.turnScript.SetWinStateCampaign(WinStateCampaign.Authority, WinReasonCampaign.DoomTimerMin, detailsTop, detailsBottom);
            }
        }
        //
        // - - - Status's
        //
        switch (GameManager.instance.aiRebelScript.status)
        {
            case ActorStatus.Captured:
                //decrement timer
                captureTimerPlayer--;
                if (captureTimerPlayer <= 0)
                { GameManager.instance.captureScript.ReleasePlayer(true); }
                else
                { GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerCapturedDays); }
                break;
            case ActorStatus.Inactive:
                switch (GameManager.instance.aiRebelScript.inactiveStatus)
                {
                    case ActorInactive.Breakdown:
                        //restore player (one stress turn only)
                        GameManager.instance.aiRebelScript.status = ActorStatus.Active;
                        GameManager.instance.aiRebelScript.inactiveStatus = ActorInactive.None;
                        if (isPlayer == true)
                        {
                            string textBreakdown = string.Format("{0} has recovered from their Breakdown", playerName);
                            GameManager.instance.messageScript.ActorStatus(textBreakdown, "has Recovered", "has recovered from their breakdown",
                              playerID, playerSide);
                        }
                        Debug.LogFormat("[Ply] ActorManager.cs -> CheckPlayerResistanceAI: Player has RECOVERED from their Breakdown{0}", "\n");
                        /*//update AI side tab status
                        GameManager.instance.aiScript.UpdateSideTabData();*/
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
                            GameManager.instance.aiRebelScript.status = ActorStatus.Active;
                            GameManager.instance.aiRebelScript.inactiveStatus = ActorInactive.None;
                            if (isPlayer == true)
                            {
                                //message -> status change
                                text = string.Format("{0} has automatically reactivated", playerName);
                                GameManager.instance.messageScript.ActorStatus(text, "is now Active", "has finished Lying Low", playerID, globalResistance);
                                Debug.LogFormat("[Ply] ActorManager.cs -> CheckPlayerResistanceAI: Player no longer Lying Low at node id {0}{1}", GameManager.instance.nodeScript.nodePlayer, "\n");
                            }
                            //check if Player has stressed condition
                            if (GameManager.instance.playerScript.CheckConditionPresent(conditionStressed, globalResistance) == true)
                            { GameManager.instance.playerScript.RemoveCondition(conditionStressed, globalResistance, "Lying Low removes Stress"); }
                        }
                        else
                        {
                            GameManager.instance.playerScript.Invisibility = invis;
                            //statistic
                            GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerLieLowDays);
                            GameManager.instance.dataScript.StatisticIncrement(StatType.LieLowDaysTotal);
                        }
                        break;
                }
                //
                // - - - Lie Low Info App - - -
                //
                if (GameManager.instance.aiRebelScript.inactiveStatus == ActorInactive.LieLow)
                {
                    if (isPlayer == true)
                    {
                        text = string.Format("{0}, PLAYER, is LYING LOW", playerName);
                        topText = "Lying Low";
                        detailsTop = "You are deliberately keeping a <b>Low Profile</b>";
                        detailsBottom = string.Format("{0}<b>You can't take ANY actions while Lying Low</b>{1}", colourAlert, colourEnd);
                        GameManager.instance.messageScript.ActiveEffect(text, topText, detailsTop, detailsBottom, GameManager.instance.playerScript.sprite, playerID);
                    }
                }
                break;
            case ActorStatus.Active:
                {
                    //
                    // - - - Stressed
                    //
                    if (GameManager.instance.playerScript.CheckConditionPresent(conditionStressed, globalResistance) == true)
                    {
                        //enforces a minimum one turn gap between successive breakdowns
                        if (GameManager.instance.aiRebelScript.isBreakdown == false)
                        {
                            //chance of a Breakdown
                            rnd = Random.Range(0, 100);
                            if (rnd < breakdownChance)
                            {
                                //player Breakdown
                                GameManager.instance.aiRebelScript.status = ActorStatus.Inactive;
                                GameManager.instance.aiRebelScript.inactiveStatus = ActorInactive.Breakdown;
                                GameManager.instance.aiRebelScript.isBreakdown = true;
                                if (isPlayer == true)
                                {
                                    GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerBreakdown);
                                    //message (public)
                                    text = "Player has suffered a Breakdown (Stressed)";
                                    string itemText = "has suffered a BREAKDOWN";
                                    string reason = "has suffered a Nervous Breakdown due to being <b>STRESSED</b>";
                                    string details = string.Format("{0}<b>Unavailable but will recover next turn</b>{1}", colourNeutral, colourEnd);
                                    GameManager.instance.messageScript.ActorStatus(text, itemText, reason, playerID, playerSide, details);
                                    Debug.LogFormat("[Rnd] ActorManager.cs -> CheckPlayerResistanceAI: Stress check SUCCESS -> need < {0}, rolled {1}{2}",
                                    breakdownChance, rnd, "\n");
                                    GameManager.instance.messageScript.GeneralRandom("Player Stress check SUCCESS", "Stress Breakdown", breakdownChance, rnd, true);
                                }
                                Debug.LogFormat("[Ply] ActorManager.cs -> CheckPlayerResistanceAI: Stress BREAKDOWN occurs{0}", "\n");
                            }
                            else
                            {
                                if (isPlayer == true)
                                {
                                    Debug.LogFormat("[Rnd] ActorManager.cs -> CheckPlayerResistanceAI: Stress check FAILED -> need < {0}, rolled {1}{2}",
                                        breakdownChance, rnd, "\n");
                                    GameManager.instance.messageScript.GeneralRandom("Player Stress check FAILED", "Stress Breakdown", breakdownChance, rnd, true);
                                }
                            }
                        }
                        else { GameManager.instance.aiRebelScript.isBreakdown = false; }
                    }
                    //
                    // - - - IMAGED condition. 
                    //
                    if (GameManager.instance.playerScript.CheckConditionPresent(conditionImaged, globalResistance) == true)
                    {
                        //Random chance of them being picked up by facial recognition software
                        rnd = Random.Range(0, 100);
                        Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                        if (rnd < playerRecognisedChance)
                        {
                            text = "Player SPOTTED by Facial Recognition Scan";
                            detailsTop = string.Format("<b>Due to being {0}IMAGED{1}, Facial Recognition Scans have identified you</b>", colourNeutral, colourEnd);
                            //Player loses a level of invisibility
                            int invis = GameManager.instance.playerScript.Invisibility;
                            if (invis > 0)
                            {
                                invis--;
                                GameManager.instance.playerScript.Invisibility = invis;
                                detailsBottom = string.Format("{0}<b>Invisibility -1{1}", colourBad, colourEnd);
                            }
                            else
                            {
                                //player location known immediately
                                detailsBottom = string.Format("{0}<b>Invisibility -1{1}{2}Authority knows IMMEDIATELY</b>{3}", colourBad, "\n", "\n", colourEnd);
                                GameManager.instance.aiScript.immediateFlagResistance = true;
                                if (node != null)
                                {
                                    GameManager.instance.messageScript.AINodeActivity("Resistance Activity \"IMAGED\" (Player)", node, playerID, 0);
                                    //AI Immediate message
                                    GameManager.instance.messageScript.AIImmediateActivity("Immediate Activity \"IMAGED\" (Player)", "Facial Recognition Scan", node.nodeID, -1, playerID);
                                }
                                else { Debug.LogErrorFormat("Invalid node (Null) for playerNodeID {0}", GameManager.instance.nodeScript.nodePlayer); }
                            }
                            //messages
                            if (isPlayer == true)
                            {
                                GameManager.instance.messageScript.PlayerSpotted(text, detailsTop, detailsBottom, node);
                                GameManager.instance.messageScript.GeneralRandom("Player IMAGED Recognition check SUCCESS", "Facial Recognition Scan", playerRecognisedChance, rnd, true);
                            }
                        }
                        else
                        {
                            if (isPlayer == true)
                            { GameManager.instance.messageScript.GeneralRandom("Player IMAGED Recognition check FAILED", "Facial Recognition Scan", playerRecognisedChance, rnd, true); }
                        }

                    }
                    //
                    // - - - QUESTIONABLE condition. 
                    //
                    if (GameManager.instance.playerScript.CheckConditionPresent(conditionQuestionable, globalResistance) == true)
                    {
                        //Random chance of losing HQ approval
                        rnd = Random.Range(0, 100);
                        if (rnd < playerQuestionableChance)
                        { GameManager.instance.factionScript.ChangeFactionApproval(-1, globalResistance, "due to QUESTIONABLE loyalty"); }

                    }
                }
                break;
                //NO Default case here, only check for what you are interested in
        }
    }


    /// <summary>
    /// run all late start turn AI Authority player checks
    /// </summary>
    private void CheckPlayerAuthorityAI(bool isPlayer)
    {
        int rnd;
        string text;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        int playerID = GameManager.instance.playerScript.actorID;
        string playerName = GameManager.instance.playerScript.PlayerName;
        //
        // - - - Conditions 
        //
        switch (GameManager.instance.aiScript.status)
        {
            case ActorStatus.Inactive:
                switch (GameManager.instance.aiScript.inactiveStatus)
                {
                    case ActorInactive.Breakdown:
                        //restore player (one stress turn only)
                        GameManager.instance.aiScript.status = ActorStatus.Active;
                        GameManager.instance.aiScript.inactiveStatus = ActorInactive.None;
                        if (isPlayer == true)
                        {
                            text = string.Format("{0} has recovered from their Breakdown", playerName);
                            GameManager.instance.messageScript.ActorStatus(text, "has Recovered", "has recovered from their breakdown",
                              playerID, playerSide);
                        }
                        Debug.LogFormat("[Ply] ActorManager.cs -> CheckPlayerAuthorityAI: Player has RECOVERED from their Breakdown{0}", "\n");
                        break;
                }
                break;
            case ActorStatus.Active:
                {
                    //check the stressed condition for a breakdown
                    if (GameManager.instance.playerScript.CheckConditionPresent(conditionStressed, globalAuthority) == true)
                    {
                        //enforces a minimum one turn gap between successive breakdowns
                        if (GameManager.instance.aiScript.isBreakdown == false)
                        {
                            rnd = Random.Range(0, 100);
                            if (rnd < breakdownChance)
                            {
                                //player Breakdown
                                GameManager.instance.aiScript.status = ActorStatus.Inactive;
                                GameManager.instance.aiScript.inactiveStatus = ActorInactive.Breakdown;
                                GameManager.instance.aiScript.isBreakdown = true;
                                if (isPlayer == true)
                                {
                                    GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerBreakdown);
                                    //message (public)
                                    text = "Player has suffered a Breakdown (Stressed)";
                                    string itemText = "has suffered a BREAKDOWN";
                                    string reason = "has suffered a Nervous Breakdown due to being <b>STRESSED</b>";
                                    string details = string.Format("{0}<b>Unavailable but will recover next turn</b>{1}", colourNeutral, colourEnd);
                                    GameManager.instance.messageScript.ActorStatus(text, itemText, reason, playerID, playerSide, details);
                                    Debug.LogFormat("[Rnd] ActorManager.cs -> CheckPlayerAuthorityAI: Stress check SUCCESS -> need < {0}, rolled {1}{2}",
                                    breakdownChance, rnd, "\n");
                                    GameManager.instance.messageScript.GeneralRandom("Player Stress check SUCCESS", "Stress Breakdown", breakdownChance, rnd, true);
                                }
                                Debug.LogFormat("[Ply] ActorManager.cs -> CheckPlayerAuthorityAI: Stress BREAKDOWN occurs{0}", "\n");
                            }
                            else
                            {
                                if (isPlayer == true)
                                {
                                    Debug.LogFormat("[Rnd] ActorManager.cs -> CheckPlayerAuthorityAI: Stress check FAILED -> need < {0}, rolled {1}{2}",
                                        breakdownChance, rnd, "\n");
                                    GameManager.instance.messageScript.GeneralRandom("Player Stress check FAILED", "Stress Breakdown", breakdownChance, rnd, true);
                                }
                            }
                        }
                        else { GameManager.instance.aiScript.isBreakdown = false; }
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
        string msgText, itemText, reason, warning;
        List<int> listOfActors = null;
        int chance, rnd;
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
                        if (actor.unhappyTimer == 1)
                        {
                            //unhappy in one turn warning
                            msgText = string.Format("{0}, {1}, in Reserves, will be Unhappy in 1 turn", actor.actorName, actor.arc.name);
                            itemText = string.Format("Reserve {0} about to become Unhappy", actor.arc.name);
                            reason = string.Format("<b>{0}, {1}{2}{3}, is upset at being left in the Reserves</b>", actor.actorName, colourAlert, actor.arc.name, colourEnd);
                            warning = string.Format("An UNHAPPY subordinate is unpredictable", colourNeutral, colourEnd, colourBad, colourEnd);
                            GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Unhappy next turn", reason, warning, false);
                        }
                        //if timer now zero, gain condition "Unhappy" -> [Edit] Already covered by Unhappy Condition message
                        if (actor.unhappyTimer == 0)
                        {
                            Condition condition = GameManager.instance.dataScript.GetCondition("UNHAPPY");
                            actor.AddCondition(condition, string.Format("{0} is upset at being left in the Reserves", actor.arc.name));
                        }
                    }
                    else
                    {
                        //unhappy timer has reached zero. Is actor's motivation > 0?
                        if (actor.GetDatapoint(ActorDatapoint.Motivation1) > 0)
                        {
                            //chance of decrementing motivation each turn till it reaches zero
                            chance = unhappyLoseMotivationChance;
                            //chance is 100% if actor was promised
                            if (actor.isPromised == true)
                            { chance = 100; }
                            rnd = Random.Range(0, 100);
                            if (rnd < chance)
                            {
                                int motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                                motivation--;
                                actor.SetDatapoint(ActorDatapoint.Motivation1, motivation, "Unhappy in Reserves");
                                Debug.LogFormat("CheckReserveActors: Resistance {0} {1} UNHAPPY, Motivation now {2}{3}", actor.arc.name, actor.actorName,
                                    motivation, "\n");
                                //lost motivation warning
                                msgText = string.Format("{0}, {1}, in Reserves, has lost Motivation", actor.actorName, actor.arc.name);
                                itemText = string.Format("Reserve {0} loses Motivation", actor.arc.name);
                                reason = string.Format("<b>{0}, {1}{2}{3}, is upset at being left in the Reserves</b>", actor.actorName, colourAlert, actor.arc.name, colourEnd);
                                warning = string.Format("{0} Motivation -1", actor.actorName);
                                GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Loses Motivation", reason, warning);
                                //random message
                                Debug.LogFormat("[Rnd] ActorManager.cs -> UpdateReserveActor: Unhappy {0} Motivation check SUCCESS need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                                msgText = string.Format("Unhappy {0} Motivation check SUCCESS", actor.arc.name);
                                GameManager.instance.messageScript.GeneralRandom(msgText, "Lose Motivation", chance, rnd, true);
                            }
                            else
                            {
                                //unhappy  warning
                                msgText = string.Format("{0}, {1}, in Reserves, is UNHAPPY", actor.actorName, actor.arc.name);
                                itemText = string.Format("Reserve {0} is UNHAPPY", actor.arc.name);
                                reason = string.Format("<b>{0}, {1}{2}{3}, is upset at being left in the Reserves</b>", actor.actorName, colourAlert, actor.arc.name, colourEnd);
                                warning = string.Format("{0} will act on their displeasure {1}<b>SOON</b>{2}", actor.actorName, colourBad, colourEnd);
                                GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Unhappy", reason, warning);
                                //random message
                                Debug.LogFormat("[Rnd] ActorManager.cs -> UpdateReserveActor: Unhappy {0} Motivation check FAILED need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                                msgText = string.Format("Unhappy {0} Motivation check FAILED", actor.arc.name);
                                GameManager.instance.messageScript.GeneralRandom(msgText, "Lose Motivation", chance, rnd, true);
                            }
                        }
                        else
                        {
                            //actor is Unhappy and has 0 motivation. Do they take action?
                            chance = unhappyTakeActionChance;
                            string situation = "";
                            //if actor has previously been promised or reassured, double chance of action (4X if both promised and reassured)
                            if (actor.isPromised == true)
                            { chance *= 2; situation = "Promised"; }
                            if (actor.isReassured == true)
                            {
                                chance *= 2;
                                if (situation.Length > 0) { situation += ", Reassured"; }
                                else { situation = "Reassured"; }
                            }
                            rnd = Random.Range(0, 100);
                            if (rnd < chance)
                            {
                                //random message
                                Debug.LogFormat("[Rnd] ActorManager.cs -> UpdateReserveActor: Unhappy {0} Action check SUCCESS need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                                msgText = string.Format("Unhappy {0} Action check SUCCESS {1}", actor.arc.name, situation.Length > 0 ? "(" + situation + ")" : "");
                                GameManager.instance.messageScript.GeneralRandom(msgText, "Take Action", chance, rnd, true);
                                //action taken
                                Debug.Log(string.Format("CheckReserveActors: Resistance {0} {1} takes ACTION {2}", actor.arc.name, actor.actorName, "\n"));
                                TakeAction(actor);
                            }
                            else
                            {
                                //random message
                                Debug.LogFormat("[Rnd] ActorManager.cs -> UpdateReserveActor: Unhappy {0} Action check FAILED need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                                msgText = string.Format("Unhappy {0} Action check FAILED {1}", actor.arc.name, situation.Length > 0 ? "(" + situation + ")" : "");
                                GameManager.instance.messageScript.GeneralRandom(msgText, "Take Action", chance, rnd, true);
                                //actor about to take action warning
                                msgText = string.Format("{0}, {1}, in Reserves, is about to Act", actor.actorName, actor.arc.name);
                                itemText = string.Format("{0}, in Reserves, will ACT", actor.arc.name);
                                reason = string.Format("{0}, {1}{2}{3}, is upset at being left in the Reserves", actor.actorName, colourAlert, actor.arc.name, colourEnd);
                                warning = string.Format("{0} is about to {1}<b>TAKE DECISIVE ACTION</b>{2}", actor.actorName, colourBad, colourEnd);
                                GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "On the Edge", reason, warning);
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
                        if (actor.unhappyTimer == 1)
                        {
                            //unhappy in one turn warning
                            msgText = string.Format("{0}, {1}, in Reserves, will be Unhappy in 1 turn", actor.actorName, actor.arc.name);
                            itemText = string.Format("Reserve {0} about to become Unhappy", actor.arc.name);
                            reason = string.Format("{0}, {1}{2}{3}, is upset at being left in the Reserves", actor.actorName, colourAlert, actor.arc.name, colourEnd);
                            warning = "An <b>UNHAPPY</b> subordinate is unpredictable";
                            GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Unhappy next turn", reason, warning, false);
                        }
                        //if timer now zero, gain condition "Unhappy" 
                        if (actor.unhappyTimer == 0)
                        {
                            Condition condition = GameManager.instance.dataScript.GetCondition("UNHAPPY");
                            actor.AddCondition(condition, string.Format("{0} is upset at being left in the Reserves", actor.arc.name));
                        }
                    }
                    else
                    {
                        //unhappy timer has reached zero. Is actor's motivation > 0?
                        if (actor.GetDatapoint(ActorDatapoint.Motivation1) > 0)
                        {
                            //chance of decrementing motivation each turn till it reaches zero
                            chance = unhappyLoseMotivationChance;
                            //chance is 100% if actor was promised
                            if (actor.isPromised == true)
                            { chance = 100; }
                            rnd = Random.Range(0, 100);
                            if (rnd < chance)
                            {
                                int motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                                motivation--;
                                actor.SetDatapoint(ActorDatapoint.Motivation1, motivation, "Unhappy in Reserves");
                                Debug.LogFormat("CheckReserveActors: Authority {0} {1} UNHAPPY, Motivation now {2}, chance {3}{4}", actor.arc.name,
                                    actor.actorName, motivation, chance, "\n");
                                //lost motivation warning
                                msgText = string.Format("{0}, {1}, in Reserves, has lost Motivation", actor.actorName, actor.arc.name);
                                itemText = string.Format("Reserve {0} loses Motivation", actor.arc.name);
                                reason = string.Format("<b>{0}, {1}{2}{3}, is upset at being left in the Reserves</b>", actor.actorName, colourAlert, actor.arc.name, colourEnd);
                                warning = string.Format("{0} <b>Motivation -1</b>", actor.actorName);
                                GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Loses Motivation", reason, warning);
                                //random message
                                Debug.LogFormat("[Rnd] ActorManager.cs -> UpdateReserveActor: Unhappy {0} Motivation check SUCCESS need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                                msgText = string.Format("Unhappy {0} Motivation check SUCCESS", actor.arc.name);
                                GameManager.instance.messageScript.GeneralRandom(msgText, "Lose Motivation", chance, rnd, true);
                            }
                            else
                            {
                                //unhappy  warning
                                msgText = string.Format("{0}, {1}, in Reserves, is UNHAPPY", actor.actorName, actor.arc.name);
                                itemText = string.Format("Reserve {0} is UNHAPPY", actor.arc.name);
                                reason = string.Format("<b>{0}, {1}{2}{3}, is upset at being left in the Reserves</b>", actor.actorName, colourAlert, actor.arc.name, colourEnd);
                                warning = string.Format("{0} is threatening action", actor.actorName);
                                GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Unhappy", reason, warning);
                                //random message
                                Debug.LogFormat("[Rnd] ActorManager.cs -> UpdateReserveActor: Unhappy {0} Motivation check FAILED need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                                msgText = string.Format("Unhappy {0} Motivation check FAILED", actor.arc.name);
                                GameManager.instance.messageScript.GeneralRandom(msgText, "Lose Motivation", chance, rnd, true);
                            }
                        }
                        else
                        {
                            //actor is Unhappy and has 0 motivation. Do they take action?
                            chance = unhappyTakeActionChance;
                            //if actor has previously been promised or reassured, double chance of action (4X if both promised and reassured)
                            if (actor.isPromised == true)
                            { chance *= 2; }
                            if (actor.isReassured == true)
                            { chance *= 2; }
                            rnd = Random.Range(0, 100);
                            if (rnd < chance)
                            {
                                //random message
                                Debug.LogFormat("[Rnd] ActorManager.cs -> UpdateReserveActor: Unhappy {0} Action check SUCCESS need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                                msgText = string.Format("Unhappy {0} Action check SUCCESS", actor.arc.name);
                                GameManager.instance.messageScript.GeneralRandom(msgText, "Take Action", chance, rnd, true);
                                //action taken
                                Debug.Log(string.Format("CheckReserveActors: Authority {0} {1} takes ACTION, chance {2}{3}", actor.arc.name, actor.actorName, chance, "\n"));
                                TakeAction(actor);
                            }
                            else
                            {
                                //random message
                                Debug.LogFormat("[Rnd] ActorManager.cs -> UpdateReserveActor: Unhappy {0} Action check FAILED need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                                msgText = string.Format("Unhappy {0} Action check FAILED", actor.arc.name);
                                GameManager.instance.messageScript.GeneralRandom(msgText, "Take Action", chance, rnd, true);
                                //actor about to take action warning
                                msgText = string.Format("{0}, {1}, in Reserves, is about to ACT", actor.actorName, actor.arc.name);
                                itemText = string.Format("Reserve {0} is about to Act", actor.arc.name);
                                reason = string.Format("{0}, {1}{2}{3}, is upset at being left in the Reserves", actor.actorName, colourAlert, actor.arc.name, colourEnd);
                                warning = string.Format("{0} is about to <b>TAKE DECISIVE ACTION</b>", actor.actorName);
                                GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "On the Edge", reason, warning);
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
        int rnd, chance;
        string msgText, itemText, reason;
        //
        // - - - Reveal Secret (check first)
        //
        if (actor.CheckNumOfSecrets() > 0)
        {
            rnd = Random.Range(0, 100);
            chance = unhappyRevealSecretChance;
            if (rnd < chance)
            {
                //random message
                Debug.LogFormat("[Rnd] ActorManager.cs -> TakeAction: Unhappy {0} Reveal Secret SUCCESS need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                msgText = string.Format("Unhappy {0} Reveal Secret SUCCESS", actor.arc.name);
                GameManager.instance.messageScript.GeneralRandom(msgText, "Reveal Secret", chance, rnd, true);

                Secret secret = actor.GetSecret();
                if (secret != null)
                {
                    secret.revealedID = actor.actorID;
                    secret.revealedWho = string.Format("{0}, {1}", actor.actorName, actor.arc.name);
                    secret.revealedWhen = GameManager.instance.turnScript.Turn;
                    secret.status = gameAPI.SecretStatus.Revealed;
                    //carry out effects
                    if (secret.listOfEffects != null)
                    {
                        //data packages
                        /*EffectDataReturn effectReturn = new EffectDataReturn();*/
                        EffectDataInput effectInput = new EffectDataInput();
                        effectInput.originText = "Reveal Secret";
                        Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                        if (node != null)
                        {
                            //loop effects
                            foreach (Effect effect in secret.listOfEffects)
                            { GameManager.instance.effectScript.ProcessEffect(effect, node, effectInput); }
                        }
                        else { Debug.LogWarning("Invalid player node (Null)"); }
                    }
                    //remove secret from all actors and player
                    GameManager.instance.secretScript.RemoveSecretFromAll(secret.name);
                    //message
                    GameManager.instance.messageScript.ActorRevealSecret(string.Format("{0} Reveals your SECRET", actor.arc.name), actor, secret, "Unhappy at being left in Reserve");
                }
                else { Debug.LogWarning("Invalid Secret (Null) -> Not revealed"); }

                return;
            }
            else
            {
                //random message
                Debug.LogFormat("[Rnd] ActorManager.cs -> TakeAction: Unhappy {0} Reveal Secret FAILED need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                msgText = string.Format("Unhappy {0} Reveal Secret FAILED", actor.arc.name);
                GameManager.instance.messageScript.GeneralRandom(msgText, "Reveal Secret", chance, rnd, true);
            }
        }
        //
        // - - -  Resign (check second)
        //
        if (actor.isComplaining == false)
        {
            rnd = Random.Range(0, 100);
            chance = unhappyResignChance;
            if (rnd < chance)
            {
                //random message
                Debug.LogFormat("[Rnd] ActorManager.cs -> TakeAction: Unhappy {0} Leave SUCCESS need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                msgText = string.Format("Unhappy {0} Leave SUCCESS", actor.arc.name);
                GameManager.instance.messageScript.GeneralRandom(msgText, "Leave", chance, rnd, true);
                //resigns in frustration
                if (GameManager.instance.dataScript.RemoveCurrentActor(GameManager.instance.sideScript.PlayerSide, actor, ActorStatus.Resigned) == true)
                {
                    //remove from reserve pool
                    GameManager.instance.dataScript.RemoveActorFromReservePool(GameManager.instance.sideScript.PlayerSide, actor);
                    //change status message
                    msgText = string.Format("{0} resigns in disgust at being left in Reserves", actor.arc.name);
                    itemText = "resigns in disgust";
                    reason = string.Format("{0}<b>was Unhappy at being left in Reserves</b>{1}", colourBad, colourEnd);
                    GameManager.instance.messageScript.ActorStatus(msgText, itemText, reason, actor.actorID, GameManager.instance.sideScript.PlayerSide);
                }
                else { Debug.LogWarningFormat("Actor, {0}, {1}, ID {2} didn't resign", actor.actorName, actor.arc.name, actor.actorID); }
                return;
            }
            else
            {
                //random message
                Debug.LogFormat("[Rnd] ActorManager.cs -> TakeAction: Unhappy {0} Leave FAILED need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                msgText = string.Format("Unhappy {0} Leave FAILED", actor.arc.name);
                GameManager.instance.messageScript.GeneralRandom(msgText, "Leave", chance, rnd, true);
            }
        }
        else
        {
            rnd = Random.Range(0, 100);
            chance = unhappyResignChance * 2;
            //double chance if actor has already complained
            if (rnd < chance)
            {
                //random message
                Debug.LogFormat("[Rnd] ActorManager.cs -> TakeAction: Unhappy {0} Leave  (already Complained) SUCCESS need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                msgText = string.Format("Unhappy {0} Leave (already Complained) SUCCESS", actor.arc.name);
                GameManager.instance.messageScript.GeneralRandom(msgText, "Leave", chance, rnd, true);

                //resigns in frustration
                if (GameManager.instance.dataScript.RemoveCurrentActor(GameManager.instance.sideScript.PlayerSide, actor, ActorStatus.Resigned) == true)
                {
                    //remove from reserve pool
                    GameManager.instance.dataScript.RemoveActorFromReservePool(GameManager.instance.sideScript.PlayerSide, actor);
                    //change status message
                    msgText = string.Format("{0} resigns in disgust at being left in Reserves", actor.arc.name);
                    itemText = "resigns in disgust";
                    reason = string.Format("{0}<b>was Unhappy at being left in Reserves</b>{1}", colourBad, colourEnd);
                    GameManager.instance.messageScript.ActorStatus(msgText, itemText, reason, actor.actorID, GameManager.instance.sideScript.PlayerSide);
                }
                return;
            }
            else
            {
                //random message
                Debug.LogFormat("[Rnd] ActorManager.cs -> TakeAction: Unhappy {0} Leave (already Complained) FAILED need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                msgText = string.Format("Unhappy {0} Leave (already Complained) FAILED", actor.arc.name);
                GameManager.instance.messageScript.GeneralRandom(msgText, "Leave (Complained)", chance, rnd, true);
            }
        }
        //
        // - - - Complain (check last)
        //
        if (actor.isComplaining == false)
        {
            rnd = Random.Range(0, 100);
            chance = unhappyComplainChance;
            if (rnd < chance)
            {
                //random message
                Debug.LogFormat("[Rnd] ActorManager.cs -> TakeAction: Unhappy {0} Complain SUCCESS need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                msgText = string.Format("Unhappy {0} Complain SUCCESS", actor.arc.name);
                GameManager.instance.messageScript.GeneralRandom(msgText, "Complain", chance, rnd, true);
                msgText = string.Format("{0} Complains about being left in Reserves", actor.arc.name);
                GameManager.instance.messageScript.ActorComplains(msgText, actor, "Unhappy at being left in Reserve", "Higher chance of Resigning or Revealing a Secret");
                actor.isComplaining = true;
                return;
            }
            else
            {
                //random message
                Debug.LogFormat("[Rnd] ActorManager.cs -> TakeAction: Unhappy {0} Complain FAILED need < {1}, rolled {2}{3}", actor.arc.name, chance, rnd, "\n");
                msgText = string.Format("Unhappy {0} Complain FAILED", actor.arc.name);
                GameManager.instance.messageScript.GeneralRandom(msgText, "Complain", chance, rnd, true);
            }
        }
        //
        // - - - NO Action take -> WARNING message
        //
        msgText = string.Format("{0}, {1}, in Reserves, Failed to Act, but will", actor.actorName, actor.arc.name);
        itemText = string.Format("Reserve {0} Failed to Act, but will", actor.arc.name);
        reason = string.Format("{0}, {1}{2}{3}, is upset at being left in the Reserves", actor.actorName, colourAlert, actor.arc.name, colourEnd);
        string warning = string.Format("{0} will {1}<b>TAKE DECISIVE ACTION</b>{2}, but didn't this turn", actor.actorName, colourBad, colourEnd);
        GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Taking Action", reason, warning);
    }

    /// <summary>
    /// Generates a log message indicating use of a trait. Output format is  "Actor uses x trait' 
    /// 'forText' (self-contained), eg.'for an attempt on a Target', 'toText (self-Contained), eg. 'to CANCEL attempt due to Security Measures' (Key word in CAPS, usually verb)
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="skillCheck"></param>
    public void TraitLogMessage(Actor actor, string forText, string toText)
    {
        if (actor != null)
        {
            Trait trait = actor.GetTrait();
            if (trait != null)
            {
                if (string.IsNullOrEmpty(toText) == false)
                {
                    if (string.IsNullOrEmpty(forText) == false)
                    {
                        Debug.LogFormat(string.Format("[Trt] {0}, {1}, uses \"{2}\" trait {3}", actor.actorName, actor.arc.name, trait.tag, toText));
                        //message
                        string msgText = string.Format("{0}, {1}, uses \"{2}\" trait {3}", actor.actorName, actor.arc.name, actor.GetTrait().tag, toText);
                        GameManager.instance.messageScript.ActorTrait(msgText, actor, trait, forText, toText);
                    }
                    else { Debug.LogWarning("Invalid forText (Null or Empty)"); }
                }
                else { Debug.LogWarning("Invalid skillCheck parameter (Null or Empty)"); }
            }
            else { Debug.LogWarning("Invalid trait (Null)"); }
        }
        else { Debug.LogWarning("Invalid actor (Null)"); }
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
                case ActorTooltip.Captured:
                    data.header = string.Format("{0}{1}{2}{3}{4}", colourSide, actor.arc.name, colourEnd, "\n", actor.actorName);
                    data.main = string.Format("{0}<size=120%>Currently{1} {2}CAPTURED{3}{4} and unavailable</size>{5}", colourNormal, colourEnd,
                        colourBad, colourEnd, colourNormal, colourEnd);
                    data.details = string.Format("{0}{1}'s future is in the hands of the Authority{2}", colourBad, actor.actorName, colourEnd);
                    break;
                case ActorTooltip.Leave:
                    data.header = string.Format("{0}{1}{2}{3}{4}", colourSide, actor.arc.name, colourEnd, "\n", actor.actorName);
                    data.main = string.Format("{0}<size=120%>On{1} {2}STRESS LEAVE{3}{4} and unavailable</size>{5}", colourNormal, colourEnd,
                        colourNeutral, colourEnd, colourNormal, colourEnd);
                    data.details = string.Format("{0}{1} is expected to return, free of Stress, shortly{2}", colourAlert, actor.actorName, colourEnd);
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
        data.header = string.Format("{0}<size=120%>Position Vacant</size>{1}", colourBad, colourEnd);
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
        data.main = string.Format("{0}Is a measure of how well known a person is within the Organisation{1}", colourNormal, colourEnd);
        /*data.details = string.Format("{0}Somebody with high renown gains {1}{2}influential friends{3}{4} and is harder to {5}{6}Dismiss{7}{8} or {9}{10}Dispose Off{11}",
            colourAlert, colourEnd, colourNeutral, colourEnd, colourAlert, colourEnd, colourBad, colourEnd, colourAlert, colourEnd, colourBad, colourEnd);*/
        data.details = string.Format("{0}The higher a Subordinates Renown, the greater their chance of joining HQ. If Promoted they will be your friends, if Dismissed, your enemies{1}", colourAlert, colourEnd);
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
        data.details = string.Format("{0}Renown is the currency you use to do things. It represents money, goodwill, accrued favours and reputation{1}", colourNormal, colourEnd);
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
            case ActorTooltip.Captured:
                data.header = string.Format("{0}Player{1}{2}{3}", colourSide, colourEnd, "\n", GameManager.instance.playerScript.PlayerName);
                data.main = string.Format("{0}<size=120%>Currently{1} {2}CAPTURED{3}{4} and unavailable</size>{5}", colourNormal, colourEnd,
                    colourBad, colourEnd, colourNormal, colourEnd);
                data.details = string.Format("{0}{1}'s future is in the hands of the Authority{2}", colourBad, GameManager.instance.playerScript.PlayerName, colourEnd);
                break;
            case ActorTooltip.Leave:
                data.header = string.Format("{0}Player{1}{2}{3}", colourSide, colourEnd, "\n", playerName);
                data.main = string.Format("{0}<size=120%>On{1} {2}STRESS LEAVE{3}{4} and unavailable</size>{5}", colourNormal, colourEnd,
                    colourNeutral, colourEnd, colourNormal, colourEnd);
                data.details = string.Format("{0}{1} is expected to return, free of Stress, shortly{2}", colourAlert, playerName, colourEnd);
                break;
            default:
                data.main = "Unknown"; data.header = "Unknown"; data.details = "Unknown";
                break;
        }
        return data;
    }


    /// <summary>
    /// sub method used to calculate adjusted renown cost for dismissing, firing , disposing off actors (where a cost is involved) due to actor threatening player and/or knowing secrets
    /// Returns a ManageRenown data package giving adjusted cost and a colour formatted tooltip string (starts on next line) explaining why, eg. '(HEAVY knows 1 secret, +1 Renown cost)'
    /// Returns baseCost and null for tooltip if no change
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="baseCost"></param>
    /// <returns></returns>
    public ManageRenownCost GetManageRenownCost(Actor actor, int baseCost)
    {
        ManageRenownCost manageRenown = new ManageRenownCost();
        //default values returned
        manageRenown.tooltip = "";
        manageRenown.renownCost = baseCost;
        //proceed with a valid actor
        if (actor != null)
        {
            //how many secrets does actor know? (depends if has already been removed or not)
            int numOfSecrets = 0;
            switch (actor.Status)
            {
                case ActorStatus.Active:
                case ActorStatus.Inactive:
                case ActorStatus.Reserve:
                    numOfSecrets = actor.CheckNumOfSecrets();
                    break;
                case ActorStatus.Dismissed:
                case ActorStatus.Killed:
                    //NOTE: use departedNumOfSecrets instead of CheckNumOfSecrets as all secrets are removed by DataManager.cs -> RemoveActorAdmin
                    numOfSecrets = actor.departedNumOfSecrets;
                    break;
            }
            int extraSecretCost = 0;
            //calculate adjusted renown cost
            if (numOfSecrets > 0)
            {
                extraSecretCost = numOfSecrets * manageSecretRenown;
                manageRenown.renownCost += extraSecretCost;
            }
            if (actor.isThreatening == true) { manageRenown.renownCost *= 2; }
            //generate tooltip
            StringBuilder builder = new StringBuilder();
            if (numOfSecrets > 0)
            {
                builder.AppendLine();
                builder.AppendFormat("{0}({1} knows {2} secret{3}, +{4} Renown cost){5}", colourBad, actor.arc.name, numOfSecrets,
                    numOfSecrets != 1 ? "s" : "", extraSecretCost, colourEnd);
            }
            if (actor.isThreatening == true)
            {
                builder.AppendLine();
                builder.AppendFormat("{0}(Double Renown cost as {1} is Threatening you){2}", colourBad, actor.arc.name, colourEnd);
            }
            //traits -> Connected / Unconnected (done last as effect is a global multiplier of all other effect)
            if (actor.CheckTraitEffect(actorRemoveActionDoubled) == true)
            {
                manageRenown.renownCost *= 2;
                builder.AppendLine();
                builder.AppendFormat("{0}(Double overall Renown cost as {1} has {2}{3}{4}{5}{6} trait){7}", colourBad, actor.arc.name, colourEnd, colourAlert, actor.GetTrait().tag, colourEnd,
                    colourBad, colourEnd);
            }
            if (actor.CheckTraitEffect(actorRemoveActionHalved) == true)
            {
                manageRenown.renownCost /= 2;
                builder.AppendLine();
                builder.AppendFormat("{0}(Halved overall Renown cost as {1} has {2}{3}{4}{5}{6} trait){7}", colourGood, actor.arc.name, colourEnd, colourAlert, actor.GetTrait().tag, colourEnd,
                    colourGood, colourEnd);
            }
            //tooltip
            manageRenown.tooltip = builder.ToString();
        }
        else { Debug.LogWarning("Invalid Actor (Null)"); }
        return manageRenown;
    }

    /// <summary>
    /// resets cooldown timer everytime a lie low action is used
    /// </summary>
    public void SetLieLowTimer()
    { lieLowTimer = lieLowCooldownPeriod; }


    public void SetDoomTimer()
    { doomTimer = playerDoomTimerValue; }

    public void StopDoomTimer()
    { doomTimer = 0; }

    /// <summary>
    /// checks for possibility of the Resistance Player/Leader being betrayed by traitorous minions or anonymous haters in RebelHQ
    /// </summary>
    /// <param name="side"></param>
    /// <param name="numOfTraitors"></param>
    private void CheckForBetrayal(int numOfTraitors)
    {
        bool isProceed = true;
        //only if Resistance Player Active
        switch (GameManager.instance.sideScript.resistanceOverall)
        {
            case SideState.AI:
                if (GameManager.instance.aiRebelScript.status != ActorStatus.Active)
                { isProceed = false; }
                break;
            case SideState.Human:
                if (GameManager.instance.playerScript.status != ActorStatus.Active)
                { isProceed = false; }
                break;
            default:
                Debug.LogErrorFormat("Unrecognised Side State for resistanceOverall \"{0}\"", GameManager.instance.sideScript.resistanceOverall);
                break;
        }
        if (isProceed == true)
        {
            string text;
            int chance = traitorActiveChance + (traitorActiveChance * numOfTraitors);
            int rndNum = Random.Range(0, 100);
            if (rndNum < chance)
            {
                //Betrayal
                Debug.LogFormat("[Rnd] ActorManager.cs -> CheckForBetrayal: Resistance Leader BETRAYED (need {0}, rolled {1}){2}", chance, rndNum, "\n");
                int invisibility = GameManager.instance.playerScript.Invisibility;
                //statistic
                GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerBetrayed);
                //immediate notification if invis 0
                Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                if (node != null)
                {
                    if (invisibility == 0)
                    {
                        GameManager.instance.aiScript.immediateFlagResistance = true;
                        //AI Immediate message
                        GameManager.instance.messageScript.AIImmediateActivity("Immediate Activity \"BETRAYED\" (Player)", "Betrayed (subordinate or RebelHQ)", node.nodeID, -1);
                    }
                    //lower invisibility by -1
                    GameManager.instance.playerScript.Invisibility--;
                    text = string.Format("{0}, Resistance Leader, BETRAYED at {0}, {1}, ID {2}{3}", GameManager.instance.playerScript.PlayerName, node.nodeName, node.Arc.name, node.nodeID);
                    GameManager.instance.messageScript.PlayerBetrayed(text, true);
                }
                else { Debug.LogErrorFormat("Invalid node (Null) for playerNodeID {0}", GameManager.instance.nodeScript.nodePlayer); }
            }
            else { Debug.LogFormat("[Rnd] ActorManager.cs -> CheckForBetrayal: Resistance Leader Not Betrayed (need {0}, rolled {1}){2}", chance, rndNum, "\n"); }
        }
    }

    /// <summary>
    /// add a new OnMap actor
    /// </summary>
    /// <param name="side"></param>
    /// <param name="node"></param>
    public void AddNewActorOnMapAI(GlobalSide side, Node node, int slotID)
    {
        if (side != null)
        {
            if (node != null)
            {
                if (slotID > -1)
                {
                    //
                    // - - - Select Actor
                    //
                    List<int> listOfPoolActors = new List<int>();
                    List<string> listOfCurrentArcs = new List<string>(GameManager.instance.dataScript.GetAllCurrentActorArcs(side));
                    if (listOfCurrentArcs != null)
                    {
                        if (listOfCurrentArcs.Count > 0)
                        {
                            if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                            {
                                //player at node, select from 3 x level 1 options, different from current OnMap actor types
                                listOfPoolActors.AddRange(GameManager.instance.dataScript.GetActorRecruitPool(1, side));
                            }
                            else
                            {
                                //actor at node, select from 3 x level 2 options (random types, could be the same as currently OnMap)
                                listOfPoolActors.AddRange(GameManager.instance.dataScript.GetActorRecruitPool(2, side));
                            }
                            if (listOfPoolActors.Count > 0)
                            {
                                //loop backwards through pool of actors (do for both) and remove any that match the curent OnMap types
                                for (int i = listOfPoolActors.Count - 1; i >= 0; i--)
                                {
                                    Actor actorTemp = GameManager.instance.dataScript.GetActor(listOfPoolActors[i]);
                                    if (actorTemp != null)
                                    {
                                        if (listOfCurrentArcs.Count > 0)
                                        {
                                            if (listOfCurrentArcs.Exists(x => x == actorTemp.arc.name))
                                            { listOfPoolActors.RemoveAt(i); }
                                        }
                                    }
                                    else { Debug.LogWarning(string.Format("Invalid actor (Null) for actorID {0}", listOfPoolActors[i])); }
                                }
                                //actors present
                                if (listOfPoolActors.Count > 0)
                                {
                                    //randomly select an actor
                                    int actorID = listOfPoolActors[Random.Range(0, listOfPoolActors.Count)];
                                    Actor actorNew = GameManager.instance.dataScript.GetActor(actorID);
                                    if (actorNew != null)
                                    {
                                        //
                                        // - - - Add Actor
                                        //
                                        GameManager.instance.dataScript.RemoveActorFromPool(actorNew.actorID, actorNew.level, side);
                                        //place actor on Map (reset states)
                                        GameManager.instance.dataScript.AddCurrentActor(side, actorNew, slotID);
                                        //stats
                                        GameManager.instance.dataScript.StatisticIncrement(StatType.ActorsRecruited);
                                        //admin
                                        Debug.LogFormat("[Rim] ActorManager.cs -> AddNewActorOnMapAI: {0}, {1}, ID {2} RECRUITED{3}", actorNew.actorName, actorNew.arc.name, actorNew.actorID, "\n");
                                        string textAutoRun = string.Format("{0}{1}{2} {3}Recruited{4}", colourAlert, actorNew.arc.name, colourEnd, colourGood, colourEnd);
                                        GameManager.instance.dataScript.AddHistoryAutoRun(textAutoRun);
                                    }
                                    else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", actorID); }
                                }
                                else { Debug.LogFormat("[Rim] ActorManager.cs -> AddNewActorOnMapAI: No actors available to add to OnMap roster{0}", "\n"); }
                            }
                            else { Debug.LogFormat("[Rim] ActorManager.cs -> AddNewActorOnMapAI: NO SUITABLE RECRUITS AVAILABLE{0}", "\n"); }
                        }
                        else { Debug.LogWarning("Info: ListOfCurrentActorArcs (Empty) ->  No actors present"); }
                    }
                    else { Debug.LogError("Invalid listOfCurrentArcs (Null)"); }
                }
                else { Debug.LogError("Invalid slotID (less than Zero)"); }
            }
            else { Debug.LogError("Invalid node (Null)"); }
        }
        else { Debug.LogError("Invalid side (Null)"); }
    }


    /// <summary>
    /// Add a new Reserve actor. NOTE: Reserve Pool is assumed to be empty
    /// </summary>
    public void AddNewActorReserveAI(GlobalSide side)
    {
        int unhappyTimer = recruitedReserveTimer;
        //get new actor (50/50 chance of level 1 or 2)
        int actorLevel = Random.Range(1, 2);
        List<int> listOfPoolActors = new List<int>();
        listOfPoolActors.AddRange(GameManager.instance.dataScript.GetActorRecruitPool(actorLevel, side));
        //actors present
        if (listOfPoolActors.Count > 0)
        {
            //randomly select an actor
            int actorID = listOfPoolActors[Random.Range(0, listOfPoolActors.Count)];
            Actor actor = GameManager.instance.dataScript.GetActor(actorID);
            if (actor != null)
            {
                //
                // - - - Add Actor
                //
                GameManager.instance.dataScript.RemoveActorFromPool(actor.actorID, actor.level, side);
                //place actor in Reserves
                if (GameManager.instance.dataScript.AddActorToReserve(actor.actorID, side) == true)
                {
                    //traits that affect unhappy timer
                    if (actor.CheckTraitEffect(actorReserveTimerDoubled) == true)
                    {
                        unhappyTimer *= 2;
                        TraitLogMessage(actor, "for their willingness to wait", "to DOUBLE Reserve Unhappy Timer");
                    }
                    else if (actor.CheckTraitEffect(actorReserveTimerHalved) == true)
                    {
                        unhappyTimer /= 2; unhappyTimer = Mathf.Max(1, unhappyTimer);
                        TraitLogMessage(actor, "for their reluctance to wait", "to HALVE Reserve Unhappy Timer");
                    }
                    //change actor's status
                    actor.Status = ActorStatus.Reserve;
                    //remove actor from appropriate pool list
                    GameManager.instance.dataScript.RemoveActorFromPool(actor.actorID, actor.level, side);
                    //initiliase unhappy timer
                    actor.unhappyTimer = unhappyTimer;
                    actor.isNewRecruit = true;
                    //stats
                    GameManager.instance.dataScript.StatisticIncrement(StatType.ActorsRecruited);
                }
                else { Debug.LogWarningFormat("Actor unable to be added to Reserve Pool"); }
                //admin
                Debug.LogFormat("[Rim] ActorManager.cs -> AddNewActorReserveAI: {0}, {1}, ID {2} added to RESERVE POOL{3}", actor.actorName, actor.arc.name, actor.actorID, "\n");
                string textAutoRun = string.Format("{0}{1}{2} {3}Reserves{4}", colourAlert, actor.arc.name, colourEnd, colourNeutral, colourEnd);
                GameManager.instance.dataScript.AddHistoryAutoRun(textAutoRun);
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", actorID); }
        }
        else { Debug.LogFormat("[Rim] ActorManager.cs -> AddNewActorReserveAI: No actors available to add to Reserve Pool{0}", "\n"); }
    }

    /// <summary>
    /// AI fires an actor, returns true if successful
    /// </summary>
    /// <param name="side"></param>
    /// <param name="actorID"></param>
    public bool DismissActorAI(GlobalSide side, Actor actor)
    {
        //Dismiss actor
        if (GameManager.instance.dataScript.RemoveCurrentActor(globalResistance, actor, ActorStatus.Dismissed) == true)
        {
            //admin
            Debug.LogFormat("[Rim] ActorManager.cs -> DismissActorAI: {0}, {1}, ID {2}, DISMISSED (Questionable loyalty){3}", actor.actorName, actor.arc.name, actor.actorID, "\n");
            string textAutoRun = string.Format("{0}{1}{2} {3}Fired{4}", colourAlert, actor.arc.name, colourEnd, colourBad, colourEnd);
            GameManager.instance.dataScript.AddHistoryAutoRun(textAutoRun);
            return true;
        }
        return false;
    }

    /// <summary>
    /// handles all Meta (between level) game actor matters
    /// </summary>
    public void ProcessMetaActors()
    {
        int count, rndNum;
        bool isProceed = true;
        int numToHQ = 0;
        //check lists of actors at end of level to see if any get transferred to the reserves -> Promoted
        List<int> listOfPromotedActors = GameManager.instance.dataScript.GetListOfPromotedActors(GameManager.instance.sideScript.PlayerSide);
        if (listOfPromotedActors != null)
        {
            count = listOfPromotedActors.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Actor actor = GameManager.instance.dataScript.GetActor(listOfPromotedActors[i]);
                    if (actor != null)
                    {
                        //add to HQ
                        rndNum = Random.Range(0, 100);
                        if (rndNum < chanceOfPromotedToHQ)
                        {
                            AddActorToHQ(actor, "after being Promoted");
                            numToHQ++;
                            if (numToHQ >= maxActorsToHQ) { isProceed = false; break; }
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid actor (Null) in listOfPromotedActors for actorID {0}", listOfPromotedActors[i]); }
                }
            }
        }
        else { Debug.LogWarning("Invalid listOfPromotedActors (Null)"); }
        //haven't yet reached max cap on actors to HQ
        if (isProceed == true)
        {
            //Resigned
            List<int> listOfResignedActors = GameManager.instance.dataScript.GetListOfResignedActors(GameManager.instance.sideScript.PlayerSide);
            if (listOfResignedActors != null)
            {
                count = listOfResignedActors.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Actor actor = GameManager.instance.dataScript.GetActor(listOfResignedActors[i]);
                        if (actor != null)
                        {
                            //add to HQ 
                            rndNum = Random.Range(0, 100);
                            if (rndNum < chanceOfResignedToHQ)
                            {
                                AddActorToHQ(actor, "after Resigning");
                                numToHQ++;
                                if (numToHQ >= maxActorsToHQ) { isProceed = false; break; }
                            }
                        }
                        else { Debug.LogWarningFormat("Invalid actor (Null) in listOfResignedActors for actorID {0}", listOfResignedActors[i]); }
                    }
                }
            }
            else { Debug.LogWarning("Invalid listOfResignedActors (Null)"); }
            //haven't yet reached max cap on actors to HQ
            if (isProceed == true)
            {
                //Dismissed
                List<int> listOfDismissedActors = GameManager.instance.dataScript.GetListOfDismissedActors(GameManager.instance.sideScript.PlayerSide);
                if (listOfDismissedActors != null)
                {
                    count = listOfDismissedActors.Count;
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            Actor actor = GameManager.instance.dataScript.GetActor(listOfDismissedActors[i]);
                            if (actor != null)
                            {
                                //add to HQ 
                                rndNum = Random.Range(0, 100);
                                if (rndNum < chanceOfDismissedToHQ)
                                {
                                    AddActorToHQ(actor, "after being Dismissed");
                                    numToHQ++;
                                    if (numToHQ >= maxActorsToHQ) { isProceed = false; break; }
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid actor (Null) in listOfDismissedActors for actorID {0}", listOfDismissedActors[i]); }
                        }
                    }
                }
                else { Debug.LogWarning("Invalid listOfDismissedActors (Null)"); }
                //haven't yet reached max cap on actors to HQ
                if (isProceed == true)
                {
                    //OnMap
                    Actor[] arrayOfCurrentActors = GameManager.instance.dataScript.GetCurrentActors(GameManager.instance.sideScript.PlayerSide);
                    if (arrayOfCurrentActors != null)
                    {
                        count = arrayOfCurrentActors.Length;
                        if (count > 0)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                Actor actor = arrayOfCurrentActors[i];
                                if (actor != null)
                                {
                                    //add to HQ 
                                    rndNum = Random.Range(0, 100);
                                    if (rndNum < chanceOfOnMapToHQ)
                                    {
                                        AddActorToHQ(actor, "from Active Duty");
                                        numToHQ++;
                                        if (numToHQ >= maxActorsToHQ) { isProceed = false; break; }
                                    }
                                }
                            }
                        }
                    }
                    else { Debug.LogError("Invalid arrayOfCurrentActors (Null)"); }
                    //haven't yet reached max cap on actors to HQ
                    if (isProceed == true)
                    {
                        //Reserves
                        List<int> listOfReserveActors = GameManager.instance.dataScript.GetListOfReserveActors(GameManager.instance.sideScript.PlayerSide);
                        if (listOfReserveActors != null)
                        {
                            count = listOfReserveActors.Count;
                            if (count > 0)
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    Actor actor = GameManager.instance.dataScript.GetActor(listOfReserveActors[i]);
                                    if (actor != null)
                                    {
                                        //add to HQ 
                                        rndNum = Random.Range(0, 100);
                                        if (rndNum < chanceOfReservesToHQ)
                                        {
                                            AddActorToHQ(actor, "from Reserves");
                                            numToHQ++;
                                            if (numToHQ >= maxActorsToHQ) { isProceed = false; break; }
                                        }
                                    }
                                    else { Debug.LogWarningFormat("Invalid actor (Null) in listOfReserveActors for actorID {0}", listOfReserveActors[i]); }
                                }
                            }
                        }
                        else { Debug.LogWarning("Invalid listOfReserveActors (Null)"); }
                    }
                }
            }
        }
    }

    /// <summary>
    /// handles all admin to transfer a normal actor to HQ (as a worker), string 'reason' is for message moved to HQ [...] ('after being dismissed', 'from active duty', 'after resigning', etc)
    /// NOTE: actor checked for Null by parent method
    /// </summary>
    /// <param name="actor"></param>
    private void AddActorToHQ(Actor actor, string reason)
    {
        actor.Status = ActorStatus.HQ;
        actor.statusHQ = ActorHQ.Worker;
        actor.hqID = hqIDCounter++;
        GameManager.instance.dataScript.AddHQActor(actor);
        GameManager.instance.dataScript.AddActorToHQPool(actor.hqID);
        //admin
        if (string.IsNullOrEmpty(reason) == true) { reason = "for reasons Unknown"; }
        Debug.LogFormat("[Act] ActorManager.cs -> AddActorToHQ: {0}, {1}, actID {2}, hqID {3}, moved to HQ {4}{5}", actor.actorName, actor.arc.name, actor.actorID, actor.hqID, reason, "\n");
    }

    /// <summary>
    /// Checks TestManager.cs for any relevant debug test settings that may require changes to actors (PlayerSide only) at startUp
    /// Can request a particular actor Arc type in the line up and/or a particular slotID actor having specified dataPoint settings
    /// NOTE: No need to deal with Authority preferrred actor teams as this is automatically taken care off by teamManager when teams are initialised (actors swapped before this happens)
    /// </summary>
    private void DebugTest()
    {
        int slotID = GameManager.instance.testScript.actorSlotID;
        if (slotID > -1)
        {
            GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
            //Actor type specified
            if (GameManager.instance.testScript.actorType != null)
            {
                ActorArc arc = GameManager.instance.testScript.actorType;
                //check correct arc for playerSide
                if (arc.side.level == playerSide.level)
                {
                    //check if that type of actor already exists in current line up
                    if (GameManager.instance.dataScript.CheckActorArcPresent(arc, playerSide, true) == false)
                    {
                        //remove existing actor at specified slot. Place in Reserves
                        if (GameManager.instance.dataScript.CheckActorSlotStatus(slotID, playerSide) == true)
                        {
                            Actor currentActor = GameManager.instance.dataScript.GetCurrentActor(slotID, playerSide);
                            if (currentActor != null)
                            {
                                //place current actor in reserves
                                if (GameManager.instance.dataScript.RemoveCurrentActor(playerSide, currentActor, ActorStatus.Reserve) == true)
                                {
                                    //need to set manually as not going through normal processes. Make it large enough that you don't have to worry for a while
                                    currentActor.unhappyTimer = 20;
                                    Debug.LogFormat("[Tst] ActorManager.cs -> DebugTest: {0}, {1}, ID {2} moved to Reserves (Swap Actor){3}",
                                        currentActor.actorName, currentActor.arc.name, currentActor.actorID, "\n");
                                    //select a new actor from Recruit Pool of the specified type and put in OnMap lineUp
                                    Actor actorNew = GameManager.instance.dataScript.GetActorFromRecruitPool(arc, 1, playerSide);
                                    if (actorNew != null)
                                    {
                                        //place actor on Map (reset states and sets up contacts)
                                        GameManager.instance.dataScript.AddCurrentActor(playerSide, actorNew, slotID);
                                        Debug.LogFormat("[Tst] ActorManager.cs -> DebugTest: {0}, {1}, ID {2} added to OnMap lineUp (Swap Actor){3}",
                                            actorNew.actorName, actorNew.arc.name, actorNew.actorID, "\n");
                                    }
                                    else { Debug.LogErrorFormat("Invalid actorNew (Null) for arc {0}, level 1, side {1}", arc.name, playerSide.name); }
                                }
                                else { Debug.LogErrorFormat("Actor {0}, {1}, ID {2} unable to be removed successfully", currentActor.actorName, currentActor.arc.name, currentActor.actorID); }
                            }
                            else { Debug.LogErrorFormat("Invalid actor (Null) for slotID {0}", slotID); }
                        }
                        else { Debug.LogErrorFormat("No actor present in slotID {0}", slotID); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid Arc \"{0}\" (Incorrect arc for playerSide {1})", arc.name, playerSide); }
                Actor actor = GameManager.instance.dataScript.GetCurrentActor(slotID, playerSide);
                if (actor != null)
                {
                    //adjust dataPoints if necessary. Note SetDatapointLoad is used, instead of SetDatapoint, to avoid actor Personality negating change
                    int value = GameManager.instance.testScript.actorDatapoint0;
                    if (value > -1)
                    {
                        actor.SetDatapointLoad(ActorDatapoint.Datapoint0, value);
                        Debug.LogFormat("[Sta] ActorManager.cs -> DebugTest: {0}, {1}, ID {2} {3} set to {4} for Testing{5}", actor.actorName, actor.arc.name, actor.actorID,
                            GameManager.instance.dataScript.GetQuality(playerSide, 0), value, "\n");
                    }
                    value = GameManager.instance.testScript.actorDatapoint1;
                    if (value > -1)
                    {
                        actor.SetDatapointLoad(ActorDatapoint.Datapoint1, value);
                        Debug.LogFormat("[Sta] ActorManager.cs -> DebugTest: {0}, {1}, ID {2} {3} set to {4} for Testing{5}", actor.actorName, actor.arc.name, actor.actorID,
                            GameManager.instance.dataScript.GetQuality(playerSide, 1), value, "\n");
                    }
                    value = GameManager.instance.testScript.actorDatapoint2;
                    if (value > -1)
                    {
                        actor.SetDatapointLoad(ActorDatapoint.Datapoint2, value);
                        Debug.LogFormat("[Sta] ActorManager.cs -> DebugTest: {0}, {1}, ID {2} {3} set to {4} for Testing{5}", actor.actorName, actor.arc.name, actor.actorID,
                            GameManager.instance.dataScript.GetQuality(playerSide, 2), value, "\n");
                    }
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for slotID {0}, side {1}", slotID, playerSide.name); }
            }
        }
    }

    /// <summary>
    /// Debug initiate an immediate relationship conflict with the actor in the specified slotID (inputString)
    /// </summary>
    /// <param name="inputString"></param>
    /// <returns></returns>
    public string DebugCreateConflict(string inputString)
    {
        string result = "Unknown";
        int slotID = -1;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //convert input to slotID
        if (string.IsNullOrEmpty(inputString) == false)
        {
            try
            { slotID = Convert.ToInt32(inputString); }
            catch (FormatException) { Debug.LogWarningFormat("Invalid nodeID {0} input (Not a Number)", inputString); }
        }
        else { Debug.LogError("Invalid inputString (Null or Empty)"); }
        //check actor at specified slot
        if (GameManager.instance.dataScript.CheckActorSlotStatus(slotID, playerSide) == true)
        {
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(slotID, playerSide);
            if (actor != null)
            {
                //set motivation to Zero (if not already). Note use SetDatapointLoad to avoid personality interference
                actor.SetDatapointLoad(ActorDatapoint.Motivation1, 0);
                //conflict
                string conflictText = ProcessActorConflict(actor);
                result = string.Format("{0}, {1} initiates a Conflict", actor.actorName, actor.arc.name);
                //outcome
                ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
                outcomeDetails.side = playerSide;
                outcomeDetails.textTop = string.Format("{0}, {1}{2}{3}, initiates a DEBUG Relationship Conflict", actor.actorName, colourAlert, actor.arc.name, colourEnd);
                outcomeDetails.textBottom = conflictText;
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActorManager.cs -> DebugCreateConflict");
            }
            else { result = string.Format("Invalid actor (Null) at slot {0}", slotID); }
        }
        else { result = string.Format("No actor is slot {0}", slotID); }
        return result;
    }

    //
    // - - - Relations
    //

    /// <summary>
    /// does an end of turn check to generate messages for existing relations in the InfoApp effects tab
    /// </summary>
    public void UpdateRelationMessages()
    {
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        List<int> tempList = new List<int>();
        string detailsTop, detailsBottom, relationship, colourRelation;
        Dictionary<int, RelationshipData> dictOfRelations = GameManager.instance.dataScript.GetDictOfRelations();
        if (dictOfRelations != null)
        {
            //loop dictionary, assumed that counter corresponds to slotID dict.Key (order of dict doesn't matter)
            for (int i = 0; i < dictOfRelations.Count; i++)
            {
                RelationshipData data = dictOfRelations[i];
                if (data != null)
                {
                    if (data.relationship != ActorRelationship.None)
                    {
                        //Get two actors in relationship
                        Actor actorFirst = GameManager.instance.dataScript.GetCurrentActor(i, playerSide);
                        if (actorFirst != null)
                        {
                            Actor actorSecond = GameManager.instance.dataScript.GetActor(data.actorID);
                            if (actorSecond != null)
                            {
                                //not on exclusion list (only want one entry for each relationship)
                                if (tempList.Exists(x => x == data.slotID) == false)
                                {
                                    if (data.slotID > -1)
                                    {
                                        //add to exclusion list
                                        tempList.Add(i);
                                        //gnerate message
                                        ActiveEffectData msgData = new ActiveEffectData();
                                        relationship = data.relationship == ActorRelationship.Friend ? "Friends" : "Enemies";
                                        colourRelation = data.relationship == ActorRelationship.Friend ? colourGood : colourBad;
                                        detailsTop = string.Format("{0}, {1}{2}{3}{4}is {5}{6}{7} with{8}{9}, {10}{11}{12}", actorFirst.actorName, colourAlert, actorFirst.arc.name, colourEnd, "\n",
                                            colourRelation, relationship, colourEnd, "\n", actorSecond.actorName, colourAlert, actorSecond.arc.name, colourEnd);
                                        detailsBottom = string.Format("Any changes in {0}Motivation{1} in one {2}can affect the other{3}", colourNeutral, colourEnd, colourAlert, colourEnd);
                                        msgData.text = string.Format("{0} and {1} are {2}", actorFirst.arc.name, actorSecond.arc.name, relationship);
                                        msgData.topText = "Relationship Exists";
                                        msgData.detailsTop = detailsTop;
                                        msgData.detailsBottom = detailsBottom;
                                        msgData.sprite = data.relationship == ActorRelationship.Friend ? GameManager.instance.guiScript.friendSprite : GameManager.instance.guiScript.enemySprite;
                                        msgData.help0 = "relation_0";
                                        msgData.help1 = "relation_1";
                                        msgData.help2 = "relation_2";
                                        msgData.help3 = "relation_3";
                                        GameManager.instance.messageScript.ActiveEffect(msgData);
                                    }
                                }
                            }
                            else { Debug.LogErrorFormat("Invalid actorSecond (Null) for actorID {0}", data.actorID); }
                        }
                        else { Debug.LogErrorFormat("Invalid actorFirst (Null) for slotID {0}", i); }
                    }
                }
                else { Debug.LogErrorFormat("Invalid relationshipData (Null) in dictOfRelations.Key slotID {0}", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfRelations (Null)"); }
    }

    //new methods above here
}
