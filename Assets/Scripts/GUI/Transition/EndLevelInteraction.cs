using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// attached to prefab endLevelOption for TransitionUI End Level screen
/// </summary>
public class EndLevelInteraction : MonoBehaviour
{

    public Image hqPortrait;
    public TextMeshProUGUI hqTitle;
    public Image medal;
    public Image commentBackground;
    public TextMeshProUGUI commentText;
    public Image statBackground;
    public TextMeshProUGUI statText;
    public Image barLeft;
    public Image barRight;
    public TextMeshProUGUI barTextLeft;


    public void Awake()
    {
        Debug.Assert(hqPortrait != null, "Invalid hqPortrait (Null)");
        Debug.Assert(hqTitle != null, "Invalid hqTitle (Null)");
        Debug.Assert(medal != null, "Invalid medal (Null)");
        Debug.Assert(commentBackground != null, "Invalid commentBackground (Null)");
        Debug.Assert(commentText != null, "Invalid commentText (Null)");
        Debug.Assert(statBackground != null, "Invalid statBackground (Null)");
        Debug.Assert(statText != null, "Invalid statText (Null)");
        Debug.Assert(barLeft != null, "Invalid barLeft (Null)");
        Debug.Assert(barRight != null, "Invalid barRight (Null)");
        Debug.Assert(barTextLeft != null, "Invalid barTextLeft (Null)");
    }
}
