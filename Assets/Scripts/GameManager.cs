﻿using delegateAPI;
using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public struct StartMethod
{
    public InitialisationDelegate handler;
    public string className;
}


/// <summary>
/// Main game controller
/// </summary>
/*[Serializable]*/
public class GameManager : MonoBehaviour
{

    #region Components
    public static GameManager i = null;                                 //static instance of GameManager which allows it to be accessed by any other script
    [HideInInspector] public StartManager startScript;                  //Start Manager
    [HideInInspector] public LevelManager levelScript;                  //Level Manager
    [HideInInspector] public PreLoadManager preloadScript;              //PreLoad Manager
    [HideInInspector] public LoadManager loadScript;                    //Load Manager
    [HideInInspector] public ValidationManager validateScript;          //Validation Manager
    [HideInInspector] public MetaManager metaScript;                    //Meta Manager
    [HideInInspector] public DataManager dataScript;                    //Data Manager
    [HideInInspector] public StatisticManager statScript;               //Statistics Manager
    [HideInInspector] public GUIManager guiScript;                      //GUI Manager
    [HideInInspector] public UIManager uiScript;                        //UI Manager
    [HideInInspector] public SpriteManager spriteScript;                //Sprite Manager
    [HideInInspector] public GlobalManager globalScript;                //Global Manager
    [HideInInspector] public TooltipManager tooltipScript;              //Tooltip Manager
    [HideInInspector] public CampaignManager campaignScript;            //Campaign Manager
    [HideInInspector] public ControlManager controlScript;              //Control Manager
    [HideInInspector] public FileManager fileScript;                    //File Manager
    [HideInInspector] public NewsManager newsScript;                    //News Manager
    [HideInInspector] public ActorManager actorScript;                  //Actor Manager 
    [HideInInspector] public PersonalityManager personScript;           //Personality Manager
    [HideInInspector] public ContactManager contactScript;              //Contact Manager
    [HideInInspector] public ActionManager actionScript;                //Action Manager
    [HideInInspector] public SideManager sideScript;                    //Side Manager
    [HideInInspector] public HelpManager helpScript;                    //Help Manager
    [HideInInspector] public TurnManager turnScript;                    //Turn Manager
    [HideInInspector] public InputManager inputScript;                  //Input Manager
    [HideInInspector] public EffectManager effectScript;                //Effect Manager
    [HideInInspector] public TargetManager targetScript;                //Target Manager
    [HideInInspector] public OptionManager optionScript;                //Option Manager
    [HideInInspector] public PlayerManager playerScript;                //Player Manager
    [HideInInspector] public TraitManager traitScript;                  //Trait Manager
    [HideInInspector] public TopicManager topicScript;                  //Topic Manager
    [HideInInspector] public HQManager hqScript;                        //HQ Manager
    [HideInInspector] public SecretManager secretScript;                //Secret Manager
    [HideInInspector] public OrganisationManager orgScript;             //Organisation Manager
    [HideInInspector] public CityManager cityScript;                    //City Manager
    [HideInInspector] public ObjectiveManager objectiveScript;          //Objectives Manager
    [HideInInspector] public NodeManager nodeScript;                    //Node Manager
    [HideInInspector] public DijkstraManager dijkstraScript;            //Dijkstra Manager
    [HideInInspector] public TeamManager teamScript;                    //Team Manager
    [HideInInspector] public GearManager gearScript;                    //Gear Manager
    [HideInInspector] public CaptureManager captureScript;              //Capture Manager
    [HideInInspector] public AIManager aiScript;                        //AI Manager
    [HideInInspector] public AIRebelManager aiRebelScript;              //AI Resistance Manager
    [HideInInspector] public NemesisManager nemesisScript;              //Nemesis Manager
    [HideInInspector] public ResistanceManager rebelScript;             //Resistance Manager
    [HideInInspector] public AuthorityManager authorityScript;          //Authority Manager
    [HideInInspector] public MessageManager messageScript;              //Message Manager
    [HideInInspector] public ItemDataManager itemDataScript;            //ItemData Manager
    [HideInInspector] public ConnectionManager connScript;              //Connection Manager
    [HideInInspector] public MissionManager missionScript;              //Mission Manager
    [HideInInspector] public ColourManager colourScript;                //Colour Manager
    [HideInInspector] public AnimationManager animateScript;            //Animation Manager
    [HideInInspector] public TestManager testScript;                    //Test Manager
    [HideInInspector] public TextManager textScript;                    //Text Manager
    [HideInInspector] public FeatureManager featureScript;              //Feature Manager
    [HideInInspector] public TutorialManager tutorialScript;            //Tutorial Manager
    [HideInInspector] public ScenarioManager scenarioScript;            //Scenario Manager
    //GUI
    [HideInInspector] public TooltipNode tooltipNodeScript;             //node tooltip static instance
    [HideInInspector] public TooltipConnection tooltipConnScript;       //connection tooltip static instance
    [HideInInspector] public TooltipActor tooltipActorScript;           //actor tooltip static instance
    [HideInInspector] public TooltipPlayer tooltipPlayerScript;         //player tooltip static instance
    [HideInInspector] public TooltipGeneric tooltipGenericScript;       //generic tooltip static instance
    [HideInInspector] public TooltipHelp tooltipHelpScript;             //help tooltip static instance
    [HideInInspector] public TooltipStory tooltipStoryScript;           //story tooltip static instance
    [HideInInspector] public ModalActionMenu actionMenuScript;          //Modal Action Menu (node)
    [HideInInspector] public ModalMainMenu mainMenuScript;              //Modal Main Menu
    [HideInInspector] public ModalOutcome outcomeScript;                //Modal Outcome window
    [HideInInspector] public ModalConfirm confirmScript;                //Modal Confirm window
    [HideInInspector] public ModalTeamPicker teamPickerScript;          //Modal Team Picker window
    [HideInInspector] public ModalGenericPicker genericPickerScript;    //Modal Generic Picker window
    [HideInInspector] public ModalInventoryUI inventoryScript;          //Modal InventoryUI window
    [HideInInspector] public ModalReviewUI reviewScript;                //Modal ReviewUI window
    [HideInInspector] public ModalHelpUI masterHelpScript;              //Modal Help UI
    /*[HideInInspector] public ModalDiceUI diceScript;                  //Modal Dice UI window*/
    [HideInInspector] public ModalGUI modalGUIScript;                   //Modal GUI 
    [HideInInspector] public AlertUI alertScript;                       //Alert UI text display
    [HideInInspector] public WidgetTopUI widgetTopScript;               //Widget Top UI
    [HideInInspector] public TopBarUI topBarScript;                         //Top Bar UI
    [HideInInspector] public CityInfoUI cityInfoScript;                 //City Info UI
    [HideInInspector] public MainInfoUI mainInfoScript;                 //Main Info UI
    [HideInInspector] public TransitionUI transitionScript;             //Transition UI
    [HideInInspector] public MetaGameUI metaUIScript;                   //Meta Game UI
    [HideInInspector] public AIDisplayUI aiDisplayScript;               //AI Display UI
    [HideInInspector] public AISideTabUI aiSideTabScript;               //AI SideTab UI
    [HideInInspector] public FinderUI finderScript;                     //Finder UI
    [HideInInspector] public FinderSideTabUI finderSideTabScript;       //FinderSideTabUI
    [HideInInspector] public ActorPanelUI actorPanelScript;             //Actor Panel UI
    [HideInInspector] public BasePanelUI basePanelScript;               //Base Panel UI
    [HideInInspector] public DebugGraphics debugGraphicsScript;         //Debug only Graphics
    [HideInInspector] public DebugGUI debugScript;                      //Debug GUI
    [HideInInspector] public TopicUI topicDisplayScript;                //Topic UI
    [HideInInspector] public PopUpDynamic popUpDynamicScript;           //PopUpDynamic
    [HideInInspector] public PopUpFixed popUpFixedScript;               //PopUpFixed
    [HideInInspector] public BillboardUI billboardScript;               //Billboard UI
    [HideInInspector] public AdvertUI advertScript;                     //Advert UI
    [HideInInspector] public ModalTabbedUI tabbedUIScript;              //Tabbed UI
    [HideInInspector] public TutorialUI tutorialUIScript;               //Tutorial UI 
    [HideInInspector] public GameHelpUI gameHelpScript;                 //GameHelp UI
    [HideInInspector] public NewTurnUI newTurnScript;                   //NewTurn UI
    #endregion


