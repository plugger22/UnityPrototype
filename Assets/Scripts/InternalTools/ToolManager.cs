using System.Collections.Generic;
using UnityEngine;
using toolsAPI;



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
    #endregion

    #region Collections
    //
    // - - - Collections
    //
    public Dictionary<string, Story> dictOfStories = new Dictionary<string, Story>();

    #endregion

    #region Components
    public static ToolManager i = null;                                 //static instance of toolManager which allows it to be accessed by any other script

    [HideInInspector] public AdventureManager adventureScript;
    [HideInInspector] public ToolDataManager toolDataScript;
    [HideInInspector] public ToolFileManager toolFileScript;
    [HideInInspector] public ToolInput toolInputScript;
    //GUI
    [HideInInspector] public AdventureUI adventureUIScript;
    [HideInInspector] public ToolUI toolUIScript;
    #endregion

    #region Awake
    private void Awake()
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
        toolDataScript = GetComponent<ToolDataManager>();
        toolFileScript = GetComponent<ToolFileManager>();
        toolInputScript = GetComponent<ToolInput>();
        //gui
        adventureUIScript = AdventureUI.Instance();
        toolUIScript = ToolUI.Instance();
        //error Check
        Debug.Assert(adventureScript != null, "Invalid adventureScript (Null)");
        Debug.Assert(toolDataScript != null, "Invalid toolDataScript (Null)");
        Debug.Assert(adventureUIScript != null, "Invalid adventureUIScript (Null)");
        Debug.Assert(toolUIScript != null, "Invalid toolUIScript (Null)");
        Debug.Assert(toolInputScript != null, "Invalid toolInputScript (Null)");
    }
    #endregion

    #region Start
    public void Start()
    {
        InitialiseAll();
    }
    #endregion

    #region InitialiseAll
    /// <summary>
    /// Sequenced Initialisation
    /// </summary>
    private void InitialiseAll()
    {
        toolUIScript.Initialise();
        toolFileScript.Initialise();
        adventureUIScript.Initialise();
        adventureScript.Initialise();
    }
    #endregion

    #region Update
    /// <summary>
    /// Only update in the entire code base -> handles redraws and input
    /// </summary>
    private void Update()
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
    #endregion






}
#endif

