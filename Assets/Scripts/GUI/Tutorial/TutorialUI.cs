﻿using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// handles all tutorial side bar UI functionality
/// </summary>
public class TutorialUI : MonoBehaviour
{
    [Header("Main Components")]
    public Canvas tutorialCanvas;
    public GameObject tutorialObject;
    public Image mainPanel;

    [Header("Buttons")]
    public Button button0;
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;
    public Button button5;
    public Button button6;
    public Button button7;
    public Button button8;
    public Button button9;

    [Header("Tutorial Control Widget")]
    public Image widget;
    public TextMeshProUGUI widgetLeft;
    public TextMeshProUGUI widgetRight;
    public TextMeshProUGUI widgetQuestion;

    #region Private...
    //button sprites
    private TutorialButtonInteraction interact0;
    private TutorialButtonInteraction interact1;
    private TutorialButtonInteraction interact2;
    private TutorialButtonInteraction interact3;
    private TutorialButtonInteraction interact4;
    private TutorialButtonInteraction interact5;
    private TutorialButtonInteraction interact6;
    private TutorialButtonInteraction interact7;
    private TutorialButtonInteraction interact8;
    private TutorialButtonInteraction interact9;

    //assorted
    private int numOfItems;                                                         //number of active items/buttons for this set
    private TutorialItem currentItem;                                               //currently selected tutorial item

    //collections
    private List<Button> listOfButtons = new List<Button>();
    private List<TutorialButtonInteraction> listOfInteractions = new List<TutorialButtonInteraction>();
    private List<TutorialItem> listOfSetItems = new List<TutorialItem>();                                                      //used to hold dynamic TutorialItems, index corresponds to active set button indexes

    //fast access
    private int maxNumOfItems = -1;
    private int maxOptions = -1;
    private Color colourDialogue;
    private Color colourInfo;
    private Color colourQuestion;
    private Color colourGoal;
    private Color colourCompleted;
    #endregion

    #region static instance...
    private static TutorialUI tutorialUI;

    /// <summary>
    /// Static instance
    /// </summary>
    /// <returns></returns>
    public static TutorialUI Instance()
    {
        if (!tutorialUI)
        {
            tutorialUI = FindObjectOfType(typeof(TutorialUI)) as TutorialUI;
            if (!tutorialUI)
            { Debug.LogError("There needs to be one active TutorialUI script on a GameObject in your scene"); }
        }
        return tutorialUI;
    }
    #endregion


