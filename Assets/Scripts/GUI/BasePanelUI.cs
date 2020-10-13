using gameAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BasePanelUI : MonoBehaviour
{

    public TextMeshProUGUI cityName;
    public TextMeshProUGUI countryName;

    private static BasePanelUI basePanelUI;


    /// <summary>
    /// provide a static reference to BasePanelUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static BasePanelUI Instance()
    {
        if (!basePanelUI)
        {
            basePanelUI = FindObjectOfType(typeof(BasePanelUI)) as BasePanelUI;
            if (!basePanelUI)
            { Debug.LogError("There needs to be one active basePanelUI script on a GameObject in your scene"); }
        }
        return basePanelUI;
    }


    public void Initialise(GameState state)
    {
        Debug.Assert(cityName != null, "Invalid cityName (Null)");
        Debug.Assert(countryName != null, "Invalid countryName (Null)");
    }


    /*public void SetNames(string nameOfCity, string nameOfCountry, byte r, byte g, byte b, byte a)
    {
        if (string.IsNullOrEmpty(nameOfCity) == false)
        {
            cityName.text = nameOfCity;
            cityName.color = new Color32(r, g, b, a);
        }
        if (string.IsNullOrEmpty(nameOfCountry) == false)
        {
            countryName.text = nameOfCountry;
            countryName.color = new Color32(r, g, b, a);
        }
    }*/

    /// <summary>
    /// Set Colour and Transparency of city and power bloc (country) names
    /// </summary>
    /// <param name="city"></param>
    public void SetBaseNames(City city)
    {
        if (city != null)
        {
            if (city.country != null)
            {
                Color color = Color.white;
                switch (city.country.name)
                {
                    case "Americon": color = GameManager.i.uiScript.Bloc1; break;
                    case "Eurasia": color = GameManager.i.uiScript.Bloc2; break;
                    case "Chinock": color = GameManager.i.uiScript.Bloc3; break;
                    default: Debug.LogWarningFormat("Unrecognised city.country.name \"{0}\"", city.country.name); break;
                }
                //transparency
                color.a = GameManager.i.guiScript.alphaBaseText;
                //texts
                cityName.text = city.tag;
                countryName.text = city.country.tag;
                //colours
                cityName.color = color;
                countryName.color = color;
            }
            else { Debug.LogErrorFormat("Invalid country (Null) for {0}", city.name); }
        }
        else { Debug.LogError("Invalid city (Null)"); }
    }
}
