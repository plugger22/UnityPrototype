using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles component access for TutorialUI buttons on RHS
/// </summary>
public class TutorialButtonInteraction : MonoBehaviour
{
    public Image buttonImage;
    public Image arrowImage;
    public TextMeshProUGUI buttonText;
    public ButtonInteraction buttonInteraction;
    public GenericTooltipUI tooltip;

    public void OnEnable()
    {
        Debug.Assert(buttonImage != null, "Invalid buttonImage (Null)");
        Debug.Assert(arrowImage != null, "Invalid arrowImage (Null)");
        Debug.Assert(buttonText != null, "Invalid buttonText (Null)");
        Debug.Assert(buttonInteraction != null, "Invalid buttonInteraction (Null)");
        Debug.Assert(tooltip != null, "Invalid (Generic) tooltip (Null)");
    }
}
