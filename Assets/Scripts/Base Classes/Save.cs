using gameAPI;
using packageAPI;
using System.Collections.Generic;


/// <summary>
/// Master serializable class containing all game data for save / load file operations
/// NOTE: does NOT derive from Monobehaviour
/// </summary>
[System.Serializable]
public class Save
{
    public SaveGameStatus gameStatus = new SaveGameStatus();
    public SaveSeedData seedData = new SaveSeedData();
    public SaveCampaignData campaignData = new SaveCampaignData();
    public SaveDataData dataData = new SaveDataData();
    public SaveOptionData optionData = new SaveOptionData();
    public SavePlayerData playerData = new SavePlayerData();
    public SaveGameData gameData = new SaveGameData();
    public SaveActorData actorData = new SaveActorData();
    public SaveNodeData nodeData = new SaveNodeData();
    public SaveConnectionData connData = new SaveConnectionData();
    public SaveNemesisData nemesisData = new SaveNemesisData();
    public SaveGearData gearData = new SaveGearData();
    public SaveTopicData topicData = new SaveTopicData();
    public SaveAIData aiData = new SaveAIData();
    public SaveScenarioData scenarioData = new SaveScenarioData();
    public SaveContactData contactData = new SaveContactData();
    public SaveTargetData targetData = new SaveTargetData();
    public SaveStatisticsData statisticsData = new SaveStatisticsData();
    public SaveGUIData guiData = new SaveGUIData();
    public SaveMetaGameData metaGameData = new SaveMetaGameData();
}

#region Managers
//
// - - - Managers - - -
//

#region SaveGameStatus
/// <summary>
/// GameStatus -> used to restore game correctly, eg. coul be the middle of a MetaGame, for example
/// </summary>
[System.Serializable]
public class SaveGameStatus
{
    public GameState gameState;
    public RestorePoint restorePoint;                   //only relevant if gameState.MetaGame. Ignored otherwise
    public int turn;                                    //info purposes only (top of the file, easy to read)
    public string time;                                 //time of save (info only)
}
#endregion


#region SaveSeedData
/// <summary>
/// Level seed used to generate level
/// </summary>
[System.Serializable]
public class SaveSeedData
{
    public int levelSeed;
    public int devSeed;  
}
#endregion


#region SaveCampaignData
/// <summary>
/// CampaignManager.cs data
/// </summary>
[System.Serializable]
public class SaveCampaignData
{
    public string campaignName;
    public int scenarioIndex;
    public int commendations;
    public int blackMarks;
    public int investigationBlackMarks;
    public bool isNpc;                 //set true if NPC present, false otherwise. Needed because unable to JSON SaveNpc as a Null
    public SaveNpc npc;
}
#endregion


#region SaveOptionData
/// <summary>
/// OptionManager.cs data
/// </summary>
[System.Serializable]
public class SaveOptionData
{
    public bool autoGearResolution;
    public bool fogOfWar;
    public bool debugData;
    public bool noAI;
    public bool showContacts;
    public bool showRenown;
    public bool connectorTooltips;
    public ColourScheme colourScheme;
}
#endregion


