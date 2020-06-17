using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attaches to workerOption interaction for transitionUI for HQ screen
/// </summary>
public class WorkerInteraction : MonoBehaviour
{

    public Image optionImage;
    public TextMeshProUGUI textUpper;
    public TextMeshProUGUI textLower;
    public GenericTooltipUI optionTooltip;
    public GenericTooltipUI renownTooltip;

    public void Awake()
    {
        Debug.Assert(optionImage != null, "Invalid optionImage (Null)");
        Debug.Assert(textUpper != null, "Invalid textUpper (Null)");
        Debug.Assert(optionTooltip != null, "Invalid optionTooltip (Null)");
        Debug.Assert(renownTooltip != null, "Invalid compatibilityTooltip (Null)");
    }
}
