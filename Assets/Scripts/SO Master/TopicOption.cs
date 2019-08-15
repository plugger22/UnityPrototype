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
    [Tooltip("Short descriptor")]
    public string tag;
    [Tooltip("Option text, max 140 chars")]
    public string text;

    [Header("Vitals")]
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
    [Tooltip("Ingore effects occur if the option isn't chosen. Can be good or bad. Optional")]
    public List<Effect> listOfIgnoreEffects;

    [Header("Player Personality Effect")]
    [Tooltip("Personality effect. Mood will increase if it Player's Personality matches the effect, decrease if it doesn't. No effect if neutral")]
    public Effect moodEffect;

    [Header("Probability")]
    [Tooltip("Anything other than 'None' makes this a Probability option where good effects occur if the roll succeeds and bad if it doesn't. Optional")]
    public GlobalChance chance;


    [HideInInspector] public string tooltipHeader;            //derived from effects
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipDetails;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null) for {0}", name);
        Debug.AssertFormat(topic != null, "Invalid topic (Null)");
    }
}
