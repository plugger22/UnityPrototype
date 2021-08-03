using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// holds child components for ModularHelpUI.cs -> quick access, home page, buttons
/// </summary>
public class MasterSetInteraction : MonoBehaviour
{
    public ButtonInteraction interact;
    public TextMeshProUGUI text;

    public void OnEnable()
    {
        Debug.AssertFormat(interact != null, "Invalid ButtonInteraction component (Null) for {0}", name);
        Debug.AssertFormat(text != null, "Invalid TextMeshProUGUI component (Null) for {0}", name);
    }

}
