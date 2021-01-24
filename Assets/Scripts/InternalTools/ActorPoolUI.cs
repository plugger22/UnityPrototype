using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using toolsAPI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runs actorPoolUI in internal tools scene
/// </summary>
public class ActorPoolUI : MonoBehaviour
{
    #region public
    public Canvas actorCanvas;

    [Header("Button Interactions")]
    public ToolButtonInteraction newPoolInteraction;
    public ToolButtonInteraction loadPoolInteraction;
    public ToolButtonInteraction savePoolInteraction;
    public ToolButtonInteraction quitPoolInteraction;

    [Header("Drop down lists")]
    public TMP_Dropdown dropInputTrait;

    #endregion

    #region Collections
    private List<string> listOfOptions = new List<string>();

    #endregion

    //static reference
    private static ActorPoolUI actorPoolUI;

    #region static Instance
    /// <summary>
    /// provide a static reference to ActorPoolUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ActorPoolUI Instance()
    {
        if (!actorPoolUI)
        {
            actorPoolUI = FindObjectOfType(typeof(ActorPoolUI)) as ActorPoolUI;
            if (!actorPoolUI)
            { Debug.LogError("There needs to be one active actorPoolUI script on a GameObject in your scene"); }
        }
        return actorPoolUI;
    }
    #endregion

    /// <summary>
    /// Master Initialiser
    /// </summary>
    public void Initialise()
    {
        InitialiseFastAccess();
        InitialiseEvents();
        InitialiseButtons();

    }

    #region InitialiseButtons
    /// <summary>
    /// Initialise Buttons
    /// </summary>
    private void InitialiseButtons()
    {
        quitPoolInteraction.SetButton(ToolEventType.CloseActorPoolUI);
    }
    #endregion

    #region InitialiseFastAccess
    /// <summary>
    /// Fast access
    /// </summary>
    public void InitialiseFastAccess()
    {
        Debug.Assert(actorCanvas != null, "Invalid actorCanvas (Null)");
        //buttons
        Debug.Assert(newPoolInteraction != null, "Invalid newPoolInteraction (Null)");
        Debug.Assert(loadPoolInteraction != null, "Invalid loadPoolInteraction (Null)");
        Debug.Assert(savePoolInteraction != null, "Invalid savePoolInteraction (Null)");
        Debug.Assert(quitPoolInteraction != null, "Invalid quitPoolInteraction (Null)");
        //drop down lists
        Debug.Assert(dropInputTrait != null, "Invalid dropInputTrait (Null)");
    }
    #endregion

    #region InitialiseEvents
    /// <summary>
    /// Initialise all event listeners
    /// </summary>
    private void InitialiseEvents()
    {
        //listeners
        ToolEvents.i.AddListener(ToolEventType.OpenActorPoolUI, OnEvent, "ActorPoolUI");
        ToolEvents.i.AddListener(ToolEventType.CloseActorPoolUI, OnEvent, "ActorPoolUI");
    }
    #endregion


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
            case ToolEventType.OpenActorPoolUI:
                OpenActorPoolUI();
                break;
            case ToolEventType.CloseActorPoolUI:
                CloseActorPoolUI();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion

    /// <summary>
    /// opens ActorPoolUI
    /// </summary>
    private void OpenActorPoolUI()
    {
        //turn on
        ToolManager.i.toolUIScript.CloseTools();
        actorCanvas.gameObject.SetActive(true);
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.ActorPool);
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Edit);
    }

    /// <summary>
    /// close ActorPoolUI
    /// </summary>
    private void CloseActorPoolUI()
    {
        ToolManager.i.toolUIScript.OpenTools();
        actorCanvas.gameObject.SetActive(false);
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.Menu);
    }
}
