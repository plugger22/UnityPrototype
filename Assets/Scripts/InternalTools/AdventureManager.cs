using System.Collections.Generic;
using System.Text;
using toolsAPI;
using UnityEngine;
using Random = System.Random;

#if (UNITY_EDITOR)
/// <summary>
/// Runs adventure generator internal tool
/// </summary>
public class AdventureManager : MonoBehaviour
{
    [Header("Names")]
    [Tooltip("A New Adventure can select from any of the Namesets provided here")]
    public NameSet[] arrayOfNameSets;

    private NameSet nameSet;                    //nameSet currently in use (default set in Initialise to be arrayOfNameSets[index 0]

    /// <summary>
    /// Initialise
    /// </summary>
    public void Initialise()
    {
        if (arrayOfNameSets.Length > 0)
        {
            for (int i = 0; i < arrayOfNameSets.Length; i++)
            {
                if (arrayOfNameSets[i] == null)
                { Debug.LogErrorFormat("Invalid nameSet (Null) for arrayOfNameSets[{0}]", i); }
            }
            //set default nameSet to be the first in the array (probably American)
            nameSet = arrayOfNameSets[0];
        }
        else { Debug.LogError("Invalid arrayOfNameSets (Empty)"); }
    }

    #region Themes
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
        int rnd = random.Next(0, 10);
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

    #endregion

    #region Characters
    /// <summary>
    /// returns a new character (no saving, just data collection)
    /// </summary>
    /// <returns></returns>
    public Character GetNewCharacter()
    {
        Character character = null;
        Random random = new Random();
        //special
        CharacterSpecial specialCharacter = ToolManager.i.toolDataScript.GetCharacterSpecial();
        if (specialCharacter != null)
        {
            switch (specialCharacter.special)
            {
                case SpecialType.None:
                    character = GetNewPerson();
                    break;
                case SpecialType.Organisation:
                    character = GetNewOrganisation();
                    break;
                case SpecialType.Object:
                    character = GetNewObject();
                    break;
                case SpecialType.OrgOrChar:
                    // 50/50 chance of either
                    if (random.Next(0, 100) < 50)
                    { character = GetNewPerson(); }
                    else { character = GetNewOrganisation(); }
                    break;
                default: Debug.LogWarningFormat("Unrecognised specialType \"{0}\"", specialCharacter.special); break;
            }

        }
        else { Debug.LogWarning("Invalid CharacterSpecial (Null)"); }
        return character;
    }
    #endregion

    /// <summary>
    /// Returns a new Character (actual person versus an Object or Organisation)
    /// </summary>
    /// <returns></returns>
    public Character GetNewPerson()
    {
        Character character = new Character() { special = SpecialType.None };
        string identity, descriptor, name, refTag, firstName;
        List<string> tempList = new List<string>();
        identity = descriptor = name = "";
        //Character -> identity
        tempList = ToolManager.i.toolDataScript.GetCharacterIdentity();
        for (int j = 0; j < tempList.Count; j++)
        { identity = string.Format("{0}{1}", identity.Length > 0 ? identity + ", " : "", tempList[j]); }
        //Character -> descriptor
        tempList = ToolManager.i.toolDataScript.GetCharacterDescriptors();
        for (int j = 0; j < tempList.Count; j++)
        { descriptor = string.Format("{0}{1}", descriptor.Length > 0 ? descriptor + ", " : "", tempList[j]); }
        //Character -> name (50/50 male/female)
        Random random = new Random();
        int rnd = random.Next(0, 100);
        if (rnd < 50)
        {
            character.sex = CharacterSex.Male;
            firstName = nameSet.firstMaleNames.GetRandomRecord();
        }
        else
        {
            character.sex = CharacterSex.Female;
            firstName = nameSet.firstFemaleNames.GetRandomRecord();
        }
        name = string.Format("{0} {1}", firstName, nameSet.lastNames.GetRandomRecord());
        refTag = name.Replace(" ", "");
        //bring together
        character.dataCreated = string.Format("{0} -> {1} -> {2}", character.sex, identity, descriptor);
        character.tag = name;
        character.refTag = refTag;
        Debug.LogFormat("[Tst] AdventureManager.cs -> GetNewCharacter: \"{0}\", refTag {1} CREATED{2}", character.tag, character.refTag, "\n");
        return character;
    }

