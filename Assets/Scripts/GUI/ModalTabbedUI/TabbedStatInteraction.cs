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
    public List<TabbedStatItemInteraction> listOfItems;         //should be 4

    public void Awake()
    {
        Debug.Assert(background != null, "Invalid background (Null)");
        Debug.Assert(header != null, "Invalid header (Null)");
        Debug.AssertFormat(listOfItems.Count == 4, "Invalid listOfItems (is {0}, should be {1})", listOfItems.Count, 4);
    }

}
