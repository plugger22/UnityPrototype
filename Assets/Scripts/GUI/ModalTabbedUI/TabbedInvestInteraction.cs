using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles component interaction for Investigation prefab in ModalTabbedUI
/// </summary>
public class TabbedInvestInteraction : MonoBehaviour
{

    public Image background;
    public Image portrait;
    public TextMeshProUGUI leftText;
    public TextMeshProUGUI middleText;
    public TextMeshProUGUI rightText;

    public void Awake()
    {
        Debug.Assert(background != null, "Invalid background (Null)");
        Debug.Assert(portrait != null, "Invalid portrait (Null)");
        Debug.Assert(leftText != null, "Invalid leftText (Null)");
        Debug.Assert(middleText != null, "Invalid middleText (Null)");
        Debug.Assert(rightText != null, "Invalid rightText (Null)");
    }
}
