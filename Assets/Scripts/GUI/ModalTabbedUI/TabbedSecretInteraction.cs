using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles component interaction for Secret prefabs for ModalTabbedUI.cs
/// </summary>
public class TabbedSecretInteraction : MonoBehaviour
{
    public Image background;                    //background sprite
    public Image portrait;
    public TextMeshProUGUI descriptor;         //tag and description
    public TextMeshProUGUI effects;             //effects if revealed        
    public TextMeshProUGUI known;              //who knows the secret


    public void Awake()
    {
        Debug.Assert(background != null, "Invalid background (Null)");
        Debug.Assert(portrait != null, "Invalid portrait (Null)");
        Debug.Assert(descriptor != null, "Invalid descriptor (Null)");
        Debug.Assert(effects != null, "Invalid effects (Null)");
        Debug.Assert(known != null, "Invalid known (Null)");
    }

}
