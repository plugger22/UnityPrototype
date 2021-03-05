using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// enum SO that defines a particular layout for a help UI
/// </summary>
[CreateAssetMenu(menuName = "Help / Layout")]
public class Layout : ScriptableObject
{
    [Tooltip("Developer notes only, not used inGame")]
    [TextArea] public string notes;

}
