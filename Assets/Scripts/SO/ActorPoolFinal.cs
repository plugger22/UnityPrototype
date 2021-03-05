using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Final, in-game, version of ActorPool.SOs
/// NOTE: These are created from script only -> Internal Tools
/// </summary>
public class ActorPoolFinal : ScriptableObject
{
    [Header("Details")]
    public string tag;
    public string author;
    public NameSet nameSet;
    public GlobalSide side;
    public string dateCreated;

    [Header("HQ Hierarchy")]
    public ActorDraftFinal hqBoss0;
    public ActorDraftFinal hqBoss1;
    public ActorDraftFinal hqBoss2;
    public ActorDraftFinal hqBoss3;
    
    [Header("HQ Workers")]
    [Tooltip("Should be 8 workers")]
    public List<ActorDraftFinal> listHqWorkers = new List<ActorDraftFinal>();

    [Header("OnMap")]
    [Tooltip("Max of 4 OnMap")]
    public List<ActorDraftFinal> listOnMap = new List<ActorDraftFinal>();

    [Header("Pools")]
    [Tooltip("Two full sets of 9 Arcs less Reserves and OnMap")]
    public List<ActorDraftFinal> listLevelOne = new List<ActorDraftFinal>();
    [Tooltip("One full set of 9 Arcs")]
    public List<ActorDraftFinal> listLevelTwo = new List<ActorDraftFinal>();
    [Tooltip("One full set of 9 Arcs")]
    public List<ActorDraftFinal> listLevelThree = new List<ActorDraftFinal>();

}
