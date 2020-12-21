using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles component interaction for TabbedStats in ModalTabbedUI.cs
/// </summary>
public class TabbedStatInteraction : MonoBehaviour
{

    public Image background;
    public TextMeshProUGUI header;
    public List<TabbedStatItemInteraction> listOfItems;

    public void Awake()
    {
        Debug.Assert(background != null, "Invalid background (Null)");
        Debug.Assert(header != null, "Invalid header (Null)");
        Debug.Assert(listOfItems.Count > 0, "Invalid listOfItems (Empty)");
    }

}
