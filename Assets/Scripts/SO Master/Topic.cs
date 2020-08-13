using gameAPI;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Topic Decision 
/// </summary>
[CreateAssetMenu(menuName = "Topic / Topic")]
public class Topic : ScriptableObject
{
    [Header("Texts")]
    [Tooltip("Short descriptor (TWO words), used as HEADER for topic UI display and other outputs")]
    public string tag;
    [Tooltip("Topic text. Keep as short as possible")]
    [TextArea] public string text = "Unknown";
    [Tooltip("Designer notes, not used inGame")]
    [TextArea] public string designNotes;

    [Header("Categories")]
    [Tooltip("Topic Type that decision is associated with")]
    public TopicType type;
    [Tooltip("Topic SubType that the decision is associated with")]
    public TopicSubType subType;
    [Tooltip("Topic SubSubType that the decision is associated with. Ignore if not applicable")]
    public TopicSubSubType subSubType;

    [Header("Vitals")]
    [Tooltip("Overall priority for this topic when being placed in the selection pool")]
    public GlobalChance priority;
    [Tooltip("Is it in the Good, Nuetral or Bad group of topics for that SubType? If not relevant use Neutral")]
    public GlobalType group;
    [Tooltip("Which side (or 'Both') does this apply to?")]
    public GlobalSide side;

    [Header("Linked Lists")]
    [Tooltip("Once topic Live and resolved, the specified topics in the list (must be in same TopicPool) will switch from Done to Dormant (and activate as per it's profile). Ignore if not relevant")]
    public List<Topic> listOfLinkedTopics;
    [Tooltip("Any topics with the same linkedIndex, eg. All equivalent topics to the current one that could have been chosen but weren't. INCLUDE CURRENT TOPIC")]
    public List<Topic> listOfBuddyTopics;

    [Header("Linked Indexes")]
    [Tooltip("The position of the topic in the linked sequence (for Level) where 0 is the start. Multiple topics can have the same linkedIndex (e.g, Buddy topics). Default -1 indicates not part of a linked sequence")]
    public int linkedIndex = -1;
    [Tooltip("Specifies the level that the topic applies to. Campaign topics, with the same levelIndex (0 to Max, usually 4), play out in their linked index sequence. Default -1 for any other than Campaign scope")]
    public int levelIndex = -1;

    [Header("Story Special")]
    [Tooltip("TopicItem that provides a sprite and sprite tooltip for topic if needed (Story topics). Ignore otherwise")]
    public TopicItem topicItem;
    [Tooltip("StoryHelp for topic. For each (max 2) there will be a mouseover icon shown under the sprite. Optional")]
    public List<StoryHelp> listOfStoryHelp;

    [Header("Profile")]
    [Tooltip("Profile with timer and repeat data")]
    public TopicProfile profile;

    [Header("Criteria")]
    [Tooltip("In order for the topic to be valid all Criteria must be TRUE")]
    public List<Criteria> listOfCriteria;

    [Header("Ignore Penalty")]
    [Tooltip("Ingore effects occur if no option in the topic is selected. Can be good or bad. Optional")]
    public List<Effect> listOfIgnoreEffects;

    [Header("Options")]
    [Tooltip("Options for the decision. Max as per TopicManager.cs -> maxOptions")]
    public List<TopicOption> listOfOptions;

    [Header("Testing")]
    [Tooltip("If true topic is disabled and not available for selection")]
    public bool isDisabled;


    [HideInInspector] public string tooltipHeader;              //sprite tooltip
    [HideInInspector] public string tooltipMain;                //sprite tooltip
    [HideInInspector] public string tooltipDetails;             //sprite tooltip

    #region Save Data Compatible
    [HideInInspector] public Status status = Status.Dormant;
    [HideInInspector] public bool isCurrent;                                                //true if topic valid for current level (in a Campaign/city topicPool), false otherwise
    //timers
    [HideInInspector] public int timerStart;
    [HideInInspector] public int timerRepeat;
    [HideInInspector] public int timerWindow;
    //stats
    [HideInInspector] public int turnsDormant;
    [HideInInspector] public int turnsActive;
    [HideInInspector] public int turnsLive;
    [HideInInspector] public int turnsDone;
    #endregion


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(text) == false, "Invalid text (Null or Empty) for {0}", name);
        Debug.AssertFormat(type != null, "Invalid type (Null) for {0}", name);
        Debug.AssertFormat(subType != null, "Invalid subType (Null) for {0}", name);
        Debug.AssertFormat(priority != null, "Invalid priority (Null) for {0}", name);
        Debug.AssertFormat(listOfOptions != null, "Invalid listOfOptions (Null) for {0}", name);
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(profile != null, "Invalid profile (Null) for {0}", name);
        Debug.AssertFormat(group != null, "Invalid group (Null) for {0}", name);
    }

    /// <summary>
    /// Returns TopicOption of the specified index from listOfOptions, null if a problem
    /// </summary>
    /// <param name="optionIndex"></param>
    /// <returns></returns>
    public TopicOption GetOption(int optionIndex)
    {
        TopicOption option = null;
        if (optionIndex >= 0 && optionIndex < listOfOptions.Count)
        { option = listOfOptions[optionIndex]; }
        return option;
    }

}
