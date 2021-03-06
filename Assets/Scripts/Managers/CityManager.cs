﻿using gameAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Handles all city related matters (Each level is a city)
/// </summary>
public class CityManager : MonoBehaviour
{
    [Tooltip("City Loyalty towards the Authority (same for both sides)")]
    [Range(1, 10)] public int maxCityLoyalty = 10;

    [Header("GUI data")]
    [Tooltip("Opacity of the 3 grey background subPanels in the city tooltip")]
    [Range(0f, 1.0f)] public float subPanelOpacity = 0.1f;
    [Tooltip("Opacity of the 3 grey background miniPanels (centre panel -> Mayor / Faction / Orgs) in the city tooltip")]
    [Range(0f, 1.0f)] public float miniPanelOpacity = 0.25f;

    [Header("Timers")]
    [Tooltip("Countdown timer, in turns, triggered when City Loyalty at Max or Min")]
    [Range(1, 10)] public int loyaltyCountdownTimer = 3;

    //nodeID's of special city districts -> assigned by LevelManager.cs -> InitialiseDistrictNames. If default value of '-1' then no special district of that type exists
    [HideInInspector] public int mayorDistrictID = -1;   //same as city hall at start of game but mayor can move around. Mayor's current locatoin
    [HideInInspector] public int cityHallDistrictID = -1;
    [HideInInspector] public int airportDistrictID = -1;
    [HideInInspector] public int harbourDistrictID = -1;
    [HideInInspector] public int iconDistrictID = -1;

    private int _cityLoyalty;                       //loyalty of city (0 to 10). Same number for both sides
    private bool isLoyaltyCheckedThisTurn;          //ensures that CheckCityLoyaltyAtLimit is checked only once per turn
    private int loyaltyMinTimer;                    //countdown timer that triggers when loyalty at min
    private int loyaltyMaxTimer;                    //countdown timer that triggers when loyalty at max

    private City city;

    private string colourRebel;
    private string colourAuthority;
    private string colourNeutral;
    private string colourNormal;
    private string colourGood;
    private string colourBad;
    private string colourGrey;
    private string colourAlert;
    private string colourSide;
    private string colourEnd;

    public int CityLoyalty
    {
        get { return _cityLoyalty; }
        set
        {
            _cityLoyalty = value;
            Debug.LogFormat("[Cit] CityManager.cs: {0} loyalty now {1}{2}", city.tag, _cityLoyalty, "\n");
            _cityLoyalty = Mathf.Clamp(_cityLoyalty, 0, 10);
            //update top widget bar and city icon
            EventManager.i.PostNotification(EventType.ChangeCityBar, this, _cityLoyalty, "CityManager.cs -> CityLoyalty");
        }
    }

