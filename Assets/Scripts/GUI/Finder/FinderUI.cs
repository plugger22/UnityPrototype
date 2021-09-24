using gameAPI;
using packageAPI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles RHS Finder (district finder) UI
/// </summary>
public class FinderUI : MonoBehaviour
{

    public Canvas finderCanvas;
    public GameObject finderObject;
    public Image finderImage;
    public ScrollRect finderScrollRect;
    public Scrollbar finderScrollBar;
    public TextMeshProUGUI finderTabText;
    public GenericTooltipUI tabTooltip;

    public List<Button> listOfButtons;                              //index linked
    public List<ButtonInteraction> listOfInteractions;              //index linked
    public List<FinderButtonClick> listOfClicks;                    //index linked
    public List<TextMeshProUGUI> listOfTexts;                       //index linked

    private bool isActive;
    private string colourEnd;
    private Color colourDefault;                                     //used for SetCurrentButton
    private Color colourHighlight;

    //Scrolling
    private int maxNumOfScrollItems;                                //max number of items in scrollable list
    private int numOfScrollItemsVisible = 21;                       //max number of items visible at any one time
    private int numOfScrollItemsCurrent;                            //number of active items
    private int scrollHighlightIndex = -1;                          //current highlight index (doesn't matter if shown as highlighted or not)
    private int scrollMaxHighlightIndex = -1;                       //numOfScrollItemsCurrent - 1
    private int currentIndex;                                       //used to highlight currently selected button
    private int previousIndex;                                      //previously selected button

    //collections
    List<FinderButtonData> listOfStartData = new List<FinderButtonData>();          //start menu list
    List<FinderButtonData> listOfNodeData = new List<FinderButtonData>();           //district list


    #region Static Instance...

    private static FinderUI finderUI;

    /// <summary>
    /// provide a static reference to FinderUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static FinderUI Instance()
    {
        if (!finderUI)
        {
            finderUI = FindObjectOfType(typeof(FinderUI)) as FinderUI;
            if (!finderUI)
            { Debug.LogError("There needs to be one active FinderUI script on a GameObject in your scene"); }
        }
        return finderUI;
    }

    #endregion

