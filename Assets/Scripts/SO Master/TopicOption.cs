using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Topic Options (component of TopicDecisions)
/// </summary>
[CreateAssetMenu(menuName = "Topic / TopicOption")]
public class TopicOption : ScriptableObject
{
    [Header("Texts")]
    [Tooltip("Full text explanation of option, eg 'Actor Contact 0 option 2', not used inGame")]
    public string descriptor;
    [Tooltip("Short descriptor (ONE word, used for option tooltip header and effect outcome messages)")]
    public string tag;
    [Tooltip("Option text, max 140 chars")]
    public string text;

    [Header("Parent")]
    [Tooltip("Topic that the option is connected with")]
    public Topic topic;

    [Header("Special")]
    [Tooltip("Select this if the option is preferred by HQ. Doing so will override the HQ Boss's personality preferences. Ignore otherwise")]
    public bool isPreferredByHQ;
    [Tooltip("Select this is the option is disliked by HQ. Doing so will override the HQ Boss's personality preference and give a 'disapprove' result")]
    public bool isDislikedByHQ;
    [Tooltip("Select this if the option is irrelevant to HQ. Doing so will override the HQ Boss's personality preferences and give a 'no opinion' result")]
    public bool isIgnoredByHQ;
    [Tooltip("If true the random option disabled, due to Player being Stressed, effect is unavailable (eg. all options available regardless of Player stress status). Optional (default false, effect applies")]
    public bool isIgnoreStress;
    [Tooltip("If true Player Mood effects will be ignore and tooltip will show 'No Effect On Player Mood' (default false, effects apply)")]
    public bool isIgnoreMood;

    [Header("Criteria")]
    [Tooltip("Option will show as Greyed text if criteria check fails. At least one option from each topic needs to have Zero criteria so an option is always available. Optional")]
    public List<Criteria> listOfCriteria;

    [Header("Effects")]
    [Tooltip("Good effects that occur. Normal options can have a mix of good and bad effects. Probability options give good effects only if roll succeeds.Optional")]
    public List<Effect> listOfGoodEffects;
    [Tooltip("Bad effects that occur. Normal options can have a mix of good and bad effects, Probability options give bad effects only if roll fails. Optional")]
    public List<Effect> listOfBadEffects;


    [Header("Player Personality Effect")]
    [Tooltip("Personality effect. Mood will increase if it Player's Personality matches the effect, decrease if it doesn't. No effect if neutral")]
    public Effect moodEffect;

    [Header("Probability")]
    [Tooltip("Anything other than 'None' makes this a Probability option where good effects occur if the roll succeeds and bad if it doesn't. Optional")]
    public GlobalChance chance;

    [Header("Newsfeed")]
    [Tooltip("Self contained new snippet that reflects Player's choice of this option. Can include tags. Optional")]
    [TextArea] public string news;

    [Header("Story Effects")]
    [Tooltip("Output (bottom text) for a story Option effect, ignore otherwise")]
    [TextArea] public string storyInfo;
    [Tooltip("Output for a story Target effect, ignore otherwise")]
    public Target storyTarget;

    [HideInInspector] public string tooltipHeader;              //tag
    [HideInInspector] public string tooltipMain;                //derived from effects (good / bad)
    [HideInInspector] public string tooltipDetails;             //derived from mood Effect
    [HideInInspector] public bool isValid;                      //true if passed criteria checks, false otherwise (displayed in greyed text)
    [HideInInspector] public string textToDisplay;              //colour formatted option text ready for display by TopicUI.cs (TopicManager.cs -> InitialiseTopicUI)
    [HideInInspector] public int optionNumber;                  //number of option for topic, eg. 0 -> 3, if named 'PlyRes3Opt1' then it's number is 1 (last character). 


    public void OnEnable()
    {
        /*Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name); DEBUG -> need to activate this once topics in place*/
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(topic != null, "Invalid topic (Null)");
        //assign optionNumber based on TopicOption name (last char is number of option in sequence, eg. 0 -> 3)
        string last;
        optionNumber = -1;
        try
         {
            last = name.Substring(name.Length - 1, 1);
            optionNumber = Convert.ToInt32(last);
        }
        catch(ArgumentOutOfRangeException)
        { Debug.LogWarningFormat("Invalid subString arguments for last for {0}", name); }
        catch(OverflowException)
        { Debug.LogWarningFormat("Invalid conversion, Overflow exception for {0}", name); }
        catch(FormatException)
        { Debug.LogWarningFormat("Invalid conversion, formatException for {0}", name); }
        Debug.AssertFormat(optionNumber > -1 && optionNumber < 4, "Invalid optionNumber \"{0}\" (should be 0 to 3) for {1}", optionNumber, name);
    }
}