    #region Variables
    [Tooltip("Leave as default 0 for random. Can be a whole number between -2147483648 and 2147483647")]
    public int seedDev = 0;                                           //random seed for development
    [Tooltip("Autoruns game for 'x' number of turns with current player & Both sides as AI. Leave at Zero for normal operation")]
    public int autoRunTurns = 0;
    [Tooltip("Scenario level to start a new game on (default 0")]
    [Range(0, 4)] public int scenarioStartLevel = 0;
    [Tooltip("Tutorial level (set) to start with (default -1, will use previously saved set for that tutorial)")]
    [Range(-1, 10)] public int tutorialStartLevel = -1;

    [Header("Debug Options")]
    [Tooltip("Switch ON to get a performance log of initialisation at Session Start")]
    public bool isPerformanceLog;
    [Tooltip("Runs ValidationManager.cs to check data at Session Start")]
    public bool isDataValidation;
    [Tooltip("Runs SO Validator at Session Start to cross reference SO's in assets vs. those in LoadManager.cs arrays. Editor only. Slow")]
    public bool isValidateSO;
    [Tooltip("Runs Data Integrity checks at completion of an autorun, at the start of a followOn level or once a save game has been loaded")]
    public bool isIntegrityCheck;

    [Header("Randomisation")]
    [Tooltip("If true then a random seed is used to generate cities rather than the specified scenario seed")]
    public bool isRandomLevel;
    [Tooltip("If true a Random city is used, not a campaign city")]
    public bool isRandomCity;
    [Tooltip("If true random actors are procedurally generated, if false, ActorPool in Campaign.SO is used")]
    public bool isRandomActors;
    [Tooltip("If true OnMap actors will be randomly selected from ActorPool in Campaign.SO (level 1). Actors in pool who are 'OnMap' will be placed into the pool prior to selection")]
    public bool isRandomOnMap;

    [Header("Save/Load")]
    [Tooltip("If true then save files are encrypted")]
    public bool isEncrypted;
    [Tooltip("If true then autoSaves made at start of every turn")]
    public bool isAutoSave;
    [Tooltip("If true then autoSave file is loaded instead of normal save file")]
    public bool isLoadAutoSave;


    private Random.State devState;                                                  //used to restore seedDev random sequence after any interlude, eg. level generation with a unique seed
    private long totalTime;                                                         //used for Performance monitoring on start up
    private float mouseWheelInput;                                                  //used for detecting mouse wheel input in the Update method
    [HideInInspector] public bool isSession;                                        //true once InitialiseNewLevel has been run at least once (for Load game functionality to detect if loading prior to any initialisation)
                                                                                    //also used by InitialiseTutorial to detect if overall initialisation has occurred already, or not

    private List<StartMethod> listOfGlobalMethods = new List<StartMethod>();        //start game global methods
    private List<StartMethod> listOfGameMethods = new List<StartMethod>();          //game managerment methods
    private List<StartMethod> listOfLevelMethods = new List<StartMethod>();         //level related methods
    private List<StartMethod> listOfTutorialMethods = new List<StartMethod>();      //tutorial related methods
    private List<StartMethod> listOfUIMethods = new List<StartMethod>();            //UI related methods
    private List<StartMethod> listOfConditionalMethods = new List<StartMethod>();   //methods (UI) that conditionally apply, eg. if Resistance Player. Used for InitialiseLoadGame
    private List<StartMethod> listOfDebugMethods = new List<StartMethod>();         //Debug related methods
    private List<StartMethod> listOfLoadMethods = new List<StartMethod>();          //Load a saved game methods (used to regenerate the loaded level). For AI (both sides) Player



    #endregion


