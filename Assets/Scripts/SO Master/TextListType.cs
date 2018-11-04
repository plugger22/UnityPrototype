using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TextListType SO. Name of SO is the type of the TextList, eg. "Contacts"
/// </summary>
[CreateAssetMenu(menuName = "TextList / TextListType")]
public class TextListType : ScriptableObject
{

    [Tooltip("Descriptor only, not used in-game")]
    public string descriptor;

}
