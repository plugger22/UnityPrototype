using gameAPI;
using packageAPI;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all message matters
/// </summary>
public class MessageManager : MonoBehaviour
{

    //fast access
    private GlobalSide globalResistance;
    private GlobalSide globalAuthority;
    private GlobalSide globalBoth;
    private string tagAIName;                                                //name of AI protagonist
    private int playerActorID = -1;
    private Sprite playerSprite;
    private Mayor mayor;

    public int messageCounter = 0;                                          //messageID counter

    /// <summary>
    /// Set up at start
    /// </summary>
    public void InitialiseEarly()
    {
        //Run in GameManager.cs -> listOfGlobalMethods with GameState.StartUp so doesn't need check for GameState.NewInitialisation
        playerActorID = GameManager.instance.playerScript.actorID;
        playerSprite = GameManager.instance.playerScript.sprite;
        Debug.Assert(playerActorID > -1, "Invalid playerActorID (-1)");
        Debug.Assert(playerSprite != null, "Invalid playerSprite (Null)");

        globalResistance = GameManager.instance.globalScript.sideResistance;
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalBoth = GameManager.instance.globalScript.sideBoth;
        tagAIName = GameManager.instance.globalScript.tagAIName;

        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalBoth != null, "Invalid globalBoth (Null)");
        Debug.Assert(string.IsNullOrEmpty(tagAIName) == false, "Invalid tagAIName (Null or Empty)");

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
    /// Reset message ID counter prior to a new level
    /// </summary>
    public void ResetCounter()
    { messageCounter = 0; }

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
        //
        // - - - Messages - - -
        //
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
        //
        // - - - ItemData - - -
        //
        List<ItemData> listOfDelayedItemData = GameManager.instance.dataScript.GetListOfDelayItemData();
        if (listOfDelayedItemData != null)
        {
            for (int index = listOfDelayedItemData.Count - 1; index >= 0; index-- )
            {
                ItemData data = listOfDelayedItemData[index];
                //decrement delay
                data.delay--;
                //ready for prime time?
                if (data.delay <= 0)
                {
                    //add to display list
                    GameManager.instance.dataScript.AddItemData(data);
                    //remove from delay list
                    listOfDelayedItemData.RemoveAt(index);
                }
            }
        }
        else { Debug.LogWarning("Invalid listOfDelayItemData (Null)"); }
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
            data.tab = ItemTab.ALERTS;
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
            data.tab = ItemTab.ALERTS;
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
            data.tag0 = "roll_0";
            data.tag1 = "roll_1";
            data.tag2 = "roll_2";
            //add (message only if a meaningful outcome)
            if (isReversed == false)
            { if (numRolled >= numNeeded) { GameManager.instance.dataScript.AddMessage(message); } }
            else { if (numRolled < numNeeded) { GameManager.instance.dataScript.AddMessage(message); } }
            
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
    }

    //
    // - - - Player - - -
    //

    /// <summary>
    /// Message -> player movement from one node to another. Returns null if text invalid. 'isStart' is only for player's starting location
    /// 'changeInvisibility' and 'aiDelay' only if spotted
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public Message PlayerMove(string text, Node node, int changeInvisibility = 0, int aiDelay = 0,  bool isStart = false)
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
            if (isStart == true)
            {
                data.itemText = "Player starting location";
                data.topText = "Start Location";
            }
            else
            {
                data.itemText = "Player moves";
                data.topText = "Move";
            }
            data.bottomText = GameManager.instance.itemDataScript.GetPlayerMoveDetails(node, changeInvisibility, aiDelay);
            data.priority = ItemPriority.Low;
            data.sprite = node.Arc.sprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
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
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.help = 1;
            data.tag0 = "secret_0";
            data.tag1 = "secret_1";
            data.tag2 = "secret_2";
            data.tag3 = "secret_3";
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

    /// <summary>
    /// Resistance Player Spotted and Damaged by Nemesis
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="dataID"></param>
    /// <returns></returns>
    public Message PlayerDamage(string text, string damageInfo, string damageEffect, int nodeID)
    {
        Debug.Assert(nodeID >= 0, string.Format("Invalid dataID {0}", nodeID));
        if (string.IsNullOrEmpty(text) == false)
        {
            bool isResistance = true;
            if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level)
            { isResistance = false; }
                Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Damage;
            message.side = globalBoth;
            message.data0 = nodeID;
            //ItemData
            ItemData data = new ItemData();
            data.topText = "Nemesis Attacks";
            data.bottomText = GameManager.instance.itemDataScript.GetPlayerDamageDetails(damageInfo, damageEffect, isResistance);
            if (isResistance)
            { data.itemText = "You have been Damaged by your NEMESIS"; }
            else { data.itemText = string.Format("{0} has been Damaged by their NEMESIS", GameManager.instance.playerScript.GetPlayerNameResistance()); }
            data.priority = ItemPriority.Low;
            data.sprite = playerSprite;
            data.tab = ItemTab.ALERTS;
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
    /// Player spotted due to having the IMAGED / TAGGED condition
    /// </summary>
    /// <param name="text"></param>
    /// <param name="detailsTop"></param>
    /// <param name="detailsBottom"></param>
    /// <param name="sprite"></param>
    /// <param name="actorID"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public Message PlayerSpotted(string text, string detailsTop, string detailsBottom, Node node)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Recognised;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            message.data0 = node.nodeID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "";
            data.bottomText = GameManager.instance.itemDataScript.GetPlayerSpottedDetails(detailsTop, detailsBottom, node);
            data.priority = ItemPriority.High;
            data.sprite = playerSprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            if (node != null)
            { data.nodeID = node.nodeID; }
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Player betrayed (drop in invisibility) by traitorous subordinate or somebody in RebelHQ. isImmediate true if immediate notification to authority.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public Message PlayerBetrayed(string text, bool isImmediate = false)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Betrayed;
            message.side = globalResistance;
            message.isPublic = true;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "You have been BETRAYED";
            data.topText = "Information Leaked";
            data.bottomText = GameManager.instance.itemDataScript.GetPlayerBetrayedDetails(isImmediate);
            data.priority = ItemPriority.High;
            data.sprite = playerSprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.help = 1;
            data.tag0 = "betrayal_0";
            data.tag1 = "betrayal_1";
            data.tag2 = "betrayal_2";
            data.tag3 = "betrayal_3";
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
    /// Actor / Player status changes, eg. Active -> Lie Low, Lie Low -> Active as a result of an Actor action. itemText in format '[Actor] ...', eg. 'sent to Reserves', 'Recalled', 'Let Go'
    /// 'Reason' is a short text tag for ItemData, in format '[Actor] ... Lying Low'
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="isPublic"></param>
    /// <returns></returns>
    public Message ActorStatus(string text, string itemTextTag, string reason, int actorID, GlobalSide side, string details = null)
    {
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID ({0})", actorID));
        Debug.Assert(side != null, "Invalid side (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.subType = MessageSubType.Actor_Status;
            message.side = side;
            message.isPublic = true;
            message.data0 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.topText = "Status Change";
            data.priority = ItemPriority.Low;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.help = 1;
            //data depends on whether an actor or player
            if (actorID == playerActorID)
            {
                message.type = MessageType.PLAYER;
                data.itemText = string.Format("{0}, Player, {1}", GameManager.instance.playerScript.PlayerName, itemTextTag);
                data.sprite = playerSprite;
                data.bottomText = GameManager.instance.itemDataScript.GetActorStatusDetails(reason, details, null);
            }
            else
            {
                //actor
                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    message.type = MessageType.ACTOR;
                    data.itemText = string.Format("{0}, {1}, {2}", actor.actorName, actor.arc.name, itemTextTag);
                    data.sprite = actor.arc.sprite;
                    data.bottomText = GameManager.instance.itemDataScript.GetActorStatusDetails(reason, details, actor);
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
    /// Authority Actor/Player takes stress leave
    /// </summary>
    /// <param name="text"></param>
    /// <param name="itemTextTag"></param>
    /// <param name="reason"></param>
    /// <param name="actorID"></param>
    /// <param name="side"></param>
    /// <param name="details"></param>
    /// <returns></returns>
    public Message ActorStressLeave(string text, int actorID, GlobalSide side)
    {
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID ({0})", actorID));
        Debug.Assert(side != null, "Invalid side (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.subType = MessageSubType.Actor_StressLeave;
            message.side = side;
            message.isPublic = true;
            message.data0 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.topText = "Stress Leave";
            data.priority = ItemPriority.Low;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.help = 1;
            //data depends on whether an actor or player
            if (actorID == playerActorID)
            {
                message.type = MessageType.PLAYER;
                if (side.level == globalAuthority.level)
                { data.itemText = string.Format("{0}, Mayor, takes Stress Leave", GameManager.instance.playerScript.GetPlayerNameAuthority()); }
                else { data.itemText = string.Format("{0}, Player, takes Stress Leave", GameManager.instance.playerScript.GetPlayerNameResistance()); }
                data.sprite = playerSprite;
                data.bottomText = GameManager.instance.itemDataScript.GetActorStressLeaveDetails(side);
            }
            else
            {
                //actor
                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    message.type = MessageType.ACTOR;
                    data.itemText = string.Format("{0}, {1}, has taken Stress Leave", actor.actorName, actor.arc.name);
                    data.sprite = actor.arc.sprite;
                    data.bottomText = GameManager.instance.itemDataScript.GetActorStressLeaveDetails(side, actor);
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
            data.tab = ItemTab.ALERTS;
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
            data.tab = ItemTab.ALERTS;
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
    /// isResistancePlayerAffected (default true) determines which side's player is affected (ignore if not player). The message will go the default human player side (which may be different)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="side"></param>
    /// <param name="isPublic"></param>
    /// <returns></returns>
    public Message ActorCondition(string text, int actorID, bool isGained = true, Condition condition = null, string reason = null, bool isResistancePlayerAffected = true)
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
            //ItemData
            ItemData data = new ItemData();
            string genericActorName = "Unknown";
            string genericActorArc = "Unknown";
            if (actorID == 999)
            {
                //player -> player who is affected which can be different from playerSide (who gets the message)
                if (isResistancePlayerAffected == true) { genericActorName = GameManager.instance.playerScript.GetPlayerNameResistance(); }
                else { genericActorName = GameManager.instance.playerScript.GetPlayerNameAuthority(); }
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
            { data.itemText = string.Format("{0}, {1}, is now {2}{3}", genericActorName, genericActorArc, condition.isNowA == true ? "a " : "", condition.tag); }
            else { data.itemText = string.Format("{0}, {1}, is no longer {2}{3}", genericActorName, genericActorArc, condition.isNowA == true ? "a " : "", condition.tag); }
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
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.help = 1;
            if (condition != null)
            {
                //tooltip
                switch (condition.tag)
                {
                    case "QUESTIONABLE":
                        data.tag0 = "questionable_0";
                        data.tag1 = "questionable_1";
                        data.tag2 = "questionable_2";
                        data.tag3 = "questionable_3";
                        break;
                }
            }
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
            data.tab = ItemTab.ALERTS;
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
            { data.itemText = string.Format("{0}, {1}, learns one of your Secrets", actor.actorName, actor.arc.name);  }
            else
            {  data.itemText = string.Format("{0}, {1}, forgets one of your Secrets", actor.actorName, actor.arc.name);  }
            data.priority = ItemPriority.Low;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.ALERTS;
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
            data.tab = ItemTab.ALERTS;
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
            data.tab = ItemTab.ALERTS;
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
            data.tab = ItemTab.ALERTS;
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
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// current status of Lie Low global cooldown timer
    /// </summary>
    /// <param name="text"></param>
    /// <param name="timer"></param>
    /// <returns></returns>
    public Message ActorLieLowOngoing(string text, int timer)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_LieLow;
            message.side = globalResistance;
            message.data0 = timer;
            //ItemData
            ItemData data = new ItemData();
            if (timer == 0)
            {
                data.itemText = "Lie Low action is AVAILABLE";
                data.topText = "Lie Low Available";
            }
            else
            {
                data.itemText = "Lie Low action is UNAVAILABLE";
                data.topText = "Lie Low Unavailable";
            }
            data.bottomText = GameManager.instance.itemDataScript.GetActorLieLowOngoingDetails(timer);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.instance.guiScript.infoSprite;
            data.tab = ItemTab.Effects;
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
    /// notification of Player (use default actorID 999) or actor being captured at a node
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="teamID"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message ActorCapture(string text, Node node, Team team, int actorID = 999)
    {
        Debug.Assert(node != null, "Invalid Node (Null)");
        Debug.Assert(team != null, "Invalid team (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Captured;
            message.side = globalBoth;
            message.isPublic = true;
            message.data0 = node.nodeID;
            message.data1 = team.teamID;
            message.data2 = actorID;
            //ItemData
            ItemData data = new ItemData();
            string playerName = GameManager.instance.playerScript.PlayerName;
            if (actorID == 999)
            {
                //player captured
                data.itemText = string.Format("{0}, Player, has been CAPTURED", playerName);
                data.topText = "Player Captured";
                data.bottomText = GameManager.instance.itemDataScript.GetActorCaptureDetails(playerName, "Player", node, team);
            }
            else
            {
                //actor captured
                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    data.itemText = string.Format("{0}, {1}, has been CAPTURED", actor.actorName, actor.arc.name);
                    data.topText = string.Format("{0} Captured", actor.arc.name);
                    data.bottomText = GameManager.instance.itemDataScript.GetActorCaptureDetails(actor.actorName, actor.arc.name, node, team);
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            data.priority = ItemPriority.High;
            data.sprite = GameManager.instance.guiScript.capturedSprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// warning, eg. Actor can be captured 'cause invisibility zero -> InfoApp effect
    /// </summary>
    /// <param name="text"></param>
    /// <param name="detailsTop"></param>
    /// <param name="detailsBottom"></param>
    /// <param name="sprite"></param>
    /// <param name="actorID"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public Message ActorWarningOngoing(string text, string detailsTop, string detailsBottom, Sprite sprite, int actorID)
    {
        Debug.Assert(sprite != null, "Invalid spirte (Null)");
        Debug.Assert(actorID > -1, "Invalid actorID (less than Zero)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Warning;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            message.data0 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Warning";
            data.bottomText = GameManager.instance.itemDataScript.GetActorWarningOngoingDetails(detailsTop, detailsBottom);
            data.priority = ItemPriority.Low;
            data.sprite = sprite;
            data.tab = ItemTab.Effects;
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
    /// notification of Player or Actor (Resistance) being released from capture
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message ActorRelease(string text, Node node, int actorID = 999)
    {
        Debug.Assert(node != null, "Invalid Node (Null)");
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Captured;
            message.side = globalBoth;
            message.isPublic = true;
            message.data0 = node.nodeID;
            message.data1 = actorID;
            //ItemData
            ItemData data = new ItemData();
            string playerName = GameManager.instance.playerScript.PlayerName;
            if (actorID == 999)
            {
                //player captured
                data.itemText = string.Format("{0}, Player, has been RELEASED", playerName);
                data.topText = "Player Released";
                data.bottomText = GameManager.instance.itemDataScript.GetActorReleaseDetails(playerName, "Player", node);
            }
            else
            {
                //actor captured
                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    data.itemText = string.Format("{0}, {1}, has been RELEASED", actor.actorName, actor.arc.name);
                    data.topText = string.Format("{0} Released", actor.arc.name);
                    data.bottomText = GameManager.instance.itemDataScript.GetActorReleaseDetails(actor.actorName, actor.arc.name, node);
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            data.priority = ItemPriority.High;
            data.sprite = GameManager.instance.guiScript.releasedSprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.tag0 = "traitor_0";
            data.tag1 = "traitor_1";
            data.tag2 = "traitor_2";
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Resistance Actor gains or loses a single contact (resistance side message). 'Reason' is self contained
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <param name="isGained"></param>
    /// <returns></returns>
    public Message ContactChange(string text, Actor actor, Node node, Contact contact, bool isGained = true, string reason = null)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(contact != null, "Invalid contact (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.CONTACT;
            message.subType = MessageSubType.Contact_Change;
            message.side = globalResistance;
            message.data0 = actor.actorID;
            message.data1 = node.nodeID;
            message.data2 = contact.contactID;
            //ItemData
            ItemData data = new ItemData();
            if (isGained == true)
            {
                data.itemText = string.Format("{0}, Aquires a new CONTACT", actor.arc.name);
                data.topText = "New Contact";
            }
            else
            {
                data.itemText = string.Format("{0}, Loses an existing CONTACT", actor.arc.name);
                data.topText = "Contact Lost";
            }
            data.bottomText = GameManager.instance.itemDataScript.GetContactDetails(reason, actor, node, contact, isGained);
            data.priority = ItemPriority.Medium;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// One of a Resistance Actors network of contacts learns of a rumour
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <param name="isGained"></param>
    /// <returns></returns>
    public Message ContactTargetRumour(string text, Actor actor, Node node, Contact contact, Target target)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(contact != null, "Invalid contact (Null)");
        Debug.Assert(target != null, "Invalid target (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.CONTACT;
            message.subType = MessageSubType.Contact_Target_Rumour;
            message.side = globalResistance;
            message.data0 = actor.actorID;
            message.data1 = node.nodeID;
            message.data2 = contact.contactID;
            message.data3 = target.targetID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("One of {0}'s network of contacts learns of a RUMOUR", actor.arc.name);
            data.topText = string.Format("{0} gets a CALL", actor.actorName);
            data.bottomText = GameManager.instance.itemDataScript.GetContactTargetRumourDetails(actor, node, contact, target);
            data.priority = ItemPriority.Low;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// One of a Resistance Actors network of contacts spots the Nemesis
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <param name="isGained"></param>
    /// <returns></returns>
    public Message ContactNemesisSpotted(string text, Actor actor, Node node, Contact contact, Nemesis nemesis, int moveNumber)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(contact != null, "Invalid contact (Null)");
        Debug.Assert(nemesis != null, "Invalid nemesis (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.CONTACT;
            message.subType = MessageSubType.Contact_Nemesis_Spotted;
            message.side = globalResistance;
            message.data0 = actor.actorID;
            message.data1 = node.nodeID;
            message.data2 = contact.contactID;
            message.data3 = moveNumber;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("One of {0}'s network of contacts spots your NEMESIS", actor.arc.name);
            data.topText = string.Format("{0} gets a CALL", actor.actorName);
            data.bottomText = GameManager.instance.itemDataScript.GetContactNemesisSpottedDetails(actor, node, contact, nemesis, moveNumber);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.instance.guiScript.aiAlertSprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// One of Resistance Actor's network of contacts spots an Authority Team (Erasure)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <param name="team"></param>
    /// <returns></returns>
    public Message ContactTeamSpotted(string text, Actor actor, Node node, Contact contact, Team team)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(contact != null, "Invalid contact (Null)");
        Debug.Assert(team != null, "Invalid team (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.CONTACT;
            message.subType = MessageSubType.Contact_Team_Spotted;
            message.side = globalResistance;
            message.data0 = actor.actorID;
            message.data1 = node.nodeID;
            message.data2 = contact.contactID;
            message.data3 = team.teamID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("One of {0}'s contacts spots an {1} Team", actor.arc.name, team.arc.name);
            data.topText = string.Format("{0} gets a CALL", actor.actorName);
            data.bottomText = GameManager.instance.itemDataScript.GetContactTeamSpottedDetails(actor, node, contact, team);
            data.priority = ItemPriority.Medium;
            data.sprite = actor.arc.sprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Nemesis spotted by a Resistance Tracer
    /// </summary>
    /// <param name="text"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public Message TracerNemesisSpotted(string text, Node node, Nemesis nemesis, int moveNumber)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(nemesis != null, "Invalid nemesis (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.CONTACT;
            message.subType = MessageSubType.Tracer_Nemesis_Spotted;
            message.side = globalResistance;
            message.data0 = node.nodeID;
            message.data1 = moveNumber;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "TRACER picks up an ANOMALOUS reading";
            data.topText = "Threat Detected";
            data.bottomText = GameManager.instance.itemDataScript.GetTracerNemesisSpottedDetails(node, nemesis, moveNumber);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.instance.guiScript.aiAlertSprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Tracer spots an Erasure team
    /// </summary>
    /// <param name="text"></param>
    /// <param name="node"></param>
    /// <param name="team"></param>
    /// <returns></returns>
    public Message TracerTeamSpotted(string text, Node node, Team team)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(team != null, "Invalid team (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.CONTACT;
            message.subType = MessageSubType.Tracer_Team_Spotted;
            message.side = globalResistance;
            message.data0 = node.nodeID;
            message.data1 = team.teamID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "TRACER detects presence of an ERASURE Team";
            data.topText = "Threat Detected";
            data.bottomText = GameManager.instance.itemDataScript.GetTracerTeamSpottedDetails(node, team);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.instance.guiScript.aiAlertSprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }



    /// <summary>
    /// Nemesis Ongoing Status -> InfoApp 'Effect' tab
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public Message NemesisOngoingEffect(string text, int nodeID, Nemesis nemesis)
    {
        Debug.Assert(nodeID > -1, "Invalid node (less than Zeros)");
        Debug.Assert(nemesis != null, "Invalid nemesis (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Nemesis;
            message.side = globalResistance;
            message.data0 = nodeID;
            message.data1 = GameManager.instance.nemesisScript.GetSearchRatingAdjusted();
            message.data2 = GameManager.instance.nemesisScript.GetStealthRatingAdjusted();
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} Nemesis current Status", nemesis.name);
            data.topText = "Nemesis Status";
            data.bottomText = GameManager.instance.itemDataScript.GetNemesisOngoingEffectDetails(nemesis, message.data1);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.instance.guiScript.aiAlertSprite;
            data.tab = ItemTab.Effects;
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
    /// Used whenever Nemesis goes in or out of Hunt mode
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="nemesis"></param>
    /// <returns></returns>
    public Message NemesisNewMode(string text, int nodeID, Nemesis nemesis)
    {
        Debug.Assert(nodeID > -1, "Invalid node (less than Zeros)");
        Debug.Assert(nemesis != null, "Invalid nemesis (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Nemesis;
            message.side = globalResistance;
            message.data0 = nodeID;
            message.data1 = GameManager.instance.nemesisScript.GetSearchRatingAdjusted();
            message.data2 = GameManager.instance.nemesisScript.GetStealthRatingAdjusted();
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} Nemesis changes mode", nemesis.name);
            data.topText = "New Nemesis Mode";
            data.bottomText = GameManager.instance.itemDataScript.GetNemesisNewModeDetails(nemesis, message.data1);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.instance.guiScript.aiAlertSprite;
            data.tab = ItemTab.ALERTS;
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
    /// Authority player nemesis control / cooldown status ('Effects' tab in InfoApp). Provide nodes only in case of Player in control
    /// </summary>
    /// <param name="text"></param>
    /// <param name="coolDownTimer"></param>
    /// <param name="controlTimer"></param>
    /// <returns></returns>
    public Message NemesisPlayerOngoing(string text, Nemesis nemesis, bool isPlayerControl, int coolDownTimer, int controlTimer, Node nodeControl = null, Node nodeCurrent = null)
    {
        Debug.Assert(nemesis != null, "Invalid nemesis (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Nemesis;
            message.side = globalAuthority;
            message.data0 = coolDownTimer;
            message.data1 = controlTimer;
            //ItemData
            ItemData data = new ItemData();
            if (GameManager.instance.nemesisScript.GetMode() != NemesisMode.Inactive)
            {
                if (isPlayerControl == true)
                {
                    data.itemText = "Nemesis under PLAYER CONTROL";
                    data.topText = "Under Player Command";
                }
                else
                {
                    if (coolDownTimer == 0)
                    {
                        data.itemText = "Nemesis AVAILABLE for Player Control";
                        data.topText = "Nemesis Available";
                    }
                    else
                    {
                        data.itemText = "Nemesis NOT YET available for Player Control";
                        data.topText = "Nemesis Cooldown Period";
                    }
                }
            }
            else
            {
                data.itemText = "Nemesis currently INACTIVE";
                data.topText = "Nemesis Inactive";
            }
            data.bottomText = GameManager.instance.itemDataScript.GetNemesisPlayerOngoingDetails(nemesis, isPlayerControl, coolDownTimer, controlTimer, nodeControl, nodeCurrent);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.instance.guiScript.aiAlertSprite;
            data.tab = ItemTab.Effects;
            data.side = message.side;
            if (nodeControl != null)
            { data.nodeID = nodeControl.nodeID; }
            data.help = 1;
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
    public Message AIConnectionActivity(string text, Node destinationNode, Connection connection, int delay)
    {
        Debug.Assert(destinationNode != null, "Invalid destinationNode (Null)");
        Debug.Assert(connection != null, "Invalid connection (Null)");
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
            message.data0 = destinationNode.nodeID;
            message.data1 = connection.connID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Resistance Leader detected using CONNECTION";
            data.topText = "Connection Activity";
            data.bottomText = GameManager.instance.itemDataScript.GetAIConnectionActivityDetails(destinationNode, delay);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.instance.guiScript.aiAlertSprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.help = 1;
            data.nodeID = destinationNode.nodeID;
            data.connID = connection.connID;
            data.delay = delay;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
    public Message AINodeActivity(string text, Node node, int actorID, int delay)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
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
            message.data0 = node.nodeID;
            message.data1 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Resistance activity detected in DISTRICT";
            data.topText = "Resistance Activity";
            data.bottomText = GameManager.instance.itemDataScript.GetAINodeActivityDetails(node, delay);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.instance.guiScript.aiAlertSprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.delay = delay;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
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
            message.side = globalBoth;
            message.isPublic = true;
            message.displayDelay = delay;
            message.data0 = nodeID;
            message.data1 = GameManager.instance.playerScript.actorID;
            //ItemData
            ItemData data = new ItemData();
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
            { data.itemText = "AI Traceback DETECTS Resistance leader"; }
            else { data.itemText = "AI Traceback DETECTS you"; }
            data.topText = "Detected!";
            data.bottomText = GameManager.instance.itemDataScript.GetAIDetectedDetails(nodeID, delay);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.instance.guiScript.aiCountermeasureSprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = nodeID;
            data.help = 1;
            data.delay = delay;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// AI notification of Player/Actor activity that results in immediateAuthorityFlag/immediateResistanceFlag being set true. 'reason' in format '[due to] ...'
    /// use either nodeID or connID and set the other to -1
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID">Set to -1 if not applicable</param>
    /// <param name="connID">Set to -1 if not applicable</param>
    /// <param name="side"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message AIImmediateActivity(string text, string reason, int nodeID, int connID, int actorID = 999)
    {
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Immediate;
            message.side = globalBoth;
            message.isPublic = true;
            message.data0 = nodeID;
            message.data1 = connID;
            message.data2 = actorID;
            //ItemData
            ItemData data = new ItemData();

            data.topText = "Location Known";
            if (actorID == 999)
            {
                if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                { data.itemText = "AI knows Resistance leader's CURRENT LOCATION"; }
                else { data.itemText = "AI knows of your  CURRENT LOCATION"; }
                data.bottomText = GameManager.instance.itemDataScript.GetAIImmediateActivityDetails(reason, nodeID, connID, GameManager.instance.playerScript.PlayerName, "Player");
            }
            else
            {
                //actor
                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    data.itemText = string.Format("AI knows of {0}, {1}'s, CURRENT LOCATION", actor.actorName, actor.arc.name);
                    data.bottomText = GameManager.instance.itemDataScript.GetAIImmediateActivityDetails(reason, nodeID, connID, actor.actorName, actor.arc.name);
                }
                else
                {
                    Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID);
                    data.bottomText = "Unknown";
                }
            }
            data.priority = ItemPriority.High;
            data.sprite = GameManager.instance.guiScript.aiAlertSprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = nodeID;
            data.connID = connID;
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
    public Message AIHacked(string text, int currentRenownCost, bool isDetected, int attemptsDetected, int attemptsTotal)
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
            data.itemText = "Resistance HACKS AI";
            data.topText = "Hacking Detected";
            data.bottomText = GameManager.instance.itemDataScript.GetAIHackedDetails(isDetected, attemptsDetected, attemptsTotal);
            if (isDetected == true) { data.priority = ItemPriority.Medium; }
            else { data.priority = ItemPriority.Low; }
            data.sprite = GameManager.instance.guiScript.alarmSprite;
            data.tab = ItemTab.ALERTS;
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
            if (rebootTimer > 0) { data.itemText = "AI REBOOTS Commences"; }
            else { data.itemText = "AI REBOOT Completed"; }
            data.topText = "AI Reboots";
            data.bottomText = GameManager.instance.itemDataScript.GetAIRebootDetails(rebootTimer, currentRenownCost);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.instance.guiScript.aiRebootSprite;
            data.tab = ItemTab.ALERTS;
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
    public Message AICounterMeasure(string text, string itemText, string warning, int timerStartValue = -1, int protocolLevelNew = -1)
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
            data.itemText = itemText;
            data.topText = "AI Countermeasures";
            data.bottomText = GameManager.instance.itemDataScript.GetAICounterMeasureDetails(warning, timerStartValue, protocolLevelNew);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.instance.guiScript.aiCountermeasureSprite;
            data.tab = ItemTab.ALERTS;
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
            //add
            GameManager.instance.dataScript.AddMessage(message);
            /*GameManager.instance.dataScript.AddItemData(data);*/
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - Decisions & Policies - - -
    //

    /// <summary>
    /// Either side (Player/AI) makes a global (city wide effect) decision that may, or may not be, public knowledge. Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="decID"></param>
    /// <param name="side"></param>
    /// <param name="isPublic"></param>
    /// <returns></returns>
    public Message DecisionGlobal(string text, string itemText, string description, int decID, int duration = 0, int loyaltyAdjust = 0, int crisisAdjust = 0)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.DECISION;
            message.subType = MessageSubType.Decision_Global;
            message.side = GameManager.instance.globalScript.sideBoth;
            message.isPublic = true;
            message.data0 = decID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = itemText;
            data.topText = "City Wide Decision";
            data.bottomText = GameManager.instance.itemDataScript.GetDecisionGlobalDetails(description, duration, loyaltyAdjust, crisisAdjust);
            data.priority = ItemPriority.Medium;
            data.sprite = mayor.sprite;
            data.tab = ItemTab.ALERTS;
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
    public Message DecisionConnection(string text, Connection connection, ConnectionType secLevel)
    {
        Debug.Assert(connection != null, "Invalid connection (Null)");
        Debug.Assert(secLevel >= 0, string.Format("Invalid secLevel {0}", secLevel));
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.DECISION;
            message.subType = MessageSubType.Decision_Connection;
            message.side = globalBoth;
            message.isPublic = true;
            message.data0 = connection.connID;
            message.data1 = (int)secLevel;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Connection Security changed";
            data.topText = "Connection Security";
            data.bottomText = GameManager.instance.itemDataScript.GetDecisionConnectionDetails(connection, secLevel);
            data.priority = ItemPriority.Medium;
            data.sprite = mayor.sprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.connID = connection.connID;
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
    public Message DecisionRequestResources(string text, string description, GlobalSide side, int amount)
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
            data.bottomText = description;
            data.priority = ItemPriority.Low;
            data.sprite = mayor.sprite;
            data.tab = ItemTab.ALERTS;
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
        /*Debug.Assert(teamID >= 0, string.Format("Invalid teamID {0}", teamID));  NOTE: Causes issues with 'Request Denied, teamID -1' */
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
            data.tab = ItemTab.ALERTS;
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
    /// Ongoing decision effects
    /// </summary>
    /// <param name="text"></param>
    /// <param name="itemText"></param>
    /// <param name="topText"></param>
    /// <param name="bottomText"></param>
    /// <param name="aiDecID"></param>
    /// <returns></returns>
    public Message DecisionOngoingEffect(string text, string itemText, string topText, string middleText, string bottomText, int aiDecID)
    {
        Debug.Assert(aiDecID > -1, "Invalid aiDecID (less than Zeros)");
        Debug.Assert(string.IsNullOrEmpty(itemText) == false, "Invalid itemText (Null or Empty)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Decision;
            message.side = globalBoth;
            message.data0 = aiDecID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = itemText;
            data.topText = "Nemesis Status";
            data.bottomText = GameManager.instance.itemDataScript.GetDecisionOngoingEffectDetails(topText, middleText, bottomText);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.instance.guiScript.aiAlertSprite;
            data.tab = ItemTab.Effects;
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
    public Message TeamAdd(string text, string reason, Team team)
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
            data.itemText = string.Format("{0} team added to Reserve Pool", team.arc.name);
            data.topText = "Team Added";
            data.bottomText = GameManager.instance.itemDataScript.GetAddTeamDetails(team, reason);
            data.priority = ItemPriority.Low;
            data.sprite = team.arc.sprite;
            data.tab = ItemTab.ALERTS;
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
    public Message TeamDeploy(string text, Node node, Team team, Actor actor = null)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(team != null, "Invalid teamID {Null}");
        if (GameManager.instance.sideScript.authorityOverall == SideState.Human)
            if (string.IsNullOrEmpty(text) == false)
            {
                Message message = new Message();
                message.text = text;
                message.type = MessageType.TEAM;
                message.subType = MessageSubType.Team_Deploy;
                message.side = globalAuthority;
                message.data0 = node.nodeID;
                message.data1 = team.teamID;
                if (actor != null)
                {
                    message.data2 = actor.actorID;
                    //ItemData (not if actor = null because this would be an AI action)
                    ItemData data = new ItemData();
                    data.itemText = string.Format("{0} team Deployed", team.arc.name);
                    data.topText = "Team Deployed";
                    data.bottomText = GameManager.instance.itemDataScript.GetTeamDeployDetails(team, node, actor);
                    data.priority = ItemPriority.Low;
                    data.sprite = team.arc.sprite;
                    data.tab = ItemTab.ALERTS;
                    data.side = message.side;
                    data.nodeID = node.nodeID;
                    data.help = 1;
                    //add
                    GameManager.instance.dataScript.AddItemData(data);
                }
                //add
                GameManager.instance.dataScript.AddMessage(message);

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
    public Message TeamAutoRecall(string text, Node node, Team team, Actor actor = null)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(team != null, "Invalid team (Null)");
        if (GameManager.instance.sideScript.authorityOverall == SideState.Human)
            if (string.IsNullOrEmpty(text) == false)
            {
                Message message = new Message();
                message.text = text;
                message.type = MessageType.TEAM;
                message.subType = MessageSubType.Team_AutoRecall;
                message.side = globalAuthority;
                message.isPublic = true;
                message.displayDelay = 0;
                message.data0 = node.nodeID;
                message.data1 = team.teamID;
                if (actor != null)
                {
                    message.data2 = actor.actorID;
                    //ItemData (not if actor = null as this would be an AI action)
                    ItemData data = new ItemData();
                    data.itemText = string.Format("{0} team AutoRecalled", team.arc.name);
                    data.topText = "Team Autorecalled";
                    data.bottomText = GameManager.instance.itemDataScript.GetTeamAutoRecallDetails(node, team, actor);
                    data.priority = ItemPriority.Low;
                    data.sprite = team.arc.sprite;
                    data.tab = ItemTab.ALERTS;
                    data.side = message.side;
                    data.nodeID = node.nodeID;
                    data.help = 1;
                    GameManager.instance.dataScript.AddItemData(data);
                }
                //add
                GameManager.instance.dataScript.AddMessage(message);
            }
            else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Team is withdrawn early by player. Returns null if text invalid. 'Reason' is self contained
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="teamID"></param>
    /// <returns></returns>
    public Message TeamWithdraw(string text, string reason, Node node, Team team, Actor actor)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(team != null, "Invalid team (Null)");
        Debug.Assert(actor != null, "Invalid actor (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Withdraw;
            message.side = globalAuthority;
            message.data0 = node.nodeID;
            message.data1 = team.teamID;
            message.data2 = actor.actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} team Withdrawn", team.arc.name);
            data.topText = "Team Withdrawn";
            data.bottomText = GameManager.instance.itemDataScript.GetTeamWithdrawDetails(reason, team, node, actor);
            data.priority = ItemPriority.Low;
            data.sprite = team.arc.sprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Team carries out it's permanent effect when auto withdrawn from node. 'EffectText' is assumed to be colour Formatted depending on good/neutral/bad by originating method
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="teamID"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public Message TeamEffect(string text, string itemText, string effectText, Node node, Team team)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(team != null, "Invalid team (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Effect;
            message.side = globalAuthority;
            message.isPublic = true;
            message.data0 = node.nodeID;
            message.data1 = team.teamID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = itemText;
            data.topText = "Team Outcome";
            data.bottomText = GameManager.instance.itemDataScript.GetTeamEffectDetails(effectText, node, team);
            data.priority = ItemPriority.Medium;
            data.sprite = team.arc.sprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
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
    public Message TeamNeutralise(string text, Node node, Team team, Actor actor)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(team != null, "Invalid team (Null)");
        Debug.Assert(actor != null, "Invalid actor (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Neutralise;
            message.side = globalBoth;
            message.isPublic = true;
            message.displayDelay = 0;
            message.data0 = node.nodeID;
            message.data1 = team.teamID;
            message.data2 = actor.actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} team NEUTRALISED by Resistance", team.arc.name);
            data.topText = "Team Neutralised";
            data.bottomText = GameManager.instance.itemDataScript.GetTeamNeutraliseDetails(node, team);
            data.priority = ItemPriority.High;
            data.sprite = team.arc.sprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
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
            data.tab = ItemTab.ALERTS;
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
            data.tab = ItemTab.ALERTS;
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
            data.tab = ItemTab.ALERTS;
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
            data.tab = ItemTab.ALERTS;
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
            data.tab = ItemTab.ALERTS;
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
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
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
    /// A new, LIVE target, pops up. Both sides notified
    /// </summary>
    /// <param name="text"></param>
    /// <param name="node"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public Message TargetNew(string text, Node node, Target target)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(target != null, "Invalid target (Null)");
        Debug.Assert(target.targetStatus == Status.Live, "Target status NOT Live");
        if (string.IsNullOrEmpty(text) == false)
        {
            string sideText;
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TARGET;
            message.subType = MessageSubType.Target_New;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.data0 = node.nodeID;
            message.data1 = target.timerWindow;
            message.data2 = target.targetID;
            //ItemData
            ItemData data = new ItemData();
            if(message.side.level == globalResistance.level)
            {
                //resistance player
                data.itemText = string.Format("Rebel HQ have identified an OPPORTUNITY at {0}", node.nodeName);
                data.topText = "Target Opportunity";
                sideText = "Opportunity";
            }
            else
            {
                //authority player
                data.itemText = string.Format("{0} has identified a vulnerability at {1}", tagAIName, node.nodeName);
                data.topText = "Target Vulnerability";
                sideText = "Vulnerability";
            }
            data.bottomText = GameManager.instance.itemDataScript.GetTargetNewDetails(node, target, sideText, message.side);
            data.priority = ItemPriority.Medium;
            data.sprite = target.sprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;          
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// A target not yet successfully attempted, has expired (timed out)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="node"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public Message TargetExpired(string text, Node node, Target target)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(target != null, "Invalid target (Null)");
        Debug.Assert(target.targetStatus == Status.Live, "Target status NOT Live");
        if (string.IsNullOrEmpty(text) == false)
        {
            string sideText;
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TARGET;
            message.subType = MessageSubType.Target_Expired;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.data0 = node.nodeID;
            message.data1 = target.numOfAttempts;
            message.data2 = target.targetID;
            //ItemData
            ItemData data = new ItemData();
            if (message.side.level == globalResistance.level)
            {
                //resistance player
                data.itemText = string.Format("Window of Opportunity at {0} has CLOSED", node.nodeName);
                data.topText = "Target Gone";
                sideText = "Opportunity";
            }
            else
            {
                //authority player
                data.itemText = string.Format("Vulnerability at {0} has been RESOLVED", node.nodeName);
                data.topText = "Vulnerability Closed";
                sideText = "Vulnerability";
            }
            data.bottomText = GameManager.instance.itemDataScript.GetTargetExpiredDetails(node, target, sideText, message.side);
            data.priority = ItemPriority.Medium;
            data.sprite = target.sprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Target about to expire. Warning message to Resistance Player only
    /// </summary>
    /// <param name="text"></param>
    /// <param name="node"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public Message TargetExpiredWarning(string text, Node node, Target target)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(target != null, "Invalid target (Null)");
        Debug.Assert(target.targetStatus == Status.Live, "Target status NOT Live");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TARGET;
            message.subType = MessageSubType.Target_Expired;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.data0 = node.nodeID;
            message.data1 = target.timerWindow;
            message.data2 = target.targetID;
            //ItemData
            ItemData data = new ItemData();
            //resistance player only
            data.itemText = string.Format("Window of Opportunity at {0} CLOSING SOON", node.nodeName);
            data.topText = "Target Warning";
            data.bottomText = GameManager.instance.itemDataScript.GetTargetExpiredWarningDetails(node, target);
            data.priority = ItemPriority.Medium;
            data.sprite = target.sprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    /// <summary>
    /// Target has been attempted successfully, or not, by an actor ('999' for player), at a node. Returns null if text invalid.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="actorID"></param>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public Message TargetAttempt(string text, Node node, int actorID, Target target)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID {0}", actorID));
        Debug.Assert(target != null, "Invalid target (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TARGET;
            message.subType = MessageSubType.Target_Attempt;
            message.side = globalResistance;
            message.data0 = node.nodeID;
            message.data1 = actorID;
            message.data2 = target.targetID;
            //ItemData, resistance only
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
            {
                ItemData data = new ItemData();
                if (target.targetStatus == Status.Live)
                {
                    data.itemText = "TARGET attempt Failed";
                    data.priority = ItemPriority.Low;
                }
                else
                {
                    data.itemText = "TARGET attempt SUCCEEDED";
                    data.priority = ItemPriority.Medium;
                }
                data.topText = "Target attempt";
                data.bottomText = GameManager.instance.itemDataScript.GetTargetAttemptDetails(node, actorID, target);
                data.sprite = target.sprite;
                data.tab = ItemTab.ALERTS;
                data.side = message.side;
                data.nodeID = node.nodeID;
                data.help = 1;
                //add
                GameManager.instance.dataScript.AddMessage(message);
                GameManager.instance.dataScript.AddItemData(data);
            }
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
    public Message TargetContained(string text, Node node, Team team, Target target)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(team != null, "Invalid team (Null)");
        Debug.Assert(target != null, "Invalid target (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TARGET;
            message.subType = MessageSubType.Target_Contained;
            message.side = globalBoth;
            message.data0 = node.nodeID;
            message.data1 = team.teamID;
            message.data2 = target.targetID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} target CONTAINED", target.targetName);
            data.topText = "Target Contained";
            data.bottomText = GameManager.instance.itemDataScript.GetTargetContainedDetails(node, team, target);
            data.priority = ItemPriority.Medium;
            data.sprite = target.sprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    //
    // - - - Effects (Ongoing & Active) - - -
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
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Created;
            message.side = globalBoth;
            message.data0 = nodeID;
            //add
            GameManager.instance.dataScript.AddMessage(message);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// current ongoing effect District -> InfoApp 'Effect' tab
    /// </summary>
    /// <param name="text"></param>
    /// <param name="ongoing"></param>
    public Message MessageOngoingEffectCurrentNode(EffectDataOngoing ongoing)
    {
        Debug.Assert(ongoing != null, "Invalid EffectDataOngoing (Null)");
        Debug.Assert(ongoing.node != null, "Invalid Ongoing.node (Null)");
        if (string.IsNullOrEmpty(ongoing.text) == false)
        {
            Message message = new Message();
            message.text = ongoing.text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Current;
            message.side = globalBoth;
            message.data0 = ongoing.node.nodeID;
            message.data1 = ongoing.timer;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0}, {1} district, ONGOING EFFECT", ongoing.node.nodeName, ongoing.node.Arc.name);
            data.topText = "District Ongoing Effect";
            data.bottomText = GameManager.instance.itemDataScript.GetOngoingEffectDetails(ongoing);
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.ongoingEffectSprite;
            data.tab = ItemTab.Effects;
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
    /// current ongoing effect Gear -> InfoApp 'Effect' tab
    /// </summary>
    /// <param name="text"></param>
    /// <param name="ongoing"></param>
    public Message MessageOngoingEffectCurrentGear(EffectDataOngoing ongoing)
    {
        Debug.Assert(ongoing != null, "Invalid EffectDataOngoing (Null)");
        if (string.IsNullOrEmpty(ongoing.text) == false)
        {
            Message message = new Message();
            message.text = ongoing.text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Current;
            message.side = globalBoth;
            message.data0 = ongoing.gearID;
            message.data1 = ongoing.timer;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0}, ONGOING EFFECT", ongoing.gearName);
            data.topText = "Gear Ongoing Effect";
            data.bottomText = GameManager.instance.itemDataScript.GetOngoingEffectDetails(ongoing);
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.instance.guiScript.ongoingEffectSprite;
            data.tab = ItemTab.Effects;
            data.side = message.side;
            data.nodeID = ongoing.node.nodeID;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    /// <summary>
    /// Ongoing effect that timed out (expired) automatically or has been shut down. DataID could be node / conn / gearID
    /// </summary>
    /// <param name="text"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public Message OngoingEffectExpired(string text, int dataID)
    {
        Debug.Assert(dataID > -1, "Invalid dataID (less than Zero)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Expired;
            message.side = globalBoth;
            message.isPublic = true;
            message.data0 = dataID;
            //add
            GameManager.instance.dataScript.AddMessage(message);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Special message used for InfoApp active 'Effects' tab. Usually either actor (actorID 999 for player) OR node based. Itemtext is text.
    /// </summary>
    /// <param name="itemData"></param>
    /// <param name="detailsTop"></param>
    /// <param name="detailsBottom"></param>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public Message ActiveEffect(string text, string topText, string detailsTop, string detailsBottom, Sprite sprite, int actorID = -1, Node node = null)
    {
        Debug.Assert(sprite != null, "Invalid spirte (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTIVE;
            message.subType = MessageSubType.Active_Effect;
            message.side = GameManager.instance.sideScript.PlayerSide;
            message.isPublic = true;
            if (node != null) { message.data0 = node.nodeID; }
            if (actorID > -1) { message.data1 = actorID; }
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = topText;
            data.bottomText = GameManager.instance.itemDataScript.GetActiveEffectDetails(detailsTop, detailsBottom, actorID, node);
            data.priority = ItemPriority.Low;
            data.sprite = sprite;
            data.tab = ItemTab.Effects;
            data.side = message.side;
            if (node != null)
            { data.nodeID = node.nodeID; }
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
            message.side = faction.side;
            message.isPublic = true;
            message.data0 = factionApprovalLevel;
            message.data1 = playerRenownBefore;
            message.data2 = supportGiven;
            //ItemData
            ItemData data = new ItemData();
            if (supportGiven > 0)
            {
                //support Approved
                data.itemText = string.Format("Support request to {0} APPROVED", faction.name);
                data.topText = "Support APPROVED";
            }
            else
            {
                //support Declined
                data.itemText = string.Format("Support request to {0} declined", faction.name);
                data.topText = "Support Declined";
            }
            data.bottomText = GameManager.instance.itemDataScript.GetFactionSupportDetails(faction, factionApprovalLevel, supportGiven);
            //high priority if given
            if (supportGiven > 0) { data.priority = ItemPriority.High; }
            else { data.priority = ItemPriority.Medium; }
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.sprite = faction.sprite;
            data.help = 1;
            data.tag0 = "fact_supp_0";
            data.tag1 = "fact_supp_1";
            data.tag2 = "fact_supp_2";
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
            message.side = faction.side;
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
            data.tab = ItemTab.ALERTS;
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
    /// Node undergoes a crisis (commences, averted, finishes). 'reductionInCityLoyalty' is for when crisis explodes and loyalty drops
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public Message NodeCrisis(string text, string itemText, Node node, int reductionInCityLoyalty = -1)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(string.IsNullOrEmpty(itemText) == false, "Invalid msgText (Null or Empty)");
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
            data.itemText = itemText;
            data.topText = "District Crisis";
            data.bottomText = GameManager.instance.itemDataScript.GetNodeCrisisDetails(node, reductionInCityLoyalty);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.instance.guiScript.nodeCrisisSprite;
            data.tab = ItemTab.ALERTS;
            data.side = message.side;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.instance.dataScript.AddMessage(message);
            GameManager.instance.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    public Message NodeOngoingEffect(string text, Node node)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Node;
            message.side = globalBoth;
            message.isPublic = true;
            message.data0 = node.nodeID;
            message.data1 = node.waitTimer;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0}, {1}, district temporarily IMMUNE to CRISIS", node.nodeName, node.Arc.name);
            data.topText = "District Immunity";
            data.bottomText = GameManager.instance.itemDataScript.GetNodeOngoingEffectDetails(node);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.instance.guiScript.nodeCrisisSprite;
            data.tab = ItemTab.Effects;
            data.side = message.side;
            data.nodeID = node.nodeID;
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
    public Message CityLoyalty(string text, string reason, int newCityLoyalty, int changeInLoyalty)
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
            if (changeInLoyalty < 0) { data.itemText = string.Format("City Loyalty DECREASES"); }
            else { data.itemText = string.Format("City Loyalty INCREASES"); }
            data.topText = "City Loyalty";
            data.bottomText = GameManager.instance.itemDataScript.GetCityLoyaltyDetails(reason, newCityLoyalty, changeInLoyalty);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.instance.guiScript.cityLoyaltySprite;
            data.tab = ItemTab.ALERTS;
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
