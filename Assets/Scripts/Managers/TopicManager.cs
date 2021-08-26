
using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Human decision system
/// </summary>
public class TopicManager : MonoBehaviour
{
    #region public variables
    [Header("Main")]
    [Tooltip("Minimum number of turns before topicType/SubTypes can be chosen again")]
    [Range(0, 10)] public int minIntervalGlobal = 2;
    [Tooltip("Maximum number of options in a topic")]
    [Range(2, 4)] public int maxOptions = 4;

    [Header("Topic Reviews")]
    [Tooltip("Frequency of topic reviews in number of turns (approximate as there is some fuzziness at the end of the countdown)")]
    [Range(0, 50)] public int reviewPeriod = 20;
    [Tooltip("Chance of Review occuring on the first turn of the possible two turn activation window. If not then auto activates on second turn")]
    [Range(0, 100)] public int reviewActivationChance = 50;

    [Header("Probability")]
    [Tooltip("Number (less than) to roll for an Extreme probability option to Succeed")]
    [Range(0, 100)] public int chanceExtreme = 90;
    [Tooltip("Number (less than) to roll for a High probability option to Succeed")]
    [Range(0, 100)] public int chanceHigh = 75;
    [Tooltip("Number (less than) to roll for a Medium probability option to Succeed")]
    [Range(0, 100)] public int chanceMedium = 50;
    [Tooltip("Number (less than) to roll for a Low probability option to Succeed")]
    [Range(0, 100)] public int chanceLow = 25;
    [Tooltip("If Opinion is neutral (2) then there is this % chance of the topic being good and the balance for it being bad")]
    [Range(0, 100)] public int chanceNeutralGood = 70;
    [Tooltip("If Player Stressed there is a random chance that any option in a topic will be unavailable")]
    [Range(0, 100)] public int chanceStressedNoOption = 25;

    [Header("Text Lists")]
    [Tooltip("List of locations (generic) that can be used in any district. Used for node Action / Topic immersion")]
    public TextList textlistGenericLocation;
    [Tooltip("List of ending lines (self contained) for a Bad resistance topic, eg. 'The Authority is spinning this one hard'. Used via the [badRES] tag")]
    public TextList textListBadResistance;
    [Tooltip("List of ending lines (self contained) for a Good resistance topic, eg. 'We could use htis'. Used via the [goodRES] tag")]
    public TextList textListGoodResistance;
    [Tooltip("List of NPC is/was 'something'")]
    public TextList textListNpcSomething;
    [Tooltip("List of reasons for wanting money")]
    public TextList textListMoneyReason;
    [Tooltip("List of HR Policy")]
    public TextList textListPolicy;
    [Tooltip("List of Handicaps")]
    public TextList textListHandicap;
    [Tooltip("List of Who, first instance, eg. Sister, best friend, etc.")]
    public TextList textListWho0;
    [Tooltip("List of Who, second instance, eg. stepsister, second cousin, etc.")]
    public TextList textListWho1;
    [Tooltip("List of condition relating to person referred to be textListWho")]
    public TextList textListCondition;
    [Tooltip("List of Friend actions for ActorPolitic topics")]
    public TextList textListFriend;
    [Tooltip("List of Enemy actions for ActorPolitic topics")]
    public TextList textListEnemy;
    [Tooltip("List of Enemy actions for ActorPolitic topics")]
    public TextList textListEnemyAction;
    [Tooltip("List of Enemy reasons for ActorPolitic topics")]
    public TextList textListEnemyReason;
    [Tooltip("List of Friend actions for ActorPolitic topics")]
    public TextList textListFriendAction;
    [Tooltip("List of Friend reasons for ActorPolitic topics")]
    public TextList textListFriendReason;
    [Tooltip("List of active verb combos for HQ topics")]
    public TextList textListHqActive;
    [Tooltip("List of issues for HQ topics")]
    public TextList textListHqIssue;
    [Tooltip("List of conflicts for HQ topics")]
    public TextList textListHqConflict;
    [Tooltip("List of wants for HQ topics")]
    public TextList textListHqWant;
    [Tooltip("List of meds for HQ topics")]
    public TextList textListMeds;
    [Tooltip("List Of diseases for HQ topics")]
    public TextList textListDisease;

    [Header("TopicTypes (with subSubTypes)")]
    [Tooltip("Used to avoid having to hard code the TopicType.SO names")]
    public TopicType actorType;
    [Tooltip("Used to avoid having to hard code the TopicType.SO names")]
    public TopicType playerType;
    [Tooltip("Used to avoid having to hard code the TopicType.SO names")]
    public TopicType authorityType;
    [Tooltip("Used to avoid having to hard code the TopicType.SO names")]
    public TopicType captureType;

    [Header("TopicSubTypes (with subSubTypes)")]
    [Tooltip("Used to avoid having to hard code the TopicType.SO names")]
    public TopicSubType actorDistrictSubType;
    [Tooltip("Used to avoid having to hard code the TopicType.SO names")]
    public TopicSubType playerDistrictSubType;
    [Tooltip("Used to avoid having to hard code the TopicType.SO names")]
    public TopicSubType authorityTeamSubType;

    [Header("Topic Scopes")]
    [Tooltip("Used to avoid having to hard code the TopicScope.SO names")]
    public TopicScope levelScope;
    [Tooltip("Used to avoid having to hard code the TopicScope.SO names")]
    public TopicScope campaignScope;

    [Header("Actor TopicSubSubTypes")]
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType actorBlowStuffUp;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType actorCreateRiots;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType actorDeployTeam;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType actorGainTargetInfo;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType actorHackSecurity;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType actorInsertTracer;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType actorNeutraliseTeam;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType actorObtainGear;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType actorRecallTeam;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType actorRecruitActor;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType actorSpreadFakeNews;

    [Header("Player TopicSubSubTypes")]
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType playerBlowStuffUp;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType playerCreateRiots;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType playerDeployTeam;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType playerGainTargetInfo;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType playerHackSecurity;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType playerInsertTracer;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType playerNeutraliseTeam;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType playerObtainGear;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType playerRecallTeam;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType playerRecruitActor;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType playerSpreadFakeNews;

    [Header("Authority Team TopicSubSubTypes")]
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType teamCivil;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType teamControl;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType teamDamage;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType teamErasure;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType teamMedia;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType teamProbe;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType teamSpider;
    #endregion

    #region Save Data Compatible
    //Story Module topic Pools
    [HideInInspector] public TopicPool storyAlphaPool;
    [HideInInspector] public TopicPool storyBravoPool;
    [HideInInspector] public TopicPool storyCharliePool;

    [HideInInspector] public bool isStoryAlphaGood;                 //determines topicGroupType.storyAlpha is good (true) or bad (false)
    [HideInInspector] public bool isStoryBravoGood;
    [HideInInspector] public bool isStoryCharlieGood;

    [HideInInspector] public int storyAlphaCurrentIndex;            //current linked index for story Alpha sequence of linked topics within the level
    [HideInInspector] public int storyBravoCurrentIndex;            //current linked index for story Bravo sequence of linked topics within the level
    [HideInInspector] public int storyCharlieCurrentIndex;          //current linked index for story Charlie sequence of linked topics within the level
    [HideInInspector] public int storyCurrentLevelIndex;            //current scenario index for all stories

    [HideInInspector] public int[,] arrayOfStoryFlags;              //flags for progressing story
    [HideInInspector] public int[,] arrayOfStoryStars;              //stars for tracking player achievement
    #endregion

    #region private variables
    //type of topic
    private TopicGlobal topicGlobal;        //what type of topic is being generated, eg. Decision, Review, etc.
    private int reviewCountdown;            //counts down review interval, set to reviewPeriod after each Review
    private bool reviewActivationMiss;      //used to accomodate a two turn window for Review activation. Set true if on first turn where activation possible it didn't happen. Auto happens next turn.

    //debugging (testManager.cs)
    private TopicPool debugTopicPool;
    private IEnumerator coroutine;
    private bool haltExecutionTopic;
    private bool isTestLog;

    //info tags (topic specific info) -> reset to defaults each turn in ResetTopicAdmin prior to use
    private int tagActorID;
    private int tagActorOtherID;        //used for dual actor effects, eg. relationships
    private int tagNodeID;
    private int tagTeamID;              //used for authority team actions
    private int tagContactID;
    private int tagTurn;
    private bool tagHqActors;           //if true, tagActorID / tagActorOtherID are from HQ, default false, ignore (used for HQ topics)
    private string tagJob;
    private string tagLocation;
    private string tagMeds;
    private string tagDisease;
    private string tagGear;                 //multipurpose, could be node action gear for both actor/player, actor held gear or gear from Player inventory (depends on topicSubType)
    private string tagRecruit;
    private string tagSecretName;               //secret.name
    private string tagSecretTag;                //secret.tag
    private string tagTeam;                //used for resistance team actions
    private string tagTarget;
    private string tagStringData;        //General purpose
    private string tagSpriteName;
    private string tagOptionText;           //option.text after CheckTopicText
    private string tagOutcome;
    private string tagOrgName;          //name of organisation, eg. 'BlueAngelCult'
    private string tagOrgTag;           //tag of organisation, eg. 'Blue Angel Cult'
    private string tagOrgWant;          //what the org wants you to do (org.textWant)
    private string tagOrgText;          //previous service provided by Org (orgData.text)
    private string tagInvestTag;        //investigation tag
    private string tagHqActorName;      //name of HQ [actor.tagActorID]
    private string tagHqOtherName;      //name of HQ [actor.tagActorOtherID]
    private string tagHqTitleActor;     //title of HQ [actor.tagActorID]
    private string tagHqTitleOther;     //title of HQ [other.tagActorOtherID]
    private ActorRelationship tagRelation;  //used for actor relationships
    private NodeAction tagNodeAction;       //used for Player district Actions (needed to find record in playerManager.cs -> listOfNodeActions in order to delete it)
    private int[] arrayOfOptionActorIDs;     //actorID's corresponding to option choices (0 -> 3) for topics where you have a choice of actors, eg. Player General
    private int[] arrayOfOptionInactiveIDs;  //actorID's corresponding to option choices (0 -> 3), inactive actors, for Player General topics

    //collections (local)
    private List<TopicType> listOfTopicTypesTurn = new List<TopicType>();                               //level topics that passed their turn checks
    private List<TopicType> listOfTypePool = new List<TopicType>();                                     //turn selection pool for topicTypes (priority based)
    private List<TopicSubType> listOfSubTypePool = new List<TopicSubType>();                            //turn selection pool for topicSubTypes (priority based)
    private List<Topic> listOfTopicPool = new List<Topic>();                                            //turn selection pool for topics (priority based)

    //Turn selection
    private TopicType turnTopicType;
    private TopicSubType turnTopicSubType;
    private TopicSubSubType turnTopicSubSubType;
    private Topic turnTopic;
    private Sprite turnSprite;
    private TopicOption turnOption;                                                                     //option selected

    private int minIntervalGlobalActual;                                                                //number used in codes. Can be less than the minIntervalGlobal

    //fast access
    private string levelScopeName;
    private string campaignScopeName;

    //adverts
    private int advertChance = -1;

    //colour palette for Modal Outcome
    private string colourGood;
    private string colourBad;
    private string colourNeutral;
    private string colourNormal;
    /*private string colourDefault;*/
    private string colourAlert;
    private string colourCancel;
    private string colourGrey;
    private string colourEnd;
    #endregion

    #region Initialise
    /// <summary>
    /// Initialisation
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
                SubInitialiseAsserts();     //needs to be first
                SubInitialiseFastAccess();
                SubInitialiseStartUp();
                SubInitialiseLevelStart();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseLevelStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseAsserts();
                SubInitialiseFastAccess();
                SubInitialiseStartUp();
                SubInitialiseStoryTopics();
                SubInitialiseEvents();
                break;
            case GameState.LoadGame:
                SubInitialiseStoryTopics();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\" (Initialise)", GameManager.i.inputScript.GameState);
                break;
        }
    }
    #endregion

    #region Initialisation SubMethods

    #region SubInitialiseStartUp
    /// <summary>
    /// Initialise Start Up
    /// </summary>
    private void SubInitialiseStartUp()
    {
        //collections
        arrayOfStoryFlags = new int[(int)StoryType.Count, GameManager.i.campaignScript.GetMaxScenarioIndex() + 1];
        arrayOfStoryStars = new int[(int)StoryType.Count, GameManager.i.campaignScript.GetMaxScenarioIndex() + 1];
        //[Tst] debug logging on/off
        isTestLog = GameManager.i.testScript.isTopicManager;
        //debug topic pool
        debugTopicPool = GameManager.i.testScript.debugTopicPool;
        //initialise Topic Profile delays
        TopicProfile[] arrayOfProfiles = GameManager.i.loadScript.arrayOfTopicProfiles;
        if (arrayOfProfiles != null)
        {
            for (int i = 0; i < arrayOfProfiles.Length; i++)
            {
                TopicProfile profile = arrayOfProfiles[i];
                if (profile != null)
                {
                    //update delays
                    profile.delayRepeat = minIntervalGlobal * profile.delayRepeatFactor;
                    profile.delayStart = minIntervalGlobal * profile.delayStartFactor;
                    profile.timerWindow = minIntervalGlobal * profile.liveWindowFactor;
                }
                else { Debug.LogWarningFormat("Invalid TopicProfile (Null) for arrayOfProfiles[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid arrayOfProfiles (Null)"); }
        //arrayOfOptionActorID's
        arrayOfOptionActorIDs = new int[maxOptions];
        arrayOfOptionInactiveIDs = new int[maxOptions];
        //calculates topicType/SubType minimum Intervals based on global setting (minTopicTypeTurns)
        List<TopicType> listOfTopicTypes = GameManager.i.dataScript.GetListOfTopicTypes();
        if (listOfTopicTypes != null)
        {
            //loop topicTypes and update minIntervals
            for (int i = 0; i < listOfTopicTypes.Count; i++)
            {
                TopicType topicType = listOfTopicTypes[i];
                if (topicType != null)
                {
                    topicType.minInterval = topicType.minIntervalFactor * minIntervalGlobal;
                    //update topicTypeData minInteval
                    TopicTypeData dataType = GameManager.i.dataScript.GetTopicTypeData(topicType.name);
                    if (dataType != null)
                    { dataType.minInterval = topicType.minInterval; }
                    else { Debug.LogWarningFormat("Invaid topicTypeData (Null) for topicType \"{0}\"", topicType.name); }
                    //get listOfSubTypes
                    if (topicType.listOfSubTypes != null)
                    {
                        //loop listOfSubTypes
                        for (int j = 0; j < topicType.listOfSubTypes.Count; j++)
                        {
                            TopicSubType subType = topicType.listOfSubTypes[j];
                            if (subType != null)
                            {
                                subType.minInterval = subType.minIntervalFactor * minIntervalGlobal;
                                //update topicTypeData minInteval
                                TopicTypeData dataSub = GameManager.i.dataScript.GetTopicSubTypeData(subType.name);
                                if (dataSub != null)
                                { dataSub.minInterval = subType.minInterval; }
                                else { Debug.LogWarningFormat("Invaid topicTypeData (Null) for topicSubType \"{0}\"", subType.name); }
                            }
                            else { Debug.LogWarningFormat("Invalid topicSubType (Null) in listOfTopicSubType[{0}] for topicType \"{1}\"", j, topicType.name); }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid topicType.listOfSubTypes (Null) for \"{0}\"", topicType.name); }
                }
                else { Debug.LogWarningFormat("Invalid topicType (Null) in listOfTopicTypes[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfTopicTypes (Null)"); }
        //obtain relevant story Modules
        GetStoryTopicPools();
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        SetColours();
        //reset all topics to isCurrent False prior to changes
        GameManager.i.dataScript.ResetTopics();
        //establish which TopicTypes are valid for the level. Initialise profile and status data.
        UpdateTopicPools();
        SetStoryGroupFlags();
        SetStoryIndexes();
        //switch off actor topics if no subordinates, on otherwise
        if (GameManager.i.optionScript.isSubordinates == false)
        { GameManager.i.dataScript.ToggleTopicType("Actor"); }
        else { GameManager.i.dataScript.ToggleTopicType("Actor", false); }
    }
    #endregion

    #region SubInitialiseStoryTopics
    /// <summary>
    /// Configures story Topics to be in the correct place within the story sequence on loading
    /// NOTE: Needs to be AFTER SubInitialiseStartUp in Initialisation sequence
    /// </summary>
    private void SubInitialiseStoryTopics()
    {
        /*SetStoryPoolsOnLoad(); EDIT -> Redundant Aug'20 */
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //adverts
        advertChance = GameManager.i.guiScript.advertChance;
        Debug.Assert(advertChance > -1, "Invalid advertChance (-1)");
    }
    #endregion

    #region SubInitialiseAsserts
    /// <summary>
    /// Assert SO checks
    /// </summary>
    private void SubInitialiseAsserts()
    {
        //text lists
        Debug.Assert(textlistGenericLocation != null, "Invalid textListGenericLocations (Null)");
        Debug.Assert(textListBadResistance != null, "Invalid textListBadResistance (Null)");
        Debug.Assert(textListGoodResistance != null, "Invalid textListGoodResistance (Null)");
        Debug.Assert(textListNpcSomething != null, "Invalid textListNpcSomething (Null)");
        Debug.Assert(textListMoneyReason != null, "Invalid textListMoneyReason (Null)");
        Debug.Assert(textListPolicy != null, "Invalid textListPolicy (Null)");
        Debug.Assert(textListHandicap != null, "Invalid textListHandicap (Null)");
        Debug.Assert(textListWho0 != null, "Invalid textListWho0 (Null)");
        Debug.Assert(textListWho1 != null, "Invalid textListWho1 (Null)");
        Debug.Assert(textListCondition != null, "Invalid textListCondition (Null)");
        Debug.Assert(textListFriend != null, "Invalid textListFriend (Null)");
        Debug.Assert(textListEnemy != null, "Invalid textListEnemy (Null)");
        Debug.Assert(textListFriendAction != null, "Invalid textListFriendAction (Null)");
        Debug.Assert(textListEnemyAction != null, "Invalid textListEnemyAction (Null)");
        Debug.Assert(textListFriendReason != null, "Invalid textListFriendReason (Null)");
        Debug.Assert(textListEnemyReason != null, "Invalid textListEnemyReason (Null)");
        Debug.Assert(textListHqActive != null, "Invalid textListHqActive (Null)");
        Debug.Assert(textListHqIssue != null, "Invalid textListHqIssue (Null)");
        Debug.Assert(textListHqConflict != null, "Invalid textListHqConflict (Null)");
        Debug.Assert(textListHqWant != null, "Invalid textListHqWant (Null)");
        Debug.Assert(textListMeds != null, "Invalid textListMeds (Null)");
        Debug.Assert(textListDisease != null, "Invalid textListDisease (Null)");
        //types
        Debug.Assert(actorType != null, "Invalid actorType (Null)");
        Debug.Assert(playerType != null, "Invalid playerType (Null)");
        Debug.Assert(authorityType != null, "Invalid authorityType (Null)");
        Debug.Assert(captureType != null, "Invalid captureType (Null)");
        Debug.Assert(actorDistrictSubType != null, "Invalid actorDistrictSubType (Null)");
        Debug.Assert(playerDistrictSubType != null, "Invalid playerDistrictSubType (Null)");
        Debug.Assert(authorityTeamSubType != null, "Invalid authorityTeamSubType (Null)");
        //scopes
        Debug.Assert(levelScope != null, "Invalid levelScope (Null)");
        Debug.Assert(campaignScope != null, "Invalid campaignScope (Null)");
        if (levelScope != null) { levelScopeName = levelScope.name; }
        if (campaignScope != null) { campaignScopeName = campaignScope.name; }
        Debug.Assert(string.IsNullOrEmpty(levelScopeName) == false, "Invalid levelScopeName (Null or Empty)");
        Debug.Assert(string.IsNullOrEmpty(campaignScopeName) == false, "Invalid campaignScopeName (Null or Empty)");
        //actor subSubTypes
        Debug.Assert(actorBlowStuffUp != null, "Invalid actorBlowStuffUp (Null)");
        Debug.Assert(actorCreateRiots != null, "Invalid actorCreateRiots (Null)");
        Debug.Assert(actorDeployTeam != null, "Invalid actorDeployTeam (Null)");
        Debug.Assert(actorGainTargetInfo != null, "Invalid actorGainTargetInfo (Null)");
        Debug.Assert(actorHackSecurity != null, "Invalid actorHackSecurity (Null)");
        Debug.Assert(actorInsertTracer != null, "Invalid actorInsertTracer (Null)");
        Debug.Assert(actorNeutraliseTeam != null, "Invalid actorNeutraliseTeam (Null)");
        Debug.Assert(actorObtainGear != null, "Invalid actorObtainGear (Null)");
        Debug.Assert(actorRecallTeam != null, "Invalid actorRecallTeam (Null)");
        Debug.Assert(actorRecruitActor != null, "Invalid actorRecruitActor (Null)");
        Debug.Assert(actorSpreadFakeNews != null, "Invalid actorSpreadFakeNews (Null)");
        //player subSubTypes
        Debug.Assert(playerBlowStuffUp != null, "Invalid playerBlowStuffUp (Null)");
        Debug.Assert(playerCreateRiots != null, "Invalid playerCreateRiots (Null)");
        Debug.Assert(playerDeployTeam != null, "Invalid playerDeployTeam (Null)");
        Debug.Assert(playerGainTargetInfo != null, "Invalid playerGainTargetInfo (Null)");
        Debug.Assert(playerHackSecurity != null, "Invalid playerHackSecurity (Null)");
        Debug.Assert(playerInsertTracer != null, "Invalid playerInsertTracer (Null)");
        Debug.Assert(playerNeutraliseTeam != null, "Invalid playerNeutraliseTeam (Null)");
        Debug.Assert(playerObtainGear != null, "Invalid playerObtainGear (Null)");
        Debug.Assert(playerRecallTeam != null, "Invalid playerRecallTeam (Null)");
        Debug.Assert(playerRecruitActor != null, "Invalid playerRecruitActor (Null)");
        Debug.Assert(playerSpreadFakeNews != null, "Invalid playerSpreadFakeNews (Null)");
        //authority Team subSubTypes
        Debug.Assert(teamCivil != null, "Invalid teamCivil (Null)");
        Debug.Assert(teamControl != null, "Invalid teamControl (Null)");
        Debug.Assert(teamDamage != null, "Invalid teamDamage (Null)");
        Debug.Assert(teamErasure != null, "Invalid teamErasure (Null)");
        Debug.Assert(teamMedia != null, "Invalid teamMedia (Null)");
        Debug.Assert(teamProbe != null, "Invalid teamProbe (Null)");
        Debug.Assert(teamSpider != null, "Invalid teamSpider (Null)");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "TopicManager");
        EventManager.i.AddListener(EventType.OutcomeClose, OnEvent, "TopicManager");
    }
    #endregion

    #endregion

    #region OnEvent
    /// <summary>
    /// Event handler
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
            case EventType.OutcomeClose:
                SetHaltExecutionTopic(false);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion

    #region SetColours
    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourGood = GameManager.i.colourScript.GetColour(ColourType.goodText);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.badText);
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourNormal = GameManager.i.colourScript.GetColour(ColourType.normalText);
        /*colourDefault = GameManager.instance.colourScript.GetColour(ColourType.whiteText);*/
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        colourCancel = GameManager.i.colourScript.GetColour(ColourType.moccasinText);
        colourGrey = GameManager.i.colourScript.GetColour(ColourType.greyText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
    }
    #endregion

    #region Topic Type

    public TopicGlobal GetTopicGlobal()
    { return topicGlobal; }

    #endregion

    #region Session Start...
    //
    // - - - Session Start - - -
    //

    #region GetStoryTopicPools
    /// <summary>
    /// Pulls selected topic pools from Campaign.SO -> StoryModule and populates the three story module variables in TopicManager.cs with them prior to processing
    /// </summary>
    private void GetStoryTopicPools()
    {
        //get current campaign
        Campaign campaign = GameManager.i.campaignScript.campaign;
        if (campaign != null)
        {
            StoryModule story = campaign.story;
            if (story != null)
            {
                //DEBUG / Placeholder -> randomly selects story modules to be used (should be player choice in new game set-up)
                if (story.listOfCampaignStories != null)
                {
                    if (story.listOfCampaignStories.Count > 0)
                    {
                        StoryData storyData = story.GetRandomCampaignStoryData();
                        if (storyData != null)
                        {
                            storyAlphaPool = storyData.pool;
                            if (storyAlphaPool == null)
                            { Debug.LogError("Invalid storyAlphaPool (Null)"); }
                            else { Debug.LogFormat("[Sto] TopicManager.cs -> GetStoryTopicPool: storyAlphaPool \"{0}\"{1}", storyAlphaPool.tag, "\n"); }
                        }
                        else { Debug.LogWarning("Invalid storyData (Null) for storyAlpha Campaign topics"); }
                    }
                }
                else { Debug.LogWarning("Invalid storyModule.listOfCampaignStories (Null)"); }
                //Bravo
                if (story.listOfFamilyStories != null)
                {
                    if (story.listOfFamilyStories.Count > 0)
                    {
                        StoryData storyData = story.GetRandomFamilyStoryData();
                        if (storyData != null)
                        {
                            storyBravoPool = storyData.pool;
                            if (storyBravoPool == null)
                            { Debug.LogError("Invalid storyBravoPool (Null)"); }
                            else { Debug.LogFormat("[Sto] TopicManager.cs -> GetStoryTopicPool: storyBravoPool \"{0}\"{1}", storyBravoPool.tag, "\n"); }
                        }
                        else { Debug.LogWarning("Invalid storyData (Null) for storyBravo Family topics"); }
                    }
                }
                else { Debug.LogWarning("Invalid storyModule.listOfFamilyStories (Null)"); }
                //Charlie
                if (story.listOfHqStories != null)
                {
                    if (story.listOfHqStories.Count > 0)
                    {
                        StoryData storyData = story.GetRandomHqStoryData();
                        if (storyData != null)
                        {
                            storyCharliePool = storyData.pool;
                            if (storyCharliePool == null)
                            { Debug.LogError("Invalid storyCharliePool (Null)"); }
                            else { Debug.LogFormat("[Sto] TopicManager.cs -> GetStoryTopicPool: storyCharliePool \"{0}\"{1}", storyCharliePool.tag, "\n"); }
                        }
                        else { Debug.LogWarning("Invalid storyData (Null) for storyCharlie Hq topics"); }
                    }
                }
                else { Debug.LogWarning("Invalid storyModule.listOfHqStories (Null)"); }
            }
            else { Debug.LogError("Invalid storyModule (Null)"); }
        }
        else { Debug.LogError("Invalid campaign (Null)"); }
    }
    #endregion

    #region UpdateTopicPools
    /// <summary>
    /// Populates DataManager.cs -> dictOfTopicPools (at start of a level) with all valid topics and sets up listOfTopicTypesLevel for all topics that are valid for the current level
    /// </summary>
    public void UpdateTopicPools()
    {
        List<TopicType> listOfTopicTypesLevel = GameManager.i.dataScript.GetListOfTopicTypesLevel();
        if (listOfTopicTypesLevel != null)
        {
            //get current topicTypes
            List<TopicType> listOfTopicTypes = GameManager.i.dataScript.GetListOfTopicTypes();
            if (listOfTopicTypes != null)
            {
                //get current campaign
                Campaign campaign = GameManager.i.campaignScript.campaign;
                if (campaign != null)
                {
                    //get current city
                    City city = GameManager.i.cityScript.GetCity();
                    if (city != null)
                    {
                        string subTypeName;
                        //loop by subTypes
                        for (int i = 0; i < listOfTopicTypes.Count; i++)
                        {
                            TopicType topicType = listOfTopicTypes[i];
                            if (topicType != null)
                            {
                                GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
                                //only proceed with topicType if either the same as Player side or 'Both'
                                if (topicType.side.level == playerSide.level || topicType.side.level == 3)
                                {
                                    for (int j = 0; j < topicType.listOfSubTypes.Count; j++)
                                    {
                                        TopicSubType topicSubType = topicType.listOfSubTypes[j];
                                        if (topicSubType != null)
                                        {
                                            //subType must be playerSide of 'both'
                                            if (topicSubType.side.level == playerSide.level || topicSubType.side.level == 3)
                                            {
                                                subTypeName = topicSubType.name;
                                                bool isValid = false;

                                                switch (topicSubType.name)
                                                {
                                                    case "ActorPolitic":
                                                        if (campaign.campaignPool.actorPoliticPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.actorPoliticPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.actorPoliticPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.actorPoliticPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.actorPoliticPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorContact":
                                                        if (campaign.campaignPool.actorContactPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.actorContactPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.actorContactPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.actorContactPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.actorContactPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorDistrict":
                                                        if (campaign.campaignPool.actorDistrictPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.actorDistrictPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.actorDistrictPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.actorDistrictPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.actorDistrictPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorGear":
                                                        if (campaign.campaignPool.actorGearPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.actorGearPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.actorGearPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.actorGearPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.actorGearPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorMatch":
                                                        if (campaign.campaignPool.actorMatchPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.actorMatchPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.actorMatchPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.actorMatchPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.actorMatchPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "PlayerDistrict":
                                                        if (campaign.campaignPool.playerDistrictPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.playerDistrictPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.playerDistrictPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.playerDistrictPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.playerDistrictPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "PlayerGeneral":
                                                        if (campaign.campaignPool.playerGeneralPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.playerGeneralPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.playerGeneralPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.playerGeneralPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.playerGeneralPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "PlayerStats":
                                                        if (campaign.campaignPool.playerStatsPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.playerStatsPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.playerStatsPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.playerStatsPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.playerStatsPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "PlayerGear":
                                                        if (campaign.campaignPool.playerGearPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.playerGearPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.playerGearPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.playerGearPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.playerGearPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "PlayerConditions":
                                                        if (campaign.campaignPool.playerConditionsPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.playerConditionsPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.playerConditionsPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.playerConditionsPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.playerConditionsPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "AuthorityTeam":
                                                        if (campaign.campaignPool.teamPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.teamPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.teamPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.teamPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.teamPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "StoryAlpha":
                                                        if (storyAlphaPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (storyAlphaPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(storyAlphaPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, storyAlphaPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(storyAlphaPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "StoryBravo":
                                                        if (storyBravoPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (storyBravoPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(storyBravoPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, storyBravoPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(storyBravoPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "StoryCharlie":
                                                        if (storyCharliePool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (storyCharliePool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(storyCharliePool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, storyCharliePool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(storyCharliePool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "CitySub":
                                                        switch (campaign.side.level)
                                                        {
                                                            case 1:
                                                                //Authority
                                                                if (city.cityPoolAuthority != null)
                                                                {
                                                                    //any subSubTypes present?
                                                                    if (city.cityPoolAuthority.listOfSubSubTypePools.Count > 0)
                                                                    { LoadSubSubTypePools(city.cityPoolAuthority, campaign.side); }
                                                                    //populate dictionary
                                                                    GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, city.cityPoolAuthority.listOfTopics);
                                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                                    SetTopicDynamicData(city.cityPoolAuthority.listOfTopics);
                                                                    isValid = true;
                                                                }
                                                                break;
                                                            case 2:
                                                                //Resistance
                                                                if (city.cityPoolResistance != null)
                                                                {
                                                                    //any subSubTypes present?
                                                                    if (city.cityPoolResistance.listOfSubSubTypePools.Count > 0)
                                                                    { LoadSubSubTypePools(city.cityPoolResistance, campaign.side); }
                                                                    //populate dictionary   
                                                                    GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, city.cityPoolResistance.listOfTopics);
                                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                                    SetTopicDynamicData(city.cityPoolResistance.listOfTopics);
                                                                    isValid = true;
                                                                }
                                                                break;
                                                            default:
                                                                Debug.LogWarningFormat("Unrecognised campaign side \"{0}\" for CitySub", campaign.side.name);
                                                                break;
                                                        }
                                                        break;
                                                    case "HQSub":
                                                        if (campaign.campaignPool.hqPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.hqPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.hqPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.hqPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.hqPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "CaptureSub":
                                                        if (campaign.campaignPool.capturePool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.capturePool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.capturePool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.capturePool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.capturePool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "OrgCure":
                                                        if (campaign.campaignPool.orgCurePool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.orgCurePool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.orgCurePool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.orgCurePool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.orgCurePool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "OrgContract":
                                                        if (campaign.campaignPool.orgContractPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.orgContractPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.orgContractPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.orgContractPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.orgContractPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "OrgHQ":
                                                        if (campaign.campaignPool.orgHQPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.orgHQPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.orgHQPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.orgHQPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.orgHQPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "OrgEmergency":
                                                        if (campaign.campaignPool.orgEmergencyPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.orgEmergencyPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.orgEmergencyPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.orgEmergencyPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.orgEmergencyPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "OrgInfo":
                                                        if (campaign.campaignPool.orgInfoPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignPool.orgInfoPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignPool.orgInfoPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignPool.orgInfoPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignPool.orgInfoPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    default:
                                                        Debug.LogWarningFormat("Unrecognised topicSubType \"{0}\" for topicType \"{1}\"", topicSubType.name, topicType.name);
                                                        break;
                                                }
                                                //set subType topicTypeData 'isAvailable' to appropriate value
                                                TopicTypeData data = GameManager.i.dataScript.GetTopicSubTypeData(topicSubType.name);
                                                if (data != null)
                                                { data.isAvailable = isValid; }
                                                else
                                                { Debug.LogErrorFormat("Invalid topicTypeData (Null) for topicSubType \"{0}\"", topicSubType.name); }
                                            }
                                        }

                                        else { Debug.LogWarningFormat("Invalid topicSubType (Null) for topicType \"{0}\" in listOFSubTypes[{1}]", topicType.name, j); }
                                    }
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid topicType (Null) for listOfTopicTypes[{0}]", i); }
                        }
                    }
                    else { Debug.LogError("Invalid City (Null)"); }
                }
                else { Debug.LogError("Invalid campaign (Null)"); }
            }
            else { Debug.LogError("Invalid listOfTopicTypes (Null)"); }
        }
        else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
    }
    #endregion

    #region SetTopicDynamicData
    /// <summary>
    /// Initialises all dynamic profile data, sets 'isCurrent' to true, zero's stats  and sets status at session start for all topics in topic pool list
    /// </summary>
    /// <param name="listOfTopics"></param>
    private void SetTopicDynamicData(List<Topic> listOfTopics)
    {
        int scenarioIndex = GameManager.i.campaignScript.GetScenarioIndex();
        //Review topics
        reviewCountdown = reviewPeriod;
        reviewActivationMiss = false;
        //Decision topics
        if (listOfTopics != null)
        {
            for (int i = 0; i < listOfTopics.Count; i++)
            {
                Topic topic = listOfTopics[i];
                if (topic != null)
                {
                    //zero status
                    topic.turnsDormant = 0;
                    topic.turnsActive = 0;
                    topic.turnsLive = 0;
                    topic.turnsDone = 0;
                    //profile
                    TopicProfile profile = topic.profile;
                    if (profile != null)
                    {
                        //set timers
                        topic.timerRepeat = profile.delayRepeat;
                        topic.timerStart = profile.delayStart;
                        topic.timerWindow = profile.timerWindow;

                        //set status (only if level Scope)
                        if (topic.subType.scope.name.Equals(levelScopeName, StringComparison.Ordinal) == true)
                        {
                            if (topic.timerStart == 0)
                            { topic.status = Status.Active; }
                            else { topic.status = Status.Dormant; }
                            //isCurrent (all topics set to false prior to changes by SubInitialiseLevelStart
                            topic.isCurrent = true;
                        }
                        //Campaign Scope
                        else if (topic.subType.scope.name.Equals(campaignScopeName, StringComparison.Ordinal) == true)
                        {
                            //check correct scenario
                            if (topic.levelIndex == scenarioIndex)
                            {
                                //check first link in sequence
                                if (topic.linkedIndex == 0)
                                {
                                    //initialise
                                    if (topic.timerStart == 0)
                                    { topic.status = Status.Active; }
                                    else { topic.status = Status.Dormant; }
                                    //isCurrent (all topics set to false prior to changes by SubInitialiseLevelStart
                                    topic.isCurrent = true;
                                }
                                else
                                {
                                    //level topic in sequence but not the first
                                    topic.status = Status.Dormant;
                                    topic.isCurrent = false;
                                }
                            }
                            else
                            {
                                //different level/scenario, not relevant
                                topic.status = Status.Done;
                                topic.isCurrent = false;
                            }
                        }
                        else { Debug.LogWarningFormat("Invalid topic.subType.scope.name \"{0}\", status not set", topic.subType.scope.name); }
                    }
                    else { Debug.LogWarningFormat("Invalid profile (Null) for topic \"{0}\"", topic.name); }
                }
                else { Debug.LogWarningFormat("Invalid topic (Null) for listOfTopics[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfTopics (Null)"); }
    }
    #endregion

    #region SetStoryGroupFlags
    /// <summary>
    /// Set isStoryAlpha/Bravo/CharlieGood flags at start of each level
    /// </summary>
    private void SetStoryGroupFlags()
    {
        //done on a random basis at present but could be changed to something else
        if (Random.Range(0, 100) < 50) { isStoryAlphaGood = true; }
        else { isStoryAlphaGood = false; }
        if (Random.Range(0, 100) < 50) { isStoryBravoGood = true; }
        else { isStoryBravoGood = false; }
        if (Random.Range(0, 100) < 50) { isStoryCharlieGood = true; }
        else { isStoryCharlieGood = false; }
        Debug.LogFormat("[Sto] TopicManager.cs -> SetStoryGroupFlags: isStoryAlphaGood set {0}{1}", isStoryAlphaGood, "\n");
        Debug.LogFormat("[Sto] TopicManager.cs -> SetStoryGroupFlags: isStoryBravoGood set {0}{1}", isStoryBravoGood, "\n");
        Debug.LogFormat("[Sto] TopicManager.cs -> SetStoryGroupFlags: isStoryCharlieGood set {0}{1}", isStoryCharlieGood, "\n");
    }
    #endregion

    #region SetStoryIndexes
    /// <summary>
    /// Set/Reset story indexes at start of a new level
    /// </summary>
    private void SetStoryIndexes()
    {
        storyAlphaCurrentIndex = 0;
        storyBravoCurrentIndex = 0;
        storyCharlieCurrentIndex = 0;
        storyCurrentLevelIndex = GameManager.i.campaignScript.GetScenarioIndex();
    }
    #endregion

    #region SetStoryPoolsOnLoad
    /// <summary>
    /// Runs through all story topic pools (campaign scope) and configures topics (status and isCurrent) so that the sequence will resume at the correct place once a save is loaded
    /// NOTE: Called from FileManager.cs -> ReadTopics once all data has been read in (done due to sequencing issues as topic data is read AFTER the Initialisation routines above
    /// </summary>
    public void SetStoryPoolsOnLoad()
    {
        /*//sequencing issues on loading require assignment of storyCurrentLevelIndx because value has not yet been loaded in from save file
        storyCurrentLevelIndex = GameManager.i.campaignScript.GetScenarioIndex();*/
        //Process topic pools
        ProcessStoryTopicPool(storyAlphaPool, storyAlphaCurrentIndex, storyCurrentLevelIndex);
        ProcessStoryTopicPool(storyBravoPool, storyBravoCurrentIndex, storyCurrentLevelIndex);
        ProcessStoryTopicPool(storyCharliePool, storyCharlieCurrentIndex, storyCurrentLevelIndex);
    }

    /// <summary>
    /// Submethod for SetStoryPoolsOnLoad which handles processing
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="indexCurrent"></param>
    private void ProcessStoryTopicPool(TopicPool pool, int indexCurrent, int levelIndex)
    {
        if (pool != null)
        {
            for (int i = 0; i < pool.listOfTopics.Count; i++)
            {
                Topic topic = pool.listOfTopics[i];
                if (topic != null)
                {
                    //check topic applies to this level
                    if (topic.levelIndex == levelIndex)
                    {
                        if (topic.linkedIndex == indexCurrent)
                        {
                            //activate topic
                            topic.status = Status.Active;
                            topic.isCurrent = true;
                        }
                        else
                        {
                            if (topic.linkedIndex > indexCurrent)
                            {
                                //yet to do topic
                                topic.status = Status.Dormant;
                                topic.isCurrent = false;
                            }
                            else
                            {
                                //already done topic (linkedIndex < currentIndex)
                                topic.status = Status.Done;
                                topic.isCurrent = false;
                            }
                        }
                    }
                    else
                    {
                        //different level, shut down topic
                        topic.status = Status.Done;
                        topic.isCurrent = false;
                    }
                }
                else { Debug.LogWarningFormat("Invalid topic (Null) for storyAlphaPool.listOfTopics[{0}]", i); }
            }
        }
    }

    #endregion   

    #endregion

    #region Select Topic...
    //
    //  - - - Select Topic - - -
    //   

    #region SelectTopic
    /// <summary>
    /// Selects a topic for the turn (there will always be one)
    /// </summary>
    public void SelectTopic(GlobalSide playerSide)
    {
        if (playerSide != null)
        {
            topicGlobal = TopicGlobal.None;
            //Player must be Active or Captured
            if (CheckPlayerStatus(playerSide) == true || GameManager.i.playerScript.status == ActorStatus.Captured)
            {
                //
                // - - - topic Global 
                //
                if (GameManager.i.playerScript.status != ActorStatus.Captured)
                {
                    if (GameManager.i.optionScript.isReviews == true)
                    {
                        //chance of Performance Review topic if not captured
                        if (reviewCountdown > 0) { reviewCountdown--; }
                        if (isTestLog)
                        { Debug.LogFormat("[Tst] TopicManager.cs -> SelectTopic: reviewCountdown {0}, reviewActivationMiss {1}{2}", reviewCountdown, reviewActivationMiss, "\n"); }
                        if (reviewCountdown == 0)
                        {
                            //chance of activation on first turn of possible two turn window, automatic activation on second turn if first missed
                            if (reviewActivationMiss == false)
                            {
                                if (Random.Range(0, 100) < reviewActivationChance)
                                { topicGlobal = TopicGlobal.Review; }
                                else
                                {
                                    //automatic activation next turn
                                    reviewActivationMiss = true;
                                    topicGlobal = TopicGlobal.Decision;
                                    //next turn message
                                    string reason = string.Format("<b>A {0}Performance Review{1} is Pending</b>", colourAlert, colourEnd);
                                    string explanation = string.Format("<b>Your peers will assess you, all being well, {0}tomorrow</b>{1}", colourNormal, colourEnd);
                                    List<string> listOfHelp = new List<string>() { "review_0", "review_1", "review_2" };
                                    GameManager.i.messageScript.GeneralInfo("Review Pending (next turn warning)", "Peer Review PENDING", "Peer Review", reason, explanation, false, listOfHelp);
                                }
                            }
                            else { topicGlobal = TopicGlobal.Review; }
                        }
                        else
                        {
                            topicGlobal = TopicGlobal.Decision;
                            //Decision pending message
                            if (reviewCountdown == 1)
                            {
                                string reason = string.Format("<b>A {0}Performance Review{1} is Pending</b>", colourAlert, colourEnd);
                                string explanation = string.Format("<b>Your peers will assess you, all being well, {0}<b>within the next couple of days</b>{1}", colourNormal, colourEnd);
                                List<string> listOfHelp = new List<string>() { "review_0", "review_1", "review_2" };
                                GameManager.i.messageScript.GeneralInfo("Review Pending (next turn warning)", "Peer Review PENDING", "Peer Review", reason, explanation, false, listOfHelp);
                            }
                        }
                    }
                    else
                    {
                        //Performance Reviews toggled off
                        topicGlobal = TopicGlobal.Decision;
                    }
                    //
                    // - - - Advert, chance of
                    //
                    if (topicGlobal == TopicGlobal.Decision)
                    {
                        if (Random.Range(0, 100) < advertChance)
                        { topicGlobal = TopicGlobal.Advert; }
                    }
                }
                else
                {
                    //Captured
                    topicGlobal = TopicGlobal.Decision;
                }
                //
                // - - - Decision topic
                //

                if (topicGlobal == TopicGlobal.Decision)
                {
                    CheckTopics();
                    //select a topic, if none found then drop the global interval by 1 and try again
                    minIntervalGlobalActual = minIntervalGlobal;
                    //initialise listOfTopicTypesTurn prior to selection loop
                    CheckForValidTopicTypes();
                    if (isTestLog)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> SelectTopic: listOfTopicTypeTurn has {0} records{1}", listOfTopicTypesTurn.Count, "\n"); }
                    do
                    {
                        //reset needs to be inside the loop
                        ResetTopicAdmin();

                        if (GetTopicType() == true)
                        {
                            if (GetTopicSubType(playerSide) == true)
                            { GetTopic(playerSide); }
                        }
                        //repeat process with a reduced minInterval
                        if (turnTopic == null)
                        {
                            minIntervalGlobalActual--;
                            if (isTestLog)
                            { Debug.LogFormat("[Tst] TopicManager.cs -> SelectTopic: REPEAT LOOP, minIntervalGlobalActual now {0}{1}", minIntervalGlobalActual, "\n"); }
                        }
                        else { break; }
                    }
                    while (turnTopic == null && minIntervalGlobalActual > 0);
                    //only if a valid topic selected
                    if (turnTopic != null)
                    {
                        //debug purposes only -> BEFORE UpdateTopicTypeData
                        UnitTestTopic(playerSide);
                    }
                }

            }
        }
        else { Debug.LogError("Invalid playerSide (Null)"); }
    }
    #endregion

    #region CheckTopics
    /// <summary>
    /// checks all current topic timers and criteria and adjusts status if required
    /// </summary>
    private void CheckTopics()
    {
        Dictionary<string, Topic> dictOfTopics = GameManager.i.dataScript.GetDictOfTopics();
        if (dictOfTopics != null)
        {
            foreach (var topic in dictOfTopics)
            {
                //current topics for level only
                if (topic.Value != null)
                {
                    if (topic.Value.isCurrent == true)
                    {
                        //
                        // - - - Criteria checks (done BEFORE timer checks for sequence issues, otherwise a start timer delay of 5 turns works out to be only 4)
                        //
                        switch (topic.Value.status)
                        {
                            case Status.Dormant:
                                //do nothing
                                break;
                            case Status.Active:
                                //check criteria (even they're aren't any)
                                if (CheckTopicCriteria(topic.Value) == true)
                                {
                                    //Criteria O.K, status Live
                                    topic.Value.status = Status.Live;
                                }
                                break;
                            case Status.Live:
                                if (CheckTopicCriteria(topic.Value) == false)
                                {
                                    //Criteria FAILED, status Active
                                    topic.Value.status = Status.Active;
                                }
                                break;
                            case Status.Done:
                                //do nothing
                                break;
                            default:
                                Debug.LogWarningFormat("Unrecognised status \"{0}\" for topic \"{1}\"", topic.Value.status, topic.Value.name);
                                break;
                        }
                        //
                        // - - - Timer checks
                        //
                        switch (topic.Value.status)
                        {
                            case Status.Dormant:
                                //Start timer (turns before activating at level start)
                                if (topic.Value.timerStart > 0)
                                {
                                    //decrement timer
                                    topic.Value.timerStart--;
                                    //if Zero, status Active (no need to reset timer as it's a once only use)
                                    if (topic.Value.timerStart == 0)
                                    { topic.Value.status = Status.Active; }
                                }
                                else if (topic.Value.turnsLive > 0 && topic.Value.timerRepeat > 0)
                                {
                                    //Repeat topic (has already been Live at least once)
                                    topic.Value.timerRepeat--;
                                    //if Zero, status Active
                                    if (topic.Value.timerRepeat == 0)
                                    {
                                        topic.Value.status = Status.Active;
                                        //reset repeat timer ready for next time (profile assumed to be not Null due to SO OnEnable checks)
                                        topic.Value.timerRepeat = topic.Value.profile.delayRepeat;
                                    }
                                }
                                else
                                {
                                    if (topic.Value.linkedIndex < 0)
                                    {
                                        //normal topic -> no timers -> status Done
                                        topic.Value.status = Status.Done;
                                    }
                                    else
                                    {
                                        //Linked topic
                                        topic.Value.status = Status.Active;
                                    }
                                }
                                break;
                            case Status.Active:
                                //do nothing
                                break;
                            case Status.Live:
                                //Window timer
                                if (topic.Value.timerWindow > 0)
                                {
                                    //decrement timer
                                    topic.Value.timerWindow--;
                                    //if Zero, status Dormant
                                    if (topic.Value.timerWindow == 0)
                                    {
                                        topic.Value.status = Status.Dormant;
                                        //reset timer ready for next time
                                        topic.Value.timerWindow = topic.Value.profile.timerWindow;
                                    }
                                }
                                break;
                            case Status.Done:
                                //do nothing
                                break;
                            default:
                                Debug.LogWarningFormat("Unrecognised status \"{0}\" for topic \"{1}\"", topic.Value.status, topic.Value.name);
                                break;
                        }
                        //
                        // - - - Stat's update
                        //
                        switch (topic.Value.status)
                        {
                            case Status.Dormant:
                                topic.Value.turnsDormant++;
                                break;
                            case Status.Active:
                                topic.Value.turnsActive++;
                                break;
                            case Status.Live:
                                topic.Value.turnsLive++;
                                break;
                            case Status.Done:
                                topic.Value.turnsDone++;
                                break;
                            default:
                                Debug.LogWarningFormat("Unrecognised status \"{0}\" for topic \"{1}\"", topic.Value.status, topic.Value.name);
                                break;
                        }
                    }
                }
                else { Debug.LogWarningFormat("Invalid topic \"{0}\" (Null) in dictOfTopics", topic.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfTopics (Null)"); }
    }
    #endregion

    #region CheckForValidTopicTypes
    /// <summary>
    /// Check all level topic types to see if they are valid for the Turn. Updates listOfTopicTypesTurn with valid topicTypes
    /// TopicType.side already taken into account with listOfTopicTypesLevel
    /// </summary>
    private void CheckForValidTopicTypes()
    {
        int turn = GameManager.i.turnScript.Turn;
        //clear list at start of selection (not done in ResetTopicAdmin as it should only be done once at start, not every iteration inside the selection loop
        listOfTopicTypesTurn.Clear();
        //special case of resistance player being captured
        if (GameManager.i.playerScript.status == ActorStatus.Captured)
        {
            //Add org type, if valid. NOTE: All org subTypes have criteria re: capture so that only one activates, eg. orgEmergency
            TopicType topicType = captureType;
            TopicTypeData topicTypeData = GameManager.i.dataScript.GetTopicTypeData(topicType.name);
            CheckForValidSubType(topicType, topicTypeData, turn);
        }
        //normal
        else
        {
            //by value, not reference, as you'll be passing them onto listOfTopicTypesTurn and removing occasionally
            List<TopicType> listOfTopicTypesLevel = new List<TopicType>(GameManager.i.dataScript.GetListOfTopicTypesLevel());
            if (listOfTopicTypesLevel != null)
            {
                //loop list of Topic Types
                foreach (TopicType topicType in listOfTopicTypesLevel)
                {
                    //Topic Types can be switched on/off for debugging purposes
                    if (topicType.isDisabled == false)
                    {
                        TopicTypeData topicTypeData = GameManager.i.dataScript.GetTopicTypeData(topicType.name);
                        CheckForValidSubType(topicType, topicTypeData, turn);
                    }
                }
            }
            else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
        }
    }
    #endregion

    #region CheckForValidSubType
    /// <summary>
    /// subMethod for CheckForValidTopicTyes, handles all subType checks, auto adds topicType to listOfTopicTypesTurn if O.K
    /// </summary>
    /// <param name="topicTypeData"></param>
    private void CheckForValidSubType(TopicType topicType, TopicTypeData topicTypeData, int turn)
    {
        string criteriaCheck;
        if (topicTypeData != null)
        {
            //check topicTypeData
            if (CheckTopicTypeData(topicTypeData, turn) == true)
            {
                //check individual topicType criteria
                CriteriaDataInput criteriaInput = new CriteriaDataInput()
                { listOfCriteria = topicType.listOfCriteria };
                criteriaCheck = GameManager.i.effectScript.CheckCriteria(criteriaInput);
                if (criteriaCheck == null)
                {
                    //get list of SubTypes
                    List<TopicSubType> listOfSubTypes = topicType.listOfSubTypes;
                    if (listOfSubTypes != null)
                    {
                        bool isProceed = false;
                        //topicType needs to have at least one valid subType present
                        foreach (TopicSubType subType in listOfSubTypes)
                        {
                            if (subType.isDisabled == false && CheckSubTypeCriteria(subType) == true)
                            {
                                isProceed = true;
                                break;
                            }
                        }
                        //valid topicType / subType, add to selection pool
                        if (isProceed == true)
                        {
                            //criteria check passed O.K, add to local list of valid TopicTypes for the Turn
                            AddTopicTypeToList(listOfTopicTypesTurn, topicType);
                            if (isTestLog)
                            { Debug.LogFormat("[Tst] TopicManager.cs -> CheckForValidTopics: topicType \"{0}\" PASSED TopicTypeData check{1}", topicType.name, "\n"); }
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid listOfSubTypes (Null) for topicType \"{0}\"", topicType.name); }
                }
                else
                {
                    //criteria check FAILED
                    //generate message explaining why criteria failed -> debug only, spam otherwise
                    if (isTestLog)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> CheckForValidTopics: topicType \"{0}\" {1} Criteria check{2}", topicType.tag, criteriaCheck, "\n"); }
                }
            }
            else
            {
                if (isTestLog)
                { Debug.LogFormat("[Tst] TopicManager.cs -> CheckForValidTopics: topicType \"{0}\" Failed TopicTypeData check{1}", topicType.tag, "\n"); }
            }
        }
        else { Debug.LogError("Invalid topicTypeData (Null)"); }
    }
    #endregion

    #region ResetTopicAdmin
    /// <summary>
    /// Resets all relevant turn topic data prior to processing for the current turn and current attempt (can be multiple)i
    /// </summary>
    private void ResetTopicAdmin()
    {
        //selected topic data
        turnTopicType = null;
        turnTopicSubType = null;
        turnTopicSubSubType = null;
        turnTopic = null;
        turnSprite = null;
        turnOption = null;
        //info tags
        tagActorID = -1;
        tagActorOtherID = -1;
        tagNodeID = -1;
        tagTeamID = -1;
        tagContactID = -1;
        tagTurn = -1;
        tagHqActors = false;
        tagJob = "";
        tagLocation = "";
        tagMeds = "";
        tagDisease = "";
        tagGear = "";
        tagRecruit = "";
        tagSecretName = "";
        tagSecretTag = "";
        tagTarget = "";
        tagTeam = "";
        tagOutcome = "";
        tagOrgName = "";
        tagOrgTag = "";
        tagOrgWant = "";
        tagOrgText = "";
        tagInvestTag = "";
        tagHqActorName = "";
        tagHqOtherName = "";
        tagHqTitleActor = "";
        tagHqTitleOther = "";
        tagSpriteName = "";
        tagOptionText = "";
        tagStringData = "";
        tagRelation = ActorRelationship.None;
        tagNodeAction = NodeAction.None;
        //empty collections
        listOfTypePool.Clear();
        listOfSubTypePool.Clear();
    }
    #endregion

    #region GetTopicType
    /// <summary>
    /// Get topicType for turn Decision based off listOfTopicTypesTurn. Returns true if valid topicType found, false otherwise
    /// </summary>
    private bool GetTopicType()
    {
        bool isSuccess = false;
        int numOfEntries;
        int count = listOfTopicTypesTurn.Count;
        if (count > 0)
        {
            //build up a pool
            foreach (TopicType topicType in listOfTopicTypesTurn)
            {
                //populate pool based on priority
                numOfEntries = GetNumOfEntries(topicType.priority);
                if (numOfEntries > 0)
                {
                    for (int i = 0; i < numOfEntries; i++)
                    { listOfTypePool.Add(topicType); }
                }
            }
            if (listOfTypePool.Count > 0)
            {
                //random draw of pool
                turnTopicType = listOfTypePool[Random.Range(0, listOfTypePool.Count)];
                if (isTestLog)
                { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicType: SELECTED turnTopicType \"{0}\"", turnTopicType.name); }
                isSuccess = true;
            }
            else { Debug.LogError("Invalid listOfTypePool (Empty) for selecting topicType"); }
        }
        return isSuccess;
    }
    #endregion

    #region GetTopicSubType
    /// <summary>
    /// Get topicSubType for turn Decision. Returns true if valid subType found, false otherwise
    /// Note: playerSide checked for null by parent method
    /// </summary>
    private bool GetTopicSubType(GlobalSide playerSide)
    {
        int numOfEntries;
        bool isProceed;
        if (turnTopicType.name.Equals("Player") == true)
        { numOfEntries = 0; }
        if (turnTopicType != null)
        {
            //loop all subTypes for topicType
            for (int i = 0; i < turnTopicType.listOfSubTypes.Count; i++)
            {
                TopicSubType subType = turnTopicType.listOfSubTypes[i];
                if (subType != null)
                {
                    isProceed = false;
                    if (subType.isDisabled == false)
                    {
                        //check for same side as Player or 'both', ignore otherwise
                        if (subType.side.level == playerSide.level || subType.side.level == 3)
                        {
                            //check TopicSubTypes isValid (topicType may have been approved with some subTypes O.K and others not)
                            TopicTypeData dataSub = GameManager.i.dataScript.GetTopicSubTypeData(subType.name);
                            if (dataSub != null)
                            {
                                //subType CRITERIA check
                                if (CheckSubTypeCriteria(subType) == true)
                                {
                                    //reset 'isProceed' ready for data checks
                                    isProceed = false;
                                    if (CheckTopicTypeData(dataSub, GameManager.i.turnScript.Turn, true) == true)
                                    {
                                        //check that there are Live topics available for subType
                                        List<Topic> listOfTopics = GameManager.i.dataScript.GetListOfTopics(subType);
                                        if (listOfTopics != null)
                                        {
                                            //loop topics looking for the first valid topic (only need one) in order to validate subType
                                            for (int k = 0; k < listOfTopics.Count; k++)
                                            {
                                                Topic topic = listOfTopics[k];
                                                if (topic != null)
                                                {
                                                    if (topic.status == Status.Live)
                                                    {
                                                        isProceed = true;
                                                        break;
                                                    }
                                                }
                                                else { Debug.LogWarningFormat("Invalid topic (Null) for subType \"{0}\"", subType.name); }
                                            }
                                            if (isProceed == true)
                                            {
                                                //populate pool based on priorities
                                                numOfEntries = GetNumOfEntries(subType.priority);
                                                if (numOfEntries > 0)
                                                {
                                                    for (int j = 0; j < numOfEntries; j++)
                                                    { listOfSubTypePool.Add(subType); }
                                                }
                                            }
                                        }
                                        else { Debug.LogErrorFormat("Invalid listOfTopics (Null) for topicSubType \"{0}\"", subType.name); }
                                    }
                                }
                            }
                        }
                        /*else { Debug.LogErrorFormat("Invalid dataTopic (Null) for topicSubType \"{0}\"", subType.name); }*/
                        //O.K to have wrong side here Actor, for example, has subTypes from both sides
                    }
                    /*else { 
                                    if (isTestLog)     
                {Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicSubType: subtype \"{0}\" isDisabled True, NOT placed in selection pool{1}", subType.name, "\n"); }}*/
                }
                else { Debug.LogWarningFormat("Invalid subType (Null) for topicType \"{0}\"", turnTopicType.name); }
            }
            //valid records in selection pool?
            if (listOfSubTypePool.Count > 0)
            {
                //random draw of pool
                turnTopicSubType = listOfSubTypePool[Random.Range(0, listOfSubTypePool.Count)];
                if (isTestLog)
                { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicSubType: SELECTED turnTopicSubType \"{0}\"", turnTopicSubType.name); }
                return true;
            }
            else
            {
                if (isTestLog)
                { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicSubType: \"{0}\" Empty Pool for topicSubType selection{1}", turnTopicType.name, "\n"); }
                //remove topicType from listOfTopicTypesTurn (selection list) to prevent it being selected again given that there are no valid topics anywhere within that topicType
                if (minIntervalGlobalActual > 1)
                {
                    int index = listOfTopicTypesTurn.FindIndex(x => x.name.Equals(turnTopicType.name, StringComparison.Ordinal) == true);
                    if (index > -1)
                    {
                        if (isTestLog)
                        { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicSubType: topicType \"{0}\" REMOVED from listOfTopicTypesTurn{1}", turnTopicType.name, "\n"); }
                        listOfTopicTypesTurn.RemoveAt(index);
                    }
                }
            }
        }
        else { Debug.LogWarning("Invalid turnTopic (Null)"); }
        //O.K for there to be no valid topic
        return false;
    }
    #endregion

    #region GetTopic
    /// <summary>
    /// Get individual topic for turn Decision
    /// </summary>
    private void GetTopic(GlobalSide playerSide)
    {
        int numOfEntries;
        //clear pool (do so here as doing in ResetTopicAdmin was causing issues with debugTopicPool
        listOfTopicPool.Clear();
        //valid subType present
        if (turnTopicSubType != null)
        {
            List<Topic> listOfPotentialTopics = new List<Topic>();
            List<Topic> listOfSubTypeTopics = GameManager.i.dataScript.GetListOfTopics(turnTopicSubType);
            if (listOfSubTypeTopics != null)
            {
                GroupType group;
                switch (turnTopicSubType.name)
                {
                    //Standard topics
                    case "HQSub":
                        //two hq actors selected at random, internal politics
                        listOfPotentialTopics = GetHQSubTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "CaptureSub":
                        //based on random 50/50 roll
                        listOfPotentialTopics = GetCaptureTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    //Dynamic topic
                    case "CitySub":
                        //based on City Loyalty
                        group = GetGroupLoyalty(GameManager.i.hqScript.ApprovalResistance);
                        listOfPotentialTopics = GetTopicGroup(listOfSubTypeTopics, group, turnTopicSubType.name);
                        break;
                    case "AuthorityTeam":
                        //base on actor Opinion
                        listOfPotentialTopics = GetAuthorityTeamTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorGear":
                        //based on actor Opinion
                        listOfPotentialTopics = GetActorGearTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorPolitic":
                        //based on relationships between actors (two actors selected)
                        listOfPotentialTopics = GetActorPoliticTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorContact":
                        //based on actor Opinion
                        listOfPotentialTopics = GetActorContactTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorMatch":
                        //based on actor Compatibility
                        listOfPotentialTopics = GetActorMatchTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorDistrict":
                        //based on actor Opinion
                        listOfPotentialTopics = GetActorDistrictTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "PlayerDistrict":
                        //based on player Mood
                        listOfPotentialTopics = GetPlayerDistrictTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "PlayerGeneral":
                        //based on player Mood
                        listOfPotentialTopics = GetPlayerGeneralTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "PlayerStats":
                        //based on player Mood
                        listOfPotentialTopics = GetPlayerStatsTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "PlayerGear":
                        //based on player Mood
                        listOfPotentialTopics = GetPlayerGearTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "PlayerConditions":
                        //based on player Mood
                        listOfPotentialTopics = GetPlayerConditionTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "OrgCure":
                        //based on your Reputation with Organisation
                        listOfPotentialTopics = GetOrgCureTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "OrgContract":
                        //based on your Reputation with Organisation
                        listOfPotentialTopics = GetOrgContractTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "OrgHQ":
                        //based on your Reputation with Organisation
                        listOfPotentialTopics = GetOrgHQTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "OrgEmergency":
                        //based on your Reputation with Organisation
                        listOfPotentialTopics = GetOrgEmergencyTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "OrgInfo":
                        //based on your Reputation with Organisation
                        listOfPotentialTopics = GetOrgInfoTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "StoryAlpha":
                        //based on isStoryAlphaGood flag
                        listOfPotentialTopics = GetStoryAlphaTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "StoryBravo":
                        //based on isStoryBravoGood flag
                        listOfPotentialTopics = GetStoryBravoTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "StoryCharlie":
                        //based on isStoryCharlieGood flag
                        listOfPotentialTopics = GetStoryCharlieTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised topicSubType \"{0}\" for topic \"{1}\"", turnTopicSubType.name, turnTopic.name);
                        break;
                }

                //SubType should have provided a list of topics ready for a random draw
                if (listOfPotentialTopics != null)
                {
                    int count = listOfPotentialTopics.Count;
                    if (isTestLog)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopic: turnTopicSubType \"{0}\", listOfPotential topics {1} record{2}", turnTopicSubType.name, count, "\n"); }
                    //shouldn't be empty although can be in certain circumstances, eg. Player/actor District actions and no suitable NodeActions
                    if (count > 0)
                    {
                        //loop topics
                        for (int i = 0; i < count; i++)
                        {
                            Topic topic = listOfPotentialTopics[i];
                            if (topic != null)
                            {
                                if (topic.isDisabled == false)
                                {
                                    //check topic Live (timer and status checks O.K)
                                    if (topic.status == Status.Live)
                                    {
                                        //populate pool based on priorities
                                        numOfEntries = GetNumOfEntries(topic.priority);
                                        if (numOfEntries > 0)
                                        {
                                            for (int j = 0; j < numOfEntries; j++)
                                            { listOfTopicPool.Add(topic); }
                                        }
                                    }
                                    else
                                    {
                                        if (isTestLog)
                                        { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopic: topic \"{0}\" status {1}, NOT LIVE{2}", topic.name, topic.status, "\n"); }
                                    }
                                }
                                else
                                {
                                    if (isTestLog)
                                    { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopic: topic \"{0}\" status {1}, DISABLED{2}", topic.name, topic.status, "\n"); }
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid topic (Null) for {0}.listOfTopics[{1}]", turnTopicSubType.name, i); }
                        }
                        //Select topic
                        count = listOfTopicPool.Count;
                        if (count > 0)
                        {
                            turnTopic = listOfTopicPool[Random.Range(0, count)];
                            Debug.LogFormat("[Top] TopicManager.cs -> GetTopic: t {0}, {1} -> {2} -> {3}{4}", GameManager.i.turnScript.Turn, turnTopicType.name, turnTopicSubType.name, turnTopic.name, "\n");
                        }
                        else { Debug.LogWarningFormat("Invalid listOfTopicPool (EMPTY) for topicSubType \"{0}\"", turnTopicSubType.name); }
                    }
                    else { Debug.LogWarningFormat("Invalid listOfTopics (EMPTY) for topicSubType \"{0}\" -> INFO Only", turnTopicSubType.name); }
                }
                else { Debug.LogErrorFormat("Invalid listOfTopics (Null) for topicSubType \"{0}\"", turnTopicSubType.name); }
            }
            else { Debug.LogErrorFormat("Invalid listOfSubTypeTopics (Null) for topicSubType \"{0}\"", turnTopicSubType.name); }
        }
        else
        {
            //No topic selected
            Debug.LogFormat("[Top] TopicManager.cs -> GetTopic: t {0}, No Topic Selected{1}", GameManager.i.turnScript.Turn, "\n");
        }
    }
    #endregion

    #endregion

    #region Dynamic Topics...
    //
    // - - - Select Dynamic Topics
    //

    #region GetActorContactTopics
    /// <summary>
    /// subType ActorContact template topics selected by actor / opinion (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <returns></returns>
    private List<Topic> GetActorContactTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //get all active, onMap actors
        List<Actor> listOfActors = GameManager.i.dataScript.GetActiveActors(playerSide);
        if (listOfActors != null)
        {
            if (listOfActors.Count > 0)
            {
                //select an actor
                Actor actor = listOfActors[Random.Range(0, listOfActors.Count)];
                if (actor != null)
                {
                    //get actor opinion
                    group = GetGroupOpinion(actor.GetDatapoint(ActorDatapoint.Opinion1));
                    //if no entries use entire list by default
                    listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
                    //Info tags
                    tagActorID = actor.actorID;
                }
                else { Debug.LogWarning("Invalid actor (Null) randomly selected from listOfActors for actorContact subType"); }
            }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null) for ActorContact subType"); }

        /*//debug
        if (isTestLog)
        {        
        foreach (Topic topic in listOfTopics)
        { Debug.LogFormat("[Tst] TopicManager.cs -> GetActorContactTopics: \"{0}\"{1}", topic.name, "\n"); }
        }*/

        return listOfTopics;
    }
    #endregion

    #region GetActorMatchTopics
    /// <summary>
    /// subType ActorMatch template topics selected by player mood (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <returns></returns>
    private List<Topic> GetActorMatchTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();

        //group depends on player mood
        group = GetGroupMood(GameManager.i.playerScript.GetMood());
        //if no entries use entire list by default
        listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
        //get a candidate actor (may change later in ProcessSpecialTopicData) -> aim for actor with opinion that coresponds to group
        List<Actor> listOfActors = GameManager.i.dataScript.GetActiveActors(playerSide);
        if (listOfActors != null)
        {
            List<Actor> selectionList = new List<Actor>();
            foreach (Actor actor in listOfActors)
            {
                if (actor != null)
                {
                    switch (group)
                    {
                        case GroupType.Good:
                            //actors with opinion 2+
                            if (actor.GetDatapoint(ActorDatapoint.Opinion1) >= 2)
                            { selectionList.Add(actor); }
                            break;
                        case GroupType.Bad:
                            //actors with opinion 2-
                            if (actor.GetDatapoint(ActorDatapoint.Opinion1) <= 2)
                            { selectionList.Add(actor); }
                            break;
                    }
                }
            }
            if (selectionList.Count == 0)
            {
                //use entire list of actors if none found by group criteria
                selectionList = listOfActors;
            }
            if (selectionList.Count > 0)
            {
                Actor actor = selectionList[Random.Range(0, selectionList.Count)];
                if (actor != null)
                { tagActorID = actor.actorID; }
                //get a random nodeID (used for news items)
                Node node = GameManager.i.dataScript.GetRandomNode();
                if (node != null)
                { tagNodeID = node.nodeID; }
                else { Debug.LogError("Invalid random node (Null)"); }
            }
            else { Debug.LogWarning("Invalid selectionList (Empty)"); }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null)"); }
        return listOfTopics;
    }
    #endregion

    #region GetActorPoliticTopics
    /// <summary>
    /// subType ActorPolitic template topics selected by relationships between actors (selects from a weighted pool). Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <returns></returns>
    private List<Topic> GetActorPoliticTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //Get a random possible relationship pair of actors from a weighted pool that favours the stronger compatibilities over the weaker
        RelationSelectData data = GameManager.i.dataScript.GetPossibleRelationData();
        if (data != null)
        {
            //need data for dual actor effect and also relationship type
            tagActorID = data.actorFirstID;
            tagActorOtherID = data.actorSecondID;
            if (data.compatibility > 0) { tagRelation = ActorRelationship.FRIEND; }
            else if (data.compatibility < 0) { tagRelation = ActorRelationship.ENEMY; }
            else
            {
                //compatibility 0, 50/50 chance of Friend/Enemy relationship
                if (Random.Range(0, 100) < 50) { tagRelation = ActorRelationship.FRIEND; } else { tagRelation = ActorRelationship.ENEMY; }
            }
            //group based on Player Mood
            group = GetGroupMood(GameManager.i.playerScript.GetMood());
            //if no entries use entire list by default
            listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
        }
        return listOfTopics;
    }
    #endregion

    #region GetActorGearTopics
    /// <summary>
    /// subType ActorGear template topics selected by random actor (who has gear) based on opinion (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <returns></returns>
    private List<Topic> GetActorGearTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //get all active, onMap actors
        List<Actor> listOfActors = GameManager.i.dataScript.GetActiveActors(playerSide);
        if (listOfActors != null)
        {
            List<Actor> selectionList = new List<Actor>();
            int numOfEntries = 0;
            int numOfActors = 0;
            foreach (Actor actor in listOfActors)
            {
                if (actor != null)
                {
                    numOfActors++;
                    //actor must have gear
                    if (actor.CheckIfGear() == true)
                    {
                        //seed selection pool by opinion (the further off neutral their opinion, the more entries they get)
                        switch (actor.GetDatapoint(ActorDatapoint.Opinion1))
                        {
                            case 3: numOfEntries = 2; break;
                            case 2: numOfEntries = 1; break;
                            case 1: numOfEntries = 2; break;
                            case 0: numOfEntries = 3; break;
                        }
                        //populate selection pool
                        for (int i = 0; i < numOfEntries; i++)
                        { selectionList.Add(actor); }
                    }
                }
                else { Debug.LogWarning("Invalid Actor (Null) in listOfActors"); }
            }
            //check at least one actors present
            if (selectionList.Count > 0 && numOfActors > 0)
            {
                //randomly select an actor from unweighted list
                Actor actor = selectionList[Random.Range(0, selectionList.Count)];
                //Actor gear (null if none present)
                string gearName = actor.GetGearName();
                //proceed only if actor has gear (should do as only those with gear have been selected)
                if (string.IsNullOrEmpty(gearName) == false)
                {
                    Gear gear = GameManager.i.dataScript.GetGear(gearName);
                    if (gear != null)
                    {
                        tagGear = gear.tag;
                        group = GetGroupOpinion(actor.GetDatapoint(ActorDatapoint.Opinion1));
                        //if no entries use entire list by default
                        listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
                        //Info tags
                        tagActorID = actor.actorID;
                    }
                    else { Debug.LogWarningFormat("Invalid gear (Null) for gearName \"{0}\"", gearName); }
                }
                else { Debug.LogWarningFormat("TopicManager.cs -> GetActorGearTopics: actor ({0}, {1}, ID {2}) doesn't have gear (should do)", actor.actorName, actor.arc.name, actor.actorID); }
            }
            else
            {
                if (isTestLog)
                { Debug.LogFormat("[Tst] TopicManager.cs -> GetActorGearTopics: No topics found (No actors with Gear present){0}", "\n"); }
            }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null) for ActorGear subType"); }
        return listOfTopics;
    }
    #endregion

    #region GetActorDistrictTopics
    /// <summary>
    /// subType ActorDistrict template topics selected by random actor based on opinion (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetActorDistrictTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        int count;
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //Get all actors with at least one district action available
        List<Actor> listOfActors = GameManager.i.dataScript.GetActiveActorsSpecial(ActorCheck.NodeActionsNOTZero, playerSide);
        count = listOfActors.Count;
        if (count > 0)
        {
            //loop list and put any actor with a viable subSubType topic pool (consider recent NodeAction only) up for random selection
            List<Actor> listOfSelection = new List<Actor>();
            for (int i = 0; i < count; i++)
            {
                Actor actorTemp = listOfActors[i];
                if (actorTemp != null)
                {
                    NodeActionData dataTemp = actorTemp.GetMostRecentNodeAction();
                    turnTopicSubSubType = GetTopicSubSubType(dataTemp.nodeAction);
                    if (turnTopicSubSubType != null)
                    {

                        //check topics of this subSubType present
                        if (CheckSubSubTypeTopicsPresent(listOfSubTypeTopics, turnTopicSubSubType.name) == true)
                        {
                            //add to selection pool
                            listOfSelection.Add(actorTemp);
                            if (isTestLog)
                            {
                                Debug.LogFormat("[Tst] TopicManager.cs -> GetActorDistrictTopics: {0}, {1}, actorID {2} has a valid SubSubType \"{3}\"{4}", actorTemp.actorName, actorTemp.arc.name,
                              actorTemp.actorID, turnTopicSubSubType.name, "\n");
                            }
                        }
                        else
                        {
                            if (isTestLog)
                            {
                                Debug.LogFormat("[Tst] TopicManager.cs -> GetActorDistrictTopics: {0}, {1}, actorID {2} has an INVALID SubSubType \"{3}\"{4}", actorTemp.actorName, actorTemp.arc.name,
                                actorTemp.actorID, turnTopicSubSubType.name, "\n");
                            }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid TopicSubSubType (Null) for Actor nodeAction \"{0}\"", dataTemp.nodeAction); }
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for listOfActors[{0}]", i); }
            }
            //select a random actor (all in pool have at least one matching topics present)
            count = listOfSelection.Count;
            if (count > 0)
            {
                Actor actor = listOfSelection[Random.Range(0, count)];
                //get the most recent actor node action
                NodeActionData data = actor.GetMostRecentNodeAction();
                if (data != null)
                {
                    //update turnTopicSubType to accommodate currently selected actor, not just the last one in the  loop above
                    turnTopicSubSubType = GetTopicSubSubType(data.nodeAction);
                    //group depends on actor opinion
                    group = GetGroupOpinion(actor.GetDatapoint(ActorDatapoint.Opinion1));
                    //if no entries use entire list by default
                    listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName, turnTopicSubSubType.name);
                    //debug
                    if (isTestLog)
                    {
                        foreach (Topic topic in listOfTopics)
                        { Debug.LogFormat("[Tst] TopicManager.cs -> GetActorDistrictTopic: listOfTopics -> {0}, turn {1}{2}", topic.name, GameManager.i.turnScript.Turn, "\n"); }
                    }
                }
                else { Debug.LogErrorFormat("Invalid nodeActionData (Null) for {0}, {1}, actorID {2}", actor.actorName, actor.arc.name, actor.actorID); }
                //Info tags
                tagActorID = actor.actorID;
                tagNodeID = data.nodeID;
                tagTurn = data.turn;
                tagStringData = data.dataName;
                tagLocation = data.dataName;
                tagGear = data.dataName;
                tagRecruit = data.dataName;
                tagTarget = data.dataName;
                tagTeam = data.dataName;
            }
            else
            {
                if (isTestLog)
                { Debug.LogFormat("[Tst] TopicManager.cs -> GetActorDistrictTopics: No topics found for ActorDistrict actions for turn {0}{1}", GameManager.i.turnScript.Turn, "\n"); }
            }
        }
        else { Debug.LogWarning("No active, onMap actors present with at least one NodeAction"); }
        return listOfTopics;
    }
    #endregion

    #region GetPlayerDistrictTopicsArchive
    /// <summary>
    /// subType PlayerDistrict template topics selected by player based on mood (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetPlayerDistrictTopicsArchive(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        bool isProceed = false;
        string playerName = GameManager.i.playerScript.PlayerName;
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //get the most recent Player node action
        NodeActionData data = GameManager.i.playerScript.GetMostRecentNodeAction();
        if (data != null)
        {
            //check that it is a viable subSubType group
            turnTopicSubSubType = GetTopicSubSubType(data.nodeAction);
            if (turnTopicSubSubType != null)
            {
                //check topics of this subSubType present
                if (CheckSubSubTypeTopicsPresent(listOfSubTypeTopics, turnTopicSubSubType.name) == true)
                { isProceed = true; }
                else
                {
                    if (isTestLog)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> GetPlayerDistrictTopics: Player has an INVALID ActionTopic \"{0}\"{1}", turnTopicSubSubType.name, "\n"); }
                }
            }
            else { Debug.LogErrorFormat("invalid TopicSubSubType (Null) for Player nodeAction \"{0}\"", data.nodeAction); }
            if (isProceed == true)
            {
                //group depends on player mood
                group = GetGroupMood(GameManager.i.playerScript.GetMood());
                //if no entries use entire list by default
                listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName, turnTopicSubSubType.name);

                /*//debug
                foreach (Topic topic in listOfTopics)
                { 
                if (isTestLog)
                {Debug.LogFormat("[Tst] TopicManager.cs -> GetPlayerDistrictTopic: listOfTopics -> {0}, turn {1}{2}", topic.name, GameManager.instance.turnScript.Turn, "\n");}
                }*/

                //Info tags
                tagActorID = data.actorID;
                tagNodeID = data.nodeID;
                tagTurn = data.turn;
                tagStringData = data.dataName;
            }
        }
        else { Debug.LogErrorFormat("Invalid nodeActionData (Null) for {0}, {1}", playerName, "Player"); }
        return listOfTopics;
    }
    #endregion

    #region GetPlayerDistrictTopics
    /// <summary>
    /// subType PlayerDistrict template topics selected by player based on mood (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetPlayerDistrictTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        bool isProceed = false;
        int numToCheck = 4;
        string playerName = GameManager.i.playerScript.PlayerName;
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        NodeActionData data = null;
        //Find a recent node action with a corresponding active, onMap actor present
        List<NodeActionData> listOfNodeActions = GameManager.i.playerScript.GetListOfNodeActions();
        if (listOfNodeActions != null)
        {
            int count = listOfNodeActions.Count();
            if (count > 0)
            {
                //only check the most recent 'x' number of entries (otherwise could result in a time sink)
                count = Mathf.Min(count, numToCheck);
                //loop to get most recent first
                for (int i = 0; i < count; i++)
                {
                    data = listOfNodeActions[i];
                    if (data != null)
                    {
                        //check that it is a viable subSubType group
                        turnTopicSubSubType = GetTopicSubSubType(data.nodeAction);
                        if (turnTopicSubSubType != null)
                        {
                            //check topics of this subSubType present
                            if (CheckSubSubTypeTopicsPresent(listOfSubTypeTopics, turnTopicSubSubType.name) == true)
                            {
                                //check actor of the required arc type is present on Map (Active)
                                int slotID = CheckActorActionTypePresent(data.nodeAction);
                                if (slotID > -1)
                                {
                                    //Action and actor combo found
                                    Actor actor = GameManager.i.dataScript.GetCurrentActor(slotID, GameManager.i.sideScript.PlayerSide);
                                    if (actor != null)
                                    {
                                        if (isTestLog)
                                        {
                                            Debug.LogFormat("[Tst] TopicManager.cs -> GetPlayerDistrictTopic: SubsubType \"{0}\" has suitable actor {1}, {2}, ID {3}{4}", turnTopicSubSubType.name,
                                            actor.actorName, actor.arc.name, actor.actorID, "\n");
                                        }
                                        //tagActorID is ID of actor who had the arc for the action
                                        tagActorID = actor.actorID;
                                        isProceed = true;
                                        break;
                                    }
                                    else { Debug.LogWarningFormat("Invalid actor (Null) for slotID {0}", slotID); }
                                }
                                else
                                {
                                    if (isTestLog)
                                    { Debug.LogFormat("[Tst] TopicManager.cs -> GetPlayerDistrictTopics: No valid actor found for NodeAction {0}{1}", data.nodeAction, "\n"); }
                                }
                            }
                            else
                            {
                                if (isTestLog)
                                { Debug.LogFormat("[Tst] TopicManager.cs -> GetPlayerDistrictTopics: Player has an INVALID ActionTopic \"{0}\"{1}", turnTopicSubSubType.name, "\n"); }
                            }
                        }
                        else { Debug.LogErrorFormat("invalid TopicSubSubType (Null) for Player nodeAction \"{0}\"", data.nodeAction); }
                    }
                    else { Debug.LogErrorFormat("Invalid nodeActionData (Null) for listOfNodeActions[{0}]", i); }
                }
            }
        }
        else { Debug.LogError("Invalid listOfNodeActions (Null)"); }
        //proceed if action and actor combo found
        if (isProceed == true && data != null)
        {
            //group depends on player mood
            group = GetGroupMood(GameManager.i.playerScript.GetMood());

            //if no entries use entire list by default
            listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName, turnTopicSubSubType.name);

            /*//debug
            foreach (Topic topic in listOfTopics)
            { Debug.LogFormat("[Tst] TopicManager.cs -> GetPlayerDistrictTopic: listOfTopics -> {0}, turn {1}{2}", topic.name, GameManager.instance.turnScript.Turn, "\n"); }*/

            //Info tags
            tagNodeID = data.nodeID;
            tagTurn = data.turn;
            tagStringData = data.dataName;
            tagNodeAction = data.nodeAction;
            tagLocation = data.dataName;
            tagGear = data.dataName;
            tagRecruit = data.dataName;
            tagTarget = data.dataName;
            tagTeam = data.dataName;
        }
        return listOfTopics;
    }
    #endregion

    #region GetPlayerGeneralTopics
    /// <summary>
    /// subType PlayerGeneral template topics selected by player based on mood (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// arrayOfOptionActorIDs is initialised with actorID's (active, OnMap actors), -1 if none, where array index corresponds to option index, eg. actorID for index 0 is for option 0
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetPlayerGeneralTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //initialise array to default -1 values (no actor present)
        for (int i = 0; i < arrayOfOptionActorIDs.Length; i++)
        {
            arrayOfOptionActorIDs[i] = -1;
            arrayOfOptionInactiveIDs[i] = -1;
        }
        //All topics based on current actor line up
        Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActorsFixed(playerSide);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.i.dataScript.CheckActorSlotStatus(i, playerSide) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    {
                        //must be active
                        if (actor.Status == ActorStatus.Active)
                        {
                            //add actorID to array
                            arrayOfOptionActorIDs[i] = actor.actorID;
                        }
                        else { arrayOfOptionInactiveIDs[i] = actor.actorID; }
                    }
                }
            }
            /*//assign a random node (used only for news text tags)
            Node node = GameManager.instance.dataScript.GetRandomNode();
            if (node != null)
            { tagNodeID = node.nodeID; }
            else { Debug.LogError("Invalid random node (Null)"); }*/

            //set node to default to avoid displaying 'Show Me' button
            tagNodeID = -1;
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
        //group based on Player Mood
        group = GetGroupMood(GameManager.i.playerScript.GetMood());
        //if no entries use entire list by default
        listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
        return listOfTopics;
    }
    #endregion

    #region GetPlayerStatsTopics
    /// <summary>
    /// subType PlayerStats template topics selected by player based on player Mood. Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// arrayOfOptionActorIDs is initialised with actorID's (active, OnMap actors), -1 if none, where array index corresponds to option index, eg. actorID for index 0 is for option 0
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetPlayerStatsTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        List<int> listOfActors = new List<int>();
        //All topics based on current actor line up
        Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActorsFixed(playerSide);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.i.dataScript.CheckActorSlotStatus(i, playerSide) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    {
                        //must be active
                        if (actor.Status == ActorStatus.Active)
                        {
                            //add actorID to list
                            listOfActors.Add(actor.actorID);
                        }
                    }
                }
            }
            //default NO nodeID so that 'Show Me' button not enabled for topic UI
            tagNodeID = -1;
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
        //choose a random actor
        if (listOfActors.Count > 0)
        {
            tagActorID = listOfActors[Random.Range(0, listOfActors.Count)];
            if (tagActorID > -1)
            {
                //group based on Actor Opinion
                group = GetGroupMood(GameManager.i.playerScript.GetMood());
                //if no entries use entire list by default
                listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
            }
            else
            { Debug.LogWarningFormat("Invalid tagActorID \"{0}\", PlayerStats topic cancelled", tagActorID); }
        }
        else
        { Debug.LogWarning("Invalid listOfActors (Empty), PlayerStats topic cancelled"); }
        return listOfTopics;
    }
    #endregion

    #region GetPlayerGearTopics
    /// <summary>
    /// subType PlayerGear template topics selected by player based on player Mood. Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetPlayerGearTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        List<string> listOfGear = new List<string>();
        //All topics based of a randomly selected item of gear from player Inventory
        listOfGear = GameManager.i.playerScript.GetListOfGear();
        if (listOfGear != null)
        {
            string gearName = listOfGear[Random.Range(0, listOfGear.Count)];
            if (string.IsNullOrEmpty(gearName) == false)
            {
                Gear gear = GameManager.i.dataScript.GetGear(gearName);
                if (gear != null)
                {
                    tagGear = gear.name;
                    tagNodeID = -1;
                    if (string.IsNullOrEmpty(tagGear) == false)
                    {
                        //group based on Actor Opinion
                        group = GetGroupMood(GameManager.i.playerScript.GetMood());
                        //if no entries use entire list by default
                        listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
                    }
                    else { Debug.LogWarning("Invalid tagGear (Null or Empty)"); }
                }
                else { Debug.LogErrorFormat("Invalid gear (Null) for gearName \"{0}\"", gearName); }
            }
            else { Debug.LogWarning("Invalid gearName (Null or Empty)"); }
        }
        else { Debug.LogError("Invalid listOfGear (Null)"); }
        return listOfTopics;
    }
    #endregion

    #region GetPlayerConditionTopics
    /// <summary>
    /// subType PlayerConditions template topics selected by player based on player Mood. Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// arrayOfOptionActorIDs is initialised with actorID's (active, OnMap actors), -1 if none, where array index corresponds to option index, eg. actorID for index 0 is for option 0
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetPlayerConditionTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        List<int> listOfActors = new List<int>();
        //All topics based on current actor line up
        Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActorsFixed(playerSide);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.i.dataScript.CheckActorSlotStatus(i, playerSide) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    {
                        //must be active
                        if (actor.Status == ActorStatus.Active)
                        {
                            //add actorID to list
                            listOfActors.Add(actor.actorID);
                        }
                    }
                }
            }
            //default NO nodeID so that 'Show Me' button not enabled for topic UI
            tagNodeID = -1;
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
        //choose a random actor
        if (listOfActors.Count > 0)
        {
            tagActorID = listOfActors[Random.Range(0, listOfActors.Count)];
            if (tagActorID > -1)
            {
                //group based on Player Mood
                group = GetGroupMood(GameManager.i.playerScript.GetMood());
                //if no entries use entire list by default
                listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
            }
            else
            { Debug.LogWarningFormat("Invalid tagActorID \"{0}\", PlayerStats topic cancelled", tagActorID); }
        }
        else
        { Debug.LogWarning("Invalid listOfActors (Empty), PlayerStats topic cancelled"); }
        return listOfTopics;
    }
    #endregion

    #region GetHQSubTopics
    /// <summary>
    /// subType HqSub template topics. Returns EMPTY if none found. 
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetHQSubTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        Actor[] arrayOfHqActors = GameManager.i.dataScript.GetArrayOfActorsHQ();
        if (arrayOfHqActors != null)
        {
            //exlude 'None' and 'Boss' (Boss can't be involved in internal politics)
            int offset = 2;
            int numOfHierarchy = GameManager.i.hqScript.numOfActorsHQ;
            //get first actor from hq hierarchy
            List<int> tempList = new List<int>();
            //loop through hierarchy and get array indexes for any valid actors
            for (int i = offset; i < offset + numOfHierarchy - 1; i++)
            {
                if (arrayOfHqActors[i] != null) { tempList.Add(i); }
                else { Debug.LogWarningFormat("Invalid actor (Null) in arrayOfHqActors[{0}]", i); }
            }
            int count = tempList.Count;
            //should always be a full complement present
            if (count != numOfHierarchy - 1) { Debug.LogWarningFormat("Missing HQ hierarchy actors (is {0}, should be {1} as Boss excluded)", count, numOfHierarchy - 1); }
            //need at least two actors in HQ for topic
            if (count > 1)
            {
                int rndNum = Random.Range(0, count);
                int indexFirst = tempList[rndNum];
                //remove record to prevent dupes
                tempList.RemoveAt(rndNum);
                int indexSecond = tempList[Random.Range(0, tempList.Count)];
                //get actors from array using randomly chosen indexes
                Actor actorFirst = arrayOfHqActors[indexFirst];
                Actor actorSecond = arrayOfHqActors[indexSecond];
                if (actorFirst != null)
                {
                    if (actorSecond != null)
                    {
                        //need data for dual actor effect and also relationship type -> NOTE: actorID's are actually hqID's (O.K 'cause tagHqActors is true which caters for this in code)
                        tagHqActors = true;
                        //tagActorID is always the actor with the highest (or equal highest) opinion (optimises HQ single actor topics)
                        if (actorFirst.GetDatapoint(ActorDatapoint.Opinion1) >= actorSecond.GetDatapoint(ActorDatapoint.Opinion1))
                        {
                            tagActorID = actorFirst.hqID;
                            tagActorOtherID = actorSecond.hqID;
                            tagHqActorName = actorFirst.actorName;
                            tagHqOtherName = actorSecond.actorName;
                            tagHqTitleActor = GameManager.i.hqScript.GetHqTitle(actorFirst.statusHQ);
                            tagHqTitleOther = GameManager.i.hqScript.GetHqTitle(actorSecond.statusHQ);
                        }
                        else
                        {
                            tagActorID = actorSecond.hqID;
                            tagActorOtherID = actorFirst.hqID;
                            tagHqActorName = actorSecond.actorName;
                            tagHqOtherName = actorFirst.actorName;
                            tagHqTitleActor = GameManager.i.hqScript.GetHqTitle(actorSecond.statusHQ);
                            tagHqTitleOther = GameManager.i.hqScript.GetHqTitle(actorFirst.statusHQ);
                        }
                        //group based on faction approval
                        group = GetGroupApproval(GameManager.i.hqScript.GetHqApproval());
                        //if no entries use entire list by default
                        listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
                    }
                    else { Debug.LogWarningFormat("Invalid actorSecond (Null) for arrayOfHQActors[{0}]", indexSecond); }
                }
                else { Debug.LogWarningFormat("Invalid actorFirst (Null) for arrayOfHQActors[{0)]", indexFirst); }
            }
            else { Debug.LogWarningFormat("Not enough HQ hierarchy actors (is {0}, need at least 2", count); }
        }
        else { Debug.LogWarning("Invalid arrayOfActorsHQ (Null)"); }
        return listOfTopics;
    }
    #endregion

    #region GetCaptureTopics
    /// <summary>
    /// subType Capture templates selected based on 50/50 random roll. Returns a list of suitable Live topics. Returns Empty if none found
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetCaptureTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();

        Organisation org = GameManager.i.campaignScript.campaign.details.orgEmergency;
        if (org != null)
        {
            tagOrgName = org.name;
            tagOrgTag = org.tag;
            tagNodeID = -1;
            tagActorID = -1;
            tagNodeID = GameManager.i.nodeScript.nodeCaptured;
            //group based on player's mood (how well do they respond to the experience of being captured?)
            group = GetGroupMood(GameManager.i.playerScript.GetMood());
            //if no entries use entire list by default
            listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
        }
        else
        { Debug.LogWarning("Invalid campaign.orgEmergency (Null)"); }
        return listOfTopics;
    }
    #endregion

    #region GetOrgCureTopics
    /// <summary>
    /// subType orgCure templates selected based on player Reputation with org. Returns a list of suitable Live topics. Returns Empty if none found
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetOrgCureTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();

        Organisation org = GameManager.i.campaignScript.campaign.details.orgCure;
        if (org != null)
        {
            tagOrgName = org.name;
            tagOrgTag = org.tag;
            tagOrgWant = org.textWant;
            tagNodeID = -1;
            tagActorID = -1;
            //Get random OrgData (if available, otherwise ignore, used for 'Payback' topics)
            OrgData data = GameManager.i.dataScript.GetRandomOrgData(OrganisationType.Cure);
            if (data != null)
            {
                tagOrgText = data.text;
                tagTurn = data.turn;
            }
            //group based on player's reputation with Organisation
            group = GetGroupMood(org.GetReputation());
            //if no entries use entire list by default
            listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
        }
        else
        { Debug.LogWarning("Invalid campaign.orgCure (Null)"); }
        return listOfTopics;
    }
    #endregion

    #region GetOrgContractTopics
    /// <summary>
    /// subType orgContract templates selected based on player Reputation with org. Returns a list of suitable Live topics. Returns Empty if none found
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetOrgContractTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();

        Organisation org = GameManager.i.campaignScript.campaign.details.orgContract;
        if (org != null)
        {

            //Select an Actor with numOfConflicts (For topics 0/1 -> offer service -> contract hit, ignore for rest)
            List<Actor> listOfActors = GameManager.i.dataScript.GetActiveActorsSpecial(ActorCheck.ActorConflictNOTZero, playerSide);
            if (listOfActors != null)
            {
                if (listOfActors.Count > 0)
                {
                    Actor actor = listOfActors[Random.Range(0, listOfActors.Count)];
                    if (actor != null)
                    {
                        tagActorID = actor.actorID;
                    }
                    else { Debug.LogWarning("Invalid list of actors (Empty)"); }
                }
                /*else { Debug.LogWarning("Invalid list of actors (Null)"); } -> EDIT: applies only to topic 0/1, so no problem if none present */
            }
            else { Debug.LogWarning("Invalid actor (Null)"); }

            //applies to all
            tagOrgName = org.name;
            tagOrgTag = org.tag;
            tagOrgWant = org.textWant;
            //Get random OrgData (if available, otherwise ignore, used for 'Payback' topics)
            OrgData data = GameManager.i.dataScript.GetRandomOrgData(OrganisationType.Contract);
            if (data != null)
            {
                tagOrgText = data.text;
                tagTurn = data.turn;
            }
            //group based on player's reputation with Organisation
            group = GetGroupMood(org.GetReputation());
            //if no entries use entire list by default
            listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);

        }
        else
        { Debug.LogWarning("Invalid campaign.orgContract (Null)"); }
        return listOfTopics;
    }
    #endregion

    #region GetOrgHQTopics
    /// <summary>
    /// subType orgHQ templates selected based on player Reputation with org. Returns a list of suitable Live topics. Returns Empty if none found
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetOrgHQTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();

        Organisation org = GameManager.i.campaignScript.campaign.details.orgHQ;
        if (org != null)
        {
            tagOrgName = org.name;
            tagOrgTag = org.tag;
            tagOrgWant = org.textWant;
            //Get random OrgData (if available, otherwise ignore, used for 'Payback' topics)
            OrgData data = GameManager.i.dataScript.GetRandomOrgData(OrganisationType.HQ);
            if (data != null)
            {
                tagOrgText = data.text;
                tagTurn = data.turn;
            }
            //group based on player's reputation with Organisation
            group = GetGroupMood(org.GetReputation());
            //if no entries use entire list by default
            listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
        }
        else
        { Debug.LogWarning("Invalid campaign.orgHQ (Null)"); }
        return listOfTopics;
    }
    #endregion

    #region GetOrgEmergencyTopics
    /// <summary>
    /// subType orgEmergency templates selected based on player Reputation with org. Returns a list of suitable Live topics. Returns Empty if none found
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetOrgEmergencyTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();

        Organisation org = GameManager.i.campaignScript.campaign.details.orgEmergency;
        if (org != null)
        {
            tagOrgName = org.name;
            tagOrgTag = org.tag;
            tagOrgWant = org.textWant;
            //Get random OrgData (if available, otherwise ignore, used for 'Payback' topics)
            OrgData data = GameManager.i.dataScript.GetRandomOrgData(OrganisationType.Emergency);
            if (data != null)
            {
                tagOrgText = data.text;
                tagTurn = data.turn;
            }
            //group based on player's reputation with Organisation
            group = GetGroupMood(org.GetReputation());
            //if no entries use entire list by default
            listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
        }
        else
        { Debug.LogWarning("Invalid campaign.orgEmergency (Null)"); }
        return listOfTopics;
    }
    #endregion

    #region GetOrgInfoTopics
    /// <summary>
    /// subType orgInfo templates selected based on player Reputation with org. Returns a list of suitable Live topics. Returns Empty if none found
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetOrgInfoTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();

        Organisation org = GameManager.i.campaignScript.campaign.details.orgInfo;
        if (org != null)
        {
            tagOrgName = org.name;
            tagOrgTag = org.tag;
            tagOrgWant = org.textWant;
            tagNodeID = -1;
            tagActorID = -1;
            //Get random OrgData (if available, otherwise ignore, used for 'Payback' topics)
            OrgData data = GameManager.i.dataScript.GetRandomOrgData(OrganisationType.Info);
            if (data != null)
            {
                tagOrgText = data.text;
                tagTurn = data.turn;
            }
            //group based on player's reputation with Organisation
            group = GetGroupMood(org.GetReputation());
            //if no entries use entire list by default
            listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
        }
        else
        { Debug.LogWarning("Invalid campaign.orgInfo (Null)"); }
        return listOfTopics;
    }
    #endregion

    #region GetStoryAlphaTopics
    /// <summary>
    /// subType storyAlpha selected based on story flags. Returns a list of suitable live topics. Returns empty if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetStoryAlphaTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //group based on story flag
        if (isStoryAlphaGood == true) { group = GroupType.Good; }
        else { group = GroupType.Bad; }
        //avoids 'Show Me' button
        tagNodeID = -1;
        //if no entries then returns empty list and a warning
        listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
        return listOfTopics;
    }
    #endregion

    #region GetStoryBravoTopics
    /// <summary>
    /// subType storyBravo selected based on story flags. Returns a list of suitable live topics. Returns empty if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetStoryBravoTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //group based on story flag
        if (isStoryBravoGood == true) { group = GroupType.Good; }
        else { group = GroupType.Bad; }
        //avoids 'Show Me' button
        tagNodeID = -1;
        //if no entries then returns empty list and a warning
        listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
        return listOfTopics;
    }
    #endregion

    #region GetStoryCharlieTopics
    /// <summary>
    /// subType storyCharlie selected based on story flags. Returns a list of suitable live topics. Returns empty if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetStoryCharlieTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //group based on story flag
        if (isStoryCharlieGood == true) { group = GroupType.Good; }
        else { group = GroupType.Bad; }
        //avoids 'Show Me' button
        tagNodeID = -1;
        //if no entries then returns empty list and a warning
        listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
        return listOfTopics;
    }
    #endregion

    #region GetAuthorityTeamTopics
    /// <summary>
    /// subType ActorDistrict template topics selected by random actor based on opinion (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetAuthorityTeamTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        int count;
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //Get all actors with at least one team action available
        List<Actor> listOfActors = GameManager.i.dataScript.GetActiveActorsSpecial(ActorCheck.TeamActionsNOTZero, playerSide);
        count = listOfActors.Count;
        if (count > 0)
        {
            //loop list and put any actor with a viable subSubType topic pool (consider recent TeamAction only) up for random selection
            List<Actor> listOfSelection = new List<Actor>();
            for (int i = 0; i < count; i++)
            {
                Actor actorTemp = listOfActors[i];
                if (actorTemp != null)
                {
                    TeamActionData dataTemp = actorTemp.GetMostRecentTeamAction();
                    turnTopicSubSubType = GetTopicSubSubType(dataTemp.teamAction);
                    if (turnTopicSubSubType != null)
                    {
                        //check topics of this subSubType present
                        if (CheckSubSubTypeTopicsPresent(listOfSubTypeTopics, turnTopicSubSubType.name) == true)
                        {
                            //add to selection pool
                            listOfSelection.Add(actorTemp);
                            if (isTestLog)
                            {
                                Debug.LogFormat("[Tst] TopicManager.cs -> GetAuthorityTeamTopics: {0}, {1}, actorID {2} has a valid SubSubType \"{3}\"{4}", actorTemp.actorName, actorTemp.arc.name,
                                actorTemp.actorID, turnTopicSubSubType.name, "\n");
                            }
                        }
                        else
                        {
                            if (isTestLog)
                            {
                                Debug.LogFormat("[Tst] TopicManager.cs -> GetAuthorityTeamTopics: {0}, {1}, actorID {2} has an INVALID SubSubType \"{3}\"{4}", actorTemp.actorName, actorTemp.arc.name,
                                actorTemp.actorID, turnTopicSubSubType.name, "\n");
                            }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid TopicSubSubType (Null) for Actor teamAction \"{0}\"", dataTemp.teamAction); }
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for listOfActors[{0}]", i); }
            }
            //select a random actor (all in pool have at least one matching topics present)
            count = listOfSelection.Count;
            if (count > 0)
            {
                Actor actor = listOfSelection[Random.Range(0, count)];
                //get the most recent actor team action
                TeamActionData data = actor.GetMostRecentTeamAction();
                //update subSubType for selected actor
                turnTopicSubSubType = GetTopicSubSubType(data.teamAction);
                if (data != null)
                {
                    //group depends on actor opinion
                    group = GetGroupOpinion(actor.GetDatapoint(ActorDatapoint.Opinion1));
                    //if no entries use entire list by default
                    listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName, turnTopicSubSubType.name);
                    //debug
                    if (isTestLog)
                    {
                        foreach (Topic topic in listOfTopics)
                        { Debug.LogFormat("[Tst] TopicManager.cs -> GetAuthorityTeamTopic: listOfTopics -> {0}, turn {1}{2}", topic.name, GameManager.i.turnScript.Turn, "\n"); }
                    }
                }
                else { Debug.LogErrorFormat("Invalid teamActionData (Null) for {0}, {1}, actorID {2}", actor.actorName, actor.arc.name, actor.actorID); }
                //Info tags
                tagActorID = actor.actorID;
                tagNodeID = data.nodeID;
                tagTeamID = data.teamID;
                tagTurn = data.turn;
                tagStringData = data.dataName;
            }
            else
            {
                if (isTestLog)
                { Debug.LogFormat("[Tst] TopicManager.cs -> GetAuthorityTeamTopics: No topics found for Actor Team actions for turn {0}{1}", GameManager.i.turnScript.Turn, "\n"); }
            }
        }
        else { Debug.LogWarning("No active, onMap actors present with at least one TeamAction"); }
        return listOfTopics;
    }
    #endregion

    #endregion

    #region ProcessTopic...
    //
    // - - - Process Topic - - - 
    //

    #region ProcessTopic
    /// <summary>
    /// Process selected topic (decision or Information)
    /// </summary>
    public void ProcessTopic(GlobalSide playerSide)
    {
        if (playerSide != null)
        {
            switch (topicGlobal)
            {
                case TopicGlobal.Decision:
                    //only if Player Active
                    if (CheckPlayerStatus(playerSide) == true)
                    {
                        //valid topic selected, ignore otherwise
                        if (turnTopic != null)
                        {
                            /*if (GameManager.instance.turnScript.CheckIsAutoRun() == false) -> DEBUGGING for randomly chosen autoRun options (not required now)
                            {
                                //prepare and send data to topicUI.cs
                                InitialiseTopicUI();
                            }
                            else
                            {
                                //autorun -> randomly choose topic option (effects not implemented)
                                turnOption = turnTopic.listOfOptions[Random.Range(0, turnTopic.listOfOptions.Count)];
                                ProcessTopicAdmin();
                            }*/

                            //prepare and send data to topicUI.cs, normal, non-capture, topics
                            InitialiseTopicUI();
                        }
                    }
                    else
                    {
                        //Resistance player Captured
                        if (GameManager.i.playerScript.status == ActorStatus.Captured)
                        {
                            //valid topic selected, ignore otherwise
                            if (turnTopic != null)
                            {
                                //prepare and send data to topicUI.cs
                                InitialiseCaptureTopicUI();
                            }
                        }
                    }
                    break;
                case TopicGlobal.Review:
                    //nothing needs to be done other than some admin
                    reviewCountdown = reviewPeriod;
                    reviewActivationMiss = false;
                    break;
            }
        }
        else { Debug.LogError("Invalid playerSide (Null)"); }
    }
    #endregion

    #region InitialiseTopicUI
    /// <summary>
    /// Sends data to topicUI ready for use when activated by GUIManager.cs Pipeline
    /// </summary>
    private void InitialiseTopicUI()
    {
        if (turnTopic != null)
        {
            //Data used for reverting back to normally selected topic if Debug topic doesn't fire
            bool tagHqActorsOriginalValue = tagHqActors;
            string topicNormal = turnTopic.name;
            string subTypeNormal = turnTopicSubType.name;
            //data package
            TopicUIData data = new TopicUIData();
            data.Reset();
            //Debug initialise data package if any debug topics present (if none use normally selected topic)
            if (debugTopicPool != null)
            {
                bool isProceed = false;
                //check at least one topic in pool is live
                foreach (Topic topic in debugTopicPool.listOfTopics)
                { if (topic.status == Status.Live) { isProceed = true; break; } }
                if (isProceed == true)
                {
                    //check subType criteria
                    if (CheckSubTypeCriteria(debugTopicPool.subType) == false)
                    {
                        isProceed = false;
                        if (isTestLog)
                        { Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseTopicUI: subType \"{0}\" FAILED CRITERIA check{1}", debugTopicPool.subType.name, "\n"); }
                    }

                    if (isProceed == true)
                    {
                        if (isTestLog)
                        { Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseTopicUI: DEBUG TOPIC POOL IN USE, subType \"{0}\"{1}", debugTopicPool.subType.name, "\n"); }
                        //select one of topics from the debug pool at random (enables testing of a small subset of topics)
                        turnTopicSubType = debugTopicPool.subType;
                        if (turnTopicSubType != null)
                        {
                            //set normal topic to null prior to selecting debug topic
                            turnTopic = null;
                            Debug.LogFormat("[Top] TopicManager.cs -> InitialiseTopicUI: Topic OVERRIDE for debugTopicPool \"{0}\"{1}", debugTopicPool.name, "\n");
                            tagHqActors = false; //needed to prevent the previously selected topic, eg. HQ, sending a 'true' result forward to the debug topic
                            GetTopic(GameManager.i.sideScript.PlayerSide);
                        }
                        if (turnTopic != null)
                        {
                            //valid debug topic, align the rest with topic
                            turnTopicType = debugTopicPool.type;
                            turnTopicSubSubType = debugTopicPool.subSubType;
                            if (isTestLog)
                            { Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseTopicUI: VALID debug Topic \"{0}\"{1}", turnTopic.name, "\n"); }
                        }
                        else
                        {
                            //revert back to normally selected topic
                            turnTopic = GameManager.i.dataScript.GetTopic(topicNormal);
                            if (turnTopic != null)
                            {
                                turnTopicSubType = GameManager.i.dataScript.GetTopicSubType(subTypeNormal);
                                if (turnTopicSubType != null)
                                {
                                    tagHqActors = tagHqActorsOriginalValue;
                                    if (isTestLog)
                                    { Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseTopicUI: REVERT (No valid Debug Topic){0}", "\n"); }
                                }
                                else { Debug.LogWarningFormat("Invalid turnTopicSubType (Null) for subTypeNormal \"{0}\"", subTypeNormal); }
                            }
                            else { Debug.LogWarningFormat("Invalid topic (Null) for topicNormal.name \"{0}\"", topicNormal); }
                        }
                    }
                }
                else
                {
                    if (isTestLog)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseTopicUI: No LIVE topics found in debugTopicPool{0}", "\n"); }
                }
            }
            if (isTestLog)
            { Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseTopicUI: turnTopicSubType \"{0}\", turnTopic {1}{2}", turnTopicSubType.name, turnTopic, "\n"); }
            //special case (need to do AFTER override otherwise can be set true by pre-override topic
            bool isPlayerGeneral = false;
            if (turnTopicSubType.name.Equals("PlayerGeneral", StringComparison.Ordinal) == true)
            { isPlayerGeneral = true; }
            //use normal or debug topic
            if (turnTopic != null)
            {
                data.topicName = turnTopic.name;
                data.header = turnTopic.tag;
                data.baseType = TopicBase.Normal;
                data.isBoss = turnTopic.subType.isBoss;
                data.listOfOptions = turnTopic.listOfOptions;
                data.listOfIgnoreEffects = turnTopic.listOfIgnoreEffects;
                data.colour = GameManager.i.uiScript.TopicNormal;
                data.listOfHelp = GetTopicSubTypeHelp();
                data.listOfStoryHelp = turnTopic.listOfStoryHelp.GetRange(0, Mathf.Min(2, turnTopic.listOfStoryHelp.Count));            //only takes the first two records, if present
                //type
                switch (turnTopicSubType.name)
                {
                    case "StoryAlpha":
                        //Comms decision topics
                        data.uiType = TopicDecisionType.Comms;
                        if (turnTopic.comms != null)
                        {
                            data.text = string.Format("ATTN: {0}, {1}{2}FRM: {3}, {4}{5}{6}{7}{8}{9}{10}",
                                GameManager.i.playerScript.PlayerName, GameManager.i.cityScript.GetCityName(), "\n", turnTopic.comms.textFrom, turnTopic.comms.textWhere, "\n", "\n",
                                turnTopic.comms.textTop, "\n", "\n", turnTopic.comms.textBottom);
                        }
                        else { Debug.LogErrorFormat("Invalid comms (Null) for topic \"{0}\"", turnTopic.text); }
                        break;
                    case "StoryBravo":
                        //Letter decision topics
                        data.uiType = TopicDecisionType.Letter;
                        if (turnTopic.letter != null)
                        {
                            data.text = string.Format("Dear {0}{1}{2}{3}{4}{5}{6}",
                              turnTopic.letter.textDear, "\n", "\n", turnTopic.letter.textTop, "\n", "\n", turnTopic.letter.textBottom);
                        }
                        else { Debug.LogErrorFormat("Invalid letter (Null) for topic \"{0}\"", turnTopic.text); }
                        break;
                    default:
                        //everything else -> Normal decision topics
                        data.uiType = TopicDecisionType.Normal;
                        data.text = turnTopic.text;
                        break;
                }
                //subSubType
                turnTopicSubSubType = turnTopic.subSubType;
                //topic must have at least one option
                if (data.listOfOptions != null && data.listOfOptions.Count > 0)
                {
                    bool isProceed = true;
                    //topic specific conditions may require topic tag data to be updated
                    if (turnTopic.listOfCriteria.Count > 0)
                    {
                        if (ProcessSpecialTopicData() == true)
                        { isProceed = true; }
                        else
                        {
                            Debug.LogWarningFormat("Invalid ProcessSpecialTopicData (FAILED) for topic \"{0}\"", turnTopic.name);
                            isProceed = false;
                        }
                    }
                    //
                    // - - - options
                    //
                    if (isProceed == true)
                    {
                        isProceed = false;
                        string colourOption;
                        for (int i = 0; i < data.listOfOptions.Count; i++)
                        {
                            TopicOption option = data.listOfOptions[i];
                            if (option != null)
                            {
                                colourOption = colourNeutral;
                                //special case of multiple actor choices
                                if (isPlayerGeneral == true)
                                {
                                    if (option.optionNumber > -1)
                                    {
                                        tagActorID = arrayOfOptionActorIDs[option.optionNumber];
                                        if (isTestLog)
                                        { Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseTopicUI: optionNumber {0}, tagActorID {1}{2}", option.optionNumber, tagActorID, "\n"); }
                                        if (tagActorID > -1)
                                        {
                                            if (CheckOptionCriteria(option) == true)
                                            {
                                                //initialise option tooltips
                                                if (InitialiseOptionTooltip(option) == true)
                                                {
                                                    isProceed = true;
                                                    option.textToDisplay = string.Format("{0}{1}{2}", colourOption, CheckTopicText(option.text, false), colourEnd);
                                                }
                                            }
                                            else
                                            {
                                                //criteria checked failed
                                                isProceed = true;
                                                option.textToDisplay = string.Format("{0}{1}{2}", colourGrey, CheckTopicText(option.text, false), colourEnd);
                                            }
                                        }
                                        else
                                        {
                                            //create option unavailable tooltip
                                            InitialiseOptionUnavailableTooltip(option);
                                            //option text
                                            option.textToDisplay = string.Format("{0}{1}{2}", colourGrey, "Subordinate unavailable", colourEnd);
                                            option.isValid = false;
                                            isProceed = true;
                                        }
                                    }
                                    else { Debug.LogWarningFormat("Invalid option.optionNumber {0} for {1}", option.optionNumber, option.name); }
                                }
                                else
                                {
                                    ///Normal -> check any criteria
                                    if (CheckOptionCriteria(option) == true)
                                    {
                                        //initialise option tooltips
                                        if (InitialiseOptionTooltip(option) == true)
                                        { isProceed = true; }
                                    }
                                    else
                                    {
                                        //criteria checked failed
                                        isProceed = true;
                                        colourOption = colourGrey;
                                    }
                                    //colourFormat textToDisplay
                                    option.textToDisplay = string.Format("{0}{1}{2}", colourOption, CheckTopicText(option.text, false), colourEnd);
                                }
                            }
                            else { Debug.LogErrorFormat("Invalid topicOption (Null) in listOfOptions[{0}] for topic \"{1}\"", i, turnTopic.name); }
                        }

                        //NodeID, needed to toggle 'Show Me' button
                        data.nodeID = tagNodeID;
                        //sprite & sprite tooltip (needs to be AFTER ProcessSpecialTopicData)
                        InitialiseSprite(data);
                        data.spriteMain = turnSprite;
                        //everything checks out O.K
                        if (isProceed == true)
                        {
                            //final tag removal for topic text
                            data.text = CheckTopicText(data.text);
                            //ignore button tooltip
                            InitialiseIgnoreTooltip(data);
                            //boss details (sprite and tooltip)
                            if (turnTopic.subType.isBoss == true)
                            { InitialiseBossDetails(data); }
                            //send to TopicUI
                            GameManager.i.topicDisplayScript.InitialiseData(data);
                        }
                        else { Debug.LogErrorFormat("Invalid listOfOptions (no valid option found) for topic \"{0}\" -> No topic for this turn", turnTopic.name); }
                    }
                }
                else
                { Debug.LogWarningFormat("Invalid listOfOptions (Null or Empty) for topic \"{0}\" -> No topic this turn", turnTopic.name); }
            }
            else { Debug.LogError("Invalid topic (Null) normal, or debug"); }
        }
        //no need for error message as possible that may equal null and all that happens is that a topic isn't generated this turn
    }
    #endregion

    #region InitialiseCaptureTopicUI
    /// <summary>
    /// Sends data to topicUI ready for use when activated by GUIManager.cs Pipeline -> Specifically for case of Resistance Player Captured
    /// NOTE: Same as InitialiseTopicUI but special case (PlayerGeneral and DebugTopics) have been stripped out. Done this way for clarity
    /// </summary>
    private void InitialiseCaptureTopicUI()
    {
        if (turnTopic != null)
        {
            TopicUIData data = new TopicUIData();
            if (isTestLog)
            { Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseCaptureTopicUI: turnTopicSubType \"{0}\", turnTopic {1}{2}", turnTopicSubType.name, turnTopic, "\n"); }

            //use normal or debug topic
            if (turnTopic != null)
            {
                data.topicName = turnTopic.name;
                data.header = turnTopic.tag;
                data.text = turnTopic.text;
                data.isBoss = turnTopic.subType.isBoss;
                data.listOfOptions = turnTopic.listOfOptions;
                data.listOfIgnoreEffects = turnTopic.listOfIgnoreEffects;
                data.colour = GameManager.i.uiScript.TopicCapture;
                data.listOfHelp = GetTopicSubTypeHelp();
                //subSubType
                turnTopicSubSubType = turnTopic.subSubType;
                //topic must have at least one option
                if (data.listOfOptions != null && data.listOfOptions.Count > 0)
                {
                    bool isProceed = true;
                    //topic specific conditions may require topic tag data to be updated
                    if (turnTopic.listOfCriteria.Count > 0)
                    {
                        if (ProcessSpecialTopicData() == true)
                        { isProceed = true; }
                        else
                        {
                            Debug.LogWarningFormat("Invalid ProcessSpecialTopicData (FAILED) for topic \"{0}\"", turnTopic.name);
                            isProceed = false;
                        }
                    }
                    //
                    // - - - options
                    //
                    if (isProceed == true)
                    {
                        isProceed = false;
                        string colourOption;
                        for (int i = 0; i < data.listOfOptions.Count; i++)
                        {
                            TopicOption option = data.listOfOptions[i];
                            if (option != null)
                            {
                                colourOption = colourNeutral;
                                ///Normal -> check any criteria
                                if (CheckOptionCriteria(option) == true)
                                {
                                    //initialise option tooltips
                                    if (InitialiseOptionTooltip(option) == true)
                                    { isProceed = true; }
                                }
                                else
                                {
                                    //criteria checked failed
                                    isProceed = true;
                                    colourOption = colourGrey;
                                }
                                //colourFormat textToDisplay
                                option.textToDisplay = string.Format("{0}{1}{2}", colourOption, CheckTopicText(option.text, false), colourEnd);
                            }
                            else { Debug.LogErrorFormat("Invalid topicOption (Null) in listOfOptions[{0}] for topic \"{1}\"", i, turnTopic.name); }
                        }

                        //NodeID, needed to toggle 'Show Me' button
                        data.nodeID = tagNodeID;
                        //sprite & sprite tooltip (needs to be AFTER ProcessSpecialTopicData)
                        InitialiseSprite(data);
                        data.spriteMain = turnSprite;
                        //everything checks out O.K
                        if (isProceed == true)
                        {
                            //final tag removal for topic text
                            data.text = CheckTopicText(data.text);
                            //ignore button tooltip
                            InitialiseIgnoreTooltip(data);
                            //boss details (sprite and tooltip)
                            if (turnTopic.subType.isBoss == true)
                            { InitialiseBossDetails(data); }
                            //send to TopicUI
                            GameManager.i.topicDisplayScript.InitialiseData(data);
                        }
                        else { Debug.LogErrorFormat("Invalid listOfOptions (no valid option found) for topic \"{0}\" -> No topic for this turn", turnTopic.name); }
                    }
                }
                else
                { Debug.LogWarningFormat("Invalid listOfOptions (Null or Empty) for topic \"{0}\" -> No topic this turn", turnTopic.name); }
            }
            else { Debug.LogError("Invalid topic (Null) normal, or debug"); }
        }
        //no need for error message as possible that may equal null and all that happens is that a topic isn't generated this turn
    }
    #endregion

    #region ProcessOption
    /// <summary>
    /// process selected topic option
    /// NOTE: optionIndex range checked by parent method
    /// </summary>
    /// <param name="optionIndex"></param>
    public void ProcessOption(int optionIndex)
    {
        turnOption = turnTopic.GetOption(optionIndex);
        int rnd, threshold;
        if (turnOption != null)
        {
            //two builders for top and bottom texts
            StringBuilder builderTop = new StringBuilder();
            StringBuilder builderBottom = new StringBuilder();
            StringBuilder builderTag = new StringBuilder();         //needed to handle story info effects that are oversized (avoids this for topic message display in MainInfoApp)
            //process outcome effects / rolls / messages / etc.
            List<Effect> listOfEffects = new List<Effect>();
            //mood effects always apply (needs to come first)
            if (turnOption.moodEffect != null) { listOfEffects.Add(turnOption.moodEffect); }
            //check if probability option
            if (turnOption.chance == null)
            {
                //ordinary option
                if (turnOption.listOfGoodEffects != null) { listOfEffects.AddRange(turnOption.listOfGoodEffects); }
                if (turnOption.listOfBadEffects != null) { listOfEffects.AddRange(turnOption.listOfBadEffects); }
            }
            else
            {
                //probability option
                rnd = Random.Range(0, 100);
                threshold = 0;
                switch (turnOption.chance.name)
                {
                    case "Extreme": threshold = chanceExtreme; break;
                    case "High": threshold = chanceHigh; break;
                    case "Medium": threshold = chanceMedium; break;
                    case "Low": threshold = chanceLow; break;
                    default: Debug.LogWarningFormat("Invalid turnOption.chance \"{0}\"", turnOption.chance.name); break;
                }
                if (rnd < threshold)
                {
                    //success -> good effects apply
                    if (turnOption.listOfGoodEffects != null) { listOfEffects.AddRange(turnOption.listOfGoodEffects); }
                    builderBottom.AppendFormat("{0}SUCCESS{1}", colourNeutral, colourEnd);
                    //random message
                    string text = string.Format("\'{0}\' event, \'{1}\' option, SUCCEEDS", turnTopic.tag, turnOption.tag);
                    GameManager.i.messageScript.GeneralRandom(text, "Event Option", threshold, rnd);
                }
                else
                {
                    //fail -> bad effects apply
                    if (turnOption.listOfBadEffects != null) { listOfEffects.AddRange(turnOption.listOfBadEffects); }
                    builderBottom.AppendFormat("{0}FAILED{1}", colourCancel, colourEnd);
                    //random message
                    string text = string.Format("\'{0}\' event, \'{1}\' option, FAILS", turnTopic.tag, turnOption.tag);
                    GameManager.i.messageScript.GeneralRandom(text, "Event Option", threshold, rnd);
                }
            }
            //check valid effects present
            if (listOfEffects != null)
            {
                //set up
                EffectDataReturn effectReturn = new EffectDataReturn();
                //pass through data package
                EffectDataInput dataInput = new EffectDataInput();
                dataInput.originText = string.Format("Event {0}\'{1}\', {2}{3}", colourAlert, turnTopic.tag, turnOption.tag, colourEnd);
                dataInput.side = GameManager.i.sideScript.PlayerSide;
                dataInput.data0 = Convert.ToInt32(turnOption.isIgnoreMood);
                dataInput.source = EffectSource.Topic;
                dataInput.dataName = turnOption.name;
                if (turnTopic.name.Equals("Story", StringComparison.Ordinal) == true)
                {
                    //Story subTypes, need to pass a storyType
                    switch (turnTopicSubType.name)
                    {
                        case "StoryAlpha": dataInput.dataSpecial = (int)StoryType.Alpha; break;
                        case "StoryBravo": dataInput.dataSpecial = (int)StoryType.Bravo; break;
                        case "StoryCharlie": dataInput.dataSpecial = (int)StoryType.Charlie; break;
                        default: Debug.LogWarningFormat("Unrecognised subType.name \"{0}\"", turnTopicSubType.name); break;
                    }
                }
                //use Player node as default placeholder (actual tagNodeID is used) except in special case of player captured
                Node node = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
                //special case of PlayerGeneral topics (where each option refers to a different OnMap actor)
                if (turnTopicSubType.name.Equals("PlayerGeneral", StringComparison.Ordinal) == true)
                {
                    if (turnOption.optionNumber > -1)
                    { tagActorID = arrayOfOptionActorIDs[turnOption.optionNumber]; }
                    else { Debug.LogWarningFormat("Invalid option.optionNumber {0} for {1}", turnOption.optionNumber, turnOption.name); }
                }
                //top text (can handle text tags)
                tagOptionText = CheckTopicText(turnOption.text, false);
                builderTop.AppendFormat("{0}{1}{2}{3}{4}{5}{6}", colourNormal, turnTopic.tag, colourEnd, "\n", colourAlert, tagOptionText, colourEnd);
                if (listOfEffects.Count > 0)
                {
                    //probability option and only one effect (which would be mood)
                    if (turnOption.chance != null && listOfEffects.Count == 1)
                    {
                        //doesn't apply if ignore mood true as the single effect would be a non-mood one
                        if (turnOption.isIgnoreMood == false)
                        { builderBottom.AppendFormat("{0}{1}Nothing happened{2}", "\n", colourGrey, colourEnd); }
                    }
                    builderTag.Append(builderBottom);
                    //loop effects
                    foreach (Effect effect in listOfEffects)
                    {
                        if (node != null)
                        {
                            //process effect
                            effectReturn = GameManager.i.effectScript.ProcessEffect(effect, node, dataInput);
                            if (effectReturn != null)
                            {
                                //builderBottom
                                if (string.IsNullOrEmpty(effectReturn.bottomText) == false)
                                {
                                    if (builderBottom.Length > 0) { builderBottom.AppendLine(); }
                                    if (builderTag.Length > 0) { builderTag.AppendLine(); }
                                    //avoids oversize font for story topics (so it displays as normal size in MainInfoApp)
                                    builderTag.AppendFormat("{0}", effectReturn.bottomText);
                                    //display story topic info effects in larger text size for ease of reading
                                    if (effectReturn.isLargeText == true)
                                    {
                                        builderBottom.AppendFormat("<size=110%>{0}</size>", effectReturn.bottomText);
                                        /*Debug.LogFormat("[Tst] TopicManager.cs -> ProcessOption: effectReturn.isLargeText TRUE{0}", "\n");*/
                                    }
                                    //all else
                                    else { builderBottom.AppendFormat("{0}", effectReturn.bottomText); }
                                }
                                //exit effect loop on error
                                if (effectReturn.errorFlag == true) { break; }
                            }
                            else
                            {
                                builderBottom.AppendLine();
                                builderBottom.Append("Error");
                                builderTag.AppendLine();
                                builderTag.Append("Error");
                                effectReturn.errorFlag = true;
                                break;
                            }
                        }
                        else { Debug.LogWarningFormat("Effect \"{0}\" not processed as invalid Node (Null) for option \"{1}\"", effect.name, turnOption.name); }
                    }
                }
                else
                {
                    builderBottom.AppendFormat("{0}{1}Nothing happened{2}", "\n", colourGrey, colourEnd);
                    builderTag.Append(builderBottom);
                }
            }
            else
            { Debug.LogWarningFormat("Invalid listOfEffects (Null) for topic \"{0}\", option {1}", turnTopic.name, turnOption.name); }
            //hq boss's opinion
            if (turnTopicSubType.isBoss == true)
            {
                if (turnOption.moodEffect != null)
                {
                    Actor actorHQ = GameManager.i.dataScript.GetHqHierarchyActor(ActorHQ.Boss);
                    if (actorHQ != null)
                    {
                        int opinionChange = GameManager.i.personScript.UpdateHQOpinion(turnOption.moodEffect.belief, actorHQ, turnOption.isPreferredByHQ, turnOption.isDislikedByHQ, turnOption.isIgnoredByHQ);
                        if (opinionChange != 0)
                        {
                            int bossOpinion = GameManager.i.hqScript.GetBossOpinion();
                            bossOpinion += opinionChange;
                            GameManager.i.hqScript.SetBossOpinion(bossOpinion, string.Format("\'{0}\', \'{1}\'", turnTopic.tag, turnOption.tag));
                            builderBottom.AppendLine();
                            builderTag.AppendLine();
                            if (opinionChange > 0)
                            {
                                builderBottom.AppendFormat("{0}{1}Boss Approves of your decision{2}", "\n", colourGood, colourEnd);
                                builderTag.AppendFormat("{0}{1}Boss Approves of your decision{2}", "\n", colourGood, colourEnd);
                            }
                            else
                            {
                                builderBottom.AppendFormat("{0}{1}Boss Disapproves of your decision{2}", "\n", colourBad, colourEnd);
                                builderTag.AppendFormat("{0}{1}Boss Disapproves of your decision{2}", "\n", colourBad, colourEnd);
                            }
                        }
                    }
                    else { Debug.LogError("Invalid actorHQ (Null) for ActorHQ.Boss"); }
                }
                else
                {
                    if (turnOption.isIgnoreMood == false)
                    { Debug.LogWarningFormat("Invalid turnOption.moodEffect (Null) for option \"{0}\"", turnOption.name); }
                }
            }
            //news item
            if (string.IsNullOrEmpty(turnOption.news) == false)
            {
                string newsSnippet = CheckTopicText(turnOption.news, false);
                NewsItem item = new NewsItem() { text = newsSnippet };
                GameManager.i.dataScript.AddNewsItem(item);
                Debug.LogFormat("[Top] {0}{1}", newsSnippet, "\n");
            }
            //outcome dialogue -> For Story topics use topic Sprite, otherwise just the info sprite
            if (turnTopicType.name.Equals("Story", StringComparison.Ordinal) == true)
            { SetTopicOutcome(builderTop, builderBottom, turnSprite); }
            else { SetTopicOutcome(builderTop, builderBottom); }
            //message outcome
            tagOutcome = builderTag.ToString();
            //tidy up
            ProcessTopicAdmin();
        }
        else { Debug.LogWarningFormat("Invalid TopicOption (Null) for optionIndex \"{0}\"", optionIndex); }

    }
    #endregion

    #region ProcessIgnore
    /// <summary>
    /// Process Ignore button clicked where topic.listOfIgnoreEffects present
    /// </summary>
    public void ProcessIgnore()
    {
        //process outcome effects / rolls / messages / etc.
        List<Effect> listOfEffects = new List<Effect>();
        if (turnTopic.listOfIgnoreEffects != null) { listOfEffects.AddRange(turnTopic.listOfIgnoreEffects); }
        //two builders for top and bottom texts
        StringBuilder builderTop = new StringBuilder();
        StringBuilder builderBottom = new StringBuilder();
        //check valid effects present
        if (listOfEffects != null && listOfEffects.Count > 0)
        {
            //set up
            EffectDataReturn effectReturn = new EffectDataReturn();

            //pass through data package
            EffectDataInput dataInput = new EffectDataInput();
            dataInput.source = EffectSource.Topic;
            dataInput.originText = string.Format("{0} IGNORE", turnTopic.tag);
            dataInput.side = GameManager.i.sideScript.PlayerSide;
            Node node = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
            //top text
            builderTop.AppendFormat("{0}{1}{2}{3}{4}{5}{6}", colourNormal, turnTopic.tag, colourEnd, "\n", colourAlert, "Ignored", colourEnd);
            //loop effects
            foreach (Effect effect in listOfEffects)
            {
                if (node != null)
                {
                    //process effect
                    effectReturn = GameManager.i.effectScript.ProcessEffect(effect, node, dataInput);
                    if (effectReturn != null)
                    {
                        //builderBottom
                        if (string.IsNullOrEmpty(effectReturn.bottomText) == false)
                        {
                            if (builderBottom.Length > 0) { builderBottom.AppendLine(); }
                            builderBottom.Append(effectReturn.bottomText);
                        }
                        //exit effect loop on error
                        if (effectReturn.errorFlag == true) { break; }
                    }
                    else
                    {
                        builderTop.AppendLine();
                        builderTop.Append("Error");
                        builderBottom.AppendLine();
                        builderBottom.Append("Error");
                        effectReturn.errorFlag = true;
                        break;
                    }
                }
                else { Debug.LogWarningFormat("Effect \"{0}\" not processed as invalid Node (Null) for IGNORE", effect.name); }
            }
        }
        else { Debug.LogWarningFormat("Invalid listOfEffects (Null or Empty) for topic \"{0}\", IGNORE", turnTopic.name); }
        //boss opinion -> player notification only if there is already an outcome dialogue
        if (turnTopicSubType.isBoss == true)
        {
            ProcessIgnoreBossOpinion();
            builderBottom.AppendLine();
            builderBottom.AppendFormat("{0}{1}Your Boss Disapproves of your inability to make a decision{2}", "\n", colourBad, colourEnd);
        }
        //stats
        GameManager.i.dataScript.StatisticIncrement(StatType.TopicsIgnored);
        //outcome dialogue
        SetTopicOutcome(builderTop, builderBottom);
        //message
        tagOutcome = builderBottom.ToString();
        //tidy up
        ProcessTopicAdmin();
    }
    #endregion

    #region ProcessIgnoreBossOpinion
    /// <summary>
    /// Boss disapproves of you ignoring decisions. Needs to be a separate method as can be called by ProcessIgnore or by TopicUI.cs -> ProcessTopicIgnore depending on whether there is an outcome dialogue or not
    /// </summary>
    public void ProcessIgnoreBossOpinion()
    {
        //hq boss's opinion
        int bossOpinion = GameManager.i.hqScript.GetBossOpinion();
        bossOpinion += -1;
        GameManager.i.hqScript.SetBossOpinion(bossOpinion, string.Format("\'{0}\', Ignored", turnTopic.tag));
    }
    #endregion

    #region SetTopicOutcome
    /// <summary>
    /// Initialise outcome for selected topic option
    /// </summary>
    public void SetTopicOutcome(StringBuilder top, StringBuilder bottom, Sprite sprite = null)
    {
        string upperText, lowerText;
        //default sprite
        if (sprite == null) { sprite = GameManager.i.spriteScript.infoSprite; }
        if (top == null || top.Length == 0)
        {
            if (string.IsNullOrEmpty(turnOption.tag) == false)
            { upperText = string.Format("{0}{1}{2}{3}{4}{5}{6}", colourNeutral, turnTopic.name, colourEnd, "\n", colourAlert, turnOption.tag, colourEnd); }
            else { upperText = string.Format("{0}{1}{2} Unknown", colourBad, turnOption.name, colourEnd); }
        }
        else { upperText = top.ToString(); }
        if (bottom == null || bottom.Length == 0)
        {
            if (string.IsNullOrEmpty(turnOption.text) == false)
            { lowerText = string.Format("{0}{1}{2}", colourAlert, turnOption.text, colourEnd); }
            else { lowerText = string.Format("{0}{1}{2} Unknown", colourBad, turnOption.name, colourEnd); }
        }
        else { lowerText = bottom.ToString(); }

        ModalOutcomeDetails details = new ModalOutcomeDetails()
        {
            textTop = upperText,
            textBottom = lowerText,
            sprite = sprite,
        };
        EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
    }
    #endregion

    #region ProcessSpecialTopicData
    /// <summary>
    /// Any topic with criteria attached may required it's topic tag data to be updated prior to sending off. Criteria.EffectCriteria is used.
    /// Multiple criteria are processed from low index to high with high overriding low in the case of conflicting tag data, eg. tagContactID (So ORDER OF CRITERIA matters)
    /// Returns true if all O.K (even if no matching criteria). False if criteria special requirements aren't met
    /// NOTE: Parent method (InitialiseTopicUI) has checked at least one criteria present
    /// </summary>
    private bool ProcessSpecialTopicData()
    {
        bool isSuccess = true;
        foreach (Criteria criteria in turnTopic.listOfCriteria)
        {
            if (criteria != null)
            {
                List<Actor> listOfActors = null;
                switch (criteria.effectCriteria.name)
                {
                    case "ContactsActorMin":
                        //need to find an actor with at least one contact
                        listOfActors = GameManager.i.dataScript.GetActiveActorsSpecial(ActorCheck.ActorContactMin, GameManager.i.sideScript.PlayerSide);
                        //choose actor and update tag data
                        isSuccess = ProcessActorContact(listOfActors);
                        break;
                    case "ContactsActorNOTMax":
                        //need an actor with less than the max number of contacts allowed
                        listOfActors = GameManager.i.dataScript.GetActiveActorsSpecial(ActorCheck.ActorContactNOTMax, GameManager.i.sideScript.PlayerSide);
                        //choose actor and update tag data
                        isSuccess = ProcessActorContact(listOfActors);
                        break;
                    case "ActorsKnowSecret":
                        //need an actor with at least one secret who doesn't have blackmailer trait
                        listOfActors = GameManager.i.dataScript.GetActiveActorsSpecial(ActorCheck.KnowsSecret, GameManager.i.sideScript.PlayerSide);
                        isSuccess = ProcessActorMatch(listOfActors, criteria.effectCriteria);
                        break;
                    case "PowerActorsLess":
                        //need an actor with less power than player
                        listOfActors = GameManager.i.dataScript.GetActiveActorsSpecial(ActorCheck.PowerLess, GameManager.i.sideScript.PlayerSide);
                        isSuccess = ProcessActorMatch(listOfActors, criteria.effectCriteria);
                        break;
                    case "PowerActorsMore":
                        //need an actor with more power than player
                        listOfActors = GameManager.i.dataScript.GetActiveActorsSpecial(ActorCheck.PowerMore, GameManager.i.sideScript.PlayerSide);
                        isSuccess = ProcessActorMatch(listOfActors, criteria.effectCriteria);
                        break;
                    case "InvestigationNormal":
                        //need an investigation that meets normal criteria (status.ongoing, isOrgHQNormal false)
                        Investigation invest = GameManager.i.playerScript.GetInvestigationNormal();
                        if (invest != null)
                        { tagStringData = invest.reference; tagInvestTag = invest.tag; }
                        else { isSuccess = false; Debug.LogWarning("No valid Normal Investigation found (Null)"); }
                        break;
                    case "InvestigationTimer":
                        //need an investigation that meets timer criteria (status.resolution, outcome.Guilty, isOrgHQTimer false)
                        invest = GameManager.i.playerScript.GetInvestigationTimer();
                        if (invest != null)
                        { tagStringData = invest.reference; tagInvestTag = invest.tag; }
                        else { isSuccess = false; Debug.LogWarning("No valid Timer Investigation found (Null)"); }
                        break;
                        //default case not required as if no match then it's assumed that no update is required
                }
            }
            else { Debug.LogWarningFormat("Invalid criteria (Null) for topic \"{0}\"", turnTopic.name); }
        }
        return isSuccess;
    }
    #endregion

    #region ProcessActorContact
    /// <summary>
    /// subMethod for ProcessSpecialTopicData to choose an actor from list and populate relevant tag data. Returns true if successful, false otherwise
    /// </summary>
    /// <param name="listOfActors"></param>
    /// <returns></returns>
    private bool ProcessActorContact(List<Actor> listOfActors)
    {
        bool isSuccess = true;
        //randomly choose an actor
        if (listOfActors?.Count > 0)
        {
            Actor actor = listOfActors[Random.Range(0, listOfActors.Count)];
            if (actor != null)
            {
                //get random contact from actor
                Contact contact = actor.GetRandomContact();
                if (contact != null)
                {
                    //update tag data
                    tagActorID = actor.actorID;
                    tagContactID = contact.contactID;
                    tagNodeID = contact.nodeID;
                }
                else { isSuccess = false; }
            }
            else
            {
                Debug.LogError("Invalid actor (Null) in listOfActors");
                isSuccess = false;
            }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null or Empty)"); isSuccess = false; }
        return isSuccess;
    }
    #endregion

    #region ProcessActorMatch
    /// <summary>
    /// subMethod for ProcessSpecialTopicData to choose an actor from list and populate relevant tag data. Returns true if successful, false otherwise
    /// </summary>
    /// <param name="listOfActors"></param>
    /// <returns></returns>
    private bool ProcessActorMatch(List<Actor> listOfActors, EffectCriteria effectCriteria)
    {
        bool isSuccess = true;
        Actor actor = null;
        if (listOfActors?.Count > 0)
        {
            if (effectCriteria != null)
            {
                switch (effectCriteria.name)
                {
                    case "ActorsKnowSecret":
                        //get random actor and actor secret from list
                        actor = listOfActors[Random.Range(0, listOfActors.Count)];
                        if (actor != null)
                        {
                            tagActorID = actor.actorID;
                            Secret secret = actor.GetSecret();
                            if (secret != null)
                            {
                                tagSecretName = secret.name;
                                tagSecretTag = secret.tag;
                            }
                            else { Debug.LogWarningFormat("Invalid secret (Null) for {0}, {1}, ID {2}", actor.actorName, actor.arc.name, actor.actorID); }
                        }
                        else { Debug.LogError("Invalid actor (Null) in listOfActors"); isSuccess = false; }
                        break;
                    case "PowerActorsLess":
                    case "PowerActorsMore":
                        //get random actor from list
                        actor = listOfActors[Random.Range(0, listOfActors.Count)];
                        if (actor != null)
                        { tagActorID = actor.actorID; }
                        else { Debug.LogError("Invalid actor (Null) in listOfActors"); isSuccess = false; }
                        break;
                    default: Debug.LogWarningFormat("Unrecognised effectCriteria.name \"{0}\"", effectCriteria.name); isSuccess = false; break;
                }
            }
            else { Debug.LogWarning("Invalid effectCriteria (Null)"); isSuccess = false; }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null or Empty)"); isSuccess = false; }
        return isSuccess;
    }
    #endregion

    #region ProcessTopicAdmin
    /// <summary>
    /// tidy up at end of topic (option could have been selected or ignore button pressed (with or without ignore effects)
    /// </summary>
    public void ProcessTopicAdmin()
    {
        //tidy up stuff related to chosen topic option / stats / histor / etc.
        UpdateTopicStatus();
        UpdateTopicAdmin();
    }
    #endregion

    #region UpdateTopicAdmin
    /// <summary>
    /// handles all admin once a topic has been displayed and the user has chosen an option (or not, then default option selected)
    /// </summary>
    private void UpdateTopicAdmin()
    {
        if (turnTopic != null)
        {
            int turn = GameManager.i.turnScript.Turn;
            //update TopicType topicTypeData
            TopicTypeData typeData = GameManager.i.dataScript.GetTopicTypeData(turnTopicType.name);
            if (typeData != null)
            {
                typeData.timesUsedLevel++;
                typeData.turnLastUsed = turn;
            }
            else { Debug.LogErrorFormat("Invalid topicTypeData (Null) for turnTopicType \"{0}\"", turnTopicType.name); }
            //update TopicSubType topicTypeData
            TopicTypeData typeSubData = GameManager.i.dataScript.GetTopicSubTypeData(turnTopicSubType.name);
            if (typeSubData != null)
            {
                typeSubData.timesUsedLevel++;
                typeSubData.turnLastUsed = turn;
            }
            else { Debug.LogErrorFormat("Invalid topicTypeData (Null) for turnTopicType \"{0}\"", turnTopicType.name); }
            //Tidy up SubSubType topics
            if (turnTopicSubSubType != null)
            {
                //Actor
                if (turnTopicType.name.Equals(actorType.name, StringComparison.Ordinal) == true)
                {
                    //Actor district
                    if (turnTopicSubType.name.Equals(actorDistrictSubType.name, StringComparison.Ordinal) == true)
                    {
                        //get actor and delete most recent NodeAction record
                        Actor actor = GameManager.i.dataScript.GetActor(tagActorID);
                        if (actor != null)
                        { actor.RemoveMostRecentNodeAction(); }
                        else { Debug.LogErrorFormat("Invalid actor (Null) for tagActorID {0}", tagActorID); }
                    }
                }
                //Player
                else if (turnTopicType.name.Equals(playerType.name, StringComparison.Ordinal) == true)
                {
                    //Player District
                    if (turnTopicSubType.name.Equals(playerDistrictSubType.name, StringComparison.Ordinal) == true)
                    {
                        //delete most recent nodeAction from Player
                        GameManager.i.playerScript.RemoveLastUsedNodeAction(tagTurn, tagNodeID, tagNodeAction);
                    }
                }
                //Authority
                else if (turnTopicType.name.Equals(authorityType.name, StringComparison.Ordinal) == true)
                {
                    //Authority team
                    if (turnTopicSubType.name.Equals(authorityTeamSubType.name, StringComparison.Ordinal) == true)
                    {
                        //get actor and delete most recent TeamAction record
                        Actor actor = GameManager.i.dataScript.GetActor(tagActorID);
                        if (actor != null)
                        { actor.RemoveMostRecentTeamAction(); }
                        else { Debug.LogErrorFormat("Invalid actor (Null) for tagActorID {0}", tagActorID); }
                    }
                }
            }
            string optionName = "Ignored";
            //log
            if (turnOption != null)
            {
                optionName = turnOption.name;
                Debug.LogFormat("[Top] TopicManager.cs -> UpdateTopicAdmin: {0}, \"{1}\" SELECTED for topic {2}, \"{3}\"{4}", optionName, turnOption.tag, turnTopic.name, turnTopic.tag, "\n");
            }
            else { Debug.LogFormat("[Top] TopicManager.cs -> UpdateTopicAdmin: NO OPTION selected for topic \"{0}\"{1}", turnTopic.name, "\n"); }
            //topic Message (not for autoRun)
            if (GameManager.i.turnScript.CheckIsAutoRun() == false)
            {
                TopicMessageData data = new TopicMessageData()
                {
                    topicName = turnTopic.tag,
                    sprite = turnSprite,
                    spriteName = tagSpriteName,
                    actorID = tagActorID,
                    nodeID = tagNodeID,
                    outcome = tagOutcome
                };
                //separate to cover Ignore option
                if (turnOption != null)
                {
                    data.optionName = string.Format("{0}{1}{2}", colourAlert, tagOptionText, colourEnd);
                    data.text = string.Format("Topic \'{0}\', option {1}, actorID {2}, nodeID {3}", turnTopic.tag, turnOption.tag, tagActorID, tagNodeID);
                }
                else
                {
                    data.optionName = string.Format("{0}Event IGNORED{1}", colourAlert, colourEnd);
                    data.text = string.Format("Topic \'{0}\', option IGNORED, actorID {1}, nodeID {2}", turnTopic.tag, tagActorID, tagNodeID);
                }
                GameManager.i.messageScript.TopicDecision(data);
            }
            //topic history
            HistoryTopic history = new HistoryTopic()
            {
                turn = turn,
                numSelect = listOfTopicTypesTurn.Count,
                topicType = turnTopicType.name,
                topicSubType = turnTopicSubType.name,
                topic = turnTopic.name,
                option = optionName
            };
            GameManager.i.dataScript.AddTopicHistory(history);
            //stats
            switch (turnTopic.group.name)
            {
                case "Good": GameManager.i.dataScript.StatisticIncrement(StatType.TopicsGood); break;
                case "Bad": GameManager.i.dataScript.StatisticIncrement(StatType.TopicsBad); break;
                default: Debug.LogWarningFormat("Unrecognised group \"{0}\" for topic \"{1}\"", turnTopic.group.name, turnTopic.name); break;
            }
        }
        //no need to generate warning message as covered elsewhere
    }
    #endregion

    #region UpdateTopicStatus
    /// <summary>
    /// Update status for selected topic once interaction complete
    /// </summary>
    private void UpdateTopicStatus()
    {
        if (turnTopic != null)
        {
            //LINKED topic
            if (turnTopic.linkedIndex > -1)
            {
                //increment relevant story index (used for save/load restoring correct place in storing sequence)
                int indexChecker = 0;
                if (turnTopicType.name.Equals("Story", StringComparison.Ordinal) == true)
                {
                    switch (turnTopicSubType.name)
                    {
                        case "StoryAlpha":
                            storyAlphaCurrentIndex++;
                            Debug.LogFormat("[Sto] TopicManager.cs -> UpdateTopicStatus: storyAlphaCurrentIndex now {0}{1}", storyAlphaCurrentIndex, "\n");
                            indexChecker = storyAlphaCurrentIndex;
                            break;
                        case "StoryBravo":
                            storyBravoCurrentIndex++;
                            Debug.LogFormat("[Sto] TopicManager.cs -> UpdateTopicStatus: storyBravoCurrentIndex now {0}{1}", storyBravoCurrentIndex, "\n");
                            indexChecker = storyBravoCurrentIndex;
                            break;
                        case "StoryCharlie":
                            storyCharlieCurrentIndex++;
                            Debug.LogFormat("[Sto] TopicManager.cs -> UpdateTopicStatus: storyCharlieCurrentIndex now {0}{1}", storyCharlieCurrentIndex, "\n");
                            indexChecker = storyCharlieCurrentIndex;
                            break;
                        default: Debug.LogWarningFormat("Unrecognised turnTopicSubType \"{0}\"", turnTopicSubType.name); break;
                    }
                }
                //next topics in chain
                if (turnTopic.listOfLinkedTopics.Count > 0)
                {
                    //set all linked topics to Dormant status (they can go active next turn depending on their profile)
                    foreach (Topic topic in turnTopic.listOfLinkedTopics)
                    {
                        topic.status = Status.Dormant;
                        topic.isCurrent = true;
                        if (isTestLog)
                        { Debug.LogFormat("[Tst] TopicManager.cs -> UpdateTopicTypeData: LINKED topic \"{0}\" set to Status.Dormant{1}", topic.name, "\n"); }
                        //index check
                        if (indexChecker != topic.linkedIndex)
                        { Debug.LogWarningFormat("Mismatch on Indexex, topic \"{0}\" linkedIndex {1} (should be {2})", topic.name, topic.linkedIndex, indexChecker); }
                    }
                }
                //negate any buddy topics (current link in the chain) that weren't selected (only one topic in the each link in the chain can be selected)
                if (turnTopic.listOfBuddyTopics.Count > 0)
                {
                    //set all Buddy topics to status Done to prevent them being selected
                    foreach (Topic topic in turnTopic.listOfBuddyTopics)
                    {
                        topic.status = Status.Done;
                        topic.isCurrent = false;
                        if (isTestLog)
                        { Debug.LogFormat("[Tst] TopicManager.cs -> UpdateTopicTypeData: BUDDY topic \"{0}\" set to Status.Done{1}", topic.name, "\n"); }
                    }
                }
                //current topic (Fail Safe measure -> should already be included in listOfBuddyTopics)
                turnTopic.status = Status.Done;
            }
            //non-linked topic
            else
            {
                //Current topic -> Back to Dormant if repeat, Done otherwise
                if (turnTopic.timerRepeat > 0)
                {
                    turnTopic.status = Status.Dormant;
                    turnTopic.isCurrent = true;
                }
                else
                {
                    turnTopic.status = Status.Done;
                    turnTopic.isCurrent = false;
                }
            }
        }
        else { Debug.LogError("Invalid turnTopic (Null)"); }
    }
    #endregion

    #endregion

    #region CheckTopicTypeData
    //
    // - - - TopicTypeData - - -
    //

    /// <summary>
    /// returns true if Topic Type Data check passes, false otherwise. 
    /// TopicTypes test for global and topicTypeData.minIntervals
    /// TopicSubTypes test for topicTypeData.isAvailable and topicTypeData.minIntervals
    /// </summary>
    /// <param name="data"></param>
    /// <param name="turn"></param>
    /// <param name="isTopicSubType"></param>
    /// <returns></returns>
    private bool CheckTopicTypeData(TopicTypeData data, int turn, bool isTopicSubType = false)
    {
        int interval;
        bool isValid = false;
        bool isProceed = true;
        //accomodate the first few turns that may be less than the minTopicTypeTurns value
        int minOverallInterval = Mathf.Min(turn, minIntervalGlobalActual);
        if (data != null)
        {
            switch (isTopicSubType)
            {
                case false:
                    //TopicTypes -> check global interval & data.minInterval -> check for global interval
                    if ((turn - data.turnLastUsed) >= minOverallInterval)
                    { isValid = true; }
                    else
                    {
                        isProceed = false;
                        isValid = false;
                    }
                    if (isProceed == true)
                    {
                        //edge case, level start, hasn't been used yet
                        if (data.timesUsedLevel == 0)
                        { interval = Mathf.Min(data.minInterval, turn); }
                        else { interval = data.minInterval; }
                        //check for minimum Interval
                        if ((turn - data.turnLastUsed) >= interval)
                        { isValid = true; }
                        else { isValid = false; }
                    }
                    break;
                case true:
                    //TopicSubTypes -> check isAvailable & data.minInterval
                    if (data.isAvailable == true)
                    {
                        isProceed = true;
                        isValid = true;
                    }
                    else { isProceed = false; }
                    if (isProceed == true)
                    {
                        //edge case, level start, hasn't been used yet
                        if (data.timesUsedLevel == 0)
                        { interval = Mathf.Min(data.minInterval, turn); }
                        else { interval = data.minInterval; }
                        //check for minimum Interval
                        if ((turn - data.turnLastUsed) >= interval)
                        { isValid = true; }
                        else { isValid = false; }
                    }
                    break;
            }
        }
        else { Debug.LogError("Invalid TopicTypeData (Null)"); }
        return isValid;
    }
    #endregion

    #region  Criteria Checks...
    //
    // - - - Criteria Checks - - -
    //

    #region CheckTopicCriteria
    /// <summary>
    /// Check a topic's criteria, returns true if all criteria O.K (or none if first instance), false if any failed
    /// NOTE: topic checked for Null by calling method
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    private bool CheckTopicCriteria(Topic topic)
    {
        bool isCheck = false;
        StoryType storyType = StoryType.None;
        //topic criteria must pass checks
        if (topic.listOfCriteria != null && topic.listOfCriteria.Count > 0)
        {
            //special cases
            switch (topic.type.name)
            {
                case "Organisation":
                    //Organisation subTypes, need an name for EffectManager.cs -> CheckCriteria
                    switch (topic.subType.name)
                    {
                        case "OrgCure": tagOrgName = GameManager.i.campaignScript.campaign.details.orgCure.name; break;
                        case "OrgContract": tagOrgName = GameManager.i.campaignScript.campaign.details.orgContract.name; break;
                        case "OrgHQ": tagOrgName = GameManager.i.campaignScript.campaign.details.orgHQ.name; break;
                        case "OrgEmergency": tagOrgName = GameManager.i.campaignScript.campaign.details.orgEmergency.name; break;
                        case "OrgInfo": tagOrgName = GameManager.i.campaignScript.campaign.details.orgInfo.name; break;
                        default: Debug.LogWarningFormat("Unrecognised subType.name \"{0}\"", topic.subType.name); break;
                    }
                    break;
                case "Story":
                    //Story subTypes, need to pass a storyType
                    switch (topic.subType.name)
                    {
                        case "StoryAlpha": storyType = StoryType.Alpha; break;
                        case "StoryBravo": storyType = StoryType.Bravo; break;
                        case "StoryCharlie": storyType = StoryType.Charlie; break;
                        default: Debug.LogWarningFormat("Unrecognised subType.name \"{0}\"", topic.subType.name); break;
                    }
                    break;
            }
            //data package to pass to EffectManager.cs
            CriteriaDataInput criteriaInput = new CriteriaDataInput()
            {
                listOfCriteria = topic.listOfCriteria,
                orgName = tagOrgName,
                storyType = storyType
            };
            string criteriaCheck = GameManager.i.effectScript.CheckCriteria(criteriaInput);
            if (criteriaCheck == null)
            {
                //criteria check passed O.K
                isCheck = true;
            }
            else
            {
                //criteria check FAILED
                isCheck = false;

                //generate message explaining why criteria failed -> debug only, spam otherwise
                if (topic.subType.name.Equals("OrgCure", StringComparison.Ordinal) == true)
                {
                    if (isTestLog)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> CheckTopicCriteria: topic \"{0}\", Criteria FAILED \"{1}\"{2}", topic.name, criteriaCheck, "\n"); }
                }
            }
        }
        else
        {
            //no criteria present
            isCheck = true;
        }
        return isCheck;
    }
    #endregion

    #region CheckTopicsAvailable
    /// <summary>
    /// Checks any topicType for availability this Turn using a cascading series of checks that exits on the first positive outcome. 
    /// used by EffectManager.cs -> CheckCriteria
    /// </summary>
    /// <param name="topicType"></param>
    /// <returns></returns>
    public bool CheckTopicsAvailable(TopicType topicType, int turn)
    {
        bool isValid = false;
        if (topicType != null)
        {
            //loop subTypes
            for (int i = 0; i < topicType.listOfSubTypes.Count; i++)
            {
                TopicSubType subType = topicType.listOfSubTypes[i];
                if (subType != null)
                {
                    //check subType pool present
                    List<Topic> listOfTopics = GameManager.i.dataScript.GetListOfTopics(subType);
                    if (listOfTopics != null)
                    {
                        //check subType topicTypeData
                        TopicTypeData dataSub = GameManager.i.dataScript.GetTopicSubTypeData(subType.name);
                        if (dataSub != null)
                        {
                            if (CheckTopicTypeData(dataSub, turn, true) == true)
                            {
                                /*if (dataSub.minInterval > 0)
                                {
                                                    if (isTestLog)
                                                    {
                                    Debug.LogFormat("[Tst] TopicManager.cs -> CheckTopicAvailable: \"{0}\", t {1}, l {2}, m {3}, isValid {4}{5}", subType.name,
                                        turn, dataSub.turnLastUsed, dataSub.minInterval, "True", "\n");
                                        }
                                }*/

                                //check subType criteria (bypass the topic checks if fail) 
                                if (CheckSubTypeCriteria(subType) == true)
                                {
                                    //loop pool of topics looking for any that are Live. Break on the first active topic (only needs to be one)
                                    for (int j = 0; j < listOfTopics.Count; j++)
                                    {
                                        Topic topic = listOfTopics[j];
                                        if (topic != null)
                                        {
                                            if (topic.isDisabled == false)
                                            {
                                                //check status
                                                if (topic.status == Status.Live)
                                                {
                                                    //at least one valid topic present, exit
                                                    isValid = true;
                                                    break;
                                                }
                                            }
                                        }
                                        else { Debug.LogErrorFormat("Invalid topic (Null) for subTopicType \"{0}\" listOfTopics[{1}]", subType.name, j); }
                                    }
                                    //check successful if any one subType of the topicType is valid
                                    if (isValid == true)
                                    { break; }
                                }
                            }
                            else
                            {
                                /*if (dataSub.minInterval > 0)
                                {
                                                    if (isTestLog)
                                                    {
                                    Debug.LogFormat("[Tst] TopicManager.cs -> CheckTopicAvailable: \"{0}\", t {1}, l {2}, m {3}, isValid {4}{5}", subType.name,
                                        turn, dataSub.turnLastUsed, dataSub.minInterval, "False", "\n");
                                        }
                                }*/
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid topicTypeData (Null) for \"{0} / {1}\"", topicType.name, subType.name); }
                    }
                }
                else { Debug.LogErrorFormat("Invalid subType (Null) for topic \"{0}\" listOfSubTypes[{1}]", topicType.name, i); }
            }
        }
        return isValid;
    }
    #endregion

    #region CheckSubTypeCriteria
    /// <summary>
    /// checks individual TopicSubType criteria and returns true if none present or if all O.K. False if criteria check fails.
    /// </summary>
    /// <param name="subType"></param>
    /// <returns></returns>
    private bool CheckSubTypeCriteria(TopicSubType subType)
    {
        //check subTopic criteria
        if (subType.listOfCriteria != null && subType.listOfCriteria.Count > 0)
        {
            string criteriaCheck;
            //special case of Organisation subTypes, need an name for EffectManager.cs -> CheckCriteria
            if (subType.type.name.Equals("Organisation", StringComparison.Ordinal) == true)
            {
                switch (subType.name)
                {
                    case "OrgCure": tagOrgName = GameManager.i.campaignScript.campaign.details.orgCure.name; break;
                    case "OrgContract": tagOrgName = GameManager.i.campaignScript.campaign.details.orgContract.name; break;
                    case "OrgHQ": tagOrgName = GameManager.i.campaignScript.campaign.details.orgHQ.name; break;
                    case "OrgEmergency": tagOrgName = GameManager.i.campaignScript.campaign.details.orgEmergency.name; break;
                    case "OrgInfo": tagOrgName = GameManager.i.campaignScript.campaign.details.orgInfo.name; break;
                    default: Debug.LogWarningFormat("Unrecognised subType.name \"{0}\"", subType.name); break;
                }
            }
            //check individual topicSubType criteria
            CriteriaDataInput criteriaInput = new CriteriaDataInput()
            {
                listOfCriteria = subType.listOfCriteria,
                orgName = tagOrgName
            };
            criteriaCheck = GameManager.i.effectScript.CheckCriteria(criteriaInput);
            if (criteriaCheck != null)
            {
                if (isTestLog)
                { Debug.LogFormat("[Tst] TopicManager.cs -> CheckSubTypeCriteria: \"{0}\" FAILED criteria check -> {1}{2}", subType.name, criteriaCheck, "\n"); }
                return false;

            }
            else
            {
                if (isTestLog)
                { Debug.LogFormat("[Tst] TopicManager.cs -> CheckSubTypeCriteria: \"{0}\" PASSED criteria check{1}", subType.name, "\n"); }
            }
        }
        return true;
    }
    #endregion

    #endregion

    #region Unit Tests
    //
    // - - - Unit Tests - - -
    //

    /// <summary>
    /// runs a turn based unit test on selected topictype/subtype/topic looking for anything out of place
    /// NOTE: checks a lot of stuff that should have already been checked but acts as a final check and picks up stuff in the case of code changes that might have thrown something out of alignment
    /// NOTE: playerSide checked for Null by parent method
    /// </summary>
    private void UnitTestTopic(GlobalSide playerSide)
    {
        //check if at least one entry in listOfTopicTypesTurn
        if (listOfTopicTypesTurn.Count > 0)
        {
            int turn = GameManager.i.turnScript.Turn;
            int interval;
            //
            // - - - topicType
            //
            if (turnTopic != null)
            {
                TopicTypeData dataType = GameManager.i.dataScript.GetTopicTypeData(turnTopicType.name);
                if (dataType != null)
                {
                    //check global interval
                    int minOverallInterval = Mathf.Min(turn, minIntervalGlobalActual);
                    if ((turn - dataType.turnLastUsed) < minOverallInterval)
                    { Debug.LogWarningFormat("Invalid topicType \"{0}\" Global Interval (turn {1}, last used t {2}, minGlobaIInterval {3})", turnTopic.name, turn, dataType.turnLastUsed, minOverallInterval); }
                    //check topicType minInterval
                    if (dataType.minInterval > 0)
                    {
                        //edge case, level start, hasn't been used yet
                        if (dataType.timesUsedLevel == 0)
                        { interval = Mathf.Min(dataType.minInterval, turn); }
                        else { interval = dataType.minInterval; }
                        //check
                        if ((turn - dataType.turnLastUsed) < interval)
                        { Debug.LogWarningFormat("Invalid topicType \"{0}\" Min Interval (turn {1}, last used t {2}, minInterval {3})", turnTopic.name, turn, dataType.turnLastUsed, dataType.minInterval); }
                    }
                    //check topicType on list
                    if (listOfTopicTypesTurn.Exists(x => x.name.Equals(turnTopicType.name, StringComparison.Ordinal)) == false)
                    { Debug.LogWarningFormat("Invalid topicType \"{0}\" NOT found in listOfTopicTypeTurn", turnTopicType.name); }
                }
                else { Debug.LogWarningFormat("Invalid topicTypeData (Null) for topicType \"{0}\"", turnTopic.name); }
            }
            else { Debug.LogWarning("Invalid topicType (Null)"); }
            //
            // - - - topicSubType
            //
            if (turnTopicSubType != null)
            {
                TopicTypeData dataSubType = GameManager.i.dataScript.GetTopicSubTypeData(turnTopicSubType.name);
                if (dataSubType != null)
                {
                    //check isAvailable 
                    if (dataSubType.isAvailable == false)
                    { Debug.LogWarningFormat("Invalid topicSubType \"{0}\" dataTopic.isAvailable \"{1}\" (should be True)", turnTopicSubType.name, dataSubType.isAvailable); }
                    //check topicSubType minInterval
                    if (dataSubType.minInterval > 0)
                    {
                        //edge case, level start, hasn't been used yet
                        if (dataSubType.timesUsedLevel == 0)
                        { interval = Mathf.Min(dataSubType.minInterval, turn); }
                        else { interval = dataSubType.minInterval; }
                        //check
                        if ((turn - dataSubType.turnLastUsed) < interval)
                        {
                            Debug.LogWarningFormat("Invalid topicSubType \"{0}\" minInterval (turn {1}, last used t {2}, minInterval {3})",
                                turnTopicSubType.name, turn, dataSubType.turnLastUsed, dataSubType.minInterval);
                        }
                    }
                    //check subType child of TopicType parent
                    if (turnTopicSubType.type.name.Equals(turnTopicType.name, StringComparison.Ordinal) == false)
                    { Debug.LogWarningFormat("Invalid topicSubType \"{0}\" has MISMATCH with topicType parent (is {1}, should be {2})", turnTopicSubType.name, turnTopicSubType.type.name, turnTopicType.name); }
                }
                else { Debug.LogWarningFormat("Invalid topicTypeData for topicSubType \"{0}\"", turnTopicSubType.name); }
            }
            else { Debug.LogWarning("Invalid topicSubType (Null)"); }
            //
            // - - - Topic
            //
            if (turnTopic != null)
            {
                //status active
                if (turnTopic.status != Status.Live)
                { Debug.LogWarningFormat("Invalid topic \"{0}\" status \"{1}\" (should be {2})", turnTopic.name, turnTopic.status, Status.Live); }
                //check correct topicSubType,
                if (turnTopic.subType.name.Equals(turnTopicSubType.name, StringComparison.Ordinal) == false)
                { Debug.LogWarningFormat("Invalid topic \"{0}\" has MISMATCH with topicSubType parent (is {1}, should be {2})", turnTopic.name, turnTopic.subType.name, turnTopicSubType.name); }
                //check correct topicType
                if (turnTopic.type.name.Equals(turnTopicType.name, StringComparison.Ordinal) == false)
                { Debug.LogWarningFormat("Invalid topic \"{0}\" has MISMATCH with topicType parent (is {1}, should be {2})", turnTopic.name, turnTopic.type.name, turnTopicType.name); }
                //check correct side
                if (turnTopic.side.level != playerSide.level)
                { Debug.LogWarningFormat("Invalid topic \"{0}\" with INCORRECT SIDE (is {1}, should be {2})", turnTopic.name, turnTopic.side.name, playerSide.name); }
            }
            else { Debug.LogWarning("Invalid topic (Null)"); }
        }
        else
        { /*Debug.LogWarning("Invalid listOfTopicTypesTurn (Empty)");*/ }
    }
    #endregion

    #region Utilities...
    //
    // - - - Utilities - - -
    //

    #region GetGroup Methods
    /// <summary>
    /// returns groupType based on Actor Opinions, returns Neutral if a problem
    /// </summary>
    /// <param name="opinion"></param>
    /// <returns></returns>
    private GroupType GetGroupOpinion(int opinion)
    {
        GroupType group = GroupType.Neutral;
        switch (opinion)
        {
            case 3: group = GroupType.Good; break;
            case 2: group = GroupType.Neutral; break;
            case 1: group = GroupType.Bad; break;
            case 0: group = GroupType.VeryBad; break;
            default: Debug.LogWarningFormat("Unrecognised Actor Opinion \"{0}\", default GroupType.Neutral used", opinion); break;
        }
        return group;
    }

    /// <summary>
    /// returns groupType based on Player Mood, returns Neutral if a problem
    /// </summary>
    /// <param name="mood"></param>
    /// <returns></returns>
    private GroupType GetGroupMood(int mood)
    {
        GroupType group = GroupType.Neutral;
        switch (mood)
        {
            case 3: group = GroupType.Good; break;
            case 2: group = GroupType.Neutral; break;
            case 1: group = GroupType.Bad; break;
            case 0: group = GroupType.VeryBad; break;
            default: Debug.LogWarningFormat("Unrecognised Player Mood \"{0}\" for {1}, {2}. Default GroupType.Neutral used", mood, GameManager.i.playerScript.PlayerName, "Player"); break;
        }
        return group;
    }

    /// <summary>
    /// returns GroupType based on Actor Compatability, returns Neutral if a problem
    /// </summary>
    /// <param name="compatibility"></param>
    /// <returns></returns>
    private GroupType GetGroupCompatibility(int compatibility)
    {
        GroupType group = GroupType.Neutral;
        switch (compatibility)
        {
            case 3: group = GroupType.Good; break;
            case 2: group = GroupType.Good; break;
            case 1: group = GroupType.Good; break;
            case -1: group = GroupType.Bad; break;
            case -2: group = GroupType.VeryBad; break;
            case -3: group = GroupType.VeryBad; break;
            default: Debug.LogWarningFormat("Unrecognised compatibility \"{0}\". default GroupType.Neutral used", compatibility); break;
        }
        return group;
    }

    /// <summary>
    /// returns GroupType based on Faction Approval, returns Neutral if a problem
    /// </summary>
    /// <param name="approval"></param>
    /// <returns></returns>
    private GroupType GetGroupApproval(int approval)
    {
        GroupType group = GroupType.Neutral;
        switch (approval)
        {
            case 10: group = GroupType.Good; break;
            case 9: group = GroupType.Good; break;
            case 8: group = GroupType.Good; break;
            case 7: group = GroupType.Good; break;
            case 6: group = GroupType.Neutral; break;
            case 5: group = GroupType.Neutral; break;
            case 4: group = GroupType.Neutral; break;
            case 3: group = GroupType.Bad; break;
            case 2: group = GroupType.Bad; break;
            case 1: group = GroupType.Bad; break;
            case 0: group = GroupType.Bad; break;
            default: Debug.LogWarningFormat("Unrecognised HQ Approval \"{0}\", GroupType.Neutral returned", approval); break;
        }
        return group;
    }

    /// <summary>
    /// returns GroupType based on City Loyalty, returns Neutral if a problem
    /// </summary>
    /// <param name="loyalty"></param>
    /// <returns></returns>
    private GroupType GetGroupLoyalty(int loyalty)
    {
        GroupType group = GroupType.Neutral;
        switch (loyalty)
        {
            case 10: group = GroupType.Good; break;
            case 9: group = GroupType.Good; break;
            case 8: group = GroupType.Good; break;
            case 7: group = GroupType.Good; break;
            case 6: group = GroupType.Neutral; break;
            case 5: group = GroupType.Neutral; break;
            case 4: group = GroupType.Neutral; break;
            case 3: group = GroupType.Bad; break;
            case 2: group = GroupType.Bad; break;
            case 1: group = GroupType.Bad; break;
            case 0: group = GroupType.Bad; break;
            default: Debug.LogWarningFormat("Unrecognised City Loyalty \"{0}\", GroupType.Neutral returned", loyalty); break;
        }
        return group;
    }

    /// <summary>
    /// returns good/bad on a 50/50 random roll
    /// </summary>
    /// <returns></returns>
    private GroupType GetGroupRandom()
    {
        GroupType group = GroupType.Good;
        if (Random.Range(0, 100) < 50)
        { group = GroupType.Bad; }
        return group;
    }

    #endregion

    #region AddTopicTypeToList
    /// <summary>
    /// add a topicType item to a list, only if not present already (uses TopicType.name for dupe checks)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="item"></param>
    private void AddTopicTypeToList(List<TopicType> list, TopicType item)
    {
        if (list != null)
        {
            if (item != null)
            {
                //autoAdd first item
                if (list.Count > 0)
                {
                    //add only if not already in list
                    if (list.Exists(x => x.name.Equals(item.name, StringComparison.Ordinal)) == false)
                    { list.Add(item); }
                }
                else { list.Add(item); }
            }
            else { Debug.LogError("Invalid TopicType item (Null)"); }
        }
        else { Debug.LogError("Invalid TopicType list (Null)"); }
    }
    #endregion

    #region GetNumOfEntries
    /// <summary>
    /// Returns the number of entries of a TopicType/SubType to place into a selection pool based on the items priority. Returns 0 if a problem.
    /// </summary>
    /// <param name="priority"></param>
    /// <returns></returns>
    private int GetNumOfEntries(GlobalChance priority)
    {
        int numOfEntries = 0;
        if (priority != null)
        {
            switch (priority.name)
            {
                case "Low":
                    numOfEntries = 1;
                    break;
                case "Medium":
                    numOfEntries = 2;
                    break;
                case "High":
                    numOfEntries = 3;
                    break;
                case "Extreme":
                    numOfEntries = 5;
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised priority.name \"{0}\"", priority.name);
                    break;
            }
        }
        else { Debug.LogError("Invalid priority (Null)"); }
        return numOfEntries;
    }
    #endregion

    #region GetTopicGroup
    /// <summary>
    /// Returns a list Of Live topics filtered out of the inputList by group type, eg. Good / Bad, etc. If Neutral then chanceNeutralGood (75%) good, 100 - chanceNeutralGood (25%) bad
    /// If no topic found of the correct group then all used.
    /// Returns EMPTY list if a problem
    /// </summary>
    /// <param name="inputList"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    private List<Topic> GetTopicGroup(List<Topic> inputList, GroupType group, string subTypeName = "Unknown", string subSubTypeName = "")
    {
        List<Topic> listOfTopics = new List<Topic>();
        if (inputList != null)
        {
            //normal
            if (string.IsNullOrEmpty(subSubTypeName) == true)
            {
                switch (group)
                {
                    case GroupType.Good:
                        //high opinion, good group
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                        t.group.name.Equals("Good", StringComparison.Ordinal)).ToList());
                        break;
                    case GroupType.Neutral:
                        //neutral opinion, use all Active topics
                        if (Random.Range(0, 100) < chanceNeutralGood)
                        { listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live && t.group.name.Equals("Good", StringComparison.Ordinal)).ToList()); }
                        else
                        { listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live && t.group.name.Equals("Bad", StringComparison.Ordinal)).ToList()); }
                        break;
                    case GroupType.Bad:
                    case GroupType.VeryBad:
                        //low opinion, bad group
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                        t.group.name.Equals("Bad", StringComparison.Ordinal)).ToList());
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised GroupType \"[0}\"", group);
                        break;
                }
                //if no entries use entire list by default
                if (listOfTopics.Count == 0)
                {
                    if (isTestLog)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicGroup: No topics found for \"{0}\", group \"{1}\", All topics used{2}", subTypeName, group, "\n"); }
                    //exclude story types from this, if none found then return empty list
                    if (turnTopicType.name.Equals("Story", StringComparison.Ordinal) == false)
                    { listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live).ToList()); }
                    else { Debug.LogWarningFormat("No valid story topic found for topicType.{0}, topicSubType.{1}", turnTopicType.name, turnTopicSubType.name); }
                }
                else
                {
                    if (isTestLog)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicGroup: {0} topics found for \"{1}\" group {2}{3}", listOfTopics.Count, subTypeName, group, "\n"); }
                }
            }
            else
            {
                //filter topics by subSubTypeName
                switch (group)
                {
                    case GroupType.Good:
                        //high opinion, good group
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                        t.subSubType.name.Equals(subSubTypeName, StringComparison.Ordinal) &&
                        t.group.name.Equals("Good", StringComparison.Ordinal)).ToList());
                        break;
                    case GroupType.Neutral:
                        //neutral opinion, use all Active topics
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                        t.subSubType.name.Equals(subSubTypeName, StringComparison.Ordinal)).ToList());
                        break;
                    case GroupType.Bad:
                    case GroupType.VeryBad:
                        //low opinion, bad group
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                        t.subSubType.name.Equals(subSubTypeName, StringComparison.Ordinal) &&
                        t.group.name.Equals("Bad", StringComparison.Ordinal)).ToList());
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised GroupType \"[0}\"", group);
                        break;
                }
                //if no entries use full sub sub type list by default
                if (listOfTopics.Count == 0)
                {
                    if (isTestLog)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> GetActorContactTopics: No topics found for \"{0}\", {1}, group {2}, All relevant topics used{2}", subTypeName, subSubTypeName, group, "\n"); }
                    listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                    t.subSubType.name.Equals(subSubTypeName, StringComparison.Ordinal)).ToList());
                }
                else
                {
                    if (isTestLog)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicGroup: {0} topics found for \"{1}\", {2}, group {3}{4}", listOfTopics.Count, subTypeName, subSubTypeName, group, "\n"); }
                }
            }

        }
        else { Debug.LogError("Invalid inputList (Null)"); }
        return listOfTopics;
    }
    #endregion

    #region GetTopicSubSubType NodeAction

    /// <summary>
    /// Get's NodeAction TopicSubSubType.SO.name given a nodeAction enum. Returns Null if a problem
    /// </summary>
    /// <param name="nodeAction"></param>
    /// <returns></returns>
    public TopicSubSubType GetTopicSubSubType(NodeAction nodeAction)
    {
        TopicSubSubType subSubType = null;
        //get specific subSubType topic pool
        switch (nodeAction)
        {
            //actor SubSubTypes
            case NodeAction.ActorBlowStuffUp: subSubType = actorBlowStuffUp; break;
            case NodeAction.ActorCreateRiots: subSubType = actorCreateRiots; break;
            case NodeAction.ActorDeployTeam: subSubType = actorDeployTeam; break;
            case NodeAction.ActorGainTargetInfo: subSubType = actorGainTargetInfo; break;
            case NodeAction.ActorHackSecurity: subSubType = actorHackSecurity; break;
            case NodeAction.ActorInsertTracer: subSubType = actorInsertTracer; break;
            case NodeAction.ActorNeutraliseTeam: subSubType = actorNeutraliseTeam; break;
            case NodeAction.ActorObtainGear: subSubType = actorObtainGear; break;
            case NodeAction.ActorRecallTeam: subSubType = actorRecallTeam; break;
            case NodeAction.ActorRecruitActor: subSubType = actorRecruitActor; break;
            case NodeAction.ActorSpreadFakeNews: subSubType = actorSpreadFakeNews; break;
            //player SubSubTypes
            case NodeAction.PlayerBlowStuffUp: subSubType = playerBlowStuffUp; break;
            case NodeAction.PlayerCreateRiots: subSubType = playerCreateRiots; break;
            case NodeAction.PlayerDeployTeam: subSubType = playerDeployTeam; break;
            case NodeAction.PlayerGainTargetInfo: subSubType = playerGainTargetInfo; break;
            case NodeAction.PlayerHackSecurity: subSubType = playerHackSecurity; break;
            case NodeAction.PlayerInsertTracer: subSubType = playerInsertTracer; break;
            case NodeAction.PlayerNeutraliseTeam: subSubType = playerNeutraliseTeam; break;
            case NodeAction.PlayerObtainGear: subSubType = playerObtainGear; break;
            case NodeAction.PlayerRecallTeam: subSubType = playerRecallTeam; break;
            case NodeAction.PlayerRecruitActor: subSubType = playerRecruitActor; break;
            case NodeAction.PlayerSpreadFakeNews: subSubType = playerSpreadFakeNews; break;
            default: Debug.LogWarningFormat("Unrecognised data.nodeAction \"{0}\"", nodeAction); break;
        }
        return subSubType;
    }

    #endregion

    #region GetTopicSubSubType TeamAction
    /// <summary>
    /// Get's TeamAction TopicSubSubType.SO.name given a teamAction enum. Returns Null if a problem
    /// </summary>
    /// <param name="nodeAction"></param>
    /// <returns></returns>
    public TopicSubSubType GetTopicSubSubType(TeamAction teamAction)
    {
        TopicSubSubType subSubType = null;
        //get specific subSubType topic pool
        switch (teamAction)
        {
            //authority SubSubTypes
            case TeamAction.TeamCivil: subSubType = teamCivil; break;
            case TeamAction.TeamControl: subSubType = teamControl; break;
            case TeamAction.TeamDamage: subSubType = teamDamage; break;
            case TeamAction.TeamErasure: subSubType = teamErasure; break;
            case TeamAction.TeamMedia: subSubType = teamMedia; break;
            case TeamAction.TeamProbe: subSubType = teamProbe; break;
            case TeamAction.TeamSpider: subSubType = teamSpider; break;
            default: Debug.LogWarningFormat("Unrecognised data.teamAction \"{0}\"", teamAction); break;
        }
        return subSubType;
    }
    #endregion

    #region CheckSubSubTypeTopicsPresent
    /// <summary>
    /// Returns true if any topic in given list has the matching TopicSubSubType.name, false if not or if a problem
    /// </summary>
    /// <param name="listOfTopics"></param>
    /// <param name="subSubTypeName"></param>
    /// <returns></returns>
    public bool CheckSubSubTypeTopicsPresent(List<Topic> listOfTopics, string subSubTypeName)
    {
        if (listOfTopics != null)
        {
            if (string.IsNullOrEmpty(subSubTypeName) == false)
            {
                if (listOfTopics.Exists(x => x.subSubType.name.Equals(subSubTypeName, StringComparison.Ordinal) && x.status == Status.Live))
                { return true; }
            }
            else { Debug.LogError("Invalid subSubTypeName (Null)"); }
        }
        else { Debug.LogError("Invalid listOfTopics (Null)"); }
        return false;
    }
    #endregion

    #region CheckActorActionTypePresent
    /// <summary>
    /// Checks if a current, onMap, active/inactive (but NOT Captured) actor is present for a given NodeAction (used by GetPlayerDistrictTopics). Returns slotID of actor if so, -1 if not
    /// NOTE: Currently set up for Resistance nodeActions only
    /// </summary>
    /// <param name="nodeAction"></param>
    /// <returns></returns>
    private int CheckActorActionTypePresent(NodeAction nodeAction)
    {
        int slotID = -1;
        string arc = null;
        switch (nodeAction)
        {
            case NodeAction.PlayerBlowStuffUp: arc = "ANARCHIST"; break;
            case NodeAction.PlayerCreateRiots: arc = "HEAVY"; break;
            case NodeAction.PlayerGainTargetInfo: arc = "PLANNER"; break;
            case NodeAction.PlayerHackSecurity: arc = "HACKER"; break;
            case NodeAction.PlayerInsertTracer: arc = "OBSERVER"; break;
            case NodeAction.PlayerNeutraliseTeam: arc = "OPERATOR"; break;
            case NodeAction.PlayerObtainGear: arc = "FIXER"; break;
            case NodeAction.PlayerRecruitActor: arc = "RECRUITER"; break;
            case NodeAction.PlayerSpreadFakeNews: arc = "BLOGGER"; break;
            default: Debug.LogWarningFormat("Unrecognised data.nodeAction \"{0}\"", nodeAction); break;
        }
        if (arc != null)
        {
            //check present on map
            slotID = GameManager.i.dataScript.CheckActorPresent(arc, GameManager.i.sideScript.PlayerSide);
        }
        return slotID;
    }
    #endregion

    #region CheckPlayerStatus
    /// <summary>
    /// Checks Player (AI/Human) status. Returns True if ACTIVE, false otherwise
    /// NOTE: playerSide checked for Null by parent method
    /// </summary>
    /// <returns></returns>
    private bool CheckPlayerStatus(GlobalSide playerSide)
    {
        bool isValid = false;
        switch (playerSide.level)
        {
            case 1:
                //Authority
                switch (GameManager.i.sideScript.authorityOverall)
                {
                    case SideState.Human:
                        if (GameManager.i.playerScript.status == ActorStatus.Active)
                        { isValid = true; }
                        break;
                    case SideState.AI:
                        if (GameManager.i.aiScript.status == ActorStatus.Active)
                        { isValid = true; }
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised authorityOverall \"{0}\"", GameManager.i.sideScript.authorityOverall);
                        break;
                }
                break;
            case 2:
                //Resistance
                switch (GameManager.i.sideScript.resistanceOverall)
                {
                    case SideState.Human:
                        if (GameManager.i.playerScript.status == ActorStatus.Active)
                        { isValid = true; }
                        break;
                    case SideState.AI:
                        if (GameManager.i.aiRebelScript.status == ActorStatus.Active)
                        { isValid = true; }
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised resistanceOverall \"{0}\"", GameManager.i.sideScript.authorityOverall);
                        break;
                }
                break;
            default:
                Debug.LogWarningFormat("Unrecognised playerSide \"{0}\"", playerSide.name);
                break;
        }
        return isValid;
    }
    #endregion

    #region LoadSubSubTypePools
    /// <summary>
    /// Load up a subType topic pool with any associated subSubType pools
    /// NOTE: Assumes that the parent method has checked that the subType has listOfSubSubTypes.Count > 0
    /// </summary>
    private void LoadSubSubTypePools(TopicPool subPool, GlobalSide side)
    {
        if (subPool != null)
        {
            //empty out any residual topics prior to repopulating
            subPool.listOfTopics.Clear();
            int count = 0;
            //populate subType listOfTopics from all subSubType pool's listOfTopics
            foreach (TopicPool subSubPool in subPool.listOfSubSubTypePools)
            {
                //Pool has correct side or 'Both'
                if (subSubPool.subSubType.side.level == side.level || subSubPool.subSubType.side.level == 3)
                {
                    /*count += subSubPool.listOfTopics.Count;
                    subPool.listOfTopics.AddRange(subSubPool.listOfTopics);*/

                    for (int i = 0; i < subSubPool.listOfTopics.Count; i++)
                    {
                        Topic topic = subSubPool.listOfTopics[i];
                        if (topic != null)
                        {
                            //Topic has correct side or 'Both'
                            if (topic.side.level == side.level || topic.side.level == 3)
                            {
                                subPool.listOfTopics.Add(topic);
                                count++;
                            }
                        }
                        else { Debug.LogWarningFormat("Invalid topic (Null) in \"{0}\", for {1}", subPool, side); }
                    }
                }
            }
            Debug.LogFormat("[Top] TopicManager.cs -> LoadSubSubTypePools: {0} topics added to {1} from subSubTopicPools{2}",
                count, subPool.name, "\n");
        }
        else { Debug.LogError("Invalid subPool (Null)"); }
    }
    #endregion

    #region GetTopicEffectData
    /// <summary>
    /// returns TopicEffectData to EffectManager.cs -> ResolveTopicData to enable topic effects to target the correct node/actor/team/contact etc.
    /// Called directly from EffectManager.cs as specific to Topic effects and best not to clutter up TopicEffecData packages with stuff that will only be used by one type of effect (Topic)
    /// </summary>
    /// <returns></returns>
    public TopicEffectData GetTopicEffectData()
    {
        TopicEffectData data = new TopicEffectData()
        {
            actorID = tagActorID,
            actorOtherID = tagActorOtherID,
            isHqActors = tagHqActors,
            nodeID = tagNodeID,
            teamID = tagTeamID,
            contactID = tagContactID,
            secret = tagSecretName,
            orgName = tagOrgName,
            investigationRef = tagStringData,
            gearName = tagGear,
            relation = tagRelation
        };
        if (turnOption != null)
        { data.target = turnOption.storyTarget; }
        return data;
    }
    #endregion

    #region CheckOptionCriteria
    /// <summary>
    /// Check's option criteria (if any) and sets option flag for isValid (passed criteria) or not. If not, option tooltip is set here explaining why it failed criteria check
    /// Returns true if criteria (if any) passes, false if not 
    /// If player STRESSED then a random chance that option will be unavailable (doesn't apply to options that failed normal check, or if isIgnoreStressCheck true)
    /// NOTE: Option checked for Null by parent method (InitialiseTopicUI)
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    private bool CheckOptionCriteria(TopicOption option)
    {
        //determines whether option if viable and can be selected or is greyed out.
        option.isValid = true;
        string effectCriteria = null;
        if (option.listOfCriteria?.Count > 0)
        {
            //pass actor to effect criteria check if a valid actorID
            int actorCurrentSlotID = -1;
            int actorCurrentHqID = -1;
            if (tagActorID > -1)
            {
                Actor actor = null;
                //normal actor uses slotID, HQ actor uses hqID
                if (tagHqActors == false)
                {
                    actor = GameManager.i.dataScript.GetActor(tagActorID);
                    if (actor != null)
                    { actorCurrentSlotID = actor.slotID; }
                    else { Debug.LogWarningFormat("Invalid actor (Null) for tagActorID \"{0}\"", tagActorID); }
                }
                else
                {
                    actor = GameManager.i.dataScript.GetHqActor(tagActorID);
                    if (actor != null)
                    { actorCurrentHqID = actor.hqID; }
                }
            }
            //orgName provided as capture topics have org criteria on certain options (ignored for the rest)
            CriteriaDataInput criteriaInput = new CriteriaDataInput()
            {
                listOfCriteria = option.listOfCriteria,
                actorSlotID = actorCurrentSlotID,
                actorHqID = actorCurrentHqID,
                orgName = tagOrgName,
                nodeID = tagNodeID
            };
            effectCriteria = GameManager.i.effectScript.CheckCriteria(criteriaInput);

        }
        //if no criteria has yet failed
        if (string.IsNullOrEmpty(effectCriteria) == true)
        {
            //player stressed
            if (GameManager.i.playerScript.isStressed == true)
            {
                //Player stressed, random chance option unavailable (can be bypassed)
                if (option.isIgnoreStress == false)
                {
                    int rnd = Random.Range(0, 100);
                    if (rnd < chanceStressedNoOption)
                    { effectCriteria = "Player STRESSED"; }
                }
            }
        }
        if (string.IsNullOrEmpty(effectCriteria) == false)
        {
            //failed criteria check -> specify tooltip here
            option.isValid = false;
            //header -> from option.tag
            if (string.IsNullOrEmpty(option.tag) == false)
            { option.tooltipHeader = string.Format("{0}OPTION UNAVAILABLE{1}", "<mark=#FFFFFF4D>", "</mark>"); }
            else { option.tooltipHeader = "Unknown"; }
            //main -> criteria feedback
            option.tooltipMain = string.Format("{0}{1}{2}", colourCancel, effectCriteria, colourEnd);
            //Details -> derived from option mood Effect

            if (option.isIgnoreMood == false)
            {
                if (option.moodEffect != null)
                { option.tooltipDetails = GameManager.i.personScript.GetMoodTooltip(option.moodEffect.belief, "Player"); }
                else
                {
                    option.tooltipDetails = string.Format("{0}No Effect on Player Mood{1}", colourGrey, colourEnd);
                    Debug.LogWarningFormat("Invalid option.moodEffect (Null) for option {0}", option.name);
                }
            }
            else { option.tooltipDetails = string.Format("{0}No Effect on Player Mood{1}", colourGrey, colourEnd); }

            /*if (option.moodEffect != null)
            {
                if (option.isIgnoreMood == false)
                { option.tooltipDetails = GameManager.instance.personScript.GetMoodTooltip(option.moodEffect.belief, "Player"); }
                else
                {
                    //ignoreMood only applies if player is Stressed
                    if (GameManager.instance.playerScript.isStressed == false)
                    { option.tooltipDetails = GameManager.instance.personScript.GetMoodTooltip(option.moodEffect.belief, "Player"); }
                    else { option.tooltipDetails = string.Format("{0}No Effect on Player Mood{1}", colourGrey, colourEnd); }
                }
            }
            else
            {
                option.tooltipDetails = string.Format("{0}No Effect on Player Mood{1}", colourGrey, colourEnd);
                Debug.LogWarningFormat("Invalid option.moodEffect (Null) for option {0}", option.name);
            }*/
        }
        return option.isValid;
    }
    #endregion

    #region InitialiseOptionUnavailableTooltip
    /// <summary>
    /// Used specifically for Player General option unavailable tooltips
    /// </summary>
    private void InitialiseOptionUnavailableTooltip(TopicOption option)
    {
        option.tooltipHeader = string.Format("{0}OPTION UNAVAILABLE{1}", "<mark=#FFFFFF4D>", "</mark>");
        option.tooltipMain = string.Format("{0}{1}{2}", colourCancel, "Subordinate Unavailable", colourEnd);
        option.tooltipDetails = string.Format("{0}No Mood effect{1}", colourGrey, colourEnd);
    }
    #endregion

    #region InitialiseOptionTooltip
    /// <summary>
    /// Initialise option tooltip prior to sending data Package to TopicUI.cs
    /// NOTE: Option checked for Null by parent method (InitialiseTopicUI.cs)
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    private bool InitialiseOptionTooltip(TopicOption option)
    {
        bool isSucceed = true;
        //header -> from option.tag
        if (string.IsNullOrEmpty(option.tag) == false)
        { option.tooltipHeader = string.Format("{0}{1}{2}", colourNormal, option.tag, colourEnd); }
        else { option.tooltipHeader = "Unknown"; }
        //Main -> derived from option good/bad effects
        StringBuilder builder = new StringBuilder();
        //normal option
        if (option.chance == null)
        {
            //Good effects
            if (option.listOfGoodEffects.Count > 0)
            { GetGoodEffects(option.listOfGoodEffects, option, builder); }
            //Bad effects
            if (option.listOfBadEffects.Count > 0)
            { GetBadEffects(option.listOfBadEffects, option, builder); }
        }
        else
        {
            //probability based option
            builder.AppendFormat("<b>{0} chance of SUCCESS</b>", GetProbability(option.chance));
            if (option.listOfGoodEffects.Count > 0)
            { GetGoodEffects(option.listOfGoodEffects, option, builder); }
            else { builder.AppendFormat("{0}{1}Nothing Happens{2}", "\n", colourGrey, colourEnd); }
            //Bad effects
            builder.AppendFormat("{0}{1}if UNSUCCESSFUL{2}", "\n", colourCancel, colourEnd);
            if (option.listOfBadEffects.Count > 0)
            { GetBadEffects(option.listOfBadEffects, option, builder); }
            else { builder.AppendFormat("{0}{1}Nothing Happens{2}", "\n", colourGrey, colourEnd); }
        }
        if (builder.Length == 0) { builder.Append("No Effects present"); }
        option.tooltipMain = builder.ToString(); ;
        //Details -> derived from option mood Effect
        if (option.isIgnoreMood == false)
        {
            if (option.moodEffect != null)
            {
                //ignoreMood only applies if player is Stressed
                if (GameManager.i.playerScript.isStressed == false)
                { option.tooltipDetails = GameManager.i.personScript.GetMoodTooltip(option.moodEffect.belief, "Player"); }
                else { option.tooltipDetails = string.Format("{0}No Effect on Player Mood{1}", colourGrey, colourEnd); }
            }
            else
            {
                option.tooltipDetails = string.Format("{0}No Effect on Player Mood{1}", colourGrey, colourEnd);
                Debug.LogWarningFormat("Invalid option.moodEffect (Null) for option {0}", option.name);
            }
        }
        else
        {
            if (option.storyTarget != null)
            {
                if (option.storyTarget.moodEffect != null)
                {
                    //special case of story target having a moodEffect
                    option.tooltipDetails = GameManager.i.personScript.GetMoodTooltip(option.storyTarget.moodEffect.belief, "Player");
                }
                else
                {
                    //option switch to ignore mood
                    option.tooltipDetails = string.Format("{0}No Effect on Player Mood{1}", colourGrey, colourEnd);
                }
            }
            else
            {
                //option switch to ignore mood
                option.tooltipDetails = string.Format("{0}No Effect on Player Mood{1}", colourGrey, colourEnd);
            }
        }
        return isSucceed;
    }
    #endregion

    #region GetProbability
    /// <summary>
    /// returns a colour formatted string for probability, eg. 'MEDIUM' (yellow), "Unknown" if a problem
    /// NOTE: chance checked for Null by parent method
    /// </summary>
    /// <param name="chance"></param>
    /// <returns></returns>
    private string GetProbability(GlobalChance chance)
    {
        string probability = "Unknown";
        switch (chance.name)
        {
            case "Extreme": probability = string.Format("{0}Extreme{1}", colourGood, colourEnd); break;
            case "High": probability = string.Format("{0}High{1}", colourGood, colourEnd); break;
            case "Medium": probability = string.Format("{0}Medium{1}", colourNeutral, colourEnd); break;
            case "Low": probability = string.Format("{0}Low{1}", colourBad, colourEnd); break;
            default: Debug.LogWarningFormat("Unrecognised Global Chance \"{0}\"", chance.name); break;
        }
        return probability;
    }
    #endregion

    #region GetGoodEffects
    /// <summary>
    /// returns (to stringbuilder parameter) colour formatted string for good effects
    /// NOTE: parameters checked for Null by parent method
    /// </summary>
    /// <param name="listOfEffects"></param>
    /// <returns></returns>
    private void GetGoodEffects(List<Effect> listOfEffects, TopicOption option, StringBuilder builder)
    {
        if (listOfEffects != null)
        {
            foreach (Effect effect in listOfEffects)
            {
                if (effect != null)
                {
                    if (effect.name.Equals("S_StoryTarget", StringComparison.Ordinal) == true)
                    {
                        //special case of Story Target Effect
                        if (option.storyTarget != null)
                        {
                            if (builder.Length > 0) { builder.AppendLine(); }
                            builder.AppendFormat("{0}Target ({1}, {2} gear){3}", colourGood, option.storyTarget.actorArc.name, option.storyTarget.gear.name, colourEnd);
                        }
                        else { Debug.LogWarningFormat("Invalid storyTarget (Null) for option \"{0}\"", option.name); }
                    }
                    else
                    {
                        //normal effect
                        if (builder.Length > 0) { builder.AppendLine(); }
                        if (string.IsNullOrEmpty(effect.description) == false)
                        { builder.AppendFormat("{0}{1} {2}{3}", colourGood, GetEffectPrefix(effect), effect.description, colourEnd); }
                        else { Debug.LogWarningFormat("Invalid effect.description (Null or Empty) for effect \"{0}\"", effect.name); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid effect (Null) in listOfGoodEffects for option \"{0}\"", option.name); }
            }
        }
        else { Debug.LogWarningFormat("Invalid listOfEffects (Null) for option \"{0}\"", option.name); }
    }
    #endregion

    #region GetBadEffects
    /// <summary>
    /// returns (to stringbuilder parameter) colour formatted string for bad effects
    /// </summary>
    /// <param name="listOfEffects"></param>
    /// <returns></returns>
    private void GetBadEffects(List<Effect> listOfEffects, TopicOption option, StringBuilder builder)
    {
        if (listOfEffects != null)
        {
            foreach (Effect effect in listOfEffects)
            {
                if (effect != null)
                {
                    if (builder.Length > 0) { builder.AppendLine(); }
                    if (string.IsNullOrEmpty(effect.description) == false)
                    { builder.AppendFormat("{0}{1} {2}{3}", colourBad, GetEffectPrefix(effect), effect.description, colourEnd); }
                    else { Debug.LogWarningFormat("Invalid effect.description (Null or Empty) for effect \"{0}\"", effect.name); }
                }
                else { Debug.LogWarningFormat("Invalid effect (Null) in listOfBadEffects for option \"{0}\"", option.name); }
            }
        }
        else { Debug.LogWarningFormat("Invalid listOfEffects (Null) for option \"{0}\"", option.name); }
    }
    #endregion

    #region InitialiseIgnoreTooltip
    /// <summary>
    /// Initialise tooltip data for ignore button (could be ignore effects present)
    /// </summary>
    private void InitialiseIgnoreTooltip(TopicUIData data)
    {
        if (data != null)
        {
            //Header is topic name
            data.ignoreTooltipHeader = string.Format("{0}{1}{2}", colourNormal, turnTopic.tag, colourEnd);
            data.ignoreTooltipMain = string.Format("{0}{1}{2}", colourAlert, "Don't bother me with this", colourEnd);
            //check ignore effects present
            if (turnTopic.listOfIgnoreEffects != null && turnTopic.listOfIgnoreEffects.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                //IGNORE Effects present
                foreach (Effect effect in turnTopic.listOfIgnoreEffects)
                {
                    if (effect != null)
                    {
                        switch (effect.typeOfEffect.name)
                        {
                            case "Good":
                                if (builder.Length > 0) { builder.AppendLine(); }
                                if (string.IsNullOrEmpty(effect.description) == false)
                                { builder.AppendFormat("{0}{1} {2}{3}", colourGood, GetEffectPrefix(effect), effect.description, colourEnd); }
                                else { Debug.LogWarningFormat("Invalid effect.description (Null or Empty) for effect \"{0}\"", effect.name); }
                                break;
                            case "Neutral":
                                if (builder.Length > 0) { builder.AppendLine(); }
                                if (string.IsNullOrEmpty(effect.description) == false)
                                { builder.AppendFormat("{0}{1} {2}{3}", colourNeutral, GetEffectPrefix(effect), effect.description, colourEnd); }
                                else { Debug.LogWarningFormat("Invalid effect.description (Null or Empty) for effect \"{0}\"", effect.name); }
                                break;
                            case "Bad":
                                if (builder.Length > 0) { builder.AppendLine(); }
                                if (string.IsNullOrEmpty(effect.description) == false)
                                { builder.AppendFormat("{0}{1} {2}{3}", colourBad, GetEffectPrefix(effect), effect.description, colourEnd); }
                                else { Debug.LogWarningFormat("Invalid effect.description (Null or Empty) for effect \"{0}\"", effect.name); }
                                break;
                            default:
                                Debug.LogWarningFormat("Unrecognised effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name);
                                break;
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid effect (Null) for topic \"{0}\"", turnTopic.name); }
                }
                if (builder.Length == 0)
                {
                    if (turnTopicSubType.isBoss == true)
                    {
                        builder.AppendFormat("{0}Nothing happens{1}{2}{3}Boss will Disapprove{4}{5}{6}ESC shortcut{7}", colourGrey, colourEnd, "\n",
                          colourBad, colourEnd, "\n", colourNeutral, colourEnd);
                    }
                    else
                    { builder.AppendFormat("{0}Nothing happens{1}{2}{3}ESC shortcut{4}", colourGrey, colourEnd, "\n", colourNeutral, colourEnd); }
                    Debug.LogWarningFormat("Invalid Ignore Effects (None showing) for topic \"{0}\"", turnTopic.name);
                }
                else
                {
                    //boss
                    if (turnTopicSubType.isBoss == true)
                    { builder.AppendFormat("{0}{1}Boss will Disapprove{2}", "\n", colourBad, colourEnd); }
                    //keyboard shortcut
                    builder.AppendFormat("{0}{1}ESC shortcut{2}", "\n", colourNeutral, colourEnd);
                }
                //details
                data.ignoreTooltipDetails = builder.ToString();
            }
            else
            {
                //No ignoreEffects -> default text
                if (turnTopicSubType.isBoss == true)
                {
                    data.ignoreTooltipDetails = string.Format("{0}Nothing happens{1}{2}{3}Boss will Disapprove{4}{5}{6}ESC shortcut{7}", colourGrey, colourEnd, "\n",
                  colourBad, colourEnd, "\n", colourNeutral, colourEnd);
                }
                else
                { data.ignoreTooltipDetails = string.Format("{0}Nothing happens{1}{2}{3}ESC shortcut{4}", colourGrey, colourEnd, "\n", colourNeutral, colourEnd); }
            }
        }
        else { Debug.LogError("Invalid TopicUIData (Null)"); }
    }
    #endregion

    #region InitialiseBossDetails
    /// <summary>
    /// Initialises tooltip and sprite for boss image, top right, if present
    /// NOTE: data checked for Null by parent method
    /// </summary>
    /// <param name="data"></param>
    private void InitialiseBossDetails(TopicUIData data)
    {
        Actor actor = GameManager.i.dataScript.GetHqHierarchyActor(ActorHQ.Boss);
        if (actor != null)
        {
            //sprite
            data.spriteBoss = actor.sprite;
            data.bossTooltipHeader = string.Format("<b>{0}{1}{2}HQ Boss{3}</b>", actor.actorName, "\n", colourAlert, colourEnd);
            //opinion of options
            StringBuilder builder = new StringBuilder();
            builder.Append("<b>What Boss thinks</b>");
            List<TopicOption> listOfOptions = turnTopic.listOfOptions;
            if (listOfOptions != null)
            {
                for (int i = 0; i < listOfOptions.Count; i++)
                {
                    if (builder.Length > 0) { builder.AppendLine(); }
                    TopicOption option = listOfOptions[i];
                    if (option != null)
                    {
                        if (option.moodEffect != null)
                        {
                            if (option.moodEffect.belief != null)
                            {
                                /*//if an HQ preferred option it is automatically 'Good', otherwise it's based on HQ Boss's personality
                                if (option.isPreferredByHQ == false)
                                {
                                    if (option.isIgnoredByHQ == false)
                                    { builder.AppendFormat("{0}{1}: {2} {3}", colourCancel, option.tag, colourEnd, GameManager.instance.personScript.GetHQTooltip(option.moodEffect.belief, actor)); }
                                    else { builder.AppendFormat("{0}{1}: {2} {3}no view{4}", colourCancel, option.tag, colourEnd, colourGrey, colourEnd); }
                                }
                                else { builder.AppendFormat("{0}{1}: {2} {3}Approves{4}", colourCancel, option.tag, colourEnd, colourGood, colourEnd); }*/

                                if (option.isPreferredByHQ == true)
                                { builder.AppendFormat("{0}{1}: {2} {3}Approves{4}", colourCancel, option.tag, colourEnd, colourGood, colourEnd); }
                                else if (option.isDislikedByHQ == true)
                                { builder.AppendFormat("{0}{1}: {2} {3}Disapproves{4}", colourCancel, option.tag, colourEnd, colourBad, colourEnd); }
                                else if (option.isIgnoredByHQ == true)
                                { builder.AppendFormat("{0}{1}: {2} {3}no view{4}", colourCancel, option.tag, colourEnd, colourGrey, colourEnd); }
                                else
                                { builder.AppendFormat("{0}{1}: {2} {3}", colourCancel, option.tag, colourEnd, GameManager.i.personScript.GetHQTooltip(option.moodEffect.belief, actor)); }
                            }
                            else { builder.AppendFormat("{0}{1}: {2} {3}no view{4}", colourCancel, option.tag, colourEnd, colourGrey, colourEnd); }
                        }
                        else
                        {
                            if (option.isIgnoreMood == false)
                            {
                                //invalid belief
                                builder.AppendFormat("{0}Unknown{1}", colourGrey, colourEnd);
                                Debug.LogWarningFormat("Invalid belief (Null) for option \"{0}\", topic {1}", option.name, turnTopic.name);
                            }
                            else { builder.AppendFormat("{0}{1}: {2} {3}no view{4}", colourCancel, option.tag, colourEnd, colourGrey, colourEnd); }
                        }
                    }
                    else
                    {
                        //invalid option
                        builder.AppendFormat("{0}Unknown{1}", colourGrey, colourEnd);
                        Debug.LogWarningFormat("Invalid option (Null) for topic \"{0}\" listOfOptions[{1}]", turnTopic.name, i);
                    }
                }
            }
            else
            {
                Debug.LogErrorFormat("Invalid listOfOptions (Null) for topic \"{0}\"", turnTopic.name);
                builder.Append("Unknown");
            }
            data.bossTooltipMain = builder.ToString();
            //Boss's view of your decision making ability
            data.bossTooltipDetails = string.Format("Boss's opinion of your Decisions{0}<b><size=115%>{1}{2}{3}</size></b>", "\n",
                colourNeutral, GameManager.i.hqScript.GetBossOpinionFormatted(), colourEnd);
        }
        else { Debug.LogError("Invalid actor (Null) for HQ Boss"); }
    }
    #endregion

    #region GetEffectPrefix
    /// <summary>
    /// returns prefix (actor name, player name, etc.) for a topic effect based on first character of effect name (topic effects have a naming convention of 'X_...' where 'X' represents a prefix type)
    /// NOTE: Effect checked for Null by parent method (InitialiseOption/IgnoreTooltip)
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    private string GetEffectPrefix(Effect effect)
    {
        string prefix = "";
        char key = effect.name[0];
        //double check that first char of effect name is followed immediately by an underscore
        if (effect.name[1].Equals('_') == true)
        {
            switch (key)
            {
                case 'A':
                    //actor -> if HQ actors then actorID is actually hqID
                    Actor actor = null;
                    if (tagHqActors == false)
                    {
                        actor = GameManager.i.dataScript.GetActor(tagActorID);
                        if (actor != null) { prefix = actor.arc.name; }
                        else { Debug.LogErrorFormat("Invalid actor (Null) for tagActorID {0}", tagActorID); }
                    }
                    else
                    {
                        actor = GameManager.i.dataScript.GetHqActor(tagActorID);
                        if (actor != null) { prefix = tagHqTitleActor; }
                        else { Debug.LogErrorFormat("Invalid HQ actor (Null) for tagActorID {0}", tagActorID); }
                    }
                    break;
                case 'B':
                    //actorOther -> if HQ actors then actorID is actually hqID
                    Actor actorOther = null;
                    if (tagHqActors == false)
                    {
                        actorOther = GameManager.i.dataScript.GetActor(tagActorOtherID);
                        if (actorOther != null) { prefix = actorOther.arc.name; }
                        else { Debug.LogErrorFormat("Invalid actorOther (Null) for tagActorOtherID {0}", tagActorOtherID); }
                    }
                    else
                    {
                        actorOther = GameManager.i.dataScript.GetHqActor(tagActorOtherID);
                        if (actorOther != null) { prefix = tagHqTitleOther; }
                        else { Debug.LogErrorFormat("Invalid HQ actorOther (Null) for tagActorOtherID {0}", tagActorOtherID); }
                    }
                    break;
                case 'L':
                    //All actors
                    break;
                case 'R':
                    //Random actors
                    break;
                case 'P':
                    //player
                    prefix = "Player";
                    break;
                case 'T':
                    //team
                    break;
                case 'C':
                    //city
                    break;
                case 'H':
                    //HQ
                    break;
                case 'S':
                    //Story
                    break;
                case 'N':
                    //Node
                    break;
                case 'O':
                    //Organisation
                    break;
                default: Debug.LogWarningFormat("Unrecognised effect.name first character \"{0}\" for effect \"{1}\"", key, effect.name); break;
            }
        }
        else { Debug.LogWarningFormat("Invalid effectName \"{0}\" (should be 'X_...')", effect.name); }
        return prefix;
    }
    #endregion

    #region CheckTopicText
    /// <summary>
    /// takes text from any topic source, checks for tags, eg. '[actor]', and replaces with context relevant info, eg. actor arc name. Returns Null if a problem
    /// isColourHighlight TRUE -> Colour and bold highlights certain texts (colourAlert), no colour formatting if false, default True
    /// if highlighting true, any unknown text inside brackets will be automatically highlighted, otherwise error condition for an invalid tag
    /// isValidation true for validating topic.text/topicOption.news having correct tags, false otherwise(iscolourHighlighting ignored for validation)
    /// objectName only required for validation checks
    /// Highlights -> actor.arc, node.nodeName
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public string CheckTopicText(string text, bool isColourHighlighting = true, bool isValidate = false, string objectName = "Unknown")
    {
        string checkedText = null;
        string colourCheckText = colourAlert; //highlight colour
        //no highlighting for storyBravo (Letter) texts
        if (turnTopicSubType?.name.Equals("StoryBravo", StringComparison.Ordinal) == true || turnTopicSubType?.name.Equals("StoryAlpha", StringComparison.Ordinal) == true)
        { isColourHighlighting = false; }
        //if validation run need dictionary of tags for analysis purposes
        Dictionary<string, int> dictOfTags = null;
        if (isValidate == true)
        {
            dictOfTags = GameManager.i.dataScript.GetDictOfTags();
            if (dictOfTags == null)
            { Debug.LogError("Invalid dictOfTags (Null)"); }
        }
        //check string for tags
        if (string.IsNullOrEmpty(text) == false)
        {
            checkedText = text;
            string tag, replaceText;
            int tagStart, tagFinish, length; //indexes
            Node node = null;
            if (tagNodeID > -1)
            { node = GameManager.i.dataScript.GetNode(tagNodeID); }
            else
            {
                //get default node 0 to accomdodate jobs tag, if required, but still not display 'Show Me' button (tagNodeID > -1)
                node = GameManager.i.dataScript.GetNode(0);
            }
            if (node == null)
            { Debug.LogWarning("Invalid node (Null)"); }
            //loop whilever tags are present
            while (checkedText.Contains("[") == true)
            {
                tagStart = checkedText.IndexOf("[");
                tagFinish = checkedText.IndexOf("]");
                length = tagFinish - tagStart;
                tag = checkedText.Substring(tagStart + 1, length - 1);
                //strip brackets
                replaceText = null;
                switch (tag)
                {
                    case "player":
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}{1}{2}", colourCheckText, GameManager.i.playerScript.PlayerName, colourEnd); }
                            else { replaceText = GameManager.i.playerScript.PlayerName; }
                        }
                        else { CountTextTag("player", dictOfTags); }
                        break;
                    case "first":
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}{1}{2}", colourCheckText, GameManager.i.playerScript.FirstName, colourEnd); }
                            else { replaceText = GameManager.i.playerScript.FirstName; }
                        }
                        else { CountTextTag("player", dictOfTags); }
                        break;
                    case "child":
                        //parent talking to Player (no highlight)
                        if (isValidate == false)
                        {
                            string child = "Son";
                            if (GameManager.i.playerScript.sex == ActorSex.Female) { child = "Daughter"; }
                            replaceText = child;
                        }
                        else { CountTextTag("child", dictOfTags); }
                        break;
                    case "sibling":
                        //sibling talking to player (no highlight)
                        if (isValidate == false)
                        {
                            string sibling = "Brother";
                            if (GameManager.i.playerScript.sex == ActorSex.Female) { sibling = "Sister"; }
                            replaceText = sibling;
                        }
                        else { CountTextTag("sibling", dictOfTags); }
                        break;
                    case "brother":
                        //first name of player's brother
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.globalScript.tagBrother, colourEnd); }
                            else { replaceText = GameManager.i.globalScript.tagBrother; }
                        }
                        else { CountTextTag("brother", dictOfTags); }
                        break;
                    case "sister":
                        //first name of player's sister
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.globalScript.tagSister, colourEnd); }
                            else { replaceText = GameManager.i.globalScript.tagSister; }
                        }
                        else { CountTextTag("sister", dictOfTags); }
                        break;
                    case "actor":
                        //actor arc name
                        if (isValidate == false)
                        {
                            if (tagActorID > -1)
                            {
                                Actor actor = GameManager.i.dataScript.GetActor(tagActorID);
                                if (actor != null)
                                {
                                    if (isColourHighlighting == true)
                                    { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                    else { replaceText = actor.arc.name; }
                                }
                                else { Debug.LogWarningFormat("Invalid actor (Null) for tagActorID \"{0}\"", tagActorID); }
                            }
                            else
                            { Debug.LogWarningFormat("Invalid tagActorID \"{0}\" for tag <actor>", tagActorID); }
                        }
                        else { CountTextTag("actor", dictOfTags); }
                        break;
                    case "other":
                        //actorOther (second actor in dual actor situations, eg. ActorPolitic topics) arc name
                        if (isValidate == false)
                        {
                            if (tagActorOtherID > -1)
                            {
                                Actor actor = GameManager.i.dataScript.GetActor(tagActorOtherID);
                                if (actor != null)
                                {
                                    if (isColourHighlighting == true)
                                    { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                    else { replaceText = actor.arc.name; }
                                }
                                else { Debug.LogWarningFormat("Invalid actor other (Null) for tagActorOtherID \"{0}\"", tagActorOtherID); }
                            }
                            else
                            { Debug.LogWarningFormat("Invalid tagActorOtherID \"{0}\" for tag <other>", tagActorOtherID); }
                        }
                        else { CountTextTag("actor", dictOfTags); }
                        break;
                    case "actors":
                        //actor arc name, possessive, eg. Hacker's
                        if (isValidate == false)
                        {
                            if (tagActorID > -1)
                            {
                                Actor actor = GameManager.i.dataScript.GetActor(tagActorID);
                                if (actor != null)
                                {
                                    if (isColourHighlighting == true)
                                    { replaceText = string.Format("{0}<b>{1}'s</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                    else { replaceText = string.Format("{0}'s", actor.arc.name); }
                                }
                                else { Debug.LogWarningFormat("Invalid actor (Null) for tagActorID \"{0}\"", tagActorID); }
                            }
                            else
                            { Debug.LogWarningFormat("Invalid tagActorID \"{0}\" for tag <actors>", tagActorID); }
                        }
                        else { CountTextTag("actors", dictOfTags); }
                        break;
                    case "option0":
                        //actor name for option 0
                        if (isValidate == false)
                        {
                            int actorID = arrayOfOptionActorIDs[0];
                            if (actorID > -1)
                            {
                                Actor actor = GameManager.i.dataScript.GetActor(actorID);
                                if (actor != null)
                                {
                                    if (isColourHighlighting == true)
                                    { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                    else { replaceText = actor.arc.name; }
                                }
                                else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfOptionActorIDs[0] \"{0}\"", actorID); }
                            }
                            else
                            {
                                Debug.LogWarningFormat("Invalid actorID \"{0}\" for arrayOfOptionActorIDs[0]", actorID);
                                /*actorID = arrayOfOptionInactiveIDs[0];
                                if (actorID > -1)
                                {
                                    Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                                    if (actor != null)
                                    {
                                        if (isColourHighlighting == true)
                                        { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                        else { replaceText = actor.arc.name; }
                                    }
                                    else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfOptionInactiveIDs[0] \"{0}\"", actorID); }
                                }*/
                            }
                        }
                        else { CountTextTag("option0", dictOfTags); }
                        break;
                    case "option1":
                        //actor name for option 1
                        if (isValidate == false)
                        {
                            int actorID = arrayOfOptionActorIDs[1];
                            if (actorID > -1)
                            {
                                Actor actor = GameManager.i.dataScript.GetActor(actorID);
                                if (actor != null)
                                {
                                    if (isColourHighlighting == true)
                                    { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                    else { replaceText = actor.arc.name; }
                                }
                                else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfOptionActorIDs[1] \"{0}\"", actorID); }
                            }
                            else
                            {
                                Debug.LogWarningFormat("Invalid actorID \"{0}\" for arrayOfOptionActorIDs[1]", actorID);
                                /*actorID = arrayOfOptionInactiveIDs[1];
                                if (actorID > -1)
                                {
                                    Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                                    if (actor != null)
                                    {
                                        if (isColourHighlighting == true)
                                        { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                        else { replaceText = actor.arc.name; }
                                    }
                                    else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfOptionInactiveIDs[1] \"{0}\"", actorID); }
                                }*/
                            }
                        }
                        else { CountTextTag("option1", dictOfTags); }
                        break;
                    case "option2":
                        //actor name for option 2
                        if (isValidate == false)
                        {
                            int actorID = arrayOfOptionActorIDs[2];
                            if (actorID > -1)
                            {
                                Actor actor = GameManager.i.dataScript.GetActor(actorID);
                                if (actor != null)
                                {
                                    if (isColourHighlighting == true)
                                    { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                    else { replaceText = actor.arc.name; }
                                }
                                else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfOptionActorIDs[0] \"{0}\"", actorID); }
                            }
                            else
                            {
                                Debug.LogWarningFormat("Invalid actorID \"{0}\" for arrayOfOptionActorIDs[2]", actorID);
                                /*actorID = arrayOfOptionInactiveIDs[2];
                                if (actorID > -1)
                                {
                                    Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                                    if (actor != null)
                                    {
                                        if (isColourHighlighting == true)
                                        { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                        else { replaceText = actor.arc.name; }
                                    }
                                    else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfOptionInactiveIDs[2] \"{0}\"", actorID); }
                                }*/
                            }
                        }
                        else { CountTextTag("option2", dictOfTags); }
                        break;
                    case "option3":
                        //actor name for option 3
                        if (isValidate == false)
                        {
                            int actorID = arrayOfOptionActorIDs[3];
                            if (actorID > -1)
                            {
                                Actor actor = GameManager.i.dataScript.GetActor(actorID);
                                if (actor != null)
                                {
                                    if (isColourHighlighting == true)
                                    { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                    else { replaceText = actor.arc.name; }
                                }
                                else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfOptionActorIDs[3] \"{0}\"", actorID); }
                            }
                            else
                            {
                                Debug.LogWarningFormat("Invalid actorID \"{0}\" for arrayOfOptionActorIDs[3]", actorID);
                                /*actorID = arrayOfOptionInactiveIDs[3];
                                if (actorID > -1)
                                {
                                    Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                                    if (actor != null)
                                    {
                                        if (isColourHighlighting == true)
                                        { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                        else { replaceText = actor.arc.name; }
                                    }
                                    else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfOptionInactiveIDs[3] \"{0}\"", actorID); }
                                }*/
                            }
                        }
                        else { CountTextTag("option3", dictOfTags); }
                        break;
                    case "node":
                        //district name
                        if (isValidate == false)
                        {
                            if (node != null)
                            {
                                if (isColourHighlighting == true)
                                { replaceText = string.Format("<b>{0}{1}{2}</b>", colourCheckText, node.nodeName, colourEnd); }
                                else { replaceText = node.nodeName; }
                            }
                        }
                        else { CountTextTag("node", dictOfTags); }
                        break;
                    case "nodeArc":
                        //district arc name (caps)
                        if (isValidate == false)
                        {
                            if (node != null)
                            {
                                if (isColourHighlighting == true)
                                { replaceText = string.Format("<b>{0}{1}{2}</b>", colourCheckText, node.Arc.name, colourEnd); }
                                else { replaceText = node.nodeName; }
                            }
                        }
                        else { CountTextTag("nodeArc", dictOfTags); }
                        break;
                    case "contact":
                        //contact name
                        if (isValidate == false)
                        {
                            if (tagContactID > -1)
                            {
                                Contact contact = GameManager.i.dataScript.GetContact(tagContactID);
                                if (contact != null)
                                {
                                    if (node != null)
                                    {
                                        if (isColourHighlighting == true)
                                        { replaceText = string.Format("<b>{0} {1}</b>,", contact.nameFirst, contact.nameLast); }
                                        else { replaceText = string.Format("{0} {1}", contact.nameFirst, contact.nameLast); }
                                    }
                                    else { Debug.LogWarningFormat("Invalid node (Null) for tagNodeID {0}", tagNodeID); }
                                }
                                else { Debug.LogWarningFormat("Invalid contact (Null) for tagContactID \"{0}\"", tagContactID); }
                            }
                            else
                            { Debug.LogWarningFormat("Invalid tagContactID \"{0}\" for tag <Contact>", tagContactID); }
                        }
                        else { CountTextTag("contact", dictOfTags); }
                        break;
                    case "contactLong":
                        //contact name + contact job + node name
                        if (isValidate == false)
                        {
                            if (tagContactID > -1)
                            {
                                Contact contact = GameManager.i.dataScript.GetContact(tagContactID);
                                if (contact != null)
                                {
                                    if (node != null)
                                    {
                                        if (isColourHighlighting == true)
                                        { replaceText = string.Format("<b>{0} {1}, {2}</b>, at {3}<b>{4}</b>{5},", contact.nameFirst, contact.nameLast, contact.job, colourCheckText, node.nodeName, colourEnd); }
                                        else { replaceText = string.Format("{0} {1}, {2}, at {3},", contact.nameFirst, contact.nameLast, contact.job, node.nodeName); }
                                    }
                                    else { Debug.LogWarningFormat("Invalid node (Null) for tagNodeID {0}", tagNodeID); }
                                }
                                else { Debug.LogWarningFormat("Invalid contact (Null) for tagContactID \"{0}\"", tagContactID); }
                            }
                            else
                            { Debug.LogWarningFormat("Invalid tagContactID \"{0}\" for tag <Contact>", tagContactID); }
                        }
                        else { CountTextTag("contactLong", dictOfTags); }
                        break;
                    case "gear":
                        //gear tag (Node action) -> default 'Gear'
                        if (isValidate == false)
                        {
                            //fail safe
                            if (string.IsNullOrEmpty(tagGear) == true) { tagGear = "Gear"; }
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, tagGear, colourEnd); }
                            else { replaceText = tagGear; }
                        }
                        break;
                    case "recruit":
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, tagRecruit, colourEnd); }
                            else { replaceText = tagRecruit; }
                        }
                        else { CountTextTag("recruit", dictOfTags); }
                        break;
                    case "team":
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, tagTeam, colourEnd); }
                            else { replaceText = tagTeam; }
                        }
                        else { CountTextTag("team", dictOfTags); }
                        break;
                    case "job":
                        //Random job name appropriate to node arc
                        if (isValidate == false)
                        {
                            string job = "Unknown";
                            if (string.IsNullOrEmpty(tagJob) == true)
                            {
                                if (node != null)
                                {
                                    ContactType contactType = node.Arc.GetRandomContactType();
                                    if (contactType != null)
                                    {
                                        job = contactType.pickList.GetRandomRecord();
                                        tagJob = job;
                                    }
                                    else
                                    { Debug.LogWarningFormat("Invalid contactType (Null) for node {0}, {1}, {2}", node.nodeName, node.Arc.name, node.nodeID); }
                                }
                                else { job = "Street Bum"; }
                            }
                            else { job = tagJob; }
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, job, colourEnd); }
                            else { replaceText = job; }
                        }
                        else { CountTextTag("job", dictOfTags); }
                        break;
                    case "genLoc":
                        //Random Generic Location(use TopicUIData.dataName if available, otherwise get random
                        if (isValidate == false)
                        {
                            string location = "Unknown";
                            if (string.IsNullOrEmpty(tagLocation) == true)
                            {
                                location = textlistGenericLocation.GetRandomRecord();
                                tagLocation = location;
                            }
                            else { location = tagLocation; }
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("<b>{0}{1}{2}</b>", colourCheckText, location, colourEnd); }
                            else { replaceText = location; }
                        }
                        else { CountTextTag("genLoc", dictOfTags); }
                        break;
                    case "policy":
                        //Random HR Policy (both sides)
                        if (isValidate == false)
                        {
                            string policy = textListPolicy.GetRandomRecord();
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("<b>{0}{1}{2}</b>", colourCheckText, policy, colourEnd); }
                            else { replaceText = policy; }
                        }
                        else { CountTextTag("policy", dictOfTags); }
                        break;
                    case "meds":
                        //'Sorry but I'm on [Knob Rot Meds]'
                        if (isValidate == false)
                        {
                            string meds = "Unknown";
                            if (string.IsNullOrEmpty(tagMeds) == true)
                            {
                                meds = textListMeds.GetRandomRecord();
                                tagMeds = meds;
                            }
                            else { meds = tagMeds; }
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("<b>{0}{1} meds{2}</b>", colourCheckText, meds, colourEnd); }
                            else { replaceText = string.Format("{0} meds", meds); }
                        }
                        else { CountTextTag("meds", dictOfTags); }
                        break;
                    case "disease":
                        //'Sorry but I'm on [Knob Rot Meds]'
                        if (isValidate == false)
                        {
                            string disease = "Unknown";
                            if (string.IsNullOrEmpty(tagDisease) == true)
                            {
                                disease = textListDisease.GetRandomRecord();
                                tagDisease = disease;
                            }
                            else { disease = tagDisease; }
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("<b>{0}{1}{2}</b>", colourCheckText, disease, colourEnd); }
                            else { replaceText = disease; }
                        }
                        else { CountTextTag("disease", dictOfTags); }
                        break;
                    case "daysAgo":
                        //how many turns ago expressed as '3 days'. Mincap at '1 day'
                        if (isValidate == false)
                        {
                            int turnsAgo = GameManager.i.turnScript.Turn - tagTurn;
                            turnsAgo = Mathf.Max(1, turnsAgo);
                            replaceText = string.Format("<b>{0} day{1} ago</b>", turnsAgo, turnsAgo != 1 ? "s" : "");
                        }
                        else { CountTextTag("daysAgo", dictOfTags); }
                        break;
                    case "badRES":
                        //end of topic text for a bad outcome (Resistance)
                        if (isValidate == false)
                        { replaceText = textListBadResistance.GetRandomRecord(); }
                        else { CountTextTag("badRES", dictOfTags); }
                        break;
                    case "goodRES":
                        //end of topic text for a good outcome (Resistance)
                        if (isValidate == false)
                        { replaceText = textListGoodResistance.GetRandomRecord(); }
                        else { CountTextTag("goodRES", dictOfTags); }
                        break;
                    case "vip":
                        //special character Npc, eg. courier (have to use 'vip' as 'npc' already taken)
                        if (isValidate == false)
                        {
                            replaceText = "Special Character";
                            if (GameManager.i.missionScript.mission.npc != null)
                            { replaceText = GameManager.i.missionScript.mission.npc.tag; }
                        }
                        else { CountTextTag("vip", dictOfTags); }
                        break;
                    case "npc":
                        if (isValidate == false)
                        {
                            if (Random.Range(0, 100) < 50) { replaceText = GameManager.i.cityScript.GetCity().nameSet.firstFemaleNames.GetRandomRecord(); }
                            else { replaceText = GameManager.i.cityScript.GetCity().nameSet.firstMaleNames.GetRandomRecord(); }
                            if (isColourHighlighting == true)
                            {
                                replaceText += " " + GameManager.i.cityScript.GetCity().nameSet.lastNames.GetRandomRecord();
                                replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, replaceText, colourEnd);
                            }
                            else { replaceText += " " + GameManager.i.cityScript.GetCity().nameSet.lastNames.GetRandomRecord(); }
                        }
                        else { CountTextTag("npc", dictOfTags); }
                        break;
                    case "npcIs":
                        //npc is/was something
                        if (isValidate == false)
                        { replaceText = textListNpcSomething.GetRandomRecord(); }
                        else { CountTextTag("npcIs", dictOfTags); }
                        break;
                    case "handicap":
                        //handicap, eg. 'my [handicap]'
                        if (isValidate == false)
                        { replaceText = textListHandicap.GetRandomRecord(); }
                        else { CountTextTag("handicap", dictOfTags); }
                        break;
                    case "money":
                        //reason they want money
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("<b>{0}{1}{2}</b>", colourCheckText, textListMoneyReason.GetRandomRecord(), colourEnd); }
                            else { replaceText = textListMoneyReason.GetRandomRecord(); }
                        }
                        else { CountTextTag("money", dictOfTags); }
                        break;
                    case "mayor":
                        //mayor + first name
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.cityScript.GetCity().mayor.mayorName, colourEnd); }
                            else { replaceText = GameManager.i.cityScript.GetCity().mayor.mayorName; }
                        }
                        else { CountTextTag("mayor", dictOfTags); }
                        break;
                    case "secret":
                        //name of random secret from among those known by the actor
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2} secret", colourCheckText, tagSecretTag, colourEnd); }
                            else { replaceText = string.Format("{0} secret", tagSecretTag); }
                        }
                        else { CountTextTag("secret", dictOfTags); }
                        break;
                    case "city":
                        //city name
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.cityScript.GetCityName(), colourEnd); }
                            else { replaceText = GameManager.i.cityScript.GetCityName(); }
                        }
                        else { CountTextTag("city", dictOfTags); }
                        break;
                    case "citys":
                        //city name possessive, eg. London's
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}'s</b>{2}", colourCheckText, GameManager.i.cityScript.GetCityName(), colourEnd); }
                            else { replaceText = string.Format("{0}'s", GameManager.i.cityScript.GetCityName()); }
                        }
                        else { CountTextTag("citys", dictOfTags); }
                        break;
                    case "cityNext":
                        //city name for next city in campaign (returns 'unknown' if last scenario of campaign)
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.cityScript.GetNextCityName(), colourEnd); }
                            else { replaceText = GameManager.i.cityScript.GetNextCityName(); }
                        }
                        else { CountTextTag("cityNext", dictOfTags); }
                        break;
                    case "target":
                        //target name
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, tagTarget, colourEnd); }
                            else { replaceText = tagTarget; }
                        }
                        else { CountTextTag("target", dictOfTags); }
                        break;
                    case "org":
                        //organisation tag
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, tagOrgTag, colourEnd); }
                            else { replaceText = tagOrgTag; }
                        }
                        else { CountTextTag("org", dictOfTags); }
                        break;
                    case "invest":
                        //investigation tag
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, tagInvestTag, colourEnd); }
                            else { replaceText = tagInvestTag; }
                        }
                        else { CountTextTag("invest", dictOfTags); }
                        break;
                    case "relation":
                        //'broken protocol by [friend/enemy]'  depends on tagRelation
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            {
                                switch (tagRelation)
                                {
                                    case ActorRelationship.FRIEND:
                                        replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.topicScript.textListFriend.GetRandomRecord(), colourEnd);
                                        break;
                                    case ActorRelationship.ENEMY:
                                        replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.topicScript.textListEnemy.GetRandomRecord(), colourEnd);
                                        break;
                                    default: Debug.LogWarningFormat("Unrecognised tagRelation \"{0}\"", tagRelation); break;
                                }
                            }
                            else { replaceText = tagRelation == ActorRelationship.FRIEND ? GameManager.i.topicScript.textListFriend.GetRandomRecord() : GameManager.i.topicScript.textListEnemy.GetRandomRecord(); }
                        }
                        else { CountTextTag("relation", dictOfTags); }
                        break;
                    case "relAct":
                        //Relationship Action (ActorPolitic),  depends on tagRelation
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            {
                                switch (tagRelation)
                                {
                                    case ActorRelationship.FRIEND:
                                        replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.topicScript.textListFriendAction.GetRandomRecord(), colourEnd);
                                        break;
                                    case ActorRelationship.ENEMY:
                                        replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.topicScript.textListEnemyAction.GetRandomRecord(), colourEnd);
                                        break;
                                    default: Debug.LogWarningFormat("Unrecognised tagRelation \"{0}\"", tagRelation); break;
                                }
                            }
                            else
                            { replaceText = tagRelation == ActorRelationship.FRIEND ? GameManager.i.topicScript.textListFriendAction.GetRandomRecord() : GameManager.i.topicScript.textListEnemyAction.GetRandomRecord(); }
                        }
                        else { CountTextTag("relAct", dictOfTags); }
                        break;
                    case "relRes":
                        //Relationship Reason (ActorPolitic),  depends on tagRelation
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            {
                                switch (tagRelation)
                                {
                                    case ActorRelationship.FRIEND:
                                        replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.topicScript.textListFriendReason.GetRandomRecord(), colourEnd);
                                        break;
                                    case ActorRelationship.ENEMY:
                                        replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.topicScript.textListEnemyReason.GetRandomRecord(), colourEnd);
                                        break;
                                    default: Debug.LogWarningFormat("Unrecognised tagRelation \"{0}\"", tagRelation); break;
                                }
                            }
                            else
                            { replaceText = tagRelation == ActorRelationship.FRIEND ? GameManager.i.topicScript.textListFriendReason.GetRandomRecord() : GameManager.i.topicScript.textListEnemyReason.GetRandomRecord(); }
                        }
                        else { CountTextTag("relRes", dictOfTags); }
                        break;
                    case "orgWant":
                        //organisation wants you to...
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, tagOrgWant, colourEnd); }
                            else { replaceText = tagOrgWant; }
                        }
                        else { CountTextTag("orgWant", dictOfTags); }
                        break;
                    case "orgText":
                        //OrgData.text, eg. service provided (use [daysAgo] for how many days back)
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, tagOrgText, colourEnd); }
                            else { replaceText = tagOrgText; }
                        }
                        else { CountTextTag("orgText", dictOfTags); }
                        break;
                    case "who":
                        //My '[best friend]'s [crazy] [sister]' 
                        if (isValidate == false)
                        {
                            replaceText = string.Format("{0}'s {1} {2}", GameManager.i.topicScript.textListWho0.GetRandomRecord(), GameManager.i.topicScript.textListCondition.GetRandomRecord(),
                              GameManager.i.topicScript.textListWho1.GetRandomRecord());
                        }
                        else { CountTextTag("who", dictOfTags); }
                        break;
                    case "stat0":
                        //Statistics -> Player Node Actions
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerNodeActions), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerNodeActions).ToString(); }
                        }
                        else { CountTextTag("stat0", dictOfTags); }
                        break;
                    case "stat1":
                        //Statistics -> Total Node Actions
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.StatisticGetLevel(StatType.NodeActionsResistance), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.StatisticGetLevel(StatType.NodeActionsResistance).ToString(); }
                        }
                        else { CountTextTag("stat1", dictOfTags); }
                        break;
                    case "stat2":
                        //Statistics -> Player Target Attempts
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerTargetAttempts), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerTargetAttempts).ToString(); }
                        }
                        else { CountTextTag("stat2", dictOfTags); }
                        break;
                    case "stat3":
                        //Statistics -> Target Attempts
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.StatisticGetLevel(StatType.TargetAttempts), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.StatisticGetLevel(StatType.TargetAttempts).ToString(); }
                        }
                        else { CountTextTag("stat3", dictOfTags); }
                        break;
                    case "stat4":
                        //Statistics -> Player Move Actions
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerMoveActions), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerMoveActions).ToString(); }
                        }
                        else { CountTextTag("stat4", dictOfTags); }
                        break;
                    case "stat5":
                        //Statistics -> Player Lie Low Days
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerLieLowDays), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerLieLowDays).ToString(); }
                        }
                        else { CountTextTag("stat5", dictOfTags); }
                        break;
                    case "stat6":
                        //Statistics -> Lie Low Days Total
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.StatisticGetLevel(StatType.LieLowDaysTotal), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.StatisticGetLevel(StatType.LieLowDaysTotal).ToString(); }
                        }
                        else { CountTextTag("stat6", dictOfTags); }
                        break;
                    case "stat7":
                        //Statistics -> Player Give Gear
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerGiveGear), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerGiveGear).ToString(); }
                        }
                        else { CountTextTag("stat7", dictOfTags); }
                        break;
                    case "stat8":
                        //Statistics -> Gear Total
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.StatisticGetLevel(StatType.GearTotal), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.StatisticGetLevel(StatType.GearTotal).ToString(); }
                        }
                        else { CountTextTag("stat8", dictOfTags); }
                        break;
                    case "stat9":
                        //Statistics -> Player Manage Actions
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerManageActions), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerManageActions).ToString(); }
                        }
                        else { CountTextTag("stat9", dictOfTags); }
                        break;
                    case "stat10":
                        //Statistics -> Player Do Nothing actions
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerDoNothing), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerDoNothing).ToString(); }
                        }
                        else { CountTextTag("stat10", dictOfTags); }
                        break;
                    case "turn":
                        //Turn (number of days)
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.turnScript.Turn, colourEnd); }
                            else { replaceText = GameManager.i.turnScript.Turn.ToString(); }
                        }
                        else { CountTextTag("turn", dictOfTags); }
                        break;
                    case "drug":
                        //Addicted drug name
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.globalScript.tagGlobalDrug, colourEnd); }
                            else { replaceText = GameManager.i.globalScript.tagGlobalDrug; }
                        }
                        else { CountTextTag("drug", dictOfTags); }
                        break;
                    case "capture":
                        //how player status is percieved by Authority when captured
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}", GameManager.i.playerScript.GetInnocenceDescriptor()); }
                            else { replaceText = GameManager.i.globalScript.tagGlobalDrug; }
                        }
                        else { CountTextTag("capture", dictOfTags); }
                        break;
                    case "cap3":
                        //who deals with player when captured for innocence level 3
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>InterroBot</b>{1}", colourCheckText, colourEnd); }
                            else { replaceText = "InterroBot"; }
                        }
                        else { CountTextTag("cap3", dictOfTags); }
                        break;
                    case "cap3s":
                        //who deals with player when captured for innocence level 3, plural
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>InterroBots</b>{1}", colourCheckText, colourEnd); }
                            else { replaceText = "InterroBots"; }
                        }
                        else { CountTextTag("cap3s", dictOfTags); }
                        break;
                    case "cap2":
                        //who deals with player when captured for innocence level 2
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>Inquisitor</b>{1}", colourCheckText, colourEnd); }
                            else { replaceText = "Inquisitor"; }
                        }
                        else { CountTextTag("cap2", dictOfTags); }
                        break;
                    case "cap2s":
                        //who deals with player when captured for innocence level 2, plural
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>Inquisitors</b>{1}", colourCheckText, colourEnd); }
                            else { replaceText = "Inquisitors"; }
                        }
                        else { CountTextTag("cap2s", dictOfTags); }
                        break;
                    case "cap1":
                        //who deals with player when captured for innocence level 1
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>TortureBot</b>{1}", colourCheckText, colourEnd); }
                            else { replaceText = "TortureBot"; }
                        }
                        else { CountTextTag("cap1", dictOfTags); }
                        break;
                    case "cap1s":
                        //who deals with player when captured for innocence level 1, plural
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>TortureBots</b>{1}", colourCheckText, colourEnd); }
                            else { replaceText = "TortureBots"; }
                        }
                        else { CountTextTag("cap1s", dictOfTags); }
                        break;
                    case "cap0":
                        //who deals with player when captured for innocence level 0 -> mayor + first name (NOTE: [cap0] is same for plural and same as [mayor])
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.cityScript.GetCity().mayor.mayorName, colourEnd); }
                            else { replaceText = GameManager.i.cityScript.GetCity().mayor.mayorName; }
                        }
                        else { CountTextTag("cap0", dictOfTags); }
                        break;
                    case "innocence":
                        //player's level of innocence
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>Innocence {1} stars</b>{2}", colourCheckText, GameManager.i.playerScript.Innocence, colourEnd); }
                            else { replaceText = GameManager.i.globalScript.tagGlobalDrug; }
                        }
                        else { CountTextTag("innocence", dictOfTags); }
                        break;
                    case "manP":
                        //man or woman -> Player
                        if (isValidate == false)
                        {
                            if (GameManager.i.playerScript.sex == ActorSex.Male) { replaceText = "man"; }
                            else if (GameManager.i.playerScript.sex == ActorSex.Female) { replaceText = "woman"; }
                            else { Debug.LogWarningFormat("Unrecognised player sex \"{0}\"", GameManager.i.playerScript.sex); replaceText = "unknown"; }
                        }
                        else { CountTextTag("manP", dictOfTags); }
                        break;
                    case "menP":
                        //men or women -> Player
                        if (isValidate == false)
                        {
                            if (GameManager.i.playerScript.sex == ActorSex.Male) { replaceText = "men"; }
                            else if (GameManager.i.playerScript.sex == ActorSex.Female) { replaceText = "women"; }
                            else { Debug.LogWarningFormat("Unrecognised player sex \"{0}\"", GameManager.i.playerScript.sex); replaceText = "unknown"; }
                        }
                        else { CountTextTag("menP", dictOfTags); }
                        break;
                    case "manA":
                        //man or woman -> Actor
                        if (isValidate == false)
                        {
                            Actor actor = GameManager.i.dataScript.GetActor(tagActorID);
                            if (actor != null)
                            {
                                if (actor.sex == ActorSex.Male) { replaceText = "man"; }
                                else if (actor.sex == ActorSex.Female) { replaceText = "woman"; }
                                else { Debug.LogWarningFormat("Unrecognised actor sex \"{0}\"", actor.sex); replaceText = "unknown"; }
                            }
                            else { Debug.LogWarningFormat("Invalid actor (Null) for tagActorID {0}", tagActorID); replaceText = "unknown"; }
                        }
                        else { CountTextTag("manP", dictOfTags); }
                        break;
                    case "menA":
                        //men or women -> Actor
                        if (isValidate == false)
                        {
                            Actor actor = GameManager.i.dataScript.GetActor(tagActorID);
                            if (actor != null)
                            {
                                if (actor.sex == ActorSex.Male) { replaceText = "men"; }
                                else if (actor.sex == ActorSex.Female) { replaceText = "women"; }
                                else { Debug.LogWarningFormat("Unrecognised actor sex \"{0}\"", actor.sex); replaceText = "unknown"; }
                            }
                            else { Debug.LogWarningFormat("Invalid actor (Null) for tagActorID {0}", tagActorID); replaceText = "unknown"; }
                        }
                        else { CountTextTag("manP", dictOfTags); }
                        break;
                    case "guy":
                        //'guy' or 'girl' depending on player sex as in 'I'm not that kind of [guy]
                        if (isValidate == false)
                        {
                            ActorSex sex = GameManager.i.playerScript.sex;
                            if (sex == ActorSex.Male) { replaceText = "guy"; }
                            else if (sex == ActorSex.Female) { replaceText = "girl"; }
                            else { Debug.LogWarningFormat("Unrecognised actor sex \"{0}\"", sex); replaceText = "unknown"; }
                        }
                        else { CountTextTag("guy", dictOfTags); }
                        break;
                    case "hqActive":
                        //HQ topic -> 'HQ internal politics have [..]' 
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("<b>{0}</b>", textListHqActive.GetRandomRecord()); }
                            else { replaceText = textListHqActive.GetRandomRecord(); }
                        }
                        else { CountTextTag("hqActive", dictOfTags); }
                        break;
                    case "hqIssue":
                        //HQ topic -> 'are arguing [..]' 
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("<b>{0}</b>", textListHqIssue.GetRandomRecord()); }
                            else { replaceText = textListHqIssue.GetRandomRecord(); }
                        }
                        else { CountTextTag("hqIssue", dictOfTags); }
                        break;
                    case "hqConflict":
                        //HQ topic -> 'are [..]' 
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("<b>{0}</b>", textListHqConflict.GetRandomRecord()); }
                            else { replaceText = textListHqConflict.GetRandomRecord(); }
                        }
                        else { CountTextTag("hqConflict", dictOfTags); }
                        break;
                    case "hqWant":
                        //HQ topic -> 'both [..] your support' 
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("<b>{0}</b>", textListHqWant.GetRandomRecord()); }
                            else { replaceText = textListHqWant.GetRandomRecord(); }
                        }
                        else { CountTextTag("hqWant", dictOfTags); }
                        break;
                    case "hqNameA":
                        //name of HQ [actor]
                        if (isValidate == false)
                        { replaceText = string.Format("<b>{0}</b>", tagHqActorName); }
                        else { CountTextTag("hqNameA", dictOfTags); }
                        break;
                    case "hqNameO":
                        //name of HQ [other]
                        if (isValidate == false)
                        { replaceText = string.Format("<b>{0}</b>", tagHqOtherName); }
                        else { CountTextTag("hqNameO", dictOfTags); }
                        break;
                    case "hqTitleA":
                        //title of HQ [actor]
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, tagHqTitleActor, colourEnd); }
                            else { replaceText = tagHqTitleActor; }
                        }
                        else { CountTextTag("hqTitleA", dictOfTags); }
                        break;
                    case "hqTitleO":
                        //title of HQ [other]
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, tagHqTitleOther, colourEnd); }
                            else { replaceText = tagHqTitleOther; }
                        }
                        else { CountTextTag("hqTitleO", dictOfTags); }
                        break;
                    case "mega1":
                        //MegaCorporation One -> first name only
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpOne), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpOne); }
                        }
                        else { CountTextTag("mega1", dictOfTags); }
                        break;
                    case "mega2":
                        //MegaCorporation Two -> first name only
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpTwo), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpTwo); }
                        }
                        else { CountTextTag("mega2", dictOfTags); }
                        break;
                    case "mega3":
                        //MegaCorporation Three -> first name only
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpThree), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpThree); }
                        }
                        else { CountTextTag("mega3", dictOfTags); }
                        break;
                    case "mega4":
                        //MegaCorporation Four -> first name only
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpFour), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpFour); }
                        }
                        else { CountTextTag("mega4", dictOfTags); }
                        break;
                    case "mega5":
                        //MegaCorporation Five -> first name only
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpFive), colourEnd); }
                            else { replaceText = GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpFive); }
                        }
                        else { CountTextTag("mega5", dictOfTags); }
                        break;
                    case "mega1Corp":
                        //MegaCorporation One plus 'Corp'
                        if (isValidate == false)
                        {
                            string megaName = string.Format("{0} Corp", GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpOne));
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, megaName, colourEnd); }
                            else { replaceText = megaName; }
                        }
                        else { CountTextTag("mega1Corp", dictOfTags); }
                        break;
                    case "mega2Corp":
                        //MegaCorporation Two plus 'Corp'
                        if (isValidate == false)
                        {
                            string megaName = string.Format("{0} Corp", GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpTwo));
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, megaName, colourEnd); }
                            else { replaceText = megaName; }
                        }
                        else { CountTextTag("mega2Corp", dictOfTags); }
                        break;
                    case "mega3Corp":
                        //MegaCorporation Three plus 'Corp'
                        if (isValidate == false)
                        {
                            string megaName = string.Format("{0} Corp", GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpThree));
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, megaName, colourEnd); }
                            else { replaceText = megaName; }
                        }
                        else { CountTextTag("mega3Corp", dictOfTags); }
                        break;
                    case "mega4Corp":
                        //MegaCorporation Four plus 'Corp'
                        if (isValidate == false)
                        {
                            string megaName = string.Format("{0} Corp", GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpFour));
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, megaName, colourEnd); }
                            else { replaceText = megaName; }
                        }
                        else { CountTextTag("mega4Corp", dictOfTags); }
                        break;
                    case "mega5Corp":
                        //MegaCorporation Five plus 'Corp'
                        if (isValidate == false)
                        {
                            string megaName = string.Format("{0} Corp", GameManager.i.dataScript.GetMegaCorpName(MegaCorpType.MegaCorpFive));
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, megaName, colourEnd); }
                            else { replaceText = megaName; }
                        }
                        else { CountTextTag("mega5Corp", dictOfTags); }
                        break;
                    case "facR1":
                        //Resistance Americon faction 'Americon HQ'
                        if (isValidate == false)
                        {
                            string factionName = string.Format("{0} HQ", GameManager.i.globalScript.tagBlocOne);
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, factionName, colourEnd); }
                            else { replaceText = factionName; }
                        }
                        else { CountTextTag("facR1", dictOfTags); }
                        break;
                    case "facR2":
                        //Resistance Eurasia faction 'Eurasia HQ'
                        if (isValidate == false)
                        {
                            string factionName = string.Format("{0} HQ", GameManager.i.globalScript.tagBlocTwo);
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, factionName, colourEnd); }
                            else { replaceText = factionName; }
                        }
                        else { CountTextTag("facR2", dictOfTags); }
                        break;
                    case "facR3":
                        //Resistance Chinock faction 'Chinock HQ'
                        if (isValidate == false)
                        {
                            string factionName = string.Format("{0} HQ", GameManager.i.globalScript.tagBlocThree);
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, factionName, colourEnd); }
                            else { replaceText = factionName; }
                        }
                        else { CountTextTag("facR3", dictOfTags); }
                        break;
                    case "bloc1":
                        //power bloc -> 'Americon'
                        if (isValidate == false)
                        {
                            string factionName = GameManager.i.globalScript.tagBlocOne;
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, factionName, colourEnd); }
                            else { replaceText = factionName; }
                        }
                        else { CountTextTag("bloc1", dictOfTags); }
                        break;
                    case "bloc2":
                        //power bloc -> 'Eurasia'
                        if (isValidate == false)
                        {
                            string factionName = GameManager.i.globalScript.tagBlocTwo;
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, factionName, colourEnd); }
                            else { replaceText = factionName; }
                        }
                        else { CountTextTag("bloc2", dictOfTags); }
                        break;
                    case "bloc3":
                        //power bloc -> 'Chinock'
                        if (isValidate == false)
                        {
                            string factionName = GameManager.i.globalScript.tagBlocThree;
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, factionName, colourEnd); }
                            else { replaceText = factionName; }
                        }
                        else { CountTextTag("bloc3", dictOfTags); }
                        break;
                    case "hub":
                        //Luna space station 'The Hub'
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>The Hub</b>{1}", colourCheckText, colourEnd); }
                            else { replaceText = "The Hub"; }
                        }
                        else { CountTextTag("hub", dictOfTags); }
                        break;
                    case "side":
                        //Player side
                        if (isValidate == false)
                        { replaceText = string.Format("<b>{0}</b>", GameManager.i.sideScript.PlayerSide.name); }
                        else { CountTextTag("side", dictOfTags); }
                        break;
                    default:
                        //No known tag -> whatever is in the brackets, highlight it (error condition if isColourHighlighting false)
                        //NOTE: Only works if the first character is an asterisk, eg. '[*highlight me]'
                        if (isValidate == false)
                        {
                            if (tag.Substring(0, 1).Equals("*") == true)
                            {
                                string freeTag = tag.Substring(1, tag.Length - 1);
                                if (isColourHighlighting == true)
                                { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, freeTag, colourEnd); }
                                else { Debug.LogWarningFormat("Unrecognised tag \"{0}\" in \"{1}\"", freeTag, text); }
                            }
                        }
                        else
                        {
                            //exclude free tags from validation checks
                            if (tag.Substring(0, 1).Equals("*") == false)
                            { Debug.LogFormat("[Val] TopicManager.cs -> CheckTopicText: Unrecognised tag \"{0}\" for topic {1}", tag, objectName); }
                        }
                        break;
                }
                //catch all
                if (replaceText == null) { replaceText = "Unknown"; }
                //swap tag for text
                checkedText = checkedText.Remove(tagStart, length + 1);
                checkedText = checkedText.Insert(tagStart, replaceText);
            }
        }
        else { Debug.LogWarning("Invalid text (Null or Empty)"); }
        return checkedText;
    }
    #endregion

    #region CountTextTag
    /// <summary>
    /// subMethod to count text tags in topics and topicOptions for analysis purposes
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="dictOfTags"></param>
    private void CountTextTag(string tag, Dictionary<string, int> dictOfTags)
    {
        if (dictOfTags != null)
        {
            if (string.IsNullOrEmpty(tag) == false)
            {
                //already in dictionary
                if (dictOfTags.ContainsKey(tag) == true)
                {
                    //increment counter
                    dictOfTags[tag]++;
                }
                else
                {
                    //new entry, count of 1
                    try { dictOfTags.Add(tag, 1); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate key \"{0}\" in dictOfTags", tag); }
                }
            }
            else { Debug.LogWarning("Invalid tag (Null or Empty)"); }
        }
        else { Debug.LogError("Invalid dictOfTags (Null)"); }
    }
    #endregion

    #region InitialiseSprite
    /// <summary>
    /// Finds sprite for turnSprite as well as setting up associated tooltip, leaves as null if a problem
    /// tooltip created if data present, no tooltip otherwise (as no default data nothing will show on mouseover)
    /// NOTE: data checked for Null by parent method
    /// </summary>
    /// <returns></returns>
    private void InitialiseSprite(TopicUIData data)
    {
        turnSprite = null;
        if (turnTopicType != null)
        {
            switch (turnTopicType.name)
            {
                case "Actor":
                    //normal actor situation or special (dual/multiple)
                    switch (turnTopicSubType.name)
                    {
                        case "ActorPolitic":
                            //use tagActorOtherID
                            if (tagActorOtherID > -1)
                            {
                                Actor actorOther = GameManager.i.dataScript.GetActor(tagActorOtherID);
                                if (actorOther != null)
                                {
                                    turnSprite = actorOther.sprite;
                                    tagSpriteName = actorOther.actorName;
                                    //based on player mood
                                    Tuple<string, string> resultsMatch = GetPlayerTooltip();
                                    if (string.IsNullOrEmpty(resultsMatch.Item1) == false)
                                    {
                                        //tooltipMain
                                        data.imageTooltipMain = resultsMatch.Item1;
                                        //main present -> Add tooltip header (Actor name and type)
                                        data.imageTooltipHeader = string.Format("<b>{0}{1}{2}{3}{4}{5}{6}</b>", colourAlert, actorOther.arc.name, colourEnd, "\n", colourNormal, actorOther.actorName, colourEnd);
                                    }
                                    if (string.IsNullOrEmpty(resultsMatch.Item2) == false)
                                    { data.imageTooltipDetails = resultsMatch.Item2; }
                                }
                                else { Debug.LogErrorFormat("Invalid actorOther (Null) for tagActorOtherID {0}", tagActorOtherID); }
                            }
                            else { Debug.LogWarningFormat("Invalid tagActorOtherID \"{0}\"", tagActorOtherID); }
                            break;
                        default:
                            //use tagActorID
                            if (tagActorID > -1)
                            {
                                Actor actor = GameManager.i.dataScript.GetActor(tagActorID);
                                if (actor != null)
                                {
                                    turnSprite = actor.sprite;
                                    tagSpriteName = actor.actorName;
                                    Tuple<string, string> resultsActor = GetActorTooltip(actor);
                                    if (string.IsNullOrEmpty(resultsActor.Item1) == false)
                                    {
                                        //tooltipMain
                                        data.imageTooltipMain = resultsActor.Item1;
                                        //main present -> Add tooltip header (Actor name and type)
                                        data.imageTooltipHeader = string.Format("<b>{0}{1}{2}{3}{4}{5}{6}</b>", colourAlert, actor.arc.name, colourEnd, "\n", colourNormal, actor.actorName, colourEnd);
                                    }
                                    if (string.IsNullOrEmpty(resultsActor.Item2) == false)
                                    { data.imageTooltipDetails = resultsActor.Item2; }
                                }
                                else { Debug.LogErrorFormat("Invalid actor (Null) for tagActorID {0}", tagActorID); }
                            }
                            else { Debug.LogWarningFormat("Invalid tagActorID \"{0}\"", tagActorID); }
                            break;
                    }
                    break;
                case "Authority":
                    break;
                case "City":
                    break;
                case "Story":
                    TopicItem topicItem = turnTopic.topicItem;
                    if (topicItem != null)
                    {
                        turnSprite = topicItem.sprite;
                        tagSpriteName = topicItem.name; //NOTE: topicItem.name NOT sprite.name (allows topicItems to use main portraits as dupes if required)
                        data.imageTooltipHeader = string.Format("{0}<size=115%>{1}</size>{2}", colourNeutral, topicItem.tag, colourEnd);
                        data.imageTooltipMain = topicItem.descriptor;
                    }
                    else { Debug.LogWarningFormat("Invalid topicItem (Null) for turnTopic \"{0}\"", turnTopic.name); }
                    break;
                case "HQ":
                    turnSprite = GameManager.i.hqScript.GetHqMainSpirte();
                    tagSpriteName = GameManager.i.sideScript.PlayerSide.name;
                    //based on HQ approval
                    Tuple<string, string> resultsHQ = GetHqTooltip();
                    if (string.IsNullOrEmpty(resultsHQ.Item1) == false)
                    {
                        //tooltipMain
                        data.imageTooltipMain = resultsHQ.Item1;
                        //main present -> Add tooltip header (Actor name and type)
                        data.imageTooltipHeader = string.Format("<b>{0}{1} HQ{2}</b>", colourAlert, tagSpriteName, colourEnd);
                    }
                    if (string.IsNullOrEmpty(resultsHQ.Item2) == false)
                    { data.imageTooltipDetails = resultsHQ.Item2; }
                    break;
                case "Capture":
                    turnSprite = GameManager.i.spriteScript.capturedSprite;
                    tagSpriteName = "Prisoner";
                    string tooltipName = "Incarcerated";
                    //tooltip
                    Tuple<string, string> resultsCapture = GetPlayerTooltip();
                    if (string.IsNullOrEmpty(resultsCapture.Item1) == false)
                    {
                        //tooltipMain
                        data.imageTooltipMain = resultsCapture.Item1;
                        //main present -> Add tooltip header (Player name)
                        data.imageTooltipHeader = string.Format("<b>{0}{1}{2}</b>", colourAlert, tooltipName, colourEnd);
                    }
                    if (string.IsNullOrEmpty(resultsCapture.Item2) == false)
                    { data.imageTooltipDetails = resultsCapture.Item2; }
                    break;
                case "Organisation":
                    Organisation org = null;
                    switch (turnTopicSubType.name)
                    {
                        case "OrgCure": org = GameManager.i.campaignScript.campaign.details.orgCure; break;
                        case "OrgContract": org = GameManager.i.campaignScript.campaign.details.orgContract; break;
                        case "OrgHQ": org = GameManager.i.campaignScript.campaign.details.orgHQ; break;
                        case "OrgEmergency": org = GameManager.i.campaignScript.campaign.details.orgEmergency; break;
                        case "OrgInfo": org = GameManager.i.campaignScript.campaign.details.orgInfo; break;
                        default: Debug.LogWarningFormat("Unrecognised turnTopicSubType.name \"{0}\"", turnTopicSubType.name); break;
                    }
                    if (org != null)
                    {
                        turnSprite = org.sprite;
                        tagSpriteName = org.tag;
                        //tooltip
                        Tuple<string, string> resultsOrg = GetOrgTooltip(org);
                        if (string.IsNullOrEmpty(resultsOrg.Item1) == false)
                        {
                            //tooltipMain
                            data.imageTooltipMain = resultsOrg.Item1;
                            //main present -> Add tooltip header (Org name)
                            data.imageTooltipHeader = string.Format("<b>{0}{1}{2}</b>", colourAlert, tagOrgTag, colourEnd);
                        }
                        if (string.IsNullOrEmpty(resultsOrg.Item2) == false)
                        { data.imageTooltipDetails = resultsOrg.Item2; }
                    }
                    else { Debug.LogWarningFormat("Invalid org (Null) for {0}", turnTopicSubType.name); }
                    break;
                case "Player":
                    tooltipName = GameManager.i.playerScript.PlayerName;
                    switch (turnTopicSubType.name)
                    {
                        case "PlayerDistrict":
                        case "PlayerGeneral":
                            turnSprite = GameManager.i.playerScript.sprite;
                            tagSpriteName = GameManager.i.playerScript.PlayerName;
                            break;
                        case "PlayerGear":
                            //use gear sprite
                            if (string.IsNullOrEmpty(tagGear) == false)
                            {
                                Gear gear = GameManager.i.dataScript.GetGear(tagGear);
                                if (gear != null)
                                {
                                    turnSprite = gear.sprite;
                                    tagSpriteName = gear.sprite.name;
                                }
                                else { Debug.LogWarningFormat("Invalid gear (Null) for tagGear \"{0}\"", tagGear); }
                            }
                            else { Debug.LogWarning("Invalid tagGear (Null or Empty) for PlayerGear topic sprite"); }
                            break;
                        case "PlayerStats":
                        case "PlayerConditions":
                            //use actor sprite
                            if (tagActorID > -1)
                            {
                                Actor actor = GameManager.i.dataScript.GetActor(tagActorID);
                                if (actor != null)
                                {
                                    turnSprite = actor.sprite;
                                    tagSpriteName = actor.actorName;
                                    tooltipName = string.Format("{0}{1}{2}{3}{4}", colourNormal, actor.actorName, colourEnd, "\n", actor.arc.name);
                                }
                                else
                                {
                                    Debug.LogWarningFormat("Invalid actor (Null) for tagActorID \"{0}\"", tagActorID);
                                    //use player sprite as a backup
                                    turnSprite = GameManager.i.playerScript.sprite;
                                    tagSpriteName = GameManager.i.playerScript.PlayerName;
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Invalid tagActorID (less than Zero)");
                                //use player sprite as a backup
                                turnSprite = GameManager.i.playerScript.sprite;
                                tagSpriteName = GameManager.i.playerScript.PlayerName;
                            }
                            break;
                        default:
                            Debug.LogWarningFormat("Unrecognised turnTopicSubType \"{0}\"", turnTopicSubType);
                            //use player sprite as a backup
                            turnSprite = GameManager.i.playerScript.sprite;
                            tagSpriteName = GameManager.i.playerScript.PlayerName;
                            break;
                    }
                    //tooltip
                    Tuple<string, string> resultsPlayer = GetPlayerTooltip();
                    if (string.IsNullOrEmpty(resultsPlayer.Item1) == false)
                    {
                        //tooltipMain
                        data.imageTooltipMain = resultsPlayer.Item1;
                        //main present -> Add tooltip header (Player name)
                        data.imageTooltipHeader = string.Format("<b>{0}{1}{2}</b>", colourAlert, tooltipName, colourEnd);
                    }
                    if (string.IsNullOrEmpty(resultsPlayer.Item2) == false)
                    { data.imageTooltipDetails = resultsPlayer.Item2; }
                    break;
                default: Debug.LogWarningFormat("Unrecognised turnTopicType \"{0}\"", turnTopicType.name); break;
            }
        }
        else { Debug.LogWarning("Invalid turnTopicType (Null)"); }
    }
    #endregion

    #region GetActorTooltip
    /// <summary>
    /// Returns tooltip main and details for various actor subTypes (Opinion). tooltip.Header already covered by parent method. If returns nothing, which is O.K, then no tooltip is shown on mouseover
    /// Note: Actor checked for Null by parent method
    /// </summary>
    /// <param name="data"></param>
    private Tuple<string, string> GetActorTooltip(Actor actor)
    {
        string textMain = "";
        string textDetails = "";
        StringBuilder builder = new StringBuilder();
        if (turnTopicSubType != null)
        {
            switch (turnTopicSubType.name)
            {
                case "ActorContact":
                    if (tagContactID > -1)
                    {
                        Contact contact = GameManager.i.dataScript.GetContact(tagContactID);
                        if (contact != null)
                        {
                            builder.AppendFormat("{0}CONTACT{1}{2}", colourCancel, colourEnd, "\n");
                            builder.AppendFormat("{0}{1} {2}{3}{4}", colourNormal, contact.nameFirst, contact.nameLast, colourEnd, "\n");
                            builder.AppendFormat("{0}{1}{2}{3}", colourNeutral, contact.job, colourEnd, "\n");
                            builder.AppendFormat("<b>{0} {1}</b>", contact.nameFirst, GameManager.i.contactScript.GetEffectivenessFormatted(contact.effectiveness));
                            textMain = builder.ToString();
                            builder.Clear();
                            builder.AppendFormat("{0}Active for {1}{2}{3}{4} {5}turn{6}{7}{8}", colourAlert, colourEnd, colourNeutral,
                                contact.turnTotal, colourEnd, colourAlert, contact.turnTotal != 1 ? "s" : "", colourEnd, "\n");
                            builder.AppendFormat("Rumours heard  {0}<b>{1}</b>{2}{3}", colourNeutral, contact.statsRumours, colourEnd, "\n");
                            builder.AppendFormat("Nemesis sightings  {0}<b>{1}</b>{2}{3}", colourNeutral, contact.statsNemesis, colourEnd, "\n");
                            builder.AppendFormat("Erasure Teams spotted  {0}<b>{1}</b>{2}{3}", colourNeutral, contact.statsTeams, colourEnd, "\n");
                            textDetails = builder.ToString();
                        }
                        else { Debug.LogErrorFormat("Invalid contact (Null) for tagContactID {0}", tagContactID); }
                    }
                    else { Debug.LogWarningFormat("Invalid tagContactID {0}", tagContactID); }
                    break;

                default:
                    //info on whether topic is good or bad and why
                    switch (turnTopic.group.name)
                    {
                        case "Good":
                            textMain = string.Format("{0}<size=115%>GOOD{1}{2}{3}Event</size>{4}", colourGood, colourEnd, "\n", colourNormal, colourEnd);
                            break;
                        case "Bad":
                            textMain = string.Format("{0}<size=115%>BAD{1}{2}{3}Event</size>{4}", colourBad, colourEnd, "\n", colourNormal, colourEnd);
                            break;
                        default: Debug.LogWarningFormat("Unrecognised turnTopic.group \"{0}\"", turnTopic.group.name); break;
                    }
                    //details
                    int opinion = actor.GetDatapoint(ActorDatapoint.Opinion1);
                    int oddsGood = chanceNeutralGood;
                    int oddsBad = 100 - chanceNeutralGood;
                    builder.AppendFormat("Determined by{0}{1}{2}'s{3}{4}{5}<size=110%>Opinion</size>{6}{7}", "\n", colourAlert, actor.arc.name, colourEnd, "\n", colourNeutral, colourEnd, "\n");
                    //highlight current opinion band, grey out the rest
                    if (opinion == 3) { builder.AppendFormat("if {0}3{1}, {2}Good{3}{4}", colourNeutral, colourEnd, colourGood, colourEnd, "\n"); }
                    else { builder.AppendFormat("<size=90%>{0}if 3, Good{1}{2}</size>", colourGrey, colourEnd, "\n"); }

                    if (opinion == 2) { builder.AppendFormat("if {0}2{1}, could be either{2}<size=90%>({3}/{4} Good/Bad)</size>{5}", colourNeutral, colourEnd, "\n", oddsGood, oddsBad, "\n"); }
                    else { builder.AppendFormat("<size=90%>{0}if 2, could be either{1}({2}/{3} Good/Bad){4}{5}</size>", colourGrey, "\n", oddsGood, oddsBad, colourEnd, "\n"); }

                    if (opinion < 2) { builder.AppendFormat("if {0}1{1} or {2}0{3}, {4}Bad{5}", colourNeutral, colourEnd, colourNeutral, colourEnd, colourBad, colourEnd); }
                    else { builder.AppendFormat("<size=90%>{0}if 1 or 0, Bad{1}</size>", colourGrey, colourEnd); }
                    textDetails = builder.ToString();
                    break;
            }
        }
        else { Debug.LogWarning("Invalid turnTopicSubType (Null)"); }
        return new Tuple<string, string>(textMain, textDetails);
    }
    #endregion

    #region GetPlayerTooltip
    /// <summary>
    /// Returns tooltip main and details for various player subTypes (Mood). tooltip.Header already covered by parent method. If returns nothing, which is O.K, then no tooltip is shown on mouseover
    /// Note: also does actorMatch as this is based around Player mood as well
    /// </summary>
    /// <param name="data"></param>
    private Tuple<string, string> GetPlayerTooltip()
    {
        string textMain = "";
        string textDetails = "";
        StringBuilder builder = new StringBuilder();
        if (turnTopicSubType != null)
        {
            switch (turnTopicSubType.name)
            {
                case "PlayerDistrict":
                case "PlayerGeneral":
                case "PlayerStats":
                case "PlayerGear":
                case "PlayerConditions":
                case "CaptureSub":
                case "ActorMatch":
                case "ActorPolitic":
                    //info on whether topic is good or bad and why
                    switch (turnTopic.group.name)
                    {
                        case "Good":
                            textMain = string.Format("{0}<size=115%>GOOD{1}{2}{3}Event</size>{4}", colourGood, colourEnd, "\n", colourNormal, colourEnd);
                            break;
                        case "Bad":
                            textMain = string.Format("{0}<size=115%>BAD{1}{2}{3}Event</size>{4}", colourBad, colourEnd, "\n", colourNormal, colourEnd);
                            break;
                        default: Debug.LogWarningFormat("Unrecognised turnTopic.group \"{0}\"", turnTopic.group.name); break;
                    }
                    //details
                    int mood = GameManager.i.playerScript.GetMood();
                    int oddsGood = chanceNeutralGood;
                    int oddsBad = 100 - chanceNeutralGood;
                    builder.AppendFormat("Determined by{0}{1}{2}'s{3}{4}{5}<size=110%>Mood</size>{6}{7}", "\n", colourAlert, "Player", colourEnd, "\n", colourNeutral, colourEnd, "\n");
                    //highlight current mood band, grey out the rest
                    if (mood == 3) { builder.AppendFormat("if {0}3{1}, {2}Good{3}{4}", colourNeutral, colourEnd, colourGood, colourEnd, "\n"); }
                    else { builder.AppendFormat("<size=90%>{0}if 3, Good{1}{2}</size>", colourGrey, colourEnd, "\n"); }

                    if (mood == 2) { builder.AppendFormat("if {0}2{1}, could be either{2}<size=90%>({3}/{4} Good/Bad)</size>{5}", colourNeutral, colourEnd, "\n", oddsGood, oddsBad, "\n"); }
                    else { builder.AppendFormat("<size=90%>{0}if 2, could be either{1}({2}/{3} Good/Bad){4}{5}</size>", colourGrey, "\n", oddsGood, oddsBad, colourEnd, "\n"); }

                    if (mood < 2) { builder.AppendFormat("if {0}1{1} or {2}0{3}, {4}Bad{5}", colourNeutral, colourEnd, colourNeutral, colourEnd, colourBad, colourEnd); }
                    else { builder.AppendFormat("<size=90%>{0}if 1 or 0, Bad{1}</size>", colourGrey, colourEnd); }
                    textDetails = builder.ToString();
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised turnTopicSubType \"{0}\"", turnTopicSubType.name);
                    break;
            }
        }
        else { Debug.LogWarning("Invalid turnTopicSubType (Null)"); }
        return new Tuple<string, string>(textMain, textDetails);
    }
    #endregion

    #region GetOrgTooltip
    /// <summary>
    /// Returns tooltip main and details for various org subTypes (Reputation). tooltip.Header already covered by parent method. If returns nothing, which is O.K, then no tooltip is shown on mouseover
    /// NOTE: Organisation checked for null by parent method
    /// </summary>
    /// <param name="org"></param>
    /// <returns></returns>
    Tuple<string, string> GetOrgTooltip(Organisation org)
    {
        string textMain = "";
        string textDetails = "";
        StringBuilder builder = new StringBuilder();
        //info on whether topic is good or bad and why
        switch (turnTopic.group.name)
        {
            case "Good":
                textMain = string.Format("{0}<size=115%>GOOD{1}{2}{3}Event</size>{4}", colourGood, colourEnd, "\n", colourNormal, colourEnd);
                break;
            case "Bad":
                textMain = string.Format("{0}<size=115%>BAD{1}{2}{3}Event</size>{4}", colourBad, colourEnd, "\n", colourNormal, colourEnd);
                break;
            default: Debug.LogWarningFormat("Unrecognised turnTopic.group \"{0}\"", turnTopic.group.name); break;
        }
        //details
        int reputation = org.GetReputation();
        int oddsGood = chanceNeutralGood;
        int oddsBad = 100 - chanceNeutralGood;
        builder.AppendFormat("Determined by{0}{1}{2}'s{3}{4}{5}<size=110%>Reputation</size>{6}{7}", "\n", colourAlert, org.tag, colourEnd, "\n", colourNeutral, colourEnd, "\n");
        //highlight current reputation band, grey out the rest
        if (reputation == 3) { builder.AppendFormat("if {0}3{1}, {2}Good{3}{4}", colourNeutral, colourEnd, colourGood, colourEnd, "\n"); }
        else { builder.AppendFormat("<size=90%>{0}if 3, Good{1}{2}</size>", colourGrey, colourEnd, "\n"); }

        if (reputation == 2) { builder.AppendFormat("if {0}2{1}, could be either{2}<size=90%>({3}/{4} Good/Bad)</size>{5}", colourNeutral, colourEnd, "\n", oddsGood, oddsBad, "\n"); }
        else { builder.AppendFormat("<size=90%>{0}if 2, could be either{1}({2}/{3} Good/Bad){4}{5}</size>", colourGrey, "\n", oddsGood, oddsBad, colourEnd, "\n"); }

        if (reputation < 2) { builder.AppendFormat("if {0}1{1} or {2}0{3}, {4}Bad{5}", colourNeutral, colourEnd, colourNeutral, colourEnd, colourBad, colourEnd); }
        else { builder.AppendFormat("<size=90%>{0}if 1 or 0, Bad{1}</size>", colourGrey, colourEnd); }
        textDetails = builder.ToString();
        return new Tuple<string, string>(textMain, textDetails);
    }
    #endregion

    #region GetHqTooltip
    /// <summary>
    /// Returns tooltip main and details based on faction (HQ) approval. tooltip.Header already covered by parent method. If returns nothing, which is O.K, then no tooltip is shown on mouseover
    /// </summary>
    /// <returns></returns>
    Tuple<string, string> GetHqTooltip()
    {
        string textMain = "";
        string textDetails = "";
        StringBuilder builder = new StringBuilder();
        //info on whether topic is good or bad and why
        switch (turnTopic.group.name)
        {
            case "Good":
                textMain = string.Format("{0}<size=115%>GOOD{1}{2}{3}Event</size>{4}", colourGood, colourEnd, "\n", colourNormal, colourEnd);
                break;
            case "Bad":
                textMain = string.Format("{0}<size=115%>BAD{1}{2}{3}Event</size>{4}", colourBad, colourEnd, "\n", colourNormal, colourEnd);
                break;
            default: Debug.LogWarningFormat("Unrecognised turnTopic.group \"{0}\"", turnTopic.group.name); break;
        }
        //details
        int approval = GameManager.i.hqScript.GetHqApproval();
        int oddsGood = 50;
        int oddsBad = 100 - oddsGood;
        builder.AppendFormat("Determined by{0}{1}<size=110%>HQ Approval</size>{2}{3}", "\n", colourAlert, colourEnd, "\n");
        //highlight current reputation band, grey out the rest
        if (approval >= 6) { builder.AppendFormat("if {0}6+{1}, {2}Good{3}{4}", colourNeutral, colourEnd, colourGood, colourEnd, "\n"); }
        else { builder.AppendFormat("<size=90%>{0}if 6+, Good{1}{2}</size>", colourGrey, colourEnd, "\n"); }

        if (approval < 6 && approval > 3) { builder.AppendFormat("if {0}4 or 5{1}, could be either{2}<size=90%>({3}/{4} Good/Bad)</size>{5}", colourNeutral, colourEnd, "\n", oddsGood, oddsBad, "\n"); }
        else { builder.AppendFormat("<size=90%>{0}if 4 or 5, could be either{1}({2}/{3} Good/Bad){4}{5}</size>", colourGrey, "\n", oddsGood, oddsBad, colourEnd, "\n"); }

        if (approval < 4) { builder.AppendFormat("if {0}3 or less{1}, {2}Bad{3}", colourNeutral, colourEnd, colourBad, colourEnd); }
        else { builder.AppendFormat("<size=90%>{0}if 3 or less, Bad{1}</size>", colourGrey, colourEnd); }
        textDetails = builder.ToString();
        return new Tuple<string, string>(textMain, textDetails);
    }
    #endregion

    #region GetTopicSubTypeHelp
    /// <summary>
    /// returns a list of Help tags for the topicSubType for use with the optional second help icon on the topic UI. Returns null if none (not an error condition as help for subTypes is optional)
    /// </summary>
    /// <returns></returns>
    private List<string> GetTopicSubTypeHelp()
    {
        List<string> listOfHelp = null;
        switch (turnTopicSubType.name)
        {
            case "ActorPolitic": listOfHelp = new List<string>() { "topicSub_0", "topicSub_1" }; break;
            case "ActorMatch": listOfHelp = new List<string>() { "topicSub_2", "topicSub_3" }; break;
            case "ActorGear": listOfHelp = new List<string>() { "topicSub_24", "topicSub_25" }; break;
            case "ActorDistrict": listOfHelp = new List<string>() { "topicSub_4", "topicSub_5", "topicSub_6" }; break;
            case "ActorContact": listOfHelp = new List<string>() { "topicSub_7", "topicSub_8", "topicSub_9" }; break;
            case "PlayerConditions": listOfHelp = new List<string>() { "topicSub_10", "topicSub_11" }; break;
            case "PlayerStats": listOfHelp = new List<string>() { "topicSub_12", "topicSub_13", "topicSub_14" }; break;
            case "PlayerGeneral": listOfHelp = new List<string>() { "topicSub_15", "topicSub_16" }; break;
            case "PlayerDistrict": listOfHelp = new List<string>() { "topicSub_17", "topicSub_18" }; break;
            case "HQSub": listOfHelp = new List<string>() { "topicSub_19", "topicSub_20" }; break;
            case "OrgContract":
            case "OrgCure":
            case "OrgEmergency":
            case "OrgHQ":
            case "OrgInfo": listOfHelp = new List<string>() { "topicSub_21", "topicSub_22", "topicSub_23" }; break;
            case "StoryAlpha": listOfHelp = new List<string>() { "topicSub_26", "topicSub_27", "topicSub_30" }; break;
            case "StoryBravo": listOfHelp = new List<string>() { "topicSub_28", "topicSub_27", "topicSub_30" }; break;
            case "StoryCharlie": listOfHelp = new List<string>() { "topicSub_29", "topicSub_27", "topicSub_30" }; break;
                //no default as it only picks up what's needed
        }
        return listOfHelp;
    }
    #endregion

    #region SetStoryFlag
    /// <summary>
    /// Sets story flag to specified value. Flag number is assumed to be current scenarioIndex. Returns true if flag set successfully to true
    /// </summary>
    /// <param name="storyType"></param>
    /// <param name="value"></param>
    public bool SetStoryFlag(StoryType storyType, int value)
    {
        int flagNumber = GameManager.i.campaignScript.GetScenarioIndex();
        if (flagNumber < 5 && flagNumber > -1)
        {
            arrayOfStoryFlags[(int)storyType, flagNumber] = value;
            Debug.LogFormat("[Sto] TopicManager.cs -> SetStoryFlag: story FLAG [{0}, {1}] now {2}{3}", storyType, flagNumber, value, "\n");
            return true;
        }
        else { Debug.LogErrorFormat("Invalid flagNumber \"{0}\" (should be 0 to 4)", flagNumber); }
        return false;
    }
    #endregion

    #region CheckStoryFlag
    /// <summary>
    /// returns value of specified story flag. 'flagNumber' is assumed to be the current scenarioIndex. Returns -1 if a problem
    /// </summary>
    /// <param name="storyType"></param>
    /// <returns></returns>
    public int CheckStoryFlag(StoryType storyType)
    {
        int flagNumber = GameManager.i.campaignScript.GetScenarioIndex();
        if (flagNumber < 5 && flagNumber > -1)
        { return arrayOfStoryFlags[(int)storyType, flagNumber]; }
        else { Debug.LogErrorFormat("Invalid flagNumber \"{0}\" (should be 0 to 4)", flagNumber); }
        return -1;
    }
    #endregion

    #region SetStoryStar
    /// <summary>
    /// Sets story star to specified value. 'starNumber' is 0 to 4 and is assumed to be currentScenarioIndex. Returns true if successful (star set to true)
    /// </summary>
    /// <param name="storyType"></param>
    /// <param name="value"></param>
    public bool SetStoryStar(StoryType storyType, int value)
    {
        int starNumber = GameManager.i.campaignScript.GetScenarioIndex();
        if (starNumber < 5 && starNumber > -1)
        {
            arrayOfStoryStars[(int)storyType, starNumber] = value;
            Debug.LogFormat("[Sto] TopicManager.cs -> SetStoryStar: story STAR [{0}, {1}] now {2}{3}", storyType, starNumber, value, "\n");
            return true;
        }
        else { Debug.LogErrorFormat("Invalid starNumber \"{0}\" (should be 0 to 4)", starNumber); }
        return false;
    }
    #endregion

    #region CheckStoryStar
    /// <summary>
    /// returns value of specified story star. 'starNumber' is assumed to be the current scenarioIndex. Returns -1 if a problem
    /// </summary>
    /// <param name="storyType"></param>
    /// <returns></returns>
    public int CheckStoryStar(StoryType storyType)
    {
        int starNumber = GameManager.i.campaignScript.GetScenarioIndex();
        if (starNumber < 5 && starNumber > -1)
        { return arrayOfStoryStars[(int)storyType, starNumber]; }
        else { Debug.LogErrorFormat("Invalid starNumber \"{0}\" (should be 0 to 4)", starNumber); }
        return -1;
    }
    #endregion

    #region SetHaltExecutionTopic
    /// <summary>
    /// Sets haltExecutionTopic to isSetting (default false)
    /// </summary>
    /// <param name="isFalse"></param>
    public void SetHaltExecutionTopic(bool isSetting = false)
    { haltExecutionTopic = isSetting; }
    #endregion

    #endregion

    #region Meta Methods...
    //
    // - - - MetaManager - - -
    //

    /// <summary>
    /// Runs at end of level, MetaManager.cs -> ProcessMetaGame, to clear out relevant topic data and update others (eg. timesUsedCampaign += timesUsedLevel)
    /// </summary>
    /// <param name="dictOfTopicTypeData"></param>
    public void ProcessMetaTopics()
    {
        //dictOfType/SubType
        UpdateTopicTypes(GameManager.i.dataScript.GetDictOfTopicTypeData());
        UpdateTopicTypes(GameManager.i.dataScript.GetDictOfTopicSubTypeData());
    }

    /// <summary>
    /// subMethod for ProcessMetaTopics to handle dictOfTopicType/SubType
    /// </summary>
    /// <param name="dictOfTopicTypeData"></param>
    private void UpdateTopicTypes(Dictionary<string, TopicTypeData> dictOfTopicTypeData)
    {
        foreach (var record in dictOfTopicTypeData)
        {
            record.Value.isAvailable = true;
            record.Value.turnLastUsed = 0;
            record.Value.timesUsedCampaign += record.Value.timesUsedLevel;
            record.Value.timesUsedLevel = 0;
        }
    }
    #endregion

    #region External...
    //
    // External hooks into TopicManager.cs
    //

    /// <summary>
    /// Used by TutorialUI.cs -> GetTopicData
    /// </summary>
    /// <param name="optionText"></param>
    /// <returns></returns>
    public string GetOptionString(string optionText)
    {
        return string.Format("{0}{1}{2}", colourNeutral, CheckTopicText(optionText, false), colourEnd);
    }

    #endregion

    #region Debug Methods...
    //
    // - - - Debug - - -
    //

    /// <summary>
    /// displays news texts for each topic in debug topic pool to enable checking
    /// </summary>
    public void DebugTestNews()
    {
        //can only do if no GUI elements visible
        if (GameManager.i.inputScript.ModalState == ModalState.Normal)
        {
            string newsSnippet = "";
            TopicPool pool = GameManager.i.testScript.debugTopicPool;
            if (pool != null)
            {
                List<Topic> listOfTopics = pool.listOfTopics;
                if (listOfTopics != null)
                {
                    int maxNodeID = GameManager.i.nodeScript.maxNodeValue;
                    tagOrgTag = GameManager.i.campaignScript.campaign.details.orgInfo.tag;
                    if (Random.Range(0, 100) < 50) { tagRecruit = GameManager.i.cityScript.GetCity().nameSet.firstFemaleNames.GetRandomRecord(); }
                    else { tagRecruit = GameManager.i.cityScript.GetCity().nameSet.firstMaleNames.GetRandomRecord(); }
                    tagRecruit += " " + GameManager.i.cityScript.GetCity().nameSet.lastNames.GetRandomRecord();
                    tagTeam = "ERASURE Team Alpha";
                    int count = listOfTopics.Count;
                    if (count > 0)
                    {
                        Sprite debugSprite = GameManager.i.spriteScript.topicDefaultSprite;
                        coroutine = DisplayNews(listOfTopics, newsSnippet, debugSprite, maxNodeID);
                        StartCoroutine(coroutine);
                    }
                    else { Debug.LogErrorFormat("Invalid listOfTopics (Empty) for topicPool \"{0}\"", pool.name); }
                }
                else { Debug.LogErrorFormat("Invalid listOfTopics (Null) for topicPool \"{0}\"", pool.name); }
            }
        }
        else { Debug.LogWarning("Invalid modalState (must be NORMAL"); }
    }

    /// <summary>
    /// Coroutine that loops all topics in a topic pool, all options within each topic and displays an individual outcome message showing the newsSnippet for each
    /// haltExecution is a class variable that controls the display of the outcome windows. OnEvent.CloseOutcomeWindow sets haltExecution to false allowing next outcome window in sequence to display
    /// </summary>
    /// <param name="listOfTopics"></param>
    /// <param name="newsSnippet"></param>
    /// <param name="debugSprite"></param>
    /// <param name="maxNodeID"></param>
    /// <returns></returns>
    IEnumerator DisplayNews(List<Topic> listOfTopics, string newsSnippet, Sprite debugSprite, int maxNodeID)
    {
        int count = listOfTopics.Count;
        //loop topics
        for (int i = 0; i < count; i++)
        {
            Topic topic = listOfTopics[i];
            if (topic != null)
            {
                //loop options within topic
                List<TopicOption> listOfOptions = topic.listOfOptions;
                if (listOfOptions != null)
                {
                    for (int j = 0; j < listOfOptions.Count; j++)
                    {
                        tagNodeID = Random.Range(0, maxNodeID);
                        SetHaltExecutionTopic(true);
                        newsSnippet = CheckTopicText(listOfOptions[j].news, false);
                        ModalOutcomeDetails details = new ModalOutcomeDetails()
                        {
                            textTop = string.Format("Topic: {0}{1}{2}{3}Option: {4}{5}{6}", colourNeutral, topic.name, colourEnd, "\n", colourAlert, listOfOptions[j].name, colourEnd),
                            textBottom = newsSnippet,
                            sprite = debugSprite
                        };
                        EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
                        yield return new WaitUntil(() => haltExecutionTopic == false);
                    }
                }
                else { Debug.LogErrorFormat("Invalid listOfOptions (Null) for topic \"{0}\"", topic.name); }
            }
            else { Debug.LogErrorFormat("Invalid listOfTopics[{0}]", i); }
        }
    }

    /// <summary>
    /// displays option tag, text and storyInfo (if present) for each topic in debug topic pool to enable checking
    /// </summary>
    public void DebugTestTopicOptions()
    {
        //can only do if no GUI elements visible
        if (GameManager.i.inputScript.ModalState == ModalState.Normal)
        {
            TopicPool pool = GameManager.i.testScript.debugTopicPool;
            if (pool != null)
            {
                List<Topic> listOfTopics = pool.listOfTopics;
                if (listOfTopics != null)
                {
                    int count = listOfTopics.Count;
                    if (count > 0)
                    {
                        coroutine = DisplayTopicOptions(listOfTopics);
                        StartCoroutine(coroutine);
                    }
                    else { Debug.LogErrorFormat("Invalid listOfTopics (Empty) for topicPool \"{0}\"", pool.name); }
                }
                else { Debug.LogErrorFormat("Invalid listOfTopics (Null) for topicPool \"{0}\"", pool.name); }
            }
        }
        else { Debug.LogWarning("Invalid modalState (must be NORMAL"); }
    }

    /// <summary>
    /// Coroutine that loops all topics in a topic pool, all options within each topic and displays an individual outcome message showing option text details for each
    /// haltExecution is a class variable that controls the display of the outcome windows. OnEvent.CloseOutcomeWindow sets haltExecution to false allowing next outcome window in sequence to display
    /// </summary>
    /// <param name="listOfTopics"></param>
    /// <returns></returns>
    IEnumerator DisplayTopicOptions(List<Topic> listOfTopics)
    {
        int count = listOfTopics.Count;
        string topicText, optionText, storyText;
        Sprite debugSprite = GameManager.i.spriteScript.topicDefaultSprite;
        Sprite topicSprite;
        //loop topics
        for (int i = 0; i < count; i++)
        {
            Topic topic = listOfTopics[i];
            if (topic != null)
            {
                topicSprite = null;
                //topic text/letter
                if (topic.letter != null)
                {
                    topicText = string.Format("<size=80%>{0}Dear {1}{2}{3}{4}{5}{6}{7}{8}</size>", colourNormal, CheckTopicText(topic.letter.textDear), "\n", "\n",
                        CheckTopicText(topic.letter.textTop), "\n", "\n", CheckTopicText(topic.letter.textBottom), colourEnd);
                }
                else if (topic.comms != null)
                {
                    topicText = string.Format("<size=80%>{0}FRM: {1}, {2}{3}{4}{5}{6}{7}{8}{9}</size>", colourNormal, topic.comms.textFrom, topic.comms.textWhere, "\n", "\n",
                        CheckTopicText(topic.comms.textTop), "\n", "\n", CheckTopicText(topic.comms.textBottom), colourEnd);
                }
                else
                { topicText = string.Format("<size=80%>{0}{1}{2}</size>", colourNormal, CheckTopicText(topic.text), colourEnd); }
                //sprite
                if (topic.topicItem != null)
                { topicSprite = topic.topicItem.sprite; }
                //loop options within topic
                List<TopicOption> listOfOptions = topic.listOfOptions;
                if (listOfOptions != null)
                {
                    for (int j = 0; j < listOfOptions.Count; j++)
                    {
                        TopicOption option = listOfOptions[j];
                        if (option != null)
                        {
                            SetHaltExecutionTopic(true);
                            optionText = CheckTopicText(option.text, false);
                            //storyInfo if present
                            if (string.IsNullOrEmpty(option.storyInfo) == false)
                            { storyText = CheckTopicText(option.storyInfo); }
                            else
                            {
                                //storyTarget if present
                                if (option.storyTarget != null)
                                {
                                    storyText = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}", option.storyTarget.targetName, "\n", colourAlert,
                                      option.storyTarget.descriptorResistance, colourEnd, "\n",
                                      option.storyTarget.actorArc.name, "\n", colourAlert, option.storyTarget.gear.name, colourEnd, "\n", option.storyTarget.nodeArc.name);
                                }
                                else { storyText = ""; }
                            }
                            ModalOutcomeDetails details = new ModalOutcomeDetails()
                            {
                                textTop = string.Format("Topic: {0}{1}{2}{3}Option: {4}{5}{6}{7}{8}{9}", colourNeutral, topic.name, colourEnd, "\n",
                                    colourAlert, option.name, colourEnd, "\n", "\n", topicText),
                                textBottom = string.Format("{0}{1}{2}{3}{4}{5}", option.tag, "\n", colourNeutral, optionText, colourEnd,
                                    storyText.Length > 0 ? string.Format("{0}{1}{2}", "\n", "\n", storyText) : ""),
                                sprite = topicSprite != null ? topicSprite : debugSprite
                            };
                            EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
                            yield return new WaitUntil(() => haltExecutionTopic == false);
                        }
                        else { Debug.LogWarningFormat("Invalid topicOption (Null) for topic \"{0}\", listOfOptions[{1}]", topic.name, j); }
                    }
                }
                else { Debug.LogErrorFormat("Invalid listOfOptions (Null) for topic \"{0}\"", topic.name); }
            }
            else { Debug.LogErrorFormat("Invalid listOfTopics[{0}]", i); }
        }
    }

    /// <summary>
    /// displays all story Help within storyData that corresponds to debug topic pool to enable checking
    /// </summary>
    public void DebugTestStoryHelp()
    {
        //can only do if no GUI elements visible
        if (GameManager.i.inputScript.ModalState == ModalState.Normal)
        {
            TopicPool pool = GameManager.i.testScript.debugTopicPool;
            StoryData data = null;
            if (pool != null)
            {
                StoryData[] arrayOfStoryData = GameManager.i.loadScript.arrayOfStoryData;
                if (arrayOfStoryData != null)
                {
                    //find storyData that has the corresponding pool
                    for (int i = 0; i < arrayOfStoryData.Length; i++)
                    {
                        if (arrayOfStoryData[i].pool.name.Equals(pool.name, StringComparison.Ordinal) == true)
                        {
                            data = arrayOfStoryData[i];
                            break;
                        }
                    }
                    if (data != null)
                    {
                        List<StoryHelp> listOfHelp = data.listOfStoryHelp;
                        if (listOfHelp != null)
                        {
                            coroutine = DisplayStoryHelp(listOfHelp);
                            StartCoroutine(coroutine);
                        }
                        else { Debug.LogWarningFormat("Invalid listOfHelp (Null) for StoryData \"{0}\"", data.name); }
                    }
                    else { Debug.LogWarningFormat("TopicPool \"{0}\" not found in arrayOfStoryData", pool.name); }
                }
                else { Debug.LogError("Invalid arrayOfStoryData (Null)"); }
            }
            else { Debug.LogWarning("TestManager.cs -> debugTopicPool is Empty (Null)"); }
        }
        else { Debug.LogWarning("Invalid modalState (must be NORMAL)"); }
    }

    /// <summary>
    /// Coroutine that loops all topics in a topic pool, all options within each topic and displays an individual outcome message showing the newsSnippet for each
    /// Use SPACE or ESC to flip through storyHelp instances
    /// </summary>
    /// <param name="listOfTopics"></param>
    /// <param name="newsSnippet"></param>
    /// <param name="debugSprite"></param>
    /// <param name="maxNodeID"></param>
    /// <returns></returns>
    IEnumerator DisplayStoryHelp(List<StoryHelp> listOfHelp)
    {
        int count = listOfHelp.Count;
        GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.Debug });
        //loop help
        for (int i = 0; i < count; i++)
        {
            StoryHelp help = listOfHelp[i];
            if (help != null)
            {
                SetHaltExecutionTopic(true);
                GameManager.i.tooltipStoryScript.SetTooltip(help, new Vector3(Screen.width / 2, Screen.height / 2));
            }
            else { Debug.LogErrorFormat("Invalid listOfTopics[{0}]", i); }
            yield return new WaitUntil(() => haltExecutionTopic == false);
        }
        GameManager.i.inputScript.ResetStates();
    }


    /// <summary>
    /// Display's topic type data in a more user friendly manner (subTypes grouped by Types)
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTopicTypes()
    {
        StringBuilder builder = new StringBuilder();
        Dictionary<string, TopicTypeData> dictOfTopicTypes = GameManager.i.dataScript.GetDictOfTopicTypeData();
        if (dictOfTopicTypes != null)
        {
            Dictionary<string, TopicTypeData> dictOfTopicSubTypes = GameManager.i.dataScript.GetDictOfTopicSubTypeData();
            if (dictOfTopicSubTypes != null)
            {
                GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
                builder.AppendFormat("- TopicTypeData for TopicTypes ('*' topic/subType -> valid Criteria){0}{1}", "\n", "\n");
                //used to print a '*' to indicate topic / subType is valid and ready to go
                bool isValidType, isValidSubType;
                //loop topic Types
                foreach (var topicTypeData in dictOfTopicTypes)
                {
                    TopicType topicType = GameManager.i.dataScript.GetTopicType(topicTypeData.Key);
                    if (topicType != null)
                    {
                        if (topicType.side.level == playerSide.level || topicType.side.level == 3)
                        {
                            isValidType = DebugCheckValidType(topicTypeData.Value);
                            builder.AppendFormat(" {0}{1}{2}", DebugDisplayTypeRecord(topicTypeData.Value), isValidType == true ? " *" : "", "\n");
                            //loop subTypes
                            foreach (TopicSubType subType in topicType.listOfSubTypes)
                            {
                                //needs to be the correct side
                                if (subType.side.level == playerSide.level || subType.side.level == 3)
                                {
                                    TopicTypeData subData = GameManager.i.dataScript.GetTopicSubTypeData(subType.name);
                                    if (subData != null)
                                    {
                                        isValidSubType = false;
                                        if (isValidType == true)
                                        { isValidSubType = DebugCheckValidType(subData); }
                                        builder.AppendFormat("  {0}{1}{2}", DebugDisplayTypeRecord(subData), isValidSubType == true ? " *" : "", "\n");
                                    }
                                    else { Debug.LogWarningFormat("Invalid subData (Null) for topicSubType \"{0}\"", subType.name); }
                                }
                                else { builder.AppendFormat("   {0} -> Not Valid for this Side{1}", subType.name, "\n"); }
                            }
                            builder.AppendLine();
                        }
                        else { builder.AppendFormat("  {0} -> Not valid for this side{1}{2}", topicType.name, "\n", "\n"); }
                    }
                    else { Debug.LogError("Invalid topicType (Null)"); }
                }
            }
            else { Debug.LogError("Invalid dictOfTopicSubTypes (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfTopicTypes (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// Sub method for DebugDisplayTopicTypes to display a TopicTypeData record (topicType / SubType)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private string DebugDisplayTypeRecord(TopicTypeData data)
    {
        return string.Format(" {0} Av {1}, Lst {2}, Min {3}, #Lv {4}, #Ca {5}", data.type, data.isAvailable, data.turnLastUsed, data.minInterval,
                         data.timesUsedLevel, data.timesUsedCampaign);
    }

    /// <summary>
    /// display two lists of topic Types
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTopicTypeLists()
    {
        StringBuilder builder = new StringBuilder();
        //listOfTopicTypesLevel
        builder.AppendFormat("- listOfTopicTypesLevel{0}", "\n");
        List<TopicType> listOfTopicTypeLevel = GameManager.i.dataScript.GetListOfTopicTypesLevel();
        if (listOfTopicTypeLevel != null)
        {
            if (listOfTopicTypeLevel.Count > 0)
            {
                foreach (TopicType typeLevel in listOfTopicTypeLevel)
                { builder.AppendFormat(" {0}{1}", typeLevel.tag, "\n"); }
            }
            else { builder.AppendFormat(" No records{0}", "\n"); }
        }
        else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
        //listOfTopicTypesTurn
        builder.AppendFormat("{0}- listOfTopicTypesTurn{1}", "\n", "\n");
        if (listOfTopicTypesTurn != null)
        {
            if (listOfTopicTypesTurn.Count > 0)
            {
                foreach (TopicType typeTurn in listOfTopicTypesTurn)
                { builder.AppendFormat(" {0}{1}", typeTurn.tag, "\n"); }
            }
            else { builder.AppendFormat(" No records{0}", "\n"); }
        }
        else { Debug.LogError("Invalid listOfTopicTypesTurn (Null)"); }
        //topicHistory
        Dictionary<int, HistoryTopic> dictOfTopicHistory = GameManager.i.dataScript.GetDictOfTopicHistory();
        if (dictOfTopicHistory != null)
        {
            builder.AppendFormat("{0} - dictOfTopicHistory ({1} records){2}", "\n", dictOfTopicHistory.Count, "\n");
            if (dictOfTopicHistory.Count > 0)
            {
                foreach (var history in dictOfTopicHistory)
                {
                    builder.AppendFormat("  t {0} -> # {1} -> {2} -> {3} -> {4} -> {5}{6}", history.Value.turn, history.Value.numSelect,
                      history.Value.topicType, history.Value.topicSubType, history.Value.topic, history.Value.option, "\n");
                }
            }
            else { builder.AppendFormat("  No records{0}", "\n"); }
        }
        else { Debug.LogError("Invalid dictOfTopicHistory (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug display of topic Type criteria
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayCriteria()
    {
        int counter = 0;
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- listOfTopicTypes -> Criteria{0}", "\n");
        //listOfTopicTypes
        List<TopicType> listOfTopicTypes = GameManager.i.dataScript.GetListOfTopicTypes();
        if (listOfTopicTypes != null)
        {
            foreach (TopicType topicType in listOfTopicTypes)
            {
                counter++;
                builder.AppendFormat("{0} {1}, pr: {2}, minInt {3}{4}", "\n", topicType.tag, topicType.priority.name, topicType.minInterval, "\n");
                if (topicType.listOfCriteria.Count > 0)
                {
                    foreach (Criteria criteria in topicType.listOfCriteria)
                    {
                        builder.AppendFormat("    \"{0}\", {1}{2}", criteria.name, criteria.description, "\n");
                        counter++;
                    }
                }
                else { builder.AppendFormat("No criteria{0}", "\n"); }
                //loop subTypes
                foreach (TopicSubType subType in topicType.listOfSubTypes)
                {
                    //needs to be the correct side
                    if (subType.side.level == playerSide.level || subType.side.level == 3)
                    {
                        builder.AppendFormat("  -{0}, pr: {1}, minInt {2}{3}", subType.name, subType.priority.name, subType.minInterval, "\n");
                        counter++;
                        if (subType.listOfCriteria.Count > 0)
                        {
                            foreach (Criteria criteria in subType.listOfCriteria)
                            {
                                builder.AppendFormat("    \"{0}\", {1}{2}", criteria.name, criteria.description, "\n");
                                counter++;
                            }
                        }
                    }
                    else { builder.AppendFormat("  -{0} -> Not Valid for this Side{1}", subType.name, "\n"); }
                }
                //limit text onscreen
                if (counter > 50)
                { break; }
            }
        }
        else { Debug.LogError("Invalid listOfTopicTypes (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug display of all topic pools (topics valid for the current level)
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTopicPools()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- dictOfTopicPools{0}", "\n");
        List<TopicType> listOfTopicTypes = GameManager.i.dataScript.GetListOfTopicTypesLevel();
        if (listOfTopicTypes != null)
        {
            //loop topic types by level
            foreach (TopicType topicType in listOfTopicTypes)
            {
                /*builder.AppendFormat("{0} {1}{2}", "\n", topicType.tag, "\n");*/
                builder.AppendLine();
                //loop topicSubTypes
                if (topicType.listOfSubTypes != null)
                {
                    foreach (TopicSubType subType in topicType.listOfSubTypes)
                    {
                        builder.AppendFormat("   {0}{1}", subType.tag, "\n");
                        //find entry in dictOfTopicPools and display all topics for that subType
                        List<Topic> listOfTopics = GameManager.i.dataScript.GetListOfTopics(subType);
                        {
                            if (listOfTopics != null)
                            {
                                if (listOfTopics.Count > 0)
                                {
                                    foreach (Topic topic in listOfTopics)
                                    {
                                        builder.AppendFormat("     {0}, {1} x Op, St: {2}, Pr: {3}, Gr: {4}{5}", topic.name, topic.listOfOptions?.Count, topic.status,
                                          topic.priority.name, topic.group.name, "\n");
                                    }
                                }
                                else { builder.AppendFormat("     none found{0}", "\n"); }
                            }
                            else
                            {
                                /*Debug.LogWarningFormat("Invalid listOfTopics (Null) for topicSubType {0}", subType.name);*/
                                builder.AppendFormat("    none found{0}", "\n");
                            }
                        }
                    }
                }
                else
                {
                    /*Debug.LogWarningFormat("Invalid listOfSubTypes (Null) for topicType \"{0}\"{1}", topicType, "\n");*/
                    builder.AppendFormat("   none found{0}", "\n");
                }
            }
        }
        else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug display of current topic pool
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayCurrentTopicPool()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- dictOfTopicPools{0}", "\n");
        if (turnTopicSubType != null)
        {
            List<Topic> listOfTopics = GameManager.i.dataScript.GetListOfTopics(turnTopicSubType);
            if (listOfTopics != null)
            {

                if (listOfTopics.Count > 0)
                {
                    //current topic subType pool
                    builder.AppendFormat("{0}- {1}{2}", "\n", turnTopicSubType.name, "\n");
                    foreach (Topic topic in listOfTopics)
                    {
                        builder.AppendFormat(" {0}, {1} x Op, St: {2}, Pr: {3}, Gr: {4}{5}", topic.name, topic.listOfOptions?.Count, topic.status,
                          topic.priority.name, topic.group.name, "\n");
                    }
                    //current topic profile data
                    builder.AppendFormat("{0}{1}- Profile data{2}", "\n", "\n", "\n");
                    foreach (Topic topic in listOfTopics)
                    {
                        if (topic.profile != null)
                        {
                            builder.AppendFormat(" {0} -> {1}{2}  ts {3}, tr {4}, tw {5} -> D {6}, A {7}, L {8}, D {9} -> {10}{11}", topic.name, topic.profile.name, "\n", topic.timerStart,
                                topic.timerRepeat, topic.timerWindow, topic.turnsDormant, topic.turnsActive, topic.turnsLive, topic.turnsDone, topic.isCurrent ? "true" : "FALSE", "\n");
                        }
                    }
                }
                else { builder.AppendFormat("     none found{0}", "\n"); }
            }
            else { builder.AppendFormat("{0} Error with turnTopicSubType", "\n"); }
        }
        else { builder.AppendFormat("{0} Invalid turnTopicSubType (NULL)", "\n"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug display of selection pools and data
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTopicSelectionData()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- Topic Selection{0}{1}", "\n", "\n");
        builder.AppendFormat(" topicGlobal: {0}{1}", topicGlobal, "\n");
        builder.AppendFormat(" reviewCountdown: {0} (interval period {1}){2}", reviewCountdown, reviewPeriod, "\n");
        builder.AppendFormat(" reviewActivationMiss: {0}{1}{2}", reviewActivationMiss, "\n", "\n");
        builder.AppendFormat(" minIntervalGlobal: {0}{1}", minIntervalGlobal, "\n");
        builder.AppendFormat(" minIntervalGlobalActual: {0}{1}{2}", minIntervalGlobalActual, "\n", "\n");
        if (turnTopicType != null)
        { builder.AppendFormat(" topicType: {0}{1}", turnTopicType.name, "\n"); }
        else { builder.AppendFormat(" topicType: NULL{0}", "\n"); }
        if (turnTopicSubType != null)
        { builder.AppendFormat(" topicSubType: {0}{1}", turnTopicSubType.name, "\n"); }
        if (turnTopicSubSubType != null)
        { builder.AppendFormat(" topicSubSubType: {0}{1}", turnTopicSubSubType.name, "\n"); }
        else { builder.AppendFormat(" topicSubType: NULL{0}", "\n"); }
        if (turnTopic != null)
        { builder.AppendFormat(" topic: {0}{1}", turnTopic.name, "\n"); }
        else { builder.AppendFormat(" topic: NULL{0}", "\n"); }
        builder.AppendLine();
        builder.AppendFormat(" actorID: {0}{1}", tagActorID, "\n");
        builder.AppendFormat(" nodeID: {0}{1}", tagNodeID, "\n");
        builder.AppendFormat(" turn: {0}{1}", tagTurn, "\n");
        builder.AppendFormat(" stringData: {0}{1}", tagStringData, "\n");
        //topic type pool
        builder.AppendFormat("{0}- listOfTopicTypePool{1}", "\n", "\n");
        if (listOfTypePool.Count > 0)
        {
            foreach (TopicType topicType in listOfTypePool)
            { builder.AppendFormat(" {0}, priority: {1}{2}", topicType.name, topicType.priority.name, "\n"); }
        }
        else { builder.AppendFormat(" No records{0}", "\n"); }
        //topic sub type pool
        builder.AppendFormat("{0}- listOfTopicSubTypePool{1}", "\n", "\n");
        if (listOfSubTypePool.Count > 0)
        {
            foreach (TopicSubType subType in listOfSubTypePool)
            { builder.AppendFormat(" {0}, proirity: {1}{2}", subType.name, subType.priority.name, "\n"); }
        }
        else { builder.AppendFormat("No records{0}", "\n"); }
        //topic pool
        builder.AppendFormat("{0}- listOfTopicPool{1}", "\n", "\n");
        if (listOfTopicPool.Count > 0)
        {
            foreach (Topic topic in listOfTopicPool)
            { builder.AppendFormat(" {0}, priority: {1}{2}", topic.name, topic.priority.name, "\n"); }
        }
        else { builder.AppendFormat("No records{0}", "\n"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug display of dynamic profile data, eg. timers etc.
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTopicProfileData()
    {
        int counter = 0;
        StringBuilder builder = new StringBuilder();
        Dictionary<string, Topic> dictOfTopics = GameManager.i.dataScript.GetDictOfTopics();
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        if (dictOfTopics != null)
        {
            builder.AppendFormat("- Topic Profile Data{0}{1}", "\n", "\n");
            foreach (var topic in dictOfTopics)
            {
                if (topic.Value.side.level == playerSide.level)
                {
                    if (topic.Value.profile != null)
                    {
                        builder.AppendFormat(" {0} -> {1} -> ts {2}, tr {3}, tw {4} -> D {5}, A {6}, L {7}, D {8} -> {9}{10}", topic.Value.name, topic.Value.profile.name, topic.Value.timerStart,
                            topic.Value.timerRepeat, topic.Value.timerWindow, topic.Value.turnsDormant, topic.Value.turnsActive, topic.Value.turnsLive,
                            topic.Value.turnsDone, topic.Value.isCurrent ? "true" : "FALSE", "\n");
                        counter++;
                    }
                    else { builder.AppendFormat(" {0} -> Invalid Profile (Null){1}", topic.Value.name, "\n"); }
                }
                //limit text on screen
                if (counter > 54)
                { break; }
            }
        }
        else { Debug.LogError("Invalid dictOfTopics (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// returns true if a valid topicType (must pass minInterval and global minInterval checks) -> Used for DebugDisplayTopicTypes to show a '*' or not
    /// </summary>
    /// <param name="topicType"></param>
    /// <returns></returns>
    private bool DebugCheckValidType(TopicTypeData data)
    {
        int turn = GameManager.i.turnScript.Turn;
        //needs to be able to handle topics and subTopics
        string topicName = data.parent;
        if (string.IsNullOrEmpty(topicName) == true)
        { topicName = data.type; }
        //get topictype
        TopicType topicType = GameManager.i.dataScript.GetTopicType(topicName);
        if (topicType != null)
        {
            if (topicType.isDisabled == true)
            { return false; }
            if (data != null)
            {
                if (data.minInterval == 0)
                {
                    //global minInterval applies
                    if (turn - data.turnLastUsed >= minIntervalGlobal)
                    { return true; }
                }
                else
                {
                    //local minInterval applies
                    if (turn - data.turnLastUsed >= data.minInterval)
                    { return true; }
                }
            }
            else { Debug.LogError("Invalid topictypeData (Null)"); }
        }
        else { Debug.LogErrorFormat("Invalid topicType (Null) for TopicTypeData.parent \"{0}\"", topicName); }
        return false;
    }

    /// <summary>
    /// displays all relevant story data (storyAlpha/Bravo/Charlie)
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayStoryData()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- Story Data{0}{1}", "\n", "\n");
        //story modules
        builder.AppendFormat(" Story Modules{0}", "\n");
        builder.AppendFormat(" storyAlpha (Campaign): {0}{1}", storyAlphaPool != null ? storyAlphaPool.tag : "None", "\n");
        builder.AppendFormat(" storyBravo (Family): {0}{1}", storyBravoPool != null ? storyBravoPool.tag : "None", "\n");
        builder.AppendFormat(" storyCharlie (Hq): {0}{1}", storyCharliePool != null ? storyCharliePool.tag : "None", "\n");
        //group flags
        builder.AppendFormat("{0} Group Flags{1}", "\n", "\n");
        builder.AppendFormat(" isStoryAlphaGood: {0}{1}", isStoryAlphaGood, "\n");
        builder.AppendFormat(" isStoryBravoGood: {0}{1}", isStoryBravoGood, "\n");
        builder.AppendFormat(" isStoryCharlieGood: {0}{1}", isStoryCharlieGood, "\n");
        //indexes
        builder.AppendFormat(" {0} Indexes{1}", "\n", "\n");
        builder.AppendFormat(" storyAlphaCurrentLinkedIndex: {0}{1}", storyAlphaCurrentIndex, "\n");
        builder.AppendFormat(" storyBravoCurrentLinkedIndex: {0}{1}", storyBravoCurrentIndex, "\n");
        builder.AppendFormat(" storyCharlieCurrentLinkedIndex: {0}{1}", storyCharlieCurrentIndex, "\n");
        builder.AppendFormat(" storyCurrentLevelIndex: {0}{1}", storyCurrentLevelIndex, "\n");
        //flags
        builder.AppendFormat(" {0} Story Flags{1}", "\n", "\n");
        builder.Append(" Alpha:  ");
        for (int i = 0; i < arrayOfStoryFlags.GetUpperBound(1) + 1; i++)
        { builder.AppendFormat("{0} flag{1} -> {2}", i == 0 ? "" : ",", i, arrayOfStoryFlags[(int)StoryType.Alpha, i]); }
        builder.AppendFormat("{0} Bravo:  ", "\n");
        for (int i = 0; i < arrayOfStoryFlags.GetUpperBound(1) + 1; i++)
        { builder.AppendFormat("{0} flag{1} -> {2}", i == 0 ? "" : ",", i, arrayOfStoryFlags[(int)StoryType.Bravo, i]); }
        builder.AppendFormat("{0} Charlie: ", "\n");
        for (int i = 0; i < arrayOfStoryFlags.GetUpperBound(1) + 1; i++)
        { builder.AppendFormat("{0} flag{1} -> {2}", i == 0 ? "" : ",", i, arrayOfStoryFlags[(int)StoryType.Charlie, i]); }
        //stars
        builder.AppendFormat(" {0}{1} Story Stars{2}", "\n", "\n", "\n");
        builder.Append(" Alpha:  ");
        for (int i = 0; i < arrayOfStoryStars.GetUpperBound(1) + 1; i++)
        { builder.AppendFormat("{0} star{1} -> {2}", i == 0 ? "" : ",", i, arrayOfStoryStars[(int)StoryType.Alpha, i]); }
        builder.AppendFormat("{0} Bravo:  ", "\n");
        for (int i = 0; i < arrayOfStoryStars.GetUpperBound(1) + 1; i++)
        { builder.AppendFormat("{0} star{1} -> {2}", i == 0 ? "" : ",", i, arrayOfStoryStars[(int)StoryType.Bravo, i]); }
        builder.AppendFormat("{0} Charlie: ", "\n");
        for (int i = 0; i < arrayOfStoryStars.GetUpperBound(1) + 1; i++)
        { builder.AppendFormat("{0} star{1} -> {2}", i == 0 ? "" : ",", i, arrayOfStoryStars[(int)StoryType.Charlie, i]); }
        return builder.ToString();
    }

    /// <summary>
    /// displays vital stats of all topics within the three story module pools
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayStoryTopics(StoryType storyType)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- Story Topics - {0} Pool (Campaign){1}{2}", storyType, "\n", "\n");
        switch (storyType)
        {
            case StoryType.Alpha: builder.Append(DebugProcessStoryPool(storyAlphaPool)); break;
            case StoryType.Bravo: builder.Append(DebugProcessStoryPool(storyBravoPool)); break;
            case StoryType.Charlie: builder.Append(DebugProcessStoryPool(storyCharliePool)); break;
            default: Debug.LogWarningFormat("Unrecognised storyType \"{0}\"", storyType); break;
        }
        return builder.ToString();
    }

    /// <summary>
    /// SubMethod for DebugDisplayStoryTopics to process individual topic pools
    /// </summary>
    /// <param name="pool"></param>
    /// <returns></returns>
    private string DebugProcessStoryPool(TopicPool pool)
    {
        int count;
        StringBuilder builder = new StringBuilder();
        count = pool.listOfTopics.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                Topic topic = pool.listOfTopics[i];
                if (topic != null)
                { builder.AppendFormat(" {0}: Link {1}, Lvl {2}, {3}, Current {4}, {5}{6}", topic.name, topic.linkedIndex, topic.levelIndex, topic.status, topic.isCurrent, topic.group.name, "\n"); }
                else { Debug.LogErrorFormat("Invalid topic (Null) in {0}.listOfTopics[{1}]", pool.name, i); }
            }
        }
        else { builder.AppendFormat(" No topics in Pool{0}", "\n"); }
        return builder.ToString();
    }


    #endregion

    //new methods above here
}
