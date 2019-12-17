using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Text names of HQ positions that correspond with enum.ActorHQ (Boss/SubBoss1/SubBoss2/SubBoss3). Different set for either side
/// </summary>
[CreateAssetMenu(menuName = "Actor / HQ Position")]
public class HqPosition : ScriptableObject
{
    [Tooltip("Name of position, used in game)")]
    public string tag;
    [Tooltip("The enum.ActorHQ that this position corresponds to, information purposes only, eg. Boss/SubBoss1/SubBoss2/SubBoss3")]
    public string position;
    [Tooltip("Side, Authority or Resistance, equivalent positions can be different for both")]
    public GlobalSide side;
}
