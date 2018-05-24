using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;


/// <summary>
/// handles all message matters
/// </summary>
public class MessageManager : MonoBehaviour
{

    //fast access
    GlobalSide globalResistance;
    GlobalSide globalAuthority;
    GlobalSide globalBoth;

    /// <summary>
    /// Set up at start
    /// </summary>
    public void Initialise()
    {
        globalResistance = GameManager.instance.globalScript.sideResistance;
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalBoth = GameManager.instance.globalScript.sideBoth;
        //event Listeners
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent, "MessageManager");
        EventManager.instance.AddListener(EventType.EndTurnFinal, OnEvent, "MessageManager");
    }


    /// <summary>
    /// handles events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.StartTurnEarly:
                StartTurnEarly();
                break;
            case EventType.EndTurnFinal:
                EndTurn();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Checks pending messages, decrements delay timers and moves any with a zero timer to Current messages.
    /// </summary>
    private void StartTurnEarly()
    {
        Dictionary<int, Message> dictOfPendingMessages = new Dictionary<int, Message>(GameManager.instance.dataScript.GetMessageDict(MessageCategory.Pending));
        if (dictOfPendingMessages != null)
        {
            List<int> listOfMessagesToMove = new List<int>();
            //loop through all messages
            foreach(var message in dictOfPendingMessages)
            {
                //look for messages that need to be displayed this turn
                if (message.Value.displayDelay <= 0)
                {
                    //add to list
                    listOfMessagesToMove.Add(message.Key);
                    //AI message
                    if (message.Value.type == gameAPI.MessageType.AI)
                    { GameManager.instance.dataScript.AIMessage(message.Value); }
                }
                else
                {
                    //decrement delay timer
                    message.Value.displayDelay--;
                }
            }
            //loop list and move all specified messages to the Current dictionary for display
            for (int i = 0; i < listOfMessagesToMove.Count; i++)
            {
                GameManager.instance.dataScript.MoveMessage(listOfMessagesToMove[i], MessageCategory.Pending, MessageCategory.Current);
            }
        }
        else { Debug.LogError("Invalid dictOfPendingMessages (Null)"); }
    }

    /// <summary>
    /// checks current messages, moves all of them to Archive messages
    /// </summary>
    private void EndTurn()
    {
        Dictionary<int, Message> dictOfCurrentMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.Current);
        if (dictOfCurrentMessages != null)
        {
            List<int> listOfMessagesToMove = new List<int>();
            foreach(var record in dictOfCurrentMessages)
            { listOfMessagesToMove.Add(record.Key); }
            //loop list and move all specified messages to the Archive dictionary for display
            for (int i = 0; i < listOfMessagesToMove.Count; i++)
            { GameManager.instance.dataScript.MoveMessage(listOfMessagesToMove[i], MessageCategory.Current, MessageCategory.Archive); }
            //error check
            if (dictOfCurrentMessages.Count > 0)
            { Debug.LogError(string.Format("There are {0} records in dictOfCurrentMethods (should be empty)", dictOfCurrentMessages.Count)); }
        }
        else { Debug.LogError("Invalid dictOfCurrentMessages (Null)"); }
    }


//
// - - - New Message Methods
//

//
// - - - Player - - -
//

