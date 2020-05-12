using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Generic class to provide floating text pop ups of status changes for UI elements
/// </summary>
public class TextPopUpUI : MonoBehaviour
{

    private TextMeshProUGUI display;

    static TextPopUpUI textPopUpUI;

    /// <summary>
    /// provide a static reference to TextPopUpUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TextPopUpUI Instance()
    {
        if (!textPopUpUI)
        {
            textPopUpUI = FindObjectOfType(typeof(TextPopUpUI)) as TextPopUpUI;
            if (!textPopUpUI)
            { Debug.LogError("There needs to be one active textPopUI script on a GameObject in your scene"); }
        }
        return textPopUpUI;
    }

    /*public TextPopUpUI Create(Vector3 position, int numToDisplay)
    {
        Transform textPopUpTransform = Instantiate(GameManager.i.modalGUIScript.popUpTransform, position, Quaternion.identity);
        TextPopUpUI textPop = textPopUpTransform.GetComponent<TextPopUpUI>();
        return textPop;
    }*/
}
