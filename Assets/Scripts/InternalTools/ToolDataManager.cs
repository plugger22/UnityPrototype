using System;
using System.Collections.Generic;
using System.Linq;
using toolsAPI;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Data Manager class
/// </summary>
public class ToolDataManager : MonoBehaviour
{

    private Dictionary<string, Story> dictOfStories = new Dictionary<string, Story>();
    private Dictionary<string, Plotpoint> dictOfPlotpoints = new Dictionary<string, Plotpoint>();
    private Dictionary<string, MetaPlotpoint> dictOfMetaPlotpoints = new Dictionary<string, MetaPlotpoint>();

    //lookup tables
    private Plotpoint[,] arrayOfPlotpointLookup;
    private MetaPlotpoint[] arrayOfMetaPlotpointLookup;
    private CharacterIdentity[] arrayOfIdentityLookup;
    private CharacterDescriptor[] arrayOfDescriptorsLookup;
    private CharacterSpecial[] arrayOfSpecialLookup;

    public ToolDataManager()
    {
        int size = 100;
        //initialise collections
        arrayOfPlotpointLookup = new Plotpoint[size, (int)ThemeType.Count];
        arrayOfMetaPlotpointLookup = new MetaPlotpoint[size];
        arrayOfIdentityLookup = new CharacterIdentity[size];
        arrayOfDescriptorsLookup = new CharacterDescriptor[size];
        arrayOfSpecialLookup = new CharacterSpecial[size];
    }

    #region Stories
    //
    // - - - Stories
    //

    public Dictionary<string, Story> GetDictOfStories()
    { return dictOfStories; }

    public void ClearDictOfStories()
    { dictOfStories.Clear(); }

