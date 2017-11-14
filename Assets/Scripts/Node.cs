using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using modalAPI;
using gameAPI;


public class Node : MonoBehaviour
{
    //NOTE -> LevelManager.arrayOfActiveNodes stores access data, eg. which nodes are active for which actor?

    public int NodeID { get; set; }                     //unique ID, sequentially derived from GameManager nodeCounter, don't skip numbers, keep it sequential, 0+
    public Material _Material { get; private set; }     //material renderer uses to draw node
    public string NodeName { get; set; }                //name of node, eg. "Downtown Bronx"
    public NodeArc arc;                                 //archetype type
    
    public int Stability { get; set; }                  //range 0 to 3
    public int Support { get; set; }                    //range 0 to 3
    public int Security { get; set; }                   //range 0 to 3

    public int NumOfTracers { get; set; }
    public int TargetID { get; set; }                   //unique ID, 0+, -1 indicates no target


    private List<Vector3> listOfNeighbours;             //list of neighbouring nodes that this node is connected to
    private List<Team> listOfTeams;                     //Authority teams present at the node

    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip
    private float fadeInTime;                           //tooltip

    /// <summary>
    /// Initialise SO's for Nodes
    /// </summary>
    /// <param name="archetype"></param>
    public void Initialise(NodeArc archetype)
    {
        arc = archetype;
    }
	
	private void Awake ()
    {
        listOfNeighbours = new List<Vector3>();
        listOfTeams = new List<Team>();
        _Material = GameManager.instance.nodeScript.GetNodeMaterial(NodeType.Normal);
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade;
        //TargetID = -1;
	}

    /// <summary>
    /// add neighbouring vector3 to list
    /// </summary>
    /// <param name="pos"></param>
    public void AddNeighbour(Vector3 pos)
    {
        listOfNeighbours.Add(pos);
        //Debug.Log("Neighbour added: " + pos);
    }

    /// <summary>
    /// Checks if a Vector3 node position is already present in the list of neighbours, e.g returns true if a connection already present
    /// </summary>
    /// <param name="newPos"></param>
    /// <returns></returns>
    public bool CheckNeighbours(Vector3 newPos)
    {
        if (listOfNeighbours.Count == 0)
        { return false; }
        else
        {
            if (listOfNeighbours.Exists(pos => pos == newPos))
            { return true;  }
            //default condition -> no match found
            return false;
        }
    }

    /// <summary>
    /// get list of neighbours
    /// </summary>
    /// <returns></returns>
    public List<Vector3> GetNeighbours()
    { return listOfNeighbours; }

    /// <summary>
    /// Mouse click
    /// </summary>
    private void OnMouseDown()
    {
        if (GameManager.instance.IsBlocked() == false)
        {
            //highlight current node
            GameManager.instance.nodeScript.ToggleNodeHighlight(NodeID);
            //exit any tooltip
            if (onMouseFlag == true)
            {
                onMouseFlag = false;
                StopCoroutine("ShowTooltip");
                GameManager.instance.tooltipNodeScript.CloseTooltip();
            }
            //Action Menu
            ModalPanelDetails details = new ModalPanelDetails()
            {
                nodeID = NodeID,
                nodeName = this.NodeName,
                nodeDetails = string.Format("{0} ID {1}", arc.name, NodeID),
                nodePos = transform.position,
                listOfButtonDetails = GameManager.instance.actorScript.GetActorActions(NodeID)
            };
            //activate menu
            GameManager.instance.actionMenuScript.SetActionMenu(details);
        }
    }

    /// <summary>
    /// mouse over exit, shut down tooltip
    /// </summary>
    private void OnMouseExit()
    {
        if (GameManager.instance.IsBlocked() == false)
        {
            onMouseFlag = false;
            StopCoroutine("ShowTooltip");
            GameManager.instance.tooltipNodeScript.CloseTooltip();
        }
    }

    /// <summary>
    /// Mouse Over tool tip
    /// </summary>
    private void OnMouseOver()
    {
        if (GameManager.instance.IsBlocked() == false)
        {
            onMouseFlag = true;
            StartCoroutine(ShowTooltip());
        }
    }


