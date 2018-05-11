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
    /// Initialises organisations in a city. 
    /// </summary>
    /// <param name="city"></param>
    public void SetOrganisationsInCity(City city)
    {
        if (city != null)
        {
            //empty out first as SO's are persistent
            city.ClearOrganisations();
            List<int> listOfDistrictTotals = city.GetListOfDistrictTotals();
            if (listOfDistrictTotals != null)
            {
                Dictionary<int, Organisation> dictOfOrganisations = GameManager.instance.dataScript.GetDictOfOrganisations();
                if (dictOfOrganisations != null)
                {
                    int index, chance;
                    //loop organisations
                    foreach(var org in dictOfOrganisations)
                    {
                        //get list index from organisation
                        index = org.Value.nodeArc.nodeArcID;
                        if (index >= 0 && index < listOfDistrictTotals.Count)
                        {
                            //chance is base chance + amount for each organisation's preferred node that's present in city
                            chance = baseCityChance + perNodeChance * listOfDistrictTotals[index];
                            if (Random.Range(0, 100) <= chance)
                            { city.AddOrganisation(org.Value); }
                        }
                    }
                }
                else { Debug.LogError("Invalid dictOfOrganisations (Null)"); }
            }
            else { Debug.LogError("Invalid city.listOfDistrictTotals (Null)"); }
        }
        else { Debug.LogError("Invalid city (Null)"); }
    }



}
