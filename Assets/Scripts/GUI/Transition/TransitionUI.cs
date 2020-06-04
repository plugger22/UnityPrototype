using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// handles TransitionUI which is a single master UI with three togglable panels ->  end of level / HQ changes / Player status
/// </summary>
public class TransitionUI : MonoBehaviour
{
    [Header("Main")]
    public GameObject transitionObject;
    public Canvas transitionCanvas;
    public Image transitionBackground;
    public TextMeshProUGUI transitionHeader; 

    [Header("Main Buttons")]
    public Button buttonBack;
    public Button buttonContinue;
    public Button buttonHelpMain;

    [Header("End Level")]
    public Canvas endLevelCanvas;
    public Image endLevelBackground;

    [Header("HQ Status")]
    public Canvas hqCanvas;
    public Image hqBackground;

    [Header("Player Status")]
    public Canvas playerStatusCanvas;
    public Image playerStatusBackground;

    //static reference
    private static TransitionUI transitionUI;

    /// <summary>
    /// provide a static reference to TransitionUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TransitionUI Instance()
    {
        if (!transitionUI)
        {
            transitionUI = FindObjectOfType(typeof(TransitionUI)) as TransitionUI;
            if (!transitionUI)
            { Debug.LogError("There needs to be one active TransitionUI script on a GameObject in your scene"); }
        }
        return transitionUI;
    }

}
