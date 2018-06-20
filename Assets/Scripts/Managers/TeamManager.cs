using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using modalAPI;
using packageAPI;
using System.Text;

/// <summary>
/// handles all team related matters
/// </summary>
public class TeamManager : MonoBehaviour
{
    
    [Tooltip("The maximum number of teams that may be present at a node at any one time")]
    [Range(1, 4)] public int maxTeamsAtNode = 3;
    [HideInInspector] public int minTeamsAtNode = 0;
    [Tooltip("How long a team is deployed for before automatically being recalled")]
    [Range(1, 4)] public int deployTime = 3;

    [Header("Team Node Effects")]
    [Tooltip("The increase to node security due to the presence of a Control Team")]
    [Range(1, 4)] public int controlNodeEffect = 2;
    [Tooltip("The increase to node stability due to the presence of a Civil Team")]
    [Range(1, 4)] public int civilNodeEffect = 2;
    [Tooltip("The decrease to node support due to the presence of a Media Team. Note that this is converted to a negative number by Node.cs")]
    [Range(1, 4)] public int mediaNodeEffect = 2;

    [Header("Team Type Priorities")]
    [Tooltip("Team Arcs that are High priority (order doesn't matter they are all assumed to the same). Used to help figure out which team is needed. Don't duplicate in other lists")]
    public List<TeamArc> listOfTeamPrioritiesHigh;
    [Tooltip("Team Arcs that are Medium priority (order doesn't matter they are all assumed to the same). Used to help figure out which team is needed. Don't duplicate in other lists")]
    public List<TeamArc> listOfTeamPrioritiesMedium;
    [Tooltip("Team Arcs that are Low priority (order doesn't matter they are all assumed to the same). Used to help figure out which team is needed. Don't duplicate in other lists")]
    public List<TeamArc> listOfTeamPrioritiesLow;
    //fast access fields
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;