    #region Organisations
    //
    // - - - Organisations
    //

    /// <summary>
    /// returns a character (Organisations are represented as characters with specialType.enum set to 'Organisation').
    /// </summary>
    /// <returns></returns>
    public Character GetNewOrganisation()
    {
        Character character = new Character() { special = SpecialType.Organisation };
        StringBuilder builder = new StringBuilder();
        builder.Append(ToolManager.i.toolDataScript.GetRandomOrganisationType());
        builder.AppendFormat(" -> O -> {0}", ToolManager.i.toolDataScript.GetRandomOrganisationOrigin());
        builder.AppendFormat(" -> L -> {0}", ToolManager.i.toolDataScript.GetRandomOrganisationLeadership());
        builder.AppendFormat(" -> M -> {0}", ToolManager.i.toolDataScript.GetRandomOrganisationMotivation());
        builder.AppendFormat(" -> M -> {0}", ToolManager.i.toolDataScript.GetRandomOrganisationMethod());
        builder.AppendFormat(" -> S -> {0}", ToolManager.i.toolDataScript.GetRandomOrganisationStrength());
        builder.AppendFormat(" -> W -> {0}", ToolManager.i.toolDataScript.GetRandomOrganisationWeakness());
        builder.AppendFormat(" -> O -> {0}", ToolManager.i.toolDataScript.GetRandomOrganisationObstacle());

        string name = string.Format("ORG {0}", nameSet.lastNames.GetRandomRecord());

        //bring together
        character.dataCreated = builder.ToString();
        character.tag = name;
        character.refTag = name.Replace(" ", "");
        Debug.LogFormat("[Tst] AdventureManager.cs -> GetNewOrganisation: \"{0}\", refTag {1} CREATED{2}", character.tag, character.refTag, "\n");
        return character;
    }

    #endregion

    #region Objects
    //
    // - - - Objects
    //

    /// <summary>
    /// returns a character (Objects are represented as Characters with specialType.enum set to 'Object')
    /// </summary>
    /// <returns></returns>
    public Character GetNewObject()
    {
        Character character = new Character() { special = SpecialType.Object };
        string name = string.Format("OBJECT {0}", nameSet.lastNames.GetRandomRecord());
        //bring together
        character.tag = name;
        character.refTag = name.Replace(" ", "");
        Debug.LogFormat("[Tst] AdventureManager.cs -> GetNewObject: \"{0}\", refTag {1} CREATED{2}", character.tag, character.refTag, "\n");
        return character;
    }


    #endregion

    #region NameSets
    //
    // - - - NameSets
    //

    /// <summary>
    /// Get arrayOfNameSets
    /// </summary>
    /// <returns></returns>
    public NameSet[] GetArrayOfNameSets()
    { return arrayOfNameSets; }

    /// <summary>
    /// Called from AdventureUI.cs and determines which nameSet will be used
    /// </summary>
    /// <param name="index"></param>
    public void SetNameSet(int index)
    {
        if (index > -1 && index < arrayOfNameSets.Length)
        {
            nameSet = arrayOfNameSets[index];
            Debug.LogFormat("[Tst] AdventureManager.cs -> SetNameSet: \"{0}\" NAMESET now in use{1}", nameSet.name, "\n");
        }
        else { Debug.LogErrorFormat("Invalid index \"{0}\" (should be between 0 and {1})", arrayOfNameSets.Length); }
    }

