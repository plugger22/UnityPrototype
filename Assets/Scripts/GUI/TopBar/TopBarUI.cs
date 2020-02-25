using gameAPI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// top Bar UI
/// </summary>
public class TopBarUI : MonoBehaviour
{
    public GameObject topBarObject;

    //TopBarItem prefab interactions LHS -> Data
    public TopBarDataInteraction commendations;
    public TopBarDataInteraction blackmarks;
    public TopBarDataInteraction investigations;
    public TopBarDataInteraction innocence;
    //TopBarItem prefab interactions RHS -> Status
    public TopBarDataInteraction unhappy;                       //number of unhappy actors in reserves
    public TopBarDataInteraction conflicts;                     //number of active relationship conflicts
    public TopBarDataInteraction blackmail;                     //number of blackmail attempts
    public TopBarDataInteraction doom;                          //number of turns remaining for doom timer
    

    public Image topBarMainPanel;
    public Image topBarLeft;                                    //cutaways on main panel corners
    public Image topBarRight;

    //tooltips
    private GenericTooltipUI tipCommendation;
    private GenericTooltipUI tipBlackmark;
    private GenericTooltipUI tipInvestigation;
    private GenericTooltipUI tipInnocence;
    private GenericTooltipUI tipUnhappy;
    private GenericTooltipUI tipConflict;
    private GenericTooltipUI tipBlackmail;
    private GenericTooltipUI tipDoom;

    //colours
    private Color colourIconData;
    private Color colourIconStatus;
    private Color colourNumber;

