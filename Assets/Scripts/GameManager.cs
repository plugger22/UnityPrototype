﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using gameAPI;
using delegateAPI;
using System.IO;
using System.Globalization;
using modalAPI;

public struct StartMethod
{
    public InitialisationDelegate handler;
    public string className;
}



/// <summary>
/// Main game controller
/// </summary>
[Serializable]
public class GameManager : MonoBehaviour
{
    #region Components
    public static GameManager instance = null;      //static instance of GameManager which allows it to be accessed by any other script
    [HideInInspector] public StartManager startScript;                //Start Manager
    [HideInInspector] public LevelManager levelScript;                //Level Manager
    [HideInInspector] public PreLoadManager preloadScript;            //PreLoad Manager
    [HideInInspector] public LoadManager loadScript;                  //Load Manager
    [HideInInspector] public MetaManager metaScript;                  //Meta Manager
    [HideInInspector] public DataManager dataScript;                  //Data Manager
    [HideInInspector] public StatisticManager statScript;             //Statistics Manager
    [HideInInspector] public GUIManager guiScript;                    //GUI Manager
    [HideInInspector] public GlobalManager globalScript;              //Global Manager
    [HideInInspector] public TooltipManager tooltipScript;            //Tooltip Manager
    [HideInInspector] public ScenarioManager scenarioScript;          //Scenario Manager
    [HideInInspector] public CampaignManager campaignScript;          //Campaign Manager
    [HideInInspector] public NewsManager newsScript;                  //News Manager
    [HideInInspector] public ActorManager actorScript;                //Actor Manager 
    [HideInInspector] public ContactManager contactScript;            //Contact Manager
    [HideInInspector] public ActionManager actionScript;              //Action Manager
    [HideInInspector] public SideManager sideScript;                  //Side Manager
    [HideInInspector] public HelpManager helpScript;                  //Help Manager
    [HideInInspector] public TurnManager turnScript;                  //Turn Manager
    [HideInInspector] public InputManager inputScript;                //Input Manager
    [HideInInspector] public EffectManager effectScript;              //Effect Manager
    [HideInInspector] public TargetManager targetScript;              //Target Manager
    [HideInInspector] public OptionManager optionScript;              //Option Manager
    [HideInInspector] public PlayerManager playerScript;              //Player Manager
    [HideInInspector] public TraitManager traitScript;                //Trait Manager
    [HideInInspector] public FactionManager factionScript;            //Faction Manager
    [HideInInspector] public SecretManager secretScript;              //Secret Manager
    [HideInInspector] public OrganisationManager orgScript;           //Organisation Manager
    [HideInInspector] public CityManager cityScript;                  //City Manager
    [HideInInspector] public ObjectiveManager objectiveScript;        //Objectives Manager
    [HideInInspector] public NodeManager nodeScript;                  //Node Manager
    [HideInInspector] public DijkstraManager dijkstraScript;          //Dijkstra Manager
    [HideInInspector] public TeamManager teamScript;                  //Team Manager
    [HideInInspector] public GearManager gearScript;                  //Gear Manager
    [HideInInspector] public CaptureManager captureScript;            //Capture Manager
    [HideInInspector] public AIManager aiScript;                      //AI Manager
    [HideInInspector] public AIRebelManager aiRebelScript;            //AI Resistance Manager
    [HideInInspector] public NemesisManager nemesisScript;            //Nemesis Manager
    [HideInInspector] public ResistanceManager rebelScript;           //Resistance Manager
    [HideInInspector] public AuthorityManager authorityScript;        //Authority Manager
    [HideInInspector] public MessageManager messageScript;            //Message Manager
    [HideInInspector] public ItemDataManager itemDataScript;          //ItemData Manager
    [HideInInspector] public ConnectionManager connScript;            //Connection Manager
    [HideInInspector] public MissionManager missionScript;            //Mission Manager
    [HideInInspector] public ColourManager colourScript;              //Colour Manager
    [HideInInspector] public TestManager testScript;                  //Test Manager
    [HideInInspector] public ValidationManager validateScript;        //Validation Manager
    [HideInInspector] public TooltipNode tooltipNodeScript;           //node tooltip static instance
    [HideInInspector] public TooltipConnection tooltipConnScript;     //connection tooltip static instance
    [HideInInspector] public TooltipActor tooltipActorScript;         //actor tooltip static instance
    [HideInInspector] public TooltipPlayer tooltipPlayerScript;       //player tooltip static instance
    [HideInInspector] public TooltipGeneric tooltipGenericScript;     //generic tooltip static instance
    [HideInInspector] public TooltipHelp tooltipHelpScript;           //help tooltip static instance
    [HideInInspector] public ModalActionMenu actionMenuScript;        //Modal Action Menu (node)
    [HideInInspector] public ModalMainMenu mainMenuScript;            //Modal Main Menu
    [HideInInspector] public ModalOutcome outcomeScript;              //Modal Outcome window
    [HideInInspector] public ModalTeamPicker teamPickerScript;        //Modal Team Picker window
    [HideInInspector] public ModalGenericPicker genericPickerScript;  //Modal Generic Picker window
    [HideInInspector] public ModalInventoryUI inventoryScript;        //Modal InventoryUI window
    /*[HideInInspector] public ModalDiceUI diceScript;                //Modal Dice UI window*/
    [HideInInspector] public ModalGUI modalGUIScript;                 //Modal GUI 
    [HideInInspector] public AlertUI alertScript;                     //Alert UI text display
    [HideInInspector] public WidgetTopUI widgetTopScript;             //Widget Top UI
    [HideInInspector] public CityInfoUI cityInfoScript;               //City Info UI
    [HideInInspector] public MainInfoUI mainInfoScript;               //Main Info UI
    [HideInInspector] public AIDisplayUI aiDisplayScript;             //AI Display UI
    [HideInInspector] public AISideTabUI aiSideTabScript;             //AI SideTab UI
    [HideInInspector] public ActorPanelUI actorPanelScript;           //Actor Panel UI
    [HideInInspector] public BasePanelUI basePanelScript;             //Base Panel UI
    [HideInInspector] public DebugGraphics debugGraphicsScript;       //Debug only Graphics
    #endregion


