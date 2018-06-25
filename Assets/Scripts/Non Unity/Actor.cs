using System.Collections;
using System.Collections.Generic;
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

        //cached trait effects
        private int actorStressNone;
        private int actorCorruptNone;
        private int actorUnhappyNone;

        private Trait trait;
        private List<int> listOfTeams = new List<int>();                    //teamID of all teams that the actor has currently deployed OnMap
        private List<int> listOfSecrets = new List<int>();
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
            //cached Trait Effects
            actorStressNone = GameManager.instance.dataScript.GetTraitEffectID("ActorStressNone");
            actorCorruptNone = GameManager.instance.dataScript.GetTraitEffectID("ActorCorruptNone");
            actorUnhappyNone = GameManager.instance.dataScript.GetTraitEffectID("ActorUnhappyNone");
            Debug.Assert(actorStressNone > -1, "Invalid actorStressNone (-1)");
            Debug.Assert(actorStressNone > -1, "Invalid actorCorruptNone (-1)");
            Debug.Assert(actorUnhappyNone > -1, "Invalid actorUnhappyNone (-1)");
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
        /// Add a new condition to list provided it isn't already present
        /// </summary>
        /// <param name="condition"></param>
        public void AddCondition(Condition condition)
        {
            bool proceedFlag = true;
            if (condition != null)
            {
                //trait check
                switch(condition.name)
                {
                    case "STRESSED":
                        if (CheckTraitEffect(actorStressNone) == true)
                        {
                            proceedFlag = false;
                            GameManager.instance.actorScript.DebugTraitMessage(this, "to prevent STRESSED Condition");
                        }
                        break;
                    case "CORRUPT":
                        if (CheckTraitEffect(actorCorruptNone) == true)
                        {
                            proceedFlag = false;
                            GameManager.instance.actorScript.DebugTraitMessage(this, "to prevent CORRUPT Condition");
                        }
                        break;
                    case "UNHAPPY":
                        if (CheckTraitEffect(actorUnhappyNone) == true)
                        {
                            proceedFlag = false;
                            GameManager.instance.actorScript.DebugTraitMessage(this, "to prevent UNHAPPY Condition");
                        }
                        break;
                }
                if (proceedFlag == true)
                {
                    //check that condition isn't already present
                    if (CheckConditionPresent(condition) == false)
                    {
                        listOfConditions.Add(condition);
                        //message
                        string msgText = string.Format("{0} {1} gains condition \"{2}\"", arc.name, actorName, condition.name);
                        Message message = GameManager.instance.messageScript.ActorCondition(msgText, actorID, GameManager.instance.sideScript.PlayerSide);
                        GameManager.instance.dataScript.AddMessage(message);
                    }
                }
            }
            else { Debug.LogError("Invalid condition (Null)"); }
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
        public bool RemoveCondition(Condition condition)
        {
            if (condition != null)
            {
                if (listOfConditions.Count > 0)
                {
                    //reverse loop -> delete and return if found
                    for (int i = listOfConditions.Count - 1; i >= 0; i--)
                    {
                        if (listOfConditions[i].name.Equals(condition.name) == true)
                        {
                            listOfConditions.RemoveAt(i);
                            //message
                            string msgText = string.Format("{0} {1} condition \"{2}\" removed", arc.name, actorName, condition.name);
                            Message message = GameManager.instance.messageScript.ActorCondition(msgText, actorID, GameManager.instance.sideScript.PlayerSide);
                            GameManager.instance.dataScript.AddMessage(message);
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

        public int CheckNumOfSecrets()
        { return listOfSecrets.Count; }

        public List<int> GetListOfSecrets()
        { return listOfSecrets; }

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




        //place methods above here
    }
}
