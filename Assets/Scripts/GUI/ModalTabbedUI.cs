using gameAPI;
using modalAPI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles all matters relating to ModalTabbedUI
/// </summary>
public class ModalTabbedUI : MonoBehaviour
{
    [Header("Canvases")]
    public Canvas tabbedCanvasMain;

    [Header("Control Objects")]
    public GameObject tabbedObjectMain;

    [Header("Backgrounds")]
    public Image backgroundImage;

    [Header("Side Tabs")]
    public GameObject sideTab0;
    public GameObject sideTab1;
    public GameObject sideTab2;
    public GameObject sideTab3;

    [Header("Controller Buttons")]
    public Button buttonSubordinates;
    public Button buttonPlayer;
    public Button buttonHq;

    [Header("Bottom Buttons")]
    public ButtonInteraction buttonInteractionCancel;

    //tabs
    private int currentSideTabIndex = -1;                           //side tabs (top to bottom)
    private int currentTopTabIndex = -1;                            //top tabs (left to right)
    private int maxSideTabIndex;                                    //side tabs only (used for keeping pgUP/DOWN movement within HQ side tabs)
    private int maxTopTabIndex;                                     //top tabs only
    private int numOfSideTabs;                                      //keyed off enum.MetaTabSide
    private int numOfTopTabs;                                       //keyed off enum.MetaTabTop
    private int offset = 1;                                         //used with '(ActorHQ)index + offset' to account for the ActorHQ.enum having index 0 being 'None'
    private float sideTabAlpha = 0.30f;                             //alpha level of side tabs when inactive
    private float topTabAlpha = 0.50f;                              //alpha level of top tabs when inactive

    //sideTab collections
    private GameObject[] arrayOfSideTabObjects;
    private TabbedInteraction[] arrayOfSideTabItems;
    private TabbedSideTabUI[] arrayOfSideTabInteractions;
    private int[] arrayOfSideTabOptions;                            //count of how many ACTIVE options are available for this tab




    private static ModalTabbedUI modalTabbedUI;


    /// <summary>
    /// Static instance so the ModalTabbedUI can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalTabbedUI Instance()
    {
        if (!modalTabbedUI)
        {
            modalTabbedUI = FindObjectOfType(typeof(ModalTabbedUI)) as ModalTabbedUI;
            if (!modalTabbedUI)
            { Debug.LogError("There needs to be one active ModalTabbedUI script on a GameObject in your scene"); }
        }
        return modalTabbedUI;
    }

    /// <summary>
    /// Initialise
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
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


    #region Initialise SubMethods

    #region SubInitialiseAsserts
    private void SubInitialiseAsserts()
    {
        //Asserts
        Debug.Assert(tabbedCanvasMain != null, "Invalid tabbedCanvasMain (Null)");
        Debug.Assert(tabbedObjectMain != null, "Invalid tabbedObjectMain (Null)");
        Debug.Assert(backgroundImage != null, "Invalid backgroundImage (Null)");
        if (buttonInteractionCancel != null)
        { buttonInteractionCancel.SetButton(EventType.TabbedClose); }
        else { Debug.LogError("Invalid buttonInteractionCancal (Null)"); }
        //controller
        Debug.Assert(buttonSubordinates != null, "Invalid buttonSubordinates (Null)");
        Debug.Assert(buttonPlayer != null, "Invalid buttonPlayer (Null)");
        Debug.Assert(buttonHq != null, "Invalid buttonHq (Null)");
    }
    #endregion

