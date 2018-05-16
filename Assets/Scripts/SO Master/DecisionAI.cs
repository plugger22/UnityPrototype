using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for AI Decisions. Name of SO is the name of the decision, eg. "Request Team"
/// </summary>
[CreateAssetMenu(menuName = "Decision / AI Decision")]
public class DecisionAI : ScriptableObject
{
    public string descriptor;
    public GlobalSide side;

    [Tooltip("Cost in AI Resources to carry out the Decision")]
    [Range(0, 5)] public int cost = 3;


    [HideInInspector] public int aiDecID;         //dynamically assigned by DataManager.cs on import

}
