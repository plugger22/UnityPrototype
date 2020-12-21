using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles component interaction for Likes and Dislikes for ModalTabbedUI.cs
/// </summary>
public class TabbedLikesInteraction : MonoBehaviour
{
    public Image background;
    public TextMeshProUGUI subHeader;
    public TextMeshProUGUI preferences;

    public void Awake()
    {
        Debug.Assert(background != null, "Invalid background (Null)");
        Debug.Assert(subHeader != null, "Invalid subHeader (Null)");
        Debug.Assert(preferences != null, "Invalid preferences (Null)");
    }
}
