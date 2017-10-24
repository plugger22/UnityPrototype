using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using gameAPI;
#if UNITY_EDITOR
using UnityEditor;
#endif






/// <summary>
/// Main game controller
/// </summary>
[Serializable]
public class GameManager : MonoBehaviour
{
    #region variables
    public static GameManager instance = null;      //static instance of GameManager which allows it to be accessed by any other script
    [HideInInspector] public LevelManager levelScript;                //sets up the levels
    [HideInInspector] public DataManager dataScript;                  //data Manager
    [HideInInspector] public GUIManager guiScript;                    //GUI Manager
    [HideInInspector] public TooltipManager tooltipScript;            //Tooltip Manager
    [HideInInspector] public ActorManager actorScript;                //Actor Manager 
    [HideInInspector] public ActionManager actionScript;              //Action Manager
    [HideInInspector] public SideManager sideScript;                  //Side Manager
    [HideInInspector] public InputManager inputScript;                //Input Manager
    [HideInInspector] public EffectManager effectScript;              //Effect Manager
    [HideInInspector] public TargetManager targetScript;              //Target Manager
    [HideInInspector] public OptionManager optionScript;              //Option Manager
    [HideInInspector] public PlayerManager playerScript;              //Player Manager
    [HideInInspector] public NodeManager nodeScript;                  //Node Manager
    //[HideInInspector] public EventManager eventScript;                //Event Manager
    [HideInInspector] public ConnectionManager connScript;            //Connection Manager
    [HideInInspector] public ColourManager colourScript;              //Colour Manager
    [HideInInspector] public TooltipNode tooltipNodeScript;           //node Tool tip static instance
    [HideInInspector] public TooltipActor tooltipActorScript;         //actor tool tip static instance
    [HideInInspector] public TooltipGeneric tooltipGenericScript;     //generic tool tip static instance
    [HideInInspector] public TickerTextScroller tickerScript;         //Ticker Text Scroller
    [HideInInspector] public ModalActionMenu actionMenuScript;        //Modal Action Menu (node)
    [HideInInspector] public ModalOutcome outcomeScript;              //Modal Outcome window
    [HideInInspector] public AlertUI alertScript;                     //Alert UI text display

    public float showSplashTimeout = 2.0f;

                                                                    
    [Tooltip("Leave as default 0 for random")]
    public int seed = 0;                                            //random seed
    private MetaLevel metaLevel = MetaLevel.None;
    private bool allowQuitting = false;
    private bool isBlocked;                                         //set True to selectively block raycasts onto game scene, eg. mouseover tooltips, etc.
                                                                    //to block use -> 'if (isBlocked == false)' in OnMouseDown/Over/Exit etc.
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
        //MetaLevel
        metaLevel = MetaLevel.City;
        //Get component references
        levelScript = GetComponent<LevelManager>();
        dataScript = GetComponent<DataManager>();
        guiScript = GetComponent<GUIManager>();
        actorScript = GetComponent<ActorManager>();
        actionScript = GetComponent<ActionManager>();
        playerScript = GetComponent<PlayerManager>();
        effectScript = GetComponent<EffectManager>();
        targetScript = GetComponent<TargetManager>();
        optionScript = GetComponent<OptionManager>();
        nodeScript = GetComponent<NodeManager>();
        //eventScript = GetComponent<EventManager>();
        connScript = GetComponent<ConnectionManager>();
        colourScript = GetComponent<ColourManager>();
        tooltipScript = GetComponent<TooltipManager>();
        sideScript = GetComponent<SideManager>();
        inputScript = GetComponent<InputManager>();
        //Get UI static references -> from PanelManager
        tooltipNodeScript = TooltipNode.Instance();
        tooltipActorScript = TooltipActor.Instance();
        tooltipGenericScript = TooltipGeneric.Instance();
        tickerScript = TickerTextScroller.Instance();
        actionMenuScript = ModalActionMenu.Instance();
        outcomeScript = ModalOutcome.Instance();
        alertScript = AlertUI.Instance();
        //make sure raycasts are active, eg. node tooltips
        isBlocked = false;
        //sets this to not be destroyed when reloading a scene
        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        //setup game
        InitialiseGame();
        //set side
        optionScript.PlayerSide = Side.Rebel;
        optionScript.ColourOption = ColourScheme.Normal;

        //register listener
        EventManager.instance.AddListener(EventType.ExitGame, OnEvent);
    }

    /// <summary>
    /// Initialise the Game
    /// </summary>
    private void InitialiseGame()
    {
        //sideScript.Initialise();
        dataScript.InitialiseEarly();
        actorScript.Initialise();
        levelScript.Initialise();
        //immediately after levelScript
        dataScript.InitialiseLate();
        guiScript.Initialise(actorScript.GetActors());
        inputScript.GameState = GameState.Normal;
        actionScript.Initialise();
        effectScript.Initialise();
        targetScript.Initialise();
        playerScript.Initialise();
        nodeScript.Initialise();
        //do a final redraw before game start
        //EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.Reset);
        nodeScript.NodeRedraw = true;
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

    /// <summary>
    /// event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //detect event type
        switch (eventType)
        {
            case EventType.ExitGame:
                Quit();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    public void Blocked(bool isBlocked)
    {
        this.isBlocked = isBlocked;
        Debug.Log(string.Format("GM: Blocked -> {0}{1}", isBlocked, "\n"));
    }

    public bool IsBlocked()
    { return isBlocked; }

    /// <summary>
    /// Quit 
    /// </summary>
    private void Quit()
    {
        //show thank you splash screen before quitting
        if (SceneManager.GetActiveScene().name != "End_Game")
        { StartCoroutine("DelayedQuit"); }
        if (allowQuitting == false)
        { Application.CancelQuit(); }

    }

    //display splash screen for a short while before quitting
    private IEnumerator DelayedQuit()
    {
        SceneManager.LoadScene(1);
        yield return new WaitForSeconds(showSplashTimeout);
        allowQuitting = true;
        //editor quit or application quit
        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    public void OnDisable()
    {
        EventManager.instance.RemoveEvent(EventType.ExitGame);
    }
    //place methods above here
}
