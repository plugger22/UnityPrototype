using UnityEngine;
using toolsAPI;
using UnityEditor;

#if (UNITY_EDITOR)

/// <summary>
/// handles main tool screen and menu UI
/// </summary>
public class ToolUI : MonoBehaviour
{

    #region Main
    public Canvas menuCanvas;
    public ToolButtonInteraction adventureInteraction;
    public ToolButtonInteraction quitInteraction;

    //static reference
    private static ToolUI toolUI;
    #endregion

    /// <summary>
    /// provide a static reference to ToolUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ToolUI Instance()
    {
        if (!toolUI)
        {
            toolUI = FindObjectOfType(typeof(ToolUI)) as ToolUI;
            if (!toolUI)
            { Debug.LogError("There needs to be one active toolUI script on a GameObject in your scene"); }
        }
        return toolUI;
    }

    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        //error checks
        Debug.Assert(menuCanvas != null, "Invalid menuCanvas (Null)");
        Debug.Assert(adventureInteraction != null, "Invalid adventureInteraction (Null)");
        Debug.Assert(quitInteraction != null, "Invalid quitInteraction (Null)");
        //button assignments
        adventureInteraction.SetButton(ToolEventType.OpenAdventureUI);
        quitInteraction.SetButton(ToolEventType.QuitTools);
        //turn on
        menuCanvas.gameObject.SetActive(true);
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.Menu);
        //listeners
        ToolEvents.i.AddListener(ToolEventType.QuitTools, OnEvent, "ToolUI");
    }


    #region Events
    /// <summary>
    /// handles events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(ToolEventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case ToolEventType.QuitTools:
                QuitTools();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion

    /// <summary>
    /// Open main menu
    /// </summary>
    public void OpenTools()
    {
        menuCanvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// close main menu
    /// </summary>
    public void CloseTools()
    {
        menuCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// quit
    /// </summary>
    private void QuitTools()
    {
        EditorApplication.isPlaying = false;
    }
    //new methods above here
}

#endif
