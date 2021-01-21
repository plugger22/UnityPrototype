using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draft actor class that is used to enable customisation prior to being converted to a full Actor.cs class at the start of a new campaign
/// </summary>
[CreateAssetMenu(menuName = "Actor / ActorDraft")]
public class ActorDraft : ScriptableObject
{
    public string actorName;
    public Sprite sprite;

    public ActorArc arc;
    public Trait trait;
    public int level;
    public string backstory;
    public ActorDraftStatus status;
    public ActorDraftSex sex;

}
