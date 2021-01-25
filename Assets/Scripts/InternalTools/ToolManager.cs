using System.Collections.Generic;
using UnityEngine;
using toolsAPI;
using UnityEngine.SceneManagement;
using System;



#if (UNITY_EDITOR)
/// <summary>
/// Internal tool Manager -> handles functionality of toolUI as well
/// </summary>
public class ToolManager : MonoBehaviour
{
    #region Variables
    //
    // - - - Variables
    //
    private float mouseWheelInput;                      //used for detecting mouse wheel input in the Update method
    private Scene scene;

    #endregion

    #region Components
    public static ToolManager i = null;                                 //static instance of toolManager which allows it to be accessed by any other script

    [HideInInspector] public AdventureManager adventureScript;
    [HideInInspector] public ActorPoolManager actorScript;
    [HideInInspector] public ToolDataManager toolDataScript;
    [HideInInspector] public ToolFileManager toolFileScript;
    [HideInInspector] public ToolInput toolInputScript;
    [HideInInspector] public ToolDetails toolDetailScript;
    [HideInInspector] public JointManager jointScript;
    //GUI
    [HideInInspector] public AdventureUI adventureUIScript;
    [HideInInspector] public ActorPoolUI actorPoolUIScript;
    [HideInInspector] public ToolUI toolUIScript;
    #endregion

    #region Awake
    private void Awake()
    {
        scene = SceneManager.GetActiveScene();
        //only run if Internal tools scene
        if (scene.name.Equals("Internal_Tools", StringComparison.Ordinal) == true)
        {
            //check if instance already exists
            if (i == null)
            { i = this; }
            //if instance already exists and it's not this
            else if (i != this)
            {
                //Then destroy this in order to reinforce the singleton pattern (can only ever be one instance of toolManager)
                Destroy(gameObject);
            }
            //components
            adventureScript = GetComponent<AdventureManager>();
            actorScript = GetComponent<ActorPoolManager>();
            toolDataScript = GetComponent<ToolDataManager>();
            toolFileScript = GetComponent<ToolFileManager>();
            toolInputScript = GetComponent<ToolInput>();
            toolDetailScript = GetComponent<ToolDetails>();
            jointScript = GetComponent<JointManager>();
            //gui
            adventureUIScript = AdventureUI.Instance();
            actorPoolUIScript = ActorPoolUI.Instance();
            toolUIScript = ToolUI.Instance();
            //error Check
            Debug.Assert(adventureScript != null, "Invalid adventureScript (Null)");
            Debug.Assert(actorScript != null, "Invalid actorScript (Null)");
            Debug.Assert(toolDataScript != null, "Invalid toolDataScript (Null)");
            Debug.Assert(adventureUIScript != null, "Invalid adventureUIScript (Null)");
            Debug.Assert(actorPoolUIScript != null, "Invalid actorPoolUIScript (Null)");
            Debug.Assert(toolUIScript != null, "Invalid toolUIScript (Null)");
            Debug.Assert(toolInputScript != null, "Invalid toolInputScript (Null)");
            Debug.Assert(toolDetailScript != null, "Invalid toolDetailScript (Null)");
            Debug.Assert(jointScript != null, "Invalid jointScript (Null)");
        }
    }
    #endregion

    #region Start
    public void Start()
    {
        //only run if internal tools scene
        if (scene.name.Equals("Internal_Tools", StringComparison.Ordinal) == true)
        {
            InitialiseAll();
        }
    }
    #endregion

    #region InitialiseAll
    /// <summary>
    /// Sequenced Initialisation
    /// </summary>
    private void InitialiseAll()
    {
        jointScript.Initialise();
        toolUIScript.Initialise();
        toolFileScript.Initialise();
        toolDetailScript.Initialise();
        adventureScript.Initialise();
        actorScript.Initialise();
        adventureUIScript.Initialise();
        actorPoolUIScript.Initialise();
    }
    #endregion

    #region Update
    /// <summary>
    /// Only update in the entire code base -> handles redraws and input
    /// </summary>
    private void Update()
    {
        //only run if there is a valid scene (and it's internal tools)
        if (scene.isLoaded == true)
        {
            //only run if internal tools scene
            if (scene.name.Equals("Internal_Tools", StringComparison.Ordinal) == true)
            {
                mouseWheelInput = 0;
                //get any mouse wheel input (restricts max value) and pass as a parameter as Input.anyKeyDown won't pick up mouse wheel input)
                mouseWheelInput += Input.GetAxis("Mouse ScrollWheel");

                //Handle Game Input
                if (mouseWheelInput != 0)
                { toolInputScript.ProcessMouseWheelInput(mouseWheelInput); }
                else if (Input.anyKeyDown == true)
                { toolInputScript.ProcessKeyInput(); }
            }
        }
    }
    #endregion





}
#endif

