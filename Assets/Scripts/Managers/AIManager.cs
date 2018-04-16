using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Handles AI management of both sides
/// </summary>
public class AIManager : MonoBehaviour
{
    [Tooltip("How many turns, after the event, that the AI will track Connection & Node activity before ignoring it")]
    [Range(5, 15)] public int activityTimeLimit = 10;

    /// <summary>
    /// Runs Resistance turn on behalf of AI
    /// </summary>
    public void ProcessAISideResistance()
    { }

    /// <summary>
    /// Runs Authority turn on behalf of AI
    /// </summary>
    public void ProcessAISideAuthority()
    { }

}
