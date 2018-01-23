﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;


/// <summary>
/// handles all message matters
/// </summary>
public class MessageManager : MonoBehaviour
{

    /// <summary>
    /// Set up at start
    /// </summary>
    public void Initialise()
    {
        //event Listeners
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent);
        EventManager.instance.AddListener(EventType.EndTurn, OnEvent);
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
            case EventType.EndTurn:
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
            foreach(var record in dictOfPendingMessages)
            {
                //look for messages that need to be displayed this turn
                if (record.Value.displayDelay <= 0)
                {
                    //add to list
                    listOfMessagesToMove.Add(record.Key);
                }
                else
                {
                    //decrement delay timer
                    record.Value.displayDelay--;
                }
            }
            //loop list and move all specified messages to the Current dictionary for display
            for (int i = 0; i < listOfMessagesToMove.Count; i++)
            { GameManager.instance.dataScript.MoveMessage(listOfMessagesToMove[i], MessageCategory.Pending, MessageCategory.Current); }
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
            message.side = Side.Resistance;
            message.isPublic = false;
            message.data0 = nodeID;
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
    public Message AISpotMove(string text, int destinationNodeID, int connectionID, int delay)
    {
        Debug.Assert(destinationNodeID >= 0, string.Format("Invalid destinationNodeID {0}", destinationNodeID));
        Debug.Assert(connectionID >= 0, string.Format("Invalid connectionID {0}", connectionID));
        Debug.Assert(delay >= 0, string.Format("Invalid delay {0}", delay));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_SpotMove;
            message.side = Side.Authority;
            message.isPublic = true;
            message.displayDelay = delay;
            message.data0 = destinationNodeID;
            message.data1 = connectionID;
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
            message.side = Side.Authority;
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
            message.side = Side.Resistance;
            message.isPublic = true;
            message.data0 = nodeID;
            message.data1 = actorID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - Teams - - -
    //

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
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Deploy;
            message.side = Side.Authority;
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
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_AutoRecall;
            message.side = Side.Authority;
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
            message.side = Side.Authority;
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
            message.side = Side.Authority;
            message.isPublic = true;
            message.data0 = nodeID;
            message.data1 = teamID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Team in neutralised by the Resitance. Returns null if text invalid
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
            message.side = Side.Authority;
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
            message.side = Side.Resistance;
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
            message.side = Side.Resistance;
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
            message.side = Side.Resistance;
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
            message.side = Side.Resistance;
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
    public Message ActorRecruited(string text, int nodeID, int actorID, Side side)
    {
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (side == Side.Resistance)
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

    //new methods above here
}
