using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Generic class to provide floating text pop ups of status changes for UI elements
/// </summary>
public class ModalPopUp : MonoBehaviour
{

    private TextMeshProUGUI display;

    static ModalPopUp modalPopUp;

    /// <summary>
    /// provide a static reference to TextPopUpUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalPopUp Instance()
    {
        if (!modalPopUp)
        {
            modalPopUp = FindObjectOfType(typeof(ModalPopUp)) as ModalPopUp;
            if (!modalPopUp)
            { Debug.LogError("There needs to be one active modalPopUp script on a GameObject in your scene"); }
        }
        return modalPopUp;
    }


}
