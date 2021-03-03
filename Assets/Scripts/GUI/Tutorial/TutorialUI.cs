using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles all tutorial side bar UI functionality
/// </summary>
public class TutorialUI : MonoBehaviour
{
    [Header("Main Components")]
    public Canvas tutorialCanvas;
    public GameObject tutorialObject;
    public Image mainPanel;

    [Header("Buttons")]
    public Button button0;
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;
    public Button button5;
    public Button button6;
    public Button button7;
    public Button button8;
    public Button button9;

    #region Private...

    private RectTransform mainTransform;

    #endregion

    #region static instance...
    private static TutorialUI tutorialUI;

    /// <summary>
    /// Static instance so the Modal Menu can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TutorialUI Instance()
    {
        if (!tutorialUI)
        {
            tutorialUI = FindObjectOfType(typeof(TutorialUI)) as TutorialUI;
            if (!tutorialUI)
            { Debug.LogError("There needs to be one active TutorialUI script on a GameObject in your scene"); }
        }
        return tutorialUI;
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
            case GameState.TutorialOptions:
                //do nothing
                break;
            case GameState.LoadGame:
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
            case GameState.StartUp:
                SubInitialiseFastAccess();
                SubInitialiseAll();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        Debug.Assert(tutorialCanvas != null, "Invalid tutorialCanvas (Null)");
        Debug.Assert(tutorialObject != null, "Invalid tutorialObject (Null)");
        Debug.Assert(mainPanel != null, "Invalid mainPanel (Null)");
        Debug.Assert(button0 != null, "Invalid button0 (Null)");
        Debug.Assert(button1 != null, "Invalid button1 (Null)");
        Debug.Assert(button2 != null, "Invalid button2 (Null)");
        Debug.Assert(button3 != null, "Invalid button3 (Null)");
        Debug.Assert(button4 != null, "Invalid button4 (Null)");
        Debug.Assert(button5 != null, "Invalid button5 (Null)");
        Debug.Assert(button6 != null, "Invalid button6 (Null)");
        Debug.Assert(button7 != null, "Invalid button7 (Null)");
        Debug.Assert(button8 != null, "Invalid button8 (Null)");
        Debug.Assert(button9 != null, "Invalid button9 (Null)");
    }
    #endregion

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        mainTransform = mainPanel.GetComponent<RectTransform>();
        if (mainTransform != null)
        {
            Vector3 position = mainTransform.position;
            position.x = Screen.width - mainTransform.rect.width - 50;
            position.y = mainTransform.rect.y;
            position = Camera.main.ScreenToWorldPoint(position);
            //move main panel to right hand side of screen
            mainTransform.position = position;
        }
        else { Debug.LogError("Invalid mainTranform (Null)"); }
    }
    #endregion

    #endregion

}
