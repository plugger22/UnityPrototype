using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// attached to prefabs hqOption and workerOption for transitionUI HQ screen
/// </summary>
public class HqInteraction : MonoBehaviour
{

    public Image optionImage;
    public TextMeshProUGUI textUpper;
    public TextMeshProUGUI textLower;
    public GenericTooltipUI optionTooltip;
    public GenericTooltipUI compatibilityTooltip;

    public void Awake()
    {
        Debug.Assert(optionImage != null, "Invalid optionImage (Null)");
        Debug.Assert(textUpper != null, "Invalid textUpper (Null)");
        Debug.Assert(textLower != null, "Invalid textLower (Null)");
        Debug.Assert(optionTooltip != null, "Invalid optionTooltip (Null)");
        Debug.Assert(compatibilityTooltip != null, "Invalid compatibilityTooltip (Null)");
    }
}
