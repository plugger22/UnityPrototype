using System.Collections.Generic;
using UnityEngine;



#if (UNITY_EDITOR)
/// <summary>
/// Internal tool Manager -> handles functionality of toolUI as well
/// </summary>
public class ToolManager : MonoBehaviour
{
    #region Components
    public static ToolManager i = null;                                 //static instance of toolManager which allows it to be accessed by any other script

    [HideInInspector] public AdventureManager adventureScript;
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
        //gui
        adventureUIScript = AdventureUI.Instance();
        toolUIScript = ToolUI.Instance();
        //error Check
        Debug.Assert(adventureScript != null, "Invalid adventureScript");
        Debug.Assert(adventureUIScript != null, "Invalid adventureUIScript");
        Debug.Assert(toolUIScript != null, "Invalid toolUIScript");

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
        adventureUIScript.Initialise();
        adventureScript.Initialise();
    }
    #endregion






}
#endif

