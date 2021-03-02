using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum SO for TutorialItem.SO's to specify their type
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialType")]
public class TutorialType : ScriptableObject
{
    [Tooltip("Developer notes only, not used inGame")]
    [TextArea] public string notes;

}
