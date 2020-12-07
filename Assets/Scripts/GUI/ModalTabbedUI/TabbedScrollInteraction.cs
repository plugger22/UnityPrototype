using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Interaction for tab scroll item components
/// </summary>
public class TabbedScrollInteraction : MonoBehaviour
{

    public TextMeshProUGUI descriptor;


    public void Awake()
    {
        Debug.Assert(descriptor != null, "Invalid descriptor (Null)");
    }
}