    #region Awake method
    /// <summary>
    /// Constructor
    /// </summary>
    private void Awake()
    {
        //check if instance already exists
        if (i == null)
        { i = this; }
        //if instance already exists and it's not this
        else if (i != this)
        {
            //Then destroy this in order to reinforce the singleton pattern (can only ever be one instance of GameManager)
            Destroy(gameObject);
        }
        //random seed to facilitate replication of results
        if (seedDev == 0)
        { seedDev = GetRandomSeed(); }
        Debug.Log("Seed: " + seedDev);
        Random.InitState(seedDev);
        //debug -> write seed to file
        DateTime date1 = DateTime.Now;
        string seedInfo = Environment.NewLine + string.Format("Dev seed {0} -> {1}", seedDev, date1.ToString("f", CultureInfo.CreateSpecificCulture("en-AU"))) + Environment.NewLine;
        File.AppendAllText("Seed.txt", seedInfo);
        //Get component references
        startScript = GetComponent<StartManager>();
        levelScript = GetComponent<LevelManager>();
        preloadScript = GetComponent<PreLoadManager>();
        loadScript = GetComponent<LoadManager>();
        validateScript = GetComponent<ValidationManager>();
        statScript = GetComponent<StatisticManager>();
        metaScript = GetComponent<MetaManager>();
        dataScript = GetComponent<DataManager>();
        guiScript = GetComponent<GUIManager>();
        uiScript = GetComponent<UIManager>();
        spriteScript = GetComponent<SpriteManager>();
        globalScript = GetComponent<GlobalManager>();
        campaignScript = GetComponent<CampaignManager>();
        controlScript = GetComponent<ControlManager>();
        fileScript = GetComponent<FileManager>();
        actorScript = GetComponent<ActorManager>();
        personScript = GetComponent<PersonalityManager>();
        contactScript = GetComponent<ContactManager>();
        actionScript = GetComponent<ActionManager>();
        playerScript = GetComponent<PlayerManager>();
        traitScript = GetComponent<TraitManager>();
        topicScript = GetComponent<TopicManager>();
        hqScript = GetComponent<HQManager>();
        secretScript = GetComponent<SecretManager>();
        orgScript = GetComponent<OrganisationManager>();
        cityScript = GetComponent<CityManager>();
        objectiveScript = GetComponent<ObjectiveManager>();
        effectScript = GetComponent<EffectManager>();
        targetScript = GetComponent<TargetManager>();
        optionScript = GetComponent<OptionManager>();
        nodeScript = GetComponent<NodeManager>();
        dijkstraScript = GetComponent<DijkstraManager>();
        teamScript = GetComponent<TeamManager>();
        gearScript = GetComponent<GearManager>();
        messageScript = GetComponent<MessageManager>();
        itemDataScript = GetComponent<ItemDataManager>();
        connScript = GetComponent<ConnectionManager>();
        missionScript = GetComponent<MissionManager>();
        colourScript = GetComponent<ColourManager>();
        testScript = GetComponent<TestManager>();
        textScript = GetComponent<TextManager>();
        tooltipScript = GetComponent<TooltipManager>();
        newsScript = GetComponent<NewsManager>();
        sideScript = GetComponent<SideManager>();
        helpScript = GetComponent<HelpManager>();
        turnScript = GetComponent<TurnManager>();
        inputScript = GetComponent<InputManager>();
        captureScript = GetComponent<CaptureManager>();
        aiScript = GetComponent<AIManager>();
        aiRebelScript = GetComponent<AIRebelManager>();
        nemesisScript = GetComponent<NemesisManager>();
        rebelScript = GetComponent<ResistanceManager>();
        authorityScript = GetComponent<AuthorityManager>();
        debugScript = GetComponent<DebugGUI>();
        animateScript = GetComponent<AnimationManager>();
        featureScript = GetComponent<FeatureManager>();
        tutorialScript = GetComponent<TutorialManager>();
        scenarioScript = GetComponent<ScenarioManager>();
        //Get UI static references -> from PanelManager
        tooltipNodeScript = TooltipNode.Instance();
        tooltipConnScript = TooltipConnection.Instance();
        tooltipActorScript = TooltipActor.Instance();
        tooltipPlayerScript = TooltipPlayer.Instance();
        tooltipGenericScript = TooltipGeneric.Instance();
        tooltipHelpScript = TooltipHelp.Instance();
        tooltipStoryScript = TooltipStory.Instance();
        actionMenuScript = ModalActionMenu.Instance();
        mainMenuScript = ModalMainMenu.Instance();
        outcomeScript = ModalOutcome.Instance();
        confirmScript = ModalConfirm.Instance();
        teamPickerScript = ModalTeamPicker.Instance();
        genericPickerScript = ModalGenericPicker.Instance();
        inventoryScript = ModalInventoryUI.Instance();
        reviewScript = ModalReviewUI.Instance();
        masterHelpScript = ModalHelpUI.Instance();
        /*diceScript = ModalDiceUI.Instance();*/
        modalGUIScript = ModalGUI.Instance();
        widgetTopScript = WidgetTopUI.Instance();
        topBarScript = TopBarUI.Instance();
        cityInfoScript = CityInfoUI.Instance();
        mainInfoScript = MainInfoUI.Instance();
        transitionScript = TransitionUI.Instance();
        metaUIScript = MetaGameUI.Instance();
        aiDisplayScript = AIDisplayUI.Instance();
        aiSideTabScript = AISideTabUI.Instance();
        finderScript = FinderUI.Instance();
        finderSideTabScript = FinderSideTabUI.Instance();
        alertScript = AlertUI.Instance();
        actorPanelScript = ActorPanelUI.Instance();
        basePanelScript = BasePanelUI.Instance();
        debugGraphicsScript = DebugGraphics.Instance();
        topicDisplayScript = TopicUI.Instance();
        popUpDynamicScript = PopUpDynamic.Instance();
        popUpFixedScript = PopUpFixed.Instance();
        billboardScript = BillboardUI.Instance();
        advertScript = AdvertUI.Instance();
        tabbedUIScript = ModalTabbedUI.Instance();
        tutorialUIScript = TutorialUI.Instance();
        gameHelpScript = GameHelpUI.Instance();
        newTurnScript = NewTurnUI.Instance();
        //Error Checking
        Debug.Assert(startScript != null, "Invalid startScript (Null)");
        Debug.Assert(levelScript != null, "Invalid levelScript (Null)");
        Debug.Assert(preloadScript != null, "Invalid preloadScript (Null)");
        Debug.Assert(loadScript != null, "Invalid loadScript (Null)");
        Debug.Assert(statScript != null, "Invalid statScript (Null)");
        Debug.Assert(metaScript != null, "Invalid metaScript (Null)");
        Debug.Assert(dataScript != null, "Invalid dataScript (Null)");
        Debug.Assert(guiScript != null, "Invalid guiScript (Null)");
        Debug.Assert(uiScript != null, "Invalid uiScript (Null)");
        Debug.Assert(spriteScript != null, "Invalid spriteScript (Null)");
        Debug.Assert(globalScript != null, "Invalid globalScript (Null)");
        Debug.Assert(campaignScript != null, "Invalid campaignScript (Null)");
        Debug.Assert(controlScript != null, "Invalid controlScript (Null)");
        Debug.Assert(fileScript != null, "Invalid fileScript (Null)");
        Debug.Assert(actorScript != null, "Invalid actorScript (Null)");
        Debug.Assert(personScript != null, "Invalid personScript (Null)");
        Debug.Assert(contactScript != null, "Invalid contactScript (Null)");
        Debug.Assert(actionScript != null, "Invalid actionScript (Null)");
        Debug.Assert(playerScript != null, "Invalid playerScript (Null)");
        Debug.Assert(traitScript != null, "Invalid traitScript (Null)");
        Debug.Assert(topicScript != null, "Invalid topicScript (Null)");
        Debug.Assert(hqScript != null, "Invalid hqScript (Null)");
        Debug.Assert(secretScript != null, "Invalid secretScript (Null)");
        Debug.Assert(orgScript != null, "Invalid orgScript (Null)");
        Debug.Assert(cityScript != null, "Invalid cityScript (Null)");
        Debug.Assert(objectiveScript != null, "Invalid objectiveScript (Null)");
        Debug.Assert(effectScript != null, "Invalid effectScript (Null)");
        Debug.Assert(targetScript != null, "Invalid targetScript (Null)");
        Debug.Assert(optionScript != null, "Invalid optionScript (Null)");
        Debug.Assert(nodeScript != null, "Invalid nodeScript (Null)");
        Debug.Assert(dijkstraScript != null, "Invalid dijkstraScript (Null)");
        Debug.Assert(teamScript != null, "Invalid teamScript (Null)");
        Debug.Assert(gearScript != null, "Invalid gearScript (Null)");
        Debug.Assert(messageScript != null, "Invalid messageScript (Null)");
        Debug.Assert(itemDataScript != null, "Invalid itemDataScript (Null)");
        Debug.Assert(connScript != null, "Invalid connScript (Null)");
        Debug.Assert(missionScript != null, "Invalid missionScript (Null)");
        Debug.Assert(colourScript != null, "Invalid colourScript (Null)");
        Debug.Assert(testScript != null, "Invalid testScript (Null)");
        Debug.Assert(textScript != null, "Invalid textScript (Null)");
        Debug.Assert(validateScript != null, "Invalid validateScript (Null)");
        Debug.Assert(tooltipScript != null, "Invalid tooltipScript (Null)");
        Debug.Assert(newsScript != null, "Invalid newsScript (Null)");
        Debug.Assert(sideScript != null, "Invalid sideScript (Null)");
        Debug.Assert(helpScript != null, "Invalid helpScript (Null)");
        Debug.Assert(turnScript != null, "Invalid turnScript (Null)");
        Debug.Assert(inputScript != null, "Invalid inputScript (Null)");
        Debug.Assert(captureScript != null, "Invalid captureScript (Null)");
        Debug.Assert(aiScript != null, "Invalid aiScript (Null)");
        Debug.Assert(nemesisScript != null, "Invalid nemesisScript (Null)");
        Debug.Assert(rebelScript != null, "Invalid rebelScript (Null)");
        Debug.Assert(authorityScript != null, "Invalid authorityScript (Null)");
        Debug.Assert(debugScript != null, "Invalid debugScript (Null)");
        Debug.Assert(animateScript != null, "Invalid animateScript (Null)");
        Debug.Assert(featureScript != null, "Invalid featureScript (Null)");
        Debug.Assert(tutorialScript != null, "Invalid tutorialScript (Null)");
        Debug.Assert(scenarioScript != null, "Invalid scenarioScript (Null)");
        //singletons
        Debug.Assert(tooltipNodeScript != null, "Invalid tooltipNodeScript (Null)");
        Debug.Assert(tooltipConnScript != null, "Invalid tooltipConnScript (Null)");
        Debug.Assert(tooltipActorScript != null, "Invalid tooltipActorScript (Null)");
        Debug.Assert(tooltipPlayerScript != null, "Invalid tooltipPlayerScript (Null)");
        Debug.Assert(tooltipGenericScript != null, "Invalid tooltipGenericScript (Null)");
        Debug.Assert(tooltipStoryScript != null, "Invalid tooltipStoryScript (Null)");
        Debug.Assert(actionMenuScript != null, "Invalid actionMenuScript (Null)");
        Debug.Assert(mainMenuScript != null, "Invalid mainMenuScript (Null)");
        Debug.Assert(outcomeScript != null, "Invalid outcomeScript (Null)");
        Debug.Assert(teamPickerScript != null, "Invalid teamPickerScript (Null)");
        Debug.Assert(genericPickerScript != null, "Invalid genericPickerScript (Null)");
        Debug.Assert(inventoryScript != null, "Invalid inventoryScript (Null)");
        Debug.Assert(modalGUIScript != null, "Invalid modalGUIScript (Null)");
        Debug.Assert(widgetTopScript != null, "Invalid widgetTopScript (Null)");
        Debug.Assert(topBarScript != null, "Invalid topBarScript (Null)");
        Debug.Assert(cityInfoScript != null, "Invalid cityInfoScript (Null)");
        Debug.Assert(mainInfoScript != null, "Invalid mainInfoScript (Null)");
        Debug.Assert(transitionScript != null, "Invalid transitionScript (Null)");
        Debug.Assert(metaUIScript != null, "Invalid metaGameScript (Null)");
        Debug.Assert(aiDisplayScript != null, "Invalid aiDisplayScript (Null)");
        Debug.Assert(aiSideTabScript != null, "Invalid aiSideTabScript (Null)");
        Debug.Assert(finderScript != null, "Invalid finderScript (Null)");
        Debug.Assert(finderSideTabScript != null, "Invalid finderSideTabScript (Null)");
        Debug.Assert(alertScript != null, "Invalid alertScript (Null)");
        Debug.Assert(actorPanelScript != null, "Invalid actorPanelScript (Null)");
        Debug.Assert(basePanelScript != null, "Invalid basePanelScript (Null)");
        Debug.Assert(debugGraphicsScript != null, "Invalid debugGraphicsScript (Null)");
        Debug.Assert(topicDisplayScript != null, "Invalid topicDisplayScript (Null)");
        Debug.Assert(popUpDynamicScript != null, "Invalid popUpDynamicScript (Null)");
        Debug.Assert(popUpFixedScript != null, "Invalid popUpFixedScript (Null)");
        Debug.Assert(billboardScript != null, "Invalid billboardScript (Null)");
        Debug.Assert(advertScript != null, "Invalid advertScript (Null)");
        Debug.Assert(tabbedUIScript != null, "Invalid tabbedUIScript (Null)");
        Debug.Assert(tutorialUIScript != null, "Invalid tutorialUIScript (Null)");
        Debug.Assert(gameHelpScript != null, "Invalid gameHelpScript (Null)");
        Debug.Assert(newTurnScript != null, "Invalid newTurnScript (Null)");
        Debug.Assert(masterHelpScript != null, "Invalid masterHelpScript (Null)");
        //set up list of delegates
        InitialiseStartSequence();
        //sets this to not be destroyed when reloading a scene
        DontDestroyOnLoad(gameObject);
    }
    #endregion