#region SaveDataData
/// <summary>
/// DataManager.cs data
/// </summary>
[System.Serializable]
public class SaveDataData
{
    //secrets
    public List<string> listOfPlayerSecrets = new List<string>();
    public List<string> listOfOrganisationSecrets = new List<string>();
    public List<string> listOfStorySecrets = new List<string>();
    public List<string> listOfRevealedSecrets = new List<string>();
    public List<string> listOfDeletedSecrets = new List<string>();
    public List<SaveSecret> listOfSecretChanges = new List<SaveSecret>();
    //investigations
    public List<Investigation> listOfInvestigations = new List<Investigation>();
    //awards
    public List<AwardData> listOfCommendations = new List<AwardData>();
    public List<AwardData> listOfBlackmarks = new List<AwardData>();
    //organisations
    public List<bool> listOfOrgInfoData = new List<bool>();
    public List<string> listOfCurrentOrganisations = new List<string>();
    public List<SaveOrganisation> listOfSaveOrganisations = new List<SaveOrganisation>();
    public List<OrgData> listOfCureOrgData = new List<OrgData>();
    public List<OrgData> listOfContractOrgData = new List<OrgData>();
    public List<OrgData> listOfEmergencyOrgData = new List<OrgData>();
    public List<OrgData> listOfHQOrgData = new List<OrgData>();
    public List<OrgData> listOfInfoOrgData = new List<OrgData>();
    //HQ
    public List<string> listOfHqEvents = new List<string>();
    //metaOptions
    public List<SaveMetaOption> listOfMetaOptions = new List<SaveMetaOption>();
    //contacts
    public int contactCounter;
    public List<int> listOfContactPool = new List<int>();
    public List<Contact> listOfContacts = new List<Contact>();
    public List<IntListWrapper> listOfActorContactsValue = new List<IntListWrapper>();
    public List<int> listOfActorContactsKey = new List<int>();
    public List<IntListWrapper> listOfNodeContactsByResistanceValue = new List<IntListWrapper>();
    public List<int> listOfNodeContactsByResistanceKey = new List<int>();
    public List<IntListWrapper> listOfNodeContactsByAuthorityValue = new List<IntListWrapper>();
    public List<int> listOfNodeContactsByAuthorityKey = new List<int>();
    public List<ContactListWrapper> listOfContactsByNodeResistanceValue = new List<ContactListWrapper>();
    public List<int> listOfContactsByNodeResistanceKey = new List<int>();
    //teams
    public int teamCounter;
    public List<SaveTeam> listOfTeams = new List<SaveTeam>();
    public List<int> listOfArrayOfTeams = new List<int>();
    public List<int> listOfTeamPoolReserve = new List<int>();
    public List<int> listOfTeamPoolOnMap = new List<int>();
    public List<int> listOfTeamPoolInTransit = new List<int>();
    //statistics
    public List<int> listOfStatisticsLevel = new List<int>();
    public List<int> listOfStatisticsCampaign = new List<int>();
    //AI
    public List<int> listOfArrayOfAIResources = new List<int>();
    public List<AITracker> listOfRecentNodes = new List<AITracker>();
    public List<AITracker> listOfRecentConnections = new List<AITracker>();
    //History
    public List<HistoryRebelMove> listOfHistoryRebel = new List<HistoryRebelMove>();
    public List<HistoryNemesisMove> listOfHistoryNemesis = new List<HistoryNemesisMove>();
    public List<HistoryNpcMove> listOfHistoryVip = new List<HistoryNpcMove>();
    public List<HistoryActor> listOfHistoryPlayer = new List<HistoryActor>();
    public List<HistoryLevel> listOfHistoryLevel = new List<HistoryLevel>();
    //Messages
    public int messageIDCounter;
    public List<int> listOfArchiveMessagesKey = new List<int>();
    public List<Message> listOfArchiveMessagesValue = new List<Message>();
    public List<int> listOfPendingMessagesKey = new List<int>();
    public List<Message> listOfPendingMessagesValue = new List<Message>();
    public List<int> listOfCurrentMessagesKey = new List<int>();
    public List<Message> listOfCurrentMessagesValue = new List<Message>();
    public List<int> listOfAIMessagesKey = new List<int>();
    public List<Message> listOfAIMessagesValue = new List<Message>();
    //Topics
    public List<string> listOfTopicTypesLevel = new List<string>();
    public List<TopicTypeData> listOfTopicTypeValues = new List<TopicTypeData>();
    public List<string> listOfTopicTypeKeys = new List<string>();
    public List<TopicTypeData> listOfTopicSubTypeValues = new List<TopicTypeData>();
    public List<string> listOfTopicSubTypeKeys = new List<string>();
    public List<string> listOfTopicPoolsKeys = new List<string>();
    public List<StringListWrapper> listOfTopicPoolsValue = new List<StringListWrapper>();
    public List<int> listOfTopicHistoryKeys = new List<int>();
    public List<HistoryTopic> listOfTopicHistoryValues = new List<HistoryTopic>();
    //Relations
    public List<int> listOfRelationshipKeys = new List<int>();
    public List<RelationshipData> listOfRelationshipValues = new List<RelationshipData>();
    //Registers
    public List<EffectDataOngoing> listOfOngoingEffects = new List<EffectDataOngoing>();
    public List<ActionAdjustment> listOfActionAdjustments = new List<ActionAdjustment>();
    //moving
    public List<int> listOfMoveNodes = new List<int>();
    //cures
    public List<SaveCure> listOfCures = new List<SaveCure>();
    //InfoPipeline
    public List<SaveInfoPipeLineInfo> listOfInfoPipelineDetails = new List<SaveInfoPipeLineInfo>();
    //MainInfo App data
    public SaveMainInfo currentInfo = new SaveMainInfo();
    public List<SavePriorityInfo> listOfPriorityData = new List<SavePriorityInfo>();
    public List<ItemData> listOfDelayedItemData = new List<ItemData>();
    public List<SaveMainInfo> listOfHistoryValue = new List<SaveMainInfo>();
    public List<int> listOfHistoryKey = new List<int>();
    //Newsfeed
    public List<NewsItem> listOfNewsItems = new List<NewsItem>();
    public List<string> listOfAdverts = new List<string>();
    //TestLists
    public List<string> listOfTextListNames = new List<string>();
    public List<int> listOfTextListIndexes = new List<int>();

}
#endregion


