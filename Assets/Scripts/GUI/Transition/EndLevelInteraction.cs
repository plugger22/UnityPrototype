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
    public TextMeshProUGUI hqRenown;
    public Image medal;
    public Image assessmentBackground;
    public TextMeshProUGUI assessmentText;
    public GenericTooltipUI tooltipPortrait;
    public GenericTooltipUI tooltipMedal;


    public void Awake()
    {
        Debug.Assert(hqPortrait != null, "Invalid hqPortrait (Null)");
        Debug.Assert(hqTitle != null, "Invalid hqTitle (Null)");
        Debug.Assert(hqRenown != null, "Invalid hqStars (Null)");
        Debug.Assert(medal != null, "Invalid medal (Null)");
        Debug.Assert(assessmentBackground != null, "Invalid statBackground (Null)");
        Debug.Assert(assessmentText != null, "Invalid statText (Null)");
        Debug.Assert(tooltipPortrait != null, "Invalid tooltipPortrait (Null)");
        Debug.Assert(tooltipMedal != null, "Invalid tooltipMedal (Null)");
    }
}