    #region Variables
    [Tooltip("Leave as default 0 for random. Can be a whole number between -2147483648 and 2147483647")]
    public int seedDev = 0;                                            //random seed for development
    [Tooltip("If true Player side set to Authority")]               //DEBUG
    public bool isAuthority;
    [Tooltip("If true AI handles both sides. OVERRIDES all other settings. Player side (at end of autoRun) is determined by 'isAuthority' setting. Switch OFF if playing normally")]
    /*public bool isBothAI;
    [Tooltip("Switch ON to get a performance log of initialisation ")]*/
    public bool isPerformanceLog;
    [Tooltip("Runs ValidationManager.cs to check data at game start")]
    public bool isValidateData;
    [Tooltip("Runs SO Validator to cross reference SO's in assets vs. those in LoadManager.cs arrays. Editor only. Slow")]
    public bool isValidateSO;
    [Tooltip("If true will choose a random city, otherwise will be the one specified by the Scenario")]
    public bool isRandomCity;
    [Tooltip("Autoruns game for 'x' number of turns with current player & Both sides as AI. Leave at Zero for normal operation")]
    public int autoRunTurns = 0;



    private Random.State devState;                                                  //used to restore seedDev random sequence after any interlude, eg. level generation with a unique seed
    private long totalTime;                                                      //used for Performance monitoring on start up

    private List<StartMethod> listOfGlobalMethods = new List<StartMethod>();        //start game global methods
    private List<StartMethod> listOfGameMethods = new List<StartMethod>();          //game managerment methods
    private List<StartMethod> listOfLevelMethods = new List<StartMethod>();         //level related methods
    private List<StartMethod> listOfUIMethods = new List<StartMethod>();            //UI related methods
    private List<StartMethod> listOfDebugMethods = new List<StartMethod>();         //Debug related methods

