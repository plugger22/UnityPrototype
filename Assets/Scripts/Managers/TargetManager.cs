using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Handles all node target related matters
/// </summary>
public class TargetManager : MonoBehaviour
{
    [Range(0, 20)]
    [Tooltip("The % of the total Nodes on the level which commence with a Live target")]
    public int startPercentTargets = 10;
    [Range(20, 50)]
    [Tooltip("The % of the total Nodes on the level that can have a Live target at any one time")]
    public int maxPercentTargets = 25;

    public int StartTargets { get; private set; }
    public int ActiveTargets { get; private set; }
    public int LiveTargets { get; private set; }
    public int MaxTargets { get; private set; }

    //colour Palette
    private string colourGood;
    private string colourGear;
    private string colourNormal;
    private string colourDefault;
    private string colourDataGood;
    private string colourDataNeutral;
    private string colourDataBad;
    private string colourEnd;

    /// <summary>
    /// Initial setup
    /// </summary>
    public void Initialise()
    {
        //calculate limits
        int numOfNodes = GameManager.instance.dataScript.GetNumOfNodes();
        StartTargets = numOfNodes * startPercentTargets / 100;
        MaxTargets = numOfNodes * maxPercentTargets / 100;
        ActiveTargets = MaxTargets - StartTargets;
        ActiveTargets = Mathf.Max(0, ActiveTargets);
        //Set initialise targets on map
        SetRandomTargets(StartTargets, Status.Live);

        //SetRandomTargets(ActiveTargets, Status.Active);

        //event listener
        EventManager.instance.AddListener(EventType.ChangeColour, this.OnEvent);
    }


