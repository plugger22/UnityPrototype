using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Action taken by player to interact with VIP (when in same node and has located VIP)
/// </summary>
[CreateAssetMenu(menuName = "Game / VIP / VipAction")]
public class VipAction : ScriptableObject
{
    [Tooltip("Action menu (Right click) item name")]
    public string tag;
    [Tooltip("Action menu (Right click) item tooltip")]
    public string tooltip;
}
