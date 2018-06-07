using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for AI Decisions. Name of SO is the name of the decision, eg. "Request Team"
/// </summary>
[CreateAssetMenu(menuName = "Decision / AI Decision")]
public class DecisionAI : ScriptableObject
{
    [Tooltip("Used for task description in the AIDisplayUI")]
    public string descriptor;
    [TextArea]
    [Tooltip("Used for task tooltip (further explanation) in the AIDisplayUI. Use Rich text formatting where needed (eg. '<b> </b>')")]
    public string tooltipDescriptor;

    public GlobalSide side;

    [Tooltip("Cost in AI Resources to carry out the Decision")]
    [Range(0, 5)] public int cost = 3;


    [HideInInspector] public int aiDecID;         //dynamically assigned by DataManager.cs on import

}
