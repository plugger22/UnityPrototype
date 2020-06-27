using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR)

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

    #region Master
    public ToolButtonInteraction newAdventureInteraction;
    public ToolButtonInteraction loadAdventureInteraction;
    public ToolButtonInteraction saveAdventureInteraction;
    public ToolButtonInteraction removeAdventureInteraction;
    public ToolButtonInteraction showListsInteraction;
    public ToolButtonInteraction addPlotLineInteraction;
    public ToolButtonInteraction removePlotLineInteraction;
    public ToolButtonInteraction exitInteraction;

    #endregion

    #region New Adventure
    public ToolButtonInteraction themeInteraction;
    public ToolButtonInteraction returnNewInteraction;
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
        Debug.Assert(newAdventureInteraction != null, "Invalid newAdventureInteraction (Null)");
        Debug.Assert(loadAdventureInteraction != null, "Invalid loadAdventureInteraction (Null)");
        Debug.Assert(saveAdventureInteraction != null, "Invalid saveAdventureInteraction (Null)");
        Debug.Assert(removeAdventureInteraction != null, "Invalid removeAdventureInteraction (Null)");
        Debug.Assert(showListsInteraction != null, "Invalid showListInteraction (Null)");
        Debug.Assert(addPlotLineInteraction != null, "Invalid addPlotLineInteraction (Null)");
        Debug.Assert(removePlotLineInteraction != null, "Invalid removePlotLineInteraction (Null)");
        Debug.Assert(exitInteraction != null, "Invalid ExitInteraction (Null)");
        Debug.Assert(themeInteraction != null, "Invalid themeInteraction (Null)");
        Debug.Assert(returnNewInteraction != null, "Invalid returnNewInteraction (Null)");
        //switch off
        adventureCanvas.gameObject.SetActive(false);
        masterCanvas.gameObject.SetActive(true);
        newAdventureCanvas.gameObject.SetActive(false);
        //assign button events
        exitInteraction.SetButton(ToolEventType.CloseAdventureUI);
        newAdventureInteraction.SetButton(ToolEventType.OpenNewAdventure);
        returnNewInteraction.SetButton(ToolEventType.CloseNewAdventure);
        //listeners
        ToolEvents.i.AddListener(ToolEventType.OpenAdventureUI, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.CloseAdventureUI, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.OpenNewAdventure, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.CloseNewAdventure, OnEvent, "AdventureUI");
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
            case ToolEventType.CloseAdventureUI:
                CloseAdventureUI();
                break;
            case ToolEventType.OpenNewAdventure:
                OpenNewAdventure();
                break;
            case ToolEventType.CloseNewAdventure:
                CloseNewAdventure();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// open
    /// </summary>
    private void SetAdventureUI()
    {
        //turn on
        ToolManager.i.toolUIScript.CloseMainMenu();
        adventureCanvas.gameObject.SetActive(true);
    }


    /// <summary>
    /// close
    /// </summary>
    private void CloseAdventureUI()
    {
        ToolManager.i.toolUIScript.OpenMainMenu();
        adventureCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// open new adventure page
    /// </summary>
    private void OpenNewAdventure()
    {
        newAdventureCanvas.gameObject.SetActive(true);
        masterCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// close new adventure page
    /// </summary>
    private void CloseNewAdventure()
    {
        masterCanvas.gameObject.SetActive(true);
        newAdventureCanvas.gameObject.SetActive(false);
    }

    //new scripts above here
}

#endif