    #region Start method
    private void Start()
    {
        //Debug
        /*StringBuilder builder = new StringBuilder();
        for (int i = 0; i < 4510; i++)
        { builder.Append(Random.Range(0, 2)); }
        File.AppendAllText("Digital.txt", builder.ToString());*/

        inputScript.GameState = GameState.StartUp;
        featureScript.InitialiseFeatures();
        //global methods
        if (isPerformanceLog == false)
        {
            InitialiseMethods(listOfGlobalMethods);
            InitialiseMethods(listOfGameMethods);
        }
        else
        {
            //need testManager in order to access performance timer
            testScript.Initialise();
            //start-up with Performance Monitoring
            InitialiseWithPerformanceMonitoring(listOfGlobalMethods);
            InitialiseWithPerformanceMonitoring(listOfGameMethods);
        }
        //AutoRun or Campaign
        if (autoRunTurns > 0)
        {
            inputScript.GameState = GameState.NewInitialisation;
            //debug -> set to campaign start
            campaignScript.Reset();
            InitialiseNewSession();
            //campaign data
            i.dataScript.SetCampaignHistoryStart();
            //commence autorun
            turnScript.SetAutoRun(autoRunTurns);
        }
        else
        {
            InitialiseMainMenu();
        }
    }
    #endregion


