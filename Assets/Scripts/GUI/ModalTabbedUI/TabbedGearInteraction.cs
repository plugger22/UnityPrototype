using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles component interaction for Gear prefab in ModalTabbedUI
/// </summary>
public class TabbedGearInteraction : MonoBehaviour
{
    public Image background;
    public Image portrait;
    public TextMeshProUGUI descriptor;
    public TextMeshProUGUI uses;
    public TextMeshProUGUI stats;


    public void Awake()
    {
        Debug.Assert(background != null, "Invalid background (Null)");
        Debug.Assert(portrait != null, "Invalid portrait (Null)");
        Debug.Assert(descriptor != null, "Invalid descriptor (Null)");
        Debug.Assert(uses != null, "Invalid uses (Null)");
        Debug.Assert(stats != null, "Invalid stats (Null)");
    }

}