    //assorted
    private float opacityLow = 0.35f;
    private float opacityHigh = 1.0f;

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
        Debug.Assert(topBarMainPanel != null, "Invalid topBarMainPanel (Null)");
        Debug.Assert(topBarLeft != null, "Invalid topBarLeft (Null)");
        Debug.Assert(topBarRight != null, "Invalid topBarRight (Null)");
        Debug.Assert(commendations != null, "Invalid commendations (Null)");
        Debug.Assert(blackmarks != null, "Invalid blackmarks (Null)");
        Debug.Assert(investigations != null, "Invalid investigations (Null)");
        Debug.Assert(innocence != null, "Invalid innocence (Null)");
        Debug.Assert(unhappy != null, "Invalid unhappy (Null)");
        Debug.Assert(conflicts != null, "Invalid conflicts (Null)");
        Debug.Assert(blackmail != null, "Invalid blackmail (Null)");
        Debug.Assert(doom != null, "Invalid doom (Null)");
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
        colourIconData = GameManager.instance.guiScript.colourIconData;
        colourIconStatus = GameManager.instance.guiScript.colourIconStatus;
        colourNumber = Color.white;
        Debug.Assert(colourIconData != null, "Invalid colourIconData (Null)");
        Debug.Assert(colourIconStatus != null, "Invalid colourIconStatus (Null)");
        Debug.Assert(colourNumber != null, "Invalid colourNumber (Null)");
        //tooltips
        tipCommendation = commendations.tooltip.GetComponent<GenericTooltipUI>();
        tipBlackmark = blackmarks.tooltip.GetComponent<GenericTooltipUI>();
        tipInvestigation = investigations.tooltip.GetComponent<GenericTooltipUI>();
        tipInnocence = innocence.tooltip.GetComponent<GenericTooltipUI>();
        tipUnhappy = unhappy.tooltip.GetComponent<GenericTooltipUI>();
        tipConflict = conflicts.tooltip.GetComponent<GenericTooltipUI>();
        tipBlackmail = blackmail.tooltip.GetComponent<GenericTooltipUI>();
        tipDoom = doom.tooltip.GetComponent<GenericTooltipUI>();
        Debug.Assert(tipCommendation != null, "Invalid tipCommendation (Null)");
        Debug.Assert(tipBlackmark != null, "Invalid tipBlackmark (Null)");
        Debug.Assert(tipInvestigation != null, "Invalid tipInvestigation (Null)");
        Debug.Assert(tipInnocence != null, "Invalid tipInnocence (Null)");
        Debug.Assert(tipUnhappy != null, "Invalid tipUnhappy (Null)");
        Debug.Assert(tipConflict != null, "Invalid tipConflict (Null)");
        Debug.Assert(tipBlackmail != null, "Invalid tipBlackmail (Null)");
        Debug.Assert(tipDoom != null, "Invalid tipDoom (Null)");
        //Set UI element colours
        Color color = GameManager.instance.guiScript.colourTopBarBackground;
        topBarMainPanel.color = new Color(color.r, color.g, color.b, 0.65f);
        topBarLeft.color = new Color(color.r, color.g, color.b, 0.65f);
        topBarRight.color = new Color(color.r, color.g, color.b, 0.65f);
        //Set icon colours -> Data
        color = colourIconData;
        commendations.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
        blackmarks.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
        investigations.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
        innocence.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
        //Set icon colours -> Status
        color = colourIconStatus;
        unhappy.textIcon.color = new Color(color.r, color.g, color.b, opacityLow);
        conflicts.textIcon.color = new Color(color.r, color.g, color.b, opacityLow);
        blackmail.textIcon.color = new Color(color.r, color.g, color.b, opacityLow);
        doom.textIcon.color = new Color(color.r, color.g, color.b, opacityLow);
        //Set number colour (white) and opacity -> Status
        color = colourNumber;
        unhappy.textData.color = new Color(color.r, color.g, color.b, opacityLow);
        conflicts.textData.color = new Color(color.r, color.g, color.b, opacityLow);
        blackmail.textData.color = new Color(color.r, color.g, color.b, opacityLow);
        doom.textData.color = new Color(color.r, color.g, color.b, opacityLow);
        //icon symbols
        commendations.textIcon.text = GameManager.instance.guiScript.commendationChar.ToString();
        blackmarks.textIcon.text = GameManager.instance.guiScript.blackmarkChar.ToString();
        investigations.textIcon.text = GameManager.instance.guiScript.investigateChar.ToString();
        innocence.textIcon.text = GameManager.instance.guiScript.innocenceChar.ToString();
        unhappy.textIcon.text = GameManager.instance.guiScript.unhappyChar.ToString();
        conflicts.textIcon.text = GameManager.instance.guiScript.conflictChar.ToString();
        blackmail.textIcon.text = GameManager.instance.guiScript.blackmailChar.ToString();
        doom.textIcon.text = GameManager.instance.guiScript.doomChar.ToString();
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
        //Set Data (innocence auto updates from method below)
        commendations.textData.text = GameManager.instance.campaignScript.GetCommendations().ToString();
        blackmarks.textData.text = GameManager.instance.campaignScript.GetBlackmarks().ToString();
        investigations.textData.text = GameManager.instance.playerScript.CheckNumOfInvestigations().ToString();

