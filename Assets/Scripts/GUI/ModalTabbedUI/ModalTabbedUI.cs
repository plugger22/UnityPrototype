using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles all matters relating to ModalTabbedUI
/// </summary>
public class ModalTabbedUI : MonoBehaviour
{

    #region public Components
    [Header("Canvases")]
    public Canvas tabbedCanvasMain;

    [Header("Control Objects")]
    public GameObject tabbedObjectMain;

    [Header("Backgrounds")]
    public Image backgroundImage;

    [Header("Page Canvases")]
    public Canvas canvasTab0;               //main page
    public Canvas canvasTab1;               //personality
    public Canvas canvasTab2;               //OnMap Actor contacts
    public Canvas canvasTab3;               //secrets
    public Canvas canvasTab4;               //Player investigations
    public Canvas canvasTab5;               //Player Likes
    public Canvas canvasTab6;               //Gear
    public Canvas canvasTab7;               //history
    public Canvas canvasTab8;               //stats
    public Canvas canvasTab9;               //no Actors (Subordinates or Reserves)

    [Header("Side Tabs")]
    public GameObject sideTab0;
    public GameObject sideTab1;
    public GameObject sideTab2;
    public GameObject sideTab3;

    [Header("Top Tabs")]
    public GameObject topTab0;
    public GameObject topTab1;
    public GameObject topTab2;
    public GameObject topTab3;
    public GameObject topTab4;
    public GameObject topTab5;
    public GameObject topTab6;
    public GameObject topTab7;
    public GameObject topTab8;

    [Header("Controller Buttons")]
    public Button buttonSubordinates;
    public Button buttonPlayer;
    public Button buttonHq;
    public Button buttonReserves;

    [Header("Controller Texts")]
    public TextMeshProUGUI textSubordinates;
    public TextMeshProUGUI textPlayer;
    public TextMeshProUGUI textHq;
    public TextMeshProUGUI textReserves;

    [Header("Help Buttons")]
    public Button buttonHelpClose;

    [Header("Button Interactions")]
    public ButtonInteraction interactClose;
    public ButtonInteraction interactSubordinates;
    public ButtonInteraction interactPlayer;
    public ButtonInteraction interactHq;
    public ButtonInteraction interactReserves;

    [Header("Texts -> Development Only")]
    public TextMeshProUGUI textActorName;
    public TextMeshProUGUI textPageMain;

    [Header("Canvas0 -> Main")]
    public TextMeshProUGUI tab0ActorName;
    public TextMeshProUGUI tab0ActorTitle;
    public TextMeshProUGUI tab0PowerText;
    public TextMeshProUGUI tab0Compatibility;
    public TextMeshProUGUI tab0NonePresent;
    public Image tab0ActorImage;
    public Image tab0MainPanel;
    public TabbedSubHeaderInteraction tab0Header0;
    public TabbedSubHeaderInteraction tab0Header1;
    public TabbedSubHeaderInteraction tab0Header2;
    public TabbedSubHeaderInteraction tab0Header3;
    public TabbedSubHeaderInteraction tab0Header4;
    public TabbedActorInteraction interactActor;

    [Header("Canvas1 -> Personality")]
    public TabbedPersonInteraction tab1Person0;
    public TabbedPersonInteraction tab1Person1;
    public TabbedPersonInteraction tab1Person2;
    public TabbedPersonInteraction tab1Person3;
    public TabbedPersonInteraction tab1Person4;
    public TabbedSubHeaderInteraction tab1Header0;
    public TabbedSubHeaderInteraction tab1Header1;
    public TabbedSubHeaderInteraction tab1Header2;
    public TabbedSubHeaderInteraction tab1Header3;
    public GenericHelpTooltipUI tab1PageHelp;

    [Header("Canvas2 -> Contacts")]
    public TextMeshProUGUI tab2NetworkStrength;
    public TextMeshProUGUI tab2NetworkStars;
    public GenericHelpTooltipUI tab2Help;
    public GenericHelpTooltipUI tab2PageHelp;
    public TabbedContactInteraction tab2Contact0;
    public TabbedContactInteraction tab2Contact1;
    public TabbedContactInteraction tab2Contact2;
    public TabbedContactInteraction tab2Contact3;
    public TabbedContactInteraction tab2Contact4;
    public TabbedContactInteraction tab2Contact5;

    [Header("Canvas3 -> Secrets")]
    public TextMeshProUGUI tab3Header;
    public TextMeshProUGUI tab3NonePresent;
    public TabbedSecretInteraction tab3Secret0;
    public TabbedSecretInteraction tab3Secret1;
    public TabbedSecretInteraction tab3Secret2;
    public TabbedSecretInteraction tab3Secret3;
    public GenericHelpTooltipUI tab3PageHelp;

    [Header("Canvas4 -> Investigations")]
    public TabbedInvestInteraction tab4Invest0;
    public TabbedInvestInteraction tab4Invest1;
    public TabbedInvestInteraction tab4Invest2;
    public GenericHelpTooltipUI tab4PageHelp;
    public TextMeshProUGUI tab4NonePresent;

    [Header("Canvas5 -> Likes")]
    public TabbedLikesInteraction tab5Likes0;       //strongly likes
    public TabbedLikesInteraction tab5Likes1;       //likes
    public TabbedLikesInteraction tab5Likes2;       //strong dislikes
    public TabbedLikesInteraction tab5Likes3;       //dislikes
    public GenericHelpTooltipUI tab5PageHelp;

    [Header("Canvas6 -> Gear")]
    public TextMeshProUGUI tab6Header;
    public TextMeshProUGUI tab6NonePresent;
    //gear
    public Image tab6GearPanel;
    public TabbedGearInteraction tab6Gear0;
    public TabbedGearInteraction tab6Gear1;
    public TabbedGearInteraction tab6Gear2;
    //devices (capture)
    public Image tab6DevicePanel;
    public TabbedDeviceInteraction tab6Device0;
    public TabbedDeviceInteraction tab6Device1;
    public TabbedDeviceInteraction tab6Device2;
    public TabbedDeviceInteraction tab6Device3;
    //option buttons
    public Image tab6OptionBar;
    public TabbedGenericOptionUI tab6Interaction0;
    public TabbedGenericOptionUI tab6Interaction1;
    public GenericHelpTooltipUI tab6PageHelp;

    [Header("Canvas7 -> History")]
    public GameObject tab7ScrollBarObject;
    public GameObject tab7ScrollBackground;                 //needed to get scrollRect component in order to manually disable scrolling when not needed
    public TabbedGenericOptionUI tab7Interaction0;
    public TabbedGenericOptionUI tab7Interaction1;
    [Tooltip("Place all tabbedScrollItems (Dark/Light) in here. NOTE: Make sure they are in the same order as your scroll list, eg. light0, dark0, light1, dark1, light2 ...")]
    public List<GameObject> listOfTab7Items;
    public GenericHelpTooltipUI tab7PageHelp;
    public TextMeshProUGUI tab7Header;

    [Header("Canvas 8 -> Statistics")]
    public TabbedStatInteraction tab8Stat0;
    public TabbedStatInteraction tab8Stat1;
    public TabbedStatInteraction tab8Stat2;
    public TabbedStatInteraction tab8Stat3;
    public TabbedStatInteraction tab8Stat4;
    public TabbedStatInteraction tab8Stat5;
    public GenericHelpTooltipUI tab8PageHelp;

    [Header("Canvas 9 -> Organisations")]
    public Image tab9OrgPanel;
    public TextMeshProUGUI tab9Header;
    public TextMeshProUGUI tab9NoneKnown;
    public TabbedOrgInteraction tab9Org0;
    public TabbedOrgInteraction tab9Org1;
    public TabbedOrgInteraction tab9Org2;
    public TabbedOrgInteraction tab9Org3;
    public TabbedOrgInteraction tab9Org4;
    public GenericHelpTooltipUI tab9PageHelp;
    public Image tab9CorpPanel;
    public TabbedCorpInteraction tab9Corp0;
    public TabbedCorpInteraction tab9Corp1;
    public TabbedCorpInteraction tab9Corp2;
    public TabbedCorpInteraction tab9Corp3;
    public TabbedCorpInteraction tab9Corp4;
    public TabbedGenericOptionUI tab9Interaction0;
    public TabbedGenericOptionUI tab9Interaction1;

    #endregion

    #region private Components
    private ScrollRect tab7ScrollRect;                                  //needed to manually disable scrolling when not needed
    private Scrollbar tab7ScrollBar;

    //help
    private GenericHelpTooltipUI helpClose;

    //tabs (side/top/actorSet)
    private int currentSideTabIndex = -1;                           //side tabs (top to bottom)
    private int currentTopTabIndex = -1;                            //top tabs (left to right)
    private int currentSetIndex = -1;                               //actor sets
    private int maxSideTabIndex;                                    //side tabs only (used for keeping pgUP/DOWN movement within HQ side tabs)
    private int maxTopTabIndex;                                     //top tabs only
    private int maxSetIndex;                                        //actor sets
    private int numOfSideTabs;                                      //keyed off enum.TabbedUISide
    private int numOfTopTabs;                                       //keyed off enum.TabbedUITop
    private int numOfPages;                                         //keyed off enum.TabbedPage
    private int offset = 1;                                         //used with '(ActorHQ)index + offset' to account for the ActorHQ.enum having index 0 being 'None'
    private float sideTabAlpha = 0.50f;                             //alpha level of side tabs when inactive
    private bool isActive;                                          //true if UI open

    //Page0
    private int numOfActors;
    private int maxNumOfConditions = 0;                              //max number of conditions allowed in tab0/page0 subHeader3 'Conditions'
    private int maxNumOfCures = 0;                                   //max number of cures allowed in tab0/page0 subHeader4 'Cures'
    //Page1
    private int maxNumOfPersonalityFactors;
    //Page2
    private int maxNumOfContacts;
    //Page 3
    private int maxNumOfSecrets;
    //Page 4
    private int maxNumOfInvestigations;
    //Page 5
    private int maxNumOfLikes;
    //Page 6
    private int maxNumOfGear;
    private int maxNumOfDevices;
    //Page7
    private int maxNumOfScrollItems;                                //max number of items in scrollable list
    private int numOfScrollItemsVisible = 11;                       //max number of items visible at any one time
    private int numOfScrollItemsCurrent;                            //number of active items
    private int scrollHighlightIndex = -1;                          //current highlight index (doesn't matter if shown as highlighted or not)
    private int scrollMaxHighlightIndex = -1;                       //numOfScrollItemsCurrent - 1
    private TabbedHistory historyOptionIndex;                       //which history option is currently selected, eg. Events / Emotions (Mood/Opinion)
    //Page 8
    private int maxNumOfStatGroups = 6;                             //hardwired prefab instances on page
    //Page 9
    private int maxNumOfOrgs = 5;
    private int maxNumOfCorps;

    //Input data
    private TabbedUIData inputData;

    //static instance
    private static ModalTabbedUI modalTabbedUI;

    //debug
    private bool isAddDebugRecords;                                 //true if a set of debug player history event records have been addeds
    //help tooltips (I don't want this as a global, just a master private field)
    private int x_offset = 200;
    private int y_offset = 40;
    #endregion

    #region Optimisations
    //optimisation -> field for individual pages to keep garbage collection to a minimum
    private StringBuilder builder;
    private int currentTurn;
    private Node nodeContact;
    private Contact contact;
    private Actor actorGeneric;
    private Effect effectSecret;
    private Secret secret;
    private Gear gear;
    private Organisation organisation;
    private MegaCorp megaCorp;
    private Investigation investigation;
    private List<Contact> listOfContacts;
    private List<int> listOfSecretActors;
    private List<Secret> listOfSecrets;
    private List<Organisation> listOfOrganisations;
    private List<MegaCorp> listOfMegaCorps;
    private List<string> listOfGear;
    private List<HelpData> listOfHelpData;
    private List<Investigation> listOfInvestigations;
    private TabbedContactInteraction interactContact;
    private TabbedSecretInteraction interactSecret;
    private TabbedGearInteraction interactGear;
    private TabbedDeviceInteraction interactDevice;
    private TabbedInvestInteraction interactInvest;
    private TabbedStatInteraction interactStat;
    private TabbedOrgInteraction interactOrg;
    private TabbedCorpInteraction interactCorp;
    private PersonProfile interactProfile;
    private Trait interactTrait;
    private string gearName;
    private string knowsSecretHeader;
    private string effectsSecretHeader;
    private string investString;
    private string personString;
    private int counter;

    #endregion

    #region Colours
    //colours
    private Color sideTabActiveColour;
    private Color sideTabDormantColour;
    private Color portraitColour;
    private Color textSideColour;                                       //generic global for toggling Side Tab text alpha settings for active/dormant
    private Color backgroundColour;
    private Color topTabDormantColour;
    private Color topTabActiveColour;
    private Color tabTextActiveColour;                                 //top tab and controller button text colours
    private Color tabTextDormantColour;                                //top tab and controller button text colours
    private Color tabSubHeaderColour;
    private Color tabSubHeaderTextActiveColour;
    private Color tabSubHeaderTextDormantColour;
    private Color tabItemHelpActiveColour;
    private Color tabItemHelpDormantColour;
    private Color personMiddleColour;
    private Color personMiddleAltColour;
    private Color personMiddleActiveColour;
    private Color personBottomColour;
    private Color personBottomTextColour;
    private Color personMiddleTextActiveColour;
    private Color personMiddleTextDormantColour;
    private Color pageOptionActiveColour;                           //history/gear page option selectors
    private Color pageOptionDormantColour;
    private Color contactActiveColour;
    private Color contactInactiveColour;
    private Color secretColour;
    private Color gearColour;
    private Color deviceColour;
    private Color orgColour;
    private Color corpColour;
    private Color investigationColour;
    private Color statItemColour;
    #endregion

    #region Collections
    //
    // - - - Collections
    //
    //canvas collection
    private Canvas[] arrayOfCanvas;
    //sideTab collections
    private GameObject[] arrayOfSideTabObjects;
    private TabbedInteractionSide[] arrayOfSideTabItems;
    private TabbedSideTabUI[] arrayOfSideTabInteractions;
    //topTab collections
    private GameObject[] arrayOfTopTabObjects;
    private Image[] arrayOfTopTabImages;
    private TextMeshProUGUI[] arrayOfTopTabTitles;
    private TabbedTopTabUI[] arrayOfTopTabInteractions;
    private TabbedPage[] arrayOfPages;
    //cached Actor sets
    private Actor[] arrayOfActorsTemp;                              //holds all actors for the current page (excludes Player -> not in array)
    private Actor[] arrayOfSubordinates;
    private Actor[] arrayOfHq;
    private Actor[] arrayOfReserves;
    //Canvas1 -> Personality
    private TabbedPersonInteraction[] arrayOfPersons;
    private int[] arrayOfPlayerFactors;
    //Canvas2 -> Contacts
    private TabbedContactInteraction[] arrayOfContacts;
    //Canvas3 -> Secrets
    private TabbedSecretInteraction[] arrayOfSecrets;
    //Canvas4 -> Investigations
    private TabbedInvestInteraction[] arrayOfInvestigations;
    //Canvas5 -> Likes
    private TabbedLikesInteraction[] arrayOfLikes;
    //Canvas6 -> Gear
    private TabbedGearInteraction[] arrayOfGear;
    private TabbedDeviceInteraction[] arrayOfDevices;
    private CaptureTool[] arrayOfCaptureTools;
    //Canvas7 -> History
    private GameObject[] arrayOfScrollObjects;
    private TabbedScrollInteraction[] arrayOfScrollInteractions;
    private List<string> listOfHistory = new List<string>();
    //Canvas8 -> Stats
    private TabbedStatInteraction[] arrayOfStats;
    //Canvas9 -> Organisations
    private TabbedOrgInteraction[] arrayOfOrgs;
    private TabbedCorpInteraction[] arrayOfCorps;
    private int[] arrayOfMegaCorpReputation;
    #endregion



    #region Instance
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
    #endregion

    #region Initialise
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
    #endregion


    #region Initialise SubMethods

    #region SubInitialiseAsserts
    private void SubInitialiseAsserts()
    {
        //Asserts
        Debug.Assert(tabbedCanvasMain != null, "Invalid tabbedCanvasMain (Null)");
        Debug.Assert(tabbedObjectMain != null, "Invalid tabbedObjectMain (Null)");
        Debug.Assert(backgroundImage != null, "Invalid backgroundImage (Null)");
        //Canvases
        Debug.Assert(canvasTab0 != null, "Invalid canvasTab0 (Null)");
        Debug.Assert(canvasTab1 != null, "Invalid canvasTab1 (Null)");
        Debug.Assert(canvasTab2 != null, "Invalid canvasTab2 (Null)");
        Debug.Assert(canvasTab3 != null, "Invalid canvasTab3 (Null)");
        Debug.Assert(canvasTab4 != null, "Invalid canvasTab4 (Null)");
        Debug.Assert(canvasTab5 != null, "Invalid canvasTab5 (Null)");
        Debug.Assert(canvasTab6 != null, "Invalid canvasTab6 (Null)");
        Debug.Assert(canvasTab7 != null, "Invalid canvasTab7 (Null)");
        Debug.Assert(canvasTab8 != null, "Invalid canvasTab8 (Null)");
        Debug.Assert(canvasTab9 != null, "Invalid canvasTab9 (Null)");
        //button interactions
        if (interactClose != null) { interactClose.SetButton(EventType.TabbedClose); }
        else { Debug.LogError("Invalid interactClose (Null)"); }
        if (interactSubordinates != null) { interactSubordinates.SetButton(EventType.TabbedSubordinates); }
        else { Debug.LogError("Invalid interactSubordinates (Null)"); }
        if (interactPlayer != null) { interactPlayer.SetButton(EventType.TabbedPlayer); }
        else { Debug.LogError("Invalid interactPlayer (Null)"); }
        if (interactHq != null) { interactHq.SetButton(EventType.TabbedHq); }
        else { Debug.LogError("Invalid interactHq (Null)"); }
        if (interactReserves != null) { interactReserves.SetButton(EventType.TabbedReserves); }
        else { Debug.LogError("Invalid interactReserves (Null)"); }
        //controller
        Debug.Assert(buttonSubordinates != null, "Invalid buttonSubordinates (Null)");
        Debug.Assert(buttonPlayer != null, "Invalid buttonPlayer (Null)");
        Debug.Assert(buttonHq != null, "Invalid buttonHq (Null)");
        Debug.Assert(buttonReserves != null, "Invalid buttonReserves (Null)");
        Debug.Assert(textSubordinates != null, "Invalid textSubordinates (Null)");
        Debug.Assert(textPlayer != null, "Invalid textPlayer (Null)");
        Debug.Assert(textHq != null, "Invalid textHq (Null)");
        Debug.Assert(textReserves != null, "Invalid textReserves (Null)");
        //help
        if (buttonHelpClose != null)
        {
            helpClose = buttonHelpClose.GetComponent<GenericHelpTooltipUI>();
            Debug.Assert(helpClose != null, "Invalid helpClose (Null)");
        }
        else { Debug.LogError("Invalid buttonHelpClose (Null)"); }
        //texts
        Debug.Assert(textActorName != null, "Invalid textActorName (Null)");
        Debug.Assert(textPageMain != null, "Invalid textPageMain (Null)");
        //tab0
        Debug.Assert(tab0ActorName != null, "Invalid tab0ActorName (Null)");
        Debug.Assert(tab0ActorTitle != null, "Invalid tab0ActorTitle (Null)");
        Debug.Assert(tab0PowerText != null, "Invalid tab0PowerText (Null)");
        Debug.Assert(tab0NonePresent != null, "Invalid tab0NonePresent (Null)");
        Debug.Assert(tab0Compatibility != null, "Invalid tab0Compatibility (Null)");
        Debug.Assert(tab0ActorImage != null, "Invalid tab0ActorImage (Null)");
        Debug.Assert(tab0MainPanel != null, "Invalid tab0MainPanel (Null)");
        Debug.Assert(tab0Header0 != null, "Invalid tab0Header0 (Null)");
        Debug.Assert(tab0Header1 != null, "Invalid tab0Header1 (Null)");
        Debug.Assert(tab0Header2 != null, "Invalid tab0Header2 (Null)");
        if (tab0Header3 != null) { maxNumOfConditions = tab0Header3.listOfItems.Count; }
        else { Debug.LogError("Invalid tab0Header3 (Null)"); }
        if (tab0Header4 != null) { maxNumOfCures = tab0Header4.listOfItems.Count; }
        else { Debug.LogError("Invalid tab0Header4 (Null)"); }
        Debug.Assert(interactActor != null, "Invalid interactActor (Null)");
        //
        // - - - canvas1
        //
        Debug.Assert(tab1Person0 != null, "Invalid tab1Person0 (Null)");
        Debug.Assert(tab1Person1 != null, "Invalid tab1Person1 (Null)");
        Debug.Assert(tab1Person2 != null, "Invalid tab1Person2 (Null)");
        Debug.Assert(tab1Person3 != null, "Invalid tab1Person3 (Null)");
        Debug.Assert(tab1Person4 != null, "Invalid tab1Person4 (Null)");
        Debug.Assert(tab1Header0 != null, "Invalid tab1Header0 (Null)");
        Debug.Assert(tab1Header1 != null, "Invalid tab1Header1 (Null)");
        Debug.Assert(tab1Header2 != null, "Invalid tab1Header2 (Null)");
        Debug.Assert(tab1Header3 != null, "Invalid tab1Header3 (Null)");
        Debug.Assert(tab1PageHelp != null, "Invalid tab1PageHelp (Null)");
        //
        // - - - canvas2
        //
        Debug.Assert(tab2Contact0 != null, "Invalid tab2Contact0 (Null)");
        Debug.Assert(tab2Contact1 != null, "Invalid tab2Contact1 (Null)");
        Debug.Assert(tab2Contact2 != null, "Invalid tab2Contact2 (Null)");
        Debug.Assert(tab2Contact3 != null, "Invalid tab2Contact3 (Null)");
        Debug.Assert(tab2Contact4 != null, "Invalid tab2Contact4 (Null)");
        Debug.Assert(tab2Contact5 != null, "Invalid tab2Contact5 (Null)");
        Debug.Assert(tab2NetworkStrength != null, "Invalid tab2NetworkStrength (Null)");
        Debug.Assert(tab2NetworkStars != null, "Invalid tab2NetworkStars (Null)");
        Debug.Assert(tab2Help != null, "Invalid tab2Help (Null)");
        Debug.Assert(tab2PageHelp != null, "Invalid tab2PageHelp (Null)");
        //
        // - - - canvas3
        //
        Debug.Assert(tab3Header != null, "Invalid tab3Header (Null)");
        Debug.Assert(tab3NonePresent != null, "Invalid tab3NonePresent (Null)");
        Debug.Assert(tab3Secret0 != null, "Invalid tab3Secret0 (Null)");
        Debug.Assert(tab3Secret1 != null, "Invalid tab3Secret1 (Null)");
        Debug.Assert(tab3Secret2 != null, "Invalid tab3Secret2 (Null)");
        Debug.Assert(tab3Secret3 != null, "Invalid tab3Secret3 (Null)");
        Debug.Assert(tab3PageHelp != null, "Invalid tab3PageHelp (Null)");
        //
        // - - - canvas4
        //
        Debug.Assert(tab4Invest0 != null, "Invalid tab4Invest0 (Null)");
        Debug.Assert(tab4Invest1 != null, "Invalid tab4Invest1 (Null)");
        Debug.Assert(tab4Invest2 != null, "Invalid tab4Invest2 (Null)");
        Debug.Assert(tab4PageHelp != null, "Invalid tab4PageHelp (Null)");
        Debug.Assert(tab4NonePresent != null, "Invalid tab4NonePresent (Null)");
        //
        // - - - canvas5
        //
        Debug.Assert(tab5Likes0 != null, "Invalid tab5Likes0 (Null)");
        Debug.Assert(tab5Likes1 != null, "Invalid tab5Likes1 (Null)");
        Debug.Assert(tab5Likes2 != null, "Invalid tab5Likes2 (Null)");
        Debug.Assert(tab5Likes3 != null, "Invalid tab5Likes3 (Null)");
        Debug.Assert(tab5PageHelp != null, "Invalid tab5PageHelp (Null)");
        //
        // - - - canvas6
        //
        Debug.Assert(tab6Header != null, "Invalid tab6Header (Null)");
        Debug.Assert(tab6NonePresent != null, "Invalid tab6NonePresent (Null)");
        Debug.Assert(tab6GearPanel != null, "Invalid tab6GearPanel (Null)");
        Debug.Assert(tab6Gear0 != null, "Invalid tab6Gear0 (Null)");
        Debug.Assert(tab6Gear1 != null, "Invalid tab6Gear1 (Null)");
        Debug.Assert(tab6Gear2 != null, "Invalid tab6Gear2 (Null)");
        Debug.Assert(tab6DevicePanel != null, "Invalid tab6DevicePanel (Null)");
        Debug.Assert(tab6Device0 != null, "Invalid tab6Device0 (Null)");
        Debug.Assert(tab6Device1 != null, "Invalid tab6Device1 (Null)");
        Debug.Assert(tab6Device2 != null, "Invalid tab6Device2 (Null)");
        Debug.Assert(tab6Device3 != null, "Invalid tab6Device3 (Null)");
        Debug.Assert(tab6PageHelp != null, "Invalid tab6PageHelp (Null)");
        //
        // - - - canvas7
        //
        for (int i = 0; i < listOfTab7Items.Count; i++)
        {
            if (listOfTab7Items[i] == null)
            { Debug.LogErrorFormat("Invalid tab7Item (Null) for listOfTab7Items[{0}]", i); }
        }
        Debug.Assert(tab7PageHelp != null, "Invalid tab7PageHelp (Null)");
        Debug.Assert(tab7Header != null, "Invalid tab7Header (Null)");
        //
        // - - - canvas 8
        //
        Debug.Assert(tab8Stat0 != null, "Invalid tab8Stat0 (Null)");
        Debug.Assert(tab8Stat1 != null, "Invalid tab8Stat1 (Null)");
        Debug.Assert(tab8Stat2 != null, "Invalid tab8Stat2 (Null)");
        Debug.Assert(tab8Stat3 != null, "Invalid tab8Stat3 (Null)");
        Debug.Assert(tab8Stat4 != null, "Invalid tab8Stat4 (Null)");
        Debug.Assert(tab8Stat5 != null, "Invalid tab8Stat5 (Null)");
        Debug.Assert(tab8PageHelp != null, "Invalid tab8PageHelp (Null)");
        //
        // - - - canvas 9
        //
        Debug.Assert(tab9OrgPanel != null, "Invalid tab9MainPanel (Null)");
        Debug.Assert(tab9Header != null, "Invalid tab9Header (Null)");
        Debug.Assert(tab9NoneKnown != null, "Invalid tab9NoneKnown (Null)");
        Debug.Assert(tab9Org0 != null, "Invalid tab9Org0 (Null)");
        Debug.Assert(tab9Org1 != null, "Invalid tab9Org1 (Null)");
        Debug.Assert(tab9Org2 != null, "Invalid tab9Org2 (Null)");
        Debug.Assert(tab9Org3 != null, "Invalid tab9Org3 (Null)");
        Debug.Assert(tab9Org4 != null, "Invalid tab9Org4 (Null)");
        Debug.Assert(tab9PageHelp != null, "Invalid tab9PageHelp (Null)");
        Debug.Assert(tab9CorpPanel != null, "Invalid tab9CorpPanel (Null)");
        Debug.Assert(tab9Corp0 != null, "Invalid tab9Corp0 (Null)");
        Debug.Assert(tab9Corp1 != null, "Invalid tab9Corp1 (Null)");
        Debug.Assert(tab9Corp2 != null, "Invalid tab9Corp2 (Null)");
        Debug.Assert(tab9Corp3 != null, "Invalid tab9Corp3 (Null)");
        Debug.Assert(tab9Corp4 != null, "Invalid tab9Corp4 (Null)");
        Debug.Assert(tab9Interaction0 != null, "Invalid tab9Interaction0 (Null)");
        Debug.Assert(tab9Interaction1 != null, "Invalid tab9Interaction1 (Null)");
    }
    #endregion

