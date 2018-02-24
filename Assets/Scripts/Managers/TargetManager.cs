using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using gameAPI;
using System.Text;
using System;

public enum TargetFactors { TargetInfo, NodeSupport, ActorAndGear, NodeSecurity, TargetLevel, Teams} //Sequence is order of factor display

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
    [Range(0, 100)]
    [Tooltip("The base chance of a target attempt being successful with no other factors in play")]
    public int baseTargetChance = 50;
    [Range(1, 3)]
    [Tooltip("How much effect having the right Gear for a target will have on the chance of success")]
    public int gearEffect = 2;
    [Range(1, 3)]
    [Tooltip("How much effect having the right Actor for a target will have on the chance of success")]
    public int actorEffect = 2;
    [Range(1, 3)]
    [Tooltip("Maximum amount of target info that can be acquired on a specific target")]
    public int maxTargetInfo = 3;

    public int StartTargets { get; private set; }
    public int ActiveTargets { get; private set; }
    public int LiveTargets { get; private set; }
    public int MaxTargets { get; private set; }

    private List<TargetFactors> listOfFactors = new List<TargetFactors>();              //used to ensure target calculations are consistent across methods

    //colour Palette
    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourGear;
    private string colourNormal;
    private string colourDefault;
    private string colourGrey;
    private string colourDataGood;
    private string colourDataNeutral;
    private string colourDataBad;
    private string colourRebel;
    private string colourTarget;
    private string colourEnd;

    /// <summary>
    /// Initial setup
    /// </summary>
    public void Initialise()
    {
        //calculate limits
        int numOfNodes = GameManager.instance.dataScript.CheckNumOfNodes();
        StartTargets = numOfNodes * startPercentTargets / 100;
        MaxTargets = numOfNodes * maxPercentTargets / 100;
        ActiveTargets = MaxTargets - StartTargets;
        ActiveTargets = Mathf.Max(0, ActiveTargets);
        //Set initialise targets on map
        SetRandomTargets(StartTargets, Status.Live);
        //set up listOfTargetFactors. Note -> Sequence matters and is the order that the factors will be displayed
        foreach(var factor in Enum.GetValues(typeof(TargetFactors)))
        { listOfFactors.Add((TargetFactors)factor); }
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
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourGear = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourDataGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourDataNeutral = GameManager.instance.colourScript.GetColour(ColourType.dataNeutral);
        colourDataBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourRebel = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourTarget = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// Populates a set number of randomly chosen level 1 targets into the level with the indicated status
    /// </summary>
    /// <param name="numOfTargets"></param>
    /// <param name="status"></param>
    public void SetRandomTargets(int numOfTargetsInput, Status status = Status.Active)
    {
        int index, nodeArcID, totalOfType, totalOfTypewithTargets, totalActive, totalLive, totalTargets, endlessCounter, numOfTargets;
        int counter = 0;
        bool doneFlag;
        //dictionary to hold list of already assigned nodeID's to prevent duplicate assignments
        Dictionary<int, List<int>> dictOfExisting = new Dictionary<int, List<int>>();
        //if not enough viable targets drop number down to what's doable
        int numPossibleTargets = GameManager.instance.dataScript.CheckNumOfPossibleTargets();
        if (numOfTargetsInput > numPossibleTargets) { numOfTargets = numPossibleTargets; }
        else { numOfTargets = numOfTargetsInput; }
        //loop and populate
        List<Target> listOfPossibleTargets = GameManager.instance.dataScript.GetPossibleTargets();
        for(int i = 0; i < numOfTargets; i++)
        {
            //successflag breaks out of while loop if a suitable node is found
            doneFlag = false;
            endlessCounter = 0;
            //keep chosing a node until you find a suitable one (max 5 goes)
            while (doneFlag == false)
            {
                //get a random target
                index = Random.Range(0, listOfPossibleTargets.Count);
                Target target = listOfPossibleTargets[index];
                //get target nodeArcId
                nodeArcID = target.nodeArc.nodeArcID;
                //check that there is a suitable spare node available
                totalOfType = GameManager.instance.dataScript.CheckNodeInfo(nodeArcID, NodeInfo.Number);
                totalOfTypewithTargets = GameManager.instance.dataScript.CheckNodeInfo(nodeArcID, NodeInfo.TargetsAll);
                if ((totalOfType - totalOfTypewithTargets) > 0)
                {
                    //get a random node
                    List<Node> tempNodes = new List<Node>(GameManager.instance.dataScript.GetListOfNodesByType(nodeArcID));
                    if (tempNodes.Count > 0)
                    {
                        //remove all duplicate nodes from list that already have targets assigned to them
                        List<Node> prunedListOfNodes = PruneListOfNodes(nodeArcID, dictOfExisting, tempNodes);
                        if (prunedListOfNodes != null)
                        {
                            if (prunedListOfNodes.Count > 0)
                            {
                                Node node = prunedListOfNodes[Random.Range(0, prunedListOfNodes.Count)];
                                //assign targetID to node
                                node.targetID = target.targetID;
                                counter++;
                                Debug.Log(string.Format("TargetManager: Node ID {0}, type \"{1}\", assigned Target ID {2}, \"{3}\"{4}",
                                    node.nodeID, node.Arc.name, target.targetID, target.name, "\n"));
                                //reset target status
                                Target dictTarget = GameManager.instance.dataScript.GetTarget(target.targetID);
                                dictTarget.targetStatus = status;
                                //Remove from listOfPossibleTargets
                                listOfPossibleTargets.RemoveAt(index);

                                //add to dictionary (used to prevent targets being assigned to duplicate nodes)
                                List<int> listOfNodeID = new List<int>();
                                if (dictOfExisting.ContainsKey(nodeArcID) == true)
                                {
                                    listOfNodeID = dictOfExisting[nodeArcID];
                                    //add new entry
                                    listOfNodeID.Add(node.nodeID);
                                }
                                else
                                {
                                    //new entry in dictionary
                                    listOfNodeID.Add(node.nodeID);
                                    try
                                    { dictOfExisting.Add(nodeArcID, listOfNodeID); }
                                    catch (ArgumentException)
                                    { Debug.LogError(string.Format("Invalid nodeArcID {0} (duplicate entry) in dictOfExisting", nodeArcID)); }
                                }

                                //Update node Array info stats
                                totalTargets = GameManager.instance.dataScript.CheckNodeInfo(nodeArcID, NodeInfo.TargetsAll) + 1;
                                GameManager.instance.dataScript.SetNodeInfo(nodeArcID, NodeInfo.TargetsAll, totalTargets);
                                switch (status)
                                {
                                    case Status.Active:
                                        totalActive = GameManager.instance.dataScript.CheckNodeInfo(nodeArcID, NodeInfo.TargetsActive) + 1;
                                        GameManager.instance.dataScript.SetNodeInfo(nodeArcID, NodeInfo.TargetsActive, totalActive);
                                        GameManager.instance.dataScript.AddTargetToPool(target, Status.Active);
                                        break;
                                    case Status.Live:
                                        totalLive = GameManager.instance.dataScript.CheckNodeInfo(nodeArcID, NodeInfo.TargetsLive) + 1;
                                        GameManager.instance.dataScript.SetNodeInfo(nodeArcID, NodeInfo.TargetsLive, totalLive);
                                        GameManager.instance.dataScript.AddTargetToPool(target, Status.Live);
                                        //assign nodeID to target
                                        target.nodeID = node.nodeID;
                                        break;
                                    default:
                                        Debug.LogError(string.Format("Invalid status \"{0}\"{1}", status, "\n"));
                                        break;
                                }
                                doneFlag = true;
                            }
                            else
                            {
                                Debug.LogWarning(string.Format("No available nodes left after pruning for nodeArcID {0}{1}", nodeArcID, "\n"));
                                doneFlag = true;
                            }
                        }
                        else
                        {
                            endlessCounter++;
                            if (endlessCounter > 5)
                            {
                                Debug.LogWarning(string.Format("TargetManager: Breaking out of loop after 5 iterations, for type \"{0}\"{1}", target.nodeArc.name, "\n"));
                                doneFlag = true;
                            }
                            Debug.LogError(string.Format("No nodes available of type {0}. Unable to assign target{1}", target.nodeArc.name, "\n"));
                        }
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("TargetManager: Insufficient nodes of type \"{0}\" (zero or less){1}", target.nodeArc.name, "\n"));
                        doneFlag = true;
                    }
                }
                else
                {
                    Debug.LogWarning(string.Format("TargetManager: There are no \"{0}\" nodes without targets remaining{1}", target.nodeArc.name,"\n"));
                    doneFlag = true;
                }
            }
        }
        //check tally
        if (counter == 0)
        { Debug.LogError(string.Format("No nodes were assigned Targets{0}", "\n")); }
        else if (counter < StartTargets)
        { Debug.LogWarning(string.Format("TargetManager: Less than the required number of starting nodes assigned targets{0}", "\n")); }
    }


    /// <summary>
    /// private sub method to take list of Nodes and remove any that match entries in the dictOfExisting to prevent SetRandomTargets assigning targets to duplicate nodes
    /// </summary>
    /// <param name="dictOfExisting"></param>
    /// <param name="tempList"></param>
    /// <returns></returns>
    private List<Node> PruneListOfNodes(int nodeArcID, Dictionary<int, List<int>> dictOfExisting, List<Node> tempList)
    {
        List<Node> listOfNodes = null;
        if (tempList != null && dictOfExisting != null)
        {
            listOfNodes = new List<Node>(tempList);
            
            //lookup dictionary to see if any entries for identical nodeArcID
            if (dictOfExisting.ContainsKey(nodeArcID))
            {
                List<int> listOfExistingNodeID = new List<int>(dictOfExisting[nodeArcID]);
                //reverse loop parameter node List and delete any that match entries in listOfNodeIDs
                if (listOfExistingNodeID != null && listOfExistingNodeID.Count > 0)
                {
                    int nodeID;
                    bool isMatch;
                    for(int i = listOfNodes.Count - 1; i >= 0; i--)
                    {
                        isMatch = false;
                        nodeID = listOfNodes[i].nodeID;
                        //check for a match with existing
                        for(int j = 0; j < listOfExistingNodeID.Count; j++)
                        {
                            if (nodeID == listOfExistingNodeID[j])
                            {
                                isMatch = true;
                                break;
                            }
                        }
                        if (isMatch == true)
                        {
                            //delete entry
                            listOfNodes.RemoveAt(i);
                        }
                    }
                }
            }
            return listOfNodes;
        }
        else { Debug.LogError("Invalid dictOfExisting or tempList (either are Null)"); }
        return listOfNodes;
    }


    /// <summary>
    /// returns a list of formatted and coloured strings ready for a node Tooltip (Side.Resistance), returns an empty list if none
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public List<string> GetTargetTooltip(int targetID)
    {
        List<string> tempList = new List<string>();
        if (targetID > -1)
        {
            //find target
            Target target = GameManager.instance.dataScript.GetTarget(targetID);
            if (target != null)
            {
                //target LIVE
                if (target.targetStatus == Status.Live)
                {
                    //put tooltip together
                    tempList.Add(string.Format("{0}{1}{2}", colourTarget, target.name, colourEnd));
                    tempList.Add(string.Format("{0}{1}{2}", colourDefault, target.description, colourEnd));
                    //good effects
                    Effect effect = null;
                    for (int i = 0; i < target.listOfGoodEffects.Count; i++)
                    {
                        effect = target.listOfGoodEffects[i];
                        if (effect != null)
                        { tempList.Add(string.Format("{0}{1}{2}", colourGood, effect.textTag, colourEnd)); }
                        else { Debug.LogError(string.Format("Invalid effect (null) for \"{0}\", ID {1}{2}", target.name, target.targetID, "\n")); }
                    }
                    //bad effects
                    for (int i = 0; i < target.listOfBadEffects.Count; i++)
                    {
                        effect = target.listOfBadEffects[i];
                        if (effect != null)
                        { tempList.Add(string.Format("{0}{1}{2}", colourBad, effect.textTag, colourEnd)); }
                        else { Debug.LogError(string.Format("Invalid effect (null) for \"{0}\", ID {1}{2}", target.name, target.targetID, "\n")); }
                    }
                    //ongoing effects
                    for (int i = 0; i < target.listOfOngoingEffects.Count; i++)
                    {
                        effect = target.listOfOngoingEffects[i];
                        if (effect != null)
                        { tempList.Add(string.Format("{0}{1}{2}", colourGood, effect.textTag, colourEnd)); }
                        else { Debug.LogError(string.Format("Invalid effect (null) for \"{0}\", ID {1}{2}", target.name, target.targetID, "\n")); }
                    }

                    //info level data colour graded

                    /*if (target.InfoLevel == 3) { infoColour = colourDataGood; }
                    else if (target.InfoLevel == 1) { infoColour = colourDataBad; }*/

                    tempList.Add(string.Format("{0}Info level{1}  {2}{3}{4}", colourDefault, colourEnd, 
                        GameManager.instance.colourScript.GetValueColour(target.infoLevel), target.infoLevel, colourEnd));
                    tempList.Add(string.Format("{0}{1} gear{2}", colourGear, target.gear.name, colourEnd));
                    tempList.Add(string.Format("{0}{1}{2}", colourGear, target.actorArc.name, colourEnd));
                }
                //target COMPLETED
                else if (target.targetStatus == Status.Completed)
                {
                    //put tooltip together
                    tempList.Add(string.Format("{0}Target \"{1}\" has been Completed{2}", colourTarget, target.name, colourEnd));
                    //ongoing effects
                    if (target.listOfOngoingEffects.Count > 0)
                    {
                        tempList.Add(string.Format("{0}Ongoing effects until contained{1}", colourDefault, colourEnd));
                        Effect effect = null;
                        for (int i = 0; i < target.listOfOngoingEffects.Count; i++)
                        {
                            effect = target.listOfOngoingEffects[i];
                            if (effect != null)
                            { tempList.Add(string.Format("{0}{1}{2}", colourGood, effect.textTag, colourEnd)); }
                            else { Debug.LogError(string.Format("Invalid effect (null) for \"{0}\", ID {1}{2}", target.name, target.targetID, "\n")); }
                        }
                    }
                }
            }
            else
            { Debug.LogError(string.Format("Invalid Target (null) for ID {0}{1}", targetID, "\n")); }
        }
        return tempList;
    }

    /// <summary>
    /// returns formatted string of all good and bad target effects. Returns null if none.
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public string GetTargetEffects(int targetID)
    {
        List<string> tempList = new List<string>();
        //find target
        Target target = GameManager.instance.dataScript.GetTarget(targetID);
        if (target != null)
        {
            //good effects
            Effect effect = null;
            tempList.Add(string.Format("{0}Target Success Effects{1}", colourTarget, colourEnd));
            if (target.listOfGoodEffects.Count > 0)
            {
                //add header
                for (int i = 0; i < target.listOfGoodEffects.Count; i++)
                {
                    effect = target.listOfGoodEffects[i];
                    if (effect != null)
                    { tempList.Add(string.Format("{0}{1}{2}", colourGood, effect.textTag, colourEnd)); }
                    else { Debug.LogError(string.Format("Invalid Good effect (null) for \"{0}\", ID {1}{2}", target.name, target.targetID, "\n")); }
                }
            }
            //bad effects
            if (target.listOfBadEffects.Count > 0)
            {
                //add header
                for (int i = 0; i < target.listOfBadEffects.Count; i++)
                {
                    effect = target.listOfBadEffects[i];
                    if (effect != null)
                    { tempList.Add(string.Format("{0}{1}{2}", colourBad, effect.textTag, colourEnd)); }
                    else { Debug.LogError(string.Format("Invalid Bad effect (null) for \"{0}\", ID {1}{2}", target.name, target.targetID, "\n")); }
                }
            }
            //Ongoing effects
            if (target.listOfOngoingEffects.Count > 0)
            {
                //add header
                for (int i = 0; i < target.listOfOngoingEffects.Count; i++)
                {
                    effect = target.listOfOngoingEffects[i];
                    if (effect != null)
                    { tempList.Add(string.Format("{0}{1} (Ongoing){2}", colourGood, effect.textTag, colourEnd)); }
                    else { Debug.LogError(string.Format("Invalid Ongoing effect (null) for \"{0}\", ID {1}{2}", target.name, target.targetID, "\n")); }
                }
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Target (null) for ID {0}{1}", targetID, "\n"));
            return null;
        }
        //convert to a string
        StringBuilder builder = new StringBuilder();
        foreach(string text in tempList)
        {
            if (builder.Length > 0)
            { builder.AppendLine(); }
            builder.Append(text);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Returns all factors involved in a particular targets resolution (eg. effects chance of success). 
    /// Used by ActorManager.cs -> GetNodeActions for action button tooltip
    /// NOTE: Tweak listOfFactors in Initialise() if you want to change any factors in the calculations
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public string GetTargetFactors(int targetID)
    {
        List<string> tempList = new List<string>();
        //get target
        Target target = GameManager.instance.dataScript.GetTarget(targetID);
        if (target != null)
        {
            //get node
            Node node = GameManager.instance.dataScript.GetNode(target.nodeID);
            if (node != null)
            {
                //
                // - - - Factors affecting Resolution - - -
                //
                tempList.Add(string.Format("{0}Target Success Chance{1}", colourTarget, colourEnd));
                //Loop listOfFactors to ensure consistency of calculations across methods
                foreach (TargetFactors factor in listOfFactors)
                {
                    switch (factor)
                    {
                        case TargetFactors.TargetInfo:
                            //good -> info
                            tempList.Add(string.Format("{0}Info {1}{2}{3}", colourGood, target.infoLevel > 0 ? "+" : "", target.infoLevel, colourEnd));
                            break;
                        case TargetFactors.NodeSupport:
                            //good -> support
                            if (node.Support > 0)
                            { tempList.Add(string.Format("{0}Support +{1}{2}", colourGood, node.Support, colourEnd)); }
                            break;
                        case TargetFactors.ActorAndGear:
                            //player or Active Actor?
                            if (GameManager.instance.nodeScript.nodePlayer == node.nodeID)
                            {
                                //Player at node -> active actor not applicable
                                if (target.actorArc != null)
                                { tempList.Add(string.Format("{0}{1}{2}", colourGrey, target.actorArc.name, colourEnd)); }
                                //player has special gear?
                                if (target.gear != null)
                                {
                                    int gearID = GameManager.instance.playerScript.CheckGearTypePresent(target.gear);
                                    if (gearID > -1)
                                    {
                                        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                                        if (gear != null)
                                        { tempList.Add(string.Format("{0}{1} +{2}{3}", colourGood, gear.name, gearEffect * (gear.rarity.level + 1), colourEnd)); }
                                        else { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", gearID)); }
                                    }
                                    else
                                    { tempList.Add(string.Format("{0}{1} gear{2}", colourGrey, target.gear.name, colourEnd)); }
                                }
                            }
                            else
                            {
                                //player not at node ->  check if node active for the correct actor
                                if (target.actorArc != null)
                                {
                                    //check if actor present in team
                                    int slotID = GameManager.instance.dataScript.CheckActorPresent(target.actorArc.ActorArcID, GameManager.instance.globalScript.sideResistance);
                                    if (slotID > -1)
                                    {
                                        //actor present and available
                                        tempList.Add(string.Format("{0}{1} +{2}{3}", colourGood, target.actorArc.name, actorEffect, colourEnd));
                                    }
                                    else
                                    {
                                        //actor either not present or unavailable
                                        tempList.Add(string.Format("{0}{1}{2}", colourGrey, target.actorArc.name, colourEnd));
                                    }
                                }
                                //gear not applicable (only when player at node)
                                if (target.gear != null)
                                { tempList.Add(string.Format("{0}{1} gear{2}", colourGrey, target.gear.name, colourEnd)); }
                            }
                            break;
                        case TargetFactors.NodeSecurity:
                            //bad -> security
                            tempList.Add(string.Format("{0}Node Security {1}{2}{3}", colourBad, node.Security > 0 ? "-" : "", node.Security, colourEnd));
                            break;
                        case TargetFactors.TargetLevel:
                            //bad -> target level
                            tempList.Add(string.Format("{0}Target Level {1}{2}{3}", colourBad, target.targetLevel > 0 ? "-" : "", target.targetLevel, colourEnd));
                            break;
                        case TargetFactors.Teams:
                            if (node.CheckTeamPresent(GameManager.instance.dataScript.GetTeamArcID("Control")) > -1)
                            { tempList.Add(string.Format("{0}Control Team -{1}{2}", colourBad, GameManager.instance.teamScript.securityTeamEffect, colourEnd)); }
                            break;
                        default:
                            Debug.LogError(string.Format("Unknown TargetFactor \"{0}\"{1}", factor, "\n"));
                            break;
                    }
                }
                //
                // - - - Total - - -
                //
                int tally = GetTargetTally(targetID);
                int chance = GetTargetChance(tally);
                //add tally and chance to string
                tempList.Add(string.Format("{0}Total {1}{2} (base {3} out of 10) {4}", colourRebel, tally > 0 ? "+" : "", tally, baseTargetChance/10, colourEnd));
                tempList.Add(string.Format("{0}{1}SUCCESS {2}%{3}{4}", colourDefault, "<mark=#FFFFFF4D>", chance, "</mark>", colourEnd));
            }
            else
            {
                Debug.LogError(string.Format("Invalid node (null), ID \"{0}\"{1}",target.nodeID, "\n"));
                tempList.Add(string.Format("{0}{1}{2}", colourBad, "Target Data inaccessible", colourEnd));
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Target (null), ID \"{0}\"{1}", targetID, "\n"));
            tempList.Add(string.Format("{0}{1}{2}", colourBad, "Target Data inaccessible", colourEnd));
        }
        //convert list to string and return
        StringBuilder builder = new StringBuilder();
        foreach(string text in tempList)
        {
            if (builder.Length > 0)
            { builder.AppendLine(); }
            builder.Append(text);
        }
        return builder.ToString();
    }

    /// <summary>
    /// returns tally of all factors in target success, eg. -2, +1, etc
    /// NOTE: Tweak listOfFactors in Initialise() if you want to change any factors in the calculations
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public int GetTargetTally(int targetID)
    {
        int tally = 0;
        //get target
        Target target = GameManager.instance.dataScript.GetTarget(targetID);
        if (target != null)
        {
            //get node
            Node node = GameManager.instance.dataScript.GetNode(target.nodeID);
            if (node != null)
            {
            //Loop listOfFactors to ensure consistency of calculations across methods
            foreach(TargetFactors factor in listOfFactors)
                {
                    switch(factor)
                    {
                        case TargetFactors.TargetInfo:
                            //good -> info
                            tally += target.infoLevel;
                            break;
                        case TargetFactors.NodeSupport:
                            //good -> support
                            if (node.Support > 0)
                            { tally += node.Support; }
                            break;
                        case TargetFactors.ActorAndGear:
                            //player or Active Actor?
                            if (GameManager.instance.nodeScript.nodePlayer == node.nodeID)
                            {
                                //player has special gear?
                                if (target.gear != null)
                                {
                                    int gearID = GameManager.instance.playerScript.CheckGearTypePresent(target.gear);
                                    if (gearID > -1)
                                    {
                                        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                                        if (gear != null)
                                        { tally += gearEffect * (gear.rarity.level + 1); }
                                        else { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", gearID)); }
                                    }
                                }
                            }
                            else
                            {
                                //player NOT at node ->  check if actor is present in OnMap line-up
                                if (target.actorArc != null)
                                {
                                    int slotID = GameManager.instance.dataScript.CheckActorPresent(target.actorArc.ActorArcID, GameManager.instance.globalScript.sideResistance);
                                    if (slotID > -1)
                                    {
                                        //actor present and available
                                        tally += actorEffect;
                                    }
                                }
                            }
                            break;
                        case TargetFactors.NodeSecurity:
                            //bad -> security
                            tally -= node.Security;
                            break;
                        case TargetFactors.TargetLevel:
                            //bad -> target level
                            tally -= target.targetLevel;
                            break;
                        case TargetFactors.Teams:
                            //Teams
                            if (node.CheckTeamPresent(GameManager.instance.dataScript.GetTeamArcID("Control")) > -1)
                            { tally -= GameManager.instance.teamScript.securityTeamEffect; }
                            break;
                        default:
                            Debug.LogError(string.Format("Unknown TargetFactor \"{0}\"{1}", factor, "\n"));
                            break;
                    }
                }
            }
            else
            { Debug.LogError(string.Format("Invalid node (null), ID \"{0}\"{1}", target.nodeID, "\n")); }
        }
        else
        { Debug.LogError(string.Format("Invalid Target (null), ID \"{0}\"{1}", targetID, "\n")); }
        return tally;
    }

    /// <summary>
    /// returns % chance (whole numbers) of target resolution being a success
    /// Formula -> baseTargetChance + tally * 10
    /// </summary>
    /// <param name="tally"></param>
    /// <returns></returns>
    public int GetTargetChance(int tally)
    {
        int chance = baseTargetChance + (tally * 10);
        chance = Mathf.Clamp(chance, 0, 100);
        return chance;
    }


    /// <summary>
    /// Contains a completed target as a result of Damage team intervention. Handles all related matters.
    /// Note: target is checked for Null by the calling method
    /// </summary>
    /// <param name="target"></param>
    public void ContainTarget(Target target)
    {
        Node node = GameManager.instance.dataScript.GetNode(target.nodeID);
        if (node != null)
        {
            GameManager.instance.connScript.RemoveOngoingEffect(target.ongoingID);
            GameManager.instance.nodeScript.RemoveOngoingEffect(target.ongoingID);
            //admin
            target.targetStatus = Status.Contained;
            GameManager.instance.dataScript.AddTargetToPool(target, Status.Contained);
            node.targetID = -1;
        }
        else { Debug.LogError(string.Format("Invalid node (Null) for target.nodeID {0}", target.nodeID)); }
    }


    //place methods above here
}