    #region Initialisation...
    /// <summary>
    /// Master Initialisation
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.FollowOnInitialisation:
                //do nothing
                break;
            case GameState.TutorialOptions:
            case GameState.LoadGame:
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
            case GameState.StartUp:
                SubInitialiseAsserts();
                SubInitialiseFastAccess();
                SubInitialiseComponents();
                SubInitialiseAll();
                SubInitialiseEvents();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        maxNumOfItems = GameManager.i.tutorialScript.maxNumOfItems;
        maxOptions = GameManager.i.topicScript.maxOptions;
        Debug.Assert(maxNumOfItems > -1, "Invalid maxNumOfItems (-1)");
        Debug.Assert(maxOptions > -1, "Invalid maxOptions (-1)");
        //colours
        colourDialogue = GameManager.i.uiScript.tutDialogue;
        colourInfo = GameManager.i.uiScript.tutInfo;
        colourQuestion = GameManager.i.uiScript.tutQuestion;
        colourGoal = GameManager.i.uiScript.tutGoal;
        colourCompleted = GameManager.i.uiScript.tutCompleted;
    }
    #endregion

    #region SubInitialiseAsserts
    private void SubInitialiseAsserts()
    {
        Debug.Assert(tutorialCanvas != null, "Invalid tutorialCanvas (Null)");
        Debug.Assert(tutorialObject != null, "Invalid tutorialObject (Null)");
        Debug.Assert(mainPanel != null, "Invalid mainPanel (Null)");
        Debug.Assert(button0 != null, "Invalid button0 (Null)");
        Debug.Assert(button1 != null, "Invalid button1 (Null)");
        Debug.Assert(button2 != null, "Invalid button2 (Null)");
        Debug.Assert(button3 != null, "Invalid button3 (Null)");
        Debug.Assert(button4 != null, "Invalid button4 (Null)");
        Debug.Assert(button5 != null, "Invalid button5 (Null)");
        Debug.Assert(button6 != null, "Invalid button6 (Null)");
        Debug.Assert(button7 != null, "Invalid button7 (Null)");
        Debug.Assert(button8 != null, "Invalid button8 (Null)");
        Debug.Assert(button9 != null, "Invalid button9 (Null)");
        Debug.Assert(widget != null, "Invalid widget (Null)");
        Debug.Assert(widgetLeft != null, "Invalid widgetLeft (Null)");
        Debug.Assert(widgetRight != null, "Invalid widgetRight (Null)");
        Debug.Assert(widgetQuestion != null, "Invalid widgetQuestion (Null)");
    }
    #endregion

    #region SubInitaliseComponents
    private void SubInitialiseComponents()
    {
        //button interaction components -> image sprite
        interact0 = button0.GetComponent<TutorialButtonInteraction>();
        interact1 = button1.GetComponent<TutorialButtonInteraction>();
        interact2 = button2.GetComponent<TutorialButtonInteraction>();
        interact3 = button3.GetComponent<TutorialButtonInteraction>();
        interact4 = button4.GetComponent<TutorialButtonInteraction>();
        interact5 = button5.GetComponent<TutorialButtonInteraction>();
        interact6 = button6.GetComponent<TutorialButtonInteraction>();
        interact7 = button7.GetComponent<TutorialButtonInteraction>();
        interact8 = button8.GetComponent<TutorialButtonInteraction>();
        interact9 = button9.GetComponent<TutorialButtonInteraction>();
        //asserts
        Debug.Assert(interact0 != null, "Invalid interact0 (Null)");
        Debug.Assert(interact1 != null, "Invalid interact1 (Null)");
        Debug.Assert(interact2 != null, "Invalid interact2 (Null)");
        Debug.Assert(interact3 != null, "Invalid interact3 (Null)");
        Debug.Assert(interact4 != null, "Invalid interact4 (Null)");
        Debug.Assert(interact5 != null, "Invalid interact5 (Null)");
        Debug.Assert(interact6 != null, "Invalid interact6 (Null)");
        Debug.Assert(interact7 != null, "Invalid interact7 (Null)");
        Debug.Assert(interact8 != null, "Invalid interact8 (Null)");
        Debug.Assert(interact9 != null, "Invalid interact9 (Null)");
    }
    #endregion

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        tutorialCanvas.gameObject.SetActive(false);
        //set up lists -> listOfButtons
        listOfButtons.Add(button0);
        listOfButtons.Add(button1);
        listOfButtons.Add(button2);
        listOfButtons.Add(button3);
        listOfButtons.Add(button4);
        listOfButtons.Add(button5);
        listOfButtons.Add(button6);
        listOfButtons.Add(button7);
        listOfButtons.Add(button8);
        listOfButtons.Add(button9);
        //button interaction components
        listOfInteractions.Add(interact0);
        listOfInteractions.Add(interact1);
        listOfInteractions.Add(interact2);
        listOfInteractions.Add(interact3);
        listOfInteractions.Add(interact4);
        listOfInteractions.Add(interact5);
        listOfInteractions.Add(interact6);
        listOfInteractions.Add(interact7);
        listOfInteractions.Add(interact8);
        listOfInteractions.Add(interact9);
        //widget
        widget.gameObject.SetActive(true);
        widgetLeft.text = string.Format("{0}", GameManager.i.guiScript.tutArrowLeft);
        widgetRight.text = string.Format("{0}", GameManager.i.guiScript.tutArrowRight);
        widgetQuestion.text = string.Format("{0}", GameManager.i.guiScript.tutQuestion);

    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.TutorialOpenUI, OnEvent, "TutorialUI.cs");
        EventManager.i.AddListener(EventType.TutorialCloseUI, OnEvent, "TutorialUI.cs");
        EventManager.i.AddListener(EventType.TutorialItemOpen, OnEvent, "TutorialUI.cs");
    }
    #endregion

    #endregion



    #region OnEvent
    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.TutorialOpenUI:
                TutorialSet set = Param as TutorialSet;
                SetTutorialUI(set);
                break;
            case EventType.TutorialCloseUI:
                CloseTutorialUI();
                break;
            case EventType.TutorialItemOpen:
                OpenTutorialItem((int)Param);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion


    #region SetTutorialUI
    /// <summary>
    /// Initialises tutorialUI for a specified tutorialSet
    /// </summary>
    private void SetTutorialUI(TutorialSet set)
    {
        if (set != null)
        {
            //populate list with current set tutorial items (indexes match button.buttonInteraction.returnValue)
            listOfSetItems = set.listOfTutorialItems;
            if (listOfSetItems != null)
            {
                //disable all buttons (default)
                for (int i = 0; i < listOfButtons.Count; i++)
                { listOfButtons[i].gameObject.SetActive(false); }
                //activate and populate a button for each item in set
                numOfItems = listOfSetItems.Count;
                if (numOfItems > 0)
                {
                    //only accept upt to max num of items
                    numOfItems = Mathf.Min(numOfItems, maxNumOfItems);
                    TutorialItem item;
                    for (int i = 0; i < numOfItems; i++)
                    {
                        item = listOfSetItems[i];
                        if (item != null)
                        {
                            //activate button
                            listOfButtons[i].gameObject.SetActive(true);
                            //button colours
                            ColorBlock buttonColours = listOfButtons[i].colors;
                            //switch off arrow
                            listOfInteractions[i].arrowImage.gameObject.SetActive(false);
                            //assign event to button along with a unique index #
                            listOfInteractions[i].buttonInteraction.SetButton(EventType.TutorialItemOpen, i);
                            //tooltip
                            listOfInteractions[i].tooltip.tooltipHeader = string.Format("<size=120%><b>{0}</b></size>", GameManager.Formatt(item.tag, ColourType.salmonText));
                            listOfInteractions[i].tooltip.tooltipMain = string.Format("This is a <size=115%><b>{0}</b></size> item", GameManager.Formatt(item.tutorialType.name, ColourType.neutralText));
                            //type of item
                            switch (item.tutorialType.name)
                            {
                                case "Dialogue":
                                    listOfInteractions[i].buttonText.text = "D";
                                    buttonColours.normalColor = colourDialogue;
                                    listOfInteractions[i].buttonText.text = string.Format("{0}", GameManager.i.guiScript.tutDialogue);
                                    listOfInteractions[i].tooltip.tooltipDetails = "Words of wisdom from your <b>training guide</b>";
                                    break;
                                case "Goal":
                                    listOfInteractions[i].buttonText.text = "G";
                                    buttonColours.normalColor = colourGoal;
                                    listOfInteractions[i].buttonText.text = string.Format("{0}", GameManager.i.guiScript.tutGoal);
                                    listOfInteractions[i].tooltip.tooltipDetails = "A specific training goal that you need to achieve <b>in order to progress</b>";
                                    break;
                                case "Information":
                                    listOfInteractions[i].buttonText.text = "I";
                                    buttonColours.normalColor = colourInfo;
                                    listOfInteractions[i].buttonText.text = string.Format("{0}", GameManager.i.guiScript.tutInfo);
                                    listOfInteractions[i].tooltip.tooltipDetails = "<b>Knowledge is power</b>. A detailed look at a topic";
                                    break;
                                case "Question":
                                    listOfInteractions[i].buttonText.text = "?";
                                    buttonColours.normalColor = colourQuestion;
                                    listOfInteractions[i].buttonText.text = string.Format("{0}", GameManager.i.guiScript.tutQuestion);
                                    listOfInteractions[i].tooltip.tooltipDetails = "There are certain things that HQ <b>needs to know</b>";
                                    break;
                                default: Debug.LogWarningFormat("Unrecognised item.TutorialType \"{0}\"", item.tutorialType.name); break;
                            }
                            listOfButtons[i].colors = buttonColours;
                        }
                        else { Debug.LogWarningFormat("Invalid listOfItems[{0}] (Null) for set \"{1}\"", i, set.name); }
                    }
                    //activate canvas
                    tutorialCanvas.gameObject.SetActive(true);
                    //activate first button
                    if (listOfButtons[0] != null)
                    { EventManager.i.PostNotification(EventType.TutorialItemOpen, this, 0); }
                    else { Debug.LogError("Invalid listOfButtons[0] (Null)"); }
                }
                else { Debug.LogErrorFormat("Invalid tutorialSet.listOfItems (Empty) for set \"{0}\"", set.name); }
            }
            else { Debug.LogErrorFormat("Invalid TutorialSet.listOfItems (Null) for set \"{0}\"", set.name); }
        }
        else { Debug.LogError("Invalid TutorialSet (Null)"); }
    }
    #endregion


    #region CloseTutorialUI
    /// <summary>
    /// Close tutorialUI
    /// </summary>
    private void CloseTutorialUI()
    {

        //disable canvas
        tutorialCanvas.gameObject.SetActive(false);
    }
    #endregion


    #region OpenTutorialItem
    /// <summary>
    /// Tutorial button on RHS clicked, open up relevant tutorial UI based on supplied item
    /// </summary>
    /// <param name="item"></param>
    private void OpenTutorialItem(int index = -1)
    {
        if (index > -1)
        {
            //get tutorialItem
            if (index < listOfSetItems.Count)
            {
                currentItem = listOfSetItems[index];
                if (currentItem != null)
                {
                    //switch off all arrows
                    for (int i = 0; i < numOfItems; i++)
                    { listOfInteractions[i].arrowImage.gameObject.SetActive(false); }
                    //display arrow next to selected + 1 tutorial button (shows the next one you need to click on)
                    if (index + 1 < numOfItems)
                    { listOfInteractions[index + 1].arrowImage.gameObject.SetActive(true); }
                    //what type of item
                    switch (currentItem.tutorialType.name)
                    {
                        case "Dialogue":
                            //open special outcome window
                            ModalOutcomeDetails details = new ModalOutcomeDetails()
                            {
                                side = GameManager.i.sideScript.PlayerSide,
                                textTop = GameManager.Formatt(currentItem.topText, ColourType.moccasinText),
                                textBottom = currentItem.bottomText,
                                sprite = GameManager.i.tutorialScript.tutorial.sprite,
                                isAction = false,
                                isSpecial = true,
                                isSpecialGood = true
                            };
                            EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
                            break;
                        case "Goal":
                            GameManager.i.tutorialScript.UpdateGoal(currentItem.goal);
                            //change colour to completed (indicates that you're currently doing, or have done, the goal)
                            ColorBlock goalColours = listOfButtons[index].colors;
                            goalColours.normalColor = colourCompleted;
                            listOfButtons[index].colors = goalColours;
                            break;
                        case "Information":
                            EventManager.i.PostNotification(EventType.GameHelpOpen, this, currentItem.gameHelp);
                            break;
                        case "Question":
                            if (currentItem.isQueryDone == false)
                            {
                                TopicUIData data = GetTopicData(currentItem);
                                if (data != null)
                                { EventManager.i.PostNotification(EventType.TopicDisplayOpen, this, data); }
                                //change colour to completed (indicates that you're currently doing, or have done, the query)
                                ColorBlock itemColours = listOfButtons[index].colors;
                                itemColours.normalColor = colourCompleted;
                                listOfButtons[index].colors = itemColours;
                            }
                            else
                            {
                                //query has already been done
                                ModalOutcomeDetails detailsDone = new ModalOutcomeDetails()
                                {
                                    side = GameManager.i.sideScript.PlayerSide,
                                    textBottom = "Haven't we already covered that?<br><br>I must be having a <b>Senior Moment</b>",
                                    sprite = GameManager.i.tutorialScript.tutorial.sprite,
                                    isAction = false,
                                    isSpecial = true,
                                    isSpecialGood = false
                                };
                                if (string.IsNullOrEmpty(currentItem.queryHeader) == false)
                                { detailsDone.textTop = GameManager.Formatt(currentItem.queryHeader, ColourType.moccasinText); }
                                else
                                {
                                    detailsDone.textTop = GameManager.Formatt("I beg your pardon", ColourType.moccasinText);
                                    Debug.LogWarningFormat("Invalid queryHeader (Null or Empty) for \"{0}\"", currentItem.name);
                                }
                                //special outcome
                                EventManager.i.PostNotification(EventType.OutcomeOpen, this, detailsDone);
                            }
                            break;
                        default: Debug.LogWarningFormat("Unrecognised item.TutorialType \"{0}\"", currentItem.tutorialType.name); break;
                    }
                }
                else { Debug.LogWarningFormat("Invalid currentItem (tutorial) (Null)"); }
            }
            else { Debug.LogWarningFormat("Invalid index (is {0}, listOfSetItems.Count is {1})", index, listOfSetItems.Count); }
        }
        else { Debug.LogWarning("Invalid index (button.buttonInteraction.returnValue (-1)"); }
    }
    #endregion


    #region GetTopicData
    /// <summary>
    /// Populates and returns a data package suitable for TopicUI. Returns null if a problem.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private TopicUIData GetTopicData(TutorialItem item)
    {
        if (item != null)
        {
            int index, limit, count;
            //create temp list by Value (will be deleting
            List<TopicOption> listOfTempOptions = new List<TopicOption>(item.listOfOptions) { };
            //data package
            TopicUIData data = new TopicUIData();
            data.topicName = item.tag;
            data.header = item.queryHeader;
            data.text = item.queryText;
            data.isBoss = false;
            data.nodeID = -1;
            data.uiType = TopicDecisionType.Tutorial;
            data.spriteMain = GameManager.i.tutorialScript.tutorial.sprite;
            data.ignoreTooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.Formatt("Disappointed", ColourType.badText));
            data.ignoreTooltipMain = "Yes, I am. Being able to make a decision is important, what's wrong with you?";
            data.ignoreTooltipDetails = string.Format("<size=115%>{0}</size>", GameManager.Formatt("I'll have to make it for you", ColourType.neutralText));
            //options
            count = item.listOfOptions.Count;
            if (item.isRandomOptions == true)
            {
                limit = Mathf.Min(maxOptions, count);
                //select up to four options randomly
                for (int i = 0; i < limit; i++)
                {
                    index = Random.Range(0, listOfTempOptions.Count);
                    TopicOption option = listOfTempOptions[index];
                    if (option != null)
                    {
                        option.textToDisplay = GameManager.i.topicScript.GetOptionString(option.text);
                        data.listOfOptions.Add(option);
                        listOfTempOptions.RemoveAt(index);
                    }
                    else { Debug.LogWarningFormat("Invalid topicOption (Null) for isRandom listOfTempOptions[{0}]", index); }
                }
            }
            else
            {
                //NOT random, take up to first four options
                limit = Mathf.Min(maxOptions, count);
                for (int i = 0; i < limit; i++)
                {
                    TopicOption option = listOfTempOptions[i];
                    if (option != null)
                    { data.listOfOptions.Add(option); }
                        else { Debug.LogWarningFormat("Invalid topicOption (Null) for normal listOfTempOptions[{0}]", i); }
                }
            }
            //option tooltips and optionNumber
            for (int i = 0; i < data.listOfOptions.Count; i++)
            {
                TopicOption option = data.listOfOptions[i];
                if (option != null)
                {
                    option.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.Formatt(option.tag, ColourType.neutralText));
                    option.tooltipMain = GameManager.i.tutorialScript.GetTutorialJobTooltip();
                    //reassign option number to be the position in the listOfOptions
                    option.optionNumber = i;
                    //set all options as Valid
                    option.isValid = true;
                }
                else { Debug.LogWarningFormat("Invalid topicOption (Null) for data.listOfOptions[{0}]", i); }
            }
            return data;
        }
        else { Debug.LogWarning("Invalid (Tutorial) item (Null)"); }
        return null;
    }
    #endregion


 
}
