using gameAPI;
using modalAPI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Handles all city related matters (Each level is a city)
/// </summary>
public class CityManager : MonoBehaviour
{
    [Tooltip("City Loyalty towards the Authority (same for both sides)")]
    [Range(1, 9)] public int maxCityLoyalty = 9;

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
            Debug.LogFormat("[Cit] CityManager.cs: {0} loyalty now {1}{2}", city.name, _cityLoyalty, "\n");
            //update top widget bar
            EventManager.instance.PostNotification(EventType.ChangeCityBar, this, _cityLoyalty, "CityManager.cs -> CityLoyalty");
        }
    }

    /// <summary>
    /// need to do BEFORE levelManager.cs -> Initialise
    /// </summary>
    public void InitialiseEarly()
    {
        //get random current city -> Placeholder
        // need to do this once at very start of a new game (set up all cities, set up data in the city SO's)
        // need to get list of all cities here from mapManager.cs
        // need to initialise levels (run graphs and get node totals) and store seeds so the cities can be duplicated
        // need to initialise all relevant info in city.SO's

        city = GameManager.instance.dataScript.GetRandomCity();
        isLoyaltyCheckedThisTurn = false;
        loyaltyMinTimer = 0;
        loyaltyMaxTimer = 0;

        //Placeholder -> do early so factionManager.cs can have data in start sequence
        city.mayor = GameManager.instance.dataScript.GetRandomMayor();
        if (city.mayor != null)
        {
            city.faction = city.mayor.faction;
            Debug.LogFormat("CityManager.cs -> City {0}, {1},  Trait {2}, Faction {3}{4}", city.name, city.mayor.name, city.mayor.trait.tag, city.mayor.faction.name, "\n");
            //initialise authority faction (determined by mayor's faction
            GameManager.instance.factionScript.factionAuthority = city.mayor.faction;
        }
        else { Debug.LogError("Invalid city Mayor (Null) -> Issues with authority faction not initialising"); }

        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "CityManager");
        EventManager.instance.AddListener(EventType.EndTurnLate, OnEvent, "CityManager");
    }

    /// <summary>
    /// need to do AFTER levelManager.cs -> Initialise
    /// </summary>
    public void InitialiseLate()
    {
        CityLoyalty = city.baseLoyalty;
        //initialise number of districts
        city.SetDistrictTotals(GameManager.instance.levelScript.GetNodeTypeTotals());

        //organisations -> placeholder (should be a loop for all cities -> must be AFTER mayor and faction have been initialised
        GameManager.instance.orgScript.SetOrganisationsInCity(city);
        //set up base panel UI
        GameManager.instance.basePanelScript.SetNames(city.name, city.country.name, city.country.colour_red, city.country.colour_green, city.country.colour_blue, GameManager.instance.guiScript.alphaBaseText);
        
        
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
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourRebel = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
        //current Player side colour
        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
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
        { this.city = city; }
        else { Debug.LogError("Invalid city (Null)"); }
    }

    /// <summary>
    /// returns city name in a Player side colour formatted string   
    /// </summary>
    /// <returns></returns>
    public string GetCityName()
    { return string.Format("{0}<size=115%><b>{1}</b></size>{2}", colourSide, city.name, colourEnd); }

    /// <summary>
    /// returns current city Arc name in 90% size, default white text format
    /// </summary>
    /// <returns></returns>
    public string GetCityArc()
    { return string.Format("{0}<size=90%>{1}</size>", "\n", city.Arc.name); }


    /// <summary>
    /// returns current city loyalty level for player side in colour formatted string
    /// </summary>
    /// <returns></returns>
    public string GetCityLoyalty()
    { return string.Format("{0}{1}{2} out of {3}", colourNeutral, _cityLoyalty, colourEnd, maxCityLoyalty); }


    public string GetCityDescription()
    {
        if (city.descriptor != null)
        { return string.Format("{0}{1}{2}", colourNormal, city.descriptor, colourEnd); }
        else { return "Unknown"; }
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
                builder.AppendFormat("<b>{0}{1}{2}</b>", colourNormal, org.name, colourEnd);
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
    /// returns a colour formatted string of current cities Faction. Used by cityInfoUI faction tooltip
    /// </summary>
    /// <returns></returns>
    public string GetFactionName()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<size=115%><b>{1}</b></size>{2}", colourSide, city.faction.name, colourEnd);
        if (string.IsNullOrEmpty(city.faction.descriptor) == false)
        { builder.AppendFormat("{0}{1}{2}{3}", "\n", colourAlert, city.faction.descriptor, colourEnd); }
        return builder.ToString();
    }

    /// <summary>
    /// returns a colour formatted string of current city's faction details (preferred node Arc, disliked node Arc). Used by cityInfoUI faction tooltip.
    /// </summary>
    /// <returns></returns>
    public string GetFactionDetails()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}Prefers{1}", colourNeutral, colourEnd);
        builder.AppendLine();
        if (city.faction.preferredArc != null)
        { builder.AppendFormat("{0}{1}{2} Districts", colourGood, city.faction.preferredArc.name, colourEnd); }
        else { builder.AppendFormat("{0}None{1}", colourGrey, colourEnd); }
        builder.AppendLine();
        builder.AppendFormat("{0}Dislikes{1}", colourNeutral, colourEnd);
        builder.AppendLine();
        if (city.faction.hostileArc != null)
        { builder.AppendFormat("{0}{1}{2} Districts", colourBad, city.faction.preferredArc.name, colourEnd); }
        else { builder.AppendFormat("{0}None{1}", colourGrey, colourEnd); }
        builder.AppendLine();
        return builder.ToString();
    }

    /// <summary>
    /// returns a colour formatted string of current cities Mayor. Used by cityInfoUI mayor tooltip
    /// </summary>
    /// <returns></returns>
    public string GetMayorName()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<size=115%><b>{1}</size></b>{2}", colourSide, city.mayor.name, colourEnd);
        if (string.IsNullOrEmpty(city.mayor.motto) == false)
        { builder.AppendFormat("{0}{1}{2}{3}", "\n", colourAlert, city.mayor.motto, colourEnd); }
        return builder.ToString();
    }

    /// <summary>
    /// returns a colour formatted string of Mayor's faction alignment. Used by cityInfoUI mayor tooltip
    /// </summary>
    /// <returns></returns>
    public string GetMayorFaction()
    { return string.Format("{0}Faction Alignment{1}{2}<b>{3}</b>", colourNeutral, colourEnd, "\n", city.faction.name); }

    /// <summary>
    /// returns a colour formatted string of the Mayor's trait. Used by cityInfoUI mayor tooltip
    /// </summary>
    /// <returns></returns>
    public string GetMayorTrait()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<font=\"Bangers SDF\"><cspace=1em>{0}</cspace></font>", city.mayor.GetTrait().tagFormatted);
        builder.AppendFormat("{0}{1}{2}{3}", "\n", colourAlert, city.mayor.GetTrait().description, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// returns a colour formatted string of the Faction's trait. Used by cityInfoUI faction tooltip
    /// </summary>
    /// <returns></returns>
    public string GetFactionTrait()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<font=\"Bangers SDF\"><cspace=1em>{0}</cspace></font>", city.faction.GetTrait().tagFormatted);
        builder.AppendFormat("{0}{1}{2}{3}", "\n", colourAlert, city.faction.GetTrait().description, colourEnd);
        return builder.ToString();
    }

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
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
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
                    msgText = string.Format("{0} Loyalty at zero. Resistance wins in {1} turn{2}", city.name, loyaltyMinTimer, loyaltyMinTimer != 1 ? "s" : "");
                    itemText = string.Format("{0} Loyalty at ZERO", city.name);
                    reason = string.Format("{0} Loyalty has Flat Lined", city.name);
                    warning = string.Format("Resistance wins in {0} turn{1}", loyaltyMinTimer, loyaltyMinTimer != 1 ? "s" : "");
                    //good for Resistance, bad for Authority player
                    isBad = false;
                    if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level) { isBad = true; }
                    GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "City Loyalty Crisis", reason, warning, true, isBad);
                }
                else
                {
                    //decrement timer
                    loyaltyMinTimer--;
                    //game over at zero
                    if (loyaltyMinTimer == 0)
                    {
                        //message
                        GameManager.instance.win = WinState.Resistance;
                        msgText = string.Format("{0} Loyalty at zero. Resistance wins", city.name);
                        itemText = string.Format("{0} has aligned with Resistance", city.name);
                        reason = string.Format("{0} Loyalty has been at ZERO for an extended period", city.name);
                        warning = "Resistance has Won";
                        //good for Resistance, bad for Authority player
                        isBad = false;
                        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level) { isBad = true; }
                        GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Resistance Wins", reason, warning, true, isBad);
                        //Loyalty at Min -> Resistance wins
                        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
                        outcomeDetails.side = side;
                        outcomeDetails.textTop = string.Format("{0}{1} has lost faith in the Authorities and joined the Resistance{2}", colourNormal, city.name, colourEnd);
                        outcomeDetails.textBottom = string.Format("{0}Resistance WINS{1}{2}{3}{4}Authority LOSES{5}", colourGood, colourEnd, "\n", "\n", colourBad, colourEnd);
                        outcomeDetails.sprite = GameManager.instance.guiScript.firedSprite;
                        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "CityManager.cs -> CheckCityLoyalty");

                    }
                    else
                    {
                        //message
                        msgText = string.Format("{0} Loyalty at zero. Resistance wins in {1} turn{2}", city.name, loyaltyMinTimer, loyaltyMinTimer != 1 ? "s" : "");
                        itemText = string.Format("{0} Loyalty is wavering", city.name);
                        reason = string.Format("{0} is considering aligning itself with the Resistance", city.name);
                        warning = string.Format("Resistance wins in {0} turn{1}", loyaltyMinTimer, loyaltyMinTimer != 1 ? "s" : "");
                        //good for Resistance, bad for Authority player
                        isBad = false;
                        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level) { isBad = true; }
                        GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Loyalty at Zero", reason, warning, true, isBad);
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
                    msgText = string.Format("{0} Loyalty at MAX. Authority wins in {1} turn{2}", city.name, loyaltyMaxTimer, loyaltyMaxTimer != 1 ? "s" : "");
                    itemText = string.Format("{0} Loyalty is at MAXIMUM", city.name);
                    reason = string.Format("{0} Loyalty to the Authority is overwhelming", city.name);
                    warning = string.Format("Authority wins in {0} turn{1}", loyaltyMaxTimer, loyaltyMaxTimer != 1 ? "s" : "");
                    //good for Authority, bad for Resistance player
                    isBad = true;
                    if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level) { isBad = false; }
                    GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Maximum Loyalty", reason, warning, true, isBad);
                }
                else
                {
                    //decrement timer
                    loyaltyMaxTimer--;
                    //game over at zero
                    if (loyaltyMaxTimer == 0)
                    {
                        GameManager.instance.win = WinState.Authority;
                        //message
                        msgText = string.Format("{0} Loyalty at MAX. Authority wins", city.name);
                        itemText = string.Format("{0} has aligned fully with Authority", city.name);
                        reason = string.Format("{0} Loyalty has been MAX for an extended period", city.name);
                        warning = "Authority has Won";
                        //good for Authority, bad for Resistance player
                        isBad = true;
                        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level) { isBad = false; }
                        GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Authority Wins", reason, warning, true, isBad);
                        //Loyalty at Max -> Authority wins
                        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
                        outcomeDetails.side = side;
                        outcomeDetails.textTop = string.Format("{0}{1} has lost all interest in joining the Resistance{2}", colourNormal, city.name, colourEnd);
                        outcomeDetails.textBottom = string.Format("{0}Authority WINS{1}{2}{3}{4}Resistance LOSES{5}", colourGood, colourEnd, "\n", "\n", colourBad, colourEnd);
                        outcomeDetails.sprite = GameManager.instance.guiScript.firedSprite;
                        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "CityManager.cs -> CheckCityLoyalty");

                    }
                    else
                    {
                        //message
                        msgText = string.Format("{0} Loyalty at MAX. Authority wins in {1} turn{2}", city.name, loyaltyMaxTimer, loyaltyMaxTimer != 1 ? "s" : "");
                        itemText = string.Format("{0} Loyalty is overwhelming", city.name);
                        reason = string.Format("{0} is considering aligning itself fully with the Authority", city.name);
                        warning = string.Format("Authority wins in {0} turn{1}", loyaltyMaxTimer, loyaltyMaxTimer != 1 ? "s" : "");
                        //good for Authority, bad for Resistance player
                        isBad = true;
                        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level) { isBad = false; }
                        GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Loyalty MAXXED", reason, warning, true, isBad);
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

    //new methods above here
}