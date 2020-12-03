using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Interaction components for Tabbed Item prefab (ModalTabbedUI)
/// </summary>
public class TabbedItemInteraction : MonoBehaviour
{

    public TextMeshProUGUI text;
    public Image image;
    public GenericHelpTooltipUI help;


    public void Awake()
    {
        Debug.Assert(text != null, "Invalid text (Null)");
        Debug.Assert(image != null, "Invalid image (Null)");
        Debug.Assert(help != null, "Invalid help (Null)");
    }

}
