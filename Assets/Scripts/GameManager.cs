using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ConnectionType { Neutral, HighSec, MedSec, LowSec, Count };
public enum NodeType { Normal, Highlight, Active, Player, Count };
public enum Side { Authority, Rebel };
public enum Comparison { None, LessThan, GreaterThan, EqualTo}
public enum EffectType { None, NodeStability, NodeSecurity, NodeSupport, NumRecruits, NumTeams, NumTracers, NumGear, TargetInfo}     //Action Effects

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
    [HideInInspector] public PlayerManager playerScript;              //Player Manager
    [HideInInspector] public NodeManager nodeScript;                  //Node Manager
    [HideInInspector] public ConnectionManager connScript;            //Connection Manager
    [HideInInspector] public ColourManager colourScript;              //Colour Manager
    [HideInInspector] public TooltipNode tooltipNodeScript;           //node Tool tip static instance
    [HideInInspector] public TooltipActor tooltipActorScript;         //actor tool tip static instance
    [HideInInspector] public TooltipGeneric tooltipGenericScript;     //generic tool tip static instance
    [HideInInspector] public TickerTextScroller tickerScript;         //Ticker Text Scroller
    [HideInInspector] public ModalActionMenu actionMenuScript;        //Modal Action Menu (node)

    public float showSplashTimeout = 2.0f;

    [HideInInspector]public Side playerSide;                        //what side is the player?
    [HideInInspector] public bool isBlocked;                        //set True to selectively block raycasts onto game scene, eg. mouseover tooltips, etc.
                                                                    //to block use -> 'if (isBlocked == false)' in OnMouseDown/Over/Exit etc.
    [Tooltip("Leave as default 0 for random")]
    public int seed = 0;                                            //random seed

    private bool allowQuitting = false;

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
        dataScript = GetComponent<DataManager>();
        guiScript = GetComponent<GUIManager>();
        actorScript = GetComponent<ActorManager>();
        actionScript = GetComponent<ActionManager>();
        playerScript = GetComponent<PlayerManager>();
        nodeScript = GetComponent<NodeManager>();
        connScript = GetComponent<ConnectionManager>();
        colourScript = GetComponent<ColourManager>();
        tooltipScript = GetComponent<TooltipManager>();
        sideScript = GetComponent<SideManager>();
        //Get static references
        tooltipNodeScript = TooltipNode.Instance();
        tooltipActorScript = TooltipActor.Instance();
        tooltipGenericScript = TooltipGeneric.Instance();
        tickerScript = TickerTextScroller.Instance();
        actionMenuScript = ModalActionMenu.Instance();
        //setup game
        InitialiseGame();
        //set side
        playerSide = Side.Rebel;
        tooltipNodeScript.InitialiseTooltip(playerSide);
        tooltipActorScript.InitialiseTooltip(playerSide);
        //make sure raycasts are active, eg. node tooltips
        isBlocked = false;
        //sets this to not be destroyed when reloading a scene
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Initialise the Game
    /// </summary>
    private void InitialiseGame()
    {
        dataScript.Initialise();
        actorScript.InitialiseActors(actorScript.numOfActorsTotal);
        levelScript.SetUpLevel();
        guiScript.Initialise(actorScript.GetActors());
    }


    private void Update()
    {
        //redraw any Nodes where required
        if (nodeScript.nodeRedraw == true)
        { levelScript.RedrawNodes(); }

        //application will quit on 'X'
        if (Input.GetKeyDown(KeyCode.X))
        {
            Quit();
        }
    }


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


    /// <summary>
    /// Debug function to swap sides mid-game
    /// </summary>
    public void SwapSides()
    {
        switch (playerSide)
        {
            case Side.Authority:
                playerSide = Side.Rebel;
                break;
            case Side.Rebel:
                playerSide = Side.Authority;
                break;
        }
        //all other related side stuff
        sideScript.SwapSides(playerSide);
    }







    //place methods above here
}
