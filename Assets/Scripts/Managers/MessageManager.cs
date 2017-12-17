using System.Collections;
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
        Dictionary<int, Message> dictOfCurrentMessages = new Dictionary<int, Message>(GameManager.instance.dataScript.GetMessageDict(MessageCategory.Current));
        if (dictOfCurrentMessages != null)
        {
            int limit = dictOfCurrentMessages.Count;
            for (int i = 0; i < limit; i++)
            {
            }
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

    //new methods above here
}