    #endregion


    #region Awake method
    /// <summary>
    /// Constructor
    /// </summary>
    private void Awake()
    {
        //check if instance already exists
        if (instance == null)
        { instance = this; }
        //if instance already exists and it's not this
        else if (instance != this)
        {
            //Then destroy this in order to reinforce the singleton pattern (can only ever be one instance of GameManager)
            Destroy(gameObject);
        }
        //random seed to facilitate replication of results
        if (seedDev == 0)
        { seedDev = (int)DateTime.Now.Ticks & 0x0000FFFF; }
        Debug.Log("Seed: " + seedDev);
        Random.InitState(seedDev);
        //debug -> write seed to file
        DateTime date1 = DateTime.Now;
        string seedInfo = string.Format("Dev seed {0} -> {1}", seedDev, date1.ToString("f", CultureInfo.CreateSpecificCulture("en-AU"))) + Environment.NewLine;
        File.AppendAllText("Seed.txt", seedInfo);
        //Get component references
        startScript = GetComponent<StartManager>();
        levelScript = GetComponent<LevelManager>();
        preloadScript = GetComponent<PreLoadManager>();
        loadScript = GetComponent<LoadManager>();
        statScript = GetComponent<StatisticManager>();
        metaScript = GetComponent<MetaManager>();
        dataScript = GetComponent<DataManager>();
        guiScript = GetComponent<GUIManager>();
        globalScript = GetComponent<GlobalManager>();
        scenarioScript = GetComponent<ScenarioManager>();
        campaignScript = GetComponent<CampaignManager>();
        actorScript = GetComponent<ActorManager>();
        contactScript = GetComponent<ContactManager>();
        actionScript = GetComponent<ActionManager>();
        playerScript = GetComponent<PlayerManager>();
        traitScript = GetComponent<TraitManager>();
        factionScript = GetComponent<FactionManager>();
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
        validateScript = GetComponent<ValidationManager>();
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
        //Get UI static references -> from PanelManager
        tooltipNodeScript = TooltipNode.Instance();
        tooltipConnScript = TooltipConnection.Instance();
        tooltipActorScript = TooltipActor.Instance();
        tooltipPlayerScript = TooltipPlayer.Instance();
        tooltipGenericScript = TooltipGeneric.Instance();
        tooltipHelpScript = TooltipHelp.Instance();
        actionMenuScript = ModalActionMenu.Instance();
        mainMenuScript = ModalMainMenu.Instance();
        outcomeScript = ModalOutcome.Instance();
        teamPickerScript = ModalTeamPicker.Instance();
        genericPickerScript = ModalGenericPicker.Instance();
        inventoryScript = ModalInventoryUI.Instance();
        /*diceScript = ModalDiceUI.Instance();*/
        modalGUIScript = ModalGUI.Instance();
        widgetTopScript = WidgetTopUI.Instance();
        cityInfoScript = CityInfoUI.Instance();
        mainInfoScript = MainInfoUI.Instance();
        aiDisplayScript = AIDisplayUI.Instance();
        aiSideTabScript = AISideTabUI.Instance();
        alertScript = AlertUI.Instance();
        actorPanelScript = ActorPanelUI.Instance();
        basePanelScript = BasePanelUI.Instance();
        debugGraphicsScript = DebugGraphics.Instance();
        //set up list of delegates
        InitialiseStartSequence();
        //sets this to not be destroyed when reloading a scene
        DontDestroyOnLoad(gameObject);
        //Error Checking
        Debug.Assert(startScript != null, "Invalid startScript (Null)");
        Debug.Assert(levelScript != null, "Invalid levelScript (Null)");
        Debug.Assert(preloadScript != null, "Invalid preloadScript (Null)");
        Debug.Assert(loadScript != null, "Invalid loadScript (Null)");
        Debug.Assert(statScript != null, "Invalid statScript (Null)");
        Debug.Assert(metaScript != null, "Invalid metaScript (Null)");
        Debug.Assert(dataScript != null, "Invalid dataScript (Null)");
        Debug.Assert(guiScript != null, "Invalid guiScript (Null)");
        Debug.Assert(globalScript != null, "Invalid globalScript (Null)");
        Debug.Assert(scenarioScript != null, "Invalid scenarioScript (Null)");
        Debug.Assert(campaignScript != null, "Invalid campaignScript (Null)");

        Debug.Assert(actorScript != null, "Invalid actorScript (Null)");
        Debug.Assert(contactScript != null, "Invalid contactScript (Null)");
        Debug.Assert(actionScript != null, "Invalid actionScript (Null)");
        Debug.Assert(playerScript != null, "Invalid playerScript (Null)");
        Debug.Assert(traitScript != null, "Invalid traitScript (Null)");
        Debug.Assert(factionScript != null, "Invalid factionScript (Null)");
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
        //singletons
        Debug.Assert(tooltipNodeScript != null, "Invalid tooltipNodeScript (Null)");
        Debug.Assert(tooltipConnScript != null, "Invalid tooltipConnScript (Null)");
        Debug.Assert(tooltipActorScript != null, "Invalid tooltipActorScript (Null)");
        Debug.Assert(tooltipPlayerScript != null, "Invalid tooltipPlayerScript (Null)");
        Debug.Assert(tooltipGenericScript != null, "Invalid tooltipGenericScript (Null)");
        Debug.Assert(actionMenuScript != null, "Invalid actionMenuScript (Null)");
        Debug.Assert(mainMenuScript != null, "Invalid mainMenuScript (Null)");
        Debug.Assert(outcomeScript != null, "Invalid outcomeScript (Null)");
        Debug.Assert(teamPickerScript != null, "Invalid teamPickerScript (Null)");
        Debug.Assert(genericPickerScript != null, "Invalid genericPickerScript (Null)");
        Debug.Assert(inventoryScript != null, "Invalid inventoryScript (Null)");
        Debug.Assert(modalGUIScript != null, "Invalid modalGUIScript (Null)");
        Debug.Assert(widgetTopScript != null, "Invalid widgetTopScript (Null)");
        Debug.Assert(cityInfoScript != null, "Invalid cityInfoScript (Null)");
        Debug.Assert(mainInfoScript != null, "Invalid mainInfoScript (Null)");
        Debug.Assert(aiDisplayScript != null, "Invalid aiDisplayScript (Null)");
        Debug.Assert(aiSideTabScript != null, "Invalid aiSideTabScript (Null)");
        Debug.Assert(alertScript != null, "Invalid alertScript (Null)");
        Debug.Assert(actorPanelScript != null, "Invalid actorPanelScript (Null)");
        Debug.Assert(basePanelScript != null, "Invalid basePanelScript (Null)");
        Debug.Assert(debugGraphicsScript != null, "Invalid debugGraphicsScript (Null)");
    }
    #endregion


