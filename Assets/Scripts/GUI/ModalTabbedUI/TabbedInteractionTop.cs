using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles Actor Top tab interaction for TabbedUI
/// </summary>
public class TabbedInteractionTop : MonoBehaviour
{
    public TabbedTopTabUI topTabUI;
    public Image topTab;
    public TextMeshProUGUI text;

    public void Awake()
    {
        Debug.Assert(topTabUI != null, "Invalid topTabUI (Null)");
        Debug.Assert(topTab != null, "Invalid topTab (Null)");
        Debug.Assert(text != null, "Invalid text (Null)");
    }
}
