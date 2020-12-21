using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// handles component interaction for TabbedStatItems for ModalTabbedUI.cs
/// </summary>
public class TabbedStatItemInteraction : MonoBehaviour
{

    public TextMeshProUGUI descriptor;
    public TextMeshProUGUI result;

    public void Awake()
    {
        Debug.Assert(descriptor != null, "Invalid descriptor (Null)");
        Debug.Assert(result != null, "Invalid result (Null)");
    }
}
