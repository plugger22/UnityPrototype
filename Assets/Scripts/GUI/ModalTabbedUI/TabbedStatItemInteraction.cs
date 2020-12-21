using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles component interaction for TabbedStatItems for ModalTabbedUI.cs
/// </summary>
public class TabbedStatItemInteraction : MonoBehaviour
{
    public Image background;
    public TextMeshProUGUI descriptor;
    public TextMeshProUGUI result;

    public void Awake()
    {
        Debug.Assert(background != null, "Invalid background (Null)");
        Debug.Assert(descriptor != null, "Invalid descriptor (Null)");
        Debug.Assert(result != null, "Invalid result (Null)");
    }
}
