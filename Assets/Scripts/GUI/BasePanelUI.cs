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


    public void Initialise()
    {
        Debug.Assert(cityName != null, "Invalid cityName (Null)");
        Debug.Assert(countryName != null, "Invalid countryName (Null)");
    }


    public void SetNames(string nameOfCity, string nameOfCountry, byte r, byte g, byte b, byte a)
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
    }
}
