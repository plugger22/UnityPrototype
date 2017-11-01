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
        //Place one team of every type in the reserve
        List<int> listOfTeamArcIDs = GameManager.instance.dataScript.GetTeamArcIDs();
        if (listOfTeamArcIDs != null && listOfTeamArcIDs.Count > 0)
        {
            //loop list and add one team of each type to teamReserve pool
            foreach(int arcID in listOfTeamArcIDs)
            { GameManager.instance.dataScript.AdjustTeamInfo(arcID, TeamInfo.Reserve, +1); }
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
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Empty)"); }


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
    }

    
    //place new method above here
}
