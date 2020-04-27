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

    //NOTE: Change this at your peril (default 4 which is number of HQ actors) as data collections and indexes all flow from it
    private int numOfTabs = 4;

    //button script handlers
    private ButtonInteraction buttonInteractionReset;
    private ButtonInteraction buttonInteractionConfirm;
    private ButtonInteraction buttonInteractionRecommended;

    //arrays
    GameObject[] tabObjects;
    MetaInteraction[] tabItems;



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

    }
    #endregion


    #endregion

    //new methods above here
}
