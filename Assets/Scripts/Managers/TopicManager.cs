using gameAPI;
using modalAPI;
using packageAPI;
using System;
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
    [Header("Main")]
    [Tooltip("Minimum number of turns before topicType/SubTypes can be chosen again")]
    [Range(0, 10)] public int minIntervalGlobal = 2;
    [Tooltip("Maximum number of options in a topic")]
    [Range(2, 4)] public int maxOptions = 4;

    [Header("Probability")]
    [Tooltip("Number (less than) to roll for an Extreme probability option to Succeed")]
    [Range(0, 100)] public int chanceExtreme = 90;
    [Tooltip("Number (less than) to roll for a High probability option to Succeed")]
    [Range(0, 100)] public int chanceHigh = 75;
    [Tooltip("Number (less than) to roll for a Medium probability option to Succeed")]
    [Range(0, 100)] public int chanceMedium = 50;
    [Tooltip("Number (less than) to roll for a Low probability option to Succeed")]
    [Range(0, 100)] public int chanceLow = 25;
    [Tooltip("If Motivation is neutral (2) then there is this % chance of the topic being good and the balance for it being bad")]
    [Range(0, 100)] public int chanceNeutralGood = 70;

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

    [Header("TopicTypes (with subSubTypes)")]
    [Tooltip("Used to avoid having to hard code the TopicType.SO names")]
    public TopicType actorType;
    [Tooltip("Used to avoid having to hard code the TopicType.SO names")]
    public TopicType playerType;
    [Tooltip("Used to avoid having to hard code the TopicType.SO names")]
    public TopicType authorityType;

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

    //debugging (testManager.cs)
    private TopicPool debugTopicPool;

    //info tags (topic specific info) -> reset to defaults each turn in ResetTopicAdmin prior to use
    private int tagActorID;
    private int tagNodeID;
    private int tagTeamID;              //used for authority team actions
    private int tagContactID;
    private int tagTurn;
    private string tagJob;
    private string tagLocation;
    private string tagGear;
    private string tagRecruit;
    private string tagSecret;              //name of secret (not tag)
    private string tagTeam;                //used for resistance team actions
    private string tagTarget;
    private string tagStringData;        //General purpose
    private int[] arrayOfOptionActorIDs;     //actorID's corresponding to option choices (0 -> 3) for topics where you have a choice of actors, eg. Player General

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


    /// <summary>
    /// Initialisation
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess(); //needs to be first
                SubInitialiseStartUp();
                SubInitialiseLevelStart();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseLevelStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseStartUp();
                SubInitialiseEvents();
                break;
            case GameState.LoadGame:
                //do nothing
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\" (Initialise)", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialisation SubMethods

    #region SubInitialiseStartUp
    private void SubInitialiseStartUp()
    {
        //debug topic pool
        debugTopicPool = GameManager.instance.testScript.debugTopicPool;
        //initialise Topic Profile delays
        TopicProfile[] arrayOfProfiles = GameManager.instance.loadScript.arrayOfTopicProfiles;
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
        //calculates topicType/SubType minimum Intervals based on global setting (minTopicTypeTurns)
        List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypes();
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
                    TopicTypeData dataType = GameManager.instance.dataScript.GetTopicTypeData(topicType.name);
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
                                TopicTypeData dataSub = GameManager.instance.dataScript.GetTopicSubTypeData(subType.name);
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
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        //reset all topics to isCurrent False prior to changes
        GameManager.instance.dataScript.ResetTopics();
        //establish which TopicTypes are valid for the level. Initialise profile and status data.
        UpdateTopicPools();
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //text lists
        Debug.Assert(textlistGenericLocation != null, "Invalid textListGenericLocations (Null)");
        Debug.Assert(textListBadResistance != null, "Invalid textListBadResistance (Null)");
        Debug.Assert(textListGoodResistance != null, "Invalid textListGoodResistance (Null)");
        Debug.Assert(textListNpcSomething != null, "Invalid textListNpcSomething (Null)");
        Debug.Assert(textListMoneyReason != null, "Invalid textListMoneyReason (Null)");
        Debug.Assert(textListPolicy != null, "Invalid textListPolicy (Null)");
        Debug.Assert(textListHandicap != null, "Invalid textListHandicap (Null)");
        //types
        Debug.Assert(actorType != null, "Invalid actorType (Null)");
        Debug.Assert(playerType != null, "Invalid playerType (Null)");
        Debug.Assert(authorityType != null, "Invalid authorityType (Null)");
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
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "TopicManager");
    }
    #endregion

    #endregion

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
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }



    #region SetColours
    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        /*colourDefault = GameManager.instance.colourScript.GetColour(ColourType.whiteText);*/
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourCancel = GameManager.instance.colourScript.GetColour(ColourType.moccasinText);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }
    #endregion

    #region Session Start
    //
    // - - - Session Start - - -
    //

    #region UpdateTopicPools
    /// <summary>
    /// Populates DataManager.cs -> dictOfTopicPools (at start of a level) with all valid topics and sets up listOfTopicTypesLevel for all topics that are valid for the current level
    /// </summary>
    public void UpdateTopicPools()
    {
        List<TopicType> listOfTopicTypesLevel = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
        if (listOfTopicTypesLevel != null)
        {
            //get current topicTypes
            List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypes();
            if (listOfTopicTypes != null)
            {
                //get current campaign
                Campaign campaign = GameManager.instance.campaignScript.campaign;
                if (campaign != null)
                {
                    //get current city
                    City city = GameManager.instance.campaignScript.scenario.city;
                    if (city != null)
                    {
                        string subTypeName;
                        //loop by subTypes
                        for (int i = 0; i < listOfTopicTypes.Count; i++)
                        {
                            TopicType topicType = listOfTopicTypes[i];
                            if (topicType != null)
                            {
                                GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
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
                                                        if (campaign.actorPoliticPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.actorPoliticPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.actorPoliticPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorPoliticPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorPoliticPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorContact":
                                                        if (campaign.actorContactPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.actorContactPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.actorContactPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorContactPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorContactPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorDistrict":
                                                        if (campaign.actorDistrictPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.actorDistrictPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.actorDistrictPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorDistrictPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorDistrictPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorGear":
                                                        if (campaign.actorGearPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.actorGearPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.actorGearPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorGearPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorGearPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorMatch":
                                                        if (campaign.actorMatchPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.actorMatchPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.actorMatchPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorMatchPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorMatchPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "PlayerDistrict":
                                                        if (campaign.playerDistrictPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.playerDistrictPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.playerDistrictPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.playerDistrictPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.playerDistrictPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "PlayerGeneral":
                                                        if (campaign.playerGeneralPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.playerGeneralPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.playerGeneralPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.playerGeneralPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.playerGeneralPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "AuthorityCampaign":
                                                        if (campaign.authorityCampaignPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.authorityCampaignPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.authorityCampaignPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.authorityCampaignPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.authorityCampaignPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "AuthorityGeneral":
                                                        if (campaign.authorityGeneralPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.authorityGeneralPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.authorityGeneralPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.authorityGeneralPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.authorityGeneralPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "AuthorityTeam":
                                                        if (campaign.teamPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.teamPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.teamPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.teamPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.teamPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ResistanceCampaign":
                                                        if (campaign.resistanceCampaignPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.resistanceCampaignPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.resistanceCampaignPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.resistanceCampaignPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.resistanceCampaignPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ResistanceGeneral":
                                                        if (campaign.resistanceGeneralPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.resistanceGeneralPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.resistanceGeneralPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.resistanceGeneralPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.resistanceGeneralPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "CampaignAlpha":
                                                        if (campaign.campaignAlphaPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignAlphaPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignAlphaPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignAlphaPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignAlphaPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "CampaignBravo":
                                                        if (campaign.campaignBravoPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignBravoPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignBravoPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignBravoPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignBravoPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "CampaignCharlie":
                                                        if (campaign.campaignCharliePool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignCharliePool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignCharliePool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignCharliePool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignCharliePool.listOfTopics);
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
                                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, city.cityPoolAuthority.listOfTopics);
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
                                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, city.cityPoolResistance.listOfTopics);
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
                                                    case "FamilyAlpha":
                                                        if (campaign.familyAlphaPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.familyAlphaPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.familyAlphaPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyAlphaPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.familyAlphaPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "FamilyBravo":
                                                        if (campaign.familyBravoPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.familyBravoPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.familyBravoPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyBravoPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.familyBravoPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "FamilyCharlie":
                                                        if (campaign.familyCharliePool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.familyCharliePool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.familyCharliePool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyCharliePool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.familyCharliePool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "HQSub":
                                                        if (campaign.hqPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.hqPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.hqPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.hqPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.hqPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    default:
                                                        Debug.LogWarningFormat("Unrecognised topicSubType \"{0}\" for topicType \"{1}\"", topicSubType.name, topicType.name);
                                                        break;
                                                }
                                                //set subType topicTypeData 'isAvailable' to appropriate value
                                                TopicTypeData data = GameManager.instance.dataScript.GetTopicSubTypeData(topicSubType.name);
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
        bool isFirstScenario = GameManager.instance.campaignScript.CheckIsFirstScenario();
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
                        else if (topic.subType.scope.name.Equals(campaignScopeName, StringComparison.Ordinal) == true)
                        {
                            //LINKED -> need to initialise linked sequence and have first pair status started normally 
                            if (isFirstScenario == true)
                            {
                                if (topic.linkedIndex == 0)
                                {
                                    if (topic.timerStart == 0)
                                    { topic.status = Status.Active; }
                                    else { topic.status = Status.Dormant; }
                                    //isCurrent (all topics set to false prior to changes by SubInitialiseLevelStart
                                    topic.isCurrent = true;
                                }
                                else
                                {
                                    //set all none start campaign topics to 'Done' (but only right at the start as save/load game data will take over from there)
                                    topic.status = Status.Done;
                                    topic.isCurrent = false;
                                }
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

    #endregion

    #region Select Topic
    //
    //  - - - Select Topic - - -
    //   

    #region SelectTopic method
    /// <summary>
    /// Selects a topic for the turn (there will always be one)
    /// </summary>
    public void SelectTopic(GlobalSide playerSide)
    {
        if (playerSide != null)
        {
            //Player must be Active
            if (CheckPlayerStatus(playerSide) == true)
            {
                CheckTopics();
                //select a topic, if none found then drop the global interval by 1 and try again
                minIntervalGlobalActual = minIntervalGlobal;
                //initialise listOfTopicTypesTurn prior to selection loop
                CheckForValidTopicTypes();
                Debug.LogFormat("[Tst] TopicManager.cs -> SelectTopic: listOfTopicTypeTurn has {0} records{1}", listOfTopicTypesTurn.Count, "\n");
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
                        Debug.LogFormat("[Tst] TopicManager.cs -> SelectTopic: REPEAT LOOP, minIntervalGlobalActual now {0}{1}", minIntervalGlobalActual, "\n");
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
        else { Debug.LogError("Invalid playerSide (Null)"); }
    }
    #endregion

    #region archive Old Code
    /*public void SelectTopic(GlobalSide playerSide)
    {
        if (playerSide != null)
        {
            //Player must be Active
            if (CheckPlayerStatus(playerSide) == true)
            {
                CheckTopics();
                //select a topic, if none found then drop the global interval by 1 and try again
                minIntervalGlobalActual = minIntervalGlobal;
                do
                {
                    //reset needs to be inside the loop
                    ResetTopicAdmin();
                    CheckForValidTopicTypes();
                    if (GetTopicType() == true)
                    {
                        if (GetTopicSubType(playerSide) == true)
                        { GetTopic(playerSide); }
                    }
                    //repeat process with a reduced minInterval
                    if (turnTopic == null)
                    {
                        minIntervalGlobalActual--;
                        Debug.LogFormat("[Tst] TopicManager.cs -> SelectTopic: REPEAT LOOP, minIntervalGlobalActual now {0}{1}", minIntervalGlobalActual, "\n");
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
        else { Debug.LogError("Invalid playerSide (Null)"); }
    }*/
    #endregion

    #region CheckTopics
    /// <summary>
    /// checks all current topic timers and criteria and adjusts status if required
    /// </summary>
    private void CheckTopics()
    {
        Dictionary<string, Topic> dictOfTopics = GameManager.instance.dataScript.GetDictOfTopics();
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

    #region CheckForValidTopicType
    /// <summary>
    /// Check all level topic types to see if they are valid for the Turn. Updates listOfTopicTypesTurn with valid topicTypes
    /// TopicType.side already taken into account with listOfTopicTypesLevel
    /// </summary>
    private void CheckForValidTopicTypes()
    {
        //clear list at start of selection (not done in ResetTopicAdmin as it should only be done once at start, not every iteration inside the selection loop
        listOfTopicTypesTurn.Clear();
        //by value, not reference, as you'll be passing them onto listOfTopicTypesTurn and removing occasionally
        List<TopicType> listOfTopicTypesLevel = new List<TopicType>(GameManager.instance.dataScript.GetListOfTopicTypesLevel());
        if (listOfTopicTypesLevel != null)
        {
            string criteriaCheck;
            int turn = GameManager.instance.turnScript.Turn;
            //loop list of Topic Types
            foreach (TopicType topicType in listOfTopicTypesLevel)
            {
                TopicTypeData topicTypeData = GameManager.instance.dataScript.GetTopicTypeData(topicType.name);
                if (topicTypeData != null)
                {
                    //check topicTypeData
                    if (CheckTopicTypeData(topicTypeData, turn) == true)
                    {
                        //check individual topicType criteria
                        CriteriaDataInput criteriaInput = new CriteriaDataInput()
                        { listOfCriteria = topicType.listOfCriteria };
                        criteriaCheck = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
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
                                    if (CheckSubTypeCriteria(subType) == true)
                                    {
                                        isProceed = true;
                                        break;
                                    }
                                }
                                //valid topicType / subType, add to selection pool
                                if (isProceed == true)
                                {

                                    /*listOfTopicTypesTurn.Add(topicType);*/

                                    //criteria check passed O.K, add to local list of valid TopicTypes for the Turn
                                    AddTopicTypeToList(listOfTopicTypesTurn, topicType);
                                    Debug.LogFormat("[Tst] TopicManager.cs -> CheckForValidTopics: topicType \"{0}\" PASSED TopicTypeData check{1}", topicType.name, "\n");
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid listOfSubTypes (Null) for topicType \"{0}\"", topicType.name); }

                        }
                        else
                        {
                            //criteria check FAILED

                            //generate message explaining why criteria failed -> debug only, spam otherwise
                            Debug.LogFormat("[Tst] TopicManager.cs -> CheckForValidTopics: topicType \"{0}\" {1} Criteria check{2}", topicType.tag, criteriaCheck, "\n");
                        }
                    }
                    else { Debug.LogFormat("[Tst] TopicManager.cs -> CheckForValidTopics: topicType \"{0}\" Failed TopicTypeData check{1}", topicType.tag, "\n"); }
                }
                else { Debug.LogError("Invalid topicTypeData (Null)"); }
            }
        }
        else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
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
        tagNodeID = -1;
        tagTeamID = -1;
        tagContactID = -1;
        tagTurn = -1;
        tagJob = "";
        tagLocation = "";
        tagGear = "";
        tagRecruit = "";
        tagSecret = "";
        tagTarget = "";
        tagTeam = "";
        tagStringData = "";
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
                Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicType: SELECTED turnTopicType \"{0}\"", turnTopicType.name);
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
                    //check for same side as Player or 'both', ignore otherwise
                    if (subType.side.level == playerSide.level || subType.side.level == 3)
                    {
                        //check TopicSubTypes isValid (topicType may have been approved with some subTypes O.K and others not)
                        TopicTypeData dataSub = GameManager.instance.dataScript.GetTopicSubTypeData(subType.name);
                        if (dataSub != null)
                        {
                            //subType CRITERIA check
                            if (CheckSubTypeCriteria(subType) == true)
                            {
                                //reset 'isProceed' ready for data checks
                                isProceed = false;
                                if (CheckTopicTypeData(dataSub, GameManager.instance.turnScript.Turn, true) == true)
                                {
                                    //check that there are Live topics available for subType
                                    List<Topic> listOfTopics = GameManager.instance.dataScript.GetListOfTopics(subType);
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
                else { Debug.LogWarningFormat("Invalid subType (Null) for topicType \"{0}\"", turnTopicType.name); }
            }
            //valid records in selection pool?
            if (listOfSubTypePool.Count > 0)
            {
                //random draw of pool
                turnTopicSubType = listOfSubTypePool[Random.Range(0, listOfSubTypePool.Count)];
                Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicSubType: SELECTED turnTopicSubType \"{0}\"", turnTopicSubType.name);
                return true;
            }
            else
            {
                Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicSubType: \"{0}\" Empty Pool for topicSubType selection{1}", turnTopicType.name, "\n");
                //remove topicType from listOfTopicTypesTurn (selection list) to prevent it being selected again given that there are no valid topics anywhere within that topicType
                if (minIntervalGlobalActual > 1)
                {
                    int index = listOfTopicTypesTurn.FindIndex(x => x.name.Equals(turnTopicType.name, StringComparison.Ordinal) == true);
                    if (index > -1)
                    {
                        Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicSubType: topicType \"{0}\" REMOVED from listOfTopicTypesTurn{1}", turnTopicType.name, "\n");
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
            List<Topic> listOfSubTypeTopics = GameManager.instance.dataScript.GetListOfTopics(turnTopicSubType);
            if (listOfSubTypeTopics != null)
            {
                GroupType group;
                switch (turnTopicSubType.name)
                {
                    //Standard topics
                    case "CampaignAlpha":
                    case "CampaignBravo":
                    case "CampaignCharlie":
                    case "AuthorityCampaign":
                    case "AuthorityGeneral":
                    case "ResistanceCampaign":
                    case "ResistanceGeneral":
                    case "HQSub":
                        //based on faction Approval
                        group = GetGroupApproval(GameManager.instance.factionScript.ApprovalResistance);
                        listOfPotentialTopics = GetTopicGroup(listOfSubTypeTopics, group, turnTopicSubType.name);
                        break;
                    case "FamilyAlpha":
                    case "FamilyBravo":
                    case "FamilyCharlie":
                        //based on PlayerMood
                        group = GetGroupMood(GameManager.instance.playerScript.GetMood());
                        listOfPotentialTopics = GetTopicGroup(listOfSubTypeTopics, group, turnTopicSubType.name);
                        break;
                    //Dynamic topic
                    case "CitySub":
                        //based on City Loyalty
                        group = GetGroupLoyalty(GameManager.instance.factionScript.ApprovalResistance);
                        listOfPotentialTopics = GetTopicGroup(listOfSubTypeTopics, group, turnTopicSubType.name);
                        break;
                    case "AuthorityTeam":
                        //base on actor Motivation
                        listOfPotentialTopics = GetAuthorityTeamTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorGear":
                        //based on actor Motivation
                        listOfPotentialTopics = GetActorGearTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorPolitic":
                        //based on actor Motivation
                        listOfPotentialTopics = GetActorPoliticTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorContact":
                        //based on actor Motivation
                        listOfPotentialTopics = GetActorContactTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorMatch":
                        //based on actor Compatibility
                        listOfPotentialTopics = GetActorMatchTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorDistrict":
                        //based on actor Motivation
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
                    default:
                        Debug.LogWarningFormat("Unrecognised topicSubType \"{0}\" for topic \"{1}\"", turnTopicSubType.name, turnTopic.name);
                        break;
                }

                //SubType should have provided a list of topics ready for a random draw
                if (listOfPotentialTopics != null)
                {
                    Debug.LogFormat("[Tst] TopicManager.cs -> GetTopic: turnTopicSubType \"{0}\", listOfPotential topics {1} record{2}", turnTopicSubType.name, listOfPotentialTopics.Count, "\n");
                    int count = listOfPotentialTopics.Count;
                    //shouldn't be empty
                    if (count > 0)
                    {
                        //loop topics
                        for (int i = 0; i < count; i++)
                        {
                            Topic topic = listOfPotentialTopics[i];
                            if (topic != null)
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
                            }
                            else { Debug.LogWarningFormat("Invalid topic (Null) for {0}.listOfTopics[{1}]", turnTopicSubType.name, i); }
                        }
                        //Select topic
                        count = listOfTopicPool.Count;
                        if (count > 0)
                        {
                            turnTopic = listOfTopicPool[Random.Range(0, count)];
                            Debug.LogFormat("[Top] TopicManager.cs -> GetTopic: t {0}, {1} -> {2} -> {3}{4}", GameManager.instance.turnScript.Turn, turnTopicType.name, turnTopicSubType.name, turnTopic.name, "\n");
                        }
                        else { Debug.LogWarningFormat("Invalid listOfTopicPool (EMPTY) for topicSubType \"{0}\"", turnTopicSubType.name); }
                    }
                    else { Debug.LogWarningFormat("Invalid listOfTopics (EMPTY) for topicSubType \"{0}\"", turnTopicSubType.name); }
                }
                else { Debug.LogErrorFormat("Invalid listOfTopics (Null) for topicSubType \"{0}\"", turnTopicSubType.name); }
            }
            else { Debug.LogErrorFormat("Invalid listOfSubTypeTopics (Null) for topicSubType \"{0}\"", turnTopicSubType.name); }
        }
        else
        {
            //No topic selected
            Debug.LogFormat("[Top] TopicManager.cs -> GetTopic: t {0}, No Topic Selected{1}", GameManager.instance.turnScript.Turn, "\n");
        }
    }
    #endregion

    #endregion

    #region Dynamic Topics
    //
    // - - - Select Dynamic Topics
    //

    #region GetActorContactTopics
    /// <summary>
    /// subType ActorContact template topics selected by actor / motivation (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <returns></returns>
    private List<Topic> GetActorContactTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //get all active, onMap actors
        List<Actor> listOfActors = GameManager.instance.dataScript.GetActiveActors(playerSide);
        if (listOfActors != null)
        {
            if (listOfActors.Count > 0)
            {
                //select an actor
                Actor actor = listOfActors[Random.Range(0, listOfActors.Count)];
                if (actor != null)
                {
                    //get actor motivation
                    group = GetGroupMotivation(actor.GetDatapoint(ActorDatapoint.Motivation1));
                    //if no entries use entire list by default
                    listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
                    //Info tags
                    tagActorID = actor.actorID;
                }
                else { Debug.LogWarning("Invalid actor (Null) randomly selected from listOfActors for actorContact subType"); }
            }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null) for ActorContact subType"); }
        //debug
        foreach (Topic topic in listOfTopics)
        { Debug.LogFormat("[Tst] TopicManager.cs -> GetActorContactTopics: \"{0}\"{1}", topic.name, "\n"); }
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
        group = GetGroupMood(GameManager.instance.playerScript.GetMood());
        //if no entries use entire list by default
        listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
        //get a candidate actor (may change later in ProcessSpecialTopicData) -> aim for actor with motivation that coresponds to group
        List<Actor> listOfActors = GameManager.instance.dataScript.GetActiveActors(playerSide);
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
                            //actors with motivation 2+
                            if (actor.GetDatapoint(ActorDatapoint.Motivation1) >= 2)
                            { selectionList.Add(actor); }
                            break;
                        case GroupType.Bad:
                            //actors with motivation 2-
                            if (actor.GetDatapoint(ActorDatapoint.Motivation1) <= 2)
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
            }
            else { Debug.LogWarning("Invalid selectionList (Empty)"); }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null)"); }


        /*//get all active, onMap actors
        List<Actor> listOfActors = GameManager.instance.dataScript.GetActiveActors(playerSide);
        if (listOfActors != null)
        {
            List<Actor> selectionList = new List<Actor>();
            int compatibility;
            foreach (Actor actor in listOfActors)
            {
                if (actor != null)
                {
                    //filter actors for compatibility != 0
                    compatibility = actor.GetPersonality().GetCompatibilityWithPlayer();
                    if (compatibility != 0)
                    {
                        //seed actor selection pool based on ABS(compatility), eg. 2 entries for compatibility +/-2 and 1 for compability +/-1
                        for (int i = 0; i < Mathf.Abs(compatibility); i++)
                        { selectionList.Add(actor); }
                    }
                }
                else { Debug.LogWarning("Invalid Actor (Null) in listOfActors"); }
            }
            //check at least one entry in selectionList
            if (selectionList.Count > 0)
            {
                //randomly select an actor from compatibility weighted list
                Actor actor = selectionList[Random.Range(0, selectionList.Count)];
                group = GetGroupCompatibility(actor.GetPersonality().GetCompatibilityWithPlayer());
                //if no entries use entire list by default
                listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
                //Debug
                if (listOfTopics.Count == 0)
                {
                    Debug.LogWarning("ActorMatch count 0");
                    Debug.LogFormat("[Tst] TopicManager.cs -> GetActorMatchTopic: listOfSubTopics.Count {0}, group {1}{2}", listOfSubTypeTopics.Count, group, "\n");
                    foreach (Topic topic in listOfSubTypeTopics)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> GetActorMatchTopic: topic \"{0}\", status {1}{2}", topic.name, topic.status, "\n"); }
                }
                //Info tags
                tagActorID = actor.actorID;
            }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null) for ActorMatch subType"); }*/

        return listOfTopics;
    }
    #endregion

    #region GetActorPoliticTopics
    /// <summary>
    /// subType ActorPolitic template topics selected by random actor based on motivation (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <returns></returns>
    private List<Topic> GetActorPoliticTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //get all active, onMap actors
        List<Actor> listOfActors = GameManager.instance.dataScript.GetActiveActors(playerSide);
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
                    //seed selection pool by motivation (the further off neutral their motivation, the more entries they get)
                    switch (actor.GetDatapoint(ActorDatapoint.Motivation1))
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
                else { Debug.LogWarning("Invalid Actor (Null) in listOfActors"); }
            }
            //check at least two actors present (need one actor interacting with another)
            if (selectionList.Count > 0 && numOfActors > 1)
            {
                //randomly select an actor from unweighted list
                Actor actor = selectionList[Random.Range(0, selectionList.Count)];
                group = GetGroupMotivation(actor.GetDatapoint(ActorDatapoint.Motivation1));
                //if no entries use entire list by default
                listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
                //Info tags
                tagActorID = actor.actorID;
            }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null) for ActorPolitic subType"); }
        return listOfTopics;
    }
    #endregion

    #region GetActorGearTopics
    /// <summary>
    /// subType ActorGear template topics selected by random actor based on motivation (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <returns></returns>
    private List<Topic> GetActorGearTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //get all active, onMap actors
        List<Actor> listOfActors = GameManager.instance.dataScript.GetActiveActors(playerSide);
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
                    //seed selection pool by motivation (the further off neutral their motivation, the more entries they get)
                    switch (actor.GetDatapoint(ActorDatapoint.Motivation1))
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
                else { Debug.LogWarning("Invalid Actor (Null) in listOfActors"); }
            }
            //check at least one actors present
            if (selectionList.Count > 0 && numOfActors > 0)
            {
                //randomly select an actor from unweighted list
                Actor actor = selectionList[Random.Range(0, selectionList.Count)];
                group = GetGroupMotivation(actor.GetDatapoint(ActorDatapoint.Motivation1));
                //if no entries use entire list by default
                listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
                //Info tags
                tagActorID = actor.actorID;
            }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null) for ActorGear subType"); }
        return listOfTopics;
    }
    #endregion

    #region GetActorDistrictTopics
    /// <summary>
    /// subType ActorDistrict template topics selected by random actor based on motivation (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
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
        List<Actor> listOfActors = GameManager.instance.dataScript.GetActiveActorsSpecial(ActorCheck.NodeActionsNOTZero, playerSide);
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
                            Debug.LogFormat("[Tst] TopicManager.cs -> GetActorDistrictTopics: {0}, {1}, actorID {2} has a valid SubSubType \"{3}\"{4}", actorTemp.actorName, actorTemp.arc.name,
                                actorTemp.actorID, turnTopicSubSubType.name, "\n");
                        }
                        else
                        {
                            Debug.LogFormat("[Tst] TopicManager.cs -> GetActorDistrictTopics: {0}, {1}, actorID {2} has an INVALID SubSubType \"{3}\"{4}", actorTemp.actorName, actorTemp.arc.name,
                                actorTemp.actorID, turnTopicSubSubType.name, "\n");
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
                    //group depends on actor motivation
                    group = GetGroupMotivation(actor.GetDatapoint(ActorDatapoint.Motivation1));
                    //if no entries use entire list by default
                    listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName, turnTopicSubSubType.name);
                    //debug
                    foreach (Topic topic in listOfTopics)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> GetActorDistrictTopic: listOfTopics -> {0}, turn {1}{2}", topic.name, GameManager.instance.turnScript.Turn, "\n"); }
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
            else { Debug.LogFormat("[Tst] TopicManager.cs -> GetActorDistrictTopics: No topics found for ActorDistrict actions for turn {0}{1}", GameManager.instance.turnScript.Turn, "\n"); }
        }
        else { Debug.LogWarning("No active, onMap actors present with at least one NodeAction"); }
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
        string playerName = GameManager.instance.playerScript.PlayerName;
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //get the most recent Player node action
        NodeActionData data = GameManager.instance.playerScript.GetMostRecentNodeAction();
        if (data != null)
        {
            //check that it is a viable subSubType group
            turnTopicSubSubType = GetTopicSubSubType(data.nodeAction);
            if (turnTopicSubSubType != null)
            {
                //check topics of this subSubType present
                if (CheckSubSubTypeTopicsPresent(listOfSubTypeTopics, turnTopicSubSubType.name) == true)
                { isProceed = true; }
                else { Debug.LogFormat("[Tst] TopicManager.cs -> GetPlayerDistrictTopics: Player has an INVALID ActionTopic \"{0}\"{1}", turnTopicSubSubType.name, "\n"); }
            }
            else { Debug.LogErrorFormat("invalid TopicSubSubType (Null) for Player nodeAction \"{0}\"", data.nodeAction); }
            if (isProceed == true)
            {
                //group depends on player mood
                group = GetGroupMood(GameManager.instance.playerScript.GetMood());
                //if no entries use entire list by default
                listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName, turnTopicSubSubType.name);

                /*//debug
                foreach (Topic topic in listOfTopics)
                { Debug.LogFormat("[Tst] TopicManager.cs -> GetPlayerDistrictTopic: listOfTopics -> {0}, turn {1}{2}", topic.name, GameManager.instance.turnScript.Turn, "\n"); }*/

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
        { arrayOfOptionActorIDs[i] = -1; }
        //All topics based on current actor line up
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(playerSide);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, playerSide) == true)
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
                    }
                }
            }
            //assign a random node (used only for news text tags)
            Node node = GameManager.instance.dataScript.GetRandomNode();
            if (node != null)
            { tagNodeID = node.nodeID; }
            else { Debug.LogError("Invalid random node (Null)"); }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
        //group based on Player Mood
        group = GetGroupMood(GameManager.instance.playerScript.GetMood());
        //if no entries use entire list by default
        listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
        return listOfTopics;
    }
    #endregion

    #region GetAuthorityTeamTopics
    /// <summary>
    /// subType ActorDistrict template topics selected by random actor based on motivation (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
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
        List<Actor> listOfActors = GameManager.instance.dataScript.GetActiveActorsSpecial(ActorCheck.TeamActionsNOTZero, playerSide);
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
                            Debug.LogFormat("[Tst] TopicManager.cs -> GetAuthorityTeamTopics: {0}, {1}, actorID {2} has a valid SubSubType \"{3}\"{4}", actorTemp.actorName, actorTemp.arc.name,
                                actorTemp.actorID, turnTopicSubSubType.name, "\n");
                        }
                        else
                        {
                            Debug.LogFormat("[Tst] TopicManager.cs -> GetAuthorityTeamTopics: {0}, {1}, actorID {2} has an INVALID SubSubType \"{3}\"{4}", actorTemp.actorName, actorTemp.arc.name,
                                actorTemp.actorID, turnTopicSubSubType.name, "\n");
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
                    //group depends on actor motivation
                    group = GetGroupMotivation(actor.GetDatapoint(ActorDatapoint.Motivation1));
                    //if no entries use entire list by default
                    listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName, turnTopicSubSubType.name);
                    //debug
                    foreach (Topic topic in listOfTopics)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> GetAuthorityTeamTopic: listOfTopics -> {0}, turn {1}{2}", topic.name, GameManager.instance.turnScript.Turn, "\n"); }
                }
                else { Debug.LogErrorFormat("Invalid teamActionData (Null) for {0}, {1}, actorID {2}", actor.actorName, actor.arc.name, actor.actorID); }
                //Info tags
                tagActorID = actor.actorID;
                tagNodeID = data.nodeID;
                tagTeamID = data.teamID;
                tagTurn = data.turn;
                tagStringData = data.dataName;
            }
            else { Debug.LogFormat("[Tst] TopicManager.cs -> GetAuthorityTeamTopics: No topics found for Actor Team actions for turn {0}{1}", GameManager.instance.turnScript.Turn, "\n"); }
        }
        else { Debug.LogWarning("No active, onMap actors present with at least one TeamAction"); }
        return listOfTopics;
    }
    #endregion

    #endregion

    #region ProcessTopic
    //
    // - - - Process Topic - - - 
    //

    #region ProcessTopic method
    /// <summary>
    /// Process selected topic (decision or Information)
    /// </summary>
    public void ProcessTopic(GlobalSide playerSide)
    {
        if (playerSide != null)
        {
            //only if Player Active
            if (CheckPlayerStatus(playerSide) == true)
            {
                //valid topic selected, ignore otherwise
                if (turnTopic != null)
                {
                    if (GameManager.instance.turnScript.CheckIsAutoRun() == false)
                    {
                        //prepare and send data to topicUI.cs
                        InitialiseTopicUI();
                    }
                    else
                    {
                        //autorun -> randomly choose topic option (effects not implemented)
                        turnOption = turnTopic.listOfOptions[Random.Range(0, turnTopic.listOfOptions.Count)];
                        ProcessTopicAdmin();
                    }
                }
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
            TopicSubType subTypeNormal = turnTopicSubType; //used for reverting back to normally selected topic
            TopicUIData data = new TopicUIData();
            //special case
            bool isPlayerGeneral = false;
            if (turnTopicSubType.name.Equals("PlayerGeneral", StringComparison.Ordinal) == true)
            { isPlayerGeneral = true; }
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
                        Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseTopicUI: subType \"{0}\" FAILED CRITERIA check{1}", debugTopicPool.subType.name, "\n");
                    }

                    if (isProceed == true)
                    {
                        Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseTopicUI: debugTopicPool IN USE{0}", "\n");
                        //select one of topics from the debug pool at random (enables testing of a small subset of topics)
                        turnTopicSubType = debugTopicPool.subType;
                        if (turnTopicSubType != null)
                        {
                            Debug.LogFormat("[Top] TopicManager.cs -> InitialiseTopicUI: Topic OVERRIDE for debugTopicPool \"{0}\"{1}", debugTopicPool.name, "\n");
                            GetTopic(GameManager.instance.sideScript.PlayerSide);
                        }
                        if (turnTopic != null)
                        {
                            //valid debug topic, align the rest with topic
                            turnTopicType = debugTopicPool.type;
                            turnTopicSubSubType = debugTopicPool.subSubType;
                            Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseTopicUI: VALID debug Topic{0}", "\n");
                        }
                        else
                        {
                            //revert back to normally selected topic
                            turnTopicSubType = subTypeNormal;
                            Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseTopicUI: REVERT{0}", "\n");
                        }
                    }
                }
                else { Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseTopicUI: No LIVE topics found in debugTopicPool{0}", "\n"); }
            }
            Debug.LogFormat("[Tst] TopicManager.cs -> InitialiseTopicUI: turnTopicSubType \"{0}\", turnTopic {1}{2}", turnTopicSubType.name, turnTopic, "\n");
            //use normal or debug topic
            if (turnTopic != null)
            {
                data.topicName = turnTopic.name;
                data.header = turnTopic.tag;
                data.text = turnTopic.text;
                data.isBoss = turnTopic.subType.isBoss;
                data.listOfOptions = turnTopic.listOfOptions;
                data.listOfIgnoreEffects = turnTopic.listOfIgnoreEffects;
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
                    //options
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
                                //check any criteria
                                if (CheckOptionCriteria(option) == true)
                                {
                                    //special case of PlayerGeneral topics (where each option refers to a different OnMap actor)
                                    if (isPlayerGeneral == true)
                                    {
                                        if (option.optionNumber > -1)
                                        { tagActorID = arrayOfOptionActorIDs[option.optionNumber]; }
                                        else { Debug.LogWarningFormat("Invalid option.optionNumber {0} for {1}", option.optionNumber, option.name); }
                                    }
                                    //initialise option tooltips
                                    if (InitialiseOptionTooltip(option) == true)
                                    { isProceed = true; }
                                }
                                else
                                {
                                    //criteria checked failed
                                    isProceed = true;
                                    colourOption = colourGrey;
                                    if (isPlayerGeneral == true)
                                    { tagActorID = -1; }
                                }
                                //colourFormat textToDisplay -> special case first
                                if (isPlayerGeneral == true && tagActorID < 0)
                                { option.textToDisplay = string.Format("{0}{1}{2}", colourOption, "Subordinate unavailable", colourEnd); }
                                else
                                { option.textToDisplay = string.Format("{0}{1}{2}", colourOption, CheckTopicText(option.text, false), colourEnd); }
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
                            GameManager.instance.topicDisplayScript.InitialiseData(data);
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
            //process outcome effects / rolls / messages / etc.
            List<Effect> listOfEffects = new List<Effect>();
            //mood effects always apply
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
                    string text = string.Format("\'{0}\' decision, \'{1}\' option, SUCCEEDS", turnTopic.tag, turnOption.tag);
                    GameManager.instance.messageScript.GeneralRandom(text, "Decision Option", threshold, rnd);
                }
                else
                {
                    //fail -> bad effects apply
                    if (turnOption.listOfBadEffects != null) { listOfEffects.AddRange(turnOption.listOfBadEffects); }
                    builderBottom.AppendFormat("{0}FAILED{1}", colourCancel, colourEnd);
                    //random message
                    string text = string.Format("\'{0}\' decision, \'{1}\' option, FAILS", turnTopic.tag, turnOption.tag);
                    GameManager.instance.messageScript.GeneralRandom(text, "Decision Option", threshold, rnd);
                }
            }
            //check valid effects present
            if (listOfEffects != null)
            {
                //set up
                EffectDataReturn effectReturn = new EffectDataReturn();
                //pass through data package
                EffectDataInput dataInput = new EffectDataInput();
                dataInput.originText = string.Format("\'{0}\', {1}", turnTopic.tag, turnOption.tag);
                dataInput.side = GameManager.instance.sideScript.PlayerSide;
                //use Player node as default placeholder (actual tagNodeID is used)
                Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                //special case of PlayerGeneral topics (where each option refers to a different OnMap actor)
                if (turnTopicSubType.name.Equals("PlayerGeneral", StringComparison.Ordinal) == true)
                {
                    if (turnOption.optionNumber > -1)
                    { tagActorID = arrayOfOptionActorIDs[turnOption.optionNumber]; }
                    else { Debug.LogWarningFormat("Invalid option.optionNumber {0} for {1}", turnOption.optionNumber, turnOption.name); }
                }
                //top text (can handle text tags)
                string optionText = CheckTopicText(turnOption.text, false);
                builderTop.AppendFormat("{0}{1}{2}{3}{4}{5}{6}", colourNormal, turnTopic.tag, colourEnd, "\n", colourAlert, optionText, colourEnd);
                if (listOfEffects.Count > 0)
                {
                    //probability option and only one effect (which would be mood)
                    if (turnOption.chance != null && listOfEffects.Count == 1)
                    { builderBottom.AppendFormat("{0}{1}Nothing happened{2}", "\n", colourGrey, colourEnd); }
                    //loop effects
                    foreach (Effect effect in listOfEffects)
                    {
                        if (node != null)
                        {
                            //process effect
                            effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput);
                            if (effectReturn != null)
                            {
                                //builderBottom
                                if (string.IsNullOrEmpty(effectReturn.bottomText) == false)
                                {
                                    if (builderBottom.Length > 0) { builderBottom.AppendLine(); }
                                    builderBottom.AppendFormat("{0}", effectReturn.bottomText);
                                }
                                //exit effect loop on error
                                if (effectReturn.errorFlag == true) { break; }
                            }
                            else
                            {
                                /*builderTop.AppendLine();
                                builderTop.Append("Error");*/
                                builderBottom.AppendLine();
                                builderBottom.Append("Error");
                                effectReturn.errorFlag = true;
                                break;
                            }
                        }
                        else { Debug.LogWarningFormat("Effect \"{0}\" not processed as invalid Node (Null) for option \"{1}\"", effect.name, turnOption.name); }
                    }
                }
                else { builderBottom.AppendFormat("{0}Nothing happened{1}", colourGrey, colourEnd); }
            }
            else
            { Debug.LogWarningFormat("Invalid listOfEffects (Null) for topic \"{0}\", option {1}", turnTopic.name, turnOption.name); }
            //hq boss's opinion
            Actor actorHQ = GameManager.instance.dataScript.GetHQHierarchyActor(ActorHQ.Boss);
            if (actorHQ != null)
            {
                int opinionChange = GameManager.instance.personScript.UpdateHQOpinion(turnOption.moodEffect.belief, actorHQ, turnOption.isPreferredByHQ);
                if (opinionChange != 0)
                {
                    int bossOpinion = GameManager.instance.factionScript.GetBossOpinion();
                    bossOpinion += opinionChange;
                    GameManager.instance.factionScript.SetBossOpinion(bossOpinion, string.Format("\'{0}\', \'{1}\'", turnTopic.tag, turnOption.tag));
                    builderBottom.AppendLine();
                    if (opinionChange > 0) { builderBottom.AppendFormat("{0}{1}Boss Approves of your decision{2}", "\n", colourGood, colourEnd); }
                    else { builderBottom.AppendFormat("{0}{1}Boss Disapproves of your decision{2}", "\n", colourBad, colourEnd); }
                }
            }
            else { Debug.LogError("Invalid actorHQ (Null) for ActorHQ.Boss"); }
            //news item
            if (string.IsNullOrEmpty(turnOption.news) == false)
            {
                string newsSnippet = CheckTopicText(turnOption.news, false);
                NewsItem item = new NewsItem() { text = newsSnippet };
                GameManager.instance.dataScript.AddNewsItem(item);
                Debug.LogFormat("[Top] {0}{1}", newsSnippet, "\n");
            }
            //outcome dialogue
            SetTopicOutcome(builderTop, builderBottom);
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
            dataInput.originText = string.Format("{0} IGNORE", turnTopic.tag);
            dataInput.side = GameManager.instance.sideScript.PlayerSide;
            //use Player node as default placeholder (actual tagNodeID is used)
            Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
            //top text
            builderTop.AppendFormat("{0}{1}{2}{3}{4}{5}{6}", colourNormal, turnTopic.tag, colourEnd, "\n", colourAlert, "Ignored", colourEnd);
            //loop effects
            foreach (Effect effect in listOfEffects)
            {
                if (node != null)
                {
                    //process effect
                    effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput);
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
        ProcessIgnoreBossOpinion();
        builderBottom.AppendLine();
        builderBottom.AppendFormat("{0}{1}Your Boss Disapproves of your inability to make a decision{2}", "\n", colourBad, colourEnd);
        //stats
        GameManager.instance.dataScript.StatisticIncrement(StatType.TopicsIgnored);
        //outcome dialogue
        SetTopicOutcome(builderTop, builderBottom);
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
        int bossOpinion = GameManager.instance.factionScript.GetBossOpinion();
        bossOpinion += -1;
        GameManager.instance.factionScript.SetBossOpinion(bossOpinion, string.Format("\'{0}\', Ignored", turnTopic.tag));
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
        if (sprite == null) { sprite = GameManager.instance.guiScript.infoSprite; }
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
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
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
                        listOfActors = GameManager.instance.dataScript.GetActiveActorsSpecial(ActorCheck.ActorContactMin, GameManager.instance.sideScript.PlayerSide);
                        //choose actor and update tag data
                        isSuccess = ProcessActorContact(listOfActors);
                        break;
                    case "ContactsActorNOTMax":
                        //need an actor with less than the max number of contacts allowed
                        listOfActors = GameManager.instance.dataScript.GetActiveActorsSpecial(ActorCheck.ActorContactNOTMax, GameManager.instance.sideScript.PlayerSide);
                        //choose actor and update tag data
                        isSuccess = ProcessActorContact(listOfActors);
                        break;
                    case "ActorsKnowSecret":
                        //need an actor with at least one secret who doesn't have blackmailer trait
                        listOfActors = GameManager.instance.dataScript.GetActiveActorsSpecial(ActorCheck.KnowsSecret, GameManager.instance.sideScript.PlayerSide);
                        isSuccess = ProcessActorMatch(listOfActors, criteria.effectCriteria);
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
        if (listOfActors?.Count > 0)
        {
            if (effectCriteria != null)
            {
                switch (effectCriteria.name)
                {
                    case "ActorsKnowSecret":
                        //get random actor and actor secret from list
                        Actor actor = listOfActors[Random.Range(0, listOfActors.Count)];
                        if (actor != null)
                        {
                            tagActorID = actor.actorID;
                            Secret secret = actor.GetSecret();
                            if (secret != null) { tagSecret = secret.name; }
                            else { Debug.LogWarningFormat("Invalid secret (Null) for {0}, {1}, ID {2}", actor.actorName, actor.arc.name, actor.actorID); }
                        }
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
            int turn = GameManager.instance.turnScript.Turn;
            //update TopicType topicTypeData
            TopicTypeData typeData = GameManager.instance.dataScript.GetTopicTypeData(turnTopicType.name);
            if (typeData != null)
            {
                typeData.timesUsedLevel++;
                typeData.turnLastUsed = turn;
            }
            else { Debug.LogErrorFormat("Invalid topicTypeData (Null) for turnTopicType \"{0}\"", turnTopicType.name); }
            //update TopicSubType topicTypeData
            TopicTypeData typeSubData = GameManager.instance.dataScript.GetTopicSubTypeData(turnTopicSubType.name);
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
                        Actor actor = GameManager.instance.dataScript.GetActor(tagActorID);
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
                        GameManager.instance.playerScript.RemoveMostRecentNodeAction();
                    }
                }
                //Authority
                else if (turnTopicType.name.Equals(authorityType.name, StringComparison.Ordinal) == true)
                {
                    //Authority team
                    if (turnTopicSubType.name.Equals(authorityTeamSubType.name, StringComparison.Ordinal) == true)
                    {
                        //get actor and delete most recent TeamAction record
                        Actor actor = GameManager.instance.dataScript.GetActor(tagActorID);
                        if (actor != null)
                        { actor.RemoveMostRecentTeamAction(); }
                        else { Debug.LogErrorFormat("Invalid actor (Null) for tagActorID {0}", tagActorID); }
                    }
                }
            }
            string optionName = "Unknown";
            //log
            if (turnOption != null)
            {
                optionName = turnOption.name;
                Debug.LogFormat("[Top] TopicManager.cs -> UpdateTopicAdmin: {0}, \"{1}\" SELECTED for topic {2}, \"{3}\"{4}", optionName, turnOption.tag, turnTopic.name, turnTopic.tag, "\n");
            }
            else { Debug.LogFormat("[Top] TopicManager.cs -> UpdateTopicAdmin: NO OPTION selected for topic \"{0}\"{1}", turnTopic.name, "\n"); }
            //topicHistory
            HistoryTopic history = new HistoryTopic()
            {
                turn = turn,
                numSelect = listOfTopicTypesTurn.Count,
                topicType = turnTopicType.name,
                topicSubType = turnTopicSubType.name,
                topic = turnTopic.name,
                option = optionName
            };
            GameManager.instance.dataScript.AddTopicHistory(history);

            //stats
            switch (turnTopic.group.name)
            {
                case "Good": GameManager.instance.dataScript.StatisticIncrement(StatType.TopicsGood); break;
                case "Bad": GameManager.instance.dataScript.StatisticIncrement(StatType.TopicsBad); break;
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
                //next topics in chain
                if (turnTopic.listOfLinkedTopics.Count > 0)
                {
                    //set all linked topics to Dormant status (they can go active next turn depending on their profile)
                    foreach (Topic topic in turnTopic.listOfLinkedTopics)
                    {
                        topic.status = Status.Dormant;
                        topic.isCurrent = true;
                        Debug.LogFormat("[Tst] TopicManager.cs -> UpdateTopicTypeData: LINKED topic \"{0}\" set to Status.Dormant{1}", topic.name, "\n");
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
                        Debug.LogFormat("[Tst] TopicManager.cs -> UpdateTopicTypeData: BUDDY topic \"{0}\" set to Status.Done{1}", topic.name, "\n");
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

    #region TopicTypeData
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

    #region  Criteria Checks
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
        //topic criteria must pass checks
        if (topic.listOfCriteria != null && topic.listOfCriteria.Count > 0)
        {
            CriteriaDataInput criteriaInput = new CriteriaDataInput()
            { listOfCriteria = topic.listOfCriteria };
            string criteriaCheck = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
            if (criteriaCheck == null)
            {
                //criteria check passed O.K
                isCheck = true;
            }
            else
            {
                //criteria check FAILED
                isCheck = false;

                /*//generate message explaining why criteria failed -> debug only, spam otherwise
                Debug.LogFormat("[Tst] TopicManager.cs -> GetTopic: topic \"{0}\", Criteria FAILED \"{1}\"{2}", topic.name, criteriaCheck, "\n");*/
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
                    List<Topic> listOfTopics = GameManager.instance.dataScript.GetListOfTopics(subType);
                    if (listOfTopics != null)
                    {
                        //check subType topicTypeData
                        TopicTypeData dataSub = GameManager.instance.dataScript.GetTopicSubTypeData(subType.name);
                        if (dataSub != null)
                        {
                            if (CheckTopicTypeData(dataSub, turn, true) == true)
                            {
                                /*if (dataSub.minInterval > 0)
                                {
                                    Debug.LogFormat("[Tst] TopicManager.cs -> CheckTopicAvailable: \"{0}\", t {1}, l {2}, m {3}, isValid {4}{5}", subType.name,
                                        turn, dataSub.turnLastUsed, dataSub.minInterval, "True", "\n");
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
                                            //check status
                                            if (topic.status == Status.Live)
                                            {
                                                //at least one valid topic present, exit
                                                isValid = true;
                                                break;
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
                                    Debug.LogFormat("[Tst] TopicManager.cs -> CheckTopicAvailable: \"{0}\", t {1}, l {2}, m {3}, isValid {4}{5}", subType.name,
                                        turn, dataSub.turnLastUsed, dataSub.minInterval, "False", "\n");
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
            //check individual topicSubType criteria
            CriteriaDataInput criteriaInput = new CriteriaDataInput()
            { listOfCriteria = subType.listOfCriteria };
            criteriaCheck = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
            if (criteriaCheck != null)
            {
                Debug.LogFormat("[Tst] TopicManager.cs -> CheckSubTypeCriteria: \"{0}\" FAILED criteria check -> {1}{2}", subType.name, criteriaCheck, "\n");
                return false;
            }
            else { Debug.LogFormat("[Tst] TopicManager.cs -> CheckSubTypeCriteria: \"{0}\" PASSED criteria check{1}", subType.name, "\n"); }
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
            int turn = GameManager.instance.turnScript.Turn;
            int interval;
            //
            // - - - topicType
            //
            if (turnTopic != null)
            {
                TopicTypeData dataType = GameManager.instance.dataScript.GetTopicTypeData(turnTopicType.name);
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
                TopicTypeData dataSubType = GameManager.instance.dataScript.GetTopicSubTypeData(turnTopicSubType.name);
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

    #region Utilities
    //
    // - - - Utilities - - -
    //

    #region GetGroup Methods
    /// <summary>
    /// returns groupType based on Actor Motivations, returns Neutral if a problem
    /// </summary>
    /// <param name="motivation"></param>
    /// <returns></returns>
    private GroupType GetGroupMotivation(int motivation)
    {
        GroupType group = GroupType.Neutral;
        switch (motivation)
        {
            case 3: group = GroupType.Good; break;
            case 2: group = GroupType.Neutral; break;
            case 1: group = GroupType.Bad; break;
            case 0: group = GroupType.VeryBad; break;
            default: Debug.LogWarningFormat("Unrecognised Actor Motivation \"{0}\", default GroupType.Neutral used", motivation); break;
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
            default: Debug.LogWarningFormat("Unrecognised Player Mood \"{0}\" for {1}, {2}. Default GroupType.Neutral used", mood, GameManager.instance.playerScript.PlayerName, "Player"); break;
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
                        //high motivation, good group
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                        t.group.name.Equals("Good", StringComparison.Ordinal)).ToList());
                        break;
                    case GroupType.Neutral:
                        //neutral motivation, use all Active topics
                        if (Random.Range(0, 100) < chanceNeutralGood)
                        { listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live && t.group.name.Equals("Good", StringComparison.Ordinal)).ToList()); }
                        else
                        { listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live && t.group.name.Equals("Bad", StringComparison.Ordinal)).ToList()); }
                        break;
                    case GroupType.Bad:
                    case GroupType.VeryBad:
                        //low motivation, bad group
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
                    Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicGroup: No topics found for \"{0}\", group \"{1}\", All topics used{2}", subTypeName, group, "\n");
                    listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live).ToList());
                }
                else { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicGroup: {0} topics found for \"{1}\" group {2}{3}", listOfTopics.Count, subTypeName, group, "\n"); }
            }
            else
            {
                //filter topics by subSubTypeName
                switch (group)
                {
                    case GroupType.Good:
                        //high motivation, good group
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                        t.subSubType.name.Equals(subSubTypeName, StringComparison.Ordinal) &&
                        t.group.name.Equals("Good", StringComparison.Ordinal)).ToList());
                        break;
                    case GroupType.Neutral:
                        //neutral motivation, use all Active topics
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                        t.subSubType.name.Equals(subSubTypeName, StringComparison.Ordinal)).ToList());
                        break;
                    case GroupType.Bad:
                    case GroupType.VeryBad:
                        //low motivation, bad group
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
                    Debug.LogFormat("[Tst] TopicManager.cs -> GetActorContactTopics: No topics found for \"{0}\", {1}, group {2}, All relevant topics used{2}", subTypeName, subSubTypeName, group, "\n");
                    listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                    t.subSubType.name.Equals(subSubTypeName, StringComparison.Ordinal)).ToList());
                }
                else { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicGroup: {0} topics found for \"{1}\", {2}, group {3}{4}", listOfTopics.Count, subTypeName, subSubTypeName, group, "\n"); }
            }

        }
        else { Debug.LogError("Invalid inputList (Null)"); }
        return listOfTopics;
    }
    #endregion

    #region GetTopicSubSubType

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
                switch (GameManager.instance.sideScript.authorityOverall)
                {
                    case SideState.Human:
                        if (GameManager.instance.playerScript.status == ActorStatus.Active)
                        { isValid = true; }
                        break;
                    case SideState.AI:
                        if (GameManager.instance.aiScript.status == ActorStatus.Active)
                        { isValid = true; }
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised authorityOverall \"{0}\"", GameManager.instance.sideScript.authorityOverall);
                        break;
                }
                break;
            case 2:
                //Resistance
                switch (GameManager.instance.sideScript.resistanceOverall)
                {
                    case SideState.Human:
                        if (GameManager.instance.playerScript.status == ActorStatus.Active)
                        { isValid = true; }
                        break;
                    case SideState.AI:
                        if (GameManager.instance.aiRebelScript.status == ActorStatus.Active)
                        { isValid = true; }
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised resistanceOverall \"{0}\"", GameManager.instance.sideScript.authorityOverall);
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
            nodeID = tagNodeID,
            teamID = tagTeamID,
            contactID = tagContactID,
            secret = tagSecret
        };
        return data;
    }
    #endregion

    #region CheckOptionCriteria
    /// <summary>
    /// Check's option criteria (if any) and sets option flag for isValid (passed criteria) or not. If not, option tooltip is set here explaining why it failed criteria check
    /// Returns true if criteria (if any) passes, false if not
    /// NOTE: Option checked for Null by parent method (InitialiseTopicUI)
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    private bool CheckOptionCriteria(TopicOption option)
    {
        //determines whether option if viable and can be selected or is greyed out.
        option.isValid = true;
        if (option.listOfCriteria?.Count > 0)
        {
            //pass actor to effect criteria check if a valid actorID
            int actorCurrentSlotID = -1;
            if (tagActorID > -1)
            {
                Actor actor = GameManager.instance.dataScript.GetActor(tagActorID);
                if (actor != null)
                { actorCurrentSlotID = actor.slotID; }
                else { Debug.LogWarningFormat("Invalid actor (Null) for tagActorID \"{0}\"", tagActorID); }
            }
            CriteriaDataInput criteriaInput = new CriteriaDataInput()
            {
                listOfCriteria = option.listOfCriteria,
                actorSlotID = actorCurrentSlotID
            };
            string effectCriteria = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
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
                if (option.moodEffect != null)
                { option.tooltipDetails = GameManager.instance.personScript.GetMoodTooltip(option.moodEffect.belief, "Player"); }
                else { option.tooltipDetails = string.Format("{0}No Mood effect{1}", colourGrey, colourEnd); }
            }
        }
        return option.isValid;
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
            { GetGoodEffects(option.listOfGoodEffects, option.name, builder); }
            //Bad effects
            if (option.listOfBadEffects.Count > 0)
            { GetBadEffects(option.listOfBadEffects, option.name, builder); }
        }
        else
        {
            //probability based option
            builder.AppendFormat("<b>{0} chance of SUCCESS</b>", GetProbability(option.chance));
            if (option.listOfGoodEffects.Count > 0)
            { GetGoodEffects(option.listOfGoodEffects, option.name, builder); }
            else { builder.AppendFormat("{0}{1}Nothing Happens{2}", "\n", colourGrey, colourEnd); }
            //Bad effects
            builder.AppendFormat("{0}{1}if UNSUCCESSFUL{2}", "\n", colourCancel, colourEnd);
            if (option.listOfBadEffects.Count > 0)
            { GetBadEffects(option.listOfBadEffects, option.name, builder); }
            else { builder.AppendFormat("{0}{1}Nothing Happens{2}", "\n", colourGrey, colourEnd); }
        }
        if (builder.Length == 0) { builder.Append("No Effects present"); }
        option.tooltipMain = builder.ToString(); ;
        //Details -> derived from option mood Effect
        if (option.moodEffect != null)
        { option.tooltipDetails = GameManager.instance.personScript.GetMoodTooltip(option.moodEffect.belief, "Player"); }
        else { option.tooltipDetails = "No Mood effect"; }
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
    private void GetGoodEffects(List<Effect> listOfEffects, string optionName, StringBuilder builder)
    {
        if (listOfEffects != null)
        {
            foreach (Effect effect in listOfEffects)
            {
                if (effect != null)
                {
                    if (builder.Length > 0) { builder.AppendLine(); }
                    if (string.IsNullOrEmpty(effect.description) == false)
                    { builder.AppendFormat("{0}{1} {2}{3}", colourGood, GetEffectPrefix(effect), effect.description, colourEnd); }
                    else { Debug.LogWarningFormat("Invalid effect.description (Null or Empty) for effect \"{0}\"", effect.name); }
                }
                else { Debug.LogWarningFormat("Invalid effect (Null) in listOfGoodEffects for option \"{0}\"", optionName); }
            }
        }
        else { Debug.LogWarningFormat("Invalid listOfEffects (Null) for option \"{0}\"", optionName); }
    }
    #endregion

    #region GetBadEffects
    /// <summary>
    /// returns (to stringbuilder parameter) colour formatted string for bad effects
    /// </summary>
    /// <param name="listOfEffects"></param>
    /// <returns></returns>
    private void GetBadEffects(List<Effect> listOfEffects, string optionName, StringBuilder builder)
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
                else { Debug.LogWarningFormat("Invalid effect (Null) in listOfBadEffects for option \"{0}\"", optionName); }
            }
        }
        else { Debug.LogWarningFormat("Invalid listOfEffects (Null) for option \"{0}\"", optionName); }
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
                    builder.AppendFormat("{0}Nothing happens{1}{2}{3}Boss will Disapprove{4}{5}{6}ESC shortcut{7}", colourGrey, colourEnd, "\n",
                        colourBad, colourEnd, "\n", colourNeutral, colourEnd);
                    Debug.LogWarningFormat("Invalid Ignore Effects (None showing) for topic \"{0}\"", turnTopic.name);
                }
                else
                {
                    //boss
                    builder.AppendFormat("{0}{1}Boss will Disapprove{2}", "\n", colourBad, colourEnd);
                    //keyboard shortcut
                    builder.AppendFormat("{0}{1}ESC shortcut{2}", "\n", colourNeutral, colourEnd);
                }
                //details
                data.ignoreTooltipDetails = builder.ToString();
            }
            else
            {
                //No ignoreEffects -> default text
                data.ignoreTooltipDetails = string.Format("{0}Nothing happens{1}{2}{3}Boss will Disapprove{4}{5}{6}ESC shortcut{7}", colourGrey, colourEnd, "\n",
                colourBad, colourEnd, "\n", colourNeutral, colourEnd);
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
        Actor actor = GameManager.instance.dataScript.GetHQHierarchyActor(ActorHQ.Boss);
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
                                //if an HQ preferred option it is automatically 'Good', otherwise it's based on HQ Boss's personality
                                if (option.isPreferredByHQ == false)
                                { builder.AppendFormat("{0}{1}: {2} {3}", colourCancel, option.tag, colourEnd, GameManager.instance.personScript.GetHQTooltip(option.moodEffect.belief, actor)); }
                                else { builder.AppendFormat("{0}{1}: {2} {3}Approves{4}", colourCancel, option.tag, colourEnd, colourGood, colourEnd); }
                            }
                        }
                        else
                        {
                            //invalid belief
                            builder.AppendFormat("{0}Unknown{1}", colourGrey, colourEnd);
                            Debug.LogWarningFormat("Invalid belief (Null) for option \"{0}\", topic {1}", option.name, turnTopic.name);
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
                colourNeutral, GameManager.instance.factionScript.GetBossOpinionFormatted(), colourEnd);
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
                    //actor
                    Actor actor = GameManager.instance.dataScript.GetActor(tagActorID);
                    if (actor != null)
                    { prefix = actor.arc.name; }
                    else { Debug.LogErrorFormat("Invalid actor (Null) for tagActorID {0}", tagActorID); }
                    break;
                case 'L':
                    //All actors
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
                case 'N':
                    //Node
                    break;
                default: Debug.LogWarningFormat("Unrecognised effect.name first character \"{0}\" for effect \"{1}\"", key, effect.name); break;
            }
        }
        else { Debug.LogWarningFormat("Invalid effectName \"{0}\" (should be 'X_...')", effect.name); }
        return prefix;
    }
    #endregion

    #region CheckText
    /// <summary>
    /// takes text from any topic source, checks for tags, eg. '[actor]', and replaces with context relevant info, eg. actor arc name. Returns Null if a problem
    /// isColourHighlight TRUE -> Colour and bold highlights certain texts (colourAlert), no colour formatting if false, default True
    /// isValidation true for validating topic.text/topicOption.news having correct tags, false otherwise(iscolourHighlighting ignored for validation), objectName only required for validation checks
    /// Highlights -> actor.arc, node.nodeName
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public string CheckTopicText(string text, bool isColourHighlighting = true, bool isValidate = false, string objectName = "Unknown")
    {
        string colourCheckText = colourAlert; //highlight colour
        string checkedText = null;
        //if validation run need dictionary of tags for analysis purposes
        Dictionary<string, int> dictOfTags = null;
        if (isValidate == true)
        {
            dictOfTags = GameManager.instance.dataScript.GetDictOfTags();
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
            { node = GameManager.instance.dataScript.GetNode(tagNodeID); }
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
                            { replaceText = string.Format("{0}{1}{2}", colourCheckText, GameManager.instance.playerScript.PlayerName, colourEnd); }
                            else { replaceText = GameManager.instance.playerScript.PlayerName; }
                        }
                        else { CountTextTag("player", dictOfTags); }
                        break;
                    case "actor":
                        //actor arc name
                        if (isValidate == false)
                        {
                            if (tagActorID > -1)
                            {
                                Actor actor = GameManager.instance.dataScript.GetActor(tagActorID);
                                if (actor != null)
                                {
                                    if (isColourHighlighting == true)
                                    { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                    else { replaceText = actor.arc.name; }
                                }
                                else { Debug.LogWarningFormat("Invalid actor (Null) for tagActorID \"{0}\"", tagActorID); }
                            }
                            else
                            { Debug.LogWarningFormat("Invalid tagActorID \"{0}\" for tag <Actor>", tagActorID); }
                        }
                        else { CountTextTag("actor", dictOfTags); }
                        break;
                    case "actors":
                        //actor arc name, possessive, eg. Hacker's
                        if (isValidate == false)
                        {
                            if (tagActorID > -1)
                            {
                                Actor actor = GameManager.instance.dataScript.GetActor(tagActorID);
                                if (actor != null)
                                {
                                    if (isColourHighlighting == true)
                                    { replaceText = string.Format("{0}<b>{1}'s</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                    else { replaceText = actor.arc.name; }
                                }
                                else { Debug.LogWarningFormat("Invalid actor (Null) for tagActorID \"{0}\"", tagActorID); }
                            }
                            else
                            { Debug.LogWarningFormat("Invalid tagActorID \"{0}\" for tag <Actor>", tagActorID); }
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
                                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                                if (actor != null)
                                {
                                    if (isColourHighlighting == true)
                                    { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                    else { replaceText = actor.arc.name; }
                                }
                                else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfOptionActorIDs[0] \"{0}\"", actorID); }
                            }
                            else
                            { Debug.LogWarningFormat("Invalid actorID \"{0}\" for arrayOfOptionActorIDs[0]", actorID); }
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
                                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                                if (actor != null)
                                {
                                    if (isColourHighlighting == true)
                                    { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                    else { replaceText = actor.arc.name; }
                                }
                                else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfOptionActorIDs[1] \"{0}\"", actorID); }
                            }
                            else
                            { Debug.LogWarningFormat("Invalid actorID \"{0}\" for arrayOfOptionActorIDs[1]", actorID); }
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
                                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                                if (actor != null)
                                {
                                    if (isColourHighlighting == true)
                                    { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                    else { replaceText = actor.arc.name; }
                                }
                                else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfOptionActorIDs[0] \"{0}\"", actorID); }
                            }
                            else
                            { Debug.LogWarningFormat("Invalid actorID \"{0}\" for arrayOfOptionActorIDs[2]", actorID); }
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
                                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                                if (actor != null)
                                {
                                    if (isColourHighlighting == true)
                                    { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, actor.arc.name, colourEnd); }
                                    else { replaceText = actor.arc.name; }
                                }
                                else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfOptionActorIDs[3] \"{0}\"", actorID); }
                            }
                            else
                            { Debug.LogWarningFormat("Invalid actorID \"{0}\" for arrayOfOptionActorIDs[3]", actorID); }
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
                        //contact name + node name
                        if (isValidate == false)
                        {
                            if (tagContactID > -1)
                            {
                                Contact contact = GameManager.instance.dataScript.GetContact(tagContactID);
                                if (contact != null)
                                {
                                    if (node != null)
                                    {
                                        if (isColourHighlighting == true)
                                        { replaceText = string.Format("<b>{0} {1}</b> at {2}<b>{3}</b>{4},", contact.nameFirst, contact.nameLast, colourCheckText, node.nodeName, colourEnd); }
                                        else { replaceText = string.Format("{0} {1} at {2},", contact.nameFirst, contact.nameLast, node.nodeName); }
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
                                Contact contact = GameManager.instance.dataScript.GetContact(tagContactID);
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
                        //gear tag
                        if (isValidate == false)
                        {
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
                                ContactType contactType = node.Arc.GetRandomContactType();
                                if (contactType != null)
                                {
                                    job = contactType.pickList.GetRandomRecord();
                                    tagJob = job;
                                }
                                else
                                { Debug.LogWarningFormat("Invalid contactType (Null) for node {0}, {1}, {2}", node.nodeName, node.Arc.name, node.nodeID); }
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
                    case "daysAgo":
                        //how many turns ago expressed as '3 days'. Mincap at '1 day'
                        if (isValidate == false)
                        {
                            int turnsAgo = GameManager.instance.turnScript.Turn - tagTurn;
                            turnsAgo = Mathf.Max(1, turnsAgo);
                            replaceText = string.Format("{0} day{1} ago", turnsAgo, turnsAgo != 1 ? "s" : "");
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
                    case "npc":
                        if (isValidate == false)
                        {
                            if (Random.Range(0, 100) < 50) { replaceText = GameManager.instance.cityScript.GetCity().country.nameSet.firstFemaleNames.GetRandomRecord(); }
                            else { replaceText = GameManager.instance.cityScript.GetCity().country.nameSet.firstMaleNames.GetRandomRecord(); }
                            if (isColourHighlighting == true)
                            {
                                replaceText += " " + GameManager.instance.cityScript.GetCity().country.nameSet.lastNames.GetRandomRecord();
                                replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, replaceText, colourEnd);
                            }
                            else { replaceText += " " + GameManager.instance.cityScript.GetCity().country.nameSet.lastNames.GetRandomRecord(); }
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
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.instance.cityScript.GetCity().mayor.mayorName, colourEnd); }
                            else { replaceText = GameManager.instance.cityScript.GetCity().mayor.mayorName; }
                        }
                        else { CountTextTag("mayor", dictOfTags); }
                        break;
                    case "secret":
                        //name of random secret from among those known by the actor
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}{1}{2} secret", colourCheckText, tagSecret, colourEnd); }
                            else { replaceText = string.Format("{0} secret", tagSecret); }
                        }
                        else { CountTextTag("secret", dictOfTags); }
                        break;
                    case "city":
                        //city name
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}</b>{2}", colourCheckText, GameManager.instance.cityScript.GetCity().name, colourEnd); }
                            else { replaceText = GameManager.instance.cityScript.GetCity().name; }
                        }
                        else { CountTextTag("city", dictOfTags); }
                        break;
                    case "citys":
                        //city name possessive, eg. London's
                        if (isValidate == false)
                        {
                            if (isColourHighlighting == true)
                            { replaceText = string.Format("{0}<b>{1}'s</b>{2}", colourCheckText, GameManager.instance.cityScript.GetCity().name, colourEnd); }
                            else { replaceText = GameManager.instance.cityScript.GetCity().name; }
                        }
                        else { CountTextTag("citys", dictOfTags); }
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
                    default:
                        if (isValidate == false)
                        { Debug.LogWarningFormat("Unrecognised tag \"{0}\"", tag); }
                        else { Debug.LogFormat("[Val] TopicManager.cs -> CheckTopicText: Unrecognised tag \"{0}\" for topic {1}", tag, objectName); }
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
                    if (tagActorID > -1)
                    {
                        Actor actor = GameManager.instance.dataScript.GetActor(tagActorID);
                        if (actor != null)
                        {
                            turnSprite = actor.sprite;
                            switch (turnTopicSubType.name)
                            {
                                case "ActorMatch":
                                    //based on player mood
                                    Tuple<string, string> resultsMatch = GetPlayerTooltip();
                                    if (string.IsNullOrEmpty(resultsMatch.Item1) == false)
                                    {
                                        //tooltipMain
                                        data.imageTooltipMain = resultsMatch.Item1;
                                        //main present -> Add tooltip header (Actor name and type)
                                        data.imageTooltipHeader = string.Format("<b>{0}{1}{2}{3}{4}{5}{6}</b>", colourAlert, actor.arc.name, colourEnd, "\n", colourNormal, actor.actorName, colourEnd);
                                    }
                                    if (string.IsNullOrEmpty(resultsMatch.Item2) == false)
                                    { data.imageTooltipDetails = resultsMatch.Item2; }
                                    break;
                                default:
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
                                    break;
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid actor (Null) for tagActorID {0}", tagActorID); }
                    }
                    else { Debug.LogWarningFormat("Invalid tagActor \"{0}\"", tagActorID); }
                    break;
                case "Authority":
                    break;
                case "Resistance":
                    break;
                case "City":
                    break;
                case "Family":
                    break;
                case "HQ":
                    break;
                case "Player":
                    turnSprite = GameManager.instance.playerScript.sprite;
                    Tuple<string, string> resultsPlayer = GetPlayerTooltip();
                    string playerName = GameManager.instance.playerScript.PlayerName;
                    if (string.IsNullOrEmpty(resultsPlayer.Item1) == false)
                    {
                        //tooltipMain
                        data.imageTooltipMain = resultsPlayer.Item1;
                        //main present -> Add tooltip header (Player name)
                        data.imageTooltipHeader = string.Format("<b>{0}{1}{2}</b>", colourAlert, playerName, colourEnd);
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
    /// Returns tooltip main and details for various actor subTypes (Motivation). tooltip.Header already covered by parent method. If returns nothing, which is O.K, then no tooltip is shown on mouseover
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
                        Contact contact = GameManager.instance.dataScript.GetContact(tagContactID);
                        if (contact != null)
                        {
                            builder.AppendFormat("{0}CONTACT{1}{2}", colourCancel, colourEnd, "\n");
                            builder.AppendFormat("{0}{1} {2}{3}{4}", colourNormal, contact.nameFirst, contact.nameLast, colourEnd, "\n");
                            builder.AppendFormat("{0}{1}{2}{3}", colourNeutral, contact.job, colourEnd, "\n");
                            builder.AppendFormat("<b>{0} {1}</b>", contact.nameFirst, GameManager.instance.contactScript.GetEffectivenessFormatted(contact.effectiveness));
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
                    int motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                    int oddsGood = chanceNeutralGood;
                    int oddsBad = 100 - chanceNeutralGood;
                    builder.AppendFormat("Determined by{0}{1}{2}'s{3}{4}{5}<size=110%>Motivation</size>{6}{7}", "\n", colourAlert, actor.arc.name, colourEnd, "\n", colourNeutral, colourEnd, "\n");
                    //highlight current motivation band, grey out the rest
                    if (motivation == 3) { builder.AppendFormat("if {0}3{1}, {2}Good{3}{4}", colourNeutral, colourEnd, colourGood, colourEnd, "\n"); }
                    else { builder.AppendFormat("<size=90%>{0}if 3, Good{1}{2}</size>", colourGrey, colourEnd, "\n"); }

                    if (motivation == 2) { builder.AppendFormat("if {0}2{1}, could be either{2}<size=90%>({3}/{4} Good/Bad)</size>{5}", colourNeutral, colourEnd, "\n", oddsGood, oddsBad, "\n"); }
                    else { builder.AppendFormat("<size=90%>{0}if 2, could be either{1}({2}/{3} Good/Bad){4}{5}</size>", colourGrey, "\n", oddsGood, oddsBad, colourEnd, "\n"); }

                    if (motivation < 2) { builder.AppendFormat("if {0}1{1} or {2}0{3}, {4}Bad{5}", colourNeutral, colourEnd, colourNeutral, colourEnd, colourBad, colourEnd); }
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
                    int mood = GameManager.instance.playerScript.GetMood();
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

    #endregion

    #region Meta Methods
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
        UpdateTopicTypes(GameManager.instance.dataScript.GetDictOfTopicTypeData());
        UpdateTopicTypes(GameManager.instance.dataScript.GetDictOfTopicSubTypeData());
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

    #region Debug Methods
    //
    // - - - Debug - - -
    //

    /// <summary>
    /// Display's topic type data in a more user friendly manner (subTypes grouped by Types)
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTopicTypes()
    {
        StringBuilder builder = new StringBuilder();
        Dictionary<string, TopicTypeData> dictOfTopicTypes = GameManager.instance.dataScript.GetDictOfTopicTypeData();
        if (dictOfTopicTypes != null)
        {
            Dictionary<string, TopicTypeData> dictOfTopicSubTypes = GameManager.instance.dataScript.GetDictOfTopicSubTypeData();
            if (dictOfTopicSubTypes != null)
            {
                GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
                builder.AppendFormat("- TopicTypeData for TopicTypes{0}{1}", "\n", "\n");
                //used to print a '*' to indicate topic / subType is valid and ready to go
                bool isValidType, isValidSubType;
                //loop topic Types
                foreach (var topicTypeData in dictOfTopicTypes)
                {
                    TopicType topicType = GameManager.instance.dataScript.GetTopicType(topicTypeData.Key);
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
                                    TopicTypeData subData = GameManager.instance.dataScript.GetTopicSubTypeData(subType.name);
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
        List<TopicType> listOfTopicTypeLevel = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
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
        Dictionary<int, HistoryTopic> dictOfTopicHistory = GameManager.instance.dataScript.GetDictOfTopicHistory();
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
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- listOfTopicTypes -> Criteria{0}", "\n");
        //listOfTopicTypes
        List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypes();
        if (listOfTopicTypes != null)
        {
            foreach (TopicType topicType in listOfTopicTypes)
            {
                builder.AppendFormat("{0} {1}, pr: {2}, minInt {3}{4}", "\n", topicType.tag, topicType.priority.name, topicType.minInterval, "\n");
                if (topicType.listOfCriteria.Count > 0)
                {
                    foreach (Criteria criteria in topicType.listOfCriteria)
                    { builder.AppendFormat("    \"{0}\", {1}{2}", criteria.name, criteria.description, "\n"); }
                }
                else { builder.AppendFormat("No criteria{0}", "\n"); }
                //loop subTypes
                foreach (TopicSubType subType in topicType.listOfSubTypes)
                {
                    //needs to be the correct side
                    if (subType.side.level == playerSide.level || subType.side.level == 3)
                    {
                        builder.AppendFormat("  -{0}, pr: {1}, minInt {2}{3}", subType.name, subType.priority.name, subType.minInterval, "\n");
                        if (subType.listOfCriteria.Count > 0)
                        {
                            foreach (Criteria criteria in subType.listOfCriteria)
                            { builder.AppendFormat("    \"{0}\", {1}{2}", criteria.name, criteria.description, "\n"); }
                        }
                    }
                    else { builder.AppendFormat("  -{0} -> Not Valid for this Side{1}", subType.name, "\n"); }
                }
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
        List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
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
                        List<Topic> listOfTopics = GameManager.instance.dataScript.GetListOfTopics(subType);
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
        List<Topic> listOfTopics = GameManager.instance.dataScript.GetListOfTopics(turnTopicSubType);
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
        StringBuilder builder = new StringBuilder();
        Dictionary<string, Topic> dictOfTopics = GameManager.instance.dataScript.GetDictOfTopics();
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
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
                    }
                    else { builder.AppendFormat(" {0} -> Invalid Profile (Null){1}", topic.Value.name, "\n"); }
                }
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
        int turn = GameManager.instance.turnScript.Turn;
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
        return false;
    }

    #endregion

    //new methods above here
}
