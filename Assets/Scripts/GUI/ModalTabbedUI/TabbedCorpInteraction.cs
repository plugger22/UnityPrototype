using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component interaction for MegaCorp.SO
/// </summary>
public class TabbedCorpInteraction : MonoBehaviour
{

    public Image background;                        //background sprite
    public Image portrait;
    public TextMeshProUGUI details;              //tag and description
    public TextMeshProUGUI descriptor;                   //Stats (Reputation and Freedom)
    public TextMeshProUGUI reputation;                //What services Org offers


    public void Awake()
    {
        Debug.Assert(background != null, "Invalid background (Null)");
        Debug.Assert(portrait != null, "Invalid portrait (Null)");
        Debug.Assert(descriptor != null, "Invalid descriptor (Null)");
        Debug.Assert(details != null, "Invalid details (Null)");
        Debug.Assert(reputation != null, "Invalid reputation (Null)");
    }

}
