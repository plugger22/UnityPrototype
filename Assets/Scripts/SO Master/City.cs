using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Cities. Name of SO is the name of the city, eg. "Gotham City"
/// </summary>
[CreateAssetMenu(menuName = "Game / City / City")]
public class City : ScriptableObject
{
    [Tooltip("Short text summary that appears in city tooltip")]
    public string descriptor;
    [Tooltip("Starting loyalty to the Authorities of the City (10 is total loyalty)")]
    [Range(0, 10)] public int baseLoyalty = 10;

    /*[Tooltip("Chance of a connection having a high security level (more than 'None')")]
    [Range(0, 100)] public int connectionSecurityChance = 25;*/

    [Tooltip("City Arc determines the size, layout and node type frequencies of the city")]
    public CityArc Arc;
    public Country country;

    
    [HideInInspector] public int cityID;         //dynamically assigned by ImportManager.cs

    //dynamically assigned data
    [HideInInspector] public Mayor mayor;                                         //alignment of mayor determines which faction is in charge of the city
    [HideInInspector] public Faction faction;                                     //ruling faction of current city (derived from mayor)

    private List<Organisation> listOfOrganisations = new List<Organisation>();    //organisations present in the city
    private List<int> listOfDistrictTotals = new List<int>();       //cityManager.cs assigns this data (needs to be in the same order as DataManager.cs -> dictOfNodeArc's)

    /// <summary>
    /// initialise district totals
    /// </summary>
    /// <param name="tempList"></param>
    public void SetDistrictTotals(int[] tempArray)
    {
        int counter = 0;
        if (tempArray != null)
        {
            listOfDistrictTotals.Clear();
            for (int i = 0; i < tempArray.Length; i++)
            {
                listOfDistrictTotals.Add(tempArray[i]);
                counter++;
            }
            Debug.Assert(counter > 0, "No records in listOfDistrictTotals");
            Debug.LogFormat("City \"{0}\" added {1} records to listOfDistrictTotals", name, listOfDistrictTotals.Count);
        }
    }


    public List<int> GetListOfDistrictTotals()
    { return listOfDistrictTotals; }

    public void ClearOrganisations()
    { listOfOrganisations.Clear(); }

    /// <summary>
    /// Add an organisation
    /// </summary>
    /// <param name="organisation"></param>
    public void AddOrganisation(Organisation organisation)
    {
        if (organisation != null)
        { listOfOrganisations.Add(organisation); }
        else { Debug.LogWarning("Invalid organisation (Null)"); }
    }
   
    public int CheckNumOfOrganisations()
    { return listOfOrganisations.Count; }

    public List<Organisation> GetListOfOrganisations()
    { return listOfOrganisations; }

}