    #region Initialise
    /// <summary>
    /// Initialise. Conditional activiation depending on player side for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {

        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
                SubInitialiseAssert();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadAtStart:
                SubInitialiseAssert();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadGame:
                SubInitialiseAssert();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }
    #endregion

    #region Initialise SubMethods...

    #region SubInitialiseAssert
    private void SubInitialiseAssert()
    {
        Debug.Assert(finderCanvas != null, "Invalid finderCanvas (Null)");
        Debug.Assert(finderObject != null, "Invalid finderCanvas (Null)");
        Debug.Assert(finderImage != null, "Invalid finderImage (Null)");
        Debug.Assert(finderScrollRect != null, "Invalid scrollRect (Null)");
        Debug.Assert(finderScrollBar != null, "Invalid scrollBar (Null)");
        Debug.Assert(finderTabText != null, "Invalid finderTabText (Null)");
        Debug.Assert(tabTooltip != null, "Invalid tabTooltip (Null)");
        Debug.Assert(listOfButtons.Count > 0, "Invalid listOfButtons (Empty)");
        Debug.Assert(listOfInteractions.Count > 0, "Invalid listOfInteractions (Empty)");
        Debug.Assert(listOfClicks.Count > 0, "Invalid listOfClicks (Empty)");
        Debug.Assert(listOfTexts.Count > 0, "Invalid listOfTexts (Empty)");
        Debug.AssertFormat(listOfButtons.Count == listOfInteractions.Count, "Count Mismatch: listOfButtons {0} records, listOfInteractions {1} records", listOfButtons.Count, listOfInteractions.Count);
        Debug.AssertFormat(listOfButtons.Count == listOfClicks.Count, "Count Mismatch: listOfButtons {0} records, listOfClicks {1} records", listOfButtons.Count, listOfClicks.Count);
        Debug.AssertFormat(listOfButtons.Count == listOfTexts.Count, "Count Mismatch: listOfButtons {0} records, listOfTexts {1} records", listOfButtons.Count, listOfTexts.Count);
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //fast access
        colourEnd = GameManager.i.colourScript.GetEndTag();
        //set to False
        isActive = false;
        finderImage.gameObject.SetActive(true);
        finderObject.SetActive(true);
        finderCanvas.gameObject.SetActive(false);
        //colours
        colourDefault = GameManager.i.uiScript.FinderDefault;
        colourHighlight = GameManager.i.uiScript.FinderHighlight;
        //Initialisations
        maxNumOfScrollItems = listOfButtons.Count;
        InitialiseFinderData();
        InitialiseFinderTooltip();
        //Side Tab (close)
        finderTabText.text = string.Format("{0}", GameManager.i.guiScript.tutArrowRight);
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.FinderOpen, OnEvent, "FinderUI.cs");
        EventManager.i.AddListener(EventType.FinderClose, OnEvent, "FinderUI.cs");
        EventManager.i.AddListener(EventType.FinderMenu, OnEvent, "FinderUI.cs");
        EventManager.i.AddListener(EventType.FinderDistricts, OnEvent, "FinderUI.cs");
        EventManager.i.AddListener(EventType.FinderScrollUp, OnEvent, "FinderUI.cs");
        EventManager.i.AddListener(EventType.FinderScrollDown, OnEvent, "FinderUI.cs");
        EventManager.i.AddListener(EventType.FinderExecuteButton, OnEvent, "FinderUI.cs");
        EventManager.i.AddListener(EventType.FinderClick, OnEvent, "FinderUI.cs");
    }
    #endregion

    #endregion

    #region InitialiseFinderData
    /// <summary>
    /// Hardcoded (apart from districts) data for finder buttons
    /// </summary>
    public void InitialiseFinderData()
    {
        //
        // - - - start list
        //
        listOfStartData.Add(new FinderButtonData() { descriptor = "<b>DISTRICTS</b>", eventType = EventType.FinderDistricts });
        listOfStartData.Add(new FinderButtonData()
        { descriptor = string.Format("{0}   Corporate", GameManager.i.guiScript.corporateChar), eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc0 });
        listOfStartData.Add(new FinderButtonData()
        { descriptor = string.Format("{0}   Gated", GameManager.i.guiScript.gatedChar), eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc1 });
        listOfStartData.Add(new FinderButtonData()
        { descriptor = string.Format("{0}   Government", GameManager.i.guiScript.governmentChar), eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc2 });
        listOfStartData.Add(new FinderButtonData()
        { descriptor = string.Format("{0}   Industrial", GameManager.i.guiScript.industrialChar), eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc3 });
        listOfStartData.Add(new FinderButtonData()
        { descriptor = string.Format("{0}   Research", GameManager.i.guiScript.researchChar), eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc4 });
        listOfStartData.Add(new FinderButtonData()
        { descriptor = string.Format("{0}   Sprawl", GameManager.i.guiScript.sprawlChar), eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc5 });
        listOfStartData.Add(new FinderButtonData()
        { descriptor = string.Format("{0}   Utility", GameManager.i.guiScript.utilityChar), eventType = EventType.NodeDisplay, nodeType = NodeUI.NodeArc6 });
        listOfStartData.Add(new FinderButtonData() { descriptor = "Targets", eventType = EventType.NodeDisplay, nodeType = NodeUI.ShowTargets });
        listOfStartData.Add(new FinderButtonData() { descriptor = "Spiders", eventType = EventType.NodeDisplay, nodeType = NodeUI.ShowSpiders });
        listOfStartData.Add(new FinderButtonData() { descriptor = "Tracers", eventType = EventType.NodeDisplay, nodeType = NodeUI.ShowTracers });
        listOfStartData.Add(new FinderButtonData() { descriptor = "Teams", eventType = EventType.NodeDisplay, nodeType = NodeUI.ShowTeams });
        listOfStartData.Add(new FinderButtonData() { descriptor = "Contacts", eventType = EventType.NodeDisplay, nodeType = NodeUI.ShowContacts });
        listOfStartData.Add(new FinderButtonData() { descriptor = "Crisis Districts", eventType = EventType.NodeDisplay, nodeType = NodeUI.CrisisNodes });
        listOfStartData.Add(new FinderButtonData() { descriptor = "Cures", eventType = EventType.NodeDisplay, nodeType = NodeUI.CureNodes });
        listOfStartData.Add(new FinderButtonData() { descriptor = "Spotted", eventType = EventType.NodeDisplay, nodeType = NodeUI.PlayerKnown });
        //
        // - - - district list
        //
        List<Node> listOfNodes = new List<Node>(GameManager.i.dataScript.GetListOfAllNodes());
        if (listOfNodes != null)
        {
            int numOfNodes = listOfNodes.Count;
            if (numOfNodes > maxNumOfScrollItems)
            { Debug.LogErrorFormat("Count mismatch: numOfNodes {0}, maxNumOfScrollItems {1}", numOfNodes, maxNumOfScrollItems); }
            else
            {
                //back to start button
                listOfNodeData.Add(new FinderButtonData() { descriptor = string.Format("<size=120%>{0}</size>", GameManager.i.guiScript.tutArrowLeft), eventType = EventType.FinderMenu });
                //sort alphabetically
                var sortedList = listOfNodes.OrderBy(o => o.nodeName);
                listOfNodes = sortedList.ToList();
                Node node;
                //add to list
                for (int i = 0; i < listOfNodes.Count; i++)
                {
                    node = listOfNodes[i];
                    if (node != null)
                    { listOfNodeData.Add(new FinderButtonData() { descriptor = node.nodeName, eventType = EventType.ShowNode, data = node.nodeID }); }
                    else { Debug.LogErrorFormat("Invalid node (Null) for listOfNodes[{0}]", i); }

                }
            }
        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
    }
    #endregion

    #region InitialiseFinderTooltip
    /// <summary>
    /// Sets up tooltips for finderUI
    /// </summary>
    private void InitialiseFinderTooltip()
    {
        //tab tooltip
        if (tabTooltip != null)
        {
            tabTooltip.isIgnoreClick = true;
            tabTooltip.x_offset = -300;
            tabTooltip.testTag = "FinderTabUI";
            tabTooltip.tooltipMain = string.Format("<size=120%>{0}</size>", GameManager.Formatt("CLOSE FINDER", ColourType.salmonText));
            tabTooltip.tooltipDetails = "Click to Close";
        }
        else { Debug.LogError("Invalid tooltip component (Null) for FinderUI"); }
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
            case EventType.FinderOpen:
                SetFinder();
                break;
            case EventType.FinderClose:
                CloseFinder();
                break;
            case EventType.FinderMenu:
                SetFinderButtons();
                break;
            case EventType.FinderDistricts:
                SetFinderButtons(false);
                break;
            case EventType.FinderScrollUp:
                ExecuteScrollUp();
                break;
            case EventType.FinderScrollDown:
                ExecuteScrollDown();
                break;
            case EventType.FinderExecuteButton:
                ExecuteButton();
                break;
            case EventType.FinderClick:
                ExecuteClick((int)Param);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion


    #region SetFinder
    /// <summary>
    /// Display finder UI
    /// </summary>
    private void SetFinder()
    {
        //toggle off finder UI
        EventManager.i.PostNotification(EventType.FinderSideTabClose, this, null, "FinderUI.cs -> SetFinder");
        //initialise
        SetFinderButtons();
        SetFinderScroller();
        //active
        isActive = true;
        //indexes
        currentIndex = 0;
        previousIndex = 0;
        SetCurrentButton();
        //open finder UI
        finderCanvas.gameObject.SetActive(true);
        //set modal status
        GameManager.i.guiScript.SetIsBlocked(true);
        //turn off any alert message and reset nodes
        GameManager.i.alertScript.CloseAlertUI(true);
        //set game state
        ModalStateData package = new ModalStateData();
        package.mainState = ModalSubState.InfoDisplay;
        package.infoState = ModalInfoSubState.Finder;
        GameManager.i.inputScript.SetModalState(package);
        Debug.LogFormat("[UI] FinderUI.cs -> SetFinder{0}", "\n");
    }
    #endregion

    #region SetFinderButtons
    /// <summary>
    /// Set, or Update, finder buttons (depends on whether at start menu or districts menu)
    /// </summary>
    private void SetFinderButtons(bool isStart = true)
    {
        scrollHighlightIndex = 0;
        currentIndex = 0;
        previousIndex = 0;
        FinderButtonData finder;
        if (isStart == true)
        {
            //Start Menu
            numOfScrollItemsCurrent = listOfStartData.Count;
            if (numOfScrollItemsCurrent > maxNumOfScrollItems)
            { Debug.LogErrorFormat("Count mismatch: numOfScrollItemsCurrent {0}, maxNumOfScrollItems {1}", numOfScrollItemsCurrent, maxNumOfScrollItems); }
            else
            {
                scrollMaxHighlightIndex = Mathf.Min(numOfScrollItemsVisible, numOfScrollItemsCurrent);
                for (int i = 0; i < listOfButtons.Count; i++)
                {
                    if (i < numOfScrollItemsCurrent)
                    {
                        finder = listOfStartData[i];
                        if (finder.nodeType != NodeUI.None)
                        { listOfInteractions[i].SetButton(finder.eventType, (int)finder.nodeType); }
                        else { listOfInteractions[i].SetButton(finder.eventType); }
                        listOfTexts[i].text = finder.descriptor;
                        listOfTexts[i].color = colourDefault;
                        listOfButtons[i].gameObject.SetActive(true);
                        listOfClicks[i].SetButton(EventType.FinderClick, i);
                    }
                    else { listOfButtons[i].gameObject.SetActive(false); }
                }
            }
        }
        else
        {
            //District menu
            numOfScrollItemsCurrent = listOfNodeData.Count;
            if (numOfScrollItemsCurrent > maxNumOfScrollItems)
            { Debug.LogErrorFormat("Count mismatch: numOfScrollItemsCurrent {0}, maxNumOfScrollItems {1}", numOfScrollItemsCurrent, maxNumOfScrollItems); }
            else
            {
                scrollMaxHighlightIndex = Mathf.Min(numOfScrollItemsVisible, numOfScrollItemsCurrent);
                for (int i = 0; i < listOfButtons.Count; i++)
                {
                    if (i < numOfScrollItemsCurrent)
                    {
                        finder = listOfNodeData[i];
                        listOfInteractions[i].SetButton(finder.eventType, finder.data);
                        listOfTexts[i].text = finder.descriptor;
                        listOfTexts[i].color = colourDefault;
                        listOfButtons[i].gameObject.SetActive(true);
                    }
                    else { listOfButtons[i].gameObject.SetActive(false); }
                }
            }
        }
        SetCurrentButton();
    }
    #endregion

    #region SetFinderScroller
    /// <summary>
    /// turns scroll bar on/off and sets to default position if on
    /// </summary>
    private void SetFinderScroller()
    {
        //scrollRect
        if (maxNumOfScrollItems > numOfScrollItemsCurrent)
        {
            //toggle on
            finderScrollBar.gameObject.SetActive(true);
            finderScrollRect.verticalScrollbar = finderScrollBar;
            //scroll view at default position
            finderScrollRect.verticalNormalizedPosition = 1.0f;
        }
        else
        {
            //toggle off
            finderScrollRect.verticalScrollbar = null;
            finderScrollBar.gameObject.SetActive(false);
        }
    }
    #endregion

    #region CloseFinder
    /// <summary>
    /// Close Finder UI
    /// </summary>
    private void CloseFinder()
    {
        //close finder UI
        finderCanvas.gameObject.SetActive(false);
        //active
        isActive = false;
        //toggle on finder UI
        EventManager.i.PostNotification(EventType.FinderSideTabOpen, this, null, "FinderUI.cs -> CloseFinder");
        //turn off any alert message and reset nodes
        GameManager.i.alertScript.CloseAlertUI(true);
        //unblock
        GameManager.i.guiScript.SetIsBlocked(false);
        //set game state
        GameManager.i.inputScript.ResetStates();
        Debug.LogFormat("[UI] FinderUI.cs -> CloseFinder{0}", "\n");
    }
    #endregion


    #region ExecuteScrollUp
    /// <summary>
    /// Scroll Mouse UP
    /// </summary>
    private void ExecuteScrollUp()
    {
        if (scrollHighlightIndex > 0)
        {
            scrollHighlightIndex--;
            previousIndex = currentIndex;
            currentIndex--;
            if (finderScrollRect.verticalNormalizedPosition != 1.0f)
            {
                float scrollPos = 1.0f - (float)scrollHighlightIndex / (numOfScrollItemsCurrent - 1);
                finderScrollRect.verticalNormalizedPosition = scrollPos;
            }
            SetCurrentButton();
        }
    }
    #endregion

    #region ExecuteScrollDown
    /// <summary>
    /// scroll mouse DOWN
    /// </summary>
    private void ExecuteScrollDown()
    {
        if (scrollHighlightIndex < numOfScrollItemsCurrent - 1)
        {
            scrollHighlightIndex++;
            previousIndex = currentIndex;
            currentIndex++;
            float scrollPos = 1.0f - (float)scrollHighlightIndex / (numOfScrollItemsCurrent - 1);
            finderScrollRect.verticalNormalizedPosition = scrollPos;
        }
        SetCurrentButton();

    }
    #endregion

    #region ExecuteButton
    /// <summary>
    /// current button activated via 'Enter'
    /// </summary>
    private void ExecuteButton()
    {
        EventManager.i.PostNotification(listOfInteractions[currentIndex].GetEvent(), this, listOfInteractions[currentIndex].GetData(), "FinderUI.cs -> ExecuteButton");
    }
    #endregion

    #region ExecuteClick
    /// <summary>
    /// Mouse click a button. Set highlight
    /// </summary>
    /// <param name="index"></param>
    private void ExecuteClick(int index)
    {
        previousIndex = currentIndex;
        currentIndex = index;
        SetCurrentButton();
    }
    #endregion

    #region Utilities...

    /// <summary>
    /// Highlights current button with a different text colour. Resets text colour on previous button
    /// </summary>
    private void SetCurrentButton()
    {
        listOfTexts[previousIndex].color = colourDefault;
        listOfTexts[currentIndex].color = colourHighlight;
    }


    #endregion

    //new methods above here
}
