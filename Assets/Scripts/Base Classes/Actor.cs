﻿using packageAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace gameAPI
{
    /// <summary>
    /// Actor class -> supporting minion for player
    /// </summary>
    public class Actor
    {

        #region Save Data compatible
        [HideInInspector] public GlobalSide side;
        [HideInInspector] public int slotID;                    //actor slot ID (eg, 0 to 3)
        [HideInInspector] public int actorID;                   //unique ID used for non-HQ actors
        [HideInInspector] public int hqID;                      //unique ID used for HQ actors, -1 default
        [HideInInspector] public int level;                     //1 (worst) to 3 (best). level 1 are start actors, level 2 are recruited, level 3 are special
        [HideInInspector] public int nodeCaptured;              //node where actor was captured (took an action), default '-1'
        //timers
        [HideInInspector] public int unhappyTimer;              //used when in Reserves. Becomes 'Unhappy' once expires
        [HideInInspector] public int blackmailTimer;            //default 0 but set to new global value once actor gains Blackmailer condition
        [HideInInspector] public int captureTimer;              //ticks down and determines how long an actor will be inactive while captured
        //states
        [HideInInspector] public bool isPromised;               //When sent to reserves Player can promise to recall them within a certain time (true), otherwise false
        [HideInInspector] public bool isNewRecruit;             //true if actor has been recruited, false if has been OnMap
        [HideInInspector] public bool isReassured;              //true if actor has been reassured, false if not (can only be reassured once)
        [HideInInspector] public bool isThreatening;            //true if actor has said they will be taking an action against player, eg. reveal secret, false otherwise
        [HideInInspector] public bool isComplaining;            //Action taken by actor in reserve pool. Can only complain once.
        [HideInInspector] public bool isBreakdown;              //set true when breakdown so that there is at least a one turn gap between successive breakdowns
        [HideInInspector] public bool isLieLowFirstturn;        //set true when lie low action, prevents invis incrementing on first turn
        [HideInInspector] public bool isStressLeave;            //set true to ensure actor spends one turn inactive on stress leave
        [HideInInspector] public bool isTraitor;                //set true to be a traitor (determined at time of release from captivity)
        [HideInInspector] public bool isDismissed;              //set true if at any time in the past actor has been dismissed
        [HideInInspector] public bool isResigned;               //set true if at any time in the past actor has resigned
        //other
        [HideInInspector] public string actorName;              //complete name
        [HideInInspector] public string firstName;              //first name
        [HideInInspector] public string backstory0;
        [HideInInspector] public string backstory1;
        [HideInInspector] public ActorArc arc;
        [HideInInspector] public ActorTooltip tooltipStatus;     //Actor sprite shows a relevant tooltip if tooltipStatus > None (Stress leave, lying low, wants to talk, etc)
        [HideInInspector] public ActorInactive inactiveStatus;   //reason actor is inactive
        [HideInInspector] public ActorHQ statusHQ;               //status if in HQ, otherwise 'None'
        [HideInInspector] public ActorSex sex;
        //stats
        [HideInInspector] public int numOfTimesBullied;         //tracked in order to calculate cost of bullying -> reserve specific
        [HideInInspector] public int numOfTimesReassured;       //reserve specific
        [HideInInspector] public int numOfTimesPromised;        //reserve specific
        [HideInInspector] public int numOfTimesCaptured;        //chance of becoming a traitor increases for each time captured
        [HideInInspector] public int numOfTimesBreakdown;       //tally of times actor suffered a breakdown
        [HideInInspector] public int numOfTimesStressLeave;     //tally of times actor took stress leave (one day duration)
        [HideInInspector] public int numOfTimesConflict;        //tally of number of relationship conflicts with the Player that the actor has had
        [HideInInspector] public int departedNumOfSecrets;      //used to record the number of secrets known at time of dismissal, etc. (needed to work out accurate power cost as secrets removed when actor leaves)
        [HideInInspector] public int numOfDaysStressed;         //tally of days spent stressed (excludes breakdown & stress leave days)
        [HideInInspector] public int numOfDaysUnhappy;          //reserve specific
        [HideInInspector] public int numOfDaysLieLow;           //tally of days spent lying low

        [HideInInspector] public int numOfVotesFor;             //tally of review votes in favour of player
        [HideInInspector] public int numOfVotesAbstained;       //tally of review votes -> abstained
        [HideInInspector] public int numOfVotesAgainst;         //tally of review votes against the player
        [HideInInspector] public int numOfDaysOnMap;            //tally of days OnMap
        [HideInInspector] public int numOfDaysReserves;         //tally of days in Reserves
        [HideInInspector] public int numOfCities;               //tally of cities where the actor has been OnMap or in the Reserves

        [HideInInspector] public int powerGainedAtHQ;           //total power gain while at HQ
        [HideInInspector] public int powerLostAtHQ;             //total power lost while at HQ

        //sprite
        [HideInInspector] [NonSerialized] public Sprite sprite;   //sprite used in-game (default copied from actorArc at present)
        [HideInInspector] public string spriteName;              //used for serialization (used to access sprite from dictOfSprites on load)
        //trait
        private Trait trait;
        private Personality personality;
        //datapoints
        private int datapoint0;                                                             //higher the number (1 to 3), see DM: arrayOfStats for string tags
        private int datapoint1;                                                             //higher the better (1 to 3)
        private int datapoint2;                                                             //higher the better (1 to 3)
        //gear
        private string gearName;                                                            //can only have one piece of gear at a time, default null
        private int gearTimer;                                                              //number of turns the actor has had the gear (NOTE: includes turn gear given as incremented at EndTurnEarly)
        private int gearTimesTaken;                                                         //tally of how many times player has taken gear from actor (harder to do so each time) NO GAME EFFECT AT PRESENT
        //collections
        private List<int> listOfTeams = new List<int>();                                    //teamID of all teams that the actor has currently deployed OnMap
        private List<Secret> listOfSecrets = new List<Secret>();                            //Player secrets that the actor knows
        private List<Condition> listOfConditions = new List<Condition>();                   //list of all conditions currently affecting the actor
        private List<string> listOfTraitEffects = new List<string>();                       //list of all traitEffect.teffID's       
        private Dictionary<int, Contact> dictOfContacts = new Dictionary<int, Contact>();   //key -> nodeID where contact is, Value -> contact
        private List<NodeActionData> listOfNodeActions = new List<NodeActionData>();        //Actor district topics
        private List<TeamActionData> listOfTeamActions = new List<TeamActionData>();        //Authority team topics
        private List<HqPowerData> listOfHqPowerData = new List<HqPowerData>();              //Hq actors only, tracks all changes to power
        private List<HistoryActor> listOfHistory = new List<HistoryActor>();                //tracks major events affecting actor, carries across levels
        #endregion

        //cached trait effects (public for serialization reasons)
        [HideInInspector] public string actorStressNone;
        [HideInInspector] public string actorCorruptNone;
        [HideInInspector] public string actorUnhappyNone;
        [HideInInspector] public string actorBlackmailNone;
        [HideInInspector] public string actorBlackmailTimerHigh;
        [HideInInspector] public string actorBlackmailTimerLow;
        [HideInInspector] public int maxNumOfSecrets = -1;
        [HideInInspector] public int maxContacts = -1;
        [HideInInspector] public int compatibilityOne;
        [HideInInspector] public int compatibilityTwo;
        [HideInInspector] public int compatibilityThree;


        //private backing field
        private ActorStatus _status;
        private int _power;

        #region Properties...

        public ActorStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                //remove gear
                if (_status != ActorStatus.Active && _status != ActorStatus.Inactive)
                {
                    if (string.IsNullOrEmpty(gearName) == false)
                    { RemoveGear(GearRemoved.Lost); }
                }
            }
        }

        public int Power
        {
            get { return _power; }
            set
            {
                _power = value;
                //need this here to prevent UpdateActorPowerUI choking on a negative power
                _power = Mathf.Max(0, _power);
                //update powerUI regardless of whether it is on or off (check in place for load save game purposes as the PowerUI is updated later due to sequencing issues)
                if (slotID > -1)
                { GameManager.i.actorPanelScript.UpdateActorPowerUI(slotID, _power); }
            }
        }
        #endregion

        #region Constructors...
        /// <summary>
        /// default constructor (called anytime an actor is created, even if it is assigned another actor's reference)
        /// </summary>
        public Actor()
        {
            hqID = -1;
            nodeCaptured = -1;
            Power = 0;
            gearName = null;
            gearTimer = 0;
            gearTimesTaken = 0;
            blackmailTimer = 0;
            captureTimer = 0;
            numOfTimesBullied = 0;
            personality = new Personality();
            //call only if a new session
            if (GameManager.i.isSession == false)
            {
                //fast access & cached
                actorStressNone = "ActorStressNone";
                actorCorruptNone = "ActorCorruptNone";
                actorUnhappyNone = "ActorUnhappyNone";
                actorBlackmailNone = "ActorBlackmailNone";
                actorBlackmailTimerHigh = "ActorBlackmailTimerHigh";
                actorBlackmailTimerLow = "ActorBlackmailTimerLow";
                maxNumOfSecrets = GameManager.i.secretScript.secretMaxNum;
                maxContacts = GameManager.i.contactScript.maxContactsPerActor;
                compatibilityOne = GameManager.i.personScript.compatibilityChanceOne;
                compatibilityTwo = GameManager.i.personScript.compatibilityChanceTwo;
                compatibilityThree = GameManager.i.personScript.compatibilityChanceThree;
                Debug.AssertFormat(maxNumOfSecrets > -1, "Invalid maxNumOfSecrets (-1) for {0}", actorName);
                Debug.AssertFormat(maxContacts > -1, "Invalid maxContacts (-1) for {0}", actorName);
                Debug.AssertFormat(compatibilityOne > 0, "Invalid compatibilityOne (Zero) for {0}", actorName);
                Debug.AssertFormat(compatibilityTwo > 0, "Invalid compatibilityTwo (Zero) for {0}", actorName);
                Debug.AssertFormat(compatibilityThree > 0, "Invalid compatibilityThree (Zero) for {0}", actorName);
            }
        }
        #endregion

        #region ResetStates
        /// <summary>
        /// reset all state indicators back to their defaults of false
        /// </summary>
        public void ResetStates()
        {
            isPromised = false;
            isNewRecruit = false;
            isReassured = false;
            isThreatening = false;
            isComplaining = false;
            isBreakdown = false;
            isLieLowFirstturn = false;
            isStressLeave = false;
            /*isTraitor = false;  //leave as is so traitor status can carry over between levels */
        }
        #endregion

        #region Datapoints...
        //
        // - - - Datapoints - - - 
        //

        /// <summary>
        /// get value of Datapoint, returns -1 if a problem
        /// </summary>
        /// <param name="datapoint"></param>
        /// <returns></returns>
        public int GetDatapoint(ActorDatapoint datapoint)
        {
            int value = -1;
            switch (datapoint)
            {
                case ActorDatapoint.Influence0:
                case ActorDatapoint.Contacts0:
                case ActorDatapoint.Datapoint0:
                    value = datapoint0;
                    break;
                case ActorDatapoint.Opinion1:
                case ActorDatapoint.Datapoint1:
                    value = datapoint1;
                    break;
                case ActorDatapoint.Ability2:
                case ActorDatapoint.Invisibility2:
                case ActorDatapoint.Datapoint2:
                    value = datapoint2;
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised ActorDatapoint \"{0}\"", datapoint);
                    break;
            }
            return value;
        }

        /// <summary>
        /// Set value of a datapoint. 'reasonForChange' needed if an Opinion shift that may be negated due to actor's compatibility with player. Ignore otherwise.
        /// Returns true if Opinion change accepted, false if actor compatibility negates it and true for all other datapoint cases (ignore, it's only for datapoint1, opinion)
        /// </summary>
        /// <param name="datapoint"></param>
        /// <param name="value"></param>
        public bool SetDatapoint(ActorDatapoint datapoint, int value, string reasonForChange = "Unknown")
        {
            bool isSuccess = true;
            string text;
            int turn = GameManager.i.turnScript.Turn;
            switch (datapoint)
            {
                case ActorDatapoint.Influence0:
                case ActorDatapoint.Contacts0:
                case ActorDatapoint.Datapoint0:
                    if ((value - datapoint0) == 0)
                    { Debug.LogWarningFormat("SetDatapoint change Datapoint0 has same value as already present for {0}, {1}, ID {2}", actorName, arc.name, actorID); }
                    datapoint0 = value;
                    break;
                case ActorDatapoint.Opinion1:
                case ActorDatapoint.Datapoint1:
                    //player side actor, take into account actor's compatibility with the player
                    if (side.level == GameManager.i.sideScript.PlayerSide.level)
                    {
                        //doesn't apply on first turn (creating actors)
                        if (turn > 0)
                        {
                            //opinion is special as actor compatibility with player can negate the change
                            int difference = value - datapoint1;
                            int rndNum = Random.Range(0, 100);
                            int numNeeded = 0;
                            bool isProceed = true;
                            bool isBadOutcome = false;
                            if (difference != 0)
                            {
                                int compatibility = personality.GetCompatibilityWithPlayer();
                                switch (compatibility)
                                {
                                    case 3:
                                        if (difference < 0)
                                        {
                                            numNeeded = compatibilityThree;
                                            if (rndNum < compatibilityThree)
                                            {
                                                //negative shift is negated due to good relationship with the player
                                                isProceed = false;
                                            }
                                        }
                                        break;
                                    case 2:
                                        if (difference < 0)
                                        {
                                            numNeeded = compatibilityTwo;
                                            if (rndNum < compatibilityTwo)
                                            {
                                                //negative shift is negated due to good relationship with the player
                                                isProceed = false;
                                            }
                                        }
                                        break;
                                    case 1:
                                        if (difference < 0)
                                        {
                                            numNeeded = compatibilityOne;
                                            if (rndNum < compatibilityOne)
                                            {
                                                //negative shift is negated due to good relationship with the player
                                                isProceed = false;
                                            }
                                        }
                                        break;
                                    case 0:
                                        //do nothing 
                                        break;
                                    case -1:
                                        if (difference > 0)
                                        {
                                            numNeeded = compatibilityOne;
                                            if (rndNum < compatibilityOne)
                                            {
                                                //positive shift is negated due to poor relationship with the player
                                                isProceed = false;
                                                isBadOutcome = true;
                                            }
                                        }
                                        break;
                                    case -2:
                                        if (difference > 0)
                                        {
                                            numNeeded = compatibilityTwo;
                                            if (rndNum < compatibilityTwo)
                                            {
                                                //positive shift is negated due to poor relationship with the player
                                                isProceed = false;
                                                isBadOutcome = true;
                                            }
                                        }
                                        break;
                                    case -3:
                                        if (difference > 0)
                                        {
                                            numNeeded = compatibilityThree;
                                            if (rndNum < compatibilityThree)
                                            {
                                                //positive shift is negated due to poor relationship with the player
                                                isProceed = false;
                                                isBadOutcome = true;
                                            }
                                        }
                                        break;
                                    default:
                                        Debug.LogWarningFormat("Unrecognised compatibility \"{0}\" for {1}, {2}, ID {3}", compatibility, actorName, arc.name, actorID);
                                        break;
                                }
                                //does the change hold?
                                if (isProceed == true)
                                { datapoint1 = value; }
                                else
                                {
                                    //Opinion shift negated due to compatibility
                                    text = string.Format("{0}, {1}, ID {2}, negates Opinion change of {3}{4} due to compatibility with Player{5}", actorName, arc.name, actorID,
                                        difference > 0 ? "+" : "", difference, "\n");
                                    GameManager.i.messageScript.ActorCompatibility(text, this, difference, reasonForChange);
                                    isSuccess = false;
                                    //Stats
                                    if (isBadOutcome == true)
                                    { GameManager.i.dataScript.StatisticIncrement(StatType.ActorCompatibilityBad); }
                                    else { GameManager.i.dataScript.StatisticIncrement(StatType.ActorCompatibilityGood); }
                                }
                                //random roll message regardless, provided a check was made
                                if (numNeeded > 0)
                                {
                                    text = string.Format("{0}, {1}, ignore Opinion {2}", actorName, arc.name, isProceed == true ? "FAILED" : "SUCCESS");
                                    GameManager.i.messageScript.GeneralRandom(text, "Compatibility", numNeeded, rndNum, isBadOutcome, "rand_0");
                                    Debug.LogFormat("[Rnd] Actor.cs -> SetDatapoint: {0} need < {1}, rolled {2} for datapoint {3}{4}", isProceed == true ? "FAILED" : "SUCCESS", numNeeded, rndNum, datapoint, "\n");
                                }
                                //opinion History
                                HistoryOpinion history = new HistoryOpinion();
                                history.change = value;
                                history.opinion = datapoint1;
                                history.isNormal = isProceed;
                                text = string.Format("{0} {1}{2}", reasonForChange, difference > 0 ? "+" : "", difference);
                                if (isProceed == false)
                                { history.descriptor = GameManager.Formatt(text, ColourType.greyText); }
                                else
                                {
                                    if (difference > 0) { history.descriptor = GameManager.Formatt(text, ColourType.goodText); }
                                    else { history.descriptor = GameManager.Formatt(text, ColourType.badText); }
                                }
                                //add to list
                                personality.AddOpinion(history);
                            }
                            else
                            {
                                /*if (Status != ActorStatus.HQ)
                                { Debug.LogWarningFormat("SetDatapoint change Datapoint1 has same value as already present for {0}, {1}, ID {2}", actorName, arc.name, actorID); }
                                else { Debug.LogWarningFormat("SetDatapoint change Datapoint1 has same value as already present for {0}, {1}, ID {2}, hqID {3}", actorName, 
                                    GameManager.instance.hqScript.GetHqTitle(statusHQ), actorID, hqID); }*/
                            }
                        }
                        else
                        {
                            //turn 0, CreateActors
                            datapoint1 = value;
                            //Starting Opinion
                            HistoryOpinion history = new HistoryOpinion();
                            history.change = 0;
                            history.opinion = datapoint1;
                            history.isNormal = true;
                            text = string.Format("Starting Opinion {0}, {1}", datapoint1, GameManager.i.cityScript.GetCityName());
                            history.descriptor = GameManager.Formatt(text, ColourType.neutralText);
                            personality.AddOpinion(history);
                        }
                    }
                    else
                    {
                        //non-Player side, do normally
                        datapoint1 = value;
                    }
                    break;
                case ActorDatapoint.Ability2:
                case ActorDatapoint.Invisibility2:
                case ActorDatapoint.Datapoint2:
                    /*if ((datapoint2 - value) == 0) EDIT -> Sometimes this happens with invisibility
                    { Debug.LogWarningFormat("SetDatapoint change Datapoint2 has same value as already present for {0}, {1}, ID {2}", actorName, arc.name, actorID); }*/
                    datapoint2 = value;
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised ActorDatapoint \"{0}\"", datapoint);
                    break;
            }
            return isSuccess;
        }

        /// <summary>
        /// Set datapoint's using loaded save game data ONLY (straight assignment, no opinion compatibility checks)
        /// </summary>
        /// <param name="datapoint"></param>
        /// <param name="value"></param>
        /// <param name="reasonForChange"></param>
        /// <returns></returns>
        public void SetDatapointLoad(ActorDatapoint datapoint, int value)
        {
            switch (datapoint)
            {
                case ActorDatapoint.Influence0:
                case ActorDatapoint.Contacts0:
                case ActorDatapoint.Datapoint0:
                    datapoint0 = value;
                    break;
                case ActorDatapoint.Opinion1:
                case ActorDatapoint.Datapoint1:
                    datapoint1 = value;
                    break;
                case ActorDatapoint.Ability2:
                case ActorDatapoint.Invisibility2:
                case ActorDatapoint.Datapoint2:
                    datapoint2 = value;
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised ActorDatapoint \"{0}\" in loaded save game data", datapoint);
                    break;
            }
        }
        #endregion

        #region Teams...
        //
        // - - - Teams - - -
        //

        /// <summary>
        /// Authority method -> returns true if actors 'Ability' allows for the deployment of another team OnMap
        /// </summary>
        /// <returns></returns>
        public bool CheckCanDeployTeam()
        {
            if (listOfTeams.Count < datapoint2)
            { return true; }
            return false;
        }

        public void AddTeam(int teamID)
        { listOfTeams.Add(teamID); }

        public void RemoveTeam(int teamID)
        { listOfTeams.Remove(teamID); }

        public int CheckNumOfTeams()
        { return listOfTeams.Count; }

        public List<int> GetListOfTeams()
        { return listOfTeams; }

        /// <summary>
        /// Empty out listOfTeams
        /// </summary>
        public void RemoveAllTeams()
        { listOfTeams.Clear(); }
        #endregion

        #region Contacts...
        //
        // - - - Contacts - - -
        //

        public Dictionary<int, Contact> GetDictOfContacts()
        { return dictOfContacts; }

        /// <summary>
        /// Add contact to dictOfContacts (ContactManager.cs -> AssignContact updates contact details)
        /// </summary>
        /// <param name="contact"></param>
        public void AddContact(Contact contact)
        {
            if (contact != null)
            {
                try
                { dictOfContacts.Add(contact.nodeID, contact); }
                catch (ArgumentException)
                { Debug.LogErrorFormat("Invalid entry in dictOfContacts for contact {0}, ID {1}", contact.nameFirst, contact.contactID); }
            }
            else { Debug.LogError("Invalid contact (Null)"); }
        }

        /// <summary>
        /// Removes contact at specified node, returns true if successful, false if not. Updates contact details NOTE: Use DataManager.cs  -> RemoveContactSingle (it calls this method and does other stuff)
        /// </summary>
        /// <param name="nodeID"></param>
        public bool RemoveContact(int nodeID)
        {
            if (dictOfContacts.ContainsKey(nodeID) == true)
            {
                Contact contact = dictOfContacts[nodeID];
                if (contact != null)
                {
                    //update contact details
                    contact.actorID = -1;
                    contact.nodeID = -1;
                    contact.status = ContactStatus.Inactive;
                    contact.turnFinish = GameManager.i.turnScript.Turn;

                }
                else { Debug.LogWarningFormat("Invalid contact (Null) for nodeID {0}", nodeID); }
            }
            //remove contact
            return dictOfContacts.Remove(nodeID);
        }

        /// <summary>
        /// Remove all contacts, used when actor being sent back to recruit pool at end of a level (ActorManager.cs -> ProcessMetaActors)
        /// </summary>
        public void RemoveAllContacts()
        {
            dictOfContacts.Clear();
        }

        /// <summary>
        /// Used for when actor leaving Map, retain contact but make status inactive
        /// </summary>
        /// <param name="nodeID"></param>
        public void ChangeContactToInactive(int nodeID)
        {
            if (dictOfContacts.ContainsKey(nodeID) == true)
            {
                Contact contact = dictOfContacts[nodeID];
                if (contact != null)
                {
                    //update contact status
                    contact.status = ContactStatus.Inactive;
                    contact.timerInactive = 0;
                    contact.turnFinish = GameManager.i.turnScript.Turn;

                }
                else { Debug.LogWarningFormat("Invalid contact (Null) for nodeID {0}", nodeID); }
            }
        }

        /// <summary>
        /// returns an Actor Contact for a specific node, Null if not found. Contact must be status 'Active'
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public Contact GetContact(int nodeID)
        {
            Debug.Assert(nodeID > -1, "Invalid nodeID (less than Zero)");
            if (dictOfContacts.ContainsKey(nodeID) == true)
            {
                if (dictOfContacts[nodeID].status == ContactStatus.Active)
                { return dictOfContacts[nodeID]; }
                /*else { Debug.LogWarningFormat("Invalid contact {0} (\"{1}\") for nodeID {2}", dictOfContacts[nodeID].nameFirst, dictOfContacts[nodeID].status, nodeID); }*/
            }
            return null;
        }

        /// <summary>
        /// returns a sum of the effectiveness levels of all the actors contact
        /// </summary>
        /// <returns></returns>
        public int GetContactNetworkEffectiveness()
        {
            int tally = 0;
            foreach (var contact in dictOfContacts)
            {
                if (contact.Value != null)
                { tally += contact.Value.effectiveness; }
                else { Debug.LogError("Invalid contact (Null) in dictOfContacts"); }
            }
            return tally;
        }

        /// <summary>
        /// Returns a random contact from the actor's network of contacts (excludes any that have already on the 'ExclusionList' of contactID's and any non-ACTIVE Contacts). 
        /// Returns null if can't find any or a problem.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Contact GetRandomContact(List<int> exclusionList)
        {
            int count;
            Contact contact = null;
            if (exclusionList != null)
            {
                if (dictOfContacts.Count > 0)
                {
                    count = exclusionList.Count;
                    //create a  temp list of all contacts in network that aren't on the exclusion list
                    List<Contact> tempList = new List<Contact>();
                    foreach (var networkContact in dictOfContacts)
                    {
                        contact = networkContact.Value;
                        if (contact != null)
                        {
                            if (contact.status == ContactStatus.Active)
                            {
                                if (count > 0)
                                {
                                    if (exclusionList.Exists(x => x == contact.contactID) == false)
                                    {
                                        //add weighted contact
                                        for (int i = 0; i < contact.effectiveness; i++)
                                        { tempList.Add(contact); }
                                    }
                                }
                                else
                                {
                                    //add weighted contact
                                    for (int i = 0; i < contact.effectiveness; i++)
                                    { tempList.Add(contact); }
                                }
                            }
                        }
                        else { Debug.LogError("Invalid contact (Null)"); }
                    }
                    //randomly choose one contact from list and return
                    if (tempList.Count > 0)
                    { contact = tempList[Random.Range(0, tempList.Count)]; }
                }
                else { Debug.LogFormat("{0}, {1}, id {2} has NO CONTACTS{3}", actorName, arc.name, actorID, "\n"); }
            }
            else { Debug.LogWarning("Invalid exclusionList (Null)"); }
            return contact;
        }

        /// <summary>
        /// Returns a random, active, contact without the need for an exclusion list of weighting of contacts in a selection pool. Returns null if none found.
        /// </summary>
        /// <returns></returns>
        public Contact GetRandomContact()
        {
            Contact contact = null;
            List<Contact> tempList = new List<Contact>();
            foreach (var networkContact in dictOfContacts)
            {
                contact = networkContact.Value;
                if (contact != null)
                {
                    if (contact.status == ContactStatus.Active)
                    { tempList.Add(contact); }
                }
            }
            if (tempList.Count > 0)
            { contact = tempList[Random.Range(0, tempList.Count)]; }
            return contact;
        }

        /// <summary>
        /// returns number of contacts, if any
        /// </summary>
        /// <returns></returns>
        public int CheckNumOfContacts()
        { return dictOfContacts.Count; }

        /*/// <summary>
        /// returns true if actor has room for a new contact (less than max allowed which is datapoint * contactsPerLevel)
        /// </summary>
        /// <returns></returns>
        public bool CheckNewContactAllowed()
        { return (dictOfContacts.Count < (datapoint0 * contactsPerLevel)); }*/

        /// <summary>
        /// returns true if actor has room for a new contact
        /// </summary>
        /// <returns></returns>
        public bool CheckNewContactAllowed()
        { return (dictOfContacts.Count < maxContacts); }

        /// <summary>
        /// Returns true if actor has at least one Active contact present, false otherwise
        /// </summary>
        /// <returns></returns>
        public bool CheckIfActiveContact()
        {
            foreach (var contact in dictOfContacts)
            {
                if (contact.Value != null)
                {
                    if (contact.Value.status == ContactStatus.Active)
                    { return true; }
                }
            }
            return false;
        }
