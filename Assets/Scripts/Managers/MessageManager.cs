﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using packageAPI;

/// <summary>
/// handles all message matters
/// </summary>
public class MessageManager : MonoBehaviour
{

    //fast access
    private GlobalSide globalResistance;
    private GlobalSide globalAuthority;
    private GlobalSide globalBoth;
    private int playerActorID = -1;
    private Sprite playerSprite;
    private Mayor mayor;

    /// <summary>
    /// Set up at start
    /// </summary>
    public void InitialiseEarly()
    {
        playerActorID = GameManager.instance.playerScript.actorID;
        playerSprite = GameManager.instance.playerScript.sprite;
        
        Debug.Assert(playerActorID > -1, "Invalid playerActorID (-1)");
        Debug.Assert(playerSprite != null, "Invalid playerSprite (Null)");

        globalResistance = GameManager.instance.globalScript.sideResistance;
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalBoth = GameManager.instance.globalScript.sideBoth;

        //event Listeners
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent, "MessageManager");
        EventManager.instance.AddListener(EventType.EndTurnEarly, OnEvent, "MessageManager");
    }

    /// <summary>
    /// needed due to gameManager initialisation sequence
    /// </summary>
    public void InitialiseLate()
    {
        mayor = GameManager.instance.cityScript.GetCity().mayor;
        Debug.Assert(mayor != null, "Invalid mayor (Null)");
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
            case EventType.EndTurnEarly:
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
            foreach (var message in dictOfPendingMessages)
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
            foreach (var record in dictOfCurrentMessages)
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
    // - - - General - - - 
    //

    /// <summary>
    /// General Info message
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public Message GeneralInfo(string text)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GENERAL;
            message.subType = MessageSubType.General_Info;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Info Alert";
            data.bottomText = text;
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.instance.guiScript.alertInformationSprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    /// <summary>
    /// General Warning message
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public Message GeneralWarning(string text)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GENERAL;
            message.subType = MessageSubType.General_Warning;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Warning";
            data.bottomText = text;
            data.priority = ItemPriority.High;
            data.sprite = GameManager.instance.guiScript.alertWarningSprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Random roll results (only ones that matter, don't spam). Auto player side.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="numNeeded"></param>
    /// <param name="numRolled"></param>
    /// <returns></returns>
    public void GeneralRandom(string text, int numNeeded, int numRolled)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            //Message
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GENERAL;
            message.subType = MessageSubType.General_Random;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.data0 = numNeeded;
            message.data1 = numRolled;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Random Result";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.alertRandomSprite;
            data.tab = ItemTab.Random;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
    }

    //
    // - - - Player - - -
    //

    /// <summary>
    /// Message -> player movement from one node to another. Returns null if text invalid.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public Message PlayerMove(string text, Node node)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Move;
            message.side = globalResistance;
            message.data0 = node.nodeID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Moved";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = node.Arc.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Player gains a new secret, defaults to player side
    /// </summary>
    /// <param name="text"></param>
    /// <param name="secretID"></param>
    /// <returns></returns>
    public Message PlayerSecret(string text, int secretID)
    {
        Debug.Assert(secretID >= 0, string.Format("Invalid secretID {0}", secretID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Secret;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.data0 = secretID;
            message.isPublic = true;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Gain Secret";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = playerSprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Player renown expended. 'dataID' is multipurpose (gearID if compromised, etc.) defaults to Player side.
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
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.data0 = nodeID;
            message.data1 = dataID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Use Renown";
            data.bottomText = text;
            data.sprite = playerSprite;
            data.priority = ItemPriority.Low;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - Actors - - -
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
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Status Change";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.tab = ItemTab.Mail;
            //data depends on whether an actor or player
            if (actorID == playerActorID)
            {
                //player
                data.sprite = playerSprite;
            }
            else
            {
                //actor
                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    data.sprite = actor.arc.sprite;
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Spoken To";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.tab = ItemTab.Mail;
            //data depends on whether an actor or player
            if (actorID == playerActorID)
            {
                //player
                data.sprite = playerSprite;
            }
            else
            {
                //actor
                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    data.sprite = actor.arc.sprite;
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Condition Change";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.tab = ItemTab.Mail;
            //data depends on whether an actor or player
            if (actorID == playerActorID)
            {
                //player
                data.sprite = playerSprite;
            }
            else
            {
                //actor
                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    data.sprite = actor.arc.sprite;
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Actor blackmail outcome -> either drops threat (motivation max) or carries out reveal of a player secret. SecretID only if reveal, ignore otherwise
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    ///<param name="secretID"></param>
    /// <returns></returns>
    public Message ActorBlackmail(string text, Actor actor, int secretID = -1)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Blackmail;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            message.data0 = actor.actorID;
            message.data1 = secretID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Blackmail";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Actor learns of a Player secret. Defaults to player side
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="secretID"></param>
    /// <returns></returns>
    public Message ActorSecret(string text, Actor actor, int secretID)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(secretID >= 0, string.Format("Invalid secretID {0}", secretID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Secret;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            message.data0 = actor.actorID;
            message.data1 = secretID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Learns Secret";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.Mail;

            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
    public Message ActorRecruited(string text, int nodeID, Actor actor, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(actor != null, "Invalid actor (Null)");
        if (side.level == globalResistance.level)
        { Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID)); }
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Recruited;
            message.side = side;
            message.data0 = nodeID;
            message.data1 = actor.actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Recruited";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Actor initiates a Relationship Conflict due to motivation dropping below zero
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="conflictID"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public Message ActorConflict(string text, Actor actor, int conflictID, GlobalSide side)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(conflictID >= 0, string.Format("Invalid conflictID {0}", conflictID));
        Debug.Assert(side != null, "Invalid side (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Conflict;
            message.side = side;
            message.data0 = actor.actorID;
            message.data1 = conflictID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Relationship Conflict";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
            //add
            GameManager.instance.dataScript.AddMessage(message);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// AI notification of Node Activity
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="actorID"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
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
            //add
            GameManager.instance.dataScript.AddMessage(message);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// AI detects player hacking the AI (TraceBack is On)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="actorID"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public Message AIDetected(string text, int nodeID, int delay)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid NodeID {0}", nodeID));
        Debug.Assert(delay >= 0, string.Format("Invalid delay {0}", delay));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Detected;
            message.side = globalAuthority;
            message.isPublic = true;
            message.displayDelay = delay;
            message.data0 = nodeID;
            message.data1 = GameManager.instance.playerScript.actorID;
            //add
            GameManager.instance.dataScript.AddMessage(message);
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
            message.data0 = nodeID;
            message.data1 = connID;
            message.data2 = actorID;
            //add
            GameManager.instance.dataScript.AddMessage(message);
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
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Captured";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.capturedSprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Release from Capture";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.releasedSprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// AI notification to one or both sides of AI being hacked
    /// </summary>
    /// <param name="text"></param>
    /// <param name="currentRenownCost"></param>
    /// <param name="isDetected"></param>
    /// <returns></returns>
    public Message AIHacked(string text, int currentRenownCost, bool isDetected)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Hacked;
            if (isDetected == true)
            { message.side = globalBoth; message.data1 = 1; message.isPublic = true; }
            else
            { message.side = globalResistance; message.data1 = 0; message.isPublic = false; }
            message.data0 = currentRenownCost;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Hacking Detected";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.alarmSprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// AI notification to both sides of an AI Security System Reboot (commence / complete)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="currentRenownCost"></param>
    /// <param name="rebootTimer"></param>
    /// <returns></returns>
    public Message AIReboot(string text, int currentRenownCost, int rebootTimer)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Reboot;
            message.side = globalBoth;
            message.isPublic = true;
            message.data0 = currentRenownCost;
            message.data1 = rebootTimer;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Reboot";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.aiRebootSprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// AI notification of countermeasures commenced / finished
    /// </summary>
    /// <param name="text"></param>
    /// <param name="timer"></param>
    /// <param name="protocolLevel"></param>
    /// <returns></returns>
    public Message AICounterMeasure(string text, int timerStartValue = -1, int protocolLevelNew = -1)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Countermeasure;
            message.side = globalBoth;
            message.isPublic = true;
            message.data0 = timerStartValue;
            message.data1 = protocolLevelNew;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Countermeasures";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.aiCountermeasureSprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// AI notification to both sides of a change in AI Alert Status
    /// </summary>
    /// <param name="text"></param>
    /// <param name="chanceOfIncrease"></param>
    /// <param name="randomRoll"></param>
    /// <returns></returns>
    public Message AIAlertStatus(string text, int chanceOfIncrease, int randomRoll)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Alert;
            message.side = globalBoth;
            message.isPublic = true;
            message.data0 = chanceOfIncrease;
            message.data1 = randomRoll;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "AI Alert Status";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.aiAlertSprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Decision Made";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = mayor.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Connection Security";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = mayor.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Request Resources";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = mayor.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Request Team";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = mayor.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
    public Message TeamAdd(string text, Team team, bool isPublic = false)
    {
        Debug.Assert(team != null, "Invalid team (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Add;
            message.side = globalAuthority;
            message.data0 = team.teamID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Add Team";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = team.arc.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
    public Message TeamDeploy(string text, int nodeID, Team team, int actorID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(team != null, "Invalid teamID {Null}");
        if (GameManager.instance.sideScript.authorityOverall == SideState.Player)
        { Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID)); }
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Deploy;
            message.side = globalAuthority;
            message.data0 = nodeID;
            message.data1 = team.teamID;
            message.data2 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Team Deployed";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = team.arc.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
    public Message TeamAutoRecall(string text, int nodeID, Team team, int actorID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(team != null, "Invalid team (Null)");
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
            message.data1 = team.teamID;
            message.data2 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Team Autorecalled";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = team.arc.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
    public Message TeamWithdraw(string text, int nodeID, Team team, int actorID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(team != null, "Invalid team (Null)");
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Withdraw;
            message.side = globalAuthority;
            message.data0 = nodeID;
            message.data1 = team.teamID;
            message.data2 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Team Withdrawn";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = team.arc.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
    public Message TeamEffect(string text, int nodeID, Team team)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(team != null, "Invalid team (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Effect;
            message.side = globalAuthority;
            message.isPublic = true;
            message.data0 = nodeID;
            message.data1 = team.teamID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Team Outcome";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = team.arc.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Team is neutralised by the Resistance. Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="teamID"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message TeamNeutralise(string text, int nodeID, Team team, int actorID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(team != null, "Invalid team (Null)");
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
            message.data1 = team.teamID;
            message.data2 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Team Neutralised";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = team.arc.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - -  Gear - - -
    //

    /// <summary>
    /// Player gifts an actor with gear and gains motivation in return (extra if it's the actor's preferred gear type)
    /// Or player takes gear from actor and it costs a set amount of motivation (extra if preferred) to the actor to do so
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="gearID"></param>
    /// <param name="motivation"></param>
    /// <returns></returns>
    public Message GearSwapOrGive(string text, int actorID, Gear gear, int motivation)
    {
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        Debug.Assert(gear != null, "Invalid gear (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Gear_Given;
            message.side = globalResistance;
            message.data0 = actorID;
            message.data1 = gear.gearID;
            message.data2 = motivation;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Give Gear";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
    public Message GearCompromised(string text, Gear gear, int nodeID = -1)
    {
        Debug.Assert(gear != null, "Invalid gear (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Comprised;
            message.side = globalResistance;
            message.isPublic = true;
            message.data0 = gear.gearID;
            message.data1 = nodeID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Gear Compromised";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Gear has been used (not compromised). Returns null if text invalid. Also updates gear.statTimesUsed
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public Message GearUsed(string text, Gear gear, int nodeID = -1)
    {
        Debug.Assert(gear != null, "Invalid gear (Null)");
        //gear stat
        gear.statTimesUsed++;
        //message
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Used;
            message.side = globalResistance;
            message.data0 = gear.gearID;
            message.data1 = nodeID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Gear Used";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Gear possessed by an Actor has been Lost. Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="gearID"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message GearLost(string text, Gear gear, int actorID)
    {
        Debug.Assert(gear != null, "Invalid gear (Null)");
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Lost;
            message.side = globalResistance;
            message.isPublic = true;
            message.data0 = gear.gearID;
            message.data1 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Gear Lost";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Gear possessed by an Actor is now available for the player to request it back (and it can be lost)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="gearID"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message GearAvailable(string text, Gear gear, int actorID)
    {
        Debug.Assert(gear != null, "Invalid gear (Null)");
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Available;
            message.side = globalResistance;
            message.isPublic = true;
            message.data0 = gear.gearID;
            message.data1 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Gear Available";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
    public Message GearObtained(string text, int nodeID, Gear gear, int actorID = 999)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(gear != null, "Invalid gear (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Obtained;
            message.side = globalResistance;
            message.data0 = nodeID;
            message.data1 = gear.gearID;
            message.data2 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Gear Obtained";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
    public Message TargetAttempt(string text, int nodeID, int actorID, Target target)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        Debug.Assert(target != null, "Invalid target (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TARGET;
            message.subType = MessageSubType.Target_Attempt;
            message.side = globalResistance;
            message.data0 = nodeID;
            message.data1 = actorID;
            message.data2 = target.targetID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Attempt Target";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = target.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
    public Message TargetContained(string text, int nodeID, int teamID, Target target)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID));
        Debug.Assert(teamID >= 0, string.Format("Invalid teamID {0}", teamID));
        Debug.Assert(target != null, "Invalid target (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TARGET;
            message.subType = MessageSubType.Target_Contained;
            message.side = globalBoth;
            message.data0 = nodeID;
            message.data1 = teamID;
            message.data2 = target.targetID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Target Contained";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = target.sprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
            message.data0 = nodeID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Ongoing Effect";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.ongoingEffectSprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Ongoing Effect Finished";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.ongoingEffectSprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - Faction - - -
    //

    /// <summary>
    /// Faction support (renown) that is given/declined at the beginning of each turn
    /// </summary>
    /// <param name="text"></param>
    /// <param name="factionSupportLevel"></param>
    /// <param name="playerRenownBefore"></param>
    /// <param name="supportGiven"></param>
    /// <returns></returns>
    public Message FactionSupport(string text, Faction faction, int factionSupportLevel, int playerRenownBefore, int supportGiven = -1)
    {
        Debug.Assert(faction != null, "Invalid faction (Null)");
        Debug.Assert(factionSupportLevel > -1, "Invalid factionSupportLevel ( < zero)");
        Debug.Assert(playerRenownBefore > -1, "Invalid playerRenownBefore ( < zero)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.FACTION;
            message.subType = MessageSubType.Faction_Support;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            message.data0 = factionSupportLevel;
            message.data1 = playerRenownBefore;
            message.data2 = supportGiven;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Faction Support";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.tab = ItemTab.Mail;
            data.sprite = faction.sprite;
            data.help = 1; //debug
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - Nodes - - -
    //

    /// <summary>
    /// Node undergoes a crisis (commences, averted, finishes)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public Message NodeCrisis(string text, Node node, int reductionInCityLoyalty = -1)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.NODE;
            message.subType = MessageSubType.Node_Crisis;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            message.data0 = node.nodeID;
            message.data1 = reductionInCityLoyalty;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "District Crisis";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.nodeCrisisSprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - Cities - - -
    //

    /// <summary>
    /// City loyalty changes for any reason
    /// </summary>
    /// <param name="text"></param>
    /// <param name="newCityLoyalty"></param>
    /// <param name="changeInLoyalty"></param>
    /// <returns></returns>
    public Message CityLoyalty(string text, int newCityLoyalty, int changeInLoyalty)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.CITY;
            message.subType = MessageSubType.City_Loyalty;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            message.data0 = newCityLoyalty;
            message.data1 = changeInLoyalty;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "City Loyalty";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.cityLoyaltySprite;
            data.tab = ItemTab.Mail;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //new methods above here
}
