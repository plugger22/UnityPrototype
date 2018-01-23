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
        [HideInInspector] public int slotID;                     //actor slot ID (eg, 0 to 3)
        [HideInInspector] public int actorID;
        [HideInInspector] public int level;                     //1 (worst) to 3 (best). level 1 are start actors, level 2 are recruited, level 3 are special
        [HideInInspector] public int renown;                   //starts at '0' and goes up (no limit)
        [HideInInspector] public string actorName;
        //[HideInInspector] public bool isLive;                   //actor can 'go silent' and be unavailable on occasion
        [HideInInspector] public ActorArc arc;
        [HideInInspector] public Trait trait;
        [HideInInspector] public ActorStatus status;
        

        

        private List<int> listOfTeams = new List<int>();    //teamID of all teams that the actor has currently deployed OnMap

        public Actor()
        { }


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