    /// <summary>
    /// Add story to dict. If already present will override with new data
    /// </summary>
    /// <param name="story"></param>
    public void AddStory(Story story)
    {
        if (story != null)
        {
            //create a new story to add to dict (stops issues with reference being deleted in UI)
            Story storyAdd = new Story(story);
            if (dictOfStories.ContainsKey(storyAdd.refTag) == false)
            {
                //not present, add new entry
                try { dictOfStories.Add(storyAdd.refTag, storyAdd); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid story (Null)"); }
                catch (ArgumentException)
                { Debug.LogErrorFormat("Duplicate entry exists for story \"{0}\"", storyAdd.refTag); }
            }
            else
            {
                //already exists -> delete existing entry
                dictOfStories.Remove(storyAdd.refTag);
                //create new entry
                try { dictOfStories.Add(storyAdd.refTag, storyAdd); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid story (Null) (after deleting original)"); }
                catch (ArgumentException)
                { Debug.LogErrorFormat("Duplicate entry exists for story \"{0}\" (after deleting original)", storyAdd.refTag); }
            }
        }
        else { Debug.LogError("Invalid story (Null)"); }
    }

    /// <summary>
    /// returns true if story present in dictOfStories, false otherwise or a problem
    /// </summary>
    /// <param name="story"></param>
    /// <returns></returns>
    public bool CheckStoryExists(Story story)
    {
        if (story != null)
        { return dictOfStories.ContainsKey(story.refTag); }
        else
        { Debug.LogError("Invalid story (Null)"); }
        return false;
    }

    /// <summary>
    /// Sets stories in dict for a load/save operation. Existing data deleted prior to new data input
    /// </summary>
    /// <param name="listOfStories"></param>
    public void SetStories(List<Story> listOfStories)
    {
        int counter = 0;
        if (listOfStories != null)
        {
            dictOfStories.Clear();
            for (int i = 0; i < listOfStories.Count; i++)
            {
                Story story = listOfStories[i];
                if (story != null)
                {
                    if (string.IsNullOrEmpty(story.refTag) == false)
                    { AddStory(story); counter++; }
                    else { Debug.LogWarningFormat("Invalid story.refTag (Null or Empty) for listOfStories[{0}]", i); }
                }
                else { Debug.LogWarningFormat("Invalid story (Null) for listOfStories[{0}]", i); }
            }
            Debug.LogFormat("[Tst] ToolDataManager.cs -> SetStories: listOfStories has {0} records, {1} have been loaded into Dict{2}", listOfStories.Count, counter, "\n");
        }
        else { Debug.LogError("Invalid listOfStories (Null)"); }

    }

    /*/// <summary>
    /// returns first record in dictOfStories (not in sorted order so could be anything). Returns null if a problem or none found
    /// </summary>
    /// <returns></returns>
    public Story GetFirstStoryInDict()
    {
        Story story = null;
        if (dictOfStories.Count > 0)
        {
            story = dictOfStories.Values.First();
        }
        else { Debug.LogWarning("No records in dictOfStories -> ALERT"); }
        return story;
    }*/

    /// <summary>
    /// returns a list of stories derived from the dictOfStories
    /// </summary>
    /// <returns></returns>
    public List<Story> GetListOfStories()
    { return dictOfStories.Values.ToList(); }
    #endregion

    #region Plotpoints
    //
    // - - - Plotpoints
    //

    public Dictionary<string, Plotpoint> GetDictOfPlotpoints()
    { return dictOfPlotpoints; }

    public Dictionary<string, MetaPlotpoint> GetDictOfMetaPlotpoints()
    { return dictOfMetaPlotpoints; }

    public Plotpoint[,] GetPlotpointLookup()
    { return arrayOfPlotpointLookup; }

    public MetaPlotpoint[] GetMetaPlotpointLookup()
    { return arrayOfMetaPlotpointLookup; }
    

    /// <summary>
    /// Add Plotpoint
    /// </summary>
    /// <param name="plot"></param>
    public void AddPlotpoint(Plotpoint plot)
    {
        try { dictOfPlotpoints.Add(plot.refTag, plot); }
        catch (ArgumentNullException)
        { Debug.LogError("Invalid plotpoint (Null)"); }
        catch (ArgumentException)
        { Debug.LogWarningFormat("Duplicate Plotpoint exists in dict for \"{0}\"", plot.refTag); }
    }

    /// <summary>
    /// Add MetaPlotpoint
    /// </summary>
    /// <param name="meta"></param>
    public void AddMetaPlotpoint(MetaPlotpoint meta)
    {
        try { dictOfMetaPlotpoints.Add(meta.refTag, meta); }
        catch (ArgumentNullException)
        { Debug.LogError("Invalid MetaPlotpoint (Null)"); }
        catch (ArgumentException)
        { Debug.LogWarningFormat("Duplicate MetaPlotpoint exists in dict for \"{0}\"", meta.refTag); }
    }

    /// <summary>
    /// returns a random plotpoint, null if a problem
    /// </summary>
    /// <returns></returns>
    public Plotpoint GetPlotpoint(ThemeType theme)
    {
        Plotpoint plotPoint = null;
        int rnd = Random.Range(0, 100);
        plotPoint = arrayOfPlotpointLookup[rnd, (int)theme];
        return plotPoint;
    }

    #endregion

    #region Characters
    //
    // - - - Characters
    //

    public CharacterSpecial[] GetArrayOfCharacterSpecial()
    { return arrayOfSpecialLookup; }

    public CharacterIdentity[] GetArrayOfCharacterIdentity()
    { return arrayOfIdentityLookup; }

    public CharacterDescriptor[] GetArrayOfCharacterDescriptors()
    { return arrayOfDescriptorsLookup; }

    /// <summary>
    /// Get random CharacterSpecial. Returns null if a problem
    /// </summary>
    /// <returns></returns>
    public CharacterSpecial GetCharacterSpecial()
    {
        int rnd = UnityEngine.Random.Range(0, 100);
        return arrayOfSpecialLookup[rnd];
    }


    /// <summary>
    /// Get a list Of character Identity (typically only one but could be two). Returns Empty list if a problem
    /// </summary>
    /// <returns></returns>
    public List<string> GetCharacterIdentity()
    {
        List<string> listOfIdentity = new List<string>();
        int rnd = UnityEngine.Random.Range(0, 100);
        CharacterIdentity identity = arrayOfIdentityLookup[rnd];
        //check roll again
        if (identity.isRollAgain == true)
        {
            int counter = 0;
            do
            {
                rnd = UnityEngine.Random.Range(0, 100);
                identity = arrayOfIdentityLookup[rnd];
                if (identity.isRollAgain == false)
                {
                    listOfIdentity.Add(identity.tag);
                    counter++;
                }
            }
            while (counter < 2);
        }
        else { listOfIdentity.Add(identity.tag); }
        return listOfIdentity;
    }

    /// <summary>
    /// Get a list Of character Descriptors (typically only one but could be two). Returns Empty list if a problem
    /// </summary>
    /// <returns></returns>
    public List<string> GetCharacterDescriptors()
    {
        List<string> listOfDescriptors = new List<string>();
        int rnd = UnityEngine.Random.Range(0, 100);
        CharacterDescriptor descriptor = arrayOfDescriptorsLookup[rnd];
        //check roll again
        if (descriptor.isRollAgain == true)
        {
            int counter = 0;
            do
            {
                rnd = UnityEngine.Random.Range(0, 100);
                descriptor = arrayOfDescriptorsLookup[rnd];
                if (descriptor.isRollAgain == false)
                {
                    listOfDescriptors.Add(descriptor.tag);
                    counter++;
                }
            }
            while (counter < 2);
        }
        else { listOfDescriptors.Add(descriptor.tag); }
        return listOfDescriptors;
    }

    #endregion

    //new methods above here
}