#region SavePlayerData
/// <summary>
/// PlayerManager.cs data
/// </summary>
[System.Serializable]
public class SavePlayerData
{
    public int renown;
    public int Invisibility;
    public int Innocence;
    public int mood;
    public ActorStatus status;
    public ActorTooltip tooltipStatus;
    public ActorInactive inactiveStatus;
    public ActorSex sex;
    public bool isBreakdown;
    public bool isEndOfTurnGearCheck;
    public bool isLieLowFirstturn;
    public bool isAddicted;
    public bool isStressLeave;
    public bool isStressed;
    public bool isSpecialMoveGear;
    public int numOfSuperStress;
    public int stressImmunityCurrent;
    public int stressImmunityStart;
    public int addictedTally;
    public string profile;
    public string profileDescriptor;
    public string profileExplanation;
    public bool[] arrayOfCaptureTools;
    public List<int> listOfPersonalityFactors = new List<int>();
    public List<string> listOfDescriptors = new List<string>();
    public List<string> listOfGear = new List<string>();
    public List<string> listOfSecrets = new List<string>();
    public List<string> listOfConditionsResistance = new List<string>();
    public List<string> listOfConditionsAuthority = new List<string>();
    public List<HistoryMood> listOfMoodHistory = new List<HistoryMood>();
    public List<Investigation> listOfInvestigations = new List<Investigation>();
    public List<NodeActionData> listOfNodeActions = new List<NodeActionData>();
}
#endregion


#region SaveMetaData
/// <summary>
/// MetaGame data (save/load during metaGame)
/// </summary>
[System.Serializable]
public class SaveMetaGameData
{
    //metaGameOptions
    public MetaGameOptions metaGameOptions;
    //
    // - - - TransitionData
    //
    //End Level
    public string objectiveStatus;
    public List<EndLevelData> listOfEndLevelData = new List<EndLevelData>();
    //HQ status
    public List<string> listOfHqSprites = new List<string>();
    public List<string> listOfHqRenown = new List<string>();
    public List<string> listOfHqTitles = new List<string>();
    public List<string> listOfHqEvents = new List<string>();
    public List<TooltipData> listOfHqTooltips = new List<TooltipData>();
    public List<string> listOfWorkerSprites = new List<string>();
    public List<string> listOfWorkerRenown = new List<string>();
    public List<string> listOfWorkerArcs = new List<string>();
    public List<TooltipData> listOfWorkerTooltips = new List<TooltipData>();

    //
    // - - - MetaData
    //
    public List<SaveMetaData> listOfBoss = new List<SaveMetaData>();
    public List<SaveMetaData> listOfSubBoss1 = new List<SaveMetaData>();
    public List<SaveMetaData> listOfSubBoss2 = new List<SaveMetaData>();
    public List<SaveMetaData> listOfSubBoss3 = new List<SaveMetaData>();
    public List<SaveMetaData> listOfStatusData = new List<SaveMetaData>();
    public List<SaveMetaData> listOfRecommended = new List<SaveMetaData>();
    public SaveMetaData selectedDefault = new SaveMetaData();
    
}

