﻿using gameAPI;
using packageAPI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles all new turn button matters
/// </summary>
public class NewTurnUI : MonoBehaviour, IPointerClickHandler
{

    public Canvas turnCanvas;
    public GameObject turnObject;
    public Image turnImage;
    public TextMeshProUGUI turnText;

    private GenericTooltipUI help;

    #region static instance...
    private static NewTurnUI newTurnUI;
    /// <summary>
    /// Static instance
    /// </summary>
    /// <returns></returns>
    public static NewTurnUI Instance()
    {
        if (!newTurnUI)
        {
            newTurnUI = FindObjectOfType(typeof(NewTurnUI)) as NewTurnUI;
            if (!newTurnUI)
            { Debug.LogError("There needs to be one active NewTurnUI script on a GameObject in your scene"); }
        }
        return newTurnUI;
    }
    #endregion

    #region Initialisation...
    /// <summary>
    /// Master Initialisation
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.FollowOnInitialisation:
                //do nothing
                break;
            case GameState.TutorialOptions:
            case GameState.LoadGame:
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
            case GameState.StartUp:
                SubInitialiseAsserts();
                SubInitialiseFastAccess();
                SubInitialiseAll();
                SubInitialiseEvents();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
    }
    #endregion

    #region SubInitialiseAsserts
    private void SubInitialiseAsserts()
    {
        Debug.Assert(turnCanvas != null, "Invalid turnCanvas (Null)");
        Debug.Assert(turnObject != null, "Invalid turnObject (Null)");
        Debug.Assert(turnImage != null, "Invalid turnImage (Null)");
        Debug.Assert(turnText != null, "Invalid turnText (Null)");
    }
    #endregion


    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        //toggle components to ready state
        turnCanvas.gameObject.SetActive(false);
        turnObject.SetActive(true);
        turnImage.gameObject.SetActive(true);
        turnText.gameObject.SetActive(true);
        //help
        help = turnImage.GetComponent<GenericTooltipUI>();
        if (help != null)
        {
            help.tooltipHeader = string.Format("<size=120%>{0}</size>",GameManager.Formatt("New Turn", ColourType.salmonText));
            help.tooltipMain = string.Format("When you are ready, press {0} or <b>Click this button</b> for the next turn", GameManager.Formatt("ENTER", ColourType.neutralText));
            help.x_offset = 25;
            help.y_offset = 50;
        }
        else { Debug.LogError("Invalid GenericTooltipUI (Null) for NewTurnUI"); }
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.NewTurnShow, OnEvent, "NewTurnUI.cs");
        EventManager.i.AddListener(EventType.NewTurnHide, OnEvent, "NewTurnUI.cs");
    }
    #endregion

    #endregion

    #region OnEvent
    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.NewTurnShow:
                SetNewTurn();
                break;
            case EventType.NewTurnHide:
                CloseNewTurn();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion

    #region SetNewTurn
    /// <summary>
    /// toggle on new turn UI element
    /// </summary>
    private void SetNewTurn()
    {
        if (GameManager.i.optionScript.isActions == true)
        {
            turnCanvas.gameObject.SetActive(true);
            Debug.LogFormat("[UI] NewTurnUI.cs -> SetNewTurn: Switch ON NewTurnUI{0}", "\n");
        }
    }
    #endregion


    #region CloseNewTurn
    /// <summary>
    /// toggle off new turn UI element
    /// </summary>
    private void CloseNewTurn()
    {
        turnCanvas.gameObject.SetActive(false);
        Debug.LogFormat("[UI] NewTurnUI.cs -> CloseNewTurn: Switch OFF NewTurnUI{0}", "\n");
    }
    #endregion


    #region OnPointerClick
    /// <summary>
    /// Mouse click -> Left / Right new turn
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
            case PointerEventData.InputButton.Right:
                EventManager.i.PostNotification(EventType.NewTurn, this, null, "NewTurnUI.cs -> OnPointerClick");
                break;
            default:
                Debug.LogError("Unknown InputButton");
                break;
        }
    }
    #endregion


}
