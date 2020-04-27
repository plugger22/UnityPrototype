using gameAPI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles MetaGame UI
/// </summary>
public class MetaGameUI : MonoBehaviour
{
    public Canvas canvasMeta;
    public Canvas canvasScroll;

    public GameObject metaObject;

    [Header("Backgrounds")]
    public Image backgroundLeft;
    public Image backgroundCentre;
    public Image backgroundRight;

    [Header("Buttons")]
    public Button buttonReset;
    public Button buttonConfirm;
    public Button buttonRecommended;

    [Header("HQ Tabs")]
    public GameObject tabBoss;
    public GameObject tabSubBoss1;
    public GameObject tabSubBoss2;
    public GameObject tabSubBoss3;


    [Header("MetaOption Items")]
    public GameObject meta_item_0;
    public GameObject meta_item_1;
    public GameObject meta_item_2;
    public GameObject meta_item_3;
    public GameObject meta_item_4;
    public GameObject meta_item_5;
    public GameObject meta_item_6;
    public GameObject meta_item_7;
    public GameObject meta_item_8;
    public GameObject meta_item_9;
    public GameObject meta_item_10;
    public GameObject meta_item_11;
    public GameObject meta_item_12;
    public GameObject meta_item_13;
    public GameObject meta_item_14;
    public GameObject meta_item_15;
    public GameObject meta_item_16;
    public GameObject meta_item_17;
    public GameObject meta_item_18;
    public GameObject meta_item_19;

    
    private int numOfTabs = 4;    //NOTE: Change this at your peril (default 4 which is number of HQ actors) as data collections and indexes all flow from it
    private int offset = 1;       //used with '(ActorHQ)index + offset' to account for the ActorHQ.enum having index 0 being 'None'

    //button script handlers
    private ButtonInteraction buttonInteractionReset;
    private ButtonInteraction buttonInteractionConfirm;
    private ButtonInteraction buttonInteractionRecommended;

    //arrays
    private GameObject[] tabObjects;
    private MetaInteraction[] tabItems;

    //is MainINfoApp active?
    private bool isRunning;



    private int highlightIndex = -1;                                 //item index of currently highlighted item
    private int maxHighlightIndex = -1;
    private int numOfItemsTotal = 20;                                //hardwired Max number of items -> 30
    private int numOfVisibleItems = 10;                              //hardwired visible items in main page -> 10
    private int numOfItemsCurrent = -1;                              //count of items in current list / page
    private int numOfItemsPrevious = -1;                             //count of items in previous list / page



    //static reference
    private static MetaGameUI metaGameUI;


    /// <summary>
    /// provide a static reference to ModalMetaUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static MetaGameUI Instance()
    {
        if (!metaGameUI)
        {
            metaGameUI = FindObjectOfType(typeof(MetaGameUI)) as MetaGameUI;
            if (!metaGameUI)
            { Debug.LogError("There needs to be one active MetaGameUI script on a GameObject in your scene"); }
        }
        return metaGameUI;
    }


    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadGame:
            case GameState.FollowOnInitialisation:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    /// <summary>
    /// Session start
    /// </summary>
    private void SubInitialiseSessionStart()
    {
        int index = 0;
        //initialise Arrays
        tabObjects = new GameObject[numOfTabs];
        tabItems = new MetaInteraction[numOfTabs];
        //canvas
        canvasScroll.gameObject.SetActive(true);
        //tab components
        if (tabBoss != null)  { tabObjects[index++] = tabBoss; }  else { Debug.LogError("Invalid tabBoss (Null)"); }
        if (tabSubBoss1 != null)  { tabObjects[index++] = tabSubBoss1; }  else { Debug.LogError("Invalid tabSubBoss1 (Null)"); }
        if (tabSubBoss2 != null)  { tabObjects[index++] = tabSubBoss2; }  else { Debug.LogError("Invalid tabSubBoss2 (Null)"); }
        if (tabSubBoss3 != null)  { tabObjects[index++] = tabSubBoss3; }  else { Debug.LogError("Invalid tabSubBoss3 (Null)"); }
        //initialise tab Interaction array
        for (int i = 0; i < tabObjects.Length; i++)
        {
            if (tabObjects[i] != null)
            {
                MetaInteraction metaInteract = tabObjects[i].GetComponent<MetaInteraction>();
                if (metaInteract != null)
                { tabItems[i] = metaInteract; }
                else { Debug.LogErrorFormat("Invalid MetaInteraction (Null) for tabObject[{0}]", i); }
            }
        }
        //buttons
        buttonInteractionReset = buttonReset.GetComponent<ButtonInteraction>();
        buttonInteractionConfirm = buttonConfirm.GetComponent<ButtonInteraction>();
        buttonInteractionRecommended = buttonRecommended.GetComponent<ButtonInteraction>();
        Debug.Assert(buttonInteractionReset != null, "Invalid buttonInteractionReset (Null)");
        Debug.Assert(buttonInteractionConfirm != null, "Invalid buttonInteractionConfirm (Null)");
        Debug.Assert(buttonInteractionRecommended != null, "Invalid buttonInteractionRecommended (Null)");
        buttonInteractionConfirm.SetButton(EventType.MetaClose);
    }
    #endregion

