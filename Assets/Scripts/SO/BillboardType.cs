using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Billboard text category (enum equivalent)
/// </summary>
[CreateAssetMenu(menuName = "Billboard / BillboardType")]
public class BillboardType : ScriptableObject
{

    [Tooltip("Descriptor only, not used in-game")]
    [TextArea] public string descriptor;
}
