using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using modalAPI;
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


    private string colourEffect;
    private string colourSide;
    private string colourTeam;
    private string colourDefault;
    private string colourNormal;
    private string colourGood;
    private string colourActor;
    private string colourBad;
    private string colourEnd;

    /// <summary>
    /// Set up at start
    /// </summary>
    public void Initialise()
    {
        InitialiseTeams();
        SeedTeamsOnMap();     //DEBUG
        //event Listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.EndTurn, OnEvent);
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent);
        EventManager.instance.AddListener(EventType.RecallTeamAction, OnEvent);
        EventManager.instance.AddListener(EventType.GenericTeamRecall, OnEvent);
        EventManager.instance.AddListener(EventType.NeutraliseTeamAction, OnEvent);
        EventManager.instance.AddListener(EventType.GenericNeutraliseTeam, OnEvent);
    }


    /// <summary>
    /// Called when an event happens
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect Event type
        switch (eventType)
        {
            case EventType.EndTurn:
                EndTurn();
                break;
            case EventType.StartTurnEarly:
                StartTurnEarly();
                break;
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.RecallTeamAction:
                InitialiseGenericPickerRecall((int)Param);
                break;
            case EventType.NeutraliseTeamAction:
                ModalActionDetails details = Param as ModalActionDetails;
                InitialiseGenericPickerNeutralise(details);
                break;
            case EventType.GenericTeamRecall:
                GenericReturnData returnDataRecall = Param as GenericReturnData;
                ProcessRecallTeam(returnDataRecall);
                break;
            case EventType.GenericNeutraliseTeam:
                GenericReturnData returnDataNeutralise = Param as GenericReturnData;
                ProcessNeutraliseTeam(returnDataNeutralise);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourEffect = GameManager.instance.colourScript.GetColour(ColourType.actionEffect);
        colourSide = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourTeam = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourActor = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// End turn activity -> Event driven
    /// </summary>
    private void EndTurn()
    {
        //decrement all timers in OnMap pool
        List<int> teamPool = GameManager.instance.dataScript.GetTeamPool(TeamPool.OnMap);
        if (teamPool != null)
        {
            for(int i = 0; i < teamPool.Count; i++)
            {
                Team team = GameManager.instance.dataScript.GetTeam(teamPool[i]);
                if (team != null)
                {
                    //decrement timer
                    team.Timer--;
                }
                else { Debug.LogError(string.Format("Invalid team (null) for TeamID {0}", teamPool[i])); }
            }
        }
        else { Debug.LogError("Invalid teamPool (Null) -> no team timers decremented"); }
    }

    /// <summary>
    /// Start turn Early activity -> Event driven. Team Management (inTransit, OnMap timers)
    /// </summary>
    private void StartTurnEarly()
    {
        List<int> teamPool = new List<int>();
        //check InTransit pool -> move any teams here to the Reserve pool -> Note: do this BEFORE checking OnMap pool below
        teamPool.AddRange(GameManager.instance.dataScript.GetTeamPool(TeamPool.InTransit));
        if (teamPool != null)
        {
            for (int i = 0; i < teamPool.Count; i++)
            {
                Team team = GameManager.instance.dataScript.GetTeam(teamPool[i]);
                if (team != null)
                {
                    //Automatically move any teams to reserve (they spend in turn in transit and are unavailable for deployment)
                    MoveTeam(TeamPool.Reserve, team.TeamID, team.ActorSlotID);
                }
                else { Debug.LogError(string.Format("Invalid team (null) for TeamID {0}", teamPool[i])); }
            }
        }
        else { Debug.LogError("Invalid teamPool (Null) -> no teams moved from InTransit"); }
        //check all timers in OnMap pool -> need a value list, not reference, here as MoveTeam changes the pool while I'm looping it below
        teamPool.Clear();
        teamPool.AddRange(GameManager.instance.dataScript.GetTeamPool(TeamPool.OnMap));
        if (teamPool != null)
        {
            for (int i = 0; i < teamPool.Count; i++)
            {
                Team team = GameManager.instance.dataScript.GetTeam(teamPool[i]);
                if (team != null)
                {
                    //check timer 
                    if (team.Timer < 0)
                    {

                        //Timer expired, team automatically recalled to InTransit pool
                        Node node = GameManager.instance.dataScript.GetNode(team.NodeID);
                        if (node != null)
                        {


                            Actor actor = GameManager.instance.dataScript.GetCurrentActor(team.ActorSlotID, Side.Authority);
                            MoveTeam(TeamPool.InTransit, team.TeamID, team.ActorSlotID, node);
                            if (actor != null)
                            {                            
                                //Permanent Team effect activated for node
                                ProcessTeamEffect(team, node, actor);
                                //message
                                string text = string.Format("{0} {1}, ID {2}, recalled from {3}, ID {4}", team.Arc.name, team.Name, team.TeamID, node.Name, node.nodeID);
                                Message message = GameManager.instance.messageScript.TeamAutoRecall(text, node.nodeID, team.TeamID, actor.actorID);
                                if (message != null) { GameManager.instance.dataScript.AddMessageNew(message); }
                            }
                            else { Debug.LogError(string.Format("Invalid actor (null) for actorSlotID {0}", team.ActorSlotID)); }
                        }
                        else { Debug.LogError(string.Format("Invalid node (null) for TeamID {0} and team.NodeID {1}", teamPool[i], team.NodeID)); }
                    }
                }
                else { Debug.LogError(string.Format("Invalid team (null) for TeamID {0}", teamPool[i])); }
            }
        }
        else { Debug.LogError("Invalid teamPool (Null) -> no teams with expired timers recalled from OnMap"); }
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
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(Side.Authority);
        if (arrayOfActors.Length > 0)
        {
            int ability, arcID;
            foreach (Actor actor in arrayOfActors)
            {
                //get actors Ability
                ability = actor.datapoint2;
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
                        actorSlotID = Random.Range(0, GameManager.instance.actorScript.numOfOnMapActors);
                        MoveTeam(TeamPool.OnMap, teamData.Key, actorSlotID, node);
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
    public bool MoveTeam(TeamPool destinationPool, int teamID, int actorSlotID = -1, Node node = null)
    {
        Debug.Assert(teamID > -1 && teamID < GameManager.instance.dataScript.CheckNumOfTeams(), "Invalid teamID");
        Team team = GameManager.instance.dataScript.GetTeam(teamID);
        bool successFlag = true;
        if (team != null)
        {
            //
            // - - - Move - - -
            //
            switch (destinationPool)
            {
                case TeamPool.Reserve:
                    //adjust tallies
                    GameManager.instance.dataScript.AdjustTeamInfo(team.Arc.TeamArcID, TeamInfo.Reserve, +1);
                    GameManager.instance.dataScript.AdjustTeamInfo(team.Arc.TeamArcID, TeamInfo.InTransit, -1);
                    //pools
                    GameManager.instance.dataScript.AddTeamToPool(TeamPool.Reserve, teamID);
                    GameManager.instance.dataScript.RemoveTeamFromPool(TeamPool.InTransit, teamID);
                    //update team status
                    team.ResetTeamData(TeamPool.Reserve);
                    //confirmation
                    Debug.Log(string.Format("TeamManager: {0} {1}, ID {2}, moved to {3}{4}", team.Arc.name, team.Name, team.TeamID, destinationPool, "\n"));
                    break;
                case TeamPool.OnMap:
                    if (actorSlotID > -1 && actorSlotID < GameManager.instance.actorScript.numOfOnMapActors)
                    {
                        //Get Actor
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, Side.Authority);
                        if (actor != null)
                        {
                            if (actor.isLive == true)
                            {
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
                                                //update team stats
                                                team.NodeID = node.nodeID;
                                                team.ActorSlotID = actor.slotID;
                                                team.Timer = deployTime;
                                                team.TurnDeployed = GameManager.instance.turnScript.Turn;
                                                //confirmation
                                                string text = string.Format("{0} {1}, ID {2}, deployed to {3}, Node ID {4}", team.Arc.name, team.Name, team.TeamID,
                                                    destinationPool, node.nodeID);
                                                Debug.Log(string.Format("TeamManager: {0}{1}", text, "\n"));
                                                //message
                                                Message message = GameManager.instance.messageScript.TeamDeploy(text, node.nodeID, team.TeamID, actor.actorID);
                                                if (message != null) { GameManager.instance.dataScript.AddMessageNew(message); }
                                            }
                                            else
                                            {
                                                Debug.LogWarning(string.Format("Node Add team operation failed for \"{0} {1}\" (could be duplicate)", 
                                                    team.Arc.name, team.Name));
                                                successFlag = false;
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogWarning(string.Format("Unable to deploy {0} {1} to {2} as Actor {3}, slotID {4}, has insufficient ability{5}",
                                                team.Arc.name, team.Name, destinationPool, actor.arc.name, actorSlotID, "\n"));
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
                            }
                            else
                            {
                                Debug.LogWarning(string.Format("{0}, ID {1} can't be deployed to {2} as Actor {3}, slotID {4} isn't Live{5}", team.Arc.name, teamID, destinationPool,
                                  actor.actorName, actorSlotID, "\n"));
                                successFlag = false;
                            }
                        }
                        else
                        {
                            Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", actorSlotID));
                            successFlag = false;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid actor slotID \"{0}\"", actorSlotID));
                        successFlag = false;
                    }
                    break;
                case TeamPool.InTransit:
                    if (actorSlotID > -1 && actorSlotID < GameManager.instance.actorScript.numOfOnMapActors)
                    {
                        //Get Actor
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, Side.Authority);
                        if (actor != null)
                        {
                            if (actor.isLive == true)
                            {
                                if (node != null)
                                {
                                    //adjust tallies
                                    GameManager.instance.dataScript.AdjustTeamInfo(team.Arc.TeamArcID, TeamInfo.OnMap, -1);
                                    GameManager.instance.dataScript.AdjustTeamInfo(team.Arc.TeamArcID, TeamInfo.InTransit, +1);
                                    //pools
                                    GameManager.instance.dataScript.RemoveTeamFromPool(TeamPool.OnMap, teamID);
                                    GameManager.instance.dataScript.AddTeamToPool(TeamPool.InTransit, teamID);
                                    //remove from node list
                                    node.RemoveTeam(team.TeamID);
                                    //update team status
                                    team.ResetTeamData(TeamPool.InTransit);
                                    //remove team from actor list
                                    actor.RemoveTeam(team.TeamID);
                                    //confirmation
                                    Debug.Log(string.Format("TeamManager: {0} {1}, ID {2}, moved to {3}, from Node ID {4}{5}", team.Arc.name, team.Name, team.TeamID,
                                    destinationPool, node.nodeID, "\n"));
                                }
                                else
                                {
                                    Debug.LogError("Invalid node (Null) for InTransit -> move Cancelled");
                                    successFlag = false;
                                }
                            }
                            else
                            {
                                Debug.LogWarning(string.Format("{0}, ID {1} can't be deployed to {2} as Actor {3}, slotID {4} isn't Live{5}", team.Arc.name, teamID, destinationPool,
                                  actor.actorName, actorSlotID, "\n"));
                                successFlag = false;
                            }
                        }
                        else
                        {
                            Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", actorSlotID));
                            successFlag = false;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid actor slotID \"{0}\"", actorSlotID));
                        successFlag = false;
                    }
                    break;
                default:
                    Debug.LogError(string.Format("Invalid pool \"{0}\"", destinationPool));
                    successFlag = false;
                    break;
            }
            if (successFlag == false)
            { return false; }

        }
        else
        {
            Debug.LogError(string.Format("Invalid Team (null) for TeamID {0}", teamID)); return false; }
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
    /// Debug method to show which actors xurrently have which teams OnMap
    /// </summary>
    /// <returns></returns>
    public string GetTeamActorAnalysis()
    {
        List<int> listOfTeams = new List<int>();
        StringBuilder builder = new StringBuilder();
        builder.Append(" OnMap Teams by Actor");
        builder.AppendLine();
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(Side.Authority);
        foreach(Actor actor in arrayOfActors)
        {
            builder.AppendLine();
            builder.Append(string.Format("{0}  Ability {1}", actor.arc.name, actor.datapoint2));
            builder.AppendLine();
            listOfTeams.Clear();
            listOfTeams.AddRange(actor.GetTeams());
            if (listOfTeams.Count > 0)
            {
                //loop teams
                for (int i = 0; i < listOfTeams.Count; i++)
                {
                    Team team = GameManager.instance.dataScript.GetTeam(listOfTeams[i]);
                    if (team != null)
                    {
                        if (i > 0) { builder.AppendLine(); }
                        builder.Append(string.Format("{0} {1}", team.Arc.name, team.Name));
                    }
                }

            }
            else
            { builder.Append("none"); }
            builder.AppendLine();
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
                    teamData.Value.Pool, teamData.Value.NodeID, teamData.Value.Timer, teamData.Value.ActorSlotID));
                builder.AppendLine();
            }
        }
        return builder.ToString();
    }

    /// <summary>
    /// Recall Team (Authority): sets up ModalGenericPicker class and triggers event: ModalGenericEvent.cs -> SetGenericPicker()
    /// does this for 1, 2 or 3 teams present at the node, immediate outcome window if none present.
    /// </summary>
    /// <param name="details"></param>
    private void InitialiseGenericPickerRecall(int nodeID)
    {
        bool errorFlag = false;
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        //does the node have any teams that can be recalled?
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            //double check to see if there are teams present at the node
            List<Team> listOfTeams = node.GetTeams();
            if (listOfTeams != null && listOfTeams.Count > 0)
            {
                genericDetails.returnEvent = EventType.GenericTeamRecall;
                genericDetails.side = Side.Authority;
                genericDetails.nodeID = nodeID;
                genericDetails.actorSlotID = -1;
                //picker text
                genericDetails.textTop = string.Format("{0}Recall{1} {2}team{3}", colourEffect, colourEnd, colourNormal, colourEnd);
                genericDetails.textMiddle = string.Format("{0}Procedures are in place to Recall a team. Any team effects will be cancelled. We await your orders{1}", 
                    colourNormal, colourEnd);
                genericDetails.textBottom = "Click on a Team to Select. Press CONFIRM to Recall team. Mouseover teams for more information.";
                //loop teams present at node
                int turnsAgo, deployedTeams; 
                string dataColour;
                for (int i = 0; i < listOfTeams.Count; i++)
                {
                    Team team = listOfTeams[i];
                    if (team != null)
                    {
                        //option details
                        GenericOptionDetails optionDetails = new GenericOptionDetails();
                        optionDetails.optionID = team.TeamID;
                        optionDetails.text = team.Arc.name.ToUpper();
                        optionDetails.sprite = team.Arc.sprite;
                        //tooltip -> TO DO
                        GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                        tooltipDetails.textHeader = string.Format("{0}{1}{2} {3}{4}{5}", colourTeam, team.Arc.name.ToUpper(), colourEnd, colourNormal, team.Name, colourEnd);
                        turnsAgo = GameManager.instance.turnScript.Turn - team.TurnDeployed;
                        if (team.Timer > 0) { dataColour = colourGood; } else { dataColour = colourBad; }
                        tooltipDetails.textMain = string.Format("Deployed {0}{1}{2} turn{3} ago and will be auto-recalled in {4}{5}{6} turn{7}", 
                            dataColour, turnsAgo, colourEnd, turnsAgo != 1 ? "s" : "", dataColour, team.Timer, colourEnd, team.Timer != 1 ? "s" : "");
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(team.ActorSlotID, Side.Authority);
                        if (actor != null)
                        {
                            deployedTeams = actor.CheckNumOfTeams();
                            if (deployedTeams < actor.datapoint2) { dataColour = colourGood; } else { dataColour = colourBad; }
                            tooltipDetails.textDetails = string.Format("{0}Inserted by {1} of {2}{3}{4}{5}{6}. They have deployed {7}{8}{9}{10}{11} of {12}{13}{14}{15}{16} possible teams{17}", 
                                colourNormal, (AuthorityActor)GameManager.instance.GetMetaLevel(), colourEnd, colourActor, actor.arc.name, colourEnd, colourNormal, 
                                colourEnd, dataColour, deployedTeams, colourEnd, colourNormal, colourEnd, 
                                dataColour, actor.datapoint2, colourEnd, colourNormal, colourEnd);
                        }
                        else { Debug.LogError(string.Format("Invalid actor (Null) fro team.ActorSlotID {0}", team.ActorSlotID)); }
                        //add to master arrays
                        genericDetails.arrayOfOptions[i] = optionDetails;
                        genericDetails.arrayOfTooltips[i] = tooltipDetails;
                    }
                    else { Debug.LogError(string.Format("Invalid team (Null) for listOfTeams[{0}]", i)); }
                    //check that limit hasn't been exceeded (max 3 options)
                    if (i > 2)
                    {
                        Debug.LogError(string.Format("Invalid number of Teams (more than 3) at NodeId {0}", nodeID));
                        break;
                    }
                }
            }
            else
            {
                Debug.LogError(string.Format("Invalid listOfTeams (Empty or Null) for NodeID {0}", nodeID));
                errorFlag = true;
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Node (null) for nodeID {0}", nodeID));
            errorFlag = true;
        }
        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.textTop = "There has been an error in communication and No teams can be Recalled.";
            outcomeDetails.textBottom = "Heads will roll!";
            outcomeDetails.side = Side.Authority;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails);
        }
    }

    /// <summary>
    /// Neutralise Team (Resistance): sets up ModalGenericPicker class and triggers event: ModalGenericEvent.cs -> SetGenericPicker()
    /// does this for 1, 2 or 3 teams present at the node, immediate outcome window if none present.
    /// </summary>
    /// <param name="details"></param>
    private void InitialiseGenericPickerNeutralise(ModalActionDetails details)
    {
        bool errorFlag = false;
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        //does the node have any teams that can be neutralised?
        Node node = GameManager.instance.dataScript.GetNode(details.NodeID);
        if (node != null)
        {
            //double check to see if there are teams present at the node
            List<Team> listOfTeams = node.GetTeams();
            if (listOfTeams != null && listOfTeams.Count > 0)
            {
                genericDetails.returnEvent = EventType.GenericNeutraliseTeam;
                genericDetails.side = Side.Resistance;
                genericDetails.nodeID = details.NodeID;
                genericDetails.actorSlotID = details.ActorSlotID;
                //picker text
                genericDetails.textTop = string.Format("{0}Neutralise{1} {2}team{3}", colourEffect, colourEnd, colourNormal, colourEnd);
                genericDetails.textMiddle = string.Format("{0}Operatives are in place to Neutralise a team. The team will be forced to retire immediately{1}",
                    colourNormal, colourEnd);
                genericDetails.textBottom = "Click on a Team to Select. Press CONFIRM to Neutralise team. Mouseover teams for more information.";
                //loop teams present at node
                int turnsAgo;
                string dataColour;
                for (int i = 0; i < listOfTeams.Count; i++)
                {
                    Team team = listOfTeams[i];
                    if (team != null)
                    {
                        //option details
                        GenericOptionDetails optionDetails = new GenericOptionDetails();
                        optionDetails.optionID = team.TeamID;
                        optionDetails.text = team.Arc.name.ToUpper();
                        optionDetails.sprite = team.Arc.sprite;
                        //tooltip -> TO DO
                        GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                        tooltipDetails.textHeader = string.Format("{0}{1}{2} {3}{4}{5}", colourTeam, team.Arc.name.ToUpper(), colourEnd, colourNormal, team.Name, colourEnd);
                        turnsAgo = GameManager.instance.turnScript.Turn - team.TurnDeployed;
                        if (team.Timer > 0) { dataColour = colourGood; } else { dataColour = colourBad; }
                        tooltipDetails.textMain = string.Format("Will be immediately removed from the location.");
                        tooltipDetails.textDetails = string.Format("{0}Automatic success{1}", colourEffect, colourEnd);
                        //add to master arrays
                        genericDetails.arrayOfOptions[i] = optionDetails;
                        genericDetails.arrayOfTooltips[i] = tooltipDetails;
                    }
                    else { Debug.LogError(string.Format("Invalid team (Null) for listOfTeams[{0}]", i)); }
                    //check that limit hasn't been exceeded (max 3 options)
                    if (i > 2)
                    {
                        Debug.LogError(string.Format("Invalid number of Teams (more than 3) at NodeId {0}", details.NodeID));
                        break;
                    }
                }
            }
            else
            {
                Debug.LogError(string.Format("Invalid listOfTeams (Empty or Null) for NodeID {0}", details.NodeID));
                errorFlag = true;
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Node (null) for nodeID {0}", details.NodeID));
            errorFlag = true;
        }
        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = Side.Resistance;
            outcomeDetails.textTop = "There has been an error in communication and No teams can be Neutralised.";
            outcomeDetails.textBottom = "Heads will roll!";
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails);
        }
    }

    /// <summary>
    /// 'Recall Team' node action. Implements action.
    /// </summary>
    /// <param name="teamID"></param>
    public void ProcessRecallTeam(GenericReturnData data)
    {
        bool successFlag = true;
        if (data.optionID > -1)
        {
            //get currently selected node
            string textTop = "Unknown";
            string textBottom = "Unknown";
            if (data.nodeID != -1)
            {
                Team team = GameManager.instance.dataScript.GetTeam(data.optionID);
                if (team != null)
                {
                    Sprite sprite = team.Arc.sprite;
                    Node node = GameManager.instance.dataScript.GetNode(data.nodeID);
                    if (node != null)
                    {
                        //need to do prior to move team as data will be reset
                        textTop = GameManager.instance.effectScript.SetTopText(team.TeamID, false);
                        textBottom = "The team will spend one turn in Transit and be available thereafter";
                        int actorID = -1;
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(team.ActorSlotID, Side.Authority);
                        if (actor != null) { actorID = actor.actorID; }
                        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", team.ActorSlotID)); }
                        if (MoveTeam(TeamPool.InTransit, team.TeamID, team.ActorSlotID, node) == true)
                        {
                            //message
                            string text = string.Format("{0} {1}, ID {2}, withdrawn early from {3}, ID {4}", team.Arc.name, team.Name, team.TeamID, 
                                node.Name, node.nodeID);
                            Message message = GameManager.instance.messageScript.TeamWithdraw(text, data.nodeID, team.TeamID, actorID);
                            if (message != null) { GameManager.instance.dataScript.AddMessageNew(message); }
                            Debug.Log(string.Format("TeamManager: {0}{1}", text, "\n"));
                        }
                        else
                        {
                            //Problem occurred, team not removed
                            textTop = "Problem occured, team NOT removed";
                            textBottom = "Who did this? Speak up and step forward immediately!";
                            successFlag = false;
                        }
                        //OUTCOME Window
                        ModalOutcomeDetails details = new ModalOutcomeDetails();
                        details.textTop = textTop;
                        details.textBottom = textBottom;
                        details.sprite = sprite;
                        details.side = Side.Authority;
                        if (successFlag == true)
                        { details.isAction = true; }
                        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
                    }
                    else { Debug.LogError(string.Format("Invalid node (Null) for NodeID {0}", data.nodeID)); }
                }
                else { Debug.LogError(string.Format("Invalid Team (Null) for teamID {0}", data.optionID)); }
            }
            else { Debug.LogError("Highlighted node invalid (default '-1' value)"); }
        }
        else { Debug.LogError("Invalid TeamID (default '-1')"); }
    }


    /// <summary>
    /// 'Neutralise Team' Resistance node action. Implements action.
    /// </summary>
    /// <param name="teamID"></param>
    public void ProcessNeutraliseTeam(GenericReturnData data)
    {
        bool successFlag = true;
        if (data.optionID > -1)
        {
            //get currently selected node
            if (data.nodeID != -1)
            {
                Team team = GameManager.instance.dataScript.GetTeam(data.optionID);
                if (team != null)
                {
                    Sprite sprite = team.Arc.sprite;
                    Node node = GameManager.instance.dataScript.GetNode(data.nodeID);
                    if (node != null)
                    {
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, Side.Resistance);
                        if (actor != null)
                        {
                            StringBuilder builderTop = new StringBuilder();
                            StringBuilder builderBottom = new StringBuilder();

                            if (MoveTeam(TeamPool.InTransit, team.TeamID, team.ActorSlotID, node) == true)
                            {
                                //message (notification to Authority Side)
                                string text = string.Format("{0} {1}, ID {2}, neutralised at {3}, ID {4}", team.Arc.name, team.Name, team.TeamID,
                                    node.Name, node.nodeID);
                                Message message = GameManager.instance.messageScript.TeamNeutralise(text, data.nodeID, team.TeamID, actor.actorID);
                                if (message != null) { GameManager.instance.dataScript.AddMessageNew(message); }
                                Debug.Log(string.Format("TeamManager: {0}{1}", text, "\n"));
                                //team successfully removed
                                builderTop.Append(string.Format("{0}Operatives have succeeded!{1}", colourNormal, colourEnd));
                                builderBottom.Append(string.Format("{0}{1}{2}{3} team removed{4}", colourTeam, team.Arc.name.ToUpper(), colourEnd, 
                                    colourEffect, colourEnd));

                                //Process any other effects, if Neutralise was successfull, ignore otherwise
                                Action action = actor.arc.nodeAction;
                                List<Effect> listOfEffects = action.GetEffects();
                                if (listOfEffects.Count > 0)
                                {
                                    foreach (Effect effect in listOfEffects)
                                    {
                                        if (effect.ignoreEffect == false)
                                        {
                                            EffectReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, actor);
                                            if (effectReturn != null)
                                            {
                                                builderTop.AppendLine();
                                                builderTop.Append(effectReturn.topText);
                                                builderBottom.AppendLine();
                                                builderBottom.Append(effectReturn.bottomText);
                                            }
                                            else { Debug.LogError("Invalid effectReturn (Null)"); }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Problem occurred, team not removed
                                builderTop.Append("Problem occured, team NOT removed");
                                builderBottom.Append("Who did this? Speak up and step forward immediately!");
                                successFlag = false;
                            }
                            //OUTCOME Window
                            ModalOutcomeDetails details = new ModalOutcomeDetails();
                            details.textTop = builderTop.ToString();
                            details.textBottom = builderBottom.ToString();
                            details.sprite = sprite;
                            details.side = Side.Resistance;
                            if (successFlag == true)
                            { details.isAction = true; }
                            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
                        }
                        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", data.actorSlotID)); }
                    }
                    else { Debug.LogError(string.Format("Invalid node (Null) for NodeID {0}", data.nodeID)); }
                }
                else { Debug.LogError(string.Format("Invalid Team (Null) for teamID {0}", data.optionID)); }
            }
            else { Debug.LogError("Highlighted node invalid (default '-1' value)"); }
        }
        else { Debug.LogError("Invalid TeamID (default '-1')"); }
    }

    /// <summary>
    /// Implements PERMANENT team effects on nodes at completion of teams OnMap Timer.
    /// Node and Team are assumed to be checked for Null by the parent method
    /// </summary>
    /// <param name="team"></param>
    /// <param name="node"></param>
    private void ProcessTeamEffect(Team team, Node node, Actor actor)
    {
        List<Effect> listOfEffects = team.Arc.listOfEffects;
        switch(team.Arc.type)
        {
            case TeamType.Control:
            case TeamType.Civil:
            case TeamType.Media:
                if (listOfEffects != null)
                {
                    foreach(Effect effect in listOfEffects)
                    {
                        GameManager.instance.effectScript.ProcessEffect(effect, node, actor);
                        string text = string.Format("{0} {1} effect: {2} at \"{3}\", ID {4}", team.Arc.name, team.Name, effect.description, node.Name, node.nodeID);
                        Message message = GameManager.instance.messageScript.TeamEffect(text, node.nodeID, team.TeamID);
                        if (message != null) { GameManager.instance.dataScript.AddMessageNew(message); }
                    }
                }
                break;
            case TeamType.Probe:

                break;
            case TeamType.Spider:
                if (node.isSpider == false)
                {
                    //add spider
                    node.isSpider = true;
                    string text = string.Format("{0} {1} effect: Spider inserted at \"{2}\", ID {3}", team.Arc.name, team.Name, node.Name, node.nodeID);
                    Message message = GameManager.instance.messageScript.TeamEffect(text, node.nodeID, team.TeamID);
                    if (message != null) { GameManager.instance.dataScript.AddMessageNew(message); }
                }
                break;
            case TeamType.Erasure:

                break;
            default:
                Debug.LogError(string.Format("Invalid team Arc name \"{0}\"", team.Arc.name));
                break;
        }
    }

    //place new method above here
}