    private string colourEffect;
    private string colourTeam;
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
        //fast acess fields -> BEFORE InitialiseTeams below
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        //Teams
        int teamArcCount = 0;
        Debug.Assert(listOfTeamPrioritiesHigh != null, "Invalid listOfTeamPrioritiesHigh (Null)");
        Debug.Assert(listOfTeamPrioritiesHigh.Count > 0, "listOfTeamPrioritiesHigh has no records");
        Debug.Assert(listOfTeamPrioritiesMedium != null, "Invalid listOfTeamPrioritiesMedium (Null)");
        Debug.Assert(listOfTeamPrioritiesMedium.Count > 0, "listOfTeamPrioritiesMedium has no records");
        Debug.Assert(listOfTeamPrioritiesLow != null, "Invalid listOfTeamPrioritiesLow (Null)");
        Debug.Assert(listOfTeamPrioritiesLow.Count > 0, "listOfTeamPrioritiesLow has no records");
        teamArcCount += listOfTeamPrioritiesHigh.Count + listOfTeamPrioritiesMedium.Count + listOfTeamPrioritiesLow.Count;
        Debug.Assert(teamArcCount == GameManager.instance.dataScript.CheckNumOfTeamArcs(), "Mismatched count of team Priority Arcs (should be same # as num of unique Team Arcs");
        InitialiseTeams();
        /*SeedTeamsOnMap();     //DEBUG*/
        //event Listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "TeamManager");
        EventManager.instance.AddListener(EventType.EndTurnEarly, OnEvent, "TeamManager");
        EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent, "TeamManager");
        EventManager.instance.AddListener(EventType.RecallTeamAction, OnEvent, "TeamManager");
        EventManager.instance.AddListener(EventType.GenericTeamRecall, OnEvent, "TeamManager");
        EventManager.instance.AddListener(EventType.NeutraliseTeamAction, OnEvent, "TeamManager");
        EventManager.instance.AddListener(EventType.GenericNeutraliseTeam, OnEvent, "TeamManager");
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
            case EventType.EndTurnEarly:
                EndTurn();
                break;
            case EventType.StartTurnLate:
                StartTurnLate();
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
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourTeam = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourActor = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// End turn activity -> Event driven, decrement all timers in OnMap pool
    /// </summary>
    private void EndTurn()
    {
        //set turnSide 
        GameManager.instance.turnScript.currentSide = globalAuthority;
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
                    team.timer--;
                }
                else { Debug.LogError(string.Format("Invalid team (null) for TeamID {0}", teamPool[i])); }
            }
        }
        else { Debug.LogError("Invalid teamPool (Null) -> no team timers decremented"); }
    }


    /// <summary>
    /// Start turn Late activity -> Event driven. Team Management (inTransit, OnMap timers)
    /// </summary>
    private void StartTurnLate()
    {
        List<int> teamPool = new List<int>();
        //set turnSide 
        GameManager.instance.turnScript.currentSide = globalAuthority;
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
                    if (GameManager.instance.sideScript.resistanceCurrent == SideState.Player)
                    { MoveTeam(TeamPool.Reserve, team.teamID, team.actorSlotID); }
                    else { MoveTeamAI(TeamPool.Reserve, team.teamID); }
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
                    if (team.timer < 0)
                    {

                        //Timer expired, team automatically recalled to InTransit pool
                        Node node = GameManager.instance.dataScript.GetNode(team.nodeID);
                        if (node != null)
                        {
                            if (GameManager.instance.sideScript.authorityOverall == SideState.Player)
                            {
                                //Human Authority Player
                                Actor actor = GameManager.instance.dataScript.GetCurrentActor(team.actorSlotID, globalAuthority);
                                MoveTeam(TeamPool.InTransit, team.teamID, team.actorSlotID, node);
                                if (actor != null)
                                {
                                    //Permanent Team effect activated for node
                                    ProcessTeamEffect(team, node, actor);
                                    //message
                                    string text = string.Format("{0} {1}, ID {2}, recalled from {3}, ID {4}", team.arc.name, team.teamName, team.teamID, node.nodeName, node.nodeID);
                                    Message message = GameManager.instance.messageScript.TeamAutoRecall(text, node.nodeID, team.teamID, actor.actorID);
                                    if (message != null) { GameManager.instance.dataScript.AddMessage(message); }
                                }
                                else
                                {
                                    //error if there should be an actor in the slot
                                    if (GameManager.instance.dataScript.CheckActorSlotStatus(team.actorSlotID, globalAuthority) == true)
                                    { Debug.LogError(string.Format("Invalid actor (null) for actorSlotID {0}", team.actorSlotID)); }
                                }
                            }
                            else
                            {
                                //AI Authority player
                                MoveTeamAI(TeamPool.InTransit, team.teamID, node);
                                //Permanent Team effect activated for node
                                ProcessTeamEffect(team, node, null);
                                //message
                                string text = string.Format("{0} {1}, ID {2}, recalled from {3}, ID {4}", team.arc.name, team.teamName, team.teamID, node.nodeName, node.nodeID);
                                Message message = GameManager.instance.messageScript.TeamAutoRecall(text, node.nodeID, team.teamID, -1);
                                if (message != null) { GameManager.instance.dataScript.AddMessage(message); }
                            }
                        }
                        else { Debug.LogError(string.Format("Invalid node (null) for TeamID {0} and team.NodeID {1}", teamPool[i], team.nodeID)); }
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

        //add teams depending on who is in charge of the authority side
        switch (GameManager.instance.sideScript.authorityOverall)
        {
            case SideState.Player:
                //Add extra teams ([edit] No, see below [/edit] 
                Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalAuthority);
                if (arrayOfActors.Length > 0)
                {
                    int numToDeploy, arcID;
                    foreach (Actor actor in arrayOfActors)
                    {
                        //Give one team fixed of the preferred type, NOT an amount equal to actor ability
                        numToDeploy = 1;
                        //get preferred team
                        arcID = actor.arc.preferredTeam.TeamArcID;
                        //add the ability number of teams to the reserve
                        GameManager.instance.dataScript.AdjustTeamInfo(arcID, TeamInfo.Reserve, numToDeploy);
                        GameManager.instance.dataScript.AdjustTeamInfo(arcID, TeamInfo.Total, numToDeploy);
                    }
                }
                else { Debug.LogError("Invalid arrayOfActors (Empty)"); }
                break;
            case SideState.AI:
                //Add extra teams, two high priority, two medium priority (if # nodes < 20, then 1 of each)
                if (listOfTeamArcIDs != null && listOfTeamArcIDs.Count > 0)
                {
                    int arcID, index;
                    //map size scaling
                    int numOfTeams = 2;
                    if (GameManager.instance.dataScript.CheckNumOfNodes() < 20) { numOfTeams = 1; }
                    //copy of list to prevent duplicate selections
                    List<TeamArc> tempList = new List<TeamArc>(listOfTeamPrioritiesHigh);
                    //High priority teams (random choice)
                    numOfTeams = Mathf.Min(numOfTeams, tempList.Count);
                    for (int i = 0; i < numOfTeams; i++)
                    {
                        index = Random.Range(0, tempList.Count);
                        arcID = tempList[index].TeamArcID;
                        if (arcID >= 0)
                        {
                            GameManager.instance.dataScript.AdjustTeamInfo(arcID, TeamInfo.Reserve, +1);
                            GameManager.instance.dataScript.AdjustTeamInfo(arcID, TeamInfo.Total, +1);
                            tempList.RemoveAt(index);
                        }
                        else { Debug.LogWarningFormat("Invalid High priority teamArcID \"{0}\"", arcID); }
                    }
                    //Medium priority teams (random choice)
                    tempList.Clear();
                    tempList = new List<TeamArc>(listOfTeamPrioritiesMedium);
                    //map size scaling
                    numOfTeams = 2;
                    if (GameManager.instance.dataScript.CheckNumOfNodes() < 20) { numOfTeams = 1; }
                    numOfTeams = Mathf.Min(numOfTeams, tempList.Count);
                    for (int i = 0; i < numOfTeams; i++)
                    {
                        index = Random.Range(0, tempList.Count);
                        arcID = tempList[index].TeamArcID;
                        if (arcID >= 0)
                        {
                            GameManager.instance.dataScript.AdjustTeamInfo(arcID, TeamInfo.Reserve, +1);
                            GameManager.instance.dataScript.AdjustTeamInfo(arcID, TeamInfo.Total, +1);
                            tempList.RemoveAt(index);
                        }
                        else { Debug.LogWarningFormat("Invalid Medium priority teamArcID \"{0}\"", arcID); }
                    }

                }
                else { Debug.LogError("Invalid listOfTeamArcIDs (Null or Empty) -> initial team setup cancelled"); }
                break;
        }
        //create actual teams and fill the reserve pool based on the number of teams decided upon above
        int numToCreate;
        /*Message message;*/
        foreach (int teamArcID in listOfTeamArcIDs)
        {
            //how many present? (only check reserve as at start of game that's where all teams are)
            numToCreate = GameManager.instance.dataScript.CheckTeamInfo(teamArcID, TeamInfo.Reserve);
            //should never be more than 2
            Debug.Assert(numToCreate < 3, string.Format("Excessive number ({0}) of {1} teams", numToCreate, GameManager.instance.dataScript.GetTeamArc(teamArcID).name));
            Debug.LogFormat("TeamManager.cs -> InitialiseTeams: {0} {1} team{2} created{3}", numToCreate, GameManager.instance.dataScript.GetTeamArc(teamArcID).name,
                numToCreate != 1 ? "s" : "", "\n");
            //create teams
            for (int i = 0; i < numToCreate; i++)
            {
                Team team = new Team(teamArcID, i);
                Debug.Assert(team != null, "Invalid team (Null)");
            }
        }
    }


    /// <summary>
    /// Debug method -> each team in reserve pool at game start has a chance of being deployed
    /// </summary>
    public void SeedTeamsOnMap()
    {
        List<Node> listOfNodes = new List<Node>(GameManager.instance.dataScript.GetDictOfNodes().Values);
        Dictionary<int, Team> dictOfTeams = GameManager.instance.dataScript.GetDictOfTeams();
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
                        actorSlotID = Random.Range(0, GameManager.instance.actorScript.maxNumOfOnMapActors);
                        //only do so if Actor is present in slot (player might start level with less than full complement of actors)
                        if (GameManager.instance.dataScript.CheckActorSlotStatus(actorSlotID, globalAuthority) == true)
                        { MoveTeam(TeamPool.OnMap, teamData.Key, actorSlotID, node); }
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
                    GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve, +1);
                    GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.InTransit, -1);
                    //pools
                    GameManager.instance.dataScript.AddTeamToPool(TeamPool.Reserve, teamID);
                    GameManager.instance.dataScript.RemoveTeamFromPool(TeamPool.InTransit, teamID);
                    //update team status
                    team.ResetTeamData(TeamPool.Reserve);
                    //confirmation
                    Debug.Log(string.Format("TeamManager: {0} {1}, ID {2}, moved to {3}{4}", team.arc.name, team.teamName, team.teamID, destinationPool, "\n"));
                    break;
                case TeamPool.OnMap:
                    if (actorSlotID > -1 && actorSlotID < GameManager.instance.actorScript.maxNumOfOnMapActors)
                    {
                        //Get Actor
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, globalAuthority);
                        if (actor != null)
                        {
                            if (actor.Status == ActorStatus.Active)
                            {
                                if (node != null)
                                {
                                    //a team available in the reserve pool?
                                    if (GameManager.instance.dataScript.CheckTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve) > 0)
                                    {
                                        //check if actor has capacity to deploy another team
                                        if (actor.CheckCanDeployTeam() == true)
                                        {
                                            if (node.AddTeam(team, actorSlotID) == true)
                                            {
                                                //adjust tallies for onMap
                                                GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.OnMap, +1);
                                                GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve, -1);
                                                //pools
                                                GameManager.instance.dataScript.AddTeamToPool(TeamPool.OnMap, teamID);
                                                GameManager.instance.dataScript.RemoveTeamFromPool(TeamPool.Reserve, teamID);
                                                //add team to Actor list
                                                actor.AddTeam(team.teamID);
                                                //update team stats
                                                team.nodeID = node.nodeID;
                                                team.actorSlotID = actor.actorSlotID;
                                                team.timer = deployTime;
                                                team.turnDeployed = GameManager.instance.turnScript.Turn;
                                                //confirmation
                                                string text = string.Format("{0} {1}, ID {2}, deployed to {3}, Node ID {4}", team.arc.name, team.teamName, team.teamID,
                                                    destinationPool, node.nodeID);
                                                Debug.Log(string.Format("TeamManager: {0}{1}", text, "\n"));
                                                //message
                                                Message message = GameManager.instance.messageScript.TeamDeploy(text, node.nodeID, team.teamID, actor.actorID);
                                                if (message != null) { GameManager.instance.dataScript.AddMessage(message); }
                                            }
                                            else
                                            {
                                                Debug.LogWarning(string.Format("Node Add team operation failed for \"{0} {1}\" (could be duplicate)",
                                                    team.arc.name, team.teamName));
                                                successFlag = false;
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogWarning(string.Format("Unable to deploy {0} {1} to {2} as Actor {3}, slotID {4}, has insufficient ability{5}",
                                                team.arc.name, team.teamName, destinationPool, actor.arc.name, actorSlotID, "\n"));
                                            successFlag = false;
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogWarning(string.Format("Not enough {0} teams present. Move cancelled", team.arc.name));
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
                                Debug.LogWarning(string.Format("{0}, ID {1} can't be deployed to {2} as Actor {3}, slotID {4} isn't Live{5}", team.arc.name, teamID, destinationPool,
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
                    if (actorSlotID > -1 && actorSlotID < GameManager.instance.actorScript.maxNumOfOnMapActors)
                    {
                        //Get Actor
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, globalAuthority);
                        if (actor != null)
                        {
                            if (actor.Status == ActorStatus.Active)
                            {
                                if (node != null)
                                {
                                    //adjust tallies
                                    GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.OnMap, -1);
                                    GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.InTransit, +1);
                                    //pools
                                    GameManager.instance.dataScript.RemoveTeamFromPool(TeamPool.OnMap, teamID);
                                    GameManager.instance.dataScript.AddTeamToPool(TeamPool.InTransit, teamID);
                                    //remove from node list
                                    node.RemoveTeam(team.teamID);
                                    //update team status
                                    team.ResetTeamData(TeamPool.InTransit);
                                    //remove team from actor list
                                    actor.RemoveTeam(team.teamID);
                                    //confirmation
                                    Debug.Log(string.Format("TeamManager: {0} {1}, ID {2}, moved to {3}, from Node ID {4}{5}", team.arc.name, team.teamName, team.teamID,
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
                                Debug.LogWarning(string.Format("{0}, ID {1} can't be deployed to {2} as Actor {3}, slotID {4} isn't Live{5}", team.arc.name, teamID, destinationPool,
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
    /// handles all admin for AI moving a team from one pool to another. Assumed movement direction is 'Reserve -> OnMap -> InTransit -> Reserve'
    /// Takes care of all checks, eg. enough teams present in reserve for one to move to the map
    /// AI Teams aren't associated with Actors
    /// only use the node parameter if the team is moving 'OnMap' (it's moving to a specific node)
    /// </summary>
    /// <param name="destinationPool"></param>
    /// <param name="teamID"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool MoveTeamAI(TeamPool destinationPool, int teamID, Node node = null)
    {
        Debug.Assert(teamID > -1 && teamID < GameManager.instance.dataScript.CheckNumOfTeams(), "Invalid teamID");
        Team team = GameManager.instance.dataScript.GetTeam(teamID);
        bool successFlag = true;
        int actorID = -1;
        if (team != null)
        {
            //
            // - - - Move - - -
            //
            switch (destinationPool)
            {
                case TeamPool.Reserve:
                    //adjust tallies
                    GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve, +1);
                    GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.InTransit, -1);
                    //pools
                    GameManager.instance.dataScript.AddTeamToPool(TeamPool.Reserve, teamID);
                    GameManager.instance.dataScript.RemoveTeamFromPool(TeamPool.InTransit, teamID);
                    //update team status
                    team.ResetTeamData(TeamPool.Reserve);
                    //confirmation
                    Debug.Log(string.Format("TeamManager: {0} {1}, ID {2}, moved to {3}{4}", team.arc.name, team.teamName, team.teamID, destinationPool, "\n"));
                    break;
                case TeamPool.OnMap:
                    if (node != null)
                    {
                        //a team available in the reserve pool?
                        if (GameManager.instance.dataScript.CheckTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve) > 0)
                        {
                            if (node.AddTeam(team, actorID) == true)
                            {

                                //adjust tallies for onMap
                                GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.OnMap, +1);
                                GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve, -1);
                                //pools
                                GameManager.instance.dataScript.AddTeamToPool(TeamPool.OnMap, teamID);
                                GameManager.instance.dataScript.RemoveTeamFromPool(TeamPool.Reserve, teamID);
                                //update team stats
                                team.nodeID = node.nodeID;
                                team.actorSlotID = -1;
                                team.timer = deployTime;
                                team.turnDeployed = GameManager.instance.turnScript.Turn;
                                //confirmation
                                string text = string.Format("{0} {1}, ID {2}, deployed to {3}, Node ID {4}", team.arc.name, team.teamName, team.teamID,
                                    destinationPool, node.nodeID);
                                /*Debug.Log(string.Format("TeamManager: {0}{1}", text, "\n"));*/
                                //message
                                Message message = GameManager.instance.messageScript.TeamDeploy(text, node.nodeID, team.teamID, actorID);
                                if (message != null) { GameManager.instance.dataScript.AddMessage(message); }
                            }
                            else
                            {
                                Debug.LogWarning(string.Format("Node Add team operation failed for \"{0} {1}\" (could be duplicate)",
                                    team.arc.name, team.teamName));
                                successFlag = false;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Invalid node (Null) for OnMap -> move Cancelled");
                        successFlag = false;
                    }
                    break;
                case TeamPool.InTransit:
                    if (node != null)
                    {
                        //adjust tallies
                        GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.OnMap, -1);
                        GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.InTransit, +1);
                        //pools
                        GameManager.instance.dataScript.RemoveTeamFromPool(TeamPool.OnMap, teamID);
                        GameManager.instance.dataScript.AddTeamToPool(TeamPool.InTransit, teamID);
                        //remove from node list
                        node.RemoveTeam(team.teamID);
                        //update team status
                        team.ResetTeamData(TeamPool.InTransit);
                        //confirmation
                        Debug.Log(string.Format("TeamManager: {0} {1}, ID {2}, moved to {3}, from Node ID {4}{5}", team.arc.name, team.teamName, team.teamID,
                        destinationPool, node.nodeID, "\n"));
                    }
                    else
                    {
                        Debug.LogError("Invalid node (Null) for InTransit -> move Cancelled");
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
        { Debug.LogError(string.Format("Invalid Team (null) for TeamID {0}", teamID)); return false; }
        return true;
    }

    /// <summary>
    /// Debug function to display a breakdown of the team pools
    /// </summary>
    /// <returns></returns>
    public string DisplayTeamAnalysis()
    {
        StringBuilder builder = new StringBuilder();
        //get dictionary of team arcs
        Dictionary<int, TeamArc> tempDict = GameManager.instance.dataScript.GetDictOfTeamArcs();
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
    public string DisplayTeamActorAnalysis()
    {
        List<int> listOfTeams = new List<int>();
        StringBuilder builder = new StringBuilder();
        builder.Append(" OnMap Teams by Actor");
        builder.AppendLine();
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalAuthority);
        foreach(Actor actor in arrayOfActors)
        {
            if (actor != null)
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
                            builder.Append(string.Format("{0} {1}", team.arc.name, team.teamName));
                        }
                    }

                }
                else
                { builder.Append("none"); }
                builder.AppendLine();
            }
        }
        return builder.ToString();
    }

    /// <summary>
    /// Debug function -> Gets data on all individual teams residing in dictOfTeams
    /// </summary>
    /// <returns></returns>
    public string DisplayIndividualTeams()
    {
        StringBuilder builder = new StringBuilder();
        //get dictionary of team arcs
        Dictionary<int, Team> teamDict = GameManager.instance.dataScript.GetDictOfTeams();
        if (teamDict != null)
        {
            //header
            //header
            builder.Append(" Teams in Dictionary");
            builder.AppendLine();
            builder.AppendLine();
            foreach (var teamData in teamDict)
            {
                builder.Append(string.Format(" ID {0}  {1} {2}  P: {3}  N: {4}  T: {5}  A: {6}", teamData.Key, teamData.Value.arc.name, teamData.Value.teamName, 
                    teamData.Value.pool, teamData.Value.nodeID, teamData.Value.timer, teamData.Value.actorSlotID));
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
                genericDetails.side = globalAuthority;
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
                        optionDetails.optionID = team.teamID;
                        optionDetails.text = team.arc.name;
                        optionDetails.sprite = team.arc.sprite;
                        //tooltip -> TO DO
                        GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                        tooltipDetails.textHeader = string.Format("{0}{1}{2} {3}{4}{5}", colourTeam, team.arc.name, colourEnd, colourNormal, team.teamName, colourEnd);
                        turnsAgo = GameManager.instance.turnScript.Turn - team.turnDeployed;
                        if (team.timer > 0) { dataColour = colourGood; } else { dataColour = colourBad; }
                        tooltipDetails.textMain = string.Format("Deployed {0}{1}{2} turn{3} ago and will be auto-recalled in {4}{5}{6} turn{7}", 
                            dataColour, turnsAgo, colourEnd, turnsAgo != 1 ? "s" : "", dataColour, team.timer, colourEnd, team.timer != 1 ? "s" : "");
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(team.actorSlotID, globalAuthority);
                        if (actor != null)
                        {
                            deployedTeams = actor.CheckNumOfTeams();
                            if (deployedTeams < actor.datapoint2) { dataColour = colourGood; } else { dataColour = colourBad; }
                            tooltipDetails.textDetails = string.Format("{0}Inserted by {1} of {2}{3}{4}{5}{6}. They have deployed {7}{8}{9}{10}{11} of {12}{13}{14}{15}{16} possible teams{17}", 
                                colourNormal, GameManager.instance.metaScript.GetAuthorityTitle(), colourEnd, colourActor, actor.arc.name, colourEnd, colourNormal, 
                                colourEnd, dataColour, deployedTeams, colourEnd, colourNormal, colourEnd, 
                                dataColour, actor.datapoint2, colourEnd, colourNormal, colourEnd);
                        }
                        else { Debug.LogError(string.Format("Invalid actor (Null) fro team.ActorSlotID {0}", team.actorSlotID)); }
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
            outcomeDetails.side = globalAuthority;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "TeamManager.cs -> InitialiseGenericPickerRecall");
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails, "TeamManager.cs -> InitialiseGenericPickerRecall");
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
        Node node = GameManager.instance.dataScript.GetNode(details.nodeID);
        if (node != null)
        {
            //double check to see if there are teams present at the node
            List<Team> listOfTeams = node.GetTeams();
            if (listOfTeams != null && listOfTeams.Count > 0)
            {
                genericDetails.returnEvent = EventType.GenericNeutraliseTeam;
                genericDetails.side = globalResistance;
                genericDetails.nodeID = details.nodeID;
                genericDetails.actorSlotID = details.actorDataID;
                //picker text
                genericDetails.textTop = string.Format("{0}Neutralise{1} {2}team{3}", colourEffect, colourEnd, colourNormal, colourEnd);
                genericDetails.textMiddle = string.Format("{0}Operatives are in place to Neutralise a team. The team will be forced to retire immediately{1}",
                    colourNormal, colourEnd);
                genericDetails.textBottom = "Click on a Team to Select. Press CONFIRM to Neutralise team. Mouseover teams for more information.";
                //loop teams present at node
                for (int i = 0; i < listOfTeams.Count; i++)
                {
                    Team team = listOfTeams[i];
                    if (team != null)
                    {
                        //option details
                        GenericOptionDetails optionDetails = new GenericOptionDetails();
                        optionDetails.optionID = team.teamID;
                        optionDetails.text = team.arc.name;
                        optionDetails.sprite = team.arc.sprite;
                        //tooltip -> TO DO
                        GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                        tooltipDetails.textHeader = string.Format("{0}{1}{2} {3}{4}{5}", colourTeam, team.arc.name, colourEnd, colourNormal, team.teamName, colourEnd);
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
                        Debug.LogError(string.Format("Invalid number of Teams (more than 3) at NodeId {0}", details.nodeID));
                        break;
                    }
                }
            }
            else
            {
                Debug.LogError(string.Format("Invalid listOfTeams (Empty or Null) for NodeID {0}", details.nodeID));
                errorFlag = true;
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Node (null) for nodeID {0}", details.nodeID));
            errorFlag = true;
        }
        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = globalResistance;
            outcomeDetails.textTop = "There has been an error in communication and No teams can be Neutralised.";
            outcomeDetails.textBottom = "Heads will roll!";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "TeamManager.cs -> InitialiseGenericPickerNeutralise");
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails, "TeamManager.cs -> InitialiseGenericPickerNeutralise");
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
                    Sprite sprite = team.arc.sprite;
                    Node node = GameManager.instance.dataScript.GetNode(data.nodeID);
                    if (node != null)
                    {
                        //need to do prior to move team as data will be reset
                        textTop = GameManager.instance.effectScript.SetTopTeamText(team.teamID, false);
                        textBottom = "The team will spend one turn in Transit and be available thereafter";
                        int actorID = -1;
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(team.actorSlotID, globalAuthority);
                        if (actor != null) { actorID = actor.actorID; }
                        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", team.actorSlotID)); }
                        if (MoveTeam(TeamPool.InTransit, team.teamID, team.actorSlotID, node) == true)
                        {
                            //message
                            string text = string.Format("{0} {1}, ID {2}, withdrawn early from {3}, ID {4}", team.arc.name, team.teamName, team.teamID, 
                                node.nodeName, node.nodeID);
                            Message message = GameManager.instance.messageScript.TeamWithdraw(text, data.nodeID, team.teamID, actorID);
                            GameManager.instance.dataScript.AddMessage(message);
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
                        details.side = globalAuthority;
                        if (successFlag == true)
                        {
                            details.isAction = true;
                            details.reason = "Recall Team";
                        }
                        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "TeamManager.cs -> ProcessRecallTeam");
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
                    Sprite sprite = team.arc.sprite;
                    Node node = GameManager.instance.dataScript.GetNode(data.nodeID);
                    if (node != null)
                    {
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, globalResistance);
                        if (actor != null)
                        {
                            StringBuilder builderTop = new StringBuilder();
                            StringBuilder builderBottom = new StringBuilder();
                            bool proceedFlag = false;
                            //move team
                            if (GameManager.instance.sideScript.authorityOverall == SideState.AI)
                            { proceedFlag = MoveTeamAI(TeamPool.InTransit, team.teamID, node); }
                            else
                            { proceedFlag = MoveTeam(TeamPool.InTransit, team.teamID, team.actorSlotID, node); }
                            //successful?
                            if (proceedFlag == true)
                            {
                                //message (notification to Authority Side)
                                string text = string.Format("{0} {1}, ID {2}, neutralised at {3}, ID {4}", team.arc.name, team.teamName, team.teamID,
                                    node.nodeName, node.nodeID);
                                Message message = GameManager.instance.messageScript.TeamNeutralise(text, data.nodeID, team.teamID, actor.actorID);
                                if (message != null) { GameManager.instance.dataScript.AddMessage(message); }
                                Debug.Log(string.Format("TeamManager: {0}{1}", text, "\n"));
                                //team successfully removed
                                builderTop.Append(string.Format("{0}Operatives have succeeded!{1}", colourNormal, colourEnd));
                                builderBottom.Append(string.Format("{0}{1}{2}{3} team removed{4}", colourTeam, team.arc.name, colourEnd, 
                                    colourEffect, colourEnd));

                                //Process any other effects, if Neutralise was successfull, ignore otherwise
                                Action action = actor.arc.nodeAction;
                                List<Effect> listOfEffects = action.GetEffects();
                                if (listOfEffects.Count > 0)
                                {
                                    EffectDataInput dataInput = new EffectDataInput();
                                    dataInput.textOrigin = "Neutralise Team";
                                    foreach (Effect effect in listOfEffects)
                                    {
                                        if (effect.ignoreEffect == false)
                                        {
                                            EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actor);
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
                            details.side = globalResistance;
                            if (successFlag == true)
                            {
                                details.isAction = true;
                                details.reason = "Neutralise Team";
                            }
                            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "TeamManager.cs -> ProcessNeutraliseTeam");
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
    /// Actor is 'null' for an AI operation
    /// </summary>
    /// <param name="team"></param>
    /// <param name="node"></param>
    private void ProcessTeamEffect(Team team, Node node, Actor actor)
    {
        bool isError = false;
        List<Effect> listOfEffects = team.arc.listOfEffects;
        EffectDataReturn effectReturn = new EffectDataReturn();
        switch(team.arc.name)
        {
            case "CONTROL":
            case "CIVIL":
            case "MEDIA":
                if (listOfEffects != null)
                {
                    EffectDataInput dataInput = new EffectDataInput();
                    dataInput.textOrigin = team.arc.name;
                    foreach(Effect effect in listOfEffects)
                    {
                        effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actor);
                        isError = effectReturn.errorFlag;
                        string text = string.Format("{0} {1} effect: {2} at \"{3}\", ID {4}", team.arc.name, team.teamName, effect.textTag, node.nodeName, node.nodeID);
                        Message message = GameManager.instance.messageScript.TeamEffect(text, node.nodeID, team.teamID);
                        GameManager.instance.dataScript.AddMessage(message);
                    }
                }
                break;
            case "PROBE":
                //destroy tracer at node if present
                if (node.isTracer == true)
                {
                    node.RemoveTracer();
                    //message
                    string text = string.Format("{0} {1}: Tracer destroyed at \"{2}\", ID {3}", team.arc.name, team.teamName, node.nodeName, node.nodeID);
                    Message message = GameManager.instance.messageScript.TeamEffect(text, node.nodeID, team.teamID);
                    GameManager.instance.dataScript.AddMessage(message);
                }
                break;
            case "SPIDER":
                if (node.isSpider == false)
                {
                    //add spider
                    node.isSpider = true;
                    node.spiderTimer = GameManager.instance.nodeScript.observerTimer;
                    //check if within tracer coverage
                    if(node.isTracerActive == true)
                    { node.isSpiderKnown = true; }
                    else { node.isSpiderKnown = false; }
                    //message
                    string text = string.Format("{0} {1}: Spider inserted at \"{2}\", ID {3}", team.arc.name, team.teamName, node.nodeName, node.nodeID);
                    Message message = GameManager.instance.messageScript.TeamEffect(text, node.nodeID, team.teamID);
                    GameManager.instance.dataScript.AddMessage(message);
                }
                break;
            case "ERASURE":

                //TO DO -> deletes any known connections ?

                break;
            case "DAMAGE":
                //at node with a completed, but uncontained, target?
                if (node.targetID > -1)
                {
                    Target target = GameManager.instance.dataScript.GetTarget(node.targetID);
                    if (target != null)
                    {
                        if (target.targetStatus == Status.Completed && target.ongoingID > -1)
                        {
                            //contain target and shut down all ongoing node effects
                            GameManager.instance.targetScript.ContainTarget(target);
                            //message
                            string text = string.Format("Target \"{0}\" Contained by {1} {2}", target.name, team.arc.name, team.teamName);
                            Message message = GameManager.instance.messageScript.TargetContained(text, node.nodeID, team.teamID, target.targetID);
                            GameManager.instance.dataScript.AddMessage(message);
                        }
                    }
                    else { Debug.LogError(string.Format("Invalid Target (Null) for targetID {0}", node.targetID)); }
                }
                break;
            default:
                Debug.LogError(string.Format("Invalid team Arc name \"{0}\"", team.arc.name));
                isError = true;
                break;
        }
        //assign renown to originating actor if all O.K
        if (isError == false && actor != null)
        { actor.renown++; }
    }

    /// <summary>
    /// Whenever an Authority actor is removed from the map (even if sent to the reserves) run this method to clean up any OnMap or InTransit teams connected with
    /// the actor. They are all placed (instantly) into the Reserve team Pool to prevent errors when accessing actors that are no longer there.
    /// Returns the number of teams that have been cleaned up
    /// </summary>
    /// <param name="actor"></param>
    public int TeamCleanUp(Actor actor)
    {
        int counter = 0;
        if (actor != null)
        {
            List<int> listOfTeams = actor.GetTeams();
            if (listOfTeams.Count > 0)
            {
                //loop teams and remove all active traces
                for (int i = 0; i < listOfTeams.Count; i++)
                {
                    Team team = GameManager.instance.dataScript.GetTeam(listOfTeams[i]);
                    if (team != null)
                    {
                        counter++;
                        //update tallies & remove team from their pool
                        GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve, +1);
                        switch (team.pool)
                        {
                            case TeamPool.OnMap:
                                GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.OnMap, -1);
                                GameManager.instance.dataScript.RemoveTeamFromPool(TeamPool.OnMap, team.teamID);
                                break;
                            case TeamPool.InTransit:
                                GameManager.instance.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.InTransit, -1);
                                GameManager.instance.dataScript.RemoveTeamFromPool(TeamPool.InTransit, team.teamID);
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid team.pool \"{0}\"", team.pool));
                                break;
                        }
                        //remove from node list
                        Node node = GameManager.instance.dataScript.GetNode(team.nodeID);
                        if (node != null)
                        { node.RemoveTeam(team.teamID); }
                        else { Debug.LogError(string.Format("Invalid node (Null) for team.nodeID {0}, team {1}, ID {2}", team.nodeID, team.arc.name, team.teamID)); }
                        //add to reserve pool
                        GameManager.instance.dataScript.AddTeamToPool(TeamPool.Reserve, team.teamID);
                        //update team status
                        team.ResetTeamData(TeamPool.Reserve);
                        //confirmation
                        Debug.Log(string.Format("TeamManager: {0} {1}, ID {2}, auto Moved to {3} as Actor has left{4}", team.arc.name, team.teamName, team.teamID, team.pool, "\n"));
                        //message
                        string text = string.Format("{0} {1}, ID {2}, withdrawn early from {3}, ID {4}", team.arc.name, team.teamName, team.teamID,
                            node.nodeName, node.nodeID);
                        Message message = GameManager.instance.messageScript.TeamWithdraw(text, node.nodeID, team.teamID, actor.actorID);
                        GameManager.instance.dataScript.AddMessage(message);
                    }
                    else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}, actor {1}, ID {2}", listOfTeams[i], actor.arc.name, actor.actorID)); }
                }
            }
        }
        else { Debug.LogError("Invalid actor (Null)"); }
        //return number of teams removed and sent to the reserves
        return counter;
    }

    public List<TeamArc> GetListOfTeamPrioritiesHigh()
    { return listOfTeamPrioritiesHigh; }

    public List<TeamArc> GetListOfTeamPrioritiesMed()
    { return listOfTeamPrioritiesMedium; }

    //place new method above here
}
