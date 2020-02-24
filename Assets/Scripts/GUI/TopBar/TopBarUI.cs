using gameAPI;
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

    //tooltips
    private GenericTooltipUI tipCommendation;
    private GenericTooltipUI tipBlackmark;
    private GenericTooltipUI tipInvestigation;
    private GenericTooltipUI tipInnocence;

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
        //tooltips
        tipCommendation = commendations.tooltip.GetComponent<GenericTooltipUI>();
        tipBlackmark = blackmarks.tooltip.GetComponent<GenericTooltipUI>();
        tipInvestigation = investigations.tooltip.GetComponent<GenericTooltipUI>();
        tipInnocence = innocence.tooltip.GetComponent<GenericTooltipUI>();
        Debug.Assert(tipCommendation != null, "Invalid tipCommendation (Null)");
        Debug.Assert(tipBlackmark != null, "Invalid tipBlackmark (Null)");
        Debug.Assert(tipInvestigation != null, "Invalid tipInvestigation (Null)");
        Debug.Assert(tipInnocence != null, "Invalid tipInnocence (Null)");
        //Set colours
        Color color = GameManager.instance.guiScript.colourTopBarBackground;
        topBarMainPanel.color = new Color(color.r, color.g, color.b, 0.65f);
        //Set icon color
        color = GameManager.instance.guiScript.colourNodeDefault;
        commendations.textIcon.color = new Color(color.r, color.g, color.b, 1.0f);
        blackmarks.textIcon.color = new Color(color.r, color.g, color.b, 1.0f);
        investigations.textIcon.color = new Color(color.r, color.g, color.b, 1.0f);
        innocence.textIcon.color = new Color(color.r, color.g, color.b, 1.0f);
        //icon symbols
        commendations.textIcon.text = GameManager.instance.guiScript.commendationChar.ToString();
        blackmarks.textIcon.text = GameManager.instance.guiScript.blackmarkChar.ToString();
        investigations.textIcon.text = GameManager.instance.guiScript.investigateChar.ToString();
        innocence.textIcon.text = GameManager.instance.guiScript.innocenceChar.ToString();
        //Tooltips
        InitialiseTooltips();

    }
    #endregion

    #region SubInitialiseResetTopBar
    /// <summary>
    /// reset data to start of level
    /// </summary>
    private void SubInitialiseResetTopBar()
    {
        //Set Data
        commendations.textData.text = GameManager.instance.campaignScript.GetCommendations().ToString();
        blackmarks.textData.text = GameManager.instance.campaignScript.GetBlackmarks().ToString();
        investigations.textData.text = GameManager.instance.playerScript.CheckNumOfInvestigations().ToString();
    }
    #endregion

    #endregion

    /// <summary>
    /// Update methods for top bar left data
    /// </summary>
    /// <param name="value"></param>
    public void UpdateCommendations(int value)
    { commendations.textData.text = value.ToString(); }

    public void UpdateBlackmarks(int value)
    { blackmarks.textData.text = value.ToString(); }

    public void UpdateInvestigations(int value)
    { investigations.textData.text = value.ToString(); }

    public void UpdateInnocence(int value)
    { innocence.textData.text = value.ToString(); }

    private void InitialiseTooltips()
    {
        //commendation
        tipCommendation.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.instance.colourScript.GetFormattedString("Commendations", ColourType.neutralText));
        tipCommendation.tooltipMain = string.Format("You have done good things and your efforts have been {0}",
            GameManager.instance.colourScript.GetFormattedString("Rewarded", ColourType.salmonText));
        tipCommendation.tooltipDetails = string.Format("Gain {0} Commendations and you will {1}",
            GameManager.instance.colourScript.GetFormattedString(GameManager.instance.campaignScript.outcomesWinLose.ToString(), ColourType.neutralText),
            GameManager.instance.colourScript.GetFormattedString("Win the Campaign", ColourType.salmonText));
        tipCommendation.x_offset = 5;
        tipCommendation.y_offset = 60;
        //blackmark
        tipBlackmark.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.instance.colourScript.GetFormattedString("Blackmarks", ColourType.neutralText));
        tipBlackmark.tooltipMain = string.Format("Your performance is {0} and it has been noted on your record",
            GameManager.instance.colourScript.GetFormattedString("below an acceptable standard", ColourType.salmonText));
        tipBlackmark.tooltipDetails = string.Format("Gain {0} Blackmarks and you will {1}",
            GameManager.instance.colourScript.GetFormattedString(GameManager.instance.campaignScript.outcomesWinLose.ToString(), ColourType.neutralText),
            GameManager.instance.colourScript.GetFormattedString("Lose the Campaign", ColourType.salmonText));
        tipBlackmark.x_offset = 5;
        tipBlackmark.y_offset = 60;
        //Investigations
        tipInvestigation.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.instance.colourScript.GetFormattedString("Investigations", ColourType.neutralText));
        tipInvestigation.tooltipMain = string.Format("Investigations continue until their is sufficient evidence for a verdict{0}{1}{2}less than 0 Stars{3}{4}{5}more than 3 Stars",
            "\n", GameManager.instance.colourScript.GetFormattedString("GUILTY", ColourType.badText), "\n",
            "\n", GameManager.instance.colourScript.GetFormattedString("INNOCENT", ColourType.goodText), "\n");
        tipInvestigation.tooltipDetails = string.Format("If the investigation reaches a resolution with a {0} you are fired immediately{1}{2}{3}You also gain {4}",
            GameManager.instance.colourScript.GetFormattedString("Guilty Verdict", ColourType.salmonText),
            "\n", GameManager.instance.colourScript.GetFormattedString("LEVEL OVER", ColourType.badText), "\n",
            GameManager.instance.colourScript.GetFormattedString("Blackmarks" , ColourType.neutralText));
        tipInvestigation.x_offset = 5;
        tipInvestigation.y_offset = 60;
        //innocence
        tipInnocence.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.instance.colourScript.GetFormattedString("Innocence", ColourType.neutralText));
        tipInnocence.tooltipMain = string.Format("How the {0} views you when you've been {1}",
            GameManager.instance.colourScript.GetFormattedString("Authority", ColourType.salmonText),
            GameManager.instance.colourScript.GetFormattedString("CAPTURED", ColourType.salmonText));
        tipInnocence.tooltipDetails = string.Format("If it drops to {0} you will be {1} when next captured{2}{3}",
            GameManager.instance.colourScript.GetFormattedString("ZERO", ColourType.neutralText),
            GameManager.instance.colourScript.GetFormattedString("ERASED", ColourType.badText),
            "\n", GameManager.instance.colourScript.GetFormattedString("CAMPAIGN OVER", ColourType.badText));
        tipInnocence.x_offset = 5;
        tipInnocence.y_offset = 60;
    }


}
