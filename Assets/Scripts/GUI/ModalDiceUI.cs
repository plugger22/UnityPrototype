using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles dice rolling window and all related interaction
/// </summary>
public class ModalDiceUI : MonoBehaviour
{
    public GameObject modalDiceObject;
    public GameObject modalPanelObject;
    public TextMeshProUGUI textTop;
    public Button buttonLeft;
    public Button buttonMiddle;
    public Button buttonRight;

    private static ModalDiceUI modalDice;

    /// <summary>
    /// provide a static reference to ModalDiceUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalDiceUI Instance()
    {
        if (!modalDice)
        {
            modalDice = FindObjectOfType(typeof(ModalDiceUI)) as ModalDiceUI;
            if (!modalDice)
            { Debug.LogError("There needs to be one active modalDice script on a GameObject in your scene"); }
        }
        return modalDice;
    }

}
