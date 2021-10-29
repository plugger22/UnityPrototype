using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// enum SO that allows you to specify a special conditions (could be anything) that activates once the item triggers (Player clicks on it)
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialCondition")]
public class TutorialCondition : ScriptableObject
{
    [Tooltip("Not used in game, explanatory purposes only")]
    [TextArea] public string note;
}
