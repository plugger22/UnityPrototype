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
    public int deployTime = 3;
    [Range(1, 4)]
    [Tooltip("The increase to node security due to the presence of a SecurityTeam")]
    public int securityTeamEffect = 2;

    /// <summary>
    /// Set up at start
    /// </summary>
    public void Initialise()
    {
        InitialiseTeams();
        SeedTeamsOnMap();     //DEBUG
    }

    /// <summary>
    /// Sets up intial Reserve pool of teams and related collections
    /// </summary>
    public void InitialiseTeams()
    {
        //Place one team of every type in the reserve
        List<int> listOfTeamArcIDs = GameManager.instance.dataScript.GetTeamArcIDs();
        if (listOfTeamArcIDs != null && listOfTeamArcIDs.Count > 0)
        {
            //loop list and add one team of each type to teamReserve pool
            foreach (int arcID in listOfTeamArcIDs)
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
            foreach (Actor actor in arrayOfActors)
            {
                //get actors Ability
                ability = actor.Datapoint2;
                //get preferred team
                arcID = actor.Arc.preferredTeam.TeamArcID;
                //add the ability number of teams to the reserve
                GameManager.instance.dataScript.AdjustTeamInfo(arcID, TeamInfo.Reserve, ability);
                GameManager.instance.dataScript.AdjustTeamInfo(arcID, TeamInfo.Total, ability);
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Empty)"); }

        //create actual teams and fill the reserve pool based on the number of teams decided upon above
        int numToCreate;
        foreach (int teamArcID in listOfTeamArcIDs)
        {
            //how many present? (only check reserve as at start of game that's where all teams are)
            numToCreate = GameManager.instance.dataScript.CheckTeamInfo(teamArcID, TeamInfo.Reserve);
            //create teams
            for (int i = 0; i < numToCreate; i++)
            { Team team = new Team(teamArcID, i); }
        }
    }


    /// <summary>
    /// Debug method -> each team in reserve pool at game start has a chance of being deployed
    /// </summary>
    public void SeedTeamsOnMap()
    {
        List<Node> listOfNodes = new List<Node>(GameManager.instance.dataScript.GetAllNodes().Values);
        Dictionary<int, Team> dictOfTeams = GameManager.instance.dataScript.GetTeams();
        if (dictOfTeams != null)
        {
            int actorSlotID;
            //loop teams
            foreach (var teamData in dictOfTeams)
            {
                //40% chance of being deployed
                if (Random.Range(0, 100) < 40 == true)
                {
                    //get a random node
                    Node node = listOfNodes[Random.Range(0, listOfNodes.Count)];
                    if (node != null)
                    {
                        //get a random Actor
                        actorSlotID = Random.Range(0, GameManager.instance.actorScript.numOfActorsTotal);
                        MoveTeam(TeamPool.OnMap, actorSlotID, teamData.Key, node);
                    }
                    else { Debug.LogError("Invalid node (Null)"); }
                }
            }
        }
        else { Debug.LogError("Invalid dictOfTeams (Null)"); }
    }


    /// <summary>
    /// handles all admin for moving a team from one pool to another. Assumed movement direction is 'Reserve -> OnMap -> InTransit -> Reserve'
    /// Takes care of all checks, eg. enough teams present in reserve for one to move to the map
    /// Will check if actor has the ability to handle another team onMap
    /// only use the node parameter if the team is moving 'OnMap' (it's moving to a specific node)
    /// </summary>
    /// <param name="destinationPool"></param>
    /// <param name="teamID"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool MoveTeam(TeamPool destinationPool, int actorSlotID, int teamID, Node node = null)
    {
        Debug.Assert(teamID > -1 && teamID < GameManager.instance.dataScript.CheckNumOfTeams(), "Invalid teamID");
        Team team = GameManager.instance.dataScript.GetTeam(teamID);
        bool successFlag = true;
        if (team != null)
        {
            if (actorSlotID > -1 && actorSlotID < GameManager.instance.actorScript.numOfActorsTotal)
            {
                //Get Actor
                Actor actor = GameManager.instance.actorScript.GetActor(actorSlotID, GameManager.instance.optionScript.PlayerSide);
                if (actor != null)
                {
                    if (actor.isLive == true)
                    {
                        switch (destinationPool)
                        {
                            case TeamPool.Reserve:
                                break;
                            case TeamPool.OnMap:
                                if (node != null)
                                {
                                    //a team available in the reserve pool?
                                    if (GameManager.instance.dataScript.CheckTeamInfo(team.Arc.TeamArcID, TeamInfo.Reserve) > 0)
                                    {
                                        //check if actor has capacity to deploy another team
                                        if (actor.CheckCanDeployTeam() == true)
                                        {
                                            if (node.AddTeam(team, actorSlotID) == true)
                                            {
                                                //adjust tallies for onMap
                                                GameManager.instance.dataScript.AdjustTeamInfo(team.Arc.TeamArcID, TeamInfo.OnMap, +1);
                                                GameManager.instance.dataScript.AdjustTeamInfo(team.Arc.TeamArcID, TeamInfo.Reserve, -1);
                                                //pools
                                                GameManager.instance.dataScript.AddTeamToPool(TeamPool.OnMap, teamID);
                                                GameManager.instance.dataScript.RemoveTeamFromPool(TeamPool.Reserve, teamID);
                                                //add team to Actor list
                                                actor.AddTeam(team.TeamID);
                                                //confirmation
                                                Debug.Log(string.Format("TeamManager: {0} {1}, ID {2}, moved to {3}, Node ID {4}{5}", team.Arc.name, team.Name, team.TeamID,
                                                    destinationPool, node.NodeID, "\n"));
                                            }
                                            else
                                            {
                                                Debug.LogWarning(string.Format("Move team operation failed for \"{0} {1}\"", team.Arc.name, team.Name));
                                                successFlag = false;
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogWarning(string.Format("Unable to deploy {0} {1} to {2} as Actor {3}, slotID {4}, has insufficient ability{5}",
                                                team.Arc.name, team.Name, destinationPool, actor.Arc.name, actorSlotID, "\n"));
                                            successFlag = false;
                                        }
                                    }
                                    
                                    else
                                    {
                                        Debug.LogWarning(string.Format("Not enough {0} teams present. Move cancelled", team.Arc.name));
                                        successFlag = false;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Invalid node (Null) for OnMap -> move Cancelled");
                                    successFlag = false;
                                }
                                break;
                            case TeamPool.InTransit:

                                //don't forget to remove team from actor list

                                break;
                            default:
                                Debug.LogError(string.Format("Invalid pool \"{0}\"", destinationPool));
                                successFlag = false;
                                break;
                        }
                        if (successFlag == false)
                        {  return false; }
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("{0}, ID {1} can't be deployed to {2} as Actor {3}, slotID {4} isn't Live{5}", team.Arc.name, teamID, destinationPool,
                          actor.Name, actorSlotID, "\n"));
                        return false;
                    }
                }
                else
                { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", actorSlotID)); return false; }
            }
            else
            { Debug.LogError(string.Format("Invalid actor slotID \"{0}\"", actorSlotID)); return false; }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Team (null) for TeamID {0}", teamID));  return false; }
        return true;
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
            builder.Append(" Team Pool Analysis");
            builder.AppendLine();
            builder.AppendLine();
            foreach (var teamData in tempDict)
            {
                //get data
                reserve = GameManager.instance.dataScript.CheckTeamInfo(teamData.Key, TeamInfo.Reserve);
                onMap = GameManager.instance.dataScript.CheckTeamInfo(teamData.Key, TeamInfo.OnMap);
                inTransit = GameManager.instance.dataScript.CheckTeamInfo(teamData.Key, TeamInfo.InTransit);
                Total = GameManager.instance.dataScript.CheckTeamInfo(teamData.Key, TeamInfo.Total);
                data = string.Format("r:{0,2}  m:{1,2}  t:{2,2}  T:{3,2}", reserve, onMap, inTransit, Total);
                //display data
                builder.Append(string.Format(" {0,-12}{1,-12}", teamData.Value.name, data));
                builder.AppendLine();
            }
            //add team pool totals
            builder.AppendLine();
            builder.Append(string.Format(" {0, -20}{1,2} teams", "Reserve Pool", GameManager.instance.dataScript.CheckTeamPoolCount(TeamPool.Reserve)));
            builder.AppendLine();
            builder.Append(string.Format(" {0, -20}{1,2} teams", "OnMap Pool", GameManager.instance.dataScript.CheckTeamPoolCount(TeamPool.OnMap)));
            builder.AppendLine();
            builder.Append(string.Format(" {0, -20}{1,2} teams", "InTransit Pool", GameManager.instance.dataScript.CheckTeamPoolCount(TeamPool.InTransit)));
            //total teams
            builder.AppendLine();
            builder.AppendLine();
            builder.Append(string.Format(" Teams in dictOfTeams   {0}", GameManager.instance.dataScript.CheckNumOfTeams()));
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
            builder.Append(" Teams in Dictionary");
            builder.AppendLine();
            builder.AppendLine();
            foreach (var teamData in teamDict)
            {
                builder.Append(string.Format(" ID {0}  {1} {2}  P: {3}  N: {4}  T: {5}  A: {6}", teamData.Key, teamData.Value.Arc.name, teamData.Value.Name, 
                    teamData.Value.Pool, teamData.Value.NodeID, teamData.Value.Timer, teamData.Value.ActorID));
                builder.AppendLine();
            }
        }
        return builder.ToString();
    }

    //place new method above here
}
