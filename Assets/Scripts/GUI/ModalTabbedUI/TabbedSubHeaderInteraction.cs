using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allows access to TabbedItemInteraction components
/// Dual purpose, can be used with TabbedItem prefabs or with a straightforward list of strings
/// </summary>
public class TabbedSubHeaderInteraction : MonoBehaviour
{
    [Tooltip("Background subHeader image")]
    public Image image;

    [Tooltip("SubHeader header")]
    public TextMeshProUGUI text;

    [Tooltip("Set true if listOfItems valid for subHeader, false if listOfStrings valid instead")]
    public bool isItems;

    [Tooltip("Fill with TabbedItem prefabs is 'isItems' is true, ignore otherwise")]
    public List<TabbedItemInteraction> listOfItems;

    [Tooltip("Use to display strings (supplied by code) when 'isItems' is false, ignore otherwise. Field can be of varying size")]
    public TextMeshProUGUI descriptor;



    public void Awake()
    {
        Debug.Assert(image != null, "Invalid image (Null)");
        Debug.Assert(text != null, "Invalid text (Null)");
        //check list not empty and doesn't contain any null items
        if (isItems == true)
        {
            //validate listOfItems
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
        else
        {
            //validate descriptor field
            Debug.Assert(descriptor != null, "Invalid descriptor (Null)");
        }
    }
}
