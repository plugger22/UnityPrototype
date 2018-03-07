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
        [HideInInspector] public int renown;                    //starts at '0' and goes up (no limit)
        [HideInInspector] public int nodeCaptured;              //node where actor was captured (took an action), default '-1'
        [HideInInspector] public int unhappyTimer;             //used when in Reserves. Becomes 'Unhappy' once expires
        [HideInInspector] public bool isPromised;               //When sent to reserves Player can promise to recall them within a certain time (true), otherwise false
        [HideInInspector] public string actorName;
        [HideInInspector] public ActorArc arc;
        [HideInInspector] public Trait trait;
        
        private List<int> listOfTeams = new List<int>();                    //teamID of all teams that the actor has currently deployed OnMap
        private List<Condition> listOfConditions = new List<Condition>();   //list of all conditions currently affecting the actor

        //private backing field
        private ActorStatus _status;

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

        public Actor()
        {
            nodeCaptured = -1;
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
            if (condition != null)
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


        //place methods above here
    }
}
