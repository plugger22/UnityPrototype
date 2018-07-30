﻿using System;
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
    #region variables
    public static GameManager instance = null;      //static instance of GameManager which allows it to be accessed by any other script
    [HideInInspector] public LevelManager levelScript;                //Level Manager
    [HideInInspector] public ImportManager importScript;              //Import Manager
    [HideInInspector] public MetaManager metaScript;                  //Meta Manager
    [HideInInspector] public DataManager dataScript;                  //Data Manager
    [HideInInspector] public GUIManager guiScript;                    //GUI Manager
    [HideInInspector] public GlobalManager globalScript;              //Global Manager
    [HideInInspector] public TooltipManager tooltipScript;            //Tooltip Manager
    [HideInInspector] public ActorManager actorScript;                //Actor Manager 
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
    [HideInInspector] public TeamManager teamScript;                  //Team Manager
    [HideInInspector] public GearManager gearScript;                  //Gear Manager
    [HideInInspector] public CaptureManager captureScript;            //Capture Manager
    [HideInInspector] public AIManager aiScript;                      //AI Manager
    [HideInInspector] public ResistanceManager rebelScript;           //Resistance Manager
    [HideInInspector] public AuthorityManager authorityScript;        //Authority Manager
    [HideInInspector] public MessageManager messageScript;            //Message Manager
    [HideInInspector] public ConnectionManager connScript;            //Connection Manager
    [HideInInspector] public ColourManager colourScript;              //Colour Manager
    [HideInInspector] public TestManager testScript;                  //Test Manager
    [HideInInspector] public TooltipNode tooltipNodeScript;           //node tooltip static instance
    [HideInInspector] public TooltipConnection tooltipConnScript;     //connection tooltip static instance
    [HideInInspector] public TooltipActor tooltipActorScript;         //actor tooltip static instance
    [HideInInspector] public TooltipPlayer tooltipPlayerScript;       //player tooltip static instance
    [HideInInspector] public TooltipGeneric tooltipGenericScript;     //generic tooltip static instance
    [HideInInspector] public TickerTextScroller tickerScript;         //Ticker Text Scroller
    [HideInInspector] public ModalActionMenu actionMenuScript;        //Modal Action Menu (node)
    [HideInInspector] public ModalOutcome outcomeScript;              //Modal Outcome window
    [HideInInspector] public ModalTeamPicker teamPickerScript;        //Modal Team Picker window
    [HideInInspector] public ModalGenericPicker genericPickerScript;  //Modal Generic Picker window
    [HideInInspector] public ModalInventoryUI inventoryScript;        //Modal InventoryUI window
    /*[HideInInspector] public ModalDiceUI diceScript;                  //Modal Dice UI window*/
    [HideInInspector] public ModalGUI modalGUIScropt;                 //Modal GUI 
    [HideInInspector] public AlertUI alertScript;                     //Alert UI text display
    [HideInInspector] public WidgetTopUI widgetTopScript;             //Widget Top UI
    [HideInInspector] public CityInfoUI cityInfoScript;               //City Info UI
    [HideInInspector] public MainInfoUI mainInfoScript;               //Main Info UI
    [HideInInspector] public AIDisplayUI aiDisplayScript;             //AI Display UI
    [HideInInspector] public AISideTabUI aiSideTabScript;             //AI SideTab UI
    [HideInInspector] public ActorPanelUI actorPanelScript;           //Actor Panel UI
    [HideInInspector] public DebugGraphics debugGraphicsScript;       //Debug only Graphics
    

                                                                   
    [Tooltip("Leave as default 0 for random")]
    public int seed = 0;                                            //random seed

    [Tooltip("Switch ON to get a performance log of initialisation ")]
    public bool isPerformanceLog;

    [HideInInspector] public WinState win = WinState.None;          //set if somebody has won
    
    private List<StartMethod> listOfStartMethods = new List<StartMethod>();
    

    #endregion


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
        levelScript = GetComponent<LevelManager>();
        importScript = GetComponent<ImportManager>();
        metaScript = GetComponent<MetaManager>();
        dataScript = GetComponent<DataManager>();
        guiScript = GetComponent<GUIManager>();
        globalScript = GetComponent<GlobalManager>();
        actorScript = GetComponent<ActorManager>();
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
        teamScript = GetComponent<TeamManager>();
        gearScript = GetComponent<GearManager>();
        messageScript = GetComponent<MessageManager>();
        connScript = GetComponent<ConnectionManager>();
        colourScript = GetComponent<ColourManager>();
        testScript = GetComponent<TestManager>();
        tooltipScript = GetComponent<TooltipManager>();
        sideScript = GetComponent<SideManager>();
        helpScript = GetComponent<HelpManager>();
        turnScript = GetComponent<TurnManager>();
        inputScript = GetComponent<InputManager>();
        captureScript = GetComponent<CaptureManager>();
        aiScript = GetComponent<AIManager>();
        rebelScript = GetComponent<ResistanceManager>();
        authorityScript = GetComponent<AuthorityManager>();
        //Get UI static references -> from PanelManager
        tooltipNodeScript = TooltipNode.Instance();
        tooltipConnScript = TooltipConnection.Instance();
        tooltipActorScript = TooltipActor.Instance();
        tooltipPlayerScript = TooltipPlayer.Instance();
        tooltipGenericScript = TooltipGeneric.Instance();
        tickerScript = TickerTextScroller.Instance();
        actionMenuScript = ModalActionMenu.Instance();
        outcomeScript = ModalOutcome.Instance();
        teamPickerScript = ModalTeamPicker.Instance();
        genericPickerScript = ModalGenericPicker.Instance();
        inventoryScript = ModalInventoryUI.Instance();
        /*diceScript = ModalDiceUI.Instance();*/
        modalGUIScropt = ModalGUI.Instance();
        widgetTopScript = WidgetTopUI.Instance();
        cityInfoScript = CityInfoUI.Instance();
        mainInfoScript = MainInfoUI.Instance();
        aiDisplayScript = AIDisplayUI.Instance();
        aiSideTabScript = AISideTabUI.Instance();
        alertScript = AlertUI.Instance();
        actorPanelScript = ActorPanelUI.Instance();
        debugGraphicsScript = DebugGraphics.Instance();
        //set up list of delegates
        InitialiseStartSequence();
        //sets this to not be destroyed when reloading a scene
        DontDestroyOnLoad(gameObject);
    }


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
    }
    
    /// <summary>
    /// set up list of delegates ready for initialisation (do this because there are two version of initialisation, performance monitoring ON or OFF)
    /// </summary>
    private void InitialiseStartSequence()
    {
        StartMethod startMethod = new StartMethod();
        //Import Manager -> InitialiseStart
        startMethod.handler = GameManager.instance.importScript.InitialiseStart;
        startMethod.className = "ImportManager";
        listOfStartMethods.Add(startMethod);
        //Global Manager -> immediately after dataScript.InitialiseStart and before dataScript.InitialiseEarly 
        startMethod.handler = GameManager.instance.globalScript.Initialise;
        startMethod.className = "GlobalManager";
        listOfStartMethods.Add(startMethod);
        //Colour Manager
        startMethod.handler = GameManager.instance.colourScript.Initialise;
        startMethod.className = "ColourManager";
        listOfStartMethods.Add(startMethod);
        //Message Manager -> after globalScript and before a lot of other stuff (pre-start messages need to be initialised for side)
        startMethod.handler = GameManager.instance.messageScript.Initialise;
        startMethod.className = "MessageManager";
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
        //Import Manager -> InitialiseEarly
        startMethod.handler = GameManager.instance.importScript.InitialiseEarly;
        startMethod.className = "ImportManager";
        listOfStartMethods.Add(startMethod);
        //GUI Manager -> before any actor scripts (acttrScript.PreInitialiseActors is O.K to be earlier)
        startMethod.handler = GameManager.instance.guiScript.Initialise;
        startMethod.className = "GUIManager";
        listOfStartMethods.Add(startMethod);
        //City Manager -> before levelScript
        startMethod.handler = GameManager.instance.cityScript.InitialiseEarly;
        startMethod.className = "CityManager";
        listOfStartMethods.Add(startMethod);
        //Objective Manager
        startMethod.handler = GameManager.instance.objectiveScript.Initialise;
        startMethod.className = "ObjectiveManager";
        listOfStartMethods.Add(startMethod);
        //Actor Panel UI -> before actorScript.Initialise
        startMethod.handler = GameManager.instance.actorPanelScript.Initialise;
        startMethod.className = "ActorPanelUI";
        listOfStartMethods.Add(startMethod);
        //Actor Manager
        startMethod.handler = GameManager.instance.actorScript.Initialise;
        startMethod.className = "ActorManager";
        listOfStartMethods.Add(startMethod);
        //Level Manager
        startMethod.handler = GameManager.instance.levelScript.Initialise;
        startMethod.className = "LevelManager";
        listOfStartMethods.Add(startMethod);
        //Data Manager -> InitialiseLate -> immediately after levelScript.Initialise
        startMethod.handler = GameManager.instance.dataScript.InitialiseLate;
        startMethod.className = "DataManager";
        listOfStartMethods.Add(startMethod);
        //Import Manager -> InitialiseLate -> immediately after levelScript.Initialise
        startMethod.handler = GameManager.instance.importScript.InitialiseLate;
        startMethod.className = "ImportManager";
        listOfStartMethods.Add(startMethod);
        //City Manager -> InitialiseLate -> after levelScript.Initialise
        startMethod.handler = GameManager.instance.cityScript.InitialiseLate;
        startMethod.className = "CityManager";
        listOfStartMethods.Add(startMethod);
        //Secret Manager -> after dataScript and before playerScript
        startMethod.handler = GameManager.instance.secretScript.Initialise;
        startMethod.className = "SecretManager";
        listOfStartMethods.Add(startMethod);
        //Faction Manager
        startMethod.handler = GameManager.instance.factionScript.Initialise;
        startMethod.className = "FactionManager";
        listOfStartMethods.Add(startMethod);
        //Input Manager
        startMethod.handler = GameManager.instance.inputScript.Initialise;
        startMethod.className = "InputManager";
        listOfStartMethods.Add(startMethod);
        //Meta Manager
        startMethod.handler = GameManager.instance.metaScript.Initialise;
        startMethod.className = "MetaManager";
        listOfStartMethods.Add(startMethod);
        //DataManager -> Initialisefinal -> after metaScript.Initialise
        startMethod.handler = GameManager.instance.dataScript.InitialiseFinal;
        startMethod.className = "DataManager";
        listOfStartMethods.Add(startMethod);
        //Action Manager
        startMethod.handler = GameManager.instance.actionScript.Initialise;
        startMethod.className = "ActionManager";
        listOfStartMethods.Add(startMethod);
        //Target Manager
        startMethod.handler = GameManager.instance.targetScript.Initialise;
        startMethod.className = "TargetManager";
        listOfStartMethods.Add(startMethod);
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

        //AI Manager -> after factionScript
        startMethod.handler = GameManager.instance.aiScript.Initialise;
        startMethod.className = "AIManager";
        listOfStartMethods.Add(startMethod);
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
        //AI Side Tab UI
        startMethod.handler = GameManager.instance.aiSideTabScript.Initialise;
        startMethod.className = "AISideTabUI";
        listOfStartMethods.Add(startMethod);
        //Widget Top UI
        startMethod.handler = GameManager.instance.widgetTopScript.Initialise;
        startMethod.className = "WidgetTopUI";
        listOfStartMethods.Add(startMethod);
    }


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

    /// <summary>
    /// Only update in the entire code base -> handles redraws and input
    /// </summary>
    private void Update()
    {
        //redraw any Nodes where required
        if (nodeScript.NodeRedraw == true)
        { EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.Redraw, "GameManager.cs -> Update"); }
        //Handle Game Input
        if (Input.anyKey == true)
        { inputScript.ProcessInput(); }
    }

    /*private void InitialiseGamePerformanceMonitor()
    {
        //lock mouse to prevent mouseover events occuring prior to full initialisation
        Cursor.lockState = CursorLockMode.Locked;
        //need testManager in order to access timer
        testScript.Initialise();

        StartMethod startMethod = listOfStartMethods[0];
        GameManager.instance.testScript.StartTimer();
        Debug.Assert(startMethod.handler != null, "Invalid startMethod (index 0)");
            startMethod.handler();
            long elapsed = GameManager.instance.testScript.StopTimer();
            Debug.LogFormat("[Per] {0} -> {1}: {2} ms{3}", startMethod.className, startMethod.handler.Method.Name, elapsed, "\n");
        GameManager.instance.testScript.StartTimer();
        globalScript.Initialise();      //must be immediately after dataScript.InitialiseStart and before dataScript.InitialiseEarly 
        Debug.LogFormat("[Per] globalScript -> Initialise: {0} ms{1}", GameManager.instance.testScript.StopTimer(), "\n");

        colourScript.Initialise();
        messageScript.Initialise();     //must be after globalScript and before a lot of other stuff (pre-start messages need to be initialised for side)
        tooltipNodeScript.Initialise();
        sideScript.Initialise();
        actorScript.PreInitialiseActors();
        importScript.InitialiseEarly();
        guiScript.Initialise();             //must be before any actor scripts (acttrScript.PreInitialiseActors is O.K to be earlier)
        cityScript.InitialiseEarly();        //before levelScript
        objectiveScript.Initialise();
        actorPanelScript.Initialise();    //must be before actorScript.Initialise
        actorScript.Initialise();
        levelScript.Initialise();
        dataScript.InitialiseLate();      //must be immediately after levelScript.Initialise
        importScript.InitialiseLate();    //must be immediately after levelScript.Initialise
        cityScript.InitialiseLate();      //must be immediately after levelScript.Initialise
        secretScript.Initialise();        //after dataScript and before playerScript
        factionScript.Initialise();
        inputScript.Initialise();
        metaScript.Initialise();
        dataScript.InitialiseFinal();   //must be after metaScript.Initialise
        actionScript.Initialise();
        targetScript.Initialise();
        nodeScript.Initialise();
        effectScript.Initialise();      //after nodeScript
        teamScript.Initialise();
        turnScript.Initialise();
        gearScript.Initialise();
        teamPickerScript.Initialise();
        //diceScript.Initialise();
        aiScript.Initialise();          //after factionScript
        captureScript.Initialise();
        authorityScript.Initialise();
        playerScript.Initialise();
        debugGraphicsScript.Initialise();
        traitScript.Initialise();
        connScript.Initialise();
        cityInfoScript.Initialise();
        aiDisplayScript.Initialise();
        aiSideTabScript.Initialise();
        widgetTopScript.Initialise();
        //do a final redraw before game start
        nodeScript.NodeRedraw = true;
        //free mouse for normal operations
        Cursor.lockState = CursorLockMode.None;
    }*/

    /*/// <summary>
    /// Initialise the Game
    /// </summary>
    private void InitialiseGame()
    {
        //lock mouse to prevent mouseover events occuring prior to full initialisation
        Cursor.lockState = CursorLockMode.Locked;
        importScript.InitialiseStart();   //must be first
        globalScript.Initialise();      //must be immediately after dataScript.InitialiseStart and before dataScript.InitialiseEarly 
        colourScript.Initialise();      
        messageScript.Initialise();     //must be after globalScript and before a lot of other stuff (pre-start messages need to be initialised for side)
        tooltipNodeScript.Initialise();
        sideScript.Initialise();
        actorScript.PreInitialiseActors();
        importScript.InitialiseEarly();
        guiScript.Initialise();             //must be before any actor scripts (acttrScript.PreInitialiseActors is O.K to be earlier)
        cityScript.InitialiseEarly();        //before levelScript
        objectiveScript.Initialise();
        actorPanelScript.Initialise();    //must be before actorScript.Initialise
        actorScript.Initialise();
        levelScript.Initialise();
        dataScript.InitialiseLate();      //must be immediately after levelScript.Initialise
        importScript.InitialiseLate();    //must be immediately after levelScript.Initialise
        cityScript.InitialiseLate();      //must be immediately after levelScript.Initialise
        secretScript.Initialise();        //after dataScript and before playerScript
        factionScript.Initialise();
        inputScript.Initialise();
        metaScript.Initialise();
        dataScript.InitialiseFinal();   //must be after metaScript.Initialise
        actionScript.Initialise();      
        targetScript.Initialise();
        nodeScript.Initialise();
        effectScript.Initialise();      //after nodeScript
        teamScript.Initialise();
        turnScript.Initialise();
        gearScript.Initialise();
        teamPickerScript.Initialise();
        //diceScript.Initialise();
        aiScript.Initialise();          //after factionScript
        captureScript.Initialise();
        authorityScript.Initialise();
        playerScript.Initialise(); 
        debugGraphicsScript.Initialise();
        traitScript.Initialise();
        connScript.Initialise();
        cityInfoScript.Initialise();
        aiDisplayScript.Initialise();
        aiSideTabScript.Initialise();
        widgetTopScript.Initialise();
        //do a final redraw before game start
        nodeScript.NodeRedraw = true;
        //free mouse for normal operations
        Cursor.lockState = CursorLockMode.None;

        //TO DO -> tap into game options chosen by player and start game as correct side or AI vs. AI
    }*/




    //place methods above here
}
