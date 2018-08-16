using packageAPI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace gameAPI
{
    /// <summary>
    /// Actor class -> supporting minion for player
    /// </summary>
    public class Actor
    {
        [HideInInspector] public int datapoint0;               //higher the number (1 to 3), see DM: arrayOfStats for string tags
        [HideInInspector] public int datapoint1;               //higher the better (1 to 3)
        [HideInInspector] public int datapoint2;               //higher the better (1 to 3)
        [HideInInspector] public GlobalSide side;
        [HideInInspector] public int actorSlotID;                    //actor slot ID (eg, 0 to 3)
        [HideInInspector] public int actorID;
        [HideInInspector] public int level;                     //1 (worst) to 3 (best). level 1 are start actors, level 2 are recruited, level 3 are special
        //[HideInInspector] public int Renown;                    //starts at '0' and goes up (no limit)
        [HideInInspector] public int nodeCaptured;              //node where actor was captured (took an action), default '-1'
        [HideInInspector] public int unhappyTimer;             //used when in Reserves. Becomes 'Unhappy' once expires
        [HideInInspector] public int blackmailTimer;            //default 0 but set to new global value once actor gains Blackmailer condition
        [HideInInspector] public bool isPromised;               //When sent to reserves Player can promise to recall them within a certain time (true), otherwise false
        [HideInInspector] public bool isNewRecruit;             //true if actor has been recruited, false if has been OnMap
        [HideInInspector] public bool isReassured;              //true if actor has been reassured, false if not (can only be reassured once)
        [HideInInspector] public bool isThreatening;            //true if actor has said they will be taking an action against player, eg. reveal secret, false otherwise
        [HideInInspector] public bool isComplaining;            //Action taken by actor in reserve pool. Can only complain once.
        [HideInInspector] public bool isBreakdown;              //set true when breakdown so that there is at least a one turn gap between successive breakdowns
        [HideInInspector] public bool isLieLowFirstturn;        //set true when lie low action, prevents invis incrementing on first turn
        [HideInInspector] public string actorName;
        [HideInInspector] public ActorArc arc;
        [HideInInspector] public ActorTooltip tooltipStatus;    //Actor sprite shows a relevant tooltip if tooltipStatus > None (Stress leave, lying low, wants to talk, etc)
        [HideInInspector] public ActorInactive inactiveStatus;  //reason actor is inactive


        //gear
        private int gearID;                                     //can only have one piece of gear at a time, default -1
        private int gearTimer;                                  //number of turns the actor has had the gear (NOTE: includes turn gear given as incremented at EndTurnEarly)
        private int gearTimesTaken;                             //tally of how many times player has taken gear from actor (harder to do so each time) NO GAME EFFECT AT PRESENT

        //cached trait effects
        private int actorStressNone;
        private int actorCorruptNone;
        private int actorUnhappyNone;
        private int actorBlackmailNone;
        private int actorBlackmailTimerHigh;
        private int actorBlackmailTimerLow;
        private int maxNumOfSecrets = -1;

        private Trait trait;
        private List<int> listOfTeams = new List<int>();                    //teamID of all teams that the actor has currently deployed OnMap
        private List<Secret> listOfSecrets = new List<Secret>();            //Player secrets that the actor knows
        private List<Condition> listOfConditions = new List<Condition>();   //list of all conditions currently affecting the actor
        private List<int> listOfTraitEffects = new List<int>();             //list of all traitEffect.teffID's

        //private backing field
        private ActorStatus _status;
        private int _renown;

        public ActorStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                //NOTE: not using an event here as it's a non-unity class and the PostNotification call requires a unity class to work
                //NOTE: don't want this to fire on first turn while everything being set-up (fires in NodeManager.Initialise at the appropriate point)
                if (GameManager.instance.turnScript.Turn > 0)
                { GameManager.instance.nodeScript.SetNodeActorFlags(); }
                //remove gear
                if (_status != ActorStatus.Active && _status != ActorStatus.Inactive)
                { RemoveGear(); }
            }
        }

        public int Renown
        {
            get { return _renown; }
            set
            {
                _renown = value;
                //update renownUI regardless of whether it is on or off
                GameManager.instance.actorPanelScript.UpdateActorRenownUI(actorSlotID, _renown);
            }
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public Actor()
        {
            nodeCaptured = -1;
            Renown = 0;
            gearID = -1;
            gearTimer = 0;
            gearTimesTaken = 0;
            blackmailTimer = 0;
            //fast access & cached
            actorStressNone = GameManager.instance.dataScript.GetTraitEffectID("ActorStressNone");
            actorCorruptNone = GameManager.instance.dataScript.GetTraitEffectID("ActorCorruptNone");
            actorUnhappyNone = GameManager.instance.dataScript.GetTraitEffectID("ActorUnhappyNone");
            actorBlackmailNone = GameManager.instance.dataScript.GetTraitEffectID("ActorBlackmailNone");
            actorBlackmailTimerHigh = GameManager.instance.dataScript.GetTraitEffectID("ActorBlackmailTimerHigh");
            actorBlackmailTimerLow = GameManager.instance.dataScript.GetTraitEffectID("ActorBlackmailTimerLow");
            maxNumOfSecrets = GameManager.instance.secretScript.secretMaxNum;
            Debug.Assert(maxNumOfSecrets > -1, "Invalid maxNumOfSecrets (-1)");
            Debug.Assert(actorStressNone > -1, "Invalid actorStressNone (-1)");
            Debug.Assert(actorStressNone > -1, "Invalid actorCorruptNone (-1)");
            Debug.Assert(actorUnhappyNone > -1, "Invalid actorUnhappyNone (-1)");
            Debug.Assert(actorBlackmailNone > -1, "Invalid actorBlackmailNone (-1)");
            Debug.Assert(actorBlackmailTimerHigh > -1, "Invalid actorBlackmailTimerHigh (-1)");
            Debug.Assert(actorBlackmailTimerLow > -1, "Invalid actorBlackmailTimerLow (-1)");
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

        public List<int> GetTeams()
        { return listOfTeams; }

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
                switch (condition.name)
                {
                    case "STRESSED":
                        if (CheckTraitEffect(actorStressNone) == true)
                        { GameManager.instance.actorScript.TraitLogMessage(this, "for a become Stressed check", "to AVOID becoming Stressed");  }
                        else { proceedFlag = true; }
                        break;
                    case "CORRUPT":
                        if (CheckTraitEffect(actorCorruptNone) == true)
                        { GameManager.instance.actorScript.TraitLogMessage(this, "for a become Corrupt check", "to AVOID becoming Corrupt"); }
                        else { proceedFlag = true; }
                        break;
                    case "UNHAPPY":
                        if (CheckTraitEffect(actorUnhappyNone) == true)
                        { GameManager.instance.actorScript.TraitLogMessage(this, "for a become Unhappy check", "to AVOID becoming Unhappy");  }
                        else { proceedFlag = true; }
                        break;
                    case "BLACKMAILER":
                        //need to have at least one secret
                        if (listOfSecrets.Count == 0)
                        { Debug.Log("Actor.cs -> AddCondition: BLACKMAIL condition NOT added (Actor has no Secrets)"); }
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
                        switch (condition.name)
                        {
                            case "BLACKMAILER":
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
                        Debug.LogFormat("[Con] Actor.cs -> AddCondition: {0}, {1} gained {2} condition{3}", actorName, arc.name, condition.name, "\n");
                        //message
                        string msgText = string.Format("{0} {1} gains condition \"{2}\"", arc.name, actorName, condition.name);
                        GameManager.instance.messageScript.ActorCondition(msgText, actorID, true, condition, reason);
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
                        if (checkCondition.name.Equals(condition.name) == true)
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
                        if (listOfConditions[i].name.Equals(condition.name) == true)
                        {
                            //special cases
                            switch(condition.name)
                            {
                                case "BLACKMAILER":
                                    blackmailTimer = 0;
                                    break;
                            }
                            listOfConditions.RemoveAt(i);
                            Debug.LogFormat("[Con] Actor.cs -> RemoveCondition: {0}, {1} lost {2} condition{3}", actorName, arc.name, condition.name, "\n");
                            //message
                            string msgText = string.Format("{0} {1} condition \"{2}\" removed", arc.name, actorName, condition.name);
                            GameManager.instance.messageScript.ActorCondition(msgText, actorID, false, condition, reason);
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

        public bool CheckSecretPresent(int secretID)
        { return listOfSecrets.Exists(x => x.secretID == secretID); }

        public void RemoveAllSecrets()
        { listOfSecrets.Clear(); }

        /// <summary>
        /// Add a Player secret, checks for duplicates and won't add if one found (warning msg)
        /// </summary>
        /// <param name="secret"></param>
        public void AddSecret(Secret secret)
        {
            if (secret != null)
            {
                //check same secret doesn't already exist
                if (listOfSecrets.Exists(x => x.secretID == secret.secretID) == false)
                {
                    //check space for a new secret
                    if (listOfSecrets.Count < maxNumOfSecrets)
                    {
                        //add secret
                        listOfSecrets.Add(secret);
                        //message
                        string msgText = string.Format("{0} learns of Secret ({1})", arc.name, secret.tag);
                        GameManager.instance.messageScript.ActorSecret(msgText, this, secret);
                    }
                    else { Debug.LogWarning("Secret NOT added as no space available"); }
                }
                else { Debug.LogWarningFormat("Duplicate secret already in list, secretID {0}", secret.secretID); }
            }
        }

        /// <summary>
        /// Remove a secret from listOfSecrets. Returns true if successful, false if not found
        /// </summary>
        /// <param name="secretID"></param>
        /// <returns></returns>
        public bool RemoveSecret(int secretID)
        {
            bool isSuccess = false;
            if (CheckSecretPresent(secretID) == true)
            {
                //reverse loop through and remove secret
                for (int i = listOfSecrets.Count - 1; i >= 0; i--)
                {
                    if (listOfSecrets[i].secretID == secretID)
                    {
                        listOfSecrets.RemoveAt(i);
                        isSuccess = true;
                        break;
                    }
                }
            }
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
                { builder.AppendFormat("{0} ID {1}, {2} ({3}), status: {4}", "\n", secret.secretID, secret.name, secret.tag, secret.status.name); }
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
            foreach(Secret secret in listOfSecrets)
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
                foreach(TraitEffect traitEffect in trait.listOfTraitEffects)
                { listOfTraitEffects.Add(traitEffect.teffID); }
            }
            else { Debug.LogError("Invalid trait (Null)"); }
        }

        /// <summary>
        /// returns true if a particular trait effect is present, false otherwise
        /// </summary>
        /// <param name="traitEffectID"></param>
        /// <returns></returns>
        public bool CheckTraitEffect(int traitEffectID)
        {
            if (listOfTraitEffects != null)
            { return listOfTraitEffects.Exists(x => x == traitEffectID); }
            else { return false; }
        }

        //
        // - - - Gear - - -
        //

        /// <summary>
        /// Add gear. Max 1 piece of gear at a time. If gear already present, new gear overrides, old gear lost. Returns null if no msg (Msg is name & type of gear, if lost)
        /// </summary>
        /// <param name="gearID"></param>
        public string AddGear(int newGearID)
        {
            Debug.Assert(newGearID > -1, "Invalid gearID (< 0)");
            string text = null;
            //existing gear?
            if (gearID > -1)
            {
                Gear gearOld = GameManager.instance.dataScript.GetGear(gearID);
                if (gearOld != null)
                {
                    text = gearOld.name;
                    Debug.LogFormat("[Gea] Actor.cs -> AddGear: {0} given to HQ by {1}{2}", gearOld.name, arc.name, "\n");
                    //gear has been lost
                    if (GameManager.instance.dataScript.RemoveGearLost(gearOld) == false)
                    { Debug.LogWarningFormat("Invalid gear Remove Lost for \"{0}\", gearID {1}", gearOld.name, gearOld.gearID); }
                    //let player know that gear has been Lost
                    string msgText = string.Format("{0} ({1}), has been GIVEN TO HQ by {2}", gearOld.name, gearOld.type.name, arc.name);
                    GameManager.instance.messageScript.GearLost(msgText, gearOld, this, true);
                }
                else { Debug.LogWarningFormat("Invalid gear (Null) for gearID {0}", gearID); }
            }
            //add new gear
            Gear gearNew = GameManager.instance.dataScript.GetGear(newGearID);
            if (gearNew != null)
            {
                gearID = newGearID;
                gearTimer = 0;
                Debug.LogFormat("[Gea] Actor.cs -> AddGear: {0} added to inventory of {1}{2}", gearNew.name, arc.name, "\n");
                //add to listOfCurrentGear (if not already present)
                GameManager.instance.dataScript.AddGearNew(gearNew);
            }
            else { Debug.LogWarningFormat("Invalid gear (Null) for gearID {0}", newGearID); }
            //return name and type of any gear that was lost (existing prior to new gear being added)
            return text;
        }

        /// <summary>
        /// remove gear for various reasons. Msg only if gear present in first place. isTaken true if you are taking gear from actor, false (by default) otherwise
        /// </summary>
        public void RemoveGear(bool isTaken = false)
        {
            if (gearID > -1)
            {
                Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                if (gear != null)
                { Debug.LogFormat("[Gea] Actor.cs -> RemoveGear: {0} removed from inventory of {1}{2}", gear.name, arc.name, "\n"); }
                else { Debug.LogWarningFormat("Invalid gear (Null) for gearID {0}", gearID); }
                //remove gear AFTER logger
                gearID = -1;
                gearTimer = 0;
                //if gear has been taken, increment counter
                if (isTaken == true)
                { gearTimesTaken++; }
                else
                {
                    //gear Lost
                    if (GameManager.instance.dataScript.RemoveGearLost(gear) == false)
                    { Debug.LogWarningFormat("Invalid gear Remove Lost for \"{0}\", gearID {1}", gear.name, gear.gearID); }
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
        /// returns -1 if no gear
        /// </summary>
        /// <returns></returns>
        public int GetGearID()
        { return gearID; }


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
            if (gearID > -1)
            { gear = GameManager.instance.dataScript.GetGear(gearID); }
            ActorTooltipData data = new ActorTooltipData()
            {
                tooltipPos = position,
                actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, side),
                action = GameManager.instance.dataScript.GetActorAction(actorSlotID, side),
                gear = gear,
                listOfSecrets = GetSecretsTooltipList(),
                arrayOfQualities = GameManager.instance.dataScript.GetQualities(side),
                arrayOfStats = GameManager.instance.dataScript.GetActorStats(actorSlotID, side)
            };
            return data;
        }


        //place methods above here
    }
}
