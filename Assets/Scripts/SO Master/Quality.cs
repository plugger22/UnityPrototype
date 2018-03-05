using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Actor qualities, eg. Connections and Influence
/// </summary>
[CreateAssetMenu(menuName = "Actor / Quality")]
public class Quality: ScriptableObject
{
    public string description;
    public GlobalSide side;
}
