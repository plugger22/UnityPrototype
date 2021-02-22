using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tutorial features (toggle on/off)
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialFeature")]
public class TutorialFeature : ScriptableObject
{

    [Tooltip("Developer notes only, not used in-game")]
    [TextArea] public string notes;
}
