using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalReviewUI : MonoBehaviour
{


    private static ModalReviewUI modalReviewUI;
    private ButtonInteraction buttonInteraction;


    /// <summary>
    /// Static instance so the ModalReviewUI can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalReviewUI Instance()
    {
        if (!modalReviewUI)
        {
            modalReviewUI = FindObjectOfType(typeof(ModalReviewUI)) as ModalReviewUI;
            if (!modalReviewUI)
            { Debug.LogError("There needs to be one active ModalReviewUI script on a GameObject in your scene"); }
        }
        return modalReviewUI;
    }

}
