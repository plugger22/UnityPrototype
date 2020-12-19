using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles component interaction for Likes and Dislikes for ModalTabbedUI.cs
/// </summary>
public class TabbedLikesInteraction : MonoBehaviour
{

    public TextMeshProUGUI preferences;

    public void Awake()
    {
        Debug.Assert(preferences != null, "Invalid preferences (Null)");
    }
}
