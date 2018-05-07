using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all city related matters (Each level is a city)
/// </summary>
public class CityManager : MonoBehaviour
{
    [Tooltip("City Loyalty (same for both sides) range from 0 to this amount")]
    [Range(0, 10)] public int maxSupportLevelCity = 10;

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


    public void Initialise()
    {
        //get random city -> Placeholder
        city = GameManager.instance.dataScript.GetRandomCity();
        Debug.Assert(city != null, "Invalid City (Null)");
        CityLoyalty = city.baseLoyalty;
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
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
                Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                break;
        }
        return description;
    }


    /// <summary>
    /// returns current city loyalty level for player side in colour formatted string
    /// </summary>
    /// <returns></returns>
    public string GetCityLoyalty()
    { return string.Format("{0}{1}{2} out of {1}", colourNeutral, _cityLoyalty, colourEnd, maxSupportLevelCity); }


    public string GetCityDescription()
    {
        if (city.descriptor != null)
        { return string.Format("{0}{1}{2}", colourNormal, city.descriptor, colourEnd); }
        else { return "Unknown"; }
    }

    //new methods above here
}