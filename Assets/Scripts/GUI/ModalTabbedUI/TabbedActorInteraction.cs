using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles component interaction for Actor Image on canvas 0 -> ModalTabbedUI.cs
/// </summary>
public class TabbedActorInteraction : MonoBehaviour
{
    public GenericHelpTooltipUI helpPower;
    public GenericHelpTooltipUI helpCompatibility;

    public void Awake()
    {
        Debug.Assert(helpPower != null, "Invalid helpPower (Null)");
        Debug.Assert(helpCompatibility != null, "Invalid helpCompatibility (Null)");
    }
}