    /// <summary>
    /// need to do BEFORE levelManager.cs -> Initialise. 
    /// NOTE: run from CampaignManager.InitialiseEarly
    /// </summary>
    public void InitialiseEarly(Mayor mayor)
    {
        switch (GameManager.i.inputScript.GameState)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
                SubInitialiseAllEarly(mayor);
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseAllEarly(mayor);
                break;
            case GameState.LoadAtStart:
                SubInitialiseAllEarly(mayor);
                SubInitialiseEvents();
                break;
            case GameState.LoadGame:
                SubInitialiseAllEarly(mayor);
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    /// <summary>
    /// need to do AFTER levelManager.cs -> Initialise. 
    /// NOTE: run from CampaignManager.InitialiseLate
    /// </summary>
    public void InitialiseLate()
    {
        switch (GameManager.i.inputScript.GameState)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
                SubInitialiseLevelStart();
                SubInitialiseAllLate();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseLevelStart();
                SubInitialiseAllLate();
                break;
            case GameState.LoadAtStart:
                SubInitialiseLevelStart();
                SubInitialiseAllLate();
                break;
            case GameState.LoadGame:
                SubInitialiseAllLate();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseAllEarly
    private void SubInitialiseAllEarly(Mayor mayor)
    {
        //use a random city if GameManager dev option set true, uses Scenario specified city otherwise
        if (GameManager.i.isRandomCity == true && GameManager.i.inputScript.GameState == GameState.NewInitialisation)
        { city = GameManager.i.dataScript.GetRandomCity(); }

        isLoyaltyCheckedThisTurn = false;
        loyaltyMinTimer = 0;
        loyaltyMaxTimer = 0;

        //Placeholder -> do early so factionManager.cs can have data in start sequence
        city.mayor = mayor;
        if (city.mayor != null)
        { Debug.LogFormat("[Cit] CityManager.cs -> City {0}, {1},  Trait {2}{3}", city.tag, city.mayor.mayorName, city.mayor.trait.tag, "\n"); }
        else { Debug.LogError("Invalid city Mayor (Null) -> Issues with authority faction not initialising"); }
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "CityManager");
        EventManager.i.AddListener(EventType.EndTurnLate, OnEvent, "CityManager");
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        //assign city loyalty (if input value zero then use a random value between 2 & 8 inclusive)
        int loyalty = GameManager.i.scenarioScript.scenario.cityStartLoyalty;
        if (loyalty == 0) { loyalty = Random.Range(2, 9); }
        CityLoyalty = loyalty;
    }
    #endregion

    #region SubInitialiseAllLate
    private void SubInitialiseAllLate()
    {
        //initialise number of districts
        city.SetDistrictTotals(GameManager.i.dataScript.GetNodeTypeTotals());
        //set up base panel UI
        GameManager.i.basePanelScript.SetBaseNames(city);
    }
    #endregion

    #endregion

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
            case EventType.EndTurnLate:
                EndTurnLate();
                break;
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourAuthority = GameManager.i.colourScript.GetColour(ColourType.badText);
        colourRebel = GameManager.i.colourScript.GetColour(ColourType.blueText);
        colourGrey = GameManager.i.colourScript.GetColour(ColourType.greyText);
        colourGood = GameManager.i.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.dataBad);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        colourNormal = GameManager.i.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
        //current Player side colour
        if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; }
        else { colourSide = colourRebel; }
    }

    /// <summary>
    /// End turn late event
    /// </summary>
    private void EndTurnLate()
    {
        //reset flag ready for next turn
        isLoyaltyCheckedThisTurn = false;
    }

    /// <summary>
    /// gets current city, null if none
    /// </summary>
    /// <returns></returns>
    public City GetCity()
    { return city; }

    public void SetCity(City city)
    {
        if (city != null)
        {
            this.city = city;
            /*Debug.LogFormat("[Cit] CityManager.cs -> SetCity: {0}{1}", city.tag, "\n");*/
        }
        else { Debug.LogError("Invalid city (Null)"); }
    }

    /// <summary>
    /// returns city name in a Player side colour formatted string   
    /// </summary>
    /// <returns></returns>
    public string GetCityNameFormatted()
    { return string.Format("{0}<size=115%><b>{1}</b></size>{2}", colourSide, city.tag, colourEnd); }


    public string GetCityName()
    { return city.tag; }

    /// <summary>
    /// returns current city Arc name in 90% size, default white text format
    /// </summary>
    /// <returns></returns>
    public string GetCityArcFormatted()
    { return string.Format("{0}<size=90%>{1}</size>", "\n", city.Arc.name); }


    /// <summary>
    /// returns current city loyalty level for player side in colour formatted string
    /// </summary>
    /// <returns></returns>
    public string GetCityLoyaltyFormatted()
    { return string.Format("{0}<size=130%>{1}{2} out of {3}</size>", colourNeutral, _cityLoyalty, colourEnd, maxCityLoyalty); }


    public string GetCityDescriptionFormatted()
    {
        if (city.descriptor != null)
        { return string.Format("{0}{1}{2}", colourNormal, city.descriptor, colourEnd); }
        else { return "Unknown"; }
    }

    /// <summary>
    /// Used for top Widget city Icon and city Bar tooltips if city loyalty critical. 'isBarTooltip' true, show default message, ignore for city icon tooltip
    /// </summary>
    /// <returns></returns>
    public string GetCityDetails(bool isBarTooltip = false)
    {
        string text = "";
        if (_cityLoyalty == 0 || _cityLoyalty == 10 && isLoyaltyCheckedThisTurn == true)
        {
            switch (GameManager.i.sideScript.PlayerSide.level)
            {
                case 1:
                    //Authority
                    switch (_cityLoyalty)
                    {
                        case 0:
                            text = string.Format("{0}Resistance wins{1}{2}in {3}{4}{5} turn{6}", colourBad, colourEnd, "\n", colourNeutral, loyaltyMinTimer, colourEnd,
                              loyaltyMinTimer != 1 ? "s" : "");
                            break;
                        case 10:
                            text = string.Format("{0}Authority wins{1}{2}in {3}{4}{5} turn{6}", colourGood, colourEnd, "\n", colourNeutral, loyaltyMaxTimer, colourEnd,
                                loyaltyMaxTimer != 1 ? "s" : "");
                            break;
                    }
                    break;
                case 2:
                    //Resistance
                    switch (_cityLoyalty)
                    {
                        case 0:
                            text = string.Format("{0}Resistance wins{1}{2}in {3}{4}{5} turn{6}", colourGood, colourEnd, "\n", colourNeutral, loyaltyMinTimer, colourEnd,
                                loyaltyMinTimer != 1 ? "s" : "");
                            break;
                        case 10:
                            text = string.Format("{0}Authority wins{1}{2}in {3}{4}{5} turn{6}", colourBad, colourEnd, "\n", colourNeutral, loyaltyMaxTimer, colourEnd,
                                loyaltyMaxTimer != 1 ? "s" : "");
                            break;
                    }
                    break;
            }
        }
        else
        {
            //Non critical
            if (isBarTooltip == true)
            {
                //show default win/loss message
                text = string.Format("At {0}Zero{1}{2}{3}Resistance wins Level{4}{5}At {6}10{7}{8}{9}Authority wins Level{10}{11}after {12}Countdowns{13}",
                    colourNeutral, colourEnd, "\n", colourAlert, colourEnd, "\n", colourNeutral, colourEnd, "\n", colourAlert, colourEnd, "\n", colourNeutral, colourEnd);
            }
        }
        return text;
    }

    /// <summary>
    /// Returns name of city in next scenario (one after the current scenario). If on last scenario in campaign will return 'Unknown' as there is no next
    /// </summary>
    /// <returns></returns>
    public string GetNextCityName()
    {
        string cityName = "Unknown";
        int scenarioIndex = GameManager.i.campaignScript.GetScenarioIndex();
        if (scenarioIndex < GameManager.i.campaignScript.GetMaxScenarioIndex())
        { cityName = GameManager.i.campaignScript.GetScenario(scenarioIndex + 1).city.tag; }
        return cityName;
    }



    /// <summary>
    /// returns colour formatted string detailed # of organisations active in city
    /// </summary>
    /// <returns></returns>
    public string GetNumOfOrganisations()
    { return string.Format("{0}{1}{2} present", colourNeutral, city.CheckNumOfOrganisations(), colourEnd); }


    /// <summary>
    /// returns a colour formatted string of all the organisation within a city. Used by cityInfoUI organisations tooltip
    /// </summary>
    /// <param name="city"></param>
    /// <returns></returns>
    public string GetOrganisationsTooltip()
    {
        StringBuilder builder = new StringBuilder();
        List<Organisation> listOfOrganisations = city.GetListOfOrganisations();
        if (listOfOrganisations != null || listOfOrganisations.Count == 0)
        {
            foreach (Organisation org in listOfOrganisations)
            {
                if (builder.Length > 0) { builder.AppendLine(); }
                builder.AppendFormat("<b>{0}{1}{2}</b>", colourNormal, org.tag, colourEnd);
                if (string.IsNullOrEmpty(org.descriptor) == false)
                { builder.AppendFormat("{0}{1}<size=95%>{2}</size>{3}", "\n", colourAlert, org.descriptor, colourEnd); }
            }
        }
        else
        {
            Debug.LogWarning("Invalid listOfOrganisations (Null)");
            builder.AppendFormat("{0}None present{1}", colourGrey, colourEnd);
        }
        return builder.ToString();
    }


    /// <summary>
    /// returns a colour formatted string of current city's faction details (preferred node Arc, disliked node Arc). Used by cityInfoUI faction tooltip.
    /// </summary>
    /// <returns></returns>
    public string GetFactionDetails()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("Faction Details{0}{1}To be Done{2}", "\n", colourAlert, colourEnd);
        /*builder.AppendFormat("{0}Prefers{1}", colourNeutral, colourEnd);
        builder.AppendLine();
        if (city.mayor.preferredArc != null)
        { builder.AppendFormat("{0}{1}{2} Districts", colourGood, city.mayor.preferredArc.name, colourEnd); }
        else { builder.AppendFormat("{0}None{1}", colourGrey, colourEnd); }
        builder.AppendLine();
        builder.AppendFormat("{0}Dislikes{1}", colourNeutral, colourEnd);
        builder.AppendLine();
        if (city.faction.hostileArc != null)
        { builder.AppendFormat("{0}{1}{2} Districts", colourBad, city.faction.preferredArc.name, colourEnd); }
        else { builder.AppendFormat("{0}None{1}", colourGrey, colourEnd); }*/
        builder.AppendLine();
        return builder.ToString();
    }

    /// <summary>
    /// returns a colour formatted string of current cities Mayor. Used by cityInfoUI mayor tooltip
    /// </summary>
    /// <returns></returns>
    public string GetMayorNameFormatted()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<size=115%><b>{1}</size></b>{2}", colourSide, city.mayor.mayorName, colourEnd);
        if (string.IsNullOrEmpty(city.mayor.motto) == false)
        { builder.AppendFormat("{0}{1}{2}{3}", "\n", colourAlert, city.mayor.motto, colourEnd); }
        return builder.ToString();
    }

    /// <summary>
    /// Name of mayor (unformatted)
    /// </summary>
    /// <returns></returns>
    public string GetMayorName()
    { return city.mayor.mayorName; }

    /// <summary>
    /// returns a colour formatted string of Mayor's faction alignment. Used by cityInfoUI mayor tooltip
    /// </summary>
    /// <returns></returns>
    public string GetMayorFactionFormatted()
    { return string.Format("{0}Faction Alignment{1}{2}<b>{3}</b>", colourNeutral, colourEnd, "\n", "Placeholder"); }

    /// <summary>
    /// returns a colour formatted string of the Mayor's trait. Used by cityInfoUI mayor tooltip
    /// </summary>
    /// <returns></returns>
    public string GetMayorTraitFormatted()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<font=\"Bangers SDF\"><cspace=1em>{0}</cspace></font>", city.mayor.GetTrait().tagFormatted);
        builder.AppendFormat("{0}{1}{2}{3}", "\n", colourAlert, city.mayor.GetTrait().description, colourEnd);
        return builder.ToString();
    }

    /* /// <summary>
     /// returns a colour formatted string of the Faction's trait. Used by cityInfoUI faction tooltip
     /// </summary>
     /// <returns></returns>
     public string GetFactionTraitFormatted()
     {
         StringBuilder builder = new StringBuilder();
         builder.AppendFormat("<font=\"Bangers SDF\"><cspace=1em>{0}</cspace></font>", city.faction.GetTrait().tagFormatted);
         builder.AppendFormat("{0}{1}{2}{3}", "\n", colourAlert, city.faction.GetTrait().description, colourEnd);
         return builder.ToString();
     }*/

    /// <summary>
    /// checks city loyalty once per turn for min and max conditions, sets timers, gives outcomes. Checks for both sides, depending on who is player
    /// </summary>
    public void CheckCityLoyaltyAtLimit()
    {
        bool isAtLimit = false;
        bool isMaxLoyalty = false;
        bool isMinLoyalty = false;
        string msgText, itemText, reason, warning;
        bool isBad;
        GlobalSide side = GameManager.i.sideScript.PlayerSide;
        //check if loyalty at limit
        if (_cityLoyalty == 0) { isAtLimit = true; isMinLoyalty = true; }
        else if (_cityLoyalty >= maxCityLoyalty) { isAtLimit = true; isMaxLoyalty = true; }
        //only check once per turn
        if (isAtLimit == true && isLoyaltyCheckedThisTurn == false)
        {
            isLoyaltyCheckedThisTurn = true;
            //
            // - - - Min Loyalty - - -
            //
            if (isMinLoyalty == true)
            {
                if (loyaltyMinTimer == 0)
                {
                    //set timer
                    loyaltyMinTimer = loyaltyCountdownTimer;
                    //message
                    msgText = string.Format("{0} Loyalty at zero. Resistance wins in {1} turn{2}", city.tag, loyaltyMinTimer, loyaltyMinTimer != 1 ? "s" : "");
                    itemText = string.Format("{0} Loyalty at ZERO", city.tag);
                    reason = string.Format("{0} Loyalty has Flat Lined", city.tag);
                    warning = string.Format("Resistance wins in {0} turn{1}", loyaltyMinTimer, loyaltyMinTimer != 1 ? "s" : "");
                    //good for Resistance, bad for Authority player
                    isBad = false;
                    if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideAuthority.level) { isBad = true; }
                    GameManager.i.messageScript.GeneralWarning(msgText, itemText, "City Loyalty Crisis", reason, warning, true, isBad);
                }
                else
                {
                    //decrement timer
                    loyaltyMinTimer--;
                    //game over at zero
                    if (loyaltyMinTimer == 0)
                    {
                        //message
                        msgText = string.Format("{0} Loyalty at zero. Resistance wins", city.tag);
                        itemText = string.Format("{0} has aligned with Resistance", city.tag);
                        reason = string.Format("{0} Loyalty has been at ZERO for an extended period", city.tag);
                        warning = "Resistance has Won";
                        //good for Resistance, bad for Authority player
                        isBad = false;
                        if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideAuthority.level) { isBad = true; }
                        GameManager.i.messageScript.GeneralWarning(msgText, itemText, "Resistance Wins", reason, warning, true, isBad);
                        //Loyalty at Min -> Resistance wins
                        string textTop = string.Format("{0}{1} has lost faith in the Authorities and joined the Resistance{2}", colourNormal, city.tag, colourEnd);
                        string textBottom = string.Format("{0}Resistance WINS{1}{2}{3}{4}Authority LOSES{5}", colourGood, colourEnd, "\n", "\n", colourBad, colourEnd);
                        GameManager.i.turnScript.SetWinStateLevel(WinStateLevel.Resistance, WinReasonLevel.CityLoyaltyMin, textTop, textBottom);


                    }
                    else
                    {
                        //message
                        msgText = string.Format("{0} Loyalty at zero. Resistance wins in {1} turn{2}", city.tag, loyaltyMinTimer, loyaltyMinTimer != 1 ? "s" : "");
                        itemText = string.Format("{0} Loyalty is wavering", city.tag);
                        reason = string.Format("{0} is considering aligning itself with the Resistance", city.tag);
                        warning = string.Format("Resistance wins in {0} turn{1}", loyaltyMinTimer, loyaltyMinTimer != 1 ? "s" : "");
                        //good for Resistance, bad for Authority player
                        isBad = false;
                        if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideAuthority.level) { isBad = true; }
                        GameManager.i.messageScript.GeneralWarning(msgText, itemText, "Loyalty at Zero", reason, warning, true, isBad);
                    }
                }
            }
            //
            // - - - Max Loyalty - - -
            //
            else if (isMaxLoyalty == true)
            {
                if (loyaltyMaxTimer == 0)
                {
                    //set timer
                    loyaltyMaxTimer = loyaltyCountdownTimer;
                    //message
                    msgText = string.Format("{0} Loyalty at MAX. Authority wins in {1} turn{2}", city.tag, loyaltyMaxTimer, loyaltyMaxTimer != 1 ? "s" : "");
                    itemText = string.Format("{0} Loyalty is at MAXIMUM", city.tag);
                    reason = string.Format("{0} Loyalty to the Authority is overwhelming", city.tag);
                    warning = string.Format("Authority wins in {0} turn{1}", loyaltyMaxTimer, loyaltyMaxTimer != 1 ? "s" : "");
                    //good for Authority, bad for Resistance player
                    isBad = true;
                    if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideAuthority.level) { isBad = false; }
                    GameManager.i.messageScript.GeneralWarning(msgText, itemText, "Maximum Loyalty", reason, warning, true, isBad);
                }
                else
                {
                    //decrement timer
                    loyaltyMaxTimer--;
                    //game over at zero
                    if (loyaltyMaxTimer == 0)
                    {
                        //message
                        msgText = string.Format("{0} Loyalty at MAX. Authority wins", city.tag);
                        itemText = string.Format("{0} has aligned fully with Authority", city.tag);
                        reason = string.Format("{0} Loyalty has been MAX for an extended period", city.tag);
                        warning = "Authority has Won";
                        //good for Authority, bad for Resistance player
                        isBad = true;
                        if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideAuthority.level) { isBad = false; }
                        GameManager.i.messageScript.GeneralWarning(msgText, itemText, "Authority Wins", reason, warning, true, isBad);
                        //Loyalty at Max -> Authority wins
                        string textTop = string.Format("{0}{1} has lost all interest in joining the Resistance{2}", colourNormal, city.tag, colourEnd);
                        string textBottom = string.Format("{0}Authority WINS{1}{2}{3}{4}Resistance LOSES{5}", colourGood, colourEnd, "\n", "\n", colourBad, colourEnd);
                        GameManager.i.turnScript.SetWinStateLevel(WinStateLevel.Authority, WinReasonLevel.CityLoyaltyMax, textTop, textBottom);
                    }
                    else
                    {
                        //message
                        msgText = string.Format("{0} Loyalty at MAX. Authority wins in {1} turn{2}", city.tag, loyaltyMaxTimer, loyaltyMaxTimer != 1 ? "s" : "");
                        itemText = string.Format("{0} Loyalty is overwhelming", city.tag);
                        reason = string.Format("{0} is considering aligning itself fully with the Authority", city.tag);
                        warning = string.Format("Authority wins in {0} turn{1}", loyaltyMaxTimer, loyaltyMaxTimer != 1 ? "s" : "");
                        //good for Authority, bad for Resistance player
                        isBad = true;
                        if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideAuthority.level) { isBad = false; }
                        GameManager.i.messageScript.GeneralWarning(msgText, itemText, "Loyalty MAXXED", reason, warning, true, isBad);
                    }
                }
            }
        }
        else
        {
            //set both timers to zero (loyalty neither min nor max)
            loyaltyMinTimer = 0;
            loyaltyMaxTimer = 0;
        }
    }

    /// <summary>
    /// returns name set for the city. Null if a problem
    /// </summary>
    /// <returns></returns>
    public NameSet GetNameSet()
    { return city.nameSet; }


    /// <summary>
    /// Set city loyalty to a new value (Debug Action Menu). Auto clamps value to allowable limits
    /// </summary>
    /// <param name="input_0"></param>
    /// <returns></returns>
    public string DebugSetLoyalty(string input_0)
    {
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        string reply = "";
        int newLoyalty = -1;
        try
        { newLoyalty = Convert.ToInt32(input_0); }
        catch (OverflowException)
        { reply = $"Invalid City Loyalty input {input_0}"; }
        newLoyalty = Mathf.Clamp(newLoyalty, 0, 10);
        CityLoyalty = newLoyalty;
        reply = $"City Loyalty is now {_cityLoyalty} (DEBUG)";
        return reply;
    }

    //new methods above here
}