using toolsAPI;
using UnityEngine;


/// <summary>
/// handles InputManager.cs functionality for internal tools
/// </summary>
public class ToolInput : MonoBehaviour
{

    private ToolModal _modalState;
    private ToolModalType _modalType;

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

    #endregion

    #region ProcessKeyInput
    /// <summary>
    /// takes care of all key and mouse press input (excludes mouse wheel movement -> see ProcessMouseWheelInput)
    /// </summary>
    public void ProcessKeyInput()
    {
        float x_axis;

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
                                { ToolEvents.i.PostNotification(ToolEventType.NextAdventure, this, null, "ToolInput.cs -> ProcessKeyInput Horizontal"); }
                                else if (x_axis < 0)
                                { ToolEvents.i.PostNotification(ToolEventType.PreviousAdventure, this, null, "ToolInput.cs -> ProcessKeyInput Horizontal"); }
                            }
                            break;
                    }
                }
                break;
            case ToolModal.New:

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
                                { ToolEvents.i.PostNotification(ToolEventType.NextLists, this, null, "ToolInput.cs -> ProcessKeyInput Horizontal"); }
                                else if (x_axis < 0)
                                { ToolEvents.i.PostNotification(ToolEventType.PreviousLists, this, null, "ToolInput.cs -> ProcessKeyInput Horizontal"); }
                            }
                            break;
                    }
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
