﻿using gameAPI;
using packageAPI;
using System;
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

    #region Save Compatible Data
    [HideInInspector] public int messageIDCounter = 0;                                          //messageID counter
    #endregion

    #region InitialseEarly

    /// <summary>
    /// Set up at start
    /// </summary>
    public void InitialiseEarly(GameState state)
    {
        //Run in GameManager.cs -> listOfGlobalMethods with GameState.StartUp so doesn't need check for GameState.NewInitialisation
        playerActorID = GameManager.i.playerScript.actorID;
        playerSprite = GameManager.i.playerScript.sprite;
        Debug.Assert(playerActorID > -1, "Invalid playerActorID (-1)");
        Debug.Assert(playerSprite != null, "Invalid playerSprite (Null)");

        globalResistance = GameManager.i.globalScript.sideResistance;
        globalAuthority = GameManager.i.globalScript.sideAuthority;
        globalBoth = GameManager.i.globalScript.sideBoth;
        tagAIName = GameManager.i.globalScript.tagGlobalAIName;

        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalBoth != null, "Invalid globalBoth (Null)");
        Debug.Assert(string.IsNullOrEmpty(tagAIName) == false, "Invalid tagAIName (Null or Empty)");

        //event Listeners
        EventManager.i.AddListener(EventType.StartTurnEarly, OnEvent, "MessageManager");
        EventManager.i.AddListener(EventType.EndTurnEarly, OnEvent, "MessageManager");
    }
    #endregion

    /// <summary>
    /// needed due to gameManager initialisation sequence
    /// NOTE: Not for GameState.LoadGame
    /// </summary>
    public void InitialiseLate(GameState state)
    {
        switch (GameManager.i.inputScript.GameState)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
            case GameState.FollowOnInitialisation:
            case GameState.LoadAtStart:
                SubInitialiseAllLate();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseAllLate
    private void SubInitialiseAllLate()
    {
        mayor = GameManager.i.cityScript.GetCity().mayor;
        Debug.Assert(mayor != null, "Invalid mayor (Null)");
    }
    #endregion

    #endregion

    #region ResetCounter
    /// <summary>
    /// Reset message ID counter prior to a new level
    /// </summary>
    public void ResetCounter()
    { messageIDCounter = 0; }
    #endregion

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


    #region StartTurnEarly
    /// <summary>
    /// Checks pending messages, decrements delay timers and moves any with a zero timer to Current messages.
    /// </summary>
    private void StartTurnEarly()
    {
        //
        // - - - Messages - - -
        //
        Dictionary<int, Message> dictOfPendingMessages = new Dictionary<int, Message>(GameManager.i.dataScript.GetMessageDict(MessageCategory.Pending));
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
                    { GameManager.i.dataScript.AIMessage(message.Value); }
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
                GameManager.i.dataScript.MoveMessage(listOfMessagesToMove[i], MessageCategory.Pending, MessageCategory.Current);
            }
        }
        else { Debug.LogError("Invalid dictOfPendingMessages (Null)"); }
        //
        // - - - ItemData - - -
        //
        List<ItemData> listOfDelayedItemData = GameManager.i.dataScript.GetListOfDelayedItemData();
        if (listOfDelayedItemData != null)
        {
            for (int index = listOfDelayedItemData.Count - 1; index >= 0; index--)
            {
                ItemData data = listOfDelayedItemData[index];
                //decrement delay
                data.delay--;
                //ready for prime time?
                if (data.delay <= 0)
                {
                    //add to display list
                    GameManager.i.dataScript.AddItemData(data);
                    //remove from delay list
                    listOfDelayedItemData.RemoveAt(index);
                }
            }
        }
        else { Debug.LogWarning("Invalid listOfDelayItemData (Null)"); }
    }
    #endregion


    #region EndTurn
    /// <summary>
    /// checks current messages, moves all of them to Archive messages
    /// </summary>
    private void EndTurn()
    {
        Dictionary<int, Message> dictOfCurrentMessages = GameManager.i.dataScript.GetMessageDict(MessageCategory.Current);
        if (dictOfCurrentMessages != null)
        {
            List<int> listOfMessagesToMove = new List<int>();
            foreach (var record in dictOfCurrentMessages)
            { listOfMessagesToMove.Add(record.Key); }
            //loop list and move all specified messages to the Archive dictionary for display
            for (int i = 0; i < listOfMessagesToMove.Count; i++)
            { GameManager.i.dataScript.MoveMessage(listOfMessagesToMove[i], MessageCategory.Current, MessageCategory.Archive); }
            //error check
            if (dictOfCurrentMessages.Count > 0)
            { Debug.LogError(string.Format("There are {0} records in dictOfCurrentMethods (should be empty)", dictOfCurrentMessages.Count)); }
        }
        else { Debug.LogError("Invalid dictOfCurrentMessages (Null)"); }
    }
    #endregion


    //
    // - - - New Message Methods
    //

    //
    // - - - General - - - 
    //

    /// <summary>
    /// General Info message. 'itemText' is itemData.text, 'reason' is a self-contained sentence, 'warning' is a self-contained and is shown in Red if 'isBad' true, otherwise Green
    /// 'topText' is RHS short tag. Default playerSide, Medium priority.
    /// ListOfHelpTags (optional) allows you to provide a list of helptag strings that will be used for RHS help, eg. "info_1", "conflict_0". Max of 4 tags allowed, rest ignored.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public Message GeneralInfo(string text, string itemText, string topText, string reason, string explanation, bool isBad = true, List<string> listOfHelpTags = null)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GENERAL;
            message.subType = MessageSubType.General_Info;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = itemText;
            data.topText = topText;
            data.bottomText = GameManager.i.itemDataScript.GetGeneralInfoDetails(reason, explanation, isBad);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.i.spriteScript.alertInformationSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            if (listOfHelpTags != null)
            {
                for (int i = 0; i < listOfHelpTags.Count; i++)
                {
                    switch (i)
                    {
                        case 0: data.tag0 = listOfHelpTags[i]; break;
                        case 1: data.tag1 = listOfHelpTags[i]; break;
                        case 2: data.tag2 = listOfHelpTags[i]; break;
                        case 3: data.tag3 = listOfHelpTags[i]; break;
                        default: Debug.LogWarningFormat("Invalid help tag \"{0}\" (can only have 4, there are {1})", listOfHelpTags[i], i); break;
                    }
                }
                data.help = 1;
            }
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    /// <summary>
    /// General Warning message. 'itemText' is itemData.text, 'reason' is a self-contained sentence, 'warning' is a self-contained (shown Red is 'isBad' true, otherwise Green)
    /// 'isHighPriority' if true, Medium priority othewise. 'topText' is RHS short tag. Default playerSide. 
    /// ListOfHelpTags (optional) allows you to provide a list of helptag strings that will be used for RHS help, eg. "info_1", "conflict_0". Max of 4 tags allowed, rest ignored.
    /// NOTE: Don't put any formatting (colour or extra linefeeds) in 'warning'
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public Message GeneralWarning(string text, string itemText, string topText, string reason, string warning, bool isHighPriority = true, bool isBad = true, List<string> listOfHelpTags = null)
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = itemText;
            data.topText = topText;
            data.bottomText = GameManager.i.itemDataScript.GetGeneralWarningDetails(reason, warning, isBad);
            if (isHighPriority == true)
            { data.priority = ItemPriority.High; }
            else { data.priority = ItemPriority.Medium; }
            data.sprite = GameManager.i.spriteScript.alertWarningSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            if (listOfHelpTags != null)
            {
                for (int i = 0; i < listOfHelpTags.Count; i++)
                {
                    switch (i)
                    {
                        case 0: data.tag0 = listOfHelpTags[i]; break;
                        case 1: data.tag1 = listOfHelpTags[i]; break;
                        case 2: data.tag2 = listOfHelpTags[i]; break;
                        case 3: data.tag3 = listOfHelpTags[i]; break;
                        default: Debug.LogWarningFormat("Invalid help tag \"{0}\" (can only have 4, there are {1})", listOfHelpTags[i], i); break;
                    }
                }
                data.help = 1;
            }
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Random roll results (only ones that matter, don't spam). Auto player side. Format text as "HQ support Declined" with NO need, rolled, etc. 
    /// Set 'isReversed' to true if success means a bad outcome, eg. in case of Gear Compromise check a success is failing the roll
    /// 'typeOfCheck' is InfoApp RHS header in format '... Check', eg. 'Compromise'. Keep short
    /// help is to add an additional help tag explaining the individual item, if needed, ignored otherwise
    /// </summary>
    /// <param name="text"></param>
    /// <param name="numNeeded"></param>
    /// <param name="numRolled"></param>
    /// <returns></returns>
    public void GeneralRandom(string text, string typeOfCheck, int numNeeded, int numRolled, bool isReversed = false, string help = "")
    {
        Debug.Assert(string.IsNullOrEmpty(typeOfCheck) == false, "Invalid typeOfCheck (Null or Empty)");
        if (string.IsNullOrEmpty(text) == false)
        {
            //Message
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GENERAL;
            message.subType = MessageSubType.General_Random;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = numNeeded;
            message.data1 = numRolled;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = GameManager.i.itemDataScript.GetRandomTopText(typeOfCheck, isReversed);
            data.bottomText = GameManager.i.itemDataScript.GetRandomDetails(numNeeded, numRolled, isReversed);
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.i.spriteScript.alertRandomSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Random;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            if (string.IsNullOrEmpty(help) == true)
            {
                data.tag0 = "roll_0";
                data.tag1 = "roll_1";
                data.tag2 = "roll_2";
            }
            else
            {
                data.tag0 = help;
                data.tag1 = "roll_1";
                data.tag2 = "roll_2";
            }
            //add (message only if a meaningful outcome)
            if (isReversed == false)
            { if (numRolled >= numNeeded) { GameManager.i.dataScript.AddMessage(message); } }
            else { if (numRolled < numNeeded) { GameManager.i.dataScript.AddMessage(message); } }

            GameManager.i.dataScript.AddItemData(data);
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
    public Message PlayerMove(string text, Node node, int changeInvisibility = 0, int aiDelay = 0, bool isStart = false)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Move;
            message.sideLevel = globalResistance.level;
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
            data.bottomText = GameManager.i.itemDataScript.GetPlayerMoveDetails(node, changeInvisibility, aiDelay);
            data.priority = ItemPriority.Low;
            data.sprite = node.Arc.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.isDisplay = false;     //DEBUG
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.dataName = secret.name;
            message.isPublic = true;
            //ItemData
            ItemData data = new ItemData();
            data.topText = secret.tag;
            data.bottomText = GameManager.i.itemDataScript.GetPlayerSecretDetails(secret, isGained);
            if (isGained == true)
            { data.itemText = "Player gains a Secret"; }
            else
            { data.itemText = "Player loses a Secret"; }
            data.priority = ItemPriority.Low;
            data.sprite = playerSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "secret_0";
            data.tag1 = "secret_1";
            data.tag2 = "secret_2";
            data.tag3 = "secret_3";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Player escapes from incarceration with help of OrgEmergency
    /// </summary>
    /// <param name="text"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public Message PlayerEscapes(string text, Node node)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Escapes;
            message.data0 = node.nodeID;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            //ItemData
            ItemData data = new ItemData();
            data.topText = "Player Escapes";
            data.bottomText = GameManager.i.itemDataScript.GetPlayerEscapesDetails(node);
            data.itemText = string.Format("{0}, has ESCAPED", GameManager.i.playerScript.PlayerName);
            data.priority = ItemPriority.High;
            data.sprite = playerSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "questionable_0";
            data.tag1 = "questionable_1";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Player Power expended. 'dataID' is multipurpose (gearID if compromised, etc.) defaults to Player side. 'Reason' is a short text tag for ItemData giving a reason why Power was used. format 'to ....'
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="dataID"></param>
    /// <returns></returns>
    public Message PowerUsedPlayer(string text, int amount, int dataID, int nodeID = -1)
    {
        Debug.Assert(dataID >= 0, string.Format("Invalid dataID {0}", dataID));
        Debug.Assert(amount > 0, "Invalid amount (<= 0)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Power;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = amount;
            message.data1 = dataID;
            message.data2 = nodeID;
            //add
            GameManager.i.dataScript.AddMessage(message);
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
            if (GameManager.i.sideScript.PlayerSide.level == globalAuthority.level)
            { isResistance = false; }
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Damage;
            message.sideLevel = globalBoth.level;
            message.data0 = nodeID;
            //ItemData
            ItemData data = new ItemData();
            data.topText = "Nemesis Attacks";
            data.bottomText = GameManager.i.itemDataScript.GetPlayerDamageDetails(damageInfo, damageEffect, isResistance);
            if (isResistance)
            { data.itemText = "You have been Damaged by your NEMESIS"; }
            else { data.itemText = string.Format("{0} has been Damaged by their NEMESIS", GameManager.i.playerScript.GetPlayerNameResistance()); }
            data.priority = ItemPriority.Low;
            data.sprite = playerSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = node.nodeID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "";
            data.bottomText = GameManager.i.itemDataScript.GetPlayerSpottedDetails(detailsTop, detailsBottom, node);
            data.priority = ItemPriority.High;
            data.sprite = playerSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            if (node != null)
            { data.nodeID = node.nodeID; }
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
            message.isPublic = true;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "You have been BETRAYED";
            data.topText = "Information Leaked";
            data.bottomText = GameManager.i.itemDataScript.GetPlayerBetrayedDetails(isImmediate);
            data.priority = ItemPriority.High;
            data.sprite = playerSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "betrayal_0";
            data.tag1 = "betrayal_1";
            data.tag2 = "betrayal_2";
            data.tag3 = "betrayal_3";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Player undergoes a mood change
    /// </summary>
    /// <param name="detailsTop"></param>
    /// <param name="detailsBottom"></param>
    /// <param name="change"></param>
    /// <param name="mood"></param>
    /// <returns></returns>
    public Message PlayerMoodChange(string details, int change, int moodAfterChange, bool isStressed)
    {
        Debug.Assert(string.IsNullOrEmpty(details) == false, "Invalid detailsTop (Null or Empty)");
        Debug.Assert(change != 0, "Invalid change (Zero)");
        string text = "Unknown";
        string topText = "Unknown";
        if (change > 0) { text = "Player's MOOD has IMPROVED"; topText = "Mood Improves"; }
        else { text = "Player's MOOD has WORSENED"; topText = "Mood Worsens"; }
        Message message = new Message();
        message.text = text;
        message.type = MessageType.PLAYER;
        message.subType = MessageSubType.Plyr_Mood;
        message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
        message.isPublic = true;
        message.data0 = change;
        message.data1 = moodAfterChange;
        //ItemData
        ItemData data = new ItemData();
        data.itemText = text;
        data.topText = topText;
        data.bottomText = GameManager.i.itemDataScript.GetPlayerMoodChangeDetails(details, change, moodAfterChange, isStressed);
        data.priority = ItemPriority.Low;
        data.sprite = playerSprite;
        data.spriteName = data.sprite.name;
        data.tab = ItemTab.ALERTS;
        data.type = message.type;
        data.subType = message.subType;
        data.sideLevel = message.sideLevel;
        data.help = 1;
        data.tag0 = "mood_0";
        data.tag1 = "mood_1";
        data.tag2 = "mood_2";
        data.tag3 = "mood_3";
        //add
        GameManager.i.dataScript.AddMessage(message);
        GameManager.i.dataScript.AddItemData(data);
        return null;
    }

    /// <summary>
    /// Player is immune to stress due to taking a dose of illegal drugs (shown when player takes drug). May, or may not, be addicted
    /// </summary>
    /// <param name="details"></param>
    /// <param name="immunePeriodCurrent"></param>
    /// <param name="immunePeriodStart"></param>
    /// <returns></returns>
    public Message PlayerImmuneStart(string text, int immunePeriodCurrent, int immunePeriodStart, bool isAddicted)
    {
        Debug.AssertFormat(immunePeriodCurrent == immunePeriodStart, "Mismatch between immunePeriodCurrent {0} and immunePeriodStart {1} (should be equal)", immunePeriodCurrent, immunePeriodStart);
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Immune;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = immunePeriodCurrent;
            message.data1 = immunePeriodStart;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Taking drugs has given you an IMMUNITY to Stress";
            data.topText = "Stress Immunity";
            data.bottomText = GameManager.i.itemDataScript.GetPlayerImmuneStartDetails(immunePeriodCurrent, isAddicted);
            data.priority = ItemPriority.Medium;
            data.sprite = playerSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "immune_0";
            data.tag1 = "immune_1";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    /// <summary>
    /// Player did NOT become stressed due to taking a dose of illegal drugs. May, or may not, be addicted
    /// </summary>
    /// <param name="details"></param>
    /// <param name="immunePeriodCurrent"></param>
    /// <param name="immunePeriodStart"></param>
    /// <returns></returns>
    public Message PlayerImmuneStress(string text, int immunePeriodCurrent, int immunePeriodStart, bool isAddicted)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Immune;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = immunePeriodCurrent;
            message.data1 = immunePeriodStart;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Drugs prevented you from becoming STRESSED";
            data.topText = "Stress Resistance";
            data.bottomText = GameManager.i.itemDataScript.GetPlayerImmuneStressDetails(immunePeriodCurrent, isAddicted);
            data.priority = ItemPriority.Medium;
            data.sprite = playerSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "immune_0";
            data.tag1 = "immune_1";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Effects tab for immunity from stress period for Player. May, or may not, be addicted
    /// </summary>
    /// <param name="details"></param>
    /// <param name="immunePeriodCurrent"></param>
    /// <param name="immunePeriodStart"></param>
    /// <returns></returns>
    public Message PlayerImmuneEffect(string text, int immunePeriodCurrent, int immunePeriodStart, bool isAddicted)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Immune;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = immunePeriodCurrent;
            message.data1 = immunePeriodStart;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "You are IMMUNE to STRESS";
            data.topText = "Stress Immunity";
            data.bottomText = GameManager.i.itemDataScript.GetPlayerImmuneEffectDetails(immunePeriodCurrent, isAddicted);
            data.priority = ItemPriority.Low;
            data.sprite = playerSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "immune_0";
            data.tag1 = "immune_1";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Addicted player has to feed their need with either Power or HQ Approval
    /// </summary>
    /// <param name="text"></param>
    /// <param name="powerCost"></param>
    /// <param name="hqApprovalCost"></param>
    /// <returns></returns>
    public Message PlayerAddicted(string text, int powerCost, int hqApprovalCost, int currentImmuneDays)
    {
        Debug.Assert(powerCost > 0 || hqApprovalCost > 0, "Invalid powerCost and hqApprovalCost (one must be > Zero)");
        Debug.AssertFormat(currentImmuneDays > -1, "Invalid currentImmuneDays {0} (should be Zero or above)", currentImmuneDays);
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Addicted;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = powerCost;
            message.data1 = hqApprovalCost;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "You FEED your drug ADDICTION";
            data.topText = "Feed the Need";
            data.bottomText = GameManager.i.itemDataScript.GetPlayerAddictedDetails(powerCost, hqApprovalCost, currentImmuneDays);
            data.priority = ItemPriority.Medium;
            data.sprite = playerSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "addict_0";
            data.tag1 = "addict_1";
            data.tag2 = "addict_2";
            data.tag3 = "addict_3";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// A cure is available (shown when first occurs) for a condition that the Player has
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="conditionName"></param>
    /// <returns></returns>
    public Message PlayerCureStatus(string text, Node node, Condition condition, bool isActivated)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(condition != null, "Invalid condition (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Plyr_Cure;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = node.nodeID;
            message.data1 = Convert.ToInt32(isActivated);
            message.dataName = condition.name;
            //ItemData
            ItemData data = new ItemData();
            if (isActivated == true)
            { data.itemText = string.Format("Cure available for your {0} condition", condition.tag); }
            else { data.itemText = string.Format("{0} Cure NO LONGER available", condition.tag); }
            data.topText = string.Format("{0} Cure", condition.tag);
            data.bottomText = GameManager.i.itemDataScript.GetPlayerCureDetails(node, condition, isActivated);
            data.priority = ItemPriority.Medium;
            data.sprite = playerSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.tag0 = "cure_0";
            data.tag1 = "cure_1";
            data.tag2 = "cure_2";
            data.tag3 = "cure_3";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
    /// HelpType enum determines which set of Help messages are shown for the help tooltip, default None.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="isPublic"></param>
    /// <returns></returns>
    public Message ActorStatus(string text, string itemTextTag, string reason, int actorID, GlobalSide side, string details = null, HelpType help = HelpType.None)
    {
        Debug.Assert(actorID >= 0, string.Format("Invalid actorID ({0})", actorID));
        Debug.Assert(side != null, "Invalid side (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.subType = MessageSubType.Actor_Status;
            message.sideLevel = side.level;
            message.isPublic = true;
            message.data0 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.topText = "Status Change";
            data.priority = ItemPriority.Low;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            //data depends on whether an actor or player
            if (actorID == playerActorID)
            {
                message.type = MessageType.PLAYER;
                data.itemText = string.Format("{0}, Player, {1}", GameManager.i.playerScript.PlayerName, itemTextTag);
                data.sprite = playerSprite;
                data.spriteName = data.sprite.name;
                data.bottomText = GameManager.i.itemDataScript.GetActorStatusDetails(reason, details, null);
            }
            else
            {
                //actor
                Actor actor = GameManager.i.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    message.type = MessageType.ACTOR;
                    data.itemText = string.Format("{0}, {1}, {2}", actor.actorName, actor.arc.name, itemTextTag);
                    data.sprite = actor.sprite;
                    data.spriteName = data.sprite.name;
                    data.bottomText = GameManager.i.itemDataScript.GetActorStatusDetails(reason, details, actor);
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            data.help = 0;
            if (help != HelpType.None)
            {
                data.help = 1;
                switch (help)
                {
                    case HelpType.PlayerBreakdown:
                        data.tag0 = "stress_0";
                        data.tag1 = "stress_1";
                        data.tag2 = "stress_2";
                        data.tag3 = "stress_3";
                        break;
                    case HelpType.StressLeave:
                        data.tag0 = "stressLeave_0";
                        data.tag1 = "stressLeave_1";
                        data.tag2 = "stressLeave_2";
                        data.tag3 = "stressLeave_3";
                        break;
                    case HelpType.LieLow:
                        data.tag0 = "lielow_0";
                        data.tag1 = "lielow_1";
                        data.tag2 = "lielow_2";
                        data.tag3 = "lielow_3";
                        break;
                    case HelpType.BadCondition:
                        data.tag0 = "bad_0";
                        data.tag1 = "bad_1";
                        break;
                }
            }

            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Both sides Actor/Player takes stress leave
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
            message.sideLevel = side.level;
            message.isPublic = true;
            message.data0 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.topText = "Stress Leave";
            data.priority = ItemPriority.Low;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //data depends on whether an actor or player
            if (actorID == playerActorID)
            {
                message.type = MessageType.PLAYER;
                if (side.level == globalAuthority.level)
                { data.itemText = string.Format("{0}, Mayor, takes Stress Leave", GameManager.i.playerScript.GetPlayerNameAuthority()); }
                else { data.itemText = string.Format("{0}, Player, takes Stress Leave", GameManager.i.playerScript.GetPlayerNameResistance()); }
                data.sprite = playerSprite;
                data.spriteName = data.sprite.name;
                data.bottomText = GameManager.i.itemDataScript.GetActorStressLeaveDetails(side);
            }
            else
            {
                //actor
                Actor actor = GameManager.i.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    message.type = MessageType.ACTOR;
                    data.itemText = string.Format("{0}, {1}, has taken Stress Leave", actor.actorName, actor.arc.name);
                    data.sprite = actor.sprite;
                    data.spriteName = data.sprite.name;
                    data.bottomText = GameManager.i.itemDataScript.GetActorStressLeaveDetails(side, actor);
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = actor.actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} has COMPLAINED", actor.arc.name);
            data.topText = "Complaint Lodged";
            data.bottomText = GameManager.i.itemDataScript.GetActorComplainsDetails(actor, reason, warning);
            data.priority = ItemPriority.Medium;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = isPublic;
            message.data0 = actor.actorID;
            message.data1 = benefit;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} has been Spoken too", actor.arc.name);
            data.topText = "You've had words";
            data.bottomText = GameManager.i.itemDataScript.GetActorSpokenTooDetails(actor, reason, benefit);
            data.priority = ItemPriority.Low;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = actorID;
            //ItemData
            ItemData data = new ItemData();
            string genericActorName = "Unknown";
            string genericActorArc = "Unknown";
            if (actorID == 999)
            {
                //player -> player who is affected which can be different from playerSide (who gets the message)
                if (isResistancePlayerAffected == true) { genericActorName = GameManager.i.playerScript.GetPlayerNameResistance(); }
                else { genericActorName = GameManager.i.playerScript.GetPlayerNameAuthority(); }
                genericActorArc = "Player";
            }
            else
            {
                //actor
                Actor actor = GameManager.i.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    genericActorName = actor.actorName;
                    genericActorArc = actor.arc.name;
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            if (isGained == true)
            { data.itemText = string.Format("{0}, is now {1}{2}", genericActorArc, condition.isNowA == true ? "a " : "", condition.tag); }
            else { data.itemText = string.Format("{0} is no longer {1}{2}", genericActorArc, condition.isNowA == true ? "a " : "", condition.tag); }
            data.topText = "Condition Change";
            if (condition != null)
            { data.bottomText = GameManager.i.itemDataScript.GetActorConditionDetails(genericActorName, genericActorArc, condition, isGained, reason); }
            else { Debug.LogWarning("Invalid condition (Null)"); }
            //Medium priority if gain, Low if lost for Actor, always medium for Player
            if (actorID != 999)
            {
                if (isGained == true) { data.priority = ItemPriority.Medium; }
                else { data.priority = ItemPriority.Low; }
            }
            else { data.priority = ItemPriority.Medium; }
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 0;
            SetConditionHelp(condition, data);
            //data depends on whether an actor or player
            if (actorID == playerActorID)
            {
                //player
                data.sprite = playerSprite;
                data.spriteName = data.sprite.name;
            }
            else
            {
                //actor
                Actor actor = GameManager.i.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    data.sprite = actor.sprite;
                    data.spriteName = data.sprite.name;
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Actor blackmail outcome -> either drops threat (opinion max) or carries out reveal of a player secret. SecretID only if reveal, ignore otherwise
    /// 'isThreatDropped' true if no longer blackmailing and 'reason' is why in format '[The threat has been dropped because]...'
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    ///<param name="secretID"></param>
    /// <returns></returns>
    public Message ActorBlackmail(string text, Actor actor, Secret secret = null, bool isThreatDropped = false, string reason = null)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Blackmail;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = actor.actorID;
            if (secret != null)
            { message.dataName = secret.name; }
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} Blackmail threat resolved", actor.arc.name);
            data.topText = "Blackmail Resolved";
            data.bottomText = GameManager.i.itemDataScript.GetActorBlackmailDetails(actor, message.dataName, isThreatDropped, reason);
            data.priority = ItemPriority.Medium;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = actor.actorID;
            message.dataName = secret.name;
            //ItemData
            ItemData data = new ItemData();
            data.topText = secret.tag;
            data.bottomText = GameManager.i.itemDataScript.GetActorSecretDetails(actor, secret, isLearnt);
            if (isLearnt == true)
            { data.itemText = string.Format("{0}, {1}, learns one of your Secrets", actor.actorName, actor.arc.name); }
            else
            { data.itemText = string.Format("{0}, {1}, forgets one of your Secrets", actor.actorName, actor.arc.name); }
            data.priority = ItemPriority.Low;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = actor.actorID;
            message.dataName = secret.name;
            //ItemData
            ItemData data = new ItemData();
            data.topText = secret.tag;
            data.bottomText = GameManager.i.itemDataScript.GetActorRevealSecretDetails(actor, secret, reason);
            data.itemText = string.Format("{0} reveals your SECRET", actor.arc.name);
            data.priority = ItemPriority.High;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
        GlobalSide side = GameManager.i.sideScript.PlayerSide;
        if (side.level == globalResistance.level)
        { Debug.Assert(nodeID >= 0, string.Format("Invalid nodeID {0}", nodeID)); }
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Recruited;
            message.sideLevel = side.level;
            message.data0 = nodeID;
            message.data1 = actor.actorID;
            message.data2 = unhappyTimer;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} Recruited", actor.arc.name);
            data.topText = "Recruited";
            data.bottomText = GameManager.i.itemDataScript.GetActorRecruitedDetails(actor, unhappyTimer);
            data.priority = ItemPriority.Low;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Actor initiates a Relationship Conflict due to opinion dropping below zero (default playerSide). If 'Nothing Happens' then conflictID -1 and provide a reasonNoConflict (format '[because]...')
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="conflictID"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public Message ActorConflict(string text, Actor actor, ActorConflict conflict = null, string reasonNoConflict = null)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Conflict;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = actor.actorID;
            if (conflict != null) { message.dataName = conflict.name; }
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} Relationship Conflict", actor.arc.name);
            data.topText = "Relationship Conflict";
            data.bottomText = GameManager.i.itemDataScript.GetActorConflictDetails(actor, conflict, reasonNoConflict);
            data.priority = ItemPriority.High;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.tag0 = "conflict_0";
            data.tag1 = "conflict_1";
            data.tag2 = "conflict_2";
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = actor.actorID;
            message.dataName = trait.name;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} uses {1} trait", actor.arc.name, trait.tag);
            data.topText = string.Format("{0} trait used", trait.tag);
            data.bottomText = GameManager.i.itemDataScript.GetActorTraitDetails(actor, trait, forText, toText);
            data.priority = ItemPriority.Low;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Traits;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            bool isCrackdown = false;
            if (GameManager.i.turnScript.authoritySecurityState == AuthoritySecurityState.SurveillanceCrackdown) { isCrackdown = true; }
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_LieLow;
            message.sideLevel = globalResistance.level;
            message.data0 = timer;
            //ItemData
            ItemData data = new ItemData();
            if (isCrackdown == false)
            {
                if (timer == 0)
                {
                    data.itemText = "Lie Low action is AVAILABLE";
                    data.topText = "Lie Low Available";
                }
                else
                {
                    data.itemText = "Lie Low action UNAVAILABLE";
                    data.topText = "Lie Low Unavailable";
                }
            }
            else
            {
                //surveillance crackdown
                data.itemText = "Lie Low action UNAVAILABLE";
                data.topText = "Lie Low Unavailable";
            }
            data.bottomText = GameManager.i.itemDataScript.GetActorLieLowOngoingDetails(timer, isCrackdown);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.i.spriteScript.infoSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "lielow_0";
            data.tag1 = "lielow_1";
            data.tag2 = "lielow_2";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalBoth.level;
            message.isPublic = true;
            message.data0 = node.nodeID;
            message.data1 = team.teamID;
            message.data2 = actorID;
            //ItemData
            ItemData data = new ItemData();
            string playerName = GameManager.i.playerScript.PlayerName;
            if (actorID == 999)
            {
                //player captured
                data.itemText = string.Format("{0}, Player, has been CAPTURED", playerName);
                data.topText = "Player Captured";
                data.bottomText = GameManager.i.itemDataScript.GetActorCaptureDetails(playerName, "Player", node, team);
            }
            else
            {
                //actor captured
                Actor actor = GameManager.i.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    data.itemText = string.Format("{0}, {1}, has been CAPTURED", actor.actorName, actor.arc.name);
                    data.topText = string.Format("{0} Captured", actor.arc.name);
                    data.bottomText = GameManager.i.itemDataScript.GetActorCaptureDetails(actor.actorName, actor.arc.name, node, team);
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.capturedSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// warning, eg. Actor can be captured 'cause invisibility zero -> InfoApp effect
    /// Provide colour formatting for top and bottom details
    /// listOfHelpTags allows up to 4 help tags, eg. "info_1". Any more are ignored.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="detailsTop"></param>
    /// <param name="detailsBottom"></param>
    /// <param name="sprite"></param>
    /// <param name="actorID"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public Message ActorWarningOngoing(string text, string detailsTop, string detailsBottom, Sprite sprite, int actorID, List<string> listOfHelpTags = null)
    {
        Debug.Assert(sprite != null, "Invalid spirte (Null)");
        Debug.Assert(actorID > -1, "Invalid actorID (less than Zero)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Warning;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Warning";
            data.bottomText = GameManager.i.itemDataScript.GetActorWarningOngoingDetails(detailsTop, detailsBottom);
            data.priority = ItemPriority.Low;
            data.sprite = sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            if (listOfHelpTags != null)
            {
                for (int i = 0; i < listOfHelpTags.Count; i++)
                {
                    switch (i)
                    {
                        case 0: data.tag0 = listOfHelpTags[i]; break;
                        case 1: data.tag1 = listOfHelpTags[i]; break;
                        case 2: data.tag2 = listOfHelpTags[i]; break;
                        case 3: data.tag3 = listOfHelpTags[i]; break;
                        default: Debug.LogWarningFormat("Invalid help tag \"{0}\" (can only have 4, there are {1})", listOfHelpTags[i], i); break;
                    }
                }
                data.help = 1;
            }
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalBoth.level;
            message.isPublic = true;
            message.data0 = node.nodeID;
            message.data1 = actorID;
            //ItemData
            ItemData data = new ItemData();
            string playerName = GameManager.i.playerScript.PlayerName;
            if (actorID == 999)
            {
                //player captured
                data.itemText = string.Format("{0}, Player, has been RELEASED", playerName);
                data.topText = "Player Released";
                data.bottomText = GameManager.i.itemDataScript.GetActorReleaseDetails(playerName, "Player", node);
            }
            else
            {
                //actor captured
                Actor actor = GameManager.i.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    data.itemText = string.Format("{0}, {1}, has been RELEASED", actor.actorName, actor.arc.name);
                    data.topText = string.Format("{0} Released", actor.arc.name);
                    data.bottomText = GameManager.i.itemDataScript.GetActorReleaseDetails(actor.actorName, actor.arc.name, node);
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.releasedSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            if (actorID == 999)
            {
                data.tag0 = "questionable_0";
                data.tag1 = "questionable_1";
            }
            else
            {
                data.tag0 = "traitor_0";
                data.tag1 = "traitor_1";
                data.tag2 = "traitor_2";
            }
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Actor negates a change in opinion due to their compatibility with the Player
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actor"></param>
    /// <param name="opinionChangeNegated"></param>
    /// <returns></returns>
    public Message ActorCompatibility(string text, Actor actor, int opinionChangeNegated, string reasonForChange)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(opinionChangeNegated != 0, "Invalid opinionChangeNegated (Zero)");
        Debug.Assert(string.IsNullOrEmpty(reasonForChange) == false, "Invalid reasonForChange (Null or Empty)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTOR;
            message.subType = MessageSubType.Actor_Compatibility;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = actor.actorID;
            message.data1 = opinionChangeNegated;
            message.data2 = actor.GetPersonality().GetCompatibilityWithPlayer();
            message.dataName = reasonForChange;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} ignores change to OPINION", actor.arc.name);
            data.topText = "No change to Opinion";
            data.bottomText = GameManager.i.itemDataScript.GetActorCompatibilityDetails(actor, opinionChangeNegated, reasonForChange);
            data.priority = ItemPriority.Medium;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "compatibility_0";
            data.tag1 = "compatibility_1";
            data.tag2 = "compatibility_2";
            data.tag3 = "compatibility_3";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
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
            data.bottomText = GameManager.i.itemDataScript.GetContactDetails(reason, actor, node, contact, isGained);
            data.priority = ItemPriority.Medium;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.tag0 = "contact_0";
            if (isGained == true)
            { data.tag1 = "contact_4"; }
            else { data.tag1 = "contact_5"; }
            data.tag2 = "contact_1";
            data.tag3 = "contact_2";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
            message.data0 = actor.actorID;
            message.data1 = node.nodeID;
            message.data2 = contact.contactID;
            message.dataName = target.name;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("One of {0}'s network of contacts learns of a RUMOUR", actor.arc.name);
            data.topText = string.Format("{0} gets a CALL", actor.actorName);
            data.bottomText = GameManager.i.itemDataScript.GetContactTargetRumourDetails(actor, node, contact, target);
            data.priority = ItemPriority.Low;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.tag0 = "contact_0";
            data.tag1 = "contact_3";
            data.tag2 = "contact_1";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
            message.data0 = actor.actorID;
            message.data1 = node.nodeID;
            message.data2 = contact.contactID;
            message.data3 = moveNumber;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("One of {0}'s network of contacts spots your NEMESIS", actor.arc.name);
            data.topText = string.Format("{0} gets a CALL", actor.actorName);
            data.bottomText = GameManager.i.itemDataScript.GetContactNemesisSpottedDetails(actor, node, contact, nemesis, moveNumber);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.aiAlertSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.tag0 = "contact_6";
            data.tag1 = "contact_1";
            data.tag2 = "nemesis_0";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// One of Resistance actors network of contacts spots the Npc
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <param name="npc"></param>
    /// <returns></returns>
    public Message ContactNpcSpotted(string text, Actor actor, Node node, Contact contact, Npc npc)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(contact != null, "Invalid contact (Null)");
        Debug.Assert(npc != null, "Invalid Npc (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.CONTACT;
            message.subType = MessageSubType.Contact_Npc_Spotted;
            message.sideLevel = globalResistance.level;
            message.data0 = actor.actorID;
            message.data1 = node.nodeID;
            message.data2 = contact.contactID;
            message.dataName = npc.tag;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("One of {0}'s network of contacts spots the {1}", actor.arc.name, npc.tag);
            data.topText = string.Format("{0} gets a CALL", actor.actorName);
            data.bottomText = GameManager.i.itemDataScript.GetContactNpcSpottedDetails(actor, node, contact, npc);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.aiAlertSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.tag0 = "npc_3";
            data.tag1 = "contact_1";
            data.tag2 = "npc_5";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
            message.data0 = actor.actorID;
            message.data1 = node.nodeID;
            message.data2 = contact.contactID;
            message.data3 = team.teamID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("One of {0}'s contacts spots an {1} Team", actor.arc.name, team.arc.name);
            data.topText = string.Format("{0} gets a CALL", actor.actorName);
            data.bottomText = GameManager.i.itemDataScript.GetContactTeamSpottedDetails(actor, node, contact, team);
            data.priority = ItemPriority.High;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.tag0 = "contact_0";
            data.tag1 = "contact_7";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Resistance Contact goes inactive for any reason
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <returns></returns>
    public Message ContactInactive(string text, string reason, Actor actor, Node node, Contact contact)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(contact != null, "Invalid contact (Null)");
        Debug.Assert(string.IsNullOrEmpty(reason) == false, "Invalid reason (Null or Empty)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.CONTACT;
            message.subType = MessageSubType.Contact_Inactive;
            message.sideLevel = globalResistance.level;
            message.data0 = actor.actorID;
            message.data1 = node.nodeID;
            message.data2 = contact.contactID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("One of {0}'s contacts goes Silent", actor.arc.name);
            data.topText = "Contact goes Silent";
            data.bottomText = GameManager.i.itemDataScript.GetContactInactiveDetails(node, contact, reason);
            data.priority = ItemPriority.Low;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.tag0 = "contact_8";
            data.tag1 = "contact_9";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Inactive resistance contact becomes active once more
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <returns></returns>
    public Message ContactActive(string text, Actor actor, Node node, Contact contact)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(contact != null, "Invalid contact (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.CONTACT;
            message.subType = MessageSubType.Contact_Active;
            message.sideLevel = globalResistance.level;
            message.data0 = actor.actorID;
            message.data1 = node.nodeID;
            message.data2 = contact.contactID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("One of {0}'s contacts returns", actor.arc.name);
            data.topText = "Contact Active";
            data.bottomText = GameManager.i.itemDataScript.GetContactActiveDetails(node, contact);
            data.priority = ItemPriority.Low;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.tag0 = "contact_10";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Effect tab update for inactive contact (how much longer before returning to Active)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="reason"></param>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <returns></returns>
    public Message ContactTimer(string text, Actor actor, Node node, Contact contact)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(contact != null, "Invalid contact (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.CONTACT;
            message.subType = MessageSubType.Contact_Inactive;
            message.sideLevel = globalResistance.level;
            message.data0 = actor.actorID;
            message.data1 = node.nodeID;
            message.data2 = contact.contactID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("One of {0}'s contacts is Silent", actor.arc.name);
            data.topText = "Contact off the Grid";
            data.bottomText = GameManager.i.itemDataScript.GetContactTimerDetails(node, contact);
            data.priority = ItemPriority.Low;
            data.sprite = actor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.tag0 = "contact_8";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
            message.data0 = node.nodeID;
            message.data1 = moveNumber;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "TRACER picks up an ANOMALOUS reading";
            data.topText = "Threat Detected";
            data.bottomText = GameManager.i.itemDataScript.GetTracerNemesisSpottedDetails(node, nemesis, moveNumber);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.aiAlertSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
            message.data0 = node.nodeID;
            message.data1 = team.teamID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "TRACER detects presence of an ERASURE Team";
            data.topText = "Threat Detected";
            data.bottomText = GameManager.i.itemDataScript.GetTracerTeamSpottedDetails(node, team);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.aiAlertSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Tracer spots Npc
    /// </summary>
    /// <param name="text"></param>
    /// <param name="npc"></param>
    /// <returns></returns>
    public Message TracerNpcSpotted(string text, Npc npc)
    {
        Debug.Assert(npc != null, "Invalid npc (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.CONTACT;
            message.subType = MessageSubType.Tracer_Npc_Spotted;
            message.sideLevel = globalResistance.level;
            message.data0 = npc.currentNode.nodeID;
            message.dataName = npc.tag;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("TRACER detects the {0}", npc.tag);
            data.topText = string.Format("{0} Detected", npc.tag);
            data.bottomText = GameManager.i.itemDataScript.GetTracerNpcSpottedDetails(npc);
            data.priority = ItemPriority.High;
            data.sprite = npc.sprite;
            data.spriteName = npc.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = npc.currentNode.nodeID;
            data.help = 1;
            data.tag0 = "npc_5";
            data.tag1 = "npc_3";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
            message.data0 = nodeID;
            message.data1 = GameManager.i.nemesisScript.GetSearchRatingAdjusted();
            message.data2 = GameManager.i.nemesisScript.GetStealthRatingAdjusted();
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} Nemesis current Status", nemesis.name);
            data.topText = "Nemesis Status";
            data.bottomText = GameManager.i.itemDataScript.GetNemesisOngoingEffectDetails(nemesis, message.data1);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.aiAlertSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
            message.data0 = nodeID;
            message.data1 = GameManager.i.nemesisScript.GetSearchRatingAdjusted();
            message.data2 = GameManager.i.nemesisScript.GetStealthRatingAdjusted();
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} Nemesis changes mode", nemesis.name);
            data.topText = "New Nemesis Mode";
            data.bottomText = GameManager.i.itemDataScript.GetNemesisNewModeDetails(nemesis, message.data1);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.i.spriteScript.aiAlertSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalAuthority.level;
            message.data0 = coolDownTimer;
            message.data1 = controlTimer;
            //ItemData
            ItemData data = new ItemData();
            if (GameManager.i.nemesisScript.GetMode() != NemesisMode.Inactive)
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
            data.bottomText = GameManager.i.itemDataScript.GetNemesisPlayerOngoingDetails(nemesis, isPlayerControl, coolDownTimer, controlTimer, nodeControl, nodeCurrent);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.i.spriteScript.aiAlertSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            if (nodeControl != null)
            { data.nodeID = nodeControl.nodeID; }
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalAuthority.level;
            message.isPublic = true;
            message.displayDelay = delay;
            message.data0 = destinationNode.nodeID;
            message.data1 = connection.connID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Resistance Leader detected using CONNECTION";
            data.topText = "Connection Activity";
            data.bottomText = GameManager.i.itemDataScript.GetAIConnectionActivityDetails(destinationNode, delay);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.i.spriteScript.aiAlertSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.nodeID = destinationNode.nodeID;
            data.connID = connection.connID;
            data.delay = delay;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalAuthority.level;
            message.isPublic = true;
            message.displayDelay = delay;
            message.data0 = node.nodeID;
            message.data1 = actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Resistance activity detected in DISTRICT";
            data.topText = "Resistance Activity";
            data.bottomText = GameManager.i.itemDataScript.GetAINodeActivityDetails(node, delay);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.i.spriteScript.aiAlertSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.delay = delay;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalBoth.level;
            message.isPublic = true;
            message.displayDelay = delay;
            message.data0 = nodeID;
            message.data1 = GameManager.i.playerScript.actorID;
            //ItemData
            ItemData data = new ItemData();
            if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideAuthority.level)
            { data.itemText = "AI Traceback DETECTS Resistance leader"; }
            else { data.itemText = "AI Traceback DETECTS you"; }
            data.topText = "Detected!";
            data.bottomText = GameManager.i.itemDataScript.GetAIDetectedDetails(nodeID, delay);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.aiCountermeasureSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = nodeID;
            data.help = 1;
            data.delay = delay;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalBoth.level;
            message.isPublic = true;
            message.data0 = nodeID;
            message.data1 = connID;
            message.data2 = actorID;
            //ItemData
            ItemData data = new ItemData();

            data.topText = "Location Known";
            if (actorID == 999)
            {
                if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideAuthority.level)
                { data.itemText = "AI knows Resistance leader's CURRENT LOCATION"; }
                else { data.itemText = "AI knows of your  CURRENT LOCATION"; }
                data.bottomText = GameManager.i.itemDataScript.GetAIImmediateActivityDetails(reason, nodeID, connID, GameManager.i.playerScript.PlayerName, "Player");
            }
            else
            {
                //actor
                Actor actor = GameManager.i.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    data.itemText = string.Format("AI knows of {0}, {1}'s, CURRENT LOCATION", actor.actorName, actor.arc.name);
                    data.bottomText = GameManager.i.itemDataScript.GetAIImmediateActivityDetails(reason, nodeID, connID, actor.actorName, actor.arc.name);
                }
                else
                {
                    Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID);
                    data.bottomText = "Unknown";
                }
            }
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.aiAlertSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = nodeID;
            data.connID = connID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// AI notification to one or both sides of AI being hacked
    /// </summary>
    /// <param name="text"></param>
    /// <param name="currentPowerCost"></param>
    /// <param name="isDetected"></param>
    /// <returns></returns>
    public Message AIHacked(string text, int currentPowerCost, bool isDetected, int attemptsDetected, int attemptsTotal)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Hacked;
            if (isDetected == true)
            { message.sideLevel = globalBoth.level; message.data1 = 1; message.isPublic = true; }
            else
            { message.sideLevel = globalResistance.level; message.data1 = 0; message.isPublic = false; }
            message.data0 = currentPowerCost;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Resistance HACKS AI";
            data.topText = "Hacking Detected";
            data.bottomText = GameManager.i.itemDataScript.GetAIHackedDetails(isDetected, attemptsDetected, attemptsTotal);
            if (isDetected == true) { data.priority = ItemPriority.Medium; }
            else { data.priority = ItemPriority.Low; }
            data.sprite = GameManager.i.spriteScript.alarmSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// AI notification to both sides of an AI Security System Reboot (commence / complete)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="currentPowerCost"></param>
    /// <param name="rebootTimer"></param>
    /// <returns></returns>
    public Message AIReboot(string text, int currentPowerCost, int rebootTimer)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.AI;
            message.subType = MessageSubType.AI_Reboot;
            message.sideLevel = globalBoth.level;
            message.isPublic = true;
            message.data0 = currentPowerCost;
            message.data1 = rebootTimer;
            //ItemData
            ItemData data = new ItemData();
            if (rebootTimer > 0) { data.itemText = "AI REBOOTS Commences"; }
            else { data.itemText = "AI REBOOT Completed"; }
            data.topText = "AI Reboots";
            data.bottomText = GameManager.i.itemDataScript.GetAIRebootDetails(rebootTimer, currentPowerCost);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.i.spriteScript.aiRebootSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalBoth.level;
            message.isPublic = true;
            message.data0 = timerStartValue;
            message.data1 = protocolLevelNew;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = itemText;
            data.topText = "AI Countermeasures";
            data.bottomText = GameManager.i.itemDataScript.GetAICounterMeasureDetails(warning, timerStartValue, protocolLevelNew);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.i.spriteScript.aiCountermeasureSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalBoth.level;
            message.isPublic = true;
            message.data0 = chanceOfIncrease;
            message.data1 = randomRoll;
            //add
            GameManager.i.dataScript.AddMessage(message);
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
    public Message DecisionGlobal(string text, string itemText, string description, string decName, int duration = 0, int loyaltyAdjust = 0, int crisisAdjust = 0)
    {
        Debug.Assert(string.IsNullOrEmpty(decName) == false, "Invalid decName (Null or Empty)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.DECISION;
            message.subType = MessageSubType.Decision_Global;
            message.sideLevel = globalBoth.level;
            message.isPublic = true;
            message.dataName = decName;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = itemText;
            data.topText = "City Wide Decision";
            data.bottomText = GameManager.i.itemDataScript.GetDecisionGlobalDetails(description, duration, loyaltyAdjust, crisisAdjust);
            data.priority = ItemPriority.Medium;
            data.sprite = mayor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalBoth.level;
            message.isPublic = true;
            message.data0 = connection.connID;
            message.data1 = (int)secLevel;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Connection Security changed";
            data.topText = "Connection Security";
            data.bottomText = GameManager.i.itemDataScript.GetDecisionConnectionDetails(connection, secLevel);
            data.priority = ItemPriority.Medium;
            data.sprite = mayor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.connID = connection.connID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = side.level;
            message.isPublic = true;
            message.data0 = amount;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Request Resources";
            data.bottomText = description;
            data.priority = ItemPriority.Low;
            data.sprite = mayor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalAuthority.level;
            message.isPublic = true;
            message.data0 = teamID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Request Team";
            data.bottomText = text;
            data.priority = ItemPriority.Low;
            data.sprite = mayor.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
    public Message DecisionOngoingEffect(string text, string itemText, string topText, string middleText, string bottomText, string decName)
    {
        Debug.Assert(string.IsNullOrEmpty(decName) == false, "Invalid decName (Null or Empty)");
        Debug.Assert(string.IsNullOrEmpty(itemText) == false, "Invalid itemText (Null or Empty)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Decision;
            message.sideLevel = globalBoth.level;
            message.dataName = decName;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = itemText;
            data.topText = "Ongoing Effect";
            data.bottomText = GameManager.i.itemDataScript.GetDecisionOngoingEffectDetails(topText, middleText, bottomText);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.i.spriteScript.aiAlertSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalAuthority.level;
            message.data0 = team.teamID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} team added to Reserve Pool", team.arc.name);
            data.topText = "Team Added";
            data.bottomText = GameManager.i.itemDataScript.GetAddTeamDetails(team, reason);
            data.priority = ItemPriority.Low;
            data.sprite = team.arc.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
        if (GameManager.i.sideScript.authorityOverall == SideState.Human)
            if (string.IsNullOrEmpty(text) == false)
            {
                Message message = new Message();
                message.text = text;
                message.type = MessageType.TEAM;
                message.subType = MessageSubType.Team_Deploy;
                message.sideLevel = globalAuthority.level;
                message.data0 = node.nodeID;
                message.data1 = team.teamID;
                if (actor != null)
                {
                    message.data2 = actor.actorID;
                    //ItemData (not if actor = null because this would be an AI action)
                    ItemData data = new ItemData();
                    data.itemText = string.Format("{0} team Deployed", team.arc.name);
                    data.topText = "Team Deployed";
                    data.bottomText = GameManager.i.itemDataScript.GetTeamDeployDetails(team, node, actor);
                    data.priority = ItemPriority.Low;
                    data.sprite = team.arc.sprite;
                    data.spriteName = data.sprite.name;
                    data.tab = ItemTab.ALERTS;
                    data.type = message.type;
                    data.subType = message.subType;
                    data.sideLevel = message.sideLevel;
                    data.nodeID = node.nodeID;
                    data.help = 1;
                    //add
                    GameManager.i.dataScript.AddItemData(data);
                }
                //add
                GameManager.i.dataScript.AddMessage(message);

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
        if (GameManager.i.sideScript.authorityOverall == SideState.Human)
            if (string.IsNullOrEmpty(text) == false)
            {
                Message message = new Message();
                message.text = text;
                message.type = MessageType.TEAM;
                message.subType = MessageSubType.Team_AutoRecall;
                message.sideLevel = globalAuthority.level;
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
                    data.bottomText = GameManager.i.itemDataScript.GetTeamAutoRecallDetails(node, team, actor);
                    data.priority = ItemPriority.Low;
                    data.sprite = team.arc.sprite;
                    data.spriteName = data.sprite.name;
                    data.tab = ItemTab.ALERTS;
                    data.type = message.type;
                    data.subType = message.subType;
                    data.sideLevel = message.sideLevel;
                    data.nodeID = node.nodeID;
                    data.help = 1;
                    GameManager.i.dataScript.AddItemData(data);
                }
                //add
                GameManager.i.dataScript.AddMessage(message);
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
            message.sideLevel = globalAuthority.level;
            message.data0 = node.nodeID;
            message.data1 = team.teamID;
            message.data2 = actor.actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} team Withdrawn", team.arc.name);
            data.topText = "Team Withdrawn";
            data.bottomText = GameManager.i.itemDataScript.GetTeamWithdrawDetails(reason, team, node, actor);
            data.priority = ItemPriority.Low;
            data.sprite = team.arc.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
    public Message TeamEffect(string text, string itemText, string effectText, Node node, Team team, bool isBothSides = false)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(team != null, "Invalid team (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TEAM;
            message.subType = MessageSubType.Team_Effect;
            if (isBothSides == true)
            { message.sideLevel = globalBoth.level; }
            else { message.sideLevel = globalAuthority.level; }
            message.isPublic = true;
            message.data0 = node.nodeID;
            message.data1 = team.teamID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = itemText;
            data.topText = "Team Outcome";
            data.bottomText = GameManager.i.itemDataScript.GetTeamEffectDetails(effectText, node, team);
            data.priority = ItemPriority.Medium;
            data.sprite = team.arc.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalBoth.level;
            message.isPublic = true;
            message.displayDelay = 0;
            message.data0 = node.nodeID;
            message.data1 = team.teamID;
            message.data2 = actor.actorID;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} team NEUTRALISED by Resistance", team.arc.name);
            data.topText = "Team Neutralised";
            data.bottomText = GameManager.i.itemDataScript.GetTeamNeutraliseDetails(node, team);
            data.priority = ItemPriority.High;
            data.sprite = team.arc.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - -  Gear - - -
    //

    /// <summary>
    /// Player gifts an actor with gear ('isGiven' defaults to true) and gains opinion in return (extra if it's the actor's preferred gear type)
    /// Or player takes gear from actor('isGiven' set to false) and it costs a set amount of opinion (extra if preferred) to the actor to do so
    /// </summary>
    /// <param name="text"></param>
    /// <param name="actorID"></param>
    /// <param name="gearID"></param>
    /// <param name="opinion"></param>
    /// <returns></returns>
    public Message GearTakeOrGive(string text, Actor actor, Gear gear, int opinion, bool isGiven = true)
    {
        Debug.Assert(actor != null, "Invalid actor (Null)");
        Debug.Assert(gear != null, "Invalid gear (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.PLAYER;
            message.subType = MessageSubType.Gear_Given;
            message.sideLevel = globalResistance.level;
            message.data0 = actor.actorID;
            message.data1 = opinion;
            message.dataName = gear.name;
            //ItemData
            ItemData data = new ItemData();
            if (isGiven == true)
            {
                data.itemText = string.Format("{0} gear Given", gear.tag);
                data.topText = "Give Gear";
            }
            else
            {
                data.itemText = string.Format("{0} gear Taken", gear.tag);
                data.topText = "Gear Taken";
            }
            data.bottomText = GameManager.i.itemDataScript.GetGearTakeOrGiveDetails(actor, gear, opinion, isGiven);
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    /// <summary>
    /// Gear has been used and compromised. Returns null if text invalid
    /// </summary>
    /// <param name="text"></param>
    /// <param name="gear"></param>
    /// <param name="powerUsed"></param>
    /// <returns></returns>
    public Message GearCompromised(string text, Gear gear, int powerUsed, int nodeID = -1)
    {
        Debug.Assert(gear != null, "Invalid gear (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Comprised;
            message.sideLevel = globalResistance.level;
            message.isPublic = true;
            message.data0 = powerUsed;
            message.data1 = nodeID;
            message.dataName = gear.name;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} gear Compromised", gear.tag);
            data.topText = "Gear Compromised";
            data.bottomText = GameManager.i.itemDataScript.GetGearCompromisedDetails(gear, powerUsed);
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
            message.dataName = gear.name;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} gear Used", gear.tag);
            data.topText = "Gear Used";
            data.bottomText = GameManager.i.itemDataScript.GetGearUsedDetails(gear);
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
            message.isPublic = true;
            message.data0 = actor.actorID;
            message.dataName = gear.name;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} gear Lost", gear.tag);
            data.topText = "Gear Lost";
            data.bottomText = GameManager.i.itemDataScript.GetGearLostDetails(gear, actor, isGivenToHQ);
            data.priority = ItemPriority.Medium;
            data.sprite = gear.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
            message.isPublic = true;
            message.data0 = actor.actorID;
            message.dataName = gear.name;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} gear Available", gear.tag);
            data.topText = "Gear Available";
            data.bottomText = GameManager.i.itemDataScript.GetGearAvailableDetails(gear, actor);
            data.priority = ItemPriority.Medium;
            data.sprite = gear.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
            message.data0 = node.nodeID;
            message.data1 = actorID;
            message.dataName = gear.name;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} gear Obtained", gear.tag);
            data.topText = "Gear Obtained";
            data.bottomText = GameManager.i.itemDataScript.GetGearObtainedDetails(gear, node, actorID);
            data.priority = ItemPriority.Low;
            data.sprite = gear.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// InsideMan/BugSpray/Neural Whisper (special gear) info dump. Anything that has ShowMe nodes plural. Text doubles as ItemText
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public Message GearSpecial(string text, string infoDump)
    {
        //message
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.GEAR;
            message.subType = MessageSubType.Gear_Special;
            message.sideLevel = globalResistance.level;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = "Special Gear Used";
            data.bottomText = GameManager.i.itemDataScript.GetGearSpecialDetails(infoDump);
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.i.spriteScript.infoSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = node.nodeID;
            message.data1 = target.timerWindow;
            message.dataName = target.name;
            //ItemData
            ItemData data = new ItemData();
            if (message.sideLevel == globalResistance.level)
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
            data.bottomText = GameManager.i.itemDataScript.GetTargetNewDetails(node, target, sideText, message.sideLevel);
            data.priority = ItemPriority.Medium;
            data.sprite = target.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            data.tag0 = "target_0";
            data.tag1 = "target_1";
            data.tag2 = "target_2";
            data.tag3 = "target_3";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = node.nodeID;
            message.data1 = target.numOfAttempts;
            message.dataName = target.name;
            //ItemData
            ItemData data = new ItemData();
            if (message.sideLevel == globalResistance.level)
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
            data.bottomText = GameManager.i.itemDataScript.GetTargetExpiredDetails(node, target, sideText, message.sideLevel);
            data.priority = ItemPriority.Medium;
            data.sprite = target.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = node.nodeID;
            message.data1 = target.timerWindow;
            message.dataName = target.name;
            //ItemData
            ItemData data = new ItemData();
            //resistance player only
            data.itemText = string.Format("Window of Opportunity at {0} CLOSING SOON", node.nodeName);
            data.topText = "Target Warning";
            data.bottomText = GameManager.i.itemDataScript.GetTargetExpiredWarningDetails(node, target);
            data.priority = ItemPriority.Medium;
            data.sprite = target.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalResistance.level;
            message.data0 = node.nodeID;
            message.data1 = actorID;
            message.dataName = target.name;
            //ItemData, resistance only
            if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideResistance.level)
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
                data.bottomText = GameManager.i.itemDataScript.GetTargetAttemptDetails(node, actorID, target);
                data.sprite = target.sprite;
                data.spriteName = data.sprite.name;
                data.tab = ItemTab.ALERTS;
                data.type = message.type;
                data.subType = message.subType;
                data.sideLevel = message.sideLevel;
                data.nodeID = node.nodeID;
                data.help = 1;
                //add
                GameManager.i.dataScript.AddMessage(message);
                GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalBoth.level;
            message.data0 = node.nodeID;
            message.data1 = team.teamID;
            message.dataName = target.name;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("Target CONTAINED at {0}", node.nodeName);
            data.topText = "Target Contained";
            data.bottomText = GameManager.i.itemDataScript.GetTargetContainedDetails(node, team, target);
            data.priority = ItemPriority.Medium;
            data.sprite = target.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalBoth.level;
            message.data0 = nodeID;
            //add
            GameManager.i.dataScript.AddMessage(message);
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
        Debug.Assert(ongoing.nodeID > -1, "Invalid Ongoing.nodeID (less than zero)");
        if (string.IsNullOrEmpty(ongoing.text) == false)
        {
            Message message = new Message();
            message.text = ongoing.text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Current;
            message.sideLevel = globalBoth.level;
            message.data0 = ongoing.nodeID;
            message.data1 = ongoing.timer;
            //ItemData
            ItemData data = new ItemData();
            Node node = GameManager.i.dataScript.GetNode(ongoing.nodeID);
            if (node != null)
            { data.itemText = string.Format("{0}, {1} district, ONGOING EFFECT", node.nodeName, node.Arc.name); }
            else
            {
                Debug.LogWarningFormat("Invalid node (Null) for ongoing.nodeID {0}", ongoing.nodeID);
                data.itemText = "Unknown Node";
            }
            data.topText = "District Ongoing Effect";
            data.bottomText = GameManager.i.itemDataScript.GetOngoingEffectDetails(ongoing);
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.i.spriteScript.ongoingEffectSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
    public Message OngoingEffectExpired(string text)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Expired;
            message.sideLevel = globalBoth.level;
            message.isPublic = true;
            //add
            GameManager.i.dataScript.AddMessage(message);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Special message used for InfoApp active 'Effects' tab. Usually either actor (actorID 999 for player) OR node based. Itemtext is text. Old version, being phased out. Use new one below.
    /// </summary>
    /// <param name="itemData"></param>
    /// <param name="detailsTop"></param>
    /// <param name="detailsBottom"></param>
    /// <param name="actor"></param>
    /// <param name="listOfHelp">Provide a list of help strings, eg. 'cure_1' to be displayed</param>
    /// <param name="node"></param>
    /// <returns></returns>
    public Message ActiveEffect(string text, string topText, string detailsTop, string detailsBottom, Sprite sprite, int actorID = -1, Node node = null, Condition condition = null)
    {
        Debug.Assert(sprite != null, "Invalid spirte (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ACTIVE;
            message.subType = MessageSubType.Active_Effect;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            if (node != null) { message.data0 = node.nodeID; }
            if (actorID > -1) { message.data1 = actorID; }
            //ItemData
            ItemData data = new ItemData();
            data.itemText = text;
            data.topText = topText;
            data.bottomText = GameManager.i.itemDataScript.GetActiveEffectDetails(detailsTop, detailsBottom, actorID, node);
            data.priority = ItemPriority.Low;
            data.sprite = sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            if (node != null)
            { data.nodeID = node.nodeID; }
            if (condition != null)
            { data.help = 1;  SetConditionHelp(condition, data); }
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// New version of Active Effect (uses data package and allows for help). 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="topText"></param>
    /// <param name="detailsTop"></param>
    /// <param name="detailsBottom"></param>
    /// <param name="sprite"></param>
    /// <param name="actorID"></param>
    /// <param name="node"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public Message ActiveEffect(ActiveEffectData dataEffect)
    {
        Debug.Assert(dataEffect.sprite != null, "Invalid spirte (Null)");
        if (string.IsNullOrEmpty(dataEffect.text) == false)
        {
            Message message = new Message();
            message.text = dataEffect.text;
            message.type = MessageType.ACTIVE;
            message.subType = MessageSubType.Active_Effect;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            if (dataEffect.node != null) { message.data0 = dataEffect.node.nodeID; }
            if (dataEffect.actorID > -1) { message.data1 = dataEffect.actorID; }
            //ItemData
            ItemData data = new ItemData();
            data.itemText = dataEffect.text;
            data.topText = dataEffect.topText;
            data.bottomText = GameManager.i.itemDataScript.GetActiveEffectDetails(dataEffect.detailsTop, dataEffect.detailsBottom, dataEffect.actorID, dataEffect.node);
            data.priority = ItemPriority.Low;
            data.sprite = dataEffect.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            if (dataEffect.node != null)
            { data.nodeID = dataEffect.node.nodeID; }
            //help (NOT for conditions, subMethod below handles this)
            if (string.IsNullOrEmpty(dataEffect.help0) == true)
            { data.help = 0; }
            else
            {
                data.help = 1;
                data.tag0 = dataEffect.help0;
                if (string.IsNullOrEmpty(dataEffect.help1) == false)
                {
                    data.tag1 = dataEffect.help1;
                    if (string.IsNullOrEmpty(dataEffect.help2) == false)
                    {
                        data.tag2 = dataEffect.help2;
                        if (string.IsNullOrEmpty(dataEffect.help3) == false)
                        { data.tag3 = dataEffect.help3; }
                    }
                }
            }
            if (dataEffect.condition != null)
            { SetConditionHelp(dataEffect.condition, data); }
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - HQ - - -
    //

    /// <summary>
    /// HQ support (Power) that is given/declined at the beginning of each turn
    /// </summary>
    /// <param name="text"></param>
    /// <param name="hqApprovalLevel"></param>
    /// <param name="playerPowerBefore"></param>
    /// <param name="supportGiven"></param>
    /// <returns></returns>
    public Message HqSupport(string text, Hq factionHQ, int hqApprovalLevel, int playerPowerBefore, int supportGiven = -1)
    {
        Debug.Assert(factionHQ != null, "Invalid HQ (Null)");
        Debug.Assert(hqApprovalLevel > -1, "Invalid hqSupportLevel ( < zero)");
        Debug.Assert(playerPowerBefore > -1, "Invalid playerPowerBefore ( < zero)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.HQ;
            message.subType = MessageSubType.HQ_Support;
            message.sideLevel = factionHQ.side.level;
            message.isPublic = true;
            message.data0 = hqApprovalLevel;
            message.data1 = playerPowerBefore;
            message.data2 = supportGiven;
            //ItemData
            ItemData data = new ItemData();
            if (supportGiven > 0)
            {
                //support Approved
                data.itemText = string.Format("Support request to {0} APPROVED", factionHQ.tag);
                data.topText = "Support APPROVED";
            }
            else
            {
                //support Declined
                data.itemText = string.Format("Support request to {0} declined", factionHQ.tag);
                data.topText = "Support Declined";
            }
            data.bottomText = GameManager.i.itemDataScript.GetHqSupportDetails(factionHQ, hqApprovalLevel, supportGiven);
            //high priority if given
            if (supportGiven > 0) { data.priority = ItemPriority.High; }
            else { data.priority = ItemPriority.Medium; }
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.sprite = factionHQ.sprite;
            data.spriteName = data.sprite.name;
            data.help = 1;
            data.tag0 = "hq_supp_0";
            data.tag1 = "hq_supp_1";
            data.tag2 = "hq_supp_2";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// HQ support unavailable for some reason
    /// </summary>
    /// <param name="text"></param>
    /// <param name="reason"></param>
    /// <param name="factionHQ"></param>
    /// <returns></returns>
    public Message HqSupportUnavailable(string text, string reason, Hq factionHQ)
    {
        Debug.Assert(string.IsNullOrEmpty(reason) == false, "Invalid reason (Null or Empty)");
        Debug.Assert(factionHQ != null, "Invalid HQ (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.HQ;
            message.subType = MessageSubType.HQ_SupportUnavailable;
            message.sideLevel = factionHQ.side.level;
            message.isPublic = true;
            message.data0 = GameManager.i.hqScript.GetHqApproval();
            message.data1 = GameManager.i.playerScript.Power;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} Support Unavailable", factionHQ.tag);
            data.topText = "No Support";
            data.bottomText = reason;
            data.priority = ItemPriority.Medium;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.sprite = factionHQ.sprite;
            data.spriteName = data.sprite.name;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Change of Approval level, 'change' in +/-, 'reason' is self-contained
    /// </summary>
    /// <param name="text"></param>
    /// <param name="reason"></param>
    /// <param name="factionHQ"></param>
    /// <param name="oldLevel"></param>
    /// <param name="change"></param>
    /// <param name="newLevel"></param>
    /// <returns></returns>
    public Message HqApproval(string text, string reason, Hq factionHQ, int oldLevel, int change, int newLevel)
    {
        Debug.Assert(factionHQ != null, "Invalid HQ (Null)");
        Debug.Assert(change != 0, "Invalid change (Zero)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.HQ;
            message.subType = MessageSubType.HQ_Approval;
            message.sideLevel = factionHQ.side.level;
            message.isPublic = true;
            message.data0 = oldLevel;
            message.data1 = change;
            message.data2 = newLevel;
            //ItemData
            ItemData data = new ItemData();
            if (change > 0)
            { data.itemText = string.Format("{0} Approval INCREASES", factionHQ.tag); }
            else
            { data.itemText = string.Format("{0} Approval DECREASES", factionHQ.tag); }
            data.topText = "Approval Changes";
            data.bottomText = GameManager.i.itemDataScript.GetHqApprovalDetails(factionHQ, reason, change, newLevel);
            data.priority = ItemPriority.Medium;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.sprite = factionHQ.sprite;
            data.spriteName = data.sprite.name;
            data.help = 1;
            data.tag0 = "hq_supp_0";
            data.tag2 = "hq_supp_2";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// HQ Relocates. Handles both start of relocation and relocation completed
    /// </summary>
    /// <param name="text"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public Message HqRelocates(string text, string reason)
    {
        Hq factionHQ = GameManager.i.hqScript.GetCurrentHQ();
        Debug.Assert(string.IsNullOrEmpty(reason) == false, "Invalid reason (Null or Empty)");
        Debug.Assert(factionHQ != null, "Invalid current HQ (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            int timer = GameManager.i.hqScript.GetHqRelocationTimer();
            Message message = new Message();
            message.text = text;
            message.type = MessageType.HQ;
            message.subType = MessageSubType.HQ_Relocates;
            message.sideLevel = factionHQ.side.level;
            message.isPublic = true;
            message.data0 = timer;
            //ItemData
            ItemData data = new ItemData();
            if (timer > 0)
            { data.itemText = string.Format("{0} Relocating", factionHQ.tag); }
            else
            { data.itemText = string.Format("{0} completed RELOCATION", factionHQ.tag); }
            data.topText = "HQ Relocation";
            data.bottomText = GameManager.i.itemDataScript.GetHqRelocationDetails(factionHQ, reason, timer);
            data.priority = ItemPriority.High;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.sprite = factionHQ.sprite;
            data.spriteName = data.sprite.name;
            data.help = 1;
            data.tag0 = "hq_0";
            data.tag1 = "hq_1";
            data.tag2 = "hq_2";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = node.nodeID;
            message.data1 = reductionInCityLoyalty;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = itemText;
            data.topText = "District Crisis";
            data.bottomText = GameManager.i.itemDataScript.GetNodeCrisisDetails(node, reductionInCityLoyalty);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.nodeCrisisSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = globalBoth.level;
            message.isPublic = true;
            message.data0 = node.nodeID;
            message.data1 = node.waitTimer;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0}, {1}, district temporarily IMMUNE to CRISIS", node.nodeName, node.Arc.name);
            data.topText = "District Immunity";
            data.bottomText = GameManager.i.itemDataScript.GetNodeOngoingEffectDetails(node);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.i.spriteScript.nodeCrisisSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = node.nodeID;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
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
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = newCityLoyalty;
            message.data1 = changeInLoyalty;
            //ItemData
            ItemData data = new ItemData();
            if (changeInLoyalty < 0) { data.itemText = string.Format("City Loyalty DECREASES"); }
            else { data.itemText = string.Format("City Loyalty INCREASES"); }
            data.topText = "City Loyalty";
            data.bottomText = GameManager.i.itemDataScript.GetCityLoyaltyDetails(reason, newCityLoyalty, changeInLoyalty);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.i.spriteScript.cityLoyaltySprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    //
    // - - - Objectives - - -
    //

    /// <summary>
    /// Objective has progressed (handles completion). Reason is '[due to]...'
    /// </summary>
    /// <param name="text"></param>
    /// <param name="reason"></param>
    /// <param name="adjustment"></param>
    /// <param name="objective"></param>
    /// <returns></returns>
    public Message ObjectiveProgress(string text, string reason, int adjustment, Objective objective)
    {
        Debug.Assert(objective != null, "Invalid objective (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.OBJECTIVE;
            message.subType = MessageSubType.Objective_Progress;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = adjustment;
            message.data1 = objective.progress;
            message.dataName = objective.name;
            //ItemData
            ItemData data = new ItemData();
            if (objective.progress < 100)
            {
                //progress made
                data.itemText = string.Format("Objective \'{0}\' PROGRESSES", objective.tag);
                data.topText = "Objective Progress";
            }
            else
            {
                //objective completed
                data.topText = "Objective Achieved";
                data.itemText = string.Format("Objective \'{0}\' COMPLETED", objective.tag);
            }
            data.bottomText = GameManager.i.itemDataScript.GetObjectiveProgressDetails(reason, adjustment, objective);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.i.spriteScript.objectiveSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    //
    // - - - Topic
    //

    /// <summary>
    /// Decision Topic outcome
    /// </summary>
    /// <param name="topicData"></param>
    /// <returns></returns>
    public Message TopicDecision(TopicMessageData topicData)
    {
        if (topicData != null)
        {
            if (string.IsNullOrEmpty(topicData.topicName) == true)
            {
                Debug.LogWarning("Invalid topic (Null or Empty)");
                topicData.topicName = "Unknown";
            }
            if (string.IsNullOrEmpty(topicData.optionName) == true)
            {
                Debug.LogWarningFormat("Invalid option (Null or Empty)");
                topicData.optionName = "Ignored";
            }
            if (topicData.sprite == null)
            {
                if (GameManager.i.turnScript.CheckIsAutoRun() == false)
                { Debug.LogWarning("Invalid sprite (Null)"); }
                topicData.sprite = GameManager.i.spriteScript.infoSprite;
            }
            if (string.IsNullOrEmpty(topicData.text) == false)
            {
                Message message = new Message();
                //message.text = text;
                message.type = MessageType.TOPIC;
                message.subType = MessageSubType.Topic_Decision;
                message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
                message.isPublic = true;
                message.data0 = topicData.nodeID;
                message.data1 = topicData.actorID;
                message.dataName = string.Format("\'{0}\', {1}", topicData.topicName, topicData.optionName);
                message.text = topicData.text;
                //ItemData
                ItemData data = new ItemData();
                if (topicData.optionName.Equals("Ignored", System.StringComparison.Ordinal) == false)
                { data.itemText = string.Format("Event '{0}'", topicData.topicName); }
                else { data.itemText = "Event Ignored"; }
                data.topText = topicData.topicName;
                data.bottomText = string.Format("<b>{0}</b>{1}{2}<b>{3}</b>", topicData.optionName, "\n", "\n", topicData.outcome);
                data.priority = ItemPriority.Low;
                data.sprite = topicData.sprite;
                data.spriteName = topicData.spriteName;
                data.tab = ItemTab.ALERTS;
                data.type = message.type;
                data.subType = message.subType;
                data.sideLevel = message.sideLevel;
                data.help = 1;
                data.tag0 = "topicMess_0";
                //add
                GameManager.i.dataScript.AddMessage(message);
                GameManager.i.dataScript.AddItemData(data);
            }
            else { Debug.LogWarning("Invalid text (Null or empty)"); }
        }
        else { Debug.LogWarning("Invalid topicMessageData package (Null)"); }
        return null;
    }

    /// <summary>
    /// Review topic result
    /// </summary>
    /// <param name="text"></param>
    /// <param name="votesFor"></param>
    /// <param name="votesAgainst"></param>
    /// <param name="votesAbstain"></param>
    /// <param name="outcome"></param>
    /// <returns></returns>
    public Message TopicReview(string text, int votesFor, int votesAgainst, int votesAbstain, CampaignOutcome outcome)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.TOPIC;
            message.subType = MessageSubType.Topic_Review;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = votesFor;
            message.data1 = votesAgainst;
            message.data2 = votesAbstain;
            message.dataName = outcome.ToString();
            //ItemData
            ItemData data = new ItemData();
            data.topText = "Peer Review";
            data.bottomText = GameManager.i.itemDataScript.GetTopicReviewDetails(votesFor, votesAgainst, votesAbstain, outcome);
            data.itemText = "Performance REVIEW Outcome";
            data.priority = ItemPriority.Low;
            data.sprite = GameManager.i.spriteScript.topicReviewSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "review_3";
            data.tag1 = "review_4";
            data.tag2 = "review_5";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Organisation reveals secret
    /// </summary>
    /// <param name="text"></param>
    /// <param name="orgName"></param>
    /// <param name="secret"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public Message OrganisationRevealSecret(string text, Organisation org, Secret secret, string reason)
    {
        Debug.Assert(org != null, "Invalid organisation (Null)");
        Debug.Assert(secret != null, "Invalid secret (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ORGANISATION;
            message.subType = MessageSubType.Org_Secret;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.dataName = org.name;
            //ItemData
            ItemData data = new ItemData();
            data.topText = secret.tag;
            data.bottomText = GameManager.i.itemDataScript.GetOrgRevealSecretDetails(org.tag, secret, reason);
            data.itemText = string.Format("{0} reveals your SECRET", org.tag);
            data.priority = ItemPriority.High;
            data.sprite = org.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Player reputation with Org at Zero, can't do anymore favours for them until it improves
    /// </summary>
    /// <param name="text"></param>
    /// <param name="org"></param>
    /// <returns></returns>
    public Message OrganisationReputation(string text, Organisation org)
    {
        Debug.Assert(org != null, "Invalid organisation (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ORGANISATION;
            message.subType = MessageSubType.Org_Reputation;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.dataName = org.name;
            //ItemData
            ItemData data = new ItemData();
            data.topText = "Lost Patience";
            data.bottomText = GameManager.i.itemDataScript.GetOrgReputationDetails(org.tag);
            data.itemText = string.Format("POOR REPUTATION with {0}", org.tag);
            data.priority = ItemPriority.Low;
            data.sprite = org.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }



    /// <summary>
    /// OrgInfo tracks nemesis on behalf of player. Move number in case of nemesis who could make multiple moves in a turn and could have multiple sighting reports (they are time stamped to differentiate)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="node"></param>
    /// <param name="nemesis"></param>
    /// <param name="moveNumber"></param>
    /// <returns></returns>
    public Message OrganisationNemesis(string text, Node node, Nemesis nemesis, int moveNumber = 1)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(nemesis != null, "Invalid nemesis (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            //get org info
            Organisation org = GameManager.i.campaignScript.campaign.orgInfo;
            if (org != null)
            {
                Message message = new Message();
                message.text = text;
                message.type = MessageType.ORGANISATION;
                message.subType = MessageSubType.Org_Nemesis;
                message.sideLevel = globalResistance.level;
                message.data0 = node.nodeID;
                message.data1 = moveNumber;
                //ItemData
                ItemData data = new ItemData();
                data.itemText = string.Format("The {0} track your NEMESIS", org.tag);
                data.topText = string.Format("{0} Direct Feed", org.tag);
                data.bottomText = GameManager.i.itemDataScript.GetOrgNemesisDetails(node, nemesis, org, moveNumber);
                data.priority = ItemPriority.High;
                data.sprite = GameManager.i.spriteScript.aiAlertSprite;
                data.spriteName = data.sprite.name;
                data.tab = ItemTab.ALERTS;
                data.type = message.type;
                data.subType = message.subType;
                data.sideLevel = message.sideLevel;
                data.nodeID = node.nodeID;
                data.help = 1;
                data.tag0 = "orgInfo_0";
                data.tag1 = "orgInfo_1";
                data.tag2 = "orgInfo_2";
                //add
                GameManager.i.dataScript.AddMessage(message);
                GameManager.i.dataScript.AddItemData(data);
            }
            else { Debug.LogWarning("Invalid orgInfo (Null)"); }
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// OrgInfo tracks Erasure Teams
    /// </summary>
    /// <param name="text"></param>
    /// <param name="node"></param>
    /// <param name="team"></param>
    /// <returns></returns>
    public Message OrganisationErasureTeam(string text, Node node, Team team)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(team != null, "Invalid team (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            //get org info
            Organisation org = GameManager.i.campaignScript.campaign.orgInfo;
            if (org != null)
            {
                Message message = new Message();
                message.text = text;
                message.type = MessageType.ORGANISATION;
                message.subType = MessageSubType.Org_Erasure;
                message.sideLevel = globalResistance.level;
                message.data0 = node.nodeID;
                message.data1 = team.teamID;
                //ItemData
                ItemData data = new ItemData();
                data.itemText = string.Format("The {0} track an ERASURE team", org.tag);
                data.topText = string.Format("{0} Direct Feed", org.tag);
                data.bottomText = GameManager.i.itemDataScript.GetOrgErasureTeamDetails(node, team, org);
                data.priority = ItemPriority.High;
                data.sprite = GameManager.i.spriteScript.aiAlertSprite;
                data.spriteName = data.sprite.name;
                data.tab = ItemTab.ALERTS;
                data.type = message.type;
                data.subType = message.subType;
                data.sideLevel = message.sideLevel;
                data.nodeID = node.nodeID;
                data.help = 1;
                data.tag0 = "orgInfo_0";
                data.tag1 = "orgInfo_1";
                data.tag2 = "orgInfo_2";
                //add
                GameManager.i.dataScript.AddMessage(message);
                GameManager.i.dataScript.AddItemData(data);
            }
            else { Debug.LogWarning("Invalid orgInfo (Null)"); }
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    public Message OrganisationNpc(string text, Node node, Npc npc)
    {
        Debug.Assert(node != null, "Invalid node (Null)");
        Debug.Assert(npc != null, "Invalid npc (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            //get org info
            Organisation org = GameManager.i.campaignScript.campaign.orgInfo;
            if (org != null)
            {
                bool isStealthMode = false;
                Message message = new Message();
                message.text = text;
                message.type = MessageType.ORGANISATION;
                message.subType = MessageSubType.Org_Npc;
                message.sideLevel = globalResistance.level;
                message.data0 = node.nodeID;
                message.dataName = npc.tag;
                //ItemData
                ItemData data = new ItemData();
                data.itemText = string.Format("The {0} track the {1}", org.tag, npc.tag);
                data.topText = string.Format("{0} Direct Feed", org.tag);
                //stealth mode check
                if (GameManager.i.missionScript.mission.npc.CheckIfInvisibleMode(node.nodeID) == true)
                { isStealthMode = true; }
                data.bottomText = GameManager.i.itemDataScript.GetOrgNpcDetails(node, npc, org, isStealthMode);
                data.priority = ItemPriority.High;
                data.sprite = GameManager.i.spriteScript.aiAlertSprite;
                data.spriteName = data.sprite.name;
                data.tab = ItemTab.ALERTS;
                data.type = message.type;
                data.subType = message.subType;
                data.sideLevel = message.sideLevel;
                data.nodeID = node.nodeID;
                data.help = 1;
                data.tag0 = "orgInfo_0";
                data.tag1 = "orgInfo_1";
                data.tag2 = "orgInfo_2";
                //extra text to explain stealth mode, ignore otherwise as may confuse player
                if (isStealthMode == true)
                { data.tag2 = "orgInfo_3"; }
                //add
                GameManager.i.dataScript.AddMessage(message);
                GameManager.i.dataScript.AddItemData(data);
            }
            else { Debug.LogWarning("Invalid orgInfo (Null)"); }
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - Npc
    //

    /// <summary>
    /// Npc has arrived in city
    /// </summary>
    /// <param name="text"></param>
    /// <param name="npc"></param>
    /// <returns></returns>
    public Message NpcArrival(string text, Npc npc)
    {
        Debug.Assert(npc != null, "Invalid Npc (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.NPC;
            message.subType = MessageSubType.Npc_Arrival;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = npc.currentStartNode.nodeID;
            message.data1 = npc.currentEndNode.nodeID;
            message.data2 = npc.maxTurns;
            message.dataName = npc.tag;
            //ItemData
            ItemData data = new ItemData();
            data.topText = "HQ Notification";
            data.bottomText = GameManager.i.itemDataScript.GetNpcArrivalDetails(npc);
            data.itemText = string.Format("The {0} arrives in {1}", npc.tag.ToUpper(), GameManager.i.cityScript.GetCityName());
            data.priority = ItemPriority.Medium;
            data.sprite = npc.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            //show start node, if appropriate, if not, end node, if appropriate
            if (npc.nodeStart.isShowMe == true)
            { data.nodeID = npc.currentStartNode.nodeID; }
            else if (npc.nodeEnd.isShowMe == true)
            { data.nodeID = npc.currentEndNode.nodeID; }
            data.tag0 = "npc_0";
            data.tag1 = "npc_1";
            data.tag2 = "npc_2";
            data.tag3 = "npc_3";
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Npc departs city without the Player interacting with them (timed out)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="npc"></param>
    /// <returns></returns>
    public Message NpcDepart(string text, Npc npc)
    {
        Debug.Assert(npc != null, "Invalid Npc (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.NPC;
            message.subType = MessageSubType.Npc_Depart;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = npc.currentStartNode.nodeID;
            message.dataName = npc.tag;
            //ItemData
            ItemData data = new ItemData();
            data.topText = "HQ Notification";
            data.bottomText = GameManager.i.itemDataScript.GetNpcDepartDetails(npc);
            data.itemText = string.Format("The {0} has departed {1}", npc.tag.ToUpper(), GameManager.i.cityScript.GetCityName());
            data.priority = ItemPriority.Medium;
            data.sprite = npc.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = npc.currentNode.nodeID;
            data.tag0 = "npc_0";
            data.tag1 = "npc_4";
            data.tag2 = "npc_1";
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Player interacts with Npc who then departs
    /// </summary>
    /// <param name="text"></param>
    /// <param name="npc"></param>
    /// <returns></returns>
    public Message NpcInteract(string text, Npc npc)
    {
        Debug.Assert(npc != null, "Invalid Npc (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.NPC;
            message.subType = MessageSubType.Npc_Interact;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.isPublic = true;
            message.data0 = npc.currentStartNode.nodeID;
            message.dataName = npc.tag;
            //ItemData
            ItemData data = new ItemData();
            data.topText = string.Format("{0} Departs", npc.tag);
            data.bottomText = GameManager.i.itemDataScript.GetNpcInteractDetails(npc);
            data.itemText = string.Format("The {0} has departed {1}", npc.tag.ToUpper(), GameManager.i.cityScript.GetCityName());
            data.priority = ItemPriority.Medium;
            data.sprite = npc.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.nodeID = npc.currentNode.nodeID;
            data.tag0 = "npc_0";
            data.tag1 = "npc_4";
            data.tag2 = "npc_1";
            data.help = 1;
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Npc ongoing status for Effects tab
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <param name="nemesis"></param>
    /// <returns></returns>
    public Message NpcOngoing(string text, Npc npc)
    {
        Debug.Assert(npc != null, "Invalid npc (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.ONGOING;
            message.subType = MessageSubType.Ongoing_Npc;
            message.sideLevel = globalResistance.level;
            message.data0 = npc.currentNode.nodeID;
            message.data1 = npc.currentEndNode.nodeID;
            message.data2 = npc.timerTurns;
            message.data3 = npc.stealthRating;
            message.dataName = npc.tag;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} current Status", npc.tag);
            data.topText = string.Format("{0} Status", npc.tag);
            data.bottomText = GameManager.i.itemDataScript.GetNpcOngoingEffectDetails(npc);
            data.priority = ItemPriority.High;
            data.sprite = npc.sprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "npc_0";
            data.tag1 = "npc_4";
            data.tag2 = "npc_1";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }


    //
    // - - - Investigations
    //

    /// <summary>
    /// new investigation launched
    /// </summary>
    /// <param name="text"></param>
    /// <param name="invest"></param>
    /// <returns></returns>
    public Message InvestigationNew(string text, Investigation invest)
    {
        Debug.Assert(invest != null, "Invalid Investigation (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.INVESTIGATION;
            message.subType = MessageSubType.Invest_New;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = invest.evidence;
            message.data1 = invest.timer;
            message.dataName = invest.tag;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Investigation launched";
            data.topText = "Under Investigation";
            data.bottomText = GameManager.i.itemDataScript.GetInvestNewDetails(invest);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.investigationSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "invest_0";
            data.tag1 = "invest_1";
            data.tag2 = "invest_2";
            data.tag3 = "invest_3";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Effects tab detailing ongoing investigation
    /// </summary>
    /// <param name="text"></param>
    /// <param name="invest"></param>
    /// <returns></returns>
    public Message InvestigationOngoing(string text, Investigation invest)
    {
        Debug.Assert(invest != null, "Invalid Investigation (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.INVESTIGATION;
            message.subType = MessageSubType.Invest_Ongoing;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = invest.evidence;
            message.data1 = invest.timer;
            message.dataName = invest.tag;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = string.Format("{0} Investigation status", invest.tag);
            data.topText = "Ongoing Investigation";
            data.bottomText = GameManager.i.itemDataScript.GetInvestOngoingDetails(invest);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.investigationSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.Effects;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "invest_1";
            data.tag1 = "invest_5";
            data.tag2 = "invest_4";
            data.tag3 = "invest_3";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// new evidence has come to light in an investigation. Source is a short text specifying where the evidence came in format '[Evidence uncovered by] ...', eg. 'your Lead Investigator'
    /// </summary>
    /// <param name="text"></param>
    /// <param name="invest"></param>
    /// <returns></returns>
    public Message InvestigationEvidence(string text, Investigation invest, string source)
    {
        Debug.Assert(invest != null, "Invalid Investigation (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.INVESTIGATION;
            message.subType = MessageSubType.Invest_Evidence;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = invest.evidence;
            message.data1 = invest.timer;
            message.dataName = invest.tag;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Investigation EVIDENCE uncovered";
            data.topText = "New Evidence";
            data.bottomText = GameManager.i.itemDataScript.GetInvestEvidenceDetails(invest, source);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.investigationSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "invest_1";
            data.tag1 = "invest_5";
            data.tag2 = "invest_4";
            data.tag3 = "invest_3";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// The investigation has reached a conclusion and the timer is ticking down to a resolution
    /// </summary>
    /// <param name="text"></param>
    /// <param name="invest"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public Message InvestigationResolution(string text, Investigation invest)
    {
        Debug.Assert(invest != null, "Invalid Investigation (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.INVESTIGATION;
            message.subType = MessageSubType.Invest_Resolution;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = invest.evidence;
            message.data1 = invest.timer;
            message.dataName = invest.tag;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Investigation counting down to a Resolution";
            data.topText = "Resolution Countdown";
            data.bottomText = GameManager.i.itemDataScript.GetInvestResolutionDetails(invest);
            if (invest.evidence <= 0) { data.priority = ItemPriority.High; }
            else { data.priority = ItemPriority.Medium; }
            data.sprite = GameManager.i.spriteScript.investigationSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "invest_6";
            data.tag1 = "invest_7";
            data.tag2 = "invest_8";
            data.tag3 = "invest_9";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Investigation completed (resolution countdown timer reaches zero)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="invest"></param>
    /// <returns></returns>
    public Message InvestigationCompleted(string text, Investigation invest)
    {
        Debug.Assert(invest != null, "Invalid Investigation (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.INVESTIGATION;
            message.subType = MessageSubType.Invest_Completed;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = invest.evidence;
            message.data1 = invest.timer;
            message.dataName = invest.tag;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Investigation COMPLETED";
            data.topText = "Verdict Enforced";
            data.bottomText = GameManager.i.itemDataScript.GetInvestCompletedDetails(invest);
            data.priority = ItemPriority.High;
            data.sprite = GameManager.i.spriteScript.investigationSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "invest_6";
            data.tag1 = "invest_7";
            data.tag2 = "invest_8";
            data.tag3 = "invest_9";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    /// <summary>
    /// Investigation Dropped (intervention by OrgHQ, for example)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="invest"></param>
    /// <returns></returns>
    public Message InvestigationDropped(string text, Investigation invest)
    {
        Debug.Assert(invest != null, "Invalid Investigation (Null)");
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.INVESTIGATION;
            message.subType = MessageSubType.Invest_Dropped;
            message.sideLevel = GameManager.i.sideScript.PlayerSide.level;
            message.data0 = invest.evidence;
            message.data1 = invest.timer;
            message.dataName = invest.tag;
            //ItemData
            ItemData data = new ItemData();
            data.itemText = "Investigation DROPPED";
            data.topText = "Investigation Falters";
            data.bottomText = GameManager.i.itemDataScript.GetInvestDroppedDetails(invest);
            data.priority = ItemPriority.Medium;
            data.sprite = GameManager.i.spriteScript.investigationSprite;
            data.spriteName = data.sprite.name;
            data.tab = ItemTab.ALERTS;
            data.type = message.type;
            data.subType = message.subType;
            data.sideLevel = message.sideLevel;
            data.help = 1;
            data.tag0 = "invest_6";
            data.tag1 = "invest_7";
            data.tag2 = "invest_8";
            data.tag3 = "invest_9";
            //add
            GameManager.i.dataScript.AddMessage(message);
            GameManager.i.dataScript.AddItemData(data);
        }
        else { Debug.LogWarning("Invalid text (Null or empty)"); }
        return null;
    }

    //
    // - - - Utilities
    //

    /// <summary>
    /// Sets help for various conditions for ActorCondition and ActiveEffect
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="data"></param>
    private void SetConditionHelp(Condition condition, ItemData data)
    {
        if (data != null)
        {
            if (condition != null)
            {
                //tooltip
                switch (condition.tag)
                {
                    case "QUESTIONABLE":
                        data.help = 1;
                        data.tag0 = "questionable_0";
                        data.tag1 = "questionable_1";
                        data.tag2 = "questionable_2";
                        data.tag3 = "questionable_3";
                        break;
                    case "STRESSED":
                        data.help = 1;
                        data.tag0 = "stress_0";
                        data.tag1 = "stress_1";
                        data.tag2 = "stress_2";
                        data.tag3 = "stress_3";
                        break;
                    case "ADDICTED":
                        data.help = 1;
                        data.tag0 = "addict_0";
                        data.tag1 = "addict_1";
                        data.tag2 = "addict_2";
                        data.tag3 = "addict_3";
                        break;
                    case "TAGGED":
                        data.help = 1;
                        data.tag0 = "tagged_0";
                        data.tag1 = "tagged_1";
                        data.tag2 = "tagged_2";
                        break;
                }
            }
            else { Debug.LogWarning("Invalid condition (Null)"); }
        }
        else { Debug.LogWarning("Invalid ItemData (Null)"); }
    }

    //new methods above here
}
