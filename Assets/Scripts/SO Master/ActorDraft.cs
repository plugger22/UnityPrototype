using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draft actor class that is used to enable customisation prior to being converted to a full Actor.cs class at the start of a new campaign
/// </summary>
[CreateAssetMenu(menuName = "Actor / ActorDraft")]
public class ActorDraft : ScriptableObject
{
    [Header("Names")]
    public string actorName;
    public string firstName;
    public string lastName;

    [Header("Details")]
    public Sprite sprite;
    public ActorArc arc;
    public Trait trait;
    public int level;
    public int power;

    [Header("Status")]
    public ActorDraftStatus status;
    public ActorDraftSex sex;

    [Header("Backstory")]
    [Tooltip("Four lines of text max (backstory1 is added on with a blank line between")]
    [TextArea(3, 5)] public string backstory0;
    [Tooltip("Two lines of text max (this is added on to backstory0 with a blank line between")]
    [TextArea(1, 2)] public string backstory1;

    [Header("Backstory Prompts")]
    public List<string> listOfIdentity;
    public List<string> listOfDescriptors;
    public List<string> listOfGoals;
    public List<string> listOfMotivation;
    public List<string> listOfFocus;
}
