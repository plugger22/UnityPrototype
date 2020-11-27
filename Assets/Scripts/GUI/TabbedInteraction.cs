using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles Actor tab interaction for TabbedUI
/// </summary>
public class TabbedInteraction : MonoBehaviour
{

    public Image background;
    public Image portrait;
    public TextMeshProUGUI title;



    public void Awake()
    {
        Debug.AssertFormat(background != null, "Invalid background (Null) for {0}", this);
        Debug.AssertFormat(portrait != null, "Invalid portrait (Null) for {0}", this);
        Debug.AssertFormat(title != null, "Invalid title (Null) for {0}", this);
    }



}
