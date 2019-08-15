using gameAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles all topic decision UI matters
/// </summary>
public class TopicUI : MonoBehaviour
{
    [Header("Main")]
    public Canvas topicCanvas;
    public GameObject topicObject;

    [Header("Buttons")]
    public Button buttonOption0;
    public Button buttonOption1;
    public Button buttonOption2;
    public Button buttonOption3;
    public Button buttonIgnore;

    [Header("Texts")]
    public TextMeshProUGUI textHeader;
    public TextMeshProUGUI textMain;
    public TextMeshProUGUI textOption0;
    public TextMeshProUGUI textOption1;
    public TextMeshProUGUI textOption2;
    public TextMeshProUGUI textOption3;

    [Header("Sprite")]
    public Image topicImage;

    //static reference
    private static TopicUI topicUI;


    /// <summary>
    /// provide a static reference to TopicUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TopicUI Instance()
    {
        if (!topicUI)
        {
            topicUI = FindObjectOfType(typeof(TopicUI)) as TopicUI;
            if (!topicUI)
            { Debug.LogError("There needs to be one active topicUI script on a GameObject in your scene"); }
        }
        return topicUI;
    }


    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadGame:
            case GameState.FollowOnInitialisation:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {

    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {


    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener

    }
    #endregion

    #endregion
}
