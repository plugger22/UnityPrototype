﻿using gameAPI;
using modalAPI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Confirmation window (message with Yes/No buttons). Modal 1 or 2
/// bottomText shown in yellow, left button default 'No', right button default 'Yes'
/// </summary>
public class ModalConfirm : MonoBehaviour
{

    public GameObject confirmObject;
    public TextMeshProUGUI topText;
    public TextMeshProUGUI bottomText;
    public Button buttonFalse;
    public Button buttonTrue;
    public TextMeshProUGUI buttonFalseText;
    public TextMeshProUGUI buttonTrueText;

    public static ModalConfirm modalConfirm;

    private int modalLevel;                                 //modal level of menu, passed in by ModalConfirmDetails in SetModalConfirm
    private ModalSubState modalState;                       //modal state to return to once confirm window closed (handles modalLevel 2+ cases, ignored for rest)

    private bool result;                                    //outcome of button press, false for buttonFalse, true for button true. Retrieve by CheckResult()

    private ModalConfirmDetails confirmDetails;

    /// <summary>
    /// provide a static reference to ModalConfirm that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalConfirm Instance()
    {
        if (!modalConfirm)
        {
            modalConfirm = FindObjectOfType(typeof(ModalConfirm)) as ModalConfirm;
            if (!modalConfirm)
            { Debug.LogError("There needs to be one active modalConfirm script on a GameObject in your scene"); }
        }
        return modalConfirm;
    }


    public void Start()
    {
        Debug.Assert(confirmObject != null, "Invalid confirmObject (Null)");
        Debug.Assert(topText != null, "Invalid topText (Null)");
        Debug.Assert(bottomText != null, "Invalid bottomText (Null)");
        Debug.Assert(buttonFalse != null, "Invalid buttonLeft (Null)");
        Debug.Assert(buttonTrue != null, "Invalid buttonRight (Null)");
        //buttons -> interaction components
        ButtonInteraction interactLeft = buttonFalse.GetComponent<ButtonInteraction>();
        if (interactLeft != null)
        { interactLeft.SetButton(EventType.ConfirmCloseLeft); }
        ButtonInteraction interactRight = buttonTrue.GetComponent<ButtonInteraction>();
        if (interactRight != null)
        { interactRight.SetButton(EventType.ConfirmCloseRight); }
        //details empty
        confirmDetails = null;
        //register a listener
        EventManager.i.AddListener(EventType.ConfirmCloseLeft, OnEvent, "ModalConfirm");
        EventManager.i.AddListener(EventType.ConfirmCloseRight, OnEvent, "ModalConfirm");
        EventManager.i.AddListener(EventType.ConfirmOpen, OnEvent, "ModalConfirm");
    }


    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //detect event type
        switch (eventType)
        {
            case EventType.ConfirmOpen:
                ModalConfirmDetails details = Param as ModalConfirmDetails;
                SetModalConfirm(details);
                break;
            case EventType.ConfirmCloseLeft:
                ConfirmCloseLeft();
                break;
            case EventType.ConfirmCloseRight:
                ConfirmCloseRight();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Initialise and display ModalConfirm window
    /// </summary>
    /// <param name="details"></param>
    private void SetModalConfirm(ModalConfirmDetails details)
    {
        if (details != null)
        {
            confirmDetails = details;
            //close tooltips
            GameManager.i.guiScript.SetTooltipsOff();

            topText.text = details.topText;
            bottomText.text = details.bottomText;
            buttonFalseText.text = details.buttonFalse;
            buttonTrueText.text = details.buttonTrue;

            //set modal true
            GameManager.i.guiScript.SetIsBlocked(true, details.modalLevel);
            //pass through data for when the confirm window is closed
            modalLevel = details.modalLevel;
            modalState = details.modalState;
            //set states
            GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.Confirm });
            Debug.LogFormat("[UI] ModalConfirm.cs -> SetModalConfirm{0}", "\n");
            //switch on object
            confirmObject.SetActive(true);
        }
        else { Debug.LogError("Invalid ModalConfirmDetails (Null)"); }
    }

    /// <summary>
    /// False button, default 'No', pressed. Returns false
    /// </summary>
    private void ConfirmCloseLeft()
    {
        CloseConfirm();
        result = false;
        //optional event to run on buttonFalse being clicked
        if (confirmDetails.eventFalse != EventType.None)
        {
            //Sends 'RestorePoint' as a paramter (if present) in case of 'Save and Exit' to allow user to change their mind and return to game
            if (confirmDetails.restorePoint != RestorePoint.None)
            { EventManager.i.PostNotification(confirmDetails.eventFalse, this, confirmDetails.restorePoint, "ModalConfirm.cs -> ConfirmCloseLeft"); }
            else
            {
                //No restorePoint, normal event, no parameter
                EventManager.i.PostNotification(confirmDetails.eventFalse, this, null, "ModalConfirm.cs -> ConfirmCloseLeft");
            }
        }
    }


    /// <summary>
    /// True button, default 'Yes', pressed. Return true
    /// </summary>
    private void ConfirmCloseRight()
    {

        CloseConfirm();
        result = true;
        //optional event to run on buttonTrue being clicked
        if (confirmDetails.eventTrue != EventType.None)
        { EventManager.i.PostNotification(confirmDetails.eventTrue, this, null, "ModalConfirm.cs -> ConfirmCloseRight"); }
    }

    /// <summary>
    /// subMethod for ConfirmCloseLeft/Right to handle closing down
    /// </summary>
    private void CloseConfirm()
    {
        confirmObject.SetActive(false);
        Debug.LogFormat("[UI] ModalConfirm.cs -> CloseModalConfirm{0}", "\n");
        //set modal false
        GameManager.i.guiScript.SetIsBlocked(false, modalLevel);
        //set game state
        GameManager.i.inputScript.ResetStates(modalState);
        //clear flag so execution can continue (if halted to await outcome)
        GameManager.i.guiScript.waitUntilDone = false;
    }

    /// <summary>
    /// Returns result of ConfirmWindow button press. False if buttonFalse pressed (left button), true if buttonTrue pressed (right button)
    /// </summary>
    /// <returns></returns>
    public bool CheckResult()
    { return result; }

    //new methods above here
}
