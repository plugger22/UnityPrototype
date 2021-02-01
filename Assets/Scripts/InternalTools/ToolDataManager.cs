using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using toolsAPI;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Data Manager class
/// </summary>
public class ToolDataManager : MonoBehaviour
{
    //Adventure dictionaries
    private Dictionary<string, Story> dictOfStories = new Dictionary<string, Story>();                                                  //key -> refTag, value -> Story
    private Dictionary<string, Plotpoint> dictOfPlotpoints = new Dictionary<string, Plotpoint>();                                       //key -> refTag, value -> Plotpoint
    private Dictionary<string, MetaPlotpoint> dictOfMetaPlotpoints = new Dictionary<string, MetaPlotpoint>();                           //key -> refTag, value -> MetaPlotpoint   
    private Dictionary<string, ConstantPlotpoint> dictOfConstantPlotpoints = new Dictionary<string, ConstantPlotpoint>();               //key -> refTag, value -> ConstantPlotpoint

    //Actor Pool dictionaries
    private Dictionary<string, Trait> dictOfTraits = new Dictionary<string, Trait>();
    private Dictionary<string, ActorArc> dictOfActorArcs = new Dictionary<string, ActorArc>();

    //lookup tables
    private Plotpoint[,] arrayOfPlotpointLookup;
    private MetaPlotpoint[] arrayOfMetaPlotpointLookup;
    private CharacterIdentity[] arrayOfIdentityLookup;
    private CharacterDescriptor[] arrayOfDescriptorsLookup;
    private CharacterGoal[] arrayOfGoalsLookup;
    private CharacterMotivation[] arrayOfMotivationLookup;
    private CharacterFocus[] arrayOfFocusLookup;
    private CharacterSpecial[] arrayOfSpecialLookup;


    //Organisations
    private List<OrganisationDescriptor> listOfOrganisationType = new List<OrganisationDescriptor>();
    private List<OrganisationDescriptor> listOfOrganisationOrigin = new List<OrganisationDescriptor>();
    private List<OrganisationDescriptor> listOfOrganisationHistory = new List<OrganisationDescriptor>();
    private List<OrganisationDescriptor> listOfOrganisationLeadership = new List<OrganisationDescriptor>();
    private List<OrganisationDescriptor> listOfOrganisationMotivation = new List<OrganisationDescriptor>();
    private List<OrganisationDescriptor> listOfOrganisationMethod = new List<OrganisationDescriptor>();
    private List<OrganisationDescriptor> listOfOrganisationStrength = new List<OrganisationDescriptor>();
    private List<OrganisationDescriptor> listOfOrganisationWeakness = new List<OrganisationDescriptor>();
    private List<OrganisationDescriptor> listOfOrganisationObstacle = new List<OrganisationDescriptor>();