#endregion


#region SaveGameData
/// <summary>
/// Assorted important game related data
/// </summary>
[System.Serializable]
public class SaveGameData
{
    //sideManager.cs
    public SideState resistanceCurrent;
    public SideState authorityCurrent;
    public SideState resistanceOverall;
    public SideState authorityOverall;
    public string playerSide;
    //turnManager.cs
    public TurnActionData turnData;
    public WinStateLevel winStateLevel;
    public WinReasonLevel winReasonLevel;
    public WinStateCampaign winStateCampaign;
    public WinReasonCampaign winReasonCampaign;
    public AuthoritySecurityState authoritySecurity;
    public string currentSide;
    public bool haltExecution;
    public bool isSecurityFlash;
}
#endregion


#region SaveScenarioData
/// <summary>
/// City, Objective and HQ Manager.cs data
/// </summary>
[System.Serializable]
public class SaveScenarioData
{
    public int cityLoyalty;
    //HqManager.cs
    public int bossOpinion;
    public int approvalZeroTimer;
    public bool isZeroTimerThisTurn;
    public int hqSupportAuthority;
    public int hqSupportResistance;
    public bool isHqRelocating;
    public int timerHqRelocating;
    //ObjectiveManager.cs
    public List<string> listOfObjectiveNames = new List<string>();
    public List<int> listOfObjectiveProgress = new List<int>();
}
#endregion


#region SaveActorData
/// <summary>
/// ActorManager.cs
/// </summary>
[System.Serializable]
public class SaveActorData
{
    //dataManager.cs collections
    public List<SaveActor> listOfDictActors = new List<SaveActor>();
    public List<SaveActor> listOfDictHQ = new List<SaveActor>();
    public List<int> listOfActors = new List<int>();
    public List<int> listOfActorsHQ = new List<int>();
    public List<bool> listOfActorsPresent = new List<bool>();
    public List<int> authorityActorPoolLevelOne = new List<int>();
    public List<int> authorityActorPoolLevelTwo = new List<int>();
    public List<int> authorityActorPoolLevelThree = new List<int>();
    public List<int> authorityActorReserve = new List<int>();
    public List<int> authorityActorDismissed = new List<int>();
    public List<int> authorityActorPromoted = new List<int>();
    public List<int> authorityActorDisposedOf = new List<int>();
    public List<int> authorityActorResigned = new List<int>();
    public List<int> resistanceActorPoolLevelOne = new List<int>();
    public List<int> resistanceActorPoolLevelTwo = new List<int>();
    public List<int> resistanceActorPoolLevelThree = new List<int>();
    public List<int> resistanceActorReserve = new List<int>();
    public List<int> resistanceActorDismissed = new List<int>();
    public List<int> resistanceActorPromoted = new List<int>();
    public List<int> resistanceActorDisposedOf = new List<int>();
    public List<int> resistanceActorResigned = new List<int>();
    public List<int> actorHQPool = new List<int>();
    //actorManager.cs
    public int actorIDCounter;
    public int hqIDCounter;
    public int lieLowTimer;
    public int doomTimer;
    public int captureTimer;
    public bool isGearCheckRequired;
    public string nameSet;
    //fast access fields for actor.cs
    public string actorStressNone;
    public string actorCorruptNone;
    public string actorUnhappyNone;
    public string actorBlackmailNone;
    public string actorBlackmailTimerHigh;
    public string actorBlackmailTimerLow;
    public int maxNumOfSecrets;
    public int compatibilityOne;
    public int compatibilityTwo;
    public int compatibilityThree;
}
#endregion


