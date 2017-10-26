using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// handles all team related matters
/// </summary>
public class TeamManager : MonoBehaviour
{
    [Range(1, 4)]
    [Tooltip("The maximum number of teams that may be present at a node at any one time")]
    public int maxTeamsAtNode = 3;
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
        //loop all nodes, 40% chance of getting a security team
        List<Node> listOfNodes = new List<Node>(GameManager.instance.dataScript.GetAllNodes().Values);
        //add a random other team as well, if security team already present
        int numOfTeamArcs = GameManager.instance.dataScript.GetNumOfTeamTypes();
        int teamArcID;
        TeamArc teamArc = null;
        foreach(Node node in listOfNodes)
        {
            if (Random.Range(0, 100) < 40)
            {
                //create a new security team
                Team team = new Team("Security");
                if (team != null)
                {
                    node.AddTeam(team);
                    //adjust tallies for onMap
                    GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.OnMap, +1);
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
    }

    
    //place new method above here
}
