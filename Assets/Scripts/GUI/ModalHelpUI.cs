﻿using gameAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles master help UI
/// </summary>
public class ModalHelpUI : MonoBehaviour
{
    //main
    public Canvas masterCanvas;
    public GameObject masterObject;
    public Image mainPanel;
    public Image mainHeaderPanel;
    public TextMeshProUGUI mainHeader;
    //display
    public Image displayPanel;
    public Image displayHeaderPanel;
    public Image helpImage;
    public TextMeshProUGUI displayHeader;
    //scrolling
    public GameObject scrollBarObject;
    public GameObject scrollBackground;                             //needed to get scrollRect component in order to manually disable scrolling when not needed
    public RectTransform scrollContent;                             //instantiated prefab options parented to this
    //prefab
    public GameObject optionPrefab;                                 //reference prefab for game help options (list/scroll)
    //buttons
    public Button closeButton;
    public Button forwardButton;
    public Button homeButton;
    public Button backButton;

    [Header("Quick Access")]
    public Button quickButton0;
    public Button quickButton1;
    public Button quickButton2;
    public Button quickButton3;
    public Button quickButton4;
    public Button quickButton5;
    public Button quickButton6;
    public Button quickButton7;
    public Button quickButton8;
    public Button quickButton9;
    public Button quickButton10;
    public Button quickButton11;
    public Button quickButton12;
    public Button quickButton13;
    public Button quickButton14;

    //button Interactoins
    private ButtonInteraction closeInteraction;
    private ButtonInteraction forwardInteraction;
    private ButtonInteraction homeInteraction;
    private ButtonInteraction backInteraction;

    //scroll bar LHS
    private ScrollRect scrollRect;                                  //needed to manually disable scrolling when not needed
    private Scrollbar scrollBar;

    private bool isOpen;
    private GameObject instanceOption;                              //used for instantiating gameHelp option prefabs

    //item tracking
    private int highlightIndex = -1;                                //item index of currently highlighted item
    private int recentIndex = -1;                                   //most recent index (used to remove highlight) -> Update AFTER a call to ShowHelpItem
    private int numOfItemsTotal = -1;                               //hardwired Max number of items -> 30
    private int historyIndex = -1;
    private int numOfHistoryTotal = -1;

    //home page 
    private GameHelp homePage;                                      //contains quick access buttons to pre-configured sets of gamehelp


    //collections
    private List<GameHelp> listOfHelp = new List<GameHelp>();
    private List<MasterHelpInteraction> listOfInteractions = new List<MasterHelpInteraction>();             //index linked with listOfHelp
    private List<int> listOfHistory = new List<int>();                                                      //tracks page indexes of all browsing history in any given open up session

    //fast access
    private Color colorInactive;                                     //help item text color in list
    private Color colorActive;


    #region static Instance...
    private static ModalHelpUI modalHelpUI;

    /// <summary>
    /// Static instance so the ModalHelpUI can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalHelpUI Instance()
    {
        if (!modalHelpUI)
        {
            modalHelpUI = FindObjectOfType(typeof(ModalHelpUI)) as ModalHelpUI;
            if (!modalHelpUI)
            { Debug.LogError("There needs to be one active ModalHelpUI script on a GameObject in your scene"); }
        }
        return modalHelpUI;
    }
    #endregion

