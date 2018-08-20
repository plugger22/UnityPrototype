using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using packageAPI;
using System.Text;

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
    /// General Info message. 'itemText' is itemData.text, 'reason' is a self-contained sentence, 'warning' is a self-contained and is shown in Red if 'isBad' true, otherwise Green
    /// 'topText' is RHS short tag. Default playerSide, Medium priority.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public Message GeneralInfo(string text, string itemText, string topText, string reason, string explanation, bool isBad = true)
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
            data.itemText = itemText;
            data.topText = topText;
            data.bottomText = GameManager.instance.itemDataScript.GetGeneralInfoDetails(reason, explanation, isBad);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.instance.guiScript.alertInformationSprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    /// <summary>
    /// General Warning message. 'itemText' is itemData.text, 'reason' is a self-contained sentence, 'warning' is a self-contained (shown Red is 'isBad' true, otherwise Green)
    /// 'isHighPriority' if true, Medium priority othewise. 'topText' is RHS short tag. Default playerSide
    /// NOTE: Don't put any formatting (colour or extra linefeeds) in 'warning'
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public Message GeneralWarning(string text, string itemText, string topText, string reason, string warning, bool isHighPriority = true, bool isBad = true)
    {
        Debug.Assert(string.IsNullOrEmpty(itemText) == false, "Invalid itemText (Null or Empty)");
        Debug.Assert(string.IsNullOrEmpty(reason) == false, "Invalid reason (Null or Empty");
        Debug.Assert(string.IsNullOrEmpty(warning) == false, "Invalid warning (Null or Empty)");
        Debug.Assert(string.IsNullOrEmpty(topText) == false, "Invalid topText (Null or Empty)");
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
            data.itemText = itemText;
            data.topText = topText;
            data.bottomText = GameManager.instance.itemDataScript.GetGeneralWarningDetails(reason, warning, isBad);
            if (isHighPriority == true)
            { data.priority = ItemPriority.High; }
            else { data.priority = ItemPriority.Medium; }
            data.sprite = GameManager.instance.guiScript.alertWarningSprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Random roll results (only ones that matter, don't spam). Auto player side. Format text as "Faction support Declined" with NO need, rolled, etc. 
    /// Set 'isReversed' to true if success means a bad outcome, eg. in case of Gear Compromise check a success is failing the roll
    /// 'typeOfCheck' is InfoApp RHS header in format '... Check', eg. 'Compromise'. Keep short
    /// </summary>
    /// <param name="text"></param>
    /// <param name="numNeeded"></param>
    /// <param name="numRolled"></param>
    /// <returns></returns>
    public void GeneralRandom(string text, string typeOfCheck, int numNeeded, int numRolled, bool isReversed = false)
    {
        Debug.Assert(string.IsNullOrEmpty(typeOfCheck) == false, "Invalid typeOfCheck (Null or Empty)");
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
            data.topText = GameManager.instance.itemDataScript.GetRandomTopText(typeOfCheck, isReversed);
            data.bottomText = GameManager.instance.itemDataScript.GetRandomDetails(numNeeded, numRolled, isReversed);
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.alertRandomSprite;
            data.tab = ItemTab.Random;
            data.side = message.side;
            data.help = 1;
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
            data.itemText = "Player moves";
            data.topText = "Move";
            data.bottomText = GameManager.instance.itemDataScript.GetPlayerMoveDetails(node);
            data.priority = ItemPriority.Low;
            data.sprite = node.Arc.sprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
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
    public Message PlayerSecret(string text, Secret secret, bool isGained = true)
    {
        Debug.Assert(secret != null, "Invalid secret (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Secret;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.data0 = secret.secretID;
            message.isPublic = true;
            //ItemData
            ItemData data = new ItemData();
            data.topText = secret.tag;
            data.bottomText = GameManager.instance.itemDataScript.GetPlayerSecretDetails(secret, isGained);
            if (isGained == true)
            { data.itemText = "Player gains a Secret"; }
            else
            { data.itemText = "Player loses a Secret"; }
            data.priority = ItemPriority.Low;
            data.sprite = playerSprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Player renown expended. 'dataID' is multipurpose (gearID if compromised, etc.) defaults to Player side. 'Reason' is a short text tag for ItemData giving a reason why renown was used. format 'to ....'
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="dataID"></param>
    /// <returns></returns>
    public Message RenownUsedPlayer(string text, int amount, int dataID, int nodeID = -1)
    {
        Debug.Assert(dataID >= 0, string.Format("Invalid dataID {0}", dataID));
        Debug.Assert(amount > 0, "Invalid amount (<= 0)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Renown;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.data0 = amount;
            message.data1 = dataID;
            message.data2 = nodeID;
            //add
            GameManager.instance.dataScript.AddMessage(message);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - Actors - - -
    //

    /// <summary>
    /// Actor / Player status changes, eg. Active -> Lie Low, Lie Low -> Active as a result of an Actor action. itemText in format '[Actor] ...', eg. 'sent to Reserves', 'Recalled', 'Let Go'
    /// 'Reason' is a short text tag for ItemData, in format '[Actor] ... Lying Low'
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="isPublic"></param>
    /// <returns></returns>
    public Message ActorStatus(string text, string itemText, string reason, int actorID, GlobalSide side, bool isPublic = false)
    {
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID ({0})", actorID));
        Debug.Assert(side != null, "Invalid side (Null)");
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
            
            data.topText = "Status Change";
            
            data.priority = ItemPriority.Low;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
            //data depends on whether an actor or player
            if (actorID == playerActorID)
            {
                data.itemText = string.Format("Player {0}", itemText);
                data.sprite = playerSprite;
                data.bottomText = GameManager.instance.itemDataScript.GetActorStatusDetails(reason, null);
            }
            else
            {
                //actor
                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    data.itemText = string.Format("{0} {1}", actor.arc.name, itemText);
                    data.sprite = actor.arc.sprite;
                    data.bottomText = GameManager.instance.itemDataScript.GetActorStatusDetails(reason, actor);
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
    /// Actor complains, 'reason' is Short, self-contained with '[Actor]' above, 'warnng' is self-contained and unformatted
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actor"></param>
    /// <param name="reason"></param>
    /// <param name="warning"></param>
    /// <returns></returns>
    public Message ActorComplains(string text, Actor actor, string reason, string warning)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Complains;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            message.data0 = actor.actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} has COMPLAINED", actor.arc.name);
            data.topText = "Complaint Lodged";
            data.bottomText = GameManager.instance.itemDataScript.GetActorComplainsDetails(actor, reason, warning);
            data.priority = ItemPriority.Medium;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
            data.sprite = actor.arc.sprite;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Actor in Reserve Pool is reassured or threatened, defaults to player side
    /// 'reason' is why the actor has been spoken to and is in format '[Actor] has been [reason]'
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="isPublic"></param>
    /// <returns></returns>
    public Message ActorSpokenToo(string text, string reason, Actor actor, int benefit, bool isPublic = false)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Reassured;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = isPublic;
            message.data0 = actor.actorID;
            message.data1 = benefit;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} has been Spoken too", actor.arc.name);
            data.topText = "You've had words";
            data.bottomText = GameManager.instance.itemDataScript.GetActorSpokenTooDetails(actor, reason, benefit);
            data.priority = ItemPriority.Low;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
            data.sprite = actor.arc.sprite;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Player / Actor condition gained or removed, eg. 'Stressed'. Creates ItemData ->  provide a 'reason' in a self-contained format, plus isGained / Lost and condition
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="side"></param>
    /// <param name="isPublic"></param>
    /// <returns></returns>
    public Message ActorCondition(string text, int actorID, bool isGained = true, Condition condition = null, string reason = null)
    {
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Condition;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            message.data0 = actorID;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            //ItemData
            ItemData data = new ItemData();
            string genericActorName = "Unknown";
            string genericActorArc = "Unknown";
            if (actorID == 999)
            {
                //player
                genericActorName = GameManager.instance.playerScript.PlayerName;
                genericActorArc = "Player";
            }
            else
            {
                //actor
                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    genericActorName = actor.actorName;
                    genericActorArc = actor.arc.name;
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            if (isGained == true)
            { data.itemText = string.Format("{0} is now {1}", genericActorArc, condition.name); }
            else { data.itemText = string.Format("{0} is no longer {1}", genericActorArc, condition.name); }
            data.topText = "Condition Change";
            if (condition != null)
            { data.bottomText = GameManager.instance.itemDataScript.GetActorConditionDetails(genericActorName, genericActorArc, condition, isGained, reason); }
            else { Debug.LogWarning("Invalid condition (Null)"); }
            //Medium priority if gain, Low if lost for Actor, always medium for Player
            if (actorID != 999)
            {
                if (isGained == true) { data.priority = ItemPriority.Medium; }
                else { data.priority = ItemPriority.Low; }
            }
            else { data.priority = ItemPriority.Medium; }
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
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
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Actor blackmail outcome -> either drops threat (motivation max) or carries out reveal of a player secret. SecretID only if reveal, ignore otherwise
    /// 'isThreatDropped' true if no longer blackmailing and 'reason' is why in format '[The threat has been dropped because]...'
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    ///<param name="secretID"></param>
    /// <returns></returns>
    public Message ActorBlackmail(string text, Actor actor, int secretID = -1, bool isThreatDropped = false, string reason = null)
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
            data.itemText = string.Format("{0} Blackmail threat resolved", actor.arc.name);
            data.topText = "Blackmail Resolved";
            data.bottomText = GameManager.instance.itemDataScript.GetActorBlackmailDetails(actor, secretID, isThreatDropped, reason);
            data.priority = ItemPriority.Medium;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
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
    public Message ActorSecret(string text, Actor actor, Secret secret, bool isLearnt = true)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(secret != null, "Invalid secret (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Secret;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            message.data0 = actor.actorID;
            message.data1 = secret.secretID;
            //ItemData
            ItemData data = new ItemData();
            data.topText = secret.tag;
            data.bottomText = GameManager.instance.itemDataScript.GetActorSecretDetails(actor, secret, isLearnt);
            if (isLearnt == true)
            { data.itemText = string.Format("{0} learns one of your Secrets", actor.arc.name);  }
            else
            {  data.itemText = string.Format("{0} forgets one of your Secrets", actor.arc.name);  }
            data.priority = ItemPriority.Low;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Secret revealed due to blackmail or unhappy actor in reserves. 
    /// 'reason' is short & self-contained & NOT colour formatted, with '[Actor]' in line above, eg. 'Unhappy in being left in Reserves' or 'Carries out Blackmail threat'
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actor"></param>
    /// <param name="secret"></param>
    /// <returns></returns>
    public Message ActorRevealSecret(string text, Actor actor, Secret secret, string reason)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(secret != null, "Invalid secret (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Secret;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            message.data0 = actor.actorID;
            message.data1 = secret.secretID;
            //ItemData
            ItemData data = new ItemData();
            data.topText = secret.tag;
            data.bottomText = GameManager.instance.itemDataScript.GetActorRevealSecretDetails(actor, secret, reason);
            data.itemText = string.Format("{0} reveals your SECRET", actor.arc.name);
            data.priority = ItemPriority.High;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
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
    public Message ActorRecruited(string text, int nodeID, Actor actor, int unhappyTimer)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
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
            message.data2 = unhappyTimer;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} Recruited", actor.arc.name);
            data.topText = "Recruited";
            data.bottomText = GameManager.instance.itemDataScript.GetActorRecruitedDetails(actor, unhappyTimer);
            data.priority = ItemPriority.Low;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Actor initiates a Relationship Conflict due to motivation dropping below zero (default playerSide). If 'Nothing Happens' then conflictID -1 and provide a reasonNoConflict (format '[because]...')
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="conflictID"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public Message ActorConflict(string text, Actor actor, int conflictID = -1, string reasonNoConflict = null)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Conflict;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.data0 = actor.actorID;
            message.data1 = conflictID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} Relationship Conflict", actor.arc.name);
            data.topText = "Relationship Conflict";
            data.bottomText = GameManager.instance.itemDataScript.GetActorConflictDetails(actor, conflictID, reasonNoConflict);
            data.priority = ItemPriority.High;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    /// <summary>
    /// Actor uses a trait, forText and toText are self-contained, eg. 'for an End Blackmail check', 'to avoid being paid off'. 
    /// NOTE: access via ActorManager.cs -> TraitLogMessage
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actor"></param>
    /// <param name="traitID"></param>
    /// <param name="forText"></param>
    /// <param name="toText"></param>
    /// <returns></returns>
    public Message ActorTrait(string text, Actor actor, Trait trait, string forText, string toText)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(trait != null, "Invalid trait (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Trait;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.data0 = actor.actorID;
            message.data1 = trait.traitID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} uses {1} trait", actor.arc.name, trait.tag);
            data.topText = string.Format("{0} trait used", trait.tag);
            data.bottomText = GameManager.instance.itemDataScript.GetActorTraitDetails(actor, trait, forText, toText);
            data.priority = ItemPriority.Low;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.Traits;
            data.side = message.side;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
    public Message DecisionGlobal(string text, string warning, int decID, GlobalSide side, bool isPublic = false)
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
            data.topText = "City Wide Decision";
            data.bottomText = GameManager.instance.itemDataScript.GetDecisionGlobalDetails(text, warning);
            data.priority = ItemPriority.Medium;
            data.sprite = mayor.sprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
    /// Player gifts an actor with gear ('isGiven' defaults to true) and gains motivation in return (extra if it's the actor's preferred gear type)
    /// Or player takes gear from actor('isGiven' set to false) and it costs a set amount of motivation (extra if preferred) to the actor to do so
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="gearID"></param>
    /// <param name="motivation"></param>
    /// <returns></returns>
    public Message GearTakeOrGive(string text, Actor actor, Gear gear, int motivation, bool isGiven = true)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(gear != null, "Invalid gear (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Gear_Given;
            message.side = globalResistance;
            message.data0 = actor.actorID;
            message.data1 = gear.gearID;
            message.data2 = motivation;
            //ItemData
            ItemData data = new ItemData();
            if (isGiven == true)
            {
                data.itemText = string.Format("{0} gear Given", gear.name);
                data.topText = "Give Gear";
            }
            else
            {
                data.itemText = string.Format("{0} gear Taken", gear.name);
                data.topText = "Gear Taken";
            }
            data.bottomText = GameManager.instance.itemDataScript.GetGearTakeOrGiveDetails(actor, gear, motivation, isGiven);
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
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
    public Message GearCompromised(string text, Gear gear, int renownUsed, int nodeID = -1)
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
            message.data1 = renownUsed;
            message.data2 = nodeID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} gear Compromised", gear.name);
            data.topText = "Gear Compromised";
            data.bottomText = GameManager.instance.itemDataScript.GetGearCompromisedDetails(gear, renownUsed);
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Gear has been used (not compromised). Returns null if text invalid.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public Message GearUsed(string text, Gear gear)
    {
        Debug.Assert(gear != null, "Invalid gear (Null)");
        //message
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Used;
            message.side = globalResistance;
            message.data0 = gear.gearID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} gear Used", gear.name);
            data.topText = "Gear Used";
            data.bottomText = GameManager.instance.itemDataScript.GetGearUsedDetails(gear);
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Gear possessed by an Actor has been Lost. Returns null if text invalid. 'isGivenToHQ' set true if gear given to actor who already has gear (old gear given to HQ)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="gearID"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message GearLost(string text, Gear gear, Actor actor, bool isGivenToHQ = false)
    {
        Debug.Assert(gear != null, "Invalid gear (Null)");
        Debug.Assert(actor != null, "Invalid actor (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Lost;
            message.side = globalResistance;
            message.isPublic = true;
            message.data0 = gear.gearID;
            message.data1 = actor.actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} gear Lost", gear.name);
            data.topText = "Gear Lost";
            data.bottomText = GameManager.instance.itemDataScript.GetGearLostDetails(gear, actor, isGivenToHQ);
            data.priority = ItemPriority.Medium;
            data.sprite = gear.sprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
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
    public Message GearAvailable(string text, Gear gear, Actor actor)
    {
        Debug.Assert(gear != null, "Invalid gear (Null)");
        Debug.Assert(actor != null, "Invalid actor (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Available;
            message.side = globalResistance;
            message.isPublic = true;
            message.data0 = gear.gearID;
            message.data1 = actor.actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} gear Available", gear.name);
            data.topText = "Gear Available";
            data.bottomText = GameManager.instance.itemDataScript.GetGearAvailableDetails(gear, actor);
            data.priority = ItemPriority.Medium;
            data.sprite = gear.sprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Gear obtained. If player has got the gear, leave actor as default '999'. Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public Message GearObtained(string text, Node node, Gear gear, int actorID = 999)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(gear != null, "Invalid gear (Null)");
        Debug.Assert(actorID >= 0, "Invalid actorID (less than Zero)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Obtained;
            message.side = globalResistance;
            message.data0 = node.nodeID;
            message.data1 = gear.gearID;
            message.data2 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} gear Obtained", gear.name);
            data.topText = "Gear Obtained";
            data.bottomText = GameManager.instance.itemDataScript.GetGearObtainedDetails(gear, node, actorID);
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
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
    /// <param name="factionApprovalLevel"></param>
    /// <param name="playerRenownBefore"></param>
    /// <param name="supportGiven"></param>
    /// <returns></returns>
    public Message FactionSupport(string text, Faction faction, int factionApprovalLevel, int playerRenownBefore, int supportGiven = -1)
    {
        Debug.Assert(faction != null, "Invalid faction (Null)");
        Debug.Assert(factionApprovalLevel > -1, "Invalid factionSupportLevel ( < zero)");
        Debug.Assert(playerRenownBefore > -1, "Invalid playerRenownBefore ( < zero)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.FACTION;
            message.subType = MessageSubType.Faction_Support;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            message.data0 = factionApprovalLevel;
            message.data1 = playerRenownBefore;
            message.data2 = supportGiven;
            //ItemData
            ItemData data = new ItemData();
            if (supportGiven > 0)
            {
                //support Approved
                data.itemText = string.Format("Support request to {0} HQ APPROVED", faction.name);
                data.topText = "Support APPROVED";
            }
            else
            {
                //support Declined
                data.itemText = string.Format("Support request to {0} HQ declined", faction.name);
                data.topText = "Support Declined";
            }
            data.bottomText = GameManager.instance.itemDataScript.GetFactionSupportDetails(faction, factionApprovalLevel, supportGiven);
            data.priority = ItemPriority.Medium;
            data.tab = ItemTab.Mail;
            data.side = message.side;
            data.sprite = faction.sprite;
            data.help = 1; //debug
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Change of Approval level, 'change' in +/-, 'reason' is self-contained
    /// </summary>
    /// <param name="text"></param>
    /// <param name="reason"></param>
    /// <param name="faction"></param>
    /// <param name="oldLevel"></param>
    /// <param name="change"></param>
    /// <param name="newLevel"></param>
    /// <returns></returns>
    public Message FactionApproval(string text, string reason, Faction faction, int oldLevel, int change, int newLevel)
    {
        Debug.Assert(faction != null, "Invalid faction (Null)");
        Debug.Assert(change != 0, "Invalid change (Zero)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.FACTION;
            message.subType = MessageSubType.Faction_Approval;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            message.data0 = oldLevel;
            message.data1 = change;
            message.data2 = newLevel;
            //ItemData
            ItemData data = new ItemData();
            if (change > 0)
            { data.itemText = string.Format("{0} faction Approval INCREASES", faction.name); }
            else
            {  data.itemText = string.Format("{0} faction Approval DECREASES", faction.name); }
            data.topText = "Approval Changes";
            data.bottomText = GameManager.instance.itemDataScript.GetFactionApprovalDetails(faction, reason, change, newLevel);
            data.priority = ItemPriority.Medium;
            data.tab = ItemTab.Mail;
            data.side = message.side;
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
            data.side = message.side;
            data.help = 1;
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
            data.side = message.side;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //new methods above here
}
