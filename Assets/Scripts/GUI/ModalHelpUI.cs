using gameAPI;
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
    public Canvas masterCanvas;
    public GameObject masterObject;
    public Image mainPanel;
    public Image mainHeaderPanel;
    public TextMeshProUGUI mainHeader;

    public Image displayPanel;
    public Image displayHeaderPanel;
    public Image helpImage;
    public TextMeshProUGUI displayHeader;

    public GameObject scrollBarObject;
    public GameObject scrollBackground;                             //needed to get scrollRect component in order to manually disable scrolling when not needed
    public RectTransform scrollContent;                             //instantiated prefab options parented to this

    public GameObject optionPrefab;                                 //reference prefab for game help options (list/scroll)

    //scroll bar LHS
    private ScrollRect scrollRect;                                  //needed to manually disable scrolling when not needed
    private Scrollbar scrollBar;

    private GameObject instanceOption;                              //used for instantiating gameHelp option prefabs


    //item tracking
    private int highlightIndex = -1;                                 //item index of currently highlighted item
    private int maxHighlightIndex = -1;
    private int numOfItemsTotal = 30;                                //hardwired Max number of items -> 30
    private int numOfVisibleItems = 10;                              //hardwired visible items in main page -> 10
    private int numOfItemsCurrent = -1;                              //count of items in current list / page
    private int numOfItemsPrevious = -1;                             //count of items in previous list / page

    private List<GameHelp> listOfHelp = new List<GameHelp>();
    private List<MasterHelpInteraction> listOfInteractions = new List<MasterHelpInteraction>();             //index linked with listOfHelp

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
        //initialise array
        listOfHelp = GameManager.i.loadScript.arrayOfGameHelp.ToList();
        if (listOfHelp != null)
        {
            numOfItemsTotal = listOfHelp.Count;
            if (numOfItemsTotal == 0)
            { Debug.LogError("Invalid listOfHelp (Empty)"); }
            else
            {
                //add prefabs for each gameHelp item in list
                for (int i = 0; i < numOfItemsTotal; i++)
                {
                    //create node from prefab
                    instanceOption = Instantiate(optionPrefab) as GameObject;
                    instanceOption.transform.SetParent(scrollContent);
                    instanceOption.SetActive(true);
                    //add to listOfInteractions
                    MasterHelpInteraction interact = instanceOption.GetComponent<MasterHelpInteraction>();
                    if (interact != null)
                    {
                        listOfInteractions.Add(interact);
                        //add name
                        listOfInteractions[i].text.text = listOfHelp[i].name;
                    }
                    else { Debug.LogErrorFormat("Invalid masterHelpInteraction (Null) for listOfHelp[{0}]", i); }
                }
            }
        }
        else { Debug.LogError("Invalid listOfHelp (Null)"); }
    }
    #endregion

    #region SubInitialiseEvents
    /// <summary>
    /// Events
    /// </summary>
    private void SubInitialiseEvents()
    {

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
        masterCanvas.gameObject.SetActive(true);
    }
    #endregion

    /// <summary>
    /// Close Master Help UI
    /// </summary>
    public void CloseHelp()
    {
        masterCanvas.gameObject.SetActive(false);
    }


    //new methods above here
}