    /// <summary>
    /// Event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Set colour palette for tooltip
    /// </summary>
    public void SetColours()
    {
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourGear = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourDataGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourDataNeutral = GameManager.instance.colourScript.GetColour(ColourType.dataNeutral);
        colourDataBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// Populates a set number of randomly chosen level 1 targets into the level with the indicated status
    /// </summary>
    /// <param name="numOfTargets"></param>
    /// <param name="status"></param>
    public void SetRandomTargets(int numOfTargets, Status status = Status.Active)
    {
        Debug.Assert(numOfTargets > 0 && numOfTargets <= GameManager.instance.dataScript.GetNumOfPossibleTargets(), "Invalid numOfTargets parameter");
        int index, nodeArcID, totalOfType, totalOfTypewithTargets, totalActive, totalLive, totalTargets, endlessCounter;
        int counter = 0;
        bool successFlag;
        List<Target> listOfPossibleTargets = GameManager.instance.dataScript.GetPossibleTargets();
        for(int i = 0; i < numOfTargets; i++)
        {
            //successflag breaks out of while loop if a suitable node is found
            successFlag = false;
            endlessCounter = 0;
            //keep chosing a node until you find a suitable one (max 5 goes)
            while (successFlag == false)
            {
                //get a random target
                index = Random.Range(0, listOfPossibleTargets.Count);
                Target target = listOfPossibleTargets[index];
                //get target nodeArcId
                nodeArcID = target.nodeArc.NodeArcID;
                //check that there is a suitable spare node available
                totalOfType = GameManager.instance.dataScript.GetNodeInfo(nodeArcID, NodeInfo.Number);
                totalOfTypewithTargets = GameManager.instance.dataScript.GetNodeInfo(nodeArcID, NodeInfo.TargetsAll);
                if ((totalOfType - totalOfTypewithTargets) > 0)
                {
                    //get a random node
                    List<Node> tempNodes = new List<Node>(GameManager.instance.dataScript.GetListOfNodesByType(nodeArcID));
                    if (tempNodes.Count > 0)
                    {
                        Node node = tempNodes[Random.Range(0, tempNodes.Count)];
                        //assign targetID to node
                        node.TargetID = target.TargetID;
                        counter++;
                        Debug.Log(string.Format("TargetManager: Node ID {0}, type \"{1}\", assigned Target ID {2}, \"{3}\"{4}",
                            node.NodeID, node.arc.name, target.TargetID, target.name, "\n"));
                        //reset target status
                        Target dictTarget = GameManager.instance.dataScript.GetTarget(target.TargetID);
                        dictTarget.TargetStatus = status;
                        //target.TargetStatus = status;
                        //Remove from listOfPossibleTargets
                        listOfPossibleTargets.RemoveAt(index);
                        //Update node Array info stats
                        totalTargets = GameManager.instance.dataScript.GetNodeInfo(nodeArcID, NodeInfo.TargetsAll) + 1;
                        GameManager.instance.dataScript.SetNodeInfo(nodeArcID, NodeInfo.TargetsAll, totalTargets);
                        switch (status)
                        {
                            case Status.Active:
                                totalActive = GameManager.instance.dataScript.GetNodeInfo(nodeArcID, NodeInfo.TargetsActive) + 1;
                                GameManager.instance.dataScript.SetNodeInfo(nodeArcID, NodeInfo.TargetsActive, totalActive);
                                GameManager.instance.dataScript.AddActiveTarget(target);
                                break;
                            case Status.Live:
                                totalLive = GameManager.instance.dataScript.GetNodeInfo(nodeArcID, NodeInfo.TargetsLive) + 1;
                                GameManager.instance.dataScript.SetNodeInfo(nodeArcID, NodeInfo.TargetsLive, totalLive);
                                GameManager.instance.dataScript.AddLiveTarget(target);
                                //assign nodeID to target
                                target.NodeID = node.NodeID;
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid status \"{0}\"{1}", status, "\n"));
                                break;
                        }
                        successFlag = true;
                    }
                    else
                    {
                        endlessCounter++;
                        if (endlessCounter > 5)
                        {
                            successFlag = true;
                            Debug.LogWarning(string.Format("TargetManager: Breaking out of loop after 5 iterations, for type \"{0}\"", target.nodeArc.name));
                        }
                        Debug.LogError(string.Format("No nodes available of type {0}. Unable to assign target{1}", target.nodeArc.name, "\n"));
                    }
                }
                else { Debug.LogWarning(string.Format("TargetManager: Insufficient nodes of type \"{0}\" (zero or less)", target.nodeArc.name)); }
            }
        }
        //check tally
        if (counter == 0)
        { Debug.LogError("No nodes were assigned Targets"); }
        else if (counter < StartTargets)
        { Debug.LogWarning("TargetManager: Less than the required number of starting nodes assigned targets"); }
    }

    /// <summary>
    /// returns a list of formatted and coloured strings ready for a node Tooltip, returns an empty list if none
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public List<string> GetTargetTooltip(int targetID)
    {
        List<string> tempList = new List<string>();
        string infoColour = colourDataNeutral;
        //find target
        Target target = GameManager.instance.dataScript.GetTarget(targetID);
        if (target != null)
        {
            //target Live?
            if (target.TargetStatus == Status.Live)
            {
                //put tooltip together
                tempList.Add(string.Format("{0}{1}{2}", colourNormal, target.name, colourEnd));
                tempList.Add(string.Format("{0}{1}{2}", colourDefault, target.description, colourEnd));
                //good effects
                Effect effect = null;
                for(int i = 0; i < target.listOfGoodEffects.Count; i++)
                {
                    effect = target.listOfGoodEffects[i];
                    if (effect != null)
                    { tempList.Add(string.Format("{0}{1}{2}", colourGood, effect.description, colourEnd)); }
                    else { Debug.LogError(string.Format("Invalid effect (null) for \"{0}\", ID {1}{2}", target.name, target.TargetID, "\n")); }
                }
                //info level data colour graded
                if (target.InfoLevel == 3) { infoColour = colourDataGood; }
                else if (target.InfoLevel == 1) { infoColour = colourDataBad; }
                tempList.Add(string.Format("{0}Info level{1}  {2}{3}{4}", colourDefault, colourEnd, infoColour, target.InfoLevel, colourEnd));
                if (target.gear != null)
                { tempList.Add(string.Format("{0}{1}{2}", colourGear, target.gear.name, colourEnd)); }
                tempList.Add(string.Format("{0}{1}{2}", colourGood, target.actorArc.name, colourEnd));
            }
        }
        return tempList;
    }
    
}