    #region SubInitialiseFastAccess
    /// <summary>
    /// fast access
    /// </summary>
    private void SubInitialiseFastAccess()
    {

    }
    #endregion

    #region SubInitialiseEvents
    /// <summary>
    /// events
    /// </summary>
    private void SubInitialiseEvents()
    {
        //listeners
        EventManager.instance.AddListener(EventType.MetaClose, OnEvent, "MetaGamesUI");
    }
    #endregion

    #endregion

    /// <summary>
    /// Event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {

            case EventType.MetaClose:
                CloseMetaUI();
                break;
            /*case EventType.MainInfoTabOpen:
                OpenTab((int)Param);
                break;
            case EventType.MainInfoHome:
                ExecuteButtonHome();
                break;
            case EventType.MainInfoEnd:
                ExecuteButtonEnd();
                break;
            case EventType.MainInfoBack:
                ExecuteButtonBack();
                break;
            case EventType.MainInfoForward:
                ExecuteButtonForward();
                break;
            case EventType.MainInfoUpArrow:
                ExecuteUpArrow();
                break;
            case EventType.MainInfoDownArrow:
                ExecuteDownArrow();
                break;
            case EventType.MainInfoLeftArrow:
                ExecuteLeftArrow();
                break;
            case EventType.MainInfoRightArrow:
                ExecuteRightArrow();
                break;*/
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    public void InitialiseMetaUI()
    {
        //initialise HQ tabs
        for (int i = 0; i < tabItems.Length; i++)
        {
            if (tabItems[i] != null)
            {
                //sprite
                Actor actor = GameManager.instance.dataScript.GetHQHierarchyActor((ActorHQ)(i + offset));
                if (actor != null)
                { tabItems[i].portrait.sprite = actor.sprite; }
                else
                {
                    //default error sprite if a problem
                    Debug.LogWarningFormat("Invalid actor (Null) for ActorHQ \"{0}\"", (ActorHQ)(i + offset));
                    tabItems[i].portrait.sprite = GameManager.instance.guiScript.errorSprite;
                }
                //title
                tabItems[i].title.text = GameManager.instance.hqScript.GetHqTitle(actor.statusHQ);
            }
            else { Debug.LogErrorFormat("Invalid tabItems[{0}] (Null)", i); }
        }

        //Activate UI
        SetMetaUI();
    }

    /// <summary>
    /// Display MetaGame Player options UI
    /// </summary>
    public void SetMetaUI()
    {

        canvasMeta.gameObject.SetActive(true);
        //set modal status
        GameManager.instance.guiScript.SetIsBlocked(true);
        //set game state
        isRunning = true;
        GameManager.instance.inputScript.SetModalState(new ModalStateData(){ mainState = ModalSubState.MetaGame });
        Debug.LogFormat("[UI] MetaGameUI.cs -> SetMetaUI{0}", "\n");

    }


    /// <summary>
    /// close Meta UI display
    /// </summary>
    private void CloseMetaUI()
    {
        GameManager.instance.tooltipGenericScript.CloseTooltip("MainInfoUI.cs -> CloseMainInfo");
        GameManager.instance.tooltipHelpScript.CloseTooltip("MainInfoUI.cs -> CloseMainInfo");
        canvasMeta.gameObject.SetActive(false);
        GameManager.instance.guiScript.SetIsBlocked(false);
        //set game state
        isRunning = false;
        GameManager.instance.inputScript.ResetStates();
        Debug.LogFormat("[UI] MainInfoUI.cs -> CloseMainInfo{0}", "\n");
        //show top bar UI at completion of meta game
        EventManager.instance.PostNotification(EventType.TopBarShow, this, null, "MetaGameUI.cs -> Show TopBarUI");
    }

    //new methods above here
}
