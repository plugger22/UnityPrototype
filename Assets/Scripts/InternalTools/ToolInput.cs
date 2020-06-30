using toolsAPI;
using UnityEngine;


/// <summary>
/// handles InputManager.cs functionality for internal tools
/// </summary>
public class ToolInput : MonoBehaviour
{

    private ToolModal _modalState;

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
    #endregion

    #region SetModalState

    public void SetModalState(ToolModal state)
    { _modalState = state; }

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
            case ToolModal.New:

                break;
            case ToolModal.Lists:
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
