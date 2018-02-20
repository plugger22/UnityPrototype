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
        [HideInInspector] public int datapoint0;               //higher the number (1 to 3), see DM: arrayOfQualities for string tags
        [HideInInspector] public int datapoint1;               //higher the better (1 to 3)
        [HideInInspector] public int datapoint2;               //higher the better (1 to 3)

        [HideInInspector] public Side actorSide;

        [HideInInspector] public GlobalSide side;
        [HideInInspector] public int slotID;                    //actor slot ID (eg, 0 to 3)
        [HideInInspector] public int actorID;
        [HideInInspector] public int level;                     //1 (worst) to 3 (best). level 1 are start actors, level 2 are recruited, level 3 are special
        [HideInInspector] public int renown;                    //starts at '0' and goes up (no limit)
        [HideInInspector] public int nodeCaptured;              //node where actor was captured (took an action), default '-1'
        [HideInInspector] public string actorName;
        [HideInInspector] public ActorArc arc;
        [HideInInspector] public Trait trait;
        
        
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
        

        

        private List<int> listOfTeams = new List<int>();    //teamID of all teams that the actor has currently deployed OnMap

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

    }
}
