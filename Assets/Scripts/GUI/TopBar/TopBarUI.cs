using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// top Bar UI
/// </summary>
public class TopBarUI : MonoBehaviour
{
    public GameObject topBarObject;

    public TopBarDataInteraction commendations;
    public TopBarDataInteraction blackmarks;
    public TopBarDataInteraction investigations;
    public TopBarDataInteraction innocence;

    public Image topBarMainPanel;

    private static TopBarUI topBarUI;

    /// <summary>
    /// provide a static reference to widgetTopUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TopBarUI Instance()
    {
        if (!topBarUI)
        {
            topBarUI = FindObjectOfType(typeof(TopBarUI)) as TopBarUI;
            if (!topBarUI)
            { Debug.LogError("There needs to be one active TopBarUI script on a GameObject in your scene"); }
        }
        return topBarUI;
    }


    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.StartUp:
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                SubInitialiseSessionStart();
                SubInitialiseResetTopBar();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                SubInitialiseSessionStart();
                SubInitialiseResetTopBar();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseResetTopBar();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {

    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {

    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //Set colours
        Color color = GameManager.instance.guiScript.colourTopBarBackground;
        topBarMainPanel.color = new Color(color.r, color.g, color.b, 1.0f);
        //Set icons
        commendations.textIcon.text = GameManager.instance.guiScript.commendationChar.ToString();
        blackmarks.textIcon.text = GameManager.instance.guiScript.blackmarkChar.ToString();
        investigations.textIcon.text = GameManager.instance.guiScript.investigateChar.ToString();
        innocence.textIcon.text = GameManager.instance.guiScript.innocenceChar.ToString();
    }
    #endregion

    #region SubInitialiseResetTopBar
    /// <summary>
    /// reset data to start of level
    /// </summary>
    private void SubInitialiseResetTopBar()
    {

    }
    #endregion

    #endregion

}
