using toolsAPI;
using UnityEngine;


/// <summary>
/// handles InputManager.cs functionality for internal tools
/// </summary>
public class ToolInput : MonoBehaviour
{

    private ToolModal _modalState;
    private ToolModalType _modalType;
    private ToolModalSubNew _modalSubNew;

    #region properties
    //needs to be updated whenever changed
    public ToolModal ModalState
    {
        get { return _modalState; }
        private set
        {
            _modalState = value;
            Debug.LogFormat("[Inp] ToolInput: ModalState now {0}{1}", _modalState, "\n");
        }
    }

    /// <summary>
    /// Any ToolModal can be in one of these possible subStates
    /// </summary>
    public ToolModalType ModalType
    {
        get { return _modalType; }
        private set
        {
            _modalType = value;
            Debug.LogFormat("[Inp] ToolInput: ModalType now {0}{1}", _modalType, "\n");
        }
    }
    #endregion

    #region SetModalState

    public void SetModalState(ToolModal state)
    { _modalState = state; }

    public void SetModalType(ToolModalType modalType)
    { _modalType = modalType; }

    public void SetModalSubNew(ToolModalSubNew modalType)
    { _modalSubNew = modalType; }

    #endregion

    #region ProcessKeyInput
    /// <summary>
    /// takes care of all key and mouse press input (excludes mouse wheel movement -> see ProcessMouseWheelInput)
    /// </summary>
    public void ProcessKeyInput()
    {
        float x_axis, y_axis;

        switch (_modalState)
        {
            case ToolModal.Main:
                {
                    switch (_modalType)
                    {
                        case ToolModalType.Read:
                            if (Input.GetButtonDown("Horizontal"))
                            {
                                //right / left arrows
                                x_axis = Input.GetAxisRaw("Horizontal");
                                if (x_axis > 0)
                                { ToolEvents.i.PostNotification(ToolEventType.NextAdventure, this, null, "ToolInput.cs -> ProcessKeyInput Horizontal RIGHT"); }
                                else if (x_axis < 0)
                                { ToolEvents.i.PostNotification(ToolEventType.PreviousAdventure, this, null, "ToolInput.cs -> ProcessKeyInput Horizontal LEFT"); }
                            }
                            else if (Input.GetButtonDown("Vertical"))
                            {
                                //up / down arrows
                                y_axis = Input.GetAxisRaw("Vertical");
                                if (y_axis > 0)
                                { ToolEvents.i.PostNotification(ToolEventType.MainSummaryUpArrow, this, null, "ToolInput.cs -> ProcessKeyInput Vertical UP"); }
                                else if (y_axis < 0)
                                { ToolEvents.i.PostNotification(ToolEventType.MainSummaryDownArrow, this, null, "ToolInput.cs -> ProcessKeyInput Vertical DOWN"); }
                            }
                            else if (Input.GetButtonDown("NewTurn") == true)
                            {
                                //ENTER pressed, open details page, close summary page
                                ToolEvents.i.PostNotification(ToolEventType.OpenMainDetails, this, null, "ToolInput.cs -> ProcessKeyInput ENTER");
                            }
                            break;
                        case ToolModalType.Details:
                            if (Input.GetButton("Cancel") == true)
                            {
                                //ESC pressed, exit main details page and return to summary view
                                ToolEvents.i.PostNotification(ToolEventType.CloseMainDetails, this, null, "ToolInput.cs -> ProcessKeyInput ESC");
                            }
                            else if (Input.GetButtonDown("Horizontal"))
                            {
                                //right / left arrows
                                x_axis = Input.GetAxisRaw("Horizontal");
                                if (x_axis > 0)
                                { ToolEvents.i.PostNotification(ToolEventType.MainDetailsRightArrow, this, null, "ToolInput.cs -> ProcessKeyInput Horizontal RIGHT"); }
                                else if (x_axis < 0)
                                { ToolEvents.i.PostNotification(ToolEventType.MainDetailsLeftArrow, this, null, "ToolInput.cs -> ProcessKeyInput Horizontal LEFT"); }
                            }
                            else if (Input.GetButtonDown("Vertical"))
                            {
                                //up / down arrows
                                y_axis = Input.GetAxisRaw("Vertical");
                                if (y_axis > 0)
                                { ToolEvents.i.PostNotification(ToolEventType.MainDetailsUpArrow, this, null, "ToolInput.cs -> ProcessKeyInput Vertical UP"); }
                                else if (y_axis < 0)
                                { ToolEvents.i.PostNotification(ToolEventType.MainDetailsDownArrow, this, null, "ToolInput.cs -> ProcessKeyInput Vertical DOWN"); }
                            }
                            break;
                            
                    }
                }
                break;
            case ToolModal.New:
                switch (_modalSubNew)
                {
                    case ToolModalSubNew.New:
                        break;
                    case ToolModalSubNew.Summary:
                        if (Input.GetButtonDown("Vertical"))
                        {
                            //right / left arrows
                            y_axis = Input.GetAxisRaw("Vertical");
                            if (y_axis > 0)
                            { ToolEvents.i.PostNotification(ToolEventType.NewSummaryUpArrow, this, null, "ToolInput.cs -> ProcessKeyInput Vertical UP"); }
                            else if (y_axis < 0)
                            { ToolEvents.i.PostNotification(ToolEventType.NewSummaryDownArrow, this, null, "ToolInput.cs -> ProcessKeyInput Vertical DOWN"); }
                        }
                        break;
                }
                break;
            case ToolModal.Lists:
                {
                    switch (_modalType)
                    {
                        case ToolModalType.Read:
                            if (Input.GetButtonDown("Horizontal"))
                            {
                                //right / left arrows
                                x_axis = Input.GetAxisRaw("Horizontal");
                                if (x_axis > 0)
                                { ToolEvents.i.PostNotification(ToolEventType.NextLists, this, null, "ToolInput.cs -> ProcessKeyInput Horizontal RIGHT"); }
                                else if (x_axis < 0)
                                { ToolEvents.i.PostNotification(ToolEventType.PreviousLists, this, null, "ToolInput.cs -> ProcessKeyInput Horizontal LEFT"); }
                            }
                            break;
                    }
                }
                break;
            case ToolModal.TurningPoint:
                {
                    /*
                    switch (_modalType)
                    {
                        case ToolModalType.Input:
                            //SpaceBar for new Plotpoint 
                            if (Input.GetButton("Multipurpose") == true)
                            { ToolEvents.i.PostNotification(ToolEventType.CreatePlotpoint, this, null, "ToolInput.cs -> ProcessKeyInput SPACEBAR"); }
                            break;
                    }
                    */
                }
                break;
        }
    }
    #endregion

    #region ProcessMouseWheelInput
    /// <summary>
    /// Handles mouse wheel input exclusively. Change is +ve if UP (+0.1), -ve if DOWN (-0.1)
    /// </summary>
    /// <param name="change"></param>
    public void ProcessMouseWheelInput(float change)
    {

    }
    #endregion


    //new methods above here
}
