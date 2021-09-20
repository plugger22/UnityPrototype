using gameAPI;
using TMPro;
using UnityEngine;

public class FinderSideTabUI : MonoBehaviour
{
    public GameObject tabObject;
    public TextMeshProUGUI tabText;

    [HideInInspector] public string tooltipHeader;
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipDetails;

    [HideInInspector] public bool isActive;                             //true if in use (Resistance Player, Authority AI), false otherwise


    private GenericTooltipUI tooltip;

    #region Static Instance...

    private static FinderSideTabUI finderSideTabUI;

    /// <summary>
    /// provide a static reference to FinderUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static FinderSideTabUI Instance()
    {
        if (!finderSideTabUI)
        {
            finderSideTabUI = FindObjectOfType(typeof(FinderSideTabUI)) as FinderSideTabUI;
            if (!finderSideTabUI)
            { Debug.LogError("There needs to be one active FinderSideTabUI script on a GameObject in your scene"); }
        }
        return finderSideTabUI;
    }

    #endregion

    #region Initialise
    /// <summary>
    /// Initialise. Conditional activiation depending on player side for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        //Resistance player only
        if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideResistance.level)
        {
            switch (state)
            {
                case GameState.TutorialOptions:
                case GameState.NewInitialisation:
                    SubInitialiseAssert();
                    SubInitialiseSessionStart();
                    break;
                case GameState.LoadAtStart:
                    SubInitialiseAssert();
                    SubInitialiseSessionStart();
                    break;
                case GameState.LoadGame:
                    SubInitialiseAssert();
                    SubInitialiseSessionStart();
                    break;
                case GameState.FollowOnInitialisation:
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                    break;
            }
        }
    }
    #endregion

    #region Initialise SubMethods...

    #region SubInitialiseAssert
    private void SubInitialiseAssert()
    {
        Debug.Assert(tabObject != null, "Invalid tabObject (Null)");
        Debug.Assert(tabText != null, "Invalid tabText (Null)");
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //tooltip
        tooltip = tabObject.GetComponent<GenericTooltipUI>();
        if (tooltip != null)
        {
            tooltip.isIgnoreClick = true;
            tooltip.testTag = "FinderSideTabUI";
            tooltip.tooltipMain = string.Format("<size=120%>{0}</size>", GameManager.Formatt("District Finder", ColourType.salmonText));
            tooltip.tooltipDetails = "Click to Open";
            tooltip.x_offset = 100;
        }
        else { Debug.LogError("Invalid tooltip component (Null) for FinderSideTabUI"); }
        //data
        tabText.text = string.Format("{0}", GameManager.i.guiScript.searchChar);
        //set to Active
        isActive = true;
        tabObject.SetActive(true);
    }
    #endregion

    #endregion

    /// <summary>
    /// Toggles finder tab on/off. Also sets optionManager.cs -> isFinder
    /// </summary>
    /// <param name="isOn"></param>
    public void ToggleFinder(bool isOn)
    {
        tabObject.SetActive(isOn);
        GameManager.i.optionScript.isFinder = isOn;
    }

}
