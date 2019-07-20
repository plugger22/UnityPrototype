using gameAPI;
using packageAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Master serializable class containing all game data for save / load file operations
/// NOTE: does NOT derive from Monobehaviour
/// </summary>
[System.Serializable]
public class Save
{
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
    public SaveAIData aiData = new SaveAIData();
    public SaveScenarioData scenarioData = new SaveScenarioData();
    public SaveContactData contactData = new SaveContactData();
    public SaveTargetData targetData = new SaveTargetData();

}

#region Managers
//
// - - - Managers - - -
//

#region SaveCampaignData
/// <summary>
/// CampaignManager.cs data
/// </summary>
[System.Serializable]
public class SaveCampaignData
{
    public string campaignName;
    public int scenarioIndex;
    public int[] arrayOfStoryStatus;
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
    public List<string> listOfRevealedSecrets = new List<string>();
    public List<string> listOfDeletedSecrets = new List<string>();
    public List<SaveSecret> listOfSecretChanges = new List<SaveSecret>();
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
    public List<HistoryRebelMove> listOfHistoryRebel = new List<HistoryRebelMove>();
    public List<HistoryNemesisMove> listOfHistoryNemesis = new List<HistoryNemesisMove>();
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
    public List<TopicData> listOfTopicTypeValues = new List<TopicData>();
    public List<string> listOfTopicTypeKeys = new List<string>();
    public List<TopicData> listOfTopicSubTypeValues = new List<TopicData>();
    public List<string> listOfTopicSubTypeKeys = new List<string>();
    //Registers
    public List<EffectDataOngoing> listOfOngoingEffects = new List<EffectDataOngoing>();
    public List<ActionAdjustment> listOfActionAdjustments = new List<ActionAdjustment>();
    //moving
    public List<int> listOfMoveNodes = new List<int>();
    //MainInfo App data
    public SaveMainInfo currentInfo = new SaveMainInfo();
    public List<SavePriorityInfo> listOfPriorityData = new List<SavePriorityInfo>();
    public List<ItemData> listOfDelayedItemData = new List<ItemData>();
    public List<SaveMainInfo> listOfHistoryValue = new List<SaveMainInfo>();
    public List<int> listOfHistoryKey = new List<int>();
    
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
    public int mood;
    public ActorStatus status;
    public ActorTooltip tooltipStatus;
    public ActorInactive inactiveStatus;
    public bool isBreakdown;
    public bool isEndOfTurnGearCheck;
    public bool isLieLowFirstturn;   
    public bool isStressLeave;
    public bool isStressed;
    public int numOfSuperStress;
    public string profile;
    public string profileDescriptor;
    public string profileExplanation;
    public List<int> listOfPersonalityFactors = new List<int>();
    public List<string> listOfDescriptors = new List<string>();
    public List<string> listOfGear = new List<string>();
    public List<string> listOfSecrets = new List<string>();
    public List<string> listOfConditionsResistance = new List<string>();
    public List<string> listOfConditionsAuthority = new List<string>();
    public List<HistoryMood> listOfMoodHistory = new List<HistoryMood>();
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
    public WinState winStateLevel;
    public WinReason winReasonLevel;
    public AuthoritySecurityState authoritySecurity;
    public string currentSide;
    public bool haltExecution;
    public bool isSecurityFlash;
}
#endregion


#region SaveScenarioData
/// <summary>
/// City, Objective and Faction Manager.cs data
/// </summary>
[System.Serializable]
public class SaveScenarioData
{
    public int cityLoyalty;
    public int factionSupportAuthority;
    public int factionSupportResistance;
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
    public List<int> listOfActors = new List<int>();
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
    //actorManager.cs
    public int actorIDCounter;
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

#endregion


#region SO's
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
    public int gainedWhen;
    public int revealedWho;
    public int revealedWhen;
    public int deleteWhen;
    public gameAPI.SecretStatus status;
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

#endregion


#region Base Classes
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
    public int actorID;
    public int datapoint0;
    public int datapoint1;
    public int datapoint2;
    public string side;
    public int slotID;
    public int level;
    public int nodeCaptured;
    public string gearName;
    public bool isMale;
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

#endregion


#region SubClasses
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

#endregion 


#region List Wrappers
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