    #region Start method
    private void Start()
    {
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
            InitialiseNewLevel();
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
        //redraw any Nodes where required
        if (nodeScript.NodeRedraw == true)
        { EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.Redraw, "GameManager.cs -> Update"); }
        //Handle Game Input
        if (Input.anyKeyDown == true)
        { inputScript.ProcessInput(); }
    }
    #endregion


    #region StartUp Sequence
    /// <summary>
    /// set up list of delegates ready for initialisation (do this because there are two version of initialisation, performance monitoring ON or OFF)
    /// </summary>
    private void InitialiseStartSequence()
    {
        StartMethod startMethod = new StartMethod();
        //
        // - - - Global Methods
        //
        //PreLoad Manager 
        startMethod.handler = GameManager.instance.preloadScript.Initialise;
        startMethod.className = "PreLoadManager";
        listOfGlobalMethods.Add(startMethod);
        //Load Manager -> InitialiseStart
        startMethod.handler = GameManager.instance.loadScript.InitialiseStart;
        startMethod.className = "LoadManager";
        listOfGlobalMethods.Add(startMethod);
#if (UNITY_EDITOR)
        //SO Checker (After LoadManager.cs / Optional)
        if (isValidateSO == true)
        {
            startMethod.handler = GameManager.instance.validateScript.ValidateSO;
            startMethod.className = "ValidationManager (SO)";
            listOfGlobalMethods.Add(startMethod);
        }
#endif
        //Global Manager -> immediately after dataScript.InitialiseStart and before dataScript.InitialiseEarly 
        startMethod.handler = GameManager.instance.globalScript.Initialise;
        startMethod.className = "GlobalManager";
        listOfGlobalMethods.Add(startMethod);
        //Colour Manager
        startMethod.handler = GameManager.instance.colourScript.Initialise;
        startMethod.className = "ColourManager";
        listOfGlobalMethods.Add(startMethod);
        //Message Manager -> after globalScript and before a lot of other stuff (pre-start messages need to be initialised for side)
        startMethod.handler = GameManager.instance.messageScript.InitialiseEarly;
        startMethod.className = "MessageManager";
        listOfGlobalMethods.Add(startMethod);
        //ItemData Manager
        startMethod.handler = GameManager.instance.itemDataScript.Initialise;
        startMethod.className = "ItemDataManager";
        listOfGlobalMethods.Add(startMethod);
        //ModalGUI
        startMethod.handler = GameManager.instance.modalGUIScript.Initialise;
        startMethod.className = "ModalGUI";
        listOfGlobalMethods.Add(startMethod);
        //Actor Manager -> PreInitialise
        startMethod.handler = GameManager.instance.actorScript.PreInitialiseActors;
        startMethod.className = "ActorManager";
        listOfGlobalMethods.Add(startMethod);
        //Load Manager -> InitialiseEarly
        startMethod.handler = GameManager.instance.loadScript.InitialiseEarly;
        startMethod.className = "LoadManager";
        listOfGlobalMethods.Add(startMethod);
        //GUI Manager -> before any actor scripts (actorScript.PreInitialiseActors is O.K to be earlier)
        startMethod.handler = GameManager.instance.guiScript.Initialise;
        startMethod.className = "GUIManager";
        listOfGlobalMethods.Add(startMethod);
        //Help Manager -> before LoadManager.cs InitialiseLate
        startMethod.handler = GameManager.instance.helpScript.Initialise;
        startMethod.className = "HelpManager";
        listOfGlobalMethods.Add(startMethod);
        //Input Manager
        startMethod.handler = GameManager.instance.inputScript.Initialise;
        startMethod.className = "InputManager";
        listOfGlobalMethods.Add(startMethod);
        //
        // - - - Game methods - - -
        //
        //Meta Manager
        startMethod.handler = GameManager.instance.metaScript.Initialise;
        startMethod.className = "MetaManager";
        listOfGameMethods.Add(startMethod);
        //Campaign Manager
        startMethod.handler = GameManager.instance.campaignScript.Initialise;
        startMethod.className = "CampaignManager";
        listOfGameMethods.Add(startMethod);
        //
        // - - - Level methods - - -
        //
        //Scenario Manager InitialiseEarly -> before level & Side Managers
        startMethod.handler = GameManager.instance.scenarioScript.InitialiseEarly;
        startMethod.className = "ScenarioManager Early";
        listOfLevelMethods.Add(startMethod);
        //Tooltip Node
        startMethod.handler = GameManager.instance.tooltipNodeScript.Initialise;
        startMethod.className = "TooltipNode";
        listOfLevelMethods.Add(startMethod);
        //Side Manager -> after scenarioManager
        startMethod.handler = GameManager.instance.sideScript.Initialise;
        startMethod.className = "SideManager";
        listOfLevelMethods.Add(startMethod);
        //Objective Manager
        startMethod.handler = GameManager.instance.objectiveScript.Initialise;
        startMethod.className = "ObjectiveManager";
        listOfLevelMethods.Add(startMethod);
        //Actor Panel UI -> before actorScript.Initialise
        startMethod.handler = GameManager.instance.actorPanelScript.Initialise;
        startMethod.className = "ActorPanelUI";
        listOfLevelMethods.Add(startMethod);
        //Base Panel UI -> before CityInfo.InitialiseLate
        startMethod.handler = GameManager.instance.basePanelScript.Initialise;
        startMethod.className = "BasePanelUI";
        listOfLevelMethods.Add(startMethod);
        //Actor Manager -> before DataManager.cs & NodeManager.cs
        startMethod.handler = GameManager.instance.actorScript.Initialise;
        startMethod.className = "ActorManager";
        listOfLevelMethods.Add(startMethod);
        //Level Manager
        startMethod.handler = GameManager.instance.levelScript.Initialise;
        startMethod.className = "LevelManager";
        listOfLevelMethods.Add(startMethod);
        //Load Manager -> InitialiseLate -> immediately after levelScript.Initialise
        startMethod.handler = GameManager.instance.loadScript.InitialiseLate;
        startMethod.className = "LoadManager";
        listOfLevelMethods.Add(startMethod);
        //Data Manager -> InitialiseLate -> immediately after LoadScript.Initialise
        startMethod.handler = GameManager.instance.dataScript.InitialiseLate;
        startMethod.className = "DataManager";
        listOfLevelMethods.Add(startMethod);
        //Statistic Manager -> Initialise -> after DataManager
        startMethod.handler = GameManager.instance.statScript.Initialise;
        startMethod.className = "StatisticManager";
        listOfLevelMethods.Add(startMethod);
        //Dijkstra Manager -> Initialise -> after dataScript & LevelScript
        startMethod.handler = GameManager.instance.dijkstraScript.Initialise;
        startMethod.className = "DijkstraManager";
        listOfLevelMethods.Add(startMethod);
        //Faction Manager
        startMethod.handler = GameManager.instance.factionScript.Initialise;
        startMethod.className = "FactionManager";
        listOfLevelMethods.Add(startMethod);
        //AI Manager -> after factionScript, before ScenarioManager -> InitialiseLate
        startMethod.handler = GameManager.instance.aiScript.Initialise;
        startMethod.className = "AIManager";
        listOfLevelMethods.Add(startMethod);
        //Scenario Manager -> InitialiseLate -> after levelScript.Initialise
        startMethod.handler = GameManager.instance.scenarioScript.InitialiseLate;
        startMethod.className = "ScenarioManager Late";
        listOfLevelMethods.Add(startMethod);
        //Message Manager -> InitialseLate -> after ScenarioManager
        startMethod.handler = GameManager.instance.messageScript.InitialiseLate;
        startMethod.className = "MessageManager";
        listOfLevelMethods.Add(startMethod);
        //Secret Manager -> after dataScript and before playerScript
        startMethod.handler = GameManager.instance.secretScript.Initialise;
        startMethod.className = "SecretManager";
        listOfLevelMethods.Add(startMethod);
        //Contact Manager -> before ActorManager.cs InitialiseLate
        startMethod.handler = GameManager.instance.contactScript.Initialise;
        startMethod.className = "ContactManager";
        listOfLevelMethods.Add(startMethod);
        //Actor Manager -> after DataManager.cs & before NodeManager.cs
        startMethod.handler = GameManager.instance.actorScript.InitialiseLate;
        startMethod.className = "ActorManager";
        listOfLevelMethods.Add(startMethod);
        //Action Manager
        startMethod.handler = GameManager.instance.actionScript.Initialise;
        startMethod.className = "ActionManager";
        listOfLevelMethods.Add(startMethod);
        //Node Manager
        startMethod.handler = GameManager.instance.nodeScript.Initialise;
        startMethod.className = "NodeManager";
        listOfLevelMethods.Add(startMethod);
        //Effect Manager -> after nodeScript
        startMethod.handler = GameManager.instance.effectScript.Initialise;
        startMethod.className = "EffectManager";
        listOfLevelMethods.Add(startMethod);
        //Team Manager
        startMethod.handler = GameManager.instance.teamScript.Initialise;
        startMethod.className = "TeamManager";
        listOfLevelMethods.Add(startMethod);
        //Turn Manager
        startMethod.handler = GameManager.instance.turnScript.Initialise;
        startMethod.className = "TurnManager";
        listOfLevelMethods.Add(startMethod);
        //Gear Manager
        startMethod.handler = GameManager.instance.gearScript.Initialise;
        startMethod.className = "GearManager";
        listOfLevelMethods.Add(startMethod);
        //Team Picker
        startMethod.handler = GameManager.instance.teamPickerScript.Initialise;
        startMethod.className = "TeamPickerUI";
        listOfLevelMethods.Add(startMethod);
        //Capture Manager
        startMethod.handler = GameManager.instance.captureScript.Initialise;
        startMethod.className = "CaptureManager";
        listOfLevelMethods.Add(startMethod);
        //Authority Manager
        startMethod.handler = GameManager.instance.authorityScript.Initialise;
        startMethod.className = "AuthorityManager";
        listOfLevelMethods.Add(startMethod);
        //Player Manager
        startMethod.handler = GameManager.instance.playerScript.Initialise;
        startMethod.className = "PlayerManager";
        listOfLevelMethods.Add(startMethod);
        //Rebel AI Manager -> after Player Manager
        startMethod.handler = GameManager.instance.aiRebelScript.Initialise;
        startMethod.className = "AIRebelManager";
        listOfLevelMethods.Add(startMethod);
        //Trait Manager
        startMethod.handler = GameManager.instance.traitScript.Initialise;
        startMethod.className = "TraitManager";
        listOfLevelMethods.Add(startMethod);
        //Connection Manager
        startMethod.handler = GameManager.instance.connScript.Initialise;
        startMethod.className = "ConnectionManager";
        listOfLevelMethods.Add(startMethod);
        //
        // - - - UI methods - - -
        //
        //City Info UI
        startMethod.handler = GameManager.instance.cityInfoScript.Initialise;
        startMethod.className = "CityInfoUI";
        listOfUIMethods.Add(startMethod);
        //Main Info UI
        startMethod.handler = GameManager.instance.mainInfoScript.Initialise;
        startMethod.className = "MainInfoUI";
        listOfUIMethods.Add(startMethod);
        //AI Display UI 
        startMethod.handler = GameManager.instance.aiDisplayScript.Initialise;
        startMethod.className = "AIDisplayUI";
        listOfUIMethods.Add(startMethod);
        //AI SideTab UI -> after AI Display UI
        startMethod.handler = GameManager.instance.aiSideTabScript.Initialise;
        startMethod.className = "AISideTabUI";
        listOfUIMethods.Add(startMethod);
        //Widget Top UI
        startMethod.handler = GameManager.instance.widgetTopScript.Initialise;
        startMethod.className = "WidgetTopUI";
        listOfUIMethods.Add(startMethod);
        //
        // - - - Debug methods - - -
        //
        //Debug Graphics Manager
        startMethod.handler = GameManager.instance.debugGraphicsScript.Initialise;
        startMethod.className = "DebugGraphicManager";
        listOfDebugMethods.Add(startMethod);
        //data Validation (Last / Optional)
        if (isValidateData == true)
        {
            startMethod.handler = GameManager.instance.validateScript.Initialise;
            startMethod.className = "ValidationManager (Content)";
            listOfDebugMethods.Add(startMethod);
        }
    }
    #endregion


    #region Performance Monitoring or not
    /// <summary>
    /// Initialise a start list of methods (without performance monitoring)
    /// </summary>
    private void InitialiseMethods(List<StartMethod> listOfMethods)
    {
        //run each method via delegates in their preset order
        if (listOfMethods != null)
        {
            foreach (StartMethod method in listOfMethods)
            {
                if (method.handler != null)
                { method.handler(); }
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
            //start timer tally to get an overall performance time
            GameManager.instance.testScript.TimerTallyStart();
            foreach (StartMethod method in listOfMethods)
            {
                if (method.handler != null)
                {
                    GameManager.instance.testScript.StartTimer();
                    method.handler();
                    long elapsed = testScript.StopTimer();
                    Debug.LogFormat("[Per] {0} -> {1}: {2} ms{3}", method.className, method.handler.Method.Name, elapsed, "\n");
                }
                else { Debug.LogErrorFormat("Invalid startMethod handler for {0}", method.className); }
            }
            totalTime += testScript.TimerTallyStop();
            /**/
        }
        else { Debug.LogError("Invalid listOfMethods (Null)"); }
    }

    private void DisplayTotalTime()
    { Debug.LogFormat("[Per] GameManager.cs -> InitialiseWithPerfomanceMonitoring: <b>TOTAL TIME {0} ms</b>{1}", totalTime, "\n"); }

    #endregion


    #region InitialiseMenu
    /// <summary>
    /// Initialise Main Menu
    /// </summary>
    private void InitialiseMainMenu()
    {
        ModalMainMenuDetails detailsMain = new ModalMainMenuDetails() {
            alignHorizontal = AlignHorizontal.Centre,
            background = Background.Start
        };
        //activate menu
        mainMenuScript.InitialiseMainMenu(detailsMain);
    }
    #endregion


    #region InitialiseNewGame
    /// <summary>
    /// start an immediate autoRun that bypasses the campaign meta game and jumps straight into a level
    /// </summary>
    public void InitialiseNewLevel()
    {
        
        //lock mouse to prevent mouseover events occuring prior to full initialisation
        Cursor.lockState = CursorLockMode.Locked;
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
        //do a final redraw before game start
        nodeScript.NodeRedraw = true;
        //colour scheme
        optionScript.ColourOption = ColourScheme.Normal;

        //free mouse for normal operations
        Cursor.lockState = CursorLockMode.None;
    }
    #endregion


    private void StartArchive()
    {
        //lock mouse to prevent mouseover events occuring prior to full initialisation
        Cursor.lockState = CursorLockMode.Locked;

        //start sequence
        if (isPerformanceLog == false)
        {
            //normal start-up
            InitialiseMethods(listOfGlobalMethods);
            InitialiseMethods(listOfGameMethods);
            InitialiseMethods(listOfLevelMethods);
            InitialiseMethods(listOfUIMethods);
            InitialiseMethods(listOfDebugMethods);
        }
        else
        {
            //need testManager in order to access performance timer
            testScript.Initialise();
            //start-up with Performance Monitoring
            InitialiseWithPerformanceMonitoring(listOfGlobalMethods);
            InitialiseWithPerformanceMonitoring(listOfGameMethods);
            InitialiseWithPerformanceMonitoring(listOfLevelMethods);
            InitialiseWithPerformanceMonitoring(listOfUIMethods);
            InitialiseWithPerformanceMonitoring(listOfDebugMethods);
            DisplayTotalTime();
        }
        //do a final redraw before game start
        nodeScript.NodeRedraw = true;
        //free mouse for normal operations
        Cursor.lockState = CursorLockMode.None;
        //colour scheme
        optionScript.ColourOption = ColourScheme.Normal;
        //AutoRun
        if (autoRunTurns > 0)
        { GameManager.instance.turnScript.SetAutoRun(autoRunTurns); }
        else
        {
            InitialiseMainMenu();
        }
    }


    #region Random Seed
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
        obj.transform.parent = null;
        obj.name = "$disposed";
        Destroy(obj);
        obj.SetActive(false);
    }
    #endregion

    //place methods above here
}
