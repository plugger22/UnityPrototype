﻿using gameAPI;
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
    public SaveSideData sideData = new SaveSideData();
    public SaveActorData actorData = new SaveActorData();
    public SaveNodeData nodeData = new SaveNodeData();
    public SaveNemesisData nemesisData = new SaveNemesisData();

}

//
// - - - Managers - - -
//

/// <summary>
/// CampaignManager.cs data
/// </summary>
[System.Serializable]
public class SaveCampaignData
{
    public int campaignID;
    public int scenarioIndex;
    public int[] arrayOfStoryStatus;
}


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


/// <summary>
/// DataManager.cs data
/// </summary>
[System.Serializable]
public class SaveDataData
{
    //secrets
    public List<int> listOfPlayerSecrets = new List<int>();
    public List<int> listOfRevealedSecrets = new List<int>();
    public List<int> listOfDeletedSecrets = new List<int>();
    public List<SaveSecret> listOfSecretChanges = new List<SaveSecret>();
    //contacts
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
    public List<SaveTeam> listOfTeams = new List<SaveTeam>();
    public List<int> listOfArrayOfTeams = new List<int>();
    public List<int> listOfTeamPoolReserve = new List<int>();
    public List<int> listOfTeamPoolOnMap = new List<int>();
    public List<int> listOfTeamPoolInTransit = new List<int>();

}


/// <summary>
/// PlayerManager.cs data
/// </summary>
[System.Serializable]
public class SavePlayerData
{
    public int renown;
    public int Invisibility;
    public ActorStatus status;
    public ActorTooltip tooltipStatus;
    public ActorInactive inactiveStatus;
    public bool isBreakdown;
    public bool isEndOfTurnGearCheck;
    public bool isLieLowFirstturn;   
    public bool isStressLeave;
    public List<int> listOfGear = new List<int>();
    public List<int> listOfSecrets = new List<int>();
    public List<string> listOfConditionsResistance = new List<string>();
    public List<string> listOfConditionsAuthority = new List<string>();   
}


/// <summary>
/// SideManager.cs data
/// </summary>
[System.Serializable]
public class SaveSideData
{
    public SideState resistanceCurrent;
    public SideState authorityCurrent;
    public SideState resistanceOverall;
    public SideState authorityOverall;
    public GlobalSide playerSide;
}

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
    //fast access fields for actor.cs
    public int actorStressNone;
    public int actorCorruptNone;
    public int actorUnhappyNone;
    public int actorBlackmailNone;
    public int actorBlackmailTimerHigh;
    public int actorBlackmailTimerLow;
    public int maxNumOfSecrets;     
}

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
}

/// <summary>
/// NemesisManager.cs
/// </summary>
[System.Serializable]
public class SaveNemesisData
{
    public NemesisSaveClass saveData;
}

//
// - - - SO's - - -
//

/// <summary>
/// Secret.S0 dynamic data
/// </summary>
[System.Serializable]
public class SaveSecret
{
    public int secretID;
    public int gainedWhen;
    public int revealedWho;
    public int revealedWhen;
    public int deleteWhen;
    public SecretStatus status;
    public List<int> listOfActors = new List<int>();
}

//
// - - - Base Classes - - -
//


/// <summary>
/// Actor.cs full data set
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
    public GlobalSide side;
    public int slotID;
    public int level;
    public int nodeCaptured;
    public int gearID;
    public bool isMale;
    public string actorName;
    public string firstName;
    public int arcID;
    public Trait trait;

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
    public List<int> listOfTeams = new List<int>();
    public List<int> listOfSecrets = new List<int>();
    public List<string> listOfConditions = new List<string>();   
    public List<int> listOfContactNodes = new List<int>();
    public List<int> listOfContacts = new List<int>();
    
    /*public List<int> listOfTraitEffects = new List<int>();*/   //trait effects generated dynamically when loading trait
}

/// <summary>
/// Node.cs
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

    public int targetID;

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
    public NodeCrisis crisis;

    public LoiterData loiter;
    public int cureID;
  
    public List<int> listOfTeams = new List<int>();                  
    public List<int> listOfOngoingEffects = new List<int>();
}

/// <summary>
/// Team.cs
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
public class ContactListWrapper : ListWrapper<Contact> { }