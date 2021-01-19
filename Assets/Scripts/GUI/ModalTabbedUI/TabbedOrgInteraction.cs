using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles component interaction for Organisations -> ModalTabbedUI
/// </summary>
public class TabbedOrgInteraction : MonoBehaviour
{

    public Image background;                        //background sprite
    public Image portrait;
    public TextMeshProUGUI descriptor;              //tag and description
    public TextMeshProUGUI stats;                   //Stats (Reputation and Freedom)
    public TextMeshProUGUI services;                //What services Org offers


    public void Awake()
    {
        Debug.Assert(background != null, "Invalid background (Null)");
        Debug.Assert(portrait != null, "Invalid portrait (Null)");
        Debug.Assert(descriptor != null, "Invalid descriptor (Null)");
        Debug.Assert(stats != null, "Invalid descriptor (Null)");
        Debug.Assert(services != null, "Invalid use (Null)");
    }
}
