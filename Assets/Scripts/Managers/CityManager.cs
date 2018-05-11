using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Handles all city related matters (Each level is a city)
/// </summary>
public class CityManager : MonoBehaviour
{
    [Tooltip("City Loyalty (same for both sides) range from 0 to this amount")]
    [Range(0, 10)] public int maxCityLoyalty = 10;
    [Tooltip("Opacity of the 3 grey background subPanels in the city tooltip")]
    [Range(0f, 1.0f)] public float subPanelOpacity = 0.1f;
    [Tooltip("Opacity of the 3 grey background miniPanels (centre panel -> Mayor / Faction / Orgs) in the city tooltip")]
    [Range(0f, 1.0f)] public float miniPanelOpacity = 0.25f;

    private int _cityLoyalty;                       //loyalty of city (0 to 10). Same number for both sides

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
            //update top widget bar
            EventManager.instance.PostNotification(EventType.ChangeCityBar, this, _cityLoyalty);
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
        

        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
    }

    /// <summary>
    /// need to do AFTER levelManager.cs -> Initialise
    /// </summary>
    public void InitialiseLate()
    {
        CityLoyalty = city.baseLoyalty;
        //initialise number of districts
        city.SetDistrictTotals(GameManager.instance.levelScript.GetNodeTypeTotals());
        //Placeholder
        city.mayor = GameManager.instance.dataScript.GetRandomMayor();
        if (city.mayor != null)
        { city.faction = city.mayor.faction; }
        else { Debug.LogError("Invalid city Mayor (Null)"); }
        //organisations -> placeholder (should be a loop for all cities -> must be AFTER mayor and faction have been initialised
        GameManager.instance.orgScript.SetOrganisationsInCity(city);
        
        
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
    /// gets current city, null if none
    /// </summary>
    /// <returns></returns>
    public City GetCity()
    { return city; }

    /// <summary>
    /// returns city name in a Player side colour formatted string   
    /// </summary>
    /// <returns></returns>
    public string GetCityName()
    {
        string description = "Unknown";
        switch (GameManager.instance.sideScript.PlayerSide.level)
        {
            case 1:
                description = string.Format("<b>{0}{1}{2}</b>", colourSide, city.name, colourEnd);
                break;
            case 2:
                description = string.Format("<b>{0}{1}{2}</b>", colourSide, city.name, colourEnd);
                break;
            default:
                description = string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name);
                break;
        }
        return description;
    }

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
    { return string.Format("{0}{1}{2} out of {1}", colourNeutral, _cityLoyalty, colourEnd, maxCityLoyalty); }


    public string GetCityDescription()
    {
        if (city.descriptor != null)
        { return string.Format("{0}{1}{2}", colourNormal, city.descriptor, colourEnd); }
        else { return "Unknown"; }
    }

    /// <summary>
    /// returns a colour formatted string of all the organisation within a city. Used by cityInfoUI organisations tooltip
    /// </summary>
    /// <param name="city"></param>
    /// <returns></returns>
    public string GetOrganisations()
    {
        StringBuilder builder = new StringBuilder();
        List<Organisation> listOfOrganisations = city.GetListOfOrganisations();
        if (listOfOrganisations != null)
        {
            foreach (Organisation org in listOfOrganisations)
            {
                if (builder.Length > 0) { builder.AppendLine(); }
                builder.AppendFormat("<b>{0}{1}{2}</b>", colourNormal, org.name, colourEnd);
            }
        }
        else
        {
            Debug.LogWarning("Invalid listOfOrganisations (Null)");
            builder.Append("None present");
        }
        return builder.ToString();
    }


    //new methods above here
}