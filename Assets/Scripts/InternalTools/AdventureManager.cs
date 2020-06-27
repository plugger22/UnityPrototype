using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR)
/// <summary>
/// Runs adventure generator internal tool
/// </summary>
public class AdventureManager : MonoBehaviour
{
    [Tooltip("Contains one of each themeType. Used to generate a theme list")]
    public List<ThemeType> listOfAllThemeTypes;


    public void Initialise()
    {
        Debug.Assert(listOfAllThemeTypes != null, "Invalid listOfAllThemeTypes (Null)");
        Debug.AssertFormat(listOfAllThemeTypes.Count == 5, "Invalid count for listOfAllThemeTypes (is {0}, should be {1}", listOfAllThemeTypes.Count, 5);
    }

    /// <summary>
    /// Get a list of themeTypes in priority order (index 0 onwards). Should be five ThemeTypes in list with index 0 being the first priority and index 3/4 being the last
    /// </summary>
    /// <returns></returns>
    public List<ThemeType> GetThemes()
    {
        List<ThemeType> listOfThemes = new List<ThemeType>();
        List<ThemeType> listOfStandard = new List<ThemeType>(listOfAllThemeTypes);
        if (listOfStandard != null)
        {
            int count = listOfStandard.Count;
            int index;
            while (listOfStandard.Count > 0)
            {
                //randomly select themes from list until none left
                index = Random.Range(0, count);
                ThemeType theme = listOfStandard[index];
                if (theme != null)
                {
                    listOfThemes.Add(theme);
                    listOfStandard.RemoveAt(index);
                }
                else { Debug.LogErrorFormat("Invalid themeType (Null) for listOfStandard[{0}]", index); }
            };
        }
        else { Debug.LogError("Invalid listStandard (Null)"); }
        return listOfThemes;
    }

}

#endif
