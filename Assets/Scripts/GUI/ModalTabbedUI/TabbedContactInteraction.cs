using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles component interaction for TabbedContact prefab (ModalTabbedUI)
/// </summary>
public class TabbedContactInteraction : MonoBehaviour
{
    public Image background;
    public Image portrait;
    public TextMeshProUGUI effectiveness;
    public TextMeshProUGUI contactName;
    public TextMeshProUGUI job;
    public TextMeshProUGUI nodeName;
    public TextMeshProUGUI status;
    public TextMeshProUGUI intel;
    public GenericHelpTooltipUI help;


    public void Awake()
    {
        Debug.Assert(background != null, "Invalid background (Null)");
        Debug.Assert(portrait != null, "Invalid portrait (Null)");
        Debug.Assert(effectiveness != null, "Invalid effectiveness (Null)");
        Debug.Assert(contactName != null, "Invalid contactName (Null)");
        Debug.Assert(job != null, "Invalid job (Null)");
        Debug.Assert(nodeName != null, "Invalid nodeName (Null)");
        Debug.Assert(status != null, "Invalid status (Null)");
        Debug.Assert(intel != null, "Invalid intel (Null)");
        Debug.Assert(help != null, "Invalid help (Null)");
    }


}
