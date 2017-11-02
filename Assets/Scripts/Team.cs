using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace gameAPI
{

    public class Team
    {
        public int TeamID { get; set; }
        public string Name { get; set; }

        private static int teamCounter = 0;

        public TeamPool Pool { get; set; }
        public TeamArc Arc { get; set; }
        public int ActorID { get; set; }                //which actor has deployed the team, '-1' if none
        public int NodeID { get; set; }                 //nodeID where team is located if 'OnMap'. '-1' if none.
        public int Timer { get; set; }                  //countdown timer for deployment OnMap. '-1' if none.
        public int TurnDeployed { get; set; }           //if deployed OnMap, turn number of when it first happened.

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
                    InitialiseTeamData(this, count);
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
                    InitialiseTeamData(this, count);
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
        private void InitialiseTeamData(Team team, int count)
        {
            team.Pool = TeamPool.Reserve;
            team.ActorID = -1;
            team.NodeID = -1;
            team.Timer = -1;
            team.TurnDeployed = -1;
            if (count > -1 && count < (int)NATO.Count)
            { team.Name = "Team " + (NATO)count; }
            else { team.Name = "Team Unknown"; }
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