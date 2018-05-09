using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using gameAPI;







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
    [HideInInspector] public FactionManager factionScript;            //Faction Manager
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
    [HideInInspector] public ModalDiceUI diceScript;                  //Modal Dice UI window
    [HideInInspector] public ModalGUI modalGUIScropt;                 //Modal GUI 
    [HideInInspector] public AlertUI alertScript;                     //Alert UI text display
    [HideInInspector] public WidgetTopUI widgetTopScript;             //Widget Top UI
    [HideInInspector] public CityInfoUI cityInfoScript;               //City Info UI
    [HideInInspector] public DebugGraphics debugGraphicsScript;       //Debug only Graphics

                                                                   
    [Tooltip("Leave as default 0 for random")]
    public int seed = 0;                                            //random seed
    
    

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
        factionScript = GetComponent<FactionManager>();
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
        diceScript = ModalDiceUI.Instance();
        modalGUIScropt = ModalGUI.Instance();
        widgetTopScript = WidgetTopUI.Instance();
        cityInfoScript = CityInfoUI.Instance();
        alertScript = AlertUI.Instance();
        debugGraphicsScript = DebugGraphics.Instance();
        //sets this to not be destroyed when reloading a scene
        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        //setup game
        InitialiseGame();
        //colour scheme
        optionScript.ColourOption = ColourScheme.Normal;
    }

    /// <summary>
    /// Initialise the Game
    /// </summary>
    private void InitialiseGame()
    {
        importScript.InitialiseStart();   //must be first
        globalScript.Initialise();      //must be immediately after dataScript.InitialiseStart and before dataScript.InitialiseEarly  
        messageScript.Initialise();     //must be after globalScript and before a lot of other stuff (pre-start messages need to be initialised for side)
        tooltipNodeScript.Initialise();
        sideScript.Initialise();
        actorScript.PreInitialiseActors();
        importScript.InitialiseEarly();
        guiScript.Initialise();         //must be before any actor scripts (acttrScript.PreInitialiseActors is O.K to be earlier)
        factionScript.Initialise();
        cityScript.Initialise();
        objectiveScript.Initialise();
        actorScript.Initialise();
        levelScript.Initialise();
        dataScript.InitialiseLate();      //must be immediately after levelScript.Initialise
        importScript.InitialiseLate();    //must be immediately after levelScript.Initialise
        guiScript.InitialiseLate();
        inputScript.Initialise();
        metaScript.Initialise();
        dataScript.InitialiseFinal();   //must be after metaScript.Initialise
        actionScript.Initialise();      
        targetScript.Initialise();
        playerScript.Initialise();
        nodeScript.Initialise();
        effectScript.Initialise();      //after nodeScript
        teamScript.Initialise();
        turnScript.Initialise();
        gearScript.Initialise();
        teamPickerScript.Initialise();
        diceScript.Initialise();
        aiScript.Initialise();          //after factionScript
        captureScript.Initialise();
        rebelScript.Initialise();
        authorityScript.Initialise();
        debugGraphicsScript.Initialise();
        widgetTopScript.Initialise();
        //do a final redraw before game start
        nodeScript.NodeRedraw = true;

        //TO DO -> tap into game options chosen by player and start game as correct side or AI vs. AI
    }


    private void Update()
    {
        //redraw any Nodes where required
        if (nodeScript.NodeRedraw == true)
        { EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.Redraw); }
        //Handle Game Input
        if (Input.anyKey == true)
        { inputScript.ProcessInput(); }
    }



    //place methods above here
}
