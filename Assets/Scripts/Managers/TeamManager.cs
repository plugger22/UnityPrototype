using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System.Text;

/// <summary>
/// handles all team related matters
/// </summary>
public class TeamManager : MonoBehaviour
{
    [Range(1, 4)]
    [Tooltip("The maximum number of teams that may be present at a node at any one time")]
    public int maxTeamsAtNode = 3;
    [Range(1, 4)]
    [Tooltip("How long a team is deployed for before automatically being recalled")]
    public int deployTime = 2;
    [Range(1, 4)]
    [Tooltip("The increase to node security due to the presence of a SecurityTeam")]
    public int securityTeamEffect = 2;

    /// <summary>
    /// Set up at start
    /// </summary>
    public void Initialise()
    {
        InitialiseTeams();
    }

    /// <summary>
    /// PlaceHolder -> place a random number of security teams on the map for testing purposes
    /// </summary>
    public void InitialiseTeams()
    {
        //Place one team of every type in the reserve
        List<int> listOfTeamArcIDs = GameManager.instance.dataScript.GetTeamArcIDs();
        if (listOfTeamArcIDs != null && listOfTeamArcIDs.Count > 0)
        {
            //loop list and add one team of each type to teamReserve pool
            foreach(int arcID in listOfTeamArcIDs)
            {
                GameManager.instance.dataScript.AdjustTeamInfo(arcID, TeamInfo.Reserve, +1);
                GameManager.instance.dataScript.AdjustTeamInfo(arcID, TeamInfo.Total, +1);
            }
        }
        else { Debug.LogError("Invalid listOfTeamArcIDs (Null or Empty) -> initial team setup cancelled"); }
        
        //Add extra teams equal to each Authority actors ability level and off their preferred type
        Actor[] arrayOfActors = GameManager.instance.actorScript.GetActors(Side.Authority);
        if (arrayOfActors.Length > 0)
        {
            int ability, arcID;
            foreach(Actor actor in arrayOfActors)
            {
                //get actors Ability
                ability = actor.Datapoint2;
                //get preferred team
                arcID = actor.arc.preferredTeam.TeamArcID;
                //add the ability number of teams to the reserve
                GameManager.instance.dataScript.AdjustTeamInfo(arcID, TeamInfo.Reserve, ability);
                GameManager.instance.dataScript.AdjustTeamInfo(arcID, TeamInfo.Total, ability);
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Empty)"); }

        //create actual teams and fill the reserve pool based on the number of teams decided upon above
        int numToCreate;
        foreach(int teamArcID in listOfTeamArcIDs)
        {
            //how many present? (only check reserve as at start of game that's where all teams are)
            numToCreate = GameManager.instance.dataScript.GetTeamInfo(teamArcID, TeamInfo.Reserve);
            //create teams
            for (int i = 0; i < numToCreate; i++)
            { Team team = new Team(teamArcID); }
        }

        /*
        //loop all nodes, 40% chance of getting a security team
        List<Node> listOfNodes = new List<Node>(GameManager.instance.dataScript.GetAllNodes().Values);
        //add a random other team as well, if security team already present
        int numOfTeamArcs = GameManager.instance.dataScript.GetNumOfTeamArcs();
        int teamArcID;
        TeamArc teamArc = null;
        foreach(Node node in listOfNodes)
        {
            if (Random.Range(0, 100) < 40)
            {

                //NOTE: Need to redo this to accomodate pools of physical teams and DM:MoveTeam()

                //create a new security team
                Team team = new Team("Control");
                if (team != null)
                {
                    node.AddTeam(team);
                    //adjust tallies for onMap
                    GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.OnMap, +1);
                    GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve, -1);
                }
                //chance of a second, random, team
                if (Random.Range(0, 100) < 50)
                {
                    teamArcID = Random.Range(0, numOfTeamArcs);
                    teamArc = GameManager.instance.dataScript.GetTeamArc(teamArcID);
                    if (teamArc != null)
                    {
                        Team secondTeam = new Team(teamArc.name);
                        node.AddTeam(secondTeam);
                        //adjust tallies for onMap
                        GameManager.instance.dataScript.AdjustTeamInfo(secondTeam.arc.TeamArcID, TeamInfo.OnMap, +1);
                    }
                    else { Debug.LogWarning(string.Format("Invalid teamArc (null) for teamArcID {0}{1}", teamArcID, "\n")); }
                }
            }
        }
        */
    }


    /// <summary>
    /// Debug function to display a breakdown of the team pools
    /// </summary>
    /// <returns></returns>
    public string GetTeamAnalysis()
    {
        StringBuilder builder = new StringBuilder();
        //get dictionary of team arcs
        Dictionary<int, TeamArc> tempDict = GameManager.instance.dataScript.GetTeamArcs();
        if (tempDict != null)
        {
            int reserve, onMap, inTransit, Total;
            string data;
            //header
            builder.Append("        Team Pool Analysis");
            builder.AppendLine();
            builder.AppendLine();
            foreach (var teamData in tempDict)
            {
                //get data
                reserve = GameManager.instance.dataScript.GetTeamInfo(teamData.Key, TeamInfo.Reserve);
                onMap = GameManager.instance.dataScript.GetTeamInfo(teamData.Key, TeamInfo.OnMap);
                inTransit = GameManager.instance.dataScript.GetTeamInfo(teamData.Key, TeamInfo.InTransit);
                Total = GameManager.instance.dataScript.GetTeamInfo(teamData.Key, TeamInfo.Total);
                data = string.Format("r:{0,2}  m:{1,2}  t:{2,2}  T:{3,2}", reserve, onMap, inTransit, Total);
                //display data
                builder.Append(string.Format("{0,-12}{1,-12}", teamData.Value.name, data));
                builder.AppendLine();
            }
            //add team pool totals
            builder.AppendLine();
            builder.Append(string.Format("{0, -20}{1,2} teams", "Reserve Pool", GameManager.instance.dataScript.GetTeamPoolCount(TeamPool.Reserve)));
            builder.AppendLine();
            builder.Append(string.Format("{0, -20}{1,2} teams", "OnMap Pool", GameManager.instance.dataScript.GetTeamPoolCount(TeamPool.OnMap)));
            builder.AppendLine();
            builder.Append(string.Format("{0, -20}{1,2} teams", "InTransit Pool", GameManager.instance.dataScript.GetTeamPoolCount(TeamPool.InTransit)));
            //total teams
            builder.AppendLine();
            builder.AppendLine();
            builder.Append(string.Format("Teams in dictOfTeams   {0}", GameManager.instance.dataScript.GetNumOfTeams()));
        }
        return builder.ToString();
    }

    /// <summary>
    /// Debug function -> Gets data on all individual teams residing in dictOfTeams
    /// </summary>
    /// <returns></returns>
    public string GetIndividualTeams()
    {
        StringBuilder builder = new StringBuilder();
        //get dictionary of team arcs
        Dictionary<int, Team> teamDict = GameManager.instance.dataScript.GetTeams();
        if (teamDict != null)
        {
            //header
            //header
            builder.Append("        Teams in dictOfTeams");
            builder.AppendLine();
            builder.AppendLine();
            foreach (var teamData in teamDict)
            {
                builder.Append(string.Format("ID {0}  {1}  P: {2}  N: {3}  T: {4}  A: {5}", teamData.Key, teamData.Value.Name, teamData.Value.Pool, teamData.Value.NodeID,
                    teamData.Value.Timer, teamData.Value.ActorID));
                builder.AppendLine();
            }
        }
        return builder.ToString();
    }

    //place new method above here
}
