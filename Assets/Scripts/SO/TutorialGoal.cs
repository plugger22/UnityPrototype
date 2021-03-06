using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// enum.SO specifying a tutorialGoal for a tutorialItem
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialGoal")]
public class TutorialGoal : ScriptableObject
{
    [Tooltip("Developer notes only, not used inGame")]
    [TextArea] public string notes;
}
