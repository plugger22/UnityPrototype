using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles component interaction for Device (capture tool) prefabs for ModalTabbedUI.cs
/// </summary>
public class TabbedDeviceInteraction : MonoBehaviour
{
    public Image background;                    //background sprite
    public Image portrait;
    public TextMeshProUGUI descriptor;          //tag and description
    public TextMeshProUGUI usage;               //innocence level device can be used at


    public void Awake()
    {
        Debug.Assert(background != null, "Invalid background (Null)");
        Debug.Assert(portrait != null, "Invalid portrait (Null)");
        Debug.Assert(descriptor != null, "Invalid descriptor (Null)");
        Debug.Assert(usage != null, "Invalid use (Null)");
    }


}