    #region Initialise
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.LoadGame:
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
                SubInitialiseAsserts();
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                //do nothing
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }
    #endregion

    #region Initialise SubMethods...

    #region SubInitialiseAsserts
    /// <summary>
    /// Asserts
    /// </summary>
    private void SubInitialiseAsserts()
    {
        Debug.Assert(masterCanvas != null, "Invalid masterCanvas (Null)");
        Debug.Assert(masterObject != null, "Invalid masterObject (Null)");
        Debug.Assert(mainPanel != null, "Invalid mainPanel (Null)");
        Debug.Assert(mainHeaderPanel != null, "Invalid mainHeaderPanel (Null)");
        Debug.Assert(mainHeader != null, "Invalid mainHeader (Null)");
        Debug.Assert(displayPanel != null, "Invalid displayPanel (Null)");
        Debug.Assert(displayHeaderPanel != null, "Invalid displayHeaderPanel (Null)");
        Debug.Assert(helpImage != null, "Invalid helpImage (Null)");
        Debug.Assert(displayHeader != null, "Invalid displayHeader (Null)");
        Debug.Assert(scrollBackground != null, "Invalid scrollBackground (Null)");
        Debug.Assert(scrollBarObject != null, "Invalid scrollBarObject (Null)");
        Debug.Assert(scrollContent != null, "Invalid scrollContent (Null)");
        Debug.Assert(optionPrefab != null, "Invalid optionPrefab (Null)");
        Debug.Assert(closeButton != null, "Invalid closeButton (Null)");
        Debug.Assert(forwardButton != null, "Invalid forwardButton (Null)");
        Debug.Assert(homeButton != null, "Invalid homeButton (Null)");
        Debug.Assert(backButton != null, "Invalid backButton (Null)");
        Debug.Assert(quickButton0 != null, "Invalid quickButton0 (Null)");
        Debug.Assert(quickButton1 != null, "Invalid quickButton1 (Null)");
        Debug.Assert(quickButton2 != null, "Invalid quickButton2 (Null)");
        Debug.Assert(quickButton3 != null, "Invalid quickButton3 (Null)");
        Debug.Assert(quickButton4 != null, "Invalid quickButton4 (Null)");
        Debug.Assert(quickButton5 != null, "Invalid quickButton5 (Null)");
        Debug.Assert(quickButton6 != null, "Invalid quickButton6 (Null)");
        Debug.Assert(quickButton7 != null, "Invalid quickButton7 (Null)");
        Debug.Assert(quickButton8 != null, "Invalid quickButton8 (Null)");
        Debug.Assert(quickButton9 != null, "Invalid quickButton9 (Null)");
        Debug.Assert(quickButton10 != null, "Invalid quickButton10 (Null)");
        Debug.Assert(quickButton11 != null, "Invalid quickButton11 (Null)");
        Debug.Assert(quickButton12 != null, "Invalid quickButton12 (Null)");
        Debug.Assert(quickButton13 != null, "Invalid quickButton13 (Null)");
        Debug.Assert(quickButton14 != null, "Invalid quickButton14 (Null)");
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        colorInactive = GameManager.i.uiScript.MasterHelpInactive;
        colorActive = GameManager.i.uiScript.MasterHelpActive;
    }
    #endregion

    #region SubInitialiseSessionStart
    /// <summary>
    /// Session Start
    /// </summary>
    private void SubInitialiseSessionStart()
    {
        //switch off
        masterCanvas.gameObject.SetActive(false);
        //set all sub elements to active
        masterObject.SetActive(true);
        mainPanel.gameObject.SetActive(true);
        mainHeaderPanel.gameObject.SetActive(true);
        mainHeader.gameObject.SetActive(true);
        displayPanel.gameObject.SetActive(true);
        displayHeaderPanel.gameObject.SetActive(true);
        helpImage.gameObject.SetActive(true);
        displayHeader.gameObject.SetActive(true);
        // - - - components
        //scrollRect & ScrollBar
        Debug.Assert(scrollBackground != null, "Invalid scrollBackground (Null)");
        Debug.Assert(scrollBarObject != null, "Invalid scrollBarObject (Null)");
        scrollRect = scrollBackground.GetComponent<ScrollRect>();
        scrollBar = scrollBarObject.GetComponent<Scrollbar>();
        Debug.Assert(scrollRect != null, "Invalid scrollRect (Null)");
        Debug.Assert(scrollBar != null, "Invalid scrollBar (Null)");
        //button interactions
        closeInteraction = closeButton.GetComponent<ButtonInteraction>();
        forwardInteraction = forwardButton.GetComponent<ButtonInteraction>();
        homeInteraction = homeButton.GetComponent<ButtonInteraction>();
        backInteraction = backButton.GetComponent<ButtonInteraction>();
        Debug.Assert(closeInteraction != null, "Invalid closeInteraction (Null)");
        Debug.Assert(forwardInteraction != null, "Invalid forwardInteraction (Null)");
        Debug.Assert(homeInteraction != null, "Invalid homeInteraction (Null)");
        Debug.Assert(backInteraction != null, "Invalid backInteraction (Null)");
        //Assign button events
        closeInteraction.SetButton(EventType.MasterHelpClose);
        forwardInteraction.SetButton(EventType.MasterHelpForward);
        homeInteraction.SetButton(EventType.MasterHelpHome);
        backInteraction.SetButton(EventType.MasterHelpBack);
        //Initialise options from prefabs and set up lists
        InitialiseHelpOptions();
    }
    #endregion

    #region SubInitialiseEvents
    /// <summary>
    /// Events
    /// </summary>
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.MasterHelpOpen, OnEvent, "ModalHelpUI");
        EventManager.i.AddListener(EventType.MasterHelpClose, OnEvent, "ModalHelpUI");
        EventManager.i.AddListener(EventType.MasterHelpUpArrow, OnEvent, "ModalHelpUI");
        EventManager.i.AddListener(EventType.MasterHelpDownArrow, OnEvent, "ModalHelpUI");
        EventManager.i.AddListener(EventType.MasterHelpSelect, OnEvent, "ModalHelpUI");
        EventManager.i.AddListener(EventType.MasterHelpForward, OnEvent, "ModalHelpUI");
        EventManager.i.AddListener(EventType.MasterHelpHome, OnEvent, "ModalHelpUI");
        EventManager.i.AddListener(EventType.MasterHelpBack, OnEvent, "ModalHelpUI");
    }
    #endregion

    #endregion

    #region InitialiseHelpOptions
    //Sets up lists and Initialise Option
    private void InitialiseHelpOptions()
    {
        string helpName;
        //clear out help list
        listOfHelp.Clear();
        //home page
        List<GameHelp> tempListOfHomeHelp = GameManager.i.loadScript.arrayOfGameHelp.Where(x => x.isHomePage == true).ToList();
        if (tempListOfHomeHelp != null)
        {
            if (tempListOfHomeHelp.Count == 1)
            {
                //take first, ignore the rest (should only be one)
                homePage = tempListOfHomeHelp[0];
            }
            else { Debug.LogWarningFormat("Invalid number of home pages in arrayOfGameHelp (is {0}, should be {1})", tempListOfHomeHelp.Count, 1); }
        }
        else { Debug.LogError("Invalid home page (Not found in arrayOfGameHelp.isHomePage == true)"); }
        //get list of descriptors
        List<string> tempListOfStrings = GameManager.i.loadScript.arrayOfGameHelp.Where(x => x.isHomePage == false).Select(x => x.descriptor).ToList();
        numOfItemsTotal = tempListOfStrings.Count;
        if (tempListOfStrings != null)
        {
            if (numOfItemsTotal > 0)
            {
                //sort list alphabetically in ascending order
                tempListOfStrings.Sort();
                //temporary List of GameHelp by value (will be deleting)
                List<GameHelp> tempListOfGameHelp = new List<GameHelp>(GameManager.i.loadScript.arrayOfGameHelp.Where(x => x.isHomePage == false));
                Debug.AssertFormat(tempListOfGameHelp.Count == numOfItemsTotal, "Mismatch in count. tempListOfGameHelp has {0} items, numOfItemsTotal is {1} (should be the same)", tempListOfGameHelp.Count, numOfItemsTotal);
                //Set up lists using sort order
                for (int i = 0; i < numOfItemsTotal; i++)
                {
                    helpName = tempListOfStrings[i];
                    if (string.IsNullOrEmpty(helpName) == false)
                    {
                        //find name (reverse loop 'cause we will delete once found
                        for (int j = tempListOfGameHelp.Count - 1; j >= 0; j--)
                        {
                            if (tempListOfGameHelp[j].descriptor.Equals(helpName, StringComparison.Ordinal) == true)
                            {
                                //match
                                listOfHelp.Add(tempListOfGameHelp[j]);
                                //remove to prevent dupes
                                tempListOfGameHelp.RemoveAt(j);
                                //job done exit loop
                                break;
                            }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid HelpName (Null or Empty) for tempListOfStrings[{0}]", i); }
                }
                //check correct number of GameHelp in list
                if (listOfHelp.Count == numOfItemsTotal)
                {
                    //clear out interactions list
                    listOfInteractions.Clear();
                    //initialise prefabs
                    for (int i = 0; i < listOfHelp.Count; i++)
                    {
                        //create help option from prefab
                        instanceOption = Instantiate(optionPrefab) as GameObject;
                        instanceOption.transform.SetParent(scrollContent);
                        instanceOption.SetActive(true);
                        //add interaction
                        MasterHelpInteraction interact = instanceOption.GetComponent<MasterHelpInteraction>();
                        if (interact != null)
                        {
                            listOfInteractions.Add(interact);
                            //add name
                            listOfInteractions[i].text.text = listOfHelp[i].descriptor;
                            listOfInteractions[i].index = i;
                        }
                        else { Debug.LogErrorFormat("Invalid masterHelpInteraction (Null) for listOfHelp[{0}]", i); }
                    }
                    //indexes
                    highlightIndex = -1;
                    recentIndex = 0;
                    //display first item
                    ShowHelpItem();
                }
                else { Debug.LogErrorFormat("Mismatch on count, listOfHelp has {0} items, numOfItemsTotal is {1} (should be the same)", listOfHelp.Count, numOfItemsTotal); }
            }
            else { Debug.LogError("Invalid tempList (Empty)"); }
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }
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
            case EventType.MasterHelpOpen:
                SetHelp();
                break;
            case EventType.MasterHelpClose:
                CloseHelp();
                break;
            case EventType.MasterHelpUpArrow:
                ExecuteUpArrow();
                break;
            case EventType.MasterHelpDownArrow:
                ExecuteDownArrow();
                break;
            case EventType.MasterHelpSelect:
                SelectHelp((int)Param);
                break;
            case EventType.MasterHelpForward:
                ExecuteForward();
                break;
            case EventType.MasterHelpHome:
                ExecuteHome();
                break;
            case EventType.MasterHelpBack:
                ExecuteBack();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion


    #region SetHelp
    /// <summary>
    /// Initialise and run Master help UI
    /// </summary>
    public void SetHelp()
    {
        if (isOpen == false)
        {
            if (GameManager.i.inputScript.ModalState == ModalState.Normal)
            {
                //close any Alert Message
                GameManager.i.alertScript.CloseAlertUI(true);
                //stop tooltips
                GameManager.i.guiScript.SetTooltipsOff();
                //stop background animations
                GameManager.i.animateScript.StopAnimations();
                //toggle on
                masterCanvas.gameObject.SetActive(true);
                //set modal status
                GameManager.i.guiScript.SetIsBlocked(true);
                //set game state
                ModalStateData package = new ModalStateData();
                package.mainState = ModalSubState.InfoDisplay;
                package.infoState = ModalInfoSubState.MasterHelp;
                GameManager.i.inputScript.SetModalState(package);
                //default to home page
                highlightIndex = -1;
                ShowHelpItem();
                recentIndex = -1;
                //clear history
                listOfHistory.Clear();
                //add home page as first history index
                AddHistory();
                //flag as open
                isOpen = true;
                //log
                Debug.LogFormat("[UI] ModalHelpUI.cs -> SetHelp{0}", "\n");
            }
        }
    }
    #endregion

    #region CloseHelp
    /// <summary>
    /// Close Master Help UI
    /// </summary>
    public void CloseHelp()
    {
        masterCanvas.gameObject.SetActive(false);
        isOpen = false;
        //set modal status
        GameManager.i.guiScript.SetIsBlocked(false);
        //set game state
        GameManager.i.inputScript.ResetStates();
        Debug.LogFormat("[UI] ModalHelpUI.cs -> CloseHelp{0}", "\n");
        //start background animations
        GameManager.i.animateScript.StartAnimations();
    }
    #endregion

    #region ExecuteUpArrow
    /// <summary>
    /// Up arrow. Adds to browsing history
    /// </summary>
    private void ExecuteUpArrow()
    {
        if (highlightIndex > 0)
        {
            highlightIndex--;
            ShowHelpItem();
            recentIndex = highlightIndex;
            AddHistory();
        }
    }
    #endregion

    #region ExecuteDownArrow
    /// <summary>
    /// Down arrow. Adds to browsing history
    /// </summary>
    private void ExecuteDownArrow()
    {
        if (highlightIndex < (numOfItemsTotal - 1))
        {
            highlightIndex++;
            ShowHelpItem();
            recentIndex = highlightIndex;
            AddHistory();
        }
    }
    #endregion

    #region ExecuteForward
    /// <summary>
    /// Forward button pressed
    /// </summary>
    private void ExecuteForward()
    {
        if (historyIndex > -1)
        {
            if (historyIndex < numOfHistoryTotal - 1)
            {
                historyIndex++;
                highlightIndex = listOfHistory[historyIndex];
                ShowHelpItem();
                recentIndex = highlightIndex;
            }
        }
    }
    #endregion

    #region ExecuteHome
    /// <summary>
    /// Home button pressed
    /// </summary>
    private void ExecuteHome()
    {
        if (historyIndex > -1)
        {
            if (highlightIndex > -1)
            {
                //return colour to normal for most recent text
                listOfInteractions[recentIndex].text.color = colorInactive;
                //update
                highlightIndex = -1;
                ShowHelpItem();
                recentIndex = -1;
                //reset history index
                historyIndex = 0;
            }
        }
    }
    #endregion

    #region ExecuteBack
    /// <summary>
    /// Back button pressed
    /// </summary>
    private void ExecuteBack()
    {
        if (historyIndex > -1)
        {
            if (historyIndex > 0)
            {
                historyIndex--;
                highlightIndex = listOfHistory[historyIndex];
                ShowHelpItem();
                recentIndex = highlightIndex;
            }
        }
    }
    #endregion

    #region SelectHelp
    /// <summary>
    /// User has clicked on an option. Select that one. Adds to browsing history
    /// </summary>
    /// <param name="index"></param>
    private void SelectHelp(int index)
    {
        if (index > -1 && index < numOfItemsTotal)
        {
            highlightIndex = index;
            ShowHelpItem();
            recentIndex = index;
            AddHistory();
        }
        else { Debug.LogWarningFormat("Invalid index \"{0}\"", index); }
    }
    #endregion


    #region ShowHelpItem
    /// <summary>
    /// Displays help (sprite) for current highlightIndex
    /// </summary>
    private void ShowHelpItem()
    {
        if (highlightIndex > -1 && highlightIndex < numOfItemsTotal)
        {
            helpImage.sprite = listOfHelp[highlightIndex].sprite0;
            displayHeader.text = listOfHelp[highlightIndex].header;
            if (recentIndex > -1)
            {
                //return colour to normal for most recent text
                listOfInteractions[recentIndex].text.color = colorInactive;
            }
            //change colour of selected text
            listOfInteractions[highlightIndex].text.color = colorActive;
            listOfInteractions[highlightIndex].text.text = listOfHelp[highlightIndex].descriptor;
        }
        else if (highlightIndex == -1)
        {
            //home page
            helpImage.sprite = homePage.sprite0;
            displayHeader.text = homePage.header;
        }
        else { Debug.LogErrorFormat("Invalid highlightIndex \"{0}\"", highlightIndex); }
    }
    #endregion

    #region AddHistory
    /// <summary>
    /// Add to listOfHistory (only if Up / Down / Select, NOT if using Fwd / Home / Back)
    /// </summary>
    private void AddHistory()
    {
        if (highlightIndex > -1)
        {
            listOfHistory.Add(highlightIndex);
            //reset index each time a new record added
            numOfHistoryTotal = listOfHistory.Count;
            historyIndex = numOfHistoryTotal - 1;
        }
    }
    #endregion

    #region Utilities...

    /// <summary>
    /// Returns true if UI already open, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool CheckIsOpen()
    { return isOpen; }

    #endregion


    //new methods above here
}
