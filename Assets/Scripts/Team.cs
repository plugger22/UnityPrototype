using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace gameAPI
{

    public class Team
    {
        [HideInInspector] public int TeamID;
        [HideInInspector] public string Name;

        private static int teamCounter = 0;

        [HideInInspector] public TeamPool Pool;
        [HideInInspector] public TeamArc Arc;
        [HideInInspector] public int ActorSlotID;                //which actor has deployed the team, '-1' if none
        [HideInInspector] public int NodeID;                 //nodeID where team is located if 'OnMap'. '-1' if none.
        [HideInInspector] public int Timer;                 //countdown timer for deployment OnMap. '-1' if none.
        [HideInInspector] public int TurnDeployed;          //if deployed OnMap, turn number of when it first happened.

        /// <summary>
        /// Creates a new team of a particular TeamArcType, eg. "Security"
        /// </summary>
        /// <param name="arcType"></param>
        public Team(string arcType, int count)
        {
            //valid arcType
            int teamArcID = GameManager.instance.dataScript.GetTeamArcID(arcType);
            if (teamArcID > -1)
            {
                //get teamArc
                TeamArc teamArc = GameManager.instance.dataScript.GetTeamArc(teamArcID);
                if (teamArc != null)
                {
                    TeamID = teamCounter++;
                    this.Arc = teamArc;
                    InitialiseTeamData(count);
                    AddToCollections(this);
                }
                else Debug.LogError(string.Format("TeamArc type \"{0}\", ID {1}, not found in dictionary -> Team not created{2}", arcType, teamArcID, "\n"));
            }
            else { Debug.LogError(string.Format("TeamArc type \"{0}\" not found in dictionary -> Team not created{1}", arcType, "\n")); }
        }


        /// <summary>
        /// Creates a new team of a particular teamArcID
        /// </summary>
        /// <param name="arcType"></param>
        public Team(int teamArcID, int count)
        {
            //valid arcType
            if (teamArcID > -1)
            {
                //get teamArc
                TeamArc teamArc = GameManager.instance.dataScript.GetTeamArc(teamArcID);
                if (teamArc != null)
                {
                    TeamID = teamCounter++;
                    this.Arc = teamArc;
                    InitialiseTeamData(count);
                    AddToCollections(this);
                }
                else Debug.LogError(string.Format("TeamArc type \"{0}\", ID {1}, not found in dictionary -> Team not created{2}", teamArc.name, teamArcID, "\n"));
            }
            else { Debug.LogError(string.Format("Invalid teamArcID {1}", teamArcID, "\n")); }
        }



        /// <summary>
        /// subMethod to set up a team's base data
        /// </summary>
        /// <param name="team"></param>
        private void InitialiseTeamData(int count)
        {
            Pool = TeamPool.Reserve;
            ActorSlotID = -1;
            NodeID = -1;
            Timer = -1;
            TurnDeployed = -1;
            if (count > -1 && count < (int)NATO.Count)
            { Name = "Team " + (NATO)count; }
            else { Name = "Team Unknown"; }
        }

        /// <summary>
        /// used by TeamManager.cs -> MoveTeam to reset data
        /// </summary>
        /// <param name="pool"></param>
        public void ResetTeamData(TeamPool pool)
        {
            Pool = pool;
            ActorSlotID = -1;
            NodeID = -1;
            Timer = -1;
            TurnDeployed = -1;
        }

        /// <summary>
        /// subMethod to add team to collections
        /// </summary>
        /// <param name="team"></param>
        private void AddToCollections(Team team)
        {
            if (GameManager.instance.dataScript.AddTeamToDict(team) == true)
            { GameManager.instance.dataScript.AddTeamToPool(TeamPool.Reserve, team.TeamID); }
        }

    }
}