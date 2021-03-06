﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Final, in-game, version of ActorDraft.SO
/// NOTE: These are created by script only -> InternalTools
/// </summary>
public class ActorDraftFinal : ScriptableObject
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

}