/// <summary>
/// Message -> player movement from one node to another. Returns null if text invalid.
/// </summary>
/// <param name="text"></param>
/// <param name="nodeID"></param>
/// <returns></returns>
public Message PlayerMove(string text, int nodeID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Move;
            message.side = globalResistance;
            message.isPublic = false;
            message.data0 = nodeID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - Actions - - -
    //

    /// <summary>
    /// Actor status changes, eg. Active -> Lie Low, Lie Low -> Active as a result of an Actor action
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="isPublic"></param>
    /// <returns></returns>
    public Message ActorStatus(string text, int actorID, GlobalSide side, bool isPublic = false)
    {
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Actor_Status;
            message.side = side;
            message.isPublic = isPublic;
            message.data0 = actorID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Actor in Reserve Pool is reassured or threatened
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="isPublic"></param>
    /// <returns></returns>
    public Message ActorSpokenToo(string text, int actorID, GlobalSide side, bool isPublic = false)
    {
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Reassured;
            message.side = side;
            message.isPublic = isPublic;
            message.data0 = actorID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Actor condition gained or removed, eg. 'Stressed'
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="side"></param>
    /// <param name="isPublic"></param>
    /// <returns></returns>
    public Message ActorCondition(string text, int actorID, GlobalSide side, bool isPublic = false)
    {
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Condition;
            message.side = side;
            message.isPublic = isPublic;
            message.data0 = actorID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Player gifts an actor with gear and may receive some renown in return if it's the actor's preferred gear type
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="gearID"></param>
    /// <param name="renown"></param>
    /// <returns></returns>
    public Message GiveGear(string text, int actorID, int gearID, int renown)
    {
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        Debug.Assert(gearID >= 0, string.Format("Invalid gearID {0}", gearID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Gear_Given;
            message.side = globalResistance;
            message.isPublic = false;
            message.data0 = actorID;
            message.data1 = gearID;
            message.data2 = renown;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - AI - - -
    //

    /// <summary>
    /// AI notification of a Player move if they were spotted, returns Null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="destinationNode"></param>
    /// <param name="connectionID"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public Message AIConnectionActivity(string text, int destinationNodeID, int connectionID, int delay)
    {
        Debug.Assert(destinationNodeID >= 0, string.Format("Invalid destinationNodeID {0}", destinationNodeID));
        Debug.Assert(connectionID >= 0, string.Format("Invalid connectionID {0}", connectionID));
        Debug.Assert(delay >= 0, string.Format("Invalid delay {0}", delay));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Connection;
            message.side = globalAuthority;
            message.isPublic = true;
            message.displayDelay = delay;
            message.data0 = destinationNodeID;
            message.data1 = connectionID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    public Message AINodeActivity(string text, int nodeID, int actorID, int delay)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid NodeID {0}", nodeID));
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        Debug.Assert(delay >= 0, string.Format("Invalid delay {0}", delay));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Node;
            message.side = globalAuthority;
            message.isPublic = true;
            message.displayDelay = delay;
            message.data0 = nodeID;
            message.data1 = actorID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// AI notification of Player/Actor activity that results in immediateAuthorityFlag/immediateResistanceFlag being set true
    /// use either nodeID or connID and set the other to -1
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID">Set to -1 if not applicable</param>
    /// <param name="connID">Set to -1 if not applicable</param>
    /// <param name="side"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message AIImmediateActivity(string text, GlobalSide side, int nodeID, int connID, int actorID = 999)
    {
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Immediate;
            message.side = side;
            message.isPublic = false;
            message.data0 = nodeID;
            message.data1 = connID;
            message.data2 = actorID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// AI notification of Player (use default actorID 999) or actor being captured at a node
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="teamID"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message AICapture(string text, int nodeID, int teamID, int actorID = 999)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid NodeID {0}", nodeID));
        Debug.Assert(teamID >= 0, string.Format("Invalid teamID {0}", teamID));
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Capture;
            message.side = globalAuthority;
            message.isPublic = true;
            message.data0 = nodeID;
            message.data1 = teamID;
            message.data2 = actorID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// AI notification of Player or Actor (Resistance) being released from capture
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message AIRelease(string text, int nodeID, int actorID = 999)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid NodeID {0}", nodeID));
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Capture;
            message.side = globalResistance;
            message.isPublic = true;
            message.data0 = nodeID;
            message.data1 = actorID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - Decisions - - -
    //

    /// <summary>
    /// Either side (Player/AI) makes a global (city wide effect) decision that may, or may not be, public knowledge. Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="decID"></param>
    /// <param name="side"></param>
    /// <param name="isPublic"></param>
    /// <returns></returns>
    public Message DecisionGlobal(string text, int decID, GlobalSide side, bool isPublic = false)
    {
        Debug.Assert(decID >= 0, string.Format("Invalid decID {0}", decID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.DECISION;
            message.subType = MessageSubType.Decision_Global;
            message.side = side;
            message.isPublic = isPublic;
            message.data0 = decID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Either side (Player/AI) changes security level on a connection. Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="connID"></param>
    /// <param name="secLevel"></param>
    /// <returns></returns>
    public Message DecisionConnection(string text, int connID, int secLevel)
    {
        Debug.Assert(connID >= 0, string.Format("Invalid connID {0}", connID));
        Debug.Assert(secLevel >= 0, string.Format("Invalid secLevel {0}", secLevel));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.DECISION;
            message.subType = MessageSubType.Decision_Connection;
            message.side = globalBoth;
            message.isPublic = true;
            message.data0 = connID;
            message.data1 = secLevel;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Any side requests additional resources. Approved or denied. Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="side"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public Message DecisionRequestResources(string text, GlobalSide side, int amount)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(amount >= 0, string.Format("Invalid amount {0}", amount));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.DECISION;
            message.subType = MessageSubType.Decision_Resources;
            message.side = side;
            message.isPublic = true;
            message.data0 = amount;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Authority requests an additional team. Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="teamID"></param>
    /// <returns></returns>
    public Message DecisionRequestTeam(string text, int teamID)
    {
        Debug.Assert(teamID >= 0, string.Format("Invalid teamID {0}", teamID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.DECISION;
            message.subType = MessageSubType.Decision_Team;
            message.side = globalAuthority;
            message.isPublic = true;
            message.data0 = teamID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - Teams - - -
    //

    /// <summary>
    /// Authority adds a new team to the reserve pool
    /// </summary>
    /// <param name="text"></param>
    /// <param name="teamID"></param>
    /// <returns></returns>
    public Message TeamAdd(string text, int teamID, bool isPublic = false)
    {
        Debug.Assert(teamID >= 0, string.Format("Invalid teamID {0}", teamID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Add;
            message.side = globalAuthority;
            message.isPublic = false;
            message.data0 = teamID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Authority deploys a team to a node. Returns null if text invalid.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="teamID"></param>
    /// <returns></returns>
    public Message TeamDeploy(string text, int nodeID, int teamID, int actorID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(teamID >= 0, string.Format("Invalid teamID {0}", teamID));
        if (GameManager.instance.sideScript.authorityOverall == SideState.Player)
        { Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID)); }
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Deploy;
            message.side = globalAuthority;
            message.isPublic = false;
            message.data0 = nodeID;
            message.data1 = teamID;
            message.data2 = actorID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Team is autorecalled from OnMap when their timer expires. Returns null if text invalid.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="teamID"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message TeamAutoRecall(string text, int nodeID, int teamID, int actorID)
    { 
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(teamID >= 0, string.Format("Invalid teamID {0}", teamID));
        if (GameManager.instance.sideScript.authorityOverall == SideState.Player)
        { Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID)); }
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_AutoRecall;
            message.side = globalAuthority;
            message.isPublic = true;
            message.displayDelay = 0;
            message.data0 = nodeID;
            message.data1 = teamID;
            message.data2 = actorID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Team is withdrawn early by player. Returns null if text invalid.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="teamID"></param>
    /// <returns></returns>
    public Message TeamWithdraw(string text, int nodeID, int teamID, int actorID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(teamID >= 0, string.Format("Invalid teamID {0}", teamID));
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Withdraw;
            message.side = globalAuthority;
            message.isPublic = false;
            message.data0 = nodeID;
            message.data1 = teamID;
            message.data2 = actorID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Team carries out it's permanent effect when auto withdrawn from node
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="teamID"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message TeamEffect(string text, int nodeID, int teamID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(teamID >= 0, string.Format("Invalid teamID {0}", teamID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Effect;
            message.side = globalAuthority;
            message.isPublic = true;
            message.data0 = nodeID;
            message.data1 = teamID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Team in neutralised by the Resistance. Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="teamID"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message TeamNeutralise(string text, int nodeID, int teamID, int actorID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(teamID >= 0, string.Format("Invalid teamID {0}", teamID));
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Neutralise;
            message.side = globalAuthority;
            message.isPublic = true;
            message.displayDelay = 0;
            message.data0 = nodeID;
            message.data1 = teamID;
            message.data2 = actorID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    /// <summary>
    /// Gear has been used and compromised. Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public Message GearCompromised(string text, int nodeID, int gearID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(gearID >= 0, string.Format("Invalid gearID {0}", gearID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Comprised;
            message.side = globalResistance;
            message.isPublic = false;
            message.data0 = nodeID;
            message.data1 = gearID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Gear has been used (not compromised). Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public Message GearUsed(string text, int nodeID, int gearID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(gearID >= 0, string.Format("Invalid gearID {0}", gearID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Used;
            message.side = globalResistance;
            message.isPublic = false;
            message.data0 = nodeID;
            message.data1 = gearID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Gear obtained. Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public Message GearObtained(string text, int nodeID, int gearID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(gearID >= 0, string.Format("Invalid gearID {0}", gearID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Obtained;
            message.side = globalResistance;
            message.isPublic = false;
            message.data0 = nodeID;
            message.data1 = gearID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Player renown expended. 'dataID' refers to GearID if gear compromised.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="dataID"></param>
    /// <returns></returns>
    public Message RenownUsedPlayer(string text, int nodeID, int dataID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(dataID >= 0, string.Format("Invalid dataID {0}", dataID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Renown;
            message.side = globalResistance;
            message.isPublic = false;
            message.data0 = nodeID;
            message.data1 = dataID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// actor has been recruited (can be used for both sides). Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="actorID"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public Message ActorRecruited(string text, int nodeID, int actorID, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (side.level == globalResistance.level)
        { Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID)); }
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Recruited;
            message.side = side;
            message.isPublic = false;
            message.data0 = nodeID;
            message.data1 = actorID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - Targets - - -
    //

    /// <summary>
    /// Target has been attempted successfully, or not, by an actor ('999' for player), at a node. Returns null if text invalid.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="actorID"></param>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public Message TargetAttempt(string text, int nodeID, int actorID, int targetID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        Debug.Assert(targetID >= 0, string.Format("Invalid targetID {0}", nodeID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TARGET;
            message.subType = MessageSubType.Target_Attempt;
            message.side = globalResistance;
            message.isPublic = false;
            message.data0 = nodeID;
            message.data1 = actorID;
            message.data2 = targetID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Target has been contained (completed -> contained) at a node, by a team. Returns null if text invalid.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="actorID"></param>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public Message TargetContained(string text, int nodeID, int teamID, int targetID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(teamID >= 0, string.Format("Invalid teamID {0}", teamID));
        Debug.Assert(targetID >= 0, string.Format("Invalid targetID {0}", nodeID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TARGET;
            message.subType = MessageSubType.Target_Contained;
            message.side = globalBoth;
            message.isPublic = false;
            message.data0 = nodeID;
            message.data1 = teamID;
            message.data2 = targetID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    //
    // - - - Effects - - -
    //

    /// <summary>
    /// Ongoing effect that has been created (node or connection)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="side"></param>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public Message OngoingEffectCreated(string text, int nodeID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID/connID {0}", nodeID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.EFFECT;
            message.subType = MessageSubType.Ongoing_Created;
            message.side = globalBoth;
            message.isPublic = false;
            message.data0 = nodeID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Ongoing effect that timed out (expired) automatically or has been shut down
    /// </summary>
    /// <param name="text"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public Message OngoingEffectExpired(string text, int nodeID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID/connID {0}", nodeID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.EFFECT;
            message.subType = MessageSubType.Ongoing_Expired;
            message.side = globalBoth;
            message.isPublic = true;
            message.data0 = nodeID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //new methods above here
}
