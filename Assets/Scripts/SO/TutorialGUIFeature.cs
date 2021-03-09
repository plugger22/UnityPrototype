using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI features toggle on/off for Tutorials
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialGUIFeature")]
public class TutorialGUIFeature : ScriptableObject
{

    [Tooltip("Developer notes only, not used in-game")]
    [TextArea] public string notes;

}