    /// <summary>
    /// tooltip coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over node
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.instance.tooltipNodeScript.CheckTooltipActive() == false)
            {
                List<string> activeList = GetNodeActors();
                List<string> teamList = new List<string>();
                if (listOfTeams.Count > 0)
                {
                    foreach (Team team in listOfTeams)
                    { teamList.Add(team.Arc.name.ToUpper()); }
                }
                List<string> targetList = new List<string>();
                if (TargetID > -1)
                { targetList = GameManager.instance.targetScript.GetTargetTooltip(TargetID); }
                //Transform transform = GetComponent<Transform>();
                GameManager.instance.tooltipNodeScript.SetTooltip(
                    NodeName,
                    string.Format("{0} ID {1}", arc.name, NodeID),
                    activeList,
                    new int[] { Stability, Support, Security },
                    teamList,
                    targetList,
                    transform.position
                    );
                yield return null;
            }
            //fade in
            float alphaCurrent;
            while (GameManager.instance.tooltipNodeScript.GetOpacity()< 1.0)
            {
                alphaCurrent = GameManager.instance.tooltipNodeScript.GetOpacity();
                alphaCurrent += Time.deltaTime / fadeInTime;
                GameManager.instance.tooltipNodeScript.SetOpacity(alphaCurrent);
                yield return null;
            }
        }
    }

    /// <summary>
    /// returns a list of actors for whom this node is active
    /// </summary>
    /// <returns></returns>
    public List<string> GetNodeActors()
    {
        List<string> tempList = new List<string>();
        int limit = GameManager.instance.actorScript.numOfActorsTotal;
        Side side = GameManager.instance.optionScript.PlayerSide;
        for (int i = 0; i < limit; i++)
        {
            if (GameManager.instance.levelScript.CheckNodeActive(NodeID, side, i) == true)
            {
                tempList.Add(GameManager.instance.dataScript.GetActorType(i, side));
            }
        }
        return tempList;
    }

    public void SetMaterial(Material newMaterial)
    { _Material = newMaterial; }

    public int GetNumOfNeighbours()
    { return listOfNeighbours.Count; }

    public Material GetMaterial()
    { return _Material; }

    /// <summary>
    /// add an authority team to the node. Returns true if placement successful.
    /// Max one instance of each type of team at node
    /// Max cap on number of teams at node
    /// </summary>
    /// <param name="team"></param>
    public bool AddTeam(Team team, int actorID)
    {
        if (team != null)
        {
            //check there is room for another team
            if (listOfTeams.Count < GameManager.instance.teamScript.maxTeamsAtNode)
            {
                //check a similar type of team not already present
                int nodeArcID = team.Arc.TeamArcID;
                if (listOfTeams.Count > 0)
                {
                    foreach(Team teamExisting in listOfTeams)
                    {
                        if (teamExisting.Arc.TeamArcID == nodeArcID)
                        {
                            //already a similar team present -> no go
                            Debug.LogWarning(string.Format("{0} Team NOT added to node {1}, ID {2} as already a similar team present{3}", 
                                team.Arc.name, NodeName, NodeID, "\n"));
                            return false;
                        }
                    }
                }
                //Add team
                listOfTeams.Add(team);
                //initialise Team data
                team.NodeID = NodeID;
                team.ActorSlotID = actorID;
                team.Pool = TeamPool.OnMap;
                team.Timer = GameManager.instance.teamScript.deployTime;
                Debug.Log(string.Format("{0} Team added to node {1}, ID {2}{3}", team.Arc.name, NodeName, NodeID, "\n"));
                return true;
            }
            else { Debug.LogWarning(string.Format("Maximum number of teams already present at Node {0}, ID {1}{2}", NodeName, NodeID, "\n")); }
        }
        else { Debug.LogError(string.Format("Invalid team (null) for Node {0}, ID {1}{2}", NodeName, NodeID, "\n")); }
        return false;
    }

    /// <summary>
    /// Remove a team with the matching teamID from the listOfTeams and adjust team status
    /// </summary>
    /// <param name="teamID"></param>
    public void RemoveTeam(int teamID)
    {
        for(int i = 0; i < listOfTeams.Count; i++)
        {
            
            if (listOfTeams[i].TeamID == teamID)
            {
                listOfTeams.RemoveAt(i);
                Debug.Log(string.Format("TeamID {0} removed from Node ID {1}", teamID, NodeID));
                return;
            }
        }
        //failed to find team
        Debug.LogError(string.Format("TeamID {0} not found in listOfTeams. Failed to remove team", teamID));
    }


    /// <summary>
    /// Returns number of teams present at node, '0' if none
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfTeams()
    { return listOfTeams.Count; }


    /// <summary>
    /// returns true if a team of that type is present at the node
    /// </summary>
    /// <param name="teamArcID"></param>
    /// <returns></returns>
    public bool CheckTeamPresent(int teamArcID)
    {
        if (listOfTeams.Count > 0 && teamArcID > -1)
        {
            foreach (Team team in listOfTeams)
            {
                if (team.Arc.TeamArcID == teamArcID)
                { return true; }
            }
        }
        return false;
    }

    /// <summary>
    /// returns empty list if none
    /// </summary>
    /// <returns></returns>
    public List<Team> GetTeams()
    { return listOfTeams; }


    //place methods above here
}
