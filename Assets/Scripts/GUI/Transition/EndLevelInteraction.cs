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
    public TextMeshProUGUI hqStars;
    public Image medal;
    public Image statBackground;
    public TextMeshProUGUI statText;



    public void Awake()
    {
        Debug.Assert(hqPortrait != null, "Invalid hqPortrait (Null)");
        Debug.Assert(hqTitle != null, "Invalid hqTitle (Null)");
        Debug.Assert(hqStars != null, "Invalid hqStars (Null)");
        Debug.Assert(medal != null, "Invalid medal (Null)");
        Debug.Assert(statBackground != null, "Invalid statBackground (Null)");
        Debug.Assert(statText != null, "Invalid statText (Null)");
    }
}
