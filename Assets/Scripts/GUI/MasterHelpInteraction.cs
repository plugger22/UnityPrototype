using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Provides access to components for MasterHelpOptions (prefab) in ModalHelpUI.cs
/// </summary>
public class MasterHelpInteraction : MonoBehaviour
{
    public Image panel;
    public TextMeshProUGUI text;

    [HideInInspector] public int index;                      //each option is assigned an index which corresponds to the index for the linked lists in ModalHelpUI.cs
}
