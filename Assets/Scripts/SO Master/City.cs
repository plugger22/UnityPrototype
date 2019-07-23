using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Cities. Name of SO is the name of the city, eg. "Gotham City"
/// </summary>
[CreateAssetMenu(menuName = "Game / City / City")]
public class City : ScriptableObject
{
    [Tooltip("In game name")]
    public string tag;
    [Tooltip("Short text summary that appears in city tooltip")]
    public string descriptor;

    /*[Header("Stats")]
    [Tooltip("Starting loyalty to the Authorities of the City (10 is total loyalty)")]
    [Range(0, 10)] public int baseLoyalty = 10;*/

    [Header("Archetypes")]
    [Tooltip("City Arc determines the size, layout and node type frequencies of the city")]
    public CityArc Arc;
    public Country country;

    [Header("Districts")]
    [Tooltip("Name of the district where the icon is located. NOTE: All cities have an icon. Same name can't be in District TextList")]
    public string iconDistrict;
    [Tooltip("Name of the district where the city airport is located. All cities have airports. NOTE: Same name can't be in District TextList")]
    public string airportDistrict;
    [Tooltip("Name of the district where the working port (if any) is located. Optional (not all cities have harbours). NOTE: Same name can't be in District TextList")]
    public string harbourDistrict;
    [Tooltip("List of district names to be randomly assigned to city nodes, min 28 required")]
    public TextList districtNames;

    [Header("Features")]
    [Tooltip("Name of a distinctive cultural icon for the city, eg 'The Eiffel Tower' or 'The Statue of Liberty'. All cities have an icon.")]
    public string iconName;

    [Header("Topic Pool")]
    [Tooltip("Topic pool of decisions for City")]
    public TopicPool cityPool;

    [Header("Debugging")]
    [Tooltip("Used for testing purposes only. If 'ON' the Mayor is ignored (DataManager.cs -> GetRandomMayor). Leave as OFF")]
    public bool isTestOff = false;


    //dynamically assigned data
    [HideInInspector] public Mayor mayor;                                           //alignment of mayor determines which faction is in charge of the city
    [HideInInspector] public Faction faction;                                       //ruling faction of current city (derived from mayor)

    private List<Organisation> listOfOrganisations = new List<Organisation>();      //organisations present in the city
    private List<int> listOfDistrictTotals = new List<int>();                       //cityManager.cs assigns this data (needs to be in the same order as DataManager.cs -> dictOfNodeArc's)


    public void OnEnable()
    {
        Debug.AssertFormat(Arc != null, "Invalid CityArc (Null) for {0}", name);
        Debug.AssertFormat(country != null, "Invalid Country (Null) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(iconDistrict) == false, "Invalid iconDistrict (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(airportDistrict) == false, "Invalid airportDistrict (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(iconName) == false, "Invalid iconName (Null or Empty) for {0}", name);
        Debug.AssertFormat(mayor != null, "Invalid mayor (Null) for {0}", name);
        Debug.AssertFormat(districtNames != null, "Invalid TextList of DistrictNames (Null) for {0}", name);
        Debug.AssertFormat(districtNames.category.name.Equals("Districts", System.StringComparison.Ordinal) == true, "Invalid districtNames TextList (wrong Category) for {0}", name);
        
    }

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
            /*Debug.LogFormat("[Cit] CityManager.cs -> SetDistrictsTotals: City \"{0}\" added {1} records to listOfDistrictTotals", name, listOfDistrictTotals.Count);*/
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
        {
            listOfOrganisations.Add(organisation);
            Debug.LogFormat("[Org] CityManager.cs -> AddOrganisation: {0} present in {1}{2}", organisation.tag, this.name, "\n");
        }
        else { Debug.LogWarning("Invalid organisation (Null)"); }
    }
   
    public int CheckNumOfOrganisations()
    { return listOfOrganisations.Count; }

    public List<Organisation> GetListOfOrganisations()
    { return listOfOrganisations; }

}
