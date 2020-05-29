using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

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
    private int spotTeamChance = -1;
    private int maxSpotTeam = -1;
    private int maxGenericOptions;

    private string colourEffect;
    private string colourTeam;
    private string colourNormal;
    private string colourNeutral;
    private string colourGood;
    private string colourActor;
    private string colourBad;
    private string colourEnd;

    #region Save Data Compatible
    [HideInInspector] public int teamIDCounter = 0;                     //provides unique ID to teams (reset at start of new level)
    #endregion

    /// <summary>
    /// Set up at start. Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                SubInitialiseLevelStart();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseLevelStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access fields -> BEFORE SubInitialiseSessionStart
        globalAuthority = GameManager.i.globalScript.sideAuthority;
        globalResistance = GameManager.i.globalScript.sideResistance;
        spotTeamChance = GameManager.i.contactScript.spotTeamChance;
        maxSpotTeam = GameManager.i.contactScript.maxSpotTeam;
        maxGenericOptions = GameManager.i.genericPickerScript.maxOptions;
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(spotTeamChance > -1, "Invalid spotTeamChance (less than Zero)");
        Debug.Assert(maxSpotTeam > -1, "Invalid maxSpotTeam (less than Zero)");
        Debug.Assert(maxGenericOptions > -1, "Invalid maxGenericOptions (-1)");
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //Teams
        int teamArcCount = 0;
        Debug.Assert(listOfTeamPrioritiesHigh != null, "Invalid listOfTeamPrioritiesHigh (Null)");
        Debug.Assert(listOfTeamPrioritiesHigh.Count > 0, "listOfTeamPrioritiesHigh has no records");
        Debug.Assert(listOfTeamPrioritiesMedium != null, "Invalid listOfTeamPrioritiesMedium (Null)");
        Debug.Assert(listOfTeamPrioritiesMedium.Count > 0, "listOfTeamPrioritiesMedium has no records");
        Debug.Assert(listOfTeamPrioritiesLow != null, "Invalid listOfTeamPrioritiesLow (Null)");
        Debug.Assert(listOfTeamPrioritiesLow.Count > 0, "listOfTeamPrioritiesLow has no records");
        teamArcCount += listOfTeamPrioritiesHigh.Count + listOfTeamPrioritiesMedium.Count + listOfTeamPrioritiesLow.Count;
        Debug.Assert(teamArcCount == GameManager.i.dataScript.CheckNumOfTeamArcs(), "Mismatched count of team Priority Arcs (should be same # as num of unique Team Arcs");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event Listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "TeamManager");
        EventManager.instance.AddListener(EventType.EndTurnEarly, OnEvent, "TeamManager");
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent, "TeamManager");
        EventManager.instance.AddListener(EventType.RecallTeamAction, OnEvent, "TeamManager");
        EventManager.instance.AddListener(EventType.GenericTeamRecall, OnEvent, "TeamManager");
        EventManager.instance.AddListener(EventType.NeutraliseTeamAction, OnEvent, "TeamManager");
        EventManager.instance.AddListener(EventType.GenericNeutraliseTeam, OnEvent, "TeamManager");
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        //needs to be after SubInitialiseSessionStart
        InitialiseTeams();
    }
    #endregion

    #endregion


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
        colourEffect = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourNormal = GameManager.i.colourScript.GetColour(ColourType.normalText);
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourTeam = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourGood = GameManager.i.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.dataBad);
        colourActor = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
    }

    /// <summary>
    /// reset team ID counter prior to new level
    /// </summary>
    public void ResetCounter()
    { teamIDCounter = 0; }

    /// <summary>
    /// End turn activity -> Event driven, decrement all timers in OnMap pool
    /// </summary>
    private void EndTurn()
    {
        //decrement all timers in OnMap pool
        List<int> teamPool = GameManager.i.dataScript.GetTeamPool(TeamPool.OnMap);
        if (teamPool != null)
        {
            for (int i = 0; i < teamPool.Count; i++)
            {
                Team team = GameManager.i.dataScript.GetTeam(teamPool[i]);
                if (team != null)
                {
                    if (team.pool != TeamPool.OnMap)
                    { Debug.LogWarningFormat("Team data shows it to be in {0} pool, not OnMap", team.pool); }
                    //decrement timer
                    team.timer--;
                }
                else { Debug.LogError(string.Format("Invalid team (null) for TeamID {0}", teamPool[i])); }
            }
        }
        else { Debug.LogError("Invalid teamPool (Null) -> no team timers decremented"); }
    }


    /// <summary>
    /// Start turn Early activity -> Event driven. Team Management (inTransit, OnMap timers)
    /// NOTE: needs to be before NodeManager.cs process ongoing effects in StartTurnLate (team can cancel effect and NodeManager.cs still creates an ongoing effect msg otherwise)
    /// </summary>
    private void StartTurnEarly()
    {
        List<int> teamPool = new List<int>();
        //check InTransit pool -> move any teams here to the Reserve pool -> Note: do this BEFORE checking OnMap pool below
        teamPool.AddRange(GameManager.i.dataScript.GetTeamPool(TeamPool.InTransit));
        if (teamPool != null)
        {
            for (int i = 0; i < teamPool.Count; i++)
            {
                Team team = GameManager.i.dataScript.GetTeam(teamPool[i]);
                if (team != null)
                {
                    if (team.pool != TeamPool.InTransit)
                    { Debug.LogWarningFormat("Team data shows it to be in {0} pool, not InTransit", team.pool); }
                    //Automatically move any teams to reserve (they spend in turn in transit and are unavailable for deployment)
                    if (GameManager.i.sideScript.authorityOverall == SideState.AI)
                    { MoveTeamAI(TeamPool.Reserve, team.teamID); }
                    else { MoveTeam(TeamPool.Reserve, team.teamID, team.actorSlotID); }
                }
                else { Debug.LogError(string.Format("Invalid team (null) for TeamID {0}", teamPool[i])); }
            }
        }
        else { Debug.LogError("Invalid teamPool (Null) -> no teams moved from InTransit"); }
        //check all timers in OnMap pool -> need a value list, not reference, here as MoveTeam changes the pool while I'm looping it below
        teamPool.Clear();
        teamPool.AddRange(GameManager.i.dataScript.GetTeamPool(TeamPool.OnMap));
        if (teamPool != null)
        {

            for (int i = 0; i < teamPool.Count; i++)
            {
                Team team = GameManager.i.dataScript.GetTeam(teamPool[i]);
                if (team != null)
                {
                    if (team.pool != TeamPool.OnMap)
                    { Debug.LogWarningFormat("Team data shows it to be in {0} pool, not OnMap", team.pool); }
                    Node node = GameManager.i.dataScript.GetNode(team.nodeID);
                    if (node != null)
                    {
                        //check timer 
                        if (team.timer < 0)
                        {
                            //Timer expired, team automatically recalled to InTransit pool

                            if (GameManager.i.sideScript.authorityOverall == SideState.Human)
                            {
                                //Human Authority Player
                                Actor actor = GameManager.i.dataScript.GetCurrentActor(team.actorSlotID, globalAuthority);
                                MoveTeam(TeamPool.InTransit, team.teamID, team.actorSlotID, node);
                                if (actor != null)
                                {
                                    //Permanent Team effect activated for node
                                    ProcessTeamEffect(team, node, actor);
                                    //TeamActionData
                                    TeamActionData data = new TeamActionData()
                                    {
                                        teamAction = GetTeamAction(team.arc.name),
                                        turn = GameManager.i.turnScript.Turn,
                                        actorID = actor.actorID,
                                        nodeID = node.nodeID,
                                        teamID = team.teamID,
                                        dataName = string.Format("{0} {1}, ID {2}", team.arc.name, team.teamName, team.teamID)
                                    };
                                    actor.AddTeamAction(data);
                                    //message
                                    string text = string.Format("{0} {1}, ID {2}, recalled from {3}, ID {4}", team.arc.name, team.teamName, team.teamID, node.nodeName, node.nodeID);
                                    GameManager.i.messageScript.TeamAutoRecall(text, node, team, actor);
                                }
                                else
                                {
                                    //error if there should be an actor in the slot
                                    if (GameManager.i.dataScript.CheckActorSlotStatus(team.actorSlotID, globalAuthority) == true)
                                    { Debug.LogError(string.Format("Invalid actor (null) for actorSlotID {0}", team.actorSlotID)); }
                                }
                            }
                            else
                            {
                                //teamActionData
                                Actor actor = null;
                                //actor previously assigned by MoveTeamAI
                                if (team.actorSlotID > -1)
                                { actor = GameManager.i.dataScript.GetCurrentActor(team.actorSlotID, GameManager.i.sideScript.PlayerSide); }
                                else
                                {
                                    //need a random actor
                                    List<Actor> listOfActors = GameManager.i.dataScript.GetActiveActors(globalAuthority);
                                    if (listOfActors != null)
                                    {
                                        if (listOfActors.Count > 0)
                                        {
                                            actor = listOfActors[Random.Range(0, listOfActors.Count)];
                                            /*Debug.LogFormat("[Tst] TeamManager.cs -> StartTurnEarly: Random Actor select {0}, {1}, ID {2} for {3} {4}, ID {5}{6}",
                                                actor.actorName, actor.arc.name, actor.actorID, team.arc.name, team.teamName, team.teamID, "\n");*/
                                        }
                                    }
                                    else { Debug.LogError("Invalid listOfActors (Null)"); }
                                }
                                if (actor != null)
                                {
                                    TeamActionData data = new TeamActionData()
                                    {
                                        teamAction = GetTeamAction(team.arc.name),
                                        turn = GameManager.i.turnScript.Turn,
                                        actorID = actor.actorID,
                                        nodeID = node.nodeID,
                                        teamID = team.teamID,
                                        dataName = string.Format("{0} {1}, ID {2}", team.arc.name, team.teamName, team.teamID)
                                    };
                                    actor.AddTeamAction(data);
                                }
                                else { Debug.LogFormat("[Tst] TeamManager.cs -> StartTurnEarly: No ACTIVE, OnMap actor available, teamActionData package not generated for {0} {1}, ID {2}{3}", 
                                    team.arc.name, team.teamName, team.teamID, "\n"); }
                                //AI Authority player
                                MoveTeamAI(TeamPool.InTransit, team.teamID, node);
                                //Permanent Team effect activated for node
                                ProcessTeamEffect(team, node, null);
                                //message
                                string text = string.Format("{0} {1}, ID {2}, recalled from {3}, ID {4}", team.arc.name, team.teamName, team.teamID, node.nodeName, node.nodeID);
                                GameManager.i.messageScript.TeamAutoRecall(text, node, team);
                            }
                        }
                        else
                        {
                            //timer O.K
                            Debug.LogFormat("[Tea] TeamManager.cs -> StartTurnLate: {0} team, id {1} currently at {2}, {3}, id {4}{5}", team.arc.name, team.teamID,
                                node.nodeName, node.Arc.name, node.nodeID, "\n");
                            //Check for erasure team sightings by contacts or tracers -> Resistance Player only
                            if (GameManager.i.sideScript.PlayerSide.level == globalResistance.level)
                            {
                                if (team.arc.name.Equals("ERASURE", StringComparison.Ordinal) == true)
                                {
                                    //orgInfo not involved
                                    if (GameManager.i.dataScript.CheckOrgInfoType(OrgInfoType.ErasureTeams) == false)
                                    { ProcessErasureTeamSighting(team, node); }
                                    else
                                    {
                                        //OrgInfo sighting (automatic)
                                        string text = string.Format("{0} tracks {1} {2} at {3}, {4}, ID {5}", GameManager.i.campaignScript.campaign.orgInfo.tag, team.arc.name, team.teamName,
                                            node.nodeName, node.Arc.name, node.nodeID);
                                        GameManager.i.messageScript.OrganisationErasureTeam(text, node, team);
                                    }
                                }
                            }
                        }
                    }
                    else { Debug.LogError(string.Format("Invalid node (null) for TeamID {0} and team.NodeID {1}", teamPool[i], team.nodeID)); }
                }
                else { Debug.LogError(string.Format("Invalid team (null) for TeamID {0}", teamPool[i])); }
            }
        }
        else { Debug.LogError("Invalid teamPool (Null) -> no teams with expired timers recalled from OnMap"); }
    }

    /// <summary>
    /// subMethod to get a teamAction (for TeamActionData package) from a specified team.Arc.name. Returns 'TeamAction.None' if a problem
    /// </summary>
    /// <param name="teamArc"></param>
    /// <returns></returns>
    private TeamAction GetTeamAction(string teamArcName)
    {
        TeamAction teamAction = TeamAction.None;
        if (string.IsNullOrEmpty(teamArcName) == false)
        {
            switch (teamArcName)
            {
                case "CIVIL": teamAction = TeamAction.TeamCivil; break;
                case "CONTROL": teamAction = TeamAction.TeamControl; break;
                case "DAMAGE": teamAction = TeamAction.TeamDamage; break;
                case "ERASURE": teamAction = TeamAction.TeamErasure; break;
                case "MEDIA": teamAction = TeamAction.TeamMedia; break;
                case "PROBE": teamAction = TeamAction.TeamProbe; break;
                case "SPIDER": teamAction = TeamAction.TeamSpider; break;
                default: Debug.LogWarningFormat("Unrecognised teamArcName \"{0}\"", teamArcName); break;
            }
        }
        return teamAction;
    }


    /// <summary>
    /// Message notification of contact spotting an Erasure team (only have messages for erasure teams). Returns true if a valid Erasure team sighting
    /// NOTE: calling method has checked team and node for Null
    /// </summary>
    /// <param name="team"></param>
    /// <param name="node"></param>
    private bool ProcessErasureTeamSighting(Team team, Node node)
    {
        bool isSpotted = true;
        string text;
        //
        // - - - CONTACT sighting
        //
        if (node.isContactResistance == true)
        {
            List<int> listOfActors = GameManager.i.dataScript.CheckContactResistanceAtNode(node.nodeID);
            if (listOfActors != null)
            {
                //loop actors and check all relevant contacts
                int count = listOfActors.Count;
                if (count > 0)
                {
                    for (int index = 0; index < count; index++)
                    {
                        Actor actor = GameManager.i.dataScript.GetActor(listOfActors[index]);
                        if (actor != null)
                        {
                            //actor must active
                            if (actor.Status == ActorStatus.Active)
                            {
                                Contact contact = actor.GetContact(node.nodeID);
                                if (contact != null)
                                {
                                    if (contact.status == ContactStatus.Active)
                                    {
                                        //message
                                        Debug.LogFormat("[Cnt] TeamManager.cs -> StartTurnLate: Contact {0} {1}, {2} spots {3} team at {4}, {5}, id {6}{7}", contact.nameFirst,
                                            contact.nameLast, contact.job, team.arc.name, node.nodeName, node.Arc.name, node.nodeID, "\n");
                                        text = string.Format("{0} team, id {1}, has been spotted by Contact {2} {3}, {4}, at district {5}, id {6}", team.teamName, team.teamID,
                                            contact.nameFirst, contact.nameLast, contact.job, node.nodeName, node.nodeID);
                                        GameManager.i.messageScript.ContactTeamSpotted(text, actor, node, contact, team);
                                        //update contact stats
                                        contact.statsTeams++;
                                    }
                                    else
                                    {
                                        Debug.LogFormat("[Cnt] TeamManager.cs -> StartTurnLate: Contact {0} {1}, {2} INACTIVE & doesn't spot {3} Team at {4}, {5}, id {6}{7}",
                                            contact.nameFirst, contact.nameLast, contact.job, team.arc.name, node.nodeName, node.Arc.name, node.nodeID, "\n");
                                        isSpotted = false;
                                    }
                                }
                                else { Debug.LogErrorFormat("Invalid contact (Null) for actor {0}, {1}, id {2}", actor.actorName, actor.arc.name, actor.actorID); }
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid actor (Null) for listOfActors.actorID {0}", listOfActors[index]); }
                    }
                }
                else { Debug.LogErrorFormat("Invalid listOfActors (Empty) for nodeID {0}", node.nodeID); }
            }
            else { Debug.LogErrorFormat("Invalid listOfActors (Null) for nodeID {0}", node.nodeID); }
        }
        //
        // - - - TRACER sighting
        //
        else if (node.isTracer == true)
        {
            text = string.Format("Tracer detects an {0} team and {1}, {2}, district, id {3}", team.arc.name, node.nodeName, node.Arc.name, node.nodeID);
            GameManager.i.messageScript.TracerTeamSpotted(text, node, team);
        }
        //NO sighting
        else
        { isSpotted = false; }
        return isSpotted;
    }


    /// <summary>
    /// Sets up intial Reserve pool of teams and related collections
    /// </summary>
    public void InitialiseTeams()
    {
        //Place one team of every type in the reserve
        List<int> listOfTeamArcIDs = GameManager.i.dataScript.GetTeamArcIDs();
        if (listOfTeamArcIDs != null && listOfTeamArcIDs.Count > 0)
        {
            //loop list and add one team of each type to teamReserve pool
            foreach (int arcID in listOfTeamArcIDs)
            {
                GameManager.i.dataScript.AdjustTeamInfo(arcID, TeamInfo.Reserve, +1);
                GameManager.i.dataScript.AdjustTeamInfo(arcID, TeamInfo.Total, +1);
            }
        }
        else { Debug.LogError("Invalid listOfTeamArcIDs (Null or Empty) -> initial team setup cancelled"); }

        //add teams depending on who is in charge of the authority side
        switch (GameManager.i.sideScript.authorityOverall)
        {
            case SideState.Human:
                //Add extra teams ([edit] No, see below [/edit] 
                Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActors(globalAuthority);
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
                        GameManager.i.dataScript.AdjustTeamInfo(arcID, TeamInfo.Reserve, numToDeploy);
                        GameManager.i.dataScript.AdjustTeamInfo(arcID, TeamInfo.Total, numToDeploy);
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
                    if (GameManager.i.dataScript.CheckNumOfNodes() < 20) { numOfTeams = 1; }
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
                            GameManager.i.dataScript.AdjustTeamInfo(arcID, TeamInfo.Reserve, +1);
                            GameManager.i.dataScript.AdjustTeamInfo(arcID, TeamInfo.Total, +1);
                            tempList.RemoveAt(index);
                        }
                        else { Debug.LogWarningFormat("Invalid High priority teamArcID \"{0}\"", arcID); }
                    }
                    //Medium priority teams (random choice)
                    tempList.Clear();
                    tempList = new List<TeamArc>(listOfTeamPrioritiesMedium);
                    //map size scaling
                    numOfTeams = 2;
                    if (GameManager.i.dataScript.CheckNumOfNodes() < 20) { numOfTeams = 1; }
                    numOfTeams = Mathf.Min(numOfTeams, tempList.Count);
                    for (int i = 0; i < numOfTeams; i++)
                    {
                        index = Random.Range(0, tempList.Count);
                        arcID = tempList[index].TeamArcID;
                        if (arcID >= 0)
                        {
                            GameManager.i.dataScript.AdjustTeamInfo(arcID, TeamInfo.Reserve, +1);
                            GameManager.i.dataScript.AdjustTeamInfo(arcID, TeamInfo.Total, +1);
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
            numToCreate = GameManager.i.dataScript.CheckTeamInfo(teamArcID, TeamInfo.Reserve);
            //should never be more than 2
            Debug.Assert(numToCreate < 3, string.Format("Excessive number ({0}) of {1} teams", numToCreate, GameManager.i.dataScript.GetTeamArc(teamArcID).name));
            Debug.LogFormat("TeamManager.cs -> InitialiseTeams: {0} {1} team{2} created{3}", numToCreate, GameManager.i.dataScript.GetTeamArc(teamArcID).name,
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
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
        Dictionary<int, Team> dictOfTeams = GameManager.i.dataScript.GetDictOfTeams();
        if (listOfNodes != null)
        {
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
                            actorSlotID = Random.Range(0, GameManager.i.actorScript.maxNumOfOnMapActors);
                            //only do so if Actor is present in slot (player might start level with less than full complement of actors)
                            if (GameManager.i.dataScript.CheckActorSlotStatus(actorSlotID, globalAuthority) == true)
                            { MoveTeam(TeamPool.OnMap, teamData.Key, actorSlotID, node); }
                        }
                        else { Debug.LogError("Invalid node (Null)"); }
                    }
                }
            }
            else { Debug.LogWarning("Invalid dictOfTeams (Null)"); }
        }
        else { Debug.LogWarning("Invalid listOfNodes (Null)"); }
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
        Debug.Assert(teamID > -1 && teamID < GameManager.i.dataScript.CheckNumOfTeams(), "Invalid teamID");
        Team team = GameManager.i.dataScript.GetTeam(teamID);
        bool successFlag = true;
        if (team != null)
        {
            //
            // - - - Move - - -
            //
            switch (destinationPool)
            {
                case TeamPool.Reserve:
                    //pools
                    if (GameManager.i.dataScript.RemoveTeamFromPool(TeamPool.InTransit, teamID) == true)
                    {
                        GameManager.i.dataScript.AddTeamToPool(TeamPool.Reserve, teamID);
                        //adjust tallies
                        GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve, +1);
                        GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.InTransit, -1);
                        //update team status
                        team.ResetTeamData(TeamPool.Reserve);
                        //confirmation
                        Debug.Log(string.Format("[Tea] TeamManager.cs -> MoveTeam: {0} {1}, ID {2}, moved to {3}{4}", team.arc.name, team.teamName, team.teamID, destinationPool, "\n"));
                    }
                    else { Debug.LogWarningFormat("{0} Team, id {1}, NOT Removed from InTransit pool", team.arc.name, team.teamID); }
                    break;
                case TeamPool.OnMap:
                    if (actorSlotID > -1 && actorSlotID < GameManager.i.actorScript.maxNumOfOnMapActors)
                    {
                        //Get Actor
                        Actor actor = GameManager.i.dataScript.GetCurrentActor(actorSlotID, globalAuthority);
                        if (actor != null)
                        {
                            if (actor.Status == ActorStatus.Active)
                            {
                                if (node != null)
                                {
                                    //a team available in the reserve pool?
                                    if (GameManager.i.dataScript.CheckTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve) > 0)
                                    {
                                        //check if actor has capacity to deploy another team
                                        if (actor.CheckCanDeployTeam() == true)
                                        {
                                            if (node.AddTeam(team, actorSlotID) == true)
                                            {
                                                //pools
                                                if (GameManager.i.dataScript.RemoveTeamFromPool(TeamPool.Reserve, teamID) == true)
                                                {
                                                    GameManager.i.dataScript.AddTeamToPool(TeamPool.OnMap, teamID);
                                                    //adjust tallies for onMap
                                                    GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.OnMap, +1);
                                                    GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve, -1);
                                                    //add team to Actor list
                                                    actor.AddTeam(team.teamID);
                                                    //update team stats
                                                    team.nodeID = node.nodeID;
                                                    team.actorSlotID = actor.slotID;
                                                    team.timer = deployTime;
                                                    team.turnDeployed = GameManager.i.turnScript.Turn;
                                                    //confirmation
                                                    string text = string.Format("{0} {1}, ID {2}, deployed to {3}, Node ID {4}", team.arc.name, team.teamName, team.teamID,
                                                        destinationPool, node.nodeID);
                                                    Debug.LogFormat("[Tea] TeamManager.cs -> MoveTeam: {0}{1}", text, "\n");
                                                    //message
                                                    GameManager.i.messageScript.TeamDeploy(text, node, team, actor);
                                                    //NodeActionData
                                                    NodeActionData nodeActionData = new NodeActionData()
                                                    {
                                                        turn = GameManager.i.turnScript.Turn,
                                                        actorID = actor.actorID,
                                                        nodeID = node.nodeID,
                                                        teamID = team.teamID,
                                                        dataName = string.Format("{0} {1}, ID {2}", team.arc.name, team.teamName, team.teamID),
                                                        nodeAction = NodeAction.ActorDeployTeam
                                                    };
                                                    //add to actor's personal list
                                                    actor.AddNodeAction(nodeActionData);
                                                    Debug.LogFormat("[Tst] TeamManager.cs -> MoveTeam: nodeActionData added to {0}, {1}{2}", actor.actorName, actor.arc.name, "\n");
                                                }
                                                else { Debug.LogWarningFormat("{0} Team, id {1}, NOT Removed from Reserve pool", team.arc.name, team.teamID); }
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
                    if (actorSlotID > -1 && actorSlotID < GameManager.i.actorScript.maxNumOfOnMapActors)
                    {
                        /*//Get Actor
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, globalAuthority);
                        if (actor != null)
                        {
                            if (actor.Status == ActorStatus.Active)
                            {*/
                        if (node != null)
                        {
                            //pools
                            if (GameManager.i.dataScript.RemoveTeamFromPool(TeamPool.OnMap, teamID) == true)
                            {
                                GameManager.i.dataScript.AddTeamToPool(TeamPool.InTransit, teamID);
                                //adjust tallies
                                GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.OnMap, -1);
                                GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.InTransit, +1);
                                //remove from node list
                                node.RemoveTeam(team.teamID);
                                //update team status
                                team.ResetTeamData(TeamPool.InTransit);
                                //Get Actor
                                Actor actor = GameManager.i.dataScript.GetCurrentActor(actorSlotID, globalAuthority);
                                if (actor != null)
                                {
                                    //remove team from actor list (if actor not present O.K as actor could have just been sent OffMap)
                                    actor.RemoveTeam(team.teamID);
                                }
                                //confirmation
                                Debug.LogFormat("[Tea] TeamManager.cs -> MoveTeam: {0} {1}, ID {2}, moved to {3}, from Node ID {4}{5}", team.arc.name, team.teamName, team.teamID,
                                destinationPool, node.nodeID, "\n");
                            }
                            else { Debug.LogWarningFormat("{0} Team, id {1}, NOT Removed from OnMap pool", team.arc.name, team.teamID); }

                        }
                        else
                        {
                            Debug.LogError("Invalid node (Null) for InTransit -> move Cancelled");
                            successFlag = false;
                        }
                        /*}
                        else
                        {   EDIT -> Nonsense, going from OnMap to reserve
                            Debug.LogWarning(string.Format("{0}, ID {1} can't be deployed to {2} as Actor {3}, slotID {4} isn't Live{5}", team.arc.name, teamID, destinationPool,
                              actor.actorName, actorSlotID, "\n")); 
                            successFlag = false;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", actorSlotID));
                        successFlag = false;
                    }*/
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
            Debug.LogError(string.Format("Invalid Team (null) for TeamID {0}", teamID)); return false;
        }
        return true;
    }

    /// <summary>
    /// handles all admin for AI moving a team from one pool to another. Assumed movement direction is 'Reserve Pool -> OnMap -> InTransit -> Reserve Pool'
    /// Takes care of all checks, eg. enough teams present in reserve for one to move to the map
    /// AI Teams aren't associated with Actors (EDIT: they are now to accomodate synching of node/team ActionData packages for topics)
    /// only use the node parameter if the team is moving 'OnMap' (it's moving to a specific node)
    /// </summary>
    /// <param name="destinationPool"></param>
    /// <param name="teamID"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool MoveTeamAI(TeamPool destinationPool, int teamID, Node node = null)
    {
        Debug.Assert(teamID > -1 && teamID < GameManager.i.dataScript.CheckNumOfTeams(), "Invalid teamID");
        Team team = GameManager.i.dataScript.GetTeam(teamID);
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
                    //pools
                    if (GameManager.i.dataScript.RemoveTeamFromPool(TeamPool.InTransit, teamID) == true)
                    {
                        GameManager.i.dataScript.AddTeamToPool(TeamPool.Reserve, teamID);
                        //adjust tallies
                        GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve, +1);
                        GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.InTransit, -1);
                        //update team status
                        team.ResetTeamData(TeamPool.Reserve);
                        //confirmation
                        Debug.LogFormat("[Tea] TeamManager.cs -> MoveTeamAI: {0} {1}, ID {2}, moved to {3}{4}", team.arc.name, team.teamName, team.teamID, destinationPool, "\n");
                    }
                    else { Debug.LogWarningFormat("{0} Team, id {1}, NOT Removed from InTransit pool", team.arc.name, team.teamID); successFlag = false; }
                    break;
                case TeamPool.OnMap:
                    if (node != null)
                    {
                        //a team available in the reserve pool?
                        if (GameManager.i.dataScript.CheckTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve) > 0)
                        {
                            if (node.AddTeam(team, actorID) == true)
                            {
                                //pools
                                if (GameManager.i.dataScript.RemoveTeamFromPool(TeamPool.Reserve, teamID) == true)
                                {
                                    GameManager.i.dataScript.AddTeamToPool(TeamPool.OnMap, teamID);
                                    //adjust tallies for onMap
                                    GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.OnMap, +1);
                                    GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve, -1);
                                    //update team stats
                                    team.nodeID = node.nodeID;
                                    team.actorSlotID = -1;
                                    team.timer = deployTime;
                                    team.turnDeployed = GameManager.i.turnScript.Turn;
                                    //confirmation
                                    string text = string.Format("{0} {1}, ID {2}, deployed to {3}, Node ID {4}", team.arc.name, team.teamName, team.teamID,
                                        destinationPool, node.nodeID);
                                    //NodeActionData -> Debug purposes only (can be left in)
                                    if (GameManager.i.sideScript.PlayerSide.level == globalAuthority.level)
                                    {
                                        //normally team actions aren't assigned to an actor but done so here for debugging purposes (to generate nodeActionData records and to test topics)
                                        List<Actor> listOfActors = GameManager.i.dataScript.GetActiveActors(globalAuthority);
                                        if (listOfActors != null)
                                        {
                                            if (listOfActors.Count > 0)
                                            {
                                                //get actor with least number of NodeActionData records to avoid loading up teams onto one or two actors
                                                Actor actor = null;
                                                int numOfActions = 99999;
                                                foreach(Actor tempActor in listOfActors)
                                                {
                                                    if (tempActor.CheckNumOfNodeActions() < numOfActions)
                                                    {
                                                        numOfActions = tempActor.CheckNumOfNodeActions();
                                                        actor = tempActor;
                                                    }
                                                }
                                                if (actor != null)
                                                {
                                                    //need to give assign ai team an actor as there could be multiple topics (DistrictAction / AuthorityTeam) referencing the same team
                                                    team.actorSlotID = actor.slotID;
                                                    //actor action
                                                    NodeActionData nodeActionData = new NodeActionData()
                                                    {
                                                        turn = GameManager.i.turnScript.Turn,
                                                        actorID = actor.actorID,
                                                        nodeID = node.nodeID,
                                                        teamID = team.teamID,
                                                        nodeAction = NodeAction.ActorDeployTeam,
                                                        dataName = string.Format("{0} {1}, ID {2}", team.arc.name, team.teamName, team.teamID)
                                                    };
                                                    //add to actor's personal list
                                                    actor.AddNodeAction(nodeActionData);
                                                    Debug.LogFormat("[Tst] TeamManager.cs -> MoveTeamAI: nodeActionData added to {0}, {1}, ID {2} for {3} {4}, ID {5}{6}", 
                                                        actor.actorName, actor.arc.name, actor.actorID, team.arc.name, team.teamName, team.teamID, "\n");
                                                }
                                                else { Debug.LogError("Invalid actor (Null) from listOfActors (Active)"); }
                                            }
                                        }
                                        else { Debug.LogError("Invalid listOfActors (Null)"); }
                                    }

                                    //message (internal only)
                                    GameManager.i.messageScript.TeamDeploy(text, node, team);
                                }
                                else { Debug.LogWarningFormat("{0} Team, id {1}, NOT Removed from Reserve pool", team.arc.name, team.teamID); successFlag = false; }
                            }
                            else
                            {
                                /*Debug.LogWarning(string.Format("Node Add team operation failed for \"{0} {1}\" (could be duplicate)",
                                    team.arc.name, team.teamName));*/
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
                        //pools
                        if (GameManager.i.dataScript.RemoveTeamFromPool(TeamPool.OnMap, teamID) == true)
                        {
                            //adjust tallies
                            GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.InTransit, +1);
                            GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.OnMap, -1);
                            GameManager.i.dataScript.AddTeamToPool(TeamPool.InTransit, teamID);
                            //remove from node list
                            node.RemoveTeam(team.teamID);
                            //update team status
                            team.ResetTeamData(TeamPool.InTransit);
                            //confirmation
                            Debug.LogFormat("[Tea] TeamManager.cs -> MoveTeamAI: {0} {1}, ID {2}, moved to {3}, from Node ID {4}{5}", team.arc.name, team.teamName, team.teamID,
                            destinationPool, node.nodeID, "\n");
                        }
                        else { Debug.LogWarningFormat("{0} Team, id {1}, NOT Removed from OnMap pool", team.arc.name, team.teamID); }

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
            /*if (successFlag == false)
            { return false; }*/
        }
        else
        { Debug.LogError(string.Format("Invalid Team (null) for TeamID {0}", teamID)); successFlag = false; }
        return successFlag;
    }

    /// <summary>
    /// Debug function to display a breakdown of the team pools
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTeamAnalysis()
    {
        StringBuilder builder = new StringBuilder();
        //get dictionary of team arcs
        Dictionary<int, TeamArc> tempDict = GameManager.i.dataScript.GetDictOfTeamArcs();
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
                reserve = GameManager.i.dataScript.CheckTeamInfo(teamData.Key, TeamInfo.Reserve);
                onMap = GameManager.i.dataScript.CheckTeamInfo(teamData.Key, TeamInfo.OnMap);
                inTransit = GameManager.i.dataScript.CheckTeamInfo(teamData.Key, TeamInfo.InTransit);
                Total = GameManager.i.dataScript.CheckTeamInfo(teamData.Key, TeamInfo.Total);
                data = string.Format("r:{0,2}  m:{1,2}  t:{2,2}  T:{3,2}", reserve, onMap, inTransit, Total);
                //display data
                builder.Append(string.Format(" {0,-12}{1,-12}", teamData.Value.name, data));
                builder.AppendLine();
            }
            //add team pool totals
            builder.AppendLine();
            builder.Append(string.Format(" {0, -20}{1,2} teams", "Reserve Pool", GameManager.i.dataScript.CheckTeamPoolCount(TeamPool.Reserve)));
            builder.AppendLine();
            builder.Append(string.Format(" {0, -20}{1,2} teams", "OnMap Pool", GameManager.i.dataScript.CheckTeamPoolCount(TeamPool.OnMap)));
            builder.AppendLine();
            builder.Append(string.Format(" {0, -20}{1,2} teams", "InTransit Pool", GameManager.i.dataScript.CheckTeamPoolCount(TeamPool.InTransit)));
            //total teams
            builder.AppendLine();
            builder.AppendLine();
            builder.Append(string.Format(" Teams in dictOfTeams   {0}", GameManager.i.dataScript.CheckNumOfTeams()));
        }
        return builder.ToString();
    }

    /// <summary>
    /// Debug method to show which actors xurrently have which teams OnMap
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTeamActorAnalysis()
    {
        List<int> listOfTeams = new List<int>();
        StringBuilder builder = new StringBuilder();
        builder.Append(" OnMap Teams by Actor");
        builder.AppendLine();
        Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActors(globalAuthority);
        foreach (Actor actor in arrayOfActors)
        {
            if (actor != null)
            {
                builder.AppendLine();
                builder.Append(string.Format("{0}  Ability {1}", actor.arc.name, actor.GetDatapoint(ActorDatapoint.Ability2)));
                builder.AppendLine();
                listOfTeams.Clear();
                listOfTeams.AddRange(actor.GetListOfTeams());
                if (listOfTeams.Count > 0)
                {
                    //loop teams
                    for (int i = 0; i < listOfTeams.Count; i++)
                    {
                        Team team = GameManager.i.dataScript.GetTeam(listOfTeams[i]);
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
    public string DebugDisplayIndividualTeams()
    {
        StringBuilder builder = new StringBuilder();
        //get dictionary of team arcs
        Dictionary<int, Team> teamDict = GameManager.i.dataScript.GetDictOfTeams();
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
        Node node = GameManager.i.dataScript.GetNode(nodeID);
        if (node != null)
        {
            //double check to see if there are teams present at the node
            List<Team> listOfTeams = node.GetListOfTeams();
            if (listOfTeams != null && listOfTeams.Count > 0)
            {
                genericDetails.returnEvent = EventType.GenericTeamRecall;
                genericDetails.textHeader = "Recall Team";
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
                        turnsAgo = GameManager.i.turnScript.Turn - team.turnDeployed;
                        if (team.timer > 0) { dataColour = colourGood; } else { dataColour = colourBad; }
                        tooltipDetails.textMain = string.Format("Deployed {0}{1}{2} turn{3} ago and will be auto-recalled in {4}{5}{6} turn{7}",
                            dataColour, turnsAgo, colourEnd, turnsAgo != 1 ? "s" : "", dataColour, team.timer, colourEnd, team.timer != 1 ? "s" : "");
                        Actor actor = GameManager.i.dataScript.GetCurrentActor(team.actorSlotID, globalAuthority);
                        if (actor != null)
                        {
                            deployedTeams = actor.CheckNumOfTeams();
                            if (deployedTeams < actor.GetDatapoint(ActorDatapoint.Ability2)) { dataColour = colourGood; } else { dataColour = colourBad; }
                            tooltipDetails.textDetails = string.Format("{0}Inserted by {1} of {2}{3}{4}{5}{6}. They have deployed {7}{8}{9}{10}{11} of {12}{13}{14}{15}{16} possible teams{17}",
                                colourNormal, GameManager.i.metaScript.GetAuthorityTitle(), colourEnd, colourActor, actor.arc.name, colourEnd, colourNormal,
                                colourEnd, dataColour, deployedTeams, colourEnd, colourNormal, colourEnd,
                                dataColour, actor.GetDatapoint(ActorDatapoint.Ability2), colourEnd, colourNormal, colourEnd);
                        }
                        else { Debug.LogError(string.Format("Invalid actor (Null) fro team.ActorSlotID {0}", team.actorSlotID)); }
                        //add to master arrays
                        genericDetails.arrayOfOptions[i] = optionDetails;
                        genericDetails.arrayOfTooltips[i] = tooltipDetails;
                    }
                    else { Debug.LogError(string.Format("Invalid team (Null) for listOfTeams[{0}]", i)); }
                    //check that limit hasn't been exceeded (maxGenericOptions)
                    if (i == maxGenericOptions)
                    {
                        Debug.LogError(string.Format("Invalid number of Teams (more than {0}) at NodeId {1}", maxGenericOptions, nodeID));
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
            EventManager.instance.PostNotification(EventType.OutcomeOpen, this, outcomeDetails, "TeamManager.cs -> InitialiseGenericPickerRecall");
        }
        else
        {
            //deactivate back button
            GameManager.i.genericPickerScript.SetBackButton(EventType.None);
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
        Node node = GameManager.i.dataScript.GetNode(details.nodeID);
        if (node != null)
        {
            //double check to see if there are teams present at the node
            List<Team> listOfTeams = node.GetListOfTeams();
            if (listOfTeams != null && listOfTeams.Count > 0)
            {
                genericDetails.returnEvent = EventType.GenericNeutraliseTeam;
                genericDetails.textHeader = "Neutralise Team";
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
                    //check that limit hasn't been exceeded (maxGenericOptions)
                    if (i == maxGenericOptions)
                    {
                        Debug.LogError(string.Format("Invalid number of Teams (more than {0}) at NodeId {1}", maxGenericOptions, details.nodeID));
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
            outcomeDetails.sprite = GameManager.i.guiScript.errorSprite;
            EventManager.instance.PostNotification(EventType.OutcomeOpen, this, outcomeDetails, "TeamManager.cs -> InitialiseGenericPickerNeutralise");
        }
        else
        {
            //deactivate back button
            GameManager.i.genericPickerScript.SetBackButton(EventType.None);
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
                Team team = GameManager.i.dataScript.GetTeam(data.optionID);
                if (team != null)
                {
                    Sprite sprite = team.arc.sprite;
                    Node node = GameManager.i.dataScript.GetNode(data.nodeID);
                    if (node != null)
                    {
                        //need to do prior to move team as data will be reset
                        textTop = GameManager.i.effectScript.SetTopTeamText(team.teamID, false);
                        textBottom = "The team will spend one turn in Transit and be available thereafter";
                        Actor actor = GameManager.i.dataScript.GetCurrentActor(team.actorSlotID, globalAuthority);
                        if (actor == null)
                        { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", team.actorSlotID)); }
                        if (MoveTeam(TeamPool.InTransit, team.teamID, team.actorSlotID, node) == true)
                        {
                            //NodeActionData
                            NodeActionData nodeActionData = new NodeActionData()
                            {
                                turn = GameManager.i.turnScript.Turn,
                                actorID = actor.actorID,
                                nodeID = node.nodeID,
                                teamID = team.teamID,
                                dataName = string.Format("{0} {1}, ID {2}", team.arc.name, team.teamName, team.teamID),
                                nodeAction = NodeAction.ActorRecallTeam
                            };
                            //add to actor's personal list
                            actor.AddNodeAction(nodeActionData);
                            Debug.LogFormat("[Tst] TeamManager.cs -> MoveTeam: nodeActionData added to {0}, {1}{2}", actor.actorName, actor.arc.name, "\n");
                            //message
                            string text = string.Format("{0} {1}, ID {2}, RECALLED from {3}, ID {4}", team.arc.name, team.teamName, team.teamID,
                                node.nodeName, node.nodeID);
                            string reason = string.Format("{0} has done so on your orders", actor.actorName);
                            GameManager.i.messageScript.TeamWithdraw(text, reason, node, team, actor);
                            Debug.LogFormat("[Tea] TeamManager.cs -> ProcessRecallTeam: {0}{1}", text, "\n");
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
                        EventManager.instance.PostNotification(EventType.OutcomeOpen, this, details, "TeamManager.cs -> ProcessRecallTeam");
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
        bool isPlayer = false;
        bool successFlag = true;
        if (data.optionID > -1)
        {
            //get currently selected node
            if (data.nodeID != -1)
            {
                Team team = GameManager.i.dataScript.GetTeam(data.optionID);
                if (team != null)
                {
                    Sprite sprite = team.arc.sprite;
                    Node node = GameManager.i.dataScript.GetNode(data.nodeID);
                    if (node != null)
                    {
                        if (node.nodeID == GameManager.i.nodeScript.GetPlayerNodeID()) { isPlayer = true; }
                        Actor actor = GameManager.i.dataScript.GetCurrentActor(data.actorSlotID, globalResistance);
                        if (actor != null)
                        {
                            StringBuilder builderTop = new StringBuilder();
                            StringBuilder builderBottom = new StringBuilder();
                            bool proceedFlag = false;
                            //move team
                            if (GameManager.i.sideScript.authorityOverall == SideState.AI)
                            { proceedFlag = MoveTeamAI(TeamPool.InTransit, team.teamID, node); }
                            else
                            { proceedFlag = MoveTeam(TeamPool.InTransit, team.teamID, team.actorSlotID, node); }
                            //successful?
                            if (proceedFlag == true)
                            {
                                //message (notification to Authority Side)
                                string text = string.Format("{0} {1}, ID {2}, NEUTRALISED at {3}, ID {4}", team.arc.name, team.teamName, team.teamID,
                                    node.nodeName, node.nodeID);
                                GameManager.i.messageScript.TeamNeutralise(text, node, team, actor);
                                Debug.LogFormat("[Tea] TeamManager.cs -> ProcessNeutraliseTeam: {0}{1}", text, "\n");
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
                                    dataInput.originText = "Neutralise Team";
                                    foreach (Effect effect in listOfEffects)
                                    {
                                        if (effect.ignoreEffect == false)
                                        {
                                            EffectDataReturn effectReturn = GameManager.i.effectScript.ProcessEffect(effect, node, dataInput, actor);
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
                                //NodeActionData package
                                if (isPlayer == false)
                                {
                                    //actor
                                    NodeActionData nodeActionData = new NodeActionData()
                                    {
                                        turn = GameManager.i.turnScript.Turn,
                                        actorID = actor.actorID,
                                        nodeID = node.nodeID,
                                        dataName = string.Format("{0} {1}, ID {2}", team.arc.name, team.teamName, team.teamID),
                                        nodeAction = NodeAction.ActorNeutraliseTeam
                                    };
                                    //add to actor's personal list
                                    actor.AddNodeAction(nodeActionData);
                                    Debug.LogFormat("[Tst] TeamManager.cs -> ProcessNeutraliseTeam: nodeActionData added to {0}, {1}{2}", actor.actorName, actor.arc.name, "\n");
                                }
                                else
                                {
                                    //player action
                                    NodeActionData nodeActionData = new NodeActionData()
                                    {
                                        turn = GameManager.i.turnScript.Turn,
                                        actorID = 999,
                                        nodeID = node.nodeID,
                                        dataName = string.Format("{0} {1}", team.arc.name, team.teamName),
                                        nodeAction = NodeAction.PlayerNeutraliseTeam
                                    };
                                    //add to player's personal list
                                    GameManager.i.playerScript.AddNodeAction(nodeActionData);
                                    Debug.LogFormat("[Tst] TeamManager.cs -> ProcessNeutraliseTeam: nodeActionData added to {0}, {1}{2}", GameManager.i.playerScript.PlayerName, "Player", "\n");
                                    //statistics
                                    GameManager.i.dataScript.StatisticIncrement(StatType.PlayerNodeActions);
                                }
                                //statistics
                                GameManager.i.dataScript.StatisticIncrement(StatType.NodeActionsResistance);
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
                            EventManager.instance.PostNotification(EventType.OutcomeOpen, this, details, "TeamManager.cs -> ProcessNeutraliseTeam");
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
    /// Implements PERMANENT team effects (Player/AI) on nodes at completion of teams OnMap Timer.
    /// Node and Team are assumed to be checked for Null by the parent method
    /// Actor is 'null' for an AI operation
    /// </summary>
    /// <param name="team"></param>
    /// <param name="node"></param>
    private void ProcessTeamEffect(Team team, Node node, Actor actor)
    {
        bool isError = false;
        string text, itemText, effectText;
        Effect teamEffect = team.arc.activeEffect;
        EffectDataReturn effectReturn = new EffectDataReturn();
        switch (team.arc.name)
        {
            case "CONTROL":
            case "CIVIL":
            case "MEDIA":
                if (teamEffect != null)
                {
                    EffectDataInput dataInput = new EffectDataInput();
                    dataInput.originText = team.arc.name;
                    effectReturn = GameManager.i.effectScript.ProcessEffect(teamEffect, node, dataInput, actor);
                    isError = effectReturn.errorFlag;
                    if (isError == false)
                    {
                        text = string.Format("{0} {1} effect: {2} at \"{3}\", ID {4}", team.arc.name, team.teamName, teamEffect.description, node.nodeName, node.nodeID);
                        itemText = string.Format("{0} Team completes TASK at {1}", team.arc.name, node.nodeName);
                        effectText = string.Format("{0}{1}{2}", colourGood, teamEffect.description, colourEnd);
                        //both sides get a message for these teams
                        GameManager.i.messageScript.TeamEffect(text, itemText, effectText, node, team, true);
                    }
                    else { Debug.LogWarningFormat("{0} effectReturn.errorFlag TRUE", team.arc.name); }
                }
                break;
            case "PROBE":
                //destroy tracer at node if present
                if (node.isTracer == true)
                {
                    node.RemoveTracer();
                    //message
                    text = string.Format("{0} {1}: Tracer destroyed at \"{2}\", ID {3}", team.arc.name, team.teamName, node.nodeName, node.nodeID);
                    itemText = string.Format("{0} Team DESTROYS Tracer", team.arc.name);
                    effectText = string.Format("{0}Resistance TRACER neutralised{1}", colourGood, colourEnd);
                    GameManager.i.messageScript.TeamEffect(text, itemText, effectText, node, team);
                }
                else
                {
                    //no tracer present
                    text = string.Format("{0} {1}: Tracer not found at \"{2}\", ID {3}", team.arc.name, team.teamName, node.nodeName, node.nodeID);
                    itemText = string.Format("{0} Team doesn't find Tracer", team.arc.name);
                    effectText = string.Format("{0}No Resistance TRACER present{1}", colourNeutral, colourEnd);
                    GameManager.i.messageScript.TeamEffect(text, itemText, effectText, node, team);
                }
                break;
            case "SPIDER":
                if (node.isSpider == false)
                {
                    //add spider
                    node.isSpider = true;
                    node.spiderTimer = GameManager.i.nodeScript.observerTimer;
                    //check if same node as a Tracer
                    if (node.isTracer == true)
                    { node.isSpiderKnown = true; }
                    else { node.isSpiderKnown = false; }
                    //message
                    text = string.Format("{0} {1}: Spider inserted at \"{2}\", ID {3}", team.arc.name, team.teamName, node.nodeName, node.nodeID);
                    itemText = string.Format("{0} Team completes TASK at District", team.arc.name);
                    effectText = string.Format("{0}Spider installed to monitor Resistance activity{1}", colourGood, colourEnd);
                    GameManager.i.messageScript.TeamEffect(text, itemText, effectText, node, team);
                }
                else
                {
                    //spider already present -> reset timer
                    node.spiderTimer = GameManager.i.nodeScript.observerTimer;
                    //message
                    text = string.Format("{0} {1}: Spider NOT inserted at \"{2}\", ID {3}", team.arc.name, team.teamName, node.nodeName, node.nodeID);
                    itemText = string.Format("{0} Team completes TASK at District", team.arc.name);
                    effectText = string.Format("{0}Spider already present in District, timer updated{1}", colourGood, colourEnd);
                    GameManager.i.messageScript.TeamEffect(text, itemText, effectText, node, team);
                }
                break;
            case "ERASURE":
                //deletes any KNOWN Resistance contacts
                if (node.isContactKnown == true)
                {
                    if (node.isContactResistance == true)
                    {
                        int numRemoved = GameManager.i.dataScript.RemoveContactsNode(node.nodeID, "due to an ERASURE Team", false, globalResistance);
                        if (numRemoved > 0)
                        {
                            //message
                            text = string.Format("{0} {1}: {2} known Contact{3} ERASED at {4}, {5}", team.arc.name, team.teamName, numRemoved, numRemoved != 1 ? "s" : "", node.nodeName, node.Arc.name);
                            itemText = string.Format("{0} Team completes TASK at District", team.arc.name);
                            effectText = string.Format("{0}{1} Resistance Contact{2} ERASED{3}", colourGood, numRemoved, numRemoved != 1 ? "s" : "", colourEnd);
                            GameManager.i.messageScript.TeamEffect(text, itemText, effectText, node, team);
                        }
                    }
                    else
                    {
                        //known contacts but none present
                        text = string.Format("{0} {1}: Zero known Contacts ERASED at {2}, {3}", team.arc.name, team.teamName, node.nodeName, node.Arc.name);
                        itemText = string.Format("{0} Team completes TASK at District", team.arc.name);
                        effectText = string.Format("{0}Zero KNOWN Resistance Contacts ERASED{1}", colourNeutral, colourEnd);
                        GameManager.i.messageScript.TeamEffect(text, itemText, effectText, node, team);
                        /*//error condition -> shouldn't happen
                        Debug.LogWarningFormat("Zero Known contacts at nodeID {0} Erased by teamID {1}{2}", node.nodeID, team.teamID, "\n");*/
                    }
                    //Authority no longer has any knowledge of contacts at node
                    node.isContactKnown = false;
                }
                else
                {
                    //contacts unknown
                    text = string.Format("{0} {1}: No Resistance Contacts Known at {2}, {3}", team.arc.name, team.teamName, node.nodeName, node.Arc.name);
                    itemText = string.Format("{0} Team completes TASK at District", team.arc.name);
                    effectText = string.Format("{0}No KNOWN Resistance Contacts present at District{1}", colourNeutral, colourEnd);
                    GameManager.i.messageScript.TeamEffect(text, itemText, effectText, node, team);
                }
                break;
            case "DAMAGE":
                //at node with a completed, but uncontained, target?
                if (string.IsNullOrEmpty(node.targetName) == false)
                {
                    Target target = GameManager.i.dataScript.GetTarget(node.targetName);
                    if (target != null)
                    {
                        if (target.targetStatus == Status.Outstanding && target.ongoingID > -1)
                        {
                            //contain target and shut down all ongoing node effects
                            GameManager.i.targetScript.ContainTarget(target);
                            //message
                            text = string.Format("Target \"{0}\" Contained by {1} {2}", target.targetName, team.arc.name, team.teamName);
                            GameManager.i.messageScript.TargetContained(text, node, team, target);
                        }
                    }
                    else { Debug.LogError(string.Format("Invalid Target (Null) for targetID {0}", node.targetName)); }
                }
                break;
            default:
                Debug.LogError(string.Format("Invalid team Arc name \"{0}\"", team.arc.name));
                isError = true;
                break;
        }
        //assign renown to originating actor if all O.K
        if (isError == false && actor != null)
        { actor.Renown++; }
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
        bool isProceed = true;
        if (actor != null)
        {
            List<int> listOfTeams = actor.GetListOfTeams();
            if (listOfTeams.Count > 0)
            {
                //loop teams and remove all active traces
                for (int i = 0; i < listOfTeams.Count; i++)
                {
                    Team team = GameManager.i.dataScript.GetTeam(listOfTeams[i]);
                    if (team != null)
                    {
                        counter++;
                        //update tallies & remove team from their pool
                        GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve, +1);
                        switch (team.pool)
                        {
                            case TeamPool.OnMap:
                                if (GameManager.i.dataScript.RemoveTeamFromPool(TeamPool.OnMap, team.teamID) == true)
                                { GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.OnMap, -1); }
                                else
                                {
                                    isProceed = false;
                                    Debug.LogWarningFormat("{0} Team, id {1}, NOT Removed from OnMap pool", team.arc.name, team.teamID);
                                }
                                break;
                            case TeamPool.InTransit:
                                if (GameManager.i.dataScript.RemoveTeamFromPool(TeamPool.InTransit, team.teamID) == true)
                                { GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.InTransit, -1); }
                                else
                                {
                                    isProceed = false;
                                    Debug.LogWarningFormat("{0} Team, id {1}, NOT Removed from InTransit pool", team.arc.name, team.teamID);
                                }
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid team.pool \"{0}\"", team.pool));
                                break;
                        }
                        if (isProceed == true)
                        {
                            //remove from node list
                            Node node = GameManager.i.dataScript.GetNode(team.nodeID);
                            if (node != null)
                            { node.RemoveTeam(team.teamID); }
                            else { Debug.LogError(string.Format("Invalid node (Null) for team.nodeID {0}, team {1}, ID {2}", team.nodeID, team.arc.name, team.teamID)); }
                            //add to reserve pool
                            GameManager.i.dataScript.AddTeamToPool(TeamPool.Reserve, team.teamID);
                            //update team status
                            team.ResetTeamData(TeamPool.Reserve);
                            //confirmation
                            Debug.LogFormat("[Tea] TeamManager.cs -> ProcessNeutraliseTeam: {0} {1}, ID {2}, AUTO MOVED to {3} (Actor has left){4}", team.arc.name, team.teamName, team.teamID, team.pool, "\n");
                            //message
                            string text = string.Format("{0} {1}, ID {2}, withdrawn early from {3}, ID {4}", team.arc.name, team.teamName, team.teamID,
                                node.nodeName, node.nodeID);
                            string reason = string.Format("{0} had to do so due to them leaving ACTIVE duty", actor.actorName);
                            GameManager.i.messageScript.TeamWithdraw(text, reason, node, team, actor);
                        }
                    }
                    else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}, actor {1}, ID {2}", listOfTeams[i], actor.arc.name, actor.actorID)); }
                }
                //empty list
                actor.RemoveAllTeams(); ;
            }
        }
        else { Debug.LogError("Invalid actor (Null)"); }
        //return number of teams removed and sent to the reserves
        return counter;
    }


    /// <summary>
    /// method run when an AI vs AI auto run is complete and control reverts to the Authority side. Need to do so as OnMap and InTransit teams aren't associated with an actor (AI doesn't need to) and
    /// errors will occur otherwise.
    /// </summary>
    public void AutoRunAssignActors()
    {
        List<int> listOfTeams = new List<int>();
        listOfTeams.AddRange(GameManager.i.dataScript.GetTeamPool(TeamPool.OnMap));
        listOfTeams.AddRange(GameManager.i.dataScript.GetTeamPool(TeamPool.InTransit));
        //assign actors to teams
        if (listOfTeams != null)
        {
            int numUpdated = 0;
            int availableTeamSlots;
            int count = listOfTeams.Count;
            if (count > 0)
            {
                //make a list of actors (slotID) with spare slots for teams (each actor can only have one team per ability level)
                List<int> listOfActorSlots = new List<int>();
                //loop actors
                Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActors(globalAuthority);
                if (arrayOfActors != null)
                {
                    for (int i = 0; i < arrayOfActors.Length; i++)
                    {
                        //check actor is present in slot (not vacant)
                        if (GameManager.i.dataScript.CheckActorSlotStatus(i, globalAuthority) == true)
                        {
                            Actor actor = arrayOfActors[i];
                            if (actor != null)
                            {
                                /*//delete any debug Node/TeamActionData -> EDIT : Best to leave in place as already assigned actors (only pick up exceptions)
                                actor.ClearAllNodeActions();
                                actor.ClearAllTeamActions();*/

                                //one entry per actor for each spare team slot they have available
                                availableTeamSlots = actor.GetDatapoint(ActorDatapoint.Ability2) - actor.CheckNumOfTeams();
                                for (int j = 0; j < availableTeamSlots; j++)
                                { listOfActorSlots.Add(actor.slotID); }
                            }
                        }
                    }
                }
                else { Debug.LogError("Invalid arrayOfActors (Null)"); }
                //assign teams to a random actor slot chosen from list of available ones or to a random actor slot if the list is empty
                int index, slotID;
                int listCount;
                for (int i = 0; i < count; i++)
                {
                    Team team = GameManager.i.dataScript.GetTeam(listOfTeams[i]);
                    if (team != null)
                    {
                        //most teams should have a valid actorSlot
                        if (team.actorSlotID > -1)
                        {
                            if (team.pool == TeamPool.OnMap)
                            {
                                //need to assign team to their assigned actor 
                                Actor actor = GameManager.i.dataScript.GetCurrentActor(team.actorSlotID, globalAuthority);
                                if (actor != null)
                                {
                                    actor.AddTeam(team.teamID);
                                    Debug.LogFormat("[Tea] TeamManager.cs -> AutoRunAssignActors: {0} {1}, id {2} assigned (existing) to {3}, {4}, ID{5}{6}",
                                        team.arc.name, team.teamName, team.teamID, actor.actorName, actor.arc.name, actor.actorID, "\n");
                                }
                                else
                                {
                                    //invalid actor, send team to reserves and remove any possility of problems by clearing the deck
                                    SendTeamToReserves(team);
                                }
                                numUpdated++;
                            }
                        }
                        //team hasn't already got an actorSlotID (Expect that most have, only looking for exceptions here)
                        else
                        {
                            listCount = listOfActorSlots.Count;
                            if (listCount > 0)
                            {
                                //only do for onMap teams
                                if (team.pool == TeamPool.OnMap)
                                {
                                    //available actors
                                    index = Random.Range(0, listCount);
                                    slotID = listOfActorSlots[index];
                                    team.actorSlotID = slotID;
                                    Actor actor = GameManager.i.dataScript.GetCurrentActor(slotID, globalAuthority);
                                    if (actor != null)
                                    {
                                        //Add team to actor's list of Teams
                                        actor.AddTeam(team.teamID);
                                        Debug.LogFormat("[Tea] TeamManager.cs -> AutoRunAssignActors: {0} {1}, id {2} assigned (random) to {3}, {4}, ID{5}{6}",
                                            team.arc.name, team.teamName, team.teamID, actor.actorName, actor.arc.name, actor.actorID, "\n");
                                        //NodeActionData record only if authority player, ignore otherwise
                                        if (GameManager.i.sideScript.PlayerSide.level == globalAuthority.level)
                                        {
                                            //add a new record to match team 
                                            NodeActionData nodeActionData = new NodeActionData()
                                            {
                                                turn = team.turnDeployed,
                                                actorID = actor.actorID,
                                                nodeID = team.nodeID,
                                                teamID = team.teamID,
                                                dataName = string.Format("{0} {1}, ID {2}", team.arc.name, team.teamName, team.teamID),
                                                nodeAction = NodeAction.ActorDeployTeam
                                            };
                                            //add to actor's personal list
                                            actor.AddNodeAction(nodeActionData);
                                            Debug.LogFormat("[Tst] TeamManager.cs -> AutoRunAssignActors: nodeActionData added to {0}, {1}{2}", actor.actorName, actor.arc.name, "\n");
                                        }

                                    }
                                    else { Debug.LogErrorFormat("Invalid current Actor (Null) for slotID {0}", slotID); }
                                    //delete list entry to prevent dupes
                                    listOfActorSlots.RemoveAt(index);
                                }
                            }
                            else
                            {
                                //no more actors present. Team auto sent to Reserves (avoid using MoveTeamAI as this bypasses the normal sequence of OnMap -> Transit -> Reserve)
                                SendTeamToReserves(team);
                            }
                            numUpdated++;
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid Team (null) for teamID {0}", listOfTeams[i]); }
                    //debug check
                    if (team.pool == TeamPool.OnMap)
                    { Debug.AssertFormat(team.actorSlotID > -1, "Invalid actorSlotID (-1) for team {0}, {1}, ID {2}", team.arc, team.teamName, team.teamID); }
                }
            }
            Debug.LogFormat("[Tea] TeamManager.cs -> AutoRunAssignActors: {0} teams of {1} have had Actors assigned{2}", numUpdated, count, "\n");
        }
        else { Debug.LogError("Invalid listOfTeams (Null)"); }
    }

    /// <summary>
    /// subMethod for AutoRunAssignActors to handle edge case situations where a team can't be allocated to an actor and needs to be sent to the reserves
    /// </summary>
    /// <param name="teamID"></param>
    private void SendTeamToReserves(Team team)
    {
        if (team != null)
        {
            //no more actors present. Team auto sent to Reserves (avoid using MoveTeamAI as this bypasses the normal sequence of OnMap -> Transit -> Reserve)
            switch (team.pool)
            {
                case TeamPool.InTransit:
                    if (GameManager.i.dataScript.RemoveTeamFromPool(TeamPool.InTransit, team.teamID) == true)
                    {
                        GameManager.i.dataScript.AddTeamToPool(TeamPool.Reserve, team.teamID);
                        //adjust tallies
                        GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve, +1);
                        GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.InTransit, -1);
                        //update team status
                        team.ResetTeamData(TeamPool.Reserve);
                        Debug.LogFormat("[Tea] TeamManager.cs -> AutoRunAssignActor: {0}, {1}, ID {2}, moved to Reserve (NO ACTOR available){3}",
                            team.arc.name, team.teamName, team.teamID, "\n");
                    }
                    else { Debug.LogErrorFormat("{0} Team, id {1}, NOT Removed from InTransit pool", team.arc.name, team.teamID); }
                    break;
                case TeamPool.OnMap:
                    Node node = GameManager.i.dataScript.GetNode(team.nodeID);
                    if (node != null)
                    {
                        if (GameManager.i.dataScript.RemoveTeamFromPool(TeamPool.OnMap, team.teamID) == true)
                        {

                            //adjust tallies
                            GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.Reserve, +1);
                            GameManager.i.dataScript.AdjustTeamInfo(team.arc.TeamArcID, TeamInfo.OnMap, -1);
                            GameManager.i.dataScript.AddTeamToPool(TeamPool.Reserve, team.teamID);
                            //remove from node list
                            node.RemoveTeam(team.teamID);
                            //update team status
                            team.ResetTeamData(TeamPool.Reserve);
                            //confirmation
                            Debug.LogFormat("[Tea] TeamManager.cs -> AutoRunAssignActor: {0} {1}, ID {2}, moved to Reserve, from nodeID {3} (NO ACTOR available){4}",
                                team.arc.name, team.teamName, team.teamID, node.nodeID, "\n");
                        }
                        else { Debug.LogErrorFormat("{0} Team, id {1}, NOT Removed from OnMap pool", team.arc.name, team.teamID); }
                    }
                    else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0} (Team not moved to reserves)", team.nodeID); }
                    break;
                default:
                    Debug.LogErrorFormat("Unrecognised team.pool \"[0}\"", team.pool);
                    break;
            }
        }
        else { Debug.LogWarning("Invalid team (Null)"); }
    }


    public List<TeamArc> GetListOfTeamPrioritiesHigh()
    { return listOfTeamPrioritiesHigh; }

    public List<TeamArc> GetListOfTeamPrioritiesMed()
    { return listOfTeamPrioritiesMedium; }

    //place new method above here
}
