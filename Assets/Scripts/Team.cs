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

        [HideInInspector] public TeamArc arc;

        /// <summary>
        /// Creates a new team of a particular TeamArcType, eg. "Security"
        /// </summary>
        /// <param name="arcType"></param>
        public Team(string arcType)
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
                    this.arc = teamArc;
                    //increment number of that type of team present in level (on map or off)
                    int countOfTeams = GameManager.instance.dataScript.GetTeamInfo(teamArcID, TeamInfo.Total);
                    countOfTeams++;
                    if (countOfTeams > -1 && countOfTeams < (int)NATO.Count)
                    { Name = "Team " + (NATO)countOfTeams; }
                    else { Name = "Team Unknown"; }
                    //update teams count
                    GameManager.instance.dataScript.AdjustTeamInfo(teamArcID, TeamInfo.Total, +1);
                    //add to dictionary
                    GameManager.instance.dataScript.AddTeam(this);
                }
                else Debug.LogError(string.Format("TeamArc type \"{0}\", ID {1}, not found in dictionary -> Team not created{2}", arcType, teamArcID, "\n"));
            }
            else { Debug.LogError(string.Format("TeamArc type \"{0}\" not found in dictionary -> Team not created{1}", arcType, "\n")); }
        }
    }


}