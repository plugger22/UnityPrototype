using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Criteria that must be true for an Effect to occur
/// </summary>
[CreateAssetMenu(menuName = "Effect / Criteria")]
public class Criteria : ScriptableObject
{
    public string description;
    [Tooltip("The game data that is being checked")]
    public EffectCriteria effectCriteria;
    [Tooltip("Use only operands  > / < / = to")]
    public EffectOperator comparison;
    [Tooltip("The extent of the criteria check, eg. current Node, neighbouring nodes, Actor, etc.")]
    public EffectApply apply;
}
