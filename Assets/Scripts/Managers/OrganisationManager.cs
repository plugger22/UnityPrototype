using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all 3rd Pary Organisation matters
/// </summary>
public class OrganisationManager : MonoBehaviour
{
    [Tooltip("Base % chance of an organisation being present in a city")]
    [Range(0, 100)] public int baseCityChance = 20;
    [Tooltip("Chance of org in city -> baseCityChance + perNodeChance x number of preferred nodes in city")]
    [Range(0, 10)] public int perNodeChance = 5;


    /// <summary>
    /// Initialises organisations in a city
    /// </summary>
    /// <param name="city"></param>
    public void SetOrganisationsInCity(City city)
    {
        if (city != null)
        {

        }
        else { Debug.LogError("Invalid city (Null)"); }
    }

}
