using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Topic enum.SO used by TopicSubTypes to globally define all topics within as Level (apply to this level only, reset thereafter) or Campaign scope (apply to campaign, do not reset status on a new level)
/// </summary>
[CreateAssetMenu(menuName = "Topic / TopicScope")]
public class TopicScope : ScriptableObject
{
    [Tooltip("Description (not used in game. Level scope is for an individual level (status reset at start of new level). Campaign scope carries over and status isn't reset")]
    public string descriptor;

}