        conflicts.textData.text = "0";
        blackmail.textData.text = "0";
        doom.textData.text = "0";
    }
    #endregion

    #endregion

    /// <summary>
    /// Update methods for top bar left data (don't use events, after speed here)
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

    /// <summary>
    /// Unhappy actors in Reserves, Status item -> bring to full opacity if value > 0
    /// </summary>
    /// <param name="value"></param>
    public void UpdateUnhappy(int value)
    {
        unhappy.textData.text = value.ToString();
        if (value > 0)
        {
            //full opacity
            Color color = colourIconStatus;
            unhappy.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            unhappy.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        else
        {
            //low opacity
            Color color = colourIconStatus;
            unhappy.textIcon.color = new Color(color.r, color.g, color.b, opacityLow);
            color = colourNumber;
            unhappy.textData.color = new Color(color.r, color.g, color.b, opacityLow);
        }
        //tooltip
        UpdateUnhappyTooltip(value);
    }

    /// <summary>
    /// Number of Relationship Conflicts, Status item -> bring to full opacity if value > 0
    /// </summary>
    /// <param name="value"></param>
    public void UpdateConflicts(int value)
    {
        conflicts.textData.text = value.ToString();
        if (value > 0)
        {
            //full opacity
            Color color = colourIconStatus;
            conflicts.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            conflicts.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        else
        {
            //low opacity
            Color color = colourIconStatus;
            conflicts.textIcon.color = new Color(color.r, color.g, color.b, opacityLow);
            color = colourNumber;
            conflicts.textData.color = new Color(color.r, color.g, color.b, opacityLow);
        }
    }

    /// <summary>
    /// Number of Blackmail attempts, Status item -> bring to full opacity if value > 0
    /// </summary>
    /// <param name="value"></param>
    public void UpdateBlackmail(int value)
    {
        blackmail.textData.text = value.ToString();
        if (value > 0)
        {
            //full opacity
            Color color = colourIconStatus;
            blackmail.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            blackmail.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        else
        {
            //low opacity
            Color color = colourIconStatus;
            blackmail.textIcon.color = new Color(color.r, color.g, color.b, opacityLow);
            color = colourNumber;
            blackmail.textData.color = new Color(color.r, color.g, color.b, opacityLow);
        }
    }

    /// <summary>
    /// Doom timer, Status item -> bring to full opacity if value > 0
    /// </summary>
    /// <param name="value"></param>
    public void UpdateDoom(int value)
    {
        doom.textData.text = value.ToString();
        if (value > 0)
        {
            //full opacity
            Color color = colourIconStatus;
            doom.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            doom.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        else
        {
            //low opacity
            Color color = colourIconStatus;
            doom.textIcon.color = new Color(color.r, color.g, color.b, opacityLow);
            color = colourNumber;
            doom.textData.color = new Color(color.r, color.g, color.b, opacityLow);
        }
    }



    /// <summary>
    /// Set up initial tooltip status
    /// </summary>
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
        //unhappy
        tipUnhappy.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.instance.colourScript.GetFormattedString("Unhappy Subordinates in Reserve Pool", ColourType.neutralText));
        tipUnhappy.tooltipMain = string.Format("There are currently {0} Unhappy subordinates", GameManager.instance.colourScript.GetFormattedString("NO", ColourType.neutralText));
        tipUnhappy.x_offset = 5;
        tipUnhappy.y_offset = 60;
        //conflicts
        tipConflict.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.instance.colourScript.GetFormattedString("Relationship Conflicts", ColourType.neutralText));
        tipConflict.tooltipMain = string.Format("You currently have {0} Relationship Conflicts", GameManager.instance.colourScript.GetFormattedString("NO", ColourType.neutralText));
        tipConflict.x_offset = 5;
        tipConflict.y_offset = 60;
        //blackmail
        tipBlackmail.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.instance.colourScript.GetFormattedString("Blackmail", ColourType.neutralText));
        tipBlackmail.tooltipMain = string.Format("{0} is currently Blackmailing you", GameManager.instance.colourScript.GetFormattedString("Nobody", ColourType.neutralText));
        tipBlackmail.x_offset = 5;
        tipBlackmail.y_offset = 60;
        //doom
        tipDoom.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.instance.colourScript.GetFormattedString("Doom Timer", ColourType.neutralText));
        tipDoom.tooltipMain = string.Format("Luckily you have {0} been injected with a fatal drug", GameManager.instance.colourScript.GetFormattedString("NOT", ColourType.neutralText));
        tipDoom.x_offset = 5;
        tipDoom.y_offset = 60;
    }

    /// <summary>
    /// dynamically updates Unhappy actors tooltip (exluding header and offsets)
    /// </summary>
    /// <param name="value"></param>
    private void UpdateUnhappyTooltip(int value)
    {
        if (value > 0)
        {

        }
        else
        { tipUnhappy.tooltipMain = string.Format("There are currently {0} Unhappy subordinates", GameManager.instance.colourScript.GetFormattedString("NO", ColourType.neutralText)); }
    }

}
