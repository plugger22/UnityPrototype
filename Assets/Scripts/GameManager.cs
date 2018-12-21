using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using gameAPI;
using delegateAPI;



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
    [HideInInspector] public GUIManager guiScript;                    //GUI Manager
    [HideInInspector] public GlobalManager globalScript;              //Global Manager
    [HideInInspector] public TooltipManager tooltipScript;            //Tooltip Manager
    [HideInInspector] public ScenarioManager scenarioScript;          //Scenario Manager
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
    [Tooltip("Leave as default 0 for random")]
    public int seed = 0;                                            //random seed
    [Tooltip("If true Player side set to Authority")]               //DEBUG
    public bool isAuthority;
    [Tooltip("Switch ON to get a performance log of initialisation ")]
    public bool isPerformanceLog;
    [Tooltip("Runs ValidationManager.cs to check data at game start")]
    public bool isValidateData;
    [Tooltip("Runs SO Validator to cross reference SO's in assets vs. those in LoadManager.cs arrays. Editor only. Slow")]
    public bool isValidateSO;
    [Tooltip("Autoruns game for 'x' number of turns with current player/AI settings. Leave at Zero for normal operation")]
    public int autoRunTurns = 0;

   
    
    private List<StartMethod> listOfStartMethods = new List<StartMethod>();
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
        if (seed == 0)
        { seed = (int)DateTime.Now.Ticks & 0x0000FFFF; }
        Debug.Log("Seed: " + seed);
        Random.InitState(seed);
        //Get component references
        startScript = GetComponent<StartManager>();
        levelScript = GetComponent<LevelManager>();
        preloadScript = GetComponent<PreLoadManager>();
        loadScript = GetComponent<LoadManager>();
        metaScript = GetComponent<MetaManager>();
        dataScript = GetComponent<DataManager>();
        guiScript = GetComponent<GUIManager>();
        globalScript = GetComponent<GlobalManager>();
        scenarioScript = GetComponent<ScenarioManager>();
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
        Debug.Assert(metaScript != null, "Invalid metaScript (Null)");
        Debug.Assert(dataScript != null, "Invalid dataScript (Null)");
        Debug.Assert(guiScript != null, "Invalid guiScript (Null)");
        Debug.Assert(globalScript != null, "Invalid globalScript (Null)");
        Debug.Assert(scenarioScript != null, "Invalid scenarioScript (Null)");
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
        //lock mouse to prevent mouseover events occuring prior to full initialisation
        Cursor.lockState = CursorLockMode.Locked;
        //need testManager in order to access performance timer
        testScript.Initialise();
        //start sequence
        if (isPerformanceLog == false)
        { InitialiseGame(); }
        else
        { InitialiseGameWithPerformanceMonitoring(); }
        //do a final redraw before game start
        nodeScript.NodeRedraw = true;
        //free mouse for normal operations
        Cursor.lockState = CursorLockMode.None;
        //colour scheme
        optionScript.ColourOption = ColourScheme.Normal;
        //AutoRun
        if (autoRunTurns > 0)
        { GameManager.instance.turnScript.SetAutoRun(autoRunTurns); }
    }
    #endregion

    #region StartUp Sequence
    /// <summary>
    /// set up list of delegates ready for initialisation (do this because there are two version of initialisation, performance monitoring ON or OFF)
    /// </summary>
    private void InitialiseStartSequence()
    {
        StartMethod startMethod = new StartMethod();
        //PreLoad Manager 
        startMethod.handler = GameManager.instance.preloadScript.Initialise;
        startMethod.className = "PreLoadManager";
        listOfStartMethods.Add(startMethod);
        //Load Manager -> InitialiseStart
        startMethod.handler = GameManager.instance.loadScript.InitialiseStart;
        startMethod.className = "LoadManager";
        listOfStartMethods.Add(startMethod);
#if (UNITY_EDITOR)
        //SO Checker (After LoadManager.cs / Optional)
        if (isValidateSO == true)
        {
            startMethod.handler = GameManager.instance.validateScript.ValidateSO;
            startMethod.className = "ValidationManager (SO)";
            listOfStartMethods.Add(startMethod);
        }
#endif
        //Global Manager -> immediately after dataScript.InitialiseStart and before dataScript.InitialiseEarly 
        startMethod.handler = GameManager.instance.globalScript.Initialise;
        startMethod.className = "GlobalManager";
        listOfStartMethods.Add(startMethod);
        //Colour Manager
        startMethod.handler = GameManager.instance.colourScript.Initialise;
        startMethod.className = "ColourManager";
        listOfStartMethods.Add(startMethod);
        //Message Manager -> after globalScript and before a lot of other stuff (pre-start messages need to be initialised for side)
        startMethod.handler = GameManager.instance.messageScript.InitialiseEarly;
        startMethod.className = "MessageManager";
        listOfStartMethods.Add(startMethod);
        //ItemData Manager
        startMethod.handler = GameManager.instance.itemDataScript.Initialise;
        startMethod.className = "ItemDataManager";
        listOfStartMethods.Add(startMethod);
        //Tooltip Node
        startMethod.handler = GameManager.instance.tooltipNodeScript.Initialise;
        startMethod.className = "TooltipNode";
        listOfStartMethods.Add(startMethod);
        //Side Manager
        startMethod.handler = GameManager.instance.sideScript.Initialise;
        startMethod.className = "SideManager";
        listOfStartMethods.Add(startMethod);
        //Actor Manager -> PreInitialise
        startMethod.handler = GameManager.instance.actorScript.PreInitialiseActors;
        startMethod.className = "ActorManager";
        listOfStartMethods.Add(startMethod);
        //Load Manager -> InitialiseEarly
        startMethod.handler = GameManager.instance.loadScript.InitialiseEarly;
        startMethod.className = "LoadManager";
        listOfStartMethods.Add(startMethod);
        //GUI Manager -> before any actor scripts (acttrScript.PreInitialiseActors is O.K to be earlier)
        startMethod.handler = GameManager.instance.guiScript.Initialise;
        startMethod.className = "GUIManager";
        listOfStartMethods.Add(startMethod);

        /*//City Manager InitialiseEarly -> before levelScript
        startMethod.handler = GameManager.instance.cityScript.InitialiseEarly;
        startMethod.className = "CityManager";
        listOfStartMethods.Add(startMethod);*/

        //Scenario Manager InitialiseEarly -> before levelScript
        startMethod.handler = GameManager.instance.scenarioScript.InitialiseEarly;
        startMethod.className = "ScenarioManager Early";
        listOfStartMethods.Add(startMethod);
        //Objective Manager
        startMethod.handler = GameManager.instance.objectiveScript.Initialise;
        startMethod.className = "ObjectiveManager";
        listOfStartMethods.Add(startMethod);
        //Actor Panel UI -> before actorScript.Initialise
        startMethod.handler = GameManager.instance.actorPanelScript.Initialise;
        startMethod.className = "ActorPanelUI";
        listOfStartMethods.Add(startMethod);
        //Base Panel UI -> before CityInfo.InitialiseLate
        startMethod.handler = GameManager.instance.basePanelScript.Initialise;
        startMethod.className = "BasePanelUI";
        listOfStartMethods.Add(startMethod);
        //Actor Manager -> before DataManager.cs & NodeManager.cs
        startMethod.handler = GameManager.instance.actorScript.Initialise;
        startMethod.className = "ActorManager";
        listOfStartMethods.Add(startMethod);
        //Level Manager
        startMethod.handler = GameManager.instance.levelScript.Initialise;
        startMethod.className = "LevelManager";
        listOfStartMethods.Add(startMethod);
        //Help Manager -> before LoadManager.cs InitialiseLate
        startMethod.handler = GameManager.instance.helpScript.Initialise;
        startMethod.className = "HelpManager";
        listOfStartMethods.Add(startMethod);
        //Load Manager -> InitialiseLate -> immediately after levelScript.Initialise
        startMethod.handler = GameManager.instance.loadScript.InitialiseLate;
        startMethod.className = "LoadManager";
        listOfStartMethods.Add(startMethod);
        //Data Manager -> InitialiseLate -> immediately after LoadScript.Initialise
        startMethod.handler = GameManager.instance.dataScript.InitialiseLate;
        startMethod.className = "DataManager";
        listOfStartMethods.Add(startMethod);

        /*//City Manager -> InitialiseLate -> after levelScript.Initialise
        startMethod.handler = GameManager.instance.cityScript.InitialiseLate;
        startMethod.className = "CityManager";
        listOfStartMethods.Add(startMethod);*/

        //Dijkstra Manager -> Initialise -> after dataScript & LevelScript
        startMethod.handler = GameManager.instance.dijkstraScript.Initialise;
        startMethod.className = "DijkstraManager";
        listOfStartMethods.Add(startMethod);
        //Faction Manager
        startMethod.handler = GameManager.instance.factionScript.Initialise;
        startMethod.className = "FactionManager";
        listOfStartMethods.Add(startMethod);
        //AI Manager -> after factionScript, before ScenarioManager -> InitialiseLate
        startMethod.handler = GameManager.instance.aiScript.Initialise;
        startMethod.className = "AIManager";
        listOfStartMethods.Add(startMethod);

        //Scenario Manager -> InitialiseLate -> after levelScript.Initialise
        startMethod.handler = GameManager.instance.scenarioScript.InitialiseLate;
        startMethod.className = "ScenarioManager Late";
        listOfStartMethods.Add(startMethod);
        //Message Manager -> InitialseLate -> after ScenarioManager
        startMethod.handler = GameManager.instance.messageScript.InitialiseLate;
        startMethod.className = "MessageManager";
        listOfStartMethods.Add(startMethod);

        //Secret Manager -> after dataScript and before playerScript
        startMethod.handler = GameManager.instance.secretScript.Initialise;
        startMethod.className = "SecretManager";
        listOfStartMethods.Add(startMethod);

        //Input Manager
        startMethod.handler = GameManager.instance.inputScript.Initialise;
        startMethod.className = "InputManager";
        listOfStartMethods.Add(startMethod);
        //Meta Manager
        startMethod.handler = GameManager.instance.metaScript.Initialise;
        startMethod.className = "MetaManager";
        listOfStartMethods.Add(startMethod);
        //Contact Manager -> before ActorManager.cs InitialiseLate
        startMethod.handler = GameManager.instance.contactScript.Initialise;
        startMethod.className = "ContactManager";
        listOfStartMethods.Add(startMethod);
        //Actor Manager -> after DataManager.cs & before NodeManager.cs
        startMethod.handler = GameManager.instance.actorScript.InitialiseLate;
        startMethod.className = "ActorManager";
        listOfStartMethods.Add(startMethod);
        //Action Manager
        startMethod.handler = GameManager.instance.actionScript.Initialise;
        startMethod.className = "ActionManager";
        listOfStartMethods.Add(startMethod);

        /*//Target Manager
        startMethod.handler = GameManager.instance.targetScript.Initialise;
        startMethod.className = "TargetManager";
        listOfStartMethods.Add(startMethod);*/

        //Node Manager
        startMethod.handler = GameManager.instance.nodeScript.Initialise;
        startMethod.className = "NodeManager";
        listOfStartMethods.Add(startMethod);
        //Effect Manager -> after nodeScript
        startMethod.handler = GameManager.instance.effectScript.Initialise;
        startMethod.className = "EffectManager";
        listOfStartMethods.Add(startMethod);
        //Team Manager
        startMethod.handler = GameManager.instance.teamScript.Initialise;
        startMethod.className = "TeamManager";
        listOfStartMethods.Add(startMethod);
        //Turn Manager
        startMethod.handler = GameManager.instance.turnScript.Initialise;
        startMethod.className = "TurnManager";
        listOfStartMethods.Add(startMethod);
        //Gear Manager
        startMethod.handler = GameManager.instance.gearScript.Initialise;
        startMethod.className = "GearManager";
        listOfStartMethods.Add(startMethod);
        //Team Picker
        startMethod.handler = GameManager.instance.teamPickerScript.Initialise;
        startMethod.className = "TeamPickerUI";
        listOfStartMethods.Add(startMethod);

        /*//Dice Manager
        diceScript.Initialise();*/

        //Capture Manager
        startMethod.handler = GameManager.instance.captureScript.Initialise;
        startMethod.className = "CaptureManager";
        listOfStartMethods.Add(startMethod);
        //Authority Manager
        startMethod.handler = GameManager.instance.authorityScript.Initialise;
        startMethod.className = "AuthorityManager";
        listOfStartMethods.Add(startMethod);
        //Player Manager
        startMethod.handler = GameManager.instance.playerScript.Initialise;
        startMethod.className = "PlayerManager";
        listOfStartMethods.Add(startMethod);
        //Debug Graphics Manager
        startMethod.handler = GameManager.instance.debugGraphicsScript.Initialise;
        startMethod.className = "DebugGraphicManager";
        listOfStartMethods.Add(startMethod);
        //Trait Manager
        startMethod.handler = GameManager.instance.traitScript.Initialise;
        startMethod.className = "TraitManager";
        listOfStartMethods.Add(startMethod);
        //Connection Manager
        startMethod.handler = GameManager.instance.connScript.Initialise;
        startMethod.className = "ConnectionManager";
        listOfStartMethods.Add(startMethod);

        /*//Mission Manager
        startMethod.handler = GameManager.instance.missionScript.Initialise;
        startMethod.className = "MissionManager";
        listOfStartMethods.Add(startMethod);*/

        //City Info UI
        startMethod.handler = GameManager.instance.cityInfoScript.Initialise;
        startMethod.className = "CityInfoUI";
        listOfStartMethods.Add(startMethod);
        //Main Info UI
        startMethod.handler = GameManager.instance.mainInfoScript.Initialise;
        startMethod.className = "MainInfoUI";
        listOfStartMethods.Add(startMethod);
        //AI Display UI 
        startMethod.handler = GameManager.instance.aiDisplayScript.Initialise;
        startMethod.className = "AIDisplayUI";
        listOfStartMethods.Add(startMethod);
        //AI SideTab UI -> after AI Display UI
        startMethod.handler = GameManager.instance.aiSideTabScript.Initialise;
        startMethod.className = "AISideTabUI";
        listOfStartMethods.Add(startMethod);
        //Widget Top UI
        startMethod.handler = GameManager.instance.widgetTopScript.Initialise;
        startMethod.className = "WidgetTopUI";
        listOfStartMethods.Add(startMethod);
        //data Validation (Last / Optional)
        if (isValidateData == true)
        {
            startMethod.handler = GameManager.instance.validateScript.Initialise;
            startMethod.className = "ValidationManager (Content)";
            listOfStartMethods.Add(startMethod);
        }
    }
    #endregion

    #region Performance Monitoring or not
    /// <summary>
    /// Initialise game start sequence with no performance monitoring
    /// </summary>
    private void InitialiseGame()
    {
        //run each method via delegates in their preset order
        if (listOfStartMethods != null)
        {
            foreach (StartMethod method in listOfStartMethods)
            {
                if (method.handler != null)
                { method.handler(); }
                else { Debug.LogErrorFormat("Invalid startMethod handler for {0}", method.className); }
            }
        }
        else { Debug.LogError("Invalid listOfStartMethods (Null)"); }
    }

    /// <summary>
    /// Initialise game start sequence with Performance Monitoring
    /// </summary>
    private void InitialiseGameWithPerformanceMonitoring()
    {
        //run each method via delegates in their preset order
        if (listOfStartMethods != null)
        {
            //start timer tally to get an overall performance time
            GameManager.instance.testScript.TimerTallyStart();
            foreach (StartMethod method in listOfStartMethods)
            {
                if (method.handler != null)
                {
                    GameManager.instance.testScript.StartTimer();
                    method.handler();
                    long elapsed = GameManager.instance.testScript.StopTimer();
                    Debug.LogFormat("[Per] {0} -> {1}: {2} ms{3}", method.className, method.handler.Method.Name, elapsed, "\n");
                }
                else { Debug.LogErrorFormat("Invalid startMethod handler for {0}", method.className); }
            }
            long totalTime = GameManager.instance.testScript.TimerTallyStop();
            Debug.LogFormat("[Per] GameManager.cs -> InitialiseGameWithPerfomanceMonitoring: TOTAL TIME {0} ms{1}", totalTime, "\n");
        }
        else { Debug.LogError("Invalid listOfStartMethods (Null)"); }
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

    //place methods above here
}
