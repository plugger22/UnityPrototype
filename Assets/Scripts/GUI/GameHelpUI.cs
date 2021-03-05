using gameAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles display of gameHelp.SO data. Set UI but internals can vary depending on the layout
/// </summary>
public class GameHelpUI : MonoBehaviour
{
    [Header("Globals")]
    public Canvas helpCanvas;
    public GameObject helpObject;
    public TextMeshProUGUI headerText;

    [Header("Single image layout")]
    public Image layout0Panel;
    public Image layout0Image;
        
    #region static instance...

    private static GameHelpUI gameHelpUI;

    /// <summary>
    /// Static instance so GameHelpUI can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static GameHelpUI Instance()
    {
        if (!gameHelpUI)
        {
            gameHelpUI = FindObjectOfType(typeof(GameHelpUI)) as GameHelpUI;
            if (!gameHelpUI)
            { Debug.LogError("There needs to be one active GameHelpUI script on a GameObject in your scene"); }
        }
        return gameHelpUI;
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
        Debug.Assert(helpCanvas != null, "Invalid helpCanvas (Null)");
        Debug.Assert(helpObject != null, "Invalid helpObject (Null)");
        Debug.Assert(headerText != null, "Invalid headerText (Null)");
        Debug.Assert(layout0Panel != null, "Invalid layout0Panel (Null)");
        Debug.Assert(layout0Image != null, "Invalid layout0Image (Null)");
    }
    #endregion


    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        //toggle components to ready state
        helpCanvas.gameObject.SetActive(false);
        helpObject.SetActive(true);
        layout0Panel.gameObject.SetActive(false);

    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.GameHelpOpen, OnEvent, "GameHelpUI.cs");
        EventManager.i.AddListener(EventType.GameHelpClose, OnEvent, "GameHelpUI.cs");
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
            case EventType.GameHelpOpen:
                GameHelp gameHelp = Param as GameHelp;
                SetGameHelp(gameHelp);
                break;
            case EventType.GameHelpClose:
                CloseGameHelp();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion


    #region SetGameHelp
    /// <summary>
    /// Open up GameHelpUI using data from GameHelp.SO parameter
    /// </summary>
    /// <param name="help"></param>
    private void SetGameHelp(GameHelp help)
    {
        if (help != null)
        {
            bool isProceed = true;
            //header
            headerText.text = help.header;
            //layout
            switch (help.layout.name)
            {
                case "HelpSingle":
                    layout0Panel.gameObject.SetActive(true);
                    layout0Image.sprite = help.sprite0;
                    break;
                default: Debug.LogWarningFormat("Unrecognised help.layout.name \"{0}\"", help.layout.name); isProceed = false;  break;
            }
            //switch on canvas
            if (isProceed == true)
            {
                //set modal true
                GameManager.i.guiScript.SetIsBlocked(true);

                /*GameManager.i.guiScript.SetIsBlocked(true, modalLevel);
                //pass through data for when the confirm window is closed
                modalLevel = details.modalLevel;
                modalState = details.modalState;*/

                //set states
                GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.GameHelp });
                Debug.LogFormat("[UI] GameHelpUI.cs -> SetGameHelp{0}", "\n");
                //turn on GameHelp
                helpCanvas.gameObject.SetActive(true);
            }
        }
        else { Debug.LogWarning("Invalid (Game) help (Null)"); }
    }
    #endregion


    #region CloseGameHelp
    /// <summary>
    /// Close GameHelpUIs
    /// </summary>
    private void CloseGameHelp()
    {
        Debug.LogFormat("[UI] GameHelp.cs -> CloseGameHelp{0}", "\n");

        /*//set modal false
        GameManager.i.guiScript.SetIsBlocked(false, modalLevel);
        //set game state
        GameManager.i.inputScript.ResetStates(modalState);*/

        GameManager.i.guiScript.SetIsBlocked(false);
        GameManager.i.inputScript.ResetStates();
        //switch off canvas
        helpCanvas.gameObject.SetActive(false);
    }
    #endregion


    //new methods above here
}
