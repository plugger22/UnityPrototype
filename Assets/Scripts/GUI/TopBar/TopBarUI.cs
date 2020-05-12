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
    public TopBarDataInteraction conflicts;                     //number of potential actor relationship conflicts (actors with motivation 0)
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
    private Color colourIconDormant;
    private Color colourIconActiveBad;
    private Color colourIconActiveGood;
    private Color colourNumber;

    //assorted
    private float opacityHigh = 1.0f;
    /*private float opacityLow = 0.35f;*/

    #region Save Data Compatible
    //data fields (hold latest values, needed for save)
    [HideInInspector] public int commendationData;
    [HideInInspector] public int blackmarkData;
    [HideInInspector] public int investigationData;
    [HideInInspector] public int innocenceData;
    [HideInInspector] public int unhappyData;
    [HideInInspector] public int conflictData;
    [HideInInspector] public int blackmailData;
    [HideInInspector] public int doomData;
    #endregion

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
                break;
            case GameState.LoadGame:
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseUpdateTopBar();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
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
        //register listener
        EventManager.instance.AddListener(EventType.TopBarShow, OnEvent, "TopBarUI");
        EventManager.instance.AddListener(EventType.TopBarHide, OnEvent, "TopBarUI");
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        colourIconDormant = GameManager.i.guiScript.colourIconDormant;
        colourIconActiveGood = GameManager.i.guiScript.colourIconActiveGood;
        colourIconActiveBad = GameManager.i.guiScript.colourIconActiveBad;
        colourNumber = Color.white;
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
        Color color = GameManager.i.guiScript.colourTopBarBackground;
        topBarMainPanel.color = new Color(color.r, color.g, color.b, 0.65f);
        topBarLeft.color = new Color(color.r, color.g, color.b, 0.65f);
        topBarRight.color = new Color(color.r, color.g, color.b, 0.65f);
        //Set icon colours -> Dormant
        color = colourIconDormant;
        commendations.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
        blackmarks.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
        investigations.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
        innocence.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
        unhappy.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
        conflicts.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
        blackmail.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
        doom.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
        //Set number colour (white) and opacity -> Status
        color = colourNumber;
        unhappy.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        conflicts.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        blackmail.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        doom.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        //icon symbols
        commendations.textIcon.text = GameManager.i.guiScript.commendationChar.ToString();
        blackmarks.textIcon.text = GameManager.i.guiScript.blackmarkChar.ToString();
        investigations.textIcon.text = GameManager.i.guiScript.investigateChar.ToString();
        innocence.textIcon.text = GameManager.i.guiScript.innocenceChar.ToString();
        unhappy.textIcon.text = GameManager.i.guiScript.unhappyChar.ToString();
        conflicts.textIcon.text = GameManager.i.guiScript.conflictChar.ToString();
        blackmail.textIcon.text = GameManager.i.guiScript.blackmailChar.ToString();
        doom.textIcon.text = GameManager.i.guiScript.doomChar.ToString();
        //Tooltips
        InitialiseTooltips();

    }
    #endregion

    #region SubInitialiseResetTopBar
    /// <summary>
    /// sets default data to start of new session
    /// </summary>
    private void SubInitialiseResetTopBar()
    {
        commendations.textData.text = "0";
        blackmarks.textData.text = "0";
        investigations.textData.text = "0";
        innocence.textData.text = GameManager.i.playerScript.Innocence.ToString();
        unhappy.textData.text = "0";
        conflicts.textData.text = "0";
        blackmail.textData.text = "0";
        doom.textData.text = "0";
    }
    #endregion

    #region SubInitialiseUpdateTopBar
    /// <summary>
    /// Updates toolbar with current data for a follow-on new level
    /// </summary>
    private void SubInitialiseUpdateTopBar()
    {
        UpdateCommendations(GameManager.i.campaignScript.GetCommendations());
        UpdateBlackmarks(GameManager.i.campaignScript.GetBlackmarks());
        UpdateInvestigations(GameManager.i.playerScript.CheckNumOfInvestigations());
        UpdateInnocence(GameManager.i.playerScript.Innocence);
        UpdateUnhappy(GameManager.i.actorScript.CheckNumOfUnhappyActors());
        UpdateConflicts(GameManager.i.actorScript.CheckNumOfConflictActors());
        UpdateBlackmail(GameManager.i.actorScript.CheckNumOfBlackmailActors());
        UpdateDoom(GameManager.i.actorScript.doomTimer);
    }
    #endregion


    #endregion


    /// <summary>
    /// Updates toolbar with loaded save game data -> Called from FileManager.cs -> UpdateGUI
    /// </summary>
    public void LoadSavedData()
    {
        UpdateCommendations(commendationData);
        UpdateBlackmarks(blackmarkData);
        UpdateInvestigations(investigationData);
        UpdateInnocence(innocenceData);
        UpdateUnhappy(unhappyData);
        UpdateConflicts(conflictData);
        UpdateBlackmail(blackmailData);
        UpdateDoom(doomData);
    }

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
            case EventType.TopBarShow:
                topBarObject.SetActive(true);
                break;
            case EventType.TopBarHide:
                topBarObject.SetActive(false);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Update methods for top bar left data (don't use events, after speed here)
    /// Set active only if > 0 and >= blackmarks
    /// </summary>
    /// <param name="value"></param>
    public void UpdateCommendations(int value)
    {
        if (value < 0) { Debug.LogWarningFormat("Invalid Commendation value \"{0}\"", value); value = 0; }
        commendationData = value;
        commendations.textData.text = value.ToString();
        if (value > 0 && value >= GameManager.i.campaignScript.GetBlackmarks())
        {
            //active Good
            commendations.textIcon.fontSize = 20.0f;
            commendations.textData.fontSize = 18.0f;
            Color color = colourIconActiveGood;
            commendations.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            commendations.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        else
        {
            //Dormant
            commendations.textIcon.fontSize = 16.0f;
            commendations.textData.fontSize = 14.0f;
            Color color = colourIconDormant;
            commendations.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            commendations.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        //tooltip
        UpdateCommendationsTooltip(value);
    }

    /// <summary>
    /// Set active only if > 0 and >= commendations 
    /// </summary>
    /// <param name="value"></param>
    public void UpdateBlackmarks(int value)
    {
        if (value < 0) { Debug.LogWarningFormat("Invalid Blackmarks value \"{0}\"", value); value = 0; }
        blackmarkData = value;
        blackmarks.textData.text = value.ToString();
        if (value > 0 && value >= GameManager.i.campaignScript.GetCommendations())
        {
            //active Bad
            blackmarks.textIcon.fontSize = 20.0f;
            blackmarks.textData.fontSize = 18.0f;
            Color color = colourIconActiveBad;
            blackmarks.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            blackmarks.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        else
        {
            //Dormant
            blackmarks.textIcon.fontSize = 16.0f;
            blackmarks.textData.fontSize = 14.0f;
            Color color = colourIconDormant;
            blackmarks.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            blackmarks.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        //tooltip
        UpdateBlackmarksTooltip(value);
    }

    /// <summary>
    /// set active if > 0
    /// </summary>
    /// <param name="value"></param>
    public void UpdateInvestigations(int value)
    {
        if (value < 0) { Debug.LogWarningFormat("Invalid investigations value \"{0}\"", value); value = 0; }
        investigationData = value;
        investigations.textData.text = value.ToString();
        if (value > 0)
        {
            //active Bad
            investigations.textIcon.fontSize = 20.0f;
            investigations.textData.fontSize = 18.0f;
            Color color = colourIconActiveBad;
            investigations.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            investigations.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        else
        {
            //Dormant
            investigations.textIcon.fontSize = 16.0f;
            investigations.textData.fontSize = 14.0f;
            Color color = colourIconDormant;
            investigations.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            investigations.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        //tooltip
        UpdateInvestigationsTooltip(value);
    }

    /// <summary>
    /// colour Active if value 1 or less, dormant otherwise
    /// </summary>
    /// <param name="value"></param>
    public void UpdateInnocence(int value)
    {
        if (value < 0) { Debug.LogWarningFormat("Invalid innocence value \"{0}\"", value); value = 0; }
        innocenceData = value;
        innocence.textData.text = value.ToString();
        if (value <= 1)
        {
            //active Bad
            innocence.textIcon.fontSize = 20.0f;
            innocence.textData.fontSize = 18.0f;
            Color color = colourIconActiveBad;
            innocence.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            innocence.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        else
        {
            //Dormant
            innocence.textIcon.fontSize = 16.0f;
            innocence.textData.fontSize = 14.0f;
            Color color = colourIconDormant;
            innocence.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            innocence.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        //tooltip
        UpdateInnocenceTooltip(value);
    }

    /// <summary>
    /// Unhappy actors in Reserves, Status item -> bring to full opacity if value > 0
    /// </summary>
    /// <param name="value"></param>
    public void UpdateUnhappy(int value)
    {

        if (value < 0) { Debug.LogWarningFormat("Invalid unhappy value \"{0}\"", value); value = 0; }
        unhappyData = value;
        unhappy.textData.text = value.ToString();
        if (value > 0)
        {
            //active Bad
            unhappy.textIcon.fontSize = 20.0f;
            unhappy.textData.fontSize = 18.0f;
            Color color = colourIconActiveBad;
            unhappy.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            unhappy.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        else
        {
            //Dormant
            unhappy.textIcon.fontSize = 16.0f;
            unhappy.textData.fontSize = 14.0f;
            Color color = colourIconDormant;
            unhappy.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            unhappy.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
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
        if (value < 0) { Debug.LogWarningFormat("Invalid conflict value \"{0}\"", value); value = 0; }
        conflictData = value;
        conflicts.textData.text = value.ToString();
        if (value > 0)
        {
            //Active Bad
            conflicts.textIcon.fontSize = 20.0f;
            conflicts.textData.fontSize = 18.0f;
            Color color = colourIconActiveBad;
            conflicts.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            conflicts.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        else
        {
            //Dormant
            conflicts.textIcon.fontSize = 16.0f;
            conflicts.textData.fontSize = 14.0f;
            Color color = colourIconDormant;
            conflicts.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            conflicts.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        //tooltip
        UpdateConflictTooltip(value);
    }

    /// <summary>
    /// Number of Blackmail attempts, Status item -> bring to full opacity if value > 0
    /// </summary>
    /// <param name="value"></param>
    public void UpdateBlackmail(int value)
    {
        if (value < 0) { Debug.LogWarningFormat("Invalid blackmail value \"{0}\"", value); value = 0; }
        blackmailData = value;
        blackmail.textData.text = value.ToString();
        if (value > 0)
        {
            //Active Bad
            blackmail.textIcon.fontSize = 20.0f;
            blackmail.textData.fontSize = 18.0f;
            Color color = colourIconActiveBad;
            blackmail.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            blackmail.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        else
        {
            //Dormant
            blackmail.textIcon.fontSize = 16.0f;
            blackmail.textData.fontSize = 14.0f;
            Color color = colourIconDormant;
            blackmail.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            blackmail.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        //tooltip
        UpdateBlackmailTooltip(value);
    }

    /// <summary>
    /// Doom timer, Status item -> bring to full opacity if value > 0
    /// </summary>
    /// <param name="value"></param>
    public void UpdateDoom(int value)
    {
        if (value < 0) { Debug.LogWarningFormat("Invalid doom value \"{0}\"", value); value = 0; }
        doomData = value;
        doom.textData.text = value.ToString();
        if (value > 0)
        {
            //Active Bad
            doom.textIcon.fontSize = 20.0f;
            doom.textData.fontSize = 18.0f;
            Color color = colourIconActiveBad;
            doom.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            doom.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        else
        {
            //Dormant
            doom.textIcon.fontSize = 16.0f;
            doom.textData.fontSize = 14.0f;
            Color color = colourIconDormant;
            doom.textIcon.color = new Color(color.r, color.g, color.b, opacityHigh);
            color = colourNumber;
            doom.textData.color = new Color(color.r, color.g, color.b, opacityHigh);
        }
        UpdateDoomTooltip(value);
    }



    /// <summary>
    /// Set up initial tooltip status
    /// NOTE: tooltipMain details are required for initial default settings (tooltips won't work if the main component isn't there)
    /// </summary>
    private void InitialiseTooltips()
    {
        //commendation
        tipCommendation.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.GetFormattedString("Commendations", ColourType.neutralText));
        tipCommendation.tooltipMain = string.Format("HQ have yet to award you {0} Commendations",
            GameManager.GetFormattedString("ANY", ColourType.neutralText));
        tipCommendation.tooltipDetails = string.Format("Gain {0} Commendations and you will{1}{2}",
            GameManager.GetFormattedString(GameManager.i.campaignScript.awardsWinLose.ToString(), ColourType.neutralText), "\n",
            GameManager.GetFormattedString("WIN the Campaign", ColourType.salmonText));
        tipCommendation.x_offset = 5;
        tipCommendation.y_offset = 60;
        //blackmark
        tipBlackmark.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.GetFormattedString("Blackmarks", ColourType.neutralText));
        tipBlackmark.tooltipMain = string.Format("You do not yet{0}have {1} Blackmarks{2}on your record", "\n",
            GameManager.GetFormattedString("ANY", ColourType.neutralText), "\n");
        tipBlackmark.tooltipDetails = string.Format("Gain {0} Blackmarks and you will{1}{2}",
            GameManager.GetFormattedString(GameManager.i.campaignScript.awardsWinLose.ToString(), ColourType.neutralText), "\n",
            GameManager.GetFormattedString("LOSE the Campaign", ColourType.salmonText));
        tipBlackmark.x_offset = 5;
        tipBlackmark.y_offset = 60;
        //Investigations
        tipInvestigation.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.GetFormattedString("Investigations", ColourType.neutralText));
        tipInvestigation.tooltipMain = string.Format("HQ are {0} currently Investigating you", GameManager.GetFormattedString("NOT", ColourType.neutralText));
        tipInvestigation.x_offset = 5;
        tipInvestigation.y_offset = 60;
        //innocence
        tipInnocence.tooltipHeader = string.Format("<size=120%>{0}</size>{1}{2}", GameManager.GetFormattedString("Innocence", ColourType.neutralText), "\n",
            GameManager.i.guiScript.GetDatapointStars(GameManager.i.playerScript.Innocence));
        tipInnocence.tooltipMain = string.Format("Authority currently views you as a{0}{1}", "\n", GameManager.i.topicScript.GetInnocenceDescriptor());
        tipInnocence.tooltipDetails = string.Format("If it drops to {0} you will be {1} when next captured{2}{3}",
            GameManager.GetFormattedString("ZERO", ColourType.neutralText),
            GameManager.GetFormattedString("ERASED", ColourType.badText),
            "\n", GameManager.GetFormattedString("CAMPAIGN OVER", ColourType.badText));
        tipInnocence.x_offset = 5;
        tipInnocence.y_offset = 60;
        //unhappy
        tipUnhappy.tooltipHeader = string.Format("<size=120%>{0}{1}{2}</size>", GameManager.GetFormattedString("Unhappy Subordinates", ColourType.neutralText), "\n",
            GameManager.GetFormattedString("RESERVE POOL", ColourType.salmonText));
        tipUnhappy.tooltipMain = string.Format("There are currently {0} Unhappy subordinates in your {1}", GameManager.GetFormattedString("NO", ColourType.neutralText),
            GameManager.GetFormattedString("Reserves", ColourType.salmonText));
        tipUnhappy.x_offset = 5;
        tipUnhappy.y_offset = 60;
        //conflicts
        tipConflict.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.GetFormattedString("Potential Conflicts", ColourType.neutralText));
        tipConflict.tooltipMain = string.Format("You currently have {0} upset Subordinates", GameManager.GetFormattedString("NO", ColourType.neutralText));
        tipConflict.tooltipDetails = string.Format("Conflicts occur if a Subordinates {0}{1}{2}", GameManager.GetFormattedString("MOTIVATION", ColourType.neutralText), "\n",
            GameManager.GetFormattedString("drops below ZERO", ColourType.salmonText));
        tipConflict.x_offset = 5;
        tipConflict.y_offset = 60;
        //blackmail
        tipBlackmail.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.GetFormattedString("Blackmail", ColourType.neutralText));
        tipBlackmail.tooltipMain = string.Format("Good news. {0} is currently Blackmailing you", GameManager.GetFormattedString("NOBODY", ColourType.neutralText));
        tipBlackmail.x_offset = 5;
        tipBlackmail.y_offset = 60;
        //doom
        tipDoom.tooltipHeader = string.Format("<size=120%>{0}</size>", GameManager.GetFormattedString("Doom Timer", ColourType.neutralText));
        tipDoom.tooltipMain = string.Format("Luckily you have {0} been injected with a fatal drug", GameManager.GetFormattedString("NOT", ColourType.neutralText));
        tipDoom.x_offset = 5;
        tipDoom.y_offset = 60;
    }

    /// <summary>
    /// dynamically updates Commendations tooltip (excludes header, details and offsets)
    /// </summary>
    private void UpdateCommendationsTooltip(int value)
    {
        if (value > 0)
        {
            tipCommendation.tooltipMain = string.Format("Congratulations!{0}HQ have awarded you{1}<size=120%>{2} Commendation{3}</size>", "\n", "\n",
                GameManager.GetFormattedString(value.ToString(), ColourType.neutralText), value != 1 ? "s" : "");
        }
        else
        { tipCommendation.tooltipMain = string.Format("HQ have yet to award you {0} Commendations", GameManager.GetFormattedString("ANY", ColourType.neutralText)); }
    }

    /// <summary>
    /// dynamically updates Blackmarks tooltip (excludes header, details and offsets)
    /// </summary>
    /// <param name="value"></param>
    private void UpdateBlackmarksTooltip(int value)
    {
        if (value > 0)
        {
            tipBlackmark.tooltipMain = string.Format("You now have{0}<size=120%>{1} BlackMark{2}</size>{3}on your record", "\n",
                GameManager.GetFormattedString(value.ToString(), ColourType.neutralText), value != 1 ? "s" : "", "\n");
        }
        else
        {
            tipBlackmark.tooltipMain = string.Format("You do not yet{0}have any {1}{2} on your record", "\n",
                GameManager.GetFormattedString("Blackmarks", ColourType.salmonText), "\n");
        }
    }

    /// <summary>
    /// dynamically updates Investigations tooltip (excludes header and offsets)
    /// </summary>
    /// <param name="value"></param>
    private void UpdateInvestigationsTooltip(int value)
    {
        if (value > 0)
        {
            tipInvestigation.tooltipMain = string.Format("HQ currently pursuing the{0}{1}{2}into your behaviour", "\n",
                GameManager.GetFormattedString("following investigations", ColourType.salmonText), "\n");
            tipInvestigation.tooltipDetails = GameManager.i.playerScript.GetInvestigationTooltip();
        }
        else
        {
            tipInvestigation.tooltipMain = string.Format("HQ are {0} currently Investigating you", GameManager.GetFormattedString("NOT", ColourType.neutralText));
            tipInvestigation.tooltipDetails = "";
        }
    }

    /// <summary>
    /// dynamically updates Innocence tooltip (excludes details and offsets)
    /// </summary>
    /// <param name="value"></param>
    private void UpdateInnocenceTooltip(int value)
    {
        if (value > 0)
        {
            tipInnocence.tooltipHeader = string.Format("<size=120%>{0}</size>{1}{2}", GameManager.GetFormattedString("Innocence", ColourType.neutralText), "\n",
                GameManager.i.guiScript.GetDatapointStars(value));
            tipInnocence.tooltipMain = string.Format("Authority currently views you as a{0}{1}", "\n",
                GameManager.GetFormattedString(GameManager.i.topicScript.GetInnocenceDescriptor(), ColourType.salmonText));
        }
        else
        {
            tipInnocence.tooltipHeader = string.Format("<size=120%>{0}</size>{1}{2}", GameManager.GetFormattedString("Innocence", ColourType.neutralText), "\n",
                GameManager.i.guiScript.GetDatapointStars(value));
            tipInnocence.tooltipMain = string.Format("Authority currently views you as a{0}{1}", "\n", GameManager.i.topicScript.GetInnocenceDescriptor());
        }
    }

    /// <summary>
    /// dynamically updates Unhappy actors tooltip (exluding header and offsets)
    /// </summary>
    /// <param name="value"></param>
    private void UpdateUnhappyTooltip(int value)
    {
        if (value > 0)
        {
            tipUnhappy.tooltipMain = "You have the following Unhappy Subordinates in your RESERVES";
            tipUnhappy.tooltipDetails = GameManager.i.actorScript.GetUnhappyActorsTooltip();
        }
        else
        {
            tipUnhappy.tooltipMain = string.Format("There are currently {0} Unhappy subordinates in your {1}", GameManager.GetFormattedString("NO", ColourType.neutralText),
                GameManager.GetFormattedString("Reserves", ColourType.salmonText));
            tipUnhappy.tooltipDetails = "";
        }
    }

    /// <summary>
    /// dynamically updates potential (motivation 0) relationship Conflicts tooltip (exluding header and offsets)
    /// </summary>
    /// <param name="value"></param>
    private void UpdateConflictTooltip(int value)
    {
        if (value > 0)
        {
            tipConflict.tooltipMain = string.Format("You currently have{0}{1} Upset Subordinate{2}{3}<i>ON THE EDGE</i>", "\n",
                GameManager.GetFormattedString(value.ToString(), ColourType.neutralText), value != 1 ? "s" : "", "\n");
            tipConflict.tooltipDetails = GameManager.i.actorScript.GetConflictActorsTooltip();
        }
        else
        {
            tipConflict.tooltipMain = string.Format("You currently have {0} Upset Subordinates", GameManager.GetFormattedString("NO", ColourType.neutralText));
            tipConflict.tooltipDetails = string.Format("Conflicts occur if a Subordinates {0}{1}{2}", GameManager.GetFormattedString("MOTIVATION", ColourType.neutralText), "\n",
                GameManager.GetFormattedString("drops below ZERO", ColourType.salmonText));
        }
    }

    /// <summary>
    /// dynamically updates Blackmail tooltip (exluding header and offsets)
    /// </summary>
    /// <param name="value"></param>
    private void UpdateBlackmailTooltip(int value)
    {
        if (value > 0)
        {
            tipBlackmail.tooltipMain = string.Format("You are being Blackmailed by the following subordinate{0}", value != 1 ? "s" : "");
            tipBlackmail.tooltipDetails = GameManager.i.actorScript.GetBlackmailActorsTooltip();
        }
        else
        {
            tipBlackmail.tooltipMain = string.Format("Good news. {0} is currently Blackmailing you", GameManager.GetFormattedString("NOBODY", ColourType.neutralText));
            tipBlackmail.tooltipDetails = "";
        }
    }

    /// <summary>
    /// dynamically updates Doom timer tooltip (exluding header and offsets)
    /// </summary>
    /// <param name="value"></param>
    private void UpdateDoomTooltip(int value)
    {
        if (value > 0)
        {
            tipDoom.tooltipMain = string.Format("A <size=120%>{0}</size> drug courses through your veins", GameManager.GetFormattedString("LETHAL", ColourType.badText));
            tipDoom.tooltipDetails = string.Format("You have{0}<size=120%>{1} day{2}</size>{3}to find a cure", "\n", GameManager.GetFormattedString(value.ToString(), ColourType.badText),
                value != 1 ? "s" : "", "\n");
        }
        else
        {
            tipDoom.tooltipMain = string.Format("Luckily you have {0} been injected with a fatal drug", GameManager.GetFormattedString("NOT", ColourType.neutralText));
            tipDoom.tooltipDetails = "";
        }
    }


    //place new methods above here
}