    public ToolDataManager()
    {
        int size = 100;
        //initialise collections
        arrayOfPlotpointLookup = new Plotpoint[size, (int)ThemeType.Count];
        arrayOfMetaPlotpointLookup = new MetaPlotpoint[size];
        arrayOfIdentityLookup = new CharacterIdentity[size];
        arrayOfDescriptorsLookup = new CharacterDescriptor[size];
        arrayOfGoalsLookup = new CharacterGoal[size];
        arrayOfMotivationLookup = new CharacterMotivation[size];
        arrayOfFocusLookup = new CharacterFocus[size];
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

    public Dictionary<string, ConstantPlotpoint> GetDictOfConstantPlotpoints()
    { return dictOfConstantPlotpoints; }

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
    /// Add ConstantPlotpoint, overrides existing data in case of duplicates
    /// </summary>
    /// <param name="constant"></param>
    public void AddConstantPlotpoint(ConstantPlotpoint constant)
    {
        try
        { dictOfConstantPlotpoints.Add(constant.refTag, constant); }
        catch (ArgumentNullException)
        { Debug.LogError("Invalid ConstantPlotpoint (Null)"); }
        catch (ArgumentException)
        {
            //exists, override data
            dictOfConstantPlotpoints[constant.refTag] = constant;
            Debug.LogFormat("[Tst] ToolDataManager.cs -> AddConstantPlotpoint: Existing data overriden in dict for \"{0}\"", constant.refTag);
        }
    }

    /// <summary>
    /// deletes specified record from dictOfConstantPlotpoints and returns true if successful, false if not
    /// </summary>
    /// <param name="constant"></param>
    /// <returns></returns>
    public bool RemoveConstantPlotpoint(ConstantPlotpoint constant)
    {
        if (dictOfConstantPlotpoints.ContainsKey(constant.refTag) == true)
        { return dictOfConstantPlotpoints.Remove(constant.refTag); }
        return false;
    }


    /// <summary>
    /// Removes all constants in dictionary with 'Campaign' scope. Returns number of records removed, 0 if none
    /// </summary>
    /// <returns></returns>
    public int RemoveCampaignConstants()
    {
        int numRemoved = 0;
        int count = dictOfConstantPlotpoints.Count;
        //place all non-campaign scope constants in a list
        List<ConstantPlotpoint> tempList = dictOfConstantPlotpoints.Values.Where(x => x.scope == ConstantScope.Game).ToList();
        //clear out dictionary
        dictOfConstantPlotpoints.Clear();
        //repopulate from list
        for (int i = 0; i < tempList.Count; i++)
        { AddConstantPlotpoint(tempList[i]); }
        //calculate how many removed
        numRemoved = count - tempList.Count;
        return numRemoved;
    }

    /// <summary>
    /// returns a random constant plotpoint from a frequency based random selection pool. Could be either game or campaign scope. Returns null if none present or a problem
    /// </summary>
    public ConstantPlotpoint GetRandomConstantPlotpoint()
    {
        ConstantPlotpoint constantPlotpoint = null;
        var selection = dictOfConstantPlotpoints.Values.Where(x => x.type == ConstantSummaryType.Plotpoint).ToList();
        if (selection.Count > 0)
        {
            List<ConstantPlotpoint> listOfPool = new List<ConstantPlotpoint>();
            for (int i = 0; i < selection.Count; i++)
            {
                ConstantPlotpoint constantSelection = selection[i];
                if (constantSelection != null)
                {
                    //add entries to the pool based on frequency
                    switch (selection[i].frequency)
                    {
                        case ConstantDistribution.High:
                            listOfPool.Add(constantSelection);
                            listOfPool.Add(constantSelection);
                            listOfPool.Add(constantSelection);
                            break;
                        case ConstantDistribution.Medium:
                            listOfPool.Add(constantSelection);
                            listOfPool.Add(constantSelection);
                            break;
                        case ConstantDistribution.Low:
                            listOfPool.Add(constantSelection);
                            break;

                    }
                }
                else { Debug.LogWarningFormat("Invalid constantSelection (Null) for selection[{0}]", i); }
            }
            //randomly select from the pool
            constantPlotpoint = listOfPool[Random.Range(0, listOfPool.Count)];
        }
        return constantPlotpoint;
    }

    /// <summary>
    /// returns a random constant character/object/organisation from a frequency based random selection pool. Could be either game or campaign scope. Returns null if none present or a problem
    /// </summary>
    public ConstantPlotpoint GetRandomConstantCharacter()
    {
        ConstantPlotpoint constantCharacter = null;
        var selection = dictOfConstantPlotpoints.Values.Where(x => x.type != ConstantSummaryType.Plotpoint).ToList();
        if (selection.Count > 0)
        {
            List<ConstantPlotpoint> listOfPool = new List<ConstantPlotpoint>();
            for (int i = 0; i < selection.Count; i++)
            {
                ConstantPlotpoint constantSelection = selection[i];
                if (constantSelection != null)
                {
                    //add entries to the pool based on frequency
                    switch (selection[i].frequency)
                    {
                        case ConstantDistribution.High:
                            listOfPool.Add(constantSelection);
                            listOfPool.Add(constantSelection);
                            listOfPool.Add(constantSelection);
                            break;
                        case ConstantDistribution.Medium:
                            listOfPool.Add(constantSelection);
                            listOfPool.Add(constantSelection);
                            break;
                        case ConstantDistribution.Low:
                            listOfPool.Add(constantSelection);
                            break;

                    }
                }
                else { Debug.LogWarningFormat("Invalid constantSelection (Null) for selection[{0}]", i); }
            }
            //randomly select from the pool
            constantCharacter = listOfPool[Random.Range(0, listOfPool.Count)];
        }
        return constantCharacter;
    }

    /// <summary>
    /// returns a random Metaplotpoint, null if a problem
    /// </summary>
    /// <returns></returns>
    public MetaPlotpoint GetRandomMetaPlotpoint()
    { return arrayOfMetaPlotpointLookup[Random.Range(0, 100)]; }

    /// <summary>
    /// returns a random plotpoint, null if a problem
    /// </summary>
    /// <returns></returns>
    public Plotpoint GetRandomPlotpoint(ThemeType theme)
    {
        Plotpoint plotPoint = null;
        int rnd = Random.Range(0, 100);
        plotPoint = arrayOfPlotpointLookup[rnd, (int)theme];
        return plotPoint;
    }

    /// <summary>
    /// Get a plotpoint from dictionary. Returns null if not found
    /// </summary>
    /// <param name="refTag"></param>
    /// <returns></returns>
    public Plotpoint GetPlotpoint(string refTag)
    {
        if (dictOfPlotpoints.ContainsKey(refTag) == true)
        { return dictOfPlotpoints[refTag]; }
        return null;
    }


    /// <summary>
    /// Debug method to get a plotpoint from dictionary using tag instead of refTag (way more inefficient). Returns null if not found
    /// </summary>
    /// <param name="refTag"></param>
    /// <returns></returns>
    public Plotpoint GetPlotpointFromTag(string tag)
    {
        var values = dictOfPlotpoints.Where(item => item.Value.tag.Equals(tag, StringComparison.Ordinal) == true).Select(item => item.Value);
        List<Plotpoint> listOfPlotpoints = values.ToList();
        if (listOfPlotpoints.Count > 0)
        { return listOfPlotpoints[0]; }
        return null;

    }

    /// <summary>
    /// Sets constantPlotpoints in dict for a load/save operation. Existing data deleted prior to new data input
    /// </summary>
    /// <param name="listOfStories"></param>
    public void SetConstantPlotpoints(List<ConstantPlotpoint> listOfConstants)
    {
        int counter = 0;
        if (listOfConstants != null)
        {
            dictOfConstantPlotpoints.Clear();
            for (int i = 0; i < listOfConstants.Count; i++)
            {
                ConstantPlotpoint constantPlotpoint = listOfConstants[i];
                if (constantPlotpoint != null)
                {
                    if (string.IsNullOrEmpty(constantPlotpoint.refTag) == false)
                    { AddConstantPlotpoint(constantPlotpoint); counter++; }
                    else { Debug.LogWarningFormat("Invalid constantPlotpoint.refTag (Null or Empty) for listOfConstants[{0}]", i); }
                }
                else { Debug.LogWarningFormat("Invalid constantPlotpoint (Null) for listOfConstants[{0}]", i); }
            }
            Debug.LogFormat("[Tst] ToolDataManager.cs -> SetConstantPlotpoints: listOfConstantPlotpoints has {0} records, {1} have been loaded into Dict{2}", listOfConstants.Count, counter, "\n");
        }
        else { Debug.LogError("Invalid listOfConstants (Null)"); }
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

    public CharacterGoal[] GetArrayOfCharacterGoals()
    { return arrayOfGoalsLookup; }

    public CharacterMotivation[] GetArrayOfCharacterMotivation()
    { return arrayOfMotivationLookup; }

    public CharacterFocus[] GetArrayOfCharacterFocus()
    { return arrayOfFocusLookup; }

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
        int rnd = Random.Range(0, 100);
        CharacterIdentity identity = arrayOfIdentityLookup[rnd];
        //check roll again
        if (identity.isRollAgain == true)
        {
            int counter = 0;
            do
            {
                rnd = Random.Range(0, 100);
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
                rnd = Random.Range(0, 100);
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

    /// <summary>
    /// Get a list of character Goals (typically only one but could be two). Returns empty list if a problem
    /// </summary>
    /// <returns></returns>
    public List<string> GetCharacterGoal()
    {
        List<string> listOfGoals = new List<string>();
        int rnd = Random.Range(0, 100);
        CharacterGoal goal = arrayOfGoalsLookup[rnd];
        //check roll again
        if (goal.isRollAgain == true)
        {
            int counter = 0;
            do
            {
                rnd = Random.Range(0, 100);
                goal = arrayOfGoalsLookup[rnd];
                if (goal.isRollAgain == false)
                {
                    listOfGoals.Add(goal.tag);
                    counter++;
                }
            }
            while (counter < 2);
        }
        else { listOfGoals.Add(goal.tag); }
        return listOfGoals;
    }

    /// <summary>
    /// Get a list of character Motivation (typically only one but could be two). Returns empty list if a problem
    /// </summary>
    /// <returns></returns>
    public List<string> GetCharacterMotivation()
    {
        List<string> listOfMotivation = new List<string>();
        int rnd = Random.Range(0, 100);
        CharacterMotivation motivation = arrayOfMotivationLookup[rnd];
        //check roll again
        if (motivation.isRollAgain == true)
        {
            int counter = 0;
            do
            {
                rnd = Random.Range(0, 100);
                motivation = arrayOfMotivationLookup[rnd];
                if (motivation.isRollAgain == false)
                {
                    listOfMotivation.Add(motivation.tag);
                    counter++;
                }
            }
            while (counter < 2);
        }
        else { listOfMotivation.Add(motivation.tag); }
        return listOfMotivation;
    }

    /// <summary>
    /// Get a list of character Focus (typically only one but could be two strings). Returns empty list if a problem
    /// </summary>
    /// <returns></returns>
    public List<string> GetCharacterFocus()
    {
        List<string> listOfFocus = new List<string>();
        CharacterFocus focus;
        int rnd = Random.Range(0, 100);
        //do big picture first
        if (rnd < 20)
        { listOfFocus.Add("Nothing stands out"); }
        else if (rnd > 90)
        {
            //extreme
            rnd = Random.Range(0, 100);
            focus = arrayOfFocusLookup[rnd];
            listOfFocus.Add(string.Format("Extreme {0}", focus.tag));
        }
        else
        {
            //one or two focuses are important
            rnd = Random.Range(0, 100);
            focus = arrayOfFocusLookup[rnd];
            //check roll again
            if (focus.isRollAgain == true)
            {
                int counter = 0;
                do
                {
                    rnd = Random.Range(0, 100);
                    focus = arrayOfFocusLookup[rnd];
                    if (focus.isRollAgain == false)
                    {
                        listOfFocus.Add(focus.tag);
                        counter++;
                    }
                }
                while (counter < 2);
            }
            else { listOfFocus.Add(focus.tag); }
        }
        return listOfFocus;
    }

    #endregion

    #region Organisations
    //
    // - - - Organisations
    //

    public string GetRandomOrganisationType()
    { return listOfOrganisationType[Random.Range(0, listOfOrganisationType.Count)].tag; }

    public string GetRandomOrganisationOrigin()
    { return listOfOrganisationOrigin[Random.Range(0, listOfOrganisationOrigin.Count)].tag; }

    public string GetRandomOrganisationHistory()
    { return listOfOrganisationHistory[Random.Range(0, listOfOrganisationHistory.Count)].tag; }

    public string GetRandomOrganisationLeadership()
    { return listOfOrganisationLeadership[Random.Range(0, listOfOrganisationLeadership.Count)].tag; }

    public string GetRandomOrganisationMotivation()
    { return listOfOrganisationMotivation[Random.Range(0, listOfOrganisationMotivation.Count)].tag; }

    public string GetRandomOrganisationMethod()
    { return listOfOrganisationMethod[Random.Range(0, listOfOrganisationMethod.Count)].tag; }

    public string GetRandomOrganisationStrength()
    { return listOfOrganisationStrength[Random.Range(0, listOfOrganisationStrength.Count)].tag; }

    public string GetRandomOrganisationWeakness()
    { return listOfOrganisationWeakness[Random.Range(0, listOfOrganisationWeakness.Count)].tag; }

    public string GetRandomOrganisationObstacle()
    { return listOfOrganisationObstacle[Random.Range(0, listOfOrganisationObstacle.Count)].tag; }

    /// <summary>
    /// Populate listOfOrganisationType
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfOrganisationType(List<OrganisationDescriptor> tempList)
    {
        if (tempList != null)
        {
            listOfOrganisationType.Clear();
            listOfOrganisationType.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    /// <summary>
    /// Populate listOfOrganisationOrigin
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfOrganisationOrigin(List<OrganisationDescriptor> tempList)
    {
        if (tempList != null)
        {
            listOfOrganisationOrigin.Clear();
            listOfOrganisationOrigin.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    /// <summary>
    /// Populate listOfOrganisationHistory
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfOrganisationHistory(List<OrganisationDescriptor> tempList)
    {
        if (tempList != null)
        {
            listOfOrganisationHistory.Clear();
            listOfOrganisationHistory.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    /// <summary>
    /// Populate listOfOrganisationLeadership
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfOrganisationLeadership(List<OrganisationDescriptor> tempList)
    {
        if (tempList != null)
        {
            listOfOrganisationLeadership.Clear();
            listOfOrganisationLeadership.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    /// <summary>
    /// Populate listOfOrganisationMotivation
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfOrganisationMotivation(List<OrganisationDescriptor> tempList)
    {
        if (tempList != null)
        {
            listOfOrganisationMotivation.Clear();
            listOfOrganisationMotivation.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    /// <summary>
    /// Populate listOfOrganisationMethod
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfOrganisationMethod(List<OrganisationDescriptor> tempList)
    {
        if (tempList != null)
        {
            listOfOrganisationMethod.Clear();
            listOfOrganisationMethod.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    /// <summary>
    /// Populate listOfOrganisationStrength
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfOrganisationStrength(List<OrganisationDescriptor> tempList)
    {
        if (tempList != null)
        {
            listOfOrganisationStrength.Clear();
            listOfOrganisationStrength.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    /// <summary>
    /// Populate listOfOrganisationWeakness
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfOrganisationWeakness(List<OrganisationDescriptor> tempList)
    {
        if (tempList != null)
        {
            listOfOrganisationWeakness.Clear();
            listOfOrganisationWeakness.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    /// <summary>
    /// Populate listOfOrganisationObstacle
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfOrganisationObstacle(List<OrganisationDescriptor> tempList)
    {
        if (tempList != null)
        {
            listOfOrganisationObstacle.Clear();
            listOfOrganisationObstacle.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    #endregion

    #region ActorPool dictionaries
    //
    // - - - Actor Pool
    //

    public Dictionary<string, Trait> GetDictOfTraits()
    { return dictOfTraits; }

    public Dictionary<string, ActorArc> GetDictOfActorArcs()
    {return dictOfActorArcs; }

    /// <summary>
    /// Get a Trait from dictionary. Returns null if not found
    /// </summary>
    /// <param name="traitName"></param>
    /// <returns></returns>
    public Trait GetTrait(string traitName)
    {
        if (dictOfTraits.ContainsKey(traitName) == true)
        { return dictOfTraits[traitName]; }
        return null;
    }

    /// <summary>
    /// Get an ActorArc from dictionary. Returns null if not found
    /// </summary>
    /// <param name="arcName"></param>
    /// <returns></returns>
    public ActorArc GetActorArc(string arcName)
    {
        if (dictOfActorArcs.ContainsKey(arcName) == true)
        { return dictOfActorArcs[arcName]; }
        return null;
    }

    #endregion

    //new methods above here
}