    #region Update method
    /// <summary>
    /// Only update in the entire code base -> handles redraws and input
    /// </summary>
    private void Update()
    {
        mouseWheelInput = 0;
        if (inputScript.GameState != GameState.ExitGame)
        {
            //redraw any Nodes where required
            if (nodeScript.NodeRedraw == true)
            { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.Redraw, "GameManager.cs -> Update"); }

            //get any mouse wheel input (restricts max value) and pass as a parameter as Input.anyKeyDown won't pick up mouse wheel input)
            mouseWheelInput += Input.GetAxis("Mouse ScrollWheel");

            //Handle Game Input
            if (mouseWheelInput != 0)
            { inputScript.ProcessMouseWheelInput(mouseWheelInput); }
            else if (Input.anyKeyDown == true)
            { inputScript.ProcessKeyInput(); }
        }
    }
    #endregion


    #region StartUp Sequence...
    /// <summary>
    /// set up list of delegates ready for initialisation (do this because there are two version of initialisation, performance monitoring ON or OFF)
    /// </summary>
    private void InitialiseStartSequence()
    {
        StartMethod startMethod = new StartMethod();

        #region GlobalMethods
        //
        // - - - Global Methods
        //
        //PreLoad Manager -> Initialise
        startMethod.handler = preloadScript.Initialise;
        startMethod.className = "PreLoadManager";
        listOfGlobalMethods.Add(startMethod);
        //Load Manager -> InitialiseStart
        startMethod.handler = loadScript.InitialiseStart;
        startMethod.className = "LoadManager";
        listOfGlobalMethods.Add(startMethod);
#if (UNITY_EDITOR)
        //SO Checker (After LoadManager.cs / Optional)
        if (isValidateSO == true)
        {
            startMethod.handler = validateScript.ValidateSO;
            startMethod.className = "ValidationManager (SO)";
            listOfGlobalMethods.Add(startMethod);
        }
#endif
        //Global Manager -> immediately after dataScript.InitialiseStart and before dataScript.InitialiseEarly 
        startMethod.handler = globalScript.Initialise;
        startMethod.className = "GlobalManager";
        listOfGlobalMethods.Add(startMethod);
        //Colour Manager
        startMethod.handler = colourScript.Initialise;
        startMethod.className = "ColourManager";
        listOfGlobalMethods.Add(startMethod);
        //Message Manager -> after globalScript and before a lot of other stuff (pre-start messages need to be initialised for side)
        startMethod.handler = messageScript.InitialiseEarly;
        startMethod.className = "MessageManager";
        listOfGlobalMethods.Add(startMethod);
        //ItemData Manager
        startMethod.handler = itemDataScript.Initialise;
        startMethod.className = "ItemDataManager";
        listOfGlobalMethods.Add(startMethod);
        //ModalGUI
        startMethod.handler = modalGUIScript.Initialise;
        startMethod.className = "ModalGUI";
        listOfGlobalMethods.Add(startMethod);
        //ModalMainMenu Manager -> after Colour Manager
        startMethod.handler = mainMenuScript.Initialise;
        startMethod.className = "MainMenuManager";
        listOfGlobalMethods.Add(startMethod);
        //Actor Manager -> PreInitialise
        startMethod.handler = actorScript.PreInitialiseActors;
        startMethod.className = "ActorManager";
        listOfGlobalMethods.Add(startMethod);
        //Load Manager -> InitialiseEarly
        startMethod.handler = loadScript.InitialiseEarly;
        startMethod.className = "LoadManager";
        listOfGlobalMethods.Add(startMethod);
        //GUI Manager -> before any actor scripts (actorScript.PreInitialiseActors is O.K to be earlier)
        startMethod.handler = guiScript.Initialise;
        startMethod.className = "GUIManager";
        listOfGlobalMethods.Add(startMethod);
        //Help Manager -> before LoadManager.cs InitialiseLate
        startMethod.handler = helpScript.Initialise;
        startMethod.className = "HelpManager";
        listOfGlobalMethods.Add(startMethod);
        //Input Manager
        startMethod.handler = inputScript.Initialise;
        startMethod.className = "InputManager";
        listOfGlobalMethods.Add(startMethod);
        //Control Manager
        startMethod.handler = controlScript.Initialise;
        startMethod.className = "ControlManager";
        listOfGlobalMethods.Add(startMethod);
        //File Manager
        startMethod.handler = fileScript.Initialise;
        startMethod.className = "FileManager";
        listOfGlobalMethods.Add(startMethod);
        //Text Manager
        startMethod.handler = textScript.Initialise;
        startMethod.className = "TextManager";
        listOfGlobalMethods.Add(startMethod);
        //Statistic Manager -> InitialiseEarly
        startMethod.handler = statScript.InitialiseEarly;
        startMethod.className = "StatisticManager Early";
        listOfGlobalMethods.Add(startMethod);
        //Data Manager -> InitialiseEarly
        startMethod.handler = dataScript.InitialiseEarly;
        startMethod.className = "DataManager Early";
        listOfGlobalMethods.Add(startMethod);
        //News Manager
        startMethod.handler = newsScript.Initialise;
        startMethod.className = "NewsManager";
        listOfGlobalMethods.Add(startMethod);
        #endregion

        #region Game Methods
        //
        // - - - Game methods - - -
        //
        //Meta Manager
        startMethod.handler = metaScript.Initialise;
        startMethod.className = "MetaManager";
        listOfGameMethods.Add(startMethod);
        //Campaign Manager
        startMethod.handler = campaignScript.InitialiseGame;
        startMethod.className = "CampaignManager Game";
        listOfGameMethods.Add(startMethod);
        #endregion

        #region Tutorial Methods
        //
        // - - - Tutorial methods - - -
        //
        //Tutorial Manager
        startMethod.handler = tutorialScript.Initialise;
        startMethod.className = "TutorialManager";
        listOfTutorialMethods.Add(startMethod);
        #endregion

        #region Level Methods
        //
        // - - - Level methods - - -
        //
        //Reset Data prior to new level
        startMethod.handler = ResetNewLevelData;
        startMethod.className = "Reset Level Data";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        //Campaign Manager InitialiseEarly -> before level & Side Managers
        startMethod.handler = campaignScript.InitialiseEarly;
        startMethod.className = "CampaignManager Early";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        //Side Manager -> after campaignManager
        startMethod.handler = sideScript.Initialise;
        startMethod.className = "SideManager";
        listOfLevelMethods.Add(startMethod);
        //Widget Top UI -> after SideManager.cs (sequence issues) 
        startMethod.handler = widgetTopScript.Initialise;
        startMethod.className = "WidgetTopUI";
        listOfLevelMethods.Add(startMethod);
        //Objective Manager
        startMethod.handler = objectiveScript.Initialise;
        startMethod.className = "ObjectiveManager";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        //Actor Panel UI -> before actorScript.Initialise
        startMethod.handler = actorPanelScript.Initialise;
        startMethod.className = "ActorPanelUI";
        listOfLevelMethods.Add(startMethod);
        //Base Panel UI -> before CityInfo.InitialiseLate
        startMethod.handler = basePanelScript.Initialise;
        startMethod.className = "BasePanelUI";
        listOfLevelMethods.Add(startMethod);
        //Actor Manager -> before DataManager.cs & NodeManager.cs
        startMethod.handler = actorScript.Initialise;
        startMethod.className = "ActorManager";
        listOfLevelMethods.Add(startMethod);
        //Level Manager
        startMethod.handler = levelScript.Initialise;
        startMethod.className = "LevelManager";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        //Load Manager -> InitialiseLate -> immediately after levelScript.Initialise
        startMethod.handler = loadScript.InitialiseLate;
        startMethod.className = "LoadManager";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        //Data Manager -> InitialiseLate -> immediately after LoadScript.Initialise
        startMethod.handler = dataScript.InitialiseLate;
        startMethod.className = "DataManager";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        //Statistic Manager -> InitialiseLate
        startMethod.handler = statScript.InitialiseLate;
        startMethod.className = "StatisticManager";
        listOfLevelMethods.Add(startMethod);
        //Dijkstra Manager -> Initialise -> after dataScript & LevelScript
        startMethod.handler = dijkstraScript.Initialise;
        startMethod.className = "DijkstraManager";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        //Organisation Manager
        startMethod.handler = orgScript.Initialise;
        startMethod.className = "OrganisationManager";
        listOfLevelMethods.Add(startMethod);
        //AI Manager -> after factionScript, before CampaignManager -> InitialiseLate
        startMethod.handler = aiScript.Initialise;
        startMethod.className = "AIManager";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);

        //Campaign Manager -> InitialiseLate (includes NemesisManager.Initialise & CityManager.InitialiseLate) -> after levelScript.Initialise
        startMethod.handler = campaignScript.InitialiseLate;
        startMethod.className = "CampaignManager Late";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        //Message Manager -> InitialseLate -> after CampaignManager
        startMethod.handler = messageScript.InitialiseLate;
        startMethod.className = "MessageManager";
        listOfLevelMethods.Add(startMethod);
        //Secret Manager -> after dataScript and before playerScript
        startMethod.handler = secretScript.Initialise;
        startMethod.className = "SecretManager";
        listOfLevelMethods.Add(startMethod);
        //Contact Manager -> before ActorManager.cs InitialiseLate
        startMethod.handler = contactScript.Initialise;
        startMethod.className = "ContactManager";
        listOfLevelMethods.Add(startMethod);
        //Actor Manager -> after DataManager.cs & before NodeManager.cs
        startMethod.handler = actorScript.InitialiseLate;
        startMethod.className = "ActorManager";
        listOfLevelMethods.Add(startMethod);
        //Action Manager
        startMethod.handler = actionScript.Initialise;
        startMethod.className = "ActionManager";
        listOfLevelMethods.Add(startMethod);
        //Node Manager
        startMethod.handler = nodeScript.Initialise;
        startMethod.className = "NodeManager";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        //Effect Manager -> after nodeScript
        startMethod.handler = effectScript.Initialise;
        startMethod.className = "EffectManager";
        listOfLevelMethods.Add(startMethod);
        //Team Manager
        startMethod.handler = teamScript.Initialise;
        startMethod.className = "TeamManager";
        listOfLevelMethods.Add(startMethod);
        //Turn Manager
        startMethod.handler = turnScript.Initialise;
        startMethod.className = "TurnManager";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        //Gear Manager
        startMethod.handler = gearScript.Initialise;
        startMethod.className = "GearManager";
        listOfLevelMethods.Add(startMethod);
        //Capture Manager
        startMethod.handler = captureScript.Initialise;
        startMethod.className = "CaptureManager";
        listOfLevelMethods.Add(startMethod);
        //Authority Manager
        startMethod.handler = authorityScript.Initialise;
        startMethod.className = "AuthorityManager";
        listOfLevelMethods.Add(startMethod);

        //Player Manager
        startMethod.handler = playerScript.Initialise;
        startMethod.className = "PlayerManager";
        listOfLevelMethods.Add(startMethod);
        //Target Manager -> InitialseLate -> after CampaignManager and PlayerManager (specific for tutorials)
        startMethod.handler = targetScript.InitialiseLate;
        startMethod.className = "TargetManager";
        listOfLevelMethods.Add(startMethod);
        //Personality Manager -> after Actor/PlayerManager.cs
        startMethod.handler = personScript.Initialise;
        startMethod.className = "PersonalityManager";
        listOfLevelMethods.Add(startMethod);
        //HQ Manager -> after PersonalityManager.cs
        startMethod.handler = hqScript.Initialise;
        startMethod.className = "HqManager";
        listOfLevelMethods.Add(startMethod);
        //Player Manager -> Late (after PersonalityManager.cs)
        startMethod.handler = playerScript.InitialiseLate;
        startMethod.className = "PlayerManager Late";
        listOfLevelMethods.Add(startMethod);
        //Rebel AI Manager -> after Player Manager
        startMethod.handler = aiRebelScript.Initialise;
        startMethod.className = "AIRebelManager";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        //Trait Manager
        startMethod.handler = traitScript.Initialise;
        startMethod.className = "TraitManager";
        listOfLevelMethods.Add(startMethod);
        //Connection Manager
        startMethod.handler = connScript.Initialise;
        startMethod.className = "ConnectionManager";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        //Topic Manager
        startMethod.handler = topicScript.Initialise;
        startMethod.className = "TopicManager";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        //Animation Manager
        startMethod.handler = animateScript.Initialise;
        startMethod.className = "AnimationManager";
        listOfLevelMethods.Add(startMethod);
        listOfLoadMethods.Add(startMethod);
        #endregion

        #region GUI Methods
        //
        // - - - GUI methods - - -
        //
        //Team Picker
        startMethod.handler = teamPickerScript.Initialise;
        startMethod.className = "TeamPickerUI";
        listOfUIMethods.Add(startMethod);
        listOfConditionalMethods.Add(startMethod);
        //City Info UI
        startMethod.handler = cityInfoScript.Initialise;
        startMethod.className = "CityInfoUI";
        listOfUIMethods.Add(startMethod);
        //Main Info UI
        startMethod.handler = mainInfoScript.Initialise;
        startMethod.className = "MainInfoUI";
        listOfUIMethods.Add(startMethod);
        //Transition UI
        startMethod.handler = transitionScript.Initialise;
        startMethod.className = "transitionUI";
        listOfUIMethods.Add(startMethod);
        //Meta Game UI
        startMethod.handler = metaUIScript.Initialise;
        startMethod.className = "MetaGameUI";
        listOfUIMethods.Add(startMethod);
        //Topic UI
        startMethod.handler = topicDisplayScript.Initialise;
        startMethod.className = "TopicUI";
        listOfUIMethods.Add(startMethod);
        //AI Display UI 
        startMethod.handler = aiDisplayScript.Initialise;
        startMethod.className = "AIDisplayUI";
        listOfUIMethods.Add(startMethod);
        listOfConditionalMethods.Add(startMethod);
        //AI SideTab UI -> after AI Display UI
        startMethod.handler = aiSideTabScript.Initialise;
        startMethod.className = "AISideTabUI";
        listOfUIMethods.Add(startMethod);
        listOfConditionalMethods.Add(startMethod);
        //Top Bar UI
        startMethod.handler = topBarScript.Initialise;
        startMethod.className = "TopBarUI";
        listOfUIMethods.Add(startMethod);
        //Tooltip Actor
        startMethod.handler = tooltipActorScript.Initialise;
        startMethod.className = "TooltipActor";
        listOfUIMethods.Add(startMethod);
        //Tooltip Player
        startMethod.handler = tooltipPlayerScript.Initialise;
        startMethod.className = "TooltipPlayer";
        listOfUIMethods.Add(startMethod);
        //Tooltip Node
        startMethod.handler = tooltipNodeScript.Initialise;
        startMethod.className = "TooltipNode";
        listOfUIMethods.Add(startMethod);
        //PopUpFixed
        startMethod.handler = popUpFixedScript.Initialise;
        startMethod.className = "PopUpFixed";
        listOfUIMethods.Add(startMethod);
        //PopUpDynamic
        startMethod.handler = popUpDynamicScript.Initialise;
        startMethod.className = "PopUpDynamic";
        listOfUIMethods.Add(startMethod);
        //ModalOutcome
        startMethod.handler = outcomeScript.Initialise;
        startMethod.className = "ModalOutcome";
        listOfUIMethods.Add(startMethod);
        //ModalInventoryUI
        startMethod.handler = inventoryScript.Initialise;
        startMethod.className = "ModalInventoryUI";
        listOfUIMethods.Add(startMethod);
        //ModalGenericPicker
        startMethod.handler = genericPickerScript.Initialise;
        startMethod.className = "ModalGenericPicker";
        listOfUIMethods.Add(startMethod);
        //BillboardUI
        startMethod.handler = billboardScript.Initialise;
        startMethod.className = "BillboardUI";
        listOfUIMethods.Add(startMethod);
        //AdvertUI
        startMethod.handler = advertScript.Initialise;
        startMethod.className = "AdvertUI";
        listOfUIMethods.Add(startMethod);
        //TabbedUI
        startMethod.handler = tabbedUIScript.Initialise;
        startMethod.className = "TabbedUI";
        listOfUIMethods.Add(startMethod);
        //TutorialUI
        startMethod.handler = tutorialUIScript.Initialise;
        startMethod.className = "TutorialUI";
        listOfUIMethods.Add(startMethod);
        //GameHelpUI
        startMethod.handler = gameHelpScript.Initialise;
        startMethod.className = "GameHelpUI";
        listOfUIMethods.Add(startMethod);
        //NewTurnUI
        startMethod.handler = newTurnScript.Initialise;
        startMethod.className = "NewTurnUI";
        listOfUIMethods.Add(startMethod);
        //MasterHelpUI
        startMethod.handler = masterHelpScript.Initialise;
        startMethod.className = "ModalHelpUI";
        listOfUIMethods.Add(startMethod);
        //FinderSideTabUI
        startMethod.handler = finderSideTabScript.Initialise;
        startMethod.className = "FinderSideTabUI";
        listOfUIMethods.Add(startMethod);
        //FinderUI
        startMethod.handler = finderScript.Initialise;
        startMethod.className = "FinderUI";
        listOfUIMethods.Add(startMethod);
        #endregion

        #region Debug Methods
        //
        // - - - Debug methods - - -
        //
        //Debug Graphics Manager
        startMethod.handler = debugGraphicsScript.Initialise;
        startMethod.className = "DebugGraphicManager";
        listOfDebugMethods.Add(startMethod);
        //FeatureManager -> Last (toggles features))
        startMethod.handler = featureScript.Initialise;
        startMethod.className = "FeatureManager";
        listOfDebugMethods.Add(startMethod);
        //data Validation (Last / Optional)
        if (isDataValidation == true)
        {
            startMethod.handler = validateScript.Initialise;
            startMethod.className = "ValidationManager (Content)";
            listOfDebugMethods.Add(startMethod);
        }
        #endregion
    }
    #endregion


    #region Initialise Methods, Performance Monitoring or not
    /// <summary>
    /// Initialise a start list of methods (without performance monitoring)
    /// </summary>
    private void InitialiseMethods(List<StartMethod> listOfMethods)
    {
        //run each method via delegates in their preset order
        if (listOfMethods != null)
        {
            GameState state = GameManager.i.inputScript.GameState;
            foreach (StartMethod method in listOfMethods)
            {
                if (method.handler != null)
                {
                    method.handler(state);
                    Debug.LogFormat("[Per] GameManager.cs -> InitialiseMethods: {0}, {1}, state {2}{3}", method.className, method.handler.Method.Name, state, "\n");
                }
                else { Debug.LogErrorFormat("Invalid startMethod handler for {0}", method.className); }
            }
        }
        else { Debug.LogError("Invalid listOfMethods (Null)"); }
    }

    /// <summary>
    /// Initialise game start sequence with Performance Monitoring
    /// </summary>
    private void InitialiseWithPerformanceMonitoring(List<StartMethod> listOfMethods)
    {
        //run each method via delegates in their preset order
        if (listOfMethods != null)
        {
            GameState state = inputScript.GameState;
            //start timer tally to get an overall performance time
            testScript.TimerTallyStart();
            foreach (StartMethod method in listOfMethods)
            {
                if (method.handler != null)
                {
                    testScript.StartTimer();
                    method.handler(state);
                    long elapsed = testScript.StopTimer();
                    Debug.LogFormat("[Per] {0} -> {1}: {2} ms, state {3}{4}", method.className, method.handler.Method.Name, elapsed, state, "\n");
                }
                else { Debug.LogErrorFormat("Invalid startMethod handler for {0}", method.className); }
            }
            totalTime += testScript.TimerTallyStop();
        }
        else { Debug.LogError("Invalid listOfMethods (Null)"); }
    }

    private void DisplayTotalTime()
    { Debug.LogFormat("[Per] GameManager.cs -> InitialiseWithPerfomanceMonitoring: <b>TOTAL TIME {0} ms</b>{1}", totalTime, "\n"); }

    #endregion


    #region InitialiseMainMenu
    /// <summary>
    /// Initialise Main Menu
    /// </summary>
    private void InitialiseMainMenu()
    {
        /*ModalMainMenuDetails detailsMain = new ModalMainMenuDetails()
        {
            alignHorizontal = AlignHorizontal.Centre,
            background = Background.Start,
            isResume = false,
        };
        mainMenuScript.InitialiseMainMenu(detailsMain);*/

        //activate menu
        EventManager.i.PostNotification(EventType.OpenMainMenu, this, MainMenuType.Main, "GameManager.cs -> InitialiseMainMenu");
    }
    #endregion


    #region InitialiseNewSession
    /// <summary>
    /// New Session create level
    /// </summary>
    public void InitialiseNewSession()
    {
        //lock mouse to prevent mouseover events occuring prior to full initialisation
        Cursor.lockState = CursorLockMode.Locked;
        //actor pool
        optionScript.isActorPool = !isRandomActors; //reversed with exclamation mark
        optionScript.isOnMapRandom = isRandomOnMap;
        //start sequence
        if (isPerformanceLog == false)
        {
            //normal start-up
            InitialiseMethods(listOfLevelMethods);
            InitialiseMethods(listOfUIMethods);
            InitialiseMethods(listOfDebugMethods);
        }
        else
        {
            //start-up with Performance Monitoring
            InitialiseWithPerformanceMonitoring(listOfLevelMethods);
            InitialiseWithPerformanceMonitoring(listOfUIMethods);
            InitialiseWithPerformanceMonitoring(listOfDebugMethods);
            DisplayTotalTime();
        }
        //set session flag
        isSession = true;
        //do a final redraw before game start
        nodeScript.NodeRedraw = true;
        //colour scheme
        optionScript.ColourOption = ColourScheme.Normal;
        //free mouse for normal operations
        Cursor.lockState = CursorLockMode.None;
    }
    #endregion


    #region InitialiseTutorial
    /// <summary>
    /// Tutorial training simulation -> gameState.TutorialOptions
    /// </summary>
    public void InitialiseTutorial()
    {
        //lock mouse to prevent mouseover events occuring prior to full initialisation
        Cursor.lockState = CursorLockMode.Locked;
        //start sequence
        if (isPerformanceLog == false)
        {
            //normal start-up
            InitialiseMethods(listOfTutorialMethods);
            if (isSession == false)
            {
                //overall session initialisation hasn't yet occured
                InitialiseMethods(listOfLevelMethods);
                InitialiseMethods(listOfUIMethods);
                InitialiseMethods(listOfDebugMethods);
                //set session flag
                isSession = true;
            }
            else { InitialiseMethods(listOfLevelMethods); }
        }
        else
        {
            //start-up with Performance Monitoring
            InitialiseWithPerformanceMonitoring(listOfTutorialMethods);
            if (isSession == false)
            {
                //overall session initialisation hasn't yet occured
                InitialiseWithPerformanceMonitoring(listOfLevelMethods);
                InitialiseWithPerformanceMonitoring(listOfUIMethods);
                InitialiseWithPerformanceMonitoring(listOfDebugMethods);
                //set session flag
                isSession = true;
            }
            else { InitialiseWithPerformanceMonitoring(listOfLevelMethods); }
            DisplayTotalTime();
        }
        //do a final redraw before game start
        nodeScript.NodeRedraw = true;
        //colour scheme
        optionScript.ColourOption = ColourScheme.Normal;
        //free mouse for normal operations
        Cursor.lockState = CursorLockMode.None;
    }
    #endregion


    #region InitialiseNewLevel
    /// <summary>
    /// New FollowOn level
    /// </summary>
    public void InitialiseNewLevel()
    {
        //lock mouse to prevent mouseover events occuring prior to full initialisation
        Cursor.lockState = CursorLockMode.Locked;
        //remove redundant events (just in case, shouldn't be any)
        EventManager.i.RemoveRedundancies();
        //start sequence
        if (isPerformanceLog == false)
        {
            //normal
            InitialiseMethods(listOfLevelMethods);
            InitialiseMethods(listOfUIMethods);
        }
        else
        {
            //Performance Monitoring
            InitialiseWithPerformanceMonitoring(listOfLevelMethods);
            InitialiseMethods(listOfUIMethods);
            DisplayTotalTime();
        }
        //do a final redraw before level start
        nodeScript.NodeRedraw = true;
        //free mouse for normal operations
        Cursor.lockState = CursorLockMode.None;
    }
    #endregion


    #region InitialiseLoadGame
    /// <summary>
    /// Load a save game, methods required to generate the new level based on the loaded scenario seed. Input playerSide.level so the appropriate AI initialisation can be done
    /// </summary>
    public void InitialiseLoadGame(int playerSide)
    {
        //lock mouse to prevent mouseover events occuring prior to full initialisation
        Cursor.lockState = CursorLockMode.Locked;
        if (isSession == true)
        {
            //initialises both AI sides regardless as some Authority AI data collections are required for the others
            InitialiseMethods(listOfLoadMethods);
            InitialiseMethods(listOfConditionalMethods);
        }
        else
        {
            InitialiseMethods(listOfLevelMethods);
            InitialiseMethods(listOfDebugMethods);
            InitialiseMethods(listOfConditionalMethods);

            //load up UI elements for a load at Start session
            if (inputScript.GameState == GameState.LoadAtStart)
            { InitialiseMethods(listOfUIMethods); }

            //set session flag
            isSession = true;
        }
        //do a final redraw before game start
        nodeScript.NodeRedraw = true;
        //colour scheme
        optionScript.ColourOption = ColourScheme.Normal;
        //free mouse for normal operations
        Cursor.lockState = CursorLockMode.None;
    }
    #endregion


    #region ResetNewLevelData
    /// <summary>
    /// reset all relevant data prior to a new level
    /// </summary>
    private void ResetNewLevelData(GameState state)
    {
        //no need to reset if a new session
        if (isSession == true)
        {
            levelScript.Reset();
            animateScript.Reset();
            if (state != GameState.LoadGame)
            {
                //follow on level / new game
                dataScript.ResetNewLevel();
                turnScript.ResetTurn();
                messageScript.ResetCounter();
                nodeScript.ResetCounters();
                contactScript.ResetCounter();
                teamScript.ResetCounter();
            }
            else
            {
                //Load Game
                dataScript.ResetLoadGame();
                nodeScript.ResetCounters();
                actorScript.ResetCounter();
                contactScript.ResetCounter();
            }
        }
    }
    #endregion


    #region Random Seed

    /// <summary>
    /// supplies a random seed
    /// </summary>
    /// <returns></returns>
    public int GetRandomSeed()
    { return (int)DateTime.Now.Ticks & 0x0000FFFF; }

    /// <summary>
    /// used to save random seedDev sequence state prior to changing the seed
    /// </summary>
    public void SaveRandomDevState()
    { devState = Random.state; }

    /// <summary>
    /// used to restore random seedDev sequence after changing the seed (sequence will continue on as before)
    /// </summary>
    public void RestoreRandomDevState()
    { Random.state = devState; }
    #endregion


    #region Safe Destroy
    /// <summary>
    /// Destory a prefab clone safely
    /// </summary>
    /// <param name="obj"></param>
    public void SafeDestroy(GameObject obj)
    {
        /*obj.transform.parent = null;*/
        obj.transform.SetParent(null);
        obj.name = "$disposed";
        Destroy(obj);
        obj.SetActive(false);
    }
    #endregion


    #region Static Methods

    /// <summary>
    /// Call this to automatically create a timeStamp with the current turn and scenario index (for a MetaGame turn will be 999 and scenarioIndex will be the scenario just completed)
    /// </summary>
    /// <returns></returns>
    public static TimeStamp SetTimeStamp()
    {
        TimeStamp timeStamp = new TimeStamp();
        timeStamp.scenario = i.campaignScript.GetScenarioIndex();
        switch (i.inputScript.GameState)
        {
            case GameState.MetaGame:
                timeStamp.turn = 999;
                break;
            default:
                timeStamp.turn = GameManager.i.turnScript.Turn;
                break;
        }
        return timeStamp;
    }

    /// <summary>
    /// returns inputted string formatted in the specified colour, ready for display. Returns original, unformatted text if a problem. 
    /// NOTE: Named 'Formatt' so as not to clash with C# 'Format'
    /// </summary>
    /// <param name="text"></param>
    /// <param name="colorType"></param>
    /// <returns></returns>
    public static string Formatt(string text, ColourType colourType)
    {
        string formattedText = text;
        if (string.IsNullOrEmpty(text) == false)
        { formattedText = string.Format("{0}{1}{2}", i.colourScript.GetColour(colourType), text, i.colourScript.GetEndTag()); }
        else { Debug.LogError("Invalid text (Null)"); }
        return formattedText;
    }

    #endregion



    //place methods above here
}