    #region SubInitialiseSessionStart
    /// <summary>
    /// Session start
    /// </summary>
    private void SubInitialiseSessionStart()
    {
        int index = 0;
        numOfSideTabs = (int)TabbedUISide.Count;
        //max tabs
        maxSideTabIndex = numOfSideTabs - 1;
        //initialise Side Arrays -> tabs
        arrayOfSideTabObjects = new GameObject[numOfSideTabs];
        arrayOfSideTabItems = new TabbedInteraction[numOfSideTabs];
        arrayOfSideTabInteractions = new TabbedSideTabUI[numOfSideTabs];
        arrayOfSideTabOptions = new int[numOfSideTabs];
        //Side tab components
        if (sideTab0 != null) { arrayOfSideTabObjects[index++] = sideTab0; } else { Debug.LogError("Invalid sideTab0 (Null)"); }
        if (sideTab1 != null) { arrayOfSideTabObjects[index++] = sideTab1; } else { Debug.LogError("Invalid sideTab1 (Null)"); }
        if (sideTab2 != null) { arrayOfSideTabObjects[index++] = sideTab2; } else { Debug.LogError("Invalid sideTab2 (Null)"); }
        if (sideTab3 != null) { arrayOfSideTabObjects[index++] = sideTab3; } else { Debug.LogError("Invalid sideTab3 (Null)"); }
        //initialise side tab arrays -> interaction
        for (int i = 0; i < numOfSideTabs; i++)
        {
            if (arrayOfSideTabObjects[i] != null)
            {
                TabbedInteraction tabbedInteract = arrayOfSideTabObjects[i].GetComponent<TabbedInteraction>();
                if (tabbedInteract != null)
                {
                    arrayOfSideTabItems[i] = tabbedInteract;
                    //tab interaction
                    TabbedSideTabUI sideTab = tabbedInteract.background.GetComponent<TabbedSideTabUI>();
                    if (sideTab != null)
                    {
                        arrayOfSideTabInteractions[i] = sideTab;
                        //identify tabs
                        sideTab.SetTabIndex(i, maxSideTabIndex);
                    }
                    else { Debug.LogErrorFormat("Invalid TabbedSideTabUI (Null) for arrayOfSideTabItems[{0}]", i); }
                }
                else { Debug.LogErrorFormat("Invalid TabbedInteraction (Null) for arrayOfSideTabObject[{0}]", i); }
            }
        }
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.TabbedOpen, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedClose, OnEvent, "ModalTabbedUI");
    }
    #endregion

    #endregion


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
            case EventType.TabbedOpen:
                TabbedUIData details = Param as TabbedUIData;
                SetTabbedUI(details);
                break;
            case EventType.TabbedClose:
                CloseTabbedUI();
                break;
            case EventType.TabbedSideTabOpen:
                OpenSideTab((int)Param);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// run prior to every TabbedUI use. Run from SetTabbedUI
    /// </summary>
    private void InitialiseTabbedUI(TabbedUIWho who)
    {
        Actor actor;
        Color portraitColor, backgroundColor;
        //initialise HQ side tabs'
        for (int index = 0; index < numOfSideTabs; index++)
        {
            if (arrayOfSideTabItems[index] != null)
            {
                backgroundColor = arrayOfSideTabItems[index].background.color;
                portraitColor = arrayOfSideTabItems[index].portrait.color;
                //sprite
                switch (who)
                {
                    case TabbedUIWho.Subordinates:
                        actor = GameManager.i.dataScript.GetHqHierarchyActor((ActorHQ)(index + offset));
                        if (actor != null)
                        { arrayOfSideTabItems[index].portrait.sprite = actor.sprite; }
                        else
                        {
                            //default error sprite if a problem
                            Debug.LogWarningFormat("Invalid actor (Null) for ActorHQ \"{0}\"", (ActorHQ)(index + offset));
                            arrayOfSideTabItems[index].portrait.sprite = GameManager.i.spriteScript.errorSprite;
                        }
                        //title
                        arrayOfSideTabItems[index].title.text = GameManager.i.hqScript.GetHqTitle(actor.statusHQ);
                        break;
                    case TabbedUIWho.HQ:
                        actor = GameManager.i.dataScript.GetHqHierarchyActor((ActorHQ)(index + offset));
                        if (actor != null)
                        { arrayOfSideTabItems[index].portrait.sprite = actor.sprite; }
                        else
                        {
                            //default error sprite if a problem
                            Debug.LogWarningFormat("Invalid actor (Null) for ActorHQ \"{0}\"", (ActorHQ)(index + offset));
                            arrayOfSideTabItems[index].portrait.sprite = GameManager.i.spriteScript.errorSprite;
                        }
                        //title
                        arrayOfSideTabItems[index].title.text = GameManager.i.hqScript.GetHqTitle(actor.statusHQ);
                        break;
                    default: Debug.LogWarningFormat("Unrecognised TabbedUIWho \"{0}\"", who); break;

                }
                //first tab (Boss) should be active on opening, rest passive
                if (index == 0)
                { portraitColor.a = 1.0f; backgroundColor.a = 1.0f; }
                else
                { portraitColor.a = sideTabAlpha; backgroundColor.a = sideTabAlpha; }
                //set colors
                arrayOfSideTabItems[index].portrait.color = portraitColor;
                arrayOfSideTabItems[index].background.color = backgroundColor;
            }
            else { Debug.LogErrorFormat("Invalid tabItems[{0}] (Null)", index); }
        }
    }

    /// <summary>
    /// Initialise and open TabbedUI
    /// </summary>
    private void SetTabbedUI(TabbedUIData details)
    {
        if (details != null)
        {
            bool errorFlag = false;
            //set modal status
            GameManager.i.guiScript.SetIsBlocked(true);
            //initialise
            InitialiseTabbedUI(details.who);
            //tooltips off
            GameManager.i.guiScript.SetTooltipsOff();
            //activate main panel
            tabbedObjectMain.SetActive(true);


            //Activate main canvas -> last
            tabbedCanvasMain.gameObject.SetActive(true);

            //error outcome message if there is a problem
            if (errorFlag == true)
            {
                tabbedObjectMain.SetActive(false);
                //create an outcome window to notify player
                ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
                outcomeDetails.textTop = "There has been a hiccup and the information isn't available";
                outcomeDetails.textBottom = "Sit tight, Thunderbirds are GO!";
                outcomeDetails.side = details.side;
                EventManager.i.PostNotification(EventType.OutcomeOpen, this, outcomeDetails, "ModalInventoryUI.cs -> SetInventoryUI");
            }
            else
            {
                //all good, inventory window displayed
                ModalStateData package = new ModalStateData() { mainState = ModalSubState.InfoDisplay, infoState = ModalInfoSubState.TabbedUI };
                GameManager.i.inputScript.SetModalState(package);
                Debug.LogFormat("[UI] ModalTabbedUI.cs -> SetTabbedUI{0}", "\n");
            }
        }
        else { Debug.LogError("Invalid TabbedUIData (Null)"); }
    }





    /// <summary>
    /// Close TabbedUI
    /// </summary>
    private void CloseTabbedUI()
    {
        tabbedCanvasMain.gameObject.SetActive(false);
        tabbedObjectMain.SetActive(false);
        GameManager.i.guiScript.SetIsBlocked(false);
        //close generic tooltip (safety check)
        GameManager.i.tooltipGenericScript.CloseTooltip("ModalTabbedUI.cs -> CloseTabbedUI");
        //close help tooltip
        GameManager.i.tooltipHelpScript.CloseTooltip("ModalTabbedUI.cs -> CloseTabbedUI");
        //set game state
        GameManager.i.inputScript.ResetStates();
        Debug.LogFormat("[UI] ModalTabbedUI.cs -> CloseTabbedUI{0}", "\n");
    }


    /// <summary>
    /// Open the designated Side tab and close whatever is open
    /// </summary>
    /// <param name="tabIndex"></param>
    private void OpenSideTab(int tabIndex)
    {
        Debug.AssertFormat(tabIndex > -1 && tabIndex < numOfSideTabs, "Invalid tab index {0}", tabIndex);
        //reset Active tabs to reflect new status
        Color portraitColor, backgroundColor;
        for (int index = 0; index < numOfSideTabs; index++)
        {
            portraitColor = arrayOfSideTabItems[index].portrait.color;
            backgroundColor = arrayOfSideTabItems[index].background.color;
            //activate indicated tab and deactivate the rest
            if (index == tabIndex)
            {
                portraitColor.a = 1.0f;
                backgroundColor.a = 1.0f;
            }
            else
            {
                portraitColor.a = sideTabAlpha;
                backgroundColor.a = sideTabAlpha;
            }
            arrayOfSideTabItems[index].portrait.color = portraitColor;
            arrayOfSideTabItems[index].background.color = backgroundColor;
        }
        //update index
        currentSideTabIndex = tabIndex;
    }


    //new methods above here
}
