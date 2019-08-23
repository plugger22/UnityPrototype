using packageAPI;
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
        [HideInInspector] public int actorID;
        [HideInInspector] public int level;                     //1 (worst) to 3 (best). level 1 are start actors, level 2 are recruited, level 3 are special
        [HideInInspector] public int nodeCaptured;              //node where actor was captured (took an action), default '-1'
        [HideInInspector] public int unhappyTimer;              //used when in Reserves. Becomes 'Unhappy' once expires
        [HideInInspector] public int blackmailTimer;            //default 0 but set to new global value once actor gains Blackmailer condition
        [HideInInspector] public int captureTimer;              //ticks down and determines how long an actor will be inactive while captured
        [HideInInspector] public int numOfTimesBullied;         //tracked in order to calculate cost of bullying
        [HideInInspector] public int numOfTimesCaptured;        //chance of becoming a traitor increases for each time captured
        [HideInInspector] public int departedNumOfSecrets;      //used to record the number of secrets known at time of dismissal, etc. (needed to work out accurate renown cost as secrets removed when actor leaves)
        [HideInInspector] public bool isPromised;               //When sent to reserves Player can promise to recall them within a certain time (true), otherwise false
        [HideInInspector] public bool isNewRecruit;             //true if actor has been recruited, false if has been OnMap
        [HideInInspector] public bool isReassured;              //true if actor has been reassured, false if not (can only be reassured once)
        [HideInInspector] public bool isThreatening;            //true if actor has said they will be taking an action against player, eg. reveal secret, false otherwise
        [HideInInspector] public bool isComplaining;            //Action taken by actor in reserve pool. Can only complain once.
        [HideInInspector] public bool isBreakdown;              //set true when breakdown so that there is at least a one turn gap between successive breakdowns
        [HideInInspector] public bool isLieLowFirstturn;        //set true when lie low action, prevents invis incrementing on first turn
        [HideInInspector] public bool isStressLeave;            //set true to ensure actor spends one turn inactive on stress leave
        [HideInInspector] public bool isTraitor;                //set true to be a traitor (determined at time of release from captivity)
        [HideInInspector] public bool isMale;                   //set true if actor is a male
        [HideInInspector] public string actorName;              //complete name
        [HideInInspector] public string firstName;              //first name
        [HideInInspector] public ActorArc arc;
        [HideInInspector] public ActorTooltip tooltipStatus;     //Actor sprite shows a relevant tooltip if tooltipStatus > None (Stress leave, lying low, wants to talk, etc)
        [HideInInspector] public ActorInactive inactiveStatus;   //reason actor is inactive
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
        #endregion

        //cached trait effects (public for serialization reasons)
        [HideInInspector] public string actorStressNone;
        [HideInInspector] public string actorCorruptNone;
        [HideInInspector] public string actorUnhappyNone;
        [HideInInspector] public string actorBlackmailNone;
        [HideInInspector] public string actorBlackmailTimerHigh;
        [HideInInspector] public string actorBlackmailTimerLow;
        [HideInInspector] public int maxNumOfSecrets = -1;
        [HideInInspector] public int contactsPerLevel = -1;
        [HideInInspector] public int compatibilityOne;
        [HideInInspector] public int compatibilityTwo;
        [HideInInspector] public int compatibilityThree;

        //private backing field
        private ActorStatus _status;
        private int _renown;

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

        public int Renown
        {
            get { return _renown; }
            set
            {
                _renown = value;
                //need this here to prevent UpdateActorRenownUI choking on a negative renown
                _renown = Mathf.Max(0, _renown);
                //update renownUI regardless of whether it is on or off (check in place for load save game purposes as the RenownUI is updated later due to sequencing issues)
                if (slotID > -1)
                { GameManager.instance.actorPanelScript.UpdateActorRenownUI(slotID, _renown); }
            }
        }

        /// <summary>
        /// default constructor (called anytime an actor is created, even if it is assigned another actor's reference)
        /// </summary>
        public Actor()
        {
            nodeCaptured = -1;
            Renown = 0;
            gearName = null;
            gearTimer = 0;
            gearTimesTaken = 0;
            blackmailTimer = 0;
            numOfTimesBullied = 0;
            personality = new Personality();
            //call only if a new session
            if (GameManager.instance.isSession == false)
            {
                //fast access & cached
                actorStressNone = "ActorStressNone";
                actorCorruptNone = "ActorCorruptNone";
                actorUnhappyNone = "ActorUnhappyNone";
                actorBlackmailNone = "ActorBlackmailNone";
                actorBlackmailTimerHigh = "ActorBlackmailTimerHigh";
                actorBlackmailTimerLow = "ActorBlackmailTimerLow";
                maxNumOfSecrets = GameManager.instance.secretScript.secretMaxNum;
                contactsPerLevel = GameManager.instance.contactScript.contactsPerLevel;
                compatibilityOne = GameManager.instance.personScript.compatibilityChanceOne;
                compatibilityTwo = GameManager.instance.personScript.compatibilityChanceTwo;
                compatibilityThree = GameManager.instance.personScript.compatibilityChanceThree;
                Debug.AssertFormat(maxNumOfSecrets > -1, "Invalid maxNumOfSecrets (-1) for {0}", actorName);
                Debug.AssertFormat(contactsPerLevel > -1, "Invalid contactsPerLevel (-1) for {0}", actorName);
                Debug.AssertFormat(compatibilityOne > 0, "Invalid compatibilityOne (Zero) for {0}", actorName);
                Debug.AssertFormat(compatibilityTwo > 0, "Invalid compatibilityTwo (Zero) for {0}", actorName);
                Debug.AssertFormat(compatibilityThree > 0, "Invalid compatibilityThree (Zero) for {0}", actorName);
            }
        }

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
        }

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
                case ActorDatapoint.Connections0:
                case ActorDatapoint.Datapoint0:
                    value = datapoint0;
                    break;
                case ActorDatapoint.Motivation1:
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
        /// Set value of a datapoint. 'reasonForChange' needed if a Motivational shift that may be negated due to actor's compatibility with player. Ignore otherwise.
        /// Returns true if Motivation change accepted, false if actor compatibility negates it and true for all other datapoint cases (ignore, it's only for datapoint1, motivation)
        /// </summary>
        /// <param name="datapoint"></param>
        /// <param name="value"></param>
        public bool SetDatapoint(ActorDatapoint datapoint, int value, string reasonForChange = "Unknown")
        {
            bool isSuccess = true;
            string text;
            int turn = GameManager.instance.turnScript.Turn;
            switch (datapoint)
            {
                case ActorDatapoint.Influence0:
                case ActorDatapoint.Connections0:
                case ActorDatapoint.Datapoint0:
                    if ((value - datapoint0) == 0)
                    { Debug.LogWarningFormat("SetDatapoint change Datapoint0 has same value as already present for {0}, {1}, ID {2}", actorName, arc.name, actorID); }
                    datapoint0 = value;
                    break;
                case ActorDatapoint.Motivation1:
                case ActorDatapoint.Datapoint1:
                    //player side actor, take into account actor's compatibility with the player
                    if (side.level == GameManager.instance.sideScript.PlayerSide.level)
                    {
                        //doesn't apply on first turn (creating actors)
                        if (turn > 0)
                        {
                            //motivation is special as actor compatibility with player can negate the change
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
                                    //Motivational shift negated due to compatibility
                                    text = string.Format("{0}, {1}, ID {2}, negates Motivational change of {3}{4} due to compatibility with Player{5}", actorName, arc.name, actorID,
                                        difference > 0 ? "+" : "", difference, "\n");
                                    GameManager.instance.messageScript.ActorCompatibility(text, this, difference, reasonForChange);
                                    isSuccess = false;
                                    //Stats
                                    if (isBadOutcome == true)
                                    { GameManager.instance.dataScript.StatisticIncrement(StatType.ActorCompatibilityBad); }
                                    else { GameManager.instance.dataScript.StatisticIncrement(StatType.ActorCompatibilityGood); }
                                }
                                //random roll message regardless, provided a check was made
                                if (numNeeded > 0)
                                {
                                    text = string.Format("{0}, {1}, Compatibility check {2}", actorName, arc.name, isProceed == true ? "FAILED" : "SUCCESS");
                                    GameManager.instance.messageScript.GeneralRandom(text, "Compatibility", numNeeded, rndNum, isBadOutcome);
                                    Debug.LogFormat("[Rnd] Actor.cs -> SetDatapoint: {0} need < {1}, rolled {2}{3}", isProceed == true ? "FAILED" : "SUCCESS", numNeeded, rndNum, "\n");
                                }
                                //motivational History
                                HistoryMotivation history = new HistoryMotivation();
                                history.change = value;
                                history.turn = turn;
                                history.motivation = datapoint1;
                                history.isNormal = isProceed;
                                text = string.Format("{0} {1}{2}", reasonForChange, difference > 0 ? "+" : "", difference);
                                if (isProceed == false)
                                { history.descriptor = GameManager.instance.colourScript.GetFormattedString(text, ColourType.greyText); }
                                else
                                {
                                    if (difference > 0) { history.descriptor = GameManager.instance.colourScript.GetFormattedString(text, ColourType.goodText); }
                                    else { history.descriptor = GameManager.instance.colourScript.GetFormattedString(text, ColourType.badText); }
                                }
                                //add to list
                                personality.AddMotivation(history);
                            }
                            else { Debug.LogWarningFormat("SetDatapoint change Datapoint1 has same value as already present for {0}, {1}, ID {2}", actorName, arc.name, actorID); }
                        }
                        else
                        {
                            //turn 0, CreateActors
                            datapoint1 = value;
                            //Starting Motivation
                            HistoryMotivation history = new HistoryMotivation();
                            history.change = 0;
                            history.turn = turn;
                            history.motivation = datapoint1;
                            history.isNormal = true;
                            text = string.Format("Starting Motivation {0}", datapoint1);
                            history.descriptor = GameManager.instance.colourScript.GetFormattedString(text, ColourType.neutralText);
                            personality.AddMotivation(history);
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
        /// Set datapoint's using loaded save game data ONLY (straight assignment, no motivation compatibility checks)
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
                case ActorDatapoint.Connections0:
                case ActorDatapoint.Datapoint0:
                    datapoint0 = value;
                    break;
                case ActorDatapoint.Motivation1:
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
        public void ClearAllTeams()
        { listOfTeams.Clear(); }

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
        /// Removes contact at specified node, returns true if successful, false if not. Updates contact details
        /// </summary>
        /// <param name="nodeID"></param>
        public bool RemoveContact(int nodeID)
        {
            if (dictOfContacts.ContainsKey(nodeID) == true)
            {
                Contact contact = dictOfContacts[nodeID];
                if (contact != null)
                { 
                    //log
                    Debug.LogFormat("[Cnt] Actor.cs -> RemoveContact: Contact {0} {1}, {2}, ID {3}, at nodeID {4} Removed{5}", 
                        contact.nameFirst, contact.nameLast, contact.job, contact.contactID, contact.nodeID, "\n");
                    //update contact details
                    contact.actorID = -1;
                    contact.nodeID = -1;
                    contact.status = ContactStatus.Inactive;
                    contact.turnFinish = GameManager.instance.turnScript.Turn;

                }
                else { Debug.LogWarningFormat("Invalid contact (Null) for nodeID {0}", nodeID); }
            }
            //remove contact
            return dictOfContacts.Remove(nodeID);
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
                else { Debug.LogWarningFormat("Invalid contact {0} (\"{1}\") for nodeID {2}", dictOfContacts[nodeID].nameFirst, dictOfContacts[nodeID].status, nodeID); }
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

        /// <summary>
        /// returns true if actor has room for a new contact (less than max allowed which is datapoint * contactsPerLevel)
        /// </summary>
        /// <returns></returns>
        public bool CheckNewContactAllowed()
        { return (dictOfContacts.Count < (datapoint0 * contactsPerLevel)); }

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
                        { GameManager.instance.actorScript.TraitLogMessage(this, "for a become Stressed check", "to AVOID becoming Stressed"); }
                        else { proceedFlag = true; }
                        break;
                    case "CORRUPT":
                        if (CheckTraitEffect(actorCorruptNone) == true)
                        { GameManager.instance.actorScript.TraitLogMessage(this, "for a become Corrupt check", "to AVOID becoming Corrupt"); }
                        else { proceedFlag = true; }
                        break;
                    case "UNHAPPY":
                        if (CheckTraitEffect(actorUnhappyNone) == true)
                        { GameManager.instance.actorScript.TraitLogMessage(this, "for a become Unhappy check", "to AVOID becoming Unhappy"); }
                        else { proceedFlag = true; }
                        break;
                    case "BLACKMAILER":
                        //need to have at least one secret
                        if (listOfSecrets.Count == 0)
                        { Debug.LogWarning("Actor.cs -> AddCondition: BLACKMAIL condition NOT added (Actor has no Secrets)"); }
                        else
                        {
                            if (CheckTraitEffect(actorBlackmailNone) == true)
                            { GameManager.instance.actorScript.TraitLogMessage(this, "for a Blackmailer check", "to AVOID Blackmailing"); }
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
                                int timer = GameManager.instance.secretScript.secretBlackmailTimer;
                                //traits
                                if (CheckTraitEffect(actorBlackmailTimerHigh) == true)
                                {
                                    timer *= 3;
                                    GameManager.instance.actorScript.TraitLogMessage(this, "for a Blackmailer Timer check", "to TRIPLE Blackmail timer");
                                }
                                if (CheckTraitEffect(actorBlackmailTimerLow) == true)
                                {
                                    timer /= 2;
                                    timer = Mathf.Max(0, timer);
                                    GameManager.instance.actorScript.TraitLogMessage(this, "for a Blackmailer Timer check", "to HALVE Blackmail timer");
                                }
                                blackmailTimer = timer;
                                break;
                        }
                        Debug.LogFormat("[Cnd] Actor.cs -> AddCondition: {0}, {1} gained {2} condition{3}", actorName, arc.name, condition.tag, "\n");
                        //message
                        if (side.level == GameManager.instance.sideScript.PlayerSide.level)
                        {
                            string msgText = string.Format("{0} {1} gains condition \"{2}\"", arc.name, actorName, condition.tag);
                            GameManager.instance.messageScript.ActorCondition(msgText, actorID, true, condition, reason);
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
                            if (side.level == GameManager.instance.sideScript.PlayerSide.level)
                            {
                                //message
                                string msgText = string.Format("{0} {1} condition \"{2}\" removed", arc.name, actorName, condition.tag);
                                GameManager.instance.messageScript.ActorCondition(msgText, actorID, false, condition, reason);
                            }
                            return true;
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid condition (Null)"); }
            return false;
        }


        public List<Condition> GetListOfConditions()
        { return listOfConditions; }

        public int CheckNumOfConditions()
        { return listOfConditions.Count; }

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
                        GameManager.instance.messageScript.ActorSecret(msgText, this, secret);
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
                            Secret secret = GameManager.instance.dataScript.GetSecret(secretName);
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
            Condition condition = GameManager.instance.dataScript.GetCondition("BLACKMAILER");
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
                Gear gearOld = GameManager.instance.dataScript.GetGear(gearName);
                if (gearOld != null)
                {
                    text = gearOld.name;
                    Debug.LogFormat("[Gea] Actor.cs -> AddGear: {0} given to HQ by {1}{2}", gearOld.name, arc.name, "\n");
                    //gear has been lost
                    if (GameManager.instance.dataScript.RemoveGearLost(gearOld) == false)
                    { Debug.LogWarningFormat("Invalid gear Remove Lost for \"{0}\"", gearOld.name); }
                    //let player know that gear has been Lost
                    string msgText = string.Format("{0} ({1}), has been GIVEN TO HQ by {2}", gearOld.name, gearOld.type.name, arc.name);
                    GameManager.instance.messageScript.GearLost(msgText, gearOld, this, true);
                }
                else { Debug.LogWarningFormat("Invalid gear Old (Null) for gear {0}", gearName); }
            }
            //add new gear
            Gear gearNew = GameManager.instance.dataScript.GetGear(gearNewName);
            if (gearNew != null)
            {
                gearName = gearNewName;
                gearTimer = 0;
                Debug.LogFormat("[Gea] Actor.cs -> AddGear: {0} added to inventory of {1}{2}", gearNew.tag, arc.name, "\n");
                //add to listOfCurrentGear (if not already present)
                GameManager.instance.dataScript.AddGearNew(gearNew);
            }
            else { Debug.LogWarningFormat("Invalid gear (Null) for gear {0}", gearNewName); }
            //return name and type of any gear that was lost (existing prior to new gear being added)
            return text;
        }

        /// <summary>
        /// remove gear for various reasons. Msg only if gear present in first place. isTaken true if you are taking gear from actor, false (by default) otherwise
        /// </summary>
        public void RemoveGear(GearRemoved reason)
        {
            if (string.IsNullOrEmpty(gearName) == false)
            {
                Gear gear = GameManager.instance.dataScript.GetGear(gearName);
                if (gear != null)
                { Debug.LogFormat("[Gea] Actor.cs -> RemoveGear: {0} removed from inventory of {1}{2}", gear.tag, arc.name, "\n"); }
                else { Debug.LogWarningFormat("Invalid gear (Null) for gear {0}", gearName); }
                //remove gear AFTER logger
                gearName = null;
                gearTimer = 0;
                switch (reason)
                {
                    case GearRemoved.Lost:
                        if (GameManager.instance.dataScript.RemoveGearLost(gear) == false)
                        { Debug.LogWarningFormat("Invalid gear Remove Lost for \"{0}\", gear {1}", gear.tag, gear.name); }
                        break;
                    case GearRemoved.Taken:
                        gearTimesTaken++;
                        break;
                    case GearRemoved.Compromised:
                        if (GameManager.instance.dataScript.RemoveGearLost(gear) == false)
                        { Debug.LogWarningFormat("Invalid gear Remove Lost for \"{0}\", gear {1}", gear.tag, gear.name); }
                        break;
                    default:
                        Debug.LogErrorFormat("Unrecognised GearRemoved reason \"{0}\"", reason);
                        break;
                }
            }
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
            { gear = GameManager.instance.dataScript.GetGear(gearName); }
            ActorTooltipData data = new ActorTooltipData()
            {
                tooltipPos = position,
                actor = GameManager.instance.dataScript.GetCurrentActor(slotID, side),
                action = GameManager.instance.dataScript.GetActorAction(slotID, side),
                gear = gear,
                listOfSecrets = GetSecretsTooltipList(),
                arrayOfQualities = GameManager.instance.dataScript.GetQualities(side),
                arrayOfStats = GameManager.instance.dataScript.GetActorStats(slotID, side)
            };
            return data;
        }

        //
        // - - - Personality
        //

        public Personality GetPersonality()
        { return personality; }

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
                if (data.turn < 0 || data.turn > GameManager.instance.turnScript.Turn)
                { Debug.LogWarningFormat("Invalid NodeActionData turn \"{0}\" for {1}, {2}", data.turn, actorName, arc.name); }
                if (data.actorID < 0 || data.actorID > GameManager.instance.actorScript.actorIDCounter)
                { Debug.LogWarningFormat("Invalid NodeActionData actorID \"{0}\" for {1}, {2}", data.actorID, actorName, arc.name); }
                if (data.nodeID < 0 || data.nodeID > GameManager.instance.nodeScript.nodeIDCounter)
                { Debug.LogWarningFormat("Invalid NodeActionData nodeID \"{0}\" for {1}, {2}", data.nodeID, actorName, arc.name); }
                if (data.nodeAction == NodeAction.None)
                { Debug.LogWarningFormat("Invalid NodeActionData nodeAction \"{0}\" for {1}, {2}", data.nodeAction, actorName, arc.name); }
                //add to list
                listOfNodeActions.Add(data);
                Debug.LogFormat("[Tst] Actor.cs -> AddNodeAction: t {0}, actorID {1}, nodeID {2}, act {3}, data {4}{5}", data.turn, data.actorID, data.nodeID, data.nodeAction, data.dataName, "\n");
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
        /// Get most recent nodeActionData package (record at end of list)
        /// </summary>
        /// <returns></returns>
        public NodeActionData GetMostRecentNodeAction()
        { return listOfNodeActions[listOfNodeActions.Count - 1]; }

        /// <summary>
        /// Delete most recent nodeActionData package (record at end of list)
        /// </summary>
        public void RemoveMostRecentNodeAction()
        {
            int index = listOfNodeActions.Count - 1;
            Debug.LogFormat("[Tst] Actor.cs -> RemoveMostRecentNodeAction: {0}, {1}, nodeAction (District) \"{2}\" Removed{3}", actorName, arc.name, listOfNodeActions[index].nodeAction, "\n");
            listOfNodeActions.RemoveAt(index);
        }

        /// <summary>
        /// Empty out listOfNodeActions
        /// </summary>
        public void ClearAllNodeActions()
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
                if (data.turn < 0 || data.turn > GameManager.instance.turnScript.Turn)
                { Debug.LogWarningFormat("Invalid TeamActionData turn \"{0}\" for {1}, {2}", data.turn, actorName, arc.name); }
                if (data.actorID < 0 || data.actorID > GameManager.instance.actorScript.actorIDCounter)
                { Debug.LogWarningFormat("Invalid TeamActionData actorID \"{0}\" for {1}, {2}", data.actorID, actorName, arc.name); }
                if (data.nodeID < 0 || data.nodeID > GameManager.instance.nodeScript.nodeIDCounter)
                { Debug.LogWarningFormat("Invalid TeamActionData nodeID \"{0}\" for {1}, {2}", data.nodeID, actorName, arc.name); }
                if (data.teamAction == TeamAction.None)
                { Debug.LogWarningFormat("Invalid TeamActionData teamAction \"{0}\" for {1}, {2}", data.teamAction, actorName, arc.name); }
                //add to list
                listOfTeamActions.Add(data);
                Debug.LogFormat("[Tst] Actor.cs -> AddTeamAction: t {0}, actorID {1}, nodeID {2}, act {3}, data {4}{5}", data.turn, data.actorID, data.nodeID, data.teamAction, data.dataName, "\n");
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
            Debug.LogFormat("[Tst] Actor.cs -> RemoveMostRecentTeamAction: {0}, {1}, teamAction (District) \"{2}\" Removed{3}", actorName, arc.name, listOfTeamActions[index].teamAction, "\n");
            listOfTeamActions.RemoveAt(index);
        }

        /// <summary>
        /// Empty out listOfTeamActions
        /// </summary>
        public void ClearAllTeamActions()
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


        //place methods above here
    }
}