    /// <summary>
    /// Get the nameSet currently in use
    /// </summary>
    /// <returns></returns>
    public string GetNameSetInUse()
    { return nameSet != null ? string.Format("{0} names", nameSet.name) : "No NameSet Selected"; }
    #endregion

    #region File Ops
    //
    // - - - Supplementary File Ops
    //

    /// <summary>
    /// Creates a ginormous dataDump string in a suitable cut and paste, Keep friendly, format, from all Stories in dictOfStories
    /// </summary>
    /// <returns></returns>
    public string CreateExportDataDump()
    {
        StringBuilder builderMain = new StringBuilder();
        Dictionary<string, Story> dictOfStories = ToolManager.i.toolDataScript.GetDictOfStories();
        if (dictOfStories != null)
        {
            int count;
            string characters;
            foreach (var story in dictOfStories)
            {
                //create an individual story builder
                StringBuilder builderStory = new StringBuilder();
                //Story Details
                builderStory.AppendFormat("- - - [NewAdventure]{0}", "\n");
                builderStory.AppendFormat("Name: {0}{1}", story.Value.tag, "\n");
                builderStory.AppendFormat("Date: {0}{1}", story.Value.date, "\n");
                builderStory.AppendFormat("NameSet: {0}{1}", story.Value.nameSet, "\n");
                builderStory.AppendFormat("Theme: {0} / {1} / {2} / {3} / {4}{5}{6}", story.Value.theme.GetThemeType(1), story.Value.theme.GetThemeType(2), story.Value.theme.GetThemeType(3),
                    story.Value.theme.GetThemeType(4), story.Value.theme.GetThemeType(5), "\n", "\n");
                builderStory.AppendFormat("{0}{1}", story.Value.notes, "\n");
                //Turning Point summary
                for (int i = 0; i < story.Value.arrayOfTurningPoints.Length; i++)
                {
                    TurningPoint turningPoint = story.Value.arrayOfTurningPoints[i];
                    builderStory.AppendFormat("{0}TurningPoint {1}: {2}{3}", "\n", i, turningPoint.tag, "\n");
                    builderStory.AppendFormat("Notes: {0}{1}", turningPoint.notes, "\n");
                    //summary
                    builderStory.AppendFormat("{0}TurningPoint {1} Summary{2}", "\n", i, "\n");
                    for (int j = 0; j < turningPoint.arrayOfDetails.Length; j++)
                    {
                        PlotDetails details = turningPoint.arrayOfDetails[j];
                        characters = "";
                        characters = string.Format("{0}{1}", details.character1.tag.Length > 0 ? " -> " + details.character1.tag : "", details.character2.tag.Length > 0 ? " / " + details.character2.tag : "");
                        builderStory.AppendFormat("{0} {1} {2}{3}", j, details.plotPoint, characters, "\n");
                    }
                }
                //lists -> Active Plotline
                builderStory.AppendFormat("{0}- Active PlotLines{1}", "\n", "\n");
                count = story.Value.lists.listOfPlotLines.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    { builderStory.AppendFormat("{0}{1}", story.Value.lists.listOfPlotLines[i].tag, "\n"); }
                }
                else { builderStory.AppendFormat("No active PlotLines remaining{0}{1}", "\n", "\n"); }
                //lists -> Removed Plotline
                builderStory.AppendFormat("{0}- Removed PlotLines{1}", "\n", "\n");
                count = story.Value.lists.listOfRemovedPlotLines.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    { builderStory.AppendFormat("{0}{1}", story.Value.lists.listOfRemovedPlotLines[i].tag, "\n"); }
                }
                else { builderStory.AppendFormat("No plotLines have been Removed{0}{1}", "\n", "\n"); }
                //lists -> Characters
                builderStory.AppendFormat("{0}- Active Characters{1}", "\n", "\n");
                count = story.Value.lists.listOfCharacters.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    { builderStory.AppendFormat("{0} -> {1}{2}", story.Value.lists.listOfCharacters[i].tag, story.Value.lists.listOfCharacters[i].dataCreated, "\n"); }
                }
                else { builderStory.AppendFormat("No active Characters remaining{0}{1}", "\n", "\n"); }
                //lists -> RemovedCharacters
                builderStory.AppendFormat("{0}- Removed Characters{1}", "\n", "\n");
                count = story.Value.lists.listOfRemovedCharacters.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    { builderStory.AppendFormat("{0} -> {1}{2}", story.Value.lists.listOfRemovedCharacters[i].tag, story.Value.lists.listOfCharacters[i].dataCreated, "\n"); }
                }
                else { builderStory.AppendFormat("No characters have been removed{0}{1}", "\n", "\n"); }
                //Characters in detail -> Active Characters
                builderStory.AppendFormat("{0}- Active Characters in Detail{1}", "\n", "\n");
                count = story.Value.lists.listOfCharacters.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Character character = story.Value.lists.listOfCharacters[i];
                        builderStory.AppendFormat("{0}{1}", character.tag, "\n");
                        builderStory.AppendFormat("  {0}{1}", character.dataCreated, "\n");
                        //notes
                        for (int j = 0; j < character.listOfNotes.Count; j++)
                        { builderStory.AppendFormat("Note: {0}{1}", character.listOfNotes[j], "\n"); }
                    }
                }
                else { builderStory.AppendFormat("No active Characters remaining{0}{1}", "\n", "\n"); }
                //Characters in detail -> Removed Characters
                builderStory.AppendFormat("{0}- Removed Characters in Detail{1}", "\n", "\n");
                count = story.Value.lists.listOfRemovedCharacters.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Character character = story.Value.lists.listOfRemovedCharacters[i];
                        builderStory.AppendFormat("{0}{1}", character.tag, "\n");
                        builderStory.AppendFormat("  {0}{1}", character.dataCreated, "\n");
                        //notes
                        for (int j = 0; j < character.listOfNotes.Count; j++)
                        { builderStory.AppendFormat("Note: {0}{1}", character.listOfNotes[j], "\n"); }
                    }
                }
                else { builderStory.AppendFormat("No characters have been removed{0}{1}", "\n", "\n"); }
                //PlotLines in detail -> Active
                builderStory.AppendFormat("{0}- Active PlotLines in Detail{1}", "\n", "\n");
                count = story.Value.lists.listOfPlotLines.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        PlotLine plotLine = story.Value.lists.listOfPlotLines[i];
                        builderStory.AppendFormat("{0}{1}", plotLine.tag, "\n");
                        //notes
                        for (int j = 0; j < plotLine.listOfNotes.Count; j++)
                        { builderStory.AppendFormat("Note: {0}{1}", plotLine.listOfNotes[j], "\n"); }
                        builderStory.AppendLine();
                    }
                }
                else { builderStory.AppendFormat("No active plotLines remaining{0}{1}", "\n", "\n"); }
                //PlotLines in detail -> Removed
                builderStory.AppendFormat("- Removed PlotLines in Detail{0}", "\n");
                count = story.Value.lists.listOfRemovedPlotLines.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        PlotLine plotLine = story.Value.lists.listOfRemovedPlotLines[i];
                        builderStory.AppendFormat("{0}{1}", plotLine.tag, "\n");
                        //notes
                        for (int j = 0; j < plotLine.listOfNotes.Count; j++)
                        { builderStory.AppendFormat("Note: {0}{1}", plotLine.listOfNotes[j], "\n"); }
                        builderStory.AppendLine();
                    }
                }
                else { builderStory.AppendFormat("No plotLines have been removed{0}{1}", "\n", "\n"); }
                //add story to main builder
                builderMain.Append(builderStory);
                builderMain.AppendLine();
                builderMain.AppendLine();
            }
        }
        else { Debug.LogError("Invalid dictOfStories (Null)"); }
        return builderMain.ToString();
    }
    #endregion

    #region Debug Methods
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
    #endregion

    //new methods above here
}

#endif