    #region SubInitialiseSessionStart
    /// <summary>
    /// Session start
    /// </summary>
    private void SubInitialiseSessionStart()
    {
        isActive = false;
        int index;
        //Colours
        sideTabActiveColour = GameManager.i.uiScript.TabbedSideTabActive;
        sideTabDormantColour = GameManager.i.uiScript.TabbedSideTabDormant;
        topTabActiveColour = GameManager.i.uiScript.TabbedTopTabActive;
        topTabDormantColour = GameManager.i.uiScript.TabbedTopTabDormant;
        tabTextActiveColour = GameManager.i.uiScript.TabbedTextActive;
        tabTextDormantColour = GameManager.i.uiScript.TabbedTextDormant;
        tabSubHeaderColour = GameManager.i.uiScript.TabbedSubHeader;
        tabSubHeaderTextActiveColour = GameManager.i.uiScript.TabbedSubHeaderText;
        personMiddleColour = GameManager.i.uiScript.TabbedPersonMiddle;
        personMiddleAltColour = GameManager.i.uiScript.TabbedPersonMiddleAlt;
        personMiddleActiveColour = GameManager.i.uiScript.TabbedPersonMiddleActive;
        personBottomColour = GameManager.i.uiScript.TabbedPersonBottom;
        personBottomTextColour = GameManager.i.uiScript.TabbedPersonBottomText;
        personMiddleTextActiveColour = GameManager.i.uiScript.TabbedPersonMiddleTextActive;
        personMiddleTextDormantColour = GameManager.i.uiScript.TabbedPersonMiddleTextDormant;
        pageOptionActiveColour = GameManager.i.uiScript.TabbedHistoryOptionActive;
        pageOptionDormantColour = GameManager.i.uiScript.TabbedHistoryOptionDormant;
        contactActiveColour = GameManager.i.uiScript.TabbedContactActive;
        contactInactiveColour = GameManager.i.uiScript.TabbedContactInactive;
        secretColour = GameManager.i.uiScript.TabbedSecretAll;
        gearColour = GameManager.i.uiScript.TabbedGearAll;
        deviceColour = GameManager.i.uiScript.TabbedDeviceAll;
        orgColour = GameManager.i.uiScript.TabbedOrgAll;
        corpColour = GameManager.i.uiScript.TabbedCorpAll;
        investigationColour = GameManager.i.uiScript.TabbedInvestigationAll;
        statItemColour = GameManager.i.uiScript.TabbedStatItem;
        Color tempColour = tabSubHeaderTextActiveColour;
        tempColour.a = 0.60f;
        tabSubHeaderTextDormantColour = tempColour;
        tempColour = tab0Header0.listOfItems[0].image.color;
        tabItemHelpActiveColour = tempColour;
        tempColour.a = 0.60f;
        tabItemHelpDormantColour = tempColour;
        //Tabs and indexes
        currentTopTabIndex = 0;
        numOfSideTabs = (int)TabbedUISide.Count;
        numOfTopTabs = (int)TabbedUITop.Count;
        numOfPages = (int)TabbedPage.Count;
        //max tabs
        maxSideTabIndex = numOfSideTabs - 1;
        maxSetIndex = (int)TabbedUIWho.Count - 1;
        maxTopTabIndex = numOfTopTabs - 1;
        //page1
        maxNumOfPersonalityFactors = GameManager.i.personScript.numOfFactors;
        arrayOfPersons = new TabbedPersonInteraction[maxNumOfPersonalityFactors];
        //page2
        maxNumOfContacts = GameManager.i.contactScript.maxContactsPerActor;
        arrayOfContacts = new TabbedContactInteraction[maxNumOfContacts];
        //page 3
        maxNumOfSecrets = GameManager.i.secretScript.secretMaxNum;
        arrayOfSecrets = new TabbedSecretInteraction[maxNumOfSecrets];
        //page 4
        maxNumOfInvestigations = GameManager.i.playerScript.maxInvestigations;
        arrayOfInvestigations = new TabbedInvestInteraction[maxNumOfInvestigations];
        //page 5
        maxNumOfLikes = 4;
        arrayOfLikes = new TabbedLikesInteraction[maxNumOfLikes];
        //page 7
        maxNumOfScrollItems = listOfTab7Items.Count;
        Debug.Assert(maxNumOfScrollItems > 0, "Invalid maxNumOfScrollItems (Zero or less)");
        //cache likes data
        PlayerLikesData data = GameManager.i.personScript.GetPlayerLikes();
        if (data != null)
        {
            if (data.listOfStrongLikes != null)
            {
                if (data.listOfStrongLikes.Count > 0)
                { tab5Likes0.preferences.text = data.listOfStrongLikes.Aggregate((x, y) => x + "\n" + y); }
                else { tab5Likes0.preferences.text = "None"; }
                //subHeader
                tab5Likes0.subHeader.text = string.Format("Mood {0} +2", GameManager.i.guiScript.opinionIcon);
            }
            else { Debug.LogWarning("Invalid listOfStrongLikes (Null)"); }
            if (data.listOfLikes != null)
            {
                if (data.listOfLikes.Count > 0)
                { tab5Likes1.preferences.text = data.listOfLikes.Aggregate((x, y) => x + "\n" + y); }
                else { tab5Likes1.preferences.text = "None"; }
                //subHeader
                tab5Likes1.subHeader.text = string.Format("Mood {0} +1", GameManager.i.guiScript.opinionIcon);

            }
            else { Debug.LogWarning("Invalid listOfLikes (Null)"); }
            if (data.listOfStrongDislikes != null)
            {
                if (data.listOfStrongDislikes.Count > 0)
                { tab5Likes2.preferences.text = data.listOfStrongDislikes.Aggregate((x, y) => x + "\n" + y); }
                else { tab5Likes2.preferences.text = "None"; }
                //subHeader
                tab5Likes2.subHeader.text = string.Format("Mood {0} -2", GameManager.i.guiScript.opinionIcon);
            }
            else { Debug.LogWarning("Invalid listOfStrongDislikes (Null)"); }
            if (data.listOfDislikes != null)
            {
                if (data.listOfDislikes.Count > 0)
                { tab5Likes3.preferences.text = data.listOfDislikes.Aggregate((x, y) => x + "\n" + y); }
                else { tab5Likes3.preferences.text = "None"; }
                //subHeader
                tab5Likes3.subHeader.text = string.Format("Mood {0} -1", GameManager.i.guiScript.opinionIcon);
            }
            else { Debug.LogWarning("Invalid listOfDislikes (Null)"); }
        }
        else { Debug.LogError("Invalid PlayerLikesData (Null)"); }
        //page 6
        maxNumOfGear = GameManager.i.gearScript.maxNumOfGear;
        arrayOfGear = new TabbedGearInteraction[maxNumOfGear];
        maxNumOfDevices = GameManager.i.captureScript.maxCaptureTools;
        arrayOfDevices = new TabbedDeviceInteraction[maxNumOfDevices];
        arrayOfCaptureTools = new CaptureTool[maxNumOfDevices];

        //page 8
        arrayOfStats = new TabbedStatInteraction[maxNumOfStatGroups];
        //page 9
        maxNumOfCorps = (int)MegaCorpType.Count;
        arrayOfOrgs = new TabbedOrgInteraction[maxNumOfOrgs];
        arrayOfCorps = new TabbedCorpInteraction[maxNumOfCorps];
        arrayOfMegaCorpReputation = new int[maxNumOfCorps];
        //initialise Canvas array
        arrayOfCanvas = new Canvas[numOfPages];
        //initialise Top Tab Arrays
        arrayOfTopTabObjects = new GameObject[numOfTopTabs];
        arrayOfTopTabImages = new Image[numOfTopTabs];
        arrayOfTopTabTitles = new TextMeshProUGUI[numOfTopTabs];
        arrayOfTopTabInteractions = new TabbedTopTabUI[numOfTopTabs];
        arrayOfPages = new TabbedPage[numOfTopTabs];
        //initialise Side Tab Arrays
        arrayOfSideTabObjects = new GameObject[numOfSideTabs];
        arrayOfSideTabItems = new TabbedInteractionSide[numOfSideTabs];
        arrayOfSideTabInteractions = new TabbedSideTabUI[numOfSideTabs];
        arrayOfActorsTemp = new Actor[numOfSideTabs];
        //Canvas array
        arrayOfCanvas[0] = canvasTab0;
        arrayOfCanvas[1] = canvasTab1;
        arrayOfCanvas[2] = canvasTab2;
        arrayOfCanvas[3] = canvasTab3;
        arrayOfCanvas[4] = canvasTab4;
        arrayOfCanvas[5] = canvasTab5;
        arrayOfCanvas[6] = canvasTab6;
        arrayOfCanvas[7] = canvasTab7;
        arrayOfCanvas[8] = canvasTab8;
        arrayOfCanvas[9] = canvasTab9;
        //Top tab components
        index = 0;
        if (topTab0 != null) { arrayOfTopTabObjects[index++] = topTab0; } else { Debug.LogError("Invalid topTab0 (Null)"); }
        if (topTab1 != null) { arrayOfTopTabObjects[index++] = topTab1; } else { Debug.LogError("Invalid topTab1 (Null)"); }
        if (topTab2 != null) { arrayOfTopTabObjects[index++] = topTab2; } else { Debug.LogError("Invalid topTab2 (Null)"); }
        if (topTab3 != null) { arrayOfTopTabObjects[index++] = topTab3; } else { Debug.LogError("Invalid topTab3 (Null)"); }
        if (topTab4 != null) { arrayOfTopTabObjects[index++] = topTab4; } else { Debug.LogError("Invalid topTab4 (Null)"); }
        if (topTab5 != null) { arrayOfTopTabObjects[index++] = topTab5; } else { Debug.LogError("Invalid topTab5 (Null)"); }
        if (topTab6 != null) { arrayOfTopTabObjects[index++] = topTab6; } else { Debug.LogError("Invalid topTab6 (Null)"); }
        if (topTab7 != null) { arrayOfTopTabObjects[index++] = topTab7; } else { Debug.LogError("Invalid topTab7 (Null)"); }
        if (topTab8 != null) { arrayOfTopTabObjects[index++] = topTab8; } else { Debug.LogError("Invalid topTab8 (Null)"); }
        //initialise top tab arrays 
        for (int i = 0; i < numOfTopTabs; i++)
        {
            if (arrayOfTopTabObjects[i] != null)
            {
                //populate top tab component arrays
                TabbedInteractionTop interact = arrayOfTopTabObjects[i].GetComponent<TabbedInteractionTop>();
                if (interact != null)
                {
                    if (interact.topTab != null) { arrayOfTopTabImages[i] = interact.topTab; } else { Debug.LogWarningFormat("Invalid interact.topTab (Null) for arrayOfTopTabObjects[{0}]", i); }
                    if (interact.topTabUI != null)
                    {
                        arrayOfTopTabInteractions[i] = interact.topTabUI;
                        arrayOfTopTabInteractions[i].SetTabIndex(i, maxTopTabIndex);
                    }
                    else { Debug.LogWarningFormat("Invalid interact.topTabUI (Null) for arrayOfTopTabObjects[{0}]", i); }
                    if (interact.text != null) { arrayOfTopTabTitles[i] = interact.text; } else { Debug.LogWarningFormat("Invalid interact.text (Null) for arrayOfTopTabObjects[{0}]", i); }
                }
                else { Debug.LogErrorFormat("Invalid tabbedInteractionTop (Null) for arrayOfTopTabObjects[{0}]", i); }
            }
            else { Debug.LogErrorFormat("Invalid TopTabObject (Null) for arrayOfTopTabObjects[{0}]", i); }
        }
        //Side tab components
        index = 0;
        if (sideTab0 != null) { arrayOfSideTabObjects[index++] = sideTab0; } else { Debug.LogError("Invalid sideTab0 (Null)"); }
        if (sideTab1 != null) { arrayOfSideTabObjects[index++] = sideTab1; } else { Debug.LogError("Invalid sideTab1 (Null)"); }
        if (sideTab2 != null) { arrayOfSideTabObjects[index++] = sideTab2; } else { Debug.LogError("Invalid sideTab2 (Null)"); }
        if (sideTab3 != null) { arrayOfSideTabObjects[index++] = sideTab3; } else { Debug.LogError("Invalid sideTab3 (Null)"); }
        //initialise side tab arrays -> interaction
        for (int i = 0; i < numOfSideTabs; i++)
        {
            if (arrayOfSideTabObjects[i] != null)
            {
                TabbedInteractionSide tabbedInteract = arrayOfSideTabObjects[i].GetComponent<TabbedInteractionSide>();
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
        //
        // - - - Optimisations
        //
        builder = new StringBuilder();
        listOfContacts = new List<Contact>();
        listOfSecretActors = new List<int>();
        listOfSecrets = new List<Secret>();
        listOfGear = new List<string>();
        listOfInvestigations = new List<Investigation>();
        listOfOrganisations = new List<Organisation>();
        listOfMegaCorps = new List<MegaCorp>();
        knowsSecretHeader = GameManager.Formatt("Who Knows?", ColourType.neutralText);
        effectsSecretHeader = GameManager.Formatt("Effects if Revealed", ColourType.neutralText);
        //
        // - - - Page 0  
        //
        //Sub Header colours
        tab0Header0.image.color = tabSubHeaderColour;
        tab0Header1.image.color = tabSubHeaderColour;
        tab0Header2.image.color = tabSubHeaderColour;
        tab0Header3.image.color = tabSubHeaderColour;
        tab0Header4.image.color = tabSubHeaderColour;
        //
        // - - - Page 1  
        //
        //Personality Matrix setup
        arrayOfPersons[0] = tab1Person0;
        arrayOfPersons[1] = tab1Person1;
        arrayOfPersons[2] = tab1Person2;
        arrayOfPersons[3] = tab1Person3;
        arrayOfPersons[4] = tab1Person4;
        //player personality factors
        arrayOfPlayerFactors = GameManager.i.playerScript.GetPersonality().GetFactors();
        Debug.Assert(arrayOfPlayerFactors != null, "Invalid arrayOfPlayerFactors (Null)");
        Debug.AssertFormat(arrayOfPlayerFactors.Length == maxNumOfPersonalityFactors, "Invalid arrayOfPlayerFactors (has {0} records, should be {1})", arrayOfPlayerFactors.Length, maxNumOfPersonalityFactors);
        //personality factors
        string[] arrayOfFactorTags = GameManager.i.dataScript.GetArrayOfFactorTags();
        if (arrayOfFactorTags != null)
        {
            if (arrayOfFactorTags.Length == arrayOfPersons.Length)
            {
                //initialise matrixes
                for (int i = 0; i < arrayOfPersons.Length; i++)
                {
                    TabbedPersonInteraction interact = arrayOfPersons[i];
                    if (interact != null)
                    {
                        //background colours
                        interact.middleFarLeft.color = personMiddleColour;
                        interact.middleLeft.color = personMiddleAltColour;
                        interact.middleCentre.color = personMiddleColour;
                        interact.middleRight.color = personMiddleAltColour;
                        interact.middleFarRight.color = personMiddleColour;
                        interact.bottom.color = personBottomColour;
                        interact.textMiddleFarLeft.color = personMiddleTextDormantColour;
                        interact.textMiddleLeft.color = personMiddleTextDormantColour;
                        interact.textMiddleCentre.color = personMiddleTextDormantColour;
                        interact.textMiddleRight.color = personMiddleTextDormantColour;
                        interact.textMiddleFarRight.color = personMiddleTextDormantColour;
                        //switch off top
                        interact.topFarLeft.gameObject.SetActive(false);
                        interact.topLeft.gameObject.SetActive(false);
                        interact.topCentre.gameObject.SetActive(false);
                        interact.topRight.gameObject.SetActive(false);
                        interact.topFarRight.gameObject.SetActive(false);
                        //factor name
                        interact.textBottom.text = arrayOfFactorTags[i];
                        interact.textBottom.color = personBottomTextColour;
                    }
                    else { Debug.LogErrorFormat("Invalid tabbedPersonInteraction (Null) in arrayOfPersons[{0}]", i); }
                }
            }
            else { Debug.LogErrorFormat("Mismatch on arrays (should be identical), arrayOfFactorsTags has {0} records, arrayOfPersons has {1} records", arrayOfFactorTags.Length, arrayOfPersons.Length); }
        }
        else { Debug.LogError("Invalid arrayOfFactorTags (Null)"); }
        //subheaders
        tab1Header0.image.color = tabSubHeaderColour;
        tab1Header1.image.color = tabSubHeaderColour;
        tab1Header2.image.color = tabSubHeaderColour;
        tab1Header3.image.color = tabSubHeaderColour;
        //
        // - - - Page 2 Contacts
        //
        arrayOfContacts[0] = tab2Contact0;
        arrayOfContacts[1] = tab2Contact1;
        arrayOfContacts[2] = tab2Contact2;
        arrayOfContacts[3] = tab2Contact3;
        arrayOfContacts[4] = tab2Contact4;
        arrayOfContacts[5] = tab2Contact5;
        //
        // - - - Page 3 Secrets
        //
        arrayOfSecrets[0] = tab3Secret0;
        arrayOfSecrets[1] = tab3Secret1;
        arrayOfSecrets[2] = tab3Secret2;
        arrayOfSecrets[3] = tab3Secret3;
        //
        // - - - Page 4 Investigations
        //
        arrayOfInvestigations[0] = tab4Invest0;
        arrayOfInvestigations[1] = tab4Invest1;
        arrayOfInvestigations[2] = tab4Invest2;
        //
        // - - - Page 5 Likes
        //
        arrayOfLikes[0] = tab5Likes0;
        arrayOfLikes[1] = tab5Likes1;
        arrayOfLikes[2] = tab5Likes2;
        arrayOfLikes[3] = tab5Likes3;
        //subheaders
        for (int i = 0; i < arrayOfLikes.Length; i++)
        { arrayOfLikes[i].background.color = tabSubHeaderColour; }
        //
        // - - - Page 6 Gear
        //
        /*gearOptionIndex = TabbedGear.Normal;*/
        arrayOfGear[0] = tab6Gear0;
        arrayOfGear[1] = tab6Gear1;
        arrayOfGear[2] = tab6Gear2;
        //devices
        arrayOfDevices[0] = tab6Device0;
        arrayOfDevices[1] = tab6Device1;
        arrayOfDevices[2] = tab6Device2;
        arrayOfDevices[3] = tab6Device3;
        //capture tool
        arrayOfCaptureTools[0] = GameManager.i.captureScript.innocence_0;
        arrayOfCaptureTools[1] = GameManager.i.captureScript.innocence_1;
        arrayOfCaptureTools[2] = GameManager.i.captureScript.innocence_2;
        arrayOfCaptureTools[3] = GameManager.i.captureScript.innocence_3;
        for (int i = 0; i < arrayOfCaptureTools.Length; i++)
        { if (arrayOfCaptureTools[i] == null) { Debug.LogErrorFormat("Invalid arrayOfCaptureTools[{0}] (Null)", i); } }
        //gear options (Player only)
        Debug.Assert(tab6OptionBar != null, "Invalid tab6OptionBar (Null)");
        if (tab6Interaction0 != null) { tab6Interaction0.SetEvent(EventType.TabbedGearNormal); } else { Debug.LogError("Invalid tab6Interaction0 (Null)"); }
        if (tab6Interaction1 != null) { tab6Interaction1.SetEvent(EventType.TabbedGearCapture); } else { Debug.LogError("Invalid tab6Interaction1 (Null)"); }
        //devices

        //
        // - - - Page 7 History
        //
        historyOptionIndex = TabbedHistory.Events;
        arrayOfScrollObjects = new GameObject[maxNumOfScrollItems];
        arrayOfScrollInteractions = new TabbedScrollInteraction[maxNumOfScrollItems];
        //scrollRect & ScrollBar
        Debug.Assert(tab7ScrollBackground != null, "Invalid tab7ScrollBackground (Null)");
        Debug.Assert(tab7ScrollBarObject != null, "Invalid tab7ScrollBarObject (Null)");
        tab7ScrollRect = tab7ScrollBackground.GetComponent<ScrollRect>();
        tab7ScrollBar = tab7ScrollBarObject.GetComponent<Scrollbar>();
        Debug.Assert(tab7ScrollRect != null, "Invalid tab7ScrollRect (Null)");
        Debug.Assert(tab7ScrollBar != null, "Invalid tab7ScrollBar (Null)");
        //history options
        if (tab7Interaction0 != null) { tab7Interaction0.SetEvent(EventType.TabbedHistoryEvents); } else { Debug.LogError("Invalid tab7Interaction0 (Null)"); }
        if (tab7Interaction1 != null) { tab7Interaction1.SetEvent(EventType.TabbedHistoryEmotions); } else { Debug.LogError("Invalid tab7Interaction1 (Null)"); }
        //scrollable items-> populate arrays
        for (int i = 0; i < listOfTab7Items.Count; i++)
        {
            arrayOfScrollObjects[i] = listOfTab7Items[i];
            arrayOfScrollInteractions[i] = listOfTab7Items[i].GetComponent<TabbedScrollInteraction>();
        }
        //error check
        for (int i = 0; i < arrayOfScrollObjects.Length; i++)
        {
            if (arrayOfScrollObjects[i] == null)
            { Debug.LogErrorFormat("Invalid scroll Object (Null) in arrayOfScrollObjects[{0}]", i); }
            if (arrayOfScrollInteractions[i] == null)
            { Debug.LogErrorFormat("Invalid scroll Interaction (Null) in arrayOfScrollInteractions[{0}]", i); }
        }
        //disable all
        DisableHistoryScrollItems();
        //
        // - - - Page 8 Statistics
        //
        arrayOfStats[0] = tab8Stat0;
        arrayOfStats[1] = tab8Stat1;
        arrayOfStats[2] = tab8Stat2;
        arrayOfStats[3] = tab8Stat3;
        arrayOfStats[4] = tab8Stat4;
        arrayOfStats[5] = tab8Stat5;
        //subheaders and statItem colours
        for (int i = 0; i < arrayOfStats.Length; i++)
        {
            arrayOfStats[i].background.color = tabSubHeaderColour;
            for (int j = 0; j < arrayOfStats[i].listOfItems.Count; j++)
            { arrayOfStats[i].listOfItems[j].background.color = statItemColour; }
        }
        //
        // - - - Page 9
        //
        //org options
        if (tab9Interaction0 != null) { tab9Interaction0.SetEvent(EventType.TabbedOrganisation); } else { Debug.LogError("Invalid tab9Interaction0 (Null)"); }
        if (tab9Interaction1 != null) { tab9Interaction1.SetEvent(EventType.TabbedMegaCorp); } else { Debug.LogError("Invalid tab9Interaction1 (Null)"); }
        //Organisations
        arrayOfOrgs[0] = tab9Org0;
        arrayOfOrgs[1] = tab9Org1;
        arrayOfOrgs[2] = tab9Org2;
        arrayOfOrgs[3] = tab9Org3;
        arrayOfOrgs[4] = tab9Org4;
        //MegaCorps
        //NOTE: index [0] is a duplicate entry as it represent enum.MegaCorpType.None as is there to pad out the list
        arrayOfCorps[0] = tab9Corp0;
        arrayOfCorps[1] = tab9Corp0;
        arrayOfCorps[2] = tab9Corp1;
        arrayOfCorps[3] = tab9Corp2;
        arrayOfCorps[4] = tab9Corp3;
        arrayOfCorps[5] = tab9Corp4;
        //cached list of MegaCorps (in order 0 -> 4)
        //NOTE list[0] is a duplicate entry as it represent enum.MegaCorpType.None as is there to pad out the list
        listOfMegaCorps.Add(GameManager.i.campaignScript.campaign.megaCorpOne);
        listOfMegaCorps.Add(GameManager.i.campaignScript.campaign.megaCorpOne);
        listOfMegaCorps.Add(GameManager.i.campaignScript.campaign.megaCorpTwo);
        listOfMegaCorps.Add(GameManager.i.campaignScript.campaign.megaCorpThree);
        listOfMegaCorps.Add(GameManager.i.campaignScript.campaign.megaCorpFour);
        listOfMegaCorps.Add(GameManager.i.campaignScript.campaign.megaCorpFive);
        //error check
        for (int i = 0; i < listOfMegaCorps.Count; i++)
        {
            if (listOfMegaCorps[i] == null)
            { Debug.LogErrorFormat("Invalid megaCorp (Null) for listOfMegaCorps[{0}]", i); }
        }
        //toggle all MegaCorps on (index[0] corresponds to enum.MegaCorpType.None and is ignored)
        for (int i = 1; i < arrayOfCorps.Length; i++)
        { arrayOfCorps[i].gameObject.SetActive(true); }
        //
        // - - - Initialisations
        //
        InitialiseTooltips();

    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.TabbedOpen, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedClose, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedSideTabOpen, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedSubordinates, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedPlayer, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedHq, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedReserves, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedUpArrow, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedDownArrow, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedRightArrow, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedLeftArrow, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedPageUp, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedPageDown, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedOpenPage, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedScrollUp, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedScrollDown, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedHistoryEvents, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedHistoryEmotions, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedGearNormal, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedGearCapture, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedOrganisation, OnEvent, "ModalTabbedUI");
        EventManager.i.AddListener(EventType.TabbedMegaCorp, OnEvent, "ModalTabbedUI");
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
            case EventType.TabbedOpen:
                TabbedUIData details = Param as TabbedUIData;
                SetTabbedUI(details);
                break;
            case EventType.TabbedClose:
                CloseTabbedUI();
                break;
            case EventType.TabbedOpenPage:
                OpenPage((int)Param, true);
                break;
            case EventType.TabbedSideTabOpen:
                OpenSideTab((int)Param, true);
                break;
            case EventType.TabbedSubordinates:
                OpenActorSet(TabbedUIWho.Subordinates);
                break;
            case EventType.TabbedPlayer:
                OpenActorSet(TabbedUIWho.Player);
                break;
            case EventType.TabbedHq:
                OpenActorSet(TabbedUIWho.HQ);
                break;
            case EventType.TabbedReserves:
                OpenActorSet(TabbedUIWho.Reserves);
                break;
            case EventType.TabbedUpArrow:
                ExecuteUpArrow();
                break;
            case EventType.TabbedDownArrow:
                ExecuteDownArrow();
                break;
            case EventType.TabbedLeftArrow:
                ExecuteLeftArrow();
                break;
            case EventType.TabbedRightArrow:
                ExecuteRightArrow();
                break;
            case EventType.TabbedPageUp:
                ExecutePageUp();
                break;
            case EventType.TabbedPageDown:
                ExecutePageDown();
                break;
            case EventType.TabbedScrollUp:
                ExecuteScrollUp();
                break;
            case EventType.TabbedScrollDown:
                ExecuteScrollDown();
                break;
            case EventType.TabbedHistoryEvents:
                OpenHistory(TabbedHistory.Events);
                break;
            case EventType.TabbedHistoryEmotions:
                OpenHistory(TabbedHistory.Emotions);
                break;
            case EventType.TabbedGearNormal:
                OpenGear(TabbedGear.Normal);
                break;
            case EventType.TabbedGearCapture:
                OpenGear(TabbedGear.Capture);
                break;
            case EventType.TabbedOrganisation:
                OpenOrganisation(TabbedOrg.Organisation);
                break;
            case EventType.TabbedMegaCorp:
                OpenOrganisation(TabbedOrg.MegaCorp);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion


    #region Initialisation...

    #region InitialiseTabbedUI
    /// <summary>
    /// run prior to every TabbedUI use. Run from SetTabbedUI
    /// </summary>
    private void InitialiseTabbedUI(TabbedUIData data)
    {
        //Start with a clean slate -> empty out cached data
        if (arrayOfSubordinates != null) { Array.Clear(arrayOfSubordinates, 0, arrayOfSubordinates.Length); }
        if (arrayOfReserves != null) { Array.Clear(arrayOfReserves, 0, arrayOfReserves.Length); }
        if (arrayOfHq != null) { Array.Clear(arrayOfHq, 0, arrayOfHq.Length); }
        //optimisation
        currentTurn = GameManager.i.turnScript.Turn;
    }
    #endregion

    #region InitialiseSideTabs
    /// <summary>
    /// Updates side tabs when necessary
    /// </summary>
    /// <param name=""></param>
    private void InitialiseSideTabs()
    {
        int index = 0;
        if (arrayOfSideTabItems[index] != null)
        {
            //clear out actor array
            Array.Clear(arrayOfActorsTemp, 0, arrayOfActorsTemp.Length);
            //Update sideTabs for required actor set
            switch (inputData.who)
            {
                case TabbedUIWho.Subordinates:
                    //Cached data available
                    if (arrayOfSubordinates != null)
                    {
                        numOfSideTabs = arrayOfSubordinates.Length;
                        if (numOfSideTabs > 0)
                        {
                            arrayOfActorsTemp = new Actor[numOfSideTabs];
                            InitialiseSubordinate(arrayOfSubordinates);
                        }
                        else
                        { textActorName.text = ""; tab0ActorName.text = ""; }
                        /*Debug.LogFormat("[Tst] ModalTabbedUI.cs -> InitialiseSideTabs: Subordinates CACHED data used{0}", "\n");*/
                    }
                    else
                    {
                        //Generate Data -> check how many actors OnMap
                        arrayOfActorsTemp = GameManager.i.dataScript.GetCurrentActorsVariable(inputData.side);
                        if (arrayOfActorsTemp != null)
                        {
                            numOfSideTabs = arrayOfActorsTemp.Length;
                            InitialiseSubordinate(arrayOfActorsTemp);
                            //store Cached data
                            if (arrayOfSubordinates == null)
                            {
                                //initialise cached Subordinates array
                                arrayOfSubordinates = new Actor[numOfSideTabs];
                                Array.Copy(arrayOfActorsTemp, arrayOfSubordinates, numOfSideTabs);
                            }
                            /*Debug.LogFormat("[Tst] ModalTabbedUI.cs -> InitialiseSideTabs: Subordinates Generated data used{0}", "\n");*/
                        }
                        else
                        {
                            numOfSideTabs = 0;
                            //clear out any residual actor name
                            textActorName.text = ""; tab0ActorName.text = "";
                            //reinitialise array
                            arrayOfActorsTemp = new Actor[maxSideTabIndex];
                        }
                    }

                    //disable empty tabs
                    if (numOfSideTabs <= maxSideTabIndex)
                    {
                        for (int i = numOfSideTabs; i <= maxSideTabIndex; i++)
                        { arrayOfSideTabObjects[i].SetActive(false); }
                    }
                    break;
                case TabbedUIWho.Reserves:
                    //Cached data available
                    if (arrayOfReserves != null)
                    {
                        numOfSideTabs = arrayOfReserves.Length;
                        if (numOfSideTabs > 0)
                        {
                            arrayOfActorsTemp = new Actor[numOfSideTabs];
                            InitialiseReserves(arrayOfReserves);
                        }
                        else
                        { textActorName.text = ""; tab0ActorName.text = ""; }
                        /*Debug.LogFormat("[Tst] ModalTabbedUI.cs -> InitialiseSideTabs: Reserves CACHED data used{0}", "\n");*/
                    }
                    else
                    {
                        //Generate Data -> how many actors in reserve
                        numOfSideTabs = GameManager.i.dataScript.CheckNumOfActorsInReserve();
                        if (numOfSideTabs > 0)
                        {
                            //reinitialise array
                            arrayOfActorsTemp = new Actor[numOfSideTabs];
                            //get actors
                            List<int> listOfActors = GameManager.i.dataScript.GetListOfReserveActors(inputData.side);
                            if (listOfActors != null)
                            {

                                InitialiseReserves(listOfActors);
                                //Cached data
                                if (arrayOfReserves == null)
                                {
                                    //initialise cached Subordinates array
                                    arrayOfReserves = new Actor[numOfSideTabs];
                                    Array.Copy(arrayOfActorsTemp, arrayOfReserves, numOfSideTabs);
                                }
                                /*Debug.LogFormat("[Tst] ModalTabbedUI.cs -> InitialiseSideTabs: Reserves Generated data used{0}", "\n");*/
                            }
                            else { Debug.LogErrorFormat("Invalid listOfReserveActors (Null) for \"{0}\"", inputData.side); }
                        }
                        else
                        {
                            numOfSideTabs = 0;
                            //clear out any residual actor name
                            textActorName.text = ""; tab0ActorName.text = "";
                            //reinitialise array
                            arrayOfActorsTemp = new Actor[maxSideTabIndex];
                        }
                    }
                    //disable empty tabs
                    if (numOfSideTabs <= maxSideTabIndex)
                    {
                        for (int i = numOfSideTabs; i <= maxSideTabIndex; i++)
                        { arrayOfSideTabObjects[i].SetActive(false); }
                    }
                    break;
                case TabbedUIWho.Player:
                    numOfSideTabs = 1;
                    index = 0;
                    textActorName.text = GameManager.i.playerScript.PlayerName;
                    //set up player tab
                    arrayOfSideTabItems[index].portrait.sprite = GameManager.i.playerScript.sprite;
                    arrayOfSideTabItems[index].title.text = GameManager.i.playerScript.FirstName;
                    //colors
                    textSideColour = arrayOfSideTabItems[index].title.color;
                    portraitColour = arrayOfSideTabItems[index].portrait.color;
                    backgroundColour = sideTabActiveColour;
                    portraitColour.a = 1.0f; backgroundColour.a = 1.0f; textSideColour.a = 1.0f;
                    //set colors
                    arrayOfSideTabItems[index].portrait.color = portraitColour;
                    arrayOfSideTabItems[index].background.color = backgroundColour;
                    arrayOfSideTabItems[index].title.color = textSideColour;
                    //activate tab
                    arrayOfSideTabObjects[index].SetActive(true);
                    currentSideTabIndex = index;
                    //disable inactive tabs
                    for (int i = numOfSideTabs; i <= maxSideTabIndex; i++)
                    { arrayOfSideTabObjects[i].SetActive(false); }
                    break;
                case TabbedUIWho.HQ:
                    //Cached data available
                    if (arrayOfHq != null)
                    {
                        numOfSideTabs = arrayOfHq.Length;
                        if (numOfSideTabs > 0)
                        {
                            arrayOfActorsTemp = new Actor[numOfSideTabs];
                            InitialiseHq(arrayOfHq);
                        }
                        else
                        { textActorName.text = ""; tab0ActorName.text = ""; }
                        /*Debug.LogFormat("[Tst] ModalTabbedUI.cs -> InitialiseSideTabs: Hq CACHED data used{0}", "\n");*/
                    }
                    else
                    {
                        //Generate Data -> assumes a full compliment of HQ actors present
                        List<Actor> listOfActors = GameManager.i.dataScript.GetListOfHqHierarchy();
                        if (listOfActors != null)
                        {
                            numOfSideTabs = listOfActors.Count;
                            arrayOfActorsTemp = listOfActors.ToArray();
                            InitialiseHq(arrayOfActorsTemp);
                            //Cached data
                            if (arrayOfHq == null)
                            {
                                //initialise cached Subordinates array
                                arrayOfHq = new Actor[numOfSideTabs];
                                Array.Copy(arrayOfActorsTemp, arrayOfHq, numOfSideTabs);
                            }
                            /*Debug.LogFormat("[Tst] ModalTabbedUI.cs -> InitialiseSideTabs: Hq Generated data used{0}", "\n");*/
                        }
                        else { Debug.LogError("Invalid listOfHqHierarchyActors (Null)"); }
                    }
                    break;
                default: Debug.LogWarningFormat("Unrecognised TabbedUIWho \"{0}\"", inputData.who); break;

            }

        }
        else { Debug.LogErrorFormat("Invalid tabItems[{0}] (Null)", index); }
    }
    #endregion

    #region InitialiseTooltips
    /// <summary>
    /// Initialise fixed tooltips
    /// </summary>
    private void InitialiseTooltips()
    {
        //main help button
        listOfHelpData = GameManager.i.helpScript.GetHelpData("tabbedUI_0", "tabbedUI_1");
        if (listOfHelpData != null)
        { helpClose.SetHelpTooltip(listOfHelpData, x_offset, y_offset); }
        else { Debug.LogWarning("Invalid listOfHelpData for helpMain (Null)"); }
        //
        // - - - Canvas 0 -> Main
        //
        listOfHelpData = GameManager.i.helpScript.GetHelpData("power_0", "power_1", "power_2", "power_3");
        interactActor.helpPower.SetHelpTooltip(listOfHelpData);
        //
        // - - - Canvas 1 -> Personality
        //
        GenericHelpTooltipUI helpCompatibilityStars = tab1Header0.descriptor.GetComponent<GenericHelpTooltipUI>();
        if (helpCompatibilityStars != null)
        {
            listOfHelpData = GameManager.i.helpScript.GetHelpData("compatibility_1", "compatibility_2", "compatibility_3");
            helpCompatibilityStars.SetHelpTooltip(listOfHelpData);
        }
        else { Debug.LogWarning("Invalid helpCompatibilityStars (Null)"); }
        listOfHelpData = GameManager.i.helpScript.GetHelpData("person_0", "person_1", "person_2");
        tab1Person0.help.SetHelpTooltip(listOfHelpData);
        listOfHelpData = GameManager.i.helpScript.GetHelpData("person_3", "person_4", "person_5");
        tab1Person1.help.SetHelpTooltip(listOfHelpData);
        listOfHelpData = GameManager.i.helpScript.GetHelpData("person_6", "person_7", "person_8");
        tab1Person2.help.SetHelpTooltip(listOfHelpData);
        listOfHelpData = GameManager.i.helpScript.GetHelpData("person_9", "person_10", "person_11");
        tab1Person3.help.SetHelpTooltip(listOfHelpData);
        listOfHelpData = GameManager.i.helpScript.GetHelpData("person_12", "person_13", "person_14");
        tab1Person4.help.SetHelpTooltip(listOfHelpData);
        listOfHelpData = GameManager.i.helpScript.GetHelpData("person_15", "person_16", "person_17", "person_18");
        tab1PageHelp.SetHelpTooltip(listOfHelpData);
        //
        // - - - Canvas 2 -> Contacts
        //
        listOfHelpData = GameManager.i.helpScript.GetHelpData("contact_1");
        for (int i = 0; i < arrayOfContacts.Length; i++)
        { arrayOfContacts[i].helpEffectiveness.SetHelpTooltip(listOfHelpData); }
        listOfHelpData = GameManager.i.helpScript.GetHelpData("contact_11");
        tab2Help.SetHelpTooltip(listOfHelpData);
        listOfHelpData = GameManager.i.helpScript.GetHelpData("contact_0", "contact_1");
        tab2PageHelp.SetHelpTooltip(listOfHelpData);
        //
        // - - - Canvas 3 -> Secrets
        //
        listOfHelpData = GameManager.i.helpScript.GetHelpData("secret_0", "secret_1", "secret_2", "secret_3");
        tab3PageHelp.SetHelpTooltip(listOfHelpData);
        //
        // - - - Canvas 4 -> Investigations
        //
        listOfHelpData = GameManager.i.helpScript.GetHelpData("invest_0", "invest_1", "invest_2", "invest_3");
        tab4PageHelp.SetHelpTooltip(listOfHelpData);
        //
        // - - - Canvas 5 -> Likes
        //
        listOfHelpData = GameManager.i.helpScript.GetHelpData("person_19", "person_20");
        tab5PageHelp.SetHelpTooltip(listOfHelpData);
        //
        // - - - Canvas 6 -> Gear
        //
        listOfHelpData = GameManager.i.helpScript.GetHelpData("gear_0", "gear_1", "gear_2", "gear_4");
        tab6PageHelp.SetHelpTooltip(listOfHelpData);
        //
        // - - - Canvas 7 -> History
        //
        listOfHelpData = GameManager.i.helpScript.GetHelpData("history_0");
        tab7PageHelp.SetHelpTooltip(listOfHelpData);
        //
        // - - - Canvas 8 -> Stats
        //
        listOfHelpData = GameManager.i.helpScript.GetHelpData("stat_0");
        tab8PageHelp.SetHelpTooltip(listOfHelpData);
        //
        // - - - Canvas 9 -> Organisations
        //
        listOfHelpData = GameManager.i.helpScript.GetHelpData("org_0", "org_1", "org_3", "org_4");
        tab9PageHelp.SetHelpTooltip(listOfHelpData);
    }
    #endregion

    #endregion


    #region SetTabbedUI
    /// <summary>
    /// Initialise and open TabbedUI
    /// </summary>
    private void SetTabbedUI(TabbedUIData details)
    {
        if (isActive == false)
        {
            if (details != null)
            {
                bool errorFlag = false;
                //set modal status
                GameManager.i.guiScript.SetIsBlocked(true, details.modalLevel);
                //store data
                inputData = details;
                //initialise actorSetIndex
                currentSetIndex = (int)details.who;
                //initialise (order is important as InitialiseTabbedUI clears arrays that are filled with cached data in InitialiseSideTabs)
                InitialiseTabbedUI(details);
                //tooltips off
                GameManager.i.guiScript.SetTooltipsOff();
                //activate main panel
                tabbedObjectMain.SetActive(true);
                //Activate main canvas -> last
                tabbedCanvasMain.gameObject.SetActive(true);
                //Initialise selected set (do so AFTER main canvas activated, not before, otherwise controller button won't highlight for the specified actor set)
                OpenActorSetStart(details.who);
                //Initialise default main top tab
                OpenPage(0);
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
                    //all good, set modal status
                    isActive = true;
                    ModalStateData package = new ModalStateData() { mainState = ModalSubState.InfoDisplay, infoState = ModalInfoSubState.TabbedUI };
                    GameManager.i.inputScript.SetModalState(package);
                    Debug.LogFormat("[UI] ModalTabbedUI.cs -> SetTabbedUI{0}", "\n");
                }
            }
            else { Debug.LogError("Invalid TabbedUIData (Null)"); }
        }
        else { Debug.LogWarning("ModalTabbedUI.cs can't be opened as already ACTIVE -> Info only"); }
    }
    #endregion

    #region CloseTabbedUI
    /// <summary>
    /// Close TabbedUI
    /// </summary>
    private void CloseTabbedUI()
    {
        tabbedCanvasMain.gameObject.SetActive(false);
        tabbedObjectMain.SetActive(false);
        //null out cached data (otherwise arrays will be empty but still valid and new data won't be generated next time UI opened)
        arrayOfSubordinates = null;
        arrayOfReserves = null;
        arrayOfHq = null;
        //clear but not if sitting on top of another UI element, eg. InventoryUI (HQ / Reserve actors)
        GameManager.i.guiScript.SetIsBlocked(false, inputData.modalLevel);
        //close generic tooltip (safety check)
        GameManager.i.tooltipGenericScript.CloseTooltip("ModalTabbedUI.cs -> CloseTabbedUI");
        //close help tooltip
        GameManager.i.tooltipHelpScript.CloseTooltip("ModalTabbedUI.cs -> CloseTabbedUI");
        //set game state
        GameManager.i.inputScript.ResetStates(inputData.modalState);
        //set inactive
        isActive = false;
        Debug.LogFormat("[UI] ModalTabbedUI.cs -> CloseTabbedUI{0}", "\n");
    }
    #endregion


    #region Page and Tab Control... 
    //
    // - - - Page and Tab control
    //

    #region OpenSideTab
    /// <summary>
    /// Open the designated Side tab and close whatever is open
    /// 'isMouseClick' true if user has clicked directly on the tab, false otherwise
    /// </summary>
    /// <param name="tabIndex"></param>
    private void OpenSideTab(int tabIndex, bool isMouseClick = false)
    {
        Debug.AssertFormat(tabIndex > -1 && tabIndex < numOfSideTabs, "Invalid tab index {0}", tabIndex);
        bool isProceed = true;
        //check not opening current side tab (mouse click only)
        if (isMouseClick == true && tabIndex == currentSideTabIndex)
        { isProceed = false; }
        if (isProceed == true)
        {
            //reset Active tabs to reflect new status
            for (int index = 0; index < numOfSideTabs; index++)
            {
                portraitColour = arrayOfSideTabItems[index].portrait.color;
                textSideColour = arrayOfSideTabItems[index].title.color;
                //activate indicated tab and deactivate the rest
                if (index == tabIndex)
                {
                    backgroundColour = sideTabActiveColour;
                    portraitColour.a = 1.0f;
                    backgroundColour.a = 1.0f;
                    textActorName.text = GetActorName(tabIndex);
                    textSideColour.a = 1.0f;
                }
                else
                {
                    backgroundColour = sideTabDormantColour;
                    portraitColour.a = sideTabAlpha;
                    backgroundColour.a = sideTabAlpha;
                    textSideColour.a = 0.5f;
                }
                arrayOfSideTabItems[index].portrait.color = portraitColour;
                arrayOfSideTabItems[index].background.color = backgroundColour;
                //update text colour (faded if inactive
                arrayOfSideTabItems[index].title.color = textSideColour;
            }
            //update index
            currentSideTabIndex = tabIndex;
            UpdatePage();
        }
        //do so regardless
        UpdateControllerButton(inputData.who);
    }
    #endregion

    #region OpenPage
    /// <summary>
    /// Open a tab page. 'isMouseClick' true for when user directly clicking on a top tab to open a page (versus left/right arrow key activation)
    /// </summary>
    /// <param name="tabIndex"></param>
    private void OpenPage(int tabIndex, bool isMouseClick = false)
    {
        bool isProceed = true;
        if (tabIndex > -1 && tabIndex < numOfTopTabs)
        {
            //check you're not opening the current page (mouse click only)
            if (isMouseClick == true && tabIndex == currentTopTabIndex)
            { isProceed = false; }
            if (isProceed == true)
            {
                //turn off help tooltip (may still be open)
                GameManager.i.tooltipHelpScript.CloseTooltip();
                //Active/Dormant tabs
                UpdateTopTabs(tabIndex);
                //turn off all canvases
                for (int i = 0; i < arrayOfCanvas.Length; i++)
                { arrayOfCanvas[i].gameObject.SetActive(false); }
                //open required canvas -> each one is assigned to a particular page
                switch (arrayOfPages[currentTopTabIndex])
                {
                    case TabbedPage.Main: arrayOfCanvas[0].gameObject.SetActive(true); break;
                    case TabbedPage.Personality: arrayOfCanvas[1].gameObject.SetActive(true); break;
                    case TabbedPage.Contacts: arrayOfCanvas[2].gameObject.SetActive(true); break;
                    case TabbedPage.Secrets: arrayOfCanvas[3].gameObject.SetActive(true); break;
                    case TabbedPage.Investigations: arrayOfCanvas[4].gameObject.SetActive(true); break;
                    case TabbedPage.Likes: arrayOfCanvas[5].gameObject.SetActive(true); break;
                    case TabbedPage.Gear: arrayOfCanvas[6].gameObject.SetActive(true); break;
                    case TabbedPage.History: arrayOfCanvas[7].gameObject.SetActive(true); break;
                    case TabbedPage.Stats: arrayOfCanvas[8].gameObject.SetActive(true); break;
                    case TabbedPage.Organisations: arrayOfCanvas[9].gameObject.SetActive(true); break;
                    default: Debug.LogWarningFormat("Unrecognised arrayOfPages \"{0}\"", arrayOfPages[currentTopTabIndex]); break;
                }
                UpdatePage();
            }
        }
        else { Debug.LogErrorFormat("Invalid tabIndex \"{0}\" (should be > -1 and < {1})", tabIndex, numOfTopTabs); }
        UpdateControllerButton(inputData.who);
    }
    #endregion

    #region UpdatePage...
    /// <summary>
    /// SubMethod ONLY CALLED by OpenPage, never called directly (unless flipping between side tabs within same actorSet). Updates data in current page for different side tab actor
    /// </summary>
    private void UpdatePage()
    {
        switch (arrayOfPages[currentTopTabIndex])
        {
            case TabbedPage.Main:
                OpenMain();
                break;

            #region Personality
            case TabbedPage.Personality:
                switch (inputData.who)
                {
                    case TabbedUIWho.Player:
                        tab1Header0.descriptor.text = GetCompatibility(true);
                        tab1Header1.descriptor.text = GetTrait(true);
                        tab1Header2.descriptor.text = GetPersonalityDescriptors(true);
                        tab1Header3.descriptor.text = GetPersonalityAssessment(true);
                        GetCompatibilityTooltip(true);
                        GetTraitTooltip(true);
                        tab1Header1.help.SetHelpTooltip(listOfHelpData);
                        GetPersonalityTooltip(true);
                        tab1Header3.help.SetHelpTooltip(listOfHelpData);
                        break;
                    case TabbedUIWho.Subordinates:
                    case TabbedUIWho.HQ:
                    case TabbedUIWho.Reserves:
                        tab1Header0.descriptor.text = GetCompatibility();
                        tab1Header1.descriptor.text = GetTrait();
                        tab1Header2.descriptor.text = GetPersonalityDescriptors();
                        tab1Header3.descriptor.text = GetPersonalityAssessment();
                        GetCompatibilityTooltip();
                        GetTraitTooltip();
                        tab1Header1.help.SetHelpTooltip(listOfHelpData);
                        GetPersonalityTooltip();
                        tab1Header3.help.SetHelpTooltip(listOfHelpData);
                        break;
                    default: Debug.LogWarningFormat("Unrecognised inputData.who \"{0}\"", inputData.who); break;
                }
                UpdatePersonality();
                break;
            #endregion

            case TabbedPage.Contacts:
                OpenContacts();
                break;
            case TabbedPage.Gear:
                //option bar only if Player
                if (inputData.who == TabbedUIWho.Player)
                { tab6OptionBar.gameObject.SetActive(true); }
                else { tab6OptionBar.gameObject.SetActive(false); }
                OpenGear(TabbedGear.Normal);
                break;
            case TabbedPage.Investigations:
                OpenInvestigations();
                break;
            case TabbedPage.Likes:
                //cached data (InitialiseSessionStart) -> no need to do anything
                break;
            case TabbedPage.Secrets:
                OpenSecrets();
                break;
            case TabbedPage.History:
                //default to Events history on first opening
                OpenHistory(historyOptionIndex);
                break;
            case TabbedPage.Stats:
                OpenStats();
                break;
            case TabbedPage.Organisations:
                OpenOrganisation(TabbedOrg.Organisation);
                break;
            default: Debug.LogWarningFormat("Unrecognised currentTopTabIndex \"{0}\"", currentTopTabIndex); break;
        }
    }
    #endregion

    #region OpenActorSet
    /// <summary>
    /// Open a new actor set, eg. Subordinates/Player/HQ/Reserves. Assumed that TabbedUI is already open
    /// 'isResetSlotID' will set slotID to 0 if true and retain the original value, if false
    /// </summary>
    /// <param name="who"></param>
    private void OpenActorSet(TabbedUIWho who, bool isResetSlotID = true)
    {
        //check not opening current page
        if (who != inputData.who)
        {
            switch (who)
            {
                case TabbedUIWho.Subordinates:
                case TabbedUIWho.Player:
                case TabbedUIWho.HQ:
                case TabbedUIWho.Reserves:
                    inputData.who = who;
                    if (isResetSlotID == true)
                    { inputData.slotID = 0; }
                    currentSetIndex = (int)who;
                    InitialiseTopTabs(who);
                    InitialiseSideTabs();
                    break;
                default: Debug.LogWarningFormat("Unrecognised who \"{0}\"", who); break;
            }
            OpenPage(0);
        }
        //do so regardless
        UpdateControllerButton(who);
    }
    #endregion

    #region OpenActorSetStart
    /// <summary>
    /// Used at startup to open first actor set (ignores opening current page check)
    /// </summary>
    /// <param name="who"></param>
    /// <param name="isResetSlotID"></param>
    private void OpenActorSetStart(TabbedUIWho who)
    {
        switch (who)
        {
            case TabbedUIWho.Subordinates:
            case TabbedUIWho.Player:
            case TabbedUIWho.HQ:
            case TabbedUIWho.Reserves:
                inputData.who = who;
                currentSetIndex = (int)who;
                InitialiseTopTabs(who);
                InitialiseSideTabs();
                break;
            default: Debug.LogWarningFormat("Unrecognised who \"{0}\"", who); break;
        }
        UpdatePage();
        UpdateControllerButton(who);
    }
    #endregion

    #region InitialiseTopTabs
    /// <summary>
    /// Sets up top tabs and arrays whenever a new actorSet is opened
    /// </summary>
    /// <param name="who"></param>
    private void InitialiseTopTabs(TabbedUIWho who)
    {
        switch (who)
        {
            case TabbedUIWho.Subordinates:
                numOfActors = GameManager.i.dataScript.CheckNumOfOnMapActors(GameManager.i.sideScript.PlayerSide);
                if (numOfActors > 0)
                {
                    arrayOfPages[0] = TabbedPage.Main;
                    arrayOfPages[1] = TabbedPage.Personality;
                    arrayOfPages[2] = TabbedPage.Contacts;
                    arrayOfPages[3] = TabbedPage.Secrets;
                    arrayOfPages[4] = TabbedPage.Gear;
                    arrayOfPages[5] = TabbedPage.History;
                    arrayOfPages[6] = TabbedPage.Stats;
                    maxTopTabIndex = 6;
                    arrayOfTopTabObjects[0].SetActive(true);
                    arrayOfTopTabObjects[1].SetActive(true);
                    arrayOfTopTabObjects[2].SetActive(true);
                    arrayOfTopTabObjects[3].SetActive(true);
                    arrayOfTopTabObjects[4].SetActive(true);
                    arrayOfTopTabObjects[5].SetActive(true);
                    arrayOfTopTabObjects[6].SetActive(true);
                    arrayOfTopTabObjects[7].SetActive(false);
                    arrayOfTopTabObjects[8].SetActive(false);
                    arrayOfTopTabTitles[0].text = "Main";
                    arrayOfTopTabTitles[1].text = "Person";
                    arrayOfTopTabTitles[2].text = "Contacts";
                    arrayOfTopTabTitles[3].text = "Secrets";
                    arrayOfTopTabTitles[4].text = "Gear";
                    arrayOfTopTabTitles[5].text = "History";
                    arrayOfTopTabTitles[6].text = "Stats";
                }
                else
                {
                    //no subordinates present
                    arrayOfPages[0] = TabbedPage.Main;
                    maxTopTabIndex = 0;
                    arrayOfTopTabObjects[0].SetActive(true);
                    arrayOfTopTabObjects[1].SetActive(false);
                    arrayOfTopTabObjects[2].SetActive(false);
                    arrayOfTopTabObjects[3].SetActive(false);
                    arrayOfTopTabObjects[4].SetActive(false);
                    arrayOfTopTabObjects[5].SetActive(false);
                    arrayOfTopTabObjects[6].SetActive(false);
                    arrayOfTopTabObjects[7].SetActive(false);
                    arrayOfTopTabObjects[8].SetActive(false);
                    arrayOfTopTabTitles[0].text = "Main";
                }
                break;
            case TabbedUIWho.Player:
                numOfActors = 1;
                arrayOfPages[0] = TabbedPage.Main;
                arrayOfPages[1] = TabbedPage.Personality;
                arrayOfPages[2] = TabbedPage.Likes;
                arrayOfPages[3] = TabbedPage.Secrets;
                arrayOfPages[4] = TabbedPage.Gear;
                arrayOfPages[5] = TabbedPage.Investigations;
                arrayOfPages[6] = TabbedPage.Organisations;
                arrayOfPages[7] = TabbedPage.History;
                arrayOfPages[8] = TabbedPage.Stats;
                maxTopTabIndex = 8;
                arrayOfTopTabObjects[0].SetActive(true);
                arrayOfTopTabObjects[1].SetActive(true);
                arrayOfTopTabObjects[2].SetActive(true);
                arrayOfTopTabObjects[3].SetActive(true);
                arrayOfTopTabObjects[4].SetActive(true);
                arrayOfTopTabObjects[5].SetActive(true);
                arrayOfTopTabObjects[6].SetActive(true);
                arrayOfTopTabObjects[7].SetActive(true);
                arrayOfTopTabObjects[8].SetActive(true);
                arrayOfTopTabTitles[0].text = "Main";
                arrayOfTopTabTitles[1].text = "Person";
                arrayOfTopTabTitles[2].text = "Likes";
                arrayOfTopTabTitles[3].text = "Secrets";
                arrayOfTopTabTitles[4].text = "Gear";
                arrayOfTopTabTitles[5].text = "Invest";
                arrayOfTopTabTitles[6].text = "Orgs";
                arrayOfTopTabTitles[7].text = "History";
                arrayOfTopTabTitles[8].text = "Stats";
                break;
            case TabbedUIWho.HQ:
                numOfActors = 4;
                arrayOfPages[0] = TabbedPage.Main;
                arrayOfPages[1] = TabbedPage.Personality;
                arrayOfPages[2] = TabbedPage.History;
                arrayOfPages[3] = TabbedPage.Stats;
                arrayOfTopTabObjects[0].SetActive(true);
                arrayOfTopTabObjects[1].SetActive(true);
                arrayOfTopTabObjects[2].SetActive(true);
                arrayOfTopTabObjects[3].SetActive(true);
                arrayOfTopTabObjects[4].SetActive(false);
                arrayOfTopTabObjects[5].SetActive(false);
                arrayOfTopTabObjects[6].SetActive(false);
                arrayOfTopTabObjects[7].SetActive(false);
                arrayOfTopTabObjects[8].SetActive(false);
                maxTopTabIndex = 3;
                arrayOfTopTabTitles[0].text = "Main";
                arrayOfTopTabTitles[1].text = "Person";
                arrayOfTopTabTitles[2].text = "History";
                arrayOfTopTabTitles[3].text = "Stats";
                break;
            case TabbedUIWho.Reserves:
                numOfActors = GameManager.i.dataScript.CheckNumOfActorsInReserve();
                if (numOfActors > 0)
                {
                    arrayOfPages[0] = TabbedPage.Main;
                    arrayOfPages[1] = TabbedPage.Personality;
                    arrayOfPages[2] = TabbedPage.History;
                    arrayOfPages[3] = TabbedPage.Stats;
                    arrayOfTopTabObjects[0].SetActive(true);
                    arrayOfTopTabObjects[1].SetActive(true);
                    arrayOfTopTabObjects[2].SetActive(true);
                    arrayOfTopTabObjects[3].SetActive(true);
                    arrayOfTopTabObjects[4].SetActive(false);
                    arrayOfTopTabObjects[5].SetActive(false);
                    arrayOfTopTabObjects[6].SetActive(false);
                    arrayOfTopTabObjects[7].SetActive(false);
                    arrayOfTopTabObjects[8].SetActive(false);
                    maxTopTabIndex = 3;
                    arrayOfTopTabTitles[0].text = "Main";
                    arrayOfTopTabTitles[1].text = "Person";
                    arrayOfTopTabTitles[2].text = "History";
                    arrayOfTopTabTitles[3].text = "Stats";
                }
                else
                {
                    //no reserves present
                    arrayOfPages[0] = TabbedPage.Organisations;
                    maxTopTabIndex = 0;
                    arrayOfTopTabObjects[0].SetActive(true);
                    arrayOfTopTabObjects[1].SetActive(false);
                    arrayOfTopTabObjects[2].SetActive(false);
                    arrayOfTopTabObjects[3].SetActive(false);
                    arrayOfTopTabObjects[4].SetActive(false);
                    arrayOfTopTabObjects[5].SetActive(false);
                    arrayOfTopTabObjects[6].SetActive(false);
                    arrayOfTopTabObjects[7].SetActive(false);
                    arrayOfTopTabObjects[8].SetActive(false);
                    arrayOfTopTabTitles[0].text = "Main";
                }
                break;
            default: Debug.LogWarningFormat("Unrecognised who \"{0}\"", who); break;
        }
        //default to first tab on changing actorSet
        currentTopTabIndex = 0;
        UpdateTopTabs(currentTopTabIndex);
    }
    #endregion

    #region UpdateControllerButton
    /// <summary>
    /// selects specified button (shows as highlighted). Other button automatically deselected
    /// </summary>
    /// <param name="who"></param>
    private void UpdateControllerButton(TabbedUIWho who)
    {
        switch (who)
        {
            case TabbedUIWho.Subordinates:
                buttonSubordinates.Select();
                textSubordinates.color = tabTextActiveColour;
                textPlayer.color = tabTextDormantColour;
                textHq.color = tabTextDormantColour;
                textReserves.color = tabTextDormantColour;
                break;
            case TabbedUIWho.Player:
                buttonPlayer.Select();
                textSubordinates.color = tabTextDormantColour;
                textPlayer.color = tabTextActiveColour;
                textHq.color = tabTextDormantColour;
                textReserves.color = tabTextDormantColour;
                break;
            case TabbedUIWho.HQ:
                buttonHq.Select();
                textSubordinates.color = tabTextDormantColour;
                textPlayer.color = tabTextDormantColour;
                textHq.color = tabTextActiveColour;
                textReserves.color = tabTextDormantColour;
                break;
            case TabbedUIWho.Reserves:
                buttonReserves.Select();
                textSubordinates.color = tabTextDormantColour;
                textPlayer.color = tabTextDormantColour;
                textHq.color = tabTextDormantColour;
                textReserves.color = tabTextActiveColour;
                break;
            default: Debug.LogWarningFormat("Unrecognised who \"{0}\"", who); break;
        }
    }
    #endregion

    #region UpdateTopTabs
    /// <summary>
    /// selects specified tab (shows as active). Other tabs set to dormant
    /// </summary>
    /// <param name="top"></param>
    private void UpdateTopTabs(int tabIndex)
    {
        if (tabIndex > -1)
        {
            //Update active/dormant tabs
            for (int i = 0; i < numOfTopTabs; i++)
            {
                if (i == tabIndex)
                {
                    //active
                    arrayOfTopTabImages[i].color = topTabActiveColour;
                    arrayOfTopTabTitles[i].color = tabTextActiveColour;
                }
                else
                {
                    //dormant
                    arrayOfTopTabImages[i].color = topTabDormantColour;
                    arrayOfTopTabTitles[i].color = tabTextDormantColour;
                }
            }
            //update index (required for when player directly clicks on a tab)
            currentTopTabIndex = tabIndex;
            //main page text
            textPageMain.text = Convert.ToString(arrayOfPages[tabIndex]);
            //actorSet
            UpdateControllerButton((TabbedUIWho)currentSetIndex);
        }
        else { Debug.LogWarning("Invalid tabIndex (-1)"); }
    }
    #endregion

    #region UpdateHistoryButtons
    /// <summary>
    /// Handles history page 7 option buttons texts and colours. Assumes that a button has been pressed (isPressed = true)
    /// </summary>
    /// <param name="history"></param>
    private void UpdateHistoryButtons(TabbedHistory history)
    {
        switch (history)
        {
            case TabbedHistory.Events:
                //image button colours
                tab7Interaction0.image.color = pageOptionActiveColour;
                tab7Interaction1.image.color = pageOptionDormantColour;
                //text colours
                tab7Interaction0.title.color = tabTextActiveColour;
                tab7Interaction1.title.color = tabTextDormantColour;
                //header
                tab7Header.text = "Events";
                break;
            case TabbedHistory.Emotions:
                //image button colours
                tab7Interaction0.image.color = pageOptionDormantColour;
                tab7Interaction1.image.color = pageOptionActiveColour;
                //text colours
                tab7Interaction0.title.color = tabTextDormantColour;
                tab7Interaction1.title.color = tabTextActiveColour;
                //header
                if (inputData.who == TabbedUIWho.Player)
                { tab7Header.text = "Mood"; }
                else { tab7Header.text = "Opinion"; }
                break;
            default: Debug.LogWarningFormat("Unrecognised TabbedHistory \"{0}\"", history); break;
        }
        //text for Emotions button
        switch (inputData.who)
        {
            case TabbedUIWho.Player: tab7Interaction1.title.text = "Mood"; break;
            default: tab7Interaction1.title.text = "Opinion"; break;
        }
        //update index
        historyOptionIndex = history;
        //reset controller buttons (they unselect once you click on a sprite 'button')
        UpdateControllerButton(inputData.who);
    }
    #endregion

    #region OpenMain
    /// <summary>
    /// Open main page
    /// </summary>
    private void OpenMain()
    {
        //toggle off none present -> default for all
        tab0MainPanel.gameObject.SetActive(true);
        tab0NonePresent.gameObject.SetActive(false);
        //Actor set
        switch (inputData.who)
        {
            case TabbedUIWho.Subordinates:
                if (numOfActors > 0)
                {
                    //populate actor data
                    tab0ActorName.text = GetActorName(currentSideTabIndex);
                    tab0ActorTitle.text = GetActorTitle(currentSideTabIndex);
                    tab0ActorImage.sprite = arrayOfSideTabItems[currentSideTabIndex].portrait.sprite;
                    //rhs
                    tab0Header0.image.gameObject.SetActive(true);
                    tab0Header1.image.gameObject.SetActive(true);
                    tab0Header2.image.gameObject.SetActive(true);
                    tab0Header3.image.gameObject.SetActive(true);
                    tab0Header4.image.gameObject.SetActive(false);
                    tab0Compatibility.text = GetCompatibility();
                    GetCompatibilityTooltip();
                    interactActor.helpCompatibility.SetHelpTooltip(listOfHelpData);
                    GetConflictTooltip();
                    GetRelationshipTooltip();
                    UpdatePower();
                    UpdateStatus();
                    if (GetConflict() == true) { tab0Header1.text.color = tabSubHeaderTextActiveColour; } else { tab0Header1.text.color = tabSubHeaderTextDormantColour; }
                    if (GetRelationship() == true) { tab0Header2.text.color = tabSubHeaderTextActiveColour; } else { tab0Header2.text.color = tabSubHeaderTextDormantColour; }
                    if (GetConditions() == true) { tab0Header3.text.color = tabSubHeaderTextActiveColour; } else { tab0Header3.text.color = tabSubHeaderTextDormantColour; }
                }
                else
                {
                    //none present
                    tab0MainPanel.gameObject.SetActive(false);
                    tab0NonePresent.gameObject.SetActive(true);
                }
                break;
            case TabbedUIWho.Player:
                //populate actor data
                tab0ActorName.text = GetActorName(currentSideTabIndex);
                tab0ActorTitle.text = GetActorTitle(currentSideTabIndex);
                tab0ActorImage.sprite = arrayOfSideTabItems[currentSideTabIndex].portrait.sprite;
                //rhs
                tab0Header0.image.gameObject.SetActive(true);
                tab0Header1.image.gameObject.SetActive(false);
                tab0Header2.image.gameObject.SetActive(false);
                tab0Header3.image.gameObject.SetActive(true);
                tab0Header4.image.gameObject.SetActive(true);
                tab0Compatibility.text = GetCompatibility(true);
                GetCompatibilityTooltip(true);
                interactActor.helpCompatibility.SetHelpTooltip(listOfHelpData);
                UpdatePower(true);
                UpdateStatus(true);
                if (GetCures() == true) { tab0Header4.text.color = tabSubHeaderTextActiveColour; } else { tab0Header4.text.color = tabSubHeaderTextDormantColour; }
                if (GetConditions(true) == true) { tab0Header3.text.color = tabSubHeaderTextActiveColour; } else { tab0Header3.text.color = tabSubHeaderTextDormantColour; }
                break;
            case TabbedUIWho.HQ:
                //populate actor data
                tab0ActorName.text = GetActorName(currentSideTabIndex);
                tab0ActorTitle.text = GetActorTitle(currentSideTabIndex);
                tab0ActorImage.sprite = arrayOfSideTabItems[currentSideTabIndex].portrait.sprite;
                //rhs
                tab0Header0.image.gameObject.SetActive(true);
                tab0Header1.image.gameObject.SetActive(false);
                tab0Header2.image.gameObject.SetActive(false);
                tab0Header3.image.gameObject.SetActive(false);
                tab0Header4.image.gameObject.SetActive(false);
                tab0Compatibility.text = GetCompatibility();
                GetCompatibilityTooltip();
                interactActor.helpCompatibility.SetHelpTooltip(listOfHelpData);
                UpdatePower();
                UpdateStatus();
                break;
            case TabbedUIWho.Reserves:
                if (numOfActors > 0)
                {
                    //populate actor data
                    tab0ActorName.text = GetActorName(currentSideTabIndex);
                    tab0ActorTitle.text = GetActorTitle(currentSideTabIndex);
                    tab0ActorImage.sprite = arrayOfSideTabItems[currentSideTabIndex].portrait.sprite;
                    //rhs
                    tab0Header0.image.gameObject.SetActive(true);
                    tab0Header1.image.gameObject.SetActive(false);
                    tab0Header2.image.gameObject.SetActive(false);
                    tab0Header3.image.gameObject.SetActive(true);
                    tab0Header4.image.gameObject.SetActive(false);
                    tab0Compatibility.text = GetCompatibility();
                    GetCompatibilityTooltip();
                    interactActor.helpCompatibility.SetHelpTooltip(listOfHelpData);
                    UpdatePower();
                    UpdateStatus();
                    if (GetConditions() == true) { tab0Header3.text.color = tabSubHeaderTextActiveColour; } else { tab0Header3.text.color = tabSubHeaderTextDormantColour; }
                }
                else
                {
                    //none present
                    tab0MainPanel.gameObject.SetActive(false);
                    tab0NonePresent.gameObject.SetActive(true);
                }
                break;
            default: Debug.LogWarningFormat("Unrecognised inputData.who \"{0}\"", inputData.who); break;
        }

    }
    #endregion

    #region OpenHistory
    /// <summary>
    /// All in one open up a history / update a history page
    /// </summary>
    /// <param name="history"></param>
    private void OpenHistory(TabbedHistory history)
    {
        // - - - Get data
        listOfHistory = GetHistory(history);
        // - - - Indexes and Initialisation
        if (listOfHistory != null)
        { numOfScrollItemsCurrent = listOfHistory.Count; }
        else
        {
            numOfScrollItemsCurrent = 0;
            Debug.LogWarningFormat("Invalid listOfHistory (Null) for \"{0}\"", inputData.who);
        }
        //max index
        scrollMaxHighlightIndex = numOfScrollItemsCurrent - 1;
        //highlight index
        if (numOfScrollItemsCurrent > numOfScrollItemsVisible)
        { scrollHighlightIndex = numOfScrollItemsVisible; }
        else { scrollHighlightIndex = 0; }
        // - - - populate history and activate
        if (numOfScrollItemsCurrent > 0)
        {
            for (int i = 0; i < maxNumOfScrollItems; i++)
            {
                if (i < numOfScrollItemsCurrent)
                {
                    if (listOfHistory[i] != null)
                    {
                        arrayOfScrollObjects[i].SetActive(true);
                        arrayOfScrollInteractions[i].descriptor.text = listOfHistory[i];
                    }
                    else { Debug.LogWarningFormat("Invalid text (Null) for listOfHistory[{0}]", i); }
                }
                else
                {
                    //disable item
                    arrayOfScrollObjects[i].SetActive(false);
                }
            }
        }
        else { DisableHistoryScrollItems(); }
        //scroll view at default position
        tab7ScrollRect.verticalNormalizedPosition = 1.0f;
        //manually activate / deactivate scrollBar as needed (because you've got deactivated objects in the scroll list the bar shows regardless unless you override here)
        if (numOfScrollItemsCurrent <= numOfScrollItemsVisible)
        {
            tab7ScrollRect.verticalScrollbar = null;
            tab7ScrollBarObject.SetActive(false);
        }
        else
        {
            tab7ScrollBarObject.SetActive(true);
            tab7ScrollRect.verticalScrollbar = tab7ScrollBar;
        }
        UpdateHistoryButtons(history);
    }
    #endregion

    #region OpenContacts
    /// <summary>
    /// All in one open/update Contacts page (Subordinates actor set only)
    /// </summary>
    private void OpenContacts()
    {
        actorGeneric = arrayOfActorsTemp[currentSideTabIndex];
        if (actorGeneric != null)
        {
            //effectiveness
            int effectiveness = actorGeneric.GetContactNetworkEffectiveness();
            tab2NetworkStrength.text = Convert.ToString(effectiveness);
            if (effectiveness == 1)
            { tab2NetworkStars.text = "Star"; }
            else { tab2NetworkStars.text = "Stars"; }
            //contacts
            Dictionary<int, Contact> dictOfContacts = actorGeneric.GetDictOfContacts();
            if (dictOfContacts != null)
            {
                listOfContacts.Clear();
                listOfContacts.AddRange(dictOfContacts.Values);
                int numOfContacts = listOfContacts.Count;
                //limit check
                if (numOfContacts > maxNumOfContacts)
                {
                    Debug.LogWarningFormat("Invalid numOfContacts (is {0}, max allowed {1})", numOfContacts, maxNumOfContacts);
                    numOfContacts = maxNumOfContacts;
                }
                //toggle contacts
                for (int i = 0; i < arrayOfContacts.Length; i++)
                {
                    if (i < numOfContacts)
                    {
                        //activate contacts
                        arrayOfContacts[i].gameObject.SetActive(true);
                        interactContact = arrayOfContacts[i];
                        if (interactContact != null)
                        {
                            contact = listOfContacts[i];
                            if (contact != null)
                            {
                                interactContact.portrait.gameObject.SetActive(true);
                                switch (contact.sex)
                                {
                                    case ActorSex.Male:
                                        interactContact.portrait.sprite = GameManager.i.spriteScript.tabbedContactMaleSprite;
                                        break;
                                    case ActorSex.Female:
                                        interactContact.portrait.sprite = GameManager.i.spriteScript.tabbedContactFemaleSprite;
                                        break;
                                    default: Debug.LogWarningFormat("Unrecognised contact.sex \"{0}\"", contact.sex); break;
                                }
                                interactContact.contactName.text = string.Format("{0} {1}", contact.nameFirst, contact.nameLast);
                                interactContact.job.text = contact.job;
                                //location
                                nodeContact = GameManager.i.dataScript.GetNode(contact.nodeID);
                                if (nodeContact != null)
                                { interactContact.nodeName.text = nodeContact.nodeName; }
                                else { Debug.LogWarningFormat("Invalid node (Null) for listOfContacts[{0}], contact.nodeID {1}", i, contact.nodeID); }
                                //status
                                switch (contact.status)
                                {
                                    case ContactStatus.Active:
                                        interactContact.status.text = GameManager.Formatt(Convert.ToString(contact.status), ColourType.goodText);
                                        interactContact.background.color = contactActiveColour;
                                        break;
                                    case ContactStatus.Inactive:
                                        interactContact.status.text = GameManager.Formatt(Convert.ToString(contact.status), ColourType.badText);
                                        interactContact.background.color = contactInactiveColour;
                                        break;
                                    default: Debug.LogWarningFormat("Unrecognised contact.Status \"{0}\"", contact.status); break;
                                }
                                interactContact.effectiveness.text = GameManager.i.guiScript.GetNormalStars(contact.effectiveness);
                            }
                            //effectiveness
                            else { Debug.LogWarningFormat("Invalid contact (Null) for listOfContacts[{0]]", i); }
                            //intel
                            int intel = contact.statsNemesis + contact.statsNpc + contact.statsRumours + contact.statsTeams;
                            interactContact.intel.text = Convert.ToString(intel);
                            //tooltip -> intel
                            builder.Clear();
                            builder.AppendFormat("Total Rumours intel<pos=65%>{0}{1}", GameManager.Formatt(Convert.ToString(contact.statsRumours), ColourType.salmonText), "\n");
                            builder.AppendFormat("Total Teams intel<pos=65%>{0}{1}", GameManager.Formatt(Convert.ToString(contact.statsTeams), ColourType.salmonText), "\n");
                            builder.AppendFormat("Total Nemesis intel<pos=65%>{0}{1}", GameManager.Formatt(Convert.ToString(contact.statsNemesis), ColourType.salmonText), "\n");
                            builder.AppendFormat("Total V.I.P intel<pos=65%>{0}", GameManager.Formatt(Convert.ToString(contact.statsNpc), ColourType.salmonText));
                            HelpData data = new HelpData()
                            {
                                tag = "",
                                header = "Intel Breakdown",
                                text = builder.ToString()
                            };
                            listOfHelpData.Clear();
                            listOfHelpData.Add(data);
                            interactContact.help.SetHelpTooltip(listOfHelpData);
                            //tooltip -> effectiveness -> taken care of in InitialiseTooltips (fixed)
                        }
                        else { Debug.LogWarningFormat("Invalid tabbedContactInteraction (Null) for arrayOfContacs[{0}]", i); }
                    }
                    else
                    {
                        //disable contact
                        arrayOfContacts[i].gameObject.SetActive(false);
                    }
                }
            }
            else { Debug.LogWarningFormat("Invalid dictOfContacts for actor {0}, {1}, ID {2}, arrayOfActorsTemp[{3}]", actorGeneric.actorName, actorGeneric.arc.name, actorGeneric.actorID, currentSideTabIndex); }
        }
        else { Debug.LogErrorFormat("Invalid actor (Null) for arrayOfActors[{0}]", currentSideTabIndex); }
    }
    #endregion

    #region OpenSecrets
    /// <summary>
    /// All in one open/update Secrets page (Player/Subordinates actor sets only)
    /// </summary>
    private void OpenSecrets()
    {
        int numOfSecrets, count;
        listOfSecrets.Clear();
        listOfSecretActors.Clear();
        switch (inputData.who)
        {
            case TabbedUIWho.Player:
                tab3Header.text = "Your Secrets";
                listOfSecrets.AddRange(GameManager.i.playerScript.GetListOfSecrets());
                break;
            case TabbedUIWho.Subordinates:
                tab3Header.text = "Secrets Known";
                listOfSecrets.AddRange(arrayOfActorsTemp[currentSideTabIndex].GetListOfSecrets());
                break;
            default: Debug.LogWarningFormat("Unrecognised inputData.who \"{0}\"", inputData.who); break;
        }
        if (listOfSecrets != null)
        {
            numOfSecrets = listOfSecrets.Count;
            if (numOfSecrets > 0)
            {
                tab3NonePresent.gameObject.SetActive(false);
                for (int i = 0; i < arrayOfSecrets.Length; i++)
                {
                    if (i < numOfSecrets)
                    {
                        secret = listOfSecrets[i];
                        if (secret != null)
                        {
                            //activate secret
                            arrayOfSecrets[i].gameObject.SetActive(true);
                            interactSecret = arrayOfSecrets[i];
                            //populate prefab data
                            interactSecret.background.color = secretColour;
                            //portrait
                            interactSecret.portrait.sprite = GameManager.i.spriteScript.secretSprite;
                            //descriptor (different for Player or Subordinate)
                            if (inputData.who == TabbedUIWho.Player)
                            { interactSecret.descriptor.text = string.Format("{0}{1}<size=90%>{2}</size>", GameManager.Formatt(secret.tag, ColourType.neutralText), "\n", secret.descriptor); }
                            else { interactSecret.descriptor.text = string.Format("{0}{1}<size=90%>{2}</size>", GameManager.Formatt(secret.tag, ColourType.neutralText), "\n", secret.descriptorOther); }
                            //Who knows?
                            listOfSecretActors = secret.GetListOfActors();
                            count = listOfSecretActors.Count;
                            if (count > 0)
                            {
                                if (count > 1)
                                {
                                    //multiple actors know
                                    builder.Clear();
                                    builder.AppendFormat("{0}<size=90%>", knowsSecretHeader);
                                    for (int j = 0; j < count; j++)
                                    {
                                        actorGeneric = GameManager.i.dataScript.GetActor(listOfSecretActors[j]);
                                        if (actorGeneric != null)
                                        {
                                            builder.AppendLine();
                                            builder.AppendFormat("{0}, {1}", actorGeneric.actorName, actorGeneric.arc.name);
                                        }
                                        else { Debug.LogWarningFormat("Invalid actor (Null) for secret {0} listOfSecretActors[{1}], actorID {2}", secret.tag, j, listOfSecretActors[j]); }
                                    }
                                    builder.Append("</size>");
                                    interactSecret.known.text = builder.ToString();
                                }
                                else
                                {
                                    //single actor knows
                                    actorGeneric = GameManager.i.dataScript.GetActor(listOfSecretActors[0]);
                                    if (actorGeneric != null)
                                    { interactSecret.known.text = string.Format("{0}{1}<size=90%>{2}, {3}</size>", knowsSecretHeader, "\n", actorGeneric.actorName, actorGeneric.arc.name); }
                                    else { Debug.LogWarningFormat("Invalid actor (Null) for secret {0} listOfSecretActors[{1}], actorID {2}", secret.tag, 0, listOfSecretActors[0]); }
                                }
                            }
                            else { interactSecret.known.text = string.Format("{0}{1}<size=90%>Nobody apart from you</size>", knowsSecretHeader, "\n"); }
                            //effects
                            builder.Clear();
                            builder.AppendFormat("{0}<size=90%>", effectsSecretHeader);
                            for (int j = 0; j < secret.listOfEffects.Count; j++)
                            {
                                effectSecret = secret.listOfEffects[j];
                                if (effectSecret != null)
                                {
                                    builder.AppendLine();
                                    switch (effectSecret.typeOfEffect.name)
                                    {
                                        case "Good": builder.AppendFormat("{0}", GameManager.Formatt(effectSecret.description, ColourType.goodText)); break;
                                        case "Bad": builder.AppendFormat("{0}", GameManager.Formatt(effectSecret.description, ColourType.badText)); break;
                                        default: Debug.LogWarningFormat("Unrecognised effect.typeOfEffect.name \"{0}\" for effect {1}", effectSecret.typeOfEffect.name, effectSecret.name); break;
                                    }
                                }
                                else { Debug.LogWarningFormat("Invalid effect (Null) in secret {0}, listOfEffects[{1}]", secret.tag, j); }
                            }
                            builder.Append("</size>");
                            interactSecret.effects.text = builder.ToString();
                        }
                        else { Debug.LogWarningFormat("Invalid secret (Null) in listOfSecrets[{0}], for {1}", i, inputData.who); }
                    }
                    else
                    {
                        //deactivate secret
                        arrayOfSecrets[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //no secrets, deactivate all
                for (int i = 0; i < arrayOfSecrets.Length; i++)
                { arrayOfSecrets[i].gameObject.SetActive(false); }
                //none present
                tab3NonePresent.gameObject.SetActive(true);
            }
        }
        else { Debug.LogWarning("Invalid listOfSecrets (Null)"); }
    }
    #endregion

    #region UpdateGearButtons
    /// <summary>
    /// Handles gear page 6 option buttons texts and colours. Assumes that a button has been pressed (isPressed = true)
    /// </summary>
    /// <param name="gear"></param>
    private void UpdateGearButtons(TabbedGear gear)
    {
        switch (gear)
        {
            case TabbedGear.Normal:
                //image button colours
                tab6Interaction0.image.color = pageOptionActiveColour;
                tab6Interaction1.image.color = pageOptionDormantColour;
                //text colours
                tab6Interaction0.title.color = tabTextActiveColour;
                tab6Interaction1.title.color = tabTextDormantColour;
                //header
                tab6Header.text = "Gear";
                break;
            case TabbedGear.Capture:
                //image button colours
                tab6Interaction0.image.color = pageOptionDormantColour;
                tab6Interaction1.image.color = pageOptionActiveColour;
                //text colours
                tab6Interaction0.title.color = tabTextDormantColour;
                tab6Interaction1.title.color = tabTextActiveColour;
                //header
                tab6Header.text = "Devices";
                break;
            default: Debug.LogWarningFormat("Unrecognised TabbedHistory \"{0}\"", gear); break;
        }
        /*//update index
        gearOptionIndex = gear;*/

        //reset controller buttons (they unselect once you click on a sprite 'button')
        UpdateControllerButton(inputData.who);
    }
    #endregion

    #region OpenGear
    /// <summary>
    /// All in one open/update Gear page (Player/Subordinates actor sets only)
    /// </summary>
    private void OpenGear(TabbedGear gearType)
    {
        int numOfGear;
        listOfGear.Clear();
        tab6NonePresent.gameObject.SetActive(false);
        switch (gearType)
        {
            case TabbedGear.Normal:
                //set panels
                tab6GearPanel.gameObject.SetActive(true);
                tab6DevicePanel.gameObject.SetActive(false);
                //player/character
                switch (inputData.who)
                {
                    case TabbedUIWho.Player:
                        //can have multiple gear items
                        listOfGear.AddRange(GameManager.i.playerScript.GetListOfGear());
                        break;
                    case TabbedUIWho.Subordinates:
                        //can only have a single item of gear
                        gearName = arrayOfActorsTemp[currentSideTabIndex].GetGearName();
                        if (string.IsNullOrEmpty(gearName) == false)
                        { listOfGear.Add(gearName); }
                        break;
                    default: Debug.LogWarningFormat("Unrecognised inputData.who \"{0}\"", inputData.who); break;
                }
                if (listOfGear != null)
                {
                    numOfGear = listOfGear.Count;
                    if (numOfGear > 0)
                    {
                        for (int i = 0; i < arrayOfGear.Length; i++)
                        {
                            if (i < numOfGear)
                            {
                                gear = GameManager.i.dataScript.GetGear(listOfGear[i]);
                                if (gear != null)
                                {
                                    //activate gear
                                    arrayOfGear[i].gameObject.SetActive(true);
                                    interactGear = arrayOfGear[i];
                                    //populate prefab data
                                    interactGear.background.color = gearColour;
                                    //portrait
                                    interactGear.portrait.sprite = gear.sprite;
                                    //rarity
                                    gearName = "Unknown";
                                    switch (gear.rarity.name)
                                    {
                                        case "Common": gearName = GameManager.Formatt(gear.rarity.name, ColourType.badText); break;
                                        case "Rare": gearName = GameManager.Formatt(gear.rarity.name, ColourType.neutralText); break;
                                        case "Unique": gearName = GameManager.Formatt(gear.rarity.name, ColourType.goodText); break;
                                        case "Special": gearName = GameManager.Formatt(gear.rarity.name, ColourType.goodText); break;
                                        default: Debug.LogWarningFormat("Unrecognised gear.rarity.name \"{0}\"", gear.rarity.name); break;
                                    }
                                    //descriptor (different for Player or Subordinate)
                                    interactGear.descriptor.text = string.Format("{0}{1}<size=90%>{2}{3}{4}</size>{5}{6}{7}", GameManager.Formatt(gear.tag.ToUpper(), ColourType.neutralText), "\n", gear.description, "\n", "\n",
                                        gearName, "\n", GameManager.Formatt(gear.type.name, ColourType.salmonText));
                                    // - - - Uses
                                    builder.Clear();
                                    builder.Append(GameManager.Formatt("Gear Uses", ColourType.neutralText));
                                    builder.Append("<size=85%>");
                                    //personal use
                                    builder.AppendLine();
                                    if (gear.listOfPersonalEffects != null && gear.listOfPersonalEffects.Count > 0)
                                    { builder.Append(GameManager.Formatt("Personal use", ColourType.salmonText)); }
                                    else
                                    { builder.Append(GameManager.Formatt("Personal use - No", ColourType.greyText)); }
                                    //AI use
                                    builder.AppendLine();
                                    if (gear.aiHackingEffect != null)
                                    { builder.Append(GameManager.Formatt("Hack AI use", ColourType.salmonText)); }
                                    else
                                    { builder.Append(GameManager.Formatt("Hack AI use - No", ColourType.greyText)); }
                                    //move use
                                    builder.AppendLine();
                                    if (gear.type.name.Equals("Movement", StringComparison.Ordinal) == true)
                                    { builder.Append(GameManager.Formatt("Movement", ColourType.salmonText)); }
                                    else
                                    { builder.Append(GameManager.Formatt("Movement - No", ColourType.greyText)); }
                                    //invisibility use
                                    builder.AppendLine();
                                    if (gear.type.name.Equals("Invisibility", StringComparison.Ordinal) == true)
                                    { builder.Append(GameManager.Formatt("Invisibility", ColourType.salmonText)); }
                                    else
                                    { builder.Append(GameManager.Formatt("Invisibility - No", ColourType.greyText)); }
                                    //target use
                                    builder.AppendLine();
                                    if (gear.type.name.Equals("Infiltration", StringComparison.Ordinal) == true)
                                    { builder.Append(GameManager.Formatt("Targets - ALL", ColourType.salmonText)); }
                                    else
                                    { builder.Append(GameManager.Formatt("Targets - Some", ColourType.salmonText)); }
                                    //has been used this turn
                                    if (gear.timesUsed > 0)
                                    {
                                        builder.AppendLine();
                                        builder.Append(GameManager.Formatt("Already USED this Turn", ColourType.badText));
                                    }
                                    builder.Append("</size>");
                                    interactGear.uses.text = builder.ToString();
                                    // - - - Stats
                                    builder.Clear();
                                    builder.Append(GameManager.Formatt("Statistics", ColourType.neutralText));
                                    builder.AppendLine();
                                    builder.Append("<size=85%>");
                                    builder.AppendFormat("Times gear Used  <b>{0}</b>{1}", gear.statTimesUsed, "\n");
                                    builder.AppendFormat("Times gear Compromised  <b>{0}</b>{1}", gear.statTimesCompromised, "\n");
                                    builder.AppendFormat("Power used to save Gear  <b>{0}</b>{1}", gear.statPowerSpent, "\n");
                                    builder.AppendFormat("Times gear Gifted  <b>{0}</b>{1}", gear.statTimesGiven, "\n");
                                    builder.AppendFormat("Had gear for (days)  <b>{0}</b>", currentTurn - gear.statTurnObtained);
                                    builder.Append("</size>");
                                    interactGear.stats.text = builder.ToString();
                                }
                                else { Debug.LogWarningFormat("Invalid gear (Null) in listOfGear[{0}], for {1}", i, inputData.who); }
                            }
                            else
                            {
                                //deactivate gear
                                arrayOfGear[i].gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        //no gear, deactivate all
                        for (int i = 0; i < arrayOfGear.Length; i++)
                        { arrayOfGear[i].gameObject.SetActive(false); }
                        //none present
                        tab6GearPanel.gameObject.SetActive(false);
                        tab6NonePresent.gameObject.SetActive(true);
                    }
                }
                else { Debug.LogWarning("Invalid listOfGear (Null)"); }
                break;
            case TabbedGear.Capture:
                //auto player only -> set panels
                tab6GearPanel.gameObject.SetActive(false);
                tab6DevicePanel.gameObject.SetActive(true);
                //arrayOfDevices
                bool[] arrayOfBools = GameManager.i.playerScript.GetArrayOfCaptureTools();
                if (arrayOfBools != null)
                {
                    counter = 0;
                    for (int i = 0; i < arrayOfBools.Length; i++)
                    {
                        if (arrayOfBools[i] == true)
                        {
                            //device present
                            counter++;
                            arrayOfDevices[i].gameObject.SetActive(true);
                            interactDevice = arrayOfDevices[i];
                            if (interactDevice != null)
                            {
                                interactDevice.background.color = deviceColour;
                                interactDevice.portrait.sprite = arrayOfCaptureTools[i].sprite;
                                interactDevice.descriptor.text = string.Format("<size=110%>{0}</size>{1}{2}{3}",
                                    GameManager.Formatt(arrayOfCaptureTools[i].tag, ColourType.neutralText), "\n", "\n", arrayOfCaptureTools[i].descriptor);
                                interactDevice.usage.text = string.Format("Use at Innocence Level{0}{1}{2}{3}{4}<size=90%>Single Use Only</size>", "\n", "\n",
                                    GameManager.i.guiScript.GetNormalStars(arrayOfCaptureTools[i].innocenceLevel), "\n", "\n");
                            }
                            else { Debug.LogErrorFormat("Invalid arrayOfDevices[{0}] (Null)", i); }
                        }
                        else
                        {
                            //not present
                            arrayOfDevices[i].gameObject.SetActive(false);
                        }
                    }
                    if (counter == 0)
                    {
                        //none present
                        tab6DevicePanel.gameObject.SetActive(false);
                        tab6NonePresent.gameObject.SetActive(true);

                    }
                }
                else { Debug.LogError("Invalid arrayOfBools (Null)"); }
                break;
        }
        UpdateGearButtons(gearType);
    }
    #endregion

    #region OpenInvestigations
    /// <summary>
    /// All in one open/update Investigation page (Player only)
    /// </summary>
    private void OpenInvestigations()
    {
        int count;
        listOfInvestigations.Clear();
        listOfInvestigations.AddRange(GameManager.i.playerScript.GetListOfInvestigations());
        if (listOfInvestigations != null)
        {
            count = listOfInvestigations.Count;
            if (count > 0)
            {
                tab4NonePresent.gameObject.SetActive(false);
                for (int i = 0; i < arrayOfInvestigations.Length; i++)
                {
                    if (i < count)
                    {
                        //activate investigation
                        arrayOfInvestigations[i].gameObject.SetActive(true);
                        investigation = listOfInvestigations[i];
                        if (investigation != null)
                        {
                            interactInvest = arrayOfInvestigations[i];
                            if (interactInvest != null)
                            {
                                //background
                                interactInvest.background.color = investigationColour;
                                //portrait -> lead investigator
                                if (investigation.lead != ActorHQ.None)
                                {
                                    actorGeneric = GameManager.i.dataScript.GetHqHierarchyActor(investigation.lead);
                                    if (actorGeneric != null)
                                    { interactInvest.portrait.sprite = actorGeneric.sprite; }
                                    else { Debug.LogWarningFormat("Invalid sprite (Null) for lead {0},  investigation \"{1}\", ref {2}", investigation.lead, investigation.tag, investigation.reference); }
                                }
                                else { Debug.LogWarningFormat("Invalid lead (ActorHQ.None) for investigation \"{0}\", ref {1}", investigation.tag, investigation.reference); }
                                //left text -> tag + status + originating city
                                builder.Clear();
                                builder.Append(GameManager.Formatt(investigation.tag, ColourType.neutralText));
                                builder.AppendLine();
                                builder.AppendLine();
                                builder.Append("Lead Investigator");
                                builder.AppendLine();
                                builder.Append(GameManager.Formatt(actorGeneric.actorName, ColourType.neutralText));
                                builder.AppendLine();
                                builder.Append(GameManager.Formatt(GameManager.i.hqScript.GetHqTitle(actorGeneric.statusHQ), ColourType.salmonText));
                                interactInvest.leftText.text = builder.ToString();
                                //middle text -> evidence 
                                builder.Clear();
                                builder.Append("Evidence");
                                builder.AppendLine();
                                builder.Append(GameManager.i.guiScript.GetNormalStars(investigation.evidence));
                                builder.AppendLine();
                                builder.AppendLine();
                                builder.Append("Status");
                                builder.AppendLine();
                                switch (investigation.status)
                                {
                                    case InvestStatus.Ongoing: builder.Append(GameManager.Formatt(Convert.ToString(investigation.status), ColourType.neutralText)); break;
                                    case InvestStatus.Resolution: builder.Append(GameManager.Formatt(Convert.ToString(investigation.status), ColourType.badText)); break;
                                    default: builder.Append(GameManager.Formatt(Convert.ToString(investigation.status), ColourType.normalText)); break;
                                }
                                interactInvest.middleText.text = builder.ToString();
                                //right text -> duration and timer
                                builder.Clear();
                                if (investigation.timer > -1)
                                {
                                    builder.Append("Investigation will conclude");
                                    builder.AppendLine();
                                    investString = string.Format("in {0} day{1}", investigation.timer, investigation.timer != 1 ? "s" : "");
                                    builder.Append(GameManager.Formatt(investString, ColourType.badText));
                                    builder.AppendLine();
                                    builder.AppendLine();
                                    builder.Append("Expected outcome");
                                    builder.AppendLine();
                                    switch (investigation.outcome)
                                    {
                                        case InvestOutcome.Guilty: builder.Append(GameManager.Formatt("Guilty", ColourType.badText)); break;
                                        case InvestOutcome.Innocent: builder.Append(GameManager.Formatt("Innocent", ColourType.goodText)); break;
                                        default: builder.Append(GameManager.Formatt("Unknown", ColourType.whiteText)); break;
                                    }
                                }
                                else
                                {
                                    builder.Append("You have been under investigation for");
                                    builder.AppendLine();
                                    int numOfDays = currentTurn - investigation.turnStart;
                                    investString = string.Format("{0} day{1}", numOfDays, numOfDays != 1 ? "s" : "");
                                    builder.Append(GameManager.Formatt(investString, ColourType.neutralText));
                                    builder.AppendLine();
                                    builder.AppendLine();
                                    builder.Append("Expected outcome");
                                    builder.AppendLine();
                                    switch (investigation.evidence)
                                    {
                                        case 0: builder.Append(GameManager.Formatt("Guilty", ColourType.badText)); break;
                                        case 1:
                                        case 2: builder.Append(GameManager.Formatt("Uncertain", ColourType.neutralText)); break;
                                        case 3: builder.Append(GameManager.Formatt("Innocent", ColourType.goodText)); break;
                                        default: builder.Append(GameManager.Formatt("Unknown", ColourType.whiteText)); break;
                                    }
                                }
                                interactInvest.rightText.text = builder.ToString();
                            }
                            else { Debug.LogWarningFormat("Invalid interactInvest (Null) for arrayOfInvestigations[{0}]", i); }
                        }
                        else { Debug.LogWarningFormat("Invalid investigation (Null) for listOfInvestigations[{0}]", i); }
                    }
                    else
                    {
                        //deactivate investigation
                        arrayOfInvestigations[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //deactivate all
                for (int i = 0; i < arrayOfInvestigations.Length; i++)
                { arrayOfInvestigations[i].gameObject.SetActive(false); }
                //none present
                tab4NonePresent.gameObject.SetActive(true);
            }
        }
        else { Debug.LogWarning("Invalid listOfInvestigations (Null)"); }
    }
    #endregion

    #region OpenStats
    /// <summary>
    /// All in one open page/update for Statistics (varies which ones depending on actorSet)
    /// </summary>
    private void OpenStats()
    {
        switch (inputData.who)
        {
            case TabbedUIWho.Player:
                //toggle modules on all
                for (int i = 0; i < arrayOfStats.Length; i++)
                { arrayOfStats[i].gameObject.SetActive(true); }
                //Overview
                interactStat = arrayOfStats[0];
                interactStat.header.text = "Service";
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(true);
                interactStat.listOfItems[3].gameObject.SetActive(false);
                interactStat.listOfItems[0].descriptor.text = "Days on the Job";
                interactStat.listOfItems[0].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.PlayerDaysOnJob));
                interactStat.listOfItems[1].descriptor.text = "Cities";
                interactStat.listOfItems[1].result.text = Convert.ToString(GameManager.i.campaignScript.GetScenarioIndex() + 1);
                interactStat.listOfItems[2].descriptor.text = "Objectives Completed";
                interactStat.listOfItems[2].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.ObjectivesComplete));
                //Evasion
                arrayOfStats[1].header.text = "Evasion";
                interactStat = arrayOfStats[1];
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(true);
                interactStat.listOfItems[3].gameObject.SetActive(true);
                interactStat.listOfItems[0].descriptor.text = "Times Captured";
                interactStat.listOfItems[0].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.PlayerCaptured));
                interactStat.listOfItems[1].descriptor.text = "Times Betrayed";
                interactStat.listOfItems[1].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.PlayerBetrayed));
                interactStat.listOfItems[2].descriptor.text = "Times Lying Low";
                interactStat.listOfItems[2].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.PlayerLieLowTimes));
                interactStat.listOfItems[3].descriptor.text = "Days spent Lying Low";
                interactStat.listOfItems[3].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.PlayerLieLowDays));

                //Mental State
                arrayOfStats[2].header.text = "Mental State";
                interactStat = arrayOfStats[2];
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(true);
                interactStat.listOfItems[3].gameObject.SetActive(true);
                interactStat.listOfItems[0].descriptor.text = "Number of Breakdowns";
                interactStat.listOfItems[0].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.PlayerBreakdown));
                interactStat.listOfItems[1].descriptor.text = "Times Stressed";
                interactStat.listOfItems[1].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.PlayerTimesStressed));
                interactStat.listOfItems[2].descriptor.text = "Times Super Stressed";
                interactStat.listOfItems[2].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.PlayerSuperStressed));
                interactStat.listOfItems[3].descriptor.text = "Number of Relationship Conflicts";
                interactStat.listOfItems[3].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.ActorConflicts));

                //Actions
                arrayOfStats[3].header.text = "Actions";
                interactStat = arrayOfStats[3];
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(true);
                interactStat.listOfItems[3].gameObject.SetActive(true);
                interactStat.listOfItems[0].descriptor.text = "Number of Move Actions";
                interactStat.listOfItems[0].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.PlayerMoveActions));
                interactStat.listOfItems[1].descriptor.text = "Number of District Actions";
                interactStat.listOfItems[1].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.PlayerNodeActions));
                interactStat.listOfItems[2].descriptor.text = "Number of Manage Actions";
                interactStat.listOfItems[2].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.PlayerManageActions));
                interactStat.listOfItems[3].descriptor.text = "Times you've Done Nothing";
                interactStat.listOfItems[3].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.PlayerDoNothing));

                //Targets
                interactStat = arrayOfStats[4];
                interactStat.header.text = "Targets";
                int attempts = GameManager.i.dataScript.StatisticGetCampaign(StatType.TargetAttempts);
                int successes = GameManager.i.dataScript.StatisticGetCampaign(StatType.TargetSuccesses);
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(true);
                interactStat.listOfItems[3].gameObject.SetActive(false);
                interactStat.listOfItems[0].descriptor.text = "Number of Target Attempts (All)";
                interactStat.listOfItems[0].result.text = Convert.ToString(attempts);
                interactStat.listOfItems[1].descriptor.text = "Successful Target Attempts";
                interactStat.listOfItems[1].result.text = Convert.ToString(successes);
                interactStat.listOfItems[2].descriptor.text = "Failed Target Attempts";
                interactStat.listOfItems[2].result.text = Convert.ToString(attempts - successes);

                //Crisis
                interactStat = arrayOfStats[5];
                interactStat.header.text = "District Crisis";
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(false);
                interactStat.listOfItems[3].gameObject.SetActive(false);
                interactStat.listOfItems[0].descriptor.text = "Number of Crisis";
                interactStat.listOfItems[0].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.NodeCrisis));
                interactStat.listOfItems[1].descriptor.text = "Crisis that have Exploded";
                interactStat.listOfItems[1].result.text = Convert.ToString(GameManager.i.dataScript.StatisticGetCampaign(StatType.NodeCrisisExplodes));
                break;
            case TabbedUIWho.Subordinates:
                actorGeneric = arrayOfActorsTemp[currentSideTabIndex];
                //toggle modules on/off
                arrayOfStats[0].gameObject.SetActive(true);
                arrayOfStats[1].gameObject.SetActive(true);
                arrayOfStats[2].gameObject.SetActive(true);
                arrayOfStats[3].gameObject.SetActive(true);
                arrayOfStats[4].gameObject.SetActive(false);
                arrayOfStats[5].gameObject.SetActive(false);
                //Overview
                interactStat = arrayOfStats[0];
                interactStat.header.text = "Service";
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(true);
                interactStat.listOfItems[3].gameObject.SetActive(false);
                interactStat.listOfItems[0].descriptor.text = "Days on the Job";
                interactStat.listOfItems[0].result.text = Convert.ToString(actorGeneric.numOfDaysOnMap);
                interactStat.listOfItems[1].descriptor.text = "Days in Reserves";
                interactStat.listOfItems[1].result.text = Convert.ToString(actorGeneric.numOfDaysReserves);
                interactStat.listOfItems[2].descriptor.text = "Cities";
                interactStat.listOfItems[2].result.text = Convert.ToString(actorGeneric.numOfCities);
                //Evasion
                arrayOfStats[1].header.text = "Evasion";
                interactStat = arrayOfStats[1];
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(false);
                interactStat.listOfItems[3].gameObject.SetActive(false);
                interactStat.listOfItems[0].descriptor.text = "Times Captured";
                interactStat.listOfItems[0].result.text = Convert.ToString(actorGeneric.numOfTimesCaptured);
                interactStat.listOfItems[1].descriptor.text = "Days spent Lying Low";
                interactStat.listOfItems[1].result.text = Convert.ToString(actorGeneric.numOfDaysLieLow);
                //Mental State
                arrayOfStats[2].header.text = "Mental State";
                interactStat = arrayOfStats[2];
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(true);
                interactStat.listOfItems[3].gameObject.SetActive(true);
                interactStat.listOfItems[0].descriptor.text = "Number of Breakdowns";
                interactStat.listOfItems[0].result.text = Convert.ToString(actorGeneric.numOfTimesBreakdown);
                interactStat.listOfItems[1].descriptor.text = "Times Stress Leave taken";
                interactStat.listOfItems[1].result.text = Convert.ToString(actorGeneric.numOfTimesStressLeave);
                interactStat.listOfItems[2].descriptor.text = "Days spent Stressed";
                interactStat.listOfItems[2].result.text = Convert.ToString(actorGeneric.numOfDaysStressed);
                interactStat.listOfItems[3].descriptor.text = "Number of Relationship Conflicts";
                interactStat.listOfItems[3].result.text = Convert.ToString(actorGeneric.numOfTimesConflict);
                //Votes
                arrayOfStats[3].header.text = "Reviews";
                interactStat = arrayOfStats[3];
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(true);
                interactStat.listOfItems[3].gameObject.SetActive(false);
                interactStat.listOfItems[0].descriptor.text = "Votes For";
                interactStat.listOfItems[0].result.text = Convert.ToString(actorGeneric.numOfVotesFor);
                interactStat.listOfItems[1].descriptor.text = "Votes Against";
                interactStat.listOfItems[1].result.text = Convert.ToString(actorGeneric.numOfVotesAgainst);
                interactStat.listOfItems[2].descriptor.text = "Votes Abstained";
                interactStat.listOfItems[2].result.text = Convert.ToString(actorGeneric.numOfVotesAbstained);
                break;
            case TabbedUIWho.HQ:
                actorGeneric = arrayOfActorsTemp[currentSideTabIndex];
                //toggle modules on/off
                arrayOfStats[0].gameObject.SetActive(true);
                arrayOfStats[1].gameObject.SetActive(false);
                arrayOfStats[2].gameObject.SetActive(false);
                arrayOfStats[3].gameObject.SetActive(true);
                arrayOfStats[4].gameObject.SetActive(false);
                arrayOfStats[5].gameObject.SetActive(false);
                //Power
                arrayOfStats[0].header.text = "Power";
                interactStat = arrayOfStats[0];
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(false);
                interactStat.listOfItems[3].gameObject.SetActive(false);
                interactStat.listOfItems[0].descriptor.text = "Power Gained while at HQ";
                interactStat.listOfItems[0].result.text = Convert.ToString(actorGeneric.powerGainedAtHQ);
                interactStat.listOfItems[1].descriptor.text = "Power Lost while at HQ";
                interactStat.listOfItems[1].result.text = Convert.ToString(actorGeneric.powerLostAtHQ);
                //Votes
                arrayOfStats[3].header.text = "Reviews";
                interactStat = arrayOfStats[3];
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(true);
                interactStat.listOfItems[3].gameObject.SetActive(false);
                interactStat.listOfItems[0].descriptor.text = "Votes For";
                interactStat.listOfItems[0].result.text = Convert.ToString(actorGeneric.numOfVotesFor);
                interactStat.listOfItems[1].descriptor.text = "Votes Against";
                interactStat.listOfItems[1].result.text = Convert.ToString(actorGeneric.numOfVotesAgainst);
                interactStat.listOfItems[2].descriptor.text = "Votes Abstained";
                interactStat.listOfItems[2].result.text = Convert.ToString(actorGeneric.numOfVotesAbstained);
                break;
            case TabbedUIWho.Reserves:
                actorGeneric = arrayOfActorsTemp[currentSideTabIndex];
                //toggle modules on/off
                arrayOfStats[0].gameObject.SetActive(true);
                arrayOfStats[1].gameObject.SetActive(true);
                arrayOfStats[2].gameObject.SetActive(false);
                arrayOfStats[3].gameObject.SetActive(true);
                arrayOfStats[4].gameObject.SetActive(false);
                arrayOfStats[5].gameObject.SetActive(false);
                //Overview
                interactStat = arrayOfStats[0];
                interactStat.header.text = "Service";
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(true);
                interactStat.listOfItems[3].gameObject.SetActive(false);
                interactStat.listOfItems[0].descriptor.text = "Days on the Job";
                interactStat.listOfItems[0].result.text = Convert.ToString(actorGeneric.numOfDaysOnMap);
                interactStat.listOfItems[1].descriptor.text = "Days in Reserves";
                interactStat.listOfItems[1].result.text = Convert.ToString(actorGeneric.numOfDaysReserves);
                interactStat.listOfItems[2].descriptor.text = "Cities";
                interactStat.listOfItems[2].result.text = Convert.ToString(actorGeneric.numOfCities);
                //Reserve specifics
                interactStat = arrayOfStats[1];
                interactStat.header.text = "Reserve Status";
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(true);
                interactStat.listOfItems[3].gameObject.SetActive(true);
                interactStat.listOfItems[0].descriptor.text = "Times Promised";
                interactStat.listOfItems[0].result.text = Convert.ToString(actorGeneric.numOfTimesPromised);
                interactStat.listOfItems[1].descriptor.text = "Times Reassured";
                interactStat.listOfItems[1].result.text = Convert.ToString(actorGeneric.numOfTimesReassured);
                interactStat.listOfItems[2].descriptor.text = "Times Bullied";
                interactStat.listOfItems[2].result.text = Convert.ToString(actorGeneric.numOfTimesBullied);
                interactStat.listOfItems[3].descriptor.text = "Days spent Unhappy";
                interactStat.listOfItems[3].result.text = Convert.ToString(actorGeneric.numOfDaysUnhappy);
                //Votes
                arrayOfStats[3].header.text = "Reviews";
                interactStat = arrayOfStats[3];
                interactStat.listOfItems[0].gameObject.SetActive(true);
                interactStat.listOfItems[1].gameObject.SetActive(true);
                interactStat.listOfItems[2].gameObject.SetActive(true);
                interactStat.listOfItems[3].gameObject.SetActive(false);
                interactStat.listOfItems[0].descriptor.text = "Votes For";
                interactStat.listOfItems[0].result.text = Convert.ToString(actorGeneric.numOfVotesFor);
                interactStat.listOfItems[1].descriptor.text = "Votes Against";
                interactStat.listOfItems[1].result.text = Convert.ToString(actorGeneric.numOfVotesAgainst);
                interactStat.listOfItems[2].descriptor.text = "Votes Abstained";
                interactStat.listOfItems[2].result.text = Convert.ToString(actorGeneric.numOfVotesAbstained);
                break;
            default: Debug.LogWarningFormat("Unrecognised inputData.who \"{0}\"", inputData.who); break;
        }
    }
    #endregion

    #region UpdateOrgButtons
    /// <summary>
    /// Handles gear page 6 option buttons texts and colours. Assumes that a button has been pressed (isPressed = true)
    /// </summary>
    /// <param name="orgType"></param>
    private void UpdateOrgButtons(TabbedOrg orgType)
    {
        switch (orgType)
        {
            case TabbedOrg.Organisation:
                //image button colours
                tab9Interaction0.image.color = pageOptionActiveColour;
                tab9Interaction1.image.color = pageOptionDormantColour;
                //text colours
                tab9Interaction0.title.color = tabTextActiveColour;
                tab9Interaction1.title.color = tabTextDormantColour;
                //header
                tab9Header.text = "Organisations";
                break;
            case TabbedOrg.MegaCorp:
                //image button colours
                tab9Interaction0.image.color = pageOptionDormantColour;
                tab9Interaction1.image.color = pageOptionActiveColour;
                //text colours
                tab9Interaction0.title.color = tabTextDormantColour;
                tab9Interaction1.title.color = tabTextActiveColour;
                //header
                tab9Header.text = "MegaCorps";
                break;
            default: Debug.LogWarningFormat("Unrecognised TabbedOrg \"{0}\"", orgType); break;
        }
        //reset controller buttons (they unselect once you click on a sprite 'button')
        UpdateControllerButton(inputData.who);
    }
    #endregion

    #region OpenOrganisation
    /// <summary>
    /// Organisations and MegaCorps -> Player only
    /// </summary>
    private void OpenOrganisation(TabbedOrg orgType)
    {
        int numOfOrgs;
        if (orgType == TabbedOrg.Organisation)
        {
            listOfOrganisations.Clear();
            listOfOrganisations.AddRange(GameManager.i.dataScript.GetListOfInContactOrganisations());
            if (listOfOrganisations != null)
            {
                //toggle off all
                for (int i = 0; i < arrayOfOrgs.Length; i++)
                { arrayOfOrgs[i].gameObject.SetActive(false); }
                //how many orgs player currently in contact with
                numOfOrgs = listOfOrganisations.Count;
                if (numOfOrgs > 0)
                {
                    //toggle on
                    tab9OrgPanel.gameObject.SetActive(true);
                    tab9CorpPanel.gameObject.SetActive(false);
                    tab9NoneKnown.gameObject.SetActive(false);
                    //loop MegaCorps (index 0 corresponds to enum.MegaCorpType.None)
                    for (int i = 0; i < numOfOrgs; i++)
                    {
                        organisation = listOfOrganisations[i];
                        if (organisation != null)
                        {
                            //activate org prefab
                            arrayOfOrgs[i].gameObject.SetActive(true);
                            interactOrg = arrayOfOrgs[i];
                            //populate prefab data
                            interactOrg.background.color = orgColour;
                            interactOrg.portrait.sprite = organisation.sprite;
                            interactOrg.descriptor.text = string.Format("<size=110%>{0}</size>{1}{2}{3}",
                                        GameManager.Formatt(organisation.tag, ColourType.neutralText), "\n", "\n", organisation.descriptor);
                            interactOrg.stats.text = string.Format("Reputation{0}{1}{2}Freedom{3}{4}{5}", "\n", GameManager.i.guiScript.GetNormalStars(organisation.GetReputation()), "\n", "\n",
                                GameManager.i.guiScript.GetNormalStars(organisation.GetFreedom()), "\n");
                            interactOrg.services.text = string.Format("<size=90%>{0}</size>{1}{2}{3}",
                                        GameManager.Formatt("Services", ColourType.neutralText), "\n", "\n", organisation.orgType.services);
                        }
                        else { Debug.LogWarningFormat("Invalid organisation (Null) for listOfOrganisations[{0}]", i); }
                    }
                }
                else
                {
                    //none Known
                    tab9OrgPanel.gameObject.SetActive(false);
                    tab9CorpPanel.gameObject.SetActive(false);
                    tab9NoneKnown.gameObject.SetActive(true);
                }
            }
            else { Debug.LogWarningFormat("Invalid listOfCurrentOrganisations (Null)"); }
        }
        else
        {
            //MegaCorps
            if (listOfMegaCorps != null)
            {
                //get current reputations
                arrayOfMegaCorpReputation = GameManager.i.dataScript.GetArrayOfMegaCorpRelations();
                if (arrayOfMegaCorpReputation != null)
                {
                    //check same as max
                    numOfOrgs = listOfMegaCorps.Count;
                    if (numOfOrgs == maxNumOfCorps)
                    {
                        //toggle on
                        tab9OrgPanel.gameObject.SetActive(false);
                        tab9CorpPanel.gameObject.SetActive(true);
                        tab9NoneKnown.gameObject.SetActive(false);
                        //loop corps
                        for (int i = 1; i < numOfOrgs; i++)
                        {
                            megaCorp = listOfMegaCorps[i];
                            if (megaCorp != null)
                            {
                                //activate megaCorp prefab
                                interactCorp = arrayOfCorps[i];
                                //populate prefab data
                                interactCorp.background.color = corpColour;
                                interactCorp.portrait.sprite = megaCorp.sprite;
                                interactCorp.details.text = string.Format("<size=110%>{0}</size>{1}{2}{3}",
                                            GameManager.Formatt(megaCorp.tag, ColourType.neutralText), "\n", "\n", megaCorp.field);
                                interactCorp.descriptor.text = string.Format("<size=90%>{0}</size>{1}{2}", GameManager.Formatt("Background", ColourType.neutralText), "\n", megaCorp.descriptor);
                                interactCorp.reputation.text = string.Format("Reputation{0}{1}{2}", "\n", "\n", GameManager.i.guiScript.GetNormalStars(arrayOfMegaCorpReputation[i]));
                            }
                            else { Debug.LogWarningFormat("Invalid megaCorp (Null) for listOfMegaCorps[{0}]", i); }
                        }
                    }
                    else
                    { Debug.LogWarningFormat("Mismatch in MegaCorps (numOfOrgs {0}, should be maxNumOfCorps {1})", numOfOrgs, maxNumOfCorps); }
                }
                else { Debug.LogErrorFormat("Invalid arrayOfMegaCorpReputation (Null)"); }
            }
            else { Debug.LogErrorFormat("Invalid listOfMegaCorps (Null)"); }
        }
        UpdateOrgButtons(orgType);
    }
    #endregion

    #endregion


    #region Movement...
    //
    // - - - Movement
    //

    /// <summary>
    /// Up arrow ->  next actor Set to the left
    /// </summary>
    private void ExecuteUpArrow()
    {
        if (currentSetIndex > -1)
        {
            if (currentSetIndex > 0)
            {
                currentSetIndex -= 1;
                OpenActorSet((TabbedUIWho)currentSetIndex);
            }
            else
            {
                //roll over
                currentSetIndex = maxSetIndex;
                OpenActorSet((TabbedUIWho)currentSetIndex);
            }
        }
    }


    /// <summary>
    /// Down arrow ->  next actor Set to the Right
    /// </summary>
    private void ExecuteDownArrow()
    {
        if (currentSetIndex > -1)
        {
            if (currentSetIndex < maxSetIndex)
            {
                currentSetIndex += 1;
                OpenActorSet((TabbedUIWho)currentSetIndex);
            }
            else
            {
                //roll over
                currentSetIndex = 0;
                OpenActorSet((TabbedUIWho)currentSetIndex); ;
            }
        }
    }

    /// <summary>
    /// Right arrow
    /// </summary>
    private void ExecuteRightArrow()
    {
        if (currentTopTabIndex > -1)
        {
            if (currentTopTabIndex < maxTopTabIndex)
            {
                currentTopTabIndex += 1;
                OpenPage(currentTopTabIndex);
            }
            else
            {
                //roll over
                currentTopTabIndex = 0;
                OpenPage(currentTopTabIndex);
            }
        }
    }


    /// <summary>
    /// Left Arrow
    /// </summary>
    private void ExecuteLeftArrow()
    {
        if (currentTopTabIndex > -1)
        {
            if (currentTopTabIndex > 0)
            {
                currentTopTabIndex -= 1;
                OpenPage(currentTopTabIndex);
            }
            else
            {
                //roll over
                currentTopTabIndex = maxTopTabIndex;
                OpenPage(currentTopTabIndex);
            }
        }
    }

    /// <summary>
    /// Page Up -> Side tabs Up
    /// </summary>
    private void ExecutePageUp()
    {
        //change tab
        if (currentSideTabIndex > -1 && numOfSideTabs > 1)
        {
            if (currentSideTabIndex > 0)
            {
                currentSideTabIndex -= 1;
                OpenSideTab(currentSideTabIndex);
            }
            else
            {
                //roll over
                currentSideTabIndex = numOfSideTabs - 1;
                OpenSideTab(currentSideTabIndex);
            }
            //highlight actorSet
            UpdateControllerButton(inputData.who);
            //page
            UpdatePage();
        }
    }

    /// <summary>
    /// Page Down -> Side tabs down
    /// </summary>
    private void ExecutePageDown()
    {
        if (currentSideTabIndex > -1 && numOfSideTabs > 1)
        {
            if (currentSideTabIndex < numOfSideTabs - 1)
            {
                currentSideTabIndex += 1;
                OpenSideTab(currentSideTabIndex);
            }
            else
            {
                //roll over
                currentSideTabIndex = 0;
                OpenSideTab(currentSideTabIndex);
            }
            //highlight actorSet
            UpdateControllerButton(inputData.who);
            //page
            UpdatePage();
        }
    }

    /// <summary>
    /// Mouse wheel -> scroll up (page 7 History)
    /// </summary>
    private void ExecuteScrollUp()
    {
        if (scrollHighlightIndex > 0)
        {
            scrollHighlightIndex--;
            if (tab7ScrollRect.verticalNormalizedPosition != 1.0f)
            {
                float scrollPos = 1.0f - (float)scrollHighlightIndex / scrollMaxHighlightIndex;
                tab7ScrollRect.verticalNormalizedPosition = scrollPos;
            }
        }
    }

    /// <summary>
    /// Mouse wheel -> scroll down (page 7 History)
    /// </summary>
    private void ExecuteScrollDown()
    {
        if (scrollHighlightIndex < scrollMaxHighlightIndex)
        {
            scrollHighlightIndex++;
            float scrollPos = 1.0f - (float)scrollHighlightIndex / scrollMaxHighlightIndex;
            tab7ScrollRect.verticalNormalizedPosition = scrollPos;
        }
    }

    #endregion


    #region SubMethods...
    //
    // - - - SubMethods
    //

    #region Main subMethods...

    /// <summary>
    /// Returns actorName (normal or HQ) for header. Runs all needed checks
    /// </summary>
    /// <param name="tabIndex"></param>
    /// <returns></returns>
    private string GetActorName(int tabIndex)
    {
        string actorName = "Unknown";
        switch (inputData.who)
        {
            case TabbedUIWho.Player:
                actorName = GameManager.i.playerScript.PlayerName;
                break;
            default:
                if (tabIndex <= maxSideTabIndex)
                {
                    if (arrayOfActorsTemp[tabIndex] != null)
                    { actorName = arrayOfActorsTemp[tabIndex].actorName; }
                    else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfActorsTemp[{0}]", tabIndex); }
                }
                else { Debug.LogWarningFormat("Invalid tabIndex (is {0}, should be <= {1})", tabIndex, maxSideTabIndex); }
                break;
        }
        return actorName;
    }

    /// <summary>
    /// Updates tabbedItem in Tab0 subHeader 'Status' for current actor
    /// </summary>
    private void UpdateStatus(bool isPlayer = false)
    {
        if (isPlayer == true)
        {
            //Player
            tab0Header0.listOfItems[0].descriptor.text = GetStatus(GameManager.i.playerScript.status, GameManager.i.playerScript.inactiveStatus);
        }
        else
        {
            //Actor
            Actor actor = arrayOfActorsTemp[currentSideTabIndex];
            if (actor != null)
            { tab0Header0.listOfItems[0].descriptor.text = GetStatus(actor.Status, actor.inactiveStatus); }
            else { Debug.LogErrorFormat("Invalid actor (Null) for arrayOfActorsTemp[{0}]", currentSideTabIndex); }
        }
    }

    /// <summary>
    /// Returns a user friendly string for actor/player status as well as initialising the help tooltip
    /// </summary>
    /// <param name="status"></param>
    /// <param name="inactive"></param>
    /// <returns></returns>
    private string GetStatus(ActorStatus status, ActorInactive inactive)
    {
        string descriptor = "Unknown";
        //NOTE: every option should populate a listOfHelpData 
        switch (status)
        {
            case ActorStatus.Active:
                if (inputData.who != TabbedUIWho.Player)
                {
                    //subordinate
                    descriptor = "Available";
                    listOfHelpData = GameManager.i.helpScript.GetHelpData("active_1");
                }
                else
                {
                    //player
                    descriptor = "On the Job";
                    listOfHelpData = GameManager.i.helpScript.GetHelpData("active_0");
                }
                break;
            case ActorStatus.Reserve:
                descriptor = "Awaiting Assignment";
                listOfHelpData = GameManager.i.helpScript.GetHelpData("reserveInv_0", "reserveInv_1");
                break;
            case ActorStatus.HQ:
                descriptor = "At HQ";
                listOfHelpData = GameManager.i.helpScript.GetHelpData("hq_over_0", "hq_over_1", "hq_over_2", "hq_over_3");
                break;
            case ActorStatus.Captured:
                descriptor = "Captured";
                listOfHelpData = GameManager.i.helpScript.GetHelpData("capture_4", "capture_5", "capture_6");
                break;
            case ActorStatus.Inactive:
                switch (inactive)
                {
                    case ActorInactive.Breakdown:
                        descriptor = "Undergoing a Nervous Breakdown";
                        listOfHelpData = GameManager.i.helpScript.GetHelpData("stressBreak_0", "stressBreak_1", "stressBreak_2");
                        break;
                    case ActorInactive.LieLow:
                        descriptor = "Lying Low";
                        listOfHelpData = GameManager.i.helpScript.GetHelpData("lielow_0", "lielow_1", "lielow_2", "lielow_3");
                        break;
                    case ActorInactive.StressLeave:
                        descriptor = "On Stress Leave";
                        listOfHelpData = GameManager.i.helpScript.GetHelpData("stressLeave_0", "stressLeave_1", "stressLeave_2", "stressLeave_3");
                        break;
                    default: Debug.LogWarningFormat("Unrecognised Inactive status \"{0}\"", inactive); break;
                }
                break;
            default: Debug.LogWarningFormat("Unrecognised ActorStatus \"{0}\"", status); break;
        }
        //help
        tab0Header0.listOfItems[0].help.SetHelpTooltip(listOfHelpData);
        return descriptor;
    }

    /// <summary>
    /// Updates Power in Tab0 for current actor
    /// </summary>
    private void UpdatePower(bool isPlayer = false)
    {
        if (isPlayer == true)
        { tab0PowerText.text = Convert.ToString(GameManager.i.playerScript.Power); }
        else
        { tab0PowerText.text = Convert.ToString(arrayOfActorsTemp[currentSideTabIndex].Power); }
    }

    /// <summary>
    /// Returns Compatibility string with Player for current actor
    /// </summary>
    private string GetCompatibility(bool isPlayer = false)
    {
        string displayText = "";
        if (isPlayer == false)
        {
            int compatibility = arrayOfActorsTemp[currentSideTabIndex].GetPersonality().GetCompatibilityWithPlayer();
            displayText = GameManager.i.guiScript.GetCompatibilityStars(compatibility);
        }
        else { displayText = "What's not to Like?"; }
        return displayText;
    }

    /// <summary>
    /// If Subordinate actorSet opinion 0 highlights actor at risk of conflict by populating subHeader1 item.tag
    /// </summary>
    /// <returns></returns>
    private bool GetConflict()
    {
        bool isPossibleConflict = false;
        switch (arrayOfActorsTemp[currentSideTabIndex].GetDatapoint(ActorDatapoint.Opinion1))
        {
            case 0:
                tab0Header1.listOfItems[0].descriptor.text = "ON THE EDGE";
                tab0Header1.listOfItems[0].image.color = tabItemHelpActiveColour;
                isPossibleConflict = true;
                break;
            case 1:
                tab0Header1.listOfItems[0].descriptor.text = "<alpha=#AA>Unlikely";
                tab0Header1.listOfItems[0].image.color = tabItemHelpDormantColour;
                break;
            default:
                tab0Header1.listOfItems[0].descriptor.text = "<alpha=#AA>Happy in the Service";
                tab0Header1.listOfItems[0].image.color = tabItemHelpDormantColour;
                break;
        }
        return isPossibleConflict;
    }

    /// <summary>
    /// Populates subHeader2 item.tag for friend/Enemy relationship. Returns true if a relationship exists, false otherwise
    /// </summary>
    /// <returns></returns>
    private bool GetRelationship()
    {
        bool isRelationship = false;
        RelationshipData data = GameManager.i.dataScript.GetRelationshipData(arrayOfActorsTemp[currentSideTabIndex].slotID);
        if (data.relationship != ActorRelationship.None)
        {
            Actor actor = GameManager.i.dataScript.GetActor(data.actorID);
            if (actor != null)
            {
                tab0Header2.listOfItems[0].descriptor.text = string.Format("{0} {1}", data.relationship == ActorRelationship.Friend ? "Friends with " : "Enemies with ", actor.arc.name);
                isRelationship = true;
            }
            else
            {
                Debug.LogWarningFormat("Invalid actor (Null) for RelationshipData.actorID {0}", data.actorID);
                tab0Header2.listOfItems[0].descriptor.text = "<alpha=#AA>None";
                tab0Header2.listOfItems[0].image.color = tabItemHelpDormantColour;
            }
        }
        else
        {
            tab0Header2.listOfItems[0].descriptor.text = "<alpha=#AA>None";
            tab0Header2.listOfItems[0].image.color = tabItemHelpDormantColour;
        }
        return isRelationship;
    }


    /// <summary>
    /// Sets friends and enemies tooltip
    /// </summary>
    private void GetRelationshipTooltip()
    {
        listOfHelpData = GameManager.i.helpScript.GetHelpData("relation_0", "relation_1", "relation_2", "relation_3");
        tab0Header2.listOfItems[0].help.SetHelpTooltip(listOfHelpData);
    }


    /// <summary>
    /// Gets tabbedItems in Tab0 subHeader 'Conditions' for current actor. NOTE: Max of 10 conditions allowed
    /// Returns true if any conditions, false if none
    /// </summary>
    /// <param name="isPlayer"></param>
    private bool GetConditions(bool isPlayer = false)
    {
        int numOfItems;
        List<Condition> listOfConditions;
        numOfItems = tab0Header3.listOfItems.Count;
        if (isPlayer == true)
        {
            //Player
            listOfConditions = GameManager.i.playerScript.GetListOfConditions(inputData.side);
        }
        else
        {
            //Actor
            listOfConditions = arrayOfActorsTemp[currentSideTabIndex].GetListOfConditions();
        }
        return UpdateConditions(listOfConditions);
    }

    /// <summary>
    /// Updates condition processing for GetConditions. Returns true if anyconditions, returns false if 'None'
    /// </summary>
    /// <param name="listOfConditions"></param>
    private bool UpdateConditions(List<Condition> listOfConditions)
    {
        bool isConditions = true;
        Condition condition;
        int count = listOfConditions.Count;
        if (count > 0)
        {
            for (int i = 0; i < maxNumOfConditions; i++)
            {
                if (i < count)
                {
                    //condition present
                    condition = listOfConditions[i];
                    if (condition != null)
                    {
                        tab0Header3.listOfItems[i].gameObject.SetActive(true);
                        tab0Header3.listOfItems[i].descriptor.text = condition.tag;
                        //turn on help
                        tab0Header3.listOfItems[i].image.gameObject.SetActive(true);
                        tab0Header3.listOfItems[i].image.color = tabItemHelpActiveColour;
                        //tooltip
                        tab0Header3.listOfItems[i].help.SetHelpTooltip(GetConditionHelp(condition));
                    }
                    else
                    {
                        Debug.LogWarningFormat("Invalid condition (Null) for listOfConditions[{0}]", i);
                        tab0Header3.listOfItems[i].descriptor.text = "Unknown";
                        tab0Header3.listOfItems[i].image.color = tabItemHelpDormantColour;
                    }
                }
                else
                {
                    //disable all other items
                    tab0Header3.listOfItems[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            //No Conditions present
            tab0Header3.listOfItems[0].descriptor.text = "<alpha=#AA>None";
            tab0Header3.listOfItems[0].image.color = tabItemHelpDormantColour;
            isConditions = false;
            //disable all other items
            for (int i = 1; i < maxNumOfConditions; i++)
            { tab0Header3.listOfItems[i].gameObject.SetActive(false); }
        }
        return isConditions;
    }

    /// <summary>
    /// Gets data for condition tooltip
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    private List<HelpData> GetConditionHelp(Condition condition)
    {
        listOfHelpData.Clear();
        switch (condition.name)
        {
            case "ADDICTED":
                listOfHelpData = GameManager.i.helpScript.GetHelpData("addict_0", "addict_1", "addict_2", "addict_3");
                break;
            case "BLACKMAILER":
                listOfHelpData = GameManager.i.helpScript.GetHelpData("blackmail_0", "blackmail_1", "blackmail_2");
                break;
            case "CORRUPT":
                listOfHelpData = GameManager.i.helpScript.GetHelpData("corrupt_0", "corrupt_1", "corrupt_2");
                break;
            case "DOOMED":
                listOfHelpData = GameManager.i.helpScript.GetHelpData("doom_0", "doom_1", "doom_2", "doom_3");
                break;
            case "IMAGED":
                listOfHelpData = GameManager.i.helpScript.GetHelpData("imaged_0", "imaged_1");
                break;
            case "INCOMPETENT":
                listOfHelpData = GameManager.i.helpScript.GetHelpData("incompetent_0", "incompetent_1", "incompetent_2");
                break;
            case "QUESTIONABLE":
                listOfHelpData = GameManager.i.helpScript.GetHelpData("questionable_0", "questionable_1", "questionable_2", "questionable_3");
                break;
            case "STAR":
                listOfHelpData = GameManager.i.helpScript.GetHelpData("star_0", "star_1", "star_2");
                break;
            case "STRESSED":
                if (inputData.who == TabbedUIWho.Player)
                { listOfHelpData = GameManager.i.helpScript.GetHelpData("stress_0", "stress_1", "stress_2", "stress_3"); }
                else
                { listOfHelpData = GameManager.i.helpScript.GetHelpData("stress_4", "stress_5", "stress_6", "stress_7"); }
                break;
            case "TAGGED":
                listOfHelpData = GameManager.i.helpScript.GetHelpData("tagged_0", "tagged_1", "tagged_2");
                break;
            case "UNHAPPY":
                listOfHelpData = GameManager.i.helpScript.GetHelpData("reserveInv_0", "reserveInv_1");
                break;
            case "WOUNDED":
                listOfHelpData = GameManager.i.helpScript.GetHelpData("wounded_0", "wounded_1");
                break;
            default: Debug.LogWarningFormat("Unrecognised condition \"{0}\"", condition.name); break;
        }
        return listOfHelpData;
    }

    /// <summary>
    /// Player specific info -> Available Cures for active conditions, returns true if any cures, false if 'None'
    /// </summary>
    /// <returns></returns>
    private bool GetCures()
    {
        bool isCure = false;
        int numOfItems;
        string cureName;
        //help
        listOfHelpData = GameManager.i.helpScript.GetHelpData("cure_0", "cure_1", "cure_2", "cure_3");
        //Get cures
        List<Node> listOfCures = GameManager.i.dataScript.GetListOfCureNodes();
        numOfItems = tab0Header4.listOfItems.Count;
        int count = listOfCures.Count;
        if (count > 0)
        {
            isCure = true;
            for (int i = 0; i < maxNumOfCures; i++)
            {
                if (i < count)
                {
                    //cure present
                    cureName = string.Format("{0} at {1}", listOfCures[i].cure.cureName, listOfCures[i].nodeName);
                    if (string.IsNullOrEmpty(cureName) == false)
                    {
                        tab0Header4.listOfItems[i].gameObject.SetActive(true);
                        tab0Header4.listOfItems[i].descriptor.text = cureName;
                        //turn on help
                        tab0Header4.listOfItems[0].image.gameObject.SetActive(true);
                        tab0Header4.listOfItems[0].image.color = tabItemHelpActiveColour;
                        //tooltip
                        tab0Header4.listOfItems[0].help.SetHelpTooltip(listOfHelpData);
                    }
                    else
                    {
                        Debug.LogWarningFormat("Invalid cure (Null or Empty) for listOfCures[{0}]", i);
                        tab0Header4.listOfItems[i].descriptor.text = "Unknown";
                        tab0Header4.listOfItems[0].image.color = tabItemHelpDormantColour;
                    }
                }
                else
                {
                    //disable all other items
                    tab0Header4.listOfItems[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            //No Cures present
            tab0Header4.listOfItems[0].descriptor.text = "<alpha=#AA>None";
            tab0Header4.listOfItems[0].image.color = tabItemHelpDormantColour;
            //help
            tab0Header4.listOfItems[0].help.SetHelpTooltip(listOfHelpData);
            //disable all other items
            for (int i = 1; i < maxNumOfCures; i++)
            { tab0Header4.listOfItems[i].gameObject.SetActive(false); }
        }
        return isCure;
    }


    /// <summary>
    /// Returns actorTitle (arc name / Player / HQ title)
    /// </summary>
    /// <param name="tabIndex"></param>
    /// <returns></returns>
    private string GetActorTitle(int tabIndex)
    {
        string actorTitle = "Unknown";
        if (tabIndex > -1 && tabIndex <= maxSideTabIndex)
        {
            switch (inputData.who)
            {
                case TabbedUIWho.Subordinates:
                case TabbedUIWho.Reserves:
                    actorTitle = arrayOfActorsTemp[tabIndex].arc.name;
                    break;
                case TabbedUIWho.Player:
                    actorTitle = "Player";
                    break;
                case TabbedUIWho.HQ:
                    actorTitle = GameManager.i.hqScript.GetHqTitle((ActorHQ)(tabIndex + 1));
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised inputData.who \"{0}\"", inputData.who); break;
            }
        }
        else { Debug.LogWarningFormat("Invalid currentSideIndex \"{0}\" (should be > -1 and <= {1})", tabIndex, maxSideTabIndex); }
        return actorTitle;
    }

    /// <summary>
    /// Sets subordinate tooltip for 'conflict' subheader item
    /// </summary>
    private void GetConflictTooltip()
    {
        listOfHelpData = GameManager.i.helpScript.GetHelpData("conflict_0", "conflict_1", "conflict_2");
        tab0Header1.listOfItems[0].help.SetHelpTooltip(listOfHelpData);
    }

    #endregion

    #region Personality subMethods...

    #region UpdatePersonality
    /// <summary>
    /// Tab1 Updates personality matrix for an actor or player
    /// </summary>
    private void UpdatePersonality()
    {
        int limit;
        //personality matrix
        int[] arrayOfFactors;
        //populate array
        if (inputData.who == TabbedUIWho.Player)
        { arrayOfFactors = arrayOfPlayerFactors; }
        else { arrayOfFactors = arrayOfActorsTemp[currentSideTabIndex].GetPersonality().GetFactors(); }
        //update matrix
        if (arrayOfFactors != null)
        {
            limit = arrayOfFactors.Length;
            if (limit == maxNumOfPersonalityFactors)
            {
                for (int i = 0; i < limit; i++)
                {
                    TabbedPersonInteraction interact = arrayOfPersons[i];
                    //Actor Personality
                    switch (arrayOfFactors[i])
                    {
                        case 2:
                            //backgrounds
                            interact.middleFarLeft.color = personMiddleColour;
                            interact.middleLeft.color = personMiddleAltColour;
                            interact.middleCentre.color = personMiddleColour;
                            interact.middleRight.color = personMiddleAltColour;
                            interact.middleFarRight.color = personMiddleActiveColour;
                            //middle texts
                            interact.textMiddleFarLeft.color = personMiddleTextDormantColour;
                            interact.textMiddleLeft.color = personMiddleTextDormantColour;
                            interact.textMiddleCentre.color = personMiddleTextDormantColour;
                            interact.textMiddleRight.color = personMiddleTextDormantColour;
                            interact.textMiddleFarRight.color = personMiddleTextActiveColour;
                            break;
                        case 1:
                            //backgrounds
                            interact.middleFarLeft.color = personMiddleColour;
                            interact.middleLeft.color = personMiddleAltColour;
                            interact.middleCentre.color = personMiddleColour;
                            interact.middleRight.color = personMiddleActiveColour;
                            interact.middleFarRight.color = personMiddleColour;
                            //middle texts
                            interact.textMiddleFarLeft.color = personMiddleTextDormantColour;
                            interact.textMiddleLeft.color = personMiddleTextDormantColour;
                            interact.textMiddleCentre.color = personMiddleTextDormantColour;
                            interact.textMiddleRight.color = personMiddleTextActiveColour;
                            interact.textMiddleFarRight.color = personMiddleTextDormantColour;
                            break;
                        case 0:
                            //backgrounds
                            interact.middleFarLeft.color = personMiddleColour;
                            interact.middleLeft.color = personMiddleAltColour;
                            interact.middleCentre.color = personMiddleActiveColour;
                            interact.middleRight.color = personMiddleAltColour;
                            interact.middleFarRight.color = personMiddleColour;
                            //middle texts
                            interact.textMiddleFarLeft.color = personMiddleTextDormantColour;
                            interact.textMiddleLeft.color = personMiddleTextDormantColour;
                            interact.textMiddleCentre.color = personMiddleTextActiveColour;
                            interact.textMiddleRight.color = personMiddleTextDormantColour;
                            interact.textMiddleFarRight.color = personMiddleTextDormantColour;
                            break;
                        case -1:
                            //backgrounds
                            interact.middleFarLeft.color = personMiddleColour;
                            interact.middleLeft.color = personMiddleActiveColour;
                            interact.middleCentre.color = personMiddleColour;
                            interact.middleRight.color = personMiddleAltColour;
                            interact.middleFarRight.color = personMiddleColour;
                            //middle texts
                            interact.textMiddleFarLeft.color = personMiddleTextDormantColour;
                            interact.textMiddleLeft.color = personMiddleTextActiveColour;
                            interact.textMiddleCentre.color = personMiddleTextDormantColour;
                            interact.textMiddleRight.color = personMiddleTextDormantColour;
                            interact.textMiddleFarRight.color = personMiddleTextDormantColour;
                            break;
                        case -2:
                            //backgrounds
                            interact.middleFarLeft.color = personMiddleActiveColour;
                            interact.middleLeft.color = personMiddleAltColour;
                            interact.middleCentre.color = personMiddleColour;
                            interact.middleRight.color = personMiddleAltColour;
                            interact.middleFarRight.color = personMiddleColour;
                            //middle texts
                            interact.textMiddleFarLeft.color = personMiddleTextActiveColour;
                            interact.textMiddleLeft.color = personMiddleTextDormantColour;
                            interact.textMiddleCentre.color = personMiddleTextDormantColour;
                            interact.textMiddleRight.color = personMiddleTextDormantColour;
                            interact.textMiddleFarRight.color = personMiddleTextDormantColour;
                            break;
                        default: Debug.LogWarningFormat("Unrecognised factor \"{0}\" in arrayOfFactors[{1}]", arrayOfFactors[i], i); break;
                    }
                    //top markers Player Personality
                    if (inputData.who == TabbedUIWho.Player)
                    {
                        //don't show if Player
                        interact.topFarLeft.gameObject.SetActive(false);
                        interact.topLeft.gameObject.SetActive(false);
                        interact.topCentre.gameObject.SetActive(false);
                        interact.topRight.gameObject.SetActive(false);
                        interact.topFarRight.gameObject.SetActive(false);
                    }
                    else
                    {
                        //show if actor
                        switch (arrayOfPlayerFactors[i])
                        {
                            case 2:
                                interact.topFarLeft.gameObject.SetActive(false);
                                interact.topLeft.gameObject.SetActive(false);
                                interact.topCentre.gameObject.SetActive(false);
                                interact.topRight.gameObject.SetActive(false);
                                interact.topFarRight.gameObject.SetActive(true);
                                break;
                            case 1:
                                interact.topFarLeft.gameObject.SetActive(false);
                                interact.topLeft.gameObject.SetActive(false);
                                interact.topCentre.gameObject.SetActive(false);
                                interact.topRight.gameObject.SetActive(true);
                                interact.topFarRight.gameObject.SetActive(false);
                                break;
                            case 0:
                                interact.topFarLeft.gameObject.SetActive(false);
                                interact.topLeft.gameObject.SetActive(false);
                                interact.topCentre.gameObject.SetActive(true);
                                interact.topRight.gameObject.SetActive(false);
                                interact.topFarRight.gameObject.SetActive(false);
                                break;
                            case -1:
                                interact.topFarLeft.gameObject.SetActive(false);
                                interact.topLeft.gameObject.SetActive(true);
                                interact.topCentre.gameObject.SetActive(false);
                                interact.topRight.gameObject.SetActive(false);
                                interact.topFarRight.gameObject.SetActive(false);
                                break;
                            case -2:
                                interact.topFarLeft.gameObject.SetActive(true);
                                interact.topLeft.gameObject.SetActive(false);
                                interact.topCentre.gameObject.SetActive(false);
                                interact.topRight.gameObject.SetActive(false);
                                interact.topFarRight.gameObject.SetActive(false);
                                break;
                            default: Debug.LogWarningFormat("Unrecognised factor \"{0}\" in arrayOfPLAYERFactors[{1}]", arrayOfPlayerFactors[i], i); break;
                        }
                    }
                }
            }
            else { Debug.LogErrorFormat("Invalid arrayOfFactors (wrong size, should be {0}, is {1})", maxNumOfPersonalityFactors, limit); }
        }
        else { Debug.LogError("Invalid arrayOfFactors (Null)"); }
    }
    #endregion

    /// <summary>
    /// Returns a list of descriptors (max 5, each on a new line) for the actor / player's personality
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    private string GetPersonalityDescriptors(bool isPlayer = false)
    {
        string displayText = "Nothing stands out";
        List<string> listOfDescriptors;
        if (isPlayer == true)
        { listOfDescriptors = GameManager.i.playerScript.GetPersonality().GetListOfDescriptors(); }
        else
        { listOfDescriptors = arrayOfActorsTemp[currentSideTabIndex].GetPersonality().GetListOfDescriptors(); }
        if (listOfDescriptors != null)
        {
            switch (listOfDescriptors.Count)
            {
                case 5:
                    displayText = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", listOfDescriptors[0], "\n", listOfDescriptors[1], "\n", listOfDescriptors[2], "\n", listOfDescriptors[3], "\n", listOfDescriptors[4]);
                    break;
                case 4:
                    displayText = string.Format("{0}{1}{2}{3}{4}{5}{6}", listOfDescriptors[0], "\n", listOfDescriptors[1], "\n", listOfDescriptors[2], "\n", listOfDescriptors[3]);
                    break;
                case 3:
                    displayText = string.Format("{0}{1}{2}{3}{4}", listOfDescriptors[0], "\n", listOfDescriptors[1], "\n", listOfDescriptors[2]);
                    break;
                case 2:
                    displayText = string.Format("{0}{1}{2}", listOfDescriptors[0], "\n", listOfDescriptors[1]);
                    break;
                case 1:
                    displayText = listOfDescriptors[0];
                    break;
                default: break;
            }
        }
        else { Debug.LogWarning("Invalid listOfDescriptors (Null)"); }
        return displayText;
    }

    /// <summary>
    /// Returns a personality profile descriptor
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    private string GetPersonalityAssessment(bool isPlayer = false)
    {
        string displayText = "Doesn't fit any known profile";
        if (isPlayer == true)
        { displayText = GameManager.i.playerScript.GetPersonality().GetProfileDescriptor(); }
        else { displayText = arrayOfActorsTemp[currentSideTabIndex].GetPersonality().GetProfileDescriptor(); }
        if (string.IsNullOrEmpty(displayText) == true)
        { displayText = "Further investigation recommended"; }
        else { displayText = string.Format("{0} personality", displayText); }
        return displayText;
    }

    /// <summary>
    /// Tooltip for personality profile. Populates listOfHelpData
    /// </summary>
    /// <returns></returns>
    private void GetPersonalityTooltip(bool isPlayer = false)
    {
        listOfHelpData.Clear();
        HelpData data = new HelpData();
        data.tag = "";
        personString = "";
        if (isPlayer == true)
        { personString = GameManager.i.playerScript.GetPersonality().GetProfile(); }
        else
        {
            //default subordinate/reserve/HQ
            actorGeneric = arrayOfActorsTemp[currentSideTabIndex];
            if (actorGeneric != null)
            { personString = actorGeneric.GetPersonality().GetProfile(); }
            else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfActorsTemp[{0}]", currentSideTabIndex); }
        }
        if (string.IsNullOrEmpty(personString) == false)
        {
            //create a dynamic, temporary HelpData package
            interactProfile = GameManager.i.dataScript.GetProfile(personString);
            data.header = string.Format("{0} Personality", interactProfile.tag);
            if (interactProfile != null)
            { data.text = interactProfile.descriptor; }
            else
            {
                data.text = "Personality Profile data unavailable";
                Debug.LogWarningFormat("Invalid personality profile  (Null or Empty) for profile \"{0}\"", personString);
            }
            listOfHelpData.Add(data);
        }
        else
        {
            data.header = "Inconclusive";
            data.text = "Psychological assessments have been unable to determine a profile for this individual";
            listOfHelpData.Add(data);
        }
    }

    /// <summary>
    /// Returns a colour coded personality trait
    /// </summary>
    /// <returns></returns>
    private string GetTrait(bool isPlayer = false)
    {
        string displayText = "";
        if (isPlayer == true)
        { displayText = "All round star"; }
        else
        {
            interactTrait = arrayOfActorsTemp[currentSideTabIndex].GetTrait();
            if (interactTrait != null)
            { displayText = interactTrait.tagFormatted; }
            else { Debug.LogWarningFormat("Invalid interactTrait (Null) for arrayOfActorsTemp[{0}]", currentSideTabIndex); }
        }
        return displayText;
    }

    /// <summary>
    /// Returns tooltip for Trait (effect depends on whether a normal character or at HQ. Populates listOfHelpData
    /// </summary>
    /// <returns></returns>
    private void GetTraitTooltip(bool isPlayer = false)
    {
        listOfHelpData.Clear();
        HelpData data = new HelpData();
        data.tag = "";
        if (isPlayer == true)
        {
            data.header = "Player Trait";
            data.text = "You don't, as a Player, have a defining Trait. Everybody else does. You are free to express yourself as you wish";
            listOfHelpData.Add(data);
        }
        else
        {
            //overview
            data.header = interactTrait.tag;
            data.text = interactTrait.description;
            listOfHelpData.Add(data);
            if (inputData.who != TabbedUIWho.HQ)
            {
                //normal use
                data = new HelpData();
                data.header = "Effect";
                data.text = interactTrait.listOfTraitEffects[0].descriptor;
                listOfHelpData.Add(data);
            }
            else
            {
                //hq use
                data = new HelpData();
                data.header = "HQ Effect";
                data.text = "This trait has no effect while the character is at HQ";
                float multiplier = interactTrait.hqPowerMultiplier;
                if (multiplier != 0f)
                {
                    if (multiplier > 1.0f)
                    {
                        data.text = string.Format("Any gains or loses of {0} will be {1} more than normal", GameManager.Formatt("POWER", ColourType.salmonText),
                          GameManager.Formatt(Convert.ToString(multiplier * 100f) + " %", ColourType.salmonText));
                    }
                    else
                    {
                        data.text = string.Format("Any gains or loses of {0} will be {1} LESS than normal", GameManager.Formatt("POWER", ColourType.salmonText),
                          GameManager.Formatt(Convert.ToString(multiplier * 100f) + " %", ColourType.salmonText));
                    }
                }
                else
                {
                    multiplier = interactTrait.hqMajorMultiplier;
                    if (multiplier != 0f)
                    {
                        if (multiplier > 1.0f)
                        {
                            data.text = string.Format("Chance of {0} events affecting this character will be {1} more likely", GameManager.Formatt("LEAVE HQ", ColourType.salmonText),
                              GameManager.Formatt(Convert.ToString(multiplier * 100f) + " %", ColourType.salmonText));
                        }
                        else
                        {
                            data.text = string.Format("Chance of {0} events affecting this character will be {1} LESS likely", GameManager.Formatt("LEAVE HQ", ColourType.salmonText),
                              GameManager.Formatt(Convert.ToString(multiplier * 100f) + " %", ColourType.salmonText));
                        }
                    }
                    else
                    {
                        multiplier = interactTrait.hqMinorMultiplier;
                        if (multiplier != 0f)
                        {
                            if (multiplier > 1.0f)
                            { data.text = string.Format("GOOD events (gain Power) affecting this character will be {0} more likely", GameManager.Formatt(Convert.ToString(multiplier * 100f) + " %", ColourType.salmonText)); }
                            else
                            { data.text = string.Format("Good events (gain Power) affecting this character will be {0} LESS likely", GameManager.Formatt(Convert.ToString(multiplier * 100f) + " %", ColourType.salmonText)); }
                        }
                        else
                        { Debug.LogWarningFormat("Invalid HQ trait tooltip (nothing applies -> something should) for trait \"{0}\"", interactTrait.tag); }
                    }
                }
                listOfHelpData.Add(data);
            }
        }
    }

    /// <summary>
    /// Compatibility tooltip depends on whether is Player or not
    /// </summary>
    /// <param name="isPlayer"></param>
    private void GetCompatibilityTooltip(bool isPlayer = false)
    {
        listOfHelpData.Clear();
        if (isPlayer == true)
        {
            //Player
            HelpData data = new HelpData()
            {
                tag = "",
                header = "Compatibility",
                text = "Compatibility is a measure of another characters relationship with you based on your Personality differences. While their OPINION can VARY due to circumstances, their underlying COMPATIBILITY is CONSTANT"
            };
            listOfHelpData.Add(data);
        }
        else
        {
            //Other character
            listOfHelpData = GameManager.i.helpScript.GetHelpData("compatibility_1", "compatibility_2", "compatibility_3");
        }
        tab1Header0.help.SetHelpTooltip(listOfHelpData);
    }

    #endregion

    #region History subMethods...

    /// <summary>
    /// Gets actor/player history, returns list of text to display
    /// NOTE: TabbedHistory.Events -> HistoryActor for all, TabbedHistory.Emotions -> HistoryOpinion for actors, HistoryMood for Player
    /// </summary>
    private List<string> GetHistory(TabbedHistory history)
    {
        int max, count, limit;
        List<string> listOfText = new List<string>();
        //
        // - - - Get required History Data and convert to listOfStrings
        //
        if (inputData.who == TabbedUIWho.Player)
        {
            //Player -> Events or Mood
            if (history == TabbedHistory.Events)
            {
                //Events
                List<HistoryActor> listOfEvents = GameManager.i.dataScript.GetListOfHistoryPlayer();
                //debug -> flag prevents multiple occurences
                if (isAddDebugRecords == false)
                { listOfEvents.AddRange(DebugGetExtraHistory()); }
                //convert to text
                if (listOfEvents != null)
                {
                    //limit to last 'x' events
                    count = listOfEvents.Count;
                    max = Mathf.Min(maxNumOfScrollItems, count);
                    limit = count - max;
                    for (int i = count - 1; i >= limit; i--)
                    {
                        HistoryActor historyEvent = listOfEvents[i];
                        if (historyEvent != null)
                        {
                            //highlight any special entries, eg. start of level, in yellow
                            if (historyEvent.isHighlight == true)
                            {
                                listOfText.Add(GameManager.Formatt(string.Format("day {0}  {1}{2}", historyEvent.turn, historyEvent.text, historyEvent.district != null ? ", at " + historyEvent.district : ""),
                                  ColourType.neutralText));
                            }
                            else { listOfText.Add(string.Format("day {0}  {1}{2}", historyEvent.turn, historyEvent.text, historyEvent.district != null ? ", at " + historyEvent.district : "")); }
                        }
                        else { Debug.LogWarningFormat("Invalid HistoryActor (Null) for listOfEvents[{0}]", i); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid listOfEvents (Null) for TabbedHistory \"{0}\", Player", history); }
            }
            else
            {
                //Mood
                List<HistoryMood> listOfMood = GameManager.i.playerScript.GetListOfMoodHistory();
                if (listOfMood != null)
                {
                    //limit to last 'x' events
                    count = listOfMood.Count;
                    max = Mathf.Min(maxNumOfScrollItems, count);
                    limit = count - max;
                    for (int i = count - 1; i >= limit; i--)
                    {
                        HistoryMood historyMood = listOfMood[i];
                        if (historyMood != null)
                        {
                            if (historyMood.isHighlight == true)
                            {
                                listOfText.Add(GameManager.Formatt(string.Format("day {0}  {1} (Mood now {2} star{3}){4}", historyMood.turn, historyMood.descriptor, historyMood.mood,
                                historyMood.mood != 1 ? "s" : "", historyMood.factor.Length > 0 ? string.Format(", influenced by <b>{0}</b>", historyMood.factor) : ""), ColourType.neutralText));
                            }
                            else
                            {
                                listOfText.Add(string.Format("day {0}  {1} (Mood now {2} star{3}){4}", historyMood.turn, historyMood.descriptor, historyMood.mood,
                                historyMood.mood != 1 ? "s" : "", historyMood.factor.Length > 0 ? string.Format(", influenced by <b>{0}</b>", historyMood.factor) : ""));
                            }
                        }
                        else { Debug.LogWarningFormat("Invalid HistoryMood (Null) for listOfMood[{0}]", i); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid listOfMood (Null) for TabbedHistory \"{0}\", Player", history); }
            }
        }
        else
        {
            //Actor -> Events or Opinion
            if (history == TabbedHistory.Events)
            {
                List<HistoryActor> listOfEvents = arrayOfActorsTemp[currentSideTabIndex].GetListOfHistory();
                if (listOfEvents != null)
                {
                    //limit to last 'x' events
                    count = listOfEvents.Count;
                    max = Mathf.Min(maxNumOfScrollItems, count);
                    limit = count - max;
                    for (int i = count - 1; i >= limit; i--)
                    {
                        HistoryActor historyEvent = listOfEvents[i];
                        if (historyEvent != null)
                        {
                            //highlight any special entries, eg. start of level, in yellow
                            if (historyEvent.isHighlight == true)
                            {
                                listOfText.Add(GameManager.Formatt(string.Format("day {0}  {1}{2}", historyEvent.turn, historyEvent.text, historyEvent.district != null ? ", at " + historyEvent.district : ""),
                                  ColourType.neutralText));
                            }
                            else { listOfText.Add(string.Format("day {0}  {1}{2}", historyEvent.turn, historyEvent.text, historyEvent.district != null ? ", at " + historyEvent.district : "")); }
                        }
                        else { Debug.LogWarningFormat("Invalid HistoryActor (Null) for listOfEvents[{0}]", i); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid listOfEvents (Null) for TabbedHistory \"{0}\", Actor", history); }
            }
            else
            {
                List<HistoryOpinion> listOfOpinion = arrayOfActorsTemp[currentSideTabIndex].GetPersonality().GetListOfOpinion();
                if (listOfOpinion != null)
                {
                    //limit to last 'x' events
                    count = listOfOpinion.Count;
                    max = Mathf.Min(maxNumOfScrollItems, count);
                    limit = count - max;
                    for (int i = count - 1; i >= limit; i--)
                    {
                        HistoryOpinion historyOpinion = listOfOpinion[i];
                        if (historyOpinion != null)
                        {
                            listOfText.Add(string.Format("day {0}  {1} (Opinion now {2} star{3}){4}", historyOpinion.turn, historyOpinion.descriptor, historyOpinion.opinion,
                              historyOpinion.opinion != 1 ? "s" : "", historyOpinion.isNormal == true ? "" : ", IGNORED"));
                        }
                        else { Debug.LogWarningFormat("Invalid HistoryOpinion (Null) for listOfOpinion[{0}]", i); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid listOfOpinion (Null) for TabbedHistory \"{0}\", Player", history); }
            }
        }
        return listOfText;
    }


    /// <summary>
    /// Debug method to pump up history data in order to test scrolling system
    /// </summary>
    private List<HistoryActor> DebugGetExtraHistory()
    {
        //make sure it's a one off
        isAddDebugRecords = true;
        List<HistoryActor> tempList = new List<HistoryActor>();
        for (int i = 0; i < 40; i++)
        { tempList.Add(new HistoryActor() { text = $"Debug History item {i}" }); }
        return tempList;
    }

    /// <summary>
    /// Disable all scroll items, History Page 7
    /// </summary>
    private void DisableHistoryScrollItems()
    {
        for (int i = 0; i < maxNumOfScrollItems; i++)
        { arrayOfScrollObjects[i].SetActive(false); }
    }



    #endregion

    #endregion


    #region Initialisation SubMethods...
    //
    // - - - Initialisation SubMethods
    //

    /// <summary>
    /// Takes an arrayOfActors and initialises data for Subordinates actor set
    /// </summary>
    /// <param name="numOfSideTabs"></param>
    /// <param name="arrayOfActors"></param>
    private void InitialiseSubordinate(Actor[] arrayOfActors)
    {
        Actor actor;
        for (int index = 0; index < numOfSideTabs; index++)
        {
            actor = arrayOfActors[index];
            if (actor != null)
            {
                //populate array
                arrayOfActorsTemp[index] = actor;
                //sprite and arc
                arrayOfSideTabItems[index].portrait.sprite = actor.sprite;
                arrayOfSideTabItems[index].title.text = actor.arc.name;
                //baseline colours
                textSideColour = arrayOfSideTabItems[index].title.color;
                portraitColour = arrayOfSideTabItems[index].portrait.color;
                //first tab should be active on opening, rest passive
                if (index == inputData.slotID)
                {
                    backgroundColour = sideTabActiveColour;
                    portraitColour.a = 1.0f; backgroundColour.a = 1.0f;
                    textActorName.text = actor.actorName;
                    currentSideTabIndex = index;
                    textSideColour.a = 1.0f;
                }
                else
                {
                    backgroundColour = sideTabDormantColour;
                    portraitColour.a = sideTabAlpha; backgroundColour.a = sideTabAlpha;
                    textSideColour.a = 0.5f;
                }
                //set colors
                arrayOfSideTabItems[index].portrait.color = portraitColour;
                arrayOfSideTabItems[index].background.color = backgroundColour;
                arrayOfSideTabItems[index].title.color = textSideColour;
                //activate tab
                arrayOfSideTabObjects[index].SetActive(true);
            }
            else
            {
                //missing Actor -> shouldn't occur
                Debug.LogWarningFormat("Invalid actor (Null) for arrayOfActors[{0}]", index);
                arrayOfSideTabObjects[index].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Takes a listOfActorID's and initialises data for Reserves actor set
    /// </summary>
    /// <param name="numOfSideTabs"></param>
    /// <param name="listOfActors"></param>
    private void InitialiseReserves(List<int> listOfActors)
    {
        Actor actor;
        int limit = listOfActors.Count;
        if (limit != numOfSideTabs)
        {
            limit = Mathf.Min(numOfSideTabs, limit);
            Debug.LogWarningFormat("Mismatch between numOfSideTabs {0} and listOfActors.Count {1} (Should be identical)", numOfSideTabs, limit);
        }
        for (int index = 0; index < limit; index++)
        {
            actor = GameManager.i.dataScript.GetActor(listOfActors[index]);
            if (actor != null)
            {
                //populate array
                arrayOfActorsTemp[index] = actor;
                //sprite and arc
                arrayOfSideTabItems[index].portrait.sprite = actor.sprite;
                arrayOfSideTabItems[index].title.text = actor.arc.name;
                //baseline colours
                portraitColour = arrayOfSideTabItems[index].portrait.color;
                textSideColour = arrayOfSideTabItems[index].title.color;
                //first tab should be active on opening, rest passive
                if (index == inputData.slotID)
                {
                    backgroundColour = sideTabActiveColour;
                    portraitColour.a = 1.0f; backgroundColour.a = 1.0f;
                    textActorName.text = actor.actorName;
                    currentSideTabIndex = index;
                    textSideColour.a = 1.0f;
                }
                else
                {
                    backgroundColour = sideTabDormantColour;
                    portraitColour.a = sideTabAlpha; backgroundColour.a = sideTabAlpha;
                    textSideColour.a = 0.5f;
                }
                //set colors
                arrayOfSideTabItems[index].portrait.color = portraitColour;
                arrayOfSideTabItems[index].background.color = backgroundColour;
                arrayOfSideTabItems[index].title.color = textSideColour;
                //activate tab
                arrayOfSideTabObjects[index].SetActive(true);
            }
            else
            {
                //missing Actor -> shouldn't occur
                Debug.LogWarningFormat("Invalid actor (Null) for listOfActors[{0}], actorID \"{1}\"", index, listOfActors[index]);
                arrayOfSideTabObjects[index].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Overloaded -> Takes an arrayOfActors and initialises data for Reserves actor set (used for cached data)
    /// </summary>
    /// <param name="numOfSideTabs"></param>
    /// <param name="listOfActors"></param>
    private void InitialiseReserves(Actor[] arrayOfActors)
    {
        Actor actor;
        for (int index = 0; index < numOfSideTabs; index++)
        {
            actor = arrayOfActors[index];
            if (actor != null)
            {
                //populate array
                arrayOfActorsTemp[index] = actor;
                //sprite and arc
                arrayOfSideTabItems[index].portrait.sprite = actor.sprite;
                arrayOfSideTabItems[index].title.text = actor.arc.name;
                //baseline colours
                portraitColour = arrayOfSideTabItems[index].portrait.color;
                textSideColour = arrayOfSideTabItems[index].title.color;
                //first tab should be active on opening, rest passive
                if (index == inputData.slotID)
                {
                    backgroundColour = sideTabActiveColour;
                    portraitColour.a = 1.0f; backgroundColour.a = 1.0f;
                    textActorName.text = actor.actorName;
                    currentSideTabIndex = index;
                    textSideColour.a = 1.0f;
                }
                else
                {
                    backgroundColour = sideTabDormantColour;
                    portraitColour.a = sideTabAlpha; backgroundColour.a = sideTabAlpha;
                    textSideColour.a = 0.5f;
                }
                //set colors
                arrayOfSideTabItems[index].portrait.color = portraitColour;
                arrayOfSideTabItems[index].background.color = backgroundColour;
                arrayOfSideTabItems[index].title.color = textSideColour;
                //activate tab
                arrayOfSideTabObjects[index].SetActive(true);
            }
            else
            {
                //missing Actor -> shouldn't occur
                Debug.LogWarningFormat("Invalid actor (Null) for arrayOfActors[{0}]", index);
                arrayOfSideTabObjects[index].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Initialises Hq data for Hq actor set
    /// </summary>
    /// <param name="numOfSideTabs"></param>
    private void InitialiseHq(Actor[] arrayOfActors)
    {
        Actor actor;
        for (int index = 0; index < numOfSideTabs; index++)
        {
            actor = arrayOfActors[index];
            if (actor != null)
            {
                //populate array
                arrayOfActorsTemp[index] = actor;
                //sprite
                arrayOfSideTabItems[index].portrait.sprite = actor.sprite;
            }
            else
            {
                //default error sprite if a problem
                Debug.LogWarningFormat("Invalid actor (Null) for ActorHQ \"{0}\"", (ActorHQ)(index + offset));
                //deactivate tab
                arrayOfSideTabObjects[index].SetActive(false);
            }
            //title
            arrayOfSideTabItems[index].title.text = GameManager.i.hqScript.GetHqTitle(actor.statusHQ);
            //activate tab
            arrayOfSideTabObjects[index].SetActive(true);
            //baseline colours
            portraitColour = arrayOfSideTabItems[index].portrait.color;
            textSideColour = arrayOfSideTabItems[index].title.color;
            //first tab should be active on opening, rest passive
            if (index == inputData.slotID)
            {
                backgroundColour = sideTabActiveColour;
                portraitColour.a = 1.0f; backgroundColour.a = 1.0f;
                textActorName.text = actor.actorName;
                currentSideTabIndex = index;
                textSideColour.a = 1.0f;
            }
            else
            {
                backgroundColour = sideTabDormantColour;
                portraitColour.a = sideTabAlpha; backgroundColour.a = sideTabAlpha;
                textSideColour.a = 0.5f;
            }
            //set colors
            arrayOfSideTabItems[index].portrait.color = portraitColour;
            arrayOfSideTabItems[index].background.color = backgroundColour;
            arrayOfSideTabItems[index].title.color = textSideColour;
        }
    }



    #endregion


    //new methods above here
}
