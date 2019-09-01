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


    [HideInInspector] public string tooltipHeader;              //tag
    [HideInInspector] public string tooltipMain;                //derived from effects (good / bad)
    [HideInInspector] public string tooltipDetails;             //derived from mood Effect
    [HideInInspector] public bool isValid;                      //true if passed criteria checks, false otherwise (displayed in greyed text)
    [HideInInspector] public string textToDisplay;              //colour formatted option text ready for display by TopicUI.cs (TopicManager.cs -> InitialiseTopicUI)


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(topic != null, "Invalid topic (Null)");

    }
}
