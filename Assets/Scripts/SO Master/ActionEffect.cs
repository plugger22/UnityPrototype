using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effects for Actions carried out at Nodes
/// </summary>
[CreateAssetMenu(menuName = "Action / Effect")]
public class ActionEffect : ScriptableObject
{
    public int EffectID { get; set; }

    public string description;

}
