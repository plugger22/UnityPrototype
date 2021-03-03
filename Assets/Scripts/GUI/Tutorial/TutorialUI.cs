using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    #region Private...

    //collections
    private List<Button> listOfButtons = new List<Button>();
    private List<ButtonInteraction> listOfButtonInteractions = new List<ButtonInteraction>();

    #endregion

    #region static instance...
    private static TutorialUI tutorialUI;

    /// <summary>
    /// Static instance so the Modal Menu can be accessed from any script
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
                SubInitialiseFastAccess();
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
    }
    #endregion

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        tutorialCanvas.gameObject.SetActive(false);
        //set up lists
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
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.TutorialOpenUI, OnEvent, "TutorialUI.cs");
        EventManager.i.AddListener(EventType.TutorialCloseUI, OnEvent, "TutorialUI.cs");
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
            List<TutorialItem> listOfItems = set.listOfTutorialItems;
            if (listOfItems != null)
            {
                //disable all buttons (default)
                for (int i = 0; i < listOfButtons.Count; i++)
                { listOfButtons[i].gameObject.SetActive(false); }
                //activate and populate a button for each item in set
                int count = listOfItems.Count;
                if (count > 0)
                {
                    TutorialItem item;
                    for (int i = 0; i < count; i++)
                    {
                        item = listOfItems[i];
                        if (item != null)
                        {
                            listOfButtons[i].gameObject.SetActive(true);
                        }
                        else { Debug.LogWarningFormat("Invalid listOfItems[{0}] (Null) for set \"{1}\"", i, set.name); }
                    }
                    //activate canvas
                    tutorialCanvas.gameObject.SetActive(true);
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

}
