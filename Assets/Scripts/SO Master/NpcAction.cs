﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Action taken by player to interact with Npc (when in same node and has located VIP)
/// </summary>
[CreateAssetMenu(menuName = "Game / Npc / NpcAction")]
public class NpcAction : ScriptableObject
{
    [Tooltip("In format '[You] ... takes action ... [the][Npc]', eg. 'take the package from'")]
    public string tag;
    [Tooltip("In format '[We want you to]...")]
    public string want;
    [Tooltip("What happens now in format '[the] [Npc] ...', eg. 'is no more'")]
    public string outcome;
}
