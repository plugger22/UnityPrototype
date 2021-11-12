using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Context sensitive, or not, help messages that appear at intervals during tutorial or main game. InfoPipeline messages.
/// </summary>
[CreateAssetMenu(menuName = "Help / HelpMessage")]
public class HelpMessage : ScriptableObject
{
    [Tooltip("Dialogue (special Outcome good) top, headline, text")]
    [TextArea] public string textTop;
    [Tooltip("Dialogue (special Outcome good) bottom, explanatory, text")]
    [TextArea] public string textBottom;

    [Tooltip("Any conditions that apply in order to make the message context sensitive. Optional")]
    public List<HelpCondition> listOfConditions;

}
