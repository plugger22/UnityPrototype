using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all modal related gui items (modal masks are here 'cause they can't go in a prefab)
/// </summary>
public class ModalGUI : MonoBehaviour
{

    [Tooltip("Panel that blocks UI input when first level gamestate.ModalUI in ply")]
    public GameObject modal1;
    [Tooltip("Panel that blocks UI input when second level gamestate.ModalUI in ply")]
    public GameObject modal2;

    private static ModalGUI modalGUI;

    /// <summary>
    /// Static instance so the ModalGUI can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalGUI Instance()
    {
        if (!modalGUI)
        {
            modalGUI = FindObjectOfType(typeof(ModalGUI)) as ModalGUI;
            if (!modalGUI)
            { Debug.LogError("There needs to be one active ModalGUI script on a GameObject in your scene"); }
        }
        return modalGUI;
    }
}