#region SaveNodeData
/// <summary>
/// NodeManager.cs
/// </summary>
[System.Serializable]
public class SaveNodeData
{
    public int crisisPolicyModifier;
    public int nodeCounter;
    public int connCounter;
    public int nodeHighlight;
    public int nodePlayer;
    public int nodeNemesis;
    public int nodeCaptured;
    //node.cs data
    public List<SaveNode> listOfNodes = new List<SaveNode>();
    public List<int> listOfCrisisNodes = new List<int>();
    public List<int> listOfCureNodes = new List<int>();
    public List<int> listOfMoveNodes = new List<int>();
}
#endregion


#region SaveConnectionData
/// <summary>
/// dynamic Connection.SO data
/// </summary>
[System.Serializable]
public class SaveConnectionData
{
    public List<SaveConnection> listOfConnectionData = new List<SaveConnection>();
}
#endregion


#region SaveGearData
/// <summary>
/// GearManager.cs
/// </summary>
[System.Serializable]
public class SaveGearData
{
    //gearManager.cs
    public int gearSaveCurrentCost;
    public List<string> listOfCompromisedGear = new List<string>();
    //gear.cs data
    public List<SaveGear> listOfGear = new List<SaveGear>();
    //dataManager collections
    public List<string> listOfCommonGear = new List<string>();
    public List<string> listOfRareGear = new List<string>();
    public List<string> listOfUniqueGear = new List<string>();
    public List<string> listOfLostGear = new List<string>();
    public List<string> listOfCurrentGear = new List<string>();
}
#endregion


#region SaveTopicData
/// <summary>
/// Topics
/// </summary>
[System.Serializable]
public class SaveTopicData
{
    public string storyAlpha;
    public string storyBravo;
    public string storyCharlie;
    public bool isStoryAlphaGood;
    public bool isStoryBravoGood;
    public bool isStoryCharlieGood;
    public int storyAlphaCurrentIndex;
    public int storyBravoCurrentIndex;
    public int storyCharlieCurrentIndex;
    public List<SaveTopic> listOfTopics = new List<SaveTopic>();
}
#endregion


#region SaveNemesisData

/// <summary>
/// NemesisManager.cs
/// </summary>
[System.Serializable]
public class SaveNemesisData
{
    public NemesisSaveClass saveData;
}
#endregion


#region SaveContactData
/// <summary>
/// ContactManager.cs
/// </summary>
[System.Serializable]
public class SaveContactData
{
    public int contactIDCounter;
    public List<int> listOfContactNetworks = new List<int>();
    public List<int> listOfActors = new List<int>();
}
#endregion


#region SaveAIData
/// <summary>
/// AIManager.cs and AIRebelManager.cs
/// </summary>
[System.Serializable]
public class SaveAIData
{
    //AIManager.cs -> collections
    public List<AITask> listOfTasksFinal = new List<AITask>();
    public List<AITask> listOfTasksPotential = new List<AITask>();
    public List<string> listOfPlayerEffects = new List<string>();
    public List<string> listOfPlayerEffectDescriptors = new List<string>();
    public List<int> listOfAITaskTypesAuthority = new List<int>();
    //AIManager.cs -> public fields
    public bool immediateFlagAuthority;
    public bool immediateFlagResistance;
    public int resourcesGainAuthority;
    public int resourcesGainResistance;
    public int aiTaskCounter;
    public int hackingAttemptsTotal;
    public int hackingAttemptsReboot;
    public int hackingAttemptsDetected;
    public int hackingCurrentCost;
    public int hackingModifiedCost;
    public bool isHacked;
    public Priority aiAlertStatus;
    public bool isRebooting;
    public int rebootTimer;
    public int numOfCrisis;
    public ActorStatus status;
    public ActorInactive inactiveStatus;
    //AIManager.cs -> private fields
    public SaveAIClass saveAI;
    //AIRebelManager.cs
    public SaveAIRebelClass saveRebel;
    public List<AITracker> listOfNemesisReports = new List<AITracker>();
    public List<AITracker> listOfErasureReports = new List<AITracker>();
    public List<int> listOfAITaskTypesRebel = new List<int>();
}
#endregion