#endregion

        #region Conditions...
        //
        // - - - Conditions - - -
        //

        /// <summary>
        /// Add a new condition to list provided it isn't already present. Returns true is successful
        /// </summary>
        /// <param name="condition"></param>
        public bool AddCondition(Condition condition, string reason)
        {
            bool proceedFlag = false;
            if (condition != null)
            {
                //keep going if reason not provided
                if (string.IsNullOrEmpty(reason) == true)
                {
                    reason = "Unknown";
                    Debug.LogWarning("Invalid reason (Null or Empty)");
                }
                //Checks (traits and other)
                switch (condition.tag)
                {
                    case "STRESSED":
                        if (CheckTraitEffect(actorStressNone) == true)
                        { GameManager.i.actorScript.TraitLogMessage(this, "for a become Stressed check", "to AVOID becoming Stressed"); }
                        else { proceedFlag = true; }
                        break;
                    case "CORRUPT":
                        if (CheckTraitEffect(actorCorruptNone) == true)
                        { GameManager.i.actorScript.TraitLogMessage(this, "for a become Corrupt check", "to AVOID becoming Corrupt"); }
                        else { proceedFlag = true; }
                        break;
                    case "UNHAPPY":
                        if (CheckTraitEffect(actorUnhappyNone) == true)
                        { GameManager.i.actorScript.TraitLogMessage(this, "for a become Unhappy check", "to AVOID becoming Unhappy"); }
                        else { proceedFlag = true; }
                        break;
                    case "BLACKMAILER":
                        //need to have at least one secret
                        if (listOfSecrets.Count == 0)
                        { Debug.LogWarning("Actor.cs -> AddCondition: BLACKMAIL condition NOT added (Actor has no Secrets)"); }
                        else
                        {
                            if (CheckTraitEffect(actorBlackmailNone) == true)
                            { GameManager.i.actorScript.TraitLogMessage(this, "for a Blackmailer check", "to AVOID Blackmailing"); }
                            else { proceedFlag = true; }
                        }
                        break;
                    default:
                        //All O.K
                        proceedFlag = true;
                        break;
                }
                if (proceedFlag == true)
                {
                    //check that condition isn't already present
                    if (CheckConditionPresent(condition) == false)
                    {
                        listOfConditions.Add(condition);
                        //special conditions
                        switch (condition.tag)
                        {
                            case "BLACKMAILER":
                                //actor is threatening player
                                isThreatening = true;
                                //blackmail timer
                                int timer = GameManager.i.secretScript.secretBlackmailTimer;
                                //traits
                                if (CheckTraitEffect(actorBlackmailTimerHigh) == true)
                                {
                                    timer *= 3;
                                    GameManager.i.actorScript.TraitLogMessage(this, "for a Blackmailer Timer check", "to TRIPLE Blackmail timer");
                                }
                                if (CheckTraitEffect(actorBlackmailTimerLow) == true)
                                {
                                    timer /= 2;
                                    timer = Mathf.Max(0, timer);
                                    GameManager.i.actorScript.TraitLogMessage(this, "for a Blackmailer Timer check", "to HALVE Blackmail timer");
                                }
                                blackmailTimer = timer;
                                break;
                        }
                        Debug.LogFormat("[Cnd] Actor.cs -> AddCondition: {0}, {1} gained {2} condition{3}", actorName, arc.name, condition.tag, "\n");
                        //message
                        if (side.level == GameManager.i.sideScript.PlayerSide.level)
                        {
                            string msgText = string.Format("{0} {1} gains condition \"{2}\"", arc.name, actorName, condition.tag);
                            GameManager.i.messageScript.ActorCondition(msgText, actorID, true, condition, reason);
                        }
                        //history
                        switch (condition.tag)
                        {
                            case "STRESSED":
                                AddHistory(new HistoryActor() { text = string.Format("Has become {0}{1} ({2})", condition.isNowA == true ? "a " : "", condition.tag, reason) });
                                break;
                            case "STAR":
                            case "BLACKMAILER":
                                AddHistory(new HistoryActor() { text = string.Format("Is now a {0}{1} ({2})", condition.isNowA == true ? "a " : "", condition.tag, reason) });
                                break;
                            case "QUESTIONABLE":
                            case "INCOMPETENT":
                                AddHistory(new HistoryActor() { text = string.Format("Declared {0}{1} ({2})", condition.isNowA == true ? "a " : "", condition.tag, reason) });
                                break;
                            default:
                                AddHistory(new HistoryActor() { text = string.Format("Is now {0}{1} ({2})", condition.isNowA == true ? "a " : "", condition.tag, reason) });

                                break;
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid condition (Null)"); }
            return proceedFlag;
        }

        /// <summary>
        /// Checks if actor has a specified condition 
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool CheckConditionPresent(Condition condition)
        {
            if (condition != null)
            {
                if (listOfConditions.Count > 0)
                {
                    foreach (Condition checkCondition in listOfConditions)
                    {
                        if (checkCondition.tag.Equals(condition.tag, StringComparison.Ordinal) == true)
                        { return true; }
                    }
                }
            }
            else { Debug.LogError("Invalid condition (Null)"); }
            return false;
        }

        /// <summary>
        /// Removes a specified condition if present
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool RemoveCondition(Condition condition, string reason)
        {
            if (condition != null)
            {
                if (listOfConditions.Count > 0)
                {
                    //keep going if reason not provided
                    if (string.IsNullOrEmpty(reason) == true)
                    {
                        reason = "Unknown";
                        Debug.LogWarning("Invalid reason (Null or Empty)");
                    }
                    //reverse loop -> delete and return if found
                    for (int i = listOfConditions.Count - 1; i >= 0; i--)
                    {
                        if (listOfConditions[i].name.Equals(condition.tag, StringComparison.Ordinal) == true)
                        {
                            //special cases
                            switch (condition.tag)
                            {
                                case "BLACKMAILER":
                                    blackmailTimer = 0;
                                    isThreatening = false;
                                    break;
                            }
                            listOfConditions.RemoveAt(i);
                            Debug.LogFormat("[Cnd] Actor.cs -> RemoveCondition: {0}, {1} lost {2} condition{3}", actorName, arc.name, condition.tag, "\n");
                            if (side.level == GameManager.i.sideScript.PlayerSide.level)
                            {
                                //message
                                string msgText = string.Format("{0} {1} condition \"{2}\" removed", arc.name, actorName, condition.tag);
                                GameManager.i.messageScript.ActorCondition(msgText, actorID, false, condition, reason);
                                //history
                                AddHistory( new HistoryActor() { text = string.Format("Is no longer {0}{1}", condition.isNowA == true ? "a " : "", condition.tag, reason) });
                            }
                            return true;
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid condition (Null)"); }
            return false;
        }

        /// <summary>
        /// Used for when an actor is promoted to HQ only, quick and dirty
        /// </summary>
        public void RemoveAllConditions()
        { listOfConditions.Clear(); }


        public List<Condition> GetListOfConditions()
        { return listOfConditions; }

        public int CheckNumOfConditions()
        { return listOfConditions.Count; }
        #endregion

        #region Secrets...
        //
        // - - - Secrets - - -
        //

        public int CheckNumOfSecrets()
        { return listOfSecrets.Count; }

        public List<Secret> GetListOfSecrets()
        { return listOfSecrets; }

        public bool CheckSecretPresent(string secretName)
        {
            if (string.IsNullOrEmpty(secretName) == false)
            { return listOfSecrets.Exists(x => x.name.Equals(secretName, StringComparison.Ordinal)); }
            else
            {
                Debug.LogError("Invalid secretName (Null)");
                return false;
            }
        }

        public void RemoveAllSecrets()
        { listOfSecrets.Clear(); }

        /// <summary>
        /// Add a Player secret, checks for duplicates and won't add if one found (warning msg). Checks also that total number of secrets known doesn't exceed max (not added if so)
        /// </summary>
        /// <param name="secret"></param>
        public void AddSecret(Secret secret)
        {
            if (secret != null)
            {
                //check same secret doesn't already exist
                if (listOfSecrets.Exists(x => x.name.Equals(secret.name, StringComparison.Ordinal)) == false)
                {
                    //check space for a new secret
                    if (listOfSecrets.Count < maxNumOfSecrets)
                    {
                        //add secret
                        listOfSecrets.Add(secret);
                        //message
                        string msgText = string.Format("{0} learns of Secret ({1})", arc.name, secret.tag);
                        GameManager.i.messageScript.ActorSecret(msgText, this, secret);
                        Debug.LogFormat("[Sec] Actor.cs -> AddSecret: {0}, {1}, ID {2}, learns {3} secret, {4}{5}", actorName, arc.name, actorID, secret.tag, secret.name, "\n");
                    }
                    else { Debug.LogFormat("[Sec] Actor.cs -> AddSecret: Secret NOT added to {0}, {1}, ID {2} as no space available{3}", actorName, arc.name, actorID, "\n"); }
                }
                else { Debug.LogWarningFormat("Duplicate secret already in list, secret {0}", secret.name); }
            }
        }

        /// <summary>
        /// Remove a secret from listOfSecrets. Returns true if successful, false if not found
        /// </summary>
        /// <param name="secretID"></param>
        /// <returns></returns>
        public bool RemoveSecret(string secretName)
        {
            bool isSuccess = false;
            if (string.IsNullOrEmpty(secretName) == false)
            {

                if (CheckSecretPresent(secretName) == true)
                {
                    //reverse loop through and remove secret
                    for (int i = listOfSecrets.Count - 1; i >= 0; i--)
                    {
                        if (listOfSecrets[i].name.Equals(secretName, StringComparison.Ordinal) == true)
                        {
                            listOfSecrets.RemoveAt(i);
                            isSuccess = true;
                            //admin
                            Secret secret = GameManager.i.dataScript.GetSecret(secretName);
                            if (secret != null)
                            { Debug.LogFormat("[Sec] Actor.cs -> RemoveSecret: {0}, {1}, {2}, {3} secret REMOVED {4}{5}", actorName, arc.name, actorID, secret.tag, secret.name, "\n"); }
                            else { Debug.LogErrorFormat("Invalid secret (Null) for secret {0}", secret.name); }
                            break;
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid secretName (Null)"); }
            return isSuccess;
        }

        /// <summary>
        /// Returns a secret known by actor, if more than one present will randomly choose. Returns null if a problem.
        /// </summary>
        /// <returns></returns>
        public Secret GetSecret()
        {
            Secret secret = null;
            if (listOfSecrets != null)
            {
                int numOfSecrets = listOfSecrets.Count;
                switch (numOfSecrets)
                {
                    case 0:
                        //none available
                        Debug.LogWarning("Invalid listOfSecrets (Empty)");
                        break;
                    case 1:
                        //one secret only, return this one
                        secret = listOfSecrets[0];
                        break;
                    default:
                        //more than one, choose randomly
                        secret = listOfSecrets[Random.Range(0, numOfSecrets)];
                        break;
                }
            }
            else { Debug.LogWarning("Invalid listOfSecrets (Null)"); }
            return secret;
        }

        /// <summary>
        /// return a list of all secretsto DataManager.cs -> DipslaySecretData for Debug display
        /// </summary>
        /// <returns></returns>
        public string DebugDisplaySecrets()
        {
            StringBuilder builder = new StringBuilder();
            if (listOfSecrets.Count > 0)
            {
                foreach (Secret secret in listOfSecrets)
                { builder.AppendFormat("{0} {1}, {2} ({3}), status: {4}", "\n", secret.name, secret.name, secret.tag, secret.status); }
            }
            else { builder.AppendFormat("{0} No records", "\n"); }
            return builder.ToString();
        }

        /// <summary>
        /// returns a list (empty if none) of secret tags for the actor tooltip
        /// </summary>
        /// <returns></returns>
        public List<string> GetSecretsTooltipList()
        {
            List<string> listTooltip = new List<string>();
            foreach (Secret secret in listOfSecrets)
            { listTooltip.Add(secret.tag); }
            return listTooltip;
        }

        /// <summary>
        /// returns a random secret from the Actor's current list of secrets. Returns null if none present or a problem
        /// </summary>
        /// <returns></returns>
        public Secret GetRandomCurrentSecret()
        {
            Secret secret = null;
            int numOfSecrets = listOfSecrets.Count;
            Condition condition = GameManager.i.dataScript.GetCondition("BLACKMAILER");
            if (condition != null)
            {
                //can't have blackmailer trait as could be using secret
                if (CheckConditionPresent(condition) == false)
                {
                    if (numOfSecrets > 0)
                    { secret = listOfSecrets[Random.Range(0, numOfSecrets)]; }
                }
            }
            return secret;
        }
        #endregion

        #region Trait...
        //
        // - - -  Trait - - -
        //

        public Trait GetTrait()
        { return trait; }

        public List<string> GetListOfTraitEffects()
        { return listOfTraitEffects; }

        /// <summary>
        /// Replaces any existing trait and overwrites listofTraitEffects with new data. Max one trait at a time.
        /// </summary>
        /// <param name="trait"></param>
        public void AddTrait(Trait trait)
        {
            if (trait != null)
            {
                listOfTraitEffects.Clear();
                this.trait = trait;
                foreach (TraitEffect traitEffect in trait.listOfTraitEffects)
                { listOfTraitEffects.Add(traitEffect.name); }
            }
            else { Debug.LogError("Invalid trait (Null)"); }
        }

        /// <summary>
        /// returns true if a particular trait effect is present, false otherwise
        /// </summary>
        /// <param name="traitEffectID"></param>
        /// <returns></returns>
        public bool CheckTraitEffect(string effectName)
        {
            if (string.IsNullOrEmpty(effectName) == false)
            {
                if (listOfTraitEffects != null)
                { return listOfTraitEffects.Exists(x => x == effectName); }
                else { Debug.LogError("Invalid listOfTraitEffects (Null)"); }
            }
            return false;
        }
        #endregion

        #region Gear...
        //
        // - - - Gear - - -
        //

        /// <summary>
        /// Add gear. Max 1 piece of gear at a time. If gear already present, new gear overrides, old gear lost. Returns null if no msg (Msg is name & type of gear, if lost)
        /// </summary>
        /// <param name="gearID"></param>
        public string AddGear(string gearNewName)
        {
            Debug.AssertFormat(string.IsNullOrEmpty(gearNewName) == false, "Invalid gear {0}", gearNewName);
            string text = null;
            //existing gear?
            if (string.IsNullOrEmpty(gearName) == false)
            {
                Gear gearOld = GameManager.i.dataScript.GetGear(gearName);
                if (gearOld != null)
                {
                    text = gearOld.name;
                    Debug.LogFormat("[Gea] Actor.cs -> AddGear: {0} given to HQ by {1}{2}", gearOld.name, arc.name, "\n");
                    //gear has been lost
                    if (GameManager.i.dataScript.RemoveGearLost(gearOld) == false)
                    { Debug.LogWarningFormat("Invalid gear Remove Lost for \"{0}\"", gearOld.name); }
                    //let player know that gear has been Lost
                    string msgText = string.Format("{0} ({1}), has been GIVEN TO HQ by {2}", gearOld.name, gearOld.type.name, arc.name);
                    GameManager.i.messageScript.GearLost(msgText, gearOld, this, true);
                }
                else { Debug.LogWarningFormat("Invalid gear Old (Null) for gear {0}", gearName); }
            }
            //add new gear
            Gear gearNew = GameManager.i.dataScript.GetGear(gearNewName);
            if (gearNew != null)
            {
                gearName = gearNewName;
                gearTimer = 0;
                Debug.LogFormat("[Gea] Actor.cs -> AddGear: {0} added to inventory of {1}{2}", gearNew.tag, arc.name, "\n");
                //add to listOfCurrentGear (if not already present)
                GameManager.i.dataScript.AddGearNew(gearNew);
            }
            else { Debug.LogWarningFormat("Invalid gear (Null) for gear {0}", gearNewName); }
            //return name and type of any gear that was lost (existing prior to new gear being added)
            return text;
        }

        /// <summary>
        /// remove gear for various reasons. Msg only if gear present in first place. isTaken true if you are taking gear from actor, false (by default) otherwise. Returns true if successful
        /// </summary>
        public bool RemoveGear(GearRemoved reason)
        {
            if (string.IsNullOrEmpty(gearName) == false)
            {
                Gear gear = GameManager.i.dataScript.GetGear(gearName);
                if (gear != null)
                { Debug.LogFormat("[Gea] Actor.cs -> RemoveGear: {0} removed from inventory of {1}{2}", gear.tag, arc.name, "\n"); }
                else { Debug.LogWarningFormat("Invalid gear (Null) for gear {0}", gearName); }
                //remove gear AFTER logger
                gearName = null;
                gearTimer = 0;
                switch (reason)
                {
                    case GearRemoved.Taken:
                        gearTimesTaken++;
                        break;
                    case GearRemoved.Decision:
                    case GearRemoved.Lost:
                    case GearRemoved.Compromised:
                        if (GameManager.i.dataScript.RemoveGearLost(gear) == false)
                        { Debug.LogWarningFormat("Invalid gear Remove Lost for \"{0}\", gear {1}", gear.tag, gear.name); }
                        break;
                    case GearRemoved.RecruitPool:
                        break;
                    default:
                        Debug.LogErrorFormat("Unrecognised GearRemoved reason \"{0}\"", reason);
                        break;
                }
                return true;
            }
            return false;
        }


        public void IncrementGearTimer()
        { gearTimer++; }

        public int GetGearTimer()
        { return gearTimer; }

        public int GetGearTimesTaken()
        { return gearTimesTaken; }

        /// <summary>
        /// returns null if no gear
        /// </summary>
        /// <returns></returns>
        public string GetGearName()
        { return gearName; }

        /// <summary>
        /// used only by FileManager.cs to load in save game data
        /// </summary>
        /// <param name="gearID"></param>
        public void SetGear(string gearName)
        { this.gearName = gearName; }

        public void SetGearTimer(int gearTimer)
        { this.gearTimer = gearTimer; }

        public void SetGearTimesTaken(int timesTaken)
        { gearTimesTaken = timesTaken; }

        /// <summary>
        /// Resets gear (single item) at start of turn (ActorManager.cs -> CheckActiveResistanceActorsHuman)
        /// NOTE: Gear checked for Null by calling method
        /// </summary>
        /// <param name="gear"></param>
        public void ResetGearItem(Gear gear)
        {
            gear.timesUsed = 0;
            gear.reasonUsed = "";
            gear.isCompromised = false;
            gear.chanceOfCompromise = 0;
        }

        /// <summary>
        /// returns true if actor has gear, false otherwise
        /// </summary>
        /// <returns></returns>
        public bool CheckIfGear()
        {
            if (string.IsNullOrEmpty(gearName) == true)
            { return false; }
            return true;
        }
        #endregion

        #region Tooltip...
        //
        // - - - Tooltip - - -
        //

        /// <summary>
        /// returns data package for actor tooltip. Null if a problem
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public ActorTooltipData GetTooltipData(Vector3 position)
        {
            Gear gear = null;
            if (string.IsNullOrEmpty(gearName) == false)
            { gear = GameManager.i.dataScript.GetGear(gearName); }
            ActorTooltipData data = new ActorTooltipData()
            {
                tooltipPos = position,
                //actor = GameManager.instance.dataScript.GetCurrentActor(slotID, side),
                //action = GameManager.instance.dataScript.GetActorAction(slotID, side),
                actor = this,
                action = arc.nodeAction,
                gear = gear,
                trait = trait,
                listOfSecrets = GetSecretsTooltipList(),
                arrayOfQualities = GameManager.i.dataScript.GetQualities(side),
                arrayOfStats = GameManager.i.dataScript.GetActorStats(slotID, side)
            };
            return data;
        }
        #endregion

        #region Personality...
        //
        // - - - Personality
        //

        public Personality GetPersonality()
        { return personality; }
        #endregion

        #region Node Actions...
        //
        // - - - Node Actions (Both sides)
        //

        public List<NodeActionData> GetListOfNodeActions()
        { return listOfNodeActions; }

        /// <summary>
        /// add a nodeActionData package to list
        /// </summary>
        /// <param name="data"></param>
        public void AddNodeAction(NodeActionData data)
        {
            if (data != null)
            {
                //validate data
                if (data.turn < 0 || data.turn > GameManager.i.turnScript.Turn)
                { Debug.LogWarningFormat("Invalid NodeActionData turn \"{0}\" for {1}, {2}", data.turn, actorName, arc.name); }
                if (data.actorID < 0 || data.actorID > GameManager.i.actorScript.actorIDCounter)
                { Debug.LogWarningFormat("Invalid NodeActionData actorID \"{0}\" for {1}, {2}", data.actorID, actorName, arc.name); }
                if (data.nodeID < 0 || data.nodeID > GameManager.i.nodeScript.nodeIDCounter)
                { Debug.LogWarningFormat("Invalid NodeActionData nodeID \"{0}\" for {1}, {2}", data.nodeID, actorName, arc.name); }
                if (data.nodeAction == NodeAction.None)
                { Debug.LogWarningFormat("Invalid NodeActionData nodeAction \"{0}\" for {1}, {2}", data.nodeAction, actorName, arc.name); }
                //add to list
                listOfNodeActions.Add(data);
                /*Debug.LogFormat("[Tst] Actor.cs -> AddNodeAction: t {0}, actorID {1}, nodeID {2}, act {3}, data {4}{5}", data.turn, data.actorID, data.nodeID, data.nodeAction, data.dataName, "\n");*/
            }
            else { Debug.LogError("Invalid nodeDataAction (Null)"); }
        }

        /// <summary>
        /// get number of nodeActionData records in list
        /// </summary>
        /// <returns></returns>
        public int CheckNumOfNodeActions()
        { return listOfNodeActions.Count; }

        /// <summary>
        /// Get most recent nodeActionData package (record at head of list)
        /// </summary>
        /// <returns></returns>
        public NodeActionData GetMostRecentNodeAction()
        { return listOfNodeActions[0]; }

        /// <summary>
        /// Delete most recent nodeActionData package (record at head of list)
        /// </summary>
        public void RemoveMostRecentNodeAction()
        {
            if (listOfNodeActions.Count > 0)
            {
                int index = 0;
                /*Debug.LogFormat("[Tst] Actor.cs -> RemoveMostRecentNodeAction: {0}, {1}, nodeAction (District) \"{2}\" Removed{3}", actorName, arc.name, listOfNodeActions[index].nodeAction, "\n");*/
                listOfNodeActions.RemoveAt(index);
            }
            else { Debug.LogWarning("Invalid listOfNodeActions (Empty)"); }
        }

        /// <summary>
        /// Empty out listOfNodeActions
        /// </summary>
        public void RemoveAllNodeActions()
        { listOfNodeActions.Clear(); }

        /// <summary>
        /// set listOfNodeActionData from saved load game data. Clears any existing data beforehand.
        /// </summary>
        /// <param name="listOfData"></param>
        public void SetNodeActionData(List<NodeActionData> listOfData)
        {
            if (listOfData != null)
            {
                listOfNodeActions.Clear();
                listOfNodeActions.AddRange(listOfData);
            }
            else { Debug.LogError("Invalid listOfData (Null)"); }
        }
        #endregion

        #region Team Actions...
        //
        // - - - Team Actions (Authority actors only)
        //

        public List<TeamActionData> GetListOfTeamActions()
        { return listOfTeamActions; }

        /// <summary>
        /// add a teamActionData package to list
        /// </summary>
        /// <param name="data"></param>
        public void AddTeamAction(TeamActionData data)
        {
            if (data != null)
            {
                //validate data
                if (data.turn < 0 || data.turn > GameManager.i.turnScript.Turn)
                { Debug.LogWarningFormat("Invalid TeamActionData turn \"{0}\" for {1}, {2}", data.turn, actorName, arc.name); }
                if (data.actorID < 0 || data.actorID > GameManager.i.actorScript.actorIDCounter)
                { Debug.LogWarningFormat("Invalid TeamActionData actorID \"{0}\" for {1}, {2}", data.actorID, actorName, arc.name); }
                if (data.nodeID < 0 || data.nodeID > GameManager.i.nodeScript.nodeIDCounter)
                { Debug.LogWarningFormat("Invalid TeamActionData nodeID \"{0}\" for {1}, {2}", data.nodeID, actorName, arc.name); }
                if (data.teamAction == TeamAction.None)
                { Debug.LogWarningFormat("Invalid TeamActionData teamAction \"{0}\" for {1}, {2}", data.teamAction, actorName, arc.name); }
                //add to list
                listOfTeamActions.Add(data);
                /*Debug.LogFormat("[Tst] Actor.cs -> AddTeamAction: t {0}, actorID {1}, nodeID {2}, act {3}, data {4}{5}", data.turn, data.actorID, data.nodeID, data.teamAction, data.dataName, "\n");*/
            }
            else { Debug.LogError("Invalid teamDataAction (Null)"); }
        }

        /// <summary>
        /// get number of teamActionData records in list
        /// </summary>
        /// <returns></returns>
        public int CheckNumOfTeamActions()
        { return listOfTeamActions.Count; }

        /// <summary>
        /// Get most recent teamActionData package (record at end of list)
        /// </summary>
        /// <returns></returns>
        public TeamActionData GetMostRecentTeamAction()
        { return listOfTeamActions[listOfTeamActions.Count - 1]; }

        /// <summary>
        /// Delete most recent teamActionData package (record at end of list)
        /// </summary>
        public void RemoveMostRecentTeamAction()
        {
            int index = listOfTeamActions.Count - 1;
            /*Debug.LogFormat("[Tst] Actor.cs -> RemoveMostRecentTeamAction: {0}, {1}, teamAction (District) \"{2}\" Removed{3}", actorName, arc.name, listOfTeamActions[index].teamAction, "\n");*/
            listOfTeamActions.RemoveAt(index);
        }

        /// <summary>
        /// Empty out listOfTeamActions
        /// </summary>
        public void RemoveAllTeamActions()
        { listOfTeamActions.Clear(); }


        /// <summary>
        /// set listOfTeamActionData from saved load game data. Clears any existing data beforehand.
        /// </summary>
        /// <param name="listOfData"></param>
        public void SetTeamActionData(List<TeamActionData> listOfData)
        {
            if (listOfData != null)
            {
                listOfTeamActions.Clear();
                listOfTeamActions.AddRange(listOfData);
            }
            else { Debug.LogError("Invalid listOfData (Null)"); }
        }
        #endregion

        #region HQ Actors...
        //
        // - - - HQ actors
        //

        public List<HqPowerData> GetListOfHqPowerData()
        { return listOfHqPowerData; }

        /// <summary>
        /// Adds data record of HQ actor's power change, returns true if successful, false if not
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool AddHqPowerData(HqPowerData data)
        {
            if (data != null)
            {
                listOfHqPowerData.Add(data);
                //stats
                if (data.change > 0) { powerGainedAtHQ += data.change; }
                else { powerLostAtHQ += Mathf.Abs(data.change); }
                return true;
            }
            else { Debug.LogError("Invalid hqPowerData (Null)"); }
            return false;
        }

        /// <summary>
        /// Returns most recent hqPowerData, null if none
        /// </summary>
        /// <returns></returns>
        public HqPowerData GetMostRecentHqPowerData()
        {
            HqPowerData data = null;
            int count = listOfHqPowerData.Count;
            if (count > 0)
            { data = listOfHqPowerData[count - 1]; }
            return data;
        }

        /// <summary>
        /// refresh HQ power data list for Save/Load data
        /// </summary>
        /// <param name="tempList"></param>
        public void SetHqPowerData(List<HqPowerData> tempList)
        {
            if (tempList != null)
            {
                listOfHqPowerData.Clear();
                listOfHqPowerData.AddRange(tempList);
            }
            else { Debug.LogError("Invalid listOfHqPowerData (Null)"); }
        }
        #endregion

        #region History...
        //
        // - - - History
        //

        public List<HistoryActor> GetListOfHistory()
        { return listOfHistory; }

        /// <summary>
        /// Add history record
        /// </summary>
        /// <param name="data"></param>
        public void AddHistory(HistoryActor data)
        {
            if (data != null)
            { listOfHistory.Add(data); }
            else { Debug.LogError("Invalid HistoryActor data (Null)"); }
        }

        /// <summary>
        /// returns true if actor's listOfHistory has any records (actor has been used previously), false if none
        /// </summary>
        /// <returns></returns>
        public bool CheckHistory()
        { return listOfHistory.Count > 0 ? true : false; }

        /// <summary>
        /// returns most recent actor History entry, Null if none found
        /// </summary>
        /// <returns></returns>
        public HistoryActor GetMostRecentActorHistory()
        {
            HistoryActor history = null;
            int count = listOfHistory.Count;
            if (count > 0)
            { return listOfHistory[count - 1]; }
            return history;
        }

        /// <summary>
        /// Display Actor History
        /// </summary>
        /// <returns></returns>
        public string DebugDisplayHistory()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("- {0}, {1}, ID {2}, LvL {3}, Mot {4}, isD {5}, isR {6}{7}", actorName, arc.name, actorID, level, GetDatapoint(ActorDatapoint.Opinion1), isDismissed, isResigned, "\n");
            int count = listOfHistory.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    HistoryActor data = listOfHistory[i];
                    builder.AppendFormat(" {0}, t{1}: {2}{3}", data.cityTag, data.turn, data.text, "\n");
                }
            }
            else { builder.AppendFormat(" No records"); }
            return builder.ToString();
        }

        /// <summary>
        /// refresh Actor History for Save/Load data
        /// </summary>
        /// <param name="tempList"></param>
        public void SetHistory(List<HistoryActor> tempList)
        {
            if (tempList != null)
            {
                listOfHistory.Clear();
                listOfHistory.AddRange(tempList);
            }
            else { Debug.LogError("Invalid listOfHistory (Null)"); }
        }
        #endregion

        //place methods above here
    }
}
