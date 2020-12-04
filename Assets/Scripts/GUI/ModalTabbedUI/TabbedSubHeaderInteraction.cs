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
        //check list not empty and doesn't contain any null items
        if (listOfItems.Count > 0)
        {
            for (int i = 0; i < listOfItems.Count; i++)
            {
                if (listOfItems[i] == null)
                { Debug.LogErrorFormat("Invalid item (Null) in listOfItems[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfItems (Empty)"); }
    }
}