#region SaveTargetData
/// <summary>
/// TargetManager.cs
/// </summary>
[System.Serializable]
public class SaveTargetData
{
    public int startTargets;
    public int activeTargets;
    public int liveTargets;
    public int maxTargets;
    public string targetOrgName;
    //dictOfTargets dynamic data
    public List<SaveTarget> listOfTargets = new List<SaveTarget>();
    //target pools
    public List<string> listOfTargetPoolActive = new List<string>();
    public List<string> listOfTargetPoolLive = new List<string>();
    public List<string> listOfTargetPoolOutstanding = new List<string>();
    public List<string> listOfTargetPoolDone = new List<string>();
    //other
    public List<int> listOfNodesWithTargets = new List<int>();
    public List<StringListWrapper> listOfGenericTargets = new List<StringListWrapper>();
}
#endregion


#region SaveStatisticsData
/// <summary>
/// StatisticsManager.cs data
/// </summary>
[System.Serializable]
public class SaveStatisticsData
{
    public float ratioPlayerNodeActions;
    public float ratioPlayerTargetAttempts;
    public float ratioPlayerMoveActions;
    public float ratioPlayerLieLowDays;
    public float ratioPlayerGiveGear;
    public float ratioPlayerManageActions;
    public float ratioPlayerDoNothing;
}
#endregion


#region SaveGUIData
/// <summary>
/// GUI related data
/// </summary>
[System.Serializable]
public class SaveGUIData
{
    //TopBarUI
    public int commendationData;
    public int blackmarkData;
    public int investigationData;
    public int innocenceData;
    public int unhappyData;
    public int conflictData;
    public int blackmailData;
    public int doomData;
}
#endregion

#endregion


#region SO's ...
//
// - - - SO's - - -
//

#region SaveSecret
/// <summary>
/// Secret.S0 dynamic data
/// </summary>
[System.Serializable]
public class SaveSecret
{
    public string secretName;
    public int revealedID;
    public string revealedWho;
    public TimeStamp gainedWhen;
    public TimeStamp revealedWhen;
    public TimeStamp deleteWhen;
    public SecretStatus status;
    public List<int> listOfActors = new List<int>();
}
#endregion

#region SaveGear
/// <summary>
/// Gear.SO dynamic data
/// </summary>
[System.Serializable]
public class SaveGear
{
    public string gearName;
    public int timesUsed;
    public bool isCompromised;
    public string reasonUsed;
    public int chanceOfCompromise;
    public int statTurnObtained;
    public int statTurnLost;
    public int statTimesUsed;
    public int statTimesGiven;
    public int statTimesCompromised;
    public int statTimesSaved;
    public int statRenownSpent;
}
#endregion

#region SaveTopic
/// <summary>
/// Topic.SO dynamic data
/// </summary>
[System.Serializable]
public class SaveTopic
{
    public string topicName;
    public Status status;
    public int timerStart;
    public int timerRepeat;
    public int timerWindow;
    public int turnsDormant;
    public int turnsActive;
    public int turnsLive;
    public int turnsDone;
}
#endregion

#endregion


#region Base Classes ...
//
// - - - Base Classes - - -
//

