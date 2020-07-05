using System.Collections.Generic;
using UnityEngine;
using toolsAPI;
using Random = System.Random;

#if (UNITY_EDITOR)
/// <summary>
/// Runs adventure generator internal tool
/// </summary>
public class AdventureManager : MonoBehaviour
{

    public void Initialise()
    {
        DebugTestCharacterDescriptors();
    }

    /// <summary>
    /// Get a list of randomly sorted themeTypes in priority order (index 0 onwards). Should be five ThemeTypes in list with index 0 being the first priority and index 3/4 being the last
    /// </summary>
    /// <returns></returns>
    public List<ThemeType> GetThemes()
    {
        List<ThemeType> listOfThemes = new List<ThemeType>();
        //populate list with all available theme types
        for (int i = 0; i < (int)ThemeType.Count; i++)
        { listOfThemes.Add((ThemeType)i); }
        //shuffle
        Random random = new Random();
        int n = listOfThemes.Count;
        ThemeType value;
        for (int i = listOfThemes.Count - 1; i > 1; i--)
        {
            int rnd = random.Next(i + 1);
            value = listOfThemes[rnd];
            listOfThemes[rnd] = listOfThemes[i];
            listOfThemes[i] = value;
        }
        return listOfThemes;
    }


    //
    // - - - Debug
    //

    /// <summary>
    /// generate ten instance of character descriptors for testing purposes
    /// </summary>
    private void DebugTestCharacterDescriptors()
    {
        string descriptors;
        List<string> tempList = new List<string>();
        Debug.LogFormat("[Tst] AdventureManager.cs -> DebugTestCharacterDescriptors: - - - - - - -{0}", "\n");
        for (int i = 0; i < 20; i++)
        {
            tempList = ToolManager.i.toolDataScript.GetCharacterDescriptors();
            descriptors = "";
            for (int j = 0; j < tempList.Count; j++)
            { descriptors = string.Format("{0}{1}", descriptors.Length > 0 ? descriptors + ", " : "", tempList[j]);  }
            Debug.LogFormat("[Tst] Test {0}: Descriptors -> {1}{2}", i, descriptors, "\n");
        }
    }


    //new methods above here
}

#endif
