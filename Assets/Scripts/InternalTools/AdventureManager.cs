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

    [Header("Names for Characters")]
    [Tooltip("Male first name textlist")]
    public TextList nameMaleFirst;
    [Tooltip("Female first name textlist")]
    public TextList nameFemaleFirst;
    [Tooltip("Last name textlist")]
    public TextList nameLast;

    public void Initialise()
    {
        /*DebugTestCharacter();*/
        Debug.Assert(nameMaleFirst != null, "Invalid nameMaleFirst (Null)");
        Debug.Assert(nameFemaleFirst != null, "Invalid nameFemaleFirst (Null)");
        Debug.Assert(nameLast != null, "Invalid nameLast (Null)");
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

    /// <summary>
    /// returns a randomly generated Theme priority (1 to 5) used for accessing the required theme for a plotpoint. Returns -1 if a problem
    /// </summary>
    /// <returns></returns>
    public int GetThemePriority()
    {
        int priority = -1;
        Random random = new Random();
        int rnd= random.Next(0, 10);
        switch (rnd)
        {
            case 0:
            case 1:
            case 2:
            case 3: priority = 1; break;
            case 4:
            case 5:
            case 6: priority = 2; break;
            case 7:
            case 8: priority = 3; break;
            case 9:
                //50/50 chance of priority 4 or 5
                rnd = random.Next(0, 10);
                if (rnd < 5) { priority = 4; }
                else { priority = 5; }
                break;
            default: Debug.LogWarningFormat("Unrecognised random \"{0}\"", random); break;
        }
        return priority;
    }




    /// <summary>
    /// returns a new character
    /// </summary>
    /// <returns></returns>
    public Character GetNewCharacter()
    {
        Character character = new Character();
        string identity, descriptor, name, refTag;
        List<string> tempList = new List<string>();
        identity = descriptor = name = "";
        //special
        CharacterSpecial special = ToolManager.i.toolDataScript.GetCharacterSpecial();
        if (special != null)
        {
            //identity
            tempList = ToolManager.i.toolDataScript.GetCharacterIdentity();
            for (int j = 0; j < tempList.Count; j++)
            { identity = string.Format("{0}{1}", identity.Length > 0 ? identity + ", " : "", tempList[j]); }
            //descriptor
            tempList = ToolManager.i.toolDataScript.GetCharacterDescriptors();
            for (int j = 0; j < tempList.Count; j++)
            { descriptor = string.Format("{0}{1}", descriptor.Length > 0 ? descriptor + ", " : "", tempList[j]); }
            //name
            Random random = new Random();
            int rnd = random.Next(0, 100);
            name = string.Format("{0} {1}", rnd < 50 ? nameMaleFirst.GetRandomRecord() : nameMaleFirst.GetRandomRecord(), nameLast.GetRandomRecord());
            refTag = name.Replace(" ", "");
            //bring together
            character.dataCreated = string.Format("{0} -> {1} -> {2} -> {3}", name, special.tag, identity, descriptor);
            character.tag = name;
            character.refTag = refTag;           
        }
        else { Debug.LogWarning("Invalid CharacterSpecial (Null)"); }
        return character; 
    }

    //
    // - - - Debug
    //

    /// <summary>
    /// generate ten instance of character descriptors for testing purposes
    /// </summary>
    private void DebugTestCharacter()
    {
        string identity, descriptor;
        List<string> tempList = new List<string>();
        Debug.LogFormat("[Tst] AdventureManager.cs -> DebugTestCharacter: - - - - - - -{0}", "\n");
        for (int i = 0; i < 10; i++)
        {
            identity = descriptor = "";
            //special
            CharacterSpecial special = ToolManager.i.toolDataScript.GetCharacterSpecial();
            if (special != null)
            {
                //identity
                tempList = ToolManager.i.toolDataScript.GetCharacterIdentity();
                for (int j = 0; j < tempList.Count; j++)
                { identity = string.Format("{0}{1}", identity.Length > 0 ? identity + ", " : "", tempList[j]); }
                //descriptor
                tempList = ToolManager.i.toolDataScript.GetCharacterDescriptors();
                for (int j = 0; j < tempList.Count; j++)
                { descriptor = string.Format("{0}{1}", descriptor.Length > 0 ? descriptor + ", " : "", tempList[j]); }
                //output
                Debug.LogFormat("[Tst] Test {0}: {1} -> {2} -> {3}", i, special.tag, identity, descriptor, "\n");
            }
            else { Debug.LogWarning("Invalid CharacterSpecial (Null)"); }
        }
    }

    //new methods above here
}

#endif