#region SaveActor
/// <summary>
/// Actor.SO full data set
/// </summary>
[System.Serializable]
public class SaveActor
{
    //data needed for all actors
    public ActorStatus status;
    public ActorHQ statusHQ;
    public ActorSex sex;
    public int actorID;
    public int hqID;
    public int datapoint0;
    public int datapoint1;
    public int datapoint2;
    public string side;
    public int slotID;
    public int level;
    public int nodeCaptured;
    public string gearName;
    public string actorName;
    public string firstName;
    public string spriteName;
    public string arcName;
    public int compatibilityWithPlayer;
    public string profile;
    public string profileDescriptor;
    public string profileExplanation;
    //public Trait trait;
    public string traitName;
    //data that can be ignored if actor is in Recruit Pool
    public int Renown;
    public int unhappyTimer;
    public int blackmailTimer;
    public int captureTimer;
    public int numOfTimesBullied;
    public int numOfTimesCaptured;
    public int numOfTimesConflict;
    public int numOfTimesBreakdown;
    public int numOfTimesStressLeave;
    public int numOfDaysStressed;
    public int numOfDaysLieLow;
    public int departedNumOfSecrets;
    public bool isPromised;
    public bool isNewRecruit;
    public bool isReassured;
    public bool isThreatening;
    public bool isComplaining;
    public bool isBreakdown;
    public bool isLieLowFirstturn;
    public bool isStressLeave;
    public bool isTraitor;
    public bool isDismissed;
    public bool isResigned;
    public ActorTooltip tooltipStatus;
    public ActorInactive inactiveStatus;
    public int gearTimer;
    public int gearTimesTaken;
    //collections
    public List<int> listOfPersonalityFactors = new List<int>();
    public List<string> listOfDescriptors = new List<string>();
    public List<HistoryMotivation> listOfMotivation = new List<HistoryMotivation>();
    public List<int> listOfTeams = new List<int>();
    public List<string> listOfSecrets = new List<string>();
    public List<string> listOfConditions = new List<string>();
    public List<int> listOfContactNodes = new List<int>();
    public List<int> listOfContacts = new List<int>();
    public List<string> listOfTraitEffects = new List<string>();
    public List<NodeActionData> listOfNodeActions = new List<NodeActionData>();
    public List<TeamActionData> listOfTeamActions = new List<TeamActionData>();
    public List<HqRenownData> listOfHqRenownData = new List<HqRenownData>();
    public List<HistoryActor> listOfHistory = new List<HistoryActor>();

    /*public List<int> listOfTraitEffects = new List<int>();*/   //trait effects generated dynamically when loading trait
}
#endregion


#region SaveNode
/// <summary>
/// Node.SO
/// </summary>
[System.Serializable]
public class SaveNode
{
    public int nodeID;
    public int security;
    public int stability;
    public int support;
    public bool isTracerKnown;
    public bool isSpiderKnown;
    public bool isContactKnown;
    public bool isTeamKnown;
    public bool isTargetKnown;

    public bool isTracer;
    public bool isSpider;
    public bool isContactResistance;
    public bool isContactAuthority;
    public bool isPreferredAuthority;
    public bool isPreferredResistance;
    public bool isCentreNode;
    public bool isLoiterNode;
    public bool isConnectedNode;
    public bool isChokepointNode;

    public string targetName;
    public char defaultChar;

    public int spiderTimer;
    public int tracerTimer;
    public int activityCount;
    public int activityTime;

    public bool isStabilityTeam;
    public bool isSecurityTeam;
    public bool isSupportTeam;
    public bool isProbeTeam;
    public bool isSpiderTeam;
    public bool isDamageTeam;
    public bool isErasureTeam;

    public int crisisTimer;
    public int waitTimer;
    public string nodeCrisisName;

    public LoiterData loiter;
    public string cureName;

    public List<int> listOfTeams = new List<int>();
    public List<int> listOfOngoingEffects = new List<int>();
}
#endregion


#region SaveConnection
/// <summary>
/// Connection.So
/// </summary>
[System.Serializable]
public class SaveConnection
{
    public int connID;
    public int activityCount;
    public int activityTime;
    public ConnectionType securityLevel;
    public List<EffectDataOngoing> listOfOngoingEffects = new List<EffectDataOngoing>();
}
#endregion


#region SaveTeam
/// <summary>
/// Team.SO
/// </summary>
[System.Serializable]
public class SaveTeam
{
    public int teamID;
    public string teamName;
    public TeamPool pool;
    public int arcID;
    public int actorSlotID;
    public int nodeID;
    public int timer;
    public int turnDeployed;
}
#endregion


#region SaveOrganisation
/// <summary>
/// Organisation.SO
/// </summary>
[System.Serializable]
public class SaveOrganisation
{
    public string name;
    public bool isContact;
    public bool isCutOff;
    public bool isSecretKnown;
    public int reputation;
    public int freedom;
    public int maxStat;
    public int timer;
}
#endregion


#region SaveTarget

/// <summary>
/// Target.SO dynamic data
/// </summary>
[System.Serializable]
public class SaveTarget
{
    public Status targetStatus;
    public int intel;
    public string targetName;
    public int ongoingID;
    public bool isKnownByAI;
    public int nodeID;
    public int distance;
    public int newIntel;
    public int intelGain;
    public int turnSuccess;
    public int turnDone;
    public int numOfAttempts;
    public int turnsWindow;
    public int timerDelay;
    public int timerHardLimit;
    public int timerWindow;
    public List<int> listOfRumourContacts = new List<int>();
}
#endregion


