using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// enum.SO specifying a tutorialGoal for a tutorialItem
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialGoal")]
public class TutorialGoal : ScriptableObject
{
    [Header("Notes")]
    [Tooltip("Developer notes only, not used inGame")]
    [TextArea] public string notes;

    [Header("Messaging")]
    [Tooltip("Descriptor used in-game. Explain to player what you want them to do")]
    [TextArea(1,2)] public string startTopText;
    [TextArea(3,4)] public string startBottomText;
    [Tooltip("Used in game. Message shown to player at completion of the goal")]
    [TextArea(1,2)] public string finishTopText;
    [TextArea] public string finishBottomText;

    [Header("Primary Goal")]
    [Tooltip("enum.SO that is used in code to look up the primary, tracker0 goal")]
    public TutorialGoalType goal0;
    [Tooltip("Explanatory notes only")]
    public string tracker0Notes;
    [Tooltip("Target value for first tracker in order to achieve goal. NOTE: Default -1, show bool as 1 or 0 (true/false)")]
    public int tracker0Target = -1;

    [Header("Secondary Goal")]
    [Tooltip("enum.SO that is used in code to look up goal associated with any secondary, tracker1 gaol. Can be ignored")]
    public TutorialGoalType goal1;
    [Tooltip("Explanatory notes only")]
    public string tracker1Notes;
    [Tooltip("Target value for second tracker (if any, leave blank if none) in order to achieve goal. NOTE: Default -1, show bool as 1 or 0 (true/false)")]
    public int tracker1Target = -1;
}
