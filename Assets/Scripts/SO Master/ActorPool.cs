﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// container class holding enough ActorDraft.SO's for a campaign to enable customation prior to conversion to Actor.cs
/// </summary>
[CreateAssetMenu(menuName = "Actor / ActorPool")]
public class ActorPool : ScriptableObject
{
    [Header("Details")]
    public int numOfActors = 48;        //4 x hq hierarchy, 8 x workers, 4 x OnMap, 5 + 9 x level One, 9 x level Two, 9 x level three
    public string tag;
    public string author;
    public string nameSet;
    public string campaign;

    [Header("HQ Hierarchy")]
    public ActorDraft hqBoss0;
    public ActorDraft hqBoss1;
    public ActorDraft hqBoss2;
    public ActorDraft hqBoss3;

    [Header("HQ Workers")]
    [Tooltip("Should be 8 workers")]
    public List<ActorDraft> listHqWorkers = new List<ActorDraft>();

    [Header("OnMap")]
    [Tooltip("Max of 4 OnMap")]
    public List<ActorDraft> listOnMap = new List<ActorDraft>();

    [Header("Pools")]
    [Tooltip("Two full sets of 9 Arcs less Reserves and OnMap")]
    public List<ActorDraft> listLevelOne = new List<ActorDraft>();
    [Tooltip("One full set of 9 Arcs")]
    public List<ActorDraft> listLevelTwo = new List<ActorDraft>();
    [Tooltip("One full set of 9 Arcs")]
    public List<ActorDraft> listLevelThree = new List<ActorDraft>();
}
