using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using modalAPI;


public class Node : MonoBehaviour
{
    //NOTE -> LevelManager.arrayOfActiveNodes stores access data, eg. which nodes are active for which actor?

    public int nodeID;                                  //unique ID, sequentially derived from GameManager nodeCounter, don't skip numbers, keep it sequential, 0+
    public Material _Material { get; private set; }     //material renderer uses to draw node
    public string nodeName { get; set; }                //name of node, eg. "Downtown Bronx"
    public NodeArc arc;                                 //archetype type
    
    [HideInInspector] public int stability;
    [HideInInspector] public int support;
    [HideInInspector] public int security;
    

    private List<Vector3> listOfNeighbours;             //list of neighbouring nodes that this node is connected to
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
        _Material = GameManager.instance.nodeScript.GetNodeMaterial(NodeType.Normal);
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade;
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
        if (GameManager.instance.isBlocked == false)
        {
            //highlight current node
            GameManager.instance.nodeScript.ToggleNodeHighlight(nodeID);
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
                nodeID = nodeID,
                nodeName = this.nodeName,
                nodeDetails = string.Format("{0} ID {1}", arc.name.ToUpper(), nodeID),
                nodePos = transform.position,
                listOfButtonDetails = GameManager.instance.actorScript.GetActorActions(nodeID)
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
        if (GameManager.instance.isBlocked == false)
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
        if (GameManager.instance.isBlocked == false)
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
                List<string> targetList = new List<string>() { "Power Grid", "All Nodes Stability -1", "Rolling Blackouts", "Info 3" };
                //Transform transform = GetComponent<Transform>();
                GameManager.instance.tooltipNodeScript.SetTooltip(
                    nodeName,
                    string.Format("{0} ID {1}", arc.name, nodeID),
                    activeList,
                    new int[] { stability, support, security },
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
        for (int i = 0; i < limit; i++)
        {
            if (GameManager.instance.levelScript.CheckNodeActive(nodeID, i))
            {
                tempList.Add(GameManager.instance.actorScript.GetActorType(i));
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


}
