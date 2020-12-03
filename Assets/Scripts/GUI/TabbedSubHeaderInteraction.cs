using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Allows access to TabbedItemInteraction components
/// </summary>
public class TabbedSubHeaderInteraction : MonoBehaviour
{

    public TextMeshProUGUI text;
    public List<TabbedItemInteraction> listOfItems;



    public void Awake()
    {
        Debug.Assert(text != null, "Invalid text (Null)");
        Debug.Assert(listOfItems.Count > 0, "Invalid listOfItems (Empty)");
    }
}
