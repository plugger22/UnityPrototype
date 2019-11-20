using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Action taken by player to interact with VIP (when in same node and has located VIP)
/// </summary>
[CreateAssetMenu(menuName = "Game / VIP / VipAction")]
public class VipAction : ScriptableObject
{
    [Tooltip("In format '[Player] ... takes action ... [VIP]', eg. 'takes package from'")]
    public string tag;
    [Tooltip("What happens now in format '[the] [VIP] ...', eg. 'is no more'")]
    public string outcome;
}
