using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all modal related gui items (modal masks are here 'cause they can't go in a prefab)
/// </summary>
public class ModalGUI : MonoBehaviour
{

    [Tooltip("Panel (1 of 2) that blocks UI input when first level gamestate.ModalUI in play. Greyed, full screen and Does NOT block raycast")]
    public GameObject modal0;
    [Tooltip("Panel (2 of 2) that blocks UI input when first level gamestate.ModalUI in play. Clear, partial screen and BLOCKS raycast")]
    public GameObject modal1;
    [Tooltip("Panel (1 of 1) that blocks UI input when second level gamestate.ModalUI in play. Clear, partial screen and BLOCKS raycast")]
    public GameObject modal2;

    private int modalLevel;             //level of modalUI, '0' if none, '1' if first level, '2' if second (eg. outcome window over an inventory window)

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

    /// <summary>
    /// Sets modal masks which are (for base level) a combination of two masks to provide an all over greyed background and a partial blocking of mouse input to UI elements
    /// level refers to modal level (can have multiple, like layers, separating UI components)
    /// </summary>
    public void SetModalMasks(bool isBlocked, int level)
    {
        switch (level)
        {
            case 1:
                if (isBlocked == true)
                {
                    //Block
                    modal0.SetActive(true);
                    modal1.SetActive(true);
                    modalLevel = 1;
                }
                else
                {
                    //Unblock
                    modal0.SetActive(false);
                    modal1.SetActive(false);
                    modalLevel = 0;
                }
                break;
            case 2:
                if (isBlocked == true)
                {
                    //Block
                    modal2.SetActive(true);
                    modalLevel = 2;
                }
                else
                {
                    //Unblock
                    modal2.SetActive(false);
                    modalLevel = 1;
                }
                break;
            default:
                Debug.LogError(string.Format("Invalid modalLevel {0}", modalLevel));
                break;
        }
    }


    public int CheckModalLevel()
    { return modalLevel; }

}
