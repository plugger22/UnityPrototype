using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// enum.SO specifying a tutorialGoal for a tutorialItem
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialGoal")]
public class TutorialGoal : ScriptableObject
{
    [Tooltip("Descriptor used in-game. Explain to player what you want them to do")]
    [TextArea] public string descriptor;
    [Tooltip("Developer notes only, not used inGame")]
    [TextArea] public string notes;

    [Tooltip("Keyword that is used in code to look up the primary, tracker0 goal")]
    public string goal0;
    [Tooltip("Explanatory notes only")]
    public string tracker0Notes;
    [Tooltip("Target value for first tracker in order to achieve goal. NOTE: Default -1, show bool as 1 or 0 (true/false)")]
    public int tracker0Target = -1;

    [Tooltip("Keyword that is used in code to look up goal associated with any secondary, tracker1 gaol. Can be ignored")]
    public string goal1;
    [Tooltip("Explanatory notes only")]
    public string tracker1Notes;
    [Tooltip("Target value for second tracker (if any, leave blank if none) in order to achieve goal. NOTE: Default -1, show bool as 1 or 0 (true/false)")]
    public int tracker1Target = -1;
}
