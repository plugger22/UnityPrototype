using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runs adventure generator UI in Internal Tools scene
/// </summary>
public class AdventureUI : MonoBehaviour
{
    #region Main
    public Canvas adventureCanvas;
    public Canvas masterCanvas;
    public Canvas newAdventureCanvas;
    //static reference
    private static AdventureUI adventureUI;
    #endregion

    /// <summary>
    /// provide a static reference to AdventureUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static AdventureUI Instance()
    {
        if (!adventureUI)
        {
            adventureUI = FindObjectOfType(typeof(AdventureUI)) as AdventureUI;
            if (!adventureUI)
            { Debug.LogError("There needs to be one active adventureUI script on a GameObject in your scene"); }
        }
        return adventureUI;
    }


    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        //error check
        Debug.Assert(adventureCanvas != null, "Invalid adventureCanvas (Null)");
        Debug.Assert(masterCanvas != null, "Invalid masterCanvas (Null)");
        Debug.Assert(newAdventureCanvas != null, "Invalid newAdventureCanvas (Null)");
        //switch off
        adventureCanvas.gameObject.SetActive(false);
        masterCanvas.gameObject.SetActive(true);
        newAdventureCanvas.gameObject.SetActive(false);
        //listeners
        ToolEvents.i.AddListener(ToolEventType.OpenAdventureUI, OnEvent, "AdventureUI");
    }

    

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
            case ToolEventType.OpenAdventureUI:
                SetAdventureUI();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    private void SetAdventureUI()
    {
        //turn on
        ToolManager.i.toolUIScript.CloseMainMenu();
        adventureCanvas.gameObject.SetActive(true);
    }

    //new scripts above here
}