#region SaveCure
/// <summary>
/// Cure.SO dynamic data
/// </summary>
[System.Serializable]
public class SaveCure
{
    public string cureName;
    public bool isActive;
    public bool isOrgCure;
}
#endregion


#region SaveNpc
/// <summary>
/// Npc.SO dynamic data (for single Npc in current mission)
/// </summary>
[System.Serializable]
public class SaveNpc
{
    public NpcStatus status;
    public int timerTurns;
    public int startNodeID;
    public int endNodeID;
    public int currentNodeID;
    public int daysActive;
}
#endregion


#region SaveMetaOption
/// <summary>
/// MetaOption.SO dynamic data
/// </summary>
[System.Serializable]
public class SaveMetaOption
{
    public string optionName;
    public int statTimesSelected;
}
#endregion


#region SaveMetaData
/// <summary>
/// MetaData packageManager data
/// </summary>
[System.Serializable]
public class SaveMetaData
{
    public string metaName;
    public string itemText;
    public string textSelect;
    public string textDeselect;
    public string textInsufficient;
    public string bottomText;
    public string inactiveText;
    public string spriteName;
    public MetaPriority priority;
    public MetaTabSide tabSide;
    public MetaTabTop tabTop;
    public int renownCost;
    public int sideLevel;
    public bool isActive;
    public bool isRecommended;
    public bool isCriteria;
    public bool isSelected;
    public MetaPriority recommendedPriority;
    public List<string> listOfEffects = new List<string>();
    public int data;
    public string dataName;
    public string dataTag;
    public int help;
    public string tag0;
    public string tag1;
    public string tag2;
    public string tag3;
}
#endregion

#endregion


#region SubClasses ...
/// <summary>
/// MainInfoData subclass
/// </summary>
[System.Serializable]
public class SaveMainInfo
{
    public List<ItemData> listOfTab0Item = new List<ItemData>();
    public List<ItemData> listOfTab1Item = new List<ItemData>();
    public List<ItemData> listOfTab2Item = new List<ItemData>();
    public List<ItemData> listOfTab3Item = new List<ItemData>();
    public List<ItemData> listOfTab4Item = new List<ItemData>();
    public List<ItemData> listOfTab5Item = new List<ItemData>();
    public string tickerText;
}

/// <summary>
/// DataManager.cs -> arrayOfItemDataByPriority
/// </summary>
[System.Serializable]
public class SavePriorityInfo
{
    public List<ItemData> listOfPriorityLow = new List<ItemData>();
    public List<ItemData> listOfPriorityMed = new List<ItemData>();
    public List<ItemData> listOfPriorityHigh = new List<ItemData>();
}

/// <summary>
/// ModalOutcomeDetails.cs (needed because there are certain outcome pipeline messages that are generated within a player's turn, eg. moving into a Nemesis district and suffering damage
/// </summary>
[System.Serializable]
public class SaveInfoPipeLineInfo
{
    public int modalLevel;
    public bool isAction;
    public string sideName;
    public string spriteName;
    public string textTop;
    public string textBottom;
    public string reason;
    public string help0;
    public string help1;
    public string help2;
    public string help3;
    public ModalSubState modalState;
    public MsgPipelineType type;
}

#endregion 


#region List Wrappers ...
//
// - - - List Wrappers (for nested Lists)
//

/// <summary>
/// Generic list wrapper class for serializing lists within lists. Use -> List<IntListWrapper> tempList = new List<IntListWrapper>()
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public class ListWrapper<T>
{
    public List<T> myList;

    public ListWrapper()
    { myList = new List<T>(); }
}

[System.Serializable]
public class IntListWrapper : ListWrapper<int> { }

[System.Serializable]
public class StringListWrapper : ListWrapper<string> { }

[System.Serializable]
public class ContactListWrapper : ListWrapper<Contact> { }
#endregion

