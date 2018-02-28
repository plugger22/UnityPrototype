using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Actor Management. Name of SO is the type of managerial action, eg. "Dismiss"
/// </summary>
[CreateAssetMenu(menuName = "Manage / Manage")]
public class ManageActor : ScriptableObject
{
    public string descriptor;
}
